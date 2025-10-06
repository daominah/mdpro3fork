using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.Duel.YGOSharp;
using TMPro;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;
using MDPro3.Utility;
using Cysharp.Threading.Tasks;

namespace MDPro3.UI
{
    public class CardListItem : MonoBehaviour
    {
        public RawImage face;
        public GameObject cardBack;
        public Image levelIcon;
        public TextMeshProUGUI textLevel;
        public GameObject chain;
        public Text chainText;
        public GameObject target;
        public Button button;

        static Color myColor = Color.cyan;
        static Color opColor = Color.red;

        public GameCard card;
        void Start()
        {
            _ = RefreshFace();
            cardBack.SetActive((card.p.position & (uint)CardPosition.FaceUp) == 0);
            if (card.GetData().Id != 0)
            {
                if (card.GetData().HasType(CardType.Monster))
                {
                    levelIcon.sprite = TextureManager.GetCardLevelIcon(card.GetData());
                    if (card.GetData().HasType(CardType.Link))
                        textLevel.text = card.GetData().GetLinkCount().ToString();
                    else
                        textLevel.text = card.GetData().Level.ToString();
                }
                else
                {
                    levelIcon.sprite = TextureManager.container.typeNone;
                    textLevel.text = string.Empty;
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
                    if (OcgCore.cardsBeTarget.Contains(card))
                        target.SetActive(true);
                    else
                        target.SetActive(false);
                }
            }
            else
            {
                levelIcon.gameObject.SetActive(false);
                textLevel.text = "";
                chain.SetActive(false);
                cardBack.SetActive(false);
            }
            button.onClick.AddListener(OnClick);
        }

        private async UniTask RefreshFace()
        {
            face.texture = TextureManager.container.unknownCard.texture;
            var code = card.GetData().Id;
            if (code != 0)
            {
                face.texture = await CardImageLoader.LoadCardAsync(code, false);
                face.material = MaterialLoader.GetCardMaterial(code);
                face.material.mainTexture = face.texture;
            }
            else
            {
                face.texture = null;
                switch (OcgCore.condition)
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
            Program.instance.ocgcore.GetUI<OcgCoreUI>().CardDescription.Show(card, face.material);
        }
    }
}
