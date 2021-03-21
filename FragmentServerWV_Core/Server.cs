using FragmentServerWV.Exceptions;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace FragmentServerWV
{

    /// <summary>
    /// Defines the main listener loop for the .hack//fragment service
    /// </summary>
    /// <remarks>
    /// While only one global instance is supported, technically multiple instances of the class can be created
    /// </remarks>
    public sealed class Server
    {
        private const int ONE_GIGABYTE = 1073741824;

        // This is our only static variable in this class.
        // Everything else will be instance specific.
        // Maintaining state in a global static class is
        // notoriously difficult and exposing ONLY a single
        // static property at least narrows this window
        // considerably. In the future, I may suggest introducing
        // a DI framework of sorts to piece together everything
        private static Server instance;
        private readonly CancellationTokenSource tokenSource;
        private readonly Dictionary<int, LobbyChatRoom> lobbyChatRooms;
        private readonly IPAddress ipAddress;
        private readonly ushort port;

        private readonly IClientProviderService gameClientService;
        private readonly ILobbyChatService lobbyChatService;
        private readonly IClientConnectionService clientConnectionService;
        private readonly IUnityContainer container;
        


        /// <summary>
        /// Gets the globally available instance of <see cref="Server"/>
        /// </summary>
        public static Server Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Server();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the token for cancellation
        /// </summary>
        public CancellationToken CancellationToken => tokenSource.Token;

        
        /// <summary>
        /// Gets the service for handling <see cref="GameClient"/> instances
        /// </summary>
        public IClientProviderService GameClientService => gameClientService;

        /// <summary>
        /// Gets the service for handling <see cref="LobbyChatRoom"/> instances
        /// </summary>
        public ILobbyChatService LobbyChatService => lobbyChatService;



        /// <summary>
        /// Creates a new instance of <see cref="Server"/> using the information exposed from the <see cref="Config"/> class
        /// </summary>
        /// <remarks>
        /// If any of the configuration items has an invalid entry, you might see a weird error coming from the constructor. Verify the settings text file and ensure that:
        /// 1. ip is set to at least 0.0.0.0
        /// 2. port is set to a valid number in the range of 1 <= x <= 65535
        /// 3. logsize is a valid POSITIVE number. While logsize can be negative, you're in for a bad time if you put this as negative
        /// </remarks>
        public Server() : this(
            IPAddress.Parse(Config.configs["ip"]),
            Convert.ToUInt16(Config.configs["port"])) { }

        internal Server(
            IPAddress ipAddress,
            ushort port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            container = this.InitializeContainer();


            this.gameClientService = container.Resolve<IClientProviderService>();
            this.lobbyChatService = container.Resolve<ILobbyChatService>();
            this.clientConnectionService = container.Resolve<IClientConnectionService>();
            this.tokenSource = new CancellationTokenSource();
        }


        /// <summary>
        /// Starts listening for Tcp connections to the server
        /// </summary>
        public void Start()
        {
            this.clientConnectionService.BeginListening(this.ipAddress, this.port);
        }

        /// <summary>
        /// Requests for the server to stop listening
        /// </summary>
        public void Stop()
        {

            this.SafeShutdownInternal();
        }

        public void StartProxy(string targetIp) => throw new NotImplementedException("TODO: Tell formless to look at C++ code if you need this otherwise nah.");




        private void SafeShutdownInternal()
        {
            foreach (var client in GameClientService.Clients)
                client.Exit();
        }

        private IUnityContainer InitializeContainer()
        {
            return new UnityContainer()
                .RegisterFactory<ILogger>((container) =>
                {
                    var logConfig = new LoggerConfiguration();

                    // configure the sinks appropriately
                    var sinks = Config.configs["sinks"]?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (sinks.Contains("console", StringComparer.OrdinalIgnoreCase))
                    {
                        logConfig.WriteTo.Console();
                    }
                    if (sinks.Contains("file", StringComparer.OrdinalIgnoreCase))
                    {
                        var path = Config.configs["folder"];
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }

                        logConfig.WriteTo.File(
                            formatter: new JsonFormatter(),
                            path: path,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromMinutes(1),
                            rollingInterval: RollingInterval.Minute,
                            rollOnFileSizeLimit: true,
                            retainedFileCountLimit: 31,
                            encoding: Encoding.UTF8);
                    }



                    return logConfig.CreateLogger();
                }, new ContainerControlledLifetimeManager())
                .RegisterSingleton<IClientProviderService, GameClientService>()
                .RegisterSingleton<IClientConnectionService, ClientConnectionService>()
                .RegisterSingleton<ILobbyChatService, LobbyChatService>();

        }

    }

}
