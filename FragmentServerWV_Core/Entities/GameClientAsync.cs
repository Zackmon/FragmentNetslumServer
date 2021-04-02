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
        private System.Timers.Timer pingTimer;
        private readonly Encoding encoding;
        private readonly SimpleConfiguration simpleConfiguration;
        
        internal IPEndPoint ipEndPoint;
        private Crypto to_crypto;
        private Crypto from_crypto;
        private readonly ILogger logger;
        private readonly ILobbyChatService lobbyChatService;
        private readonly IClientProviderService clientProviderService;
        private readonly IMailService mailService;
        private readonly IBulletinBoardService bulletinBoardService;
        private NetworkStream ns;
        private TcpClient client;
        private uint clientIndex;
        internal short currentLobbyIndex = -1;
        internal byte[] to_key;
        internal byte[] from_key;
        internal ushort client_seq_nr;
        internal ushort server_seq_nr;
        internal bool isAreaServer;
        internal byte[] ipdata;
        internal byte[] externalIPAddress;
        internal byte[] publish_data_1;
        internal byte[] publish_data_2;
        internal byte[] last_status;
        internal ushort as_usernum;
        internal byte[] areaServerName;
        internal ushort areaServerLevel;
        internal byte areaServerStatus;

        #region Player Data
        /* These are all TEMPORARILY public */
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
        public uint _characterPlayerID = 0;
        public ushort _guildID = 0;
        public bool isGuildMaster = false;
        public uint _itemDontationID = 0;
        public ushort _itemDonationQuantity = 0;
        public ushort currentGuildInvitaionSelection = 0;
        public ushort _rankingCategoryID = 0;
        public ushort currentLobbyType = 0;
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

        /// <summary>
        /// Gets whether this client is actually an area server
        /// </summary>
        public bool IsAreaServer => isAreaServer;

        #endregion

        #region Area Server Properties

        /// <summary>
        /// Gets the name of this area server (in a byte array)
        /// </summary>
        public byte[] AreaServerName => isAreaServer ? areaServerName : null;

        /// <summary>
        /// Gets the level of this area server (if applicable)
        /// </summary>
        public ushort AreaServerLevel => isAreaServer ? areaServerLevel : throw new InvalidOperationException("This is not an Area Server");

        /// <summary>
        /// Gets the current status of the area server
        /// </summary>
        public byte AreaServerStatus => isAreaServer ? areaServerStatus : throw new InvalidOperationException("This is not an Area Server");

        /// <summary>
        /// Gets the number of players in the area server
        /// </summary>
        public ushort Players => isAreaServer ? as_usernum : throw new InvalidOperationException("This is not an Area Server");
        
        #endregion



        public GameClientAsync(
            ILogger logger,
            ILobbyChatService lobbyChatService,
            IClientProviderService clientProviderService,
            IMailService mailService,
            IBulletinBoardService bulletinBoardService,
            SimpleConfiguration simpleConfiguration)
        {
            // Why are we doing this?
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            encoding = Encoding.GetEncoding("Shift-JIS");

            // Define a few constants
            server_seq_nr = 0xe;

            this.simpleConfiguration = simpleConfiguration;
            this.logger = logger;
            this.lobbyChatService = lobbyChatService;
            this.clientProviderService = clientProviderService;
            this.mailService = mailService;
            this.bulletinBoardService = bulletinBoardService;
        }

        /// <summary>
        /// Initializes the client with the appropriate seed data
        /// </summary>
        /// <param name="clientIndex">The local client index</param>
        /// <param name="tcpClient">The connected <see cref="TcpClient"/></param>
        public void InitializeClient(uint clientIndex, TcpClient tcpClient)
        {
            this.client = tcpClient;
            this.clientIndex = clientIndex;
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


        /// <summary>
        /// Tells the Game Client to disconnect safely
        /// </summary>
        public void Exit()
        {
            tokenSource.Cancel();
        }


        private async Task InternalConnectionLoop(CancellationToken token)
        {
            var tickRate = Convert.ToInt32(simpleConfiguration.Get("tick", "30"));
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
                            await Task.Delay(TimeSpan.FromMilliseconds(tickRate));
                        }
                        else
                        {
                            // Packet has been read, bring over what we do now
                            await HandleIncomingPacket(packet);
                        }
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
                        await rm.DispatchStatusAsync(argument, (int)clientIndex);
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
                case OpCodes.OPCODE_DATA_MAIL_SEND:
                    // The original code wrote to a memorystream
                    // we're just gonna write to a 4k buffer
                    var buffer = new byte[4096];
                    while (ns.DataAvailable) await ns.ReadAsync(buffer, 0, buffer.Length);
                    await mailService.SaveMailAsync(argument);
                    await SendDataPacket(OpCodes.OPCODE_DATA_MAIL_SEND_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_MAIL_GET:
                    await HandleMailInbox(argument);
                    break;
                case OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY:
                    await HandleMailContentRequest(argument);
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_GETLIST:
                    await HandleGetAreaServers(argument);
                    break;
                case 0x771E:
                    // I'm not quite sure what this is for
                    await SendDataPacket(0x771F, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GET_ALL_GUILDS:
                    await HandleGetGuildListForShopping(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GET_LIST_OF_ITEMS:
                    await HandleGetGuildStoreItems(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GETMENU:
                    await HandleGetGuildMenu(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GET_INFO: // Get Guild Info
                    await HandleRetrieveGuildDetails(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_CREATE: //create Guild
                    await HandleGuildCreation(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS: // get the logged in character Guild Info (if enlisted)
                    await HandleGetGuildActiveMembers(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_MEMBERLIST: //get Guild member list
                    await HandleGetGuildMemberList(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GETITEMS_TOBUY: // Get Guild Items for members to buy from 
                    await HandleGetGuildItemsForPurchase(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GETITEMS: //Guild Item List
                    await HandleGetGuildItems(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_BUY_ITEM: //buy Item from guild 
                    await HandlePlayerBuysGuildItem(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_DONATE_ITEM: // Donate Item to Guild
                    await HandlePlayerDonatesItemToGuild(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GET_DONATION_SETTINGS:
                    await SendDataPacket(0x787a, GuildManagementService.GetInstance().GetItemDonationSettings(isGuildMaster));
                    break;
                case OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY:
                    await HandlePlayerUpdatingItemPricingAndAvailability(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING: //update item pricing (from Master window)
                    await SendDataPacket(0x7713, GuildManagementService.GetInstance().SetItemVisibilityAndPrice(argument));
                    break;
                case 0x787B:// no idea what this is but I think it's only ACK
                    await SendDataPacket(0x787C, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_GUILD_GM_LEAVING:// Leve Guild and assign someone else the master of the guild
                    await HandleGuildMasterLeaving(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_PLAYER_LEAVING: //Player leaving the guild
                    await HandlePlayerLeavingGuild();
                    break;
                case OpCodes.OPCODE_DATA_GUILD_PLAYER_KICKED: //kick player from guild
                    await HandleKickingPlayerFromGuild(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_DISSOLVED: // Dissolve the guild
                    await HandleGuildBeingDissolved();
                    break;
                case OpCodes.OPCODE_DATA_GUILD_UPDATE_DETAILS: // Update Guild Emblem and Comment
                    await HandleGuildDetailsBeingUpdated(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_TAKE_GP: // Take out GP from the Guild Inventory
                    await HandlePlayerTakingGuildGP(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_TAKE_ITEM:// take item from the guild inventory
                    await HandlePlayerTakingGuildItem(argument);
                    break;
                case OpCodes.OPCODE_DATA_GUILD_DONATE_COINS:// donate Coins to Guild
                    await HandlePlayerDonatesCoinsToGuild(argument);
                    break;
                case OpCodes.OPCODE_INVITE_TO_GUILD: //invite player to Guild
                    await HandleInvitePlayerToGuild(argument);
                    break;
                case OpCodes.OPCODE_ACCEPT_GUILD_INVITE: //accept Guild Invitation
                    await HandleGuildAcceptOrReject(argument);
                    break;
                case OpCodes.OPCODE_GUILD_VIEW: //get Guild info (in lobby )
                    await HandleGuildView(argument);
                    break;
                case OpCodes.OPCODE_DATA_AS_UPDATE_STATUS:
                    HandleExtractAreaServerInformation(argument);
                    break;
                case 0x780f: // Create Thread Request 
                    await SendDataPacket(0x7810, new byte[] { 0x01, 0x92 });
                    break;
                case OpCodes.OPCODE_DATA_BBS_POST:
                    await HandleCreateBbsPost(argument);
                    break;
                case OpCodes.OPCODE_RANKING_VIEW_ALL: // ranking Page
                    await HandleGetRankingPageInformation(argument);
                    break;
                case OpCodes.OPCODE_RANKING_VIEW_PLAYER: //Ranking Char Detail
                    await HandleGetRankingPlayerInformation(argument);
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS:
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT:
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_EXIT_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_BBS_GETMENU:
                    await HandleSendBbsMainMenu(argument);
                    break;
                case OpCodes.OPCODE_DATA_BBS_THREAD_GETMENU:
                    await HandleSendBbsThread(argument);
                    break;
                case OpCodes.OPCODE_DATA_BBS_THREAD_GET_CONTENT:
                    await HandleSendBbsThreadContent(argument);
                    break;
                case OpCodes.OPCODE_DATA_NEWS_GETMENU:
                    await SendDataPacket(OpCodes.OPCODE_DATA_NEWS_CATEGORYLIST, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_AS_DISKID:
                    await SendDataPacket(OpCodes.OPCODE_DATA_AS_DISKID_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_LOBBY_EVENT:
                    if (lobbyChatService.TryGetLobby((ushort)LobbyIndex, out var lcr))
                    {
                        await lcr.DispatchPublicBroadcastAsync(argument, this.ClientIndex);
                    }
                    break;
                case OpCodes.OPCODE_DATA_MAILCHECK:
                    if (DBAcess.getInstance().checkForNewMailByAccountID(ReadAccountID(argument, 0)))
                        await SendDataPacket(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] { 0x00, 0x00, 0x01, 0x00 });
                    else
                        await SendDataPacket(OpCodes.OPCODE_DATA_MAILCHECK_OK, new byte[] { 0x00, 0x01 });

                    break;
                case OpCodes.OPCODE_DATA_BBS_GET_UPDATES:
                    await SendDataPacket(0x786b, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_NEWCHECK:
                    await SendDataPacket(OpCodes.OPCODE_DATA_NEWCHECK_OK, new byte[] { 0x00, 0x00 });
                    break;
                case OpCodes.OPCODE_DATA_COM:
                    await SendDataPacket(OpCodes.OPCODE_DATA_COM_OK, new byte[] { 0xDE, 0xAD });
                    break;
                case 0x787E: // enter ranking screen
                    await SendDataPacket(0x787F, new byte[] { 0x00, 0x00 });
                    break;
                case 0x788C:
                    ushort destid = swap16(BitConverter.ToUInt16(argument, 2));
                    //Server.Instance.LobbyChatRooms[room_index].DispatchPrivateBroadcast(argument, this.index, destid);
                    if (lobbyChatService.TryGetLobby((ushort)LobbyIndex, out var p))
                    {
                        await p.DispatchPrivateBroadcastAsync(argument, ClientIndex, destid);
                    }
                    break;
                case OpCodes.OPCODE_DATA_SELECT_CHAR:

                    await SendDataPacket(OpCodes.OPCODE_DATA_SELECT_CHAROK, new byte[] { 0x00, 0x00 });

                    break;
                case OpCodes.OPCODE_DATA_SELECT2_CHAR:

                    await SendDataPacket(OpCodes.OPCODE_DATA_SELECT2_CHAROK, new byte[] { 0x30, 0x30, 0x30, 0x30 });
                    break;
                case OpCodes.OPCODE_DATA_LOGON:
                    if (argument[1] == OpCodes.OPCODE_DATA_SERVERKEY_CHANGE)
                    {
                        logger.Information("Client #" + this.ClientIndex + " : New Area Server Joined");
                        isAreaServer = true;
                        await SendDataPacket(0x78AC, new byte[] { 0xDE, 0xAD });
                    }
                    else
                    {
                        logger.Information("Client #" + ClientIndex + " : New Game Client Joined");
                        await SendDataPacket(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x74, 0x32 });
                    }

                    break;
                case OpCodes.OPCODE_DATA_AS_PUBLISH:
                    await SendDataPacket(OpCodes.OPCODE_DATA_AS_PUBLISH_OK, new byte[] { 0x00, 0x00 });
                    break;
                default:
                    logger.Information("Client #" + this.ClientIndex +
                                  " : \n !!!UNKNOWN DATA CODE RECEIVED, PLEASE REPORT : 0x" +
                                  code.ToString("X4") + "!!!\n");
                    break;
            }


        }

        private async Task HandleSendBbsThreadContent(byte[] argument)
        {
            var q = swap32(BitConverter.ToUInt32(argument, 4));
            var postID = Convert.ToInt32(q);
            var bbsPostBody = await bulletinBoardService.GetThreadPostContentAsync(postID); // DBAcess.getInstance().GetPostBodyByPostId(postID);
            var bbsPostData = await bulletinBoardService.ConvertThreadPostToBytesAsync(bbsPostBody);
            await SendDataPacket(0x781d, bbsPostData);
        }

        private async Task HandleSendBbsThread(byte[] argument)
        {
            var i = swap32(BitConverter.ToUInt32(argument, 0));
            var threadID = Convert.ToInt32(i);
            var postMetaList = await bulletinBoardService.GetThreadDetailsAsync(threadID);
            await SendDataPacket(OpCodes.OPCODE_DATA_BBS_THREAD_LIST, BitConverter.GetBytes(swap32((uint)postMetaList.Count)));
            foreach (var meta in postMetaList)
            {
                var postMetaBytes = await bulletinBoardService.ConvertThreadDetailsToBytesAsync(meta);
                await SendDataPacket(OpCodes.OPCODE_DATA_BBS_ENTRY_POST_META, postMetaBytes);
            }
        }

        private async Task HandleSendBbsMainMenu(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            if (u == 0)
            {
                var categoryList = await bulletinBoardService.GetCategoriesAsync();
                await SendDataPacket(OpCodes.OPCODE_DATA_BBS_CATEGORYLIST, BitConverter.GetBytes(swap16((ushort)categoryList.Count)));
                foreach (var category in categoryList)
                {
                    var categoryData = await bulletinBoardService.ConvertCategoryToBytesAsync(category);
                    await SendDataPacket(OpCodes.OPCODE_DATA_BBS_ENTRY_CATEGORY, categoryData);
                }
            }

            else
            {
                var categoryID = Convert.ToInt32(u);
                var threadsList = await bulletinBoardService.GetThreadsAsync(categoryID);
                await SendDataPacket(OpCodes.OPCODE_DATA_BBS_THREADLIST, BitConverter.GetBytes(swap16((ushort)threadsList.Count)));
                foreach (var thread in threadsList)
                {
                    var threadData = await bulletinBoardService.ConvertThreadToBytesAsync(thread);
                    await SendDataPacket(OpCodes.OPCODE_DATA_BBS_ENTRY_THREAD, threadData);
                }
            }
        }

        private async Task HandleGetRankingPlayerInformation(byte[] argument)
        {
            uint rankPlayerID = swap32(BitConverter.ToUInt32(argument, 0));
            await SendDataPacket(0x7839, RankingManagementService.GetInstance().getRankingPlayerInfo(rankPlayerID));
        }

        private async Task HandleGetRankingPageInformation(byte[] argument)
        {
            var rankingArgs = swap16(BitConverter.ToUInt16(argument, 0));
            if (rankingArgs == 0) // get the first ranking page
            {
                var rankCategoryList = RankingManagementService.GetInstance().GetRankingCategory();
                await SendDataPacket(0x7833, BitConverter.GetBytes(swap16((ushort)rankCategoryList.Count)));

                foreach (var category in rankCategoryList)
                {
                    await SendDataPacket(0x7835, category);
                }
            }
            else if (rankingArgs >= 8) // get class List
            {
                _rankingCategoryID = rankingArgs;
                var rankClassList = RankingManagementService.GetInstance().GetClassList();
                await SendDataPacket(0x7833, BitConverter.GetBytes(swap16((ushort)rankClassList.Count)));
                foreach (var category in rankClassList)
                {
                    await SendDataPacket(0x7835, category);
                }
            }
            else
            {
                var playerRankingList = RankingManagementService.GetInstance().GetPlayerRanking(_rankingCategoryID, rankingArgs);
                await SendDataPacket(0x7836, BitConverter.GetBytes(swap32((uint)playerRankingList.Count)));
                foreach (var player in playerRankingList)
                {
                    await SendDataPacket(0x7837, player);
                }
            }
        }

        private async Task HandleCreateBbsPost(byte[] argument)
        {
            uint id = swap32(BitConverter.ToUInt32(argument, 0));
            DBAcess.getInstance().CreateNewPost(argument, id);
            await SendDataPacket(0x7813, new byte[] { 0x00, 0x00 });
        }

        private void HandleExtractAreaServerInformation(byte[] argument)
        {
            publish_data_2 = argument;
            ExtractAreaServerData(argument);
        }

        private async Task HandleGuildView(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            currentGuildInvitaionSelection = u;
            await SendDataPacket(0x772D, GuildManagementService.GetInstance().GetGuildInfo(u));
        }

        private async Task HandleGuildAcceptOrReject(byte[] argument)
        {
            var ms = new MemoryStream();
            await ms.WriteAsync(new byte[] { 0x76, 0xB0, 0x54, 0x45, 0x53, 0x54, 0x00 });
            if (argument[1] == 0x08) //accepted the invitation
            {
                DBAcess.getInstance().EnrollPlayerInGuild(currentGuildInvitaionSelection, _characterPlayerID, false);
                await SendDataPacket(0x760A, ms.ToArray()); // send guild ID
            }
            else
            {
                // rejected
                await SendDataPacket(0x760A, ms.ToArray()); // send guild ID
            }
        }

        private async Task HandleInvitePlayerToGuild(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            // Fun fact: we don't check if we're in a guild lobby
            // 'cause I think this is only possible to invoke from a guild lobby so
            // screw it
            if (lobbyChatService.TryGetLobby((ushort)currentLobbyIndex, out var guildLobby))
            {
                await guildLobby.GuildInvitationAsync(argument, (int)this.clientIndex, u, _guildID);
                logger.Information($"Invited Player ID {u}");
                await SendDataPacket(0x7604, new byte[] { 0x00, 0x00 }); //send to confirm that the player accepted the invite 
            }
        }

        private async Task HandlePlayerDonatesCoinsToGuild(byte[] argument)
        {
            await SendDataPacket(0x7701, GuildManagementService.GetInstance().DonateCoinsToGuild(argument));
        }

        private async Task HandlePlayerTakingGuildItem(byte[] argument)
        {
            ushort guildIDTakeItem = swap16(BitConverter.ToUInt16(argument, 0));
            uint itemIDToTakeOut = swap32(BitConverter.ToUInt32(argument, 2));
            ushort quantityToTake = swap16(BitConverter.ToUInt16(argument, 6));

            logger.Debug("Guild ID " + guildIDTakeItem + "\nItem ID to take " + itemIDToTakeOut + "\n quantity to take out " + quantityToTake);
            await SendDataPacket(0x7711, GuildManagementService.GetInstance().TakeItemFromGuild(guildIDTakeItem, itemIDToTakeOut, quantityToTake)); // quantity  to give to the player
        }

        private async Task HandlePlayerTakingGuildGP(byte[] argument)
        {
            ushort guildIDTakeMoney = swap16(BitConverter.ToUInt16(argument, 0));
            uint amountOfMoneyToTakeOut = swap32(BitConverter.ToUInt32(argument, 2));

            logger.Debug("Guild ID " + guildIDTakeMoney + "\nAmount of money to Take out " + amountOfMoneyToTakeOut);
            await SendDataPacket(0x770F, GuildManagementService.GetInstance().TakeMoneyFromGuild(guildIDTakeMoney, amountOfMoneyToTakeOut)); // amount of money to give to the player 
        }

        private async Task HandleGuildDetailsBeingUpdated(byte[] argument)
        {
            await SendDataPacket(0x761D, GuildManagementService.GetInstance().UpdateGuildEmblemComment(argument, _guildID));
        }

        private async Task HandleGuildBeingDissolved()
        {
            await SendDataPacket(0x761A, GuildManagementService.GetInstance().DestroyGuild(_guildID));
        }

        private async Task HandleKickingPlayerFromGuild(byte[] argument)
        {
            uint playerToKick = swap32(BitConverter.ToUInt32(argument, 0));
            await SendDataPacket(0x7865, GuildManagementService.GetInstance().KickPlayerFromGuild(_guildID, playerToKick));
        }

        private async Task HandlePlayerLeavingGuild()
        {
            await SendDataPacket(0x7617, GuildManagementService.GetInstance().LeaveGuild(_guildID, _characterPlayerID));
        }

        private async Task HandleGuildMasterLeaving(byte[] argument)
        {
            uint assigningPlayerID = swap32(BitConverter.ToUInt32(argument, 0));
            await SendDataPacket(0x788E, GuildManagementService.GetInstance().LeaveGuildAndAssignMaster(_guildID, assigningPlayerID));
        }

        private async Task HandlePlayerUpdatingItemPricingAndAvailability(byte[] argument)
        {
            uint GeneralPrice = swap32(BitConverter.ToUInt32(argument, 0));
            uint MemberPrice = swap32(BitConverter.ToUInt32(argument, 4));
            bool isGeneral = BitConverter.ToBoolean(argument, 8);
            bool isMember = BitConverter.ToBoolean(argument, 9);

            Console.Write("GenePrice " + GeneralPrice + "\nMemberPrice " + MemberPrice + "\nisGeneral " + isGeneral + "\nisMember " + isMember);


            await SendDataPacket(0x7705, GuildManagementService.GetInstance().AddItemToGuildInventory(_guildID, _itemDontationID,
                _itemDonationQuantity, GeneralPrice, MemberPrice, isGeneral, isMember, isGuildMaster)); // how many to deduct from the player
        }

        private async Task HandlePlayerDonatesItemToGuild(byte[] argument)
        {
            _itemDontationID = swap32(BitConverter.ToUInt32(argument, 2));
            _itemDonationQuantity = swap16(BitConverter.ToUInt16(argument, 6));
            await SendDataPacket(0x7704, GuildManagementService.GetInstance().GetPriceOfItemToBeDonated(_guildID, _itemDontationID));
        }

        private async Task HandlePlayerBuysGuildItem(byte[] argument)
        {
            await SendDataPacket(0x770D, GuildManagementService.GetInstance().BuyItemFromGuild(argument));
        }

        private async Task HandleGetGuildItems(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            List<byte[]> allGuildItems =
                GuildManagementService.GetInstance().GetAllGuildItemsWithSettings(u);
            await SendDataPacket(0x7729, BitConverter.GetBytes(swap16((ushort)allGuildItems.Count)));

            foreach (var item in allGuildItems)
            {
                await SendDataPacket(0x772A, item);
            }
        }

        private async Task HandleGetGuildItemsForPurchase(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            List<byte[]> membersItemList = GuildManagementService.GetInstance().GetGuildItems(u, false);
            await SendDataPacket(0x7709, BitConverter.GetBytes(swap16((ushort)membersItemList.Count))); // number of items

            foreach (var item in membersItemList)
            {
                await SendDataPacket(0x770a, item);
            }
        }

        private async Task HandleGetGuildMemberList(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            if (u == 0)// Guild Member Category List
            {
                List<byte[]> listOfClasses = GuildManagementService.GetInstance().GetClassList();
                await SendDataPacket(0x7611, BitConverter.GetBytes(swap16((ushort)listOfClasses.Count))); //Size of List
                foreach (var className in listOfClasses)
                {
                    await SendDataPacket(0x7613, className);// send categories    
                }

            }
            else //MemberList in that Category
            {
                List<byte[]> memberList =
                    GuildManagementService.GetInstance().GetGuildMembersListByClass(_guildID, u, _characterPlayerID);
                await SendDataPacket(0x7614, BitConverter.GetBytes(swap16((ushort)memberList.Count)));//Size of List

                foreach (var member in memberList)
                {
                    await SendDataPacket(0x7615, member); //Member Details
                }

            }
        }

        private async Task HandleGetGuildActiveMembers(byte[] argument)
        {
            var guildId = swap16(BitConverter.ToUInt16(argument, 0));
            await SendDataPacket(0x789d, GuildManagementService.GetInstance().GetGuildInfo(guildId));
        }

        internal async Task SendRegularPacket(ushort code, byte[] data, uint checksum)
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

        internal async Task SendDataPacket(ushort code, byte[] data)
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
            if (tokenSource.IsCancellationRequested)
            {
                pingTimer.Dispose();
                return;
            }
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
            await room.DispatchAllStatusAsync((int)this.clientIndex);
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
                while (((m.Length + 2) % 8) != 0) m.WriteByte(0); // looks like some form of padding to align the message
                await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_ENTRY_LOBBY, m.ToArray());
            }
        }

        private async Task HandleMailInbox(byte[] argument)
        {
            var accountId = ReadAccountID(argument, 0);
            var mail = await mailService.GetMailAsync(accountId);
            await SendDataPacket(OpCodes.OPCODE_DATA_MAIL_GETOK, BitConverter.GetBytes(swap32((uint)mail.Count)));

            foreach (var item in mail)
            {
                var mailContent = await mailService.ConvertMailMetaIntoBytes(item);
                await SendDataPacket(OpCodes.OPCODE_DATA_MAIL_GET_NEWMAIL_HEADER, mailContent);
            }
        }

        private async Task HandleMailContentRequest(byte[] argument)
        {
            var mailId = (int)swap32(BitConverter.ToUInt32(argument, 4));
            var messageBodyModel = await mailService.GetMailContent(mailId);
            var messageBody = await mailService.ConvertMailBodyIntoBytes(messageBodyModel);
            await SendDataPacket(OpCodes.OPCODE_DATA_MAIL_GET_MAIL_BODY_RESPONSE, messageBody);
        }

        private async Task HandleGetAreaServers(byte[] data)
        {
            // Zero indicates "gimme the list of categories"
            if (data[1] == 0)
            {

                logger.Information($"Client #{clientIndex} has requested the list of available categories");

                // Tell the game there's ONE category
                await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_CATEGORYLIST, new byte[] { 0x00, 0x01 });

                // This is the category. This byte array translates to, basically, MAIN:
                // "\0\u0001MAIN\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ne"
                // Uses \u0001MAIN with the rest as padding, more than likely
                await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_CATEGORY,
                    new byte[]
                    {
                        0x00, 0x01, 0x4D, 0x41, 0x49, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x6E, 0x65
                    });
            }
            else
            {
                logger.Information($"Client #{clientIndex} has requested the list of area servers for the MAIN category");

                // We don't care about categories any longer. We're here for the list of servers
                var areaServers = clientProviderService.AreaServers;

                // Tell the client how many area servers we got
                await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_SERVERLIST, BitConverter.GetBytes(swap16((ushort)areaServers.Count)));

                // Now we need to send all the area servers to the client
                foreach (var client in areaServers)
                {
                    var m = new MemoryStream();
                    m.WriteByte(0);
                    if (client.ipEndPoint.Address == this.ipEndPoint.Address)
                        await m.WriteAsync(client.ipdata, 0, 6);
                    else
                        await m.WriteAsync(client.externalIPAddress, 0, 6);

                    var buff = BitConverter.GetBytes(swap16(client.as_usernum));
                    int pos = 0;
                    while (client.publish_data_1[pos++] != 0) ;
                    pos += 4;
                    client.publish_data_1[pos++] = buff[0];
                    client.publish_data_1[pos++] = buff[1];
                    await m.WriteAsync(client.publish_data_1, 0, client.publish_data_1.Length);
                    while (m.Length < 45) m.WriteByte(0);

                    var usr = encoding.GetString(BitConverter.GetBytes(swap16(client.as_usernum)));
                    var pup1 = encoding.GetString(client.publish_data_1);
                    var pup2 = encoding.GetString(client.publish_data_2);
                    logger.Debug($"AREA SERVER: {pup1}; {pup2}; {usr}", pup1, pup2, usr);

                    await SendDataPacket(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_SERVER, m.ToArray());
                }

            }
        }

        private async Task HandleGetGuildListForShopping(byte[] argument)
        {
            var u = swap16(BitConverter.ToUInt16(argument, 0));
            // This is very similar to the area server handler
            // If this is zero, show the guild category breakdown (basically)
            // otherwise, show them all the guilds
            if (u == 0)
            {
                // Tell them we have ONE guild
                await SendDataPacket(0x7723, new byte[] { 0x00, 0x01 });

                // This should print "All" on the game
                await SendDataPacket(0x7725, new byte[] { 0x00, 0x01, 0x41, 0x6c, 0x6c, 0x00 });
            }
            else
            {
                var listOfGuilds = GuildManagementService.GetInstance().GetListOfGuilds();
                await SendDataPacket(0x7726, BitConverter.GetBytes(swap16((ushort)listOfGuilds.Count)));
                foreach (var guildName in listOfGuilds)
                {
                    await SendDataPacket(0x7727, guildName);
                }
            }
        }

        private async Task HandleGetGuildStoreItems(byte[] argument)
        {
            var guildId = swap16(BitConverter.ToUInt16(argument, 0));
            var listOfItemsForGeneralStore = GuildManagementService.GetInstance().GetGuildItems(guildId, true);

            await SendDataPacket(0x7730, BitConverter.GetBytes(swap16((ushort)listOfItemsForGeneralStore.Count)));

            foreach (var item in listOfItemsForGeneralStore)
            {
                await SendDataPacket(0x7731, item);
            }
        }

        private async Task HandleGetGuildMenu(byte[] argument)
        {
            var mode = swap16(BitConverter.ToUInt16(argument, 0));
            if (mode == 0)// Guild Category List
            {
                await SendDataPacket(0x7734, new byte[] { 0x00, 0x01 }); //Size of List
                await SendDataPacket(0x7736, new byte[] { 0x00, 0x01, 0x41, 0x6c, 0x6c, 0x00 }); //Category Name (ALL)
            }
            else // Guild Listing of the selected Category
            {
                var listOfGuild = GuildManagementService.GetInstance().GetListOfGuilds();
                await SendDataPacket(0x7737, BitConverter.GetBytes(swap16((ushort)listOfGuild.Count))); //Size of List

                foreach (var guildName in listOfGuild)
                {
                    await SendDataPacket(0x7738, guildName);
                }

            }
        }

        private async Task HandleRetrieveGuildDetails(byte[] argument)
        {
            var guildId = swap16(BitConverter.ToUInt16(argument, 0));
            await SendDataPacket(OpCodes.OPCODE_DATA_GET_GUILD_INFO_RESPONSE, GuildManagementService.GetInstance().GetGuildInfo(guildId));
        }

        private async Task HandleGuildCreation(byte[] argument)
        {
            var u = GuildManagementService.GetInstance().CreateGuild(argument, _characterPlayerID);
            _guildID = u;
            isGuildMaster = true;
            await SendDataPacket(0x7601, BitConverter.GetBytes(swap16(u)));
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

            logger.Information("Player Information");
            logger.Information("gold coin count " + goldCoinCount);
            logger.Information("silver coin count " + silverCoinCount);
            logger.Information("bronze coin count " + bronzeCoinCount);

            logger.Information("Character Date \n save_slot " + save_slot + "\n char_id " + encoding.GetString(save_id) + " \n char_name " + encoding.GetString(char_id) +
                              "\n char_class " + char_class + "\n char_level " + char_level + "\n greeting " + encoding.GetString(greeting) + "\n charmodel " + char_model + "\n char_hp " + char_HP +
                              "\n char_sp " + char_SP + "\n char_gp " + char_GP + "\n onlien god counter " + online_god_counter + "\n offline god counter " + offline_godcounter + "\n\n\n\n full byte araray " + BitConverter.ToString(data));

            return DBAcess.getInstance().PlayerLogin(this);
        }

        static char GetCharacterModelClass(uint modelNumber)
        {
            char[] classLetters = { 't', 'b', 'h', 'a', 'l', 'w' };
            int index = (int)(modelNumber & 0x0F);
            return classLetters[index];
        }

        static int GetCharacterModelNumber(uint modelNumber)
        {
            return (int)(modelNumber >> 4 & 0x0F) + 1;
        }

        static char GetCharacterModelType(uint modelNumber)
        {
            char[] typeLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };
            int index = (int)(modelNumber >> 12) & 0x0F;
            return typeLetters[index];
        }

        static string GetCharacterModelColorCode(uint modelNumber)
        {
            string[] colorCodes = { "rd", "bl", "yl", "gr", "br", "pp" };
            int index = (int)(modelNumber >> 8) & 0x0F;
            return colorCodes[index];
        }

        static int ReadAccountID(byte[] data, int pos)
        {
            byte[] accountID = new byte[4];
            Buffer.BlockCopy(data, pos, accountID, 0, 4);
            return (int)swap32(BitConverter.ToUInt32(accountID));
        }

        void ExtractAreaServerData(byte[] data)
        {
            int pos = 67;
            areaServerName = ReadByteString(data, pos);
            pos += areaServerName.Length;
            areaServerLevel = swap16(BitConverter.ToUInt16(data, pos));
            pos += 4;
            areaServerStatus = data[pos++];
        }

        #endregion Miscellaneous Helper Methods

    }
}
