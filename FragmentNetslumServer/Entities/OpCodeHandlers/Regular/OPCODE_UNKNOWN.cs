using FragmentNetslumServer.Entities.Attributes;
using System.ComponentModel;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Regular
{
    [OpCode(0x0002), DisplayName("Unknown Handler"), Description("Handles an opcode that seemingly has no function")]
    public sealed class OPCODE_UNKNOWN : NoResponseOpCodeHandler { }
}
