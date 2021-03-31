using FragmentServerWV.Entities;
using FragmentServerWV.Enumerations;
using FragmentServerWV.Services.Interfaces;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace FragmentServerWV.Services
{
    public sealed class GameClientService : IClientProviderService
    {

        private readonly List<GameClientAsync> clients;
        private readonly ILogger logger;
        private readonly ILobbyChatService lobbyChatService;
        private readonly SimpleConfiguration simpleConfiguration;



        public ReadOnlyCollection<GameClientAsync> Clients => clients.AsReadOnly();

        public string ServiceName => "Game Client Service";

        public ServiceStatusEnum ServiceStatus { get; private set; }

        public GameClientService(ILogger logger, ILobbyChatService lobbyChatService, SimpleConfiguration simpleConfiguration)
        {
            this.logger = logger;
            this.lobbyChatService = lobbyChatService;
            this.simpleConfiguration = simpleConfiguration;
            this.clients = new List<GameClientAsync>();
            this.ServiceStatus = ServiceStatusEnum.Active;
        }



        public void AddClient(TcpClient client, uint clientId)
        {
            this.AddClient(new GameClientAsync(clientId, logger, lobbyChatService, client, simpleConfiguration));
        }

        public void AddClient(GameClientAsync client)
        {
            this.logger.Information($"Client {client.ClientIndex} has connected");
            this.clients.Add(client);
            this.logger.Information($"There are {clients.Count} connected clients");
            client.OnGameClientDisconnected += Client_OnGameClientDisconnected;
        }

        public void RemoveClient(uint index) => this.RemoveClient(clients.Find(c => c.ClientIndex == (int)index));

        public void RemoveClient(GameClientAsync client)
        {
            this.logger.Information($"Client {client.ClientIndex} is disconnecting");
            this.clients.Remove(client);
            this.logger.Information($"There are {clients.Count} connected clients");
        }

        public bool TryGetClient(uint index, out GameClientAsync client)
        {
            client = null;
            foreach (var c in clients)
            {
                if (c.ClientIndex == index)
                {
                    client = c;
                    return true;
                }
            }
            return false;
        }

        private void Client_OnGameClientDisconnected(object sender, System.EventArgs e)
        {
            if (!(sender is GameClientAsync client)) return;
            RemoveClient(client);
            if (client.PlayerID != 0)
            {
                DBAcess.getInstance().setPlayerAsOffline(client.PlayerID);
            }
            lobbyChatService.AnnounceRoomDeparture((ushort)client.LobbyIndex, (uint)client.ClientIndex);
            client.OnGameClientDisconnected -= Client_OnGameClientDisconnected;
        }

    }

}
