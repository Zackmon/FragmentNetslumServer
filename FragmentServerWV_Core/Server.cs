using FragmentServerWV.Exceptions;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly CancellationTokenSource tokenSource;
        private readonly IPAddress ipAddress;
        private readonly ushort port;

        private readonly IClientProviderService gameClientService;
        private readonly ILobbyChatService lobbyChatService;
        private readonly IClientConnectionService clientConnectionService;
        


        /// <summary>
        /// Gets the globally available instance of <see cref="Server"/>
        /// </summary>
        public static Server Instance { get; private set; }

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



        public Server(
            IClientProviderService gameClientService,
            ILobbyChatService lobbyChatService,
            IClientConnectionService clientConnectionService,
            SimpleConfiguration configuration)
        {
            Instance = Instance ?? this;
            IPAddress.TryParse(configuration.Get("ip"), out this.ipAddress);
            ushort.TryParse(configuration.Get("port"), out this.port);


            this.gameClientService = gameClientService;
            this.lobbyChatService = lobbyChatService;
            this.clientConnectionService = clientConnectionService;
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

    }

}
