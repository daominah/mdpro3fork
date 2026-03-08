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
            CleanupOverFrame(); // prevent OCG overlay sticking on Rush cards

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
            CleanupOverFrame();

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

        private static Texture2D LoadPngFromResourcesOrFile(string resourcesPathNoExt, string filePath)
        {
            var tex = Resources.Load<Texture2D>(resourcesPathNoExt);
            if (tex != null) return tex;

            if (!File.Exists(filePath)) return null;

            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(filePath));
            return tex;
        }

        // OverFrame source supports both folder spellings used in mods.
        private static Texture2D LoadOverFrameTexture(int code)
        {
            string fileName = code + ".png";

            var tex = LoadPngFromResourcesOrFile(
                $"Picture/OverFrame/{code}",
                Path.Combine("Picture", "OverFrame", fileName)
            );
            if (tex != null) return tex;

            return LoadPngFromResourcesOrFile(
                $"Picture/Overframe/{code}",
                Path.Combine("Picture", "Overframe", fileName)
            );
        }

        // ────────────────────────────────────────────────
        //  OverFrame rendering helper (OCG / TCG)
        //  - Loads overlay from Picture/OverFrame/<code>.png
        //  - Does NOT hide stats (ATK/DEF/Level/etc stay on top)
        //  - Adds a proxy-like wash/fade over the effect text box area
        // ────────────────────────────────────────────────


// ────────────────────────────────────────────────
// OverFrame (OCG proxy-style OverArt) — IMPORTANT CLIP REFS
// ────────────────────────────────────────────────

public enum OverFrameEffectBoxRectMode
{
    /// <summary>
    /// Auto-detect whether the chosen effect-box rect includes the parchment border.
    /// Uses normalized rect comparison + sprite border data when available.
    /// </summary>
    Auto,

    /// <summary>
    /// The chosen rect includes the parchment border lines (outer box).
    /// We will cut INWARD (9-slice border if available, otherwise fallback inset) so the OverArt never tints the border.
    /// </summary>
    OuterRectIncludesBorder,

    /// <summary>
    /// The chosen rect is already the INNER fill area (border already excluded).
    /// We will NOT apply border/fallback inset (only tiny filtering safety).
    /// </summary>
    InnerRectAlreadyInset
}

[Header("OverFrame / Proxy (OverArt)")]
[Tooltip("Assign the parchment/effect-box Image that draws the effect text box (INCLUDING the border). This must be the real parchment Image (the one that visually has the orange/gold border), not the TMP text rect or a padding container.")]
public Image overFrameEffectBoxImage;

[Tooltip("How to interpret the effect-box rect when clipping OverArt into the parchment area. Auto is recommended. If your parchment sprite has NO 9-slice border data, Auto still works by comparing to the reference rect.")]
public OverFrameEffectBoxRectMode overFrameEffectBoxRectMode = OverFrameEffectBoxRectMode.Auto;

        private struct OverFrameSpec
        {
            // Per-card tuning for overlay placement
            public readonly float scale;
            public readonly Vector2 offset;

            public OverFrameSpec(float scale, Vector2 offset)
            {
                this.scale = scale;
                this.offset = offset;
            }
        }

        // OverFrame renders automatically when an overlay file for the card ID exists.
        // This table is optional and only used for per-card placement tweaks.
        private static readonly OverFrameSpec DefaultOverFrameSpec = new OverFrameSpec(1.00f, Vector2.zero);
        private static readonly Dictionary<int, OverFrameSpec> OverFrameCardTweaks = new()
        {
            // Example tweak entry:
            // { 100256012, new OverFrameSpec(1.00f, new Vector2(0f, 0f)) },
        };

        // Cached UI objects (created once, reused each render)
        private RawImage _overFrameArt;

        // Effect-box wash overlay (proxy-like, clipped to parchment shape when possible)
        private RectTransform _overFrameEffectFadeMaskRt;
        private Image _overFrameEffectFadeMaskImg;
        private Mask _overFrameEffectFadeMask;
        private VerticalGradientGraphic _overFrameEffectFade;

        // Proxy-style split: hard stop + faint continuation inside parchment
        private RawImage _overFrameArtText;

        // BG continuation (uses base card art so the parchment area is never "empty")
        private RawImage _overFrameArtTextBG;

        // Side continuations (outside parchment box; fixes the "empty red zones" in proxy comparison)
        private RawImage _overFrameArtSideL;
        private RawImage _overFrameArtSideR;

        private RectTransform _overFrameSideClipL_Rt;
        private RectTransform _overFrameSideClipR_Rt;
        private RectMask2D _overFrameSideClipL;
        private RectMask2D _overFrameSideClipR;

        // Pendulum-only extra continuation clips for the left/right scale parchments.
        private RectTransform _overFramePendulumCenterClipRt;
        private RectMask2D _overFramePendulumCenterClip;
        private RawImage _overFramePendulumCenterArt;
        private RectTransform _overFramePendulumScaleClipL_Rt;
        private RectTransform _overFramePendulumScaleClipR_Rt;
        private RectMask2D _overFramePendulumScaleClipL;
        private RectMask2D _overFramePendulumScaleClipR;
        private RawImage _overFramePendulumScaleArtL;
        private RawImage _overFramePendulumScaleArtR;

        // How strong the base-art continuation is inside the parchment
        // Tuned to match Konami proxy readability in the effect box (less muddy continuation).
        private const float OverFrameTextBgAlpha = 0.00f;


        private RectTransform _overFrameMainClipRt;   // clips main overframe (upper area)
        private RectMask2D _overFrameMainClip;

        private RectTransform _overFrameTextClipRt;   // clips faint continuation (parchment area)
        private RectMask2D _overFrameTextClip;


        // ────────────────────────────────────────────────
        // Fade tuning knobs (proxy-like readability)
        // ────────────────────────────────────────────────
        private const float OverFrameFadeTopAlpha = 0.05f;
        private const float OverFrameFadeBottomAlpha = 0.11f;
        private const float OverFramePendulumFadeTopAlpha = 0.025f;
        private const float OverFramePendulumFadeBottomAlpha = 0.055f;

        // When the parchment background Image exists, use it as the wash overlay (looks like the proxy, not a flat rectangle)
        private const bool OverFramePreferSpriteWash = true;
        private const bool OverFrameEnableWashOverlay = true;
        private const float OverFrameWashSpriteAlpha = 0.12f;
        private const float OverFramePendulumWashSpriteAlpha = 0.07f;

        // Pendulum-only extra fade overlays for left/right scale parchment zones.
        private VerticalGradientGraphic _overFrameEffectFadePendulumC;
        private VerticalGradientGraphic _overFrameEffectFadePendulumL;
        private VerticalGradientGraphic _overFrameEffectFadePendulumR;

        private const float OverFrameFadePadTop = 0f;
        private const float OverFrameFadePadBottom = 0f;
        private const float OverFrameFadePadSide = 0f;

        // Debug: makes the fade obviously visible
        private const bool OverFrameFadeDebugMagenta = false;


        // ────────────────────────────────────────────────
        // Cleanup (disable overlays when not used)
        // ────────────────────────────────────────────────
        private void CleanupOverFrame()
        {
            if (_overFrameArt) _overFrameArt.gameObject.SetActive(false);
            if (_overFrameArtText) _overFrameArtText.gameObject.SetActive(false);
            if (_overFrameArtTextBG) _overFrameArtTextBG.gameObject.SetActive(false);

            if (_overFrameMainClipRt) _overFrameMainClipRt.gameObject.SetActive(false);
            if (_overFrameTextClipRt) _overFrameTextClipRt.gameObject.SetActive(false);
            if (_overFrameArtSideL) _overFrameArtSideL.gameObject.SetActive(false);
            if (_overFrameArtSideR) _overFrameArtSideR.gameObject.SetActive(false);
            if (_overFramePendulumCenterArt) _overFramePendulumCenterArt.gameObject.SetActive(false);
            if (_overFramePendulumScaleArtL) _overFramePendulumScaleArtL.gameObject.SetActive(false);
            if (_overFramePendulumScaleArtR) _overFramePendulumScaleArtR.gameObject.SetActive(false);

            if (_overFrameSideClipL_Rt) _overFrameSideClipL_Rt.gameObject.SetActive(false);
            if (_overFrameSideClipR_Rt) _overFrameSideClipR_Rt.gameObject.SetActive(false);
            if (_overFramePendulumCenterClipRt) _overFramePendulumCenterClipRt.gameObject.SetActive(false);
            if (_overFramePendulumScaleClipL_Rt) _overFramePendulumScaleClipL_Rt.gameObject.SetActive(false);
            if (_overFramePendulumScaleClipR_Rt) _overFramePendulumScaleClipR_Rt.gameObject.SetActive(false);

            if (_overFrameEffectFadeMaskRt) _overFrameEffectFadeMaskRt.gameObject.SetActive(false);

            if (_overFrameEffectFade) _overFrameEffectFade.gameObject.SetActive(false);
            if (_overFrameEffectFadePendulumC) _overFrameEffectFadePendulumC.gameObject.SetActive(false);
            if (_overFrameEffectFadePendulumL) _overFrameEffectFadePendulumL.gameObject.SetActive(false);
            if (_overFrameEffectFadePendulumR) _overFrameEffectFadePendulumR.gameObject.SetActive(false);
        }


        // ────────────────────────────────────────────────
        // Helper: pick the currently active OCG artwork image
        // (different UI variants can be enabled depending on card type)
        // ────────────────────────────────────────────────
        private RawImage GetActiveOcgArtImage()
        {
            if (cardArt != null && cardArt.gameObject.activeSelf) return cardArt;
            if (cardArtPendulumSquare != null && cardArtPendulumSquare.gameObject.activeSelf) return cardArtPendulumSquare;
            if (cardArtPendulumWidth != null && cardArtPendulumWidth.gameObject.activeSelf) return cardArtPendulumWidth;
            if (cardArtPendulum != null && cardArtPendulum.gameObject.activeSelf) return cardArtPendulum;
            return cardArt;
        }

        private bool IsPendulumArtImage(RawImage artImage)
        {
            return artImage != null &&
                   (artImage == cardArtPendulum ||
                    artImage == cardArtPendulumSquare ||
                    artImage == cardArtPendulumWidth);
        }


        // ────────────────────────────────────────────────
        // Helper: move overlay out of Mask / RectMask2D so it can extend past art window
        // ────────────────────────────────────────────────
        private Transform GetOverFrameSafeParent(RawImage baseArt)
        {
            if (baseArt == null) return null;
            var p = baseArt.transform.parent;
            if (p == null) return null;

            // If base art sits under a Mask/RectMask2D (common for artwork windows),
            // move overlay one level up so it can extend outside the window.
            if (p.GetComponent<Mask>() != null || p.GetComponent<RectMask2D>() != null)
                return p.parent != null ? p.parent : p;

            return p;
        }


        // ────────────────────────────────────────────────
        // Helper: match dst RectTransform to src world rect (in dstParent space)
        // ────────────────────────────────────────────────
        private static void MatchRectByWorldCorners(RectTransform dst, RectTransform src, RectTransform dstParent)
        {
            var corners = new Vector3[4];
            src.GetWorldCorners(corners); // 0=BL, 1=TL, 2=TR, 3=BR

            var bl = (Vector3)dstParent.InverseTransformPoint(corners[0]);
            var tr = (Vector3)dstParent.InverseTransformPoint(corners[2]);

            var size = new Vector2(tr.x - bl.x, tr.y - bl.y);
            var center = new Vector2(bl.x + size.x * 0.5f, bl.y + size.y * 0.5f);

            // Center anchors/pivot so scaling expands evenly (prevents “shrink/slide” feel)
            dst.anchorMin = dst.anchorMax = new Vector2(0.5f, 0.5f);
            dst.pivot = new Vector2(0.5f, 0.5f);
            dst.localScale = Vector3.one;
            dst.localRotation = Quaternion.identity;

            dst.sizeDelta = size;
            dst.anchoredPosition = center;
        }


        // ────────────────────────────────────────────────
        // Helper: cheap alpha test (warn if your overlay has no transparency)
        // ────────────────────────────────────────────────
        private static bool TextureHasTransparency(Texture2D tex, int grid = 64, byte alphaThreshold = 250)
        {
            if (tex == null || !tex.isReadable) return true; // assume ok if we can't read

            int w = tex.width;
            int h = tex.height;

            // Sample a grid (fast, no huge allocations)
            for (int gy = 0; gy < grid; gy++)
            {
                int y = (gy * (h - 1)) / (grid - 1);
                for (int gx = 0; gx < grid; gx++)
                {
                    int x = (gx * (w - 1)) / (grid - 1);
                    var c = tex.GetPixel(x, y);
                    if (c.a * 255f < alphaThreshold)
                        return true;
                }
            }
            return false;
        }


        // ────────────────────────────────────────────────
        // Simple vertical gradient quad for UI (no shader needed)
        // Used as the "washed out" overlay in the effect text box
        // ────────────────────────────────────────────────
        private class VerticalGradientGraphic : Graphic
        {
            public Color topColor = new Color(1f, 1f, 1f, 0f);
            public Color bottomColor = new Color(1f, 1f, 1f, 0.75f);

            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();
                var r = GetPixelAdjustedRect();

                // BL, TL, TR, BR
                var v0 = UIVertex.simpleVert; v0.position = new Vector3(r.xMin, r.yMin); v0.color = bottomColor;
                var v1 = UIVertex.simpleVert; v1.position = new Vector3(r.xMin, r.yMax); v1.color = topColor;
                var v2 = UIVertex.simpleVert; v2.position = new Vector3(r.xMax, r.yMax); v2.color = topColor;
                var v3 = UIVertex.simpleVert; v3.position = new Vector3(r.xMax, r.yMin); v3.color = bottomColor;

                vh.AddVert(v0);
                vh.AddVert(v1);
                vh.AddVert(v2);
                vh.AddVert(v3);

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
        }


        // ────────────────────────────────────────────────
        // Effect-box rectangle detection (find the actual parchment/panel behind text)
        // ────────────────────────────────────────────────
        private Text GetActiveOcgDescriptionText()
        {
            // Pendulum cards keep both description texts active; prefer the pendulum zone when it has content.
            if (cardDescriptionPendulum != null &&
                cardDescriptionPendulum.gameObject.activeInHierarchy &&
                !string.IsNullOrEmpty(cardDescriptionPendulum.text))
                return cardDescriptionPendulum;

            if (cardDescription != null && cardDescription.gameObject.activeInHierarchy) return cardDescription;
            if (cardDescriptionPendulum != null && cardDescriptionPendulum.gameObject.activeInHierarchy) return cardDescriptionPendulum;
            return cardDescription;
        }

        
        private RectTransform GetEffectBoxRectTransform(Text preferredDesc = null, bool allowExplicitOverride = true, float minAreaOverTextFactor = 1.05f)
        {
            // 1) Explicit override (preferred): the REAL parchment Image rect.
            // This prevents accidentally masking with TMP text rects / padding containers.
            if (allowExplicitOverride &&
                overFrameEffectBoxImage != null &&
                overFrameEffectBoxImage.gameObject != null &&
                overFrameEffectBoxImage.gameObject.activeInHierarchy)
                return overFrameEffectBoxImage.rectTransform;

            var desc = preferredDesc ?? GetActiveOcgDescriptionText();
            if (desc == null) return null;

            var descRt = desc.GetComponent<RectTransform>();
            if (descRt == null) return null;

            // We want the *parchment box* (bordered area), not just the text rect.
            // In some prefabs the background Image is a SIBLING of the Text, not a parent.
            // So we search nearby scopes for the smallest Image-backed rect that fully contains the text rect.

            // Desc world AABB
            var dc = new Vector3[4];
            descRt.GetWorldCorners(dc);
            float dMinX = dc[0].x, dMinY = dc[0].y, dMaxX = dc[0].x, dMaxY = dc[0].y;
            for (int i = 1; i < 4; i++)
            {
                dMinX = Mathf.Min(dMinX, dc[i].x);
                dMinY = Mathf.Min(dMinY, dc[i].y);
                dMaxX = Mathf.Max(dMaxX, dc[i].x);
                dMaxY = Mathf.Max(dMaxY, dc[i].y);
            }
            float dArea = Mathf.Max(0.0001f, (dMaxX - dMinX) * (dMaxY - dMinY));

            float maxArea = float.PositiveInfinity;
            if (cardFrame != null)
            {
                var fc = new Vector3[4];
                cardFrame.rectTransform.GetWorldCorners(fc);
                float fMinX = fc[0].x, fMinY = fc[0].y, fMaxX = fc[0].x, fMaxY = fc[0].y;
                for (int i = 1; i < 4; i++)
                {
                    fMinX = Mathf.Min(fMinX, fc[i].x);
                    fMinY = Mathf.Min(fMinY, fc[i].y);
                    fMaxX = Mathf.Max(fMaxX, fc[i].x);
                    fMaxY = Mathf.Max(fMaxY, fc[i].y);
                }
                maxArea = (fMaxX - fMinX) * (fMaxY - fMinY) * 0.85f; // avoid selecting the full card/frame
            }

            RectTransform best = null;
            float bestArea = float.PositiveInfinity;

            // Prefer the actual parchment Image (usually has a 9-slice border / Sliced type).
            // This avoids accidentally picking the TMP text rect or any OverFrame helper masks.
            RectTransform bestBordered = null;
            float bestBorderedArea = float.PositiveInfinity;

            // Search up to 3 ancestor scopes (parent, grandparent, great-grandparent)
            var scope = descRt.parent as RectTransform;
            for (int depth = 0; depth < 3 && scope != null; depth++)
            {
                var imgs = scope.GetComponentsInChildren<Image>(true);
                foreach (var img in imgs)
                {
                    if (img == null || img.sprite == null) continue;
                    var rt = img.rectTransform;
                    if (rt == null) continue;
                    if (cardFrame != null && rt == cardFrame.rectTransform) continue;
                    if (rt.gameObject != null && rt.gameObject.name.StartsWith("OverFrame")) continue;

                    var wc = new Vector3[4];
                    rt.GetWorldCorners(wc);
                    float oMinX = wc[0].x, oMinY = wc[0].y, oMaxX = wc[0].x, oMaxY = wc[0].y;
                    for (int i = 1; i < 4; i++)
                    {
                        oMinX = Mathf.Min(oMinX, wc[i].x);
                        oMinY = Mathf.Min(oMinY, wc[i].y);
                        oMaxX = Mathf.Max(oMaxX, wc[i].x);
                        oMaxY = Mathf.Max(oMaxY, wc[i].y);
                    }

                    // Must fully contain the text rect
                    if (oMinX > dMinX || oMinY > dMinY || oMaxX < dMaxX || oMaxY < dMaxY)
                        continue;

                    float area = Mathf.Max(0.0001f, (oMaxX - oMinX) * (oMaxY - oMinY));

                    // Must be meaningfully larger than text rect, but not huge
                    if (area < dArea * minAreaOverTextFactor) continue;
                    if (area > maxArea) continue;

                    bool bordered = img.type == Image.Type.Sliced;
                    if (!bordered && img.sprite != null)
                    {
                        var b = img.sprite.border;
                        bordered = (b.x > 0f || b.y > 0f || b.z > 0f || b.w > 0f);
                    }

                    if (bordered)
                    {
                        if (area < bestBorderedArea)
                        {
                            bestBorderedArea = area;
                            bestBordered = rt;
                        }
                    }
                    else
                    {
                        if (area < bestArea)
                        {
                            bestArea = area;
                            best = rt;
                        }
                    }
                }

                // If we found something plausible, stop early (nearest scope is usually correct)
                if (best != null) break;

                scope = scope.parent as RectTransform;
            }

            if (bestBordered != null) return bestBordered;

            if (best != null) return best;

            // Fallback: the old parent-walk heuristic
            if (descRt.parent is RectTransform pRt)
            {
                var img = pRt.GetComponent<Image>();
                if (img != null) return pRt;
            }

            var cur = descRt.parent as RectTransform;
            while (cur != null)
            {
                var img = cur.GetComponent<Image>();
                if (img != null)
                {
                    var cr = cur.rect;
                    var dr = descRt.rect;
                    if (cr.width >= dr.width && cr.height >= dr.height)
                        return cur;
                }
                cur = cur.parent as RectTransform;
            }

            // Last resort: text rect (may be too narrow, but prevents null refs)
            return descRt;
        }

        // ────────────────────────────────────────────────
        // Effect-box normalized rectangle (relative to full card frame)
        // Converts the detected parchment/effect-box world rect to a [0..1] normalized rect in frame space.
        // This is what lets the continuation reach the true parchment edges without negative insets.
        // ────────────────────────────────────────────────
        private bool TryGetEffectBoxNormalizedRect(RectTransform frameRt, out Rect rectNrm)
        {
            rectNrm = default;

            if (frameRt == null)
                return false;

            var boxRt = GetEffectBoxRectTransform();
            if (boxRt == null)
                return false;

            // Frame world corners
            var fc = new Vector3[4];
            frameRt.GetWorldCorners(fc); // 0=BL,1=TL,2=TR,3=BR

            Vector3 bl = fc[0];
            Vector3 br = fc[3];
            Vector3 tl = fc[1];

            Vector3 w = br - bl;
            Vector3 h = tl - bl;

            float wLen2 = Vector3.Dot(w, w);
            float hLen2 = Vector3.Dot(h, h);
            if (wLen2 < 1e-6f || hLen2 < 1e-6f)
                return false;

            // Box world corners
            var bc = new Vector3[4];
            boxRt.GetWorldCorners(bc);

            float minX =  999f, minY =  999f;
            float maxX = -999f, maxY = -999f;

            for (int i = 0; i < 4; i++)
            {
                Vector3 rel = bc[i] - bl;
                float nx = Vector3.Dot(rel, w) / wLen2;
                float ny = Vector3.Dot(rel, h) / hLen2;

                minX = Mathf.Min(minX, nx);
                minY = Mathf.Min(minY, ny);
                maxX = Mathf.Max(maxX, nx);
                maxY = Mathf.Max(maxY, ny);
            }

            // Clamp to sane bounds (avoid tiny floating drift)
            minX = Mathf.Clamp01(minX);
            minY = Mathf.Clamp01(minY);
            maxX = Mathf.Clamp01(maxX);
            maxY = Mathf.Clamp01(maxY);

            float width = Mathf.Max(0f, maxX - minX);
            float height = Mathf.Max(0f, maxY - minY);

            // Reject degenerate rects
            if (width < 0.001f || height < 0.001f)
                return false;


            // If the user explicitly assigned the parchment Image, trust it.
            // The old sanity-check is meant to protect against accidentally selecting TMP text/mask rects.
            bool explicitBox = (overFrameEffectBoxImage != null && boxRt == overFrameEffectBoxImage.rectTransform);
            if (explicitBox)
            {
                rectNrm = new Rect(minX, minY, width, height);
                return true;
            }

            // Sanity check: sometimes the search finds the Text rect or a mask instead of the real parchment box.
            // If the detected rect is way off from the expected box, ignore it and fall back to OverFrameEffectBoxNrm.
            var expected = OverFrameEffectBoxNrm;
            if (Mathf.Abs(minX - expected.xMin) > 0.12f ||
                Mathf.Abs(width - expected.width) > 0.20f ||
                Mathf.Abs(minY - expected.yMin) > 0.08f ||
                Mathf.Abs(height - expected.height) > 0.12f)
            {
                return false;
            }

            rectNrm = new Rect(minX, minY, width, height);
            return true;
        }

        // Converts an arbitrary RectTransform world-rect into frame-normalized coordinates [0..1].
        // Unlike TryGetEffectBoxNormalizedRect, this has no sanity filter and is used for pendulum-specific regions.
        private static bool TryGetRectNormalizedInFrame(RectTransform frameRt, RectTransform sourceRt, out Rect rectNrm)
        {
            rectNrm = default;
            if (frameRt == null || sourceRt == null) return false;

            var fc = new Vector3[4];
            frameRt.GetWorldCorners(fc); // 0=BL,1=TL,2=TR,3=BR

            Vector3 bl = fc[0];
            Vector3 br = fc[3];
            Vector3 tl = fc[1];

            Vector3 w = br - bl;
            Vector3 h = tl - bl;

            float wLen2 = Vector3.Dot(w, w);
            float hLen2 = Vector3.Dot(h, h);
            if (wLen2 < 1e-6f || hLen2 < 1e-6f) return false;

            var sc = new Vector3[4];
            sourceRt.GetWorldCorners(sc);

            float minX = 999f, minY = 999f;
            float maxX = -999f, maxY = -999f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 rel = sc[i] - bl;
                float nx = Vector3.Dot(rel, w) / wLen2;
                float ny = Vector3.Dot(rel, h) / hLen2;

                minX = Mathf.Min(minX, nx);
                minY = Mathf.Min(minY, ny);
                maxX = Mathf.Max(maxX, nx);
                maxY = Mathf.Max(maxY, ny);
            }

            minX = Mathf.Clamp01(minX);
            minY = Mathf.Clamp01(minY);
            maxX = Mathf.Clamp01(maxX);
            maxY = Mathf.Clamp01(maxY);

            float width = Mathf.Max(0f, maxX - minX);
            float height = Mathf.Max(0f, maxY - minY);
            if (width < 0.001f || height < 0.001f) return false;

            rectNrm = new Rect(minX, minY, width, height);
            return true;
        }

        private static Rect ExpandNormalizedRect(Rect rectNrm, float padLeft, float padRight, float padTop, float padBottom)
        {
            float xMin = Mathf.Clamp01(rectNrm.xMin - padLeft);
            float xMax = Mathf.Clamp01(rectNrm.xMin + rectNrm.width + padRight);
            float yMin = Mathf.Clamp01(rectNrm.yMin - padBottom);
            float yMax = Mathf.Clamp01(rectNrm.yMin + rectNrm.height + padTop);

            float w = Mathf.Max(0f, xMax - xMin);
            float h = Mathf.Max(0f, yMax - yMin);
            return new Rect(xMin, yMin, w, h);
        }

        private bool TryGetImageBackedZoneNormalized(
            RectTransform frameRt,
            Text zoneText,
            out RectTransform zoneBoxRt,
            out Rect zoneNrm)
        {
            zoneBoxRt = null;
            zoneNrm = default;

            if (frameRt == null || zoneText == null || !zoneText.gameObject.activeInHierarchy)
                return false;

            float minAreaFactor = (zoneText == lScale || zoneText == rScale) ? 0.92f : 1.02f;
            var candidateRt = GetEffectBoxRectTransform(zoneText, false, minAreaFactor);
            if (candidateRt == null) return false;

            var img = candidateRt.GetComponent<Image>();
            if (img == null || img.sprite == null) return false;

            if (!TryGetRectNormalizedInFrame(frameRt, candidateRt, out zoneNrm))
                return false;

            if (zoneNrm.width <= 0.001f || zoneNrm.height <= 0.001f)
                return false;

            zoneBoxRt = candidateRt;
            return true;
        }

        private static bool IsLikelyMatchingPendulumZone(Rect fallbackZoneNrm, Rect imageZoneNrm)
        {
            if (fallbackZoneNrm.width <= 0.001f || fallbackZoneNrm.height <= 0.001f ||
                imageZoneNrm.width <= 0.001f || imageZoneNrm.height <= 0.001f)
                return false;

            float widthRatio = imageZoneNrm.width / Mathf.Max(0.0001f, fallbackZoneNrm.width);
            float heightRatio = imageZoneNrm.height / Mathf.Max(0.0001f, fallbackZoneNrm.height);
            if (widthRatio < 0.55f || widthRatio > 1.12f ||
                heightRatio < 0.55f || heightRatio > 1.20f)
                return false;

            float ixMin = Mathf.Max(fallbackZoneNrm.xMin, imageZoneNrm.xMin);
            float iyMin = Mathf.Max(fallbackZoneNrm.yMin, imageZoneNrm.yMin);
            float ixMax = Mathf.Min(fallbackZoneNrm.xMax, imageZoneNrm.xMax);
            float iyMax = Mathf.Min(fallbackZoneNrm.yMax, imageZoneNrm.yMax);

            float iArea = Mathf.Max(0f, ixMax - ixMin) * Mathf.Max(0f, iyMax - iyMin);
            float baseArea = Mathf.Max(0.0001f, Mathf.Min(
                fallbackZoneNrm.width * fallbackZoneNrm.height,
                imageZoneNrm.width * imageZoneNrm.height));

            float overlapRatio = iArea / baseArea;
            float fallbackCx = 0.5f * (fallbackZoneNrm.xMin + fallbackZoneNrm.xMax);
            float imageCx = 0.5f * (imageZoneNrm.xMin + imageZoneNrm.xMax);
            float fallbackCy = 0.5f * (fallbackZoneNrm.yMin + fallbackZoneNrm.yMax);
            float imageCy = 0.5f * (imageZoneNrm.yMin + imageZoneNrm.yMax);

            return overlapRatio >= 0.40f &&
                   Mathf.Abs(fallbackCx - imageCx) <= 0.14f &&
                   Mathf.Abs(fallbackCy - imageCy) <= 0.07f;
        }

        private bool TryGetPendulumParchmentZonesNormalized(
            RectTransform frameRt,
            out Rect centerZoneNrm,
            out Rect leftScaleZoneNrm,
            out Rect rightScaleZoneNrm,
            out Rect fullBandNrm,
            out bool usedImageBoxRects,
            out RectTransform centerZoneBoxRt,
            out RectTransform leftScaleZoneBoxRt,
            out RectTransform rightScaleZoneBoxRt,
            float separatorGapNrm)
        {
            centerZoneNrm = default;
            leftScaleZoneNrm = default;
            rightScaleZoneNrm = default;
            fullBandNrm = default;
            usedImageBoxRects = false;
            centerZoneBoxRt = null;
            leftScaleZoneBoxRt = null;
            rightScaleZoneBoxRt = null;

            if (frameRt == null ||
                cardDescriptionPendulum == null ||
                lScale == null ||
                rScale == null ||
                !cardDescriptionPendulum.gameObject.activeInHierarchy ||
                !lScale.gameObject.activeInHierarchy ||
                !rScale.gameObject.activeInHierarchy ||
                string.IsNullOrEmpty(cardDescriptionPendulum.text))
                return false;

            // 1) Prefer REAL parchment Image rects (same behavior as normal overframe seam clipping).
            // We intentionally bypass explicit override here because pendulum has separate center/scale parchments.
            RectTransform centerBoxRt = null;
            RectTransform leftBoxRt = null;
            RectTransform rightBoxRt = null;
            bool hasCenterBoxRt = TryGetImageBackedZoneNormalized(frameRt, cardDescriptionPendulum, out centerBoxRt, out var centerBoxNrm);
            bool hasLeftBoxRt = TryGetImageBackedZoneNormalized(frameRt, lScale, out leftBoxRt, out var leftBoxNrm);
            bool hasRightBoxRt = TryGetImageBackedZoneNormalized(frameRt, rScale, out rightBoxRt, out var rightBoxNrm);

            if (hasCenterBoxRt && hasLeftBoxRt && hasRightBoxRt)
            {
                float imgLeftX0 = leftBoxNrm.xMin;
                float imgLeftX1 = leftBoxNrm.xMin + leftBoxNrm.width;
                float imgCenterX0 = centerBoxNrm.xMin;
                float imgCenterX1 = centerBoxNrm.xMin + centerBoxNrm.width;
                float imgRightX0 = rightBoxNrm.xMin;
                float imgRightX1 = rightBoxNrm.xMin + rightBoxNrm.width;

                bool ordered =
                    imgLeftX0 < imgLeftX1 &&
                    imgCenterX0 < imgCenterX1 &&
                    imgRightX0 < imgRightX1 &&
                    imgLeftX0 < imgCenterX0 &&
                    imgCenterX1 < imgRightX1;

                // Side scale boxes are expected to be narrower than the center pendulum text parchment.
                bool shapeLooksLikePendulumParchment =
                    leftBoxNrm.width < centerBoxNrm.width &&
                    rightBoxNrm.width < centerBoxNrm.width;

                if (ordered && shapeLooksLikePendulumParchment)
                {
                    float imgYMin = Mathf.Min(centerBoxNrm.yMin, Mathf.Min(leftBoxNrm.yMin, rightBoxNrm.yMin));
                    float imgYMax = Mathf.Max(centerBoxNrm.yMin + centerBoxNrm.height, Mathf.Max(leftBoxNrm.yMin + leftBoxNrm.height, rightBoxNrm.yMin + rightBoxNrm.height));
                    if (imgYMax > imgYMin)
                    {
                        // With real parchment Image rects, use their exact bounds (no midpoint approximation).
                        // The border-safe inner cut is handled later by clip/fade inset logic.
                        leftScaleZoneNrm = leftBoxNrm;
                        centerZoneNrm = centerBoxNrm;
                        rightScaleZoneNrm = rightBoxNrm;
                        centerZoneBoxRt = centerBoxRt;
                        leftScaleZoneBoxRt = leftBoxRt;
                        rightScaleZoneBoxRt = rightBoxRt;
                        float imgBandXMin = Mathf.Min(leftScaleZoneNrm.xMin, Mathf.Min(centerZoneNrm.xMin, rightScaleZoneNrm.xMin));
                        float imgBandXMax = Mathf.Max(leftScaleZoneNrm.xMax, Mathf.Max(centerZoneNrm.xMax, rightScaleZoneNrm.xMax));
                        fullBandNrm = new Rect(imgBandXMin, imgYMin, imgBandXMax - imgBandXMin, imgYMax - imgYMin);
                        usedImageBoxRects = true;
                        return fullBandNrm.width > 0.001f && fullBandNrm.height > 0.001f;
                    }
                }
            }

            // 2) Fallback: infer parchments from text rects + tuned padding.
            if (!TryGetRectNormalizedInFrame(frameRt, cardDescriptionPendulum.rectTransform, out var centerTextNrm) ||
                !TryGetRectNormalizedInFrame(frameRt, lScale.rectTransform, out var leftScaleTextNrm) ||
                !TryGetRectNormalizedInFrame(frameRt, rScale.rectTransform, out var rightScaleTextNrm))
                return false;

            Rect center = ExpandNormalizedRect(
                centerTextNrm,
                OverFramePendulumCenterPadSideNrm,
                OverFramePendulumCenterPadSideNrm,
                OverFramePendulumCenterPadTopNrm,
                OverFramePendulumCenterPadBottomNrm);

            float yMin = center.yMin;
            float yMax = center.yMin + center.height;
            if (yMax <= yMin) return false;

            float leftX0 = Mathf.Clamp01(leftScaleTextNrm.xMin - OverFramePendulumScalePadOuterNrm);
            float rightX1 = Mathf.Clamp01(rightScaleTextNrm.xMin + rightScaleTextNrm.width + OverFramePendulumScalePadOuterNrm);

            // In fallback mode, center text rect is narrower than center parchment.
            // Derive seam boundaries from the scale box inner edges so fade/clip reaches inner separator walls.
            float centerBoundaryLeft = Mathf.Clamp01(leftScaleTextNrm.xMin + leftScaleTextNrm.width + OverFramePendulumScalePadInnerNrm);
            float centerBoundaryRight = Mathf.Clamp01(rightScaleTextNrm.xMin - OverFramePendulumScalePadInnerNrm);
            if (centerBoundaryRight <= centerBoundaryLeft)
                return false;

            float separatorHalfGap = Mathf.Max(0f, separatorGapNrm * 0.5f);
            float leftX1 = Mathf.Clamp01(centerBoundaryLeft - separatorHalfGap);
            float centerX0 = Mathf.Clamp01(centerBoundaryLeft + separatorHalfGap);
            float centerX1 = Mathf.Clamp01(centerBoundaryRight - separatorHalfGap);
            float rightX0 = Mathf.Clamp01(centerBoundaryRight + separatorHalfGap);

            if (leftX1 <= leftX0 || centerX1 <= centerX0 || rightX1 <= rightX0)
                return false;

            leftScaleZoneNrm = new Rect(leftX0, yMin, leftX1 - leftX0, yMax - yMin);
            centerZoneNrm = new Rect(centerX0, yMin, centerX1 - centerX0, yMax - yMin);
            rightScaleZoneNrm = new Rect(rightX0, yMin, rightX1 - rightX0, yMax - yMin);

            // Use REAL parchment Image rects when they clearly match the intended zone role.
            // This prevents accidental "full upper band" picks from collapsing all three zones.
            float roleTolX = 10f / 704f;

            bool centerBoxRoleValid =
                hasCenterBoxRt &&
                centerBoxNrm.xMin >= centerBoundaryLeft - roleTolX &&
                centerBoxNrm.xMax <= centerBoundaryRight + roleTolX &&
                centerBoxNrm.width > 0.04f;

            bool leftBoxRoleValid =
                hasLeftBoxRt &&
                leftBoxNrm.xMax <= centerBoundaryLeft + roleTolX &&
                leftBoxNrm.xMin <= centerBoundaryLeft - roleTolX &&
                leftBoxNrm.width > 0.02f;

            bool rightBoxRoleValid =
                hasRightBoxRt &&
                rightBoxNrm.xMin >= centerBoundaryRight - roleTolX &&
                rightBoxNrm.xMax >= centerBoundaryRight + roleTolX &&
                rightBoxNrm.width > 0.02f;

            if (centerBoxRoleValid)
            {
                centerZoneNrm = centerBoxNrm;
                centerZoneBoxRt = centerBoxRt;
            }

            if (leftBoxRoleValid)
            {
                leftScaleZoneNrm = leftBoxNrm;
                leftScaleZoneBoxRt = leftBoxRt;
            }

            if (rightBoxRoleValid)
            {
                rightScaleZoneNrm = rightBoxNrm;
                rightScaleZoneBoxRt = rightBoxRt;
            }

            // Lock fallback seam boundaries to adjacent real boxes (when present), otherwise to
            // scale-inner fallback boundaries. This removes vertical dead strips at separators.
            float seamLeft = centerZoneBoxRt != null
                ? centerZoneNrm.xMin
                : (leftScaleZoneBoxRt != null ? leftScaleZoneNrm.xMax : centerBoundaryLeft);
            float seamRight = centerZoneBoxRt != null
                ? centerZoneNrm.xMax
                : (rightScaleZoneBoxRt != null ? rightScaleZoneNrm.xMin : centerBoundaryRight);

            seamLeft = Mathf.Clamp01(seamLeft);
            seamRight = Mathf.Clamp01(seamRight);
            if (seamRight <= seamLeft)
                return false;

            if (leftScaleZoneBoxRt == null)
                leftScaleZoneNrm = new Rect(
                    leftScaleZoneNrm.xMin,
                    leftScaleZoneNrm.yMin,
                    Mathf.Max(0f, seamLeft - leftScaleZoneNrm.xMin),
                    leftScaleZoneNrm.height);

            if (centerZoneBoxRt == null)
                centerZoneNrm = new Rect(
                    seamLeft,
                    centerZoneNrm.yMin,
                    Mathf.Max(0f, seamRight - seamLeft),
                    centerZoneNrm.height);

            if (rightScaleZoneBoxRt == null)
                rightScaleZoneNrm = new Rect(
                    seamRight,
                    rightScaleZoneNrm.yMin,
                    Mathf.Max(0f, rightScaleZoneNrm.xMax - seamRight),
                    rightScaleZoneNrm.height);

            if (leftScaleZoneNrm.width <= 0.001f || centerZoneNrm.width <= 0.001f || rightScaleZoneNrm.width <= 0.001f)
                return false;

            usedImageBoxRects = centerZoneBoxRt != null || leftScaleZoneBoxRt != null || rightScaleZoneBoxRt != null;
            float bandXMin = Mathf.Min(leftScaleZoneNrm.xMin, Mathf.Min(centerZoneNrm.xMin, rightScaleZoneNrm.xMin));
            float bandXMax = Mathf.Max(leftScaleZoneNrm.xMax, Mathf.Max(centerZoneNrm.xMax, rightScaleZoneNrm.xMax));
            float bandYMin = Mathf.Min(leftScaleZoneNrm.yMin, Mathf.Min(centerZoneNrm.yMin, rightScaleZoneNrm.yMin));
            float bandYMax = Mathf.Max(leftScaleZoneNrm.yMax, Mathf.Max(centerZoneNrm.yMax, rightScaleZoneNrm.yMax));
            fullBandNrm = new Rect(bandXMin, bandYMin, bandXMax - bandXMin, bandYMax - bandYMin);
            return fullBandNrm.width > 0.001f && fullBandNrm.height > 0.001f;
        }

        private void UpdateMaskedContinuationClip(
            ref RectTransform clipRt,
            ref RectMask2D clipMask,
            ref RawImage artCopy,
            string clipName,
            string artName,
            Rect zoneNrm,
            RectTransform frameRt,
            RectTransform clipParentRt,
            RectTransform artReferenceRt,
            Texture2D tex,
            Rect uvRect,
            OverFrameSpec spec,
            float alpha,
            float insetLeft = 0f,
            float insetRight = 0f,
            float insetTop = 0f,
            float insetBottom = 0f,
            RectTransform parchmentBoxRt = null,
            float borderCutFactor = OverFrameBorderCutFactor,
            float borderSafetyInset = OverFrameBorderSafetyInset)
        {
            bool hasZoneNrm = zoneNrm.width > 0.001f && zoneNrm.height > 0.001f;
            bool hasParchmentBox = parchmentBoxRt != null;
            bool valid =
                tex != null &&
                frameRt != null &&
                clipParentRt != null &&
                artReferenceRt != null &&
                (hasZoneNrm || hasParchmentBox);

            if (!valid)
            {
                if (clipRt != null) clipRt.gameObject.SetActive(false);
                if (artCopy != null) artCopy.gameObject.SetActive(false);
                return;
            }

            if (clipRt == null)
            {
                var go = new GameObject(clipName, typeof(RectTransform), typeof(RectMask2D));
                go.transform.SetParent(clipParentRt, false);
                clipRt = go.GetComponent<RectTransform>();
                clipMask = go.GetComponent<RectMask2D>();
            }
            else if (clipRt.parent != clipParentRt)
            {
                clipRt.SetParent(clipParentRt, false);
            }

            clipRt.gameObject.SetActive(true);

            if (hasParchmentBox)
                MatchRectByWorldCorners(clipRt, parchmentBoxRt, clipParentRt);
            else
                MatchRectToFrameNormalized(clipRt, frameRt, clipParentRt, zoneNrm);

            float borderInsetLeft = 0f;
            float borderInsetRight = 0f;
            float borderInsetTop = 0f;
            float borderInsetBottom = 0f;
            if (hasParchmentBox &&
                TryGetEffectBoxBorderMidWorld(parchmentBoxRt, out var midLeftW, out var midRightW, out var midTopW, out var midBottomW))
            {
                midLeftW *= borderCutFactor;
                midRightW *= borderCutFactor;
                midTopW *= borderCutFactor;
                midBottomW *= borderCutFactor;

                borderInsetLeft = WorldToLocalX(clipRt, midLeftW) + borderSafetyInset;
                borderInsetRight = WorldToLocalX(clipRt, midRightW) + borderSafetyInset;
                borderInsetTop = WorldToLocalY(clipRt, midTopW) + borderSafetyInset;
                borderInsetBottom = WorldToLocalY(clipRt, midBottomW) + borderSafetyInset;
            }

            InsetRect(
                clipRt,
                Mathf.Max(0f, borderInsetLeft + insetLeft),
                Mathf.Max(0f, borderInsetRight + insetRight),
                Mathf.Max(0f, borderInsetTop + insetTop),
                Mathf.Max(0f, borderInsetBottom + insetBottom));

            if (artCopy == null && _overFrameArt != null)
            {
                var clone = Instantiate(_overFrameArt, clipRt);
                clone.name = artName;
                clone.raycastTarget = false;
                artCopy = clone;

                var arf = artCopy.GetComponent<AspectRatioFitter>();
                if (arf) arf.enabled = false;
            }
            else if (artCopy != null && artCopy.transform.parent != clipRt)
            {
                artCopy.transform.SetParent(clipRt, false);
            }

            if (artCopy == null) return;

            artCopy.gameObject.SetActive(true);
            artCopy.texture = tex;
            artCopy.uvRect = uvRect;
            artCopy.color = new Color(1f, 1f, 1f, alpha);
            artCopy.maskable = true;

            MatchRectByWorldCorners(artCopy.rectTransform, artReferenceRt, clipRt);
            artCopy.rectTransform.sizeDelta *= spec.scale;
            artCopy.rectTransform.anchoredPosition += spec.offset;
        }

        private void UpdateGradientFadeZone(
            ref VerticalGradientGraphic fadeGraphic,
            string objectName,
            Rect zoneNrm,
            RectTransform frameRt,
            RectTransform fadeParentRt,
            Color washRgb,
            float topAlpha,
            float bottomAlpha,
            float insetLeft,
            float insetRight,
            float insetTop,
            float insetBottom,
            RectTransform parchmentBoxRt = null,
            float borderCutFactor = OverFrameBorderCutFactor,
            float borderSafetyInset = OverFrameBorderSafetyInset)
        {
            bool hasZoneNrm = zoneNrm.width > 0.001f && zoneNrm.height > 0.001f;
            bool hasParchmentBox = parchmentBoxRt != null;
            bool valid =
                fadeParentRt != null &&
                (hasParchmentBox || (frameRt != null && hasZoneNrm));

            if (!valid)
            {
                if (fadeGraphic != null) fadeGraphic.gameObject.SetActive(false);
                return;
            }

            if (fadeGraphic == null)
            {
                var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalGradientGraphic));
                go.transform.SetParent(fadeParentRt, false);
                fadeGraphic = go.GetComponent<VerticalGradientGraphic>();
                fadeGraphic.raycastTarget = false;
            }
            else if (fadeGraphic.transform.parent != fadeParentRt)
            {
                fadeGraphic.transform.SetParent(fadeParentRt, false);
            }

            fadeGraphic.gameObject.SetActive(true);
            fadeGraphic.topColor = new Color(washRgb.r, washRgb.g, washRgb.b, topAlpha);
            fadeGraphic.bottomColor = new Color(washRgb.r, washRgb.g, washRgb.b, bottomAlpha);

            RectTransform rt = fadeGraphic.rectTransform;
            if (hasParchmentBox)
                MatchRectByWorldCorners(rt, parchmentBoxRt, fadeParentRt);
            else
                MatchRectToFrameNormalized(rt, frameRt, fadeParentRt, zoneNrm);

            float borderInsetLeft = 0f;
            float borderInsetRight = 0f;
            float borderInsetTop = 0f;
            float borderInsetBottom = 0f;
            if (hasParchmentBox &&
                TryGetEffectBoxBorderMidWorld(parchmentBoxRt, out var midLeftW, out var midRightW, out var midTopW, out var midBottomW))
            {
                midLeftW *= borderCutFactor;
                midRightW *= borderCutFactor;
                midTopW *= borderCutFactor;
                midBottomW *= borderCutFactor;

                borderInsetLeft = WorldToLocalX(rt, midLeftW) + borderSafetyInset;
                borderInsetRight = WorldToLocalX(rt, midRightW) + borderSafetyInset;
                borderInsetTop = WorldToLocalY(rt, midTopW) + borderSafetyInset;
                borderInsetBottom = WorldToLocalY(rt, midBottomW) + borderSafetyInset;
            }

            InsetRect(
                rt,
                Mathf.Max(0f, borderInsetLeft + insetLeft),
                Mathf.Max(0f, borderInsetRight + insetRight),
                Mathf.Max(0f, borderInsetTop + insetTop),
                Mathf.Max(0f, borderInsetBottom + insetBottom));
            rt.sizeDelta += new Vector2(OverFrameFadePadSide * 2f, OverFrameFadePadTop + OverFrameFadePadBottom);
            rt.anchoredPosition += new Vector2(0f, (OverFrameFadePadTop - OverFrameFadePadBottom) * 0.5f);
        }

// ────────────────────────────────────────────────
// Effect-box BORDER mid-thickness in WORLD units.
// We use the parchment background sprite's 9-slice border to compute where
// the proxy cuts the art: right in the MIDDLE of the border thickness.
// Returns mid-border distances (left/right/top/bottom) measured in world units.
// ────────────────────────────────────────────────
private bool TryGetEffectBoxBorderMidWorld(
    RectTransform boxRt,
    out float midLeftW,
    out float midRightW,
    out float midTopW,
    out float midBottomW)
{
    midLeftW = midRightW = midTopW = midBottomW = 0f;
    if (boxRt == null) return false;

    var img = boxRt.GetComponent<Image>();
    if (img == null || img.sprite == null) return false;

    var sp = img.sprite;
    Vector4 b = sp.border;
    if (b.x <= 0f && b.y <= 0f && b.z <= 0f && b.w <= 0f)
        return false;

    // Convert sprite border pixels -> local UI units in boxRt
    float sx = (sp.rect.width  > 0.001f) ? (boxRt.rect.width  / sp.rect.width)  : 0f;
    float sy = (sp.rect.height > 0.001f) ? (boxRt.rect.height / sp.rect.height) : 0f;

    float midL = 0.5f * b.x * sx;
    float midR = 0.5f * b.z * sx;
    float midB = 0.5f * b.y * sy;
    float midT = 0.5f * b.w * sy;

    // Local -> world
    var ls = boxRt.lossyScale;
    midLeftW   = Mathf.Abs(ls.x) > 0.0001f ? midL * ls.x : midL;
    midRightW  = Mathf.Abs(ls.x) > 0.0001f ? midR * ls.x : midR;
    midTopW    = Mathf.Abs(ls.y) > 0.0001f ? midT * ls.y : midT;
    midBottomW = Mathf.Abs(ls.y) > 0.0001f ? midB * ls.y : midB;

    // Use absolute distances
    midLeftW   = Mathf.Abs(midLeftW);
    midRightW  = Mathf.Abs(midRightW);
    midTopW    = Mathf.Abs(midTopW);
    midBottomW = Mathf.Abs(midBottomW);

    return true;
}

private bool TryGetEffectBoxBorderMidWorld(out float midLeftW, out float midRightW, out float midTopW, out float midBottomW)
{
    return TryGetEffectBoxBorderMidWorld(GetEffectBoxRectTransform(), out midLeftW, out midRightW, out midTopW, out midBottomW);
}

private static float WorldToLocalX(RectTransform rt, float worldDist)
{
    if (rt == null) return 0f;
    float s = Mathf.Abs(rt.lossyScale.x);
    if (s < 0.0001f) return worldDist;
    return worldDist / s;
}

private static float WorldToLocalY(RectTransform rt, float worldDist)
{
    if (rt == null) return 0f;
    float s = Mathf.Abs(rt.lossyScale.y);
    if (s < 0.0001f) return worldDist;
    return worldDist / s;
}





// ────────────────────────────────────────────────
// Effect-box inner-vs-outer logic
//
// We clip the OverArt continuation to the INNER edge of the parchment border.
// - If the chosen rect includes the border (outer box), we cut inward by border thickness (9-slice if present, else fallback).
// - If the chosen rect is already the INNER fill (border excluded), we do NOT apply border/fallback cut (only tiny filtering safety).
// This prevents the classic "shrunken continuation margins" bug.
// ────────────────────────────────────────────────
private static bool RectApproxEqual(Rect a, Rect b, float tolPos, float tolSize)
{
    return Mathf.Abs(a.xMin - b.xMin) <= tolPos &&
           Mathf.Abs(a.yMin - b.yMin) <= tolPos &&
           Mathf.Abs(a.width - b.width) <= tolSize &&
           Mathf.Abs(a.height - b.height) <= tolSize;
}

private static bool ImageHasSpriteBorder(Image img)
{
    if (img == null || img.sprite == null) return false;
    if (img.type == Image.Type.Sliced) return true;
    var br = img.sprite.border;
    return (br.x > 0f || br.y > 0f || br.z > 0f || br.w > 0f);
}

private bool IsEffectBoxRectInnerFill(RectTransform boxRt, Rect measuredNrm)
{
    // Explicit override from Inspector
    if (overFrameEffectBoxRectMode == OverFrameEffectBoxRectMode.InnerRectAlreadyInset) return true;
    if (overFrameEffectBoxRectMode == OverFrameEffectBoxRectMode.OuterRectIncludesBorder) return false;

    

    // If we couldn't locate the parchment Image at all (common in mods where the textbox is baked into the frame),
    // we must assume our reference rects are already "inner" and avoid the big fallback border inset (which causes
    // the classic "shrunken continuation margins" bug).
    if (boxRt == null) return true;

    // If we match the known inner reference rect, treat as INNER.
    if (RectApproxEqual(measuredNrm, OverFrameEffectBoxInnerNrm, OverFrameOuterRectDetectToleranceNrm, OverFrameOuterRectDetectToleranceNrm))
        return true;
// Auto:
    // 1) If we are close to the known/reference outer parchment rect → treat as OUTER.
    var expectedOuter = OverFrameEffectBoxNrm;
    if (RectApproxEqual(measuredNrm, expectedOuter, OverFrameOuterRectDetectToleranceNrm, OverFrameOuterRectDetectToleranceNrm))
        return false;

    // 2) If the measured rect is clearly contained INSIDE the expected outer box on all sides → treat as INNER.
    bool inside =
        measuredNrm.xMin > expectedOuter.xMin + OverFrameInnerRectDetectMarginNrm &&
        measuredNrm.yMin > expectedOuter.yMin + OverFrameInnerRectDetectMarginNrm &&
        (measuredNrm.xMin + measuredNrm.width) < (expectedOuter.xMin + expectedOuter.width) - OverFrameInnerRectDetectMarginNrm &&
        (measuredNrm.yMin + measuredNrm.height) < (expectedOuter.yMin + expectedOuter.height) - OverFrameInnerRectDetectMarginNrm;

    if (inside)
        return true;

    // 3) If the chosen rect's Image has 9-slice border data, it almost certainly includes the border → OUTER.
    if (boxRt != null)
    {
        var img = boxRt.GetComponent<Image>();
        if (ImageHasSpriteBorder(img))
            return false;
    }

    // Default to OUTER (safer: avoids tinting border lines). If this causes shrink, set mode to InnerRectAlreadyInset.
    return false;
}


        // ────────────────────────────────────────────────
        // Apply fade overlay over the effect-text box region
        // Must be: OverFrameArt → FadeOverlay → Text/UI
        // ────────────────────────────────────────────────
        
        private void ApplyOverFrameEffectBoxFade(bool isPendulum)
        {
            if (!OverFrameEnableWashOverlay)
            {
                // Only fade the artwork layers (continuation), never tint the parchment/text.
                if (_overFrameEffectFadeMaskRt) _overFrameEffectFadeMaskRt.gameObject.SetActive(false);
                if (_overFrameEffectFade) _overFrameEffectFade.gameObject.SetActive(false);
                if (_overFrameEffectFadePendulumC) _overFrameEffectFadePendulumC.gameObject.SetActive(false);
                if (_overFrameEffectFadePendulumL) _overFrameEffectFadePendulumL.gameObject.SetActive(false);
                if (_overFrameEffectFadePendulumR) _overFrameEffectFadePendulumR.gameObject.SetActive(false);
                return;
            }

            var desc = (isPendulum && cardDescription != null && cardDescription.gameObject.activeInHierarchy)
                ? cardDescription
                : GetActiveOcgDescriptionText();
            if (desc == null) return;

            // IMPORTANT: parent fade in the SAME UI layer as the description text
            // (this is the layer that is visible above the artwork).
            var fadeParentRt = desc.transform.parent as RectTransform;
            if (fadeParentRt == null) return;

            // "Parchment" wash tone (used only for the overlay, never for the text itself)
            Color washRgb = OverFrameFadeDebugMagenta
                ? new Color(1f, 0f, 1f, 1f)
                : new Color(0.93f, 0.86f, 0.74f, 1f);

            float topA = OverFrameFadeDebugMagenta
                ? 1f
                : (isPendulum ? OverFramePendulumFadeTopAlpha : OverFrameFadeTopAlpha);
            float botA = OverFrameFadeDebugMagenta
                ? 1f
                : (isPendulum ? OverFramePendulumFadeBottomAlpha : OverFrameFadeBottomAlpha);

            if (isPendulum &&
                cardFrame != null &&
                cardFrame.gameObject.activeInHierarchy &&
                TryGetPendulumParchmentZonesNormalized(
                    cardFrame.rectTransform,
                    out var pendCenterNrm,
                    out var pendLeftNrm,
                    out var pendRightNrm,
                    out _,
                    out _,
                    out var pendCenterBoxRt,
                    out var pendLeftBoxRt,
                    out var pendRightBoxRt,
                    OverFramePendulumSeparatorGapFadeNrm))
            {
                if (_overFrameEffectFadeMaskRt != null)
                    _overFrameEffectFadeMaskRt.gameObject.SetActive(false);

                float centerFadeInsetSide = OverFramePendulumCenterFadeInsetSide;
                float centerFadeInsetTop = OverFramePendulumCenterFadeInsetTop;
                float centerFadeInsetBottom = OverFramePendulumCenterFadeInsetBottom;
                float scaleFadeInsetOuter = OverFramePendulumScaleFadeInsetOuter;
                float scaleFadeInsetInner = OverFramePendulumScaleFadeInsetInner;
                float scaleFadeInsetTop = OverFramePendulumScaleFadeInsetTop;
                float scaleFadeInsetBottom = OverFramePendulumScaleFadeInsetBottom;
                float fallbackFadeInsetSide = OverFramePendulumZoneFallbackFadeInsetSide;
                float fallbackFadeInsetTop = OverFramePendulumZoneFallbackFadeInsetTop;
                float fallbackFadeInsetBottom = OverFramePendulumZoneFallbackFadeInsetBottom;

                if (pendCenterBoxRt == null)
                {
                    centerFadeInsetSide += fallbackFadeInsetSide;
                    centerFadeInsetTop += fallbackFadeInsetTop;
                    centerFadeInsetBottom += fallbackFadeInsetBottom;
                }

                if (pendLeftBoxRt == null)
                {
                    scaleFadeInsetOuter += fallbackFadeInsetSide;
                    scaleFadeInsetInner += fallbackFadeInsetSide;
                    scaleFadeInsetTop += fallbackFadeInsetTop;
                    scaleFadeInsetBottom += fallbackFadeInsetBottom;
                }

                if (pendRightBoxRt == null)
                {
                    scaleFadeInsetOuter += fallbackFadeInsetSide;
                    scaleFadeInsetInner += fallbackFadeInsetSide;
                    scaleFadeInsetTop += fallbackFadeInsetTop;
                    scaleFadeInsetBottom += fallbackFadeInsetBottom;
                }

                UpdateGradientFadeZone(
                    ref _overFrameEffectFadePendulumC,
                    "OverFrameEffectFadePendulumC",
                    pendCenterNrm,
                    cardFrame.rectTransform,
                    fadeParentRt,
                    washRgb,
                    topA,
                    botA,
                    centerFadeInsetSide,
                    centerFadeInsetSide,
                    centerFadeInsetTop,
                    centerFadeInsetBottom,
                    pendCenterBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);

                UpdateGradientFadeZone(
                    ref _overFrameEffectFadePendulumL,
                    "OverFrameEffectFadePendulumL",
                    pendLeftNrm,
                    cardFrame.rectTransform,
                    fadeParentRt,
                    washRgb,
                    topA,
                    botA,
                    scaleFadeInsetOuter,
                    scaleFadeInsetInner,
                    scaleFadeInsetTop,
                    scaleFadeInsetBottom,
                    pendLeftBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);

                UpdateGradientFadeZone(
                    ref _overFrameEffectFadePendulumR,
                    "OverFrameEffectFadePendulumR",
                    pendRightNrm,
                    cardFrame.rectTransform,
                    fadeParentRt,
                    washRgb,
                    topA,
                    botA,
                    scaleFadeInsetInner,
                    scaleFadeInsetOuter,
                    scaleFadeInsetTop,
                    scaleFadeInsetBottom,
                    pendRightBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);

                int fadeIdx = Mathf.Clamp(desc.transform.GetSiblingIndex(), 0, fadeParentRt.childCount - 1);
                if (_overFrameEffectFadePendulumC) _overFrameEffectFadePendulumC.rectTransform.SetSiblingIndex(fadeIdx);
                if (_overFrameEffectFadePendulumL) _overFrameEffectFadePendulumL.rectTransform.SetSiblingIndex(fadeIdx);
                if (_overFrameEffectFadePendulumR) _overFrameEffectFadePendulumR.rectTransform.SetSiblingIndex(fadeIdx);
            }
            else
            {
                if (_overFrameEffectFadePendulumC) _overFrameEffectFadePendulumC.gameObject.SetActive(false);
                if (_overFrameEffectFadePendulumL) _overFrameEffectFadePendulumL.gameObject.SetActive(false);
                if (_overFrameEffectFadePendulumR) _overFrameEffectFadePendulumR.gameObject.SetActive(false);
            }

            bool hasExplicitEffectBox =
                overFrameEffectBoxImage != null &&
                overFrameEffectBoxImage.gameObject != null &&
                overFrameEffectBoxImage.gameObject.activeInHierarchy;

            var boxRt = hasExplicitEffectBox ? overFrameEffectBoxImage.rectTransform : GetEffectBoxRectTransform(desc);
            if (boxRt == null) return;

            // If we can, use the parchment background sprite itself as the wash overlay.
            // This looks like the proxy (corners/edges match) and avoids a flat gray rectangle.
            Image boxImg = hasExplicitEffectBox ? overFrameEffectBoxImage : boxRt.GetComponent<Image>();
            // Sprite-mask wash is only reliable when we know the explicit parchment Image.
            bool canMaskBySprite = hasExplicitEffectBox && boxImg != null && boxImg.sprite != null && OverFramePreferSpriteWash;

            if (canMaskBySprite)
            {
                if (_overFrameEffectFadeMaskRt == null)
                {
                    var go = new GameObject("OverFrameEffectFadeMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
                    go.transform.SetParent(fadeParentRt, false);

                    _overFrameEffectFadeMaskRt = go.GetComponent<RectTransform>();
                    _overFrameEffectFadeMaskImg = go.GetComponent<Image>();
                    _overFrameEffectFadeMask = go.GetComponent<Mask>();

                    _overFrameEffectFadeMaskImg.raycastTarget = false;
                }
                else if (_overFrameEffectFadeMaskRt.parent != fadeParentRt)
                {
                    _overFrameEffectFadeMaskRt.SetParent(fadeParentRt, false);
                }

                _overFrameEffectFadeMaskRt.gameObject.SetActive(true);

                // Draw the parchment sprite as the wash (and also use it as the stencil)
                _overFrameEffectFadeMask.showMaskGraphic = true;
                _overFrameEffectFadeMaskImg.sprite = boxImg.sprite;
                _overFrameEffectFadeMaskImg.type = boxImg.type;
                _overFrameEffectFadeMaskImg.preserveAspect = boxImg.preserveAspect;
                _overFrameEffectFadeMaskImg.fillCenter = boxImg.fillCenter;

                float washAlpha = isPendulum ? OverFramePendulumWashSpriteAlpha : OverFrameWashSpriteAlpha;
                _overFrameEffectFadeMaskImg.color = new Color(washRgb.r, washRgb.g, washRgb.b, washAlpha);

                // If an old gradient exists from earlier builds, disable it so ONLY the art is faded.
                if (_overFrameEffectFade != null)
                    _overFrameEffectFade.gameObject.SetActive(false);
            }
            else
            {
                // Fallback: gradient quad (rectangular)
                if (_overFrameEffectFadeMaskRt != null)
                    _overFrameEffectFadeMaskRt.gameObject.SetActive(false);

                if (_overFrameEffectFade == null)
                {
                    var go = new GameObject("OverFrameEffectFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalGradientGraphic));
                    go.transform.SetParent(fadeParentRt, false);
                    _overFrameEffectFade = go.GetComponent<VerticalGradientGraphic>();
                    _overFrameEffectFade.raycastTarget = false;
                }
                else if (_overFrameEffectFade.transform.parent != fadeParentRt)
                {
                    _overFrameEffectFade.transform.SetParent(fadeParentRt, false);
                }

                _overFrameEffectFade.gameObject.SetActive(true);

                // stretch
                var rt = _overFrameEffectFade.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;

                _overFrameEffectFade.topColor = new Color(washRgb.r, washRgb.g, washRgb.b, topA);
                _overFrameEffectFade.bottomColor = new Color(washRgb.r, washRgb.g, washRgb.b, botA);
            }

            // Position the wash overlay to match the EFFECT BOX bounds.
            RectTransform targetRt = canMaskBySprite ? _overFrameEffectFadeMaskRt : _overFrameEffectFade.rectTransform;

            if (cardFrame != null && cardFrame.gameObject.activeInHierarchy)
            {
                Rect boxNrm = OverFrameEffectBoxNrm;
                if (isPendulum &&
                    desc == cardDescriptionPendulum &&
                    TryGetRectNormalizedInFrame(cardFrame.rectTransform, boxRt, out var pendBoxNrm))
                {
                    boxNrm = ExpandNormalizedRect(
                        pendBoxNrm,
                        OverFramePendulumCenterPadSideNrm,
                        OverFramePendulumCenterPadSideNrm,
                        OverFramePendulumCenterPadTopNrm,
                        OverFramePendulumCenterPadBottomNrm);
                }
                else if (hasExplicitEffectBox && TryGetEffectBoxNormalizedRect(cardFrame.rectTransform, out var dynBoxNrm))
                {
                    boxNrm = dynBoxNrm;
                }

                MatchRectToFrameNormalized(targetRt, cardFrame.rectTransform, fadeParentRt, boxNrm);
            }
            else
            {
                MatchRectByWorldCorners(targetRt, boxRt, fadeParentRt);
            }

            // Inset so we don't tint the parchment border line
            float fadeInsetSide = isPendulum ? OverFramePendulumFadeInsetSide : OverFrameFadeInsetSide;
            float fadeInsetTop = isPendulum ? OverFramePendulumFadeInsetTop : OverFrameFadeInsetTop;
            float fadeInsetBottom = isPendulum ? OverFramePendulumFadeInsetBottom : OverFrameFadeInsetBottom;
            InsetRect(targetRt, fadeInsetSide, fadeInsetSide, fadeInsetTop, fadeInsetBottom);

            // Expand to match proxy region feel
            targetRt.sizeDelta += new Vector2(OverFrameFadePadSide * 2f, OverFrameFadePadTop + OverFrameFadePadBottom);
            targetRt.anchoredPosition += new Vector2(0f, (OverFrameFadePadTop - OverFrameFadePadBottom) * 0.5f);

            // Guarantee the wash is above the continuation art, but below the text (text is pushed last after this call).
            targetRt.SetAsLastSibling();
        }


        // Moves the parchment top border UP by this many pixels (frame ref height = 1024).
        // Increase this if the hard cut happens BELOW the border.
        private const float OverFrameParchmentTopRaisePx = 8f; // try 4, 8, 12...
        private const float OverFrameRefH = 1024f;


        // Effect box (parchment incl. border) rect inside card_frame01.png (704x1024)
        // Measured bounds: x=[52..652], y=[767..960] in image pixels (y=0 at top)

        // Upper region = everything above the parchment box (hard cut)
        private static readonly Rect OverFrameUpperAreaNrm = new Rect(
            0f,
            0.25097656f + (OverFrameParchmentTopRaisePx / OverFrameRefH),
            1f,
            0.74902344f - (OverFrameParchmentTopRaisePx / OverFrameRefH) // keep yMin+height = 1
        );

        private static readonly Rect OverFrameEffectBoxNrm = new Rect(
            0.05965909f, // xMin = 42 / 704
            0.05859375f, // yMin = 60 / 1024
            0.88068182f, // width = 620 / 704
            0.19238281f + (OverFrameParchmentTopRaisePx / OverFrameRefH) // height = 197 / 1024 (+ top raise)
        );

        
        // Inner parchment rectangle (EXCLUDES the orange border lines). Measured from card_frame01.png (704x1024).
        // This is the exact "stop here" region for OverArt continuation so border lines stay untouched (proxy 1:1).
        private static readonly Rect OverFrameEffectBoxInnerNrm = new Rect(
            0.07102273f,  // xMin = 50 / 704
            0.06152344f,  // yMin = 63 / 1024  (bottom origin)
            0.86647727f,  // width = 610 / 704 (50..660)
            0.19726562f   // height = 202 / 1024 (759..961 from top)
        );

        // Sets dst RectTransform to a world-space BL/TR rectangle, expressed in dstParent space.
        private static void SetRectByWorldBLTR(RectTransform dst, Vector3 worldBL, Vector3 worldTR, RectTransform dstParent)
        {
            var bl = (Vector3)dstParent.InverseTransformPoint(worldBL);
            var tr = (Vector3)dstParent.InverseTransformPoint(worldTR);

            var size = new Vector2(tr.x - bl.x, tr.y - bl.y);
            var center = new Vector2(bl.x + size.x * 0.5f, bl.y + size.y * 0.5f);

            dst.anchorMin = dst.anchorMax = new Vector2(0.5f, 0.5f);
            dst.pivot = new Vector2(0.5f, 0.5f);
            dst.localScale = Vector3.one;
            dst.localRotation = Quaternion.identity;

            dst.sizeDelta = size;
            dst.anchoredPosition = center;
        }

        // ────────────────────────────────────────────────
        // Proxy split tuning knobs
        // ────────────────────────────────────────────────

        // Faint continuation strength inside the parchment box
        // (you already have OverFrameTextArtAlpha; tweak here if desired)
        private const float OverFrameTextArtAlpha = 0.36f;
        private const float OverFramePendulumTextArtAlpha = 0.36f;
        private const float OverFramePendulumUpperZoneArtAlpha = OverFramePendulumTextArtAlpha;
        private const bool OverFramePendulumUpperUseBaseArtContinuation = false;

        // Because the overlay is rendered ABOVE the frame sprite, we must keep the entire
        // border thickness clear (otherwise the artwork tints/overlaps the border line).
        // 1.0 = cut in the middle of the border, 2.0 = cut at the inner edge (full border).
        private const float OverFrameBorderCutFactor = 2.0f;
        private const float OverFramePendulumUpperZoneBorderCutFactor = 2.00f;
        private const float OverFramePendulumUpperZoneBorderSafetyInset = 0.25f;

        // If the parchment background sprite has no 9-slice border data,
        // we fall back to a safe local-unit border inset so the artwork never touches the border line.
        private const float OverFrameFallbackBorderInset = 13.0f;

        // When the chosen effect-box RectTransform already represents the INNER fill (border excluded),
        // we only apply a tiny inset to avoid texture filtering/half-pixel bleed.
        private const float OverFrameInnerRectNoBorderInset = 0.15f;
        private const float OverFramePendulumInnerRectNoBorderInset = 0.20f;

        // Inner-rect variant for the upper clip nudge (usually 0; the rect is already on the inner edge).
        private const float OverFrameUpperClipInsetBottomInner = 0.0f;
        private const float OverFramePendulumUpperClipInsetBottomInner = 0.35f;

        // Pendulum center text rect expansion in normalized frame space (704x1024 reference).
        // This maps the center parchment box boundaries from the text rect without touching separator lines.
        private const float OverFramePendulumCenterPadSideNrm = 8.5f / 704f;
        private const float OverFramePendulumCenterPadTopNrm = 8.5f / 1024f;
        private const float OverFramePendulumCenterPadBottomNrm = 8f / 1024f;
        // Pendulum scale parchment widths inferred from the scale text rects.
        private const float OverFramePendulumScalePadOuterNrm = 9f / 704f;
        private const float OverFramePendulumScalePadInnerNrm = 2f / 704f;
        // Keep separate separator dead-zones for wash-fade vs hard-clip:
        // - Fade gap: small, so wash reaches close to inner parchment walls.
        // - Clip gap: larger, so art clips hard on delimiter seams.
        private const float OverFramePendulumSeparatorGapFadeNrm = 0.00f / 704f;
        private const float OverFramePendulumSeparatorGapClipNrm = 0.35f / 704f;
        private const float OverFramePendulumUpperFallbackSeamRaiseNrm = 4.20f / 1024f;

        // Auto-detection thresholds in normalized frame space (0..1).
        // These only matter in Auto mode and are generous to handle minor prefab/layout differences.
        private const float OverFrameOuterRectDetectToleranceNrm = 0.010f;
        private const float OverFrameInnerRectDetectMarginNrm = 0.008f;

        // Inset the clips so we DON'T tint the orange/gold border line.
        // Units are in UI local units (works regardless of canvas scaling).
        //
        // IMPORTANT:
        // - The border thickness itself comes from the parchment sprite's 9-slice border (TryGetEffectBoxBorderMidWorld + OverFrameBorderCutFactor).
        // - These values are EXTRA "safety" insets added on top, to avoid any texture filtering / half-pixel bleed on the border lines.
        private const float OverFrameUpperClipInsetBottom = 1.20f;  // pushes the upper hard-stop slightly ABOVE the border line
        private const float OverFrameBorderSafetyInset    = 0.75f;  // extra shrink to keep border lines perfectly clean

        // Extra insets for the continuation clip inside the parchment (usually keep at 0 and tune safety above).
        private const float OverFrameTextClipInsetLeft = 0.0f;
        private const float OverFrameTextClipInsetRight = 0.0f;
        private const float OverFrameTextClipInsetTop = 0.0f;
        private const float OverFrameTextClipInsetBottom = -1.35f;
        private const float OverFramePendulumTextClipInsetBottom = -2.10f;

        // Side continuation clip insets (preserve outer card border + parchment frame line)
        private const float OverFrameSideClipInsetOuter  = 0.0f;
        private const float OverFrameSideClipInsetInner  = 0.0f;
        private const float OverFrameSideClipInsetTop    = 0.0f;
        private const float OverFrameSideClipInsetBottom = 0.0f;
        private const float OverFrameSideContinuationSafetyInset = 0.00f;
        private const float OverFramePendulumSideClipInsetOuter = 0.00f;
        private const float OverFramePendulumSideContinuationSafetyInset = 0.00f;
        // Tiny overlap between split clips to hide 1px seam gaps at left/right.
        private const float OverFrameSplitSeamOverlap = 0.90f;
        private const float OverFramePendulumSplitSeamOverlap = 0.90f;
        // Optional: also inset the parchment wash overlay so it doesn't tint the border
        private const float OverFrameFadeInsetSide = 0.00f;
        private const float OverFrameFadeInsetTop = 0.00f;
        private const float OverFrameFadeInsetBottom = -0.20f;
        private const float OverFramePendulumFadeInsetSide = 0.70f;
        private const float OverFramePendulumFadeInsetTop = 0.10f;
        private const float OverFramePendulumFadeInsetBottom = 0.75f;
        // Upper pendulum fade boxes (center + scales) are tuned separately from lower effect-box fade.
        private const float OverFramePendulumCenterFadeInsetSide = 0.10f;
        private const float OverFramePendulumCenterFadeInsetTop = 0.25f;
        private const float OverFramePendulumCenterFadeInsetBottom = 0.00f;
        private const float OverFramePendulumScaleFadeInsetOuter = 0.60f;
        private const float OverFramePendulumScaleFadeInsetInner = 0.10f;
        private const float OverFramePendulumScaleFadeInsetTop = 0.25f;
        private const float OverFramePendulumScaleFadeInsetBottom = 0.00f;
        // Upper pendulum parchments (center/left-scale/right-scale) use separate clips.
        // Keep center and scale seam trims separate so narrow scale boxes don't get over-masked.
        private const float OverFramePendulumCenterClipInsetSide = 0.20f;
        private const float OverFramePendulumCenterClipInsetTop = 0.45f;
        private const float OverFramePendulumCenterClipInsetBottom = 0.00f;
        private const float OverFramePendulumScaleClipInsetOuter = 0.60f;
        private const float OverFramePendulumScaleClipInsetInner = 0.20f;
        private const float OverFramePendulumScaleClipInsetTop = 0.45f;
        private const float OverFramePendulumScaleClipInsetBottom = 0.00f;
        // Applied when a pendulum upper zone has no dedicated parchment Image rect.
        // Split fade vs clip so fade can reach inner walls while clip stays hard/stable.
        private const float OverFramePendulumZoneFallbackFadeInsetSide = 0.00f;
        private const float OverFramePendulumZoneFallbackFadeInsetTop = 0.00f;
        private const float OverFramePendulumZoneFallbackFadeInsetBottom = 0.00f;
        private const float OverFramePendulumZoneFallbackClipInsetSide = 0.00f;
        private const float OverFramePendulumZoneFallbackClipInsetTop = 0.00f;
        private const float OverFramePendulumZoneFallbackClipInsetBottom = 0.00f;

        private static void InsetRect(RectTransform rt, float left, float right, float top, float bottom)
        {
            if (rt == null) return;

            // shrink
            var sd = rt.sizeDelta;
            sd.x = Mathf.Max(0f, sd.x - (left + right));
            sd.y = Mathf.Max(0f, sd.y - (top + bottom));
            rt.sizeDelta = sd;

            // re-center (positive bottom moves center up; positive left moves center right)
            rt.anchoredPosition += new Vector2((left - right) * 0.5f, (bottom - top) * 0.5f);
        }

        // Matches dst to a normalized sub-rect INSIDE the frame rect (frameRt), using world corners (mask/canvas safe).
        private static void MatchRectToFrameNormalized(RectTransform dst, RectTransform frameRt, RectTransform dstParent, Rect nrm)
        {
            var c = new Vector3[4];
            frameRt.GetWorldCorners(c); // 0=BL,1=TL,2=TR,3=BR

            Vector3 bl = c[0];
            Vector3 br = c[3];
            Vector3 tl = c[1];

            Vector3 widthVec = br - bl;
            Vector3 heightVec = tl - bl;

            Vector3 worldBL = bl + widthVec * nrm.xMin + heightVec * nrm.yMin;
            Vector3 worldTR = bl + widthVec * (nrm.xMin + nrm.width) + heightVec * (nrm.yMin + nrm.height);

            SetRectByWorldBLTR(dst, worldBL, worldTR, dstParent);
        }

        private bool ApplyOverFrameProxySplit(Texture2D tex, RawImage baseArt, OverFrameSpec spec, RectTransform frameRt, Transform anchorParent, RectTransform anchorParentRt, bool isPendulum)
        {
            if (tex == null || frameRt == null || anchorParent == null || anchorParentRt == null)
                return false;

            // Full-card overframe art should track the card frame bounds (same behavior as normal overframe path).
            RectTransform splitArtReferenceRt = frameRt;
            float splitSeamOverlap = isPendulum ? OverFramePendulumSplitSeamOverlap : OverFrameSplitSeamOverlap;

            // Prefer stable reference bounds unless an explicit parchment Image is assigned.
            // This avoids auto-detect picking text/padding rects that shrink the mask width.
            bool hasExplicitEffectBox =
                overFrameEffectBoxImage != null &&
                overFrameEffectBoxImage.gameObject != null &&
                overFrameEffectBoxImage.gameObject.activeInHierarchy;

            // Dynamically derive the parchment/effect-box bounds.
            Rect effectBoxNrm = OverFrameEffectBoxNrm;
            Rect effectBoxInnerNrm = OverFrameEffectBoxInnerNrm;
            var realBoxRt = hasExplicitEffectBox ? overFrameEffectBoxImage.rectTransform : null;
            bool effectBoxIsInnerFill;
            Rect pendCenterNrm = default;
            Rect pendLeftScaleNrm = default;
            Rect pendRightScaleNrm = default;
            Rect pendFullBandNrm = default;
            bool pendUsedImageBoxRects = false;
            RectTransform pendCenterBoxRt = null;
            RectTransform pendLeftBoxRt = null;
            RectTransform pendRightBoxRt = null;
            bool hasPendulumParchmentZones = isPendulum &&
                                             TryGetPendulumParchmentZonesNormalized(
                                                 frameRt,
                                                 out pendCenterNrm,
                                                 out pendLeftScaleNrm,
                                                 out pendRightScaleNrm,
                                                 out pendFullBandNrm,
                                                 out pendUsedImageBoxRects,
                                                 out pendCenterBoxRt,
                                                 out pendLeftBoxRt,
                                                 out pendRightBoxRt,
                                                 OverFramePendulumSeparatorGapClipNrm);

            if (hasPendulumParchmentZones)
            {
                effectBoxNrm = pendCenterNrm;
                effectBoxInnerNrm = effectBoxNrm;
                effectBoxIsInnerFill = true;
                realBoxRt = null;
            }
            else if (isPendulum &&
                     cardDescriptionPendulum != null &&
                     cardDescriptionPendulum.gameObject.activeInHierarchy &&
                     !string.IsNullOrEmpty(cardDescriptionPendulum.text) &&
                     TryGetRectNormalizedInFrame(frameRt, cardDescriptionPendulum.rectTransform, out var pendCenterFallbackNrm))
            {
                effectBoxNrm = ExpandNormalizedRect(
                    pendCenterFallbackNrm,
                    OverFramePendulumCenterPadSideNrm,
                    OverFramePendulumCenterPadSideNrm,
                    OverFramePendulumCenterPadTopNrm,
                    OverFramePendulumCenterPadBottomNrm);
                effectBoxInnerNrm = effectBoxNrm;
                effectBoxIsInnerFill = true;
                realBoxRt = null;
            }
            else
            {
                Rect dynEffectBoxNrm = default;
                bool haveDynEffectBox = false;
                if (hasExplicitEffectBox)
                    haveDynEffectBox = TryGetEffectBoxNormalizedRect(frameRt, out dynEffectBoxNrm);
                if (haveDynEffectBox)
                    effectBoxNrm = dynEffectBoxNrm;

                // IMPORTANT: clip using explicit parchment Image when available.
                // Decide whether that rect is OUTER (includes border) or already INNER (border excluded).
                effectBoxIsInnerFill = IsEffectBoxRectInnerFill(realBoxRt, effectBoxNrm);
                if (realBoxRt == null) effectBoxIsInnerFill = true;
            }

            float clipSafetyInset = effectBoxIsInnerFill
                ? (isPendulum ? OverFramePendulumInnerRectNoBorderInset : OverFrameInnerRectNoBorderInset)
                : OverFrameBorderSafetyInset;

            float yCut;
            if (hasPendulumParchmentZones)
            {
                float pendBandTop = pendFullBandNrm.yMin + pendFullBandNrm.height;
                float fallbackSeamRaise = !pendUsedImageBoxRects ? OverFramePendulumUpperFallbackSeamRaiseNrm : 0f;
                yCut = Mathf.Clamp01(pendBandTop + fallbackSeamRaise);

                // Keep fallback pendulum clip zones touching the moved seam.
                // Without this, raising yCut can leave a thin uncovered strip near upper separators.
                if (fallbackSeamRaise > 0f)
                {
                    float raisedTop = yCut;

                    if (raisedTop > (pendCenterNrm.yMin + pendCenterNrm.height))
                        pendCenterNrm = new Rect(
                            pendCenterNrm.xMin,
                            pendCenterNrm.yMin,
                            pendCenterNrm.width,
                            Mathf.Max(0f, raisedTop - pendCenterNrm.yMin));

                    if (raisedTop > (pendLeftScaleNrm.yMin + pendLeftScaleNrm.height))
                        pendLeftScaleNrm = new Rect(
                            pendLeftScaleNrm.xMin,
                            pendLeftScaleNrm.yMin,
                            pendLeftScaleNrm.width,
                            Mathf.Max(0f, raisedTop - pendLeftScaleNrm.yMin));

                    if (raisedTop > (pendRightScaleNrm.yMin + pendRightScaleNrm.height))
                        pendRightScaleNrm = new Rect(
                            pendRightScaleNrm.xMin,
                            pendRightScaleNrm.yMin,
                            pendRightScaleNrm.width,
                            Mathf.Max(0f, raisedTop - pendRightScaleNrm.yMin));

                    if (raisedTop > (effectBoxNrm.yMin + effectBoxNrm.height))
                    {
                        effectBoxNrm = new Rect(
                            effectBoxNrm.xMin,
                            effectBoxNrm.yMin,
                            effectBoxNrm.width,
                            Mathf.Max(0f, raisedTop - effectBoxNrm.yMin));
                        effectBoxInnerNrm = effectBoxNrm;
                    }

                    if (raisedTop > (pendFullBandNrm.yMin + pendFullBandNrm.height))
                        pendFullBandNrm = new Rect(
                            pendFullBandNrm.xMin,
                            pendFullBandNrm.yMin,
                            pendFullBandNrm.width,
                            Mathf.Max(0f, raisedTop - pendFullBandNrm.yMin));
                }
            }
            else
            {
                yCut = Mathf.Clamp01(effectBoxNrm.yMin + effectBoxNrm.height);
            }
            Rect upperAreaNrm = new Rect(0f, yCut, 1f, Mathf.Max(0f, 1f - yCut));


            // Compute mid-border thickness from the parchment sprite so clips land exactly
            // in the middle of the border (proxy 1:1) without negative insets / bleeding.
            float midLeftW = 0f, midRightW = 0f, midTopW = 0f, midBottomW = 0f;
            bool hasBorder = TryGetEffectBoxBorderMidWorld(out midLeftW, out midRightW, out midTopW, out midBottomW);

            // Cut away the FULL border thickness (not just half) so the frame's border line stays visible.
            if (hasBorder)
            {
                midLeftW   *= OverFrameBorderCutFactor;
                midRightW  *= OverFrameBorderCutFactor;
                midTopW    *= OverFrameBorderCutFactor;
                midBottomW *= OverFrameBorderCutFactor;
            }

            // ── A) MAIN CLIP (upper region only) ─────────────────────────────
            if (_overFrameMainClipRt == null)
            {
                var go = new GameObject("OverFrameMainClip", typeof(RectTransform), typeof(RectMask2D));
                go.transform.SetParent(anchorParent, false);
                _overFrameMainClipRt = go.GetComponent<RectTransform>();
                _overFrameMainClip = go.GetComponent<RectMask2D>();
            }
            else if (_overFrameMainClipRt.parent != anchorParent)
            {
                _overFrameMainClipRt.SetParent(anchorParent, false);
            }

            _overFrameMainClipRt.gameObject.SetActive(true);

            // Clip area = everything above parchment
            MatchRectToFrameNormalized(_overFrameMainClipRt, frameRt, anchorParentRt, upperAreaNrm);

            // Inset so we don't tint the parchment border line
            float upperClipInsetBottomInner = isPendulum ? OverFramePendulumUpperClipInsetBottomInner : OverFrameUpperClipInsetBottomInner;
            float mainCutLocal =
                (effectBoxIsInnerFill ? 0f : (hasBorder ? WorldToLocalY(_overFrameMainClipRt, midTopW) : OverFrameFallbackBorderInset))
                + (effectBoxIsInnerFill ? upperClipInsetBottomInner : OverFrameUpperClipInsetBottom)
                + clipSafetyInset; // keep the top parchment border line perfectly clean // keep the top parchment border line perfectly clean
            InsetRect(_overFrameMainClipRt, 0f, 0f, 0f, mainCutLocal);

            // Parent main art under the clip so it hard-stops at the parchment border
            if (_overFrameArt != null && _overFrameArt.transform.parent != _overFrameMainClipRt)
                _overFrameArt.transform.SetParent(_overFrameMainClipRt, false);

            // RectMask2D only affects MaskableGraphic
            if (_overFrameArt != null) _overFrameArt.maskable = true;

            // Align main art to full frame (then it gets clipped)
            if (_overFrameArt != null)
            {
                MatchRectByWorldCorners(_overFrameArt.rectTransform, splitArtReferenceRt, _overFrameMainClipRt);
                _overFrameArt.rectTransform.sizeDelta *= spec.scale;
                _overFrameArt.rectTransform.anchoredPosition += spec.offset;
            }

            
            // ── A2) SIDE CONTINUATION (outside parchment box) ────────────────
            // Proxy shows the artwork also in the left/right bottom margins (outside the text box).
            // Without this, those zones stay "empty" (the red-circled missing areas in your screenshots).
            float boxX0 = effectBoxNrm.xMin;
            float boxX1 = effectBoxNrm.xMin + effectBoxNrm.width;
            float boxY1 = effectBoxNrm.yMin + effectBoxNrm.height;
            // Continue side gutters from card bottom to the current split seam.
            // For pendulum cards this avoids empty left/right zones below the lower parchment box.
            float sideY0 = 0f;
            float sideY1 = boxY1;
            float sideH = Mathf.Max(0f, sideY1 - sideY0);

            Rect leftSideNrm;
            Rect rightSideNrm;
            if (hasPendulumParchmentZones)
            {
                float leftOuterW = Mathf.Max(0f, pendLeftScaleNrm.xMin);
                float rightOuterX = pendRightScaleNrm.xMin + pendRightScaleNrm.width;
                leftSideNrm = new Rect(0f, sideY0, leftOuterW, sideH);
                rightSideNrm = new Rect(rightOuterX, sideY0, Mathf.Max(0f, 1f - rightOuterX), sideH);
            }
            else
            {
                leftSideNrm = new Rect(0f, sideY0, Mathf.Max(0f, boxX0), sideH);
                rightSideNrm = new Rect(boxX1, sideY0, Mathf.Max(0f, 1f - boxX1), sideH);
            }

            Texture sideTex = tex;
            Rect sideUv = _overFrameArt != null ? _overFrameArt.uvRect : new Rect(0f, 0f, 1f, 1f);

            // Left clip
            if (leftSideNrm.width > 0.001f && leftSideNrm.height > 0.001f)
            {
                if (_overFrameSideClipL_Rt == null)
                {
                    var go = new GameObject("OverFrameSideClipL", typeof(RectTransform), typeof(RectMask2D));
                    go.transform.SetParent(anchorParent, false);
                    _overFrameSideClipL_Rt = go.GetComponent<RectTransform>();
                    _overFrameSideClipL = go.GetComponent<RectMask2D>();
                }
                else if (_overFrameSideClipL_Rt.parent != anchorParent)
                {
                    _overFrameSideClipL_Rt.SetParent(anchorParent, false);
                }

                _overFrameSideClipL_Rt.gameObject.SetActive(true);
                MatchRectToFrameNormalized(_overFrameSideClipL_Rt, frameRt, anchorParentRt, leftSideNrm);
                // Inset so the over-art never touches the parchment border lines (left side margin)
                // NOTE: This clip is OUTSIDE the parchment box, so we MUST NOT apply the parchment border thickness here.
                // Applying the fallback border inset (13px) creates visible "empty margins" near the corners.
                float sideSafety = isPendulum ? OverFramePendulumSideContinuationSafetyInset : OverFrameSideContinuationSafetyInset;
                float sideOuterInset = isPendulum ? OverFramePendulumSideClipInsetOuter : OverFrameSideClipInsetOuter;
                float sideL_Inner = sideSafety + OverFrameSideClipInsetInner;
                float sideL_Top = sideSafety + OverFrameSideClipInsetTop;
                float sideL_Bottom = sideSafety + OverFrameSideClipInsetBottom;

                InsetRect(_overFrameSideClipL_Rt,
                    sideOuterInset,
                    Mathf.Max(0f, sideL_Inner - splitSeamOverlap),
                    Mathf.Max(0f, sideL_Top),
                    Mathf.Max(0f, sideL_Bottom));
                if (_overFrameArtSideL == null)
                {
                    var clone = Instantiate(_overFrameArt, _overFrameSideClipL_Rt);
                    clone.name = "OverFrameArtSideL";
                    clone.raycastTarget = false;
                    _overFrameArtSideL = clone;

                    var arfL = _overFrameArtSideL.GetComponent<AspectRatioFitter>();
                    if (arfL) arfL.enabled = false;
                }
                else if (_overFrameArtSideL.transform.parent != _overFrameSideClipL_Rt)
                {
                    _overFrameArtSideL.transform.SetParent(_overFrameSideClipL_Rt, false);
                }

                _overFrameArtSideL.gameObject.SetActive(true);
                _overFrameArtSideL.texture = sideTex;
                _overFrameArtSideL.uvRect = sideUv;
                _overFrameArtSideL.color = Color.white;
                _overFrameArtSideL.maskable = true;

                MatchRectByWorldCorners(_overFrameArtSideL.rectTransform, splitArtReferenceRt, _overFrameSideClipL_Rt);
                _overFrameArtSideL.rectTransform.sizeDelta *= spec.scale;
                _overFrameArtSideL.rectTransform.anchoredPosition += spec.offset;
            }
            else
            {
                if (_overFrameSideClipL_Rt) _overFrameSideClipL_Rt.gameObject.SetActive(false);
                if (_overFrameArtSideL) _overFrameArtSideL.gameObject.SetActive(false);
            }

            // Right clip
            if (rightSideNrm.width > 0.001f && rightSideNrm.height > 0.001f)
            {
                if (_overFrameSideClipR_Rt == null)
                {
                    var go = new GameObject("OverFrameSideClipR", typeof(RectTransform), typeof(RectMask2D));
                    go.transform.SetParent(anchorParent, false);
                    _overFrameSideClipR_Rt = go.GetComponent<RectTransform>();
                    _overFrameSideClipR = go.GetComponent<RectMask2D>();
                }
                else if (_overFrameSideClipR_Rt.parent != anchorParent)
                {
                    _overFrameSideClipR_Rt.SetParent(anchorParent, false);
                }

                _overFrameSideClipR_Rt.gameObject.SetActive(true);
                MatchRectToFrameNormalized(_overFrameSideClipR_Rt, frameRt, anchorParentRt, rightSideNrm);
                // Inset so the over-art never touches the parchment border lines (right side margin)
                // NOTE: This clip is OUTSIDE the parchment box, so we MUST NOT apply the parchment border thickness here.
                // Applying the fallback border inset (13px) creates visible "empty margins" near the corners.
                float sideSafety = isPendulum ? OverFramePendulumSideContinuationSafetyInset : OverFrameSideContinuationSafetyInset;
                float sideOuterInset = isPendulum ? OverFramePendulumSideClipInsetOuter : OverFrameSideClipInsetOuter;
                float sideR_Inner = sideSafety + OverFrameSideClipInsetInner;
                float sideR_Top = sideSafety + OverFrameSideClipInsetTop;
                float sideR_Bottom = sideSafety + OverFrameSideClipInsetBottom;

                InsetRect(_overFrameSideClipR_Rt,
                    Mathf.Max(0f, sideR_Inner - splitSeamOverlap),
                    sideOuterInset,
                    Mathf.Max(0f, sideR_Top),
                    Mathf.Max(0f, sideR_Bottom));
                if (_overFrameArtSideR == null)
                {
                    var clone = Instantiate(_overFrameArt, _overFrameSideClipR_Rt);
                    clone.name = "OverFrameArtSideR";
                    clone.raycastTarget = false;
                    _overFrameArtSideR = clone;

                    var arfR = _overFrameArtSideR.GetComponent<AspectRatioFitter>();
                    if (arfR) arfR.enabled = false;
                }
                else if (_overFrameArtSideR.transform.parent != _overFrameSideClipR_Rt)
                {
                    _overFrameArtSideR.transform.SetParent(_overFrameSideClipR_Rt, false);
                }

                _overFrameArtSideR.gameObject.SetActive(true);
                _overFrameArtSideR.texture = sideTex;
                _overFrameArtSideR.uvRect = sideUv;
                _overFrameArtSideR.color = Color.white;
                _overFrameArtSideR.maskable = true;

                MatchRectByWorldCorners(_overFrameArtSideR.rectTransform, splitArtReferenceRt, _overFrameSideClipR_Rt);
                _overFrameArtSideR.rectTransform.sizeDelta *= spec.scale;
                _overFrameArtSideR.rectTransform.anchoredPosition += spec.offset;
            }
            else
            {
                if (_overFrameSideClipR_Rt) _overFrameSideClipR_Rt.gameObject.SetActive(false);
                if (_overFrameArtSideR) _overFrameArtSideR.gameObject.SetActive(false);
            }

            // ── B) TEXT CLIP (continuation inside parchment) ─────────────────
            var desc = (isPendulum && cardDescription != null && cardDescription.gameObject.activeInHierarchy)
                ? cardDescription
                : GetActiveOcgDescriptionText();
            if (desc == null)
            {
                if (_overFramePendulumCenterClipRt) _overFramePendulumCenterClipRt.gameObject.SetActive(false);
                if (_overFramePendulumCenterArt) _overFramePendulumCenterArt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipL_Rt) _overFramePendulumScaleClipL_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipR_Rt) _overFramePendulumScaleClipR_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtL) _overFramePendulumScaleArtL.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtR) _overFramePendulumScaleArtR.gameObject.SetActive(false);
                return true;
            }

            var fadeParentRt = desc.transform.parent as RectTransform;
            if (fadeParentRt == null)
            {
                if (_overFramePendulumCenterClipRt) _overFramePendulumCenterClipRt.gameObject.SetActive(false);
                if (_overFramePendulumCenterArt) _overFramePendulumCenterArt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipL_Rt) _overFramePendulumScaleClipL_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipR_Rt) _overFramePendulumScaleClipR_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtL) _overFramePendulumScaleArtL.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtR) _overFramePendulumScaleArtR.gameObject.SetActive(false);
                return true;
            }

            RectTransform textClipBoxRt = realBoxRt;
            Rect textClipBoxNrm = effectBoxInnerNrm;
            bool textClipIsInnerFill = effectBoxIsInnerFill;
            float textClipSafetyInset = clipSafetyInset;

            float textMidLeftW = midLeftW;
            float textMidRightW = midRightW;
            float textMidTopW = midTopW;
            float textMidBottomW = midBottomW;
            bool textHasBorder = hasBorder;

            if (isPendulum)
            {
                textClipBoxRt = hasExplicitEffectBox ? overFrameEffectBoxImage.rectTransform : GetEffectBoxRectTransform(desc);

                bool trustDetectedLowerBox = false;
                if (textClipBoxRt != null)
                {
                    if (hasExplicitEffectBox)
                    {
                        trustDetectedLowerBox = true;
                    }
                    else
                    {
                        var detectedImg = textClipBoxRt.GetComponent<Image>();
                        trustDetectedLowerBox = detectedImg != null && detectedImg.sprite != null;
                    }
                }

                if (trustDetectedLowerBox && textClipBoxRt != null && TryGetRectNormalizedInFrame(frameRt, textClipBoxRt, out var pendLowerBoxNrm))
                {
                    // Reject text-rect-like detections: pendulum lower box should be close to normal effect-box width.
                    bool likelyTooNarrow = pendLowerBoxNrm.width < (OverFrameEffectBoxNrm.width - (8f / 704f));
                    textClipBoxNrm = pendLowerBoxNrm;
                    textClipIsInnerFill = likelyTooNarrow ? true : IsEffectBoxRectInnerFill(textClipBoxRt, textClipBoxNrm);

                    if (likelyTooNarrow && !hasExplicitEffectBox)
                    {
                        textClipBoxRt = null;
                        textClipBoxNrm = OverFrameEffectBoxNrm;
                    }
                }
                else
                {
                    textClipBoxRt = null;
                    textClipBoxNrm = OverFrameEffectBoxNrm;
                    textClipIsInnerFill = true;
                }

                textClipSafetyInset = textClipIsInnerFill
                    ? OverFramePendulumInnerRectNoBorderInset
                    : OverFrameBorderSafetyInset;

                textHasBorder = TryGetEffectBoxBorderMidWorld(textClipBoxRt, out textMidLeftW, out textMidRightW, out textMidTopW, out textMidBottomW);
                if (textHasBorder)
                {
                    textMidLeftW *= OverFrameBorderCutFactor;
                    textMidRightW *= OverFrameBorderCutFactor;
                    textMidTopW *= OverFrameBorderCutFactor;
                    textMidBottomW *= OverFrameBorderCutFactor;
                }
            }

            if (_overFrameTextClipRt == null)
            {
                var go = new GameObject("OverFrameTextClip", typeof(RectTransform), typeof(RectMask2D));
                go.transform.SetParent(fadeParentRt, false);
                _overFrameTextClipRt = go.GetComponent<RectTransform>();
                _overFrameTextClip = go.GetComponent<RectMask2D>();
            }
            else if (_overFrameTextClipRt.parent != fadeParentRt)
            {
                _overFrameTextClipRt.SetParent(fadeParentRt, false);
            }

            _overFrameTextClipRt.gameObject.SetActive(true);

            // Clip area = parchment box (prefer the REAL parchment Image rect when available)
            // realBoxRt already computed above (explicit parchment Image preferred).
            if (textClipBoxRt != null)
                MatchRectByWorldCorners(_overFrameTextClipRt, textClipBoxRt, fadeParentRt);
            else
                MatchRectToFrameNormalized(_overFrameTextClipRt, frameRt, fadeParentRt, textClipBoxNrm);

            // Inset so continuation doesn't tint the orange border line.
            // We cut at the INNER edge of the parchment border (9-slice border * OverFrameBorderCutFactor),
            // then add a small safety inset to avoid half-pixel filtering bleed.
            float textInsetL = (textClipIsInnerFill ? 0f : (textHasBorder ? WorldToLocalX(_overFrameTextClipRt, textMidLeftW) : OverFrameFallbackBorderInset))
                               + textClipSafetyInset + OverFrameTextClipInsetLeft;
            float textInsetR = (textClipIsInnerFill ? 0f : (textHasBorder ? WorldToLocalX(_overFrameTextClipRt, textMidRightW) : OverFrameFallbackBorderInset))
                               + textClipSafetyInset + OverFrameTextClipInsetRight;
            float textInsetT = (textClipIsInnerFill ? 0f : (textHasBorder ? WorldToLocalY(_overFrameTextClipRt, textMidTopW) : OverFrameFallbackBorderInset))
                               + textClipSafetyInset + OverFrameTextClipInsetTop;
            float textClipInsetBottom = isPendulum ? OverFramePendulumTextClipInsetBottom : OverFrameTextClipInsetBottom;
            float textInsetB = (textClipIsInnerFill ? 0f : (textHasBorder ? WorldToLocalY(_overFrameTextClipRt, textMidBottomW) : OverFrameFallbackBorderInset))
                               + textClipSafetyInset + textClipInsetBottom;

            InsetRect(
                _overFrameTextClipRt,
                Mathf.Max(0f, textInsetL - splitSeamOverlap),
                Mathf.Max(0f, textInsetR - splitSeamOverlap),
                Mathf.Max(0f, textInsetT),
                Mathf.Max(0f, textInsetB));
            // --- BG continuation under the parchment (prevents "empty" transparent gaps) ---
            Texture bgTex = baseArt != null ? baseArt.texture : null;
            Rect bgUv = baseArt != null ? baseArt.uvRect : new Rect(0f, 0f, 1f, 1f);

            if (_overFrameArtTextBG == null)
            {
                RawImage template = baseArt != null ? baseArt : _overFrameArt;
                if (template != null)
                {
                    var bgClone = Instantiate(template, _overFrameTextClipRt);
                    bgClone.name = "OverFrameArtTextBG";
                    bgClone.raycastTarget = false;
                    _overFrameArtTextBG = bgClone;

                    var arfBg = _overFrameArtTextBG.GetComponent<AspectRatioFitter>();
                    if (arfBg) arfBg.enabled = false;
                }
                else
                {
                    var go = new GameObject("OverFrameArtTextBG", typeof(RawImage));
                    go.transform.SetParent(_overFrameTextClipRt, false);
                    _overFrameArtTextBG = go.GetComponent<RawImage>();
                    _overFrameArtTextBG.raycastTarget = false;
                }
            }
            else if (_overFrameArtTextBG.transform.parent != _overFrameTextClipRt)
            {
                _overFrameArtTextBG.transform.SetParent(_overFrameTextClipRt, false);
            }

            _overFrameArtTextBG.gameObject.SetActive(true);
            _overFrameArtTextBG.texture = bgTex != null ? bgTex : tex; // fallback if base art missing
            _overFrameArtTextBG.uvRect = bgUv;
            _overFrameArtTextBG.color = new Color(1f, 1f, 1f, OverFrameTextBgAlpha);
            _overFrameArtTextBG.maskable = true;

            // Align BG continuation to full frame (then clipped by parchment mask)
            MatchRectByWorldCorners(_overFrameArtTextBG.rectTransform, splitArtReferenceRt, _overFrameTextClipRt);
            _overFrameArtTextBG.rectTransform.sizeDelta *= spec.scale;
            _overFrameArtTextBG.rectTransform.anchoredPosition += spec.offset;

            // Create the faint continuation copy
            if (_overFrameArtText == null && _overFrameArt != null)
            {
                var clone = Instantiate(_overFrameArt, _overFrameTextClipRt);
                clone.name = "OverFrameArtText";
                clone.raycastTarget = false;
                _overFrameArtText = clone;

                var arf = _overFrameArtText.GetComponent<AspectRatioFitter>();
                if (arf) arf.enabled = false;
            }
            else if (_overFrameArtText != null && _overFrameArtText.transform.parent != _overFrameTextClipRt)
            {
                _overFrameArtText.transform.SetParent(_overFrameTextClipRt, false);
            }

            if (_overFrameArtText != null)
            {
                _overFrameArtText.gameObject.SetActive(true);
                _overFrameArtText.texture = tex;
                _overFrameArtText.uvRect = _overFrameArt != null ? _overFrameArt.uvRect : _overFrameArtText.uvRect;
                float textArtAlpha = isPendulum ? OverFramePendulumTextArtAlpha : OverFrameTextArtAlpha;
                _overFrameArtText.color = new Color(1f, 1f, 1f, textArtAlpha);
                _overFrameArtText.maskable = true;

                // Align continuation to full frame (then clipped by parchment mask)
                MatchRectByWorldCorners(_overFrameArtText.rectTransform, splitArtReferenceRt, _overFrameTextClipRt);
                _overFrameArtText.rectTransform.sizeDelta *= spec.scale;
                _overFrameArtText.rectTransform.anchoredPosition += spec.offset;
            }
            // Ensure ordering: BG first, cutout second
            if (_overFrameArtTextBG != null) _overFrameArtTextBG.transform.SetSiblingIndex(0);
            if (_overFrameArtText != null) _overFrameArtText.transform.SetSiblingIndex(1);

            if (hasPendulumParchmentZones)
            {
                Texture2D pendTex = tex;
                Rect pendUv = _overFrameArt != null ? _overFrameArt.uvRect : new Rect(0f, 0f, 1f, 1f);
                if (OverFramePendulumUpperUseBaseArtContinuation &&
                    baseArt != null &&
                    baseArt.texture is Texture2D baseTex)
                {
                    // Scale parchments should continue the underlying artwork, not transparent cutout regions.
                    pendTex = baseTex;
                    pendUv = baseArt.uvRect;
                }

                float pendAlpha = OverFramePendulumUpperZoneArtAlpha;
                float centerInsetSide = OverFramePendulumCenterClipInsetSide;
                float centerInsetTop = OverFramePendulumCenterClipInsetTop;
                float centerInsetBottom = OverFramePendulumCenterClipInsetBottom;
                float scaleInsetOuter = OverFramePendulumScaleClipInsetOuter;
                float scaleInsetInner = OverFramePendulumScaleClipInsetInner;
                float scaleInsetTop = OverFramePendulumScaleClipInsetTop;
                float scaleInsetBottom = OverFramePendulumScaleClipInsetBottom;
                float fallbackClipInsetSide = OverFramePendulumZoneFallbackClipInsetSide;
                float fallbackClipInsetTop = OverFramePendulumZoneFallbackClipInsetTop;
                float fallbackClipInsetBottom = OverFramePendulumZoneFallbackClipInsetBottom;

                if (pendCenterBoxRt == null)
                {
                    centerInsetSide += fallbackClipInsetSide;
                    centerInsetTop += fallbackClipInsetTop;
                    centerInsetBottom += fallbackClipInsetBottom;
                }

                if (pendLeftBoxRt == null)
                {
                    scaleInsetOuter += fallbackClipInsetSide;
                    scaleInsetInner += fallbackClipInsetSide;
                    scaleInsetTop += fallbackClipInsetTop;
                    scaleInsetBottom += fallbackClipInsetBottom;
                }

                if (pendRightBoxRt == null)
                {
                    scaleInsetOuter += fallbackClipInsetSide;
                    scaleInsetInner += fallbackClipInsetSide;
                    scaleInsetTop += fallbackClipInsetTop;
                    scaleInsetBottom += fallbackClipInsetBottom;
                }

                UpdateMaskedContinuationClip(
                    ref _overFramePendulumCenterClipRt,
                    ref _overFramePendulumCenterClip,
                    ref _overFramePendulumCenterArt,
                    "OverFramePendulumCenterClip",
                    "OverFramePendulumCenterArt",
                    pendCenterNrm,
                    frameRt,
                    fadeParentRt,
                    splitArtReferenceRt,
                    pendTex,
                    pendUv,
                    spec,
                    pendAlpha,
                    centerInsetSide,
                    centerInsetSide,
                    centerInsetTop,
                    centerInsetBottom,
                    pendCenterBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);

                UpdateMaskedContinuationClip(
                    ref _overFramePendulumScaleClipL_Rt,
                    ref _overFramePendulumScaleClipL,
                    ref _overFramePendulumScaleArtL,
                    "OverFramePendulumScaleClipL",
                    "OverFramePendulumScaleArtL",
                    pendLeftScaleNrm,
                    frameRt,
                    fadeParentRt,
                    splitArtReferenceRt,
                    pendTex,
                    pendUv,
                    spec,
                    pendAlpha,
                    scaleInsetOuter,
                    scaleInsetInner,
                    scaleInsetTop,
                    scaleInsetBottom,
                    pendLeftBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);

                UpdateMaskedContinuationClip(
                    ref _overFramePendulumScaleClipR_Rt,
                    ref _overFramePendulumScaleClipR,
                    ref _overFramePendulumScaleArtR,
                    "OverFramePendulumScaleClipR",
                    "OverFramePendulumScaleArtR",
                    pendRightScaleNrm,
                    frameRt,
                    fadeParentRt,
                    splitArtReferenceRt,
                    pendTex,
                    pendUv,
                    spec,
                    pendAlpha,
                    scaleInsetInner,
                    scaleInsetOuter,
                    scaleInsetTop,
                    scaleInsetBottom,
                    pendRightBoxRt,
                    OverFramePendulumUpperZoneBorderCutFactor,
                    OverFramePendulumUpperZoneBorderSafetyInset);
            }
            else
            {
                if (_overFramePendulumCenterClipRt) _overFramePendulumCenterClipRt.gameObject.SetActive(false);
                if (_overFramePendulumCenterArt) _overFramePendulumCenterArt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipL_Rt) _overFramePendulumScaleClipL_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleClipR_Rt) _overFramePendulumScaleClipR_Rt.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtL) _overFramePendulumScaleArtL.gameObject.SetActive(false);
                if (_overFramePendulumScaleArtR) _overFramePendulumScaleArtR.gameObject.SetActive(false);
            }

            // IMPORTANT: the continuation must be BELOW the description text (so text stays crisp),
            // but ABOVE the parchment background (so you can actually see the art "under" the parchment like the official proxy).
            // We insert at desc index so Unity shifts desc (and anything above it) one slot up.
            int descIdx = desc.transform.GetSiblingIndex();
            _overFrameTextClipRt.SetSiblingIndex(Mathf.Clamp(descIdx, 0, fadeParentRt.childCount - 1));
            if (_overFramePendulumCenterClipRt) _overFramePendulumCenterClipRt.SetSiblingIndex(Mathf.Clamp(descIdx, 0, fadeParentRt.childCount - 1));
            if (_overFramePendulumScaleClipL_Rt) _overFramePendulumScaleClipL_Rt.SetSiblingIndex(Mathf.Clamp(descIdx, 0, fadeParentRt.childCount - 1));
            if (_overFramePendulumScaleClipR_Rt) _overFramePendulumScaleClipR_Rt.SetSiblingIndex(Mathf.Clamp(descIdx, 0, fadeParentRt.childCount - 1));

            return true;
        }


        // ────────────────────────────────────────────────
        // MAIN: Render OverFrame for a given card code
        // Returns true if OverFrame applied, false if not applicable / failed.
        // ────────────────────────────────────────────────
        private bool TryRenderOverFrame(int code)
        {
            // 0) Optional per-card tune values (render itself is ID-file driven).
            var spec = DefaultOverFrameSpec;
            if (OverFrameCardTweaks.TryGetValue(code, out var tweakSpec))
                spec = tweakSpec;

            // 1) Find base artwork UI
            var baseArt = GetActiveOcgArtImage();
            if (baseArt == null)
            {
                CleanupOverFrame();
                return false;
            }
            bool isPendulum = IsPendulumArtImage(baseArt);

            // 2) Load overlay PNG from OverFrame/Overframe folder by card ID.
            var tex = LoadOverFrameTexture(code);

            if (tex == null)
            {
                CleanupOverFrame();
                return false;
            }

            // 3) Create overlay RawImage once (clone base art to inherit material/settings)
            if (_overFrameArt == null)
            {
                var clone = Instantiate(baseArt, baseArt.transform.parent);
                clone.name = "OverFrameArt";
                clone.raycastTarget = false;
                _overFrameArt = clone;
            }

            // 4) Parent overlay in a mask-safe location so it can extend outside the art window
            var safeParent = GetOverFrameSafeParent(baseArt);
            if (safeParent == null)
            {
                CleanupOverFrame();
                return false;
            }

            if (_overFrameArt.transform.parent != safeParent)
                _overFrameArt.transform.SetParent(safeParent, true);

            // 5) Apply texture + basic settings
            _overFrameArt.texture = tex;
            _overFrameArt.color = Color.white;
            _overFrameArt.gameObject.SetActive(true);

            // If the clone inherited an AspectRatioFitter, disable it (prevents “shrink to fit”)
            var arf = _overFrameArt.GetComponent<AspectRatioFitter>();
            if (arf) arf.enabled = false;

            // UV is decided after we know whether this is a full-card overlay (see step 6).

            // 6) Decide anchor: full-card (frame) vs art-window alignment
            RectTransform anchorRt;
            Transform anchorParent;

            bool looksFullCard = false;
            if (cardFrame != null)
            {
                float aTex = (float)tex.width / Mathf.Max(1, tex.height);
                var fr = cardFrame.rectTransform.rect;
                float aFrame = fr.width / Mathf.Max(1f, fr.height);
                looksFullCard = Mathf.Abs(aTex - aFrame) < 0.06f;
            }

            
            // Decide UV cropping:
            // - Full-card overlays must NOT inherit baseArt.uvRect (it would zoom/crop the overlay).
            // - Keep full UV for full-card overlays to match the legacy overframe mod behavior.
            Rect overUv = baseArt.uvRect;
            if (looksFullCard && cardFrame != null)
            {
                overUv = new Rect(0f, 0f, 1f, 1f);
            }

            _overFrameArt.uvRect = overUv;

            if (looksFullCard && cardFrame != null)
            {
                // Full card overlay aligns to card frame bounds
                anchorRt = cardFrame.rectTransform;
                anchorParent = cardFrame.transform.parent;
            }
            else
            {
                // Default aligns to the artwork window bounds
                anchorRt = baseArt.rectTransform;
                anchorParent = GetOverFrameSafeParent(baseArt);
            }

            if (anchorParent == null)
            {
                CleanupOverFrame();
                return false;
            }

            // Ensure overlay is under the chosen anchor parent (still mask-safe)
            if (_overFrameArt.transform.parent != anchorParent)
                _overFrameArt.transform.SetParent(anchorParent, true);

            var parentRt = anchorParent as RectTransform;
            if (parentRt == null)
            {
                CleanupOverFrame();
                return false;
            }

            // 7) Reset placement every render (no accumulation)
            // Proxy style split only makes sense for full-card overlays.
            bool splitApplied = false;

            if (looksFullCard && cardFrame != null)
            {
                splitApplied = ApplyOverFrameProxySplit(tex, baseArt, spec, cardFrame.rectTransform, anchorParent, parentRt, isPendulum);
            }

            if (!splitApplied)
            {
                MatchRectByWorldCorners(_overFrameArt.rectTransform, anchorRt, parentRt);

                // 8) Apply per-card tuning after reset
                _overFrameArt.rectTransform.sizeDelta *= spec.scale;
                _overFrameArt.rectTransform.anchoredPosition += spec.offset;
            }

            // 9) Layering: place OverFrame near frame/art but keep text on top
            //    - If overlay has alpha, it can safely go above the frame for true "overframe"
            //    - If it has NO alpha (checkerboard baked), keep frame visible (place under it)
            Transform overFrameRoot = (splitApplied && _overFrameMainClipRt != null) ? _overFrameMainClipRt.transform : _overFrameArt.transform;
            bool hasAlpha = TextureHasTransparency(tex);

            // Reference index (fallback) based on the anchor used
            int refIdx = anchorRt.transform.GetSiblingIndex();
            if (cardFrame != null && overFrameRoot.parent == cardFrame.transform.parent)
                refIdx = Mathf.Max(refIdx, cardFrame.transform.GetSiblingIndex());

            int finalIdx = refIdx + 1;

            if (cardFrame != null && overFrameRoot.parent == cardFrame.transform.parent)
            {
                int frameIdx = cardFrame.transform.GetSiblingIndex();
                if (hasAlpha)
                {
                    finalIdx = frameIdx + 1;            // true overframe: above frame
                }
                else
                {
                    Debug.LogWarning($"[OverFrame] {code} overlay has no alpha (checkerboard baked). " +
                                     $"Export RGBA cutout. Placing UNDER frame as fallback.");
                    finalIdx = Mathf.Max(0, frameIdx - 1); // fallback: keep frame visible
                }
            }

            if (splitApplied)
            {
                int idx = finalIdx;
                if (_overFrameSideClipL_Rt) _overFrameSideClipL_Rt.SetSiblingIndex(idx++);
                if (_overFrameSideClipR_Rt) _overFrameSideClipR_Rt.SetSiblingIndex(idx++);
                if (_overFrameMainClipRt) _overFrameMainClipRt.SetSiblingIndex(idx++);
            }
            else
            {
                overFrameRoot.SetSiblingIndex(finalIdx);
            }

            // 10) FINAL: Apply effect-box fade + enforce ordering
            //     OverFrameArt → FadeOverlay → Text/UI
            ApplyOverFrameEffectBoxFade(isPendulum);

            // Keep important UI on top (don’t hide ATK/Level/etc)
            if (cardName) cardName.transform.SetAsLastSibling();
            if (attrIcon) attrIcon.transform.SetAsLastSibling();
            if (attrRuby) attrRuby.transform.SetAsLastSibling();
            if (spellType) spellType.transform.SetAsLastSibling();

            if (levels) levels.transform.SetAsLastSibling();
            if (ranks) ranks.transform.SetAsLastSibling();
            if (rank13) rank13.transform.SetAsLastSibling();
            if (linkMarkers) linkMarkers.transform.SetAsLastSibling();

            if (line) line.transform.SetAsLastSibling();
            if (textATK) textATK.transform.SetAsLastSibling();
            if (textDEF) textDEF.transform.SetAsLastSibling();
            if (numATK) numATK.transform.SetAsLastSibling();
            if (numDEF) numDEF.transform.SetAsLastSibling();
            if (linkCount) linkCount.transform.SetAsLastSibling();

            if (cardDescription) cardDescription.transform.SetAsLastSibling();
            if (cardDescriptionPendulum) cardDescriptionPendulum.transform.SetAsLastSibling();
            if (lScale) lScale.transform.SetAsLastSibling();
            if (rScale) rScale.transform.SetAsLastSibling();
            if (cardPassword) cardPassword.transform.SetAsLastSibling();
            if (cardAuther) cardAuther.transform.SetAsLastSibling();

            return true;
        }


        private void SetRushDuelCard(Card data, Texture2D art)
        {
            ocg.SetActive(false);
            rd.SetActive(true);
            CleanupOverFrame(); // prevent OCG overlay sticking on Rush cards

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
            CleanupOverFrame(); // prevent OverFrame overlay sticking on other OCG cards

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
            numATK.text = data.GetAttackString();
            numDEF.text = data.GetDefenseString();
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
            if (art != null)
                TryRenderOverFrame(data.Id);
            else
                CleanupOverFrame();
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

            if (!Language.UseLatin(language))
                description = description.Replace(Program.STRING_SLASH, BIG_SLASH);
            else
                description = description.Replace(Program.STRING_SLASH, SMALL_SLASH);

            if (!Language.UseLatin(language))
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
