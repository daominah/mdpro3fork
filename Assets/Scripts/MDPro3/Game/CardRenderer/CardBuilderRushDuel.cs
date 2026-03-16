using MDPro3.Duel.YGOSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static YgomGame.Card.Content;

namespace MDPro3
{
    public class CardBuilderRushDuel : CardBuilder
    {
        [Header("Rush Duel")]
        [SerializeField] private RawImage imageArt;
        [SerializeField] private RawImage imageArtPendulum;
        [SerializeField] private RawImage imageArtPendulumWide;

        [SerializeField] private GameObject legend;
        [SerializeField] private RectTransform moveParts;
        [SerializeField] private GameObject maxAtk;
        [SerializeField] private TextMeshProUGUI numMaxAtk;
        [SerializeField] private GameObject atk;
        [SerializeField] private TextMeshProUGUI numAtk;
        [SerializeField] private GameObject def;
        [SerializeField] private TextMeshProUGUI numDef;
        [SerializeField] private GameObject level;
        [SerializeField] private TextMeshProUGUI numLevel;
        [SerializeField] private GameObject rank;
        [SerializeField] private TextMeshProUGUI numRank;
        [SerializeField] private GameObject link;

        protected override CardRenderer.CardStyle CardStyle => CardRenderer.CardStyle.RUSH_DUEL;
        protected override float[] FontSizeSimplifiedChinese => new float[] { 50f, 27f };
        protected override float[] FontSizeTraditionalChinese => new float[] { 55f, 28f };
        protected override float[] FontSizeKorean => new float[] { 50f, 27f };
        protected override float[] FontSizeJapanese => new float[] { 55f, 29f };
        protected override float[] FontSizeEnglish => new float[] { 63f, 30f };

        protected override void ShowNameOnlyParts()
        {
            base.ShowNameOnlyParts();

            imageArt.gameObject.SetActive(false);
            imageArtPendulum.gameObject.SetActive(false);
            imageArtPendulumWide.gameObject.SetActive(false);
            legend.SetActive(false);
            level.SetActive(false);
            rank.SetActive(false);
            link.SetActive(false);
            numLevel.text = string.Empty;
            numRank.text = string.Empty;

            legend.SetActive(false);
            maxAtk.SetActive(false);
            numMaxAtk.text = string.Empty;
        }

        public override void SetCard(Card data, string language, Texture art, Texture2D overFrame = null)
        {
            base.SetCard(data, language, art, overFrame);
            numAtk.text = data.GetAttackString();
            numDef.text = data.GetDefenseString();
            atk.SetActive(true);
            def.SetActive(true);
            moveParts.gameObject.SetActive(true);
            moveParts.anchoredPosition = Vector2.zero;

            imageAttr.sprite = TextureManager.container.GetCardAttributeIcon(data, true);
            tmpCardType.text = data.GetTypeForRushDuelRender();

            if (data.HasType(CardType.Pendulum))
            {
                moveParts.anchoredPosition = new Vector2(0f, 133f);

                if(art.width == art.height)
                {
                    imageArt.gameObject.SetActive(true);
                    imageArt.texture = art;
                }
                else if (art.width > art.height)
                {
                    imageArtPendulumWide.gameObject.SetActive(true);
                    imageArtPendulumWide.texture = art;
                }
                else
                {
                    imageArtPendulum.gameObject.SetActive(true);
                    imageArtPendulum.texture = art;
                }
                textDescriptionPendulum.text = TextForRender(data.GetPendulumDescription(true), data);

                var authorSplit = GetAuthorFromDescription(data.GetMonsterDescription(true));
                textAuther.text = authorSplit[1];
                textDescription.text = TextForRender(authorSplit[0], data);

                textLScale.text = data.LScale.ToString();
                textRScale.text = data.RScale.ToString();
            }
            else
            {
                imageArt.gameObject.SetActive(true);
                imageArt.texture = art;
                var authorSplit = GetAuthorFromDescription(data.Desc);
                textDescription.text = TextForRender(authorSplit[0], data);
                textAuther.text = authorSplit[1];
            }

            if (data.IsLevelZeroMonster())
                data.Level = 0;
            if (data.HasType(CardType.Link))
            {
                tmpCardName.color = Color.white;
                def.SetActive(false);
                numDef.text = string.Empty;
                numLevel.text = data.GetLinkCount().ToString();

                link.SetActive(true);
                for (int i = 0; i < 8; i++)
                {
                    int bitIndex = i < 4 ? i : i + 1;
                    link.transform.GetChild(i).gameObject.SetActive((data.LinkMarker & (1 << bitIndex)) != 0);
                }
            }
            else if(data.HasType(CardType.Xyz))
            {
                tmpCardName.color = Color.white;
                if (!data.HasType(CardType.Pendulum))
                    tmpCardType.color = Color.white;
                rank.SetActive(true);
                numRank.text = data.Level.ToString();
            }
            else if (data.HasType(CardType.Monster))
            {
                level.SetActive(true);
                numLevel.text = data.Level.ToString();
            }
            else if(data.HasAnyType(CardType.Spell, CardType.Trap))
            {
                atk.SetActive(false);
                def.SetActive(false);
                numAtk.text = string.Empty;
                numDef.text = string.Empty;
            }

            legend.SetActive(data.HasType(CardType.Legend));

            if(data.HasType(CardType.Maximum))
            {
                maxAtk.SetActive(true);
                numMaxAtk.text = data.GetMaximumAttackString();
            }
        }

        public override RawImage GetArtPartForVideo(bool isPendulum)
        {
            return isPendulum ? imageArtPendulum : imageArt;
        }

        public override void SetAllArtPartsOff()
        {
            imageArt.gameObject.SetActive(false);
            imageArtPendulum.gameObject.SetActive(false);
            imageArtPendulumWide.gameObject.SetActive(false);
        }

    }
}