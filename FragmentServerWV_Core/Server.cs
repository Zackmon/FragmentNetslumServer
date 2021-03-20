using FragmentServerWV.Exceptions;
using FragmentServerWV.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        // This is our only static variable in this class.
        // Everything else will be instance specific.
        // Maintaining state in a global static class is
        // notoriously difficult and exposing ONLY a single
        // static property at least narrows this window
        // considerably. In the future, I may suggest introducing
        // a DI framework of sorts to piece together everything
        private static Server instance;


        private readonly CancellationTokenSource tokenSource;
        // private readonly List<ProxyClient> proxies;
        private readonly Dictionary<int, LobbyChatRoom> lobbyChatRooms;
        
        private readonly TcpListener listener;
        private readonly IPAddress ipAddress;
        private readonly ushort port;
        private readonly int logSize;

        private readonly Services.GameClientService gameClientService;



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
        /// Gets the lobby rooms
        /// </summary>
        public Dictionary<int, LobbyChatRoom> LobbyChatRooms => lobbyChatRooms;

        ///// <summary>
        ///// Gets the theoretically connected clients
        ///// </summary>
        //private ReadOnlyCollection<GameClient> Clients => GameClientService.Clients;

        /// <summary>
        /// Gets the service for handling <see cref="GameClient"/> instances
        /// </summary>
        public GameClientService GameClientService => gameClientService;



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
            Convert.ToUInt16(Config.configs["port"]),
            Convert.ToInt32(Config.configs["logsize"]))
        {

        }

        internal Server(
            IPAddress ipAddress,
            ushort port,
            int logSize)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.logSize = logSize;
            this.gameClientService = new Services.GameClientService();
            this.listener = new TcpListener(ipAddress, port);
            this.lobbyChatRooms = new Dictionary<int, LobbyChatRoom>();
            this.tokenSource = new CancellationTokenSource();
            this.lobbyChatRooms.Add(1, new LobbyChatRoom("Main Lobby", 1, 0x7403));
            // this.proxies = new List<ProxyClient>();
        }


        /// <summary>
        /// Starts listening for Tcp connections to the server
        /// </summary>
        public void Start()
        {
            listener.Start();
            Task.Run(async () => await ListenForConnections());
        }

        /// <summary>
        /// Requests for the server to stop listening
        /// </summary>
        public void Stop()
        {
            tokenSource.Cancel();
        }
        public void StartProxy(string targetIp) => throw new NotImplementedException("TODO: Tell formless to look at C++ code if you need this otherwise nah.");



        private async Task ListenForConnections()
        {
            var logSb = new StringBuilder()
                .AppendLine($"Listening on: {ipAddress}:{port}")
                .AppendLine($"Log Size: {logSize:N0}")
                .AppendLine($"Ping Delay: {Config.configs["ping"]}ms");
            Log.Writeline(logSb.ToString());

            var count = 1;
            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    var incomingClient = await listener.AcceptTcpClientAsync(); // its magic but infectious magic
                    GameClientService.AddClient(new GameClient(incomingClient, count++));
                    Log.Writeline($"New client connected with ID #{count:N0}");
                    CancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException oce)
            {
                // safe shutdown
                Log.Writeline("Safe Shutdown initiated");
                SafeShutdownInternal();
            }
            catch (Exception e)
            {
                // probably FUBAR
                Log.Writeline("An unexpected issue arose and is taking down the server");
                Log.Writeline(e.Message);
                Log.Writeline(e.StackTrace);
                SafeShutdownInternal();
            }
            finally
            {
                listener.Stop();
            }
        }


        private void SafeShutdownInternal()
        {
            foreach (var client in GameClientService.Clients)
                client.Exit();
        }

    }

}
