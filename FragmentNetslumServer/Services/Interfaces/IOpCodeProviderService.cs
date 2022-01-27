using FragmentNetslumServer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Services.Interfaces
{

    /// <summary>
    /// A service provider that's responsible for the discovery and management of the various <see cref="IOpCodeHandler"/> instances
    /// </summary>
    public interface IOpCodeProviderService : IBaseService
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
        Task<IEnumerable<ResponseContent>> HandlePacketAsync(GameClientAsync gameClient, PacketAsync packet);

        /// <summary>
        /// Determines whether or not <see cref="IOpCodeProviderService"/> can currently handle this <see cref="PacketAsync"/>
        /// </summary>
        /// <param name="packet"><see cref="PacketAsync"/></param>
        /// <returns>True if possible</returns>
        bool CanHandleRequest(PacketAsync packet);

    }

}
