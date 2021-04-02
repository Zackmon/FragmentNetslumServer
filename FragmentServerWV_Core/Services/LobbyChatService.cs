using FragmentServerWV.Enumerations;
using FragmentServerWV.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FragmentServerWV.Services
{
    public sealed class LobbyChatService : ILobbyChatService
    {

        private readonly ConcurrentDictionary<int, LobbyChatRoom> lobbies;
        private readonly LobbyChatRoom mainLobby;
        private readonly ILogger logger;
        private readonly IServiceProvider provider;

        public ReadOnlyDictionary<int, LobbyChatRoom> Lobbies => new ReadOnlyDictionary<int, LobbyChatRoom>(lobbies);

        public LobbyChatRoom Main => mainLobby;

        public string ServiceName => "Lobby Service";

        public ServiceStatusEnum ServiceStatus { get; private set; } = ServiceStatusEnum.Inactive;

        public LobbyChatService(ILogger logger, IServiceProvider provider)
        {
            this.logger = logger;
            this.provider = provider;
            this.lobbies = new ConcurrentDictionary<int, LobbyChatRoom>();
            this.mainLobby = GetOrAddLobby(1, "Main Lobby", OpCodes.LOBBY_TYPE_MAIN, out var _);
            this.ServiceStatus = ServiceStatusEnum.Active;
        }


        public LobbyChatRoom GetOrAddLobby(ushort lobbyId, string lobbyName, ushort lobbyType, out bool isCreated)
        {
            var localCreated = false;
            this.logger.Information("Looking for a Lobby of ID {@lobbyId}", lobbyId);
            var lobby = lobbies.GetOrAdd(lobbyId, _ =>
            {
                localCreated = true;
                this.logger.Information("Lobby {@lobbyId} does not exist. Creating a new Lobby named '{@lobbyName}'", lobbyId, lobbyName);
                return new LobbyChatRoom(lobbyName, lobbyId, lobbyType, provider.GetRequiredService<IClientProviderService>());
            });
            isCreated = localCreated;
            return lobby;
        }

        public bool TryGetLobby(ushort lobbyId, out LobbyChatRoom lobbyChatRoom)
        {
            this.logger.Information("Looking for a Lobby of ID {@lobbyId}", lobbyId);
            lobbyChatRoom = null;
            if (lobbies.ContainsKey(lobbyId))
            {
                lobbyChatRoom = lobbies[lobbyId];
                this.logger.Information("Found Lobby ID {@lobbyId}", lobbyId);
                return true;
            }
            this.logger.Information("Could not find {@lobbyId}", lobbyId);
            return false;
        }

        public async Task AnnounceRoomDeparture(LobbyChatRoom lobbyChatRoom, uint clientIndex)
        {
            logger.Information("Client #{@clientIndex} is leaving their lobby", clientIndex);
            await lobbyChatRoom.ClientLeavingRoomAsync((int)clientIndex);
            lobbyChatRoom.Users.Remove((int)clientIndex);
            logger.Information($"Lobby '{lobbyChatRoom.name}' now has {lobbyChatRoom.Users.Count:N0} Users");
            await Task.Yield();
        }

        public async Task AnnounceRoomDeparture(ushort lobbyId, uint clientIndex)
        {
            if (TryGetLobby(lobbyId, out var lobby))
            {
                await AnnounceRoomDeparture(lobby, clientIndex);
            }
        }
    }

}
