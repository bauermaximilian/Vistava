// SPDX-License-Identifier: GPL-3.0-or-later

using System.Security;
using System.Web;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Vistava.Service.Common;
using Vistava.Service.Contracts;

namespace Vistava.Service.Controllers;

[Route(Route)]
[ApiController]
public class FilesController(ILocalFileSystem fileSystem) : ControllerBase
{
    public const string Route = "api/files";

    [HttpGet("{**path}")]
    public async Task<ActionResult> Get(string path)
    {
        try
        {
            path = HttpUtility.UrlDecode(path);
            var handle = await fileSystem.GetFileAsync(path, HttpContext.RequestAborted);
            return await Get(handle, HttpContext.RequestAborted);
        }
        catch (Exception exc)
        {
            return exc switch
            {
                FileNotFoundException or DirectoryNotFoundException => new NotFoundResult(),
                SecurityException or UnauthorizedAccessException => new UnauthorizedResult(),
                _ => Problem($"Internal server error ({exc.GetType().Name}).")
            };
        }
    }

    private async Task<FileStreamResult> Get(FileListEntryHandle entryHandle, CancellationToken token)
    {
        Stream? imageStream = null;
        string? imageType = null;

        try
        {
            if (entryHandle.ContentType == "image/tiff" || entryHandle.ContentType == "image/psd" ||
                entryHandle.ContentType == "image/vnd-ms.dds")
            {
                using var image = new MagickImage(entryHandle.Stream);
                imageStream = new MemoryStream();
                await image.WriteAsync(imageStream, MagickFormat.Jpeg, token);
                imageStream.Position = 0;
                await entryHandle.Stream.DisposeAsync();
                imageType = "image/jpeg";
            }
            else
            {
                imageStream = entryHandle.Stream;
                imageType = entryHandle.ContentType;
            }
        }
        catch
        {
            entryHandle.Stream?.Dispose();
            imageStream?.Dispose();

            throw;
        }

        return new FileStreamResult(imageStream, imageType)
        {
            EnableRangeProcessing = true
        };
    }
}
