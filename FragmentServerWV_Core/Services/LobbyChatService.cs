using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FragmentServerWV.Services
{
    public sealed class LobbyChatService : Interfaces.ILobbyChatService
    {

        private readonly ConcurrentDictionary<int, LobbyChatRoom> lobbies;
        private readonly LobbyChatRoom mainLobby;
        private readonly ILogger logger;

        public ReadOnlyDictionary<int, LobbyChatRoom> Lobbies => new ReadOnlyDictionary<int, LobbyChatRoom>(Lobbies);

        public LobbyChatRoom Main => mainLobby;


        public LobbyChatService(ILogger logger)
        {
            this.logger = logger;
            this.lobbies = new ConcurrentDictionary<int, LobbyChatRoom>();
            this.mainLobby = GetOrAddLobby(1, "Main Lobby", OpCodes.LOBBY_TYPE_MAIN, out var _);
        }


        public LobbyChatRoom GetOrAddLobby(ushort lobbyId, string lobbyName, ushort lobbyType, out bool isCreated)
        {
            var localCreated = false;
            this.logger.Information("Looking for a Lobby of ID {@lobbyId}", lobbyId);
            var lobby = lobbies.GetOrAdd(lobbyId, _ =>
            {
                localCreated = true;
                this.logger.Information("Lobby {@lobbyId} does not exist. Creating a new Lobby named '{@lobbyName}'", lobbyId, lobbyName);
                return new LobbyChatRoom(lobbyName, lobbyId, lobbyType);
            });
            isCreated = localCreated;
            return lobby;
        }

        public bool TryGetLobby(ushort lobbyId, out LobbyChatRoom lobbyChatRoom)
        {
            lobbyChatRoom = null;
            if (lobbies.ContainsKey(lobbyId))
            {
                lobbyChatRoom = lobbies[lobbyId];
                return true;
            }
            return false;
        }
    }

}
