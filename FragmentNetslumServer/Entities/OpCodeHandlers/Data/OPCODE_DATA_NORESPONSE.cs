using System;
using System.Collections.Generic;
using FragmentNetslumServer.Entities.Attributes;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_PING),
     OpCodeData(OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY),
     OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS3),
     OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS4),
     OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS6),
     Description("Provides no response to DATA OpCodes")]
    public sealed class OPCODE_DATA_NORESPONSE : NoResponseOpCodeHandler
    {
        private readonly ILogger logger;
        private readonly IClientProviderService clientProviderService;

        public OPCODE_DATA_NORESPONSE(ILogger logger, IClientProviderService clientProviderService)
        {
            this.logger = logger;
            this.clientProviderService = clientProviderService;
        }

        public override async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            if (request.DataOpCode == OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY)
            {
                logger.LogData(request.Data,request.OpCode,request.Client.ClientIndex,"Get Bookmark",0,0);
                var encoding = Encoding.GetEncoding("Shift-JIS");
                var areaServers = clientProviderService.AreaServers;
                responses.Add(request.CreateResponse(0x7859, BitConverter.GetBytes(((ushort)areaServers.Count).Swap())));
                
                // Now we need to send all the area servers to the client
                foreach (var client in areaServers)
                {
                    var m = new MemoryStream();
                    m.WriteByte(0);
                    if (client.ipEndPoint.Address == request.Client.ipEndPoint.Address)
                        await m.WriteAsync(client.ipdata, 0, 6);
                    else
                        await m.WriteAsync(client.externalIPAddress, 0, 6);

                    var buff = BitConverter.GetBytes((client.as_usernum.Swap()));
                    int pos = 0;
                    while (client.publish_data_1[pos++] != 0) ;
                    pos += 4;
                    client.publish_data_1[pos++] = buff[0];
                    client.publish_data_1[pos++] = buff[1];
                    await m.WriteAsync(client.publish_data_1, 0, client.publish_data_1.Length);
                    while (m.Length < 45) m.WriteByte(0);

                    var usr = encoding.GetString(BitConverter.GetBytes((client.as_usernum.Swap())));
                    var pup1 = encoding.GetString(client.publish_data_1);
                    var pup2 = encoding.GetString(client.publish_data_2);
                    logger.Debug($"AREA SERVER: {pup1}; {pup2}; {usr}", pup1, pup2, usr);

                    responses.Add(request.CreateResponse(0x786A, m.ToArray()));
                }

                return responses;
            }
            
            
            else
            {
                return await base.HandleIncomingRequestAsync(request);                
            }

            
        }
    }
}
