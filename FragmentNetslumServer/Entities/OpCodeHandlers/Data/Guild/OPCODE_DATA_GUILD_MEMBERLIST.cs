using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_MEMBERLIST)]
    public sealed class OPCODE_DATA_GUILD_MEMBERLIST : IOpCodeHandler
    {
        private readonly IGuildManagementService guildManagementService;

        public OPCODE_DATA_GUILD_MEMBERLIST(IGuildManagementService guildManagementService)
        {
            this.guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var responses = new List<ResponseContent>();
            var u = swap16(BitConverter.ToUInt16(request.Data, 0));
            if (u == 0)// Guild Member Category List
            {
                List<byte[]> listOfClasses = GetClassList();
                responses.Add(request.CreateResponse(0x7611, BitConverter.GetBytes(swap16((ushort)listOfClasses.Count))));
                foreach (var className in listOfClasses)
                {
                    responses.Add(request.CreateResponse(0x7613, className));
                }

            }
            else //MemberList in that Category
            {
                List<byte[]> memberList = guildManagementService.GetGuildMembersListByClass(request.Client._guildID, u, request.Client._characterPlayerID);
                responses.Add(request.CreateResponse(0x7614, BitConverter.GetBytes(swap16((ushort)memberList.Count))));
                foreach (var member in memberList)
                {
                    responses.Add(request.CreateResponse(0x7615, member));
                }
            }
            return Task.FromResult<IEnumerable<ResponseContent>>(responses);
        }
    }
}
