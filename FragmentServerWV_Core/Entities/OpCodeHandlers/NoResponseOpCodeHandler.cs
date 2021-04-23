using FragmentServerWV.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers
{
    public abstract class NoResponseOpCodeHandler : IOpCodeHandler
    {
        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
            => Task.FromResult<IEnumerable<ResponseContent>>(new[] { ResponseContent.Empty });
    }
}
