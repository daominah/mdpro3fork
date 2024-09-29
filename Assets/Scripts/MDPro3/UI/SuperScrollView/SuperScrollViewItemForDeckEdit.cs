using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using static MDPro3.EditDeck;

namespace MDPro3.UI
{
    public class SuperScrollViewItemForDeckEdit : SuperScrollViewItem
    {
        public Button button;
        public Image limitIcon;
        public Image dot1;
        public Image dot2;
        public Image dot3;

        public int code;

        RawImage face;
        IEnumerator enummerator;

        private void Start()
        {
            button.GetComponent<EventDrag>().onClick = OnClick;
            button.GetComponent<EventDrag>().onClickRight = OnClickRight;
            button.GetComponent<EventDrag>().onBeginDrag = OnBeginDrag;
            button.GetComponent<EventDrag>().onDrag = OnDrag;
            button.GetComponent<EventDrag>().onEndDrag = OnEndDrag;
            var scale = Config.GetUIScale();
            transform.localScale = Vector3.one * scale;
        }
        public override void Refresh()
        {
            if (face == null)
                face = GetComponent<RawImage>();
            face.material = null;

            var data = CardsManager.Get(code);
            if ((data.Type & (uint)CardType.Pendulum) > 0)
            {
                if ((data.Type & (uint)CardType.Normal) > 0)
                    face.texture = TextureManager.container.cardFramePendulumNormal.texture;
                else if ((data.Type & (uint)CardType.Xyz) > 0)
                    face.texture = TextureManager.container.cardFramePendulumXyz.texture;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    face.texture = TextureManager.container.cardFramePendulumSynchro.texture;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    face.texture = TextureManager.container.cardFramePendulumFusion.texture;
                else if ((data.Type & (uint)CardType.Ritual) > 0)
                    face.texture = TextureManager.container.cardFramePendulumRitual.texture;
                else
                    face.texture = TextureManager.container.cardFramePendulumEffect.texture;
            }
            else
            {
                if ((data.Type & (uint)CardType.Normal) > 0)
                    face.texture = TextureManager.container.cardFrameNormal.texture;
                else if ((data.Type & (uint)CardType.Xyz) > 0)
                    face.texture = TextureManager.container.cardFrameXyz.texture;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    face.texture = TextureManager.container.cardFrameSynchro.texture;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    face.texture = TextureManager.container.cardFrameFusion.texture;
                else if ((data.Type & (uint)CardType.Ritual) > 0 && (data.Type & (uint)CardType.Monster) > 0)
                    face.texture = TextureManager.container.cardFrameRitual.texture;
                else if ((data.Type & (uint)CardType.Link) > 0)
                    face.texture = TextureManager.container.cardFrameLink.texture;
                else if ((data.Type & (uint)CardType.Spell) > 0)
                    face.texture = TextureManager.container.cardFrameSpell.texture;
                else if ((data.Type & (uint)CardType.Trap) > 0)
                    face.texture = TextureManager.container.cardFrameTrap.texture;
                else if ((data.Type & (uint)CardType.Token) > 0)
                    face.texture = TextureManager.container.cardFrameToken.texture;
                else
                    face.texture = TextureManager.container.cardFrameEffect.texture;
            }

            RefreshCountDot();
            RefreshLimiteIcon();
            if (enummerator != null)
                StopCoroutine(enummerator);
            enummerator = RefreshAsync();
            StartCoroutine(enummerator);
        }

        public void RefreshCountDot()
        {
            int max = Program.I().editDeck.banlist.GetQuantity(code);
            int count = Program.I().editDeck.GetCardCount(code);
            dot1.gameObject.SetActive(false);
            dot2.gameObject.SetActive(false);
            dot3.gameObject.SetActive(false);
            if (count > 0)
                dot1.gameObject.SetActive(true);
            if (count > 1)
                dot2.gameObject.SetActive(true);
            if (count > 2)
                dot3.gameObject.SetActive(true);
            if (max > count)
            {
                dot1.color = Color.white;
                dot2.color = Color.white;
                dot3.color = Color.white;
            }
            else if (max == count)
            {
                dot1.color = Color.yellow;
                dot2.color = Color.yellow;
                dot3.color = Color.yellow;
            }
            else
            {
                dot1.color = Color.red;
                dot2.color = Color.red;
                dot3.color = Color.red;
            }
            if (count == 1)
                dot1.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -127);
            else if (count == 2)
            {
                dot1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, -127);
                dot2.GetComponent<RectTransform>().anchoredPosition = new Vector2(5, -127);
            }
            else if (count == 3)
            {
                dot1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10, -127);
                dot2.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -127);
                dot3.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, -127);
            }
        }

        public void RefreshLimiteIcon()
        {
            var limit = Program.I().editDeck.banlist.GetQuantity(code);
            if (limit == 3)
                limitIcon.sprite = TextureManager.container.typeNone;
            else if (limit == 2)
                limitIcon.sprite = TextureManager.container.limit2;
            else if (limit == 1)
                limitIcon.sprite = TextureManager.container.limit1;
            else
                limitIcon.sprite = TextureManager.container.banned;
        }

        bool refreshed;
        IEnumerator RefreshAsync()
        {
            refreshed = false;
            for (int i = 0; i < transform.GetSiblingIndex() * 2; i++)
                yield return null;

            var task = TextureManager.LoadCardAsync(code);
            while(!task.IsCompleted)
                yield return null;

            var mat = TextureManager.GetCardMaterial(code, false);
            mat.mainTexture = task.Result;

            face.material = mat;
            face.material.SetTexture("_LoadingTex", face.texture);
            face.texture = null;

            face.material.SetFloat("_LoadingBlend", 1f);
            float blend = 1;
            DOTween.To(() => blend, x => { blend = x; face.material.SetFloat("_LoadingBlend", blend); }, 0f, 0.2f);
            enummerator = null;
            refreshed = true;
        }

        public void Dispose()
        {
            StartCoroutine(DisposeAsync());
        }

        IEnumerator DisposeAsync()
        {
            while(!refreshed)
                yield return null;
            Destroy(gameObject);
        }

        void OnClick(PointerEventData eventData)
        {
            if (!refreshed)
                return;
            var cardFace = GetComponent<RawImage>().material.mainTexture;
            var mat = GetComponent<RawImage>().material;
            if (Program.I().editDeck.manager.GetElement<Tab>("TabHistory").selected)
                Program.I().editDeck.Description(code, cardFace, mat, false);
            else
                Program.I().editDeck.Description(code, cardFace, mat);
        }

        void OnClickRight(PointerEventData eventData)
        {
            if (!Program.I().editDeck.deckIsFromLocalFile)
                return;

            if (Program.I().editDeck.condition == Condition.ChangeSide)
                return;
            var max = Program.I().editDeck.banlist.GetQuantity(code);
            var count = Program.I().editDeck.GetCardCount(code);
            if (count < max)
            {
                AudioManager.PlaySE("SE_DECK_PLUS");

                var item = Instantiate(Program.I().editDeck.itemOnTable);
                var handler = item.GetComponent<CardOnEdit>();
                handler.code = code;

                var card = CardsManager.Get(code);
                var isExtra = card.IsExtraCard();
                if (!isExtra)
                {
                    if (Program.I().editDeck.mainCount < 60)
                    {
                        handler.id = Program.I().editDeck.mainCount;
                        Program.I().editDeck.mainCount++;
                    }
                    else
                    {
                        handler.id = Program.I().editDeck.sideCount + 2000;
                        Program.I().editDeck.sideCount++;
                    }
                }
                else
                {
                    if (Program.I().editDeck.extraCount < 15)
                    {
                        handler.id = Program.I().editDeck.extraCount + 1000;
                        Program.I().editDeck.extraCount++;
                    }
                    else
                    {
                        handler.id = Program.I().editDeck.sideCount + 2000;
                        Program.I().editDeck.sideCount++;
                    }
                }

                item.transform.SetParent(Program.I().editDeck.cardsOnEditParent, false);

                handler.GetComponent<RectTransform>().anchoredPosition = handler.GetPosition();
                var endPositon = item.transform.position;

                item.transform.position = transform.position;
                var scale = Config.GetUIScale();
                item.transform.localScale = Vector3.one * 1.2f * scale;

                item.transform.DOMove(endPositon, CardOnEdit.moveTime);
                item.transform.DOScale(Vector3.one, CardOnEdit.moveTime);
                foreach (var c in Program.I().editDeck.cards)
                    c.Move();
                Program.I().editDeck.cards.Add(handler);
                Program.I().editDeck.RefreshListItemIcons();
            }
        }

        CardOnEdit dragItem;

        void OnBeginDrag(PointerEventData eventData)
        {
            if (!Program.I().editDeck.deckIsFromLocalFile)
                return;

            if (Program.I().editDeck.condition == Condition.ChangeSide)
                return;

            var item = Instantiate(Program.I().editDeck.itemOnTable);
            dragItem = item.GetComponent<CardOnEdit>();
            dragItem.code = code;
            dragItem.id = 99999999;

            var scale = Config.GetUIScale();
            dragItem.transform.SetParent(Program.I().editDeck.cardsOnEditParent, false);
            dragItem.transform.localScale = Vector3.one * scale * 1.2f;
            dragItem.button.GetComponent<Image>().raycastTarget = false;
        }
        void OnDrag(PointerEventData eventData)
        {
            if (!Program.I().editDeck.deckIsFromLocalFile)
                return;

            if (Program.I().editDeck.condition == Condition.ChangeSide)
                return;

            var dragTarget = dragItem.GetComponent<RectTransform>();
            Vector3 uiPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragTarget, eventData.position, eventData.enterEventCamera, out uiPosition);
            uiPosition.z = 90f;

            dragTarget.position = uiPosition;
        }
        void OnEndDrag(PointerEventData eventData)
        {
            if (!Program.I().editDeck.deckIsFromLocalFile)
                return;

            if (Program.I().editDeck.condition == Condition.ChangeSide)
                return;

            var max = Program.I().editDeck.banlist.GetQuantity(code);
            var count = Program.I().editDeck.GetCardCount(code);
            if (count >= max)
            {
                Destroy(dragItem.gameObject);
                return;
            }

            dragItem.button.GetComponent<Image>().raycastTarget = true;
            CardOnEdit hover = null;
            foreach (var card in Program.I().editDeck.cards)
                if (card.hover)
                {
                    hover = card;
                    break;
                }
            if (hover != null)
            {
                Program.I().editDeck.cards.Add(dragItem);
                Program.I().editDeck.SwitchCard(dragItem, hover);
            }
            else
            {
                var c = CardsManager.Get(code);
                var isExtra = c.IsExtraCard();

                if (Program.I().editDeck.manager.GetElement<UIHover>("DummyMain").hover)
                {
                    if (!isExtra)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.cards.Add(dragItem);
                        dragItem.id = Program.I().editDeck.mainCount;
                        Program.I().editDeck.mainCount++;
                    }
                    else
                    {
                        Destroy(dragItem.gameObject);
                        return;
                    }
                }
                else if (Program.I().editDeck.manager.GetElement<UIHover>("DummyExtra").hover)
                {
                    if (isExtra)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.cards.Add(dragItem);
                        dragItem.id = Program.I().editDeck.extraCount + 1000;
                        Program.I().editDeck.extraCount++;
                    }
                    else
                    {
                        Destroy(dragItem.gameObject);
                        return;
                    }
                }
                else if (Program.I().editDeck.manager.GetElement<UIHover>("DummySide").hover)
                {
                    Program.I().editDeck.dirty = true;
                    Program.I().editDeck.cards.Add(dragItem);
                    dragItem.id = Program.I().editDeck.sideCount + 2000;
                    Program.I().editDeck.sideCount++;
                }
                else
                {
                    Destroy(dragItem.gameObject);
                    return;
                }
            }
            foreach (var card in Program.I().editDeck.cards)
                card.Move();
            Program.I().editDeck.SetCardSiblingIndex(CardOnEdit.moveTime);
            Program.I().editDeck.RefreshListItemIcons();
        }

        private void OnDestroy()
        {
            //Resources.UnloadAsset(GetComponent<RawImage>().texture);
        }
    }
}
