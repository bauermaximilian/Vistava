// SPDX-License-Identifier: GPL-3.0-or-later

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Vistava.Service.Common;
using Vistava.Service.Contracts;
using Vistava.Service.Services;

namespace Vistava.Service;

public static class Program
{
    private static readonly char[] RandomUrlCharacters;

    static Program()
    {
        char[] ambiguousCharacters = ['I', 'l', '1', '0', 'O'];
        RandomUrlCharacters = Enumerable.Range(65, 25).Select(i => (char)i)
            .Concat(Enumerable.Range(97, 25).Select(i => (char)i))
            .Concat(Enumerable.Range(48, 9).Select(i => (char)i))
            .SkipWhile(c => ambiguousCharacters.Contains(c))
            .ToArray();
    }
    
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
        Action<IApplicationBuilder> configureApplication)
    {
        var serviceConfiguration = ServiceConfiguration.Parse(userConfiguration);
        builder.Services.AddSingleton(serviceConfiguration);

        var defaultLogLevel = serviceConfiguration.Debug ? "Debug" : "Information";

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            { "Logging:LogLevel:Default", defaultLogLevel },
            { "Logging:LogLevel:Microsoft.AspNetCore", "Warning" },
        });
        
        configureServices(builder.Services);

        string listenerUrl = GenerateListenerUrl(serviceConfiguration);
        var kestrelProperties = new KestrelProperties() { Endpoint = listenerUrl };
        builder.Services.AddSingleton(kestrelProperties);
        builder.WebHost.UseKestrel((_, options) => options.Configure(kestrelProperties.Configuration, true));
        
        string basePath = serviceConfiguration.RandomizeBasePath ? $"/{GenerateRandomString(6)}" : "";
        builder.Services.AddSingleton(new ApplicationParameters(basePath));

        var rootApp = builder.Build();

        if (serviceConfiguration.AllowCors)
        {
            rootApp.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            builder.Services.AddCors();
        }

        rootApp.Map(basePath, app =>
        {
            app.UsePathBase(basePath);
            app.UseRouting();
            configureApplication(app);
        });

        return rootApp;
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHostedService<AppEndpointReporter>();
        services.AddSingleton<ILocalFileSystem, LocalFileSystem>();
        services.AddSingleton<MimeTypeProvider>();
        services.AddSingleton<AppPathProvider>();
        services.AddTransient<IMemoryCache, MemoryCache>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<IThumbnailProvider, WindowsThumbnailProvider>();
        } 
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddSingleton<IThumbnailProvider, LinuxThumbnailProvider>();
        }
    }

    private static void ConfigureApplication(IApplicationBuilder app)
    {
        var fileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "wwwroot");

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

    private static string GenerateListenerUrl(ServiceConfiguration configuration)
    {
        return $"http://{(configuration.Public ? "*" : "127.0.0.1")}:{configuration.Port}";
    }
    
    private static string GenerateRandomString(int length)
    {
        return RandomNumberGenerator.GetString(RandomUrlCharacters, length);
    }
}
