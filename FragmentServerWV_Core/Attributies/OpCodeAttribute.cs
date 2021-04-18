using System;

namespace FragmentServerWV.Attributies
{
    public class OpCodeAttribute : Attribute
    {
        private ushort opcode;

        public OpCodeAttribute(ushort opcode)
        {
            this.opcode = opcode;
        }
    }
}