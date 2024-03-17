using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace MDPro3.Net
{
    public class Server
    {
        public static void Main()
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse("192.168.3.2");
            int port = 7911;
            serverSocket.Bind(new IPEndPoint(ip, port));
            serverSocket.Listen(100);
            Debug.Log("服务器启动了");

            var clientSocket = serverSocket.Accept();
            Debug.Log("服务器接受了一个客户端的连接，客户端的详细数据为：" + clientSocket.RemoteEndPoint.ToString());
            byte[] msgArr = new byte[1024];
            clientSocket.Receive(msgArr);
            string msg = Encoding.UTF8.GetString(msgArr);
            Debug.Log("从客户端接收到的数据为：" + msg);
        }


        public static string GetLocalIPv4Address()
        {
            var addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (var address in addresses)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address.ToString();
            throw new Exception("No valid IPv4 address was found.");
        }

    }

    public class Client
    {
        public static void Main()
        {
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse("192.168.3.2");
            int port = 7911;
            clientSocket.Connect(new IPEndPoint(ip, port));
            Debug.Log("连接到了服务器");

            var connect = "666666";

            byte[] msg = Encoding.UTF8.GetBytes(connect);
            clientSocket.Send(msg);
        }
    }




}