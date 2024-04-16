using DG.Tweening.Plugins.Core.PathCore;
using MDPro3.Net;
using MDPro3.UI;
using MDPro3.YGOSharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MDPro3
{
    public class Online : Servant
    {
        public InputField inputName;
        public InputField inputHost;
        public InputField inputPort;
        public InputField inputPassword;
        public ScrollRect scrollView;
        SuperScrollView superScrollView;

        public struct HostAddress
        {
            public string name;
            public string host;
            public string port;
            public string password;
        }

        readonly string savePath = "Data/hosts.conf";
        public List<HostAddress> addresses = new List<HostAddress>();
        List<string[]> tasks = new List<string[]>();

        public override void Initialize()
        {
            depth = 1;
            haveLine = true;
            returnServant = Program.I().menu;
            inputName.onEndEdit.AddListener(OnNameChange);
            inputHost.onEndEdit.AddListener(OnHostChange);
            inputPort.onEndEdit.AddListener(OnPortChange);
            inputPassword.onEndEdit.AddListener(OnPasswordChange);
            base.Initialize();

            LoadHostAddress();
        }

        void LoadHostAddress()
        {
            if (!File.Exists(savePath))
                return;
            var txtString = File.ReadAllText(savePath);
            var lines = txtString.Replace("\r", "").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var mats = Regex.Split(lines[i], " ");
                var address = new HostAddress();
                if (mats.Length >= 3)
                {
                    address.name = mats[0];
                    address.host = mats[1];
                    address.port = mats[2];
                    address.password = "";
                    if (mats.Length > 3)
                        address.password = mats[3];
                    addresses.Add(address);
                }
            }
            Print();
        }
        public void Save()
        {
            var content = "";
            foreach (var address in addresses)
            {
                content += address.name + " ";
                content += address.host + " ";
                content += address.port + " ";
                content += address.password + " \r\n";
            }
            File.WriteAllText(savePath, content);
        }
        public void Print(string search = "")
        {
            superScrollView?.Clear();
            tasks.Clear();
            foreach (var address in addresses)
            {
                if (address.name.Contains(search))
                {
                    string[] task = new string[] { address.name, address.host, address.port, address.password };
                    tasks.Add(task);
                }
            }
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonHostAddress");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollView
                    (
                    1,
                    360,
                    80,
                    0,
                    0,
                    result.Result,
                    ItemOnListRefresh,
                    scrollView
                    );
                superScrollView.Print(tasks);
            };
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemForAddress>();
            handler.addressName = task[0];
            handler.addressHost = task[1];
            handler.addressPort = task[2];
            handler.addressPassword = task[3];
            handler.Refresh();
        }

        public override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            inputName.text = Config.Get("DuelPlayerName0", "@ui");
            inputHost.text = Config.Get("Host", "s1.ygo233.com");
            inputPort.text = Config.Get("Port", "233");
            inputPassword.text = Config.Get("Password", "@ui");
        }

        public override void Hide(int preDepth)
        {
            if (!isShowed)
                return;
            base.Hide(preDepth);
            Config.Save();
            Save();
        }
        public void OnSaveAddress()
        {
            var title = InterString.Get("请输入预设名称");
            var selections = new List<string>()
        {
            InterString.Get("请输入预设名称"),
            string.Empty
        };
            UIManager.ShowPopupInput(selections, AddAddress, null, InputValidation.ValidationType.NoSpace);
        }
        void AddAddress(string name)
        {
            var address = new HostAddress();
            address.name = name;
            address.host = inputHost.text;
            address.port = inputPort.text;
            address.password = inputPassword.text;
            foreach (var add in addresses)
            {
                if (add.name == name)
                {
                    addresses.Remove(add);
                    break;
                }
            }
            addresses.Add(address);
            Save();
            Print();
        }

        public void CreateServer()
        {
            string args = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                "7911",
                BanlistManager.GetIndexByName(serverSelections[1]),
                GetPoolCodeByName(serverSelections[2]),
                GetModeCodeByName(serverSelections[3]),
                "F",
                serverSelections[4],
                serverSelections[5],
                serverSelections[7],
                serverSelections[8],
                serverSelections[9],
                serverSelections[6],
                "0"
                );
            Room.fromSolo = false;
            Room.fromLocalHost = true;
            YgoServer.StartServer(args);
            string name = Config.Get("DuelPlayerName0", "@ui");
            (new Thread(() => { Thread.Sleep(200); TcpHelper.Join("127.0.0.1", Config.Get("DuelPlayerName0", "@ui"), "7911", "", ""); })).Start();
        }

        string GetPoolCodeByName(string pool)
        {
            for (int i = 1481; i < 1487; i++)
            {
                if (StringHelper.GetUnsafe(i) == pool)
                    return (i - 1481).ToString();
            }
            return "5";
        }
        string GetModeCodeByName(string mode)
        {
            for (int i = 1244; i < 1247; i++)
            {
                if (StringHelper.GetUnsafe(i) == mode)
                    return (i - 1244).ToString();
            }
            return "0";
        }

        public List<string> serverSelections;
        public static bool severSelectionsInitialized;

        public void OnServer()
        {
            if (!severSelectionsInitialized)
            {
                serverSelections = new List<string>()
                {
                    InterString.Get("创建主机"),
                    BanlistManager.Banlists[0].Name,
                    StringHelper.GetUnsafe(1481),
                    StringHelper.GetUnsafe(1244),
                    "F",
                    "F",
                    "180",
                    "8000",
                    "5",
                    "1"
                };
                severSelectionsInitialized = true;
            }
            UIManager.ShowPopupServer(serverSelections);
        }
        void OnNameChange(string name)
        {
            Config.Set("DuelPlayerName0", name == "" ? "@ui" : name);
            Config.Save();
        }
        public void OnHostChange(string host)
        {
            Config.Set("Host", host);
            Config.Save();
        }
        public void OnPortChange(string port)
        {

            Config.Set("Port", port);
            Config.Save();
        }
        public void OnPasswordChange(string password)
        {
            Config.Set("Password", password == "" ? "@ui" : password);
            Config.Save();
        }

        public void Join()
        {
            KF_OnlineGame(inputName.text, inputHost.text, inputPort.text, "", inputPassword.text);
        }
        public void KF_OnlineGame(string name, string ip, string port, string version, string password)
        {
            if (name == "")
            {
                MessageManager.Cast("用户名不能为空。");
                return;
            }

            if (ip == "" || port == "")
            {
                MessageManager.Cast("主机地址和端口不能为空。");
                return;
            }
            if (!TcpHelper.canJoin)
                return;
            Room.fromSolo = false;
            Room.fromLocalHost = false;
            new Thread(() => { TcpHelper.Join(ip, Config.Get("DuelPlayerName0", "@ui"), port, password, version); }).Start();
        }
    }
}
