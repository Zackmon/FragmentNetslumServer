﻿using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data.Guild
{
    [OpCodeData(OpCodes.OPCODE_DATA_GUILD_UPDATEITEM_PRICING)]
    public sealed class OPCODE_DATA_GUILD_UPDATEITEM_PRICING : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            throw new NotImplementedException();
        }
    }
}