using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_PLAYER_LEAVING)]
    public sealed class OPCODE_DATA_GUILD_PLAYER_LEAVING : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_PLAYER_LEAVING(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x7617,
                    _guildManagementService.LeaveGuild(request.Client._guildID, request.Client._characterPlayerID))
            });
        }
    }
}
