// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using System.Text;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using ImageMagick;
using LiteDB;
using Vistava.Service.Common;
using Vistava.Service.Utils;

namespace Vistava.Service.Services;

public class MediaFileInfoProvider
{
    private readonly ILogger logger;
    private const int PrecacheConcurrencyMaximum = 10;
    private readonly string ThumbnailCacheConnectionString = AppPathsHelper.GenerateMediaCachePath();
    private readonly KeyedGeneratorBundle<string, MediaFileInfo> mediaFileInfoLoaders;
    private readonly KeyedGeneratorBundle<string, MediaFileThumbnail> mediaFileThumbnailLoaders;
    private readonly object databaseLock = new();

    public MediaFileInfoProvider(ServiceConfiguration configuration, ILogger<MediaFileInfoProvider> logger)
    {
        this.logger = logger;

        mediaFileInfoLoaders = new KeyedGeneratorBundle<string, MediaFileInfo>(GetMediaFileInfo);
        mediaFileThumbnailLoaders = new KeyedGeneratorBundle<string, MediaFileThumbnail>(GetMediaFileThumbnail);

        if (configuration.Debug)
        {
            try
            {
                File.Delete(ThumbnailCacheConnectionString);
                logger.LogInformation("Initializing media cache file at '{cacheLocation}'.",
                    ThumbnailCacheConnectionString);
            }
            catch
            {
                logger.LogWarning("Couldn't delete temporary cache file at '{path}'.",
                    ThumbnailCacheConnectionString);
            }
        }
        else
        {
            logger.LogInformation("Using media cache file at '{cacheLocation}'.",
                ThumbnailCacheConnectionString);
        }
        LogFFtoolVersion(GlobalFFOptions.GetFFMpegBinaryPath());
        LogFFtoolVersion(GlobalFFOptions.GetFFProbeBinaryPath());
    }
    
    public async Task<MediaFileInfo> GetMediaFileInfoAsync(string filePath, CancellationToken stoppingToken)
    {
        return await mediaFileInfoLoaders.GenerateAsync(filePath, stoppingToken);
    }

    public async Task PrecacheMediaFileInfosAsync(IEnumerable<string> filePaths, CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        foreach (var filePath in filePaths)
        {
            if (tasks.Count > PrecacheConcurrencyMaximum)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
            tasks.Add(GetMediaFileInfoAsync(filePath, CancellationToken.None));
        }
        await Task.WhenAll(tasks).WaitAsync(stoppingToken);
    }
    
    public async Task<MediaFileThumbnail> GetMediaFileThumbnailAsync(string filePath, CancellationToken stoppingToken)
    {
        return await mediaFileThumbnailLoaders.GenerateAsync(filePath, stoppingToken);
    }

    private void LogFFtoolVersion(string path)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8
            });
            var timeoutCts = new CancellationTokenSource(1000);
            var processOutput = process?.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            processOutput?.Wait();
            process?.StandardOutput.Close();
            if (processOutput?.IsCompletedSuccessfully == true)
            {
                logger.LogDebug("Using {info}", processOutput.Result);
            }
            else
            {
                throw new InvalidOperationException("The process didn't provide the expected output.");
            }
        }
        catch (Exception exc)
        {
            logger.LogError("The initialisation of dependency '{dependency}' failed. {exc}", path, exc.Message);
        }
    }
    
    private MediaFileThumbnail GetMediaFileThumbnail(string filePath)
    {
        var lastWriteTime = File.GetLastWriteTimeUtc(filePath);

        MediaFileThumbnail? mediaThumbnail = null;
        PerformDatabaseAction<MediaFileThumbnail>(c => mediaThumbnail = c.FindById(filePath));

        if (mediaThumbnail == null || mediaThumbnail.LastModified < lastWriteTime)
        {
            mediaThumbnail = new MediaFileThumbnail()
            {
                Path = filePath,
                LastModified = lastWriteTime,
                ThumbnailData = GenerateThumbnail(filePath)
            };

            PerformDatabaseAction<MediaFileThumbnail>(c =>
            {
                c.Delete(new BsonValue(filePath));
                c.Insert(mediaThumbnail);
            });
        }

        return mediaThumbnail;
    }

    private MediaFileInfo GetMediaFileInfo(string filePath)
    {
        var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
        
        MediaFileInfo? mediaInfo = null;
        PerformDatabaseAction<MediaFileInfo>(c => mediaInfo = c.FindById(filePath));
        
        if (mediaInfo == null || mediaInfo.LastModified < lastWriteTime)
        {
            mediaInfo = GenerateMediaFileInfo(filePath, lastWriteTime);

            PerformDatabaseAction<MediaFileInfo>(c =>
            {
                c.Delete(new BsonValue(filePath));
                c.Insert(mediaInfo);
            });
        }

        return mediaInfo;
    }

    private void PerformDatabaseAction<TCollection>(Action<ILiteCollection<TCollection>> action)
    {
        try
        {
            lock (databaseLock)
            {
                using ILiteDatabase cache = new LiteDatabase(ThumbnailCacheConnectionString);
                var collection = cache.GetCollection<TCollection>();
                action(collection);
            }
        }
        catch (Exception exc)
        {
            logger.LogError("Couldn't perform database action. {exc}", exc.Message);
        }
    }
    
    private MediaFileInfo GenerateMediaFileInfo(string filePath, DateTime lastModified)
    {
        var type = MediaFileTypes.GetFileType(filePath);
        TimeSpan duration;

        try
        {
            if (type == MediaFileType.Video)
            {
                duration = GetVideoDuration(filePath);
            }
            else
            {
                duration = TimeSpan.Zero;
            }
        }
        catch
        {
            duration = TimeSpan.Zero;
        }

        return new MediaFileInfo()
            {
                Path = filePath,
                LastModified = lastModified,
                Duration = duration,
                Type = type
            };
    }

    private byte[]? GenerateThumbnail(string filePath)
    {
        var mediaFileType = MediaFileTypes.GetFileType(filePath);
        byte[]? thumbnailData = null;

        try
        {
            if (mediaFileType is MediaFileType.Image or MediaFileType.ImageConvertible)
            {
                thumbnailData = GenerateImageThumbnail(filePath);
            }
            else if (mediaFileType == MediaFileType.Video)
            {
                thumbnailData = GenerateVideoThumbnail(filePath);
            }
            else
            {
                logger.LogWarning("Couldn't generate thumbnail for '{filePath}': Unsupported file type.", filePath);
            }
        }
        catch (Exception exc)
        {
            // If thumbnail generation failed or no thumbnail could be generated, an empty
            // thumbnail will be generated.
            logger.LogWarning("Couldn't generate thumbnail for '{filePath}'. {exc}", filePath, exc.Message);
        }

        thumbnailData ??= GenerateEmptyFallbackThumbnail();

        return thumbnailData;
    }
    
    private static byte[] GenerateImageThumbnail(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        using var image = new MagickImage(File.OpenRead(filePath));
        image.Thumbnail(200, 500);
        return image.ToByteArray(MagickFormat.Jpg);
    }

    private static byte[] GenerateEmptyFallbackThumbnail()
    {
        using var image = new MagickImage(MagickColor.FromRgb(0, 0, 0), 200, 200);
        return image.ToByteArray(MagickFormat.Jpg);
    }

    private TimeSpan GetVideoDuration(string filePath)
    {
        return FFProbe.Analyse(filePath).Duration;
    }

    private static byte[] GenerateVideoThumbnail(string filePath)
    {
        using var thumbnailStream = new MemoryStream();
        var thumbnailStreamPipe = new StreamPipeSink(thumbnailStream);

        var ffmpegArguments = FFMpegArguments
            .FromFileInput(new FileInfo(filePath),
                // Only try to find a non-black thumbnail in the first 30 seconds of the video
                options => options.WithDuration(TimeSpan.FromSeconds(30)))
            .OutputToPipe(thumbnailStreamPipe, options =>
            {
                options.WithFrameOutputCount(1);
                options.WithCustomArgument("""
                -vf "blackframe=0,metadata=select:key=lavfi.blackframe.pblack:value=80:function=less,scale=200:-1"
                """);
                options.WithVideoCodec("mjpeg");
                options.ForceFormat("image2");
            });

        ffmpegArguments.ProcessSynchronously(true, new FFOptions() { LogLevel = FFMpegLogLevel.Quiet });

        var thumbnailData = thumbnailStream.ToArray();
        if (thumbnailData.Length == 0)
        {
            throw new InvalidOperationException(
                "The thumbnail generation failed with empty thumbnail data being returned by ffmpeg.");
        }
        return thumbnailData;
    }
}