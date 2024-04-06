using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3.UI
{
    public class PopupDuelSelectCardItem : MonoBehaviour
    {
        public Image head;
        public Image locationIcon;
        public RawImage cardFace;
        public GameObject cardBack;
        public Button button;
        public Image levelIcon;
        public Text textBlack;
        public Text textWhite;
        public GameObject checkOn;
        public GameObject orderBase;
        public Text orderText;
        public GameObject chain;
        public Text chainText;
        public GameObject target;

        public int id;
        public List<GameCard> cards;
        public PopupDuelSelectCard manager;

        public GameCard card;
        static Color opColor = new Color(0.9f, 0, 0, 1);

        public bool selected;
        public bool unselectable;
        static Color unselectableColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public bool preselected;

        private void Start()
        {
            StartCoroutine(RefreshCard(card.GetData().Id));

            if ((card.p.location & (uint)CardLocation.Search) > 0)
            {
                GetComponent<Image>().color = Color.black;
                head.color = Color.black;
            }
            else if (card.p.controller != 0)
            {
                GetComponent<Image>().color = opColor;
                head.color = opColor;
            }

            bool showHead = false;
            if (id == 0)
                showHead = true;
            else if (card.p.location != cards[id - 1].p.location)
                showHead = true;
            if (showHead)
                locationIcon.sprite = TextureManager.GetCardLocationIcon(card.p);
            else
                head.gameObject.SetActive(false);

            bool isEnd = false;
            if (id == cards.Count - 1)
                isEnd = true;
            else if (card.p.location != cards[id + 1].p.location)
                isEnd = true;
            if (isEnd)
                GetComponent<RectTransform>().sizeDelta = new Vector2(145, 180);
            else
                GetComponent<RectTransform>().sizeDelta = new Vector2(180, 180);


            if ((card.p.position & (uint)CardPosition.FaceUp) > 0)
                cardBack.SetActive(false);

            if (card.chains.Count > 0)
            {
                chain.SetActive(true);
                chainText.text = card.chains[0].i.ToString();
            }
            else
            {
                chain.SetActive(false);

                if (Program.I().ocgcore.cardsBeTarget.Contains(card))
                    target.SetActive(true);
                else
                    target.SetActive(false);
            }

            if ((CardsManager.Get(card.GetData().Id).Type & (uint)CardType.Monster) > 0)
            {
                levelIcon.sprite = TextureManager.GetCardLevelIcon(card.GetData());
                if ((card.GetData().Type & (uint)CardType.Link) > 0)
                    textBlack.text = CardDescription.GetCardLinkCount(card.GetData()).ToString();
                else
                    textBlack.text = card.GetData().Level.ToString();
                textWhite.text = textBlack.text;
            }
            else
            {
                levelIcon.sprite = TextureManager.container.typeNone;
                textBlack.text = "";
                textWhite.text = "";
            }

            button.onClick.AddListener(OnClick);
        }

        IEnumerator RefreshCard(int code)
        {
            cardFace.texture = TextureManager.container.unknownCard.texture;
            var ie = Program.I().texture_.LoadCardAsync(code, true);
            while (ie.MoveNext())
                yield return null;
            var mat = TextureManager.GetCardMaterial(code);
            cardFace.material = mat;
            cardFace.material.mainTexture = ie.Current;
            cardFace.texture = ie.Current;
        }

        float clickTime;
        void OnClick()
        {
            AudioManager.PlaySE("SE_MENU_SELECT_01");
            if ((card.p.location & (uint)CardLocation.Onfield) > 0
                && (card.p.location & (uint)CardLocation.Overlay) == 0)
            {
                if (manager.arrow == null)
                {
                    manager.arrow = ABLoader.LoadFromFile("Effects/other/fxp_arrow_aim_001", true);
                    Program.I().ocgcore.allGameObjects.Add(manager.arrow);
                }
                manager.arrow.transform.position = card.model.transform.position;
            }
            else
            {
                if (manager.arrow != null)
                    manager.arrow.SetActive(false);
            }

            Program.I().ocgcore.description.Show(card, cardFace.material);

            if (selected)
            {
                if (!unselectable)
                {
                    if ((Time.time - clickTime) < 0.2f)
                    {
                        if (manager.selectedCount == 1 && manager.min == 1 && manager.max == 1)
                            manager.OnConfirm();
                        else
                            UnselectThis();
                    }
                    else
                        UnselectThis();
                }
            }
            else
            {
                if (!unselectable)
                {
                    SelectThis();
                    clickTime = Time.time;
                }
                else
                {
                    if (manager.max == 1 && manager.min == 1)
                    {
                        foreach (var card in manager.monos)
                            card.UnselectThis();
                        SelectThis();
                        clickTime = Time.time;
                    }
                }
            }

        }

        void SelectThis()
        {
            if (selected) return;
            selected = true;
            manager.selectedCount++;

            if (!manager.order)
                checkOn.SetActive(true);
            else
            {
                orderBase.SetActive(true);
                orderText.text = manager.selectedCount.ToString();
            }
        }

        public void RemoveOrder(int i)
        {
            if (!selected)
                return;
            int order = int.Parse(orderText.text);
            if (order > i)
                orderText.text = (order - 1).ToString();
        }

        public int GetOrder()
        {
            return int.Parse(orderText.text);
        }

        void UnselectThis()
        {
            if (!selected || unselectable) return;
            selected = false;
            manager.selectedCount--;

            if (!manager.order)
                checkOn.SetActive(false);
            else
            {
                orderBase.SetActive(false);
                manager.RemoveOrder(GetOrder());
            }
        }
        public void UnselectableThis()
        {
            unselectable = true;
            cardFace.color = unselectableColor;
            cardBack.GetComponent<Image>().color = unselectableColor;
            levelIcon.color = unselectableColor;
            textWhite.color = unselectableColor;
        }
        public void SelectableThis()
        {
            if (preselected)
                return;
            unselectable = false;
            cardFace.color = Color.white;
            cardBack.GetComponent<Image>().color = Color.white;
            levelIcon.color = Color.white;
            textWhite.color = Color.white;
        }

        public void PreSelectThis()
        {
            preselected = true;
            SelectThis();
            UnselectableThis();
        }
    }
}
