using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServers
{
    /// <summary>
    /// 网络消息类
    /// 作用：每次发送消息都发送这个类，接收到消息后需要转换成这个类
    /// </summary>
    public class NetMsg
    {
        /// <summary>
        /// 操作码
        /// </summary>
        public int opCode { get; set; }

        /// <summary>
        /// 子操作码
        /// </summary>
        public int subCode { get; set; }

        /// <summary>
        /// 传递的参数
        /// </summary>
        public object value { get; set; }

        public NetMsg(int opCode,int subCode,object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }

        public NetMsg()
        {

        }

        /// <summary>
        /// 改变消息内容
        /// 作用：每次消息都需要new，消耗性能，
        /// </summary>
        public void change(int opCode, int subCode, object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }
    }
}
