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

            if (this.OpCode == OpCodes.OPCODE_DATA)
            {

            }
            else
            {

            }

        }

    }

}
