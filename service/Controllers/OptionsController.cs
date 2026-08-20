// SPDX-License-Identifier: GPL-3.0-or-later

using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Vistava.Service.Common;
using Vistava.Service.Services;
using Vistava.Service.Utils;

namespace Vistava.Service.Controllers;

[Route(Route)]
[ApiController]
public class OptionsController(KestrelProperties kestrelProperties, AppPathProvider appPathProvider,
    IFileProvider fileProvider, ServiceConfiguration serviceConfiguration,
    ILogger<OptionsController> logger) : ControllerBase
{
    public const string Route = "api/options";
    private const string SourceDefinitionSuffix = ".source.json";

    private readonly JsonSerializerOptions sourceDefinitionSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    [HttpGet("config/{filename}")]
    public async Task<ActionResult<string>> GetConfiguration(string filename)
    {
        var configurationDirectoryContents = fileProvider.GetDirectoryContents(
            AppPathsHelper.ConfigurationsDirectoryName);
        if (configurationDirectoryContents.Exists)
        {
            var requestedFile = configurationDirectoryContents.FirstOrDefault(
                file => file.Name == filename);
            try
            {
                if (requestedFile != null)
                {
                    using var stream = requestedFile.CreateReadStream();
                    using var streamReader = new StreamReader(stream);
                    return await streamReader.ReadToEndAsync();
                }
            }
            catch
            {
                // If the file can't be read, return "null" (just as if the file wasn't found).
            }
        }

        // Return the string "null", which is valid JSON (to avoid 404 errors).
        return "null";
    }

    [HttpGet("info")]
    public async Task<ActionResult<ServiceInformation>> GetServiceInformation()
    {
        return new ServiceInformation(
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0",
            IsLocalRequest() ? new Uri(AppPathsHelper.GenerateIncludePath()).AbsoluteUri : null,
            serviceConfiguration.Debug);
    }

    [HttpGet("sources")]
    public async Task<ActionResult<IDictionary<string, SourceConfiguration>>> GetSources() 
    {
        var sources = new Dictionary<string, SourceConfiguration>();
        foreach (var fileEntry in fileProvider.GetDirectoryContents("Sources")) 
        {
            var fileName = fileEntry.Name.ToLower();
            int sourceDefinitionSuffixIndex = fileName.LastIndexOf(SourceDefinitionSuffix);
            if (fileName.EndsWith(SourceDefinitionSuffix)) 
            {
                var sourceName = fileName[..sourceDefinitionSuffixIndex];
                if (sources.ContainsKey(sourceName))
                {
                    logger.LogWarning("The source '{name}' was defined more than once - only the " + 
                        "first encounter will be loaded.", sourceName);
                }
                
                SourceConfiguration? sourceConfiguration = null;
                try 
                {
                    using var fileEntryStream = fileEntry.CreateReadStream();
                    sourceConfiguration = await JsonSerializer.DeserializeAsync<SourceConfiguration>(
                        fileEntryStream, sourceDefinitionSerializerOptions) ?? 
                        throw new InvalidOperationException("The definition was deserialized into a null value.");
                    if (string.IsNullOrWhiteSpace(sourceConfiguration.ModuleFileName)) 
                    {
                        throw new InvalidOperationException("The definition did not provide a valid module file name.");
                    }
                    if (!fileProvider.GetDirectoryContents("Sources").Any(
                        file => file.Name == sourceConfiguration.ModuleFileName)) 
                    {
                        throw new InvalidOperationException("The definition specified a module file name " +
                            "that couldn't be found.");
                    }
                }
                catch (Exception exc) 
                {
                    logger.LogWarning("The source definition '{name}' couldn't be loaded. {exc}", 
                        sourceName, exc.Message);
                }
                
                if (sourceConfiguration != null) 
                {
                    sources[sourceName] = sourceConfiguration;
                }
            }
        }
        return sources;
    }

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
        string newAddress = $"{appPathProvider.Scheme}://{host}:{currentPort}";
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
