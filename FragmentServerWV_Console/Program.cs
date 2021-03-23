using FragmentServerWV;
using FragmentServerWV.Services.Interfaces;
using FragmentServerWV.Services;
using FragmentServerWV_WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
using Serilog;
using Serilog.Formatting.Json;
using System.Linq;

namespace FragmentServerWV_Console
{
    class Program
    {

        private const int ONE_GIGABYTE = 1073741824;


        static void Main(string[] args)
        {
            var serviceCollection = InitializeContainer();
            var provider = serviceCollection.BuildServiceProvider();
            var server = provider.GetRequiredService<Server>();
            var config = provider.GetRequiredService<SimpleConfiguration>();
            server.Start();
            CreateHostBuilder(new[] { config.Get("ip") }, provider).Build().Run();
        }

        // if you modify InitializeContainer you gotta go down below
        // and add it to the ConfigureServices() if you want WebAPI to care about
        // it otherwise tough cookies.
        private static IServiceCollection InitializeContainer()
        {
            return new ServiceCollection()
                .AddTransient<ILogger>((provider) =>
                {
                    var cfg = provider.GetRequiredService<SimpleConfiguration>();
                    var logConfig = new LoggerConfiguration();

                    // configure the sinks appropriately
                    var sinks = cfg.Get("sinks")?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (sinks.Contains("console", StringComparer.OrdinalIgnoreCase))
                    {
                        logConfig.WriteTo.Console();
                    }
                    if (sinks.Contains("file", StringComparer.OrdinalIgnoreCase))
                    {
                        var path = cfg.Get("folder");
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }

                        logConfig.WriteTo.File(
                            formatter: new JsonFormatter(),
                            path: path,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromMinutes(1),
                            rollingInterval: RollingInterval.Hour,
                            rollOnFileSizeLimit: true,
                            retainedFileCountLimit: 31,
                            fileSizeLimitBytes: ONE_GIGABYTE,
                            encoding: Encoding.UTF8);
                    }
                    return logConfig.CreateLogger();
                })
                .AddSingleton<IClientProviderService, GameClientService>()
                .AddSingleton<IClientConnectionService, ClientConnectionService>()
                .AddSingleton<ILobbyChatService, LobbyChatService>()
                .AddSingleton<SimpleConfiguration>()
                .AddSingleton<Server>();
            //return services.BuildServiceProvider();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IServiceProvider p)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://").Append(args[0]).Append(":5000");
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        // if you modify InitializeContainer up above you gotta come down here
                        // and add it here if you want WebAPI to care about it otherwise tough
                        // cookies.
                        services.AddSingleton(p.GetRequiredService<IClientProviderService>());
                        services.AddSingleton(p.GetRequiredService<IClientConnectionService>());
                        services.AddSingleton(p.GetRequiredService<ILobbyChatService>());
                        services.AddSingleton(p.GetRequiredService<SimpleConfiguration>());
                        services.AddSingleton(p.GetRequiredService<Server>());
                    });
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(sb.ToString());
                });
        }
    }



    

}
