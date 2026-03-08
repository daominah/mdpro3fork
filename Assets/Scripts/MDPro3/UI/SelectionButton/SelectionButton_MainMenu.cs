using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using YgomSystem.UI;

namespace MDPro3.UI
{
    public class SelectionButton_MainMenu : SelectionButton
    {
        private const float ArrowRestX = -5f;
        private const float ArrowStartX = -262f;
        private const float ArrowTextGap = 30f;
        private static float s_BaseButtonWidth = -1f;
        private static float s_BaseTextWidth = -1f;

        protected override void Awake()
        {
            ElementsReset();
        }

        protected override void OnDisable()
        {
        }

        private void ElementsReset()
        {
            EnsureMainMenuButtonWidth();

            // Out
            Manager.GetElement<CanvasGroup>("Out").alpha = 1f;
            Manager.GetElement<RectTransform>("Line").localScale = Vector3.one;

            // Hover
            Manager.GetElement<CanvasGroup>("Hover").alpha = 0f;
            Manager.GetElement<RectTransform>("PlateTween").localScale = new Vector3(0.5f, 1f, 1f);
            Manager.GetElement<RectTransform>("HoverTextMask").offsetMax = new Vector2(-340f, 0f);
            Manager.GetElement<RectTransform>("Arrow").localPosition = new Vector2(ArrowRestX, 0f);

            // Cursor
            Manager.GetElement<RectTransform>("Corner").offsetMin = new Vector2(0f, 0f);
            Manager.GetElement<RectTransform>("Corner").offsetMax = new Vector2(0f, 0f);

            foreach (var ccg in transform.GetComponentsInChildren<ColorContainerGraphic>(true))
                ccg.SetColor(ColorContainer.SelectMode.Unselected, ColorContainer.StatusMode.Normal, true);
        }

        protected override void HoverOn()
        {
            if (hoverd)
                return;
            base.HoverOn();
            EnsureMainMenuButtonWidth();

            Manager.GetElement<CanvasGroup>("Out").alpha = 0f;

            Manager.GetElement<RectTransform>("PlateTween").localScale = new Vector3(0.5f, 1f, 1f);
            var tween1 = Manager.GetElement<RectTransform>("PlateTween").DOScaleX(1f, 0.33f).SetEase(Ease.OutQuart);
            hoverOnTweens.Add(tween1);

            Manager.GetElement<DOTweenAnimation>("PlateBlink").DOPlay();

            Manager.GetElement<RectTransform>("HoverTextMask").offsetMax = new Vector2(-340f, 0f);
            Manager.GetElement<RectTransform>("HoverTextMask").DOSizeDelta(Vector2.zero, 0.2f);

            Manager.GetElement<RectTransform>("Arrow").anchoredPosition = new Vector2(ArrowStartX, 0f);
            var tween2 = Manager.GetElement<RectTransform>("Arrow").DOAnchorPosX(ArrowRestX, 0.33f).SetEase(Ease.OutQuart);
            hoverOnTweens.Add(tween2);
        }

        private void EnsureMainMenuButtonWidth()
        {
            var selfRect = transform as RectTransform;
            if (selfRect == null || selfRect.parent == null)
                return;

            var selfTextRect = Manager.GetElement<RectTransform>("Text");
            if (s_BaseButtonWidth < 0f)
                s_BaseButtonWidth = selfRect.sizeDelta.x;
            if (s_BaseTextWidth < 0f && selfTextRect != null)
                s_BaseTextWidth = selfTextRect.rect.width;

            if (s_BaseButtonWidth <= 0f || s_BaseTextWidth <= 0f)
                return;

            var buttons = selfRect.parent.GetComponentsInChildren<SelectionButton_MainMenu>(true);
            var maxTargetWidth = s_BaseButtonWidth;

            foreach (var button in buttons)
            {
                if (button == null)
                    continue;

                var buttonRect = button.transform as RectTransform;
                if (buttonRect == null)
                    continue;

                var text = button.Manager.GetElement<TextMeshProUGUI>("Text");
                var textOver = button.Manager.GetElement<TextMeshProUGUI>("TextOver");
                var textRect = button.Manager.GetElement<RectTransform>("Text");
                if (text == null || textRect == null)
                    continue;

                text.ForceMeshUpdate();
                textOver?.ForceMeshUpdate();

                var maxLabelWidth = text.preferredWidth;
                if (textOver != null && textOver.preferredWidth > maxLabelWidth)
                    maxLabelWidth = textOver.preferredWidth;
                var neededTextWidth = maxLabelWidth + ArrowTextGap;
                var extraWidth = Mathf.Max(0f, neededTextWidth - s_BaseTextWidth);
                var targetWidth = s_BaseButtonWidth + extraWidth;

                if (targetWidth > maxTargetWidth)
                    maxTargetWidth = targetWidth;
            }

            var resolvedWidth = Mathf.Ceil(maxTargetWidth);

            foreach (var button in buttons)
            {
                if (button == null)
                    continue;
                var buttonRect = button.transform as RectTransform;
                if (buttonRect == null)
                    continue;
                if (Mathf.Abs(buttonRect.sizeDelta.x - resolvedWidth) < 0.01f)
                    continue;
                buttonRect.sizeDelta = new Vector2(resolvedWidth, buttonRect.sizeDelta.y);
            }
        }

        protected override void HoverOff(bool force = false)
        {
            base.HoverOff();

            Manager.GetElement<CanvasGroup>("Out").alpha = 1f;
            Manager.GetElement<RectTransform>("Line").localScale = new Vector3(7f, 1f, 1f);
            var tween1 = Manager.GetElement<RectTransform>("Line").DOScaleX(1f, 0.4f).SetEase(Ease.OutQuart);
            hoverOffTweens.Add(tween1);

            Manager.GetElement<DOTweenAnimation>("PlateBlink").DOPause();
        }

        protected override void OnNavigation(AxisEventData eventData)
        {
            base.OnNavigation(eventData);
            if (eventData.moveVector.y > 0 && Selectable.navigation.selectOnUp != null)
                UserInput.RumbleForUp();
            else if (eventData.moveVector.y < 0 && Selectable.navigation.selectOnDown != null)
                UserInput.RumbleForDown();
        }

        protected override void OnSelect(bool playSE)
        {
            base.OnSelect(playSE);
            Program.instance.menu.lastSelectedButton = this;
        }
    }
}
