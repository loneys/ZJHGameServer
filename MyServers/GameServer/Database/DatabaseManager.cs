using MyServers;
using MySql.Data.MySqlClient;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    public class DatabaseManager
    {
        private static MySqlConnection sqlConcect;
        private static Dictionary<int, ClientPeer> idClientDic;
        private static RankListDto rankListDto;

        /// <summary>
        /// 连接mysql
        /// </summary>
        public static void StartConnect()
        {
            rankListDto = new RankListDto();
            idClientDic = new Dictionary<int, ClientPeer>();
            string conStr = "database=zjhgame;data source = 127.0.0.1;port=3306;user = root;pwd=root";
            sqlConcect = new MySqlConnection(conStr);
            sqlConcect.Open();
        }

        /// <summary>
        /// 判断用户名是否存在
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsExistUserName(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select UserName from userinfo where UserName=@name", sqlConcect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public static void CreateUser(string userName, string pwd)
        {
            MySqlCommand cmd = new MySqlCommand("insert into userinfo set UserName=@name,Password=@pwd,Online=0,IconName=@iconName", sqlConcect);
            cmd.Parameters.AddWithValue("name", userName);
            cmd.Parameters.AddWithValue("pwd", pwd);
            Random ran = new Random();
            int index = ran.Next(0, 19);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + index.ToString());
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 判断账号、密码是否匹配
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool IsMatch(string userName, string pwd)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where UserName=@name", sqlConcect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                bool result = reader.GetString("Password") == pwd;
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// 判断用户是否在线
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsOnline(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select Online from userinfo where UserName=@name", sqlConcect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                bool result = reader.GetBoolean("Online");
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// 登录上线
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="client"></param>
        public static void Login(string userName,ClientPeer client)
        {
            //登录后设置玩家的状态为在线
            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=true where UserName = @name", sqlConcect);
            cmd.Parameters.AddWithValue("name", userName);
            cmd.ExecuteNonQuery();

            //给玩家对象赋值id和username,并加入字典
            MySqlCommand cmd1 = new MySqlCommand("select * from userinfo where UserName=@name", sqlConcect);
            cmd1.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd1.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int id = reader.GetInt32("Id");
                client.Id = id;
                client.UserName = userName;
                if (idClientDic.ContainsKey(id) == false)
                {
                    idClientDic.Add(id, client);
                }
            }
            reader.Close();
        }

        public static void OffLine(ClientPeer client)
        {
            if (idClientDic.ContainsKey(client.Id))
            {
                idClientDic.Remove(client.Id);
            }
            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=false where Id=@id", sqlConcect);
            cmd.Parameters.AddWithValue("id", client.Id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 使用用户id获取客户端连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClientPeer GetClientPeerByUserId(int id)
        {
            if (idClientDic.ContainsKey(id))
            {
                return idClientDic[id];
            }
            return null;
        }

        public static UserDto CreateUserDto(int userId)
        {
            MySqlCommand cmd = new MySqlCommand("select *from userinfo where Id=@id",sqlConcect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                UserDto dto = new UserDto(userId, reader.GetString("UserName"), reader.GetString("IconName"), reader.GetInt32("Coin"));
                reader.Close();
                return dto;
            }
            reader.Close();
            return null;

        }
        /// <summary>
        /// 获取排行榜信息
        /// </summary>
        /// <returns></returns>
        public static RankListDto GetRankListDto()
        {
            MySqlCommand cmd = new MySqlCommand("select UserName,Coin from userinfo order by Coin desc", sqlConcect);
            MySqlDataReader reader = cmd.ExecuteReader();
            rankListDto.Clear();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RankItemDto dto = new RankItemDto(reader.GetString("UserName"), reader.GetInt32("Coin"));
                    rankListDto.Add(dto);
                }
                reader.Close();
                return rankListDto;
            }
            reader.Close();
            return null;
        }

        public static int UpdateCoinCount(int userId,int value)
        {
            MySqlCommand cmd = new MySqlCommand("select Coin from userinfo where Id =@id ", sqlConcect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int remainCoinCount = reader.GetInt32("Coin");
                reader.Close();

                MySqlCommand cmdUpdate = new MySqlCommand("update userinfo set Coin=@coin where Id = @id", sqlConcect);
                cmdUpdate.Parameters.AddWithValue("coin", remainCoinCount + value);
                cmdUpdate.Parameters.AddWithValue("id", userId);
                cmdUpdate.ExecuteNonQuery();

                return remainCoinCount + value;
            }
            reader.Close();
            return 0;
        }
    }
}
