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

namespace MDPro3
{
    public class SelectDeck : Servant
    {
        public ScrollRect scrollRect;
        public InputField search;

        SuperScrollView superScrollView;
        public Dictionary<string, Deck> decks = new Dictionary<string, Deck>();
        public List<SuperScrollViewItemForDeck> items;
        public ButtonSwitchForDeckPickup btnPickup;
        public ToggleForDeckDelete btnDelete;

        public enum State
        {
            ForEdit,
            ForDuel,
            ForSolo
        }
        public static State state = State.ForEdit;
        public override void Initialize()
        {
            depth = 1;
            haveLine = true;
            returnServant = Program.I().menu;
            base.Initialize();
            search.onEndEdit.AddListener(Print);
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
            DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
            {
                btnPickup.OnSwitchOff();
                superScrollView.Clear();
                Clear();
                depth = 1;
                state = State.ForEdit;
                returnServant = Program.I().menu;
            });
        }

        public void RefreshList()
        {
            Clear();
            btnDelete.SwitchOffWithoutAction();
            btnPickup.OnSwitchOff();
            if (!Directory.Exists("Deck"))
                Directory.CreateDirectory("Deck");
            var files = Directory.GetFiles("Deck", "*.ydk");
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
            var defau = 1000f;
#if UNITY_ANDROID
            defau = 1500f;
#endif
            var scale = float.Parse(Config.Get("UIScale", defau.ToString())) / 1000;

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
                    var task = new string[6]
                    {
                deck.Key,
                deck.Value.Case[0].ToString(),
                "0", "0", "0",
                deck.Value.Protector[0].ToString()
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
            var handler = item.GetComponent<SuperScrollViewItemForDeck>();
            handler.deckName = task[0];
            handler.deckCase = int.Parse(task[1]);
            handler.card1 = int.Parse(task[2]);
            handler.card2 = int.Parse(task[3]);
            handler.card3 = int.Parse(task[4]);
            handler.protector = task[5];
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
            var path = $"Deck/{deckName}.ydk";

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
                var path = $"Deck/{deckName}.ydk";
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
                Program.I().selectDeck.RefreshList();
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
                foreach (var item in items)
                    if (item.selected)
                    {
                        count++;
                        File.Delete("Deck/" + item.deckName + ".ydk");
                        MessageManager.Cast(InterString.Get("已删除卡组「[?]」", item.deckName));
                    }
                if (count > 0)
                    RefreshList();
                else
                {
                    ExitDeleteDeck();
                }
            }
        }

        void ExitDeleteDeck()
        {
            deleting = false;
            foreach (var item in items)
                item.HideToggle();
        }
    }
}
