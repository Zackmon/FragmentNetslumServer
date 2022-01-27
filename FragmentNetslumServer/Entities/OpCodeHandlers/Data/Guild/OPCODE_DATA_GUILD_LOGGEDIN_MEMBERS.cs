using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS)]
    public sealed class OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS : IOpCodeHandler
    {
        private readonly IGuildManagementService guildManagementService;

        public OPCODE_DATA_GUILD_LOGGEDIN_MEMBERS(IGuildManagementService guildManagementService)
        {
            this.guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var guildId = swap16(BitConverter.ToUInt16(request.Data, 0));
            return Task.FromResult<IEnumerable<ResponseContent>>(new[] { request.CreateResponse(0x789d, guildManagementService.GetGuildInfo(guildId)) });
        }
    }
}
