using DG.Tweening;
using MDPro3.YGOSharp;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static YgomSystem.UI.ColorContainer;
using YgomSystem.UI;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using static MDPro3.UI.DeckView;
using System.Drawing.Printing;
using UnityEngine.InputSystem.HID;

namespace MDPro3.UI
{
    public class SelectionButton_CardInDeck : SelectionButton, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Header("SelectionButton CardInDeck")]
        public DeckView deckView;
        private Card _card;
        public Card Card
        {
            get { return _card; }
            set
            {
                if(_card == null || value.Id != _card.Id)
                {
                    _card = value;
                    SetIcons();
                    Refresh();
                }
            }
        }

        public bool Refreshed => ImageHandler.Refreshed;
        public DeckLocation location;
        private Vector3 dragScale = new(1.7f, 1.7f, 1f);
        private RectTransform child;

        private CardRawImageHandler m_ImageHandler;
        private CardRawImageHandler ImageHandler =>
            m_ImageHandler = m_ImageHandler != null ? m_ImageHandler
            : Manager.GetElement<CardRawImageHandler>("ImageCard");

        protected override void Awake()
        {
            manuallySetNavigation = false;
            base.Awake();
            child = transform.GetChild(0).GetComponent<RectTransform>();
            SetClickEvent(() =>
            {
                if(UserInput.gamepadType == UserInput.GamepadType.None)
                {
                    if (DeckEditor.useMobileLayout) 
                    {
                        if (dragProcessing)
                            return;
                        AudioManager.PlaySE("SE_MENU_DECIDE");
                        Program.instance.deckEditor.lastSelectedCardInDeck = this;
                        Program.instance.deckEditor._ResponseRegion = DeckEditor.ResponseRegion.Deck;
                        Program.instance.deckEditor.ShowCardActionMenu();
                    }
                    else
                    {
                        Program.instance.deckEditor.AddHistoryCard(Card.Id);
                        Program.instance.deckEditor.ShowDetail(Card);
                    }
                }
                else
                {
                    if (DeckEditor.condition == DeckEditor.Condition.EditDeck)
                        Program.instance.deckEditor.RemoveCardWithAnimation(this);
                }
            });
            SetRightClickEvent(() =>
            {
                if(DeckEditor.condition != DeckEditor.Condition.ChangeSide)
                {
                    Program.instance.deckEditor.RemoveCardWithAnimation(this);
                }
                else
                {
                    Program.instance.deckEditor.CardChangeSide(this);
                }
                Program.instance.deckEditor.ShowDetail(Card);
            });
            SetMiddleClickEvent(() =>
            {
                Program.instance.deckEditor.AddCard(Card);
                Program.instance.deckEditor.ShowDetail(Card);
            });
        }

        protected override void OnSelect(bool playSE)
        {
            base.OnSelect(playSE);

            foreach (var ccg in Manager.GetElement<Transform>("ImageCard")
                .GetComponentsInChildren<ColorContainerGraphic>(true))
                ccg.SetColor(SelectMode.Selected, hovering ? StatusMode.Enter : StatusMode.Normal, Selectable.interactable);

            Program.instance.deckEditor.ShowDetail(Card);
            Program.instance.deckEditor.lastSelectedCardInDeck = this;
            Program.instance.deckEditor._ResponseRegion = DeckEditor.ResponseRegion.Deck;
        }

        private void Refresh()
        {
            ImageHandler.SetCard(Card);
        }

        public void RefreshRarity(int code)
        {
            ImageHandler.RefreshRarity(code);
        }

        public void SetRegulationIcon()
        {
            Manager.GetElement<Image>("IconLimit").sprite
                = TextureManager.container.GetCardRegulationIcon(Card.Id, DeckEditor.banlist);
        }

        private void SetIcons()
        {
            SetRegulationIcon();

            var attributeIcon = TextureManager.container.GetCardAttributeIcon(Card);
            Manager.GetElement<Image>("IconAttribute").sprite =
                attributeIcon == null
                ? TextureManager.container.typeNone
                : attributeIcon;

            var spellTrapTypeIcon = TextureManager.container.GetCardSpellTrapTypeIcon(Card);
            Manager.GetElement<Image>("IconSpellTrapType").sprite =
                spellTrapTypeIcon == null
                ? TextureManager.container.typeNone
                : spellTrapTypeIcon;

            var raceIcon = TextureManager.container.GetCardRaceIcon(Card);
            Manager.GetElement<Image>("IconRace").sprite =
                raceIcon == null
                ? TextureManager.container.typeNone
                : raceIcon;
            Manager.GetElement<Image>("IconPool").sprite =
                TextureManager.container.GetCardPoolIcon(Card);

            Manager.GetElement<TextMeshProUGUI>("TextLevel").text = Card.Level.ToString();
            Manager.GetElement<TextMeshProUGUI>("TextRank").text = Card.Level.ToString();
            Manager.GetElement<TextMeshProUGUI>("TextLink").text = Card.GetLinkCount().ToString();
            Manager.GetElement<TextMeshProUGUI>("TextPendulumScale").text = Card.LScale.ToString();

            RefreshIcons();
        }

        public void RefreshIcons()
        {
            Manager.GetElement("IconAttribute").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconSpellTrapType").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconRace").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail);
            Manager.GetElement("IconTuner").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && Card.HasType(CardType.Tuner));
            var levelType = Card.GetLevelType();
            Manager.GetElement("IconLevel").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && Card.HasType(CardType.Monster) && levelType == Card.LevelType.Level);
            Manager.GetElement("IconRank").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && Card.HasType(CardType.Monster) && levelType == Card.LevelType.Rank);
            Manager.GetElement("IconLink").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && Card.HasType(CardType.Monster) && levelType == Card.LevelType.Link);
            Manager.GetElement("IconPendulumScale").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Detail
                && Card.HasType(CardType.Pendulum));
            Manager.GetElement("IconPool").SetActive(DeckEditor._CardInfoType == DeckEditor.CardInfoType.Pool);
        }

        public void PlayBirthAnimation()
        {
            StartCoroutine(PlayBirthAnimationAsync());
        }

        private IEnumerator PlayBirthAnimationAsync()
        {
            yield return null;
            child.SetParent(Program.instance.ui_.transform, true);
            child.localScale = dragScale;
            child.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuart).OnComplete(() =>
            {
                child.SetParent(transform, true);
                child.localPosition = Vector3.zero;
                child.localScale = Vector3.one;
                child.localEulerAngles = Vector3.zero;
            });
        }

        /// <summary>
        /// 锁定卡片child当前的位置，并在下一帧开始移动到父级初始位置
        /// </summary>
        public void LockPosition()
        {
            //child.SetParent(Program.instance.ui_.transform, true);
            child.SetParent(Program.instance.deckEditor.deckView.TempView, true);
            StartCoroutine(AutoMoveToParent());
        }

        /// <summary>
        /// 锁定卡片child到指定位置position，并在下一帧开始移动到父级初始位置
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        public void LockPosition(Vector3 position, Vector3 scale)
        {
            child.SetParent(Program.instance.ui_.transform, true);
            child.position = position;
            child.localScale = scale;
            StartCoroutine(AutoMoveToParent());
        }

        private IEnumerator AutoMoveToParent()
        {
            yield return null;
            foreach (var ccg in child.GetComponentsInChildren<ColorContainerGraphic>(true))
                ccg.SetColor(selected ? SelectMode.Selected : SelectMode.Unselected, StatusMode.Normal, Selectable.interactable);

            var position = transform.position;
            DOTween.Sequence()
                .Append(child.DOMove(position, 0.1f).SetEase(Ease.OutCubic))
                .Join(child.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    child.SetParent(transform, true);
                    child.localPosition = Vector3.zero;
                    child.localScale = Vector3.one;
                    child.localEulerAngles = Vector3.zero;
                });
        }

        public void MoveToParent(Vector3 position)
        {
            child.SetParent(Program.instance.ui_.transform, true);
            child.localScale = dragScale;
            child.position = position;
            StartCoroutine(AutoMoveToParent());
        }
        
        public void MoveToParentSequence(Vector3 position)
        {
            if (!DeckEditor.useMobileLayout)
            {
                child.SetParent(Program.instance.ui_.transform, true);
                child.localScale = dragScale;
                child.position = position;
            }
            StartCoroutine(AutoMoveToParentSequence(position));
        }

        private IEnumerator AutoMoveToParentSequence(Vector3 position)
        {
            if (DeckEditor.useMobileLayout)
            {
                child.gameObject.SetActive(false);
                yield return null;
                Program.instance.deckEditor.deckView.ScrollTo(this);
            }

            yield return null;
            if (DeckEditor.useMobileLayout)
            {
                child.gameObject.SetActive(true);
                child.SetParent(Program.instance.ui_.transform, true);
                child.localScale = dragScale;
                child.position = position;
            }
            yield return null;
            foreach (var ccg in child.GetComponentsInChildren<ColorContainerGraphic>(true))
                ccg.SetColor(SelectMode.Unselected, StatusMode.Normal, Selectable.interactable);

            var endPosition = transform.position;
            DOTween.Sequence()
                .Append(child.DOMove(endPosition, 0.2f).SetEase(Ease.OutCubic))
                .Append(child.DOScale(Vector3.one, 0.2f).SetEase(Ease.InCubic))
                .OnComplete(() =>
                {
                    child.SetParent(transform, true);
                    child.localPosition = Vector3.zero;
                    child.localScale = Vector3.one;
                    child.localEulerAngles = Vector3.zero;
                });
        }

        public bool IsHovering()
        {
            return hovering;
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

            deckView.ScrollRect.OnBeginDrag(eventData);
            dragStartPosition = eventData.position;
            dragProcessing = true;
            draging = !DeckEditor.useMobileLayout;
            dragIni = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if(draging)
            {
                if(!dragIni)
                {
                    dragTarget = Program.instance.deckEditor.GetDragCardImage();
                    dragTarget.gameObject.SetActive(true);
                    dragTarget.GetChild(0).GetComponent<RawImage>().texture
                        = ImageHandler.RawImage.texture;
                    dragTarget.GetChild(0).GetComponent<RawImage>().material
                        = ImageHandler.RawImage.material;
                    dragIni = true;

                    UIHover.HoveringLabel = string.Empty;
                    Program.instance.deckEditor.cardCollectionView.SetBookmarkDropArea(Card.Id);

                    UserInput.Draging = true;
                    var cg = GetComponent<CanvasGroup>();
                    cg.blocksRaycasts = false;
                    cg.alpha = 0f;
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
                deckView.ScrollRect.OnDrag(eventData);
                draging = NeedStartDrag(dragStartPosition, eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            deckView.ScrollRect.OnEndDrag(eventData);
            dragProcessing = false;
            UserInput.Draging = false;

            if(draging)
            {
                var cg = GetComponent<CanvasGroup>();
                cg.blocksRaycasts = true;
                cg.alpha = 1f;

                dragTarget.gameObject.SetActive(false);

                Program.instance.deckEditor.DragCardTo(this);
                Program.instance.deckEditor.deckView.HideDeckLocationTable();
            }
        }

        private Vector3 lastDragStartPosition;
        private bool diffYOverLimit;
        private bool NeedStartDrag(Vector3 startPosition, Vector3 position)
        {
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

        #endregion

        #region Navigation
        protected override int GetButtonsCount()
        {
            return Program.instance.deckEditor.deckView.GetDeckLocationCount(location);
        }

        protected override int GetColumnsCount()
        {
            return Program.instance.deckEditor.deckView.GetDeckLocationParent(location)
                .GetComponent<GridLayoutGroup>().Size().x;
        }

        protected override void OnNavigation(AxisEventData eventData)
        {
            var selfIndex = transform.GetSiblingIndex();

            var count = GetButtonsCount();
            var columes = GetColumnsCount();
            if(columes == 0)
            {
                Debug.LogError("divide by zero");
                return;
            }

            var targetIndex = selfIndex + 1;

            if (eventData.moveDir == MoveDirection.Left)
            {
                if (selfIndex % columes == 0)
                    return;
                targetIndex = selfIndex - 1;
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                if (selfIndex % columes == columes - 1
                    || selfIndex == count - 1)
                {
                    Program.instance.deckEditor.SelectNearestCollectionViewItem(transform.position);
                    return;
                }
            }
            else if (eventData.moveDir == MoveDirection.Up)
            {
                targetIndex = selfIndex - columes;
                if (targetIndex < 0)
                {
                    SelectTarget(GetNavivationTarget(eventData.moveDir));
                    return;
                }
            }
            else if (eventData.moveDir == MoveDirection.Down)
            {
                targetIndex = selfIndex + columes;
                if (targetIndex >= count)
                {
                    if(location == DeckLocation.SideDeck
                        && !Tools.InLastRow(selfIndex, count, columes))
                        targetIndex = count - 1;
                    else
                    {
                        SelectTarget(GetNavivationTarget(eventData.moveDir));
                        return;
                    }
                }
            }

            for (int i = 0; i < transform.parent.childCount; i++)
            {
                var child = transform.parent.GetChild(i);
                if (!child.gameObject.activeSelf)
                    continue;

                var buttonIndex = child.GetComponent<SelectionButton>().index;
                if (buttonIndex < 0)
                    buttonIndex = i;

                if (buttonIndex == targetIndex)
                {
                    UserInput.NextSelectionIsAxis = true;
                    EventSystem.current.SetSelectedGameObject(transform.parent.GetChild(i).gameObject);
                    break;
                }
            }
        }

        private SelectionButton_CardInDeck GetNavivationTarget(MoveDirection direction)
        {
            if (direction == MoveDirection.Up)
            {
                if (location == DeckLocation.MainDeck)
                    return null;
                else if(location == DeckLocation.ExtraDeck)
                    return Program.instance.deckEditor.deckView.GetNavigationTarget(DeckLocation.MainDeck, direction, transform.position);
                else if (location == DeckLocation.SideDeck)
                    return Program.instance.deckEditor.deckView.GetNavigationTarget(DeckLocation.ExtraDeck, direction, transform.position);
            }
            else if (direction == MoveDirection.Down)
            {
                if (location == DeckLocation.MainDeck)
                    return Program.instance.deckEditor.deckView.GetNavigationTarget(DeckLocation.ExtraDeck, direction, transform.position);
                else if (location == DeckLocation.ExtraDeck)
                    return Program.instance.deckEditor.deckView.GetNavigationTarget(DeckLocation.SideDeck, direction, transform.position);
                else if (location == DeckLocation.SideDeck)
                    return null;
            }
            return null;
        }

        private void SelectTarget(SelectionButton_CardInDeck target)
        {
            if (target == null)
                return;
            UserInput.NextSelectionIsAxis = true;
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }

        #endregion
    }
}
