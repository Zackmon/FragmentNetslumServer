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
    public class GameClient
    {
        public readonly object _sync = new object();
        public bool _exit = false;
        public bool _exited = false;
        public TcpClient client;
        public NetworkStream ns;
        public int index;
        public short room_index = -1;
        public Thread t;
        public Crypto to_crypto;
        public Crypto from_crypto;
        public byte[] to_key;
        public byte[] from_key;
        public ushort client_seq_nr;
        public ushort server_seq_nr;
        public bool isAreaServer;
        public byte[] ipdata;
        public byte[] publish_data_1;
        public byte[] publish_data_2;
        public byte[] last_status;
        public ushort as_usernum;

        public byte save_slot;
        public byte[] save_id;
        public byte[] char_id;
        public byte char_class;
        public ushort char_level;
        public byte[] greeting;
        public uint char_model;
        public ushort char_HP;
        public ushort char_SP;
        public uint char_GP;
        public ushort online_god_counter;
        public ushort offline_godcounter;
        public Stopwatch pingtimer;
        

        public GameClient(TcpClient c, int idx)
        {
            pingtimer = new Stopwatch();
            pingtimer.Restart();
            isAreaServer = false;
            server_seq_nr = 0xe;
            client = c;
            ns = client.GetStream();
            ns.ReadTimeout = 100;
            ns.WriteTimeout = 100;
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
            int pingdelay = Convert.ToInt32(Config.configs["ping"]);
            while (run)
            {
                lock (_sync)
                {
                    run = !_exit;
                }
                MemoryStream m;
                Packet p = new Packet(ns, from_crypto);
                if(p.datalen != 0)
                    switch (p.code)
                    {
                        case 0x0002:
                            break;
                        case OpCodes.OPCODE_KEY_EXCHANGE_REQUEST:
                            Log.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            m = new MemoryStream();
                            m.Write(p.data, 4, 16);
                            from_key = m.ToArray();
                            to_key = new byte[16];
                            Random rnd = new Random();
                            for (int i = 0; i < 16; i++)
                                to_key[i] = (byte)rnd.Next(256);
                            m = new MemoryStream();
                            m.WriteByte(0);
                            m.WriteByte(0x10);
                            m.Write(from_key, 0, 16);
                            m.WriteByte(0);
                            m.WriteByte(0x10);
                            m.Write(to_key, 0, 16);
                            m.Write(new byte[] { 0, 0, 0, 0xe, 0, 0, 0, 0, 0, 0 }, 0, 10);
                            uint checksum = Crypto.Checksum(m.ToArray());
                            SendPacket(0x35, m.ToArray(), checksum);
                            break;
                        case OpCodes.OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT:
                            Log.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            from_crypto = new Crypto(from_key);
                            to_crypto = new Crypto(to_key);
                            break;
                        case 0x30:
                            Log.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            HandlerPacket30(p.data, index, to_crypto);
                            break;
                        default:
                            Log.Writeline("Client Handler #" + index + " : Received packet with unknown code");
                            Log.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            run = false;
                            break;
                    }
                if (pingtimer.ElapsedMilliseconds > 10000)
                {
                    try
                    {
                        SendPacket30(2, new byte[0]);
                        pingtimer.Restart();
                    }
                    catch (Exception)
                    {
                        run = false;
                    }
                }
            }
            Log.Writeline("Client Handler #" + index + " exited");
            if (room_index != -1)
            {
                LobbyChatRoom room = Server.lobbyChatRooms[room_index - 1];
                room.Users.Remove(this.index);
                Log.Writeline("Lobby '" + room.name + "' now has " + room.Users.Count() + " Users");
            }
            _exited = true;
        }

        public void HandlerPacket30(byte[] data, int index, Crypto crypto)
        {
            client_seq_nr = swap16(BitConverter.ToUInt16(data, 2));
            ushort arglen = swap16(BitConverter.ToUInt16(data, 6));
            arglen -= 2;
            ushort code = swap16(BitConverter.ToUInt16(data, 8));
            MemoryStream m = new MemoryStream();
            LobbyChatRoom room;
            m.Write(data, 10, arglen);
            byte[] argument = m.ToArray();
            ushort u;
            switch (code)
            {
                case 2:
                case OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY:
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS3:
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS4:
                case 0x78A7:
                    break;
                case OpCodes.OPCODE_DATA_LOGON_REPEAT:
                    SendPacket30(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x02, 0x10 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_ENTERROOM:
                    room_index = (short)swap16(BitConverter.ToUInt16(data, 0xA));
                    room = Server.lobbyChatRooms[room_index - 1];
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_ENTERROOM_OK, BitConverter.GetBytes(swap16((ushort)room.Users.Count)));
                    room.Users.Add(this.index);
                    Log.Writeline("Client #" + this.index + " : Lobby '" + room.name + "' now has " + room.Users.Count() + " Users");
                    room.DispatchAllStatus(this.index);
                    break;
                case 0x7009:
                    Server.lobbyChatRooms[room_index - 1].DispatchStatus(argument, this.index);
                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1:
                    int end = argument.Length - 1;
                    while (argument[end] == 0)
                        end--;
                    end++;
                    m = new MemoryStream();
                    m.Write(argument, 65, end - 65);
                    publish_data_1 = m.ToArray();
                    SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1_OK, new byte[] { 0x00, 0x01 });
                    break;
                case OpCodes.OPCODE_DATA_AS_IPPORT:
                    ipdata = argument;
                    Log.Writeline("Client #" + this.index + " : IP=" +
                                  ipdata[3] + "." +
                                  ipdata[2] + "." +
                                  ipdata[1] + "." +
                                  ipdata[0] + " Port:" +
                                  swap16(BitConverter.ToUInt16(ipdata, 4)));
                    SendPacket30(OpCodes.OPCODE_DATA_AS_IPPORT_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2:
                    SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2_OK, new byte[] { 0xDE, 0xAD });
                    break;
                case OpCodes.OPCODE_DATA_LOGON_AS2:
                    SendPacket30(0x701C, new byte[] { 0x02, 0x11 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_CHATROOM_GETLIST:
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] { 0x00, 0x01, 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_UPDATE_USERNUM:
                    as_usernum = swap16(BitConverter.ToUInt16(argument, 2));
                    break;
                case OpCodes.OPCODE_DATA_DISKID:
                    SendPacket30(OpCodes.OPCODE_DATA_DISKID_OK, new byte[] { 0x78, 0x94 });
                    break;
                case OpCodes.OPCODE_DATA_SAVEID:
                    m = new MemoryStream();
                    m.Write(BitConverter.GetBytes((int)0), 0, 4);
                    byte[] buff = Encoding.ASCII.GetBytes(File.ReadAllText("welcome.txt"));
                    m.WriteByte((byte)(buff.Length - 1));
                    m.Write(buff, 0, buff.Length);
                    while (m.Length < 0x200)
                        m.WriteByte(0);

                    byte[] response = m.ToArray();
                    String responseString  = BitConverter.ToString(response);
                    
                    SendPacket30(0x742A, response);
                    break;
                case OpCodes.OPCODE_DATA_REGISTER_CHAR:
                    ExtractCharacterData(argument);
                    SendPacket30(OpCodes.OPCODE_DATA_REGISTER_CHAROK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_UNREGISTER_CHAR:
                    SendPacket30(OpCodes.OPCODE_DATA_UNREGISTER_CHAROK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_EXITROOM:
                    if (room_index != -1)
                    {
                        room = Server.lobbyChatRooms[room_index - 1];
                        room.Users.Remove(this.index);
                        Log.Writeline("Lobby '" + room.name + "' now has " + room.Users.Count() + " Users");
                    }
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_EXITROOM_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_RETURN_DESKTOP:
                    SendPacket30(OpCodes.OPCODE_DATA_RETURN_DESKTOP_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETMENU:
                    GetLobbyMenu();
                    break;
                case OpCodes.OPCODE_DATA_MAIL_SEND:
                    m = new MemoryStream();
                    while (ns.DataAvailable) m.WriteByte((byte)ns.ReadByte());
                    Log.LogData(m.ToArray(), 0xFFFF, this.index, "Recv Mail Data", 0, 0);
                    SendPacket30(OpCodes.OPCODE_DATA_MAIL_SEND_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_MAIL_GET:
                    SendPacket30(OpCodes.OPCODE_DATA_MAIL_GETOK, new byte[] { 0x00, 0x06 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_GETLIST:
                    GetServerList(argument);                    
                    break;
                case 0x771E:
                    SendPacket30(0x771F, new byte[] { 0x00, 0x00 });
                    break;
                case 0x7722:
                    u = swap16(BitConverter.ToUInt16(argument, 0));
                    if (u == 0)
                        SendPacket30(0x7723, new byte[] { 0x00, 0x00 });
                    else
                        SendPacket30(0x7725, new byte[] { 0x00, 0x00 });
                    break;
                case 0x7733:
                    u = swap16(BitConverter.ToUInt16(argument, 0));
                    if (u == 0)
                        SendPacket30(0x7734, new byte[] { 0x00, 0x00 });
                    else
                        SendPacket30(0x7737, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_UPDATE_STATUS:
                    publish_data_2 = argument;
                    break;
                case 0x780f:
                    SendPacket30(0x7810, new byte[] { 0x01, 0x92 });
                    break;
                case OpCodes.OPCODE_DATA_BBS_POST:
                    SendPacket30(0x7813, new byte[] { 0x00, 0x00 });
                    break;
                case 0x7832:
                    u = swap16(BitConverter.ToUInt16(argument, 0));
                    if (u == 0)
                        SendPacket30(0x7833, new byte[] { 0x00, 0x00 });
                    else
                        SendPacket30(0x7836, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS:
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT:
                    SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_BBS_GETMENU:
                    
                    u = swap16(BitConverter.ToUInt16(argument, 0));
                    if (u == 0)
                    {
                        SendPacket30(OpCodes.OPCODE_DATA_BBS_CATEGORYLIST, new byte[] {0x00, 0x03});

                        m = new MemoryStream();
                        
                        m.Write(BitConverter.GetBytes((int)1), 0, 2);
                        byte[] buff2 = Encoding.ASCII.GetBytes("First Category ");
                        m.WriteByte((byte)(buff2.Length - 1));
                        m.Write(buff2, 0, buff2.Length);
                        while (m.Length < 0x24)
                            m.WriteByte(0);
                        
                        SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, m.ToArray());
                        
                        m = new MemoryStream();
                        m.Write(BitConverter.GetBytes((int)2), 0, 2);
                        byte[] buff3 = Encoding.ASCII.GetBytes("this is the second category");
                        m.WriteByte((byte)(buff3.Length - 1));
                        m.Write(buff3, 0, buff3.Length);
                        
                        while (m.Length < 0x24)
                            m.WriteByte(0);

                        
                        
                        SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, m.ToArray());
                        
                        
                        m = new MemoryStream();
                        m.Write(BitConverter.GetBytes((int)3), 0, 2);
                        buff3 = Encoding.ASCII.GetBytes("third I guess :)");
                        m.WriteByte((byte)(buff3.Length - 1));
                        m.Write(buff3, 0, buff3.Length);
                        
                        while (m.Length < 0x24)
                            m.WriteByte(0);
                        
                        SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, m.ToArray());


                    }

                    else
                    {
                        Console.WriteLine("value of argument " + argument);
                        Console.WriteLine("Value of u" + u);
                        Console.WriteLine("Value of u after converting to int32 " + Convert.ToInt32(u));
                        
                        SendPacket30(OpCodes.OPCODE_DATA_BBS_THREADLIST, new byte[] { 0x00, 0x00 });    
                    }

                    
                    break;
                case OpCodes.OPCODE_DATA_NEWS_GETMENU:
                    SendPacket30(OpCodes.OPCODE_DATA_NEWS_CATEGORYLIST, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_DISKID:
                    SendPacket30(OpCodes.OPCODE_DATA_AS_DISKID_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_EVENT:
                    Server.lobbyChatRooms[room_index - 1].DispatchPublicBroadcast(argument, this.index);
                    break;
                case OpCodes.OPCODE_DATA_MAILCHECK:
                    SendPacket30(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] { 0x00, 0x01 });
                    break;
                case OpCodes.OPCODE_DATA_BBS_GET_UPDATES:
                    SendPacket30(0x786b, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_NEWCHECK:
                    SendPacket30(OpCodes.OPCODE_DATA_NEWCHECK_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_COM:
                    SendPacket30(OpCodes.OPCODE_DATA_COM_OK, new byte[] { 0xDE, 0xAD });
                    break;
                case 0x787E:
                    SendPacket30(0x787F, new byte[] { 0x00, 0x00 });
                    break;
                case 0x788C:
                    ushort destid = swap16(BitConverter.ToUInt16(argument, 2));
                    Server.lobbyChatRooms[room_index - 1].DispatchPrivateBroadcast(argument, this.index, destid);
                    break;
                case OpCodes.OPCODE_DATA_SELECT_CHAR:
                    SendPacket30(OpCodes.OPCODE_DATA_SELECT_CHAROK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_SELECT2_CHAR:
                    SendPacket30(OpCodes.OPCODE_DATA_SELECT2_CHAROK, new byte[] { 0x30, 0x30, 0x30, 0x30});
                    break;
                case OpCodes.OPCODE_DATA_LOGON:
                    if (argument[1] == 0x31)
                    {
                        Log.Writeline("Client #" + this.index + " : New Area Server Joined");
                        isAreaServer = true;
                        SendPacket30(0x78AC, new byte[] { 0xDE, 0xAD });
                    }
                    else
                    {
                        Log.Writeline("Client #" + this.index + " : New Game Client Joined");
                        SendPacket30(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x74, 0x32 });
                    }
                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH:
                    SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_OK, new byte[] { 0x00, 0x00 });
                    break;
                default:
                    Log.Writeline("Client #" + this.index + " : \n !!!UNKNOWN DATA CODE RECEIVED, PLEASE REPORT : 0x" + code.ToString("X4") + "!!!\n");
                    break;
            }
        }

        public void GetLobbyMenu()
        {
            SendPacket30(OpCodes.OPCODE_DATA_LOBBY_LOBBYLIST, BitConverter.GetBytes(swap16((ushort)Server.lobbyChatRooms.Count())));            
            foreach (LobbyChatRoom room in Server.lobbyChatRooms)
            {
                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16((ushort)room.ID)), 0, 2);
                foreach (char c in room.name)
                    m.WriteByte((byte)c);
                    m.WriteByte(0);
                m.Write(BitConverter.GetBytes(swap16((ushort)room.Users.Count())), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)(room.Users.Count() + 1))), 0, 2);
                while (((m.Length + 2) % 8) != 0)
                    m.WriteByte(0);
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_ENTRY_LOBBY, m.ToArray());
            }            
        }
        
        public void GetServerList(byte[] data)
        {
            if (data[1] == 0)
            {
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_CATEGORYLIST, new byte[] { 0x00, 0x01 });
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_CATEGORY, new byte[] { 0x00, 0x01, 0x4D, 0x41, 0x49, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6E, 0x65 });
            }
            else
            {
                ushort count = 0;
                foreach (GameClient client in Server.clients)
                    if (client.isAreaServer)
                        count++;
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_SERVERLIST, BitConverter.GetBytes(swap16(count)));
                foreach (GameClient client in Server.clients)
                    if (client.isAreaServer)
                    {
                        MemoryStream m = new MemoryStream();
                        m.WriteByte(0);
                        m.Write(client.ipdata, 0, 6);
                        byte[] buff = BitConverter.GetBytes(swap16(client.as_usernum));
                        int pos = 0;
                        while (client.publish_data_1[pos++] != 0) ;
                        pos += 4;
                        client.publish_data_1[pos++] = buff[0];
                        client.publish_data_1[pos++] = buff[1];
                        m.Write(client.publish_data_1, 0, client.publish_data_1.Length);
                        while (m.Length < 45)
                            m.WriteByte(0);
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_SERVER, m.ToArray());
                    }
            }
        }

        public void ExtractCharacterData(byte[] data)
        {
            save_slot = data[0];
            save_id = ReadByteString(data, 1);
            int pos = 1 + save_id.Length;
            char_id = ReadByteString(data, pos);
            pos += char_id.Length;
            char_class = data[pos++];
            char_level = swap16(BitConverter.ToUInt16(data, pos)); pos += 2;
            greeting = ReadByteString(data, pos);
            pos += greeting.Length;
            char_model = swap32(BitConverter.ToUInt32(data, pos)); pos += 5;
            char_HP = swap16(BitConverter.ToUInt16(data, pos)); pos += 2;
            char_SP = swap16(BitConverter.ToUInt16(data, pos)); pos += 2;
            char_GP = swap32(BitConverter.ToUInt32(data, pos)); pos += 4;
            online_god_counter = swap16(BitConverter.ToUInt16(data, pos)); pos += 2;
            offline_godcounter = swap16(BitConverter.ToUInt16(data, pos)); pos += 2;
        }

        public byte[] ReadByteString(byte[] data, int pos)
        {
            MemoryStream m = new MemoryStream();
            while (true)
            {
                byte b = data[pos++];
                m.WriteByte(b);
                if (b == 0)
                    break;
                if (pos >= data.Length)
                    break;
            }
            return m.ToArray();
        }

        public void SendPacket30(ushort code, byte[] data)
        {
            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32(server_seq_nr++)), 0, 4);
            ushort len = (ushort)(data.Length + 2);
            m.Write(BitConverter.GetBytes(swap16(len)), 0, 2);
            m.Write(BitConverter.GetBytes(swap16(code)), 0, 2);
            m.Write(data, 0, data.Length);
            uint checksum = Crypto.Checksum(m.ToArray());
            while (((m.Length + 2)& 7) != 0)
                m.WriteByte(0);
            SendPacket(OpCodes.OPCODE_DATA, m.ToArray(), checksum);
        }

        public void SendPacket(ushort code, byte[] data, uint checksum)
        {
            MemoryStream m = new MemoryStream();
            m.WriteByte((byte)(checksum >> 8));
            m.WriteByte((byte)(checksum & 0xFF));
            m.Write(data, 0, data.Length);
            byte[] buff = m.ToArray();
            Log.LogData(buff, code, index, "Send Data", (ushort)checksum, (ushort)checksum);
            buff = to_crypto.Encrypt(buff);
            ushort len = (ushort)(buff.Length + 2);
            m = new MemoryStream();
            m.WriteByte((byte)(len >> 8));
            m.WriteByte((byte)(len & 0xFF));
            m.WriteByte((byte)(code >> 8));
            m.WriteByte((byte)(code & 0xFF));
            m.Write(buff, 0, buff.Length);
            ns.Write(m.ToArray(), 0, (int)m.Length);
        }

        public static ushort swap16(ushort data)
        {
            ushort result = 0;
            result = (ushort)((data >> 8) + ((data & 0xFF) << 8));
            return result;
        }


        public static uint swap32(uint data)
        {
            uint result = 0;
            result |= (uint)((data & 0xFF) << 24);
            result |= (uint)(((data >> 8) & 0xFF) << 16);
            result |= (uint)(((data >> 16) & 0xFF) << 8);
            result |= (uint)((data >> 24) & 0xFF);
            return result;
        }
    }
}
