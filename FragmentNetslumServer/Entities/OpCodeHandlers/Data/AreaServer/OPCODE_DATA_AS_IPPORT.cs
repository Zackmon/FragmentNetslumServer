using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities.Attributes;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data.AreaServer
{
    [OpCodeData(OpCodes.OPCODE_DATA_AS_IPPORT), Description("Determines the external IP address for the Client. This does also attempt to handle rewriting 127.0.0.1 to a 'more correct' IP address")]
    public sealed class OPCODE_DATA_AS_IPPORT : SimpleResponseOpCodeHandler
    {
        public OPCODE_DATA_AS_IPPORT() : base(OpCodes.OPCODE_DATA_AS_IPPORT_OK, new byte[] { 0x00, 0x00 }) { }

        public override Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(request.Data);
                request.Client.ipdata = memoryStream.ToArray();
            }
            //request.Client.ipdata = request.Data;
            var externalIpAddress = request.Client.ipEndPoint.Address.ToString();
            if (externalIpAddress == Helpers.IPAddressHelpers.LOOPBACK_IP_ADDRESS)
            {
                externalIpAddress = Helpers.IPAddressHelpers.GetLocalIPAddress2();
            }
            var ipAddress = externalIpAddress.Split('.');
            // var ipAddressBytes = ipAddress.Reverse().Select(c => byte.Parse(c)).ToArray();
           
           request.Data[3] = byte.Parse(ipAddress[0]);
           request.Data[2] = byte.Parse(ipAddress[1]);
           request.Data[1] = byte.Parse(ipAddress[2]);
           request.Data[0] = byte.Parse(ipAddress[3]);
           
            request.Client.externalIPAddress = request.Data;
            return base.HandleIncomingRequestAsync(request);
        }
    }
}
