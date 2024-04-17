using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using System;

namespace MDPro3
{
    public class CardRenderer : MonoBehaviour
    {
        public RawImage cardArt;
        public RawImage cardArtPendulum;
        public RawImage cardArtPendulumSquare;
        public RawImage cardArtPendulumWidth;
        public Image cardFrame;
        public Image cardAttribute;
        public Text cardName;
        public TextMeshProUGUI cardNameTMP;
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
        public Text spellType;
        public Image spellTypeIcon;

        public Font atkDef;
        public RenderTexture renderTexture;

        static readonly string bigSlash = "£¯";

        public void SwitchLanguage()
        {
            //cardDescription
            cardName.fontSize = 50;
            cardName.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 80);
            var language = Config.Get("Language", "zh-CN");
            if (language == "zh-CN")
            {
                var handle = Addressables.LoadAssetAsync<Font>("RenderFontChineseSimplified");
                handle.Completed += (result) =>
                {
                    cardName.font = handle.Result;
                    cardDescription.font = handle.Result;
                    cardDescriptionPendulum.font = handle.Result;
                    spellType.font = handle.Result;
                };
            }
            else if (language == "zh-TW")
            {
                var handle = Addressables.LoadAssetAsync<Font>("RenderFontChineseTraditional");
                handle.Completed += (result) =>
                {
                    cardName.font = handle.Result;
                    cardDescription.font = handle.Result;
                    cardDescriptionPendulum.font = handle.Result;
                    spellType.font = handle.Result;
                };
            }
            else if (language == "ko-KR")
            {
                var handle = Addressables.LoadAssetAsync<Font>("RenderFontKorean");
                handle.Completed += (result) =>
                {
                    cardName.font = handle.Result;
                    cardDescription.font = handle.Result;
                    cardDescriptionPendulum.font = handle.Result;
                    spellType.font = handle.Result;
                };
            }
            else if (language == "ja-JP")
            {
                var handle = Addressables.LoadAssetAsync<Font>("RenderFontJapanese");
                handle.Completed += (result) =>
                {
                    cardName.font = handle.Result;
                    cardDescription.font = handle.Result;
                    cardDescriptionPendulum.font = handle.Result;
                    spellType.font = handle.Result;
                };
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<Font>("RenderFontEnglish");
                handle.Completed += (result) =>
                {
                    cardDescription.font = handle.Result;
                    cardDescriptionPendulum.font = handle.Result;
                    spellType.font = handle.Result;
                };
            }
        }

        public void RenderName(int code)
        {
            var data = CardsManager.Get(code);
            if (data.Id == 0)
                return;
            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardNameTMP.GetComponent<RectTransform>().localScale = Vector3.one;
            if (Config.Get("Language", "zh-CN") == "en-US"
                || Config.Get("Language", "zh-CN") == "es-ES")
            {
                cardName.text = string.Empty;
                cardNameTMP.text = data.Name;
                cardNameTMP.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
                var nameWidth = cardNameTMP.GetComponent<RectTransform>().rect.width;
                if (nameWidth > 520)
                    cardNameTMP.GetComponent<RectTransform>().localScale = new Vector3(520 / nameWidth, 1, 1);
            }
            else
            {
                cardName.text = data.Name;
                cardNameTMP.text = string.Empty;
                cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
                var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
                if (nameWidth > 520)
                    cardName.GetComponent<RectTransform>().localScale = new Vector3(520 / nameWidth, 1, 1);
            }

            cardName.color = Color.white;
            cardNameTMP.color = Color.white;

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
            spellTypeIcon.sprite = TextureManager.container.typeNone;
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
        }

        public void RenderCard(int code, Texture2D art)
        {
            var data = CardsManager.Get(code);
            if (data.Id == 0)
                return;

            cardName.GetComponent<RectTransform>().localScale = Vector3.one;
            cardNameTMP.GetComponent<RectTransform>().localScale = Vector3.one;
            if (Config.Get("Language", "zh-CN") == "en-US"
                || Config.Get("Language", "zh-CN") == "es-ES")
            {
                cardName.text = string.Empty;
                cardNameTMP.text = data.Name;
                cardNameTMP.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
                var nameWidth = cardNameTMP.GetComponent<RectTransform>().rect.width;
                if (nameWidth > 520)
                    cardNameTMP.GetComponent<RectTransform>().localScale = new Vector3(520 / nameWidth, 1, 1);
            }
            else
            {
                cardName.text = data.Name;
                cardNameTMP.text = string.Empty;
                cardName.GetComponent<ContentSizeFitter>().SetLayoutHorizontal();
                var nameWidth = cardName.GetComponent<RectTransform>().rect.width;
                if (nameWidth > 520)
                    cardName.GetComponent<RectTransform>().localScale = new Vector3(520 / nameWidth, 1, 1);
            }

            cardName.color = Color.black;
            cardNameTMP.color = Color.black;

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
            spellTypeIcon.sprite = TextureManager.container.typeNone;
            cardDescription.GetComponent<RectTransform>().sizeDelta = new Vector2(590f, 160f);
            cardAttribute.sprite = CardDescription.GetCardAttribute(data).sprite;

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
                var pendulumDescription = CardDescription.GetCardDescriptionSplit(data.Desc);
                cardDescription.text = StringHelper.GetType(data).Replace(Program.slash, bigSlash) + "\r\n" + TextForRender(pendulumDescription[1]);
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
                    description = StringHelper.GetType(data).Replace(Program.slash, bigSlash) + "\r\n";
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
                    cardNameTMP.color = Color.white;
                    line.SetActive(false);
                    textATK.SetActive(false);
                    textDEF.SetActive(false);
                    numATK.text = "";
                    numDEF.text = "";
                    var bracketLeft = "¡¾";
                    var bracketRight = "¡¿";
                    var spaces = "   ";
                    spellTypeIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-79f, 364);
                    switch (Config.Get("Language", "zh-CN"))
                    {
                        case "en-US":
                            bracketLeft = "[";
                            bracketRight = "]";
                            spaces = "     ";
                            break;
                        case "es-ES":
                            bracketLeft = "[";
                            bracketRight = "]";
                            spaces = "     ";
                            break;
                        case "ko-KR":
                            bracketLeft = "[";
                            bracketRight = "]";
                            spaces = "  ";
                            spellTypeIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80f, 364);
                            break;
                        case "zh-TW":
                            spaces = "  ";
                            spellTypeIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-85f, 364);
                            break;
                        default:
                            spellTypeIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-79f, 364);
                            break;
                    }

                    if ((data.Type & (uint)CardType.Spell) > 0)
                    {
                        cardFrame.sprite = TextureManager.container.cardFrameSpellOF;
                        var type = bracketLeft + InterString.Get("Ä§·¨¿¨") + spaces + bracketRight;
                        if ((data.Type & (uint)CardType.Field) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeField;
                        else if ((data.Type & (uint)CardType.Equip) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeEquip;
                        else if ((data.Type & (uint)CardType.Continuous) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeContinuous;
                        else if ((data.Type & (uint)CardType.QuickPlay) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeQuickPlay;
                        else if ((data.Type & (uint)CardType.Ritual) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeRitual;
                        else
                            type = type.Replace(spaces, "");
                        spellType.text = type;
                    }
                    else
                    {
                        cardFrame.sprite = TextureManager.container.cardFrameTrapOF;
                        var type = bracketLeft + InterString.Get("ÏÝÚå¿¨") + spaces + bracketRight;
                        if ((data.Type & (uint)CardType.Counter) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeCounter;
                        else if ((data.Type & (uint)CardType.Continuous) > 0)
                            spellTypeIcon.sprite = TextureManager.container.typeContinuous;
                        else
                            type = type.Replace("   ", "");
                        spellType.text = type;
                    }
                }
                else
                    cardFrame.sprite = TextureManager.container.cardFrameEffectOF;
            }

            data = AdjustLevelForRender(data);

            if ((data.Type & (uint)CardType.Link) > 0)
            {
                cardName.color = Color.white;
                cardNameTMP.color = Color.white;
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
                cardNameTMP.color = Color.white;
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
            var language = Config.Get("Language", "zh-CN");

            if (language == "ja-JP")
            {
                description = description.Replace("\t", "\f\f\f");
                description = description.Replace("\n¡ñ", "¡ñ¡ñ¡ñ");
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
                    .Replace("\r\n¢á", "¢á")
                    .Replace("\n¢Ú", "¢Ú")
                    .Replace("\n¢Û", "¢Û")
                    .Replace("\n¢Ü", "¢Ü")
                    .Replace("\n¢Ý", "¢Ý")
                    .Replace("\n¢Þ", "¢Þ")
                    .Replace("\n¢ß", "¢ß")
                    .Replace("\n¢à", "¢à")
                    .Replace("\n¢á", "¢á");
            }

            description = description.Replace(Program.slash, bigSlash);
            if (language != "en-US" && language != "es-ES")
                description = description.Replace(" ", "\u00A0");
            description = description.Replace("\r\n\r\n", "\r\n");
            return description;
        }
    }
}
