using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Cache;
using GameServer.Cache.Fight;
using GameServer.Database;
using MyServers;
using Protocol.Code;

namespace GameServer.Logic
{
    public class FightHandler : IHandler
    {

        private FightCache fightCache = Caches.fightCache;

        public void Disconnect(ClientPeer clientPeer)
        {


        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case FightCode.Leave_CREQ:
                    LeaveRoom(client);
                    break;
                    
                default:
                    break;
            }
        }

        /// <summary>
        /// 客户端离开请求的处理
        /// </summary>
        /// <param name="client"></param>
        private void LeaveRoom(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //不在战斗房间,忽略
                if (fightCache.IsFighting(client.Id) == false)
                {
                    return;
                }

                FightRoom room = fightCache.GetFightRoomByUserId(client.Id);
                room.leaveUserIdList.Add(client.Id);

                DatabaseManager.UpdateCoinCount(client.Id, -(room.bottomStakes * 20));
                room.Broadcase(OpCode.Fight, FightCode.Leave_BRO, client.Id);

                if (room.leaveUserIdList.Count == 1)
                {
                    if (room.giveUpCardUserIdList.Count == 0)
                    {
                        //离开的玩家是本次下注的玩家
                        //这样需转换下一个玩家下注
                        if (room.roundModel.CurrentStakesUserId == client.Id)
                        {
                            //轮换下注TODO

                        }
                    }
                }

                if (room.leaveUserIdList.Count == 2)
                {
                    //TODO
                    return;
                }
                if (room.leaveUserIdList.Count == 3)
                {
                    fightCache.DestoryRoom(room);
                }
            });
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="roomType"></param>
        public void StartFight(List<ClientPeer> clientList,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                FightRoom room = fightCache.CreateRoom(clientList);
                switch (roomType)
                {
                    case 0:
                        room.bottomStakes = 10;
                        room.topStakes = 100;
                        room.lastPlayerStakesCount = 10;
                        break;
                    case 1:
                        room.bottomStakes = 20;
                        room.topStakes = 200;
                        room.lastPlayerStakesCount = 20;
                        break;
                    case 2:
                        room.bottomStakes = 50;
                        room.topStakes = 500;
                        room.lastPlayerStakesCount = 50;
                        break;
                    default:
                        break;
                }
                //选择庄家
                ClientPeer bankerClient = room.SetBanker();
                //发牌
                room.DealCard();

                //对手牌排序
                room.SortAllPlayerCard();
                //获得牌型
                room.GetAllPlayerCardType();

                room.Broadcase(OpCode.Fight, FightCode.StartFight_BRO, room.playerList);
                //转换下注,换到下一个玩家下注
                //TODO
            });
        }
    }
}
