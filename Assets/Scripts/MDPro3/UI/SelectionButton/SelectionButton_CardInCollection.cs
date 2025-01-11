using DG.Tweening;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SelectionButton_CardInCollection : SelectionButton, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private int _cardCode;
        public int CardCode
        {
            get { return _cardCode; }
            set
            {
                if (_cardCode != value)
                {
                    _cardCode = value;
                    card = CardsManager.Get(_cardCode);
                    SetIcons();
                    Refresh();
                }
            }
        }
        public Card card;
        public CardCollectionView cardCollectionView;

        private CardRawImageHandler m_ImageHandler;
        private CardRawImageHandler ImageHandler =>
            m_ImageHandler = m_ImageHandler != null ? m_ImageHandler
            : Manager.GetElement<CardRawImageHandler>("ImageCard");

        protected override void Awake()
        {
            manuallySetNavigation = false;
            base.Awake();
            SetClickEvent(() =>
            {
                if(UserInput.gamepadType == UserInput.GamepadType.None)
                {

                    if (DeckEditor.useMobileLayout)
                    {
                        if (dragProcessing)
                            return;
                        AudioManager.PlaySE("SE_MENU_DECIDE");
                        Program.instance.deckEditor.lastSelectedCardInCollection = this;
                        Program.instance.deckEditor._ResponseRegion = DeckEditor.ResponseRegion.Collection;
                        Program.instance.deckEditor.ShowCardActionMenu();
                    }
                    else
                    {
                        Program.instance.deckEditor.ShowDetail(card);
                        if (cardCollectionView.area != CardCollectionView.Area.History)
                            Program.instance.deckEditor.AddHistoryCard(card.Id);
                    }
                }
                else
                {
                    AddThisToDeck();
                }
            });
            SetRightClickEvent(() =>
            {
                AddThisToDeck();
            });
        }

        protected override void OnSelect(bool playSE)
        {
            base.OnSelect(playSE);
            Program.instance.deckEditor.ShowDetail(card);
            Program.instance.deckEditor.lastSelectedCardInCollection = this;
            Program.instance.deckEditor._ResponseRegion = DeckEditor.ResponseRegion.Collection;
        }

        private void AddThisToDeck()
        {
            var position = transform.GetChild(0).position;
            Program.instance.deckEditor.AddCardFromCollection(card, position);
            Program.instance.deckEditor.ShowDetail(card);
        }

        public void Refresh()
        {
            ImageHandler.SetCard(card);
        }

        public void RefreshRarity(int code)
        {
            ImageHandler.RefreshRarity(code);
        }

        public void SetRegulationIcon()
        {
            Manager.GetElement<Image>("IconLimit").sprite
                = TextureManager.container.GetCardRegulationIcon(CardCode, DeckEditor.banlist);
        }

        private void SetIcons()
        {
            SetRegulationIcon();

            var attributeIcon = TextureManager.container.GetCardAttributeIcon(card);
            Manager.GetElement<Image>("IconAttribute").sprite =
                attributeIcon == null
                ? TextureManager.container.typeNone
                : attributeIcon;

            var spellTrapTypeIcon = TextureManager.container.GetCardSpellTrapTypeIcon(card);
            Manager.GetElement<Image>("IconSpellTrapType").sprite =
                spellTrapTypeIcon == null
                ? TextureManager.container.typeNone
                : spellTrapTypeIcon;

            var raceIcon = TextureManager.container.GetCardRaceIcon(card);
            Manager.GetElement<Image>("IconRace").sprite =
                raceIcon == null
                ? TextureManager.container.typeNone
                : raceIcon;
            Manager.GetElement<Image>("IconPool").sprite =
                TextureManager.container.GetCardPoolIcon(card);

            Manager.GetElement<TextMeshProUGUI>("TextLevel").text = card.Level.ToString();
            Manager.GetElement<TextMeshProUGUI>("TextRank").text = card.Level.ToString();
            Manager.GetElement<TextMeshProUGUI>("TextLink").text = card.GetLinkCount().ToString();
            Manager.GetElement<TextMeshProUGUI>("TextPendulumScale").text = card.LScale.ToString();

            RefreshIcons();
            RefreshCountIcon();
        }

        public void RefreshIcons()
        {
            Manager.GetElement("IconAttribute").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconSpellTrapType").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconRace").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconTuner").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && card.HasType(CardType.Tuner));
            var levelType = card.GetLevelType();
            Manager.GetElement("IconLevel").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && card.HasType(CardType.Monster) && levelType == Card.LevelType.Level);
            Manager.GetElement("IconRank").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && card.HasType(CardType.Monster) && levelType == Card.LevelType.Rank);
            Manager.GetElement("IconLink").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && card.HasType(CardType.Monster) && levelType == Card.LevelType.Link);
            Manager.GetElement("IconPendulumScale").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && card.HasType(CardType.Pendulum));
            Manager.GetElement("IconPool").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Pool);
        }

        public void RefreshCountIcon()
        {
            var ragulation = DeckEditor.banlist.GetQuantity(card.Id);
            var count = Program.instance.deckEditor.deckView.GetCardCount(card.Id);
            var color = Color.white;
            if(count == ragulation)
                color = Color.yellow;
            if(count > ragulation)
                color = Color.red;

            Manager.GetElement<Image>("IconCardUse1").color = color;
            Manager.GetElement<Image>("IconCardUse2").color = color;
            Manager.GetElement<Image>("IconCardUse3").color = color;

            for (int i = 1; i <= count && i < 4; i++)
                Manager.GetElement("IconCardUse" + i).SetActive(true);
            for (int i = count + 1; i < 4; i++)
                Manager.GetElement("IconCardUse" + i).SetActive(false);
        }

        #region Drag

        private RectTransform dragTarget;
        private Vector2 dragStartPosition;
        private bool dragProcessing;
        private bool draging;
        private bool dragIni;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            cardCollectionView.superScrollView.scrollRect.OnBeginDrag(eventData);
            dragStartPosition = eventData.position;
            dragProcessing = true;
            //draging = !DeckEditor.useMobileLayout;
            draging = false;
            dragIni = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (draging)
            {
                if (!dragIni)
                {
                    dragTarget = Program.instance.deckEditor.GetDragCardImage();
                    dragTarget.gameObject.SetActive(true);
                    dragTarget.GetChild(0).GetComponent<RawImage>().texture
                        = ImageHandler.RawImage.texture;
                    dragTarget.GetChild(0).GetComponent<RawImage>().material
                        = ImageHandler.RawImage.material;
                    dragIni = true;

                    Program.instance.deckEditor.cardCollectionView.SetDropAreaActive(false);
                    UIHover.HoveringLabel = string.Empty;
                    UserInput.Draging = true;
                }

                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    dragTarget, eventData.position, eventData.enterEventCamera, out var position);
                dragTarget.position = position;
                var anchoredPositon = dragTarget.anchoredPosition3D;
                anchoredPositon.z = -10f;
                dragTarget.anchoredPosition3D = anchoredPositon;
            }
            else
            {
                cardCollectionView.superScrollView.scrollRect.OnDrag(eventData);
                draging = NeedStartDrag(dragStartPosition, eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            cardCollectionView.superScrollView.scrollRect.OnEndDrag(eventData);
            dragProcessing = false;

            if (draging)
            {
                UserInput.Draging = false;
                Program.instance.deckEditor.cardCollectionView.SetDropAreaActive(true);
                dragTarget.gameObject.SetActive(false);
                Program.instance.deckEditor.AddCardFromCollection(card);
                Program.instance.deckEditor.deckView.HideDeckLocationTable();
            }
        }

        private Vector3 lastDragStartPosition;
        private bool diffYOverLimit;
        private bool NeedStartDrag(Vector3 startPosition, Vector3 position)
        {
            if (!NeedDrag()) return false;

            if (lastDragStartPosition == null || startPosition != lastDragStartPosition)
            {
                diffYOverLimit = false;
                lastDragStartPosition = startPosition;
            }

            if (diffYOverLimit)
                return false;
            var diffX = Mathf.Abs(position.x - startPosition.x);
            var diffY = Mathf.Abs(position.y - startPosition.y);
            if (diffY > 100f)
            {
                diffYOverLimit = true;
                return false;
            }
            if (diffX > 10f && diffX > diffY)
                return true;
            return false;
        }

        private bool NeedDrag()
        {
            return DeckEditor.condition != DeckEditor.Condition.ChangeSide;
        }

        #endregion

        #region Navigation

        protected override int GetButtonsCount()
        {
            return Program.instance.deckEditor.cardCollectionView.superScrollView.items.Count;
        }

        protected override int GetColumnsCount()
        {
            return 6;
        }

        protected override void OnNavigationLeftBorder()
        {
            base.OnNavigationLeftBorder();
            Program.instance.deckEditor.SelectNearestDeckViewItem(transform.GetChild(0).position);
        }

        protected override void OnNavigationUpBorder()
        {
            base.OnNavigationUpBorder();
            var target = Program.instance.deckEditor.cardCollectionView.GetUpNavigationObject();
            if(target != null)
            {
                UserInput.NextSelectionIsAxis = true;
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        #endregion
    }
}