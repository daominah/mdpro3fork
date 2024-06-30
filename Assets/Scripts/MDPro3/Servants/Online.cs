using System;
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

namespace MDPro3.Net
{
    public class Online : Servant
    {
        public GameObject goLegacy;
        public GameObject goLocal;
        public GameObject goMyCard;
        public ButtonList defaultFunctionList;

        public InputField inputName;
        public InputField inputHost;
        public InputField inputPort;
        public InputField inputPassword;
        public ScrollRect scrollView;
        SuperScrollView superScrollView;

        [Header("Local Server")]
        public Text textLflist;
        public Text textPool;
        public Text textMode;
        public UI.Toggle toggleNoCheck;
        public UI.Toggle toggleNoShuffle;
        public InputField inputTime;
        public InputField inputLP;
        public InputField inputHand;
        public InputField inputDraw;

        [Header("My Card")]
        public GameObject goMyCardLogin;
        public GameObject goMyCardFunctions;

        public InputField inputMyCardAccount;
        public InputField inputMyCardPassword;
        public RawImage rawImageMyCardAvatar;

        public Image iconRankBG;
        public Image iconRankIcon;
        public Image iconRankTier;
        public Image iconRankTier2;
        public Image iconRankTier3;

        public Text textUserName;
        public Text textExp;
        public Text textDP;
        public Text textAWin;
        public Text textARatio;
        public Text textARank;
        public Text textECount;
        public Text textERank;
        public Text textEntertain;
        public Text textAthletic;


        public DeckSelector deckSelector;
        public WatchListHandler watchListHandler;

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
            defaultFunctionList.SelectThis();
            LoadHostAddress();
            TryTokenin();
        }

        public override void Show(int preDepth)
        {
            if(Program.exitOnReturn)
                Menu.GameQuit();
            else
                base.Show(preDepth);
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
                    address.password = string.Empty;
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
            RefreshDeckSelector();
            StartCoroutine(RefreshMyCardAssets());
        }

        public override void ApplyHideArrangement(int preDepth)
        {
            base.ApplyHideArrangement(preDepth);
            Config.Save();
            Save();
            MyCard.CloseAthleticWatchListWebSocket();
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
            (new Thread(() => { Thread.Sleep(200); TcpHelper.Join("127.0.0.1", Config.Get("DuelPlayerName0", "@ui"), "7911", ""); })).Start();
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
            KF_OnlineGame(inputName.text, inputHost.text, inputPort.text, inputPassword.text);
        }
        public void KF_OnlineGame(string name, string ip, string port, string password)
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
            new Thread(() => { TcpHelper.Join(ip, Config.Get("DuelPlayerName0", "@ui"), port, password); }).Start();
        }

        public void SwitchFunction(int id)
        {
            goLegacy.SetActive(false);
            goLocal.SetActive(false);
            goMyCard.SetActive(false);

            switch (id)
            {
                case 0:
                    goLegacy.SetActive(true); 
                    break;
                case 1:
                    goLocal.SetActive(true);
                    break;
                case 2:
                    goMyCard.SetActive(true);
                    break;
            }
        }

        #region Local Host

        public void LocalHostInitialize()
        {
            textLflist.text = BanlistManager.Banlists[0].Name;
            textPool.text = StringHelper.GetUnsafe(1481);
            textMode.text = StringHelper.GetUnsafe(1244);
            toggleNoCheck.SwitchOff();
            toggleNoShuffle.SwitchOff();
            inputTime.text = "180";
            inputLP.text = "8000";
            inputHand.text = "5";
            inputDraw.text = "1";
        }

        public void OnLocalHostCreate()
        {
            serverSelections = GetSelections();
            CreateServer();
        }

        List<string> GetSelections()
        {
            return new List<string>()
            {
                InterString.Get("创建主机"),
                textLflist.text,
                textPool.text,
                textMode.text,
                toggleNoCheck.switchOn ? "T" : "F",
                toggleNoShuffle.switchOn ? "T" : "F",
                inputTime.text == "" ? "0" : inputTime.text,
                inputLP.text == "" ? "8000" : inputLP.text,
                inputHand.text == "" ? "5" : inputHand.text,
                inputDraw.text == "" ? "1" : inputDraw.text
            };
        }

        public void OnLflist()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("禁限卡表")
            };
            foreach (var list in BanlistManager.Banlists)
                selections.Add(list.Name);
            UIManager.ShowPopupSelection(selections, ChangeBanlist, null);
        }
        void ChangeBanlist()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            textLflist.text = selected;
        }

        public void OnPool()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("卡片允许")
            };
            for (int i = 1481; i < 1487; i++)
                selections.Add(StringHelper.GetUnsafe(i));
            UIManager.ShowPopupSelection(selections, ChangePool, null);
        }
        void ChangePool()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            textPool.text = selected;
        }
        public void OnMode()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("决斗模式")
            };
            for (int i = 1244; i < 1247; i++)
                selections.Add(StringHelper.GetUnsafe(i));
            UIManager.ShowPopupSelection(selections, ChangeMode, null);
        }
        void ChangeMode()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            textMode.text = selected;
        }


        #endregion


        #region MyCard
        public void OnMyCardRegister()
        {
            Application.OpenURL("https://accounts.moecube.com/signup");
        }

        public void OnMyCardLogin()
        {
            if (string.IsNullOrEmpty(inputMyCardAccount.text))
            {
                MessageManager.Cast(InterString.Get("账号不能为空"));
                return;
            }
            if (string.IsNullOrEmpty(inputMyCardPassword.text) || inputMyCardPassword.text.Length < 6)
            {
                MessageManager.Cast(InterString.Get("密码不能少于6位"));
                return;
            }

            StartCoroutine(MyCardLoginAsync());
        }

        IEnumerator MyCardLoginAsync()
        {
            goMyCardLogin.SetActive(false);
            var task = MyCard.Login(inputMyCardAccount.text, inputMyCardPassword.text);
            while(!task.IsCompleted)
                yield return null;
            if (task.Result.user.id == 0)
            {
                MessageManager.Cast(InterString.Get("登录失败：") + task.Result.user.username);
                goMyCardLogin.SetActive(true);
                yield break;
            }
            Config.Set("MyCardToken", task.Result.token);
            Config.Save();
            DoWhenLoginSuccess();
        }

        void TryTokenin()
        {
            StartCoroutine(TryTokenInAsync());
        }

        IEnumerator TryTokenInAsync()
        {
            var token = Config.Get("MyCardToken", Config.stringNo);
            if(token == Config.stringNo)
            {
                goMyCardLogin.SetActive(true);
                goMyCardFunctions.SetActive(false);
                yield break;
            }
            var task = MyCard.TokenIn(token);
            while (!task.IsCompleted)
                yield return null;

            if (task.Result.user.id == 0)
            {
                Debug.Log("TokenIn Failed.");
                goMyCardLogin.SetActive(true);
                goMyCardFunctions.SetActive(false);
                yield break;
            }
            DoWhenLoginSuccess();
        }

        void DoWhenLoginSuccess()
        {
            goMyCardLogin.SetActive(false);
            MyCard.ConnectToAthleticWatchListWebSocket();
            StartCoroutine(RefreshMyCardAssets());
            StartCoroutine(SyncDecks());
        }

        IEnumerator RefreshMyCardAssets()
        {
            if (MyCard.account == null || MyCard.account.user == null)
                yield break;

            textUserName.text = MyCard.account.user.username;


            var task = MyCard.GetExp();
            while (!task.IsCompleted)
                yield return null;
            textExp.text = task.Result.exp.ToString();
            textDP.text = task.Result.pt.ToString();
            textAWin.text = task.Result.athletic_win.ToString();
            textARatio.text = task.Result.athletic_wl_ratio + "%";
            textARank.text = task.Result.arena_rank.ToString();
            textECount.text = task.Result.entertain_all.ToString();
            textERank.text = task.Result.exp_rank.ToString();

            while (!Appearance.loaded)
                yield return null;
            rawImageMyCardAvatar.material = Appearance.duelFrameMat0;

            if (MyCard.avatar == null)
            {
                var avatarTask = Tools.DownloadImageAsync(MyCard.account.user.avatar);
                while (!avatarTask.IsCompleted)
                    yield return null;
                MyCard.avatar = avatarTask.Result;
                rawImageMyCardAvatar.texture = MyCard.avatar;
            }

            goMyCardFunctions.SetActive(true);

            while (TextureManager.container == null)
                yield return null;

            var rankSprites = TextureManager.container.GetRankSprites(task.Result.pt);
            iconRankBG.sprite = rankSprites[0];
            iconRankIcon.sprite = rankSprites[1];
            iconRankTier.sprite = rankSprites[2];
            iconRankTier2.sprite = rankSprites[3];
            iconRankTier3.sprite = rankSprites[4];
        }

        void RefreshDeckSelector()
        {
            if (OnlineDeck.decks == null || OnlineDeck.decks.Length == 0)
            {
                deckSelector.SetDeck(null, InterString.Get("未选中有效卡组"));
                return;
            }

            var configDeck = Config.Get("DeckInUse", "@ui");
            bool found = false;
            foreach (var deck in OnlineDeck.decks)
            {
                if (deck.deckName == configDeck)
                {
                    found = true;
                    deckSelector.SetDeck(new Deck(deck.deckYdk, deck.deckId, deck.deckId, deck.userid.ToString()), deck.deckName);
                    break;
                }
            }
            if (!found)
            {
                deckSelector.SetDeck(null, InterString.Get("未选中有效卡组"));
            }
        }

        public void OnExitLogin()
        {
            List<string> tasks = new List<string>()
            {
                InterString.Get("退出登录"),
                InterString.Get("是否确认退出登录？"),
                InterString.Get("确认"),
                InterString.Get("取消")
            };
            UIManager.ShowPopupYesOrNo(tasks, ExitLogin, null);
        }

        void ExitLogin()
        {
            Config.Set("MyCardToken", Config.stringNo);
            Config.Save();
            MyCard.account = null;
            goMyCardFunctions.SetActive(false);
            goMyCardLogin.SetActive(true);
            inputMyCardAccount.text = string.Empty;
            inputMyCardPassword.text = string.Empty;
        }

        public void OnDeckSelect()
        {
            Program.I().selectDeck.SwitchCondition(SelectDeck.Condition.MyCard);
            Program.I().ShiftToServant(Program.I().selectDeck);
        }

        public void OnEntertainMatch()
        {
            if(entertainMatch == null)
            {
                StartCoroutine(entertainMatch = EntertainMatchAsync());
                if(athleticMatch != null)
                {
                    StopCoroutine(athleticMatch);
                    athleticMatch = null;
                }
                textAthletic.text = InterString.Get("竞技匹配");
            }
            else
            {
                textEntertain.text = InterString.Get("娱乐匹配");
                StopCoroutine(entertainMatch);
                entertainMatch = null;
            }
        }
        public void OnAthleticMatch()
        {
            if (athleticMatch == null)
            {
                StartCoroutine(athleticMatch = AthleticMatchAsync());
                if (entertainMatch != null)
                {
                    StopCoroutine(entertainMatch);
                    entertainMatch = null;
                }
                textEntertain.text = InterString.Get("娱乐匹配");
            }
            else
            {
                textAthletic.text = InterString.Get("竞技匹配");
                StopCoroutine(athleticMatch);
                athleticMatch = null;
            }
        }

        public static IEnumerator entertainMatch;
        public static IEnumerator athleticMatch;

        IEnumerator EntertainMatchAsync()
        {
            var task = MyCard.GetMatchInfo("entertain");
            var startTime = DateTime.Now;

            while(!task.IsCompleted)
            {
                var elapsedTimeInSeconds = (DateTime.Now - startTime).TotalSeconds;
                int minutes = (int)Math.Floor(elapsedTimeInSeconds / 60);
                int seconds = (int)(elapsedTimeInSeconds % 60);
                textEntertain.text = $"{minutes:D2}:{seconds:D2}";
                yield return new WaitForSeconds(0.5f);
            }

            if(task.Result != null)
            {
                textEntertain.text = InterString.Get("娱乐匹配");
                (new Thread(() => { TcpHelper.Join(task.Result.address, MyCard.account.user.username, task.Result.port.ToString(), task.Result.password); })).Start();
            }
            else
            {
                textEntertain.text = InterString.Get("匹配失败");
            }
            entertainMatch = null;
        }
        IEnumerator AthleticMatchAsync()
        {
            var task = MyCard.GetMatchInfo("athletic");
            var startTime = DateTime.Now;

            while (!task.IsCompleted)
            {
                var elapsedTimeInSeconds = (DateTime.Now - startTime).TotalSeconds;
                int minutes = (int)Math.Floor(elapsedTimeInSeconds / 60);
                int seconds = (int)(elapsedTimeInSeconds % 60);
                textAthletic.text = $"{minutes:D2}:{seconds:D2}";
                yield return new WaitForSeconds(0.5f);
            }

            if (task.Result != null)
            {
                textAthletic.text = InterString.Get("竞技匹配");
                (new Thread(() => { TcpHelper.Join(task.Result.address, MyCard.account.user.username, task.Result.port.ToString(), task.Result.password); })).Start();
            }
            else
            {
                textAthletic.text = InterString.Get("匹配失败");
            }
            athleticMatch = null;
        }

        IEnumerator SyncDecks()
        {
            if(OnlineDeck.decks == null)
            {
                MessageManager.Cast(InterString.Get("同步卡组失败。"));
                yield break;
            }

            var deckFiles = Directory.GetFiles(Program.deckPath, "*.ydk");
            var deckList = new List<Deck>();
            foreach(var deck in deckFiles)
                deckList.Add(new Deck(deck));

            var decksNeedUpload = new Dictionary<string, Deck>();//没在服务器找到对应的deckId的本地卡组
            var decksNeedUpdate= new Dictionary<string, Deck>();//找到deckId但本地时间大于服务器时间五秒以上的卡组
            var decksNeedUpdate2 = new Dictionary<string, Deck>();//找到deckId但本地时间小于服务器时间五秒以上的卡组
            var localDeckIds = new List<string>();
            for (int i = 0; i < deckList.Count; i++)
            {
                var deckName = Path.GetFileNameWithoutExtension(deckFiles[i]);
                if(OnlineDeck.StringIsIdFormat(deckList[i].deckId))
                    localDeckIds.Add(deckList[i].deckId);
                if (deckList[i].userId != MyCard.account.user.id.ToString())
                {
                    decksNeedUpload.Add(deckName, deckList[i]);
                    continue;
                }

                bool deckIdFound = false;
                foreach(var od in OnlineDeck.decks)
                    if (od.deckId == deckList[i].deckId)
                    {
                        deckIdFound = true;
                        var fileInfo = new FileInfo(deckFiles[i]);
                        DateTime serverTime;
                        try
                        {
                            serverTime = DateTime.Parse(od.deckUpdateDate);
                        }
                        catch
                        {
                            serverTime = DateTime.Parse(od.deckUploadDate);
                        }
                        var diff = serverTime - fileInfo.LastWriteTime;
                        if (diff.TotalSeconds > 5f)
                        {
                            if(fileInfo.LastWriteTime > serverTime)
                                decksNeedUpdate.Add(deckName, deckList[i]);
                            else
                                decksNeedUpdate2.Add(deckName, deckList[i]);
                        }
                    }

                if (!deckIdFound)
                    decksNeedUpload.Add(deckName, deckList[i]);
            }



            //上传已经有Id的本地较新卡组
            foreach (var deck in decksNeedUpdate)
            {
                var fileInfo = new FileInfo(Program.deckPath + deck.Key + Program.ydkExpansion);
                fileInfo.LastWriteTime = DateTime.Now;
                
                var ydk = EditDeck.FromDeckToYDK(deck.Value);
                var task = OnlineDeck.SyncDeck(deck.Value.deckId, deck.Key, ydk, false);
                while (!task.IsCompleted)
                    yield return null;
            }
            //下载已经有Id的本地较旧卡组
            foreach (var deck in decksNeedUpdate2)
            {
                var od = OnlineDeck.GetDeck(deck.Value.deckId);
                while(!od.IsCompleted) 
                    yield return null;
                File.WriteAllText(Program.deckPath + deck.Key + Program.ydkExpansion, od.Result.deckYdk);
                var fileInfo = new FileInfo(Program.deckPath + deck.Key + Program.ydkExpansion);
                fileInfo.LastWriteTime = DateTime.Parse(od.Result.deckUpdateDate);
            }

            //上传没有Id的本地卡组
            var decks = new List<Deck>();
            var deckNames = new List<string>();
            foreach(var deck in decksNeedUpload)
            {
                deckNames.Add(deck.Key);
                decks.Add(deck.Value);
            }
            var task2 = OnlineDeck.SyncDecks(decks, deckNames);
            while (!task2.IsCompleted)
                yield return null;

            //下载本地ID不存在的服务器卡组
            List<OnlineDeck.OnlineDeckData> odtd = new List<OnlineDeck.OnlineDeckData>();
            foreach(var od in OnlineDeck.decks)
                if (!localDeckIds.Contains(od.deckId))
                    odtd.Add(od);
            foreach(var deck in odtd)
            {
                var d = new Deck(deck.deckYdk, Deck.defaultDeckAuthor);
                d.userId = MyCard.account.user.id.ToString();
                d.deckId = deck.deckId;
                var ydk = EditDeck.FromDeckToYDK(d);
                int avoid = 2;
                string tail = string.Empty;
                while(File.Exists(Program.deckPath + deck.deckName + tail + Program.ydkExpansion))
                {
                    tail = $" ({avoid})";
                    avoid++;
                }

                File.WriteAllText(Program.deckPath + deck.deckName + tail + Program.ydkExpansion, ydk);

                var info = new FileInfo(Program.deckPath + deck.deckName + Program.ydkExpansion);
                try
                {
                    info.LastWriteTime = DateTime.Parse(deck.deckUpdateDate);
                }
                catch
                {
                    Debug.Log("ERROR Update Date: " + deck.deckUpdateDate);
                    info.LastWriteTime = DateTime.Parse(deck.deckUploadDate);
                }
            }
        }

        public void SetWatchRooms(List<MyCardRoom> rooms)
        {
            watchListHandler.SetRooms(rooms);
        }
        public void CreateWatchRoom(MyCardRoom room)
        {
            watchListHandler.CreateRoom(room);
        }
        public void UpdateWatchRoom(MyCardRoom room)
        {
            watchListHandler.UpdateRoom(room);
        }
        public void DeleteWatchRoom(string roomId)
        {
            watchListHandler.DeleteRoom(roomId);
        }
        public void ClearWatchList()
        {
            watchListHandler.Clear();
        }

        #endregion
    }
}
