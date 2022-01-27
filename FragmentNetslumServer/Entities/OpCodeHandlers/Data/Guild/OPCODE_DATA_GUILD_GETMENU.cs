using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    /// <remarks>
    /// Looks quite simliar to <see cref="OPCODE_DATA_GUILD_GET_ALL_GUILDS"/> but uses different opcodes...
    /// </remarks>
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_GETMENU)]
    public sealed class OPCODE_DATA_GUILD_GETMENU : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var u = swap16(BitConverter.ToUInt16(request.Data, 0));
            // This is very similar to the area server handler
            // If this is zero, show the guild category breakdown (basically)
            // otherwise, show them all the guilds
            if (u == 0)
            {
                // Tell them we have ONE guild listing
                responses.Add(request.CreateResponse(0x7734, new byte[] { 0x00, 0x01 }));

                // This should print "ALL" on the game
                responses.Add(request.CreateResponse(0x7736, new byte[] { 0x00, 0x01, 0x41, 0x6c, 0x6c, 0x00 }));
            }
            else
            {
                var listOfGuilds = GuildManagementService.GetInstance().GetListOfGuilds();
                responses.Add(request.CreateResponse(0x7737, BitConverter.GetBytes(swap16((ushort)listOfGuilds.Count))));
                foreach (var guildName in listOfGuilds)
                {
                    responses.Add(request.CreateResponse(0x7738, guildName));
                }
            }
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
