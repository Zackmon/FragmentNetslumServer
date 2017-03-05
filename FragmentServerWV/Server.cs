using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    public static class Server
    {
        public static Thread t;
        public static bool _exit = false;
        public static readonly object _sync = new object();
        public static List<GameClient> clients;
        public static List<LobbyChatRoom> lobbyChatRooms;
        public static TcpListener listener;

        public static void Start()
        {
            if (t == null)
            {
                clients = new List<GameClient>();
                lobbyChatRooms = new List<LobbyChatRoom>();
                ushort count = 0;
                lobbyChatRooms.Add(new LobbyChatRoom("Main Lobby", count++, 0x7403));
                lobbyChatRooms.Add(new LobbyChatRoom("Test Lobby", count++, 0x7403));
                t = new Thread(MainThread);
                t.Start();
            }
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
            listener = new TcpListener(IPAddress.Parse(Config.configs["ip"]), Convert.ToUInt16(Config.configs["port"]));
            Log.Writeline("Server started on " + Config.configs["ip"] + ":" + Config.configs["port"]);
            Log.Writeline(" Log Size = " + Convert.ToInt32(Config.configs["logsize"]));
            Log.Writeline(" Ping Delay = " + Convert.ToInt32(Config.configs["ping"]) + "ms");
            listener.Start();
            bool run = true;
            int count = 1;
            try
            {
                while (run)
                {
                    clients.Add(new GameClient(listener.AcceptTcpClient(), count));
                    Log.Writeline("New client connected with ID #" + count++);
                    lock (_sync)
                    {
                        run = !_exit;
                    }
                }
            }
            catch (Exception)
            {
            }
            foreach (GameClient client in clients)
                client.Exit();
            Log.Writeline("Server exited");
        }
    }
}
