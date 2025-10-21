// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Tmds.DBus;
using Vistava.Service.Contracts;

[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
namespace Vistava.Service.Services;

public class LinuxThumbnailProvider : IThumbnailProvider, IDisposable
{
	private class ThumbnailerJob
	{
		public bool IsCompleted { get; private set; }
		public Exception? Error { get; private set; }
		public string ThumbnailUri { get; }
		public string FileUri { get; }
		public CancellationToken CancellationToken { get; }
		public event EventHandler? Completed;

		public ThumbnailerJob(string filePath, CancellationToken cancellationToken)
		{
			FileUri = GetFileUri(filePath);
			ThumbnailUri = GetThumbnailPath(FileUri);
			CancellationToken = cancellationToken;
		}

		public void SetCompleted(Exception? error = null)
		{
			if (error != null)
			{
				Error = error;
				if (!IsCompleted)
				{
					IsCompleted = true;
					Completed?.Invoke(this, EventArgs.Empty);
				}
			}
			else if (!IsCompleted && !TrySetCompleted())
			{
				Error = new Exception("The generated thumbnail file couldn't be found.");
				IsCompleted = true;
				Completed?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool TrySetCompleted()
		{
			if (File.Exists(ThumbnailUri) && !IsCompleted)
			{
				IsCompleted = true;
				Completed?.Invoke(this, EventArgs.Empty);
				return true;
			}
			else
			{
				return false;
			}
		}

		private static string GetFileUri(string filePath)
		{
			UriBuilder uriBuilder = new()
			{
				Path = filePath,
				Scheme = "file",
				Host = string.Empty
			};
			return uriBuilder.ToString();
		}

		private static string GetThumbnailPath(string uri)
		{
			var inputBytes = Encoding.UTF8.GetBytes(uri);
			var hashBytes = MD5.HashData(inputBytes);
			var uriHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
			
			var thumbnailRootPath = Environment.GetEnvironmentVariable("XDG_CACHE_HOME",
				EnvironmentVariableTarget.User);
			if (string.IsNullOrWhiteSpace(thumbnailRootPath))
			{
				thumbnailRootPath = (Environment.GetEnvironmentVariable("HOME") ?? "") + "/.cache";
			}
			thumbnailRootPath += $"/thumbnails/{ThumbnailerFlavor}";

			return Path.Combine(thumbnailRootPath, $"{uriHash}.png");
		}
	}

	public const string ThumbnailerServiceName = "org.freedesktop.thumbnails.Thumbnailer1";
	public const string ThumbnailerServicePath = "/org/freedesktop/thumbnails/Thumbnailer1";
	private const string ThumbnailerFlavor = "large";
	private const string ThumbnailerScheduler = "foreground";

	public bool IsDisposed => disposedValue;
	public string ThumbnailMimeType => MimeTypeProvider.MimeTypePng;

	private readonly MimeTypeProvider mimeTypeProvider;
	private readonly ILogger logger;
	private readonly Thread workerThread;
	private readonly BlockingCollection<ThumbnailerJob> thumbnailerJobs = new();
	private readonly CancellationTokenSource workerThreadCts = new();

	private bool disposedValue;

	public LinuxThumbnailProvider(MimeTypeProvider mimeTypeProvider, ILogger<LinuxThumbnailProvider> logger)
	{
		this.mimeTypeProvider = mimeTypeProvider ?? throw new ArgumentNullException(nameof(mimeTypeProvider));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		workerThread = new(RunWorker) { IsBackground = true };
		workerThread.Start();
	}

	~LinuxThumbnailProvider()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public async Task<Stream> GetThumbnailAsync(string filePath, CancellationToken cancellationToken)
	{
		ObjectDisposedException.ThrowIf(thumbnailerJobs.IsAddingCompleted || IsDisposed, this);

		using SemaphoreSlim semaphore = new(0);

		var job = new ThumbnailerJob(filePath, cancellationToken);

		void jobCompleted(object? sender, EventArgs args)
		{
			job.Completed -= jobCompleted;
			try { semaphore.Release(1); }
			catch (ObjectDisposedException) { }
		}
		job.Completed += jobCompleted;

		thumbnailerJobs.Add(job, cancellationToken);

		await semaphore.WaitAsync(4000, cancellationToken);

		if (job.Error != null)
		{
			throw job.Error;
		}
		// Only fail if the thumbnail file really doesn't exist (and it's not just the service not saying so).
		else if (!job.IsCompleted && !job.TrySetCompleted()) 
		{
			throw new TimeoutException("The generation of the thumbnail timed out.");
		}
		else
		{
			return new FileStream(job.ThumbnailUri, FileMode.Open);
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void RunWorker()
	{
		IThumbnailer1 thumbnailer;
		try
		{
			thumbnailer = Connection.Session.CreateProxy<IThumbnailer1>(ThumbnailerServiceName,
				ThumbnailerServicePath);
			thumbnailer.GetSupportedAsync().Wait();
		}
		catch (Exception exc)
		{
			logger.LogError("The service {service} couldn't be started. {exc}", ThumbnailerServiceName, exc);
			thumbnailerJobs.CompleteAdding();
			while (thumbnailerJobs.TryTake(out var item))
			{
				item.SetCompleted(new Exception("The thumbnailer service couldn't be started."));
			}

			return;
		}

		var jobHandles = new ConcurrentDictionary<uint, ThumbnailerJob>();

		void onQueueItemFinished(uint handle)
		{
			if (jobHandles.TryRemove(handle, out var job) && !job.IsCompleted)
			{
				job.SetCompleted();
			}
		}
		void onQueueItemFailed((uint handle, string[] failedUris, int errorCode, string message) args)
		{
			if (jobHandles.TryRemove(args.handle, out var job))
			{
				job.SetCompleted(new Exception($"Code {args.errorCode}): {args.message}."));
			}
		}

		using var queueItemFinishedWatcher = thumbnailer.WatchFinishedAsync(onQueueItemFinished).Result;
		using var queueItemFailedWatcher = thumbnailer.WatchErrorAsync(onQueueItemFailed).Result;

		logger.LogDebug("Thumbnailer service started successfully.");

		try
		{
			while (!thumbnailerJobs.IsCompleted)
			{
				var job = thumbnailerJobs.Take();
				if (job.CancellationToken.IsCancellationRequested || workerThreadCts.IsCancellationRequested)
				{
					job.SetCompleted(new OperationCanceledException());
				}
				else if (!job.TrySetCompleted()) // Only enqueue a thumbnail request if the thumbnail file doesn't exist.
				{
					var mimeType = mimeTypeProvider.GetMimeType(job.FileUri);

					var handle = thumbnailer.QueueAsync([job.FileUri], [mimeType], ThumbnailerFlavor,
						ThumbnailerScheduler, 0).Result;

					if (!jobHandles.TryAdd(handle, job))
					{
						job.SetCompleted(new Exception("The thumbnailer service returned a handle which is already in use."));
					}
				}
			}
		}
		catch (InvalidOperationException)
		{
			// Occurs on the "Take" method when the collection is marked as complete for adding - no handling required.
		}

		logger.LogDebug("Thumbnailer service stopped.");
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				thumbnailerJobs.CompleteAdding();
				workerThreadCts.Cancel();
				if (!workerThread.Join(5000))
				{
					logger.LogWarning("Thumbnailer service couldn't be terminated (timeout).");
				}
			}

			disposedValue = true;
		}
	}
}

#pragma warning disable CS8625 // Auto-generated code.
[DBusInterface("org.freedesktop.thumbnails.Thumbnailer1")]
interface IThumbnailer1 : IDBusObject
{
	Task<uint> QueueAsync(string[] Uris, string[] MimeTypes, string Flavor, string Scheduler, uint HandleToUnqueue);
	Task DequeueAsync(uint Handle);
	Task<(string[] uriSchemes, string[] mimeTypes)> GetSupportedAsync();
	Task<string[]> GetSchedulersAsync();
	Task<string[]> GetFlavorsAsync();
	Task<IDisposable> WatchStartedAsync(Action<uint> handler, Action<Exception> onError = null);
	Task<IDisposable> WatchFinishedAsync(Action<uint> handler, Action<Exception> onError = null);
	Task<IDisposable> WatchReadyAsync(Action<(uint handle, string[] uris)> handler, Action<Exception> onError = null);
	Task<IDisposable> WatchErrorAsync(Action<(uint handle, string[] failedUris, int errorCode, string message)> handler, Action<Exception> onError = null);
}
#pragma warning restore CS8625 // Auto-generated code.
