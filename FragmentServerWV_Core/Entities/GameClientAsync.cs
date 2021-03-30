using FragmentServerWV.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities
{
    public sealed class GameClient2
    {
        private CancellationTokenSource tokenSource;
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

        //public short room_index = -1;
        public byte[] to_key;
        public byte[] from_key;
        public ushort client_seq_nr;
        public ushort server_seq_nr;
        public bool isAreaServer;
        public byte[] ipdata;
        public byte[] externalIPAddress;
        public byte[] publish_data_1;
        public byte[] publish_data_2;
        public byte[] last_status;
        public ushort as_usernum;
        public byte[] areaServerName;
        public ushort areaServerLevel;
        public byte areaServerStatus;



        public GameClient2(
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
        }





        private async Task InternalConnectionLoop(CancellationToken token)
        {
            using (token.Register(() => client.Close()))
            {
                while (!token.IsCancellationRequested)
                {
                    var packet = new Packet(ns, from_crypto);
                    if (packet.datalen == 0) continue;

                    //logger.Verbose("Invoking AcceptTcpClientAsync()");
                    //var incomingConnection = await listener.AcceptTcpClientAsync();
                    //logger.Verbose($"AcceptTcpClientAsync() has returned with a client, migrating to {nameof(IClientProviderService)}");
                    //clientProviderService.AddClient(incomingConnection, clientIds++);
                    //logger.Verbose("Performing Cancellation Token check...");
                    //token.ThrowIfCancellationRequested();
                    //logger.Verbose("Performing Cancellation Token check...passed");
                }
            }
        }

    }
}
