using Serilog;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using FragmentServerWV.Services;

namespace FragmentServerWV.Entities
{

    /// <summary>
    /// A packet class that's responsible for reading an incoming data feed
    /// </summary>
    public sealed class PacketAsync
    {
        private readonly ILogger logger;
        private readonly NetworkStream networkStream;
        private readonly Crypto crypto;

        private ushort datalen;
        private ushort code;
        private ushort checksum_inpacket;
        private ushort checksum_ofpacket;
        private byte[] data;
        private byte[] encryptedData;



        /// <summary>
        /// Gets the length of the packet data
        /// </summary>
        public ushort Length => datalen;

        /// <summary>
        /// Gets the OPCODE of the packet
        /// </summary>
        public ushort Code => code;

        /// <summary>
        /// Gets the decrypted byte array of data
        /// </summary>
        public byte[] Data => data;

        /// <summary>
        /// Gets the original encrypted byte array of data
        /// </summary>
        public byte[] EncryptedData => encryptedData;


        /// <summary>
        /// Gets the checksum of the packet as described by the packet
        /// </summary>
        public ushort ChecksumInPacket => checksum_inpacket;

        /// <summary>
        /// Gets the checksum of the packet computed server-side
        /// </summary>
        public ushort ChecksumOfPacket => checksum_ofpacket;


        /// <summary>
        /// Creates a new Packet reading class
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="networkStream"><see cref="NetworkStream"/></param>
        /// <param name="crypto"><see cref="Crypto"/></param>
        public PacketAsync(
            ILogger logger,
            NetworkStream networkStream,
            Crypto crypto)
        {
            this.logger = logger;
            this.networkStream = networkStream;
            this.crypto = crypto;
        }

        /// <summary>
        /// Reads the packet data asynchronously
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        public async Task<bool> ReadPacketAsync()
        {
            logger.Verbose("Asking to read network packet...");
            try
            {
                byte[] buff = new byte[2];
                datalen = 0;
                if (!networkStream.DataAvailable)
                {
                    logger.Verbose("No network packet available");
                    return false;
                }
                int read = await networkStream.ReadAsync(buff, 0, 2);
                if (read == 0)
                {
                    logger.Warning("Somehow read no data? There was supposed to be data!");
                    return false;
                }
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

                if (checksum_inpacket != checksum_ofpacket)
                {
                    logger.Warning("This packet and its checksum do NOT match! This could be important to investigate");
                }

                return true;
            }
            catch
            {
                logger.Warning("An error occurred attempting to read data from the client. It may not be important, but, still...");
                return false;
            }
            finally
            {
                logger.LogData(Data, Code, -1, "", ChecksumInPacket, ChecksumOfPacket);
                logger.Verbose("Completed reading network packet...");
            }
        }

    }
}
