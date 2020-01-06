using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServers;
using Protocol.Code;
using GameServer.Cache;
using GameServer.Database;
using Protocol.Dto;

namespace GameServer.Logic
{
    /// <summary>
    /// 开始游戏的委托
    /// </summary>
    /// <param name="clientList"></param>
    /// <param name="roomType"></param>
    public delegate void StartFight(List<ClientPeer> clientList, int roomType);
    public class MatchHandler : IHandler
    {
        /// <summary>
        /// 匹配房间缓存集合
        /// </summary>
        private List<MatchCache> matchCacheList = Caches.matchCacheList;

        public StartFight startFight;

        public void Disconnect(ClientPeer clientPeer)
        {
            for (int i = 0; i < matchCacheList.Count; i++)
            {
                LeaveRoom(clientPeer, i);
            }
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case MatchCode.Enter_CREQ:
                    Console.WriteLine("--进入的类型是多少--" + (int)value);
                    EnterRoom(client, (int)value);
                    break;
                case MatchCode.Leave_CREQ:
                    LeaveRoom(client, (int)value);
                    break;
                case MatchCode.Read_CREQ:
                    Ready(client, (int)(value));
                    break;
                case MatchCode.UnReady_CREQ:
                    UnReady(client, (int)(value));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void UnReady(ClientPeer client, int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.UnReady(client.Id);
                room.Broadcase(OpCode.Match, MatchCode.UnReady_BRO, client.Id);
            });
        }

        /// <summary>
        /// 客户端发来的准备请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void Ready(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.Ready(client.Id);
                room.Broadcase(OpCode.Match, MatchCode.Read_BRO, client.Id);

                //全部都准备了，可以开始游戏了
                if (room.IsAllReady())
                {
                    startFight(room.clientList, roomType);
                    //通知房间内的所有玩家开始游戏了
                    room.Broadcase(OpCode.Match, MatchCode.StartGame_BRO, null);
                    //销毁房间
                    matchCacheList[roomType].DestroyRoom(room);
                }
            });
        }

        /// <summary>
        /// 客户端进入房间的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void EnterRoom(ClientPeer client, int roomType)
        {
            
            SingleExecute.Instance.Execute(() => {
                //判断一下当前的客户端连接对象是否在匹配房间里面，如果在则忽略
                if (matchCacheList[roomType].IsMatching(client.Id))
                {
                    return;
                }
                else
                {
                    Console.WriteLine("--wangzhi--进入的玩家为：--" + client.Id);
                    MatchRoom room = matchCacheList[roomType].Enter(client);
                    //构造用户数据传输模型
                    UserDto userDto = DatabaseManager.CreateUserDto(client.Id);
                    //广播给房间内的所有玩家，除了自身，有新的玩家进来了，参数：新进用户的userdto
                    room.Broadcase(OpCode.Match, MatchCode.Enter_BRO, userDto, client);

                    //给客户端一个响应 参数：房间传输模型，包含房间内的正在等待的玩家以及准备的玩家id集合
                    Console.WriteLine("--wangzhi--房间里的玩家个数--"+room.clientList.Count);
                    Console.WriteLine("--wangzhi--房间ID为：--"+room.roomId);
                    foreach (var player in room.clientList)
                    {
                        Console.WriteLine("--wangzhi--玩家id分别为：--"+player.Id);
                    }
                    client.SendMsg(OpCode.Match, MatchCode.Enter_SRES, MakeMatchRoomDto(room,client.Id));

                    if (roomType == 0)
                    {
                        Console.WriteLine(userDto.UserName + "进入底注为10，顶注为100的房间");
                    }
                    if (roomType == 1)
                    {
                        Console.WriteLine(userDto.UserName + "进入底注为20，顶注为200的房间");
                    }
                    if (roomType == 2)
                    {
                        Console.WriteLine(userDto.UserName + "进入底注为50，顶注为500的房间");
                    }
                }
            });
        }

        private MatchRoomDto MakeMatchRoomDto(MatchRoom room,int clientId)
        {
            MatchRoomDto dto = new MatchRoomDto();
            for(int i = 0; i < room.clientList.Count; i++)
            {
                dto.Enter(DatabaseManager.CreateUserDto(room.clientList[i].Id));
                
            }
            dto.readyUserIdList = room.readyUIdList;
            return dto;
        }

        /// <summary>
        /// 客户端离开房间的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void LeaveRoom(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //不在匹配房间，忽略
                if (matchCacheList[roomType].IsMatching(client.Id) == false)
                {
                    return;
                }

                MatchRoom room = matchCacheList[roomType].Leave(client.Id);
                room.Broadcase(OpCode.Match, MatchCode.Leave_BRO, client.Id);
            });
        }
    }
}
