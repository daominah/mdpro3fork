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
        const float ExtraDeckToggleButtonWidth = 64f;
        const float ExtraDeckToggleButtonHeight = 62f;
        const float ExtraDeckToggleButtonIconSize = 42f;
        const float ExtraDeckToggleButtonOverlap = 4f;
        const float ExtraDeckToggleButtonTopInset = 0f;
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
            if ((this.location & CardLocation.Extra) > 0 && (!showing || (previousLocation & CardLocation.Extra) == 0))
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
            return (location & CardLocation.Extra) > 0 && extraDeckGridMode;
        }

        void EnsureExtraDeckModeToggle()
        {
            if (extraDeckModeButton != null)
                return;

            var parent = baseRect;
            if (parent == null)
                return;

            var go = new GameObject("ExtraDeckViewToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(0);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(GetExtraDeckToggleAnchoredX(), -ExtraDeckToggleButtonTopInset);
            rect.sizeDelta = new Vector2(ExtraDeckToggleButtonWidth, ExtraDeckToggleButtonHeight);

            var bgImage = go.GetComponent<Image>();
            var baseImage = baseRect != null ? baseRect.GetComponent<Image>() : null;
            if (baseImage != null)
            {
                bgImage.sprite = baseImage.sprite;
                bgImage.type = baseImage.type;
                bgImage.color = baseImage.color;
            }
            bgImage.raycastTarget = true;

            extraDeckModeButton = go.GetComponent<Button>();
            extraDeckModeButton.targetGraphic = bgImage;
            extraDeckModeButton.onClick.AddListener(OnExtraDeckModeButtonClick);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(ExtraDeckToggleButtonIconSize, ExtraDeckToggleButtonIconSize);

            extraDeckModeButtonIcon = icon.GetComponent<Image>();
            extraDeckModeButtonIcon.preserveAspect = true;
            extraDeckModeButtonIcon.color = Color.white;
            extraDeckModeButtonIcon.raycastTarget = false;
        }

        void UpdateExtraDeckModeToggle()
        {
            if (extraDeckModeButton == null || extraDeckModeButtonIcon == null)
                return;

            var isExtraLocation = (location & CardLocation.Extra) > 0;
            extraDeckModeButton.gameObject.SetActive(isExtraLocation);
            if (!isExtraLocation)
                return;

            if (ShouldUseExtraDeckGrid())
                extraDeckModeButtonIcon.sprite = TextureManager.container.listViewIconDefault != null
                    ? TextureManager.container.listViewIconDefault
                    : TextureManager.container.listMyDeck;
            else
                extraDeckModeButtonIcon.sprite = TextureManager.container.listViewIconExpand != null
                    ? TextureManager.container.listViewIconExpand
                    : (controller == 0 ? TextureManager.container.listMyDeck : TextureManager.container.listOpDeck);
        }

        void OnExtraDeckModeButtonClick()
        {
            if ((location & CardLocation.Extra) == 0)
                return;

            extraDeckGridMode = !extraDeckGridMode;
            RefreshList();
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
