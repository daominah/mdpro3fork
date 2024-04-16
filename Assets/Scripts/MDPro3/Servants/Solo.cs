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
        public class BotInfo
        {
            public string name;
            public string command;
            public string desc;
            public string[] flags;
            public int main0;
        }
        private IList<BotInfo> bots = new List<BotInfo>();

        public override void Initialize()
        {
            depth = 1;
            haveLine = true;
            returnServant = Program.I().menu;
            base.Initialize();
            btnDeck.transform.GetChild(0).GetComponent<Text>().text = Config.Get("DeckInUse", "@ui");
            btnDeck.SetActive(false);
            Load();
        }

        public void Load()
        {
            ReadBots(Program.localesPath + Program.slash + Config.Get("Language", "zh-CN") + "/bot.conf");
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
                        if(File.Exists("Data/WindBot/Decks/Ai_" + deckName + ".ydk")) 
                        {
                            aiDeck = new Deck("Data/WindBot/Decks/Ai_" + deckName + ".ydk");
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
            SelectDeck.state = SelectDeck.State.ForSolo;
            Program.I().selectDeck.depth = 3;
            Program.I().selectDeck.returnServant = this;
            Program.I().ShiftToServant(Program.I().selectDeck);
        }

        public void StartAI(int aiCode)
        {
            BotInfo bot = bots[aiCode];
            string aiCommand = bot.command;
            if (aiCode == 4)
            {
                string selectedDeck = btnDeck.transform.GetChild(0).GetComponent<Text>().text;
                if (!File.Exists("Deck/" + selectedDeck + ".ydk"))
                {
                    MessageManager.Cast(InterString.Get("请先为AI选择有效的卡组。"));
                    return;
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
            Launch(aiCommand, toggleLockHand.isOn, toggleNoCheck.isOn, toggleNoShuffle.isOn);
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

        public void Launch(string command, bool lockHand, bool noCheck, bool noShuffle)
        {
            command = command.Replace("'", "\"");
            if(lockHand)
                command += " Hand=1";
            command += " Host=127.0.0.1";

            string port = inputPort.text;
            if (string.IsNullOrEmpty(port) || port == "0")
                port = "7911";
            command += " Port=" + port;

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
            (new Thread(() => { Thread.Sleep(200); TcpHelper.Join("127.0.0.1", Config.Get("DuelPlayerName0", "@ui"), "7911", "", ""); })).Start();
            (new Thread(() => { Thread.Sleep(300); WindBot.Program.Main(Tools.SplitWithPreservedQuotes(command)); })).Start();
        }
    }
}

