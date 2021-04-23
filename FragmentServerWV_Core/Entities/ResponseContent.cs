using static FragmentServerWV.Services.Extensions;
using System;
using System.IO;

namespace FragmentServerWV.Entities
{

    /// <summary>
    /// Defines the response object for a particular request
    /// </summary>
    public sealed class ResponseContent
    {

        private static readonly ResponseContent empty = new ResponseContent(null, 0, new byte[0], null);

        /// <summary>
        /// Gets an empty <see cref="ResponseContent"/>
        /// </summary>
        public static ResponseContent Empty => empty;

        /// <summary>
        /// Gets the <see cref="RequestContent"/> that generated this <see cref="ResponseContent"/>
        /// </summary>
        public RequestContent Request { get; }

        /// <summary>
        /// Gets the responding OpCode
        /// </summary>
        public ushort OpCode { get; private set; }

        /// <summary>
        /// Gets the responding byte array of content
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the responding checksum
        /// </summary>
        /// <remarks>
        /// If Checksum is NULL, the response type is ASSUMED to be a Data Packet response (meaning the <see cref="RequestContent"/> OpCode was <see cref="OpCodes.OPCODE_DATA"/>)
        /// </remarks>
        public uint? Checksum { get; private set; }



        /// <summary>
        /// Creates a new <see cref="ResponseContent"/> object
        /// </summary>
        /// <param name="request">The <see cref="RequestContent"/> that spawned this response</param>
        /// <param name="responseOpCode">The replying opcode</param>
        /// <param name="responseData">The data to submit back</param>
        /// <param name="checksum">An optionally provided checksum</param>
        public ResponseContent(
            RequestContent request,
            ushort responseOpCode,
            byte[] responseData,
            uint? checksum)
        {
            this.Request = request;
            this.OpCode = responseOpCode;
            this.Data = responseData;
            this.Checksum = checksum;
            if (request is null)
            {
                return;
            }
            if (checksum is null)
            {
                this.CreateDataPacketResponse();
            }
            else
            {
                this.CreatePacketResponse();
            }
        }





        /// <summary>
        /// Formats the packet of data correctly so that it can be written directly to the client networkstream
        /// </summary>
        /// <param name="code">Optionally provide the opcode for the payload</param>
        /// <param name="data">Optionally provide the data payload</param>
        /// <param name="checksum">Optionally provide the checksum</param>
        /// <remarks>
        /// If no parameters are provided then <see cref="OpCode"/>, <see cref="Data"/>, and <see cref="Checksum"/> will be used instead.
        /// These are allowed to be NULL simply because <see cref="CreateDataPacketResponse(ushort?, byte[])"/> has a dependency on this method as well
        /// </remarks>
        internal void CreatePacketResponse(ushort? code = null, byte[] data = null, uint? checksum = null)
        {
            code ??= this.OpCode;
            data ??= this.Data;
            checksum ??= this.Checksum;

            var to_crypto = new Crypto(Request.Client.to_key);
            var responseStream = new MemoryStream();
            responseStream.WriteByte((byte)(checksum >> 8));
            responseStream.WriteByte((byte)(checksum & 0xFF));
            responseStream.Write(data, 0, data.Length);
            var buff = responseStream.ToArray();
            buff = to_crypto.Encrypt(buff);
            var len = (ushort)(buff.Length + 2);
            responseStream = new MemoryStream();
            responseStream.WriteByte((byte)(len >> 8));
            responseStream.WriteByte((byte)(len & 0xFF));
            responseStream.WriteByte((byte)(code >> 8));
            responseStream.WriteByte((byte)(code & 0xFF));
            responseStream.Write(buff, 0, buff.Length);
            this.Data = responseStream.ToArray();
        }

        /// <summary>
        /// Formats the packet of data correctly so that it can be written directly to the client networkstream
        /// </summary>
        /// <param name="code">Optionally provide the opcode for the payload</param>
        /// <param name="data">Optionally provide the data payload</param>
        /// <remarks>
        /// If no parameters are provided then <see cref="OpCode"/> and <see cref="Data"/> will be used instead.
        /// This method also has a rather NASTY side effect in that <see cref="Request.Client.server_seq_nr"/> is incremented as a result of running this method
        /// </remarks>
        internal void CreateDataPacketResponse(ushort? code = null, byte[] data = null)
        {
            code ??= this.OpCode;
            data ??= this.Data;

            var responseStream = new MemoryStream();
            responseStream.Write(BitConverter.GetBytes(swap32(Request.Client.server_seq_nr)), 0, 4);
            var len = (ushort)(data.Length + 2);
            responseStream.Write(BitConverter.GetBytes(swap16(len)), 0, 2);
            responseStream.Write(BitConverter.GetBytes(swap16(code)), 0, 2);
            responseStream.Write(data, 0, data.Length);
            var checksum = Crypto.Checksum(responseStream.ToArray());
            while (((responseStream.Length + 2) & 7) != 0) responseStream.WriteByte(0);
            this.CreatePacketResponse(OpCodes.OPCODE_DATA, responseStream.ToArray(), checksum);
        }

    }

}
