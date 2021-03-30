using FragmentServerWV.Models;
using FragmentServerWV.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
        public IPEndPoint ipEndPoint;
        public byte[] ipdata;
        public byte[] externalIPAddress;
        public byte[] publish_data_1;
        public byte[] publish_data_2;
        public byte[] last_status;
        public ushort as_usernum;
        public byte[] areaServerName;
        public ushort areaServerLevel;
        public byte areaServerStatus;

        public int AccountId;
        public byte save_slot;
        public byte[] save_id;
        public byte[] char_name;
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
        public ushort goldCoinCount;
        public ushort silverCoinCount;
        public ushort bronzeCoinCount;
        public char classLetter;
        public int modelNumber;
        public char modelType;
        public string colorCode;
        public string charModelFile;
        
        public Stopwatch pingtimer;
        
        
        
        
        private Encoding _encoding;

        private uint _characterPlayerID = 0;
        
        private ushort _guildID = 0;
        private bool isGuildMaster = false;
        
        private uint _itemDontationID = 0;
        private ushort _itemDonationQuantity = 0 ;

        private ushort currentGuildInvitaionSelection = 0;

        private ushort _rankingCategoryID = 0;

        private ushort lobbyType = 0;

        private readonly ILogger logger;

        private readonly int ping;


        public GameClient(TcpClient c, int idx, ILogger logger, SimpleConfiguration simpleConfiguration)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");
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
            ipEndPoint = (IPEndPoint) client.Client.RemoteEndPoint;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Handler));
            this.logger = logger;
            int.TryParse(simpleConfiguration.Get("ping", "5000"), out ping);
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
            while (run)
            {
                lock (_sync)
                {
                    run = !_exit;
                }

                MemoryStream m;
                Packet p = new Packet(ns, from_crypto);
                if (p.datalen != 0)
                    switch (p.code)
                    {
                        case 0x0002:
                            break;
                        case OpCodes.OPCODE_KEY_EXCHANGE_REQUEST:
                            logger.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            m = new MemoryStream();
                            m.Write(p.data, 4, 16);
                            from_key = m.ToArray();
                            to_key = new byte[16];
                            Random rnd = new Random();
                            for (int i = 0; i < 16; i++)
                                to_key[i] = (byte) rnd.Next(256);
                            m = new MemoryStream();
                            m.WriteByte(0);
                            m.WriteByte(0x10);
                            m.Write(from_key, 0, 16);
                            m.WriteByte(0);
                            m.WriteByte(0x10);
                            m.Write(to_key, 0, 16);
                            m.Write(new byte[] {0, 0, 0, 0xe, 0, 0, 0, 0, 0, 0}, 0, 10);
                            uint checksum = Crypto.Checksum(m.ToArray());
                            SendPacket(OpCodes.OPCODE_KEY_EXCHANGE_RESPONSE, m.ToArray(), checksum);
                            break;
                        case OpCodes.OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT:
                            logger.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            
                            from_crypto = new Crypto(from_key);
                            to_crypto = new Crypto(to_key);
                            break;
                        case OpCodes.OPCODE_DATA:
                            logger.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
                            HandlerPacket30(p.data, index, to_crypto);
                            break;
                        default:
                            logger.Information("Client Handler #" + index + " : Received packet with unknown code");
                            logger.LogData(p.data, p.code, index, "Recv Data", p.checksum_inpacket, p.checksum_ofpacket);
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

            logger.Information("Client Handler #" + index + " exited");
            if (room_index != -1)
            {
                if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out var room))
                {
                    room.ClientLeavingRoom(this.index);
                    room.Users.Remove(this.index);
                    logger.Information("Lobby '" + room.name + "' now has " + room.Users.Count() + " Users");
                }
            }

            _exited = true;
            client.Close();

            if (_characterPlayerID != 0)
            {
                DBAcess.getInstance().setPlayerAsOffline(_characterPlayerID);
            }

            // this should also work
            Server.Instance.GameClientService.RemoveClient((uint)index);

            //for (int i = 0; i < Server.Instance.GameClientService.Clients.Count; i++)
            //{
            //    if (Server.Instance.Clients[i].index == this.index)
            //    {
            //        Server.Instance.Clients.RemoveAt(i);
            //        break;
            //    }
            //}

        }

        public void HandlerPacket30(byte[] data, int index, Crypto crypto)
        {
            try
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
                logger.LogData(data, code, index, "Recv Data 0X30", 0, 0);
                switch (code)
                {
                    case 2:
                    case OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY:
                    case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS3:
                    case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS4:
                    case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS6:
                        break;
                    case OpCodes.OPCODE_DATA_LOGON_REPEAT:
                        SendPacket30(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] {0x02, 0x10});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_ENTERROOM:
                        logger.LogData(argument, OpCodes.OPCODE_DATA_LOBBY_ENTERROOM, this.index, "Lobby Login", 0, 0);
                        room_index = (short) swap16(BitConverter.ToUInt16(argument, 0));
                        lobbyType = swap16(BitConverter.ToUInt16(argument, 2));
                        logger.Information("Lobby Room ID: {@room_index}", room_index);
                        logger.Information("Lobby Type ID: {@lobbyType}", lobbyType);
                        
                        if (lobbyType == OpCodes.LOBBY_TYPE_GUILD) //Guild Room
                        {
                            //TODO add Guild Specific Code
                            room = Server.Instance.LobbyChatService.GetOrAddLobby((ushort)room_index, "Guild Room", OpCodes.LOBBY_TYPE_GUILD, out var _);
                        }
                        else
                        {
                            Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out room);
                        }
                        
                        
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_ENTERROOM_OK,
                        BitConverter.GetBytes(swap16((ushort) room.Users.Count)));
                        room.Users.Add(this.index);
                        logger.Information("Client #" + this.index + " : Lobby '" + room.name + "' now has " +
                                      room.Users.Count() + " Users");
                        room.DispatchAllStatus(this.index);
                        break;
                    case 0x7009:
                        if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out var rm))
                        {
                            rm.DispatchStatus(argument, this.index);
                        }
                        break;
                    case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1:
                        int end = argument.Length - 1;
                        while (argument[end] == 0)
                            end--;
                        end++;
                        m = new MemoryStream();
                        m.Write(argument, 65, end - 65);
                        publish_data_1 = m.ToArray();
                        SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1_OK, new byte[] {0x00, 0x01});
                        break;
                    case OpCodes.OPCODE_DATA_AS_IPPORT:
                        ipdata = argument;

                        var localIpAddress = "";
                        var externalIpAddress = ipEndPoint.Address.ToString();

                        if (externalIpAddress == Helpers.IPAddressHelpers.LOOPBACK_IP_ADDRESS)
                        {
                            localIpAddress = externalIpAddress;
                            externalIpAddress = Helpers.IPAddressHelpers.GetLocalIPAddress2();
                        }

                        string[] ipAddress = externalIpAddress.Split('.');
                        argument[3] = byte.Parse(ipAddress[0]);
                        argument[2] = byte.Parse(ipAddress[1]);
                        argument[1] = byte.Parse(ipAddress[2]);
                        argument[0] = byte.Parse(ipAddress[3]);
                        externalIPAddress = argument;

                        logger.Information("Area Server Client #" + this.index + " : Local IP=" +
                                      ipdata[3] + "." +
                                      ipdata[2] + "." +
                                      ipdata[1] + "." +
                                      ipdata[0] + " Port:" +
                                      swap16(BitConverter.ToUInt16(ipdata, 4)));

                        logger.Information("Area Server Client #" + this.index + " : External IP=" +
                                      externalIPAddress[3] + "." +
                                      externalIPAddress[2] + "." +
                                      externalIPAddress[1] + "." +
                                      externalIPAddress[0] + " Port:" +
                                      swap16(BitConverter.ToUInt16(externalIPAddress, 4)));

                        SendPacket30(OpCodes.OPCODE_DATA_AS_IPPORT_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2:
                        SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2_OK, new byte[] {0xDE, 0xAD});
                        break;
                    case OpCodes.OPCODE_DATA_LOGON_AS2:
                        SendPacket30(0x701C, new byte[] {0x02, 0x11});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_CHATROOM_GETLIST:
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY,
                            new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] {0x00, 0x01, 0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_AS_UPDATE_USERNUM:
                        as_usernum = swap16(BitConverter.ToUInt16(argument, 2));
                        break;
                    case OpCodes.OPCODE_DATA_DISKID:
                        SendPacket30(OpCodes.OPCODE_DATA_DISKID_OK, new byte[] {0x36, 0x36, 0x31, 0x36});
                        break;
                    case OpCodes.OPCODE_DATA_SAVEID:
                        logger.Debug($"Data Save ID Arguments: {BitConverter.ToString(argument)}");
                        byte[] saveID = ReadByteString(argument, 0);
                        this.save_id = saveID;
                        m = new MemoryStream();

                        AccountId = DBAcess.getInstance().GetPlayerAccountId(_encoding.GetString(saveID));

                        uint swapped = swap32((uint) AccountId);

                        m.Write(BitConverter.GetBytes(swapped), 0, 4);
                        byte[] buff = _encoding.GetBytes(DBAcess.getInstance().MessageOfTheDay);
                        m.WriteByte((byte) (buff.Length - 1));
                        m.Write(buff, 0, buff.Length);
                        while (m.Length < 0x200)
                            m.WriteByte(0);

                        byte[] response = m.ToArray();
                        String responseString = BitConverter.ToString(response);

                        SendPacket30(0x742A, response);
                        break;
                    case OpCodes.OPCODE_DATA_REGISTER_CHAR:
                        logger.LogData(argument, 0xFFFF, this.index, "character data", 0, 0);
                        _characterPlayerID = ExtractCharacterData(argument);

                        byte[] guildStatus = GuildManagementService.GetInstance().GetPlayerGuild(_characterPlayerID);
                        if (guildStatus[0] == 0x01)
                        {
                            isGuildMaster = true;
                        }

                        _guildID = swap16(BitConverter.ToUInt16(guildStatus, 1));

                        SendPacket30(OpCodes.OPCODE_DATA_REGISTER_CHAROK,
                            guildStatus); //first byte is membership status 0=none 1= master 2= member
                        break;
                    case OpCodes.OPCODE_DATA_UNREGISTER_CHAR:
                        SendPacket30(OpCodes.OPCODE_DATA_UNREGISTER_CHAROK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_EXITROOM:
                        if (room_index != -1)
                        {
                            //room = Server.Instance.LobbyChatRooms[room_index];
                            if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out room))
                            {
                                room.ClientLeavingRoom(this.index);
                                room.Users.Remove(this.index);
                                logger.Information("Lobby '" + room.name + "' now has " + room.Users.Count() + " Users");
                            }
                        }

                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_EXITROOM_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_RETURN_DESKTOP:
                        DBAcess.getInstance().setPlayerAsOffline(_characterPlayerID);
                        SendPacket30(OpCodes.OPCODE_DATA_RETURN_DESKTOP_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_GETMENU:
                        GetLobbyMenu();
                        break;
                    case OpCodes.OPCODE_DATA_MAIL_SEND:
                        // Empty out the stream
                        m = new MemoryStream();
                        while (ns.DataAvailable) m.WriteByte((byte) ns.ReadByte());

                        SaveMailData(argument);

                        SendPacket30(OpCodes.OPCODE_DATA_MAIL_SEND_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_MAIL_GET:
                        logger.LogData(argument, 0xFFFF, this.index, "ACCOUNT ID FOR MAIL HEADER", 0, 0);

                        List<MailMetaModel> metaList = DBAcess.getInstance().GetAccountMail(ReadAccountID(argument, 0));

                        //send the count for the mail 
                        SendPacket30(OpCodes.OPCODE_DATA_MAIL_GETOK,
                            BitConverter.GetBytes(swap32((uint) metaList.Count)));

                        // iterate through the mails 
                        if (metaList.Count > 0)
                        {
                            foreach (MailMetaModel meta in metaList)
                            {
                                SendPacket30(OpCodes.OPCODE_DATA_MAIL_GET_NEWMAIL_HEADER, GetMailMeta(meta));
                            }
                        }


                        break;
                    case OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY:
                        logger.LogData(argument, 0xFFFF, this.index, "ID FOR MAIL BODY", 0, 0);

                        int mailID = (int) swap32(BitConverter.ToUInt32(argument, 4));

                        logger.Information("Mail ID:{@mailID}", mailID);

                        MailBodyModel bodyModel = DBAcess.getInstance().GetMailBodyByMailId(mailID);

                        SendPacket30(OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY_RESPONSE, GetMailBody(bodyModel));

                        break;

                    case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_GETLIST:
                        GetServerList(argument);
                        break;
                    case 0x771E:
                        SendPacket30(0x771F, new byte[] {0x00, 0x00});
                        break;
                    case 0x7722: // GUILD Shop
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        if (u == 0)
                        {
                            SendPacket30(0x7723, new byte[] {0x00, 0x01});
                            SendPacket30(0x7725, new byte[] {0x00,0x01, 0x41, 0x6c, 0x6c,0x00});
                        }
                        else
                        {
                            List<byte[]> listOfGuilds = GuildManagementService.GetInstance().GetListOfGuilds();
                            SendPacket30(0x7726, BitConverter.GetBytes(swap16((ushort) listOfGuilds.Count)));
                            foreach (var guildName in listOfGuilds)
                            {
                                SendPacket30(0x7727, guildName);                                
                            }

                        }
                        
                        break;
                    case 0x772f: // Get the Item List for the Selected Guild (General Store)
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        List<byte[]> listOfItemsForGeneralStore =
                            GuildManagementService.GetInstance().GetGuildItems(u, true);
                        
                        SendPacket30(0x7730, BitConverter.GetBytes(swap16((ushort) listOfItemsForGeneralStore.Count)));

                        foreach (var item in listOfItemsForGeneralStore)
                        {
                            SendPacket30(0x7731, item);
                        }
                        
                        break;
                        
                    case 0x7733: //Guild Menu
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        if (u == 0)// Guild Category List
                        {
                            SendPacket30(0x7734, new byte[] {0x00, 0x01}); //Size of List
                            SendPacket30(0x7736,new byte[]{0x00,0x01,0x41,0x6c,0x6c,0x00});//Category Name (ALL)
                        }
                        else // Guild Listing of the selected Category
                        {
                            List<byte[]> listOfGuild = GuildManagementService.GetInstance().GetListOfGuilds();
                            SendPacket30(0x7737, BitConverter.GetBytes(swap16((ushort) listOfGuild.Count))); //Size of List

                            foreach (var guildName in listOfGuild)
                            {
                                SendPacket30(0x7738,guildName);    
                            }
                            
                        }
                        break;
                    case 0x7739: // Get Guild Info
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        logger.Information("Guild ID: {@u}", u);
                        SendPacket30(0x7740,GuildManagementService.GetInstance().GetGuildInfo(u));
                        break;
                    case 0x7600: //create Guild
                        logger.LogData(argument,0xffff,this.index,"Create Guild Info",0,0);
                        u = GuildManagementService.GetInstance().CreateGuild(argument,_characterPlayerID);
                        _guildID = u;
                        isGuildMaster = true;
                        
                        SendPacket30(0x7601,BitConverter.GetBytes(swap16(u))); // send guild ID
                        
                        break;
                     case 0x789c: // get the logged in character Guild Info (if enlisted)
                         u = swap16(BitConverter.ToUInt16(argument, 0));

                         SendPacket30(0x789d,GuildManagementService.GetInstance().GetGuildInfo(u));
                         break;
                    case 0x7610: //get Guild member list
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        if (u == 0)// Guild Member Category List
                        {
                            List<byte[]> listOfClasses = GuildManagementService.GetInstance().GetClassList();
                            SendPacket30(0x7611, BitConverter.GetBytes(swap16((ushort)listOfClasses.Count))); //Size of List
                            foreach (var className in listOfClasses)
                            {
                                SendPacket30(0x7613,className);// send categories    
                            }
                             
                        }
                        else //MemberList in that Category
                        {
                            List<byte[]> memberList =
                                GuildManagementService.GetInstance().GetGuildMembersListByClass(_guildID, u,_characterPlayerID);
                            SendPacket30(0x7614, BitConverter.GetBytes(swap16((ushort) memberList.Count)));//Size of List

                            foreach (var member in memberList)
                            {
                                SendPacket30(0x7615,member); //Member Details    
                            }
                            
                        }
                        break;
                    case 0x7708: // Get Guild Items for members to buy from 
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        List<byte[]> membersItemList = GuildManagementService.GetInstance().GetGuildItems(u, false);
                        SendPacket30(0x7709 , BitConverter.GetBytes(swap16((ushort) membersItemList.Count))); // number of items

                        foreach (var item in membersItemList)
                        {
                            SendPacket30(0x770a,item);
                        }
                        
                        break;
                    case 0x7728: //Guild Item List
                        u = swap16(BitConverter.ToUInt16(argument,0));
                        List<byte[]> allGuildItems =
                            GuildManagementService.GetInstance().GetAllGuildItemsWithSettings(u);
                        SendPacket30(0x7729,BitConverter.GetBytes(swap16((ushort) allGuildItems.Count)));

                        foreach (var item in allGuildItems)
                        {
                            SendPacket30(0x772A,item);
                        }
                        
                        break;
                    case 0x770C: //buy Item from guild 
                        SendPacket30(0x770D,GuildManagementService.GetInstance().BuyItemFromGuild(argument) ); // how many to give the player 
                        break;
                    case 0x7702: // Donate Item to Guild
                        logger.LogData(argument,0xfff,this.index,"Item Donated to Guild",0,0);

                         _itemDontationID = swap32(BitConverter.ToUInt32(argument, 2));
                         _itemDonationQuantity = swap16(BitConverter.ToUInt16(argument, 6));
                        
                        Console.WriteLine("Item ID For Donation " + _itemDontationID);
                        Console.WriteLine("Item Quantity For Donation " + _itemDonationQuantity);
                        
                        SendPacket30(0x7704,GuildManagementService.GetInstance().GetPriceOfItemToBeDonated(_guildID,_itemDontationID));
                        break;
                    case 0x7879:
                        logger.LogData(argument,0xfff,this.index,"Get Member and General Screen Permission",0,0);
                        
                        SendPacket30(0x787a,GuildManagementService.GetInstance().GetItemDonationSettings(isGuildMaster));
                        
                        break;
                    case 0x7703:
                        logger.LogData(argument,0x7703,this.index,"Member + General Item Price",0,0);
                        uint GeneralPrice = swap32(BitConverter.ToUInt32(argument, 0));
                        uint MemberPrice = swap32(BitConverter.ToUInt32(argument, 4));
                        bool isGeneral = BitConverter.ToBoolean(argument, 8);
                        bool isMember = BitConverter.ToBoolean(argument, 9);
                        
                        Console.Write("GenePrice " + GeneralPrice +"\nMemberPrice "+ MemberPrice+"\nisGeneral "+ isGeneral + "\nisMember "+ isMember);
                        
                        
                        SendPacket30(0x7705,GuildManagementService.GetInstance().AddItemToGuildInventory(_guildID,_itemDontationID,
                            _itemDonationQuantity, GeneralPrice, MemberPrice, isGeneral, isMember,isGuildMaster)); // how many to deduct from the player
                        break;
                    case 0x7712: //update item pricing (from Master window)
                        logger.LogData(argument,0x7712,this.index,"Member + General Item Price",0,0);
                        
                        SendPacket30(0x7713,GuildManagementService.GetInstance().SetItemVisibilityAndPrice(argument)); 
                        break;
                    case 0x787B:// no idea what this is but I think it's only ACK
                        logger.LogData(argument,0x787B,this.index,"After Selecting the member and general",0,0);
                        SendPacket30(0x787C,new byte[] {0x00,0x00});
                        break;
                    case 0x788D:// Leve Guild and assign someone else the master of the guild
                        uint assigningPlayerID = swap32(BitConverter.ToUInt32(argument, 0));
                        Console.WriteLine("Player to assign the Master Guild to " + assigningPlayerID);
                        SendPacket30(0x788E,GuildManagementService.GetInstance().LeaveGuildAndAssignMaster(_guildID,assigningPlayerID));
                        break;
                    case 0x7616: //Player leaving the guild
                        SendPacket30(0x7617,GuildManagementService.GetInstance().LeaveGuild(_guildID,_characterPlayerID));
                        break;
                    case 0x7864: //kick player from guild
                        uint playerToKick = swap32(BitConverter.ToUInt32(argument, 0));
                        Console.WriteLine("Player to kick from guild " + playerToKick);
                        SendPacket30(0x7865,GuildManagementService.GetInstance().KickPlayerFromGuild(_guildID,playerToKick));
                        break;
                    case 0x7619: // Dissolve the guild
                        SendPacket30(0x761A,GuildManagementService.GetInstance().DestroyGuild(_guildID));
                        break;
                    case 0x761C: // Update Guild Emblem and Comment
                        
                        SendPacket30(0x761D,GuildManagementService.GetInstance().UpdateGuildEmblemComment(argument,_guildID));
                        break;
                    case 0x770E: // Take out GP from the Guild Inventory
                        ushort guildIDTakeMoney = swap16(BitConverter.ToUInt16(argument, 0));
                        uint amountOfMoneyToTakeOut = swap32(BitConverter.ToUInt32(argument, 2));
                        
                        Console.WriteLine("Guild ID " + guildIDTakeMoney + "\nAmount of money to Take out " + amountOfMoneyToTakeOut);
                        SendPacket30(0x770F,GuildManagementService.GetInstance().TakeMoneyFromGuild(guildIDTakeMoney,amountOfMoneyToTakeOut)); // amount of money to give to the player 
                        break;
                    case 0x7710:// take item from the guild inventory
                        ushort guildIDTakeItem = swap16(BitConverter.ToUInt16(argument, 0));
                        uint itemIDToTakeOut = swap32(BitConverter.ToUInt32(argument, 2));
                        ushort quantityToTake = swap16(BitConverter.ToUInt16(argument, 6));
                        
                        Console.WriteLine("Guild ID " + guildIDTakeItem + "\nItem ID to take " + itemIDToTakeOut + "\n quantity to take out " + quantityToTake);
                        SendPacket30(0x7711,GuildManagementService.GetInstance().TakeItemFromGuild(guildIDTakeItem,itemIDToTakeOut,quantityToTake)); // quantity  to give to the player
                        break;
                    case 0x7700:// donate Coins to Guild
                        //logger.LogData(argument,0xff,this.index,"Donate Coin to guild",0,0);
                        
                        SendPacket30(0x7701,GuildManagementService.GetInstance().DonateCoinsToGuild(argument));
                        break;
                    
                    case OpCodes.OPCODE_INVITE_TO_GUILD: //invite player to Guild
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        //Server.Instance.LobbyChatRooms[room_index].GuildInvitation(argument, this.index, u,_guildID);
                        if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out var guildLobby))
                        {
                            guildLobby.GuildInvitation(argument, this.index, u, _guildID);
                            Console.WriteLine("Invited Player ID " + u);
                            SendPacket30(0x7604, new byte[] { 0x00, 0x00 }); //send to confirm that the player accepted the invite 
                        }
                        break;
                    case OpCodes.OPCODE_ACCEPT_GUILD_INVITE: //accept Guild Invitation
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        logger.LogData(argument,0x7607,this.index,"guild invitation acceptance",0,0);
                        m = new MemoryStream();
                        m.Write(new byte[] {0x76, 0xB0, 0x54,0x45,0x53,0x54,0x00});
                        if (argument[1] == 0x08) //accepted the invitation
                        {
                            DBAcess.getInstance().EnrollPlayerInGuild(currentGuildInvitaionSelection,
                                _characterPlayerID, false);
                            
                            SendPacket30(0x760A,m.ToArray()); // send guild ID
                          
                        }
                        else
                        {
                            SendPacket30(0x760A,m.ToArray()); // send guild ID
                        }

                        break;
                    case OpCodes.OPCODE_GUILD_VIEW: //get Guild info (in lobby )
                         u = swap16(BitConverter.ToUInt16(argument, 0));
                         currentGuildInvitaionSelection = u;
                        SendPacket30(0x772D,GuildManagementService.GetInstance().GetGuildInfo(u));
                        break;
                    case OpCodes.OPCODE_DATA_AS_UPDATE_STATUS:
                        publish_data_2 = argument;
                        ExtractAreaServerData(argument);
                        break;
                    
                    case 0x780f: // Create Thread Request 
                        SendPacket30(0x7810, new byte[] {0x01, 0x92});
                        break;
                    case OpCodes.OPCODE_DATA_BBS_POST:

                        uint id = swap32(BitConverter.ToUInt32(argument, 0));

                        logger.LogData(argument, 0xFFFF, this.index, "BBS_POST_ARG,", 0, 0);

                        DBAcess.getInstance().CreateNewPost(argument, id);

                        SendPacket30(0x7813, new byte[] {0x00, 0x00});
                        break;

                    case OpCodes.OPCODE_RANKING_VIEW_ALL: // ranking Page
                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        if (u == 0) // get the first ranking page
                        {
                            List<byte[]> rankCategoryList = RankingManagementService.GetInstance().GetRankingCategory();
                            SendPacket30(0x7833, BitConverter.GetBytes(swap16((ushort) rankCategoryList.Count)));

                            foreach (var category in rankCategoryList)
                            {
                                SendPacket30(0x7835, category);    
                            }
                            
                        }

                        else if (u >= 8) // get class List
                        {
                            _rankingCategoryID = u;
                            List<byte[]> rankClassList = RankingManagementService.GetInstance().GetClassList();
                            
                            SendPacket30(0x7833, BitConverter.GetBytes(swap16((ushort) rankClassList.Count)));

                            foreach (var category in rankClassList)
                            {
                                SendPacket30(0x7835, category);    
                            }
                        }
                        else
                        {
                            List<byte[]> playerRankingList = RankingManagementService.GetInstance()
                                .GetPlayerRanking(_rankingCategoryID, u);
                            
                            
                            SendPacket30(0x7836, BitConverter.GetBytes(swap32((uint)playerRankingList.Count)));

                            foreach (var player in playerRankingList)
                            {
                                SendPacket30(0x7837, player);   
                            }
                           
                        }

                        break;
                    
                    case OpCodes.OPCODE_RANKING_VIEW_PLAYER: //Ranking Char Detail
                        uint rankPlayerID = swap32(BitConverter.ToUInt32(argument, 0));
                        
                        SendPacket30(0x7839,RankingManagementService.GetInstance().getRankingPlayerInfo(rankPlayerID));
                        break;

                    case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS:
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT:
                        SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_BBS_GETMENU:

                        u = swap16(BitConverter.ToUInt16(argument, 0));
                        if (u == 0)
                        {
                            /*
                             * Expected  values are
                             * Category ID
                             * Category Name
                             */

                            List<BbsCategoryModel> categoryList = DBAcess.getInstance().GetListOfBbsCategory();
                            // how many categories to be expected 
                            SendPacket30(OpCodes.OPCODE_DATA_BBS_CATEGORYLIST,
                                BitConverter.GetBytes(swap16((ushort) categoryList.Count)));

                            foreach (var category in categoryList)
                            {
                                SendCategoryDetails(m, category);
                            }
                        }

                        else
                        {
                            /*
                             *  Returning the Threads of the selected Category
                             *
                             * The Thread Consist of 
                             * Thread ID
                             * Thread Title
                             */

                            // assuming cat 1 has 5 threads , cat 2 has 3 threads , cat 3 has 1 thread 
                            int categoryID = Convert.ToInt32(u);

                            List<BbsThreadModel> threadsList = DBAcess.getInstance().getThreadsByCategoryID(categoryID);

                            SendPacket30(OpCodes.OPCODE_DATA_BBS_THREADLIST,
                                BitConverter.GetBytes(swap16((ushort) threadsList.Count)));

                            foreach (var thread in threadsList)
                            {
                                SendThreads(m, thread);
                            }
                        }


                        break;
                    case OpCodes.OPCODE_DATA_BBS_THREAD_GETMENU:
                        uint i = swap32(BitConverter.ToUInt32(argument, 0));


                        int threadID = Convert.ToInt32(i);

                        List<BbsPostMetaModel> postMetaList = DBAcess.getInstance().GetPostsMetaByThreadId(threadID);


                        SendPacket30(OpCodes.OPCODE_DATA_BBS_THREAD_LIST,
                            BitConverter.GetBytes(swap32((uint) postMetaList.Count)));

                        foreach (var meta in postMetaList)
                        {
                            SendPostsMetaData(m, meta);
                        }

                        break;

                    case OpCodes.OPCODE_DATA_BBS_THREAD_GET_CONTENT:
                        /*byte[] postData =
                        {
                            0x00, 0x00, 0x00, 0x00, 0x54, 0x48, 0x49, 0x53, 0x20, 0x49, 0x53, 0x20, 0x41, 0x20, 0x54, 0x45,
                            0x53, 0x54, 0x20, 0x50, 0x4f, 0x53, 0x54, 0x21, 0x0a, 0x42, 0x49, 0x54, 0x43, 0x48, 0x45, 0x53,
                            0x21, 0x00, 0x42, 0x49, 0x54, 0x43, 0x48, 0x45, 0x53, 0x21, 0x00, 0x54, 0x48, 0x49, 0x53, 0x00
                        };*/

                        uint q = swap32(BitConverter.ToUInt32(argument, 4));
                        int postID = Convert.ToInt32(q);


                        BbsPostBody bbsPostBody = DBAcess.getInstance().GetPostBodyByPostId(postID);
                        Console.WriteLine("Queryed for the post ID number " + postID);
                        SendPostBody(m, bbsPostBody);
                        break;

                    case OpCodes.OPCODE_DATA_NEWS_GETMENU:
                        SendPacket30(OpCodes.OPCODE_DATA_NEWS_CATEGORYLIST, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_AS_DISKID:
                        SendPacket30(OpCodes.OPCODE_DATA_AS_DISKID_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_LOBBY_EVENT:
                        if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out var lcr))
                        {
                            lcr.DispatchPublicBroadcast(argument, this.index);
                        }
                        break;
                    case OpCodes.OPCODE_DATA_MAILCHECK:
                        logger.LogData(argument, 0xFFFF, this.index, "CHECK FOR NEW MAIL NOTIFICATION ", 0, 0);

                        if (DBAcess.getInstance().checkForNewMailByAccountID(ReadAccountID(argument, 0)))
                            SendPacket30(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] {0x00, 0x00, 0x01, 0x00});
                        else
                            SendPacket30(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] {0x00, 0x01});

                        break;
                    case OpCodes.OPCODE_DATA_BBS_GET_UPDATES:
                        SendPacket30(0x786b, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_NEWCHECK:
                        SendPacket30(OpCodes.OPCODE_DATA_NEWCHECK_OK, new byte[] {0x00, 0x00});
                        break;
                    case OpCodes.OPCODE_DATA_COM:
                        SendPacket30(OpCodes.OPCODE_DATA_COM_OK, new byte[] {0xDE, 0xAD});
                        break;
                    case 0x787E: // enter ranking screen
                        SendPacket30(0x787F, new byte[] {0x00, 0x00});
                        break;
                    case 0x788C:
                        ushort destid = swap16(BitConverter.ToUInt16(argument, 2));
                        //Server.Instance.LobbyChatRooms[room_index].DispatchPrivateBroadcast(argument, this.index, destid);
                        if (Server.Instance.LobbyChatService.TryGetLobby((ushort)room_index, out var p))
                        {
                            p.DispatchPrivateBroadcast(argument, this.index, destid);
                        }
                        break;
                    case OpCodes.OPCODE_DATA_SELECT_CHAR:
                        
                        SendPacket30(OpCodes.OPCODE_DATA_SELECT_CHAROK, new byte[] {0x00, 0x00});
                        
                        break;
                    case OpCodes.OPCODE_DATA_SELECT2_CHAR:
                        
                        SendPacket30(OpCodes.OPCODE_DATA_SELECT2_CHAROK, new byte[] {0x30, 0x30, 0x30, 0x30});
                        break;
                    case OpCodes.OPCODE_DATA_LOGON:
                        if (argument[1] == OpCodes.OPCODE_DATA_SERVERKEY_CHANGE)
                        {
                            logger.Information("Client #" + this.index + " : New Area Server Joined");
                            isAreaServer = true;
                            SendPacket30(0x78AC, new byte[] {0xDE, 0xAD});
                        }
                        else
                        {
                            logger.Information("Client #" + this.index + " : New Game Client Joined");
                            SendPacket30(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] {0x74, 0x32});
                        }

                        break;
                    case OpCodes.OPCODE_DATA_AS_PUBLISH:
                        SendPacket30(OpCodes.OPCODE_DATA_AS_PUBLISH_OK, new byte[] {0x00, 0x00});
                        break;
                    default:
                        logger.Information("Client #" + this.index +
                                      " : \n !!!UNKNOWN DATA CODE RECEIVED, PLEASE REPORT : 0x" +
                                      code.ToString("X4") + "!!!\n");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error while processing Packet the following packet for the Client# " +this.index+"\n"+ data + "\n"+e );
                Exit();
                //throw;
            }
        }

        public void GetLobbyMenu()
        {
            List <LobbyChatRoom> nonGuildLobbies = new List<LobbyChatRoom>();

            foreach (var room in Server.Instance.LobbyChatService.Lobbies.Values)
            {
                if (room.type == OpCodes.LOBBY_TYPE_MAIN)
                {
                    nonGuildLobbies.Add(room);
                }
            }
            
            SendPacket30(OpCodes.OPCODE_DATA_LOBBY_LOBBYLIST,
                BitConverter.GetBytes(swap16((ushort) nonGuildLobbies.Count())));
            foreach (LobbyChatRoom room in nonGuildLobbies)
            {
                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16((ushort) room.ID)), 0, 2);
                foreach (char c in room.name)
                    m.WriteByte((byte) c);
                m.WriteByte(0);
                m.Write(BitConverter.GetBytes(swap16((ushort) room.Users.Count())), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort) (room.Users.Count() + 1))), 0, 2);
                while (((m.Length + 2) % 8) != 0)
                    m.WriteByte(0);
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_ENTRY_LOBBY, m.ToArray());
            }
        }

        public void GetServerList(byte[] data)
        {
            if (data[1] == 0)
            {
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_CATEGORYLIST, new byte[] {0x00, 0x01});
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_CATEGORY,
                    new byte[]
                    {
                        0x00, 0x01, 0x4D, 0x41, 0x49, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x6E, 0x65
                    });
            }
            else
            {
                ushort count = 0;
                foreach (var client in Server.Instance.GameClientService.Clients)
                    if (client.isAreaServer)
                        count++;
                SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_SERVERLIST, BitConverter.GetBytes(swap16(count)));
                foreach (var client in Server.Instance.GameClientService.Clients)
                    if (client.isAreaServer && !client._exited)
                    {
                        MemoryStream m = new MemoryStream();
                        m.WriteByte(0);
                        if (client.ipEndPoint.Address == this.ipEndPoint.Address)
                            m.Write(client.ipdata, 0, 6);
                        else
                            m.Write(client.externalIPAddress, 0, 6);
                        
                        byte[] buff = BitConverter.GetBytes(swap16(client.as_usernum));
                        int pos = 0;
                        while (client.publish_data_1[pos++] != 0) ;
                        pos += 4;
                        client.publish_data_1[pos++] = buff[0];
                        client.publish_data_1[pos++] = buff[1];
                        m.Write(client.publish_data_1, 0, client.publish_data_1.Length);
                        while (m.Length < 45)
                            m.WriteByte(0);

                        string usr = _encoding.GetString(BitConverter.GetBytes(swap16(client.as_usernum)));
                        string pup1 = _encoding.GetString(client.publish_data_1);
                        string pup2 =_encoding.GetString(client.publish_data_2);
                        Console.WriteLine(pup1 +"\n" +pup2+ "\n" + client.as_usernum + "\n");
                        
                            SendPacket30(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_SERVER, m.ToArray());
                    }
            }
        }


        public void ExtractAreaServerData(byte[] data)
        {
            int pos = 67;
            areaServerName = ReadByteString(data, pos);
            pos += areaServerName.Length;
            areaServerLevel = swap16(BitConverter.ToUInt16(data, pos));
            pos += 4;
            areaServerStatus = data[pos++];
        }

        public void SaveMailData(byte[] data)
        {
            
            byte[] reciver_accountID = new byte[4];
            byte[] reciver = new byte[16];
            byte[] sender_accountID = new byte[4];
            byte[] sender = new byte[16];
            byte[] subject = new byte[32];
            byte[] body = new byte[1200];
            byte[] face = new byte[25];
            
            
            
            Buffer.BlockCopy(data, 4, reciver_accountID,0 ,4);
            Buffer.BlockCopy(data, 8, reciver,0 ,16);
            Buffer.BlockCopy(data, 26, sender_accountID,0 ,4);
            Buffer.BlockCopy(data, 30, sender,0 ,16);
            Buffer.BlockCopy(data, 48, subject,0 ,32);
            Buffer.BlockCopy(data, 176, body,0 ,1200);
            Buffer.BlockCopy(data, 1378, face,0 ,25);
            
            
            
            Console.WriteLine("REC_AccountID " + swap32(BitConverter.ToUInt32(reciver_accountID)));
            Console.WriteLine("reciver " +_encoding.GetString(reciver));
            Console.WriteLine("Sender_AccountID " + swap32(BitConverter.ToUInt32(sender_accountID)));
            Console.WriteLine("sender " + _encoding.GetString(sender));
            Console.WriteLine("mail_subject " + _encoding.GetString(subject));
            Console.WriteLine("body " + _encoding.GetString(body));
            Console.WriteLine("face " + _encoding.GetString(face));
            
            logger.LogData(face,0xFFFF,this.index,"FACE ID",0,0);
            
            
            MailMetaModel metaModel = new MailMetaModel();
            metaModel.Receiver_Account_ID = (int) swap32(BitConverter.ToUInt32(reciver_accountID));
            metaModel.Receiver_Name = reciver;
            metaModel.Sender_Account_ID = (int) swap32(BitConverter.ToUInt32(sender_accountID));
            metaModel.Sender_Name = sender;
            metaModel.Mail_Subject = subject;
            metaModel.date = DateTime.UtcNow;
            metaModel.Mail_Delivered = false;
            
            MailBodyModel bodyModel = new MailBodyModel();
            bodyModel.Mail_Body = body;
            bodyModel.Mail_Face_ID = _encoding.GetString(face);
            
            DBAcess.getInstance().CreateNewMail(metaModel,bodyModel);


        }


        

        public byte[] GetMailMeta(MailMetaModel metaModel)
        {
            List<byte> messageID = BitConverter.GetBytes(swap32((uint) metaModel.Mail_ID)).ToList();
           
            
            List<byte> sender = metaModel.Sender_Name.ToList();

            while (sender.Count<18)
            {
                sender.Add(0x00);
            }

            List<byte> receiver = metaModel.Receiver_Name.ToList();
            while (receiver.Count<18)
            {
                receiver.Add(0x00);
            }
            List<byte> sender_accountID = BitConverter.GetBytes(swap32((uint) metaModel.Sender_Account_ID)).ToList();
           
            
            List<byte> receiver_accountID = BitConverter.GetBytes(swap32((uint) metaModel.Receiver_Account_ID)).ToList();

            List<byte> mail_subject = metaModel.Mail_Subject.ToList();
            

            while (mail_subject.Count<128)
            {
                mail_subject.Add(0x00);
            }
            
            
            
            TimeSpan t = metaModel.date - new DateTime(1970, 1, 1);
            
            List<byte> date = new List<byte>(){0x00, 0x00,0x00,0x00};
            date.AddRange( BitConverter.GetBytes(swap32((uint) t.TotalSeconds)).ToList());
            
            MemoryStream m = new MemoryStream();
            


            m.Write(messageID.ToArray(), 0, messageID.Count);
            m.Write(receiver_accountID.ToArray(),0,sender_accountID.Count);
            m.Write(date.ToArray(),0,date.Count);
            m.Write(new Byte[]{0x07});
            m.Write(sender_accountID.ToArray(),0,sender_accountID.Count);
            m.Write(sender.ToArray(),0,sender.Count);
            m.Write(BitConverter.GetBytes(0),0,4);
            m.Write(receiver.ToArray(),0,receiver.Count);
            m.Write(mail_subject.ToArray(),0,mail_subject.Count);
            
            return m.ToArray();
        }

        public byte[] GetMailBody(MailBodyModel bodyModel)
        {


            List<byte> face = _encoding.GetBytes(bodyModel.Mail_Face_ID).ToList();
            
            while (face.Count<130)
            {
                face.Add(0x00);
            }
            
         
            MemoryStream m = new MemoryStream();
            
            
            List<byte> body= bodyModel.Mail_Body.ToList();

            while (body.Count<1200)
            {
                body.Add(0x00);
            }
            
            m.Write(BitConverter.GetBytes(5),0,4);
            m.Write(BitConverter.GetBytes(0),0,2);
            m.Write(body.ToArray(),0,body.Count);
            m.Write(BitConverter.GetBytes(0),0,2);
            m.Write(face.ToArray(),0,face.Count);
            
            
            return m.ToArray();

        }

        public uint ExtractCharacterData(byte[] data)
        {
            save_slot = data[0];
            char_id = ReadByteString(data, 1);
            int pos = 1 + char_id.Length;
            char_name = ReadByteString(data, pos);
            pos += char_name.Length;
            char_class = data[pos++];
            char_level = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            greeting = ReadByteString(data, pos);
            pos += greeting.Length;
            char_model = swap32(BitConverter.ToUInt32(data, pos));
            pos += 5;
            char_HP = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            char_SP = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            char_GP = swap32(BitConverter.ToUInt32(data, pos));
            pos += 4;
            online_god_counter = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            offline_godcounter = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            goldCoinCount = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            silverCoinCount = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            bronzeCoinCount = swap16(BitConverter.ToUInt16(data, pos));
            
             classLetter = GetCharacterModelClass(char_model);
             modelNumber = GetCharacterModelNumber(char_model);
             modelType = GetCharacterModelType(char_model);
             colorCode = GetCharacterModelColorCode(char_model);

             charModelFile = "xf" + classLetter + modelNumber + modelType +"_"+ colorCode;
            
           
            Console.WriteLine("gold coin count " + goldCoinCount);
            Console.WriteLine("silver coin count " + silverCoinCount);
            Console.WriteLine("bronze coin count " + bronzeCoinCount);
            
            Console.WriteLine("Character Date \n save_slot "+ save_slot + "\n char_id " +_encoding.GetString(save_id) + " \n char_name " + _encoding.GetString(char_id) +
                              "\n char_class " + char_class + "\n char_level " + char_level + "\n greeting "+ _encoding.GetString(greeting) +"\n charmodel " +char_model + "\n char_hp " + char_HP+
                              "\n char_sp " + char_SP + "\n char_gp " + char_GP + "\n onlien god counter "+ online_god_counter + "\n offline god counter "+ offline_godcounter +"\n\n\n\n full byte araray " + BitConverter.ToString(data));
            
            return DBAcess.getInstance().PlayerLogin(this);
        }



        public void CaptureGuildCreation(byte[] argument)
        {
            int pos = 0;
            
            byte[] guildNameBytes = ReadByteString(argument, pos);
            pos += guildNameBytes.Length;
            
            byte[] guildCommentBytes = ReadByteString(argument, pos);
            pos += guildCommentBytes.Length;
            
            byte[] guildEmblem = ReadByteGuildEmblem(argument, pos);
            
            Console.WriteLine("Guild Name = "+ _encoding.GetString(guildNameBytes));
            Console.WriteLine("Guild Comment = "+ _encoding.GetString(guildCommentBytes));
            
            logger.LogData(guildEmblem,0xff,this.index,"Guild Emblem Data",0,0);

        }

        public byte[] GetGuildInfo()
        {
            MemoryStream m = new MemoryStream();

            byte[] guildName = _encoding.GetBytes("ZackGuild1234567");
           
            m.Write(guildName);
                      
            
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            m.Write(new byte[] {0x00});
            m.Write(_encoding.GetBytes("2020/10/31")); //date

            m.Write(new byte[] {0x00});


            byte[] guildMaster = _encoding.GetBytes("zackTest");
            //Buffer.BlockCopy(,0,guildMaster,0,_encoding.GetBytes("zackTest").Length);
            m.Write(guildMaster);
            m.Write(new byte[] {0x00});
            

            //byte[] avgText = _encoding.GetBytes("fiejfiejfijefijefiej");
         //   m.Write(avgText);
           // m.Write(new byte[] {0x00});
            //m.Write(new byte[] {0x01});// level
            //uint date  = 50;
            //m.Write(BitConverter.GetBytes(swap32( date)), 0, 4);
          //  byte[] guildMaster = new byte[18];
          //  Buffer.BlockCopy(_encoding.GetBytes("zackTest"),0,guildMaster,0,_encoding.GetBytes("zackTest").Length);
           // m.Write(guildMaster);
          
           ushort memTotal = 21;
           m.Write(BitConverter.GetBytes(swap16( memTotal)), 0, 2);
           
           ushort twinBlade = 6;
           m.Write(BitConverter.GetBytes(swap16( twinBlade)), 0, 2);
           ushort bladeMaster = 5;
           m.Write(BitConverter.GetBytes(swap16( bladeMaster)), 0, 2);
           ushort heavyBlade = 4;
           m.Write(BitConverter.GetBytes(swap16( heavyBlade)), 0, 2);
           ushort heaveyAxes = 3;
           m.Write(BitConverter.GetBytes(swap16( heaveyAxes)), 0, 2);
           ushort longArm = 2;
           m.Write(BitConverter.GetBytes(swap16( longArm)), 0, 2);
           ushort waveMaster = 1;
           m.Write(BitConverter.GetBytes(swap16( waveMaster)), 0, 2);
           
           ushort avgLevel = 55;
           m.Write(BitConverter.GetBytes(swap16( avgLevel)), 0, 2);
           
           
           
           uint goldCoins = 50;
           m.Write(BitConverter.GetBytes(swap32( goldCoins)), 0, 4);
           uint silverCoins = 40;
           m.Write(BitConverter.GetBytes(swap32( silverCoins)), 0, 4);
           uint bronzeCoins = 30;
           m.Write(BitConverter.GetBytes(swap32( bronzeCoins)), 0, 4);

           uint GP = 5000;
           m.Write(BitConverter.GetBytes(swap32( GP)), 0, 4);
           
           byte[] guildComment = _encoding.GetBytes("This is the Guild Comment");
           m.Write(guildComment);
           m.Write(new byte[] {0x00});
            //"This is the Guild Comment";
            
           // m.Write(guildComment);
            
            byte[] guildEmblem ={0x53,0x45,0x39,0x46,0x54,0x55,0x49,0x77,0x4D,0x41,0x41,0x65,0x41,0x41,0x41,0x41,0x67,0x49,0x43,0x41,0x41,0x49,0x43,0x41,0x67,0x41,0x43,0x41,0x67,0x49,0x41,0x41,0x67,0x49,0x43,0x41,0x41,0x45,0x38,0x4B,0x41,0x41,0x41,0x41,0x41,0x49,0x41,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x47,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x43,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x34,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x53,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x46,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x42,0x6F,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x65,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x49,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x43,0x59,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x71,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x4C,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x44,0x49,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x32,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x4F,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x44,0x34,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x42,0x43,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x52,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x45,0x6F,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x42,0x4F,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x55,0x67,0x49,0x43,0x41,0x67,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x46,0x59,0x43,0x41,0x67,0x49,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x42,0x61,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x58,0x67,0x49,0x43,0x41,0x67,0x41,0x38,0x51,0x41,0x41,0x41,0x41,0x47,0x49,0x43,0x41,0x67,0x49,0x42,0x61,0x43,0x67,0x41,0x41,0x41,0x42,0x6D,0x41,0x67,0x49,0x42,0x54,0x59,0x78,0x63,0x41,0x41,0x41,0x41,0x61,0x67,0x49,0x43,0x41,0x4D,0x41,0x49,0x42,0x41,0x41,0x41,0x41,0x47,0x34,0x43,0x41,0x67,0x49,0x41,0x42,0x42,0x77,0x41,0x41,0x41,0x42,0x79,0x41,0x67,0x49,0x43,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x41,0x64,0x67,0x49,0x43,0x41,0x67,0x41,0x3D,0x3D};
            m.Write(guildEmblem);
            m.Write(new byte[] {0x00});
           
           
            


          

            
            
            return m.ToArray();
        }


        public char GetCharacterModelClass(uint modelNumber)
        {
            char[] classLetters = { 't', 'b', 'h', 'a', 'l', 'w' };
            
            int index = (int) (modelNumber & 0x0F);
            
            return classLetters[index];
        }
        public int GetCharacterModelNumber(uint modelNumber)
        {
            return (int) (modelNumber >> 4 & 0x0F) + 1;
        }
        public char GetCharacterModelType(uint modelNumber)
        {
            char[] typeLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };
            
            int index = (int) (modelNumber >> 12) & 0x0F;
            
            return typeLetters[index];
            
        }
        public string GetCharacterModelColorCode(uint modelNumber)
        {
            string[] colorCodes = { "rd", "bl", "yl", "gr", "br", "pp" };
            
            int index = (int) (modelNumber >> 8) & 0x0F;
            return colorCodes[index];
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
        
        public byte[] ReadByteGuildEmblem(byte[] data, int pos)
        {
            MemoryStream m = new MemoryStream();
            while (true)
            {
                byte b = data[pos++];
                m.WriteByte(b);
                if (b == 0x3D)
                    break;
                if (pos >= data.Length)
                    break;
            }

            return m.ToArray();
        }
        
        public byte[] ReadByteStringNoNull(byte[] data, int pos)
        {
            MemoryStream m = new MemoryStream();
            while (true)
            {
                byte b = data[pos++];
                if (b == 0)
                    break;
                m.WriteByte(b);
                
                if (pos >= data.Length)
                    break;
            }

            return m.ToArray();
        }

        public int ReadAccountID(byte[] data, int pos)
        {
            byte[] accountID = new byte[4];
            Buffer.BlockCopy(data,pos,accountID,0,4);
            return (int) swap32(BitConverter.ToUInt32(accountID));
        }

        public void SendPacket30(ushort code, byte[] data)
        {
            try
            {
                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap32(server_seq_nr++)), 0, 4);
                ushort len = (ushort) (data.Length + 2);
                m.Write(BitConverter.GetBytes(swap16(len)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16(code)), 0, 2);
                m.Write(data, 0, data.Length);
                uint checksum = Crypto.Checksum(m.ToArray());
                while (((m.Length + 2) & 7) != 0) m.WriteByte(0);
                SendPacket(OpCodes.OPCODE_DATA, m.ToArray(), checksum);
            }
            catch (Exception e)
            {
                Console.WriteLine("error sending packet to client " + this.index + " maybe disconnected \n" + e);
                Exit();
                //throw;
            }
        }

        public void SendPacket(ushort code, byte[] data, uint checksum)
        {
            try
            {
                MemoryStream m = new MemoryStream();
                m.WriteByte((byte) (checksum >> 8));
                m.WriteByte((byte) (checksum & 0xFF));
                m.Write(data, 0, data.Length);
                byte[] buff = m.ToArray();
                logger.LogData(buff, code, index, "Send Data", (ushort) checksum, (ushort) checksum);
                buff = to_crypto.Encrypt(buff);
                ushort len = (ushort) (buff.Length + 2);
                m = new MemoryStream();
                m.WriteByte((byte) (len >> 8));
                m.WriteByte((byte) (len & 0xFF));
                m.WriteByte((byte) (code >> 8));
                m.WriteByte((byte) (code & 0xFF));
                m.Write(buff, 0, buff.Length);

                ns.Write(m.ToArray(), 0, (int) m.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("error sending packet to client " + this.index + " maybe disconnected \n" + e);
                Exit();
                //throw;
            }
            
        }





        public void SendCategoryDetails(MemoryStream m, BbsCategoryModel categoryModel)
        {
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16((ushort) categoryModel.categoryID)), 0, 2);
            byte[] buff2 = _encoding.GetBytes(categoryModel.categoryName);
           // m.WriteByte((byte) (buff2.Length - 1));
            m.Write(buff2, 0, buff2.Length);
            //must fill with empty bytes until 0x24
            while (m.Length < 0x24)
                m.WriteByte(0);

            SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, m.ToArray());
        }

        public void SendThreads(MemoryStream m, BbsThreadModel threadModel)
        {
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32((ushort) threadModel.threadID)), 0, 4);
            byte[] threadTitleBytes = threadModel.threadTitle;
           // m.WriteByte((byte) (threadTitleBytes.Length - 1));
            m.Write(threadTitleBytes, 0, threadTitleBytes.Length);
            while (m.Length < 0x26)
                m.WriteByte(0);
            SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_THREAD, m.ToArray());
        }

        public void SendPostsMetaData(MemoryStream m, BbsPostMetaModel postMetaModel)
        {
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap32((uint) postMetaModel.unk0)), 0, 4); //unk
            m.Write(BitConverter.GetBytes(swap32((uint) postMetaModel.postID)), 0, 4); //postid
            m.Write(BitConverter.GetBytes(swap32((uint) postMetaModel.unk2)), 0, 4); //unk2
            Console.WriteLine("The date and time is " + postMetaModel.date);
            
            TimeSpan t = postMetaModel.date - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            //Console.WriteLine(secondsSinceEpoch);
            m.Write(BitConverter.GetBytes(swap32((uint) secondsSinceEpoch)), 0, 4); //date

            // Setting the username
            if (postMetaModel.username.Length > 15) //if the lenghth is more than 15 char then truncate 
            {
                byte[] temp = new byte[17];
                Buffer.BlockCopy(postMetaModel.username,0,temp,0,15);
                postMetaModel.username = temp;
            }

            byte[] usernameBytes = postMetaModel.username;
            //m.WriteByte((byte) (username.Length - 1));
            m.Write(usernameBytes, 0, usernameBytes.Length); //username
            while (m.Length < 0x20)
                m.WriteByte(0);


            //setting the Subtitle
            if (postMetaModel.subtitle.Length > 17) //if the lengh is more than 17 then truncate
            {
                byte[] temp = new byte[17];
                Buffer.BlockCopy(postMetaModel.subtitle,0,temp,0,17);
                postMetaModel.subtitle = temp;
            }

            byte[] subtitleBytes = postMetaModel.subtitle;
            //m.WriteByte((byte) (subtitleBytes.Length - 1));
            m.Write(subtitleBytes, 0, subtitleBytes.Length); // subtitles
            while (m.Length < 0x32)
                m.WriteByte(0);


            //setting unk3
            if (postMetaModel.unk3.Length > 45)
            {
                postMetaModel.unk3 = postMetaModel.unk3.Substring(0, 45);
            }

            byte[] unk3Bytes = _encoding.GetBytes(postMetaModel.unk3);
            m.Write(unk3Bytes, 0, unk3Bytes.Length);

            while (m.Length < 0x60)
                m.WriteByte(0);


            //setting the title

            if (postMetaModel.title.Length > 32) //if the length is more than 17 then truncate
            {
                byte[] temp = new byte[32];
                Buffer.BlockCopy(postMetaModel.title,0,temp,0,32);
                postMetaModel.title = temp;
            }

            byte[] titleBytes = postMetaModel.title;
            // m.WriteByte((byte) (titleBytes.Length - 1));
            m.Write(titleBytes, 0, titleBytes.Length); // title

            while (m.Length < 0x80)
                m.WriteByte(0);


            SendPacket30(OpCodes.OPCODE_DATA_BBS_ENTRY_POST_META, m.ToArray());
        }

        public void SendPostBody(MemoryStream m, BbsPostBody postBody)
        {
            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(0), 0, 4);

            byte[] bodyBytes = postBody.postBody;

            m.Write(bodyBytes, 0, bodyBytes.Length); // message body 

            SendPacket30(0x781d, m.ToArray());
        }


        static ushort swap16(ushort data) => data.Swap();

        static uint swap32(uint data) => data.Swap();
    }
}