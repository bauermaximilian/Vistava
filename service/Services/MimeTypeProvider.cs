// SPDX-License-Identifier: GPL-3.0-or-later

using Microsoft.AspNetCore.StaticFiles;

namespace Vistava.Service.Services;

public class MimeTypeProvider
{
    public const string MimeTypeDefault = "application/octet-stream";

    public const string MimeTypePng = "image/png";

    private readonly FileExtensionContentTypeProvider baseProvider = new();

    public string GetMimeType(string filePath)
    {
        if (!baseProvider.TryGetContentType(filePath, out var fileMimeType))
        {
            return MimeTypeDefault;
        }
        else
        {
            return fileMimeType;
        }
    }
}
