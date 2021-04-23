using FragmentServerWV.Services.Interfaces;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers
{
    public abstract class NoResponseOpCodeHandler : IOpCodeHandler
    {
        public Task<ResponseContent> HandleIncomingRequestAsync(RequestContent request) => Task.FromResult(ResponseContent.Empty);
    }
}
