using Protocol.Constant;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto
{
    /// <summary>
    /// 玩家传输模型
    /// </summary>
    [Serializable]
    public class PlayerDto
    {
        //用户ID
        public int userId { get; set; }

        //用户名
        public string userName { get; set; }

        //下注总数
        public int stakesSum { get; set; }

        //身份
        public Identity identity { get; set; }

        //自己的手牌
        public List<CardDto> cardList;

        public CardType cardType;

        public PlayerDto(int userId,string userName)
        {
            this.userId = userId;
            this.userName = userName;

            stakesSum = 0;
            identity = Identity.Normal;
            cardList = new List<CardDto>();
            cardType = CardType.None;
        }

        /// <summary>
        /// 添加卡牌
        /// </summary>
        /// <param name="dto"></param>
        public void AddCard(CardDto dto)
        {
            cardList.Add(dto);
        }

        /// <summary>
        /// 移除卡牌
        /// </summary>
        /// <param name="dto"></param>
        public void RemoveCard(CardDto dto)
        {
            cardList.Remove(dto);
        }
    }
}
