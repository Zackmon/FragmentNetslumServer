using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FragmentNetslumServer.Services
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

        

        public GameClientService(ILogger logger,ILobbyChatService lobbyChatService ,IServiceProvider provider)
        {
            this.logger = logger;
            this.lobbyChatService = lobbyChatService;
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
            client.OnGameClientDisconnected += Client_OnGameClientDisconnected;
            this.clients.Add(client);
            this.logger.Information($"There are {clients.Count:N0} connected clients");
            
        }

        public void RemoveClient(uint index) => this.RemoveClient(clients.Find(c => c.ClientIndex == (int)index));

        public void RemoveClient(GameClientAsync client)
        {
            this.logger.Information($"Client {client.ClientIndex} has disconnected");
            this.clients.Remove(client);
            this.logger.Information($"There are {clients.Count:N0} connected clients");
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
