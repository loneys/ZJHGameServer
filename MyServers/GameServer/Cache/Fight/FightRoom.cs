using MyServers;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗房间
    /// </summary>
    public class FightRoom
    {
        //房间ID
        public int roomId;

        //玩家列表
        public List<PlayerDto> playerList;

        //牌库
        public CardLibrary cardLibrary;

        //回合管理类
        public RoundModel roundModel;

        //离开的玩家列表
        public List<int> leaveUserIdList;

        //弃牌的玩家列表
        public List<int> giveUpCardUserIdList;

        //顶注
        public int topStakes;

        //底注
        public int bottomStakes;

        //上一位玩家下注的数量
        public int lastPlayerStakesCount;

        //总下注数
        public int stakesSum;

        //庄家在玩家列表中的下标
        private int bankerIndex = -1;

        public FightRoom(int roomId, List<ClientPeer> clientList)
        {
            this.roomId = roomId;
            playerList = new List<PlayerDto>();
            foreach (var client in clientList)
            {
                PlayerDto dto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(dto);
            }
            cardLibrary = new CardLibrary();
            roundModel = new RoundModel();
            leaveUserIdList = new List<int>();
            giveUpCardUserIdList = new List<int>();
            stakesSum = 0;
        }

        public void Init(List<ClientPeer> clientList)
        {
            playerList.Clear();
            foreach (var client in clientList)
            {
                PlayerDto dto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(dto);
            }
            stakesSum = 0;
        }

        /// <summary>
        /// 选择庄家
        /// </summary>
        public ClientPeer SetBanker()
        {
            Random ran = new Random();
            int ranIndex = ran.Next(0, playerList.Count);
            bankerIndex = ranIndex;
            //随机到的庄家用户id
            int bankerId = playerList[ranIndex].userId;
            playerList[ranIndex].identity = Identity.Banker;
            roundModel.Start(bankerId);
            ClientPeer bankerClient = Database.DatabaseManager.GetClientPeerByUserId(bankerId);
            string bankerUserName = bankerClient.UserName;
            Console.WriteLine("庄家为：" + bankerUserName);
            return bankerClient;
        }

        /// <summary>
        /// 发牌
        /// </summary>
        public void DealCard()
        {
            //默认庄家先发
            int dealCardIndex = bankerIndex;
            for (int i = 0; i < 9; i++)
            {
                playerList[dealCardIndex].AddCard(cardLibrary.DealCard());
                dealCardIndex++;
                if (dealCardIndex > playerList.Count - 1)
                {
                    dealCardIndex = 0;
                }
            }
        }

        /// <summary>
        /// 对牌排序
        /// </summary>
        /// <param name="cardList"></param>
        private void SortCard(ref List<CardDto> cardList)
        {
            for (int i = 0; i < cardList.Count-1; i++)
            {
                for (int j = 0; j < cardList.Count-1-i; j++)
                {
                    if (cardList[j].Weight < cardList[j + 1].Weight)
                    {
                        CardDto temp = cardList[j];
                        cardList[j] = cardList[j + 1];
                        cardList[j + 1] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// 对房间内的所有玩家手牌进行排序
        /// </summary>
        public void SortAllPlayerCard()
        {
            foreach (var player in playerList)
            {
                SortCard(ref player.cardList);
            }
        }

        /// <summary>
        /// 获取所有玩家牌型
        /// </summary>
        public void GetAllPlayerCardType()
        {
            foreach (var player in playerList)
            {
                player.cardType = GetCardType(player.cardList);
            }
        }

        /// <summary>
        /// 获取牌型
        /// </summary>
        private CardType GetCardType(List<CardDto> cardList)
        {
            CardType temp = CardType.Max;
            if (cardList[0].Weight == 5 && cardList[1].Weight == 3 && cardList[2].Weight == 2)
            {
                temp = CardType.Max;
            }
            else if (cardList[0].Weight == cardList[1].Weight && cardList[0].Weight == cardList[2].Weight)
            {
                temp = CardType.Baozi;
            }
            else if (cardList[0].Color == cardList[1].Color && cardList[0].Color == cardList[2].Color
                && cardList[0].Weight == cardList[1].Weight + 1 && cardList[0].Weight == cardList[2].Weight + 2)
            {
                temp = CardType.Shunjin;
            }
            else if (cardList[0].Color == cardList[1].Color && cardList[0].Color == cardList[2].Color)
            {
                temp = CardType.Jinhua;
            }
            else if (cardList[0].Weight == cardList[1].Weight + 1 && cardList[0].Weight == cardList[2].Weight + 2)
            {
                temp = CardType.Shunzi;
            }
            else if (cardList[0].Weight == cardList[1].Weight || cardList[1].Weight == cardList[2].Weight)
            {
                temp = CardType.Duizi;
            }
            else
            {
                temp = CardType.Min;
            }
            return temp;
        }

        /// <summary>
        /// 是否离开
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsLeaveRoom(int userId)
        {
            return leaveUserIdList.Contains(userId);
        }

        /// <summary>
        /// 是否棋牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsGiveUpCard(int userId)
        {
            return giveUpCardUserIdList.Contains(userId);
        }

        /// <summary>
        /// 广播发消息
        /// </summary>
        public void Broadcase(int opCode, int subCode, object value, ClientPeer exceptClient = null)
        {
            Console.WriteLine("--广播有玩家加入的消息--" + opCode + subCode);
            NetMsg msg = new NetMsg(opCode, subCode, value);
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach (var player in playerList)
            {
                ClientPeer client = Database.DatabaseManager.GetClientPeerByUserId(player.userId);
                if (client == exceptClient)
                {
                    Console.WriteLine("--wangzhi--房间里玩家的id--" + client.Id);
                    Console.WriteLine("--wangzhi--忽略的玩家ID--" + exceptClient.Id);
                    continue;
                }

                client.SendMsg(packet);
            }
        }

        /// <summary>
        /// 轮换下注
        /// </summary>
        /// <returns>下一次下注的玩家ID</returns>
        public int Turn()
        {
            int currentUserId = roundModel.CurrentStakesUserId;
            int nextUserId = GetNextUserId(currentUserId);
            roundModel.Turn(nextUserId);
            return nextUserId;
        }

        /// <summary>
        /// 获得下一次下注的玩家ID
        /// </summary>
        /// <param name="currentId"></param>
        /// <returns></returns>
        private int GetNextUserId(int currentId)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].userId == currentId)
                {
                    if (i == playerList.Count - 1)
                    {
                        return playerList[0].userId;
                    }
                    else
                    {
                        return playerList[i + 1].userId;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 更新玩家下注总数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="stakesCount"></param>
        public void UpdatePlayerStakesSum(int userId, int stakesCount)
        {
            foreach (var player in playerList)
            {
                if (player.userId == userId)
                {
                    player.stakesSum += stakesCount;
                }
            }
        }

        /// <summary>
        /// 重置房间数据
        /// </summary>
        public void Destory()
        {
            playerList.Clear();
            cardLibrary.Init();
            roundModel.Init();
            leaveUserIdList.Clear();
            giveUpCardUserIdList.Clear();
            stakesSum = 0;
            bankerIndex = -1;
        }
    }
}
