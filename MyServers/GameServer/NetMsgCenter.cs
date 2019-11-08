using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServers;
using Protocol.Code;

namespace GameServer
{
    public class NetMsgCenter : IApplication
    {
        public void Disconnect(ClientPeer client)
        {
            
        }

        public void Receive(ClientPeer client, NetMsg msg)
        {
            switch (msg.opCode)
            {

                case OpCode.Account:
                    break;
                case OpCode.Match:
                    break;
                case OpCode.Chat:
                    break;
                case OpCode.Fight:
                    break;
                default:
                    break;
            }
        }
    }
}
