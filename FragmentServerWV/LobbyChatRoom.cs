using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    public class LobbyChatRoom
    {
        public string name;
        public ushort ID;
        public ushort type;
        public List<int> Users;
        public LobbyChatRoom(string desc, ushort id, ushort t)
        {
            Users = new List<int>();
            name = desc;
            ID = id;
            type = t;
        }

        public void DispatchStatus(byte[] data, int who)
        {
            MemoryStream m = new MemoryStream();
            m.Write(BitConverter.GetBytes(GameClient.swap16(ID)), 0, 2);
            m.Write(BitConverter.GetBytes(GameClient.swap16((ushort)(data.Length + 2))), 0, 2);
            m.Write(data, 0, data.Length);
            byte[] buff = m.ToArray();
            foreach (GameClient client in Server.clients)
                if (!client.isAreaServer && client.index != who && !client._exited && client.room_index == ID)
                    client.SendPacket30(0x7009, buff);
        }

        public void DispatchBroadcast(byte[] data, int who)
        {
            foreach (GameClient client in Server.clients)
                if (!client.isAreaServer && !client._exited && client.room_index == ID && client.index != who)
                        client.SendPacket30(0x7862, data);
                else if (client.index == who)
                {
                    byte[] temp = new byte[data.Length];
                    data.CopyTo(temp, 0);
                    temp[0] = 0xff;
                    temp[1] = 0xff;
                    client.SendPacket30(0x7862, temp);
                }
        }
    }
}
