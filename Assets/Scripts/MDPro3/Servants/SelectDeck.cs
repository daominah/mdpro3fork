using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.UI;
using MDPro3.Net;
using System.Runtime.CompilerServices;
using static MDPro3.YGOSharp.PacksManager;

namespace MDPro3
{
    public class SelectDeck : Servant
    {
        public ScrollRect scrollRect;
        public InputField search;

        SuperScrollView superScrollView;
        public Dictionary<string, Deck> decks = new Dictionary<string, Deck>();
        public List<SuperScrollViewItemForDeckSelect> items;
        public ButtonSwitchForDeckPickup btnPickup;
        public ToggleForDeckDelete btnDelete;
        public GameObject btnOnline;
        public GameObject btnSync;
        public Text title;

        public enum Condition
        {
            ForEdit,
            ForDuel,
            ForSolo,
            MyCard
        }
        public static Condition condition = Condition.ForEdit;
        public void SwitchCondition(Condition condition)
        {
            SelectDeck.condition = condition;
            switch (condition)
            {
                case Condition.ForEdit:
                    returnServant = Program.I().menu;
                    depth = 1;
                    btnOnline.SetActive(true);
                    btnSync.SetActive(false);
                    title.text = InterString.Get("编辑卡组");
                    break;
                case Condition.ForDuel:
                    returnServant = Program.I().room;
                    depth = 3;
                    btnOnline.SetActive(false);
                    btnSync.SetActive(false);
                    title.text = InterString.Get("选择卡组");
                    break;
                case Condition.ForSolo:
                    returnServant = Program.I().solo;
                    depth = 4;
                    btnOnline.SetActive(false);
                    btnSync.SetActive(false);
                    title.text = InterString.Get("选择卡组");
                    break;
                case Condition.MyCard:
                    returnServant = Program.I().online;
                    depth = 2;
                    btnOnline.SetActive(false);
                    btnSync.SetActive(false);
                    title.text = InterString.Get("MyCard卡组");
                    break;
            }
        }

        public override void Initialize()
        {
            haveLine = true;
            SwitchCondition(Condition.ForEdit);
            base.Initialize();
            search.onEndEdit.AddListener(Print);
        }
        public override void OnExit()
        {
            if (Program.exitOnReturn)
                Menu.GameQuit();
            else
                Program.I().ShiftToServant(returnServant);
        }

        public override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            RefreshList();
        }

        public override void ApplyHideArrangement(int preDepth)
        {
            base.ApplyHideArrangement(preDepth);
            Config.Save();
            DOTween.To(v => { }, 0, 0, transitionTime * 0.9f).OnComplete(() =>
            {
                btnPickup.OnSwitchOff();
                if(superScrollView != null)
                    foreach (var item in superScrollView.items)
                    {
                        item.gameObject.transform.SetParent(Program.I().container_2D, false);
                        item.gameObject.GetComponent<SuperScrollViewItemForDeckSelect>().Dispose();
                    }
                Clear();
            });
        }

        public void RefreshList()
        {
            if (!isShowed)
                return;

            Clear();
            btnDelete.SwitchOffWithoutAction();
            btnPickup.OnSwitchOff();

            if(condition == Condition.MyCard)
            {
                if(OnlineDeck.decks == null)
                    decks.Clear();

                foreach (var d in OnlineDeck.decks)
                {
                    if (decks.ContainsKey(d.deckName))
                    {
                        int avoid = 2;
                        while (decks.ContainsKey(d.deckName + $" ({avoid})"))
                            avoid++;
                        d.deckName += $" ({avoid})";
                    }
                    decks.Add(d.deckName, new Deck(d.deckYdk, d.deckId));
                }

                var configDeck = Config.Get("DeckInUse", "");
                if (decks.ContainsKey(configDeck))
                {
                    var deck = decks[configDeck];
                    decks.Remove(configDeck);
                    var newDecks = new Dictionary<string, Deck>();
                    newDecks[configDeck] = deck;
                    foreach(var d in decks)
                        newDecks.Add(d.Key, d.Value);
                    decks = newDecks;
                }
            }
            else
            {
                if (!Directory.Exists(Program.deckPath))
                    Directory.CreateDirectory(Program.deckPath);
                var files = Directory.GetFiles(Program.deckPath, "*.ydk");
                List<string> fileList = files.ToList();
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    fileName = fileName.Substring(0, fileName.Length - 4);
                    if (fileName == Config.Get("DeckInUse", ""))
                    {
                        fileList.Remove(file);
                        fileList.Insert(0, file);
                        break;
                    }
                }
                List<string> list = new List<string>();
                foreach (var deck in fileList)
                {
                    var name = Path.GetFileName(deck);
                    name = name.Substring(0, name.Length - 4);
                    decks.Add(name, new Deck(deck));
                }
            }
            Print(search.text);
        }

        void Clear()
        {
            decks.Clear();
            items.Clear();
        }

        public void Print(string search = "")
        {
            ExitDeleteDeck();

            if (superScrollView != null)
            {
                superScrollView.Clear();
                items.Clear();
            }
            var defau = 1f;
#if UNITY_ANDROID
            defau = 1.5f;
#endif
            var scale = Config.GetFloat("UIScale", defau);

            var handle = Addressables.LoadAssetAsync<GameObject>("DeckOnSelect");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollView
                (
                (int)Math.Floor(scrollRect.content.rect.width / (260 * scale)),
                260 * scale,
                240 * scale,
                0,
                128,
                result.Result,
                ItemOnListRefresh,
                scrollRect
                );
                List<string[]> tasks = new List<string[]>();
                foreach (var deck in decks)
                {
                    if (!deck.Key.Contains(search))
                        continue;
                    var task = new string[8]
                    {
                        deck.Key,
                        deck.Value.Case[0].ToString(),
                        "0", "0", "0",
                        deck.Value.Protector[0].ToString(),
                        "0",//For Delete
                        deck.Value.deckId
                    };
                    if (deck.Value.Pickup.Count > 0)
                        task[2] = deck.Value.Pickup[0].ToString();
                    if (deck.Value.Pickup.Count > 1)
                        task[3] = deck.Value.Pickup[1].ToString();
                    if (deck.Value.Pickup.Count > 2)
                        task[4] = deck.Value.Pickup[2].ToString();
                    tasks.Add(task);
                }
                superScrollView.Print(tasks);
            };
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemForDeckSelect>();
            handler.deckName = task[0];
            handler.deckCase = int.Parse(task[1]);
            handler.card1 = int.Parse(task[2]);
            handler.card2 = int.Parse(task[3]);
            handler.card3 = int.Parse(task[4]);
            handler.protector = task[5];
            handler.selected = task[6] != "0";
            handler.deckId = task[7];
            handler.Refresh();
        }

        public bool hoverOn
        {
            get { return m_hoverOn; }
            set
            {
                m_hoverOn = value;
                DeckHover();
            }
        }
        private bool m_hoverOn = false;
        public void DeckHover()
        {
            foreach (var item in items)
                item.Hover(m_hoverOn);
        }

        public void DeckCreate()
        {
            ExitDeleteDeck();
            var selections = new List<string>()
        {
            InterString.Get("请输入卡组名。@n创建卡组时会自动导入剪切板中的卡组码。"),
            string.Empty
        };
            UIManager.ShowPopupInput(selections, DeckCheck, null, InputValidation.ValidationType.Path);
        }

        void DeckCheck(string deckName)
        {
            var path = Program.deckPath + deckName + Program.ydkExpansion;

            if (File.Exists(path))
            {
                deckInUse = deckName;
                List<string> tasks = new List<string>()
                {
                    InterString.Get("该卡组名已存在"),
                    InterString.Get("该卡组名的文件已存在，是否直接覆盖创建？"),
                    InterString.Get("覆盖"),
                    InterString.Get("取消")
                };
                DOTween.To(v => { }, 0, 0, transitionTime + 0.1f).OnComplete(() =>
                {
                    UIManager.ShowPopupYesOrNo(tasks, DeckFileCreateWithName, null);
                });
            }
            else
                DeckFileCreate(deckName);
        }

        public static string deckInUse;
        void DeckFileCreateWithName()
        {
            DeckFileCreate(deckInUse);
        }

        void DeckFileCreate(string deckName)
        {
            try
            {
                var path = Program.deckPath + deckName + Program.ydkExpansion;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Create(path).Close();

                string clipBoard = GUIUtility.systemCopyBuffer;
                if (clipBoard.Contains("#main"))
                    File.WriteAllText(path!, clipBoard, Encoding.UTF8);
                else if (clipBoard.Contains("ygotype=deck&v=1&d="))
                {
                    var uri = new Uri(clipBoard);
                    var deck = DeckShareURL.UriToDeck(uri);
                    Program.I().editDeck.SaveDeckFile(deck, deckName);
                }
                Config.Set("DeckInUse", deckName);
                RefreshList();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                MessageManager.Cast(InterString.Get("创建卡组失败！请检查文件夹权限。"));
            }
        }

        bool deleting;
        public void DeckDelete()
        {
            if (!deleting)
            {
                deleting = true;
                foreach (var item in items)
                    item.ShowToggle();
            }
            else
            {
                deleting = false;
                int count = 0;
                var toDelete = new List<string>();
                foreach (var item in superScrollView.items)
                    if (item.args[6] != "0")
                    {
                        count++;
                        File.Delete(Program.deckPath + item.args[0] + Program.ydkExpansion);
                        MessageManager.Cast(InterString.Get("已删除卡组「[?]」", item.args[0]));
                        toDelete.Add(item.args[7]);
                    }
                DeleteOnlineDecks(toDelete);
                if (count > 0)
                    RefreshList();
                else
                    ExitDeleteDeck();
            }
        }

        void DeleteOnlineDecks(List<string> ids)
        {
            if (MyCard.account == null)
                return;
            StartCoroutine(DeleteOnlineDecksAsync(ids));
        }

        IEnumerator DeleteOnlineDecksAsync(List<string> ids)
        {
            var task = OnlineDeck.DeleteDecks(ids);
            while(!task.IsCompleted)
                yield return null;

            var task2 = OnlineDeck.GetAllDecks();
            while (!task2.IsCompleted)
                yield return null;
        }

        void ExitDeleteDeck()
        {
            deleting = false;
            if(superScrollView != null)
                foreach(var item in superScrollView.items)
                    item.args[6] = "0";
            foreach (var item in items)
                item.HideToggle();
        }

        public void OnOnlineDeckView()
        {
            Program.I().ShiftToServant(Program.I().onlineDeckViewer);
        }

        public void OnSyncDeck()
        {
            var hint = InterString.Get("本地卡组数量：");
            hint += Tools.GetLocalDeckCount() + " ";

            hint += InterString.Get("本地卡组最后编辑时间：");
            hint += Tools.GetLocalDeckLastEditTime() + "\r\n";

            hint += InterString.Get("云端卡组数量：");
            hint += OnlineDeck.decks.Length + " ";

            hint += InterString.Get("云端卡组最后编辑时间：");
            hint += OnlineDeck.GetDeckLastEditTime();

            List<string> selections = new List<string>
                {
                    InterString.Get("同步卡组"),
                    hint,
                    InterString.Get("本地至云端"),
                    InterString.Get("云端至本地")
                };
            UIManager.ShowPopupYesOrNoOrCancel(selections, SyncDecksFromLocalToServer, SyncDecksFromServerToLocal);
        }

        void SyncDecksFromLocalToServer()
        {
        }

        void SyncDecksFromServerToLocal()
        {
        }
    }
}
