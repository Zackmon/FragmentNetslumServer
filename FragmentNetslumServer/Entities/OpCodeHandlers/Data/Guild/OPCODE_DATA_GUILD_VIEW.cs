using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_VIEW)]
    public sealed class OPCODE_DATA_GUILD_VIEW : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_VIEW(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
            
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var u = BitConverter.ToUInt16(request.Data, 0).Swap();
            request.Client.currentGuildInvitaionSelection = u;
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x772D,
                    _guildManagementService.GetGuildInfo(u))
            });
        }
    }
}
