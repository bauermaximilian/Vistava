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
    private const string CliFlagHelp = "help";
    private const string CliFlagPort = "port";
    private const string CliFlagRandomizeBasePath = "randomizeBasePath";
    private const string CliFlagPublic = "public";
    private const string CliFlagAllowCors = "allowCors";
    private const string CliTrue = "true";
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder();

        var userConfiguration = new ConfigurationBuilder().AddCommandLine(args).Build();

        var app = BuildApplication(builder, userConfiguration, ConfigureApplicationServices, ConfigureApplication);

        if (args.Any(arg => arg.ToLowerInvariant().TrimStart('/', '-') == "help"))
        {
            PrintHelp(app.Logger);
        }
        else
        {
            app.Run();
        }
    }

    private static void PrintHelp(ILogger logger)
    {
        logger.LogInformation(@$"--{CliFlagHelp}: Print this help. 
--{CliFlagPort}=PORT: Accept for HTTP traffic on the specified port.
--{CliFlagRandomizeBasePath}=true: Randomize the application URL root.
--{CliFlagAllowCors}=true: Allow CORS (for any origins).
--{CliFlagPublic}=true: Accept connections from all hosts and not just localhost.");
    }

    private static WebApplication BuildApplication(WebApplicationBuilder builder, 
        IConfiguration userConfiguration, Action<IServiceCollection> configureServices, 
        Action<IApplicationBuilder> configureApplication)
    {
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            { "Logging:LogLevel:Default", "Information" },
            { "Logging:LogLevel:Microsoft.AspNetCore", "Warning" },
        });
        
        configureServices(builder.Services);

        string listenerUrl = GenerateListenerUrl(userConfiguration);
        var kestrelProperties = new KestrelProperties() { Endpoint = listenerUrl };
        builder.Services.AddSingleton(kestrelProperties);
        builder.WebHost.UseKestrel((_, options) => options.Configure(kestrelProperties.Configuration, true));
        
        string basePath = (userConfiguration[CliFlagRandomizeBasePath]?.ToLowerInvariant() == CliTrue) ?
            $"/{GenerateRandomString(6)}" : "";
        builder.Services.AddSingleton(new ApplicationParameters(basePath));

        var rootApp = builder.Build();

        if (userConfiguration[CliFlagAllowCors]?.ToLowerInvariant().Trim() == CliTrue)
        {
            rootApp.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            builder.Services.AddCors();
        }

        rootApp.Map(basePath, app =>
        {
            app.UsePathBase(basePath);
            app.UseRouting();
            configureApplication(app);

            #if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
            #endif
        });

        return rootApp;
    }

    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        services.AddControllers();

        #if DEBUG
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        #endif
        
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

    private static string GenerateListenerUrl(IConfiguration configuration)
    {
        if (!int.TryParse(configuration[CliFlagPort], out int port) || port is <= 0 or > 65535)
        {
            port = 0;
        }

        string host = configuration[CliFlagPublic]?.ToLowerInvariant().Trim() == CliTrue ? 
            "*" : "127.0.0.1";
        
        return $"http://{host}:{port}";
    }
    
    private static string GenerateRandomString(int length)
    {
        return RandomNumberGenerator.GetString(Enumerable.Range(65, 25).Select(i => (char)i)
            .Concat(Enumerable.Range(97, 25).Select(i => (char)i))
            .Concat(Enumerable.Range(48, 9).Select(i => (char)i)).ToArray(), length);
    }
}
