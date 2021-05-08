using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_ACCEPT_GUILD_INVITE)]
    public sealed class OPCODE_DATA_ACCEPT_GUILD_INVITE : IOpCodeHandler
    {
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var ms = new MemoryStream();
            await ms.WriteAsync(new byte[] { 0x76, 0xB0, 0x54, 0x45, 0x53, 0x54, 0x00 });
            if (request.Data[1] == 0x08) //accepted the invitation
            {
                DBAcess.getInstance().EnrollPlayerInGuild(request.Client.currentGuildInvitaionSelection, request.Client._characterPlayerID, false);
                responses.Add(request.CreateResponse(0x760A, ms.ToArray())); // send guild ID
            }
            else
            {
                // rejected
                responses.Add(request.CreateResponse(0x760A, ms.ToArray())); // send guild ID
            }
            return responses;
        }
    }
}
