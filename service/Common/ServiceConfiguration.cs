// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public record ServiceConfiguration
{
    public const string CliFlagHelp = "help";
    public const string CliFlagPort = "port";
    public const string CliFlagDebug = "debug";
    public const string CliFlagRandomizeBasePath = "randomizeBasePath";
    public const string CliFlagPublic = "public";
    public const string CliFlagAllowCors = "allowCors";
    public const string CliFlagDisableHttps = "disableHttps";
    public const string CliTrue = "true";

    public int Port { get; set; }
    public bool Debug { get; set; }
    public bool RandomizeBasePath { get; set; }
    public bool Public { get; set; }
    public bool AllowCors { get; set; }
    public bool DisableHttps { get; set; }

    public static ServiceConfiguration Parse(IConfiguration configuration)
    {
        if (!int.TryParse(configuration[CliFlagPort], out int port) || port is <= 0 or > 65535)
        {
            port = 0;
        }
        return new ServiceConfiguration
        {
            Port = port,
            Debug = configuration[CliFlagDebug]?.ToLowerInvariant().Trim() == CliTrue,
            RandomizeBasePath = configuration[CliFlagRandomizeBasePath]?.ToLowerInvariant() == CliTrue,
            Public = configuration[CliFlagPublic]?.ToLowerInvariant().Trim() == CliTrue,
            AllowCors = configuration[CliFlagAllowCors]?.ToLowerInvariant().Trim() == CliTrue,
            DisableHttps = configuration[CliFlagDisableHttps]?.ToLowerInvariant().Trim() == CliTrue
        };
    }

    public static void PrintHelp(ILogger logger)
    {
        logger.LogInformation(@$"--{CliFlagHelp}: Print this help. 
--{CliFlagDebug}=true: Set default log level to 'debug'.
--{CliFlagPort}=PORT: Accept for HTTP/S traffic on the specified port.
--{CliFlagRandomizeBasePath}=true: Randomize the application URL root.
--{CliFlagAllowCors}=true: Allow CORS (for any origins).
--{CliFlagPublic}=true: Accept connections from all hosts and not just localhost.
--{CliFlagDisableHttps}=true: Ignore any HTTPS certificates and always disable HTTPS.");
    }
}