using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;

namespace FragmentNetslumServer.Services
{
    public sealed class LobbyChatService : ILobbyChatService
    {

        private readonly ConcurrentDictionary<int, LobbyChatRoom> lobbies;
        private readonly ILogger logger;
        private LobbyChatRoom mainLobby;

        public ReadOnlyDictionary<int, LobbyChatRoom> Lobbies => new ReadOnlyDictionary<int, LobbyChatRoom>(lobbies);

        public LobbyChatRoom Main => mainLobby;

        public string ServiceName => "Lobby Service";

        public ServiceStatusEnum ServiceStatus { get; private set; } = ServiceStatusEnum.Inactive;

        public LobbyChatService(ILogger logger)
        {
            this.logger = logger;
            this.lobbies = new ConcurrentDictionary<int, LobbyChatRoom>();
            this.ServiceStatus = ServiceStatusEnum.Active;
            this.mainLobby = GetOrAddLobby(1, "Main Lobby", OpCodes.LOBBY_TYPE_MAIN, out var _);
        }


        public LobbyChatRoom GetOrAddLobby(ushort lobbyId, string lobbyName, ushort lobbyType, out bool isCreated)
        {
            var localCreated = false;
            this.logger.Debug("Looking for a Lobby of ID {@lobbyId}", lobbyId);
            var lobby = lobbies.GetOrAdd(lobbyId, _ =>
            {
                localCreated = true;
                this.logger.Debug("Lobby {@lobbyId} does not exist. Creating a new Lobby named '{@lobbyName}'", lobbyId, lobbyName);
                return new LobbyChatRoom(lobbyName, lobbyId, lobbyType);
            });
            isCreated = localCreated;
            return lobby;
        }

        public bool TryGetLobby(ushort lobbyId, out LobbyChatRoom lobbyChatRoom)
        {
            this.logger.Debug("Looking for a Lobby of ID {@lobbyId}", lobbyId);
            lobbyChatRoom = null;
            if (lobbies.ContainsKey(lobbyId))
            {
                lobbyChatRoom = lobbies[lobbyId];
                this.logger.Debug("Found Lobby ID {@lobbyId}", lobbyId);
                return true;
            }
            this.logger.Debug("Could not find {@lobbyId}", lobbyId);
            return false;
        }

        public async Task AnnounceRoomDeparture(LobbyChatRoom lobbyChatRoom, uint clientIndex)
        {
            logger.Debug("Client #{@clientIndex} is leaving their lobby", clientIndex);
            await lobbyChatRoom.ClientLeavingRoomAsync((int)clientIndex);
            logger.Debug($"Lobby '{lobbyChatRoom.Name}' now has {lobbyChatRoom.Clients.Count:N0} Users");
            await Task.Yield();
        }

        public async Task AnnounceRoomDeparture(ushort lobbyId, uint clientIndex)
        {
            if (TryGetLobby(lobbyId, out var lobby))
            {
                await AnnounceRoomDeparture(lobby, clientIndex);
            }
        }

        public bool TryFindLobby(uint clientIndex, out LobbyChatRoom lobbyChatRoom)
        {
            lobbyChatRoom = null;
            foreach(var lobby in Lobbies)
            {
                var lobbyId = lobby.Key;
                var lcr = lobby.Value;
                if (lcr.Clients.Any(c => c.ClientIndex == clientIndex))
                {
                    lobbyChatRoom = lcr;
                    return true;
                }
            }
            return false;
        }

        public bool TryFindLobby(GameClientAsync gameClientAsync, out LobbyChatRoom lobbyChatRoom)
        {
            return TryFindLobby((uint)gameClientAsync.ClientIndex, out lobbyChatRoom);
        }
    }

}
