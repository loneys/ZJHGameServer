using MyServers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache
{
    /// <summary>
    /// 匹配房间
    /// </summary>
    public  class MatchRoom
    {
        //房间ID
        public int roomId { get; private set; }

        //房间内的玩家
        public List<ClientPeer> clientList { get; private set; }

        //房间内准备的玩家ID列表
        public List<int > readyUIdList { get; set; }

        public MatchRoom(int Id)
        {
            roomId = Id;
            clientList = new List<ClientPeer>();
            readyUIdList = new List<int>();
        }

        /// <summary>
        /// 获取房间是否满了
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return clientList.Count == 3;
        }

        /// <summary>
        /// 获取房间是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return clientList.Count == 0;
        }

        /// <summary>
        /// 是否都准备了
        /// </summary>
        /// <returns></returns>
        public bool IsAllReady()
        {
            return readyUIdList.Count == 3;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="client"></param>
        public void Enter(ClientPeer client)
        {
            clientList.Add(client);
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="client"></param>
        public void Leave(ClientPeer client)
        {
            clientList.Remove(client);
            if (readyUIdList.Contains(client.Id))
            {
                readyUIdList.Remove(client.Id);
            }
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId)
        {
            readyUIdList.Add(userId);
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId)
        {
            readyUIdList.Remove(userId);
        }

        /// <summary>
        /// 广播发消息
        /// </summary>
        public void Broadcase(int opCode,int subCode,object value,ClientPeer exceptClient=null)
        {
            Console.WriteLine("--广播有玩家加入的消息--" + opCode + subCode);
            NetMsg msg = new NetMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach(var client in clientList)
            {
                if (client == exceptClient)
                {
                    Console.WriteLine("--wangzhi--房间里玩家的id--" + client.Id);
                    Console.WriteLine("--wangzhi--忽略的玩家ID--" + exceptClient.Id);
                    continue;
                }

                client.SendMsg(packet);
            }
        }
    }
}
