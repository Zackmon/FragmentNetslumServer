using FragmentNetslumServer.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Entities.OpCodeHandlers
{
    /// <summary>
    /// A base class <see cref="IOpCodeHandler"/> that does not have an actual response to the client
    /// </summary>
    public abstract class NoResponseOpCodeHandler : IOpCodeHandler
    {
        public virtual Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
            => Task.FromResult<IEnumerable<ResponseContent>>(new[] { ResponseContent.Empty });
    }
}
