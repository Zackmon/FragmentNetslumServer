using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities
{

    /// <summary>
    /// A packet class that's responsible for reading an incoming data feed
    /// </summary>
    public sealed class Packet2
    {
        private readonly NetworkStream networkStream;
        private readonly Crypto crypto;

        private ushort datalen;
        private ushort code;
        private ushort checksum_inpacket;
        private ushort checksum_ofpacket;
        private byte[] data;
        private byte[] encryptedData;


        /// <summary>
        /// Creates a new Packet reading class
        /// </summary>
        /// <param name="networkStream"><see cref="NetworkStream"/></param>
        /// <param name="crypto"><see cref="Crypto"/></param>
        public Packet2(
            NetworkStream networkStream,
            Crypto crypto)
        {
            this.networkStream = networkStream;
            this.crypto = crypto;
        }



        public async Task<bool> ReadPacketAsync()
        {
            byte[] buff = new byte[2];
            datalen = 0;
            if (!networkStream.DataAvailable) return false;
            try
            {
                int read = await networkStream.ReadAsync(buff, 0, 2);
                if (read == 0) return false;
                datalen = (ushort)((buff[0] << 8) + buff[1]);
                data = new byte[datalen];
                encryptedData = new byte[datalen];
                await networkStream.ReadAsync(data, 0, datalen);
                if (datalen > 1)
                {
                    code = (ushort)((data[0] << 8) + data[1]);
                    if (datalen > 9)
                    {
                        MemoryStream m = new MemoryStream();
                        datalen -= 2;
                        await m.WriteAsync(data, 2, datalen);
                        encryptedData = m.ToArray();
                        data = crypto.Decrypt(m.ToArray());
                        checksum_inpacket = (ushort)((data[0] << 8) + data[1]);
                        m = new MemoryStream();
                        await m.WriteAsync(data, 2, datalen - 2);
                        checksum_ofpacket = Crypto.Checksum(m.ToArray());
                    }
                }
                else
                {
                    code = 0;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
