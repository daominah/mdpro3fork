using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.Duel.YGOSharp;
using MDPro3.UI.ServantUI;
using MDPro3.Servant;

namespace MDPro3.UI
{
    public class CardList : MonoBehaviour
    {
        const float SingleColumnItemStep = 140f;
        const int ExtraDeckColumns = 3;
        const float ExtraDeckBaseItemWidth = 78f;
        const float ExtraDeckVisualItemHeight = 124f;
        const float ExtraDeckScrollbarWidth = 20f;
        const float ExtraDeckHorizontalInset = 10f;
        const float ExtraDeckHorizontalGap = 16f;
        const float ExtraDeckVerticalGap = 20f;
        const float ExtraDeckRowLineHeight = 2f;
        const float ExtraDeckToggleButtonWidth = 88f;
        const float ExtraDeckToggleButtonHeight = 68f;
        const float ExtraDeckToggleButtonIconSize = 26f;
        const float ExtraDeckToggleButtonOverlap = 4f;
        const float ExtraDeckToggleButtonTopInset = 4f;
        const float ExtraDeckToggleHeaderLineHeight = 2f;
        const float ExtraDeckToggleHeaderLineRightInset = 10f;
        const float ExtraDeckHeaderHeight = 96f;
        const float ExtraDeckLocationIconSize = 64f;
        const float ExtraDeckLocationIconTopInset = 8f;
        const float HiddenPadding = 20f;

        public RectTransform baseRect;
        public Image locationIcon;
        public ScrollRect scrollRect;
        public GameObject item;

        bool showing;

        List<GameCard> cards;
        List<GameObject> cardObjs = new List<GameObject>();
        float transitionTime = 0.15f;
        CardLocation location;
        int controller;
        bool showWithCloseDuelLog = false;
        float defaultBaseWidth = -1f;
        bool extraDeckGridMode = false;
        Button extraDeckModeButton;
        Image extraDeckModeButtonIcon;
        Image extraDeckModeButtonFrame;
        RectTransform extraDeckModeButtonHeaderLineRect;
        RectTransform locationIconRect;
        RectTransform listHeaderRect;
        RectTransform listScrollRect;
        Vector2 defaultLocationIconAnchoredPosition;
        Vector2 defaultLocationIconSizeDelta;
        Vector2 defaultHeaderSizeDelta;
        Vector2 defaultScrollViewAnchoredPosition;
        Vector2 defaultScrollViewSizeDelta;
        bool defaultListLayoutCached;

        public void Show(List<GameCard> cards, CardLocation location, int controller)
        {
            if(OcgCore.cantCheckGrave && location == CardLocation.Grave)
            {
                MessageManager.Cast(InterString.Get("现在不能查看此处的卡片。"));
                return;
            }

            var previousLocation = this.location;
            this.cards = cards;
            this.location = location;
            this.controller = controller;
            if (IsToggleListLocation(this.location) && (!showing || !IsToggleListLocation(previousLocation)))
                extraDeckGridMode = false;

            if (!showing)
            {
                RefreshList();
                baseRect.DOAnchorPosX(-30, transitionTime);

                if (Program.instance.ocgcore.GetUI<OcgCoreUI>().DuelLog.showing)
                {
                    Program.instance.ocgcore.GetUI<OcgCoreUI>().OnLog(true);
                    showWithCloseDuelLog = true;
                }
            }
            else
            {
                baseRect.DOAnchorPosX(GetHiddenPosX(), transitionTime).OnComplete(() =>
                {
                    RefreshList();
                    baseRect.DOAnchorPosX(-30, transitionTime);// TODO: tween in tween
                });
            }

            showing = true;
            baseRect.localScale = Vector3.one * Config.GetUIScale(1.18f);
        }

        public void Hide()
        {
            if (!showing)
                return;
            showing = false;
            if (extraDeckModeButton != null)
                extraDeckModeButton.gameObject.SetActive(false);
            if (extraDeckModeButtonHeaderLineRect != null)
                extraDeckModeButtonHeaderLineRect.gameObject.SetActive(false);
            baseRect.DOAnchorPosX(GetHiddenPosX(), 0.3f);
            if(showWithCloseDuelLog)
            {
                showWithCloseDuelLog = false;
                Program.instance.ocgcore.GetUI<OcgCoreUI>().OnLog();
            }
        }

        void RefreshList()
        {
            EnsureWidthForLocation();
            UpdateListHeaderLayout();
            EnsureExtraDeckModeToggle();
            UpdateExtraDeckModeToggle();
            locationIcon.sprite = GetListLocationIcon(location, controller);
            ClearList();

            if (ShouldUseExtraDeckGrid())
            {
                RefreshExtraDeckGrid();
                return;
            }

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, SingleColumnItemStep * cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                var go = Instantiate(item);
                go.SetActive(true);
                cardObjs.Add(go);
                go.transform.SetParent(scrollRect.content, false);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -SingleColumnItemStep * (cards.Count - 1 - i));
                var mono = go.GetComponent<CardListItem>();
                mono.card = cards[i];
            }
        }

        void RefreshExtraDeckGrid()
        {
            var rowCount = Mathf.CeilToInt(cards.Count / (float)ExtraDeckColumns);
            var usableWidth = Mathf.Max(ExtraDeckBaseItemWidth,
                baseRect.rect.width - ExtraDeckScrollbarWidth - ExtraDeckHorizontalInset * 2f);
            var itemScale = Mathf.Clamp(
                (usableWidth - ExtraDeckHorizontalGap * (ExtraDeckColumns - 1)) / (ExtraDeckBaseItemWidth * ExtraDeckColumns),
                0.35f, 1f);
            var scaledItemWidth = ExtraDeckBaseItemWidth * itemScale;
            var scaledItemHeight = ExtraDeckVisualItemHeight * itemScale;
            var rowHeight = scaledItemHeight + ExtraDeckVerticalGap;
            var usedWidth = scaledItemWidth * ExtraDeckColumns + ExtraDeckHorizontalGap * (ExtraDeckColumns - 1);
            var startX = Mathf.Max(0f, (usableWidth - usedWidth) * 0.5f) + ExtraDeckHorizontalInset;

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, rowHeight * rowCount);
            AddExtraDeckRowLines(rowCount, usableWidth, rowHeight);
            for (int i = 0; i < cards.Count; i++)
            {
                var go = Instantiate(item);
                go.SetActive(true);
                cardObjs.Add(go);
                go.transform.SetParent(scrollRect.content, false);

                var visualIndex = cards.Count - 1 - i;
                var row = visualIndex / ExtraDeckColumns;
                var column = visualIndex % ExtraDeckColumns;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(ExtraDeckBaseItemWidth, ExtraDeckRowLineHeight);
                rect.localScale = Vector3.one * itemScale;
                rect.anchoredPosition = new Vector2(
                    startX + column * (scaledItemWidth + ExtraDeckHorizontalGap),
                    -row * rowHeight);
                if (go.TryGetComponent(out Image cardLineImage))
                {
                    var c = cardLineImage.color;
                    c.a = 0f;
                    cardLineImage.color = c;
                }

                var mono = go.GetComponent<CardListItem>();
                mono.card = cards[i];
            }
        }

        void AddExtraDeckRowLines(int rowCount, float usableWidth, float rowHeight)
        {
            var lineSprite = item != null && item.TryGetComponent(out Image itemBg) ? itemBg.sprite : null;
            for (int i = 0; i <= rowCount; i++)
            {
                var line = new GameObject($"ExtraDeckRowLine_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                line.transform.SetParent(scrollRect.content, false);
                cardObjs.Add(line);

                var lineRect = line.GetComponent<RectTransform>();
                lineRect.anchorMin = new Vector2(0f, 1f);
                lineRect.anchorMax = new Vector2(0f, 1f);
                lineRect.pivot = new Vector2(0f, 1f);
                lineRect.sizeDelta = new Vector2(usableWidth, ExtraDeckRowLineHeight);
                lineRect.anchoredPosition = new Vector2(ExtraDeckHorizontalInset, -i * rowHeight);

                var image = line.GetComponent<Image>();
                image.sprite = lineSprite;
                image.type = Image.Type.Simple;
                image.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);
                image.raycastTarget = false;
            }
        }

        void EnsureWidthForLocation()
        {
            if (defaultBaseWidth < 0f)
                defaultBaseWidth = baseRect.sizeDelta.x;

            var targetWidth = defaultBaseWidth;
            if (ShouldUseExtraDeckGrid())
                targetWidth = Mathf.Max(defaultBaseWidth, GetExtraDeckRequiredWidth());

            if (!Mathf.Approximately(baseRect.sizeDelta.x, targetWidth))
                baseRect.sizeDelta = new Vector2(targetWidth, baseRect.sizeDelta.y);
        }

        float GetExtraDeckRequiredWidth()
        {
            return ExtraDeckScrollbarWidth
                + ExtraDeckHorizontalInset * 2f
                + ExtraDeckBaseItemWidth * ExtraDeckColumns
                + ExtraDeckHorizontalGap * (ExtraDeckColumns - 1);
        }

        bool ShouldUseExtraDeckGrid()
        {
            return IsToggleListLocation(location) && extraDeckGridMode;
        }

        void EnsureListLayoutBindings()
        {
            if (locationIcon != null)
            {
                if (locationIconRect == null)
                    locationIconRect = locationIcon.rectTransform;
                if (listHeaderRect == null)
                    listHeaderRect = locationIconRect.parent as RectTransform;
                locationIcon.preserveAspect = true;
            }

            if (scrollRect != null)
            {
                if (listScrollRect == null)
                    listScrollRect = scrollRect.transform as RectTransform;
            }

            if (defaultListLayoutCached || locationIconRect == null || listHeaderRect == null || listScrollRect == null)
                return;

            defaultLocationIconAnchoredPosition = locationIconRect.anchoredPosition;
            defaultLocationIconSizeDelta = locationIconRect.sizeDelta;
            defaultHeaderSizeDelta = listHeaderRect.sizeDelta;
            defaultScrollViewAnchoredPosition = listScrollRect.anchoredPosition;
            defaultScrollViewSizeDelta = listScrollRect.sizeDelta;
            defaultListLayoutCached = true;
        }

        void UpdateListHeaderLayout()
        {
            EnsureListLayoutBindings();
            if (!defaultListLayoutCached)
                return;

            var useExpandedExtraHeader = IsExtraListLocation(location);
            var headerHeight = useExpandedExtraHeader ? ExtraDeckHeaderHeight : defaultHeaderSizeDelta.y;

            listHeaderRect.sizeDelta = new Vector2(defaultHeaderSizeDelta.x, headerHeight);
            listScrollRect.anchoredPosition = new Vector2(defaultScrollViewAnchoredPosition.x, -headerHeight * 0.5f);
            listScrollRect.sizeDelta = new Vector2(defaultScrollViewSizeDelta.x, -headerHeight);

            locationIconRect.anchoredPosition = useExpandedExtraHeader
                ? new Vector2(defaultLocationIconAnchoredPosition.x, -ExtraDeckLocationIconTopInset)
                : defaultLocationIconAnchoredPosition;
            locationIconRect.sizeDelta = useExpandedExtraHeader
                ? new Vector2(ExtraDeckLocationIconSize, ExtraDeckLocationIconSize)
                : defaultLocationIconSizeDelta;
        }

        void EnsureExtraDeckModeToggle()
        {
            TryBindExistingExtraDeckModeToggle();
            if (extraDeckModeButton != null)
            {
                EnsureExtraDeckModeToggleHeaderLine();
                UpdateExtraDeckModeToggleLayout();
                return;
            }

            var parent = baseRect;
            if (parent == null)
                return;

            var go = new GameObject("ExtraDeckViewToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            var bgImage = go.GetComponent<Image>();
            var baseImage = baseRect != null ? baseRect.GetComponent<Image>() : null;
            var blackBg = TextureManager.container != null ? TextureManager.container.black : null;
            if (blackBg != null)
            {
                bgImage.sprite = blackBg;
                bgImage.type = Image.Type.Simple;
                bgImage.color = Color.white;
            }
            else if (baseImage != null)
            {
                bgImage.sprite = baseImage.sprite;
                bgImage.type = baseImage.type;
                bgImage.color = baseImage.color;
            }
            bgImage.raycastTarget = true;

            extraDeckModeButton = go.GetComponent<Button>();
            extraDeckModeButton.targetGraphic = bgImage;
            extraDeckModeButton.onClick.AddListener(OnExtraDeckModeButtonClick);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(go.transform, false);
            frame.transform.SetAsFirstSibling();
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;

            extraDeckModeButtonFrame = frame.GetComponent<Image>();
            extraDeckModeButtonFrame.sprite = TextureManager.container != null
                ? TextureManager.container.duelCardSelectionListFrame
                : null;
            extraDeckModeButtonFrame.type = Image.Type.Sliced;
            extraDeckModeButtonFrame.color = Color.white;
            extraDeckModeButtonFrame.raycastTarget = false;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, -1f);
            iconRect.sizeDelta = new Vector2(ExtraDeckToggleButtonIconSize, ExtraDeckToggleButtonIconSize);

            extraDeckModeButtonIcon = icon.GetComponent<Image>();
            extraDeckModeButtonIcon.preserveAspect = true;
            extraDeckModeButtonIcon.color = Color.white;
            extraDeckModeButtonIcon.raycastTarget = false;

            EnsureExtraDeckModeToggleHeaderLine();
            UpdateExtraDeckModeToggleLayout();
        }

        void TryBindExistingExtraDeckModeToggle()
        {
            if (extraDeckModeButton != null || baseRect == null)
                return;

            var existing = baseRect.Find("ExtraDeckViewToggle");
            if (existing == null)
                return;

            extraDeckModeButton = existing.GetComponent<Button>();
            if (extraDeckModeButton == null)
                return;

            var iconTransform = existing.Find("Icon");
            if (iconTransform != null)
                extraDeckModeButtonIcon = iconTransform.GetComponent<Image>();

            var frameTransform = existing.Find("Frame");
            if (frameTransform != null)
                extraDeckModeButtonFrame = frameTransform.GetComponent<Image>();

            TryBindExistingExtraDeckModeToggleHeaderLine();

            var bgImage = existing.GetComponent<Image>();
            if (bgImage != null)
                extraDeckModeButton.targetGraphic = bgImage;

            extraDeckModeButton.onClick.RemoveListener(OnExtraDeckModeButtonClick);
            extraDeckModeButton.onClick.AddListener(OnExtraDeckModeButtonClick);
        }

        void UpdateExtraDeckModeToggle()
        {
            if (extraDeckModeButton == null || extraDeckModeButtonIcon == null)
                return;

            UpdateExtraDeckModeToggleLayout();

            var isToggleLocation = IsToggleListLocation(location);
            extraDeckModeButton.gameObject.SetActive(isToggleLocation);
            if (extraDeckModeButtonHeaderLineRect != null)
                extraDeckModeButtonHeaderLineRect.gameObject.SetActive(isToggleLocation);
            if (!isToggleLocation)
                return;

            if (ShouldUseExtraDeckGrid())
                extraDeckModeButtonIcon.sprite = TextureManager.container.duelCardSelectionListViewIconVertical != null
                    ? TextureManager.container.duelCardSelectionListViewIconVertical
                    : TextureManager.container.listViewIconDefault != null
                        ? TextureManager.container.listViewIconDefault
                        : TextureManager.container.listMyDeck;
            else
                extraDeckModeButtonIcon.sprite = TextureManager.container.duelCardSelectionListViewIconHorizontal != null
                    ? TextureManager.container.duelCardSelectionListViewIconHorizontal
                    : TextureManager.container.listViewIconExpand != null
                        ? TextureManager.container.listViewIconExpand
                        : (controller == 0 ? TextureManager.container.listMyDeck : TextureManager.container.listOpDeck);
        }

        void OnExtraDeckModeButtonClick()
        {
            if (!IsToggleListLocation(location))
                return;

            extraDeckGridMode = !extraDeckGridMode;
            RefreshList();
        }

        bool IsToggleListLocation(CardLocation targetLocation)
        {
            return (targetLocation & (CardLocation.Extra | CardLocation.Grave | CardLocation.Removed)) > 0;
        }

        bool IsExtraListLocation(CardLocation targetLocation)
        {
            return (targetLocation & CardLocation.Extra) > 0;
        }

        float GetHiddenPosX()
        {
            return (baseRect.sizeDelta.x + HiddenPadding + GetToggleLeftProtrusion()) * Config.GetUIScale(1.18f)
                + SafeAreaAdapter.GetSafeAreaRightOffset();
        }

        float GetToggleLeftProtrusion()
        {
            if (extraDeckModeButton == null || !extraDeckModeButton.gameObject.activeSelf)
                return 0f;

            if (extraDeckModeButton.transform is RectTransform rt)
                return Mathf.Max(0f, -rt.anchoredPosition.x);

            return Mathf.Max(0f, -GetExtraDeckToggleAnchoredX());
        }

        float GetExtraDeckToggleAnchoredX()
        {
            return -ExtraDeckToggleButtonWidth + ExtraDeckToggleButtonOverlap;
        }

        float GetExtraDeckToggleHeaderLineAnchoredX()
        {
            return ExtraDeckToggleButtonWidth - ExtraDeckToggleButtonOverlap - 2f;
        }

        void UpdateExtraDeckModeToggleLayout()
        {
            if (extraDeckModeButton != null && extraDeckModeButton.transform is RectTransform buttonRect)
            {
                buttonRect.anchoredPosition = new Vector2(GetExtraDeckToggleAnchoredX(), -ExtraDeckToggleButtonTopInset);
                buttonRect.sizeDelta = new Vector2(ExtraDeckToggleButtonWidth, ExtraDeckToggleButtonHeight);
            }

            if (extraDeckModeButtonFrame != null && extraDeckModeButtonFrame.transform is RectTransform frameRect)
            {
                frameRect.anchorMin = Vector2.zero;
                frameRect.anchorMax = Vector2.one;
                frameRect.offsetMin = Vector2.zero;
                frameRect.offsetMax = Vector2.zero;
            }

            if (extraDeckModeButtonHeaderLineRect != null)
            {
                var width = Mathf.Max(0f,
                    baseRect.rect.width - GetExtraDeckToggleHeaderLineAnchoredX() - ExtraDeckToggleHeaderLineRightInset);
                extraDeckModeButtonHeaderLineRect.anchoredPosition =
                    new Vector2(GetExtraDeckToggleHeaderLineAnchoredX(), 0f);
                extraDeckModeButtonHeaderLineRect.sizeDelta =
                    new Vector2(width, ExtraDeckToggleHeaderLineHeight);
            }
        }

        void TryBindExistingExtraDeckModeToggleHeaderLine()
        {
            if (extraDeckModeButtonHeaderLineRect != null || baseRect == null)
                return;

            EnsureListLayoutBindings();
            var line = listHeaderRect != null
                ? listHeaderRect.Find("ExtraDeckHeaderLine")
                : null;
            if (line == null)
                line = baseRect.Find("ExtraDeckHeaderLine");
            if (line == null)
                return;

            extraDeckModeButtonHeaderLineRect = line as RectTransform;
            if (extraDeckModeButtonHeaderLineRect == null)
                extraDeckModeButtonHeaderLineRect = line.GetComponent<RectTransform>();

            var desiredParent = listHeaderRect != null ? listHeaderRect : baseRect;
            if (desiredParent != null && extraDeckModeButtonHeaderLineRect != null
                && extraDeckModeButtonHeaderLineRect.parent != desiredParent)
                extraDeckModeButtonHeaderLineRect.SetParent(desiredParent, false);
        }

        void EnsureExtraDeckModeToggleHeaderLine()
        {
            TryBindExistingExtraDeckModeToggleHeaderLine();
            EnsureListLayoutBindings();

            var parent = listHeaderRect != null ? listHeaderRect : baseRect;
            if (extraDeckModeButtonHeaderLineRect != null || parent == null)
                return;

            var blackBg = TextureManager.container != null ? TextureManager.container.black : null;
            var line = new GameObject("ExtraDeckHeaderLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            line.transform.SetParent(parent, false);
            line.transform.SetSiblingIndex(0);

            extraDeckModeButtonHeaderLineRect = line.GetComponent<RectTransform>();
            extraDeckModeButtonHeaderLineRect.anchorMin = new Vector2(0f, 0f);
            extraDeckModeButtonHeaderLineRect.anchorMax = new Vector2(0f, 0f);
            extraDeckModeButtonHeaderLineRect.pivot = new Vector2(0f, 0f);

            var lineImage = line.GetComponent<Image>();
            lineImage.sprite = blackBg;
            lineImage.type = Image.Type.Simple;
            lineImage.color = new Color(0.78f, 0.78f, 0.78f, 1f);
            lineImage.raycastTarget = false;
        }

        void ClearList()
        {
            foreach (var obj in cardObjs)
                Destroy(obj);
            cardObjs.Clear();
        }

        public static Sprite GetListLocationIcon(CardLocation location, int controller)
        {
            if (controller == 0)
            {
                if ((location & CardLocation.Deck) > 0)
                    return TextureManager.container.listMyDeck;
                else if ((location & CardLocation.Extra) > 0)
                    return TextureManager.container.listMyExtra;
                else if ((location & CardLocation.Grave) > 0)
                    return TextureManager.container.listMyGrave;
                else if ((location & CardLocation.Removed) > 0)
                    return TextureManager.container.listMyRemoved;
                else
                    return TextureManager.container.listMyXyz;
            }
            else
            {
                if ((location & CardLocation.Deck) > 0)
                    return TextureManager.container.listOpDeck;
                else if ((location & CardLocation.Extra) > 0)
                    return TextureManager.container.listOpExtra;
                else if ((location & CardLocation.Grave) > 0)
                    return TextureManager.container.listOpGrave;
                else if ((location & CardLocation.Removed) > 0)
                    return TextureManager.container.listOpRemoved;
                else
                    return TextureManager.container.listOpXyz;
            }
        }

    }
}
