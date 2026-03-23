using DG.Tweening;
using MDPro3.Duel.YGOSharp;
using MDPro3.Servant;
using MDPro3.UI;
using MDPro3.UI.ServantUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.Duel
{
    public class DuelLog : MonoBehaviour
    {
        #region Mono

        public static Color myColor = new(0f, 0.18f, 0.76f, 1f);
        public static Color opColor = Color.red;
        public static Color myArrowColor = new(0f, 0.5f, 1f, 1f);
        public static Color opArrowColor = new(1f, 0.2f, 0.2f, 1f);
        public static Color myChainColor = new(0.2f, 0.6f, 1f, 1f);
        public static Color opChainColor = new(1f, 0.2f, 0.2f, 1f);
        public static Color damageColor = Color.red;
        public static Color recoverColor = new(0, 0.7f, 1f, 1f);
        public static Color boxedHeaderColor = new(0.42f, 0.44f, 0.44f, 0.92f);
        public static Color boxedBodyColor = new(0.34f, 0.35f, 0.35f, 0.92f);
        public static Color chainBandColor = new(0.42f, 0.44f, 0.47f, 0.9f);

        public RectTransform baseRect;
        public ScrollRect scrollRect;
        public bool showing;
        private bool draged = false;
        private float fullHeight;

        private void Start()
        {
            scrollRect.verticalScrollbar.onValueChanged.AddListener(Refresh);
            scrollRect.GetComponent<DoWhenOnDrag>().action = () => { draged = true; };
        }

        public void Show()
        {
            showing = true;
            draged = false;
            AudioManager.PlaySE("SE_LOG_OPEN");
            baseRect.DOAnchorPosX(-20f, 0.2f).SetUpdate(true);
            baseRect.localScale = Vector3.one * Config.GetUIScale(1.15f);
            SnapToLatest();
        }

        public void Hide(bool mute = false)
        {
            showing = false;
            draged = false;
            baseRect.DOAnchorPosX(400f * Config.GetUIScale(1.15f) + SafeAreaAdapter.GetSafeAreaRightOffset(), 0.2f).SetUpdate(true);

            if (!mute)
                AudioManager.PlaySE("SE_LOG_CLOSE");
        }

        public void AddLog(GameObject item, bool indent = false, bool chainIndent = true)
        {
            var rect = item.GetComponent<RectTransform>();
            var height = rect.rect.height;
            rect.SetParent(scrollRect.content, false);
            rect.sizeDelta = new Vector2(0, height);
            rect.anchoredPosition = new Vector2(0, -fullHeight);
            fullHeight += height;
            scrollRect.content.sizeDelta = new Vector2(0, fullHeight);

            if (indent || (chainIndent && LogMessage.chainSolvingIndex > 0 && rect.GetChild(1).name == "Image Side"))
            {
                rect.GetChild(0).gameObject.SetActive(false);
                rect.GetChild(1).gameObject.SetActive(false);
                rect.offsetMin = new Vector2(50f, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
            }
            if (!showing && fullHeight > scrollRect.viewport.rect.height)
                item.SetActive(false);
            if (!draged)
                scrollRect.DOVerticalNormalizedPos(0f, 0.1f).SetUpdate(true);
        }

        public void ClearLog()
        {
            scrollRect.content.DestroyAllChildren();
            fullHeight = 0;
        }

        private void ConfigureBoxedTextItem(GameObject item)
        {
            var rootRect = item.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, 36f);

            var backgroundRect = item.transform.GetChild(0) as RectTransform;
            var backgroundImage = item.transform.GetChild(0).GetComponent<Image>();
            backgroundImage.color = boxedHeaderColor;
            backgroundRect.offsetMin = new Vector2(56f, 0f);
            backgroundRect.offsetMax = new Vector2(-8f, -6f);

            var textRect = item.transform.GetChild(1) as RectTransform;
            textRect.offsetMin = new Vector2(66f, 0f);
            textRect.offsetMax = new Vector2(-12f, -6f);
        }

        private void ConfigureBoxedSingleCardItem(GameObject item)
        {
            var sideRect = item.transform.GetChild(1) as RectTransform;
            var sideImage = item.transform.GetChild(1).GetComponent<Image>();
            sideImage.color = boxedBodyColor;
            sideRect.anchorMin = Vector2.zero;
            sideRect.anchorMax = Vector2.one;
            sideRect.offsetMin = new Vector2(56f, 6f);
            sideRect.offsetMax = new Vector2(-8f, 0f);
            item.transform.GetChild(0).gameObject.SetActive(false);

            var cardNameRect = item.transform.GetChild(2) as RectTransform;
            cardNameRect.offsetMin = new Vector2(66f, cardNameRect.offsetMin.y);
            cardNameRect.offsetMax = new Vector2(-12f, cardNameRect.offsetMax.y);

            var reasonRect = item.transform.GetChild(3) as RectTransform;
            reasonRect.anchoredPosition = new Vector2(206f, reasonRect.anchoredPosition.y);
            reasonRect.sizeDelta = new Vector2(135f, reasonRect.sizeDelta.y);

            var cardRect = item.transform.GetChild(4) as RectTransform;
            cardRect.anchoredPosition = new Vector2(94f, cardRect.anchoredPosition.y);

            var fromStateRect = item.transform.GetChild(5) as RectTransform;
            fromStateRect.anchoredPosition = new Vector2(152f, fromStateRect.anchoredPosition.y);

            var arrowRect = item.transform.GetChild(6) as RectTransform;
            arrowRect.anchoredPosition = new Vector2(210f, arrowRect.anchoredPosition.y);

            var toStateRect = item.transform.GetChild(7) as RectTransform;
            toStateRect.anchoredPosition = new Vector2(268f, toStateRect.anchoredPosition.y);
        }

        public void ConfigureChainingItem(GameObject item)
        {
            var hasCard = item.transform.GetChild(3).gameObject.activeSelf;
            var topGap = hasCard ? 4f : 0f;
            var rootRect = item.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, 30f + topGap);

            var backgroundRect = item.transform.GetChild(0) as RectTransform;
            backgroundRect.anchorMin = new Vector2(0f, 1f);
            backgroundRect.anchorMax = new Vector2(1f, 1f);
            backgroundRect.pivot = new Vector2(0.5f, 1f);
            backgroundRect.anchoredPosition = new Vector2(0f, -topGap);
            backgroundRect.sizeDelta = new Vector2(0f, 30f);

            var backgroundImage = item.transform.GetChild(0).GetComponent<Image>();
            backgroundImage.color = chainBandColor;

            var backgroundButton = item.transform.GetChild(0).GetComponent<Button>();
            var colors = backgroundButton.colors;
            colors.normalColor = chainBandColor;
            colors.highlightedColor = new Color(chainBandColor.r + 0.05f, chainBandColor.g + 0.05f, chainBandColor.b + 0.05f, chainBandColor.a);
            colors.pressedColor = new Color(chainBandColor.r * 0.9f, chainBandColor.g * 0.9f, chainBandColor.b * 0.9f, chainBandColor.a);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = chainBandColor;
            backgroundButton.colors = colors;

            var chainRect = item.transform.GetChild(1) as RectTransform;
            chainRect.anchorMin = new Vector2(0f, 1f);
            chainRect.anchorMax = new Vector2(0f, 1f);
            chainRect.pivot = new Vector2(0f, 1f);
            chainRect.anchoredPosition = new Vector2(20f, -topGap);
            chainRect.sizeDelta = new Vector2(30f, 30f);

            var textRect = item.transform.GetChild(2) as RectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(-16f, -topGap);
            textRect.sizeDelta = new Vector2(-176f, 30f);

            var cardRect = item.transform.GetChild(3) as RectTransform;
            cardRect.anchorMin = new Vector2(1f, 1f);
            cardRect.anchorMax = new Vector2(1f, 1f);
            cardRect.pivot = new Vector2(1f, 0.5f);
            cardRect.anchoredPosition = new Vector2(-8f, -15f - topGap);
            cardRect.sizeDelta = new Vector2(25f, 32f);
        }

        public void AddTextMessageToLog(string text, bool indent = false, bool boxed = false)
        {
            var item = ABLoader.LoadMasterDuelGameObject(LogMessage.chainSolvingIndex > 0 ? "DuelLogText2" : "DuelLogText");
            item.transform.GetChild(1).GetComponent<Text>().text = text;
            if (boxed)
                ConfigureBoxedTextItem(item);
            AddLog(item, indent);
        }

        private void SnapToLatest()
        {
            draged = false;
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0f;
            Refresh(0f);
        }

        private void Refresh(float value)
        {
            if (!showing)
                return;
            draged = value > 0.02f;
            var visibleRect = GetVisibleRect();
            int stage = 0;
            bool visible = false;
            if (value > 0.5f)
            {
                for (int i = 0; i < scrollRect.content.childCount; i++)
                {
                    var childRect = scrollRect.content.GetChild(i) as RectTransform;
                    if (stage < 2)
                    {
                        var isVisible = IsRectVisible(childRect, visibleRect);
                        if (visible != isVisible)
                        {
                            visible = isVisible;
                            stage++;
                        }
                        childRect.gameObject.SetActive(isVisible);
                    }
                    else
                        childRect.gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = scrollRect.content.childCount - 1; i >= 0; i--)
                {
                    var childRect = scrollRect.content.GetChild(i) as RectTransform;
                    if (stage < 2)
                    {
                        var isVisible = IsRectVisible(childRect, visibleRect);
                        if (visible != isVisible)
                        {
                            visible = isVisible;
                            stage++;
                        }
                        childRect.gameObject.SetActive(isVisible);
                    }
                    else
                        childRect.gameObject.SetActive(false);
                }
            }
        }

        private Rect GetVisibleRect()
        {
            Rect viewportRect = scrollRect.viewport.rect;

            float top = -scrollRect.content.anchoredPosition.y;
            float bottom = top - viewportRect.height;

            Rect visibleRect = new(0f, bottom, viewportRect.width, viewportRect.height);
            return visibleRect;
        }

        private bool IsRectVisible(RectTransform rectTransform, Rect visibleRect)
        {
            float top = rectTransform.anchoredPosition.y;
            float bottom = top - rectTransform.rect.height;
            return top > visibleRect.yMin && bottom < visibleRect.yMax;
        }

        #endregion

        #region Message

        public void AddSingleCardMessageToLog(int code, GPS from, GPS to, string reason, bool indent = false, bool boxed = false)
        {
            var core = Program.instance.ocgcore;

            var item = ABLoader.LoadMasterDuelGameObject(code > 0 ? "DuelLogSingleCard" : "DuelLogSingleCard2");
            var sideImage = item.transform.GetChild(1).GetComponent<Image>();
            Color targetColor = boxed ? boxedBodyColor : (to.InMyControl() ? myColor : opColor);
            if (!boxed)
                targetColor.a = 0.75f;
            sideImage.color = targetColor;
            if (boxed)
                ConfigureBoxedSingleCardItem(item);

            if (code > 0)
                item.transform.GetChild(2).GetComponent<Text>().text = CardsManager.Get(code).Name;

            item.transform.GetChild(3).GetComponent<Text>().text = reason;

            var cardFace = item.transform.GetChild(4).GetComponent<RawImage>();
            if (code > 0)
                _ = Program.instance.texture_.LoadCardToRawImageWithoutMaterialAsync(cardFace, code, true);
            else
            {
                cardFace.texture = null;
                cardFace.material = to.controller == 0 ? OcgCore.myProtector : OcgCore.opProtector;
                cardFace.transform.GetChild(0).gameObject.SetActive(false);
            }
            cardFace.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                core.GetUI<OcgCoreUI>().CardDescription.Show(null, null, code, to);
            });

            if (to.InPosition(CardPosition.Defence) && to.InLocation(CardLocation.MonsterZone))
                cardFace.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            if(to.InPosition(CardPosition.FaceUp))
                cardFace.transform.GetChild(0).gameObject.SetActive(false);

            List<Sprite> icons;
            icons = TextureManager.container.GetLocationIcons(from ?? to);

            if (icons.Count == 2)
            {
                item.transform.GetChild(5).GetComponent<Image>().sprite = icons[1];
                item.transform.GetChild(5).GetChild(0).GetComponent<Image>().sprite = icons[0];
            }
            else
            {
                item.transform.GetChild(5).GetComponent<Image>().sprite = icons[0];
                item.transform.GetChild(5).GetChild(0).gameObject.SetActive(false);
                if (from == null)
                {
                    var reasonText = item.transform.GetChild(3).GetComponent<Text>();
                    reasonText.alignment = TextAnchor.MiddleLeft;
                    var reasonRect = item.transform.GetChild(3) as RectTransform;
                    reasonRect.pivot = new Vector2(0f, 0.5f);
                    reasonRect.anchoredPosition = new Vector2(82f, reasonRect.anchoredPosition.y);
                    reasonRect.sizeDelta = new Vector2(128f, reasonRect.sizeDelta.y);
                }
            }

            if (from != null)
            {
                if (to.controller == 0)
                    item.transform.GetChild(6).GetComponent<Image>().color = DuelLog.myArrowColor;
                else
                    item.transform.GetChild(6).GetComponent<Image>().color = DuelLog.opArrowColor;
                icons = TextureManager.container.GetLocationIcons(to);
                if (icons.Count == 2)
                {
                    item.transform.GetChild(7).GetComponent<Image>().sprite = icons[1];
                    item.transform.GetChild(7).GetChild(0).GetComponent<Image>().sprite = icons[0];
                }
                else
                {
                    item.transform.GetChild(7).GetComponent<Image>().sprite = icons[0];
                    item.transform.GetChild(7).GetChild(0).gameObject.SetActive(false);
                }
            }
            else
            {
                item.transform.GetChild(6).gameObject.SetActive(false);
                item.transform.GetChild(7).gameObject.SetActive(false);
            }

            AddLog(item, indent, !boxed);

#if UNITY_EDITOR
            if (OcgCore.currentMessage == GameMessage.Move
                || OcgCore.currentMessage == GameMessage.Summoning
                || OcgCore.currentMessage == GameMessage.SpSummoning)
            {
                var targetReason = LogMessage.lastMoveReason;
                item.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.LogFormat("{0:X}", targetReason);
                });
            }

            item.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.LogFormat("Location: {0:X}, Sequence: {1}, Position: {2:X}", from.location, from.sequence, from.position);
            });
            if (to != null)
            {
                item.transform.GetChild(7).GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.LogFormat("Location: {0:X}, Sequence: {1}, Position: {2:X}", to.location, to.sequence, to.position);
                });
            }
#endif
        }

        public void AddOpeningDrawMessageToLog(IReadOnlyList<int> codes, GPS to, string reason)
        {
            var core = Program.instance.ocgcore;
            var item = ABLoader.LoadMasterDuelGameObject("DuelLogSingleCard");
            var rootRect = item.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, 148f);

            var sideRect = item.transform.GetChild(1) as RectTransform;
            var sideImage = item.transform.GetChild(1).GetComponent<Image>();
            var targetColor = to.InMyControl() ? myColor : opColor;
            targetColor.a = 0.75f;
            sideImage.color = targetColor;

            var titleRect = item.transform.GetChild(2) as RectTransform;
            var titleText = item.transform.GetChild(2).GetComponent<Text>();
            titleText.text = reason;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(8f, -2f);
            titleRect.sizeDelta = new Vector2(128f, 20f);

            item.transform.GetChild(3).gameObject.SetActive(false);
            item.transform.GetChild(6).gameObject.SetActive(false);
            item.transform.GetChild(7).gameObject.SetActive(false);

            var icons = TextureManager.container.GetLocationIcons(to);
            var fromState = item.transform.GetChild(5);
            if (icons.Count > 0)
            {
                fromState.GetComponent<Image>().sprite = icons[0];
                fromState.GetChild(0).gameObject.SetActive(false);
                var fromStateRect = fromState as RectTransform;
                fromStateRect.anchoredPosition = new Vector2(28f, -46f);
            }
            else
            {
                fromState.gameObject.SetActive(false);
            }

            var count = Mathf.Max(1, codes.Count);
            const float cardAreaLeft = 30f;
            const float cardAreaWidth = 272f;
            const float maxCardWidth = 49f;
            const float minCardWidth = 33f;
            const float cardGap = 6f;
            var cardWidth = count > 1
                ? Mathf.Clamp((cardAreaWidth - cardGap * (count - 1)) / count, minCardWidth, maxCardWidth)
                : maxCardWidth;
            var spacing = count > 1 ? cardWidth + cardGap : 0f;
            var cardHeight = Mathf.Round(cardWidth * 1.45f);
            var startX = cardAreaLeft + cardWidth * 0.5f;
            var sideWidth = cardAreaLeft + cardWidth * count + cardGap * Mathf.Max(0, count - 1) + 18f;
            sideRect.sizeDelta = new Vector2(Mathf.Max(sideRect.sizeDelta.x, sideWidth), sideRect.sizeDelta.y);

            var cardTemplate = item.transform.GetChild(4).gameObject;
            for (int i = 0; i < count; i++)
            {
                var cardObject = i == 0 ? cardTemplate : Instantiate(cardTemplate, item.transform);
                var cardRect = cardObject.GetComponent<RectTransform>();
                cardRect.anchoredPosition = new Vector2(startX + spacing * i, -106f);
                cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

                var cardFace = cardObject.GetComponent<RawImage>();
                cardFace.texture = null;
                cardFace.material = null;
                cardFace.transform.GetChild(0).gameObject.SetActive(false);

                var button = cardFace.transform.GetChild(1).GetComponent<Button>();
                button.onClick.RemoveAllListeners();

                var code = codes[i];
                if (code > 0)
                {
                    _ = Program.instance.texture_.LoadCardToRawImageWithoutMaterialAsync(cardFace, code, true);
                    var targetCode = code;
                    var targetTo = to;
                    button.interactable = true;
                    button.onClick.AddListener(() =>
                    {
                        core.GetUI<OcgCoreUI>().CardDescription.Show(null, null, targetCode, targetTo);
                    });
                }
                else
                {
                    button.interactable = false;
                    cardFace.material = to.controller == 0 ? OcgCore.myProtector : OcgCore.opProtector;
                }
            }

            AddLog(item);
        }

        public void AddLpPChangeMessageToLog(int player, string reason, int value, bool red = true, bool indent = false)
        {
            var core = Program.instance.ocgcore;

            var item = ABLoader.LoadMasterDuelGameObject("DuelLogLpChange");
            var targetColor = player == 0 ? myColor : opColor;
            item.transform.GetChild(1).GetComponent<Image>().color = targetColor;
            var frame = item.transform.GetChild(2).GetComponent<Image>();
            frame.material = player == 0 ? core.GetUI<OcgCoreUI>().AvatarPlayer0.material : core.GetUI<OcgCoreUI>().AvatarPlayer1.material;
            frame.sprite = player == 0 ? core.GetUI<OcgCoreUI>().AvatarPlayer0.sprite : core.GetUI<OcgCoreUI>().AvatarPlayer1.sprite;
            item.transform.GetChild(3).GetComponent<Text>().text = reason;
            item.transform.GetChild(4).GetComponent<Text>().text = value.ToString();
            item.transform.GetChild(4).GetComponent<Text>().color = red ? damageColor : recoverColor;
            var lp = player == 0 ? LogMessage.life0 : LogMessage.life1;
            if (lp < 0)
                lp = 0;
            item.transform.GetChild(6).GetComponent<Text>().text = lp.ToString();
            AddLog(item, indent);
        }

        #endregion
    }
}
