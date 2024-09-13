using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MDPro3
{
    public class CardRenderer : MonoBehaviour
    {
        public GameObject ocg;
        public GameObject rd;

        public enum CardStyle
        {
            OCG_TCG,
            RUSH_DUEL
        }
        public const string bigSlash = "£¯";
        public const string smallSlash = " / ";
        static readonly float cardNameLabelWidthOCG = 520f;
        static readonly float cardNameLabelWidthRushDuel = 520f;

        #region Public Reference
        [Header("OCG")]
        public RawImage cardArt;
        public RawImage cardArtPendulum;
        public RawImage cardArtPendulumSquare;
        public RawImage cardArtPendulumWidth;
        public Image cardFrame;
        public Image cardAttribute;
        public TextMeshProUGUI cardName;
        public Text cardDescription;
        public Text cardDescriptionPendulum;
        public Text lScale;
        public Text rScale;
        public GameObject levels;
        public GameObject ranks;
        public GameObject rank13;
        public GameObject levelsMask;
        public GameObject ranksMask;
        public GameObject rank13Mask;
        public GameObject linkMarkers;
        public GameObject line;
        public GameObject textATK;
        public GameObject textDEF;
        public Text numATK;
        public Text numDEF;
        public Image linkCount;
        public TextMeshProUGUI spellType;
        public Font atkDef;
        public RenderTexture renderTexture;

        [Header("RD")]
        public RawImage cardArtRD;
        public RawImage cardArtPendulumRD;
        public RawImage cardArtPendulumWidthRD;
        public Image cardFrameRD;
        public Image cardAttributeRD;
        public GameObject cardLegendRD;
        public RectTransform movePartsRD;
        public TextMeshProUGUI cardNameRD;
        public TextMeshProUGUI cardTypeRD;
        public Text cardDescriptionRD;
        public Text cardDescriptionPendulumRD;
        public Text lScaleRD;
        public Text rScaleRD;
        public GameObject maxAtkRD;
        public TextMeshProUGUI maxAtkNumRD;
        public GameObject atkRD;
        public TextMeshProUGUI atkNumRD;
        public GameObject defRD;
        public TextMeshProUGUI defNumRD;
        public GameObject levelRD;
        public TextMeshProUGUI levelNumRD;
        public GameObject rankRD;
        public TextMeshProUGUI rankNumRD;
        public GameObject linkRD;
        public GameObject linkUL;
        public GameObject linkU;
        public GameObject linkUR;
        public GameObject linkR;
        public GameObject linkBR;
        public GameObject linkB;
        public GameObject linkBL;
        public GameObject linkL;

        #endregion

        public void SwitchLanguage()
        {
            var language = Language.GetCardConfig();
            if (language == Language.SimplifiedChinese)
            {
                cardName.fontSize = 50f;
                cardNameRD.fontSize = 50f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSize = 27f;
                SetFont("RenderFontChineseSimplified");
            }
            else if (language == Language.TraditionalChinese)
            {
                cardName.fontSize = 55f;
                cardNameRD.fontSize = 55f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSize = 28f;
                SetFont("RenderFontChineseTraditional");
            }
            else if (language == Language.Korean)
            {
                cardName.fontSize = 50f;
                cardNameRD.fontSize = 50f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSize = 27f;
                SetFont("RenderFontKorean");
            }
            else if (language == Language.Japanese)
            {
                cardName.fontSize = 55f;
                cardNameRD.fontSize = 55f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSize = 29f;
                SetFont("RenderFontJapanese");
            }
            else
            {
                cardName.fontSize = 63f;
                cardNameRD.fontSize = 63f;
                spellType.fontSize = 43f;
                cardTypeRD.fontSize = 30f;
                SetFont("RenderFontEnglish");
            }
        }

        private void SetFont(string fontName)
        {
            var handle = Addressables.LoadAssetAsync<TMP_FontAsset>(fontName);
            handle.Completed += (result) =>
            {
                cardName.font = result.Result;
                cardNameRD.font = result.Result;
                spellType.font = result.Result;
                cardTypeRD.font = result.Result;
            };
            var handle2 = Addressables.LoadAssetAsync<Font>(fontName);
            handle2.Completed += (result) =>
            {
                cardDescription.font = result.Result;
                cardDescriptionRD.font = result.Result;
                cardDescriptionPendulum.font = result.Result;
                cardDescriptionPendulumRD.font = result.Result;
            };
        }

        public static bool NeedRushDuelStyle(int code)
        {
            var config = Config.Get("CardStyle", CardStyle.OCG_TCG.ToString());
            if (config == CardStyle.RUSH_DUEL.ToString())
                return true;
            if(code >= 120000000 && code < 130000000)
                return true;
            return false;
        }

        public void RenderName(int code)
        {
            if (NeedRushDuelStyle(code))
                RenderRushDuelName(code);
            else
                RenderOcgName(code);
        }

        private void RenderRushDuelName(int code)
        {
            ocg.SetActive(false);
            rd.SetActive(true);

            var data = CardsManager.GetRenderCard(code);
            if (data.Id == 0)
                return;
            cardNameRD.GetComponent<RectTransform>().localScale = Vector3.one;

            cardNameRD.text = data.Name;
            cardNameRD.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardNameRD.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthRushDuel)
                cardNameRD.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthRushDuel / nameWidth, 1f, 1f);

            cardNameRD.color = Color.white;

            cardArtRD.gameObject.SetActive(false);
            cardArtPendulumRD.gameObject.SetActive(false);
            cardArtPendulumWidthRD.gameObject.SetActive(false);
            cardFrameRD.gameObject.SetActive(false);
            cardAttributeRD.gameObject.SetActive(false);
            cardLegendRD.SetActive(false);

            Program.I().camera_.cameraRenderTexture.Render();
        }

        private void RenderOcgName(int code)
        {
            ocg.SetActive(true);
            rd.SetActive(false);

            var data = CardsManager.GetRenderCard(code);
            if (data.Id == 0)
                return;
            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardName.text = data.Name;
            cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthOCG)
                cardName.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthOCG / nameWidth, 1, 1);

            cardName.color = Color.white;

            cardFrame.gameObject.SetActive(false);
            cardArt.gameObject.SetActive(false);
            cardArtPendulum.gameObject.SetActive(false);
            cardArtPendulumSquare.gameObject.SetActive(false);
            cardArtPendulumWidth.gameObject.SetActive(false);
            levels.SetActive(false);
            ranks.SetActive(false);
            rank13.SetActive(false);
            cardAttribute.gameObject.SetActive(false);
            levelsMask.SetActive(false);
            ranksMask.SetActive(false);
            rank13Mask.SetActive(false);
            linkMarkers.SetActive(false);
            spellType.text = string.Empty;
            data = AdjustLevelForRender(data);
            if ((data.Type & (uint)CardType.Xyz) > 0)
            {
                if (data.Level == 13)
                    rank13Mask.SetActive(true);
                else
                {
                    ranksMask.SetActive(true);
                    for (int i = 0; i < 12; i++)
                    {
                        if (i < data.Level)
                            ranksMask.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            ranksMask.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

            }
            else if ((data.Type & (uint)CardType.Monster) > 0
                && (data.Type & (uint)CardType.Link) == 0)
            {
                levelsMask.SetActive(true);
                for (int i = 0; i < 12; i++)
                {
                    if (i < data.Level)
                        levelsMask.transform.GetChild(i).gameObject.SetActive(true);
                    else
                        levelsMask.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            Program.I().camera_.cameraRenderTexture.Render();
        }

        public bool RenderCard(int code, Texture2D art)
        {
            if(NeedRushDuelStyle(code))
                return RenderRushDuelCard(code, art);
            else
                return RenderOcgCard(code, art);
        }

        private bool RenderRushDuelCard(int code, Texture2D art)
        {
            ocg.SetActive(false);
            rd.SetActive(true);

            RenderTexture.active = renderTexture;

            Card data = CardsManager.GetRenderCard(code);
            if (data == null || data.Id == 0)
                return false;

            cardNameRD.GetComponent<RectTransform>().localScale = Vector3.one;
            cardNameRD.text = data.Name;
            cardNameRD.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardNameRD.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthRushDuel)
                cardNameRD.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthRushDuel / nameWidth, 1, 1);

            cardNameRD.color = Color.black;
            cardTypeRD.color = Color.black;

            cardArtRD.gameObject.SetActive(false);
            cardArtPendulumRD.gameObject.SetActive(false);
            cardArtPendulumWidthRD.gameObject.SetActive(false);

            cardFrameRD.gameObject.SetActive(true);
            cardAttributeRD.gameObject.SetActive(true);
            cardDescriptionPendulumRD.text = string.Empty;
            lScaleRD.text = string.Empty;
            rScaleRD.text = string.Empty;
            levelRD.SetActive(false);
            rankRD.SetActive(false);
            linkRD.SetActive(false);
            levelNumRD.gameObject.SetActive(false);
            rankNumRD.gameObject.SetActive(false);
            atkNumRD.text = data.Attack == -2 ? "?" : data.Attack.ToString();
            defNumRD.text = data.Defense == -2 ? "?" : data.Defense.ToString();
            atkRD.SetActive(true);
            defRD.SetActive(true);
            movePartsRD.gameObject.SetActive(true);
            movePartsRD.anchoredPosition = Vector2.zero;

            cardAttributeRD.sprite = CardDescription.GetCardAttribute(data, true).sprite;
            cardTypeRD.text = StringHelper.GetType(data, true, true);
            if (Language.CardUseLatin())
                cardTypeRD.text = cardTypeRD.text.Replace(Program.slash, smallSlash);
            else
                cardTypeRD.text = cardTypeRD.text.Replace(Program.slash, bigSlash);

            if ((data.Type & (uint)CardType.Pendulum) > 0)
            {
                movePartsRD.anchoredPosition = new Vector2(0f, 133f);

                if (art.width == art.height)
                {
                    cardArtRD.gameObject.SetActive(true);
                    cardArtRD.texture = art;
                }
                else if (art.width > art.height)
                {
                    cardArtPendulumWidthRD.gameObject.SetActive(true);
                    cardArtPendulumWidthRD.texture = art;
                }
                else
                {
                    cardArtPendulumRD.gameObject.SetActive(true);
                    cardArtPendulumRD.texture = art;
                }
                var pendulumDescription = CardDescription.GetCardDescriptionSplit(data.Desc, true);
                cardDescriptionRD.text = TextForRender(pendulumDescription[1]);
                cardDescriptionPendulumRD.text = TextForRender(pendulumDescription[0]);
                lScaleRD.text = data.LScale.ToString();
                rScaleRD.text = data.RScale.ToString();
                if ((data.Type & (uint)CardType.Xyz) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumXyz;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumSynchro;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumFusion;
                else if ((data.Type & (uint)CardType.Ritual) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumRitual;
                else if ((data.Type & (uint)CardType.Link) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumLink;
                else if ((data.Type & (uint)CardType.Normal) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumNormal;
                else
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumEffect;
            }
            else
            {
                cardArtRD.gameObject.SetActive(true);
                cardArtRD.texture = art;
                cardDescriptionRD.text = TextForRender(data.Desc);
                cardDescriptionPendulumRD.text = string.Empty;

                if (code == 10000000)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Obelisk;
                else if (code == 10000010)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Ra;
                else if (code == 10000020)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Slifer;
                else if ((data.Type & (uint)CardType.Link) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Link;
                else if ((data.Type & (uint)CardType.Xyz) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Xyz;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Synchro;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Fusion;
                else if ((data.Type & (uint)CardType.Ritual) > 0 && (data.Type & (uint)CardType.Monster) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Ritual;
                else if ((data.Type & (uint)CardType.Token) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Token;
                else if ((data.Type & (uint)CardType.Normal) > 0)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Normal;
                else if ((data.Type & ((uint)CardType.Spell) + (uint)CardType.Trap) > 0)
                {
                    atkRD.SetActive(false);
                    defRD.SetActive(false);
                    atkNumRD.text = string.Empty;
                    defNumRD.text = string.Empty;

                    if ((data.Type & (uint)CardType.Spell) > 0)
                        cardFrameRD.sprite = TextureManager.container.rd_Frame_Spell;
                    else
                        cardFrameRD.sprite = TextureManager.container.rd_Frame_Trap;
                }
                else
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Effect;
            }

            data = AdjustLevelForRender(data);

            if ((data.Type & (uint)CardType.Link) > 0)
            {
                cardNameRD.color = Color.white;
                //cardTypeRD.color = Color.white;
                defRD.SetActive(false);
                defNumRD.text = string.Empty;
                levelNumRD.gameObject.SetActive(true);
                levelNumRD.text = CardDescription.GetCardLinkCount(data).ToString();

                linkRD.SetActive(true);
                for (int i = 0; i < 8; i++)
                {
                    if (i < 4)
                    {
                        if ((data.LinkMarker & (1 << i)) > 0)
                            linkRD.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            linkRD.transform.GetChild(i).gameObject.SetActive(false);
                    }
                    else
                    {
                        if ((data.LinkMarker & (1 << (i + 1))) > 0)
                            linkRD.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            linkRD.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            else if ((data.Type & (uint)CardType.Xyz) > 0)
            {
                cardNameRD.color = Color.white;
                if((data.Type & (uint)CardType.Pendulum) == 0)
                    cardTypeRD.color = Color.white;

                rankRD.SetActive(true);
                rankNumRD.gameObject.SetActive(true);
                rankNumRD.text = data.Level.ToString();
            }
            else if ((data.Type & (uint)CardType.Monster) > 0)
            {
                levelRD.SetActive(true);
                levelNumRD.gameObject.SetActive(true);
                levelNumRD.text = data.Level.ToString();
            }

            Program.I().camera_.cameraRenderTexture.Render();
            return true;
        }

        private bool RenderOcgCard(int code, Texture2D art)
        {
            ocg.SetActive(true);
            rd.SetActive(false);

            RenderTexture.active = renderTexture;

            Card data = CardsManager.GetRenderCard(code);
            if (data == null || data.Id == 0)
                return false;

            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardName.text = data.Name;
            cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthOCG)
                cardName.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthOCG / nameWidth, 1, 1);

            cardName.color = Color.black;

            cardArt.gameObject.SetActive(false);
            cardArtPendulum.gameObject.SetActive(false);
            cardArtPendulumSquare.gameObject.SetActive(false);
            cardArtPendulumWidth.gameObject.SetActive(false);

            cardFrame.gameObject.SetActive(true);
            cardAttribute.gameObject.SetActive(true);
            cardDescriptionPendulum.text = "";
            lScale.text = "";
            rScale.text = "";
            levels.SetActive(false);
            ranks.SetActive(false);
            rank13.SetActive(false);
            levelsMask.SetActive(false);
            ranksMask.SetActive(false);
            rank13Mask.SetActive(false);
            linkMarkers.SetActive(false);
            line.SetActive(true);
            textATK.SetActive(true);
            textDEF.SetActive(true);
            numATK.text = data.Attack == -2 ? "?" : data.Attack.ToString();
            numDEF.text = data.Defense == -2 ? "?" : data.Defense.ToString();
            linkCount.gameObject.SetActive(false);
            spellType.text = "";
            cardDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(590f, 160f);
            cardAttribute.sprite = CardDescription.GetCardAttribute(data, true).sprite;

            if ((data.Type & (uint)CardType.Pendulum) > 0)
            {
                if (art.width == art.height)
                {
                    cardArtPendulumSquare.gameObject.SetActive(true);
                    cardArtPendulumSquare.texture = art;
                }
                else if (art.width > art.height)
                {
                    cardArtPendulumWidth.gameObject.SetActive(true);
                    cardArtPendulumWidth.texture = art;
                }
                else
                {
                    cardArtPendulum.gameObject.SetActive(true);
                    cardArtPendulum.texture = art;
                }
                var pendulumDescription = CardDescription.GetCardDescriptionSplit(data.Desc, true);
                cardDescription.text = StringHelper.GetType(data, true).Replace(Program.slash, bigSlash) + "\r\n" + TextForRender(pendulumDescription[1]);
                cardDescriptionPendulum.text = TextForRender(pendulumDescription[0]);
                lScale.text = data.LScale.ToString();
                rScale.text = data.RScale.ToString();
                if ((data.Type & (uint)CardType.Xyz) > 0)
                    cardFrame.sprite = TextureManager.container.cardFramePendulumXyzOF;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    cardFrame.sprite = TextureManager.container.cardFramePendulumSynchroOF;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    cardFrame.sprite = TextureManager.container.cardFramePendulumFusionOF;
                else if ((data.Type & (uint)CardType.Ritual) > 0)
                    cardFrame.sprite = TextureManager.container.cardFramePendulumRitualOF;
                else if ((data.Type & (uint)CardType.Normal) > 0)
                    cardFrame.sprite = TextureManager.container.cardFramePendulumNormalOF;
                else
                    cardFrame.sprite = TextureManager.container.cardFramePendulumEffectOF;
            }
            else
            {
                cardArt.gameObject.SetActive(true);
                cardArt.texture = art;
                var description = "";
                if ((data.Type & (uint)CardType.Monster) > 0)
                    description = StringHelper.GetType(data, true).Replace(Program.slash, bigSlash) + "\r\n";
                description += TextForRender(data.Desc);
                cardDescription.text = description;

                if (code == 10000000)
                    cardFrame.sprite = TextureManager.container.cardFrameObeliskOF;
                else if (code == 10000010)
                    cardFrame.sprite = TextureManager.container.cardFrameRaOF;
                else if (code == 10000020)
                    cardFrame.sprite = TextureManager.container.cardFrameOsirisOF;
                else if ((data.Type & (uint)CardType.Link) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameLinkOF;
                else if ((data.Type & (uint)CardType.Xyz) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameXyzOF;
                else if ((data.Type & (uint)CardType.Synchro) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameSynchroOF;
                else if ((data.Type & (uint)CardType.Fusion) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameFusionOF;
                else if ((data.Type & (uint)CardType.Ritual) > 0 && (data.Type & (uint)CardType.Monster) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameRitualOF;
                else if ((data.Type & (uint)CardType.Token) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameTokenOF;
                else if ((data.Type & (uint)CardType.Normal) > 0)
                    cardFrame.sprite = TextureManager.container.cardFrameNormalOF;
                else if ((data.Type & ((uint)CardType.Spell) + (uint)CardType.Trap) > 0)
                {
                    cardDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(590, 185);
                    cardName.color = Color.white;
                    line.SetActive(false);
                    textATK.SetActive(false);
                    textDEF.SetActive(false);
                    numATK.text = "";
                    numDEF.text = "";
                    spellType.text = StringHelper.GetType(data, true, false);

                    if ((data.Type & (uint)CardType.Spell) > 0)
                        cardFrame.sprite = TextureManager.container.cardFrameSpellOF;
                    else
                        cardFrame.sprite = TextureManager.container.cardFrameTrapOF;
                }
                else
                    cardFrame.sprite = TextureManager.container.cardFrameEffectOF;
            }

            data = AdjustLevelForRender(data);

            if ((data.Type & (uint)CardType.Link) > 0)
            {
                cardName.color = Color.white;
                linkMarkers.SetActive(true);
                textDEF.SetActive(false);
                numDEF.text = "";
                linkCount.gameObject.SetActive(true);
                switch (CardDescription.GetCardLinkCount(data))
                {
                    case 1:
                        linkCount.sprite = TextureManager.container.link1R;
                        break;
                    case 2:
                        linkCount.sprite = TextureManager.container.link2R;
                        break;
                    case 3:
                        linkCount.sprite = TextureManager.container.link3R;
                        break;
                    case 4:
                        linkCount.sprite = TextureManager.container.link4R;
                        break;
                    case 5:
                        linkCount.sprite = TextureManager.container.link5R;
                        break;
                    case 6:
                        linkCount.sprite = TextureManager.container.link6R;
                        break;
                    case 7:
                        linkCount.sprite = TextureManager.container.link7R;
                        break;
                    case 8:
                        linkCount.sprite = TextureManager.container.link8R;
                        break;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i < 4)
                    {
                        if ((data.LinkMarker & (1 << i)) > 0)
                            linkMarkers.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            linkMarkers.transform.GetChild(i).gameObject.SetActive(false);
                    }
                    else
                    {
                        if ((data.LinkMarker & (1 << (i + 1))) > 0)
                            linkMarkers.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            linkMarkers.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            else if ((data.Type & (uint)CardType.Xyz) > 0)
            {
                cardName.color = Color.white;
                if (data.Level == 13)
                    rank13.SetActive(true);
                else
                {
                    ranks.SetActive(true);
                    for (int i = 0; i < 12; i++)
                    {
                        if (i < data.Level)
                            ranks.transform.GetChild(i).gameObject.SetActive(true);
                        else
                            ranks.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            else if ((data.Type & (uint)CardType.Monster) > 0)
            {
                levels.SetActive(true);
                for (int i = 0; i < 12; i++)
                {
                    if (i < data.Level)
                        levels.transform.GetChild(i).gameObject.SetActive(true);
                    else
                        levels.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            Program.I().camera_.cameraRenderTexture.Render();

            return true;
        }

        static Card AdjustLevelForRender(Card data)
        {
            int code = data.Id;
            if (code == 1686814)
                data.Level = 0;
            else if (code == 90884403)
                data.Level = 0;
            else if (code == 26973555)
                data.Level = 0;
            else if (code == 43490025)
                data.Level = 0;
            else if (code == 65305468)
                data.Level = 0;
            else if (code == 52653092)
                data.Level = 0;
            return data;
        }

        string TextForRender(string description)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;
            var language = Language.GetCardConfig();

            if (language == Language.Japanese)
            {
                description = description.Replace("\t\r\n", "\f\f\f");
                description = description.Replace("\r\n¡ñ", "¡ñ¡ñ¡ñ");
                description = description.Replace("\r", string.Empty);
                description = description.Replace("\n", string.Empty);
                description = description.Replace("\f\f\f", "\r\n");
                description = description.Replace("¡ñ¡ñ¡ñ", "\r\n¡ñ");
            }
            else
            {
                description = description
                    .Replace("\r\n¢Ú", "¢Ú")
                    .Replace("\r\n¢Û", "¢Û")
                    .Replace("\r\n¢Ü", "¢Ü")
                    .Replace("\r\n¢Ý", "¢Ý")
                    .Replace("\r\n¢Þ", "¢Þ")
                    .Replace("\r\n¢ß", "¢ß")
                    .Replace("\r\n¢à", "¢à")
                    .Replace("\r\n¢á", "¢á");
            }

            if (!Language.CardUseLatin())
                description = description.Replace(Program.slash, bigSlash);
            else
                description = description.Replace(Program.slash, smallSlash);

            if (!Language.CardUseLatin())
                description = description.Replace(" ", "\u00A0");
            description = description.Replace("\r\n\r\n", "\r\n");
            return description;
        }


    }
}
