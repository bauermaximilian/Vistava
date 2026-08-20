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
        var includeFileProvider = CreateFileProvider(includePath);
        builder.Services.AddSingleton(includeFileProvider);

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
            if (!Directory.Exists(includePath))
            {
                Directory.CreateDirectory(includePath);
            }
        }
        catch (Exception exc)
        {
            rootApp.Logger.LogError("The include directory under \"{path}\" couldn't be created. {error}",
            includePath, exc);
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
            configureApplication(app, includeFileProvider);
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

        if (extensionsPath != null && Directory.Exists(extensionsPath))
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
