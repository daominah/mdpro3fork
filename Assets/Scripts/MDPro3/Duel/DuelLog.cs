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

        public static Color myColor = Color.blue;
        public static Color opColor = Color.red;
        public static Color myArrowColor = new(0f, 0.5f, 1f, 1f);
        public static Color opArrowColor = new(1f, 0.2f, 0.2f, 1f);
        public static Color myChainColor = new(0.2f, 0.6f, 1f, 1f);
        public static Color opChainColor = new(1f, 0.2f, 0.2f, 1f);
        public static Color damageColor = Color.red;
        public static Color recoverColor = new(0, 0.7f, 1f, 1f);

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
            AudioManager.PlaySE("SE_LOG_OPEN");
            baseRect.DOAnchorPosX(-20f, 0.2f).SetUpdate(true);
            baseRect.localScale = Vector3.one * Config.GetUIScale(1.15f);
        }

        public void Hide(bool mute = false)
        {
            showing = false;
            draged = false;
            baseRect.DOAnchorPosX(400f * Config.GetUIScale(1.15f) + SafeAreaAdapter.GetSafeAreaRightOffset(), 0.2f).SetUpdate(true);

            if (!mute)
                AudioManager.PlaySE("SE_LOG_CLOSE");
        }

        public void AddLog(GameObject item, bool indent = false)
        {
            var rect = item.GetComponent<RectTransform>();
            var height = rect.rect.height;
            rect.SetParent(scrollRect.content, false);
            rect.sizeDelta = new Vector2(0, height);
            rect.anchoredPosition = new Vector2(0, -fullHeight);
            fullHeight += height;
            scrollRect.content.sizeDelta = new Vector2(0, fullHeight);

            if (indent || LogMessage.chainSolvingIndex > 0 && rect.GetChild(1).name == "Image Side")
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

        private void Refresh(float value)
        {
            if (!showing)
                return;
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

        public void AddSingleCardMessageToLog(int code, GPS from, GPS to, string reason, bool indent = false)
        {
            var core = Program.instance.ocgcore;

            var item = ABLoader.LoadMasterDuelGameObject(code > 0 ? "DuelLogSingleCard" : "DuelLogSingleCard2");
            Color targetColor = to.InMyControl() ? myColor : opColor;
            targetColor.a = 0.75f;
            item.transform.GetChild(1).GetComponent<Image>().color = targetColor;

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

            AddLog(item, indent);

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
