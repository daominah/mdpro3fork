using Cysharp.Threading.Tasks;
using MDPro3.Duel.YGOSharp;
using MDPro3.Utility;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MDPro3
{
    public class CardRenderer : MonoBehaviour
    {

        public enum CardStyle
        {
            OCG_TCG,
            RUSH_DUEL
        }

        public const string BIG_SLASH = "／";
        public const string SMALL_SLASH = " / ";
        private static readonly float cardNameLabelWidthOCG = 520f;
        private static readonly float cardNameLabelWidthRushDuel = 520f;
        private string currentFontLanguage;
        private static bool fontsLoaded;
        private static int prefabIndex = 0;

        #region Reference

        [Header("CardRenderer")]
        [SerializeField] private GameObject ocg;
        [SerializeField] private GameObject rd;
        [SerializeField] private Camera renderCamera;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage renderedCardFrame;
        public RenderTexture renderTexture;

        [Header("OCG")]
        public RawImage cardArt;
        public RawImage cardArtPendulum;
        public RawImage cardArtPendulumSquare;
        public RawImage cardArtPendulumWidth;
        public Image cardFrame;
        public Image attrIcon;
        public TextMeshProUGUI attrRuby;
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
        public Text cardPassword;
        public Text cardAuther;

        [Header("RD")]
        public RawImage cardArtRD;
        public RawImage cardArtPendulumRD;
        public RawImage cardArtPendulumWidthRD;
        public Image cardFrameRD;
        public Image attrIconRD;
        public TextMeshProUGUI attrRubyRD;
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
        public Text cardPasswordRD;
        public Text cardAutherRD;

        #endregion

        #region Fonts

        private static Font fontChineseSimplified;
        private static Font fontChineseTraditional;
        private static Font fontKorean;
        private static Font fontJapanese;
        private static Font fontEnglish;

        private static TMP_FontAsset tmpFontChineseSimplified;
        private static TMP_FontAsset tmpFontChineseTraditional;
        private static TMP_FontAsset tmpFontKorean;
        private static TMP_FontAsset tmpFontJapanese;
        private static TMP_FontAsset tmpFontEnglish;

        private static async UniTask LoadFontsAsync()
        {
            if (fontsLoaded)
                return;

            //if(fontChineseSimplified != null)
                fontChineseSimplified = await Addressables.LoadAssetAsync<Font>("RenderFontChineseSimplified").ToUniTask();
            //if(tmpFontChineseSimplified != null)
                tmpFontChineseSimplified = await Addressables.LoadAssetAsync<TMP_FontAsset>("RenderFontChineseSimplified").ToUniTask();
            //if (fontChineseTraditional != null)
                fontChineseTraditional = await Addressables.LoadAssetAsync<Font>("RenderFontChineseTraditional").ToUniTask();
            //if (tmpFontChineseTraditional != null)
                tmpFontChineseTraditional = await Addressables.LoadAssetAsync<TMP_FontAsset>("RenderFontChineseTraditional").ToUniTask();
            //if (fontKorean != null)
                fontKorean = await Addressables.LoadAssetAsync<Font>("RenderFontKorean").ToUniTask();
            //if (tmpFontKorean != null)
                tmpFontKorean = await Addressables.LoadAssetAsync<TMP_FontAsset>("RenderFontKorean").ToUniTask();
            //if (fontJapanese != null)
                fontJapanese = await Addressables.LoadAssetAsync<Font>("RenderFontJapanese").ToUniTask();
            //if (tmpFontJapanese != null)
                tmpFontJapanese = await Addressables.LoadAssetAsync<TMP_FontAsset>("RenderFontJapanese").ToUniTask();
            //if (fontEnglish != null)
                fontEnglish = await Addressables.LoadAssetAsync<Font>("RenderFontEnglish").ToUniTask();
            //if (tmpFontEnglish != null)
                tmpFontEnglish = await Addressables.LoadAssetAsync<TMP_FontAsset>("RenderFontEnglish").ToUniTask();

            fontsLoaded = true;
        }

        private void SetFonts(Font font, TMP_FontAsset tmpFont)
        {
            cardDescription.font = font;
            cardDescriptionRD.font = font;
            cardDescriptionPendulum.font = font;
            cardDescriptionPendulumRD.font = font;
            cardAuther.font = font;
            cardAutherRD.font = font;

            cardName.font = tmpFont;
            cardNameRD.font = tmpFont;
            spellType.font = tmpFont;
            cardTypeRD.font = tmpFont;
            attrRuby.font = tmpFont;
            attrRubyRD.font = tmpFont;
        }

        #endregion

        private void Awake()
        {
            _ = LoadFontsAsync();

            prefabIndex++;
            transform.position = new Vector3(0f, 200f * prefabIndex, 0f);
        }

        public void SwitchLanguage(string language = null)
        {
            if (!fontsLoaded)
                return;
            language ??= Language.GetCardConfig();
            if (currentFontLanguage == language)
                return;
            currentFontLanguage = language;

            LoadText(language);
            if (language == Language.SimplifiedChinese)
            {
                cardName.fontSize = 50f;
                cardNameRD.fontSize = 50f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSizeMax = 27f;
                SetFonts(fontChineseSimplified, tmpFontChineseSimplified);
            }
            else if (language == Language.TraditionalChinese)
            {
                cardName.fontSize = 55f;
                cardNameRD.fontSize = 55f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSizeMax = 28f;
                SetFonts(fontChineseTraditional, tmpFontChineseTraditional);
            }
            else if (language == Language.Korean)
            {
                cardName.fontSize = 50f;
                cardNameRD.fontSize = 50f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSizeMax = 27f;
                SetFonts(fontKorean, tmpFontKorean);
            }
            else if (language == Language.Japanese)
            {
                cardName.fontSize = 55f;
                cardNameRD.fontSize = 55f;
                spellType.fontSize = 40f;
                cardTypeRD.fontSizeMax = 29f;
                SetFonts(fontJapanese, tmpFontJapanese);
            }
            else
            {
                cardName.fontSize = 63f;
                cardNameRD.fontSize = 63f;
                spellType.fontSize = 43f;
                cardTypeRD.fontSizeMax = 30f;
                SetFonts(fontEnglish, tmpFontEnglish);
            }

            if (Language.CardUseLatin())
            {
                cardName.fontStyle = FontStyles.SmallCaps;
                cardNameRD.fontStyle = FontStyles.SmallCaps;
            }
            else
            {
                cardName.fontStyle = FontStyles.Normal;
                cardNameRD.fontStyle = FontStyles.Normal;
            }
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
            var data = CardsManager.GetRenderCard(code);
            if (data.Id == 0)
                return;
            if (data.isPre)
                SwitchLanguage(Language.GetPrereleaseConfig());
            else
                SwitchLanguage();

            if (NeedRushDuelStyle(code))
                SetRushDuelCardName(data);
            else
                SetOcgCardName(data);

            renderCamera.Render();
        }

        private void SetRushDuelCardName(Card data)
        {
            ocg.SetActive(false);
            rd.SetActive(true);

            cardNameRD.GetComponent<RectTransform>().localScale = Vector3.one;

            cardNameRD.text = data.Name;
            cardNameRD.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardNameRD.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthRushDuel)
                cardNameRD.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthRushDuel / nameWidth, 1f, 1f);

            cardNameRD.color = Color.white;
            attrRubyRD.text = GetAttributeText(data);

            cardArtRD.gameObject.SetActive(false);
            cardArtPendulumRD.gameObject.SetActive(false);
            cardArtPendulumWidthRD.gameObject.SetActive(false);
            cardFrameRD.gameObject.SetActive(false);
            attrIconRD.gameObject.SetActive(false);
            cardLegendRD.SetActive(false);
        }

        private void SetOcgCardName(Card data)
        {
            ocg.SetActive(true);
            rd.SetActive(false);

            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardName.text = data.Name;
            cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthOCG)
                cardName.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthOCG / nameWidth, 1, 1);

            cardName.color = Color.white;
            attrRuby.text = GetAttributeText(data);

            cardFrame.gameObject.SetActive(false);
            cardArt.gameObject.SetActive(false);
            cardArtPendulum.gameObject.SetActive(false);
            cardArtPendulumSquare.gameObject.SetActive(false);
            cardArtPendulumWidth.gameObject.SetActive(false);
            levels.SetActive(false);
            ranks.SetActive(false);
            rank13.SetActive(false);
            attrIcon.gameObject.SetActive(false);
            levelsMask.SetActive(false);
            ranksMask.SetActive(false);
            rank13Mask.SetActive(false);
            linkMarkers.SetActive(false);
            spellType.text = string.Empty;
            data = AdjustLevelForRender(data);
            if (data.HasType(CardType.Xyz))
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
            else if (data.HasType(CardType.Monster)
                && !data.HasType(CardType.Link))
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
        }

        public bool RenderCard(int code, Texture2D art)
        {
            Card data = CardsManager.GetRenderCard(code);
            if (data == null || data.Id == 0)
                return false;

            if (data.isPre)
                SwitchLanguage(Language.GetPrereleaseConfig());
            else
                SwitchLanguage();

            if (NeedRushDuelStyle(code))
                SetRushDuelCard(data, art);
            else
                SetOcgCard(data, art);

            renderCamera.Render();
            return true;
        }

        private void SetRushDuelCard(Card data, Texture2D art)
        {
            ocg.SetActive(false);
            rd.SetActive(true);

            if (Settings.Data.CardRenderPassword)
                cardPasswordRD.text = data.Id.ToString("D8");
            else
                cardPasswordRD.text = string.Empty;

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
            attrIconRD.gameObject.SetActive(true);
            cardDescriptionPendulumRD.text = string.Empty;
            lScaleRD.text = string.Empty;
            rScaleRD.text = string.Empty;
            levelRD.SetActive(false);
            rankRD.SetActive(false);
            linkRD.SetActive(false);
            levelNumRD.gameObject.SetActive(false);
            rankNumRD.gameObject.SetActive(false);
            atkNumRD.text = data.GetAttackString();
            defNumRD.text = data.GetDefenseString();
            atkRD.SetActive(true);
            defRD.SetActive(true);
            movePartsRD.gameObject.SetActive(true);
            movePartsRD.anchoredPosition = Vector2.zero;

            attrIconRD.sprite = TextureManager.container.GetCardAttributeIcon(data, true);
            attrRubyRD.text = GetAttributeText(data);
            cardTypeRD.text = data.GetTypeForRushDuelRender();

            if (data.HasType(CardType.Pendulum))
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
                cardDescriptionPendulumRD.text = TextForRender(data.GetPendulumDescription(true), data.isPre);

                var authorSplit = GetAuthorFromDescription(data.GetMonsterDescription(true));
                cardAutherRD.text = authorSplit[1];
                cardDescriptionRD.text = TextForRender(authorSplit[0], data.isPre);

                lScaleRD.text = data.LScale.ToString();
                rScaleRD.text = data.RScale.ToString();
                if (data.HasType(CardType.Xyz))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumXyz;
                else if (data.HasType(CardType.Synchro))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumSynchro;
                else if (data.HasType(CardType.Fusion))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumFusion;
                else if (data.HasType(CardType.Ritual))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumRitual;
                else if (data.HasType(CardType.Link))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumLink;
                else if (data.HasType(CardType.Normal))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumNormal;
                else
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_PendulumEffect;
            }
            else
            {
                cardArtRD.gameObject.SetActive(true);
                cardArtRD.texture = art;
                var authorSplit = GetAuthorFromDescription(data.Desc);
                cardDescriptionRD.text = TextForRender(authorSplit[0], data.isPre);
                cardAutherRD.text = TextForRender(authorSplit[1], data.isPre);
                cardDescriptionPendulumRD.text = string.Empty;

                if (data.Id == 10000000)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Obelisk;
                else if (data.Id == 10000010)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Ra;
                else if (data.Id == 10000020)
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Slifer;
                else if (data.HasType(CardType.Link))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Link;
                else if (data.HasType(CardType.Xyz))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Xyz;
                else if (data.HasType(CardType.Synchro))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Synchro;
                else if (data.HasType(CardType.Fusion))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Fusion;
                else if (data.HasType(CardType.Ritual) && data.HasType(CardType.Monster))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Ritual;
                else if (data.HasType(CardType.Token))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Token;
                else if (data.HasType(CardType.Normal))
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Normal;
                else if ((data.Type & ((uint)CardType.Spell) + (uint)CardType.Trap) > 0)
                {
                    atkRD.SetActive(false);
                    defRD.SetActive(false);
                    atkNumRD.text = string.Empty;
                    defNumRD.text = string.Empty;

                    if (data.HasType(CardType.Spell))
                        cardFrameRD.sprite = TextureManager.container.rd_Frame_Spell;
                    else
                        cardFrameRD.sprite = TextureManager.container.rd_Frame_Trap;
                }
                else
                    cardFrameRD.sprite = TextureManager.container.rd_Frame_Effect;
            }

            data = AdjustLevelForRender(data);

            if (data.HasType(CardType.Link))
            {
                cardNameRD.color = Color.white;
                defRD.SetActive(false);
                defNumRD.text = string.Empty;
                levelNumRD.gameObject.SetActive(true);
                levelNumRD.text = data.GetLinkCount().ToString();

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
            else if (data.HasType(CardType.Xyz))
            {
                cardNameRD.color = Color.white;
                if (!data.HasType(CardType.Pendulum))
                    cardTypeRD.color = Color.white;
                rankRD.SetActive(true);
                rankNumRD.gameObject.SetActive(true);
                rankNumRD.text = data.Level.ToString();
            }
            else if (data.HasType(CardType.Monster))
            {
                levelRD.SetActive(true);
                levelNumRD.gameObject.SetActive(true);
                levelNumRD.text = data.Level.ToString();
            }
        }

        private void SetOcgCard(Card data, Texture2D art)
        {
            ocg.SetActive(true);
            rd.SetActive(false);

            if (Settings.Data.CardRenderPassword)
                cardPassword.text = data.Id.ToString("D8");
            else
                cardPassword.text = string.Empty;
            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardName.text = data.Name;
            cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
            var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
            if (nameWidth > cardNameLabelWidthOCG)
                cardName.GetComponent<RectTransform>().localScale = new Vector3(cardNameLabelWidthOCG / nameWidth, 1, 1);

            cardName.color = Color.black;
            cardPassword.color = Color.black;
            cardAuther.color = Color.black;

            cardArt.gameObject.SetActive(false);
            cardArtPendulum.gameObject.SetActive(false);
            cardArtPendulumSquare.gameObject.SetActive(false);
            cardArtPendulumWidth.gameObject.SetActive(false);

            cardFrame.gameObject.SetActive(true);
            attrIcon.gameObject.SetActive(true);
            cardDescriptionPendulum.text = string.Empty;
            lScale.text = string.Empty;
            rScale.text = string.Empty;
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
            spellType.text = string.Empty;
            cardDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(590f, 160f);
            attrIcon.sprite = TextureManager.container.GetCardAttributeIcon(data, true);
            attrRuby.text = GetAttributeText(data);

            if (data.HasType(CardType.Pendulum))
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
                var pendulumDescription = data.GetDescriptionSplit(true);
                cardDescription.text = data.GetTypeForRushDuelRender();
                cardDescriptionPendulum.text = TextForRender(pendulumDescription[0], data.isPre);

                var authorSplit = GetAuthorFromDescription(pendulumDescription[1]);
                cardDescription.text += Program.STRING_LINE_BREAK + TextForRender(authorSplit[0], data.isPre);
                cardAuther.text = authorSplit[1];

                lScale.text = data.LScale.ToString();
                rScale.text = data.RScale.ToString();
                if (data.HasType(CardType.Xyz))
                    cardFrame.sprite = TextureManager.container.cardFramePendulumXyzOF;
                else if (data.HasType(CardType.Synchro))
                    cardFrame.sprite = TextureManager.container.cardFramePendulumSynchroOF;
                else if (data.HasType(CardType.Fusion))
                    cardFrame.sprite = TextureManager.container.cardFramePendulumFusionOF;
                else if (data.HasType(CardType.Ritual))
                    cardFrame.sprite = TextureManager.container.cardFramePendulumRitualOF;
                else if (data.HasType(CardType.Normal))
                    cardFrame.sprite = TextureManager.container.cardFramePendulumNormalOF;
                else
                    cardFrame.sprite = TextureManager.container.cardFramePendulumEffectOF;
            }
            else
            {
                cardArt.gameObject.SetActive(true);
                cardArt.texture = art;
                var description = string.Empty;
                if (data.HasType(CardType.Monster))
                    description = data.GetTypeForRushDuelRender() + Program.STRING_LINE_BREAK;

                var authorSplit = GetAuthorFromDescription(data.Desc);
                description += TextForRender(authorSplit[0], data.isPre);
                cardDescription.text = description;
                cardAuther.text = authorSplit[1];

                if (data.Id == 10000000)
                    cardFrame.sprite = TextureManager.container.cardFrameObeliskOF;
                else if (data.Id == 10000010)
                    cardFrame.sprite = TextureManager.container.cardFrameRaOF;
                else if (data.Id == 10000020)
                    cardFrame.sprite = TextureManager.container.cardFrameOsirisOF;
                else if (data.HasType(CardType.Link))
                    cardFrame.sprite = TextureManager.container.cardFrameLinkOF;
                else if (data.HasType(CardType.Xyz))
                    cardFrame.sprite = TextureManager.container.cardFrameXyzOF;
                else if (data.HasType(CardType.Synchro))
                    cardFrame.sprite = TextureManager.container.cardFrameSynchroOF;
                else if (data.HasType(CardType.Fusion))
                    cardFrame.sprite = TextureManager.container.cardFrameFusionOF;
                else if (data.HasType(CardType.Ritual) && data.HasType(CardType.Monster))
                    cardFrame.sprite = TextureManager.container.cardFrameRitualOF;
                else if (data.HasType(CardType.Token))
                    cardFrame.sprite = TextureManager.container.cardFrameTokenOF;
                else if (data.HasType(CardType.Normal))
                    cardFrame.sprite = TextureManager.container.cardFrameNormalOF;
                else if ((data.Type & ((uint)CardType.Spell) + (uint)CardType.Trap) > 0)
                {
                    cardDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(590, 185);
                    cardName.color = Color.white;
                    line.SetActive(false);
                    textATK.SetActive(false);
                    textDEF.SetActive(false);
                    numATK.text = string.Empty;
                    numDEF.text = string.Empty;
                    spellType.text = data.GetSpellTypeForOCGRender();

                    if (data.HasType(CardType.Spell))
                        cardFrame.sprite = TextureManager.container.cardFrameSpellOF;
                    else
                        cardFrame.sprite = TextureManager.container.cardFrameTrapOF;
                }
                else
                    cardFrame.sprite = TextureManager.container.cardFrameEffectOF;
            }

            data = AdjustLevelForRender(data);

            if (data.HasType(CardType.Link))
            {
                cardName.color = Color.white;
                linkMarkers.SetActive(true);
                textDEF.SetActive(false);
                numDEF.text = string.Empty;
                linkCount.gameObject.SetActive(true);
                switch (data.GetLinkCount())
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
            else if (data.HasType(CardType.Xyz))
            {
                cardName.color = Color.white;
                if (!data.HasType(CardType.Pendulum))
                {
                    cardPassword.color = Color.white;
                    cardAuther.color = Color.white;
                }

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
            else if (data.HasType(CardType.Monster))
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
        }

        private static Card AdjustLevelForRender(Card data)
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

        private string TextForRender(string description, bool isPre)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;
            var language = isPre ? Language.GetPrereleaseConfig() : Language.GetCardConfig();

            //if (language == Language.Japanese)
            //{
                description = description.Replace("\t\r\n", "\f\f\f");
                description = description.Replace("\r\n●", "●●●");
                description = description.Replace("\r", string.Empty);
                description = description.Replace("\n", string.Empty);
                description = description.Replace("\f\f\f", Program.STRING_LINE_BREAK);
                description = description.Replace("●●●", $"{Program.STRING_LINE_BREAK}●");
            //}
            //else
            //{
            //    description = description
            //        .Replace("\r\n②", "②")
            //        .Replace("\r\n③", "③")
            //        .Replace("\r\n④", "④")
            //        .Replace("\r\n⑤", "⑤")
            //        .Replace("\r\n⑥", "⑥")
            //        .Replace("\r\n⑦", "⑦")
            //        .Replace("\r\n⑧", "⑧")
            //        .Replace("\r\n⑨", "⑨");
            //}

            if (!Language.CardUseLatin(language))
                description = description.Replace(Program.STRING_SLASH, BIG_SLASH);
            else
                description = description.Replace(Program.STRING_SLASH, SMALL_SLASH);

            if (!Language.CardUseLatin(language))
                description = description.Replace(" ", "\u00A0");
            description = description.Replace($"{Program.STRING_LINE_BREAK}{Program.STRING_LINE_BREAK}", Program.STRING_LINE_BREAK);
            return description;
        }

        private static List<string> GetAuthorFromDescription(string description)
        {
            var lines = description.Split(Program.STRING_LINE_BREAK);
            var returnValue = new List<string>();

            StringBuilder beforeDiySymbol = new StringBuilder();
            bool foundDIY = false;

            foreach (var line in lines)
            {
                if(!foundDIY && line.StartsWith(Settings.Data.DiySymbol))
                {
                    var beforeDiySymbolText = beforeDiySymbol.ToString();
                    returnValue.Add(beforeDiySymbolText);
                    returnValue.Add(line);
                    foundDIY = true;
                }
                else if(!foundDIY && !string.IsNullOrEmpty(line))
                {
                    beforeDiySymbol.Append(line);
                }

                if (foundDIY) 
                    break;
            }

            if (!foundDIY)
            {
                returnValue.Add(description);
                returnValue.Add(string.Empty);
            }

            return returnValue;
        }

        #region Video Card

        public static bool CardHasVideoArt(int code)
        {
            if (!Config.GetBool("VideoCard", true))
                return false;
            if (File.Exists(Program.PATH_VIDEO_ART + code.ToString() + Program.EXPANSION_MP4))
                return true;
            return false;
        }

        private static string GetVideoURL(int code)
        {
            string path = Program.PATH_VIDEO_ART + code.ToString() + Program.EXPANSION_MP4;
            path = Tools.GetPlatformPath(path);
            path = Tools.FormatPlatformUrl(path);

            return path;
        }

        public async UniTask<Texture> GetVideoCardAsync(int code)
        {
            if (!CardHasVideoArt(code))
                return null;

            Card data = CardsManager.GetRenderCard(code);
            if (data == null || data.Id == 0)
                return null;

            if (data.isPre)
                SwitchLanguage(Language.GetPrereleaseConfig());
            else
                SwitchLanguage();

            var isRD = NeedRushDuelStyle(data.Id);
            var isPendulum = data.HasType(CardType.Pendulum);

            if (isRD)
            {
                SetRushDuelCard(data, null);
                cardArtRD.gameObject.SetActive(false);
                cardArtPendulumRD.gameObject.SetActive(false);
                cardArtPendulumWidthRD.gameObject.SetActive(false);
            }
            else
            {
                SetOcgCard(data, null);
                cardArt.gameObject.SetActive(false);
                cardArtPendulum.gameObject.SetActive(false);
                cardArtPendulumSquare.gameObject.SetActive(false);
                cardArtPendulumWidth.gameObject.SetActive(false);
            }

            videoPlayer.gameObject.SetActive(true);
            videoPlayer.url = GetVideoURL(code);
            videoPlayer.targetTexture = Instantiate(videoPlayer.targetTexture);

            RawImage targetImage;
            if (isRD)
            {
                if (isPendulum)
                    targetImage = cardArtPendulumRD;
                else
                    targetImage = cardArtRD;
            }
            else
            {
                if (isPendulum)
                    targetImage = cardArtPendulumSquare;
                else
                    targetImage = cardArt;
            }

            renderCamera.Render();
            RenderTexture.active = renderTexture;
            var onlyFrame = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.RGBA32, true);
            onlyFrame.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            onlyFrame.Apply();
            onlyFrame.name = "Card_" + code;
            renderedCardFrame.texture = onlyFrame;
            renderedCardFrame.gameObject.SetActive(true);

            targetImage.gameObject.SetActive(true);
            targetImage.texture = videoPlayer.targetTexture;
            targetImage.transform.SetParent(transform);
            renderedCardFrame.transform.SetAsLastSibling();
            Destroy(ocg);
            Destroy(rd);

            videoPlayer.Prepare();
            await UniTask.WaitUntil(() => videoPlayer.isPrepared);

            renderCamera.gameObject.SetActive(true);
            renderCamera.targetTexture = Instantiate(renderTexture);
            renderCamera.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.EveryFrame);
            renderTexture = renderCamera.targetTexture;

            return renderTexture;
        }

        public void PauseVideo()
        {
            renderCamera.gameObject.SetActive(false);
            videoPlayer.Pause();
        }

        public void PlayVideo()
        {
            renderCamera.gameObject.SetActive(true);
            videoPlayer.Play();
        }

        public void Dispose()
        {
            Destroy(renderTexture);
            Destroy(gameObject);
        }

        #endregion

        #region IDS_SYS

        private readonly Dictionary<string, string> idsSysText = new();        

        private void LoadText(string language)
        {
            idsSysText.Clear();
            var path = $"{Program.PATH_LOCALES}{language}/IDS/IDS_SYS.txt";
            if (!File.Exists(path))
                return;
            var text = File.ReadAllText(path);
            var lines = text.Replace("\r", string.Empty).Split('\n');

            string currentKey = null;
            string currentValue = null;

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"(?<=\[IDS_SYS\.).*?(?=\])");
                if (match.Success)
                {
                    if (currentValue != null)
                        idsSysText[currentKey] = currentValue;
                    currentKey = match.Value;
                }
                else
                    currentValue = line;
            }

            if(currentKey != null && currentValue != null)
                idsSysText[currentKey] = currentValue;
        }

        private string GetIdsSysText(string key)
        {
            if (idsSysText.TryGetValue(key, out var value))
                return value;
            return string.Empty;
        }

        private string GetAttributeText(Card data)
        {
            if (data.HasType(CardType.Spell))
                return GetIdsSysText("ATTR_MAGIC_RUBY");
            else if (data.HasType(CardType.Trap))
                return GetIdsSysText("ATTR_TRAP_RUBY");
            else if (data.IsAttribute(CardAttribute.Light))
                return GetIdsSysText("ATTR_LIGHT_RUBY");
            else if(data.IsAttribute(CardAttribute.Dark))
                return GetIdsSysText("ATTR_DARK_RUBY");
            else if (data.IsAttribute(CardAttribute.Water))
                return GetIdsSysText("ATTR_WATER_RUBY");
            else if (data.IsAttribute(CardAttribute.Fire))
                return GetIdsSysText("ATTR_FIRE_RUBY");
            else if (data.IsAttribute(CardAttribute.Earth))
                return GetIdsSysText("ATTR_EARTH_RUBY");
            else if (data.IsAttribute(CardAttribute.Wind))
                return GetIdsSysText("ATTR_WIND_RUBY");
            else if (data.IsAttribute(CardAttribute.Divine))
                return GetIdsSysText("ATTR_GOD_RUBY");
            else
                return string.Empty;
        }

        #endregion

    }
}
