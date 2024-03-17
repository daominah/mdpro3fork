using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3.UI
{
    public class CardListItem : MonoBehaviour
    {
        public RawImage face;
        public GameObject cardBack;
        public Image levelIcon;
        public Text textWhite;
        public Text textBlack;
        public GameObject chain;
        public Text chainText;
        public GameObject target;
        public Button button;

        static Color myColor = Color.cyan;
        static Color opColor = Color.red;

        public GameCard card;
        void Start()
        {
            StartCoroutine(RefreshFace());
            cardBack.SetActive((card.p.position & (uint)CardPosition.FaceUp) == 0);
            if (card.GetData().Id != 0)
            {
                if ((card.GetData().Type & (uint)CardType.Monster) > 0)
                {
                    levelIcon.sprite = TextureManager.GetCardLevelIcon(card.GetData());
                    if ((card.GetData().Type & (uint)CardType.Link) > 0)
                        textWhite.text = CardDescription.GetCardLinkCount(card.GetData()).ToString();
                    else
                        textWhite.text = card.GetData().Level.ToString();
                    textBlack.text = textWhite.text;
                }
                else
                {
                    levelIcon.sprite = TextureManager.container.typeNone;
                    textWhite.text = "";
                    textBlack.text = "";
                }
                if (card.chains.Count > 0)
                {
                    chain.SetActive(true);
                    chainText.text = card.chains[0].i.ToString();
                    if (card.p.controller == 0)
                        chainText.color = Color.cyan;
                    else
                        chainText.color = Color.red;
                    target.SetActive(false);
                }
                else
                {
                    chain.SetActive(false);
                    if (Program.I().ocgcore.cardsBeTarget.Contains(card))
                        target.SetActive(true);
                    else
                        target.SetActive(false);
                }
            }
            else
            {
                levelIcon.gameObject.SetActive(false);
                textWhite.text = "";
                textBlack.text = "";
                chain.SetActive(false);
                cardBack.SetActive(false);
            }
            button.onClick.AddListener(OnClick);
        }

        IEnumerator RefreshFace()
        {
            face.texture = TextureManager.container.unknownCard.texture;
            var code = card.GetData().Id;
            if (code != 0)
            {
                IEnumerator ie = Program.I().texture_.LoadCardAsync(code);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                var mat = TextureManager.GetCardMaterial(code);
                face.material = mat;
                face.material.mainTexture = ie.Current as Texture2D;
                face.texture = ie.Current as Texture2D;
            }
            else
            {
                face.texture = null;
                switch (Program.I().ocgcore.condition)
                {
                    case OcgCore.Condition.Duel:
                        if (card.p.controller == 0)
                            face.material = Appearance.duelProtector0;
                        else
                            face.material = Appearance.duelProtector1;
                        break;
                    case OcgCore.Condition.Watch:
                        if (card.p.controller == 0)
                            face.material = Appearance.watchProtector0;
                        else
                            face.material = Appearance.watchProtector1;
                        break;
                    case OcgCore.Condition.Replay:
                        if (card.p.controller == 0)
                            face.material = Appearance.replayProtector0;
                        else
                            face.material = Appearance.replayProtector1;
                        break;
                }
            }
        }

        void OnClick()
        {
            Program.I().ocgcore.description.Show(card, face.material);
        }
    }
}
