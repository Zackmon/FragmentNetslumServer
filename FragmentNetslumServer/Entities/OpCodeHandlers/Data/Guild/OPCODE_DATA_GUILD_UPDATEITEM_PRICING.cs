using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;
using Serilog;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING)]
    public sealed class OPCODE_DATA_GUILD_UPDATEITEM_PRICING : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_UPDATEITEM_PRICING(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
            
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x7713,
                    _guildManagementService.SetItemVisibilityAndPrice(request.Data))
            });
        }
    }
}
