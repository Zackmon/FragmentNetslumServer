using System;

namespace FragmentNetslumServer.Entities.Attributes
{

    /// <summary>
    /// Defines an OpCode that is explicitly intended for usage with the data OpCode (0x30)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class OpCodeDataAttribute : OpCodeAttribute
    {

        /// <summary>
        /// Gets the data-based OpCode
        /// </summary>
        public ushort DataOpCode { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="OpCodeDataAttribute"/>
        /// </summary>
        /// <param name="dataOpCode">The OpCode to use for mapping</param>
        public OpCodeDataAttribute(ushort dataOpCode) : base(OpCodes.OPCODE_DATA) => DataOpCode = dataOpCode;
    }
}
