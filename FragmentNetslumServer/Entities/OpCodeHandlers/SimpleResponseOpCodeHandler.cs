using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers
{
    /// <summary>
    /// A base class <see cref="IOpCodeHandler"/> that supports a single response of a predictable result to the client
    /// </summary>
    /// <remarks>
    /// This is only useful when the response is deterministic. Thankfully, there are several OPCODES where the response is indeed deterministic
    /// </remarks>
    public abstract class SimpleResponseOpCodeHandler : IOpCodeHandler
    {
        private readonly ushort responseOpCode;
        private readonly byte[] responseData;

        public SimpleResponseOpCodeHandler(ushort responseOpCode, byte[] responseData)
        {
            this.responseOpCode = responseOpCode;
            this.responseData = responseData;
        }

        public virtual Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request) =>
            Task.FromResult<IEnumerable<ResponseContent>>(new[] { request.CreateResponse(responseOpCode, responseData) });
    }
}
