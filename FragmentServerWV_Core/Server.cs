using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FragmentServerWV.Exceptions;
using FragmentServerWV.Services;

namespace FragmentServerWV
{
    public static class Server
    {
        public static Thread t;
        public static bool _exit = false;
        public static bool _proxymode = false;
        public static string proxyIP;
        public static readonly object _sync = new object();
        public static List<GameClient> clients;
        public static List<ProxyClient> proxies;
        public static Dictionary<int,LobbyChatRoom> lobbyChatRooms;
        public static TcpListener listener;

        public static void StartProxy(string targetIP)
        {
            proxyIP = targetIP;
            _proxymode = true;
            Start();
        }

        public static void Start()
        {
            clients = new List<GameClient>();
            proxies = new List<ProxyClient>();
            lobbyChatRooms = new Dictionary<int, LobbyChatRoom>();
            if (!_proxymode)
            {
                ushort lobbyID = 1;
                lobbyChatRooms.Add(lobbyID,new LobbyChatRoom("Main Lobby", lobbyID, 0x7403));
            }

            DBAcess.getInstance().LoadMessageOfDay();
            ThreadPool.QueueUserWorkItem(new WaitCallback(MainThread));
            // t = new Thread(MainThread);
           // t.Start();
        }

        public static void Stop()
        {
            lock (_sync)
            {
                _exit = true;
                if (listener != null)
                    listener.Stop();
            }
        }

        public static void MainThread(object obj)
        {
            string ip = Config.configs["ip"];
            //string any = IPAddress.Loopback.ToString();
            listener = new TcpListener(IPAddress.Parse(Config.configs["ip"]), Convert.ToUInt16(Config.configs["port"]));
            Log.Writeline("Server started on " + ip + ":" + Config.configs["port"]);
            Log.Writeline(" Log Size = " + Convert.ToInt32(Config.configs["logsize"]));
            Log.Writeline(" Ping Delay = " + Convert.ToInt32(Config.configs["ping"]) + "ms");
            Log.Writeline(" Proxy Mode = " + _proxymode);
            if (_proxymode)
                Log.Writeline(" Proxy Target IP = " + proxyIP);
            listener.Start();
            bool run = true;
            int count = 1;

            try
            {
                while (run)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    if (_proxymode)
                        proxies.Add(new ProxyClient(client, count, proxyIP, Config.configs["port"]));
                    else
                        clients.Add(new GameClient(client, count));
                    Log.Writeline("New client connected with ID #" + count++);
                    lock (_sync)
                    {
                        run = !_exit;
                    }
                }
            }
            catch (Exception e)
            {
                throw new LobbyEmuCrashException("Server Crashed ",t,e);
            }
            finally
            {
                foreach (GameClient client in clients)
                    client.Exit();
                foreach (ProxyClient client in proxies)
                    client.Exit();
                Log.Writeline("Server exited");
            }
        }
    }
}
