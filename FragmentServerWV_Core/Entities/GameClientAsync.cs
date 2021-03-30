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

        public short currentLobbyIndex = -1;

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
        private int AccountId;
        private byte save_slot;
        private byte[] save_id;
        private byte[] char_name;
        private byte[] char_id;
        private byte char_class;
        private ushort char_level;
        private byte[] greeting;
        private uint char_model;
        private ushort char_HP;
        private ushort char_SP;
        private uint char_GP;
        private ushort online_god_counter;
        private ushort offline_godcounter;
        private ushort goldCoinCount;
        private ushort silverCoinCount;
        private ushort bronzeCoinCount;
        private char classLetter;
        private int modelNumber;
        private char modelType;
        private string colorCode;
        private string charModelFile;
        private uint _characterPlayerID = 0;
        private ushort _guildID = 0;
        private bool isGuildMaster = false;
        private uint _itemDontationID = 0;
        private ushort _itemDonationQuantity = 0;
        private ushort currentGuildInvitaionSelection = 0;
        private ushort _rankingCategoryID = 0;
        private ushort lobbyType = 0;
        #endregion Player Data

        #region Events

        /// <summary>
        /// Raised when the client is disconnecting
        /// </summary>
        public event EventHandler OnGameClientDisconnected;

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
                    break;
                case OpCodes.OPCODE_DATA_LOGON_REPEAT:
                    await SendDataPacket(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x02, 0x10 });
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



        [Obsolete("Eventually replace this with direct calls")]
        static ushort swap16(ushort data) => data.Swap();

        [Obsolete("Eventually replace this with direct calls")]
        static uint swap32(uint data) => data.Swap();

    }
}
