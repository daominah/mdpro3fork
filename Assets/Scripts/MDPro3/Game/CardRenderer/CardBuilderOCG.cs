using MDPro3.Duel.YGOSharp;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3
{
    public class CardBuilderOCG : CardBuilder
    {
        [Header("OCG")]

        [SerializeField] private RawImage imageArt;
        [SerializeField] private RawImage imageArtPendulum;
        [SerializeField] private RawImage imageArtPendulumSquare;
        [SerializeField] private RawImage imageArtPendulumWide;

        [SerializeField] private GameObject levels;
        [SerializeField] private GameObject ranks;
        [SerializeField] private GameObject ranks13;
        [SerializeField] private GameObject levelMasks;
        [SerializeField] private GameObject rankMasks;
        [SerializeField] private GameObject rank13Masks;
        [SerializeField] private GameObject linkMarkers;
        [SerializeField] private GameObject line;
        [SerializeField] private GameObject atk;
        [SerializeField] private GameObject def;
        [SerializeField] private Text numAtk;
        [SerializeField] private Text numDef;
        [SerializeField] private Image imageLinkCount;

        private RectTransform descriptionRT;
        private RectTransform DescriptionRT => 
            descriptionRT = descriptionRT != null ? descriptionRT : textDescription.GetComponent<RectTransform>();

        protected override void ShowNameOnlyParts()
        {
            base.ShowNameOnlyParts();

            imageArt.gameObject.SetActive(false);
            imageArtPendulum.gameObject.SetActive(false);
            imageArtPendulumSquare.gameObject.SetActive(false);
            imageArtPendulumWide.gameObject.SetActive(false);
            levels.SetActive(false);
            ranks.SetActive(false);
            ranks13.SetActive(false);
            levelMasks.SetActive(false);
            rankMasks.SetActive(false);
            rank13Masks.SetActive(false);
            linkMarkers.SetActive(false);
            tmpCardType.text = string.Empty;

            line.SetActive(false);
            atk.SetActive(false);
            def.SetActive(false);
            numAtk.text = string.Empty;
            numDef.text = string.Empty;
            imageLinkCount.gameObject.SetActive(false);
            tmpCardType.text = string.Empty;
        }

        public override void SetCardName(Card data, string language)
        {
            base.SetCardName(data, language);

            if (data.IsLevelZeroMonster())
                data.Level = 0;
            if (data.HasType(CardType.Xyz))
            {
                if (data.Level == 13)
                    rank13Masks.SetActive(true);
                else
                {
                    rankMasks.SetActive(true);
                    for (int i = 0; i < 12; i++)
                        rankMasks.transform.GetChild(i).gameObject.SetActive(i < data.Level);
                }
            }
            else if (data.HasType(CardType.Monster)
                && !data.HasType(CardType.Link))
            {
                levelMasks.SetActive(true);
                for (int i = 0; i < 12; i++)
                    levelMasks.transform.GetChild(i).gameObject.SetActive(i < data.Level);
            }
        }

        public override void SetCard(Card data, string language, Texture art, Texture2D overFrame = null)
        {
            base.SetCard(data, language, art, overFrame);

            levelMasks.SetActive(false);
            rankMasks.SetActive(false);
            rank13Masks.SetActive(false);

            line.SetActive(true);
            atk.SetActive(true);
            def.SetActive(true);
            numAtk.text = data.GetAttackString();
            numDef.text = data.GetDefenseString();
            DescriptionRT.sizeDelta = new Vector2(590f, 160f);
            imageAttr.sprite = TextureManager.container.GetCardAttributeIcon(data, true);

            if (data.HasType(CardType.Pendulum))
            {
                if(art.width == art.height)
                {
                    imageArtPendulumSquare.gameObject.SetActive(true);
                    imageArtPendulumSquare.texture = art;
                }
                else if(art.width > art.height)
                {
                    imageArtPendulumWide.gameObject.SetActive(true);
                    imageArtPendulumWide.texture = art;
                }
                else
                {
                    imageArtPendulum.gameObject.SetActive(true);
                    imageArtPendulum.texture = art;
                }

                var pendulumDesc = data.GetDescriptionSplit(true);
                textDescription.text = data.GetTypeForRushDuelRender();
                textDescriptionPendulum.text = TextForRender(pendulumDesc[0], data);

                var authorSplit = GetAuthorFromDescription(pendulumDesc[1]);
                textDescription.text += Program.STRING_LINE_BREAK + TextForRender(authorSplit[0], data);
                textAuther.text = authorSplit[1];

                textLScale.text = data.LScale.ToString();
                textRScale.text = data.RScale.ToString();
            }
            else
            {
                imageArt.gameObject.SetActive(true);
                imageArt.texture = art;
                var desc = string.Empty;
                if (data.HasType(CardType.Monster))
                    desc = data.GetTypeForRushDuelRender() + Program.STRING_LINE_BREAK;

                var authorSplit = GetAuthorFromDescription(data.Desc);
                desc += TextForRender(authorSplit[0], data);
                textDescription.text = desc;
                textAuther.text = authorSplit[1];
            }

            if (data.IsLevelZeroMonster())
                data.Level = 0;
            if (data.HasType(CardType.Link))
            {
                tmpCardName.color = Color.white;
                linkMarkers.SetActive(true);
                def.SetActive(false);
                numDef.text = string.Empty;
                imageLinkCount.gameObject.SetActive(true);
                imageLinkCount.sprite = TextureManager.container.GetOcgLinkCount(data.GetLinkCount());
                for (int i = 0; i < 8; i++)
                {
                    int bitIndex = i < 4 ? i : i + 1;
                    linkMarkers.transform.GetChild(i).gameObject.SetActive((data.LinkMarker & (1 << bitIndex)) != 0);
                }
            }
            else if (data.HasType(CardType.Xyz))
            {
                tmpCardName.color = Color.white;
                if (!data.HasType(CardType.Xyz))
                {
                    textPassword.color = Color.white;
                    textAuther.color = Color.white;
                }

                if(data.Level == 13)
                    ranks13.SetActive(true);
                else
                {
                    ranks.SetActive(true);
                    for (int i = 0; i < 12; i++)
                        ranks.transform.GetChild(i).gameObject.SetActive(i < data.Level);
                }
            }
            else if (data.HasType(CardType.Monster))
            {
                levels.SetActive(true);
                for (int i = 0; i < 12; i++)
                    levels.transform.GetChild(i).gameObject.SetActive(i < data.Level);
            }
            else if (data.HasAnyType(CardType.Spell, CardType.Trap))
            {
                descriptionRT.sizeDelta = new Vector2(590, 185);
                tmpCardName.color = Color.white;
                line.SetActive(false);
                atk.SetActive(false);
                def.SetActive(false);
                numAtk.text = string.Empty;
                numDef.text = string.Empty;
                tmpCardType.text = data.GetSpellTypeForOCGRender();
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
            imageArtPendulumSquare.gameObject.SetActive(false);
            imageArtPendulumWide.gameObject.SetActive(false);
        }
    }
}