using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_DISSOLVED)]
    public sealed class OPCODE_DATA_GUILD_DISSOLVED : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>
            {
                request.CreateResponse(0x761A, GuildManagementService.GetInstance().DestroyGuild(request.Client._guildID))
            };
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
