using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MDPro3.Net
{
    public static class LocalNet
    {
        private static bool isRefreshing = false;
        private static bool isClosing = false;
        private static List<HostPacket> hosts = new List<HostPacket>();
        private static Dictionary<(uint, ushort), (uint, ushort)> remotes = new Dictionary<(uint, ushort), (uint, ushort)>();
        private static UdpClient udpClient;
        private static Thread refreshThread;
        private static Mutex gMutex = new Mutex();

        [Serializable]
        public struct HostRequest
        {
            public uint identifier;
        }

        [Serializable]
        public struct HostInfo
        {
            public uint lflist;
            public uint rule;
            public uint mode;
            public uint draw_count;
            public uint start_hand;
            public uint start_lp;
            public bool no_check_deck;
            public bool no_shuffle_deck;
            public uint duel_rule;
        }

        [Serializable]
        public struct HostPacket
        {
            public uint identifier;
            public uint version;
            public HostInfo host;
            public uint ipaddr;
            public ushort port;
            public string name;
        }

        // Constants
        private const uint NETWORK_CLIENT_ID = 0x12345678;
        private const uint NETWORK_SERVER_ID = 0x87654321;
        private const uint PRO_VERSION = 1;
        private const uint DEFAULT_DUEL_RULE = 1;

        public static void BeginRefreshHost()
        {
            if (isRefreshing)
                return;

            isRefreshing = true;
            //btnLanRefresh.interactable = false;
            //lstHostList.Clear();

            remotes.Clear();
            hosts.Clear();

            udpClient = new UdpClient(7921);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

            refreshThread = new Thread(() => RefreshThread(udpClient));
            refreshThread.Start();

            SendRequest();
        }

        private static void RefreshThread(UdpClient client)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = client.Receive(ref remoteIp);

                        HandleBroadcastReply(data, remoteIp);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.TimedOut)
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                client.Close();
                isRefreshing = false;
                if (!isClosing)
                {
                    //UnityEngineMainThreadDispatcher.Instance().Enqueue(() =>
                    //{
                    //    btnLanRefresh.interactable = true;
                    //});
                }
            }
        }

        private static void SendRequest()
        {
            HostRequest hReq = new HostRequest
            {
                identifier = NETWORK_CLIENT_ID
            };

            UdpClient sender = new UdpClient();
            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sender.Connect(new IPEndPoint(IPAddress.Broadcast, 7920));

            byte[] buffer = BitConverter.GetBytes(hReq.identifier);
            sender.Send(buffer, buffer.Length);
            sender.Close();
        }

        private static void HandleBroadcastReply(byte[] data, IPEndPoint remoteIp)
        {
            if (isClosing)
                return;

            HostPacket packet = new HostPacket();
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    packet.identifier = br.ReadUInt32();
                    packet.version = br.ReadUInt32();
                    packet.host.lflist = br.ReadUInt32();
                    packet.host.rule = br.ReadUInt32();
                    packet.host.mode = br.ReadUInt32();
                    packet.host.draw_count = br.ReadUInt32();
                    packet.host.start_hand = br.ReadUInt32();
                    packet.host.start_lp = br.ReadUInt32();
                    packet.host.no_check_deck = br.ReadUInt32() != 0;
                    packet.host.no_shuffle_deck = br.ReadUInt32() != 0;
                    packet.host.duel_rule = br.ReadUInt32();
                    packet.name = Encoding.Unicode.GetString(br.ReadBytes(40)).TrimEnd('\0');
                }
            }

            if (packet.identifier != NETWORK_SERVER_ID || packet.version != PRO_VERSION)
                return;

            uint ipaddr = BitConverter.ToUInt32(remoteIp.Address.GetAddressBytes(), 0);
            ushort port = (ushort)remoteIp.Port;

            var remote = (ipaddr, port);
            if (!remotes.ContainsKey(remote))
            {
                gMutex.WaitOne();
                remotes.Add(remote, remote);

                packet.ipaddr = ipaddr;
                hosts.Add(packet);

                string hoststr = $"[{GetLFListName(packet.host.lflist)}]" +
                                $"[{GetRuleName(packet.host.rule)}]" +
                                $"[{GetModeName(packet.host.mode)}]" +
                                $"[{GetDefaultOrCustom(packet)}]" +
                                $"{packet.name}";
                Debug.Log(hoststr);
                //UnityEngineMainThreadDispatcher.Instance().Enqueue(() =>
                //{
                //    lstHostList.Add(hoststr);
                //});
                gMutex.ReleaseMutex();
            }
        }

        private static string GetLFListName(uint lflist)
        {
            // Implement this function to return the name of the LF list
            return "LFListName";
        }

        private static string GetRuleName(uint rule)
        {
            // Implement this function to return the name of the rule
            return "RuleName";
        }

        private static string GetModeName(uint mode)
        {
            // Implement this function to return the name of the mode
            return "ModeName";
        }

        private static string GetDefaultOrCustom(HostPacket packet)
        {
            if (packet.host.draw_count == 1 && packet.host.start_hand == 5 && packet.host.start_lp == 8000 &&
                !packet.host.no_check_deck && !packet.host.no_shuffle_deck &&
                packet.host.duel_rule == DEFAULT_DUEL_RULE)
            {
                return "Default";
            }
            else
            {
                return "Custom";
            }
        }
    }

}
