// SPDX-License-Identifier: GPL-3.0-or-later

using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Vistava.Service.Common;
using Vistava.Service.Contracts;
using Vistava.Service.Services;
using Vistava.Service.Utils;

namespace Vistava.Service;

public static class Program
{
    private static readonly char[] RandomUrlCharacters = GenerateNonambiguousCharacterList();

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder();

        var userConfiguration = new ConfigurationBuilder().AddCommandLine(args).Build();

        var app = BuildApplication(builder, userConfiguration, ConfigureApplicationServices, ConfigureApplication);

        if (args.Any(arg => arg.ToLowerInvariant().TrimStart('/', '-') == ServiceConfiguration.CliFlagHelp))
        {
            ServiceConfiguration.PrintHelp(app.Logger);
        }
        else
        {
            app.Run();
        }
    }

    private static WebApplication BuildApplication(WebApplicationBuilder builder,
        IConfiguration userConfiguration, Action<IServiceCollection> configureServices,
        Action<IApplicationBuilder, IFileProvider> configureApplication)
    {
        var serviceConfiguration = ServiceConfiguration.Parse(userConfiguration);
        builder.Services.AddSingleton(serviceConfiguration);

        var defaultLogLevel = serviceConfiguration.Debug ? "Debug" : "Information";

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            { "Logging:LogLevel:Default", defaultLogLevel },
            { "Logging:LogLevel:Microsoft.AspNetCore", "Warning" },
            { "Logging:LogLevel:Microsoft.Hosting.Lifetime", "Warning" },
            { "Logging:LogLevel:Microsoft.Extensions.Hosting.Internal.Host", "Information" }
        });

        configureServices(builder.Services);

        var certificatePath = AppPathsHelper.GenerateHttpsCertificatePath();
        Exception? httpsCertificateError = null;
        HttpsCertificate? httpsCertificate = null;
        if (!serviceConfiguration.DisableHttps)
        {
            TryLoadOrCreateHttpsCertificate(certificatePath, out httpsCertificate, out httpsCertificateError);
        }

        var includePath = AppPathsHelper.GenerateIncludePath();
        var combinedFileProvider = CreateFileProvider(includePath);
        builder.Services.AddSingleton(combinedFileProvider);

        string listenerUrl = GenerateListenerUrl(serviceConfiguration, httpsCertificate != null);
        var kestrelProperties = new KestrelProperties() { Endpoint = listenerUrl };
        builder.Services.AddSingleton(kestrelProperties);
        builder.WebHost.UseKestrel((_, options) => {
            options.Configure(kestrelProperties.Configuration, true);
            if (httpsCertificate != null)
            {
                options.ConfigureHttpsDefaults(
                    httpsOptions => httpsOptions.ServerCertificate = httpsCertificate.Certificate);
            }
        });

        string baseUrl = serviceConfiguration.RandomizeBasePath ? $"/{GenerateRandomString(6)}" : "";
        var applicationParameters = new ApplicationParameters(
            httpsCertificate != null ? "https" : "http", baseUrl, includePath, certificatePath);
        builder.Services.AddSingleton(applicationParameters);

        var rootApp = builder.Build();

        try
        {
            CreateAndPopulateAppdataDirectories(combinedFileProvider);
        }
        catch (Exception exc)
        {
            rootApp.Logger.LogError("The application data couldn't be initialized. {error}", exc);
        }

        if (httpsCertificateError != null && httpsCertificateError is not FileNotFoundException)
        {
            rootApp.Logger.LogError("The HTTPS certificate couldn't be loaded. {error}", httpsCertificateError);
        }

        if (serviceConfiguration.AllowCors)
        {
            rootApp.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            builder.Services.AddCors();
        }

        rootApp.Map(baseUrl, app =>
        {
            app.UsePathBase(baseUrl);
            app.UseRouting();
            configureApplication(app, combinedFileProvider);
        });

        return rootApp;
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.WriteIndented = true;
        });
        services.AddHostedService<AppInfoReporter>();
        services.AddSingleton<ILocalFileSystem, LocalFileSystem>();
        services.AddSingleton<MimeTypeProvider>();
        services.AddSingleton<AppPathProvider>();
        services.AddSingleton<MediaFileInfoProvider>();
        services.AddSingleton<IThumbnailProvider, EmbeddedThumbnailProvider>();
    }

    private static void ConfigureApplication(IApplicationBuilder app, IFileProvider fileProvider)
    {       
        var defaultFileOptions = new DefaultFilesOptions();
        defaultFileOptions.DefaultFileNames.Clear();
        defaultFileOptions.DefaultFileNames.Add("index.html");
        defaultFileOptions.FileProvider = fileProvider;

        app.UseDefaultFiles(defaultFileOptions);
        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = fileProvider,
            RequestPath = ""
        });

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    private static IFileProvider CreateFileProvider(string? extensionsPath)
    {
        var fileProviders = new List<IFileProvider>
        {
            new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "wwwroot")
        };

        if (extensionsPath != null)
        {
            fileProviders.Add(new PhysicalFileProvider(extensionsPath));
        }

        return new CompositeFileProvider([.. fileProviders]);
    }

    private static bool TryLoadOrCreateHttpsCertificate(string certificatePath,
        out HttpsCertificate? httpsCertificate, out Exception? httpsCertificateError)
    {
        try
        {
            if (File.Exists(certificatePath))
            {
                httpsCertificate = HttpsCertificate.Import(certificatePath);
                if (DateTime.Now < httpsCertificate.Certificate.NotBefore)
                {
                    throw new InvalidOperationException($"The certificate is not valid yet.");
                }
                if (DateTime.Now > httpsCertificate.Certificate.NotAfter)
                {
                    throw new InvalidOperationException("The certificate is not valid anymore.");
                }
                httpsCertificateError = null;
                return true;
            }
            else
            {
                httpsCertificate = null;
                httpsCertificateError = new FileNotFoundException("No HTTPS certificate file was found under \"" +
                    certificatePath + "\".");
                return false;
            }
        }
        catch (Exception exc)
        {
            httpsCertificate = null;
            httpsCertificateError = new InvalidOperationException(
                $"The HTTPS certificate under \"{certificatePath}\" can't be used.", exc);
            return false;
        }
    }
    
    private static void CreateAndPopulateAppdataDirectories(IFileProvider applicationFilesProvider)
    {
        string configurationsPath = AppPathsHelper.GenerateConfigurationsIncludePath();
        try
        {
            if (!Directory.Exists(configurationsPath))
            {
                Directory.CreateDirectory(configurationsPath);
            }
        }
        catch (Exception exc)
        {
            throw new InvalidOperationException("The directory for configurations couldn't be created " +
                $"under \"{configurationsPath}\".", exc);
        }

        var configurationFilesNames = new List<string>() { AppPathsHelper.ConfigurationKeyboardFileName,
            AppPathsHelper.ConfigurationGamepadFileName, AppPathsHelper.ConfigurationApplicationFileName };

        foreach (var configurationFileName in configurationFilesNames)
        {
            var targetFilePath = Path.Combine(configurationsPath, configurationFileName);
            try
            {
                if (!File.Exists(targetFilePath))
                {
                    var sourceFileInfo = applicationFilesProvider.GetFileInfo(
                        AppPathsHelper.DefaultConfigurationsDirectoryUrl + configurationFileName);
                    using var sourceFileStream = sourceFileInfo.CreateReadStream();
                    using var targetFileStream = File.Create(targetFilePath);
                    sourceFileStream.CopyTo(targetFileStream);
                }
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException("The default configuration file under \"" +
                    targetFilePath + "\" couldn't be created.", exc);
            }
        }

        string sourcesPath = AppPathsHelper.GenerateSourcesIncludePath();
        try
        {
            if (!Directory.Exists(sourcesPath))
            {
                Directory.CreateDirectory(sourcesPath);
            }
        }
        catch (Exception exc)
        {
            throw new InvalidOperationException("The directory for sources couldn't be created " +
                $"under \"{configurationsPath}\".", exc);
        }
    }

    private static string GenerateListenerUrl(ServiceConfiguration configuration, bool useHttps)
    {
        var scheme = useHttps ? "https" : "http";
        return $"{scheme}://{(configuration.Public ? "*" : "127.0.0.1")}:{configuration.Port}";
    }

    private static char[] GenerateNonambiguousCharacterList()
    {
        char[] ambiguousCharacters = ['I', 'l', '1', '0', 'O'];
        return [.. Enumerable.Range(65, 25).Select(i => (char)i)
            .Concat(Enumerable.Range(97, 25).Select(i => (char)i))
            .Concat(Enumerable.Range(48, 9).Select(i => (char)i))
            .SkipWhile(c => ambiguousCharacters.Contains(c))];
    }

    private static string GenerateRandomString(int length)
    {
        return RandomNumberGenerator.GetString(RandomUrlCharacters, length);
    }
}
