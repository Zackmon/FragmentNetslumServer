using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_ACCEPT_GUILD_INVITE)]
    public sealed class OPCODE_DATA_ACCEPT_GUILD_INVITE : IOpCodeHandler
    {
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var ms = new MemoryStream();
            await ms.WriteAsync(new byte[] { 0x76, 0x0B});
            
            if (request.Data[1] == 0x08) //accepted the invitation
            {
                DBAccess.getInstance().EnrollPlayerInGuild(request.Client.currentGuildInvitaionSelection, request.Client._characterPlayerID, false);
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
