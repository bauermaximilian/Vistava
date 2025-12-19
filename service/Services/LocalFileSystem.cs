// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web;
using FFMpegCore;
using Microsoft.Extensions.Caching.Memory;
using Vistava.Service.Common;
using Vistava.Service.Contracts;
using Vistava.Service.Controllers;

namespace Vistava.Service.Services;

public class LocalFileSystem : ILocalFileSystem
{
	private const string FileSystemEntryEnumerationPattern = "*";

	private record MediaDetails(DateTime LastModification, double? MediaDuration);

	/// <summary>
	/// Defines the content of a valid PNG file that consists of one grey pixel.
	/// </summary>
	private readonly byte[] emptyPngData = {
		0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44,
		0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F,
		0x15, 0xC4, 0x89, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xA8,
		0xAF, 0xAF, 0xFF, 0x0F, 0x00, 0x05, 0x7B, 0x02, 0x7D, 0x1B, 0x81, 0xA3, 0x90, 0x00, 0x00,
		0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
	};
	private static readonly string[] allowedFileExtensions =
	[
		".jpg",
		".jpeg",
		".png",
		".gif",
		".webp",
		".svg",
		".mp4",
		".webm"
	];

	private static readonly string[] allowedConvertableFileExtensions =
	[
		".tif",
		".tiff",
		".psd",
		".dds"
	];

	private readonly IComparer<string> filePathComparer = new FilePathComparer();
	private static readonly EnumerationOptions enumerationOptions = new()
	{
		IgnoreInaccessible = true,
		AttributesToSkip = FileAttributes.System | FileAttributes.Hidden
	};

	private readonly MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
	{
		AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
	};

	private readonly IMemoryCache mediaDetailsCache;
	private readonly ILogger<LocalFileSystem> logger;
	private readonly MimeTypeProvider mimeTypeProvider;
	private readonly IThumbnailProvider? thumbnailProvider;
	private readonly bool enableFFProbe;

	public LocalFileSystem(IThumbnailProvider? thumbnailProvider, IMemoryCache mediaDetailsCache, 
		MimeTypeProvider mimeTypeProvider, ILogger<LocalFileSystem> logger)
	{
		this.logger = logger;
		this.mimeTypeProvider = mimeTypeProvider;
		this.thumbnailProvider = thumbnailProvider;
		if (thumbnailProvider == null)
		{
			logger.LogWarning("No thumbnail provider was found - " +
				"file thumbnails will not be available.");
		}
		
		this.mediaDetailsCache = mediaDetailsCache;

		try
		{
			if (string.IsNullOrWhiteSpace(GlobalFFOptions.GetFFProbeBinaryPath()))
			{
				throw new InvalidOperationException("FFProbe binary not found.");
			}

			enableFFProbe = true;
		}
		catch (Exception exc)
		{
			logger.LogWarning("Video duration detection not supported ({error}).", exc.Message);
		}
	}

	public async Task<FileListEntryHandle> GetThumbnailAsync(string path, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		Stream? thumbnailStream = null;
		string? contentType = null;

		if (thumbnailProvider?.ThumbnailMimeType != null)
		{
			contentType = thumbnailProvider.ThumbnailMimeType;
			try
			{
				thumbnailStream = thumbnailProvider == null ? null :
					await thumbnailProvider.GetThumbnailAsync(path, token);
			}
			catch (Exception exc)
			{
				logger.LogWarning("The thumbnail for file {path} couldn't be loaded. {exc}",
					path, exc.Message);
			}
		}

		FileListEntryHandle fileListEntryHandle;

		if (thumbnailStream == null || contentType == null)
		{
			if (thumbnailStream != null)
			{
				await thumbnailStream.DisposeAsync();
			}
			fileListEntryHandle = new FileListEntryHandle(
				new MemoryStream(emptyPngData, false), MimeTypeProvider.MimeTypePng);
		}
		else
		{
			fileListEntryHandle = new FileListEntryHandle(thumbnailStream, contentType);
		}

		return fileListEntryHandle;
	}

	public Task<FileListEntryHandle> GetFileAsync(string path, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		if (File.Exists(path))
		{
			var fileName = Path.GetFileName(path);
			return Task.FromResult(new FileListEntryHandle(File.OpenRead(path),
				mimeTypeProvider.GetMimeType(fileName)));
		}
		else
		{
			throw new FileNotFoundException(path);
		}
	}

	public async Task OpenPathItemLocationInFileExplorer(string directoryPath, int index, Uri baseHostUri,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		string? path = null;

		try
		{
			IList<FileListEntry> entries;
			if (PlaylistImporter.SupportedFileExtensions.Any(directoryPath.EndsWith))
			{
				entries = await ListPlaylistAsync(directoryPath, baseHostUri, 0, 0, cancellationToken);
			}
			else
			{
				entries = await ListDirectoryAsync(directoryPath, baseHostUri, 0, 0, cancellationToken);
			}

			if (index > 0 && index < entries.Count)
			{
				path = entries[index].FileSystemPath;
			}
		}
		catch
		{
			throw new InvalidOperationException("The specified entry couldn't be retrieved.");
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var sanitizedPath = path?.Replace(Path.AltDirectorySeparatorChar,
				Path.DirectorySeparatorChar);

			if (File.Exists(sanitizedPath) || Directory.Exists(sanitizedPath))
			{
				ProcessStartInfo processStartInfo = new()
				{
					FileName = "explorer",
					Arguments = $"/e, /select, \"{sanitizedPath}\""
				};

				Process.Start(processStartInfo);
			}
		}
		else
		{
			if (File.Exists(path))
			{
				path = Path.GetDirectoryName(path) ?? string.Empty;
			}

			if (Directory.Exists(path))
			{
				ProcessStartInfo processStartInfo = new()
				{
					FileName = "xdg-open",
					Arguments = $"\"{path}\""
				};

				Process.Start(processStartInfo);
			}
		}
	}

	public async Task<IList<FileListEntry>> ListDirectoryAsync(string path, Uri baseHostUri, int start, int limit,
		CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var results = new List<FileListEntry>();

		if (path.Length == 0 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			results.AddRange(EnumerateDriveEntries(start, limit, token));
		}
		else
		{
			var skippedEntries = 0;
			var parentDirectoryEntry = GetPathParentDirectoryEntry(path);
			if (parentDirectoryEntry != null)
			{
				if (start == 0)
				{
					results.Add(parentDirectoryEntry);
				}
				else
				{
					skippedEntries = 1;
				}
			}

			if (path.Length == 0)
			{
				path = "/";
			}

			try
			{

				await foreach (var entry in EnumeratePathDirectoryEntriesAsync(path, token))
				{
					if ((start == 0 || (results.Count + skippedEntries) >= start) && results.Count < limit)
					{
						results.Add(entry);
					}
					else
					{
						skippedEntries++;
					}
				}

				if (limit == 0 || results.Count < limit)
				{
					await foreach (var entry in EnumeratePathFileEntriesAsync(path, baseHostUri,
						Math.Max(start - skippedEntries, 0), Math.Max(limit - results.Count, 0), token))
					{
						results.Add(entry);
					}
				}
			}
			catch (TaskCanceledException)
			{
				throw;
			}
			catch (Exception exc)
			{
				logger.LogWarning("The content of the directory \"{path}\" (or parts of it) " +
					"couldn't be accessed. {exc}", path, exc.Message);
			}
		}

		return results;
	}

	public async Task<IList<FileListEntry>> ListPlaylistAsync(string path, Uri baseHostUri, int start, int limit,
		CancellationToken token)
	{
		var results = new List<FileListEntry>();

		string? parentDirectoryPath = Path.GetDirectoryName(path);
		if (parentDirectoryPath != null)
		{
			results.Add(new FileListEntry
			{
				QueryTarget = parentDirectoryPath,
				Type = FileListEntryType.ParentCollection
			});
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			results.Add(new FileListEntry
			{
				QueryTarget = string.Empty,
				Type = FileListEntryType.ParentCollection
			});
		}

		var playlist = await PlaylistImporter.ImportPlaylistAsync(path, token);

		foreach (var playlistItem in playlist)
		{
			if (token.IsCancellationRequested)
			{
				throw new TaskCanceledException();
			}

			string fileExtension = Path.GetExtension(playlistItem.FilePath);
			string mimeType = mimeTypeProvider.GetMimeType(playlistItem.FilePath);

			if (allowedFileExtensions == null ||
				allowedFileExtensions.Contains(fileExtension.ToLowerInvariant()))
			{
				results.Add(new FileListEntry
				{
					Label = playlistItem.Label,
					MediaUrl = BuildApiMediaUrl(playlistItem.FilePath, baseHostUri),
					MediaType = mimeType,
					ThumbnailUrl = BuildApiThumbnailUrl(playlistItem.FilePath, baseHostUri),
					ThumbnailType = "image/jpeg",
					FileSystemPath = playlistItem.FilePath
				});
			}
		}

		return results;
	}

	private static IEnumerable<FileListEntry> EnumerateDriveEntries(int start, int limit, CancellationToken token)
	{
		var drives = DriveInfo.GetDrives();
		for (var i = start; (limit > 0 ? i < limit : true) && i < drives.Length; i++)
		{
			var drive = drives[i];
			if (token.IsCancellationRequested)
			{
				throw new TaskCanceledException();
			}

			if (drive.IsReady)
			{
				yield return new FileListEntry
				{
					Label = drive.Name,
					QueryTarget = drive.Name,
					Type = FileListEntryType.ChildCollection,
					FileSystemPath = drive.Name
				};
			}
		}
	}

	private static FileListEntry? GetPathParentDirectoryEntry(string path)
	{
		var parentDirectoryPath = Path.GetDirectoryName(path);

		if (parentDirectoryPath != null)
		{
			return new FileListEntry
			{
				QueryTarget = parentDirectoryPath,
				Type = FileListEntryType.ParentCollection,
			};
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return new FileListEntry
			{
				QueryTarget = string.Empty,
				Type = FileListEntryType.ParentCollection
			};
		}
		else
		{
			return null;
		}
	}

	private async IAsyncEnumerable<FileListEntry> EnumeratePathDirectoryEntriesAsync(string path,
		[EnumeratorCancellation] CancellationToken token)
	{
		IEnumerable<string> directoryEnumerator = [];

		await Task.Run(() => directoryEnumerator = Directory.EnumerateDirectories(path,
			FileSystemEntryEnumerationPattern, enumerationOptions)
			.Order(filePathComparer), token);
		foreach (string directoryPath in directoryEnumerator)
		{
			token.ThrowIfCancellationRequested();

			string directoryName = Path.GetFileName(directoryPath);

			yield return new FileListEntry
			{
				Label = directoryName,
				QueryTarget = directoryPath,
				FileSystemPath = directoryPath,
				Type = FileListEntryType.ChildCollection
			};
		}
	}

	private async IAsyncEnumerable<FileListEntry> EnumeratePathFileEntriesAsync(string path, Uri baseHostUri,
		int start, int limit, [EnumeratorCancellation] CancellationToken token)
	{
		List<string> entriesEnumerator = [];

		await Task.Run(() => entriesEnumerator = Directory.EnumerateFiles(path,
			FileSystemEntryEnumerationPattern, enumerationOptions)
			.Order(filePathComparer)
			.Where(filePath => allowedFileExtensions.Contains(Path.GetExtension(filePath)) ||
				allowedConvertableFileExtensions.Contains(Path.GetExtension(filePath)) ||
				PlaylistImporter.SupportedFileExtensions.Contains(Path.GetExtension(filePath)))
			.Skip(start)
			.Take(limit > 0 ? limit : int.MaxValue)
			.ToList(), token);
		
		await UpdateMediaFileDurationCacheAsync(entriesEnumerator, token);
		
		foreach (var filePath in entriesEnumerator)
		{
			token.ThrowIfCancellationRequested();

			var fileName = Path.GetFileNameWithoutExtension(filePath);
			var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

			if (allowedFileExtensions.Contains(fileExtension))
			{
				var mimeType = GetMimeType(fileExtension);
				yield return new FileListEntry
				{
					Label = fileName,
					FileSystemPath = filePath,
					MediaUrl = BuildApiMediaUrl(filePath, baseHostUri),
					MediaDuration = mediaDetailsCache.Get<double?>(filePath),
					MediaType = mimeType,
					ThumbnailUrl = thumbnailProvider != null ? BuildApiThumbnailUrl(filePath, baseHostUri) : null,
					ThumbnailType = thumbnailProvider?.ThumbnailMimeType ?? null
				};
			}
			else if (allowedConvertableFileExtensions.Contains(fileExtension))
			{
				var mimeType = GetConvertedMimeType(fileExtension);
				yield return new FileListEntry
				{
					Label = fileName,
					FileSystemPath = filePath,
					MediaUrl = BuildApiMediaUrl(filePath, baseHostUri),
					MediaDuration = mediaDetailsCache.Get<double?>(filePath),
					MediaType = mimeType,
					ThumbnailUrl = thumbnailProvider != null ? BuildApiThumbnailUrl(filePath, baseHostUri) : null,
					ThumbnailType = thumbnailProvider?.ThumbnailMimeType ?? null
				};
			}
			else if (PlaylistImporter.SupportedFileExtensions.Contains(fileExtension))
			{
				yield return new FileListEntry
				{
					Label = fileName,
					QueryTarget = filePath,
					FileSystemPath = filePath,
					Type = FileListEntryType.ChildCollection
				};
			}
		}
	}

	private async Task UpdateMediaFileDurationCacheAsync(IEnumerable<string> filePaths, CancellationToken token)
	{
		if (enableFFProbe)
		{
			var updateTasks = new List<Task>(); 
			foreach (string filePath in filePaths)
			{
				string extension = Path.GetExtension(filePath).ToLowerInvariant();
				if (extension is ".mp4" or ".webm")
				{
					updateTasks.Add(Task.Run(() =>
					{
						token.ThrowIfCancellationRequested();
						if (mediaDetailsCache.Get(filePath) == null)
						{
							double? duration;
							try
							{
								duration = FFProbe.Analyse(filePath)?.Duration.TotalSeconds ?? null;
							}
							catch
							{
								duration = null;
							}

							mediaDetailsCache.Set(filePath, duration);
						}
					}, token));
				}
			}

			await Task.WhenAll(updateTasks);
		}
	}

	private string GetMimeType(string filePath)
	{
		return mimeTypeProvider.GetMimeType(filePath);
	}

	private string GetConvertedMimeType(string filePath)
	{
		return mimeTypeProvider.GetMimeType(filePath);
	}

	private static string BuildApiMediaUrl(string filePath, Uri baseUri)
	{
		if (Uri.TryCreate(baseUri, $"{FilesController.Route}/{HttpUtility.UrlEncode(filePath)}",
			out var absoluteUri))
		{
			return absoluteUri.ToString();
		}
		else
		{
			throw new ArgumentException("The specified file path or base URI are invalid.");
		}
	}

	private static string BuildApiThumbnailUrl(string filePath, Uri baseUri)
	{
		if (Uri.TryCreate(baseUri, $"{ThumbnailsController.Route}/{HttpUtility.UrlEncode(filePath)}",
			out var absoluteUri))
		{
			return absoluteUri.ToString();
		}
		else
		{
			throw new ArgumentException("The specified file path or base URI are invalid.");
		}
	}
}
