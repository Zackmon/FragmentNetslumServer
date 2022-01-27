using FragmentServerWV.Entities.Attributes;
using System.ComponentModel;

namespace FragmentServerWV.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_PING),
        OpCodeData(OpCodes.OPCODE_DATA_LOBBY_FAVORITES_AS_INQUIRY),
        OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS3),
        OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS4),
        OpCodeData(OpCodes.OPCODE_DATA_AS_PUBLISH_DETAILS6),
        Description("Provides no response to DATA OpCodes")]
    public sealed class OPCODE_DATA_NORESPONSE : NoResponseOpCodeHandler { }
}
