using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace MyServers
{
    public class ClientPeer
    {
        public Socket clientSocket { get; set; }

        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        public ClientPeer()
        {
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this;
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }

        /// <summary>
        /// 接收到消息后，存放到数据缓存区
        /// </summary>
        private List<byte> cache = new List<byte>();

        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isProcessingReceive = false;

        /// <summary>
        /// 消息处理完成后的委托
        /// </summary>
        /// <param name="clientPeer"></param>
        /// <param name="netMsg"></param>
        /// 
        public delegate void ReceiveCompleted(ClientPeer clientPeer, NetMsg netMsg);

        public ReceiveCompleted receiveCompleted;


        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        /// <param name="packet"></param>
        public void ProcesReceive(byte[] packet)
        {
            cache.AddRange(packet);
            if(isProcessingReceive == false)
            {
                ProcessData();
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            isProcessingReceive = true;
            //解析包，从缓存区里取出一个完整的包
            byte[] packet = EncodeTool.DecodePacket(ref cache);
            if(packet == null)
            {
                isProcessingReceive = false;
                return;
            }
            NetMsg msg = EncodeTool.DecodeMsg(packet);
            if (receiveCompleted != null)
            {
                receiveCompleted(this,msg);
            }
            ProcessData();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {

        }

    }
}
