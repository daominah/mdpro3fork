using DG.Tweening;
using MDPro3.Net;
using MDPro3.UI;
using MDPro3.UI.PropertyOverrider;
using MDPro3.YGOSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using static MDPro3.UI.DeckView;

namespace MDPro3
{
    public class DeckEditor : Servant
    {
        #region Elements

        private const string LABEL_RT_DRAGCARD = "DragCard";
        private RectTransform m_DragCard;
        private RectTransform DragCard =>
            m_DragCard = m_DragCard != null ? m_DragCard
            : managerUI.GetElement<RectTransform>(LABEL_RT_DRAGCARD);

        private const string LABEL_RIMG_DRAGCARDIMAGE = "DragCard/ImageCard";
        private RawImage m_DragCardImage;
        private RawImage DragCardImage =>
            m_DragCardImage = m_DragCardImage != null ? m_DragCardImage
            : managerUI.GetNestedElement<RawImage>(LABEL_RIMG_DRAGCARDIMAGE);

        #endregion

        #region Reference
        public static Deck Deck { get; set; }
        public static string DeckName { get; set; }
        public static bool DeckIsFromLocal;
        public static Banlist banlist;
        public static List<int> historyCards;
        public static bool useMobileLayout;
        public static string onlineDeckID;
        private static bool deckLiked;

        private ElementObjectManager managerUI;
        public ElementObjectManager managerOverHeader;
        private ElementObjectManager managerHeader;

        public DeckView deckView;
        public CardCollectionView cardCollectionView;
        private CardDetailView cardDetailView;
        private CardActionMenu cardActionMenu;

        //private bool needSave;

        public enum CardInfoType
        {
            None = 0,
            Detail = 1,
            Pool = 2
        }

        public static CardInfoType _CardInfoType = CardInfoType.None;

        public enum ResponseRegion
        {
            Deck,
            Collection,
            Action
        }

        private ResponseRegion _responseRegion;
        public ResponseRegion _ResponseRegion
        {
            get { return _responseRegion; }
            set
            {
                _responseRegion = value;
                ShiftToResponseRegion();
            }
        }
        private void ShiftToResponseRegion()
        {
            deckView.SetCursor(_ResponseRegion == ResponseRegion.Deck);
            cardCollectionView.SetCursor(_ResponseRegion == ResponseRegion.Collection);
        }

        public enum Condition
        {
            EditDeck,
            OnlineDeck,
            ReplayDeck,
            ChangeSide
        }
        public static Condition condition = Condition.EditDeck;
        public void SwitchCondition(Condition condition, string deckName = "", Deck deck = null)
        {
            DeckEditor.condition = condition;
            switch (condition)
            {
                case Condition.EditDeck:
                    returnServant = Program.instance.selectDeck;
                    DeckName = Config.Get("DeckInUse", "@ui");
                    Deck = new Deck(Program.deckPath + DeckName + Program.ydkExpansion);
                    DeckIsFromLocal = true;
                    historyCards = new();
                    break;
                case Condition.OnlineDeck:
                    returnServant = Program.instance.onlineDeckViewer;
                    DeckName = deckName;
                    Deck = null;
                    DeckIsFromLocal = false;
                    historyCards = new();
                    break;
                case Condition.ReplayDeck:
                    returnServant = Program.instance.replay;
                    DeckName = deckName;
                    Deck = deck;
                    DeckIsFromLocal = false;
                    historyCards = new();
                    break;
                case Condition.ChangeSide:
                    DeckName = Config.Get("DeckInUse", "@ui");
                    Deck = TcpHelper.deck;
                    DeckIsFromLocal = false;
                    historyCards = Program.instance.ocgcore.sideReference.Main;
                    break;
            }
        }

        #endregion

        #region Servant
        [HideInInspector] public SelectionButton_CardInDeck lastSelectedCardInDeck;
        [HideInInspector] public SelectionButton_CardInCollection lastSelectedCardInCollection;
        private TMP_InputField inputDeckName;
        private TMP_InputField inputSearch;
        private bool gotoAppearance;

        public override void Initialize()
        {
            SystemEvent.OnResolutionChange += ChangeCanvasMatch;
            transitionTime = 0.6f;
            showLine = false;
            needExit = false;
            depth = 5;
            returnServant = Program.instance.selectDeck;

            base.Initialize();

            banlist = BanlistManager.Banlists[0];
        }

        protected override void ApplyShowArrangement(int preDepth)
        {
            if (!gotoAppearance)
            {
                useMobileLayout = PropertyOverrider.NeedMobileLayout();
                var address = useMobileLayout ? "DeckEditUIMobile" : "DeckEditUI";
                var handle = Addressables.InstantiateAsync(address);
                handle.Completed += (result) =>
                {
                    UIManager.Translate(result.Result);
                    result.Result.transform.SetParent(transform, false);
                    base.ApplyShowArrangement(preDepth);
                    UIManager.SetCanvasMatch(GetCanvasMatch(), transitionTime);

                    managerUI = transform.GetChild(0).GetComponent<ElementObjectManager>();
                    managerOverHeader = managerUI.GetElement<ElementObjectManager>("OverHeader");
                    managerHeader = managerUI.GetElement<ElementObjectManager>("Header");
                    //managerFooter = managerUI.GetElement<ElementObjectManager>("TemplateFooterDesc");

                    deckView = managerUI.GetElement<DeckView>("DeckView");
                    InitializeDeckView();
                    cardCollectionView = managerUI.GetElement<CardCollectionView>("CardCollectionView");
                    InitializeCardCollectionView();
                    cardActionMenu = managerUI.GetElement<CardActionMenu>("CardActionMenu");
                    InitializeCardActionMenu();
                    cardDetailView = managerUI.GetElement<CardDetailView>("CardDetailView");
                    InitializeCardDetailView();

                    InitializeOverHeader();
                    InitializeHeader();

                    if (condition != Condition.ChangeSide)
                        ShowBackButton();
                    else
                        HideBackButton();
                };
            }
            else
            {
                gotoAppearance = false;

                base.ApplyShowArrangement(preDepth);
                ShowBackButton();
            }
        }

        protected override void ApplyHideArrangement(int preDepth)
        {
            base.ApplyHideArrangement(preDepth);
            HideBackButton();
            if (!gotoAppearance)
            {
                UIManager.SetCanvasMatch(1f, transitionTime);
                CardRarity.Save();
                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    Dispose();
                });
            }
        }

        private void Dispose()
        {
            Destroy(transform.GetChild(0).gameObject);
            if(loadOnlineDeckCoroutine != null)
                StopCoroutine(loadOnlineDeckCoroutine);
            callExit = false;
            deckLiked = false;
        }

        public override void PerFrameFunction()
        {
            if (!NeedResponseInput())
                return;

            if (UserInput.WasRightShoulderPressing)
            {
                if (UserInput.WasGamepadButtonNorthPressed)
                    OnBanlist();
                else if (UserInput.WasGamepadButtonWestPressed)
                    SetCardInfoType();
                else if (UserInput.WasGamepadStartPressed)
                    ShiftToAppearance();
                return;
            }

            if (UserInput.WasCancelPressed && condition != Condition.ChangeSide)
                OnReturn();

            if (UserInput.WasGamepadSelectPressed)
            {
                if(condition == Condition.ChangeSide)
                    OnChangeSideComplete();
                else
                    OnSave();
            }

            if (UserInput.WasGamepadStartPressed)
                OnSubMenu();

            if (UserInput.WasLeftTriggerPressed)
                ShowCardActionMenu();

            if (UserInput.WasRightTriggerPressed)
            {
                if (_ResponseRegion == ResponseRegion.Deck)
                    SelectLastCollectionViewItem();
                else if (_ResponseRegion == ResponseRegion.Collection)
                    SelectLastDeckViewItem();
            }


            if (_ResponseRegion == ResponseRegion.Deck)
            {
                if (UserInput.WasGamepadButtonNorthPressed)
                    inputDeckName.ActivateInputField();
                else if (UserInput.WasGamepadButtonWestPressed)
                    OnDeckButtonClicked();
            }
            else if (_ResponseRegion == ResponseRegion.Collection)
            {
                if (cardCollectionView.area == CardCollectionView.Area.Collection)
                {
                    if (UserInput.WasLeftStickPressed)
                        cardCollectionView.PrintSearchCards();

                    if (cardCollectionView.showingRelatedCards)
                        return;

                    if (UserInput.WasGamepadButtonNorthPressed)
                    {
                        if (UserInput.WasLeftShoulderPressing)
                            cardCollectionView.ShowSortOrder();
                        else if (inputSearch != null)
                            inputSearch.ActivateInputField();
                    }
                    else if (UserInput.WasGamepadButtonWestPressed)
                    {
                        if (UserInput.WasLeftShoulderPressing)
                            cardCollectionView.ResetFilters();
                        else
                            cardCollectionView.ShowFilters();
                    }
                }

                if (UserInput.WasRightStickPressed)
                    cardCollectionView.OnTabRight();
            }
        }

        public override bool NeedResponseInput()
        {
            if (!showing)
                return false;
            if (inTransition)
                return false;
            if (inputDeckName != null && inputDeckName.isFocused)
                return false;
            if (inputSearch != null && inputSearch.isFocused)
                return false;
            if (cardActionMenu != null && cardActionMenu.showing)
                return false;
            return base.NeedResponseInput();
        }

        public override void SelectLastSelectable()
        {
            if (_ResponseRegion == ResponseRegion.Collection)
                SelectLastCollectionViewItem();
            else if (_ResponseRegion == ResponseRegion.Deck)
                SelectLastDeckViewItem();
            else if (_ResponseRegion == ResponseRegion.Action)
            {
                if (Selected != null)
                    EventSystem.current.SetSelectedGameObject(Selected.gameObject);
                else
                    cardActionMenu.SelectDefaultButton();
            }
        }

        private void SelectLastDeckViewItem()
        {
            _ResponseRegion = ResponseRegion.Deck;
            if (lastSelectedCardInDeck != null)
                EventSystem.current.SetSelectedGameObject(lastSelectedCardInDeck.gameObject);
            else
                deckView.SelectDefaultItem();
        }

        public void SelectNearestDeckViewItem(Vector3 position)
        {
            _ResponseRegion = ResponseRegion.Deck;
            UserInput.NextSelectionIsAxis = true;
            deckView.SelectNearestCard(position);
        }

        private void SelectLastCollectionViewItem()
        {
            _ResponseRegion = ResponseRegion.Collection;
            if (lastSelectedCardInCollection != null)
                EventSystem.current
                    .SetSelectedGameObject(lastSelectedCardInCollection.gameObject);
            else
                cardCollectionView.SelectDefaultItem();
        }

        public void SelectNearestCollectionViewItem(Vector3 position)
        {
            _ResponseRegion = ResponseRegion.Collection;
            UserInput.NextSelectionIsAxis = true;
            cardCollectionView.SelectNearestCard(position);
        }

        public override void OnReturn()
        {
            if (!deckView.GetDirty() || !DeckIsFromLocal)
                base.OnReturn();
            else
            {
                callExit = true;

                var selections = new List<string>
                {
                    InterString.Get("卡组未保存"),
                    InterString.Get("卡组已修改，是否保存？"),
                    InterString.Get("保存"),
                    InterString.Get("不保存")
                };
                UIManager.ShowPopupYesOrNo(selections, OnSave, OnExit);
            }
        }

        public override void JudgeInputBlockerExitMark(object o)
        {
            _ResponseRegion = (ResponseRegion)o;
        }

        #endregion

        #region Detail View

        private void InitializeCardDetailView()
        {
            if (cardDetailView == null)
                return;

            cardDetailView.SetRelatedCardEvent(() =>
            {
                ShowRelatedCard(cardDetailView.Card);
            });
        }

        public void ShowDetail(Card data)
        {
            if (cardDetailView != null)
                cardDetailView.ShowCard(data);
        }

        public void ChangeRarity(CardRarity.Rarity rarity)
        {
            var code = 0;
            if (_ResponseRegion == ResponseRegion.Action)
                code = cardActionMenu.Card.Id;
            else if (cardDetailView != null)
                code = cardDetailView.Card.Id;
            CardRarity.SetRarity(code, rarity);
            UpdateRarity(code);
        }

        private void UpdateRarity(int code)
        {
            if (cardDetailView != null)
                cardDetailView.RefreshRarity(code);
            if (cardActionMenu.showing)
                cardActionMenu.RefreshRarity(code);
            cardCollectionView.RefreshRarity(code);
            deckView.RefreshRarity(code);
        }

        #endregion

        #region Deck View

        private void InitializeDeckView()
        {
            deckView.SetNoItemButtonNavigationEvent(MoveDirection.Right, () =>
            {
                UserInput.NextSelectionIsAxis = true;
                SelectLastCollectionViewItem();
            });
            inputDeckName = deckView.GetInputField();
            inputDeckName.onEndEdit.AddListener((string text) =>
            {
                SelectLastDeckViewItem();
            });
            var editConditon = DeckView.Condition.Editable;
            if (condition == Condition.OnlineDeck
                || condition == Condition.ReplayDeck)
                editConditon = DeckView.Condition.NonEditable;
            else if(condition == Condition.ChangeSide)
                editConditon = DeckView.Condition.ChangeSide;

            deckView.PrintDeck(Deck, DeckName, editConditon);
            deckView.ButtonDeck.SetClickEvent(OnDeckButtonClicked);
            if (Deck == null)
                loadOnlineDeckCoroutine = StartCoroutine(LoadOnlineDeckAsync());

            SetDeckButtonText();
        }

        private Coroutine loadOnlineDeckCoroutine;
        private IEnumerator LoadOnlineDeckAsync()
        {
            var task = OnlineDeck.GetDeck(onlineDeckID);
            while (!task.IsCompleted)
                yield return null;
            var onlineDeckData = task.Result;
            if (onlineDeckData == null)
            {
                MessageManager.Cast(InterString.Get("网络异常，获取在线卡组失败。"));
                yield break;
            }

            DeckName = onlineDeckData.deckName;
            Deck = new Deck(onlineDeckData.deckYdk, onlineDeckData.deckContributor);

            loadOnlineDeckCoroutine = null;
        }

        private void RefreshShowingCardCount()
        {
            if (cardDetailView != null)
                cardDetailView.SetCardCount();
            cardCollectionView.RefreshCardCount();
            if (_ResponseRegion == ResponseRegion.Action)
                cardActionMenu.SetCardCount();
        }

        /// <summary>
        /// +1按钮、CardInDeck中键添加卡片
        /// </summary>
        public void AddCard(Card data)
        {
            bool playAnimation = _ResponseRegion != ResponseRegion.Action;
            if (!deckView.AddCard(data, playAnimation, playAnimation))
                return;
            AddHistoryCard(data.Id);
            RefreshShowingCardCount();
        }

        /// <summary>
        /// 拖动CollectionCard添加进卡组
        /// </summary>
        /// <param name="code"></param>
        public void AddCardFromCollection(Card data)
        {
            if (!deckView.AddCardFromPosition(data, GetDragCardPositon()))
                return;
            if (cardCollectionView.area != CardCollectionView.Area.History)
                AddHistoryCard(data.Id);
            RefreshShowingCardCount();
        }

        /// <summary>
        /// 右击CollectionCard添加进卡组
        /// </summary>
        /// <param name="code"></param>
        /// <param name="position"></param>
        public void AddCardFromCollection(Card data, Vector3 position)
        {
            if (!deckView.AddCardFromPositionWithSequence(data, position))
                return;
            if (cardCollectionView.area != CardCollectionView.Area.History)
                AddHistoryCard(data.Id);
            RefreshShowingCardCount();
        }

        /// <summary>
        /// -1按钮删除卡片
        /// </summary>
        /// <param name="data"></param>
        public void RemoveCard(Card data)
        {
            if (condition == Condition.ChangeSide)
                return;

            var card = deckView.GetCardByData(data);
            if (card == null)
            {
                MessageManager.Toast(InterString.Get("无法删除更多卡片"));
                return;
            }

            RemoveCardWithAnimation(card);
        }

        /// <summary>
        /// -1按钮 调用此方法
        /// CardInDeck右键删除卡片
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardWithAnimation(SelectionButton_CardInDeck card)
        {
            bool needSelect = _ResponseRegion != ResponseRegion.Action;
            if (!deckView.RemoveCard(card, needSelect, true, false))
                return;
            AddHistoryCard(card.Card.Id);

            AudioManager.PlaySE("SE_DECK_MINUS");

            card.transform.SetParent(transform, true);
            card.transform.localScale = new Vector3(2f, 2f, 1f);
            var cg = card.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            RefreshShowingCardCount();

            if (needSelect)
            {
                var endPostion = cardCollectionView.GetRubbishBinPositon();
                endPostion.z -= 1f;
                var startPostion = card.transform.position;
                startPostion.z = endPostion.z;
                card.transform.position = startPostion;

                DOTween.Sequence()
                    .Append(card.transform.DOMove(endPostion, 0.4f).SetEase(Ease.OutCubic))
                    .Append(card.transform.DOScale(1f, 0.2f).SetEase(Ease.InCubic))
                    .Join(cg.DOFade(0f, 0.2f))
                    .OnComplete(() =>
                    {
                        Destroy(card.gameObject);
                    });
            }
            else
            {
                Destroy(card.gameObject);
            }
        }

        /// <summary>
        /// 拖动Card到DropArea删除卡片
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardByDrag(SelectionButton_CardInDeck card)
        {
            if (!deckView.RemoveCard(card, false, true, true))
                return;
            AddHistoryCard(card.Card.Id);
            PlayDragCardShrinkAnimation();
        }

        public void DragCardTo(SelectionButton_CardInDeck dragCard)
        {
            if (!deckView.deckLoaded) return;
            if(condition != Condition.ChangeSide 
                && !deckView.CanEditCard()) return;

            var position = GetDragCardPositon();
            var hoverCard = deckView.GetHoveringCard();

            var location = DeckLocation.All;
            if (hoverCard == null)
            {
                if(UIHover.HoveringLabel == UIHover.LABEL_REMOVEDECK)
                {
                    RemoveCardByDrag(dragCard);
                    return;
                }
                else if(UIHover.HoveringLabel == UIHover.LABEL_ADDBOOKMARK)
                {
                    BookmarkCard(dragCard.Card.Id);
                    PlayDragCardShrinkAnimation();
                    return;
                }
                else if (UIHover.HoveringLabel == UIHover.LABEL_CANNOTADDBOOKMARK)
                {
                    MessageManager.Toast(InterString.Get("已加入卡片收藏"));
                    return;
                }
                else if (UIHover.HoveringLabel == UIHover.LABEL_MAINDECK)
                    location = DeckLocation.MainDeck;
                else if (UIHover.HoveringLabel == UIHover.LABEL_EXTRADECK)
                    location = DeckLocation.ExtraDeck;
                else if (UIHover.HoveringLabel == UIHover.LABEL_SIDEDECK)
                    location = DeckLocation.SideDeck;
                else
                {
                    dragCard.MoveToParent(position);
                    return;
                }
            }
            else
                location = hoverCard.location;

            if(!deckView.CanSwitchPosition(dragCard.Card, location))
            {
                dragCard.MoveToParent(position);
                return;
            }

            if(hoverCard == null)
            {
                if (dragCard.location == location)
                    dragCard.MoveToParent(position);
                else
                    deckView.MoveCardToLocation(dragCard, location, GetDragCardPositon());
            }
            else
                deckView.MoveCardToLocationWithSiblingIndex(dragCard, location
                    , hoverCard.transform.GetSiblingIndex(), GetDragCardPositon());
        }

        public void CardChangeSide(SelectionButton_CardInDeck card)
        {
            var location = DeckLocation.SideDeck;
            if(card.location == DeckLocation.SideDeck)
                location = card.Card.IsExtraCard() ? DeckLocation.ExtraDeck : DeckLocation.MainDeck;
            deckView.MoveCardToLocation(card, location, card.transform.position);
        }

        public RectTransform GetDragCardImage()
        {
            if (managerUI == null)
                return null;

            return DragCard;
        }

        public Vector3 GetDragCardPositon()
        {
            return managerUI.GetElement<RectTransform>("DragCard").position;
        }

        private void OnDeckButtonClicked()
        {
            if (!deckView.deckLoaded) return;

            if (!deckView.ButtonDeck.gameObject.activeSelf)
                return;

            if(!DeckIsFromLocal && condition == Condition.OnlineDeck)
            {
                OnlineDeck.LikeDeck(onlineDeckID);
                deckLiked = true;
                SetDeckButtonText();
                return;
            }

            if(deckView.GetDirty() || !DeckIsFromLocal)
            {
                if (condition != Condition.ChangeSide)
                    MessageManager.Toast(InterString.Get("请先保存卡组"));
                return;
            }

            if(MyCard.account != null)
            {
                var onlineDeck = OnlineDeck.GetByID(Deck.deckId);
                if (onlineDeck == null || onlineDeck.isDelete)
                    return;
                _ = OnlineDeck.UpdatePublicState(Deck.deckId, !onlineDeck.isPublic);
                onlineDeck.isPublic = !onlineDeck.isPublic;
            }

            SetDeckButtonText();
        }

        private void SetDeckButtonText()
        {
            string text = string.Empty;
            if (DeckIsFromLocal)
            {
                if(MyCard.account != null)
                {
                    var onlineDeck = OnlineDeck.GetByID(Deck.deckId);
                    if (onlineDeck == null || onlineDeck.isDelete)
                    {
                        deckView.ButtonDeck.gameObject.SetActive(false);
                        return;
                    }
                    else
                    {
                        if (onlineDeck.isPublic)
                            text = InterString.Get("公开中");
                        else
                            text = InterString.Get("非公开中");
                    }
                }
            }
            else
            {
                if(condition == Condition.OnlineDeck)
                {
                    text = InterString.Get("点赞");
                    if (deckLiked)
                    {
                        deckView.ButtonDeck.gameObject.SetActive(false);
                        return;
                    }
                }
            }
            deckView.ButtonDeck.SetButtonText(text);
            deckView.ButtonDeck.gameObject.SetActive(text != string.Empty);
        }

        private void PlayDragCardShrinkAnimation()
        {
            if (managerUI == null)
                return;

            DragCard.gameObject.SetActive(true);
            DragCard.localScale = Vector3.one;
            DragCard.DOScale(0.5f, 0.2f).SetEase(Ease.InCubic);
            DragCardImage.DOFade(0.5f, 0.2f).SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    DragCard.localScale = Vector3.one;
                    DragCardImage.color = Color.white;
                    DragCard.gameObject.SetActive(false);
                });
        }

        #endregion

        #region Card Collection View

        private void InitializeCardCollectionView()
        {
            cardCollectionView.historyCards = historyCards;
            cardCollectionView.SetNoItemButtonNavigationEvent(MoveDirection.Left, () =>
            {
                UserInput.NextSelectionIsAxis = true;
                SelectLastDeckViewItem();
            });
            inputSearch = cardCollectionView.GetInputField();
        }

        public void BookmarkCard(int code)
        {
            CardRarity.BookmarkCard(code);
            if (cardDetailView != null)
                cardDetailView.RefreshBookmarkToggle();
            if (_ResponseRegion == ResponseRegion.Action)
                cardActionMenu.RefreshBookmarkToggle();
            if (cardCollectionView.area == CardCollectionView.Area.Bookmark)
                cardCollectionView.PrintBookmarkCards();
        }

        public void UnbookmarkCard(int code)
        {
            CardRarity.UnbookmarkCard(code);
            if (cardDetailView != null)
                cardDetailView.RefreshBookmarkToggle();
            if (_ResponseRegion == ResponseRegion.Action)
                cardActionMenu.RefreshBookmarkToggle();
            if (cardCollectionView.area == CardCollectionView.Area.Bookmark)
                cardCollectionView.PrintBookmarkCards();
        }

        public void AddHistoryCard(int code)
        {
            if (condition == Condition.ChangeSide)
                return;
            cardCollectionView.AddHistoryCard(code);
        }

        public void AddHistoryCards(List<int> codes)
        {
            if (condition == Condition.ChangeSide)
                return;
            cardCollectionView.AddHistoryCards(codes);
        }

        public bool NeedAddCardToHistoryWhenClick()
        {
            return cardCollectionView.area != CardCollectionView.Area.History;
        }

        public void ShowRelatedCard(Card data)
        {
            cardCollectionView.ShowRelatedCard(data);
        }

        public void HideRelatedCard()
        {
            cardCollectionView.HideRelatedCard();
        }

        #endregion

        #region Action Menu

        private void InitializeCardActionMenu()
        {
            cardActionMenu.SetRelatedCardEvent(() =>
            {
                cardActionMenu.blockMark = ResponseRegion.Collection;
                cardActionMenu.Hide();
                ShowRelatedCard(cardActionMenu.Card);
            });
        }

        public void ShowCardActionMenu()
        {
            if (_ResponseRegion == ResponseRegion.Deck
                && lastSelectedCardInDeck != null)
            {
                var list = new List<Card>();
                var index = 0;
                for (int i = 0; i < deckView.cards.Count; i++)
                {
                    list.Add(deckView.cards[i].Card);
                    if (deckView.cards[i] == lastSelectedCardInDeck)
                        index = i;
                }
                cardActionMenu.Show(list, index, _ResponseRegion);
                _ResponseRegion = ResponseRegion.Action;
            }
            else if (_ResponseRegion == ResponseRegion.Collection)
            {
                if (cardCollectionView.printedCards == null
                    || cardCollectionView.printedCards.Count == 0)
                    return;
                if (lastSelectedCardInCollection == null
                    || !lastSelectedCardInCollection.selected)
                    return;
                var list = new List<Card>();
                var index = 0;
                for (int i = 0; i < cardCollectionView.printedCards.Count; i++)
                {
                    if (lastSelectedCardInCollection.card.Id == cardCollectionView.printedCards[i])
                    {
                        index = i;
                        break;
                    }
                }
                cardActionMenu.Show(cardCollectionView.printedCards, index, _ResponseRegion);
                _ResponseRegion = ResponseRegion.Action;
            }
        }



        #endregion

        #region Header

        private bool callExit;

        private void InitializeHeader()
        {
            if (managerHeader == null)
                return;
            managerHeader.GetElement<SelectionButton>("ButtonBanlist")
                .SetButtonText(banlist.Name);
            managerHeader.GetElement<SelectionButton>("ButtonBanlist")
                .SetClickEvent(OnBanlist);
            managerHeader.GetElement<SelectionButton>("ButtonTest")
                .SetClickEvent(OnHandTest);
            managerHeader.GetElement<SelectionButton>("ButtonSort")
                .SetClickEvent(OnSort);
            managerHeader.GetElement<SelectionButton>("ButtonSave")
                .SetClickEvent(OnSave);
            managerHeader.GetElement<SelectionButton>("ButtonMenu")
                .SetClickEvent(OnSubMenu);
            managerHeader.GetElement<SelectionButton>("Back")
                .SetClickEvent(OnReturn);
            managerHeader.GetElement<SelectionButton>("ButtonChangeSide")
                .SetClickEvent(OnChangeSideComplete);

            if(condition == Condition.ChangeSide)
            {
                Destroy(managerHeader.GetElement("ButtonTest"));
                Destroy(managerHeader.GetElement("ButtonSave"));
            }
            else
            {
                Destroy(managerHeader.GetElement("ButtonChangeSide"));
            }
        }

        private void SetCardInfoType()
        {
            var type = (CardInfoType)(((int)_CardInfoType + 1) % 3);
            SetCardInfoType(type);
            SelectionButton_CardInfoType.instance.SetCardInfoTypeIcon(type);
        }

        public void SetCardInfoType(CardInfoType type)
        {
            AudioManager.PlaySE("SE_MENU_SELECT_01");
            _CardInfoType = type;
            switch (_CardInfoType)
            {
                case CardInfoType.None:
                    MessageManager.Toast(InterString.Get("切换到简单显示"));
                    break;
                case CardInfoType.Detail:
                    MessageManager.Toast(InterString.Get("切换到详情显示"));
                    break;
                case CardInfoType.Pool:
                    MessageManager.Toast(InterString.Get("切换到归属显示"));
                    break;
            }

            deckView.SetCardInfoType(type);
            cardCollectionView.SetCardInfoType(type);
        }

        private void RefreshRegulationIcons()
        {
            foreach (var card in deckView.cards)
                card.SetRegulationIcon();
            foreach (var go in cardCollectionView.superScrollView.gameObjects)
                go.GetComponent<SelectionButton_CardInCollection>()
                    .SetRegulationIcon();
        }

        private void OnBanlist()
        {
            AudioManager.PlaySE("SE_MENU_DECIDE");
            List<string> selections = new()
            {
                InterString.Get("禁限卡表"),
                string.Empty
            };
            foreach (var list in BanlistManager.Banlists)
                selections.Add(list.Name);
            UIManager.ShowPopupSelection(selections, ChangeBanlist);
        }

        private void ChangeBanlist()
        {
            string selected = EventSystem.current.currentSelectedGameObject
                .GetComponent<SelectionButton>().GetButtonText();
            banlist = BanlistManager.GetByName(selected);
            managerHeader.GetElement<SelectionButton>("ButtonBanlist")
                .SetButtonText(selected);
            RefreshRegulationIcons();
        }

        private void OnSubMenu()
        {
            if (!deckView.deckLoaded) return;
            var menus = new List<string>()
                {
                    InterString.Get("副菜单"),
                    InterString.Get("重置"),
                    InterString.Get("排序"),
                    InterString.Get("打乱")
                };
            var actions = new List<Action>()
                {
                    null,
                    OnReset,
                    OnSort,
                    OnRandom
                };

            if (condition != Condition.ChangeSide)
            {
                menus.AddRange(new List<string>
                {
                    InterString.Get("复制"),
                    InterString.Get("分享"),
                    InterString.Get("测试"),
                    InterString.Get("清空")
                });
                actions.AddRange(new List<Action>()
                {
                    OnCopy,
                    OnShare,
                    OnHandTest,
                    OnClearDeck
                });
            }
            UIManager.ShowSubMenu(menus, actions);
        }

        private void OnSave()
        {
            if (DeckIsFromLocal && !deckView.GetDirty()) return;

            if(DeckIsFromLocal)
                if (banlist.Name != BanlistManager.EmptyBanlistName)
                {
                    if (deckView.mainCount > 60 || deckView.extraCount > 15 || deckView.sideCount > 15)
                    {
                        List<string> tasks = new()
                        {
                            InterString.Get("保存失败"),
                            InterString.Get("卡组内卡片张数超过限制。@n如需无视限制，请将禁限卡表设置为无（N/A）。")
                        };
                        UIManager.ShowPopupConfirm(tasks);
                        callExit = false;
                        return;
                    }
                }

            if (!DeckIsFromLocal && File.Exists(Program.deckPath + DeckName + Program.ydkExpansion))
            {
                List<string> tasks = new()
                {
                    InterString.Get("该卡组名已存在"),
                    InterString.Get("该卡组名的文件已存在，是否直接覆盖创建？"),
                    InterString.Get("覆盖"),
                    InterString.Get("取消")
                };

                UIManager.ShowPopupYesOrNo(tasks, OnSaveConfirmed, () => { callExit = false; });
            }
            else
                OnSaveConfirmed();
        }

        private void OnSaveConfirmed()
        {
            if (!deckView.Save())
                return;

            if (callExit)
            {
                cg.blocksRaycasts = false;
                inTransition = true;//block input
                DOTween.To(v => { }, 0, 0, 2f).OnComplete(() =>
                {
                    OnExit();
                });
                return;
            }
            DeckIsFromLocal = true;
            SetDeckButtonText();
        }

        private void OnReset()
        {
            deckView.ResetDeck();
        }

        private void OnSort()
        {
            deckView.Sort();
        }

        private void OnRandom()
        {
            deckView.Randomize();
        }

        private void OnCopy()
        {
            deckView.Copy();
        }

        private void OnShare()
        {
            deckView.Share();
        }

        private void OnHandTest()
        {

        }

        private void OnClearDeck()
        {
            var codes = new List<int>();
            foreach (var card in deckView.cards)
                codes.Add(card.Card.Id);
            if (!deckView.ClearDeck())
                return;
            AudioManager.PlaySE("SE_DECK_MINUS");
            AddHistoryCards(codes);
            RefreshShowingCardCount();
        }

        public void OnChangeSideComplete()
        {
            TcpHelper.CtosMessage_UpdateDeck(deckView.FromObjectDeckToCodedDeck());
        }

        private void ShowBackButton()
        {
            if (managerHeader == null)
                return;
            managerHeader.GetElement("Back").SetActive(true);

            var rect = managerHeader.GetElement<RectTransform>("Back");
            rect.anchoredPosition3D = new Vector3(24f, 120f, 0f);
            DOTween.Sequence()
                .AppendInterval(0.6f)
                .Append(rect.DOAnchorPos3D(new Vector3(24f, 0f, 0f), 0.2f).SetEase(Ease.OutQuart));
        }

        private void HideBackButton()
        {
            managerHeader.GetElement("Back").SetActive(false);
        }

        #endregion

        #region Over Header

        private void InitializeOverHeader()
        {
            if (managerOverHeader == null)
                return;
            if (condition == Condition.ChangeSide)
                managerOverHeader.GetElement("AppearanceGroup").SetActive(false);
            else
            {
                managerOverHeader.GetElement<SelectionButton>("AppearanceGroup").SetClickEvent(ShiftToAppearance);
                StartCoroutine(RefreshOverHeaderIconsAsync());
            }
        }

        private IEnumerator RefreshOverHeaderIconsAsync()
        {
            if (managerOverHeader == null)
                yield break;

            managerOverHeader.GetElement<Image>("IconCase").color = Color.clear;
            managerOverHeader.GetElement<Image>("IconProtector").color = Color.clear;
            managerOverHeader.GetElement<Image>("IconField").color = Color.clear;
            managerOverHeader.GetElement<Image>("IconGrave").color = Color.clear;
            managerOverHeader.GetElement<Image>("IconStand").color = Color.clear;
            managerOverHeader.GetElement<Image>("IconMate").color = Color.clear;

            while (Deck == null)
                yield return null;

            var ie = Program.items.LoadItemIconAsync(Deck.Case.ToString(), Items.ItemType.Case);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            managerOverHeader.GetElement<Image>("IconCase").color = Color.white;
            managerOverHeader.GetElement<Image>("IconCase").sprite = ie.Current;

            var im = ABLoader.LoadProtectorMaterial(Deck.Protector.ToString());
            StartCoroutine(im);
            while (im.MoveNext())
                yield return null;
            managerOverHeader.GetElement<Image>("IconProtector").color = Color.white;
            managerOverHeader.GetElement<Image>("IconProtector").material = im.Current;

            ie = Program.items.LoadItemIconAsync(Deck.Field.ToString(), Items.ItemType.Mat);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            managerOverHeader.GetElement<Image>("IconField").color = Color.white;
            managerOverHeader.GetElement<Image>("IconField").sprite = ie.Current;

            ie = Program.items.LoadItemIconAsync(Deck.Grave.ToString(), Items.ItemType.Grave);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            managerOverHeader.GetElement<Image>("IconGrave").color = Color.white;
            managerOverHeader.GetElement<Image>("IconGrave").sprite = ie.Current;

            ie = Program.items.LoadItemIconAsync(Deck.Stand.ToString(), Items.ItemType.Stand);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            managerOverHeader.GetElement<Image>("IconStand").color = Color.white;
            managerOverHeader.GetElement<Image>("IconStand").sprite = ie.Current;

            var mate = Deck.Mate.ToString();
            if (mate.Length == 7 && mate.StartsWith("100"))
            {
                ie = Program.items.LoadItemIconAsync(mate, Items.ItemType.Mate);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                managerOverHeader.GetElement<Image>("IconMate").color = Color.white;
                managerOverHeader.GetElement<Image>("IconMate").sprite = ie.Current;
            }
            else
            {
                var task = TextureManager.LoadArtAsync(Deck.Mate, true);
                while (!task.IsCompleted)
                    yield return null;
                managerOverHeader.GetElement<Image>("IconMate").color = Color.white;
                managerOverHeader.GetElement<Image>("IconMate").sprite = TextureManager.Texture2Sprite(task.Result);
            }
        }

        private void ShiftToAppearance()
        {
            if (!deckView.deckLoaded)
                return;
            if (condition == Condition.ChangeSide)
                return;
            if (!DeckIsFromLocal)
            {
                if(condition != Condition.ChangeSide)
                    MessageManager.Toast(InterString.Get("请先保存卡组"));
                return;
            }
            gotoAppearance = true;
            Program.instance.appearance.SwitchCondition(Appearance.Condition.DeckEditor);
            Program.instance.ShiftToServant(Program.instance.appearance);
        }

        #endregion

        #region Other
        private void ChangeCanvasMatch()
        {
            if (!showing)
                return;

            UIManager.SetCanvasMatch(GetCanvasMatch(), 0f);
        }

        private int GetCanvasMatch()
        {
            if ((float)Screen.width / Screen.height > 16f / 9f)
                return 1;
            else return 0;
        }

        #endregion
    }
}