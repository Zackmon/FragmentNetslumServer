using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_SAVEID), Description("Associates a GameClient with an Account ID and loads out the MOTD")]
    public sealed class OPCODE_DATA_SAVEID : IOpCodeHandler
    {
        public async Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            var encoding = Encoding.GetEncoding("Shift-JIS");
            MemoryStream m;
            byte[] saveID = ReadByteString(request.Data, 0);
            request.Client.save_id = saveID;
            m = new MemoryStream();
            request.Client.AccountId = DBAcess.getInstance().GetPlayerAccountId(encoding.GetString(saveID));
            uint swapped = swap32((uint)request.Client.AccountId);
            await m.WriteAsync(BitConverter.GetBytes(swapped), 0, 4);
            byte[] buff = encoding.GetBytes(DBAcess.getInstance().MessageOfTheDay);
            m.WriteByte((byte)(buff.Length - 1));
            await m.WriteAsync(buff, 0, buff.Length);
            while (m.Length < 0x200) m.WriteByte(0);
            byte[] response = m.ToArray();
            return new[] { request.CreateResponse(0x742A, response) };
        }
    }
}
