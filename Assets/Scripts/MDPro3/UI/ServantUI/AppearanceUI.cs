using MDPro3.Duel.YGOSharp;
using MDPro3.Servant;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MDPro3.Servant.Appearance;
using static YgomGame.Duel.BattleAimingEffect;

namespace MDPro3.UI.ServantUI
{
    public class AppearanceUI : ServantUI
    {

        #region Elements

        private const string LABEL_SR = "ScrollRect";
        private ScrollRect m_ScrollRect;
        public ScrollRect ScrollRect =>
            m_ScrollRect = m_ScrollRect != null ? m_ScrollRect
            : Manager.GetElement<ScrollRect>(LABEL_SR);
        private CanvasGroup m_ScrollRectCG;
        private CanvasGroup ScrollRectCG =>
            m_ScrollRectCG = m_ScrollRectCG != null ? m_ScrollRectCG
            : ScrollRect.GetComponent<CanvasGroup>();

        private const string LABEL_CG_DETAILS = "Details";
        private CanvasGroup m_Details;
        private CanvasGroup Details =>
            m_Details = m_Details != null ? m_Details
            : Manager.GetElement<CanvasGroup>(LABEL_CG_DETAILS);

        private const string LABEL_TXT_DETAILTITLE = "TextDetailTitle";
        private TextMeshProUGUI m_TextDetailTitle;
        private TextMeshProUGUI TextDetailTitle =>
            m_TextDetailTitle = m_TextDetailTitle != null ? m_TextDetailTitle
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_DETAILTITLE);

        private const string LABEL_TXT_DETAILSETTING = "TextDetailSetting";
        private TextMeshProUGUI m_TextDetailSetting;
        private TextMeshProUGUI TextDetailSetting =>
            m_TextDetailSetting = m_TextDetailSetting != null ? m_TextDetailSetting
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_DETAILSETTING);

        private const string LABEL_TXT_DETAILDESCRIPTION = "TextDetailDescription";
        private TextMeshProUGUI m_TextDetailDescription;
        private TextMeshProUGUI TextDetailDescription =>
            m_TextDetailDescription = m_TextDetailDescription != null ? m_TextDetailDescription
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_DETAILDESCRIPTION);

        private const string LABEL_IMG = "Image";
        private Image m_Image;
        public Image Image =>
            m_Image = m_Image != null ? m_Image
            : Manager.GetElement<Image>(LABEL_IMG);

        private const string LABEL_RIMG = "RawImage";
        private RawImage m_RawImage;
        public RawImage RawImage =>
            m_RawImage = m_RawImage != null ? m_RawImage
            : Manager.GetElement<RawImage>(LABEL_RIMG);

        private const string LABEL_TXT_HOVER = "TextHover";
        private TextMeshProUGUI m_TextHover;
        private TextMeshProUGUI TextHover =>
            m_TextHover = m_TextHover != null ? m_TextHover
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_HOVER);

        private const string LABEL_GO_NAMETABLE = "NameTable";
        private GameObject m_NameTable;
        private GameObject NameTable =>
            m_NameTable = m_NameTable != null ? m_NameTable
            : Manager.GetElement(LABEL_GO_NAMETABLE);

        private const string LABEL_STG_PAGE00 = "Page00PlayerName";
        private SelectionToggle_AppearanceGenre m_Page00;
        private SelectionToggle_AppearanceGenre Page00PlayerName =>
            m_Page00 = m_Page00 != null ? m_Page00
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE00);

        private const string LABEL_STG_PAGE01 = "Page01Wallpaper";
        private SelectionToggle_AppearanceGenre m_Page01;
        private SelectionToggle_AppearanceGenre Page01Wallpaper =>
            m_Page01 = m_Page01 != null ? m_Page01
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE01);

        private const string LABEL_STG_PAGE02 = "Page02Face";
        private SelectionToggle_AppearanceGenre m_Page02;
        private SelectionToggle_AppearanceGenre Page02Face =>
            m_Page02 = m_Page02 != null ? m_Page02
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE02);

        private const string LABEL_STG_PAGE03 = "Page03Frame";
        private SelectionToggle_AppearanceGenre m_Page03;
        private SelectionToggle_AppearanceGenre Page03Frame =>
            m_Page03 = m_Page03 != null ? m_Page03
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE03);

        private const string LABEL_STG_PAGE04 = "Page04Case";
        private SelectionToggle_AppearanceGenre m_Page04;
        private SelectionToggle_AppearanceGenre Page04Case =>
            m_Page04 = m_Page04 != null ? m_Page04
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE04);

        private const string LABEL_STG_PAGE05 = "Page05Protector";
        private SelectionToggle_AppearanceGenre m_Page05;
        private SelectionToggle_AppearanceGenre Page05Protector =>
            m_Page05 = m_Page05 != null ? m_Page05
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE05);

        private const string LABEL_STG_PAGE06 = "Page06Field";
        private SelectionToggle_AppearanceGenre m_Page06;
        private SelectionToggle_AppearanceGenre Page06Field =>
            m_Page06 = m_Page06 != null ? m_Page06
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE06);

        private const string LABEL_STG_PAGE07 = "Page07Grave";
        private SelectionToggle_AppearanceGenre m_Page07;
        private SelectionToggle_AppearanceGenre Page07Grave =>
            m_Page07 = m_Page07 != null ? m_Page07
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE07);

        private const string LABEL_STG_PAGE08 = "Page08Stand";
        private SelectionToggle_AppearanceGenre m_Page08;
        private SelectionToggle_AppearanceGenre Page08Stand =>
            m_Page08 = m_Page08 != null ? m_Page08
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE08);

        private const string LABEL_STG_PAGE09 = "Page09Mate";
        private SelectionToggle_AppearanceGenre m_Page09;
        private SelectionToggle_AppearanceGenre Page09Mate =>
            m_Page09 = m_Page09 != null ? m_Page09
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE09);

        private const string LABEL_STG_PAGE10 = "Page10Pickup";
        private SelectionToggle_AppearanceGenre m_Page10;
        private SelectionToggle_AppearanceGenre Page10Pickup =>
            m_Page10 = m_Page10 != null ? m_Page10
            : Manager.GetElement<SelectionToggle_AppearanceGenre>(LABEL_STG_PAGE10);

        private const string LABEL_STG_OVERWRITE = "ToggleOverwrite";
        private SelectionToggle m_ToggleOverwrite;
        private SelectionToggle ToggleOverwrite =>
            m_ToggleOverwrite = m_ToggleOverwrite != null ? m_ToggleOverwrite
            : Manager.GetElement<SelectionToggle>(LABEL_STG_OVERWRITE);

        private const string LABEL_STG_PLAYER0 = "TogglePlayer0";
        private SelectionToggle_AppearancePlayer m_TogglePlayer0;
        private SelectionToggle_AppearancePlayer TogglePlayer0 =>
            m_TogglePlayer0 = m_TogglePlayer0 != null ? m_TogglePlayer0
            : Manager.GetElement<SelectionToggle_AppearancePlayer>(LABEL_STG_PLAYER0);

        private const string LABEL_TXT_INPUTHINT = "TextInputHint";
        private TextMeshProUGUI m_TextInputHint;
        private TextMeshProUGUI TextInputHint =>
            m_TextInputHint = m_TextInputHint != null ? m_TextInputHint
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_INPUTHINT);

        private const string LABEL_MONO_DECKPICKUP = "DeckPickup";
        private DeckPickup m_DeckPickup;
        private DeckPickup DeckPickup =>
            m_DeckPickup = m_DeckPickup != null ? m_DeckPickup
            : Manager.GetElement<DeckPickup>(LABEL_MONO_DECKPICKUP);

        private const string LABEL_IPT_PLAYERNAME = "InputFieldPlayerName";
        private TMP_InputField m_InputPlayerName;
        public TMP_InputField InputPlayerName =>
            m_InputPlayerName = m_InputPlayerName != null ? m_InputPlayerName
            : Manager.GetElement<TMP_InputField>(LABEL_IPT_PLAYERNAME);

        #endregion

        private static readonly List<GameObject> wallpapers = new List<GameObject>();
        private static readonly List<GameObject> faces = new List<GameObject>();
        private static readonly List<GameObject> frames = new List<GameObject>();
        private static readonly List<GameObject> protectors = new List<GameObject>();
        private static readonly List<GameObject> mats = new List<GameObject>();
        private static readonly List<GameObject> graves = new List<GameObject>();
        private static readonly List<GameObject> stands = new List<GameObject>();
        private static readonly List<GameObject> mates = new List<GameObject>();
        private static readonly List<GameObject> cases = new List<GameObject>();

        private readonly Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>
        {
            { "Wallpaper", wallpapers },
            { "Face", faces },
            { "Frame", frames },
            { "Protector", protectors },
            { "Field", mats },
            { "Grave", graves },
            { "Stand", stands },
            { "Mate", mates },
            { "Case", cases },
        };

        private void Awake()
        {
            if (Config.GetBool("OverrideDeckAppearance", false))
                ToggleOverwrite.SetToggleOn();
            else
                ToggleOverwrite.SetToggleOff();
        }

        public override void SelectDefaultSelectable()
        {
            if (condition == Condition.DeckEditor)
                Page04Case.GetSelectable().Select();
            else
                Page00PlayerName.GetSelectable().Select();
        }

        public override void ShowEvent()
        {
            base.ShowEvent();

            switch (condition)
            {
                case Condition.Duel:
                    Title.text = InterString.Get("决斗外观");
                    break;
                case Condition.Watch:
                    Title.text = InterString.Get("观战外观");
                    break;
                case Condition.Replay:
                    Title.text = InterString.Get("回放外观");
                    break;
                case Condition.DeckEditor:
                    Title.text = InterString.Get("卡组外观");
                    break;
            }

            Page00PlayerName.gameObject.SetActive(condition != Condition.DeckEditor);
            Page01Wallpaper.gameObject.SetActive(condition != Condition.DeckEditor);
            Page02Face.gameObject.SetActive(condition != Condition.DeckEditor);
            Page03Frame.gameObject.SetActive(condition != Condition.DeckEditor);
            Page04Case.gameObject.SetActive(condition == Condition.DeckEditor);
            Page10Pickup.gameObject.SetActive(condition == Condition.DeckEditor);

            if (condition == Condition.DeckEditor)
            {
                Page10Pickup.GetSelectable().Select();
                Page10Pickup.SetToggleOn();
            }
            else
            {
                Page00PlayerName.GetSelectable().Select();
                Page00PlayerName.SetToggleOn();
            }

            TogglePlayer0.SetToggleOn();
        }

        protected override void HideEvent()
        {
            base.HideEvent();

            if (condition != Condition.DeckEditor)
            {
                Program.instance.setting.GetUI<SettingServantUI>().RefreshAppearanceModeText();

                if (UIManager.currentWallpaper != Config.Get("Wallpaper", Program.items.wallpapers[0].id.ToString()))
                {
                    UIManager.currentWallpaper = Config.Get("Wallpaper", Program.items.wallpapers[0].id.ToString());
                    Program.instance.ui_.ChangeWallpaper(UIManager.currentWallpaper);
                }
            }
        }

        protected override void AfterHideEvent()
        {
            base.AfterHideEvent();

            foreach (var pool in pools)
            {
                foreach (var item in pool.Value)
                    item.GetComponent<SelectionToggle_AppearanceItem>().Dispose();
                pool.Value.Clear();
            }
            Config.Save();
        }

        public void SavePlayerName(string nameValue)
        {
            Config.Set(condition.ToString() + "PlayerName" + player, nameValue == string.Empty ? "@ui" : nameValue);
        }

        public bool CanSwitchPlayer()
        {
            return TogglePlayer0.gameObject.activeSelf;
        }

        public void OnPlayerLeft()
        {
            TogglePlayer0.OnLeftSelection();
        }

        public void OnPlayerRight()
        {
            TogglePlayer0.OnRightSelection();
        }

        public void SetDetailName(string itemName)
        {
            TextDetailSetting.text = itemName;
        }

        public void SetDetailDescription(string desc)
        {
            TextDetailDescription.text = desc;
        }

        public void SetHoverText(string hover)
        {
            TextHover.text = hover;
        }

        public void SetDetailImage(Sprite sprite)
        {
            Image.sprite = sprite;
            Image.gameObject.SetActive(true);
            RawImage.gameObject.SetActive(false);
        }

        public void SetDetailImageMaterial(Material mat)
        {
            Image.material = mat;
            Image.gameObject.SetActive(true);
            RawImage.gameObject.SetActive(false);
        }

        public void SetDetailRawImageMaterial(Material mat)
        {
            RawImage.material = mat;
            RawImage.gameObject.SetActive(true);
            Image.gameObject.SetActive(false);
        }


        public static string currentContent = "PlayerName";
        private static List<Items.Item> targetItems;
        private static List<GameObject> currentList;
        private static List<GameObject> onlyOpSideShowItems = new();
        public void ShowItems(string type)
        {
            currentContent = type;
            pools.TryGetValue(currentContent, out currentList);
            if (condition == Condition.DeckEditor)
                TogglePlayer0.transform.parent.gameObject.SetActive(false);
            else
                TogglePlayer0.transform.parent.gameObject.SetActive(true);

            DeckPickup.gameObject.SetActive(currentContent == "Pickup");

            if (currentContent == "PlayerName")
            {
                ScrollRectCG.alpha = 0;
                ScrollRectCG.blocksRaycasts = false;
                Details.alpha = 0f;
                NameTable.SetActive(true);

                InputPlayerName.text = Config.Get(condition.ToString() + currentContent + player, "@ui");
                if (player == "0")
                    TextInputHint.text = InterString.Get("请输入您的昵称：");
                else if (player == "1")
                    TextInputHint.text = InterString.Get("请输入对方的昵称，留空则显示真实昵称：");
                else if (player == "0Tag")
                    TextInputHint.text = InterString.Get("请输入您的队友的昵称，留空则显示真实昵称：");
                else if (player == "1Tag")
                    TextInputHint.text = InterString.Get("请输入对方的队友的昵称，留空则显示真实昵称：");
                return;
            }
            else if (currentContent == "Pickup")
            {
                ScrollRectCG.alpha = 0f;
                ScrollRectCG.blocksRaycasts = false;
                Details.alpha = 0f;

                DeckPickup.gameObject.SetActive(true);
                DeckPickup.SetDeck(DeckEditor.Deck);
                return;
            }
            else if (currentContent == "Wallpaper")
            {
                TogglePlayer0.transform.parent.gameObject.SetActive(false);
            }

            ScrollRectCG.alpha = 1.0f;
            ScrollRectCG.blocksRaycasts = true;
            Details.alpha = 1f;
            NameTable.SetActive(false);
            DeckPickup.gameObject.SetActive(false);

            bool isWallpaper = false;
            switch (currentContent)
            {
                case "Wallpaper":
                    targetItems = Program.items.wallpapers;
                    isWallpaper = true;
                    break;
                case "Face":
                    targetItems = Program.items.faces;
                    break;
                case "Frame":
                    targetItems = Program.items.frames;
                    break;
                case "Protector":
                    targetItems = Program.items.protectors;
                    break;
                case "Field":
                    targetItems = Program.items.mats;
                    break;
                case "Grave":
                    targetItems = Program.items.graves;
                    break;
                case "Stand":
                    targetItems = Program.items.stands;
                    break;
                case "Mate":
                    targetItems = Program.items.mates;
                    break;
                case "Case":
                    targetItems = Program.items.cases;
                    break;
                default:
                    targetItems = Program.items.mates;
                    break;
            }


            foreach (var pool in pools)
                if (pool.Key != currentContent)
                    foreach (var item in pool.Value)
                        item.GetComponent<SelectionToggle_AppearanceItem>().Hide();

            if (currentList.Count == 0)
            {
                int itemCount = 0;
                for (int i = 0; i < targetItems.Count; i++)
                {
                    GameObject item = Instantiate(appearanceItem);
                    var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                    itemMono.index = i;
                    itemCount = itemMono.index;
                    itemMono.itemID = targetItems[i].id;
                    itemMono.description = targetItems[i].description;
                    itemMono.itemName = targetItems[i].name;
                    itemMono.path = Items.CodeToIconPath(itemMono.itemID.ToString());
                    itemMono.transform.SetParent(ScrollRect.content, false);
                    itemMono.Refresh();
                    currentList.Add(item);
                }

#if UNITY_ANDROID
                if (currentContent == "Mate")
                {
                    var files = new DirectoryInfo(Program.root + "CrossDuel").GetFiles("*.bundle");
                    for (int i = 0; i < files.Length; i++)
                    {
                        int code = int.Parse(files[i].Name.Replace(".bundle", ""));
                        var card = CardsManager.Get(code, true);
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = i + targetItems.Count;
                        itemCount = itemMono.index;
                        itemMono.itemID = code;
                        if (card.Id == 0)
                            itemMono.itemName = MateViewerUI.GetRushDuelMateName(code);
                        else
                            itemMono.itemName = card.Name;
                        itemMono.description = card.Desc;
                        itemMono.path = string.Empty;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                    }
                }
#endif
                if (condition != Condition.DeckEditor)
                {
                    if (Program.items.ListHaveNone(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = ++itemCount;
                        itemMono.itemID = Items.CODE_NONE;
                        itemMono.description = InterString.Get("该项设置将设置为无。");
                        itemMono.itemName = InterString.Get("不设置");
                        itemMono.path = (isWallpaper ? "WallPaperIcon" : string.Empty) + Items.PATH_ICON_NONE;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                    }

                    if (Program.items.ListHaveRandom(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = ++itemCount;
                        itemMono.itemID = Items.CODE_RANDOM;
                        itemMono.description = InterString.Get("该项设置将随机设置。");
                        itemMono.itemName = InterString.Get("随机");
                        itemMono.path = (isWallpaper ? "WallPaperIcon" : string.Empty) + Items.PATH_ICON_RANDOM;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                    }
                    if (Program.items.ListHaveSame(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = ++itemCount;
                        itemMono.itemID = Items.CODE_SAME;
                        itemMono.description = InterString.Get("该项设置将与场地设置保持一致。");
                        itemMono.itemName = InterString.Get("一致");
                        itemMono.path = Items.PATH_ICON_SAME;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                    }

                    if (Program.items.ListHaveDIY(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = ++itemCount;
                        itemMono.itemID = Items.CODE_DIY;
                        itemMono.description = InterString.Get("我方头像：") +
                                                                Program.diyPath + meString + Program.pngExpansion + "\n" +
                                                                InterString.Get("对方头像：") +
                                                                Program.diyPath + opString + Program.pngExpansion + "\n" +
                                                                InterString.Get("我方队友头像：") +
                                                                Program.diyPath + meTagString + Program.pngExpansion + "\n" +
                                                                InterString.Get("对方队友头像：") +
                                                                Program.diyPath + opTagString + Program.pngExpansion;
                        itemMono.itemName = InterString.Get("自定义");
                        itemMono.path = Items.PATH_ICON_DIY;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                    }

                    if (targetItems == Program.items.mats)
                    {
                        GameObject item = Instantiate(appearanceItem);
                        var itemMono = item.GetComponent<SelectionToggle_AppearanceItem>();
                        itemMono.index = ++itemCount;
                        itemMono.itemID = Items.CODE_SAME;
                        itemMono.description = InterString.Get("该项设置将与我方场地设置保持一致。");
                        itemMono.itemName = InterString.Get("一致");
                        itemMono.path = Items.PATH_ICON_SAME;
                        itemMono.transform.SetParent(ScrollRect.content, false);
                        itemMono.Refresh();
                        currentList.Add(item);
                        onlyOpSideShowItems.Add(item);
                    }
                }
            }
            foreach (var item in currentList)
            {
                if (player.Contains("0") && onlyOpSideShowItems.Contains(item))
                    item.GetComponent<SelectionToggle_AppearanceItem>().Hide();
                else
                    item.GetComponent<SelectionToggle_AppearanceItem>().Show();
            }
            foreach (var item in currentList)
            {
                if (currentContent == "Wallpaper")
                {
                    if (item.GetComponent<SelectionToggle_AppearanceItem>().itemID.ToString() == Config.Get("Wallpaper", targetItems[0].id.ToString()))
                    {
                        item.GetComponent<SelectionToggle_AppearanceItem>().SetToggleOn();
                        break;
                    }
                }
                else
                {
                    var itemID = item.GetComponent<SelectionToggle_AppearanceItem>().itemID;

                    if (condition == Condition.DeckEditor)
                    {
                        if (itemID == DeckEditor.Deck.Case
                            || itemID == DeckEditor.Deck.Protector
                            || itemID == DeckEditor.Deck.Field
                            || itemID == DeckEditor.Deck.Grave
                            || itemID == DeckEditor.Deck.Stand
                            || itemID == DeckEditor.Deck.Mate)
                        {
                            item.GetComponent<SelectionToggle_AppearanceItem>().SetToggleOn();
                            break;
                        }
                    }
                    else
                    {
                        if (itemID.ToString() == Config.Get(condition.ToString() + currentContent + player, targetItems[0].id.ToString()))
                        {
                            item.GetComponent<SelectionToggle_AppearanceItem>().SetToggleOn();
                            break;
                        }
                    }
                }
            }
        }

        public void SwitchPlayer(string player)
        {
            Appearance.player = player;
            if (condition == Condition.Duel && player == "0")
                ToggleOverwrite.gameObject.SetActive(true);
            else
                ToggleOverwrite.gameObject.SetActive(false);
            ShowItems(currentContent);
        }

        public void SetOverride(bool over)
        {
            Config.SetBool("OverrideDeckAppearance", over);
        }

        public int GetCurrentGenreCount()
        {
            foreach (var pool in pools)
                if (pool.Key == currentContent)
                    return pool.Value.Count;
            return 0;
        }

        public GameObject GetCurrentContentItem()
        {
            if (currentContent == "PlayerName")
                return InputPlayerName.gameObject;
            if (currentContent == "Pickup")
                return null;

            if (Program.instance.appearance.lastSelectedItem != null 
                && Program.instance.appearance.lastSelectedItem.gameObject.activeSelf)
                return Program.instance.appearance.lastSelectedItem.gameObject;
            return ScrollRect.content.GetChild(0).gameObject;
        }

        public void SelectPlayerNameToggle()
        {
            UserInput.NextSelectionIsAxis = true;
            Page00PlayerName.GetSelectable().Select();
        }

    }
}