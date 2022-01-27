using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Lobby
{

    [OpCodeData(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_GETLIST)]
    public sealed class OPCODE_DATA_LOBBY_GETSERVERS_GETLIST : IOpCodeHandler
    {
        private readonly ILogger logger;
        private readonly IClientProviderService clientProviderService;

        public OPCODE_DATA_LOBBY_GETSERVERS_GETLIST(ILogger logger, IClientProviderService clientProviderService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.clientProviderService = clientProviderService ?? throw new ArgumentNullException(nameof(clientProviderService));
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            // Zero indicates "gimme the list of categories"
            var encoding = Encoding.GetEncoding("Shift-JIS");
            var responses = new List<ResponseContent>();
            if (request.Data[1] == 0)
            {

                logger.Information("Client #{@clientIndex} has requested the list of available categories for Area Server Selection", new { clientIndex = request.Client.ClientIndex });

                // Tell the game there's ONE category
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_CATEGORYLIST, new byte[] { 0x00, 0x01 }));

                // This is the category. This byte array translates to, basically, MAIN:
                // "\0\u0001MAIN\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ne"
                // Uses \u0001MAIN with the rest as padding, more than likely
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_CATEGORY,
                    new byte[]
                    {
                        0x00, 0x01, 0x4D, 0x41, 0x49, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x6E, 0x65
                    }));
            }
            else
            {
                logger.Information("Client #{@clientIndex} has requested the list of area servers for the MAIN category", new { clientIndex = request.Client.ClientIndex });

                // We don't care about categories any longer. We're here for the list of servers
                var areaServers = clientProviderService.AreaServers;

                // Tell the client how many area servers we got
                responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_SERVERLIST, BitConverter.GetBytes(swap16((ushort)areaServers.Count))));

                // Now we need to send all the area servers to the client
                foreach (var client in areaServers)
                {
                    var m = new MemoryStream();
                    m.WriteByte(0);
                    if (client.ipEndPoint.Address == request.Client.ipEndPoint.Address)
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

                    responses.Add(request.CreateResponse(OpCodes.OPCODE_DATA_LOBBY_GETSERVERS_ENTRY_SERVER, m.ToArray()));
                }

                // Shorten the TTL / ping timer so we can detect more
                // quickly when a player disconnects and hops over to an area server.
                // pingTimer.Interval = EnhancedPingTimeout.TotalMilliseconds;

            }
            return responses;
        }
    }
}
