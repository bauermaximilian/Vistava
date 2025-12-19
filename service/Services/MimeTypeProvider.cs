// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.AspNetCore.StaticFiles;

namespace Vistava.Service.Services;

public class MimeTypeProvider()
{
    public const string MimeTypeDefault = "application/octet-stream";

    public const string MimeTypePng = "image/png";

    private readonly FileExtensionContentTypeProvider baseProvider = new();

    public string GetMimeType(string filePath)
    {
        if (baseProvider.TryGetContentType(filePath, out var fileMimeType) &&
            fileMimeType != MimeTypeDefault)
        {
            return fileMimeType;
        }
        else
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            return fileExtension switch
            {
                ".psd" => "image/psd",
                ".dds" => "image/vnd-ms.dds",
                _ => MimeTypeDefault
            };
        }
    }
}
