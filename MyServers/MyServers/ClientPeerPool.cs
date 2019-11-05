using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServers
{
    /// <summary>
    /// 客户端连接池
    /// </summary>
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int maxCount)
        {
            clientPeerQueue = new Queue<ClientPeer>(maxCount);
        }

        public void Enqueue(ClientPeer client)
        {
            clientPeerQueue.Enqueue(client);
        }

        public ClientPeer Dequeue()
        {
            return clientPeerQueue.Dequeue();
        }
    }
}
