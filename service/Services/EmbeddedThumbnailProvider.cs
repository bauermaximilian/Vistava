// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using Vistava.Service.Contracts;

namespace Vistava.Service.Services;

public class EmbeddedThumbnailProvider : IThumbnailProvider
{
    private readonly MediaFileInfoProvider mediaFileInfoProvider;
    private readonly ILogger<EmbeddedThumbnailProvider> logger;

    public string ThumbnailMimeType => MimeTypeProvider.MimeTypeJpeg;

    public EmbeddedThumbnailProvider(MediaFileInfoProvider mediaFileInfoProvider, 
        ILogger<EmbeddedThumbnailProvider> logger)
    {
        this.mediaFileInfoProvider = mediaFileInfoProvider;
        this.logger = logger;
    }

    public async Task<Stream> GetThumbnailAsync(string filePath, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var mediaInfo = await mediaFileInfoProvider.GetMediaFileThumbnailAsync(filePath, cancellationToken);
        Stream output = new MemoryStream(mediaInfo.ThumbnailData ?? [], false);
        
        stopwatch.Stop();
        
        logger.LogTrace("Thumbnail creation of {File} took {Time} ms ({length} bytes)",
           Path.GetFileName(filePath), stopwatch.ElapsedMilliseconds, output.Length);

        return output;
    }
}
