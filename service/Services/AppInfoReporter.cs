// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Services;

public class AppInfoReporter(AppPathProvider pathProvider, ApplicationParameters applicationParameters,
    ILogger<AppInfoReporter> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var address = await pathProvider.GetAppUrlLocal(stoppingToken);
        logger.LogInformation("Application started under URL '{appUrl}'.", address);

        if (applicationParameters.Scheme == "https")
        {
            logger.LogInformation("Using HTTPS with certificate at '{path}'.",
                applicationParameters.HttpsCertificatePath);
        }
        else
        {
            logger.LogInformation("Using basic HTTP (no valid certificate found at '{path}').",
                applicationParameters.HttpsCertificatePath);
        }

        logger.LogInformation("Using '{path}' for loading application extensions.",
            applicationParameters.ExtensionPath);
    }
}
