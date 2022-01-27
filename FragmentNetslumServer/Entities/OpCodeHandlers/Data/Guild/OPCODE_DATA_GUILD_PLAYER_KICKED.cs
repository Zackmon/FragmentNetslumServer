using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_PLAYER_KICKED)]
    public sealed class OPCODE_DATA_GUILD_PLAYER_KICKED : IOpCodeHandler
    {
        
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_PLAYER_KICKED(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            uint playerToKick = BitConverter.ToUInt32(request.Data, 0).Swap();

            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x7865,
                    _guildManagementService.KickPlayerFromGuild(request.Client._guildID, playerToKick))
            });
        }
    }
}
