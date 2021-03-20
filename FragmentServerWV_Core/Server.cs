using FragmentServerWV.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    //public static class Server
    //{
    //    // do not do this >:(
    //    public static Thread t;
    //    public static bool _exit = false;
    //    public static bool _proxymode = false;
    //    public static string proxyIP;
    //    public static readonly object _sync = new object();
    //    public static List<GameClient> clients;
    //    public static List<ProxyClient> proxies;
    //    public static Dictionary<int,LobbyChatRoom> LobbyChatRooms;
    //    public static TcpListener listener;


    //    // TODO: Check OG C++ code to see if this is still needed
    //    public static void StartProxy(string targetIP)
    //    {
    //        proxyIP = targetIP;
    //        _proxymode = true;
    //        Start();
    //    }

    //    public static void Start()
    //    {
    //        clients = new List<GameClient>();
    //        proxies = new List<ProxyClient>();
    //        LobbyChatRooms = new Dictionary<int, LobbyChatRoom>();
    //        if (!_proxymode)
    //        {
    //            ushort lobbyID = 1;
    //            LobbyChatRooms.Add(lobbyID,new LobbyChatRoom("Main Lobby", lobbyID, 0x7403));
    //        }

    //        //DBAcess.getInstance().LoadMessageOfDay();
    //        ThreadPool.QueueUserWorkItem(new WaitCallback(MainThread));
    //        // t = new Thread(MainThread);
    //       // t.Start();
    //    }

    //    public static void Stop()
    //    {
    //        lock (_sync)
    //        {
    //            _exit = true;
    //            if (listener != null)
    //                listener.Stop();
    //        }
    //    }

    //    public static void MainThread(object obj)
    //    {
    //        string ip = Config.configs["ip"];
    //        //string any = IPAddress.Loopback.ToString();
    //        listener = new TcpListener(IPAddress.Parse(Config.configs["ip"]), Convert.ToUInt16(Config.configs["port"]));
    //        Log.Writeline("Server started on " + ip + ":" + Config.configs["port"]);
    //        Log.Writeline(" Log Size = " + Convert.ToInt32(Config.configs["logsize"]));
    //        Log.Writeline(" Ping Delay = " + Convert.ToInt32(Config.configs["ping"]) + "ms");
    //        Log.Writeline(" Proxy Mode = " + _proxymode);
    //        if (_proxymode)
    //            Log.Writeline(" Proxy Target IP = " + proxyIP);
    //        listener.Start();
    //        bool run = true;
    //        int count = 1;

    //        try
    //        {
    //            while (run)
    //            {
    //                TcpClient client = listener.AcceptTcpClient();
    //                if (_proxymode)
    //                    proxies.Add(new ProxyClient(client, count, proxyIP, Config.configs["port"]));
    //                else
    //                    clients.Add(new GameClient(client, count));
    //                Log.Writeline("New client connected with ID #" + count++);
    //                lock (_sync)
    //                {
    //                    run = !_exit;
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            throw new LobbyEmuCrashException("Server Crashed ",t,e);
    //        }
    //        finally
    //        {
    //            foreach (GameClient client in clients)
    //                client.Exit();
    //            foreach (ProxyClient client in proxies)
    //                client.Exit();
    //            Log.Writeline("Server exited");
    //        }
    //    }
    //}

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
        private readonly List<GameClient> clients;
        // private readonly List<ProxyClient> proxies;
        private readonly Dictionary<int, LobbyChatRoom> lobbyChatRooms;
        
        private readonly TcpListener listener;
        private readonly IPAddress ipAddress;
        private readonly ushort port;
        private readonly int logSize;



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

        /// <summary>
        /// Gets the theoretically connected clients
        /// </summary>
        public List<GameClient> Clients => clients;



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
            this.listener = new TcpListener(ipAddress, port);
            this.clients = new List<GameClient>();
            this.lobbyChatRooms = new Dictionary<int, LobbyChatRoom>();
            this.tokenSource = new CancellationTokenSource();
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
                    clients.Add(new GameClient(incomingClient, count++));
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
            foreach (var client in clients)
                client.Exit();
        }

    }

}
