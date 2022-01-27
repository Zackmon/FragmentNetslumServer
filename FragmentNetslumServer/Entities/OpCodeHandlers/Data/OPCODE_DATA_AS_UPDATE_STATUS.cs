using FragmentNetslumServer.Entities.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_UPDATE_STATUS)]
    public sealed class OPCODE_DATA_AS_UPDATE_STATUS : NoResponseOpCodeHandler
    {
        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            request.Client.publish_data_2 = request.Data;
            ExtractAreaServerData(request);
            return base.HandleIncomingRequestAsync(request);
        }

        private void ExtractAreaServerData(RequestContent request)
        {
            var data = request.Data;
            var client = request.Client;
            int pos = 67; // isn't this interesting
            client.areaServerName = ReadByteString(data, pos);
            pos += client.areaServerName.Length;
            client.areaServerLevel = swap16(BitConverter.ToUInt16(data, pos));
            pos += 4;
            client.areaServerStatus = data[pos++];
        }
    }
}
