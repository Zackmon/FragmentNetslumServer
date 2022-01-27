using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services.Interfaces;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1),
        OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2)]
    class OPCODE_DATA_AS_PUBLISH_DETAILS : IOpCodeHandler
    {
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            switch(request.DataOpCode)
            {
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1:
                    MemoryStream m;
                    int end = request.Data.Length - 1;
                    while (request.Data[end] == 0) end--;
                    end++;
                    m = new MemoryStream();
                    await m.WriteAsync(request.Data, 65, end - 65);
                    request.Client.publish_data_1 = m.ToArray();
                    return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1_OK, new byte[] { 0x00, 0x01 }) };
                case OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2:
                    return new[] { request.CreateResponse(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2_OK, new byte[] { 0xDE, 0xAD }) };
            }
            throw new ArgumentOutOfRangeException(nameof(request.DataOpCode), $"{request.DataOpCode:X2} is outside the expected values ({OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS1:X2} & {OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS2:X2})");
        }
    }
}
