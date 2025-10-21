// SPDX-License-Identifier: GPL-3.0-or-later

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Vistava.Service.Common;
using Vistava.Service.Services;

namespace Vistava.Service.Controllers;

[Route(Route)]
[ApiController]
public class OptionsController(KestrelProperties kestrelProperties, AppPathProvider appPathProvider,
    ILogger<OptionsController> logger) : ControllerBase
{
    public const string Route = "api/options";

    [HttpGet("appUrls")]
    public async Task<ActionResult<IEnumerable<AppPath>>> GetAppPaths()
    {
        if (!IsLocalRequest())
        {
            return new UnauthorizedResult();
        }

        return (await GetAppPathsAsync(HttpContext.RequestAborted)).ToList();
    }
    
    [HttpGet("listenAnyIp")]
    public ActionResult<bool> GetListenAnyIp()
    {
        if (!IsLocalRequest())
        {
            return new UnauthorizedResult();
        }

        return IsListeningToAnyIp();
    }

    [HttpPost("listenAnyIp/{isEnabled:bool}")]
    public async Task<ActionResult<IEnumerable<AppPath>>> PostListenAnyIp(bool isEnabled)
    {
        if (!IsLocalRequest())
        {
            return new UnauthorizedResult();
        }

        int currentPort = await appPathProvider.GetApplicationPort(4000, HttpContext.RequestAborted);
        string host = isEnabled ? "*" : "127.0.0.1";
        string newAddress = $"http://{host}:{currentPort}";
        kestrelProperties.Endpoint = newAddress;
        
        logger.LogInformation("Now listening on: {url}", newAddress);

        return (await GetAppPathsAsync(HttpContext.RequestAborted)).ToList();
    }

    private async Task<IEnumerable<AppPath>> GetAppPathsAsync(CancellationToken token)
    {
        var publicAppPaths = IsListeningToAnyIp() ? await appPathProvider.GetAppUrlsExternal(token) : [];
        var privateAppPath = await appPathProvider.GetAppUrlLocal(token);

        return publicAppPaths.Select(appPath => new AppPath(appPath, true))
            .Prepend(new AppPath(privateAppPath, false));
    }

    private bool IsListeningToAnyIp()
    {
        string? address = kestrelProperties.Endpoint;
        return address.Contains("*");
    }
    
    private bool IsLocalRequest()
    {
        return IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress?? IPAddress.Loopback);
    }
}
