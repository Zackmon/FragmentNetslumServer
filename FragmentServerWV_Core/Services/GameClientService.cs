using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;

namespace FragmentServerWV.Services
{
    public sealed class GameClientService
    {

        private readonly List<GameClient> clients;


        /// <summary>
        /// Gets the theoretically connected clients
        /// </summary>
        public ReadOnlyCollection<GameClient> Clients => clients.AsReadOnly();








        public void AddClient(TcpClient client, int clientId) => AddClient(new GameClient(client, clientId));

        public void AddClient(GameClient client) => clients.Add(client);

        public bool RemoveClient(GameClient client) => clients.Remove(client);

        public bool RemoveClient(int index)
        {
            if (index < 0) return false;
            if (index > clients.Count) return false;
            clients.RemoveAt(index);
            return true;
        }

    }
}
