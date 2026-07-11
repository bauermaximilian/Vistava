// SPDX-License-Identifier: GPL-3.0-or-later

using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.FileProviders;
using Vistava.Service.Common;
using Vistava.Service.Contracts;
using Vistava.Service.Services;

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

        var extensionsPath = GenerateExtensionsPath();
        var fileProvider = CreateFileProvider(extensionsPath);
        builder.Services.AddSingleton(fileProvider);

        string listenerUrl = GenerateListenerUrl(serviceConfiguration);
        var kestrelProperties = new KestrelProperties() { Endpoint = listenerUrl };
        builder.Services.AddSingleton(kestrelProperties);
        builder.WebHost.UseKestrel((_, options) => options.Configure(kestrelProperties.Configuration, true));

        string baseUrl = serviceConfiguration.RandomizeBasePath ? $"/{GenerateRandomString(6)}" : "";
        builder.Services.AddSingleton(new ApplicationParameters(baseUrl, extensionsPath));

        var rootApp = builder.Build();

        if (serviceConfiguration.AllowCors)
        {
            rootApp.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            builder.Services.AddCors();
        }

        rootApp.Map(baseUrl, app =>
        {
            app.UsePathBase(baseUrl);
            app.UseRouting();
            configureApplication(app, fileProvider);
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
    
    private static string GenerateExtensionsPath()
    {
        var applicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "0.0";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            $"vistava-{applicationVersion}", "extensions");
    }

    private static string GenerateListenerUrl(ServiceConfiguration configuration)
    {
        return $"http://{(configuration.Public ? "*" : "127.0.0.1")}:{configuration.Port}";
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
