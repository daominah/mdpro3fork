using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3
{
    public class CardDescription : MonoBehaviour
    {
        ElementObjectManager manager;

        public static Color upColor = Color.cyan;
        public static Color downColor = Color.red;
        public static Color equalColor = Color.white;
        public Card data;
        public bool showing;
        private void Start()
        {
            manager = GetComponent<ElementObjectManager>();
            manager.GetElement<Button>("CardButton").onClick.AddListener(ShowDetail);
        }


        public void Hide()
        {
            showing = false;
            manager.GetElement<RectTransform>("Window").DOAnchorPosX(-420, 0.01f);
        }

        void ShowDetail()
        {
            Program.I().ocgcore.list.Hide();
            var cardFace = manager.GetElement<RawImage>("Card").texture;
            var mat = manager.GetElement<RawImage>("Card").material;
            Program.I().ocgcore.detail.Show(data, cardFace, mat);
        }

        IEnumerator RefreshFace(int code)
        {
            var mat = TextureManager.GetCardMaterial(code, true);
            var ie = Program.I().texture_.LoadCardAsync(code, true);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            mat.mainTexture = ie.Current;
            mat.renderQueue = 3000;
            manager.GetElement<RawImage>("Card").material = mat;
        }

        public void Show(GameCard card, Material mat)
        {
            var data = card.GetData();
            var tails = "<color=#0FFF0F>" + card.tails.managedString + "</color>";
            var p = card.p;
            if (data.Id == 0)
                return;

            this.data = data;
            var origin = CardsManager.Get(data.Id);

            manager.GetElement<RectTransform>("Window").DOAnchorPosX(20, 0.01f);
            showing = true;

            if (p == null || (p.location & (uint)CardLocation.Search) > 0)
            {
                manager.GetElement<Image>("Player").color = new Color(0, 0, 0, 0.3f);
                manager.GetElement("BaseActivated").SetActive(false);
            }
            else if (p.controller == 0)
            {
                manager.GetElement<Image>("Player").color = new Color(0, 0, 1, 0.3f);
                if (Program.I().ocgcore.myActivated.Contains(data.Id))
                    manager.GetElement("BaseActivated").SetActive(true);
                else
                    manager.GetElement("BaseActivated").SetActive(false);
            }
            else
            {
                manager.GetElement<Image>("Player").color = new Color(1, 0, 0, 0.3f);
                if (Program.I().ocgcore.opActivated.Contains(data.Id))
                    manager.GetElement("BaseActivated").SetActive(true);
                else
                    manager.GetElement("BaseActivated").SetActive(false);
            }

            manager.GetElement<Text>("TextName").text = data.Name;
            var attributeSprite = GetCardAttribute(data);
            manager.GetElement<Image>("Attribute").sprite = attributeSprite.sprite;
            manager.GetElement("ArributeOutline").SetActive(attributeSprite.notOriginal);
            var frameColors = GetCardFrameColor(data);
            manager.GetElement<Image>("BaseName").color = frameColors[0];
            manager.GetElement<Image>("BaseType").color = frameColors[1];
            manager.GetElement<Image>("BaseActivated").color = frameColors[1];

            if (mat == null)
                StartCoroutine(RefreshFace(data.Id));
            else
            {
                manager.GetElement<RawImage>("Card").material = Instantiate(mat);
                manager.GetElement<RawImage>("Card").material.SetFloat("_Monochrome", 0);
                manager.GetElement<RawImage>("Card").material.renderQueue = 3000;
            }

            manager.GetElement<Text>("TextType").text = StringHelper.GetType(data);
            if ((data.Type & (uint)CardType.Pendulum) > 0)
            {
                var texts = GetCardDescriptionSplit(origin.Desc);
                string monster = InterString.Get("【怪兽效果】");
                if ((data.Type & (uint)CardType.Effect) == 0)
                    monster = InterString.Get("【怪兽描述】");
                if (p != null
                    && ((p.location & (uint)CardLocation.PendulumZone) > 0 ||
                    ((p.location & (uint)CardLocation.SpellZone) > 0
                    && (data.Type & (uint)CardType.Equip) == 0
                    && (data.Type & (uint)CardType.Continuous) == 0
                    && (data.Type & (uint)CardType.Trap) == 0)))
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + GetSetName(data.Id) + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "\n"
                        + "<color=#666666>" + monster + "\n" + texts[1] + "</color>";
                else if (p != null && (p.location & (uint)CardLocation.MonsterZone) > 0)
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + GetSetName(data.Id) + monster + "\n" + texts[1] + "\n"
                        + "<color=#666666>" + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "</color>";
                else
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + GetSetName(data.Id) + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "\n"
                        + monster + "\n" + texts[1];

            }
            else
                manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + GetSetName(data.Id) + data.Desc;

            bool isMonster = WhetherCardIsMonster(data);
            if (isMonster)
            {
                manager.GetElement("PropertyMonster").SetActive(true);
                manager.GetElement("PropertySpell").SetActive(false);

                var raceSprite = GetCardRace(data);
                manager.GetElement<Image>("Race").sprite = raceSprite.sprite;
                manager.GetElement("RaceOutline").SetActive(raceSprite.notOriginal);
                manager.GetElement<Image>("Level").sprite = TextureManager.GetCardLevelIcon(data);
                if ((data.Type & (uint)CardType.Link) > 0)
                {
                    manager.GetElement<Text>("TextLevel").text = GetCardLinkCount(data).ToString();

                    manager.GetElement("Scale").SetActive(false);
                    manager.GetElement("TextScale").SetActive(false);
                    manager.GetElement("Defense").SetActive(false);
                    manager.GetElement("TextDefense").SetActive(false);

                    manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -45);
                    manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -45);
                }
                else
                {
                    manager.GetElement<Text>("TextLevel").text = data.Level.ToString();
                    if (data.Level > origin.Level)
                        manager.GetElement<Text>("TextLevel").color = upColor;
                    else if (data.Level < origin.Level)
                        manager.GetElement<Text>("TextLevel").color = downColor;
                    else
                        manager.GetElement<Text>("TextLevel").color = equalColor;

                    manager.GetElement("Defense").SetActive(true);
                    manager.GetElement("TextDefense").SetActive(true);
                    manager.GetElement<Text>("TextDefense").text = data.Defense == -2 ? "?" : data.Defense.ToString();
                    if (data.Defense > (origin.Defense < 0 ? 0 : origin.Defense))
                        manager.GetElement<Text>("TextDefense").color = upColor;
                    else if (data.Defense < origin.Defense)
                        manager.GetElement<Text>("TextDefense").color = downColor;
                    else
                        manager.GetElement<Text>("TextDefense").color = equalColor;

                    if ((data.Type & (uint)CardType.Pendulum) > 0)
                    {
                        manager.GetElement("Scale").SetActive(true);
                        manager.GetElement("TextScale").SetActive(true);
                        manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -90);
                        manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -90);
                        manager.GetElement<RectTransform>("Defense").anchoredPosition = new Vector2(0, -135);
                        manager.GetElement<RectTransform>("TextDefense").anchoredPosition = new Vector2(40, -135);

                        manager.GetElement<Text>("TextScale").text = data.LScale.ToString();
                        if (data.LScale > origin.LScale)
                            manager.GetElement<Text>("TextScale").color = upColor;
                        else if (data.LScale < origin.LScale)
                            manager.GetElement<Text>("TextScale").color = downColor;
                        else
                            manager.GetElement<Text>("TextScale").color = equalColor;
                    }
                    else
                    {
                        manager.GetElement("Scale").SetActive(false);
                        manager.GetElement("TextScale").SetActive(false);
                        manager.GetElement<RectTransform>("Attack").anchoredPosition = new Vector2(0, -45);
                        manager.GetElement<RectTransform>("TextAttack").anchoredPosition = new Vector2(40, -45);
                        manager.GetElement<RectTransform>("Defense").anchoredPosition = new Vector2(0, -90);
                        manager.GetElement<RectTransform>("TextDefense").anchoredPosition = new Vector2(40, -90);
                    }
                }

                manager.GetElement<Text>("TextAttack").text = data.Attack == -2 ? "?" : data.Attack.ToString();
                if (data.Attack > (origin.Attack < 0 ? 0 : origin.Attack))
                    manager.GetElement<Text>("TextAttack").color = upColor;
                else if (data.Attack < origin.Attack)
                    manager.GetElement<Text>("TextAttack").color = downColor;
                else
                    manager.GetElement<Text>("TextAttack").color = equalColor;
            }
            else
            {
                manager.GetElement("PropertyMonster").SetActive(false);
                manager.GetElement("PropertySpell").SetActive(true);
                manager.GetElement<Image>("SpellType").sprite = TextureManager.GetSpellTrapTypeIcon(data);
                manager.GetElement<Text>("TextSpellType").text = StringHelper.SecondType(data.Type) + StringHelper.MainType(data.Type);
            }
            RefreshLimitIcon(data.Id);
        }
        void RefreshLimitIcon(int code)
        {
            var banlist = Program.I().editDeck.banlist;//TODO
            var limit = banlist.GetQuantity(code);
            if (limit == 3)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.typeNone;
            else if (limit == 2)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit2;
            else if (limit == 1)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit1;
            else
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.banned;
        }


        public static bool WhetherCardIsMonster(Card data)
        {
            var origin = CardsManager.Get(data.Id);
            if ((origin.Type & (uint)CardType.Monster) == 0)
            {
                if ((data.Type & (uint)CardType.Monster) > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                if ((data.Type & (uint)CardType.Spell) > 0)
                    return false;
                else if ((data.Type & (uint)CardType.Trap) > 0)
                    return false;
                else
                    return true;
            }
        }

        public struct AttributeSprite
        {
            public Sprite sprite;
            public bool notOriginal;
        }
        public static AttributeSprite GetCardAttribute(Card data)
        {
            var origin = CardsManager.Get(data.Id);
            var returnValue = new AttributeSprite();

            bool isMonster = WhetherCardIsMonster(data);
            if (isMonster)
            {
                if ((origin.Type & (uint)CardType.Monster) == 0)
                {
                    returnValue.sprite = TextureManager.GetCardAttributeIcon(data.Attribute);
                    returnValue.notOriginal = true;
                }
                else
                {
                    if ((data.Attribute ^ origin.Attribute) == 0)
                    {
                        returnValue.sprite = TextureManager.GetCardAttributeIcon(data.Attribute);
                        returnValue.notOriginal = false;
                    }
                    else
                    {
                        returnValue.notOriginal = true;
                        if (data.Attribute != origin.Attribute)
                            returnValue.sprite = TextureManager.GetCardAttributeIcon(data.Attribute);
                        else
                            returnValue.sprite = TextureManager.GetCardAttributeIcon(data.Attribute - origin.Attribute);
                    }
                }
            }
            else
            {
                if ((origin.Type & (uint)CardType.Monster) == 0)
                {
                    if ((data.Type & (uint)CardType.Spell & origin.Type) > 0)
                    {
                        returnValue.sprite = TextureManager.container.attributeSpell;
                        returnValue.notOriginal = false;
                    }
                    else if ((data.Type & (uint)CardType.Trap & origin.Type) > 0)
                    {
                        returnValue.sprite = TextureManager.container.attributeTrap;
                        returnValue.notOriginal = false;
                    }
                    else
                    {
                        returnValue.notOriginal = true;
                        if ((data.Type & (uint)CardType.Spell) > 0)
                            returnValue.sprite = TextureManager.container.attributeSpell;
                        else
                            returnValue.sprite = TextureManager.container.attributeTrap;
                    }
                }
                else
                {
                    if ((data.Type & (uint)CardType.Spell) > 0)
                    {
                        returnValue.sprite = TextureManager.container.attributeSpell;
                        returnValue.notOriginal = true;
                    }
                    else// if ((data.Type & (uint)CardType.Trap) > 0)
                    {
                        returnValue.sprite = TextureManager.container.attributeTrap;
                        returnValue.notOriginal = true;
                    }
                }
            }
            return returnValue;
        }
        public struct RaceSprite
        {
            public Sprite sprite;
            public bool notOriginal;
        }
        public static RaceSprite GetCardRace(Card data)
        {
            var returnValue = new RaceSprite();
            returnValue.notOriginal = false;
            var origin = CardsManager.Get(data.Id);
            if ((origin.Type & (uint)CardType.Monster) == 0)
                returnValue.notOriginal = true;
            else
                if (data.Race != origin.Race)
                returnValue.notOriginal = true;
            returnValue.sprite = TextureManager.GetCardRaceIcon(data.Race);
            return returnValue;
        }
        public static int GetCardLinkCount(Card data)
        {
            int returnValue = 0;
            for (int i = 0; i < 9; i++)
                if (((data.LinkMarker >> i) & 1u) > 0 && i != 4)
                    returnValue++;
            return returnValue;
        }
        public static Color[] GetCardFrameColor(Card data)
        {
            var returnValue = new Color[2];
            returnValue[0] = new Color(0.7764f, 0.6784f, 0.6274f, 1f);
            returnValue[1] = returnValue[0];
            if (data.Id == 0)
                return returnValue;
            Card origin = CardsManager.Get(data.Id);
            if (data.Id == 10000000)
            {
                returnValue[0] = new Color(0.4745f, 0.4549f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (data.Id == 10000020)
            {
                returnValue[0] = new Color(1f, 0.2470f, 0.2156f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (data.Id == 10000010)
            {
                returnValue[0] = new Color(1f, 0.9882f, 0.1882f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Pendulum) > 0)
            {
                if ((origin.Type & (uint)CardType.Fusion) > 0)
                {
                    returnValue[0] = new Color(0.8823f, 0.345f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((origin.Type & (uint)CardType.Synchro) > 0)
                {
                    returnValue[0] = new Color(1f, 1f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((origin.Type & (uint)CardType.Xyz) > 0)
                {
                    returnValue[0] = new Color(0f, 0f, 0f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((origin.Type & (uint)CardType.Ritual) > 0)
                {
                    returnValue[0] = new Color(0.3176f, 0.5882f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((origin.Type & (uint)CardType.Effect) > 0)
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((origin.Type & (uint)CardType.Normal) > 0)
                {
                    returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
            }
            else if ((origin.Type & (uint)CardType.Fusion) > 0)
            {
                returnValue[0] = new Color(0.8823f, 0.345f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Synchro) > 0)
            {
                returnValue[0] = new Color(1f, 1f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Xyz) > 0)
            {
                returnValue[0] = new Color(0f, 0f, 0f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Link) > 0)
            {
                returnValue[0] = new Color(0f, 0.3764f, 0.7764f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Ritual) > 0 && (origin.Type & (uint)CardType.Monster) > 0)
            {
                returnValue[0] = new Color(0.3176f, 0.5882f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Token) > 0)
            {
                returnValue[0] = new Color(0.7764f, 0.6784f, 0.6274f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Effect) > 0)
            {
                returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Normal) > 0)
            {
                returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if ((origin.Type & (uint)CardType.Spell) > 0)
            {
                if ((data.Type & (uint)CardType.Effect) > 0)
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if ((data.Type & (uint)CardType.Normal) > 0)
                {
                    returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else
                {
                    returnValue[0] = new Color(0f, 0.8901f, 0.7411f, 1f);
                    returnValue[1] = returnValue[0];
                }
            }
            else if ((origin.Type & (uint)CardType.Trap) > 0)
            {
                if ((data.Type & (uint)CardType.Effect) > 0)
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(1f, 0.0509f, 0.6784f, 1f);
                }
                else if ((data.Type & (uint)CardType.Normal) > 0)
                {
                    returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                    returnValue[1] = new Color(1f, 0.0509f, 0.6784f, 1f);
                }
                else
                {
                    returnValue[0] = new Color(1f, 0.0509f, 0.6784f, 1f);
                    returnValue[1] = returnValue[0];
                }
            }
            return returnValue;
        }

        public static string[] GetCardDescriptionSplit(string description)
        {
            var returnValue = new string[2];
            var lines = description.Replace("\r", "").Split('\n');

            int pendulumEnd = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Contains("【"))
                {
                    pendulumEnd = i;
                    break;
                }
            }
            for (int i = 1; i < lines.Length; i++)
            {
                if (i < pendulumEnd)
                {
                    if (pendulumEnd - i == 1)
                        returnValue[0] += lines[i];
                    else
                        returnValue[0] += lines[i] + "\r\n";
                }
                else if (i > pendulumEnd)
                {
                    if (i == lines.Length - 1)
                        returnValue[1] += lines[i];
                    else
                        returnValue[1] += lines[i] + "\r\n";
                }
            }
            return returnValue;
        }

        public static string GetSetName(int code)
        {
            var data = CardsManager.Get(code);

            var returnValue = StringHelper.GetSetName(data.Setcode, true);
            if (returnValue.Length > 0)
            {
                returnValue = "<color=#FFF000>" +
                    StringHelper.GetUnsafe(1329) + returnValue + "</color>" + "\r\n";
            }
            return returnValue;
        }


    }
}
