using FragmentServerWV.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentServerWV.Services.Interfaces
{

    /// <summary>
    /// A service provider that's responsible for the discovery and management of the various <see cref="IOpCodeHandler"/> instances
    /// </summary>
    public interface IOpCodeProviderService
    {

        /// <summary>
        /// Gets a collection of <see cref="IOpCodeHandler"/>s but as their defined type
        /// </summary>
        IReadOnlyCollection<Type> Handlers { get; }

        /// <summary>
        /// Handles processing the incoming <see cref="PacketAsync"/>
        /// </summary>
        /// <param name="gameClient">The <see cref="GameClientAsync"/> that submitted the request</param>
        /// <param name="packet">The <see cref="PacketAsync"/> to handle</param>
        Task<ResponseContent> HandlePacketAsync(GameClientAsync gameClient, PacketAsync packet);

    }

}
