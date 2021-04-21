using FragmentServerWV.Entities;
using System.Threading.Tasks;

namespace FragmentServerWV.Services.Interfaces
{

    /// <summary>
    /// Defines a lightweight interface who is responsible for handling incoming packet requests
    /// </summary>
    public interface IOpCodeHandler
    {

        /// <summary>
        /// Handles the incoming <see cref="PacketAsync"/> instance
        /// </summary>
        /// <param name="request">The incoming <see cref="RequestContent"/></param>
        /// <returns>A promise to handle the packet asynchronously</returns>
        Task<ResponseContent> HandleIncomingRequestAsync(RequestContent request);

    }

}
