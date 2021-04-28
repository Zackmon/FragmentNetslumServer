using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS)]
    public sealed class OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var guildId = swap16(BitConverter.ToUInt16(request.Data, 0));
            return Task.FromResult<IEnumerable<ResponseContent>>(new[] { request.CreateResponse(0x789d, GuildManagementService.GetInstance().GetGuildInfo(guildId)) });
        }
    }
}
