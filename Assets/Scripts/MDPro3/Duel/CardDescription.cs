using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.Duel.YGOSharp;
using static MDPro3.CardRenderer;
using static MDPro3.Servant.OcgCore;
using MDPro3.Utility;
using MDPro3.UI;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;
using Cysharp.Threading.Tasks;

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
            manager.GetElement<RectTransform>("Window").DOAnchorPosX(-1020 - SafeAreaAdapter.GetSafeAreaLeftOffset(), 0.01f);
        }

        private void ShowDetail()
        {
            Program.instance.ocgcore.GetUI<OcgCoreUI>().CardList.Hide();
            Program.instance.ui_.chatPanel.Hide();

            //var cardFace = manager.GetElement<RawImage>("Card").texture;
            //var mat = manager.GetElement<RawImage>("Card").material;
            //Program.instance.ui_.cardDetail.Show(data, cardFace, mat);

            UIManager.ShowCardInfoDetail(data);
        }

        private async UniTask RefreshFace(int code)
        {
            var mat = MaterialLoader.GetCardMaterial(code);
            mat.mainTexture = await CardImageLoader.LoadCardAsync(code, false, destroyCancellationToken);
            manager.GetElement<RawImage>("Card").material = mat;
        }

        public void Show(GameCard card, Material mat, int code = -1, GPS gps = null)
        {
            Card data;
            if(code > -1)
                data = CardsManager.Get(code);
            else
                data = card.GetData();
            var origin = CardsManager.Get(data.Id);

            if (origin.Id == 0)
                return;

            var tails = string.Empty;
            if(code == -1)
                tails = "<color=#0FFF0F>" + card.tails.managedString + "</color>";
            GPS p;
            if (code > -1)
                p = gps;
            else
                p = card.p;

            this.data = data;

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
                if (myActivated.Contains(data.Id))
                    manager.GetElement("BaseActivated").SetActive(true);
                else
                    manager.GetElement("BaseActivated").SetActive(false);
            }
            else
            {
                manager.GetElement<Image>("Player").color = new Color(1, 0, 0, 0.3f);
                if (opActivated.Contains(data.Id))
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
            _ = RefreshFace(data.Id);
            else
            {
                manager.GetElement<RawImage>("Card").material = Instantiate(mat);
                manager.GetElement<RawImage>("Card").material.SetFloat("_Monochrome", 0);
                manager.GetElement<RawImage>("Card").material.renderQueue = 3000;
            }

            manager.GetElement<Text>("TextType").text = data.GetTypeForUI();
            if (data.HasType(CardType.Pendulum))
            {
                var texts = origin.GetDescriptionSplit();
                string monster = InterString.Get("【怪兽效果】");
                if (!data.HasType(CardType.Effect))
                    monster = InterString.Get("【怪兽描述】");
                if (p != null
                    && ((p.location & (uint)CardLocation.PendulumZone) > 0 ||
                    ((p.location & (uint)CardLocation.SpellZone) > 0
                    && !data.HasType(CardType.Equip)
                    && !data.HasType(CardType.Continuous)
                    && !data.HasType(CardType.Trap))))
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + data.GetSetNameWithColor() + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "\n"
                        + "<color=#666666>" + monster + "\n" + texts[1] + "</color>";
                else if (p != null && (p.location & (uint)CardLocation.MonsterZone) > 0)
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + data.GetSetNameWithColor() + monster + "\n" + texts[1] + "\n"
                        + "<color=#666666>" + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "</color>";
                else
                    manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + data.GetSetNameWithColor() + InterString.Get("【灵摆效果】") + "\n" + texts[0] + "\n"
                        + monster + "\n" + texts[1];
            }
            else
                manager.GetElement<TextMeshProUGUI>("TextDescription").text = tails + data.GetSetNameWithColor() + data.Desc;

            manager.GetElement<TextMeshProUGUI>("TextDescription").fontSize = 25f * Config.GetUIScale(1.35f);

            bool isMonster = CardIsMonster(data);
            if (isMonster)
            {
                manager.GetElement("PropertyMonster").SetActive(true);
                manager.GetElement("PropertySpell").SetActive(false);

                var raceSprite = GetCardRace(data);
                manager.GetElement<Image>("Race").sprite = raceSprite.sprite;
                manager.GetElement("RaceOutline").SetActive(raceSprite.notOriginal);
                bool isTuner = false;
                if (data.HasType(CardType.Tuner))
                {
                    isTuner = true;
                    manager.GetElement("Tuner").SetActive(true);
                }
                else
                    manager.GetElement("Tuner").SetActive(false);

                if(isTuner)
                {
                    if (origin.HasType(CardType.Tuner))
                        manager.GetElement("TunerOutline").SetActive(false);
                    else
                        manager.GetElement("TunerOutline").SetActive(true);
                }
                else
                    manager.GetElement("TunerOutline").SetActive(false);

                manager.GetElement<Image>("Level").sprite = TextureManager.GetCardLevelIcon(data);
                if (data.HasType(CardType.Link))
                {
                    manager.GetElement<Text>("TextLevel").text = data.GetLinkCount().ToString();

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

                    if (data.HasType(CardType.Pendulum))
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
                manager.GetElement<Text>("TextSpellType").text = data.GetSpellTrapType();
            }
            RefreshLimitIcon(data.Id);
        }

        private void RefreshLimitIcon(int code)
        {
            var banlist = DeckEditor.banlist;
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

        public static bool CardIsMonster(Card data)
        {
            var origin = CardsManager.Get(data.Id);
            if (!origin.HasType(CardType.Monster))
            {
                if (data.HasType(CardType.Monster))
                    return true;
                else
                    return false;
            }
            else
            {
                if (data.HasType(CardType.Spell))
                    return false;
                else if (data.HasType(CardType.Trap))
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

        private static AttributeSprite GetCardAttribute(Card data)
        {
            var origin = CardsManager.Get(data.Id);
            var returnValue = new AttributeSprite();

            bool isMonster = CardIsMonster(data);

            if (isMonster)
            {
                if (!origin.HasType(CardType.Monster))
                {
                    returnValue.notOriginal = true;
                    returnValue.sprite = TextureManager.container.GetCardAttributeIcon(data);
                }
                else
                {
                    if (origin.HasType(CardType.Trap))
                    {
                        returnValue.notOriginal = true;
                        returnValue.sprite = TextureManager.container.GetCardAttributeIcon(data);
                    }
                    else
                    {
                        if (data.Attribute == origin.Attribute)
                        {
                            returnValue.notOriginal = false;
                            returnValue.sprite = TextureManager.container.GetCardAttributeIcon(data);
                        }
                        else
                        {
                            returnValue.notOriginal = true;
                            var newData = data.Clone();
                            newData.Attribute = data.Attribute ^ origin.Attribute;
                            returnValue.sprite = TextureManager.container.GetCardAttributeIcon(newData);
                        }
                    }
                }
            }
            else
            {
                if (!origin.HasType(CardType.Monster))
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
                        if (data.HasType(CardType.Spell))
                            returnValue.sprite = TextureManager.container.attributeSpell;
                        else
                            returnValue.sprite = TextureManager.container.attributeTrap;
                    }
                }
                else
                {
                    returnValue.notOriginal = true;
                    if (data.HasType(CardType.Spell))
                        returnValue.sprite = TextureManager.container.attributeSpell;
                    else
                        returnValue.sprite = TextureManager.container.attributeTrap;
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
            if (!origin.HasType(CardType.Monster))
                returnValue.notOriginal = true;
            else
                if (data.Race != origin.Race)
                returnValue.notOriginal = true;
            returnValue.sprite = TextureManager.GetCardRaceIcon(data.Race);
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
            else if (origin.HasType(CardType.Pendulum))
            {
                if (origin.HasType(CardType.Fusion))
                {
                    returnValue[0] = new Color(0.8823f, 0.345f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (origin.HasType(CardType.Synchro))
                {
                    returnValue[0] = new Color(1f, 1f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (origin.HasType(CardType.Xyz))
                {
                    returnValue[0] = new Color(0f, 0f, 0f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (origin.HasType(CardType.Ritual))
                {
                    returnValue[0] = new Color(0.3176f, 0.5882f, 1f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (origin.HasType(CardType.Effect))
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (origin.HasType(CardType.Normal))
                {
                    returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
            }
            else if (origin.HasType(CardType.Fusion))
            {
                returnValue[0] = new Color(0.8823f, 0.345f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Synchro))
            {
                returnValue[0] = new Color(1f, 1f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Xyz))
            {
                returnValue[0] = new Color(0f, 0f, 0f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Link))
            {
                returnValue[0] = new Color(0f, 0.3764f, 0.7764f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Ritual) && origin.HasType(CardType.Monster))
            {
                returnValue[0] = new Color(0.3176f, 0.5882f, 1f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Token))
            {
                returnValue[0] = new Color(0.7764f, 0.6784f, 0.6274f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Effect))
            {
                returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Normal))
            {
                returnValue[0] = new Color(1f, 0.7450f, 0.3294f, 1f);
                returnValue[1] = returnValue[0];
            }
            else if (origin.HasType(CardType.Spell))
            {
                if (data.HasType(CardType.Effect))
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(0f, 0.8901f, 0.7411f, 1f);
                }
                else if (data.HasType(CardType.Normal))
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
            else if (origin.HasType(CardType.Trap))
            {
                if (data.HasType(CardType.Effect))
                {
                    returnValue[0] = new Color(1f, 0.4745f, 0.1882f, 1f);
                    returnValue[1] = new Color(1f, 0.0509f, 0.6784f, 1f);
                }
                else if (data.HasType(CardType.Normal))
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

    }
}