using FragmentServerWV.Entities;
using FragmentServerWV.Enumerations;
using FragmentServerWV.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;

namespace FragmentServerWV.Services
{
    public sealed class GameClientService : IClientProviderService
    {

        private readonly List<GameClientAsync> clients;
        private readonly ILogger logger;
        private readonly ILobbyChatService lobbyChatService;
        private readonly IServiceProvider provider;

        public ReadOnlyCollection<GameClientAsync> Clients => clients.AsReadOnly();

        public ReadOnlyCollection<GameClientAsync> AreaServers => clients.Where(c => c.isAreaServer).ToList().AsReadOnly();

        public string ServiceName => "Game Client Service";

        public ServiceStatusEnum ServiceStatus { get; private set; }

        

        public GameClientService(ILogger logger, IServiceProvider provider)
        {
            this.logger = logger;
            this.lobbyChatService = provider.GetRequiredService<ILobbyChatService>();
            this.provider = provider;
            this.clients = new List<GameClientAsync>();
            this.ServiceStatus = ServiceStatusEnum.Active;
        }



        public void AddClient(TcpClient client, uint clientId)
        {
            var gameClient = provider.GetRequiredService<GameClientAsync>();
            gameClient.InitializeClient(clientId, client);
            this.AddClient(gameClient);
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

        private void Client_OnGameClientDisconnected(object sender, EventArgs e)
        {
            if (!(sender is GameClientAsync client)) return;
            RemoveClient(client);
            if (client.PlayerID != 0)
            {
                DBAccess.getInstance().setPlayerAsOffline(client.PlayerID);
            }
            if (!client.IsAreaServer && lobbyChatService.TryFindLobby(client, out var lobby))
            {
                lobbyChatService.AnnounceRoomDeparture(lobby, (uint)client.ClientIndex);
            }
            client.OnGameClientDisconnected -= Client_OnGameClientDisconnected;
        }

    }

}
