using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities;

namespace FragmentNetslumServer.Services.Interfaces
{

    /// <summary>
    /// Defines how the lobby service system should operate
    /// </summary>
    public interface ILobbyChatService: IBaseService
    {

        /// <summary>
        /// Gets a collection of currently created Lobbies
        /// </summary>
        ReadOnlyDictionary<int, LobbyChatRoom> Lobbies { get; }

        /// <summary>
        /// Gets the main lobby room
        /// </summary>
        LobbyChatRoom Main { get; }

        /// <summary>
        /// Returns an instance of <see cref="LobbyChatRoom"/>
        /// </summary>
        /// <param name="lobbyId">The unique Lobby ID</param>
        /// <param name="lobbyName">The name of the lobby to create</param>
        /// <param name="lobbyType">The type of lobby to create</param>
        /// <param name="isCreated">Indicates if the supplied <see cref="LobbyChatRoom"/> already existed</param>
        /// <returns><see cref="LobbyChatRoom"/></returns>
        /// <remarks>For more information on the correct TYPE to pass in, take a look in <see cref="OpCodes"/></remarks>
        LobbyChatRoom GetOrAddLobby(ushort lobbyId, string lobbyName, ushort lobbyType, out bool isCreated);

        /// <summary>
        /// Attempts to retrieve a <see cref="LobbyChatRoom"/>
        /// </summary>
        /// <param name="lobbyId">The unique Lobby ID</param>
        /// <param name="lobbyChatRoom">The <see cref="LobbyChatRoom"/> found, if applicable</param>
        /// <returns><see cref="LobbyChatRoom"/></returns>
        bool TryGetLobby(ushort lobbyId, out LobbyChatRoom lobbyChatRoom);

        /// <summary>
        /// Announces that a client has left a particular <see cref="LobbyChatRoom"/>
        /// </summary>
        /// <param name="lobbyChatRoom"><see cref="LobbyChatRoom"/></param>
        /// <param name="clientIndex">The identifier of the client</param>
        /// <returns>A Task that intends to complete the departure announcement</returns>
        Task AnnounceRoomDeparture(LobbyChatRoom lobbyChatRoom, uint clientIndex);

        /// <summary>
        /// Announces that a client has left a particular <see cref="LobbyChatRoom"/>
        /// </summary>
        /// <param name="lobbyId">The lobby identifier</param>
        /// <param name="clientIndex">The identifier of the client</param>
        /// <returns>A Task that intends to complete the departure announcement</returns>
        Task AnnounceRoomDeparture(ushort lobbyId, uint clientIndex);

        /// <summary>
        /// Attempts to locate the lobby where the client is currently at
        /// </summary>
        /// <param name="clientIndex">The index of the client</param>
        /// <param name="lobbyChatRoom">The discovered <see cref="LobbyChatRoom"/></param>
        /// <returns>True or false depending on whether or not the lobby was found</returns>
        bool TryFindLobby(uint clientIndex, out LobbyChatRoom lobbyChatRoom);

        /// <summary>
        /// Attempts to locate the lobby where the client is currently at
        /// </summary>
        /// <param name="gameClientAsync">The game client reference</param>
        /// <param name="lobbyChatRoom">The discovered <see cref="LobbyChatRoom"/></param>
        /// <returns>True or false depending on whether or not the lobby was found</returns>
        bool TryFindLobby(GameClientAsync gameClientAsync, out LobbyChatRoom lobbyChatRoom);

    }

}