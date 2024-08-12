using MDPro3.Net;
using MDPro3.UI;
using MDPro3.YGOSharp;
using System;
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
    public class Solo : Servant
    {
        public ScrollRect scrollRect;
        public Text description;
        public SuperScrollViewTwoStage superScrollView;
        List<string[]> tasks = new List<string[]>();

        public UnityEngine.UI.Toggle toggleLockHand;
        public UnityEngine.UI.Toggle toggleNoCheck;
        public UnityEngine.UI.Toggle toggleNoShuffle;
        public InputField inputPort;
        public InputField inputLP;
        public InputField inputHand;
        public InputField inputDraw;
        public GameObject btnDeck;

        public static string port;
        const string windbotDialogsPath = "Data/Windbot/Dialogs/";

        public class BotInfo
        {
            public string name;
            public string command;
            public string desc;
            public string[] flags;
            public int main0;
        }
        private IList<BotInfo> bots = new List<BotInfo>();

        public enum Condition
        {
            ForSolo,
            ForRoom
        }

        public static Condition condition;

        public void SwitchCondition(Condition condition)
        {
            Solo.condition = condition;
            switch (condition)
            {
                case Condition.ForSolo:
                    returnServant = Program.I().menu;
                    depth = 1;
                    break;
                case Condition.ForRoom:
                    returnServant = Program.I().room;
                    depth = 3;
                    break;
            }
        }

        public override void Initialize()
        {
            haveLine = true;
            SwitchCondition(Condition.ForSolo);
            base.Initialize();
            btnDeck.transform.GetChild(0).GetComponent<Text>().text = Config.Get("DeckInUse", "@ui");
            btnDeck.SetActive(false);
            Load();
        }

        public void Load()
        {
            ReadBots(Program.localesPath + Config.Get("Language", "zh-CN") + "/bot.conf");
            Print();
            StartCoroutine(SelectZero());
        }

        private void ReadBots(string confPath)
        {
            bots.Clear();
            StreamReader reader = new StreamReader(new FileStream(confPath, FileMode.Open, FileAccess.Read));
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();
                if (line.Length > 0 && line[0] == '!')
                {
                    BotInfo newBot = new BotInfo();
                    newBot.name = line.TrimStart('!');
                    newBot.command = reader.ReadLine().Trim();
                    newBot.desc = reader.ReadLine().Trim();
                    line = reader.ReadLine().Trim();
                    newBot.flags = line.Split(' ');

                    newBot.main0 = 5990062;
                    Deck aiDeck = new Deck();
                    try
                    {
                        string deckName = "";
                        deckName = newBot.command.Split(new string[] { "Deck=", " Dialog=" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("'", "").Replace(" ", "");
                        if(File.Exists("Data/WindBot/Decks/Ai_" + deckName + Program.ydkExpansion)) 
                        {
                            aiDeck = new Deck("Data/WindBot/Decks/Ai_" + deckName + Program.ydkExpansion);
                            if(aiDeck.Main.Count > 0)
                                newBot.main0 = aiDeck.Main[0];
                        }
                    }
                    catch (Exception e) { }

                    bots.Add(newBot);
                }
            }
        }

        public void Print()
        {
            superScrollView?.Clear();
            tasks.Clear();

            for (int i = 0; i < bots.Count; i++)
            {
                string[] task = new string[]
                {
                    i.ToString(),
                };
                tasks.Add(task);
            }
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonTwoStageForSolo");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollViewTwoStage
                    (
                    1,
                    700,
                    140,
                    0,
                    -10,
                    result.Result,
                    ItemOnListRefresh,
                    scrollRect,
                    30
                    );
                superScrollView.Print(tasks);
            };
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemTwoStageForSolo>();
            handler.id = int.Parse(task[0]);
            handler.botInfo = bots[handler.id];
            handler.Refresh();
        }

        IEnumerator SelectZero()
        {
            while(superScrollView == null || superScrollView.items.Count == 0)
                yield return null;
            var item0 = superScrollView.items[0].gameObject.GetComponent<SuperScrollViewItemTwoStageForSolo>();
            while(!item0.refreshed)
                yield return null;
            item0.ToStage1();
        }

        public void OnSelectAIDeck()
        {
            Program.I().selectDeck.SwitchCondition(SelectDeck.Condition.ForSolo);
            Program.I().ShiftToServant(Program.I().selectDeck);
        }

        string GetWindBotCommand(int aiCode, bool diyDeck)
        {
            BotInfo bot = bots[aiCode];
            string aiCommand = bot.command;
            if (diyDeck)
            {
                string selectedDeck = btnDeck.transform.GetChild(0).GetComponent<Text>().text;
                if (!File.Exists(Program.deckPath + selectedDeck + Program.ydkExpansion))
                {
                    MessageManager.Cast(InterString.Get("请先为AI选择有效的卡组。"));
                    return string.Empty;
                }
                aiCommand += " DeckFile=\"" + btnDeck.transform.GetChild(0).GetComponent<Text>().text + "\"";
            }
            Match match = Regex.Match(aiCommand, "Random=(\\w+)");
            if (match.Success)
            {
                string randomFlag = match.Groups[1].Value;
                string command = GetRandomBot(randomFlag);
                if (command != string.Empty)
                    aiCommand = command;
            }
            return aiCommand;
        }

        public void StartAIForSolo(int aiCode, bool diyDeck)
        {
            string aiCommand = GetWindBotCommand(aiCode, diyDeck);
            if(aiCommand != string.Empty)
                Launch(aiCommand, toggleLockHand.isOn, toggleNoCheck.isOn, toggleNoShuffle.isOn);
        }

        public void StartAIForRoom(int aiCode, bool diyDeck)
        {
            string aiCommand = GetWindBotCommand(aiCode, diyDeck);
            if (aiCommand != string.Empty)
            {
                StartWindBot(aiCommand, TcpHelper.joinedAddress, TcpHelper.joinedPort, TcpHelper.joinedPassword, toggleLockHand.isOn);
                Program.I().ShiftToServant(Program.I().room);
            }
        }

        private string GetRandomBot(string flag)
        {
            IList<BotInfo> foundBots = new List<BotInfo>();
            foreach (var bot in bots)
            {
                if (Array.IndexOf(bot.flags, flag) >= 0) foundBots.Add(bot);
            }
            if (foundBots.Count > 0)
            {
                System.Random rand = new System.Random();
                BotInfo bot = foundBots[rand.Next(foundBots.Count)];
                return bot.command;
            }
            return "";
        }


        public void StartWindBot(string command, string ip, string port, string password, bool lockHand)
        {
            command = command.Replace("'", "\"");
            if (lockHand)
                command += " Hand=1";
            command += " Host=" + ip;
            command += " Port=" + port;
            command += " HostInfo=" + password;

            var args = Tools.SplitWithPreservedQuotes(command);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("Dialog="))
                {
                    var path = args[i][7..];
                    if(!File.Exists(windbotDialogsPath + path + ".json"))
                    {
                        var config = Config.Get("Language", "zh-CN");
                        if (config == "en-US")
                            config = "default";
                        args[i] = "Dialog=" + config;
                    }
                    break;
                }
            }

            (new Thread(() => { Thread.Sleep(300); WindBot.Program.Main(args); })).Start();
        }

        public void Launch(string command, bool lockHand, bool noCheck, bool noShuffle)
        {
            port = inputPort.text;
            if (string.IsNullOrEmpty(port) || int.Parse(port) <= 0 || int.Parse(port) > 65535)
            {
                port = "7911";
                inputPort.text = port;
            }

            string lp = inputLP.text;
            if (string.IsNullOrEmpty(lp) /*|| lp == "0"*/)
                lp = "8000";
            string hand = inputHand.text;
            if (string.IsNullOrEmpty(hand) /*|| hand == "0"*/)
                hand = "5";
            string draw = inputDraw.text;
            if (string.IsNullOrEmpty(draw) /*|| draw == "0"*/)
                draw = "5";
            string args = port + " -1 5 0 F " + (noCheck ? "T " : "F ") + (noShuffle ? "T " : "F ") + lp + " " + hand + " " + draw + " 0 0";
            YgoServer.StartServer(args);

            Room.fromSolo = true;
            if (lockHand)
                Room.soloLockHand = true;
            else
                Room.soloLockHand = false;
            Room.fromLocalHost = false;
            (new Thread(() => { Thread.Sleep(200); TcpHelper.Join("127.0.0.1", Config.Get("DuelPlayerName0", "@ui"), port, ""); })).Start();

            StartWindBot(command, "127.0.0.1", port, string.Empty, lockHand);
        }
    }
}

