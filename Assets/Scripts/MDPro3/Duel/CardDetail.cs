using DG.Tweening;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YgomSystem.ElementSystem;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Collections;

namespace MDPro3
{
    public class CardDetail : MonoBehaviour
    {
        ElementObjectManager manager;
        public bool showing;
        float transitionTime = 0.1f;
        float bigShowTime = 0.2f;
        float hideScale = 0.9f;
        int code;
        List<int> cards;
        int cardIndex;
        private void Start()
        {
            manager = GetComponent<ElementObjectManager>();
            manager.GetElement<CanvasGroup>("Window").alpha = 0f;
        }

        private void Update()
        {
            if(!showing || cards == null)
                return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnLeft();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnRight();

            if (Input.GetKeyDown(KeyCode.UpArrow))
                OnUp();

            if (Input.GetKeyDown(KeyCode.DownArrow))
                OnDown();
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
            OnDown();
        }
        public void Show(Card data, Texture cardFace, Material mat, List<int> cards = null, int cardIndex = -1)
        {
            if (data.Id == 0)
                return;
            //OnDown();
            this.cards = cards;
            this.cardIndex = cardIndex;
            manager.GetElement("ButtonLeft").SetActive(cards != null);
            manager.GetElement("ButtonRight").SetActive(cards != null);

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

            if (mat != null)
            {
                manager.GetElement<RawImage>("Card").texture = cardFace;
                manager.GetElement<RawImage>("Card").material = mat;
            }
            else
            {
                if(loadEnumerator != null)
                    StopCoroutine(loadEnumerator);
                loadEnumerator = LoadCardPictureAsync();
                StartCoroutine(loadEnumerator);
            }

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
                if ((origin.Type & (uint)CardType.Tuner) > 0)
                    manager.GetElement("Tuner").SetActive(true);
                else
                    manager.GetElement("Tuner").SetActive(false);
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

        IEnumerator loadEnumerator;

        IEnumerator LoadCardPictureAsync()
        {
            var mat = TextureManager.GetCardMaterial(code);
            mat.renderQueue = 3000;

            var task = TextureManager.LoadCardAsync(code);
            while (!task.IsCompleted)
                yield return null;
            mat.mainTexture = task.Result;

            manager.GetElement<RawImage>("Card").material = mat;
            manager.GetElement<RawImage>("Card").texture = task.Result;

            loadEnumerator = null;
        }

        public void GenerateCard()
        {
            if (!Directory.Exists(Program.cardPicPath))
                Directory.CreateDirectory(Program.cardPicPath);
            try
            {
                var texture = manager.GetElement<RawImage>("Card").texture;

                if (texture == null)
                    texture = manager.GetElement<RawImage>("Card").material.GetTexture("_MainTex");

                var picture = ((Texture2D)texture).EncodeToPNG();
                var fullPath = Program.cardPicPath + Program.slash + code + Program.pngExpansion;
                File.WriteAllBytes(Program.cardPicPath + Program.slash + code + Program.pngExpansion, picture);
                MessageManager.Cast(InterString.Get("卡图已保存于：[?]", fullPath));
            }
            catch
            {
                MessageManager.Cast(InterString.Get("没有写入权限，无法保存。"));
            }
        }

        public void OnLeft()
        {
            if(cards == null)
                return;
            if (cardIndex < 0)
                cardIndex = 0;

            cardIndex = (cardIndex + cards.Count - 1) % cards.Count;
            var data = CardsManager.Get(cards[cardIndex]);

            while(data.Id == code)
            {
                cardIndex = (cardIndex + cards.Count - 1) % cards.Count;
                data = CardsManager.Get(cards[cardIndex]);
            }
            Show(data, null, null, cards, cardIndex);
        }
        public void OnRight()
        {
            if (cards == null)
                return;
            if (cardIndex < 0)
                cardIndex = 0;

            cardIndex = (cardIndex + 1) % cards.Count;
            var data = CardsManager.Get(cards[cardIndex]);

            while (data.Id == code)
            {
                cardIndex = (cardIndex + 1) % cards.Count;
                data = CardsManager.Get(cards[cardIndex]);
            }
            Show(data, null, null, cards, cardIndex);
        }

        bool bigShowing = false;
        public void OnScale()
        {
            if(bigShowing)
                OnDown();
            else
                OnUp();
        }

        public void OnUp()
        {
            bigShowing = true;
#if UNITY_ANDROID
            BigShowMobile();
#else
            BigShowDesktop();
#endif
        }

        private void BigShowMobile()
        {
            var cardRect = manager.GetElement<RectTransform>("Card");
            var limit = manager.GetElement<Image>("Limit");
            limit.DOFade(0f, bigShowTime);
            var extraWidth = 1080f * Screen.width / Screen.height - 737f * 2f;
            cardRect.DOAnchorPos(new Vector2(extraWidth / 2f, -1035f), bigShowTime);
            cardRect.DOLocalRotate(new Vector3(0f, 0f, 90f), bigShowTime);
            cardRect.DOScale(2f, bigShowTime);
        }
        private void BigShowDesktop()
        {
            var cardRect = manager.GetElement<RectTransform>("Card");
            var limit = manager.GetElement<Image>("Limit");
            limit.DOFade(0f, bigShowTime);
            cardRect.DOAnchorPos(new Vector2(25f, -25f), bigShowTime);
            cardRect.DOLocalRotate(Vector3.zero, bigShowTime);
            cardRect.DOScale(1.4f, bigShowTime);

            var detailRect = manager.GetElement<RectTransform>("Detail");
            DOTween.To(() => detailRect.offsetMin.x, x => detailRect.offsetMin = new Vector2(x, 0f), 750f, bigShowTime);
        }
        public void OnDown()
        {
            bigShowing = false;
            var cardRect = manager.GetElement<RectTransform>("Card");
            var limit = manager.GetElement<Image>("Limit");
            limit.DOFade(1f, bigShowTime);
            cardRect.DOAnchorPos(new Vector2(60f, -145f), bigShowTime);
            cardRect.DOLocalRotate(Vector3.zero, bigShowTime);
            cardRect.DOScale(1f, bigShowTime);

            var detailRect = manager.GetElement<RectTransform>("Detail");
            DOTween.To(() => detailRect.offsetMin.x, x => detailRect.offsetMin = new Vector2(x, 0f), 630f, bigShowTime);
        }


        string TextForDetail(string text)
        {
            if(string.IsNullOrEmpty(text))
                text = string.Empty;

            if (!Language.UseLatin())
                return text.Replace(" ", "\u00A0");
            else
                return text;
        }
    }
}
