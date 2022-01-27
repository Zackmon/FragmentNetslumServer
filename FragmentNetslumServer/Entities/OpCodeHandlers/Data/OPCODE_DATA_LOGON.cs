using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_LOGON)]
    public sealed class OPCODE_DATA_LOGON : IOpCodeHandler
    {
        private readonly ILogger _logger;
        public OPCODE_DATA_LOGON(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            
            if (request.Data[1] == OpCodes.OPCODE_DATA_SERVERKEY_CHANGE)
            {
                _logger.Information("Client #{@clientIndex} has identified itself as an Area Server", request.Client.ClientIndex);
                request.Client.isAreaServer = true;
                return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_AREASERVER_OK, new byte[] { 0xDE, 0xAD }) };
                
            }
            else
            {
                _logger.Information("Client #{@clientIndex} has identified itself as a Game Client (PS2 / PCSX2)", request.Client.ClientIndex);
                return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_LOGON_RESPONSE, new byte[] { 0x74, 0x32 }) };
            }
        }
    }
}
