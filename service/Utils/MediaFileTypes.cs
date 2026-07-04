// SPDX-License-Identifier: GPL-3.0-or-later

using Vistava.Service.Common;

namespace Vistava.Service.Utils;

public static class MediaFileTypes
{
    private static readonly string[] supportedImageFileExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".svg",
    ];

    private static readonly string[] supportedVideoFileExtensions =
    [
        ".mp4",
        ".webm"
    ];

    private static readonly string[] convertibleImageFileExtensions =
    [
        ".tif",
        ".tiff",
        ".psd",
        ".dds"
    ];
    
    public static bool IsSupported(string filePath)
    {
        var type = GetFileType(filePath);
        return type != MediaFileType.Unknown;
    }

    public static bool IsSupported(string filePath, MediaFileType requiredFileType)
    {
        var type = GetFileType(filePath);
        return type == requiredFileType;
    }

    public static MediaFileType GetFileType(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant().Trim();
        if (supportedImageFileExtensions.Contains(fileExtension))
        {
            return MediaFileType.Image;
        }
        else if (supportedVideoFileExtensions.Contains(fileExtension))
        {
            return MediaFileType.Video;
        }
        else if (convertibleImageFileExtensions.Contains(fileExtension))
        {
            return MediaFileType.ImageConvertible;
        }
        else
        {
            return MediaFileType.Unknown;
        }
    }
}