using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace FragmentServerWV
{
    public class Packet
    {
        public ushort datalen;
        public ushort code;
        public ushort checksum_inpacket;
        public ushort checksum_ofpacket;
        public byte[] data;

        public Packet(NetworkStream ns, Crypto crypto)
        {
            byte[] buff = new byte[2];
            datalen = 0;
            if (!ns.DataAvailable)
                return;
            try
            {
                int read = ns.Read(buff, 0, 2);
                if (read == 0)
                    return;
                datalen = (ushort)((buff[0] << 8) + buff[1]);
                data = new byte[datalen];
                ns.Read(data, 0, datalen);
                if (datalen > 1)
                {
                    code = (ushort)((data[0] << 8) + data[1]);
                    if (datalen > 9)
                    {
                        MemoryStream m = new MemoryStream();
                        datalen -= 2;
                        m.Write(data, 2, datalen);
                        data = crypto.Decrypt(m.ToArray());
                        checksum_inpacket = (ushort)((data[0] << 8) + data[1]);
                        m = new MemoryStream();
                        m.Write(data, 2, datalen - 2);
                        checksum_ofpacket = Crypto.Checksum(m.ToArray());
                    }
                }
                else
                    code = 0;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
