using FragmentServerWV.Entities;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace FragmentServerWV.Services.Interfaces
{
    /// <summary>
    /// Defines a service that's responsible for managing connected client
    /// </summary>
    public interface IClientProviderService: IBaseService
    {

        /// <summary>
        /// Gets a collection of currently connected <see cref="GameClientAsync"/> instances
        /// </summary>
        ReadOnlyCollection<GameClientAsync> Clients { get; }

        /// <summary>
        /// Gets a collection of currently connected <see cref="GameClientAsync"/> that have declared themselves as Area Servers
        /// </summary>
        ReadOnlyCollection<GameClientAsync> AreaServers { get; }



        /// <summary>
        /// Adds a new client to the <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client">The newly connected <see cref="TcpClient"/></param>
        /// <param name="clientId">An identifier for the client</param>
        void AddClient(TcpClient client, uint clientId);

        /// <summary>
        /// Adds a new client to the <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client">The newly created <see cref="GameClientAsync"/></param>
        void AddClient(GameClientAsync client);


        /// <summary>
        /// Removes an existing client from <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="clientId">The identifier for the client</param>
        void RemoveClient(uint clientId);

        /// <summary>
        /// Removes an existing client from <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client"><see cref="GameClientAsync"/> to remove</param>
        void RemoveClient(GameClientAsync client);

        /// <summary>
        /// Attempts to retrieve a <see cref="GameClientAsync"/> based on its index
        /// </summary>
        /// <param name="index">The expected client index</param>
        /// <param name="client">The <see cref="GameClientAsync"/></param>
        /// <returns>A boolean value to indicate success or failure</returns>
        bool TryGetClient(uint index, out GameClientAsync client);
    }

}