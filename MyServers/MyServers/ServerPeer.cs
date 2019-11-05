using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServers
{
    public class ServerPeer
    {
        //服务器Socket
        private Socket serverSocket;

        //计量器
        private Semaphore semaphore;

        // 客户端对象连接池
        private ClientPeerPool clientPeerPool;

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void StartServer(string ip, int port, int maxClient)
        {
            try
            {
                clientPeerPool = new ClientPeerPool(maxClient);
                semaphore = new Semaphore(maxClient, maxClient);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //填满客户端对象连接池
                for (int i = 0; i < maxClient; i++)
                {
                    ClientPeer temp = new ClientPeer();
                    temp.receiveCompleted = ReceiveProcessCompleted;
                    temp.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    clientPeerPool.Enqueue(temp);
                }

                //绑定到进程
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //最大的监听数
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");
                StartAccpet(null);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
            
        }



        #region 接收客户端的连接请求

        /// <summary>
        /// 接收客户端的连接
        /// </summary>
        private void StartAccpet(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }

            //如果result为true，代表正在接收连接，连接之后会调用completed
            //如果rusult为false，代表接收成功，
            bool result = serverSocket.AcceptAsync(e);
            if (result == false)
            {
                ProcessAccept(e);
            }
        }

        /// <summary>
        /// 异步接收客户端的连接后的触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接的请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            semaphore.WaitOne();
            //ClientPeer client = new ClientPeer();     每次都new耗费性能，采用一次全部new出来的方式
            ClientPeer client = clientPeerPool.Dequeue();
            client.clientSocket = e.AcceptSocket;
            Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端连接成功");
            //接收消息TODO
            StartRecoive(client);

            e.AcceptSocket = null;
            StartAccpet(e);
        }

        #endregion

        #region 接收数据
        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <param name="client"></param>
        private void StartRecoive(ClientPeer client)
        {
            try
            {
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

        }
        /// <summary>
        /// 异步接收数据完成后的调用
        /// </summary>
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /// <summary>
        /// 处理数据的接收
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            ClientPeer client = e.UserToken as ClientPeer;
            //判断数据是否接收成功
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);

                //让ClientPeer自身处理接收到的数据
                client.ProcesReceive(packet);
                StartRecoive(client);
            }
            //数据接收不成功，断开连接了
            else
            {
                //没有传输的字节数，代表断开连接
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnect(client, "客户端主动断开连接");
                    }
                    //因为网络原因被动断开连接
                    else
                    {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 一条消息处理完成后的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void ReceiveProcessCompleted(ClientPeer client,NetMsg msg)
        {
            //TODO
        }

        #endregion

        #region 断开连接
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        private void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空,无法断开连接");
                }
                Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端断开连接,原因：" + reason);
                //让客户端断开连接
                client.DisConnect();
                clientPeerPool.Enqueue(client);
                semaphore.Release();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }
        #endregion





    }
}
