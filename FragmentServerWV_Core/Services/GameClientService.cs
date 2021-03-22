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


        /// <summary>
        /// Gets the theoretically connected clients
        /// </summary>
        public ReadOnlyCollection<GameClient> Clients => clients.AsReadOnly();


        public GameClientService(ILogger logger)
        {
            this.logger = logger;
            this.clients = new List<GameClient>();
        }



        public void AddClient(TcpClient client, uint clientId) => this.AddClient(new GameClient(client, (int)clientId));

        public void AddClient(GameClient client)
        {
            this.logger.Information("Client {@client.index} has connected", client);
            this.clients.Add(client);
            this.logger.Information("There are {@clients.Count} connected clients", this.clients);
        }

        public void RemoveClient(uint index) => this.RemoveClient(clients.Find(c => c.index == (int)index));

        public void RemoveClient(GameClient client)
        {
            this.logger.Information("Client {@client.index} is disconnecting", client);
            this.clients.Remove(client);
            this.logger.Information("There are {@clients.Count} connected clients", this.clients);
        }
    }

}
