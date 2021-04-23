using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers
{
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
