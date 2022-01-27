using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_DONATE_COINS)]
    public sealed class OPCODE_DATA_GUILD_DONATE_COINS : IOpCodeHandler
    {
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_DONATE_COINS(IGuildManagementService guildManagementService)
        {
            _guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
                { request.CreateResponse(0x7701, _guildManagementService.DonateCoinsToGuild(request.Data)) });
        }
    }
    
}
