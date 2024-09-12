using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.UI;
using DG.Tweening.Plugins.Core.PathCore;

namespace MDPro3
{
    public class Appearance : Servant
    {
        public ButtonList defaultButton;
        public ButtonList defaultButtonDeck;
        public ButtonList defaultPlayer;
        public Text title;

        public Text detailTitle;
        public Text description;
        public Image detailImage;
        public RawImage detailProtector;

        public Text playerNameEx;
        public InputField playerName;

        public GameObject table;
        public Text deckName;
        public Text mainCount;
        public Text extraCount;
        public Text sideCount;
        public RectTransform cardsRoot;

        public Text hover;
        public ScrollRect scrollView;
        public ButtonSwitch btnOverride;

        public static Sprite duelFace0;
        public static Sprite duelFace1;
        public static Sprite watchFace0;
        public static Sprite watchFace1;
        public static Sprite replayFace0;
        public static Sprite replayFace1;
        public static Sprite duelFace0Tag;
        public static Sprite duelFace1Tag;
        public static Sprite watchFace0Tag;
        public static Sprite watchFace1Tag;
        public static Sprite replayFace0Tag;
        public static Sprite replayFace1Tag;
        public static Sprite defaultFace0;
        public static Sprite defaultFace1;

        public static Material duelFrameMat0;
        public static Material duelFrameMat1;
        public static Material watchFrameMat0;
        public static Material watchFrameMat1;
        public static Material replayFrameMat0;
        public static Material replayFrameMat1;
        public static Material duelFrameMat0Tag;
        public static Material duelFrameMat1Tag;
        public static Material watchFrameMat0Tag;
        public static Material watchFrameMat1Tag;
        public static Material replayFrameMat0Tag;
        public static Material replayFrameMat1Tag;

        public static Material duelProtector0;
        public static Material duelProtector1;
        public static Material watchProtector0;
        public static Material watchProtector1;
        public static Material replayProtector0;
        public static Material replayProtector1;
        public static Material duelProtector0Tag;
        public static Material duelProtector1Tag;
        public static Material watchProtector0Tag;
        public static Material watchProtector1Tag;
        public static Material replayProtector0Tag;
        public static Material replayProtector1Tag;

        public static Material matForFace;
        public static string player = "0";
        public const string meString = "Me";
        public const string opString = "Op";
        public const string meTagString = "MeTag";
        public const string opTagString = "OpTag";


        static List<GameObject> wallpapers = new List<GameObject>();
        static List<GameObject> faces = new List<GameObject>();
        static List<GameObject> frames = new List<GameObject>();
        static List<GameObject> protectors = new List<GameObject>();
        static List<GameObject> mats = new List<GameObject>();
        static List<GameObject> graves = new List<GameObject>();
        static List<GameObject> stands = new List<GameObject>();
        static List<GameObject> mates = new List<GameObject>();
        static List<GameObject> cases = new List<GameObject>();

        Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>
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

        public GameObject appearanceItem;
        public enum Condition
        {
            Duel,
            Watch,
            Replay,
            Deck
        }
        public Condition condition = Condition.Duel;
        public void SwitchCondition(Condition condition)
        {
            this.condition = condition;
            switch (condition)
            {
                case Condition.Duel:
                    depth = 3;
                    title.text = InterString.Get("决斗外观");
                    break;
                case Condition.Watch:
                    depth = 3;
                    title.text = InterString.Get("观战外观");
                    break;
                case Condition.Replay:
                    depth = 3;
                    title.text = InterString.Get("回放外观");
                    break;
                case Condition.Deck:
                    depth = 7;
                    title.text = InterString.Get("卡组外观");
                    break;
            }
        }
        public override void Initialize()
        {
            depth = 3;
            haveLine = true;
            subBlackAlpha = 0.9f;
            base.Initialize();
            Program.onScreenChanged += RefreshItemsPosition;
            playerName.onEndEdit.AddListener(SavePlayerName);

            AssetBundle ab = AssetBundle.LoadFromFile(Program.root + "MasterDuel/Frame/ProfileFrameMat1030001");
            matForFace = ab.LoadAsset<Material>("ProfileFrameMat1030001");
            ab.Unload(false);
            var handle = Addressables.LoadAssetAsync<GameObject>("AppearanceItem");
            handle.Completed += (result) =>
            {
                appearanceItem = result.Result;
            };

            btnOverride.onAction = () =>
            {
                Config.SetBool("OverrideDeckAppearance", true);
            };
            btnOverride.offAction = () =>
            {
                Config.SetBool("OverrideDeckAppearance", false);
            };

            StartCoroutine(LoadSettingAssets());
        }


        public static bool loaded;
        public IEnumerator LoadSettingAssets()
        {
            loaded = false;

            #region Face
            var ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 0);
            while (ie.MoveNext())
                yield return null;
            duelFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 1);
            while (ie.MoveNext())
                yield return null;
            duelFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 2);
            while (ie.MoveNext())
                yield return null;
            duelFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 3);
            while (ie.MoveNext())
                yield return null;
            duelFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync("1010039", Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            defaultFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync("1010001", Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            defaultFace1 = ie.Current;

            #endregion

            #region Frame

            Sprite duelFrame0;
            Sprite duelFrame1;
            Sprite watchFrame0;
            Sprite watchFrame1;
            Sprite replayFrame0;
            Sprite replayFrame1;
            Sprite duelFrame0Tag;
            Sprite duelFrame1Tag;
            Sprite watchFrame0Tag;
            Sprite watchFrame1Tag;
            Sprite replayFrame0Tag;
            Sprite replayFrame1Tag;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame0 = ie.Current;

            var im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat0 = im.Current;
            duelFrameMat0.SetTexture("_ProfileFrameTex", duelFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat1 = im.Current;
            duelFrameMat1.SetTexture("_ProfileFrameTex", duelFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat0Tag = im.Current;
            duelFrameMat0Tag.SetTexture("_ProfileFrameTex", duelFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat1Tag = im.Current;
            duelFrameMat1Tag.SetTexture("_ProfileFrameTex", duelFrame1Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame0 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat0 = im.Current;
            watchFrameMat0.SetTexture("_ProfileFrameTex", watchFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat1 = im.Current;
            watchFrameMat1.SetTexture("_ProfileFrameTex", watchFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat0Tag = im.Current;
            watchFrameMat0Tag.SetTexture("_ProfileFrameTex", watchFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat1Tag = im.Current;
            watchFrameMat1Tag.SetTexture("_ProfileFrameTex", watchFrame1Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame0 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat0 = im.Current;
            replayFrameMat0.SetTexture("_ProfileFrameTex", replayFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat1 = im.Current;
            replayFrameMat1.SetTexture("_ProfileFrameTex", replayFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat0Tag = im.Current;
            replayFrameMat0Tag.SetTexture("_ProfileFrameTex", replayFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat1Tag = im.Current;
            replayFrameMat1Tag.SetTexture("_ProfileFrameTex", replayFrame1Tag.texture);

            #endregion

            #region Protector
            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector1Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector1Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector1Tag = im.Current;

            #endregion

            loaded = true;
        }

        void SavePlayerName(string nameValue)
        {
            Config.Set(condition.ToString() + "PlayerName" + player, nameValue == "" ? "@ui" : nameValue);
            playerName.text = Config.Get(condition.ToString() + "PlayerName" + player, "@ui");
        }

        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            if (condition == Condition.Deck)
            {
                defaultButton.transform.parent.gameObject.SetActive(false);
                defaultButtonDeck.transform.parent.gameObject.SetActive(true);
                defaultButtonDeck.SelectThis();
                deckName.text = Program.I().editDeck.input.text;
                mainCount.text = Program.I().editDeck.mainCount.ToString();
                extraCount.text = Program.I().editDeck.extraCount.ToString();
                sideCount.text = Program.I().editDeck.sideCount.ToString();
                foreach (var card in Program.I().editDeck.cards)
                {
                    card.transform.SetParent(cardsRoot, false);
                    card.RefreshPositionInstant();
                }
                PrePick();
            }
            else
            {
                defaultButton.transform.parent.gameObject.SetActive(true);
                defaultButtonDeck.transform.parent.gameObject.SetActive(false);
                defaultButton.SelectThis();
            }
            defaultPlayer.SelectThis();
        }

        public override void OnReturn()
        {
            base.OnReturn();
            if (returnAction != null) return;
            if (inTransition) return;
            OnExit();
        }

        public override void OnExit()
        {
            if (condition != Condition.Deck)
            {
                if (Program.I().currentSubServant == this)
                    Program.I().ShowSubServant(Program.I().setting);
                else
                    Program.I().ShiftToServant(Program.I().setting);

                Program.I().setting.duelAppearanceValue.text = Config.Get("DuelPlayerName0", "@ui");
                Program.I().setting.watchAppearanceValue.text = Config.Get("WatchPlayerName0", "@ui");
                Program.I().setting.replayAppearanceValue.text = Config.Get("ReplayPlayerName0", "@ui");

                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    foreach (var pool in pools)
                    {
                        foreach (var item in pool.Value)
                            StartCoroutine(item.GetComponent<AppearanceItem>().Dispose());
                        pool.Value.Clear();
                    }
                    Config.Save();
                });

                if (UIManager.currentWallpaper != Config.Get("Wallpaper", Program.items.wallpapers[0].id.ToString()))
                {
                    UIManager.currentWallpaper = Config.Get("Wallpaper", Program.items.wallpapers[0].id.ToString());
                    Program.I().ui_.ChangeWallpaper(UIManager.currentWallpaper);
                }
            }
            else
            {
                Program.I().ShiftToServant(Program.I().editDeck);
                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    foreach (var pool in pools)
                    {
                        foreach (var item in pool.Value)
                            StartCoroutine(item.GetComponent<AppearanceItem>().Dispose());
                        pool.Value.Clear();
                    }
                });
                Program.I().editDeck.deck.Pickup.Clear();
                foreach (var card in Program.I().editDeck.cards)
                {
                    card.transform.SetParent(Program.I().editDeck.cardsOnEditParent, false);
                    card.RefreshPositionInstant();
                    if (card.picked)
                        Program.I().editDeck.deck.Pickup.Add(card.code);
                    card.PickUp(false);
                }
            }
        }

        public static string currentContent = "";
        static List<Items.Item> targetItems;
        static List<GameObject> currentList;
        public void ShowItems(string type)
        {
            currentContent = type;
            pools.TryGetValue(currentContent, out currentList);
            if (Program.I().appearance.condition == Condition.Deck)
                defaultPlayer.transform.parent.gameObject.SetActive(false);
            else
                defaultPlayer.transform.parent.gameObject.SetActive(true);
            table.gameObject.SetActive(false);
            cardsRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -1080f);

            if (currentContent == "PlayerName")
            {
                scrollView.GetComponent<CanvasGroup>().alpha = 0;
                scrollView.GetComponent<CanvasGroup>().blocksRaycasts = false;
                detailTitle.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                detailTitle.transform.parent.GetComponent<CanvasGroup>().blocksRaycasts = false;
                playerName.transform.parent.parent.gameObject.SetActive(true);

                playerName.text = Config.Get(Program.I().appearance.condition.ToString() + currentContent + player, "");
                if (player == "0")
                    playerNameEx.text = InterString.Get("请输入您的昵称：");
                else if (player == "1")
                    playerNameEx.text = InterString.Get("请输入对方的昵称，留空则显示真实昵称：");
                else if (player == "0Tag")
                    playerNameEx.text = InterString.Get("请输入您的队友的昵称，留空则显示真实昵称：");
                else if (player == "1Tag")
                    playerNameEx.text = InterString.Get("请输入对方的队友的昵称，留空则显示真实昵称：");
                return;
            }
            else if (currentContent == "Pickup")
            {
                scrollView.GetComponent<CanvasGroup>().alpha = 0;
                scrollView.GetComponent<CanvasGroup>().blocksRaycasts = false;
                detailTitle.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                detailTitle.transform.parent.GetComponent<CanvasGroup>().blocksRaycasts = false;
                playerName.transform.parent.parent.gameObject.SetActive(false);
                table.gameObject.SetActive(true);
                cardsRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                return;
            }
            else if (currentContent == "Wallpaper")
            {
                defaultPlayer.transform.parent.gameObject.SetActive(false);
            }

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

            scrollView.GetComponent<CanvasGroup>().alpha = 1;
            scrollView.GetComponent<CanvasGroup>().blocksRaycasts = true;
            detailTitle.transform.parent.GetComponent<CanvasGroup>().alpha = 1;
            detailTitle.transform.parent.GetComponent<CanvasGroup>().blocksRaycasts = true;
            playerName.transform.parent.parent.gameObject.SetActive(false);
            table.gameObject.SetActive(false);

            foreach (var pool in pools)
                if (pool.Key != currentContent)
                    foreach (var item in pool.Value)
                        StartCoroutine(item.GetComponent<AppearanceItem>().Hide());

            if (currentList.Count == 0)
            {
                int itemCount = 0;
                for (int i = 0; i < targetItems.Count; i++)
                {
                    GameObject item = Instantiate(appearanceItem);
                    AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                    itemMono.id = i;
                    itemCount = itemMono.id;
                    itemMono.itemID = targetItems[i].id;
                    itemMono.description = targetItems[i].description;
                    itemMono.itemName = targetItems[i].name;
                    itemMono.path = Items.CodeToIconPath(itemMono.itemID.ToString());
                    itemMono.transform.SetParent(scrollView.content, false);
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
                        AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                        itemMono.id = i + targetItems.Count;
                        itemCount = itemMono.id;
                        itemMono.itemID = code;
                        if (card.Id == 0)
                            itemMono.itemName = MateView.GetRushDuelMateName(code);
                        else
                            itemMono.itemName = card.Name;
                        itemMono.description = card.Desc;
                        itemMono.path = "";
                        itemMono.transform.SetParent(scrollView.content, false);
                        currentList.Add(item);
                    }
                }
#endif
                if(condition != Condition.Deck)
                {
                    if (Program.items.ListHaveNone(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                        itemMono.id = ++itemCount;
                        itemMono.itemID = Items.noneCode;
                        itemMono.description = InterString.Get("该项设置将设置为无。");
                        itemMono.itemName = InterString.Get("不设置");
                        itemMono.path = (isWallpaper ? "WallPaperIcon" : string.Empty) + Items.noneIconPath;
                        itemMono.transform.SetParent(scrollView.content, false);
                        currentList.Add(item);
                    }

                    if (Program.items.ListHaveRandom(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                        itemMono.id = ++itemCount;
                        itemMono.itemID = Items.randomCode;
                        itemMono.description = InterString.Get("该项设置将随机设置。");
                        itemMono.itemName = InterString.Get("随机");
                        itemMono.path = (isWallpaper ? "WallPaperIcon" : string.Empty) + Items.randomIconPath;
                        itemMono.transform.SetParent(scrollView.content, false);
                        currentList.Add(item);
                    }
                    if (Program.items.ListHaveSame(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                        itemMono.id = ++itemCount;
                        itemMono.itemID = Items.sameCode;
                        itemMono.description = InterString.Get("该项设置将与场地设置保持一致。");
                        itemMono.itemName = InterString.Get("一致");
                        itemMono.path = Items.sameIconPath;
                        itemMono.transform.SetParent(scrollView.content, false);
                        currentList.Add(item);
                    }
                    if (Program.items.ListHaveDIY(targetItems))
                    {
                        GameObject item = Instantiate(appearanceItem);
                        AppearanceItem itemMono = item.GetComponent<AppearanceItem>();
                        itemMono.id = ++itemCount;
                        itemMono.itemID = Items.diyCode;
                        itemMono.description = InterString.Get("我方头像：") + 
                                                                Program.diyPath + meString + Program.pngExpansion + "\n" +
                                                                InterString.Get("对方头像：") + 
                                                                Program.diyPath + opString + Program.pngExpansion + "\n" +
                                                                InterString.Get("我方队友头像：") +
                                                                Program.diyPath + meTagString + Program.pngExpansion + "\n" +
                                                                InterString.Get("对方队友头像：") +
                                                                Program.diyPath + opTagString + Program.pngExpansion;
                        itemMono.itemName = InterString.Get("自定义");
                        itemMono.path = Items.diyIconPath;
                        itemMono.transform.SetParent(scrollView.content, false);
                        currentList.Add(item);
                    }
                }
            }
            foreach (var item in currentList)
            {
                item.SetActive(true);
                item.GetComponent<AppearanceItem>().Show();
            }
            foreach (var item in currentList)
            {
                if (currentContent == "Wallpaper")
                {
                    if (item.GetComponent<AppearanceItem>().itemID.ToString() == Config.Get("Wallpaper", targetItems[0].id.ToString()))
                    {
                        item.GetComponent<AppearanceItem>().SelectThis();
                        break;
                    }
                }
                else
                {
                    if (Program.I().appearance.condition == Condition.Deck)
                    {
                        if (item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Case[0]
                            || item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Protector[0]
                            || item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Field[0]
                            || item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Grave[0]
                            || item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Stand[0]
                            || item.GetComponent<AppearanceItem>().itemID == Program.I().editDeck.deck.Mate[0])
                        {
                            item.GetComponent<AppearanceItem>().SelectThis();
                            break;
                        }
                    }
                    else
                    {
                        if (item.GetComponent<AppearanceItem>().itemID.ToString() == Config.Get(Program.I().appearance.condition.ToString() + currentContent + player, targetItems[0].id.ToString()))
                        {
                            item.GetComponent<AppearanceItem>().SelectThis();
                            break;
                        }
                    }
                }
            }
            RefreshItemsPosition();
            scrollView.content.anchoredPosition = Vector2.zero;
        }
        public void SwitchPlayer(string player)
        {
            Appearance.player = player;
            if(condition == Condition.Duel)
            {
                if(player == "0")
                {
                    btnOverride.gameObject.SetActive(true);
                    if (Config.GetBool("OverrideDeckAppearance", false))
                        btnOverride.OnSwitchOn();
                    else
                        btnOverride.OnSwitchOff();
                }
                else
                    btnOverride.gameObject.SetActive(false);
            }
            else
                btnOverride.gameObject.SetActive(false);
            if (isShowed)
                ShowItems(currentContent);
        }
        int numOfEachLine;
        public void ScrollViewResize()
        {
            int screenWidth = (int)(1080 * (float)Screen.width / Screen.height);
            int targetWidth = screenWidth - 450 - 450 - 30 - 10 - 100;
            numOfEachLine = targetWidth / 160;
            if (numOfEachLine < 1)
                numOfEachLine = 1;
            targetWidth = 160 * numOfEachLine + 30 + 10;
            scrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, scrollView.GetComponent<RectTransform>().sizeDelta.y);
        }
        public void RefreshItemsPosition()
        {
            if (currentList == null) return;
            ScrollViewResize();
            foreach (var item in scrollView.content.GetComponentsInChildren<AppearanceItem>(true))
            {
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    10 + (item.id % numOfEachLine) * 160,
                    -20 - (int)Math.Floor(item.id / (float)numOfEachLine) * 160
                    );
            }
            int lines = (int)Math.Ceiling(currentList.Count / (float)numOfEachLine);
            scrollView.content.sizeDelta = new Vector2(scrollView.content.sizeDelta.x, lines * 160 + 40);
        }

        int pickCount;
        public void PickThis(CardOnEdit card)
        {
            if (!card.picked)
            {
                if (pickCount > 2)
                    return;
                else
                {
                    pickCount++;
                    card.PickUp(true);
                }
            }
            else
            {
                pickCount--;
                card.PickUp(false);
            }
        }
        void PrePick()
        {
            pickCount = 0;
            for (int i = 0; i < Program.I().editDeck.deck.Pickup.Count; i++)
            {
                foreach (var card in Program.I().editDeck.cards)
                {
                    if (card.code == Program.I().editDeck.deck.Pickup[i])
                    {
                        pickCount++;
                        card.PickUp(true);
                        break;
                    }
                }
            }
        }
    }
}
