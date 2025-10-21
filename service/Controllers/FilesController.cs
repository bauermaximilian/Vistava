// SPDX-License-Identifier: GPL-3.0-or-later

using System.Security;
using System.Web;
using Microsoft.AspNetCore.Mvc;
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
            return new FileStreamResult(handle.Stream, handle.ContentType)
            {
                EnableRangeProcessing = true
            };
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
}
