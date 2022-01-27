using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_BUY_ITEM)]
    public sealed class OPCODE_DATA_GUILD_BUY_ITEM : IOpCodeHandler
    {
        private readonly IGuildManagementService guildManagementService;

        public OPCODE_DATA_GUILD_BUY_ITEM(IGuildManagementService guildManagementService)
        {
            this.guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>
            {
                request.CreateResponse(0x770D, guildManagementService.BuyItemFromGuild(request.Data))
            };
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
