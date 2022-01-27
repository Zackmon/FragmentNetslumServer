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
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_TAKE_GP)]
    public sealed class OPCODE_DATA_GUILD_TAKE_GP : IOpCodeHandler
    {
        private readonly ILogger _logger;
        private readonly IGuildManagementService _guildManagementService;

        public OPCODE_DATA_GUILD_TAKE_GP(ILogger logger,IGuildManagementService guildManagementService)
        {
            _logger = logger;
            _guildManagementService = guildManagementService;
            
        }
        
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            ushort guildIDTakeMoney = BitConverter.ToUInt16(request.Data, 0).Swap();
            uint amountOfMoneyToTakeOut = BitConverter.ToUInt32(request.Data, 2).Swap();
            
            _logger.Debug("Guild ID {guildIDTakeMoney} \nAmount of money to Take out {amountOfMoneyToTakeOut}",guildIDTakeMoney,amountOfMoneyToTakeOut);
            return Task.FromResult<IEnumerable<ResponseContent>>(new[]
            {
                request.CreateResponse(0x770F,
                    _guildManagementService.TakeMoneyFromGuild(guildIDTakeMoney, amountOfMoneyToTakeOut))
            });
        }
    }
}
