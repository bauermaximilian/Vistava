// SPDX-License-Identifier: GPL-3.0-or-later

using System.Security;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Vistava.Service.Common;
using Vistava.Service.Contracts;
using Vistava.Service.Services;

namespace Vistava.Service.Controllers;

[Route(Route)]
[ApiController]
public class FileEntriesController(ApplicationParameters applicationParameters, ILocalFileSystem fileSystem) : ControllerBase
{
    public const string Route = "api/fileEntries";
    
    [HttpGet("{*path}")]
    public async Task<ActionResult<IEnumerable<FileListEntry>>> Get(
        string? path, [FromQuery]int start = 0, [FromQuery]int limit = 50)
    {
        try
        {
            path = HttpUtility.UrlDecode(path ?? "");
            var baseHostUri = GetHostUri(HttpContext);
            if (PlaylistImporter.SupportedFileExtensions.Any(path.EndsWith))
            {
                return new ActionResult<IEnumerable<FileListEntry>>(
                    await fileSystem.ListPlaylistAsync(path, baseHostUri, start, limit,
                        HttpContext.RequestAborted));
            }
            else
            {
                return new ActionResult<IEnumerable<FileListEntry>>(
                    await fileSystem.ListDirectoryAsync(path, baseHostUri, start, limit,
                        HttpContext.RequestAborted));
            }
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
    
    private Uri GetHostUri(HttpContext context) 
    {
        return new UriBuilder()
        {
            Scheme = context.Request.Scheme,
            Host = context.Request.Host.Host,
            Port = context.Request.Host.Port ?? 80,
            Path = applicationParameters.Path.TrimEnd('/') + "/"
        }.Uri;
    }
}
