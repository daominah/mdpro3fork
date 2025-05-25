using System.CodeDom;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.Servant;
using static MDPro3.Servant.DeckSelector;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;
using MDPro3.Duel.YGOSharp;
using DG.Tweening;
using MDPro3.Net;
using System.Text;
using System;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Linq;
using MDPro3.UI.PropertyOverride;

namespace MDPro3.UI.ServantUI
{
    public class DeckSelectorUI : ServantUI
    {

        #region Elements

        private const string LABEL_SR = "ScrollRect";
        private ScrollRect m_ScrollRect;
        private ScrollRect ScrollRect =>
            m_ScrollRect = m_ScrollRect != null ? m_ScrollRect
            : Manager.GetElement<ScrollRect>(LABEL_SR);

        private const string LABEL_IPT = "InputField";
        private TMP_InputField m_Input;
        private TMP_InputField Input =>
            m_Input = m_Input != null ? m_Input
            : Manager.GetElement<TMP_InputField>(LABEL_IPT);

        private const string LABEL_TXT_DECKNUMVALUE = "TextDeckNumValue";
        private TextMeshProUGUI m_TextDeckNumValue;
        private TextMeshProUGUI TextDeckNumValue =>
            m_TextDeckNumValue = m_TextDeckNumValue != null ? m_TextDeckNumValue
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_DECKNUMVALUE);

        private const string LABEL_STG_PICKUPCARD = "TogglePickupCard";
        private SelectionToggle m_TogglePickupCard;
        public SelectionToggle TogglePickupCard =>
            m_TogglePickupCard = m_TogglePickupCard != null ? m_TogglePickupCard
            : Manager.GetElement<SelectionToggle>(LABEL_STG_PICKUPCARD);

        private const string LABEL_SBN_ONLINE = "ButtonOnline";
        private SelectionButton m_ButtonOnline;
        public SelectionButton ButtonOnline =>
            m_ButtonOnline = m_ButtonOnline != null ? m_ButtonOnline
            : Manager.GetElement<SelectionButton>(LABEL_SBN_ONLINE);

        private const string LABEL_RT_HEADER = "Header";
        private RectTransform m_Header;
        private RectTransform Header => 
            m_Header = m_Header != null ? m_Header
            : Manager.GetElement<RectTransform>(LABEL_RT_HEADER);

        private const string LABEL_RT_FOOTER = "Footer";
        private RectTransform m_Footer;
        private RectTransform Footer =>
            m_Footer = m_Footer != null ? m_Footer
            : Manager.GetElement<RectTransform>(LABEL_RT_FOOTER);

        private const string LABEL_SBN_DELETE = "ButtonDelete";
        private SelectionButton m_ButtonDelete;
        private SelectionButton ButtonDelete =>
            m_ButtonDelete = m_ButtonDelete != null ? m_ButtonDelete
            : Manager.GetElement<SelectionButton>(LABEL_SBN_DELETE);

        private const string LABEL_SBN_DELETECANCEL = "ButtonDeleteCancel";
        private SelectionButton m_ButtonDeleteCancel;
        private SelectionButton ButtonDeleteCancel =>
            m_ButtonDeleteCancel = m_ButtonDeleteCancel != null ? m_ButtonDeleteCancel
            : Manager.GetElement<SelectionButton>(LABEL_SBN_DELETECANCEL);

        private const string LABEL_SBN_DELETECONFIRM = "ButtonDeleteConfirm";
        private SelectionButton m_ButtonDeleteConfirm;
        private SelectionButton ButtonDeleteConfirm =>
            m_ButtonDeleteConfirm = m_ButtonDeleteConfirm != null ? m_ButtonDeleteConfirm
            : Manager.GetElement<SelectionButton>(LABEL_SBN_DELETECONFIRM);

        #endregion

        public SuperScrollView superScrollView;
        public Dictionary<string, Deck> decks = new();
        public bool buttonLayoutSwitching;

        public override void ShowEvent()
        {
            base.ShowEvent();

            switch (condition)
            {
                case Condition.ForEdit:
                    ButtonOnline.gameObject.SetActive(true);
                    Title.text = InterString.Get("编辑卡组");
                    break;
                case Condition.ForDuel:
                    ButtonOnline.gameObject.SetActive(false);
                    Title.text = InterString.Get("选择卡组");
                    break;
                case Condition.ForSolo:
                    ButtonOnline.gameObject.SetActive(false);
                    Title.text = InterString.Get("选择卡组");
                    break;
                case Condition.MyCard:
                    ButtonOnline.gameObject.SetActive(false);
                    Title.text = InterString.Get("选择卡组");
                    break;
            }

            ShowDefaultButtons();
        }

        public override void AfterShowEvent()
        {
            base.AfterShowEvent();

            RefreshList();
        }

        protected override void AfterHideEvent()
        {
            base.AfterHideEvent();

            Config.Save();
            TogglePickupCard.SetToggleOff();
            superScrollView.Clear();
        }

        public void RefreshList()
        {
            decks.Clear();
            ShowDefaultButtons();
            TogglePickupCard.SetToggleOff();

            if (!Directory.Exists(Program.PATH_DECK))
                Directory.CreateDirectory(Program.PATH_DECK);
            var files = Directory.GetFiles(Program.PATH_DECK, "*.ydk");
            List<string> fileList = files.ToList();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                fileName = fileName.Substring(0, fileName.Length - 4);
                if (fileName == Config.GetConfigDeckName())
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

            Print(Input.text);
        }

        public void ActivateInputField()
        {
            Input.ActivateInputField();
        }

        public void Print(string search = "")
        {
            ExitDeleteDeck();

            superScrollView?.Clear();

            var handle = Addressables.LoadAssetAsync<GameObject>("UI/ItemDeck.prefab");
            handle.Completed += (result) =>
            {
                var itemWidth = PropertyOverrider.NeedMobileLayout() ? 336f : 260f;
                var itemHeight = PropertyOverrider.NeedMobileLayout() ? 300f : 232f;
                var space = PropertyOverrider.NeedMobileLayout() ? 30f : 24f;
                var bottomPadding = (PropertyOverrider.NeedMobileLayout() ? 196f : 150f) - space;
                superScrollView = new SuperScrollView(
                    -1,
                    itemWidth + space,
                    itemHeight + space,
                    10,
                    bottomPadding,
                    result.Result,
                    ItemOnListRefresh,
                    Manager.GetElement<ScrollRect>("ScrollRect"));
                List<string[]> tasks = new() { new string[7] { string.Empty, "0", "0", "0", "0", "0", "0" } };
                foreach (var deck in decks)
                {
                    if (!deck.Key.Contains(search))
                        continue;
                    var task = new string[8]
                    {
                        deck.Key,
                        deck.Value.Case.ToString(),
                        "0", "0", "0",
                        deck.Value.Protector.ToString(),
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
                Program.instance.deckSelector.lastSelectedDeckItem = superScrollView.items[0].gameObject.GetComponent<SelectionToggle_Deck>();
                if (Cursor.lockState == CursorLockMode.Locked)
                    Program.instance.deckSelector.Select();
                UpdateDeckNum();
            };
        }

        private void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SelectionToggle_Deck>();
            handler.deckName = task[0];
            handler.deckCase = int.Parse(task[1]);
            handler.card0 = int.Parse(task[2]);
            handler.card1 = int.Parse(task[3]);
            handler.card2 = int.Parse(task[4]);
            handler.protector = task[5];
            handler.isOn = task[6] != "0";
            handler.Refresh();
        }

        public bool PickupShowing
        {
            get { return m_pickupShowing; }
            set
            {
                m_pickupShowing = value;
                DeckHover();
            }
        }
        private bool m_pickupShowing = false;
        public void DeckHover()
        {
            if (superScrollView == null)
                return;

            foreach (var item in superScrollView.items)
            {
                if (item.gameObject == null)
                    continue;
                var handler = item.gameObject.GetComponent<SelectionToggle_Deck>();

                if (PickupShowing)
                    handler.ShowPickup(true);
                else
                    handler.HidePickup(true);
            }
        }

        public void DeckCreate()
        {
            ExitDeleteDeck();
            var selections = new List<string>()
            {
                InterString.Get("请输入卡组名。@n创建卡组时会自动导入剪切板中的卡组码。"),
                string.Empty
            };
            UIManager.ShowPopupInput(selections, DeckCheck, null, TmpInputValidation.ValidationType.Path);
        }

        private void DeckCheck(string deckName)
        {
            var path = Program.PATH_DECK + deckName + Program.EXPANSION_YDK;

            if (File.Exists(path))
            {
                deckInUse = deckName;
                List<string> tasks = new()
                {
                    InterString.Get("该卡组名已存在"),
                    InterString.Get("该卡组名的文件已存在，是否直接覆盖创建？"),
                    InterString.Get("覆盖"),
                    InterString.Get("取消")
                };
                DOTween.To(v => { }, 0, 0, 0.6f).OnComplete(() =>
                {
                    UIManager.ShowPopupYesOrNo(tasks, DeckFileCreateWithName, null);
                });
            }
            else
                DeckFileCreate(deckName);
        }

        public static string deckInUse;
        private void DeckFileCreateWithName()
        {
            DeckFileCreate(deckInUse);
        }

        private void DeckFileCreate(string deckName)
        {
            try
            {
                var path = Program.PATH_DECK + deckName + Program.EXPANSION_YDK;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Create(path).Close();

                string clipBoard = GUIUtility.systemCopyBuffer;
                if (clipBoard.Contains("#main"))
                    File.WriteAllText(path!, clipBoard, Encoding.UTF8);
                else if (clipBoard.Contains("ygotype=deck&v=1&d="))
                {
                    var uri = new Uri(clipBoard);
                    var deck = DeckShareURL.UriToDeck(uri);
                    deck.Save(deckName, DateTime.Now);
                }
                else if (clipBoard.Contains(YdkeConverter.ydkeHeader))
                {
                    var deck = YdkeConverter.Ydke2Deck(clipBoard);
                    deck.Save(deckName, DateTime.Now);
                }
                Config.SetConfigDeck(deckName);
                RefreshList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                MessageManager.Cast(InterString.Get("创建卡组失败！请检查文件夹权限。"));
            }
        }

        private void DeleteOnlineDecks(List<string> ids)
        {
            if (MyCard.account == null)
                return;
            _ = OnlineDeck.DeleteDecks(ids);
        }

        public void OnOnlineDeckView()
        {
            Program.instance.ShiftToServant(Program.instance.onlineDeckViewer);
        }

        public void OnShowPickup()
        {
            PickupShowing = true;
        }

        public void OnHidePickup()
        {
            PickupShowing = false;
        }


        #region Delete Deck

        public void OnDelete()
        {
            if (buttonLayoutSwitching) return;
            SwitchButtonLayouts(false);
        }

        public void OnDeleteCancel()
        {
            if (buttonLayoutSwitching) return;
            SwitchButtonLayouts(true);
            foreach (var item in superScrollView.items)
            {
                if (item.gameObject == null)
                    continue;
                item.gameObject.GetComponent<SelectionToggle_Deck>().HideToggle();
            }
        }

        public void OnDeleteConfirm()
        {
            if (buttonLayoutSwitching) return;

            var toDeleteIndex = new List<int>();
            var toDeleteIds = new List<string>();
            for (int i = 0; i < superScrollView.items.Count; i++)
                if (superScrollView.items[i].args[6] != "0")
                {
                    File.Delete(Program.PATH_DECK + superScrollView.items[i].args[0] + Program.EXPANSION_YDK);
                    toDeleteIndex.Add(i);
                    toDeleteIds.Add(superScrollView.items[i].args[7]);
                }

            var lastSelect = Program.instance.deckSelector.lastSelectedDeckItem.index;
            int removedCount = 0;
            for (int i = 0; i < toDeleteIndex.Count; i++)
            {
                superScrollView.RemoveAt(toDeleteIndex[i] - removedCount);
                removedCount++;
            }
            Program.instance.deckSelector.lastSelectedDeckItem = (SelectionToggle_Deck)superScrollView.GetItemByIndex(lastSelect);
            if (Cursor.lockState == CursorLockMode.Locked)
                Program.instance.deckSelector.Select();
            DeleteOnlineDecks(toDeleteIds);

            ExitDeleteDeck(true);
            UpdateDeckNum();
        }

        private void ExitDeleteDeck(bool needSwitch = false)
        {
            if (superScrollView == null || superScrollView.items == null)
                return;

            foreach (var item in superScrollView.items)
                item.args[6] = "0";
            foreach (var item in superScrollView.items)
            {
                if (item.gameObject == null)
                    continue;
                item.gameObject.GetComponent<SelectionToggle_Deck>().HideToggle();
            }

            buttonLayoutSwitching = true;

            if (needSwitch)
            {
                var header = Manager.GetElement<RectTransform>("Header");
                var footer = Manager.GetElement<RectTransform>("Footer");
                UIManager.HideExitButton(0.2f);

                DOTween.Sequence()
                    .Append(header.DOAnchorPosY(PropertyOverrider.NeedMobileLayout() ? 130f : 120f, 0.2f).OnComplete(() =>
                    {
                        ShowDefaultButtons();
                        UIManager.ShowExitButton(0.3f, Ease.OutQuart);
                    }))
                    .Append(header.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutQuart));

                DOTween.Sequence()
                    .Append(footer.DOAnchorPosY(PropertyOverrider.NeedMobileLayout() ? -186f : -140f, 0.2f))
                    .Append(footer.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutQuart)).OnComplete(() =>
                    {
                        buttonLayoutSwitching = false;
                    });
            }
            else
            {
                ShowDefaultButtons();
                buttonLayoutSwitching = false;
            }
        }

        private void SwitchButtonLayouts(bool showDefault)
        {
            buttonLayoutSwitching = true;

            var header = Manager.GetElement<RectTransform>("Header");
            var footer = Manager.GetElement<RectTransform>("Footer");
            UIManager.HideExitButton(0.2f);

            DOTween.Sequence()
                .Append(header.DOAnchorPosY(PropertyOverrider.NeedMobileLayout() ? 130f : 120f, 0.2f).OnComplete(() =>
                {
                    if (showDefault)
                        ShowDefaultButtons();
                    else
                        ShowDeleteButtons();
                    UIManager.ShowExitButton(0.3f, Ease.OutQuart);
                    if (!showDefault)
                        foreach (var item in superScrollView.items)
                        {
                            if (item.gameObject == null)
                                continue;
                            item.gameObject.GetComponent<SelectionToggle_Deck>().ShowToggle();
                        }
                }))
                .Append(header.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutQuart));

            DOTween.Sequence()
                .Append(footer.DOAnchorPosY(PropertyOverrider.NeedMobileLayout() ? -186f : -140f, 0.2f))
                .Append(footer.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutQuart)).OnComplete(() =>
                {
                    buttonLayoutSwitching = false;
                });
        }

        #endregion

        private void ShowDefaultButtons()
        {
            ButtonDelete.gameObject.SetActive(true);
            ButtonDeleteCancel.gameObject.SetActive(false);
            ButtonOnline.gameObject.SetActive(true);
            ButtonDeleteConfirm.gameObject.SetActive(false);
            Input.gameObject.SetActive(true);
        }

        private void ShowDeleteButtons()
        {
            ButtonDelete.gameObject.SetActive(false);
            ButtonDeleteCancel.gameObject.SetActive(true);
            ButtonOnline.gameObject.SetActive(false);
            ButtonDeleteConfirm.gameObject.SetActive(true);
            Input.gameObject.SetActive(false);
        }

        private void UpdateDeckNum()
        {
            TextDeckNumValue.text = decks.Count.ToString();
        }

    }
}