using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities
{
    public sealed class GameClientAsync
    {

        private CancellationTokenSource tokenSource;
        private readonly System.Timers.Timer pingTimer;
        private readonly Encoding encoding;
        private readonly TcpClient client;
        private readonly SimpleConfiguration simpleConfiguration;
        private readonly NetworkStream ns;
        private readonly IPEndPoint ipEndPoint;
        private readonly Crypto to_crypto;
        private readonly Crypto from_crypto;
        private readonly uint clientIndex;
        private readonly ILogger logger;
        private readonly ILobbyChatService lobbyChatService;

        private short currentLobbyIndex = -1;
        private byte[] to_key;
        private byte[] from_key;
        private ushort client_seq_nr;
        private ushort server_seq_nr;
        private bool isAreaServer;
        private byte[] ipdata;
        private byte[] externalIPAddress;
        private byte[] publish_data_1;
        private byte[] publish_data_2;
        private byte[] last_status;
        private ushort as_usernum;
        private byte[] areaServerName;
        private ushort areaServerLevel;
        private byte areaServerStatus;

        #region Player Data
        internal int AccountId;
        internal byte save_slot;
        internal byte[] save_id;
        internal byte[] char_name;
        internal byte[] char_id;
        internal byte char_class;
        internal ushort char_level;
        internal byte[] greeting;
        internal uint char_model;
        internal ushort char_HP;
        internal ushort char_SP;
        internal uint char_GP;
        internal ushort online_god_counter;
        internal ushort offline_godcounter;
        internal ushort goldCoinCount;
        internal ushort silverCoinCount;
        internal ushort bronzeCoinCount;
        internal char classLetter;
        internal int modelNumber;
        internal char modelType;
        internal string colorCode;
        internal string charModelFile;
        internal uint _characterPlayerID = 0;
        internal ushort _guildID = 0;
        internal bool isGuildMaster = false;
        internal uint _itemDontationID = 0;
        internal ushort _itemDonationQuantity = 0;
        internal ushort currentGuildInvitaionSelection = 0;
        internal ushort _rankingCategoryID = 0;
        internal ushort currentLobbyType = 0;
        #endregion Player Data

        #region Events

        /// <summary>
        /// Raised when the client is disconnecting
        /// </summary>
        public event EventHandler OnGameClientDisconnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Client Index
        /// </summary>
        public int ClientIndex => (int)clientIndex;

        /// <summary>
        /// Gets the current Lobby Index
        /// </summary>
        public short LobbyIndex => currentLobbyIndex;

        /// <summary>
        /// Gets the currently logged in Player ID
        /// </summary>
        public uint PlayerID => _characterPlayerID;

        #endregion

        public GameClientAsync(
            uint clientIndex,
            ILogger logger,
            ILobbyChatService lobbyChatService,
            TcpClient tcpClient,
            SimpleConfiguration simpleConfiguration)
        {
            // Why are we doing this?
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("Shift-JIS");

            // Define a few constants
            server_seq_nr = 0xe;

            this.client = tcpClient;
            this.simpleConfiguration = simpleConfiguration;
            this.clientIndex = clientIndex;
            this.logger = logger;
            this.lobbyChatService = lobbyChatService;
            ns = client.GetStream();
            ns.ReadTimeout = 100;
            ns.WriteTimeout = 100;

            to_crypto = new Crypto();
            from_crypto = new Crypto();
            ipEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            tokenSource = new CancellationTokenSource();
            Task.Run(async () => await InternalConnectionLoop(tokenSource.Token));
            this.pingTimer = new System.Timers.Timer(TimeSpan.FromMilliseconds(10_000).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };
            this.pingTimer.Elapsed += PingTimer_Elapsed;
        }



        private async Task InternalConnectionLoop(CancellationToken token)
        {
            try
            {
                using (token.Register(() => client.Close()))
                {
                    while (!token.IsCancellationRequested)
                    {
                        var packet = new PacketAsync(logger, ns, from_crypto);
                        var readResult = await packet.ReadPacketAsync();

                        if (!readResult)
                        {
                            logger.Debug("Client #{@clientIndex} has no data at this time; suspending for a short duration", clientIndex);
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }

                        // Packet has been read, bring over what we do now
                        await HandleIncomingPacket(packet);
                    }
                }
            }
            catch (ObjectDisposedException ode)
            {
                logger.Error(ode, $"The {nameof(GameClientAsync)} was told to shutdown or threw some sort of error; cleaning up the Client");
            }
            catch (InvalidOperationException ioe)
            {
                // Either tcpListener.Start wasn't called (a bug!)
                // or the CancellationToken was cancelled before
                // we started accepting (giving an InvalidOperationException),
                // or the CancellationToken was cancelled after
                // we started accepting (giving an ObjectDisposedException).
                //
                // In the latter two cases we should surface the cancellation
                // exception, or otherwise rethrow the original exception.
                logger.Error(ioe, $"The {nameof(GameClientAsync)} was told to shutdown, or errored, before an incoming packet was read. More context is necessary to see if this Error can be safely ignored");
                token.ThrowIfCancellationRequested();
                logger.Error(ioe, $"The {nameof(GameClientAsync)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, $"The {nameof(GameClientAsync)} was told to explicitly shutdown and no further action is necessary");
            }
            finally
            {
                OnGameClientDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task HandleIncomingPacket(PacketAsync packet)
        {
            var responseStream = new MemoryStream();
            logger.LogData(packet.Data, packet.Code, (int)clientIndex, "Recv Data", packet.ChecksumInPacket, packet.ChecksumOfPacket);
            switch (packet.Code)
            {
                case 0x0002: // I don't know this opcode
                    break;
                case OpCodes.OPCODE_KEY_EXCHANGE_REQUEST:
                    responseStream.Write(packet.Data, 4, 16);
                    from_key = responseStream.ToArray();
                    to_key = new byte[16];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(to_key);
                    }
                    await responseStream.DisposeAsync();
                    responseStream = new MemoryStream();
                    responseStream.WriteByte(0);
                    responseStream.WriteByte(0x10);
                    responseStream.Write(from_key, 0, 16);
                    responseStream.WriteByte(0);
                    responseStream.WriteByte(0x10);
                    responseStream.Write(to_key, 0, 16);
                    responseStream.Write(new byte[] { 0, 0, 0, 0xe, 0, 0, 0, 0, 0, 0 }, 0, 10);
                    uint checksum = Crypto.Checksum(responseStream.ToArray());
                    await SendRegularPacket(OpCodes.OPCODE_KEY_EXCHANGE_RESPONSE, responseStream.ToArray(), checksum);
                    break;
                case OpCodes.OPCODE_KEY_EXCHANGE_ACKNOWLEDGMENT:
                    from_crypto.PrepareStructure(from_key);
                    to_crypto.PrepareStructure(to_key);
                    break;
                case OpCodes.OPCODE_DATA:
                    await HandleIncomingDataPacket(packet);
                    break;
                default:
                    logger.Information("Client Handler #" + clientIndex + ": Received packet with unknown code");
                    // we might break here but for now we're OK
                    break;
            }
        }

        private async Task HandleIncomingDataPacket(PacketAsync packet)
        {
            var data = packet.Data;
            client_seq_nr = swap16(BitConverter.ToUInt16(data, 2));
            var arglen = (ushort)(swap16(BitConverter.ToUInt16(data, 6)) - 2);
            var code = swap16(BitConverter.ToUInt16(data, 8));
            var m = new MemoryStream();
            await m.WriteAsync(data, 10, arglen);
            var argument = m.ToArray();
            logger.LogData(data, code, (int)clientIndex, "Recv Data 0X30", 0, 0);

            switch(code)
            {
                case OpCodes.OPCODE_DATA_PING:
                case OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY:
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS3:
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS4:
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS6:
                    // do nothing
                    break;
                case OpCodes.OPCODE_DATA_LOGON_REPEAT:
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x02, 0x10 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_ENTERROOM:
                    await HandleLobbyRoomEntrance(argument);
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_STATUS_UPDATE:
                    if (lobbyChatService.TryGetLobby((ushort)currentLobbyIndex, out var rm))
                    {
                        rm.DispatchStatus(argument, (int)clientIndex);
                    }
                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1:
                    int end = argument.Length - 1;
                    while (argument[end] == 0) end--;
                    end++;
                    m = new MemoryStream();
                    m.Write(argument, 65, end - 65);
                    publish_data_1 = m.ToArray();
                    await SendDataPacket(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1_OK, new byte[] { 0x00, 0x01 });
                    break;
                case OpCodes.OPCODE_DATA_AS_IPPORT:
                    await HandleIncomingIpData(argument);
                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2:
                    await SendDataPacket(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2_OK, new byte[] { 0xDE, 0xAD });
                    break;
                case OpCodes.OPCODE_DATA_LOGON_AS2:
                    await SendDataPacket(0x701C, new byte[] { 0x02, 0x11 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_CHATROOM_GETLIST:
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY,
                        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_CHATROOM_CATEGORY, new byte[] { 0x00, 0x01, 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_UPDATE_USERNUM:
                    as_usernum = swap16(BitConverter.ToUInt16(argument, 2));
                    break;
                case OpCodes.OPCODE_DATA_DISKID:
                    await SendDataPacket(OpCodes.OPCODE_DATA_DISKID_OK, new byte[] { 0x36, 0x36, 0x31, 0x36 });
                    break;
                case OpCodes.OPCODE_DATA_SAVEID: // This also sends the MOTD to the Client
                    await HandleSaveIdToAccountIdAssociation(argument);
                    break;
                case OpCodes.OPCODE_DATA_REGISTER_CHAR: // Why are we handling Guild nonsense here as well?
                    await HandleCharacterRegistrationAndGuildDetails(argument);
                    break;
                case OpCodes.OPCODE_DATA_UNREGISTER_CHAR:
                    await SendDataPacket(OpCodes.OPCODE_DATA_UNREGISTER_CHAROK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_EXITROOM:
                    await lobbyChatService.AnnounceRoomDeparture((ushort)currentLobbyIndex, clientIndex);
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_EXITROOM_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_RETURN_DESKTOP:
                    DBAcess.getInstance().setPlayerAsOffline(_characterPlayerID);
                    await SendDataPacket(OpCodes.OPCODE_DATA_RETURN_DESKTOP_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETMENU:
                    await HandleLobbyMenu();
                    break;
            }


        }



        private async Task SendRegularPacket(ushort code, byte[] data, uint checksum)
        {
            try
            {
                var responseStream = new MemoryStream();
                responseStream.WriteByte((byte)(checksum >> 8));
                responseStream.WriteByte((byte)(checksum & 0xFF));
                await responseStream.WriteAsync(data, 0, data.Length);
                var buff = responseStream.ToArray();
                logger.LogData(buff, code, (int)clientIndex, "Send Data", (ushort)checksum, (ushort)checksum);
                buff = to_crypto.Encrypt(buff);
                var len = (ushort)(buff.Length + 2);
                responseStream = new MemoryStream();
                responseStream.WriteByte((byte)(len >> 8));
                responseStream.WriteByte((byte)(len & 0xFF));
                responseStream.WriteByte((byte)(code >> 8));
                responseStream.WriteByte((byte)(code & 0xFF));
                await responseStream.WriteAsync(buff, 0, buff.Length);
                await ns.WriteAsync(responseStream.ToArray(), 0, (int)responseStream.Length);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error has occurred sending a packet to the Client; disconnecting the client for now");
                tokenSource.Cancel();
            }

        }

        private async Task SendDataPacket(ushort code, byte[] data)
        {
            try
            {
                var responseStream = new MemoryStream();
                await responseStream.WriteAsync(BitConverter.GetBytes(swap32(server_seq_nr++)), 0, 4);
                var len = (ushort)(data.Length + 2);
                await responseStream.WriteAsync(BitConverter.GetBytes(swap16(len)), 0, 2);
                await responseStream.WriteAsync(BitConverter.GetBytes(swap16(code)), 0, 2);
                await responseStream.WriteAsync(data, 0, data.Length);
                var checksum = Crypto.Checksum(responseStream.ToArray());
                while (((responseStream.Length + 2) & 7) != 0) responseStream.WriteByte(0);
                await SendRegularPacket(OpCodes.OPCODE_DATA, responseStream.ToArray(), checksum);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error has occurred sending a data packet to the Client; disconnecting the client for now");
                tokenSource.Cancel();
            }
        }

        private async void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            logger.Debug($"Client #{clientIndex} is ready to PING");
            await SendDataPacket(OpCodes.OPCODE_DATA_PING, new byte[0]);
            logger.Debug($"Client #{clientIndex} has finished pinging");
        }

        #region Packet Handlers

        private async Task HandleLobbyRoomEntrance(byte[] argument)
        {
            LobbyChatRoom room;
            currentLobbyIndex = (short)swap16(BitConverter.ToUInt16(argument, 0));
            currentLobbyType = swap16(BitConverter.ToUInt16(argument, 2));
            logger.Information("Lobby Room ID: {@room_index}", currentLobbyIndex);
            logger.Information("Lobby Type ID: {@lobbyType}", currentLobbyType);

            if (currentLobbyType == OpCodes.LOBBY_TYPE_GUILD) //Guild Room
            {
                //TODO add Guild Specific Code
                room = lobbyChatService.GetOrAddLobby((ushort)currentLobbyIndex, "Guild Room", OpCodes.LOBBY_TYPE_GUILD, out var _);
            }
            else
            {
                lobbyChatService.TryGetLobby((ushort)currentLobbyIndex, out room);
            }

            await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_ENTERROOM_OK, BitConverter.GetBytes(swap16((ushort)room.Users.Count)));
            room.Users.Add((int)this.clientIndex);
            logger.Information("Client #" + this.clientIndex + " : Lobby '" + room.name + "' now has " + room.Users.Count + " Users");
            room.DispatchAllStatus((int)this.clientIndex);
        }

        private async Task HandleIncomingIpData(byte[] argument)
        {
            ipdata = argument;
            var externalIpAddress = ipEndPoint.Address.ToString();
            if (externalIpAddress == Helpers.IPAddressHelpers.LOOPBACK_IP_ADDRESS)
            {
                externalIpAddress = Helpers.IPAddressHelpers.GetLocalIPAddress2();
            }
            string[] ipAddress = externalIpAddress.Split('.');
            argument[3] = byte.Parse(ipAddress[0]);
            argument[2] = byte.Parse(ipAddress[1]);
            argument[1] = byte.Parse(ipAddress[2]);
            argument[0] = byte.Parse(ipAddress[3]);
            externalIPAddress = argument;

            logger.Information("Area Server Client #" + clientIndex + " : Local IP=" +
                          ipdata[3] + "." +
                          ipdata[2] + "." +
                          ipdata[1] + "." +
                          ipdata[0] + " Port:" +
                          swap16(BitConverter.ToUInt16(ipdata, 4)));

            logger.Information("Area Server Client #" + clientIndex + " : External IP=" +
                          externalIPAddress[3] + "." +
                          externalIPAddress[2] + "." +
                          externalIPAddress[1] + "." +
                          externalIPAddress[0] + " Port:" +
                          swap16(BitConverter.ToUInt16(externalIPAddress, 4)));
            await SendDataPacket(OpCodes.OPCODE_DATA_AS_IPPORT_OK, new byte[] { 0x00, 0x00 });
        }

        private async Task HandleSaveIdToAccountIdAssociation(byte[] argument)
        {
            MemoryStream m;
            byte[] saveID = ReadByteString(argument, 0);
            save_id = saveID;
            m = new MemoryStream();
            AccountId = DBAcess.getInstance().GetPlayerAccountId(encoding.GetString(saveID));
            uint swapped = swap32((uint)AccountId);
            await m.WriteAsync(BitConverter.GetBytes(swapped), 0, 4);
            byte[] buff = encoding.GetBytes(DBAcess.getInstance().MessageOfTheDay);
            m.WriteByte((byte)(buff.Length - 1));
            await m.WriteAsync(buff, 0, buff.Length);
            while (m.Length < 0x200) m.WriteByte(0);
            byte[] response = m.ToArray();
            await SendDataPacket(0x742A, response);
        }

        private async Task HandleCharacterRegistrationAndGuildDetails(byte[] argument)
        {
            _characterPlayerID = ExtractCharacterData(argument);

            byte[] guildStatus = GuildManagementService.GetInstance().GetPlayerGuild(_characterPlayerID);
            if (guildStatus[0] == 0x01)
            {
                isGuildMaster = true;
            }

            _guildID = swap16(BitConverter.ToUInt16(guildStatus, 1));
            // The first byte is membership status 0=none 1= master 2= member
            await SendDataPacket(OpCodes.OPCODE_DATA_REGISTER_CHAROK, guildStatus);
        }

        private async Task HandleLobbyMenu()
        {
            var nonGuildLobbies = new List<LobbyChatRoom>();

            foreach (var room in lobbyChatService.Lobbies.Values)
            {
                if (room.type == OpCodes.LOBBY_TYPE_MAIN)
                {
                    nonGuildLobbies.Add(room);
                }
            }

            await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_LOBBYLIST, BitConverter.GetBytes(swap16((ushort)nonGuildLobbies.Count)));
            foreach (var room in nonGuildLobbies)
            {
                MemoryStream m = new MemoryStream();
                m.Write(BitConverter.GetBytes(swap16((ushort)room.ID)), 0, 2);
                foreach (char c in room.name)
                    m.WriteByte((byte)c);
                m.WriteByte(0);
                m.Write(BitConverter.GetBytes(swap16((ushort)room.Users.Count)), 0, 2);
                m.Write(BitConverter.GetBytes(swap16((ushort)(room.Users.Count + 1))), 0, 2);
                while (((m.Length + 2) % 8) != 0) m.WriteByte(0);
                await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_ENTRY_LOBBY, m.ToArray());
            }
        }

        #endregion


        #region Miscellaneous Helper Methods

        static ushort swap16(ushort data) => data.Swap();

        static uint swap32(uint data) => data.Swap();

        static byte[] ReadByteString(byte[] data, int pos)
        {
            var m = new MemoryStream();
            while (true)
            {
                byte b = data[pos++];
                m.WriteByte(b);
                if (b == 0) break;
                if (pos >= data.Length) break;
            }
            return m.ToArray();
        }

        uint ExtractCharacterData(byte[] data)
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

            charModelFile = "xf" + classLetter + modelNumber + modelType + "_" + colorCode;


            Console.WriteLine("gold coin count " + goldCoinCount);
            Console.WriteLine("silver coin count " + silverCoinCount);
            Console.WriteLine("bronze coin count " + bronzeCoinCount);

            Console.WriteLine("Character Date \n save_slot " + save_slot + "\n char_id " + encoding.GetString(save_id) + " \n char_name " + encoding.GetString(char_id) +
                              "\n char_class " + char_class + "\n char_level " + char_level + "\n greeting " + encoding.GetString(greeting) + "\n charmodel " + char_model + "\n char_hp " + char_HP +
                              "\n char_sp " + char_SP + "\n char_gp " + char_GP + "\n onlien god counter " + online_god_counter + "\n offline god counter " + offline_godcounter + "\n\n\n\n full byte araray " + BitConverter.ToString(data));

            return DBAcess.getInstance().PlayerLogin(this);
        }

        char GetCharacterModelClass(uint modelNumber)
        {
            char[] classLetters = { 't', 'b', 'h', 'a', 'l', 'w' };
            int index = (int)(modelNumber & 0x0F);
            return classLetters[index];
        }
        int GetCharacterModelNumber(uint modelNumber)
        {
            return (int)(modelNumber >> 4 & 0x0F) + 1;
        }
        char GetCharacterModelType(uint modelNumber)
        {
            char[] typeLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };
            int index = (int)(modelNumber >> 12) & 0x0F;
            return typeLetters[index];

        }
        string GetCharacterModelColorCode(uint modelNumber)
        {
            string[] colorCodes = { "rd", "bl", "yl", "gr", "br", "pp" };
            int index = (int)(modelNumber >> 8) & 0x0F;
            return colorCodes[index];
        }

        #endregion Miscellaneous Helper Methods

    }
}
