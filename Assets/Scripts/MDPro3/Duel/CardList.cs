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
        public RectTransform baseRect;
        public Image locationIcon;
        public ScrollRect scrollRect;
        public GameObject item;
        public GameObject IconH;
        public GameObject IconV;

        private bool showing;
        private bool single = true;
        private List<GameCard> cards;
        private readonly List<GameObject> cardObjs = new();
        private const float transitionTime = 0.15f;
        private CardLocation location;
        private int controller;
        private bool showWithCloseDuelLog = false;

        public void Show(List<GameCard> cards, CardLocation location, int controller)
        {
            if(OcgCore.cantCheckGrave && location == CardLocation.Grave)
            {
                MessageManager.Cast(InterString.Get("现在不能查看此处的卡片。"));
                return;
            }

            this.cards = cards;
            this.location = location;
            this.controller = controller;

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
                baseRect.DOAnchorPosX(360, transitionTime).OnComplete(() =>
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
            baseRect.DOAnchorPosX(360f * Config.GetUIScale(1.18f) + SafeAreaAdapter.GetSafeAreaRightOffset(), 0.3f);
            if(showWithCloseDuelLog)
            {
                showWithCloseDuelLog = false;
                Program.instance.ocgcore.GetUI<OcgCoreUI>().OnLog();
            }
        }

        private void RefreshList()
        {
            locationIcon.sprite = GetListLocationIcon(location, controller);
            ClearList();
            for (int i = 0; i < cards.Count; i++)
            {
                var go = Instantiate(item);
                go.SetActive(true);
                cardObjs.Add(go);
                go.transform.SetParent(scrollRect.content, false);
                var mono = go.GetComponent<CardListItem>();
                mono.card = cards[i];
            }
            SetButtonState(true);
            RefreshScrollView(true);
            single = true;
        }

        private void RefreshScrollView(bool single)
        {
            if (single)
            {
                baseRect.sizeDelta = new Vector2(130f, 820f);
                scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 140 * cards.Count);
                for (int i = 0; i < cardObjs.Count; i++)
                    cardObjs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140 * (cards.Count - 1 - i));
            }
            else
            {
                baseRect.sizeDelta = new Vector2(320f, 820f);
                scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 140 * ((cards.Count / 3) + 1));
                for (int i = 0; i < cardObjs.Count; i++)
                    cardObjs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(GetCardXPosition(i), GetCardYPosition(i, cards.Count));
            }
        }

        private float GetCardXPosition(int index)
        {
            int sign = (index % 3) switch
            {
                0 => -1,
                1 => 0,
                2 => 1,
                _ => 0,
            };
            return sign * 94f;
        }

        private int GetCardYPosition(int index, int count)
        {
            int lines = (count - 1 - index) / 3;
            return -140 * lines;
        }

        private void ClearList()
        {
            foreach (var obj in cardObjs)
                Destroy(obj);
            cardObjs.Clear();
        }

        private void SetButtonState(bool single)
        {
            IconH.SetActive(!single);
            IconV.SetActive(single);
        }

        public void SwitchLayout()
        {
            SwitchLayout(!single);
        }

        private void SwitchLayout(bool single)
        {
            if (single == this.single)
                return;

            this.single = single;

            SetButtonState(single);
            RefreshScrollView(single);
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
