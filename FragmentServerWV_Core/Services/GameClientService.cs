using FragmentServerWV.Enumerations;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace FragmentServerWV.Services
{
    public sealed class GameClientService : Interfaces.IClientProviderService
    {

        private readonly List<GameClient> clients;
        private readonly ILogger logger;
        private readonly SimpleConfiguration simpleConfiguration;


        /// <summary>
        /// Gets the theoretically connected clients
        /// </summary>
        public ReadOnlyCollection<GameClient> Clients => clients.AsReadOnly();

        public string ServiceName => "Game Client Service";

        public ServiceStatusEnum ServiceStatus { get; private set; }

        public GameClientService(ILogger logger, SimpleConfiguration simpleConfiguration)
        {
            this.logger = logger;
            this.simpleConfiguration = simpleConfiguration;
            this.clients = new List<GameClient>();
            this.ServiceStatus = ServiceStatusEnum.Active;
        }



        public void AddClient(TcpClient client, uint clientId)
        {
            this.AddClient(new GameClient(client, (int)clientId, logger, simpleConfiguration));
        }

        public void AddClient(GameClient client)
        {
            this.logger.Information($"Client {client.index} has connected");
            this.clients.Add(client);
            this.logger.Information($"There are {clients.Count} connected clients");
        }

        public void RemoveClient(uint index) => this.RemoveClient(clients.Find(c => c.index == (int)index));

        public void RemoveClient(GameClient client)
        {
            this.logger.Information($"Client {client.index} is disconnecting");
            this.clients.Remove(client);
            this.logger.Information($"There are {clients.Count} connected clients");
        }
    }

}
