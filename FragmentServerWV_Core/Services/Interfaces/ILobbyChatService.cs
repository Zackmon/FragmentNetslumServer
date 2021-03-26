using System.Collections.ObjectModel;

namespace FragmentServerWV.Services.Interfaces
{

    /// <summary>
    /// Defines how the lobby service system should operate
    /// </summary>
    public interface ILobbyChatService
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

    }

}