using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace FragmentServerWV
{
    public class ProxyClient
    {
        public readonly object _sync = new object();
        public bool _exit = false;
        public bool _exited = false;
        public TcpClient client;
        public TcpClient target;
        public NetworkStream ns_client;
        public NetworkStream ns_target;
        public int index;
        public Thread t;
        public Crypto to_crypto;
        public Crypto from_crypto;
        public byte[] to_key;
        public byte[] from_key;

        public ProxyClient(TcpClient c, int idx, string targetIP, string port)
        {
            client = c;
            ns_client = client.GetStream();
            ns_client.ReadTimeout = 100;
            ns_client.WriteTimeout = 100;
            target = new TcpClient(targetIP, Convert.ToInt32(port));
            ns_target = target.GetStream();
            ns_target.ReadTimeout = 100;
            ns_target.WriteTimeout = 100;
            index = idx;
            to_crypto = new Crypto();
            from_crypto = new Crypto();
            t = new Thread(Handler);
            t.Start();
        }

        public void Exit()
        {
            lock (_sync)
            {
                _exit = true;
            }
        }

        public void Handler(object obj)
        {
            bool run = true;
            MemoryStream m;
            Packet p_srv;
            Packet p_cln;
            while (run)
            {
                lock (_sync)
                {
                    run = !_exit;
                }
                while (true)
                {
                    p_cln = new Packet(ns_client, from_crypto);
                    if (p_cln.data != null)
                    {
                        m = new MemoryStream();
                        m.Write(p_cln.data, 2, p_cln.datalen - 2);
                        if (p_cln.code != 2)
                            SendPacket(p_cln.code, m.ToArray(), p_cln.checksum_inpacket, false);
                        if (p_cln.datalen != 0)
                        {
                            Log.LogData(p_cln.data, p_cln.code, index, "Recv Data", p_cln.checksum_inpacket, p_cln.checksum_ofpacket);
                            if (p_cln.code == 0x36)
                            {
                                from_crypto = new Crypto(from_key);
                                to_crypto = new Crypto(to_key);
                            }
                        }
                    }
                    else
                        break;
                }
                while (true)
                {
                    p_srv = new Packet(ns_target, to_crypto);
                    if (p_srv.data != null)
                    {
                        m = new MemoryStream();
                        m.Write(p_srv.data, 2, p_srv.datalen - 2);
                        if (p_srv.code != 2)
                            SendPacket(p_srv.code, m.ToArray(), p_srv.checksum_inpacket, true);
                        if (p_srv.datalen != 0)
                        {
                            Log.LogData(p_srv.data, p_srv.code, index, "Send Data", p_srv.checksum_inpacket, p_srv.checksum_ofpacket);
                            if (p_srv.code == 0x35)
                            {
                                to_key = new byte[16];
                                from_key = new byte[16];
                                for (int i = 0; i < 16; i++)
                                {
                                    to_key[i] = p_srv.data[22 + i];
                                    from_key[i] = p_srv.data[4 + i];
                                }
                            }
                        }
                    }
                    else
                        break;
                }
            }
            Log.Writeline("Client Handler #" + index + " exited");
            _exited = true;
        }

        public void SendPacket(ushort code, byte[] data, uint checksum, bool toclient)
        {
            MemoryStream m = new MemoryStream();
            m.WriteByte((byte)(checksum >> 8));
            m.WriteByte((byte)(checksum & 0xFF));
            m.Write(data, 0, data.Length);
            byte[] buff = m.ToArray();
            if (toclient)
                buff = to_crypto.Encrypt(buff);
            else
                buff = from_crypto.Encrypt(buff);
            ushort len = (ushort)(buff.Length + 2);
            m = new MemoryStream();
            m.WriteByte((byte)(len >> 8));
            m.WriteByte((byte)(len & 0xFF));
            m.WriteByte((byte)(code >> 8));
            m.WriteByte((byte)(code & 0xFF));
            m.Write(buff, 0, buff.Length);
            if (toclient)
                ns_client.Write(m.ToArray(), 0, (int)m.Length);
            else
                ns_target.Write(m.ToArray(), 0, (int)m.Length);
        }
    }
}
