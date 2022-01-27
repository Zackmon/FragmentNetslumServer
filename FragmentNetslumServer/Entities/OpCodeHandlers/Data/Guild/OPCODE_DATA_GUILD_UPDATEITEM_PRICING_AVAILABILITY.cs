using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Services;
using Serilog;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY)]
    public sealed class OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY : IOpCodeHandler
    {
        private readonly ILogger _logger;
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_UPDATEITEM_PRICING_AVAILABILITY(ILogger logger,IGuildManagementService guildManagementService)
        {
            _logger = logger;
            _guildManagementService = guildManagementService;
            
        }
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            uint generalPrice = BitConverter.ToUInt32(request.Data, 0).Swap();
            uint memberPrice = BitConverter.ToUInt32(request.Data, 4).Swap();
            bool isGeneral = BitConverter.ToBoolean(request.Data, 8);
            bool isMember = BitConverter.ToBoolean(request.Data, 9);

            _logger.Debug("GenePrice {GeneralPrice}\nMemberPrice {MemberPrice}\nisGeneral {isGeneral}\nisMember {isMember}",generalPrice,memberPrice,isGeneral,isMember);
            
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x7705,
                    _guildManagementService.AddItemToGuildInventory(
                        request.Client._guildID,request.Client._itemDontationID,
                        request.Client._itemDonationQuantity,
                        generalPrice,memberPrice,isGeneral,isMember,request.Client.isGuildMaster))
            });
        }
    }
}
