using MySql.Data.MySqlClient;
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

        /// <summary>
        /// 连接mysql
        /// </summary>
        public static void StartConnect()
        {
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
    }
}
