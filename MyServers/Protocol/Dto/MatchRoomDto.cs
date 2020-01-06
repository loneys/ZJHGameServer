using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto
{
    /// <summary>
    /// 匹配房间传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomDto
    {
        /// <summary>
        /// 用户id与该用户userDto之间的映射字典
        /// </summary>
        public Dictionary<int, UserDto> userIdUserDtoDic { get; private set; }

        /// <summary>
        /// 准备的用户ID
        /// </summary>
        public List<int> readyUserIdList { get; set; }

        /// <summary>
        /// 进入房间顺序的用户ID列表
        /// </summary>
        public List<int> enterOrderUserIdList { get; private set; }

        //左边玩家ID
        public int LeftPlayerId { get; private  set; }

        //右边玩家ID
        public int RightPlayerId { get; private set; }

        public MatchRoomDto()
        {
            userIdUserDtoDic = new Dictionary<int, UserDto>();
            readyUserIdList = new List<int>();
            enterOrderUserIdList = new List<int>();
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="dto"></param>
        public void Enter(UserDto dto)
        {
            userIdUserDtoDic.Add(dto.UserId, dto);
            enterOrderUserIdList.Add(dto.UserId);
        }

        /// <summary>
        /// 离开
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(int userId)
        {
            userIdUserDtoDic.Remove(userId);
            readyUserIdList.Remove(userId);
            enterOrderUserIdList.Remove(userId);
        }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="userId"></param>
        public void Ready(int userId)
        {
            readyUserIdList.Add(userId);
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId)
        {
            readyUserIdList.Remove(userId);
        }

        /// <summary>
        /// 重置位置给三个玩家排序
        /// </summary>
        /// <param name="myUserId"></param>
        public void ResetPosition(int myUserId)
        {
            RightPlayerId = -1;
            LeftPlayerId = -1;

            if (enterOrderUserIdList.Count == 1)
            {
                return;
            }

            if (enterOrderUserIdList.Count == 2)
            {
                if (enterOrderUserIdList[0] == myUserId)
                {
                    RightPlayerId = enterOrderUserIdList[1];
                }

                if (enterOrderUserIdList[1] == myUserId)
                {
                    LeftPlayerId = enterOrderUserIdList[0];
                }
            }

            if (enterOrderUserIdList.Count == 3)
            {
                if (enterOrderUserIdList[0] == myUserId)
                {
                    RightPlayerId = enterOrderUserIdList[1];
                    LeftPlayerId = enterOrderUserIdList[2];
                }

                if (enterOrderUserIdList[1] == myUserId)
                {
                    RightPlayerId = enterOrderUserIdList[2];
                    LeftPlayerId = enterOrderUserIdList[0];
                }

                if (enterOrderUserIdList[2] == myUserId)
                {
                    RightPlayerId = enterOrderUserIdList[1];
                    LeftPlayerId = enterOrderUserIdList[2];
                }
            }

        }
    }
}
