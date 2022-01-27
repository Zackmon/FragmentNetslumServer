using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_CREATE)]
    public sealed class OPCODE_DATA_GUILD_CREATE : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var u = GuildManagementService.GetInstance().CreateGuild(request.Data, request.Client._characterPlayerID);
            request.Client._guildID = u;
            request.Client.isGuildMaster = true;
            return Task.FromResult<IEnumerable<ResponseContent>>(new[] { request.CreateResponse(0x7601, BitConverter.GetBytes(swap16(u))) });
        }
    }
}
