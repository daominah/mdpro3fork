using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3.UI
{
    public class CardList : MonoBehaviour
    {
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
        public void Show(List<GameCard> cards, CardLocation location, int controller)
        {
            this.cards = cards;
            this.location = location;
            this.controller = controller;

            if (!showing)
            {
                RefreshList();
                baseRect.DOAnchorPosX(-30, transitionTime);
            }
            else
            {
                baseRect.DOAnchorPosX(150, transitionTime).OnComplete(() =>
                {
                    RefreshList();
                    baseRect.DOAnchorPosX(-30, transitionTime);
                });
            }

            showing = true;
        }

        public void Hide()
        {
            if (!showing)
                return;
            showing = false;
            baseRect.DOAnchorPosX(150, 0.3f);
        }

        void RefreshList()
        {
            locationIcon.sprite = GetListLocationIcon(location, controller);
            ClearList();
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 140 * cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                var go = Instantiate(item);
                cardObjs.Add(go);
                go.transform.SetParent(scrollRect.content, false);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140 * (cards.Count - 1 - i));
                var mono = go.GetComponent<CardListItem>();
                mono.card = cards[i];
            }
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
