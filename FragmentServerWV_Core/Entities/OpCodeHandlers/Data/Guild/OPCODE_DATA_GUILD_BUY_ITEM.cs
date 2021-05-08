using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_BUY_ITEM)]
    public sealed class OPCODE_DATA_GUILD_BUY_ITEM : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>
            {
                request.CreateResponse(0x770D, GuildManagementService.GetInstance().BuyItemFromGuild(request.Data))
            };
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
