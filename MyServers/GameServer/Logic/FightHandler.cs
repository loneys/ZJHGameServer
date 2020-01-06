using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Cache;
using GameServer.Cache.Fight;
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
