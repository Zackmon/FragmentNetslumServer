using System;

namespace FragmentServerWV.Entities.Attributes
{
    /// <summary>
    /// Defines a non-data based OpCode
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OpCodeAttribute : Attribute
    {

        /// <summary>
        /// Gets the OpCode this attribute is meant to represent
        /// </summary>
        public ushort OpCode { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="OpCodeAttribute"/>
        /// </summary>
        /// <param name="opcode">The OpCode to use for mapping</param>
        public OpCodeAttribute(ushort opcode) => OpCode = opcode;

    }
}
