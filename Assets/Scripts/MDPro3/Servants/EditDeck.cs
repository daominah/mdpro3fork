using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using MDPro3.UI;
using Toggle = MDPro3.UI.Toggle;
using UnityEngine.Android;
using static YgomSystem.Utility.AssetLinker;

namespace MDPro3
{
    public class EditDeck : Servant
    {
        [Serializable]
        public enum CardRarity
        {
            Normal = 1,
            Shine = 2,
            Royal = 8
        }

        public InputField input;
        public Text textMainCount;
        public Text textExtraCount;
        public Text textSideCount;
        public Image deckCase;

        int m_mainCount;
        public int mainCount
        {
            get { return m_mainCount; }
            set
            {
                m_mainCount = value;
                textMainCount.text = m_mainCount.ToString();
            }
        }
        int m_extraCount;
        public int extraCount
        {
            get { return m_extraCount; }
            set
            {
                m_extraCount = value;
                textExtraCount.text = m_extraCount.ToString();
            }
        }
        int m_sideCount;
        public int sideCount
        {
            get { return m_sideCount; }
            set
            {
                m_sideCount = value;
                textSideCount.text = m_sideCount.ToString();
            }
        }

        public CardDetail detail;
        public Transform cardsOnEditParent;
        public GameObject itemOnTable;
        public GameObject itemOnList;
        public List<CardOnEdit> cards = new List<CardOnEdit>();

        public ElementObjectManager manager;
        Tabs tabs;

        bool loaded;
        public bool dirty;
        string deckName;
        public Deck deck;
        Deck book;
        Deck history;
        public Deck shine;
        public Deck royal;
        readonly string bookPath = "Data/book.ydk";
        readonly string shinePath = "Data/sr.ydk";
        readonly string royalPath = "Data/ur.ydk";
        Card cardShowing;
        public Banlist banlist;
        public static string pack = "";
        SuperScrollView superScrollView;
        bool intoAppearance;

        public override void Initialize()
        {
            depth = 2;
            haveLine = false;
            returnServant = Program.I().selectDeck;
            manager = GetComponent<ElementObjectManager>();
            manager.GetElement<Button>("CardButton").onClick.AddListener(ShowDetail);
            tabs = manager.GetElement<Tabs>("List");
            tabs.tabs[0].onSelected = OnList;
            tabs.tabs[1].onSelected = OnBook;
            tabs.tabs[2].onSelected = OnHistory;

            banlist = BanlistManager.Banlists[0];
            manager.GetElement<Text>("TextBanlist").text = banlist.Name;
            manager.GetElement<Button>("ButtonAppearance").onClick.AddListener(ShowAppearance);
            manager.GetElement<Button>("ButtonBanlist").onClick.AddListener(ShowBanlists);
            manager.GetElement<InputField>("InputSearch").onEndEdit.AddListener(OnSearch);
            manager.GetElement<Button>("ButtonSearch").onClick.AddListener(OnClickSearch);

            Program.onScreenChanged += AdjustSize;
            AdjustSize();
            if (File.Exists(bookPath))
                book = new Deck(bookPath);
            else
                book = new Deck();
            if (File.Exists(shinePath))
                shine = new Deck(shinePath);
            else
                shine = new Deck();
            if (File.Exists(royalPath))
                royal = new Deck(royalPath);
            else
                royal = new Deck();
            base.Initialize();

            var handle = Addressables.LoadAssetAsync<GameObject>("CardOnEdit");
            handle.Completed += (result) =>
            {
                itemOnTable = result.Result;
            };
            handle = Addressables.LoadAssetAsync<GameObject>("CardOnList");
            handle.Completed += (result) =>
            {
                itemOnList = result.Result;
            };
        }
        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            if (intoAppearance)
                intoAppearance = false;
            else
            {
                AudioManager.PlayBGM("BGM_MENU_02");

                deckName = Config.Get("DeckInUse", "");
                input.text = deckName;
                ScrollViewInstall();
                ChangeCondition(condition);
                StartCoroutine(RefreshAsync());
                StartCoroutine(RefreshIcons());
                manager.GetElement("Group").SetActive(false);
            }
        }

        public override void Hide(int preDepth)
        {
            if (!isShowed)
                return;
            base.Hide(preDepth);

            if (!intoAppearance)
            {
                AudioManager.PlayBGM("BGM_MENU_01");
                var content = InterString.Get("#该文件是用于保存【卡片收藏】中的卡的卡组码。");
                content += "\r\n#main\r\n";
                for (int i = 0; i < book.Main.Count; i++)
                    content += book.Main[i] + "\r\n";
                File.WriteAllText(bookPath, content, Encoding.UTF8);
                content = InterString.Get("#该文件是用于保存UR卡的卡组码。");
                content += "\r\n#main\r\n";
                for (int i = 0; i < royal.Main.Count; i++)
                    content += royal.Main[i] + "\r\n";
                File.WriteAllText(royalPath, content, Encoding.UTF8);
                content = InterString.Get("#该文件是用于保存SR卡的卡组码。");
                content += "\r\n#main\r\n";
                for (int i = 0; i < shine.Main.Count; i++)
                    content += shine.Main[i] + "\r\n";
                File.WriteAllText(shinePath, content, Encoding.UTF8);
                condition = EditDeckCondition.EditDeck;
                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    Dispose();
                    if (superScrollView != null)
                        superScrollView.Clear();
                });
            }
        }


        public override void OnReturn()
        {
            if (!loaded)
                return;
            if (!dirty)
                base.OnReturn();
            else
            {
                List<string> selections = new List<string>
                {
                    InterString.Get("卡组未保存"),
                    InterString.Get("卡组已修改，是否保存？"),
                    InterString.Get("保存"),
                    InterString.Get("不保存")
                };
                UIManager.ShowPopupYesOrNo(selections, OnSave, OnExit);
            }
        }

        IEnumerator RefreshAsync()
        {
            loaded = false;

            mainCount = deck.Main.Count;
            extraCount = deck.Extra.Count;
            sideCount = deck.Side.Count;

            var casePath = deck.Case[0].ToString();
            var ie = TextureManager.LoadItemIcon(casePath);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            deckCase.sprite = ie.Current;

            for (int i = 0; i < deck.Main.Count; i++)
            {
                var card = Instantiate(itemOnTable);
                card.transform.SetParent(cardsOnEditParent, false);
                var mono = card.GetComponent<CardOnEdit>();
                mono.id = i;
                mono.code = deck.Main[i];
                mono.RefreshPosition();
                cards.Add(mono);
                yield return null;
            }
            for (int i = 0; i < deck.Extra.Count; i++)
            {
                var card = Instantiate(itemOnTable);
                card.transform.SetParent(cardsOnEditParent, false);
                var mono = card.GetComponent<CardOnEdit>();
                mono.id = i + 1000;
                mono.code = deck.Extra[i];
                mono.RefreshPosition();
                cards.Add(mono);
                yield return null;
            }
            for (int i = 0; i < deck.Side.Count; i++)
            {
                var card = Instantiate(itemOnTable);
                card.transform.SetParent(cardsOnEditParent, false);
                var mono = card.GetComponent<CardOnEdit>();
                mono.id = i + 2000;
                mono.code = deck.Side[i];
                mono.RefreshPosition();
                cards.Add(mono);
                yield return null;
            }
            loaded = true;
            dirty = false;
        }

        IEnumerator RefreshIcons()
        {
            manager.GetElement<Image>("IconCase").color = Color.clear;
            manager.GetElement<Image>("IconProtector").color = Color.clear;
            manager.GetElement<Image>("IconField").color = Color.clear;
            manager.GetElement<Image>("IconGrave").color = Color.clear;
            manager.GetElement<Image>("IconStand").color = Color.clear;
            manager.GetElement<Image>("IconMate").color = Color.clear;

            yield return null;
            manager.GetElement<Tabs>("List").AdjustSize();

            var ie = TextureManager.LoadItemIcon(deck.Case[0].ToString());
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager.GetElement<Image>("IconCase").color = Color.white;
            manager.GetElement<Image>("IconCase").sprite = ie.Current;

            var im = ABLoader.LoadProtectorMaterial(deck.Protector[0].ToString());
            StartCoroutine(im);
            while (im.MoveNext())
                yield return null;
            manager.GetElement<Image>("IconProtector").color = Color.white;
            manager.GetElement<Image>("IconProtector").material = im.Current;

            ie = TextureManager.LoadItemIcon(deck.Field[0].ToString());
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager.GetElement<Image>("IconField").color = Color.white;
            manager.GetElement<Image>("IconField").sprite = ie.Current;

            ie = TextureManager.LoadItemIcon(deck.Grave[0].ToString());
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager.GetElement<Image>("IconGrave").color = Color.white;
            manager.GetElement<Image>("IconGrave").sprite = ie.Current;

            ie = TextureManager.LoadItemIcon(deck.Stand[0].ToString());
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            manager.GetElement<Image>("IconStand").color = Color.white;
            manager.GetElement<Image>("IconStand").sprite = ie.Current;

            var mate = deck.Mate[0].ToString();
            if (mate.Length == 7 && mate.StartsWith("100"))
            {
                ie = TextureManager.LoadItemIcon(mate);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                manager.GetElement<Image>("IconMate").color = Color.white;
                manager.GetElement<Image>("IconMate").sprite = ie.Current;
            }
            else
            {
                var art = Program.I().texture_.LoadArtAsync(deck.Mate[0], true);
                StartCoroutine(art);
                while (art.MoveNext())
                    yield return null;
                manager.GetElement<Image>("IconMate").color = Color.white;
                manager.GetElement<Image>("IconMate").sprite =
                    Sprite.Create(art.Current, new Rect(0, 0, art.Current.width, art.Current.height), new Vector2(0.5f, 0.5f));
            }
        }
        void Dispose()
        {
            foreach (var card in cards)
                Destroy(card.gameObject);
            cards.Clear();
        }

        public void OnRelatedDescripton()
        {
            var cardFace = manager.GetElement<RawImage>("RawImageRelatedCard").texture;
            var mat = manager.GetElement<RawImage>("RawImageRelatedCard").material;
            Description(relatedCard.Id, cardFace, mat);
        }
        Texture showingFace;
        public void Description(int code, Texture cardFace, Material mat, bool inHistory = true)
        {
            var data = CardsManager.Get(code);
            if (data.Id == 0)
                return;
            if (condition == EditDeckCondition.EditDeck && inHistory)
            {
                if (history.Main.Contains(code))
                    history.Main.Remove(code);
                history.Main.Insert(0, code);
                if (manager.GetElement<Tab>("TabHistory").selected)
                    PrintHistoryCards();
            }
            manager.GetElement("Group").SetActive(true);
            cardShowing = data;
            if (showingFace != null)
                Destroy(showingFace);
            showingFace = Instantiate(cardFace);
            manager.GetElement<RawImage>("Card").texture = showingFace;
            manager.GetElement<RawImage>("Card").material = mat;
            manager.GetElement<Text>("TextName").text = data.Name;
            var colors = CardDescription.GetCardFrameColor(data);
            manager.GetElement<Image>("BaseName").color = colors[0];
            manager.GetElement<Image>("BaseType").color = colors[1];
            manager.GetElement<Image>("Attribute").sprite = CardDescription.GetCardAttribute(data).sprite;
            manager.GetElement<Text>("TextType").text = StringHelper.GetType(data);
            if ((data.Type & (uint)CardType.Monster) > 0)
            {
                manager.GetElement("PropertyMonster").SetActive(true);
                manager.GetElement("PropertySpell").SetActive(false);
                manager.GetElement<Image>("Level").sprite = TextureManager.GetCardLevelIcon(data);
                manager.GetElement<Text>("TextAttack").text = data.Attack == -2 ? "?" : data.Attack.ToString();
                manager.GetElement<Image>("Race").sprite = CardDescription.GetCardRace(data).sprite;
                if ((data.Type & (uint)CardType.Pendulum) > 0)
                {
                    var texts = CardDescription.GetCardDescriptionSplit(data.Desc);
                    string monster = InterString.Get("【怪兽效果】");
                    if ((data.Type & (uint)CardType.Effect) == 0)
                        monster = InterString.Get("【怪兽描述】");

                    manager.GetElement<TextMeshProUGUI>("TextDescription").text =
                        CardDescription.GetSetName(data.Id) +
                        InterString.Get("【灵摆效果】") + "\n" + texts[0] + "\n" +
                        monster + "\n" + texts[1];
                    manager.GetElement("Scale").SetActive(true);
                    manager.GetElement("TextScale").SetActive(true);
                    manager.GetElement<Text>("TextScale").text = data.LScale.ToString();
                    manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -90);
                    manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -90);
                    manager.GetElement<RectTransform>("Defense").anchoredPosition = new Vector2(0, -135);
                    manager.GetElement<RectTransform>("TextDefense").anchoredPosition = new Vector2(40, -135);
                }
                else
                {
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = CardDescription.GetSetName(data.Id) + data.Desc;
                    manager.GetElement("Scale").SetActive(false);
                    manager.GetElement("TextScale").SetActive(false);
                    manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -45);
                    manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -45);
                    manager.GetElement<RectTransform>("Defense").anchoredPosition = new Vector2(0, -90);
                    manager.GetElement<RectTransform>("TextDefense").anchoredPosition = new Vector2(40, -90);
                }

                if ((data.Type & (uint)CardType.Link) > 0)
                {
                    manager.GetElement<Text>("TextLevel").text = CardDescription.GetCardLinkCount(data).ToString();
                    manager.GetElement("Defense").SetActive(false);
                    manager.GetElement("TextDefense").SetActive(false);
                    manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -45);
                    manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -45);
                }
                else
                {
                    manager.GetElement<Text>("TextLevel").text = data.Level.ToString();
                    manager.GetElement("Defense").SetActive(true);
                    manager.GetElement("TextDefense").SetActive(true);
                    manager.GetElement<Text>("TextDefense").text = data.Defense == -2 ? "?" : data.Defense.ToString();
                }
            }
            else
            {
                manager.GetElement("PropertyMonster").SetActive(false);
                manager.GetElement("PropertySpell").SetActive(true);
                manager.GetElement<Image>("SpellType").sprite = TextureManager.GetSpellTrapTypeIcon(data);
                manager.GetElement<Text>("TextSpellType").text = StringHelper.SecondType(data.Type) + StringHelper.MainType(data.Type);
                manager.GetElement<TextMeshProUGUI>("TextDescription").text = CardDescription.GetSetName(data.Id) + data.Desc;
            }
            RefreshLimitIcon();
            if (book.Main.Contains(code))
                manager.GetElement<Toggle>("ButtonBook").SwitchOn();
            else
                manager.GetElement<Toggle>("ButtonBook").SwitchOff();
            if (shine.Main.Contains(code))
                manager.GetElement<Toggle>("ButtonSR").SwitchOnWithoutAction();
            else
                manager.GetElement<Toggle>("ButtonSR").SwitchOffWithoutAction();
            if (royal.Main.Contains(code))
                manager.GetElement<Toggle>("ButtonUR").SwitchOnWithoutAction();
            else
                manager.GetElement<Toggle>("ButtonUR").SwitchOffWithoutAction();
        }

        void RefreshLimitIcon()
        {
            if (!manager.GetElement("Group").activeInHierarchy)
                return;

            var limit = banlist.GetQuantity(cardShowing.Id);
            if (limit == 3)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.typeNone;
            else if (limit == 2)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit2;
            else if (limit == 1)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit1;
            else
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.banned;
        }

        void ShowDetail()
        {
            var cardFace = manager.GetElement<RawImage>("Card").texture;
            var mat = manager.GetElement<RawImage>("Card").material;
            detail.Show(cardShowing, cardFace, mat);
        }

        public override void PerFrameFunction()
        {
            if (isShowed)
            {
                if (Program.InputGetMouse1Up)
                {
                    if (detail.showing)
                        detail.Hide();
                    else if (returnAction != null)
                        returnAction();
                }
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    if (detail.showing)
                        detail.Hide();
                    else if (returnAction != null)
                        returnAction();
                    else
                        OnReturn();
                }
            }
        }

        public float descriptionWidth;
        public float tableWidth;
        public float listWidth;
        public float outerWidth;
        public float innerWidth;

        void AdjustSize()
        {
            var uiWidth = Screen.width * 1080f / Screen.height;
            descriptionWidth = 420f;
            tableWidth = 790f;
            listWidth = 550f;//1920
            outerWidth = 50;
            innerWidth = 30;
            if (uiWidth <= 1920)
            {
                if (uiWidth >= 1920 - 80)
                {
                    descriptionWidth -= 1920 - uiWidth;
                }
                else if (uiWidth >= 1920 - 80 - 2 * (50 + 30))
                {
                    descriptionWidth = 420 - 80;
                    float percent = (uiWidth - 1920 + 240) / 160f;
                    outerWidth *= percent;
                    innerWidth *= percent;
                }
                else
                {
                    descriptionWidth = 420 - 80;
                    outerWidth = 0;
                    innerWidth = 0;
                }
            }
            manager.GetElement<RectTransform>("Description").anchoredPosition = new Vector2(outerWidth, -120);
            manager.GetElement<RectTransform>("Description").sizeDelta = new Vector2(descriptionWidth, 900);
            manager.GetElement<RectTransform>("Table").anchoredPosition = new Vector2(outerWidth + descriptionWidth + innerWidth, -120);
            manager.GetElement<RectTransform>("List").anchoredPosition = new Vector2(outerWidth + descriptionWidth + innerWidth + tableWidth + innerWidth, -180);
            listWidth = uiWidth - (outerWidth * 2 + descriptionWidth + innerWidth * 2 + tableWidth);
            manager.GetElement<RectTransform>("List").sizeDelta = new Vector2(listWidth, 840);

            var startX = 810f;
            var space = 20f;
            var fullWidth = uiWidth - startX - 30 - space * 5;
            var buttonWidth = fullWidth / 6;
            manager.GetElement<RectTransform>("ButtonDeckReset").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonDeckSort").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonDeckRandom").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonDeckCopy").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonDeckShare").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonDeckSave").sizeDelta = new Vector2(buttonWidth, 62);
            manager.GetElement<RectTransform>("ButtonChangeSide").sizeDelta = new Vector2(buttonWidth * 4 + space * 3, 62);

            manager.GetElement<RectTransform>("ButtonDeckReset").anchoredPosition = new Vector2(startX, -34);
            manager.GetElement<RectTransform>("ButtonDeckSort").anchoredPosition = new Vector2(startX + buttonWidth + space, -34);
            manager.GetElement<RectTransform>("ButtonDeckRandom").anchoredPosition = new Vector2(startX + (buttonWidth + space) * 2, -34);
            manager.GetElement<RectTransform>("ButtonDeckCopy").anchoredPosition = new Vector2(startX + (buttonWidth + space) * 3, -34);
            manager.GetElement<RectTransform>("ButtonDeckShare").anchoredPosition = new Vector2(startX + (buttonWidth + space) * 4, -34);
            manager.GetElement<RectTransform>("ButtonDeckSave").anchoredPosition = new Vector2(startX + (buttonWidth + space) * 5, -34);
            manager.GetElement<RectTransform>("ButtonChangeSide").anchoredPosition = new Vector2(startX + (buttonWidth + space) * 2, -34);

            foreach (var card in cards)
                card.RefreshPositionInstant();

            uiWidth = manager.GetElement<RectTransform>("List").sizeDelta.x - 40;
            if (uiWidth < 0) uiWidth = 0;
            manager.GetElement<RectTransform>("ButtonFilter").sizeDelta = new Vector2(uiWidth / 3f, 60);
            manager.GetElement<RectTransform>("ButtonSort").sizeDelta = new Vector2(uiWidth / 3f, 60);
            manager.GetElement<RectTransform>("ButtonReset").sizeDelta = new Vector2(uiWidth / 3f, 60);

            ScrollViewInstall();
        }

        void OnList()
        {
            manager.GetElement<RectTransform>("ScrollView").sizeDelta = new Vector2(0, 680);

            if (relatedCards.Count == 0)
            {
                manager.GetElement("SearchComponents").SetActive(true);
                manager.GetElement("RelatedComponents").SetActive(false);
                if (isShowed)
                    OnClickSearch();
            }
            else
            {
                manager.GetElement("SearchComponents").SetActive(false);
                manager.GetElement("RelatedComponents").SetActive(true);
                PrintCards(relatedCards);
            }
        }
        void OnBook()
        {
            manager.GetElement("SearchComponents").SetActive(false);
            manager.GetElement("RelatedComponents").SetActive(false);
            manager.GetElement<RectTransform>("ScrollView").sizeDelta = new Vector2(0, 820);
            PrintBookedCards();
        }
        void OnHistory()
        {
            manager.GetElement("SearchComponents").SetActive(false);
            manager.GetElement("RelatedComponents").SetActive(false);
            manager.GetElement<RectTransform>("ScrollView").sizeDelta = new Vector2(0, 820);
            PrintHistoryCards();
        }
        void ShowAppearance()
        {
            if (!loaded)
                return;
            intoAppearance = true;
            Appearance.type = Appearance.AppearanceType.Deck;
            Program.I().ShiftToServant(Program.I().appearance);
        }
        void ShowBanlists()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("禁限卡表")
            };
            foreach (var list in BanlistManager.Banlists)
                selections.Add(list.Name);
            UIManager.ShowPopupSelection(selections, ChangeBanlist);
        }

        void ChangeBanlist()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            banlist = BanlistManager.GetByName(selected);
            manager.GetElement<Text>("TextBanlist").text = selected;
            foreach (var card in cards)
                card.RefreshLimitIcon();
            RefreshLimitIcon();
            RefreshListItemIcons();
        }

        public void RefreshCardID()
        {
            CardOnEdit cardDrag = null;

            foreach (var card in cards)
                if (card.dragging)
                {
                    cardDrag = card;
                    break;
                }
            if (cardDrag == null)
                return;

            CardOnEdit cardHover = null;
            foreach (var card in cards)
                if (card.hover && !card.dragging)
                {
                    cardHover = card;
                    break;
                }
            if (cardHover != null)
            {
                SwitchCard(cardDrag, cardHover);
                dirty = true;
            }
            else
            {
                var c = CardsManager.Get(cardDrag.code);
                var isExtra = c.IsExtraCard();

                if (manager.GetElement<UIHover>("DummyMain").hover)
                {
                    if (cardDrag.id > 1999 && !isExtra)
                    {
                        dirty = true;

                        foreach (var card in cards)
                            if (card.id > cardDrag.id)
                                card.id--;
                        cardDrag.id = mainCount;
                        mainCount++;
                        sideCount--;
                    }
                }
                else if (manager.GetElement<UIHover>("DummyExtra").hover)
                {
                    if (cardDrag.id > 1999 && isExtra)
                    {
                        dirty = true;

                        foreach (var card in cards)
                            if (card.id > cardDrag.id)
                                card.id--;
                        cardDrag.id = extraCount + 1000;
                        extraCount++;
                        sideCount--;
                    }
                }
                else if (manager.GetElement<UIHover>("DummySide").hover)
                {
                    if (cardDrag.id < 1000)
                    {
                        dirty = true;

                        foreach (var card in cards)
                            if (card.id > cardDrag.id && card.id < 1000)
                                card.id--;
                        cardDrag.id = sideCount + 2000;
                        mainCount--;
                        sideCount++;
                    }
                    else if (cardDrag.id > 999 && cardDrag.id < 2000)
                    {
                        dirty = true;

                        foreach (var card in cards)
                            if (card.id > cardDrag.id && card.id < 2000)
                                card.id--;
                        cardDrag.id = sideCount + 2000;
                        extraCount--;
                        sideCount++;
                    }
                }
            }
            foreach (var card in cards)
                card.Move();
            SetCardSiblingIndex(CardOnEdit.moveTime);
        }

        public void SwitchCard(CardOnEdit dragCard, CardOnEdit hoverCard)
        {
            var hover = hoverCard.id;
            if (dragCard.id == 99999999)
            {
                var data = CardsManager.Get(dragCard.code);
                var isExtra = data.IsExtraCard();
                if (!isExtra)
                {
                    if (hover < 1000)
                    {
                        foreach (var card in cards)
                            if (card.id >= hover && card.id < 1000)
                                card.id++;
                        dragCard.id = hover;
                        mainCount++;
                    }
                    else if (hover > 1999)
                    {
                        foreach (var card in cards)
                            if (card.id >= hover)
                                card.id++;
                        dragCard.id = hover;
                        sideCount++;
                    }
                    else
                    {
                        cards.Remove(dragCard);
                        Destroy(dragCard.gameObject);
                    }
                }
                else
                {
                    if (hover < 1000)
                    {
                        cards.Remove(dragCard);
                        Destroy(dragCard.gameObject);
                    }
                    else if (hover > 1999)
                    {
                        foreach (var card in cards)
                            if (card.id >= hover)
                                card.id++;
                        dragCard.id = hover;
                        sideCount++;
                    }
                    else
                    {
                        foreach (var card in cards)
                            if (card.id >= hover && card.id < 2000)
                                card.id++;
                        dragCard.id = hover;
                        extraCount++;
                    }
                }


            }
            else if (dragCard.id < 1000)
            {
                if (hover < 1000)
                {
                    foreach (var card in cards)
                        if (card.id > dragCard.id)
                            card.id--;
                    foreach (var card in cards)
                        if (card.id >= hover)
                            card.id++;
                    dragCard.id = hover;
                }
                else if (hover > 999 && hover < 2000)
                    return;
                else if (hover > 1999)
                {
                    foreach (var card in cards)
                        if (card.id > dragCard.id && card.id < 1000)
                            card.id--;
                    foreach (var card in cards)
                        if (card.id >= hover)
                            card.id++;
                    dragCard.id = hover;
                    Program.I().editDeck.mainCount--;
                    Program.I().editDeck.sideCount++;
                }
            }
            else if (dragCard.id > 999 && dragCard.id < 2000)
            {
                if (hover < 1000)
                    return;
                else if (hover > 999 && hover < 2000)
                {
                    foreach (var card in cards)
                        if (card.id > dragCard.id)
                            card.id--;
                    foreach (var card in cards)
                        if (card.id >= hover)
                            card.id++;
                    dragCard.id = hover;
                }
                else if (hover > 1999)
                {
                    foreach (var card in cards)
                        if (card.id > dragCard.id && card.id > 999 && card.id < 2000)
                            card.id--;
                    foreach (var card in cards)
                        if (card.id >= hover)
                            card.id++;
                    dragCard.id = hover;
                    Program.I().editDeck.extraCount--;
                    Program.I().editDeck.sideCount++;
                }
            }
            else if (dragCard.id > 1999)
            {
                var c = CardsManager.Get(dragCard.code);
                var isExtra = c.IsExtraCard();

                if (hover < 1000)
                {
                    if (!isExtra)
                    {
                        foreach (var card in cards)
                            if (card.id > dragCard.id)
                                card.id--;
                        foreach (var card in cards)
                            if (card.id >= hover && card.id < 1000)
                                card.id++;
                        dragCard.id = hover;
                        Program.I().editDeck.mainCount++;
                        Program.I().editDeck.sideCount--;
                    }
                }
                else if (hover > 999 && hover < 2000)
                {
                    if (isExtra)
                    {
                        foreach (var card in cards)
                            if (card.id > dragCard.id)
                                card.id--;
                        foreach (var card in cards)
                            if (card.id >= hover && card.id < 2000)
                                card.id++;
                        dragCard.id = hover;
                        Program.I().editDeck.extraCount++;
                        Program.I().editDeck.sideCount--;
                    }
                }
                else if (hover > 1999)
                {
                    foreach (var card in cards)
                        if (card.id > dragCard.id)
                            card.id--;
                    foreach (var card in cards)
                        if (card.id >= hover)
                            card.id++;
                    dragCard.id = hover;
                }
            }
        }

        public void SetCardSiblingIndex(float delay)
        {
            DOTween.To(v => { }, 0, 0, delay).OnComplete(() =>
            {
                cards.Sort((x, y) => x.id.CompareTo(y.id));
                for (int i = 0; i < cards.Count; i++)
                    cards[i].transform.SetSiblingIndex(i);
            });
        }

        public void DeleteCard(CardOnEdit card)
        {
            if (condition == EditDeckCondition.ChangeSide)
                return;

            dirty = true;
            AudioManager.PlaySE("SE_DECK_MINUS");

            card.transform.SetSiblingIndex(cards.Count - 1);
            cards.Remove(card);
            Destroy(card.gameObject, 0.4f);
            Vector3 end;
            if (manager.GetElement<Tab>("TabList").selected)
            {
                end = manager.GetElement<Transform>("ScrollView").GetChild(0).position;
            }
            else
            {
                end = manager.GetElement<Transform>("TabList").GetChild(0).position;
            }
            var sequence = DOTween.Sequence();
            sequence.Append(card.transform.DOMove(end, 0.2f));
            sequence.Join(card.transform.DOScale(Vector3.one * 1.5f, 0.2f));
            sequence.Append(card.GetComponent<CanvasGroup>().DOFade(0, 0.2f));
            sequence.Join(card.transform.DOScale(Vector3.one * 0.7f, 0.2f));

            if (card.id < 1000)
            {
                foreach (var c in cards)
                    if (c.id > card.id && c.id < 1000)
                        c.id--;
                mainCount--;
            }
            else if (card.id > 999 && card.id < 2000)
            {
                foreach (var c in cards)
                    if (c.id > card.id && c.id > 999 && c.id < 2000)
                        c.id--;
                extraCount--;
            }
            else if (card.id > 1999)
            {
                foreach (var c in cards)
                    if (c.id > card.id && c.id > 1999)
                        c.id--;
                sideCount--;
            }
            foreach (var c in cards)
                c.Move();
            SetCardSiblingIndex(0);
            RefreshListItemIcons();
        }

        public void OnReset()
        {
            if (!loaded)
                return;
            Dispose();
            StartCoroutine(RefreshAsync());
        }
        public void OnSort()
        {
            dirty = true;

            List<CardOnEdit> main = new List<CardOnEdit>();
            List<CardOnEdit> extra = new List<CardOnEdit>();
            List<CardOnEdit> side = new List<CardOnEdit>();
            foreach (var card in cards)
            {
                if (card.id < 1000)
                    main.Add(card);
                else if (card.id > 1999)
                    side.Add(card);
                else
                    extra.Add(card);
            }
            main.Sort((left, right) =>
            {
                return CardsManager.ComparisonOfCard()
                (CardsManager.Get(left.code), CardsManager.Get(right.code));
            });
            for (int i = 0; i < main.Count; i++)
                main[i].id = i;
            extra.Sort((left, right) =>
            {
                return CardsManager.ComparisonOfCard()
                (CardsManager.Get(left.code), CardsManager.Get(right.code));
            });
            for (int i = 0; i < extra.Count; i++)
                extra[i].id = i + 1000;
            side.Sort((left, right) =>
            {
                return CardsManager.ComparisonOfCard()
                (CardsManager.Get(left.code), CardsManager.Get(right.code));
            });
            for (int i = 0; i < side.Count; i++)
                side[i].id = i + 2000;
            foreach (var card in cards)
                card.Move();
            SetCardSiblingIndex(0);
        }
        public void OnRandom()
        {
            dirty = true;

            List<CardOnEdit> main = new List<CardOnEdit>();
            foreach (var card in cards)
                if (card.id < 1000)
                    main.Add(card);
            System.Random rand = new System.Random();
            for (int i = 0; i < main.Count; i++)
            {
                int random_index = rand.Next() % main.Count;
                var buffer = main[i];
                main[i] = main[random_index];
                main[random_index] = buffer;
            }
            for (int i = 0; i < main.Count; i++)
                main[i].id = i;
            foreach (var card in cards)
                card.Move();
            SetCardSiblingIndex(0);
        }
        public void OnCopy()
        {
            dirty = true;

            deckName += " - " + InterString.Get("复制");
            input.text = deckName;
        }
        public void OnShare()
        {
            if(dirty || !File.Exists("Deck/" + deckName + ".ydk"))
            {
                MessageManager.Cast(InterString.Get("请先保存卡组。"));
                return;
            }

            //#if UNITY_ANDROID && !UNITY_EDITOR
            //            new NativeShare().SetText(File.ReadAllText("Deck/" + deckName + ".ydk")).Share();
            //#else
            //            Tools.TryOpenInFileExplorer(Path.GetFullPath("Deck/" + deckName + ".ydk"));
            //#endif
            var url = DeckShareURL.DeckToUri(deck.Main, deck.Extra, deck.Side).ToString();
            GUIUtility.systemCopyBuffer = url;
            Application.OpenURL(url);
        }
        public void OnSave()
        {
            if (manager.GetElement<Text>("TextBanlist").text != "N/A")
            {
                if (mainCount > 60 || extraCount > 15 || sideCount > 15)
                {
                    List<string> tasks = new List<string>();
                    tasks.Add(InterString.Get("保存失败"));
                    tasks.Add(InterString.Get("卡组内卡片张数超过限制。@n如需无视限制，请将禁限卡表设置为无（N/A）。"));
                    UIManager.ShowPopupConfirm(tasks);
                    return;
                }
            }
            deck = FromObjectDeckToCodedDeck();

            FileSave();
            if (returnAction != null)
                OnExit();
        }
        public void OnChangeSideComplete()
        {
            TcpHelper.CtosMessage_UpdateDeck(FromObjectDeckToCodedDeck());
        }

        Deck FromObjectDeckToCodedDeck()
        {
            cards.Sort((left, right) =>
            {
                if (left.id < right.id) return -1;
                if (left.id > right.id) return 1;
                return 0;
            });
            var deck = new Deck();
            foreach (var card in cards)
            {
                if (card.id < 1000)
                    deck.Main.Add(card.code);
                else if (card.id > 1999)
                    deck.Side.Add(card.code);
                else
                    deck.Extra.Add(card.code);
            }
            foreach (var pickup in this.deck.Pickup)
                deck.Pickup.Add(pickup);
            deck.Protector.Add(this.deck.Protector[0]);
            deck.Case.Add(this.deck.Case[0]);
            deck.Field.Add(this.deck.Field[0]);
            deck.Grave.Add(this.deck.Grave[0]);
            deck.Stand.Add(this.deck.Stand[0]);
            deck.Mate.Add(this.deck.Mate[0]);
            return deck;
        }

        void FileSave()
        {
            try
            {
                SaveDeckFile(deck, input.text);
                if (input.text != deckName)
                    File.Delete("Deck/" + deckName + ".ydk");
                deckName = input.text;
                MessageManager.Cast(InterString.Get("卡组「[?]」已保存。", input.text));
                dirty = false;
            }
            catch
            {
                MessageManager.Cast(InterString.Get("保存失败！"));
            }
        }

        public void SaveDeckFile(Deck deck, string deckName)
        {
            var value = "#created by mdpro3\r\n#main\r\n";
            for (var i = 0; i < deck.Main.Count; i++) value += deck.Main[i] + "\r\n";
            value += "#extra\r\n";
            for (var i = 0; i < deck.Extra.Count; i++) value += deck.Extra[i] + "\r\n";
            value += "!side\r\n";
            for (var i = 0; i < deck.Side.Count; i++) value += deck.Side[i] + "\r\n";
            value += "#pickup\r\n";
            for (var i = 0; i < deck.Pickup.Count; i++) value += deck.Pickup[i] + "#\r\n";
            value += "#case\r\n";
            for (var i = 0; i < deck.Case.Count; i++) value += deck.Case[i] + "#\r\n";
            value += "#protector\r\n";
            for (var i = 0; i < deck.Protector.Count; i++) value += deck.Protector[i] + "#\r\n";
            value += "#field\r\n";
            for (var i = 0; i < deck.Field.Count; i++) value += deck.Field[i] + "#\r\n";
            value += "#grave\r\n";
            for (var i = 0; i < deck.Grave.Count; i++) value += deck.Grave[i] + "#\r\n";
            value += "#stand\r\n";
            for (var i = 0; i < deck.Stand.Count; i++) value += deck.Stand[i] + "#\r\n";
            value += "#mate\r\n";
            for (var i = 0; i < deck.Mate.Count; i++) value += deck.Mate[i] + "#\r\n";

            try
            {
                File.WriteAllText("Deck/" + deckName + ".ydk", value, Encoding.UTF8);
                Config.Set("DeckInUse", deckName);
            }
            catch
            {
                MessageManager.Cast(InterString.Get("保存失败！"));
            }
        }

        public void OnPlusOne()
        {
            if (condition == EditDeckCondition.ChangeSide)
                return;
            if (GetCardCount(cardShowing.Id) >= banlist.GetQuantity(cardShowing.Id))
                return;
            AudioManager.PlaySE("SE_DECK_PLUS");

            var card = Instantiate(itemOnTable);
            card.transform.SetParent(cardsOnEditParent, false);
            var mono = card.GetComponent<CardOnEdit>();

            if (!cardShowing.IsExtraCard())
            {
                if (mainCount < 60)
                {
                    mono.id = mainCount;
                    mainCount++;
                }
                else
                {
                    mono.id = sideCount + 2000;
                    sideCount++;
                }
            }
            else
            {
                if (extraCount < 15)
                {
                    mono.id = extraCount + 1000;
                    extraCount++;
                }
                else
                {
                    mono.id = sideCount + 2000;
                    sideCount++;
                }
            }
            mono.code = cardShowing.Id;
            mono.RefreshPosition();
            cards.Add(mono);
            foreach (var c in cards)
                c.Move();
            SetCardSiblingIndex(0);
            RefreshListItemIcons();
        }
        public void OnMinusOne()
        {
            if (condition == EditDeckCondition.ChangeSide)
                return;
            foreach (var c in cards)
            {
                var card = CardsManager.Get(c.code);
                if (cardShowing.Alias == 0)
                {
                    if (card.Id == cardShowing.Id || card.Alias == cardShowing.Id)
                    {
                        DeleteCard(c);
                        break;
                    }
                }
                else
                {
                    if (card.Id == cardShowing.Alias || card.Alias == cardShowing.Alias)
                    {
                        DeleteCard(c);
                        break;
                    }
                }
            }
        }
        public int GetCardCount(int code)
        {
            var data = CardsManager.Get(code);
            if (data == null) return 0;
            var alias = data.Alias;
            int count = 0;
            foreach (var card in cards)
            {
                var c = CardsManager.Get(card.code);
                if (c == null)
                    break;
                if (alias == 0)
                {
                    if (c.Id == code || c.Alias == code)
                        count++;
                }
                else
                {
                    if (c.Id == alias || c.Alias == alias)
                        count++;
                }
            }
            return count;
        }

        public void OnDeckNameChange()
        {
            dirty = true;
        }

        void ScrollViewInstall()
        {
            superScrollView?.Clear();

            var defau = 1000f;
#if UNITY_ANDROID
            defau = 1500f;
#endif
            var scale = float.Parse(Config.Get("UIScale", defau.ToString())) / 1000;


            superScrollView = new SuperScrollView
            (
            (int)Math.Floor((manager.GetElement<RectTransform>("ScrollView").rect.width - 30f) / (86f * scale)),
            86 * scale,
            140 * scale,
            0,
            0,
            itemOnList,
            ItemOnListRefresh,
            manager.GetElement<ScrollRect>("ScrollView")
            );

            manager.GetElement<Text>("LabelSearch").text = InterString.Get("搜索");

            if (manager.GetElement<Tab>("TabBook").selected)
                PrintBookedCards();
            else if (manager.GetElement<Tab>("TabHistory").selected)
                PrintHistoryCards();
            else
            {
                if (relatedCards.Count > 0)
                    PrintCards(relatedCards);
            }
        }

        void OnSearch(string search)//For Input Field
        {
            OnClickSearch();
        }
        public void OnClickSearch()
        {
            List<int> cards = new List<int>();
            var result = CardsManager.Search(manager.GetElement<InputField>("InputSearch").text, filters, banlist, pack);
            switch (sortOrder)
            {
                case SortOrder.ByType:
                    result.Sort(CardsManager.ComparisonOfCard());
                    break;
                case SortOrder.ByTypeReverse:
                    result.Sort(CardsManager.ComparisonOfCardReverse());
                    break;
                case SortOrder.ByLevelUp:
                    result.Sort(CardsManager.ComparisonOfCard_LV_Up());
                    break;
                case SortOrder.ByLevelDown:
                    result.Sort(CardsManager.ComparisonOfCard_LV_Down());
                    break;
                case SortOrder.ByAttackUp:
                    result.Sort(CardsManager.ComparisonOfCard_ATK_Up());
                    break;
                case SortOrder.ByAttackDown:
                    result.Sort(CardsManager.ComparisonOfCard_ATK_Down());
                    break;
                case SortOrder.ByDefenceUp:
                    result.Sort(CardsManager.ComparisonOfCard_DEF_Up());
                    break;
                case SortOrder.ByDefenceDown:
                    result.Sort(CardsManager.ComparisonOfCard_DEF_Down());
                    break;
                case SortOrder.ByRarityUp:
                    result.Sort(CardsManager.ComparisonOfCard_Rarity_Up());
                    break;
                case SortOrder.ByRarityDown:
                    result.Sort(CardsManager.ComparisonOfCard_Rarity_Down());
                    break;
            }
            foreach (var card in result)
                cards.Add(card.Id);
            manager.GetElement<Text>("LabelSearch").text = cards.Count.ToString();
            PrintCards(cards);
        }

        public enum SortOrder
        {
            ByType = 1,
            ByTypeReverse = 2,
            ByLevelUp = 3,
            ByLevelDown = 4,
            ByAttackUp = 5,
            ByAttackDown = 6,
            ByDefenceUp = 7,
            ByDefenceDown = 8,
            ByRarityUp = 9,
            ByRarityDown = 10
        }
        public SortOrder sortOrder = SortOrder.ByType;
        public void OnSearchSort()
        {
            var handle = Addressables.InstantiateAsync("PopupSearchOrder");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                result.Result.GetComponent<PopupSearchOrder>().Show();
            };
        }


        void PrintCards(List<int> codes)
        {
            var args = new List<string[]>();
            for (int i = 0; i < codes.Count; i++)
            {
                string[] arg = new string[1] { codes[i].ToString() };
                args.Add(arg);
            }
            superScrollView.Print(args);
        }

        void PrintBookedCards()
        {
            var list = new List<int>();
            foreach (var card in book.Main)
                list.Add(card);
            PrintCards(list);
        }
        void PrintHistoryCards()
        {
            var list = new List<int>();
            foreach (var card in history.Main)
                list.Add(card);
            PrintCards(list);
        }
        void ItemOnListRefresh(string[] tasks, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemForDeckEdit>();
            handler.code = int.Parse(tasks[0].ToString());
            handler.Refresh();
        }

        public void RefreshListItemIcons()
        {
            if (superScrollView != null)
            {
                foreach (var item in superScrollView.items)
                {
                    if (item.gameObject != null)
                    {
                        var handler = item.gameObject.GetComponent<SuperScrollViewItemForDeckEdit>();
                        handler.RefreshCountDot();
                        handler.RefreshLimiteIcon();
                    }
                }
            }
        }

        public void BookCard()
        {
            if (book.Main.Contains(cardShowing.Id))
            {
                book.Main.Remove(cardShowing.Id);
                AudioManager.PlaySE("SE_MENU_S_DECIDE_02");
            }
            else
            {
                book.Main.Add(cardShowing.Id);
                AudioManager.PlaySE("SE_MENU_S_DECIDE_01");
            }

            List<Card> cards = new List<Card>();
            foreach (var code in book.Main)
                cards.Add(CardsManager.Get(code));
            cards.Sort(CardsManager.ComparisonOfCard());
            book.Main.Clear();
            foreach (var card in cards)
                book.Main.Add(card.Id);

            if (manager.GetElement<Tab>("TabBook").selected)
                PrintBookedCards();
        }
        Card relatedCard;
        List<int> relatedCards = new List<int>();
        public void OnRelated()
        {
            relatedCard = CardsManager.Get(cardShowing.Id);
            var related = CardsManager.RelatedSearch(cardShowing.Id);
            relatedCards = new List<int>();
            foreach (var card in related)
                relatedCards.Add(card.Id);
            manager.GetElement<Tab>("TabList").TabThis();

            manager.GetElement("SearchComponents").SetActive(false);
            manager.GetElement("RelatedComponents").SetActive(true);
            manager.GetElement<RawImage>("RawImageRelatedCard").texture =
                Instantiate(manager.GetElement<RawImage>("Card").texture);
            manager.GetElement<RawImage>("RawImageRelatedCard").material =
                Instantiate(manager.GetElement<RawImage>("Card").material);
            manager.GetElement<Text>("TextRelatedCard").text = InterString.Get("「[?]」的相关卡片", relatedCard.Name);

            PrintCards(relatedCards);
        }

        public void OnRelatedReturn()
        {
            manager.GetElement("SearchComponents").SetActive(true);
            manager.GetElement("RelatedComponents").SetActive(false);
            relatedCards.Clear();
            ScrollViewInstall();
        }

        public List<long> filters = new List<long>();
        public void OnFilter()
        {
            UIManager.ShowPopupFilter();
        }
        public void OnFilterReset()
        {
            filters.Clear();
            pack = "";
            manager.GetElement<InputField>("InputSearch").text = "";
            FilterButtonSwitch(false);
            OnClickSearch();
        }

        public void FilterButtonSwitch(bool on)
        {
            if (on)
            {
                manager.GetElement<Image>("ButtonFilter").sprite = TextureManager.container.toggleM_On;
                var state = manager.GetElement<Button>("ButtonFilter").spriteState;
                state.highlightedSprite = TextureManager.container.toggleM_On;
                state.pressedSprite = TextureManager.container.toggleM_On;
                manager.GetElement<Button>("ButtonFilter").spriteState = state;
                manager.GetElement<Transform>("ButtonFilter").GetChild(0).GetComponent<Image>().color = Color.black;
            }
            else
            {
                manager.GetElement<Image>("ButtonFilter").sprite = TextureManager.container.toggleM;
                var state = manager.GetElement<Button>("ButtonFilter").spriteState;
                state.highlightedSprite = TextureManager.container.toggleM_Over;
                state.pressedSprite = TextureManager.container.toggleM_Over;
                manager.GetElement<Button>("ButtonFilter").spriteState = state;
                manager.GetElement<Transform>("ButtonFilter").GetChild(0).GetComponent<Image>().color = Color.white;
            }
        }

        public void ChangeRarity(int rarity)
        {
            CardRarity cardRarity = rarity == 1 ? CardRarity.Shine : CardRarity.Royal;
            if (cardRarity == CardRarity.Shine)
            {
                manager.GetElement<Toggle>("ButtonUR").SwitchOffWithoutAction();
                royal.Main.Remove(cardShowing.Id);
                if (shine.Main.Contains(cardShowing.Id))
                {
                    AudioManager.PlaySE("SE_MENU_S_DECIDE_02");
                    shine.Main.Remove(cardShowing.Id);
                    UpdateRarity();
                }
                else
                {
                    AudioManager.PlaySE("SE_MENU_S_DECIDE_01");
                    shine.Main.Add(cardShowing.Id);
                    UpdateRarity();
                }
            }
            else
            {
                manager.GetElement<Toggle>("ButtonSR").SwitchOffWithoutAction();
                shine.Main.Remove(cardShowing.Id);
                if (royal.Main.Contains(cardShowing.Id))
                {
                    AudioManager.PlaySE("SE_MENU_S_DECIDE_02");
                    royal.Main.Remove(cardShowing.Id);
                    UpdateRarity();
                }
                else
                {
                    AudioManager.PlaySE("SE_MENU_S_DECIDE_01");
                    royal.Main.Add(cardShowing.Id);
                    UpdateRarity();
                }
            }
        }

        void UpdateRarity()
        {
            Material mat = TextureManager.GetCardMaterial(cardShowing.Id);
            manager.GetElement<RawImage>("Card").material = mat;
            if (relatedCard != null && relatedCard.Id == cardShowing.Id)
                manager.GetElement<RawImage>("RawImageRelatedCard").material = mat;
            foreach (var card in cards)
                if (card.code == cardShowing.Id)
                    card.gameObject.GetComponent<RawImage>().material = mat;
            foreach (var item in superScrollView.items)
                if (item.gameObject != null)
                    if (item.gameObject.GetComponent<SuperScrollViewItemForDeckEdit>().code == cardShowing.Id)
                        item.gameObject.GetComponent<RawImage>().material = mat;
        }

        public static CardRarity GetRarity(int code)
        {
            var rarity = CardRarity.Normal;
            if (Program.I().editDeck.shine.Main.Contains(code))
                rarity = CardRarity.Shine;
            else if (Program.I().editDeck.royal.Main.Contains(code))
                rarity = CardRarity.Royal;
            return rarity;
        }

        public enum EditDeckCondition
        {
            EditDeck,
            ChangeSide
        }
        public EditDeckCondition condition = EditDeckCondition.EditDeck;
        public void ChangeCondition(EditDeckCondition condition)
        {
            if (condition == EditDeckCondition.EditDeck)
            {
                manager.GetElement("ButtonChangeSide").SetActive(false);
                manager.GetElement("ButtonAppearance").SetActive(true);
                deck = new Deck($"Deck/{deckName}.ydk");
                history = new Deck();
                //tabs.tabs[0].TabThis();
            }
            else if (condition == EditDeckCondition.ChangeSide)
            {
                manager.GetElement("ButtonChangeSide").SetActive(true);
                manager.GetElement("ButtonAppearance").SetActive(false);
                deck = TcpHelper.deck;
                history = Program.I().ocgcore.sideReference;
                tabs.tabs[2].TabThis();
            }
        }
    }
}
