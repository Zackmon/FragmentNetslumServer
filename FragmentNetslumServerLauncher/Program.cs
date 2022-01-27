using System;
using System.IO;
using System.Linq;
using System.Text;
using FragmentNetslumServer;
using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using FragmentNetslumServerWebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;

namespace FragmentNetslumServerLauncher
{
    class Program
    {

        private const int ONE_GIGABYTE = 1073741824;


        static void Main(string[] args)
        {

            // check if settings.txt exists , if not then quit application 
            if (!File.Exists("settings.ini"))
            {
                Console.Error.WriteLine("settings.ini doesn't exist");
                return;
            }


            var serviceCollection = InitializeContainer();
            var provider = serviceCollection.BuildServiceProvider();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //hack hack
            //provider.GetRequiredService<ILobbyChatService>().Initialize();
            var iocps = provider.GetRequiredService<IOpCodeProviderService>();
            var logger = provider.GetRequiredService<ILogger>();

            logger.Information("BEGIN - OPCODES");
            foreach (var t in iocps.Handlers)
            {
                logger.Information(Extensions.ConvertHandlerToString(t));
            }
            logger.Information("END - OPCODES");

            var config = provider.GetRequiredService<SimpleConfiguration>();
            var server = provider.GetRequiredService<Server>();
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
                    var cfg = new SimpleConfiguration();
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

                    // set the minimum level
                    var lobbyLogLevel = cfg.Get("lobbyloglevel");
                    var apiLogLevel = cfg.Get("apiloglevel");
                    logConfig.MinimumLevel.Is(Enum.Parse<LogEventLevel>(lobbyLogLevel));
                    logConfig.MinimumLevel.Override("Microsoft.AspNetCore", Enum.Parse<LogEventLevel>(apiLogLevel))
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails();
                    return logConfig.CreateLogger();
                })
                .AddSingleton<IClientProviderService, GameClientService>()
                .AddSingleton<IClientConnectionService, ClientConnectionService>()
                .AddSingleton<ILobbyChatService, LobbyChatService>()
                .AddSingleton<IMailService, MailService>()
                .AddSingleton<IBulletinBoardService, BulletinBoardService>()
                .AddSingleton<INewsService,NewsService>()
                .AddSingleton<IOpCodeProviderService, OpCodeProviderService>()
                .AddTransient<GameClientAsync>()
                .AddSingleton<SimpleConfiguration>()
                .AddSingleton<Server>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IServiceProvider p)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://").Append(args[0]).Append(":5000");
            return Host.CreateDefaultBuilder(args)
                .UseSerilog(logger: p.GetRequiredService<ILogger>())
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
                        services.AddSingleton(p.GetRequiredService<IMailService>());
                        services.AddSingleton(p.GetRequiredService<IBulletinBoardService>());
                        services.AddSingleton(p.GetRequiredService<INewsService>());
                        services.AddSingleton(p.GetRequiredService<SimpleConfiguration>());
                        services.AddSingleton(p.GetRequiredService<Server>());
                    });
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(sb.ToString());
                });
        }
    }



    

}
