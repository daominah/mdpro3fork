using DG.Tweening;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;

namespace MDPro3
{
    public class CardDetail : MonoBehaviour
    {
        ElementObjectManager manager;
        public bool showing;
        float transitionTime = 0.1f;
        float hideScale = 0.9f;
        int code;
        private void Start()
        {
            manager = GetComponent<ElementObjectManager>();
        }
        public void Hide()
        {
            showing = false;
            //CameraManager.UIBlurMinus();
            AudioManager.PlaySE("SE_DUEL_CANCEL");
            manager.GetElement<RectTransform>("Window").DOScale(hideScale, transitionTime);
            manager.GetElement<CanvasGroup>("Window").DOFade(0, transitionTime);
            manager.GetElement<CanvasGroup>("Window").blocksRaycasts = false;
            manager.GetElement<CanvasGroup>("Window").interactable = false;

            manager.GetElement<CanvasGroup>("BlackBack").DOFade(0, transitionTime);
            manager.GetElement<CanvasGroup>("BlackBack").blocksRaycasts = false;
            manager.GetElement<CanvasGroup>("BlackBack").interactable = false;

            if (Program.I().currentServant == Program.I().editDeck)
                UIManager.ShowFPSRight();

        }
        public void Show(Card data, Texture cardFace, Material mat)
        {
            if (data.Id == 0)
                return;
            code = data.Id;
            if (Program.I().currentServant == Program.I().editDeck)
                UIManager.ShowFPSLeft();
            //CameraManager.UIBlurPlus();
            showing = true;
            AudioManager.PlaySE("SE_DECK_WINDOW_OPEN");
            manager.GetElement<RectTransform>("Window").localScale = Vector3.one * hideScale;
            manager.GetElement<RectTransform>("Window").DOScale(1f, transitionTime);
            manager.GetElement<CanvasGroup>("Window").DOFade(1, transitionTime);
            manager.GetElement<CanvasGroup>("Window").blocksRaycasts = true;
            manager.GetElement<CanvasGroup>("Window").interactable = true;
            manager.GetElement<CanvasGroup>("BlackBack").DOFade(1, transitionTime);
            manager.GetElement<CanvasGroup>("BlackBack").blocksRaycasts = true;
            manager.GetElement<CanvasGroup>("BlackBack").interactable = true;

            var origin = CardsManager.Get(data.Id);

            manager.GetElement<RawImage>("Card").texture = cardFace;
            manager.GetElement<RawImage>("Card").material = mat;

            var colors = CardDescription.GetCardFrameColor(origin);
            manager.GetElement<Image>("NameBase").color = colors[0];
            manager.GetElement<Image>("StatusBase").color = colors[0];
            manager.GetElement<Image>("PendulumBase").color = colors[1];
            manager.GetElement<Image>("EffectBase").color = colors[0];

            manager.GetElement<Text>("TextName").text = origin.Name;
            manager.GetElement<Image>("Attribute").sprite = CardDescription.GetCardAttribute(data).sprite;
            manager.GetElement<Text>("TextType").text = StringHelper.GetType(origin) + StringHelper.GetSetName(origin.Setcode)
                + "【" + origin.Id.ToString() + "】" + (origin.Alias != 0 ? "【" + origin.Alias.ToString() + "】" : "");

            var statusRect = manager.GetElement<RectTransform>("Status");
            var effectRect = manager.GetElement<RectTransform>("Effect");
            if ((origin.Type & (uint)CardType.Monster) > 0)
            {
                statusRect.sizeDelta = new Vector2(statusRect.sizeDelta.x, 140);
                manager.GetElement("StatusMonster").SetActive(true);
                manager.GetElement("StatusSpell").SetActive(false);
                manager.GetElement<Image>("Level").sprite = TextureManager.GetCardLevelIcon(origin);
                manager.GetElement<Image>("Race").sprite = TextureManager.GetCardRaceIcon(origin.Race);
                manager.GetElement<Text>("TextATK").text = origin.Attack == -2 ? "?" : origin.Attack.ToString();
                if ((origin.Type & (uint)CardType.Link) > 0)
                {
                    manager.GetElement<Text>("TextLevel").text = CardDescription.GetCardLinkCount(origin).ToString();
                    manager.GetElement("DEF").SetActive(false);
                    manager.GetElement("TextDEF").SetActive(false);
                }
                else
                {
                    manager.GetElement<Text>("TextLevel").text = origin.Level.ToString();
                    manager.GetElement("DEF").SetActive(true);
                    manager.GetElement("TextDEF").SetActive(true);
                    manager.GetElement<Text>("TextDEF").text = origin.Defense == -2 ? "?" : origin.Defense.ToString();
                }
                if ((origin.Type & (uint)CardType.Pendulum) > 0)
                {
                    manager.GetElement("Scale").SetActive(true);
                    manager.GetElement("TextScale").SetActive(true);
                    manager.GetElement<Text>("TextScale").text = origin.LScale.ToString();
                    manager.GetElement("Pendulum").SetActive(true);
                    effectRect.sizeDelta = new Vector2(effectRect.sizeDelta.x, 330);
                    var texts = CardDescription.GetCardDescriptionSplit(origin.Desc);
                    manager.GetElement<Text>("TextPendulum").text = TextForDetail(texts[0]);
                    manager.GetElement<Text>("TextEffect").text = TextForDetail(texts[1]);
                }
                else
                {
                    manager.GetElement("Scale").SetActive(false);
                    manager.GetElement("TextScale").SetActive(false);
                    manager.GetElement("Pendulum").SetActive(false);
                    effectRect.sizeDelta = new Vector2(effectRect.sizeDelta.x, 565);
                    manager.GetElement<Text>("TextEffect").text = TextForDetail(origin.Desc);
                }
            }
            else
            {
                statusRect.sizeDelta = new Vector2(statusRect.sizeDelta.x, 76);
                manager.GetElement("Pendulum").SetActive(false);
                manager.GetElement("StatusMonster").SetActive(false);
                manager.GetElement<Text>("TextEffect").text = origin.Desc;
                effectRect.sizeDelta = new Vector2(effectRect.sizeDelta.x, 630);

                manager.GetElement("StatusSpell").SetActive(true);
                manager.GetElement<Image>("TypeSpell").sprite = TextureManager.GetSpellTrapTypeIcon(origin);
                manager.GetElement<Text>("TextTypeSpell").text = StringHelper.SecondType(origin.Type) + StringHelper.MainType(origin.Type);
                if (manager.GetElement<Text>("TextTypeSpell").text.Contains(StringHelper.GetUnsafe(1054)))
                    manager.GetElement<RectTransform>("TextTypeSpell").anchoredPosition = new Vector2(15, -7);
                else
                    manager.GetElement<RectTransform>("TextTypeSpell").anchoredPosition = new Vector2(60, -7);
            }

            Banlist banlist;
            if (Program.I().currentServant == Program.I().editDeck)
                banlist = Program.I().editDeck.banlist;
            else
            {
                //TODO
                banlist = Program.I().editDeck.banlist;
            }
            var limit = banlist.GetQuantity(data.Id);
            if (limit == 3)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.typeNone;
            else if (limit == 2)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit2;
            else if (limit == 1)
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.limit1;
            else
                manager.GetElement<Image>("Limit").sprite = TextureManager.container.banned;
        }

        public void GenerateCard()
        {
            if (!Directory.Exists(Program.cardPicPath))
                Directory.CreateDirectory(Program.cardPicPath);
            try
            {
                Texture2D texture = (Texture2D)manager.GetElement<RawImage>("Card").texture;
                var picture = texture.EncodeToPNG();
                var fullPath = Program.cardPicPath + Program.slash + code + ".png";
                File.WriteAllBytes(Program.cardPicPath + Program.slash + code + ".png", picture);
                MessageManager.Cast(InterString.Get("卡图已保存于：[?]", fullPath));
            }
            catch
            {
                MessageManager.Cast(InterString.Get("没有写入权限，无法保存。"));
            }
        }

        string TextForDetail(string text)
        {
            if(string.IsNullOrEmpty(text))
                text = string.Empty;

            if (Config.Get("Language", "zh-CN") != "en-US"
                || Config.Get("Language", "zh-CN") != "es-ES")
            {
                return text.Replace(" ", "\u00A0");
            }
            else
                return text;
        }
    }
}
