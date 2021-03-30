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
        /// Gets a collection of currently connected <see cref="GameClient"/> instances
        /// </summary>
        ReadOnlyCollection<GameClientAsync> Clients { get; }


        /// <summary>
        /// Adds a new client to the <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client">The newly connected <see cref="TcpClient"/></param>
        /// <param name="clientId">An identifier for the client</param>
        void AddClient(TcpClient client, uint clientId);

        /// <summary>
        /// Adds a new client to the <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client">The newly created <see cref="GameClient"/></param>
        void AddClient(GameClientAsync client);


        /// <summary>
        /// Removes an existing client from <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="clientId">The identifier for the client</param>
        void RemoveClient(uint clientId);

        /// <summary>
        /// Removes an existing client from <see cref="IClientProviderService"/>
        /// </summary>
        /// <param name="client"><see cref="GameClient"/> to remove</param>
        void RemoveClient(GameClientAsync client);

    }

}