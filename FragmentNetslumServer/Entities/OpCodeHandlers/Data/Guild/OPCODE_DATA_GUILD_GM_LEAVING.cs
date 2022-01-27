using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GM_LEAVING)]
    public sealed class OPCODE_DATA_GUILD_GM_LEAVING : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_GM_LEAVING(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            uint assigningPlayerID = BitConverter.ToUInt32(request.Data, 0).Swap();

            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x788E,
                    _guildManagementService.LeaveGuildAndAssignMaster(request.Client._guildID, assigningPlayerID))
            });
            
        }
    }
}
