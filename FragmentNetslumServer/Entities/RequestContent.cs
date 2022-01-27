using System.IO;
using System;
using static FragmentServerWV.Services.Extensions;

namespace FragmentServerWV.Entities
{

    /// <summary>
    /// Defines a particular request that was sent via a connected client
    /// </summary>
    public sealed class RequestContent
    {

        /// <summary>
        /// Gets the <see cref="GameClientAsync"/> that has submitted this request
        /// </summary>
        public GameClientAsync Client { get; private set; }

        /// <summary>
        /// Gets the OpCode that was submitted by the <see cref="Client"/>
        /// </summary>
        public ushort OpCode { get; private set; }

        /// <summary>
        /// Gets the optional Data OpCode that was submitted by the <see cref="Client"/>
        /// </summary>
        /// <remarks>
        /// If <see cref="OpCode"/> does not equal <see cref="OpCodes.OPCODE_DATA"/> (or 0x30) then this will be NULL
        /// </remarks>
        public ushort? DataOpCode { get; private set; }

        /// <summary>
        /// Gets the data payload that was submitted by the <see cref="Client"/>
        /// </summary>
        public byte[] Data { get; private set; }



        /// <summary>
        /// Creates a new <see cref="RequestContent"/> instance for the given client and packet
        /// </summary>
        /// <param name="gameClient">The <see cref="GameClientAsync"/> that has received a packet of information</param>
        /// <param name="packet">The <see cref="PacketAsync"/> of data that was received</param>
        public RequestContent(
            GameClientAsync gameClient,
            PacketAsync packet)
        {
            this.Client = gameClient;
            this.OpCode = packet.Code;
            this.Data = packet.Data;

            if (this.OpCode == OpCodes.OPCODE_DATA)
            {

                // For the DATA opcode, we have some special handling
                // that needs to occur now. This additional logic is
                // responsible for the "data opcode" and also properly
                // decoding the data payload
                var data = packet.Data;
                if (Client != null) // Client can be NULL if we're just asking 'can we handle this'
                {
                    Client.SetClientSequenceNumber(swap16(BitConverter.ToUInt16(data, 2)));
                }
                var arglen = (ushort)(swap16(BitConverter.ToUInt16(data, 6)) - 2);
                var code = swap16(BitConverter.ToUInt16(data, 8));
                var m = new MemoryStream();
                m.Write(data, 10, arglen);

                this.DataOpCode = code;
                this.Data = m.ToArray();
            }

        }

        /// <summary>
        /// A shortcut method to create a response
        /// </summary>
        /// <param name="responseOpCode">The OpCode to respond with</param>
        /// <param name="responseData">The data to transmit</param>
        /// <param name="checksum">An optional checksum of the data</param>
        /// <returns><see cref="ResponseContent"/></returns>
        public ResponseContent CreateResponse(
            ushort responseOpCode,
            byte[] responseData,
            uint? checksum = null)
        {
            return new ResponseContent(this, responseOpCode, responseData, checksum);
        }

    }

}
