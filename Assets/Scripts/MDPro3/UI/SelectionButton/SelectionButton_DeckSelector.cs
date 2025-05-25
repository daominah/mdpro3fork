using DG.Tweening;
using MDPro3.Duel.YGOSharp;
using MDPro3.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SelectionButton_DeckSelector : SelectionButton
    {
        #region Elements

        private const string LABEL_TEXT_DECK_NAME = "TextDeckName";
        private TextMeshProUGUI m_TextDeckName;
        private TextMeshProUGUI TextDeckName =>
            m_TextDeckName = m_TextDeckName != null ? m_TextDeckName
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TEXT_DECK_NAME);

        private const string LABEL_IMG_DECK = "DeckImage";
        private Image m_ImageDeck;
        private Image ImageDeck =>
            m_ImageDeck = m_ImageDeck != null ? m_ImageDeck
            : Manager.GetElement<Image>(LABEL_IMG_DECK);

        private const string LABEL_RIMG_CARD0 = "CardImage0";
        private RawImage m_ImageCard0;
        private RawImage ImageCard0 =>
            m_ImageCard0 = m_ImageCard0 != null ? m_ImageCard0
            : Manager.GetElement<RawImage>(LABEL_RIMG_CARD0);
        private RectTransform m_CardPos0RT;
        private RectTransform CardPos0RT =>
            m_CardPos0RT = m_CardPos0RT != null ? m_CardPos0RT
            : Manager.GetElement<RectTransform>(LABEL_RIMG_CARD0);

        private const string LABEL_RIMG_CARD1 = "CardImage1";
        private RawImage m_ImageCard1;
        private RawImage ImageCard1 =>
            m_ImageCard1 = m_ImageCard1 != null ? m_ImageCard1
            : Manager.GetElement<RawImage>(LABEL_RIMG_CARD1);
        private RectTransform m_CardPos1RT;
        private RectTransform CardPos1RT =>
            m_CardPos1RT = m_CardPos1RT != null ? m_CardPos1RT
            : Manager.GetElement<RectTransform>(LABEL_RIMG_CARD1);

        private const string LABEL_RIMG_CARD2 = "CardImage2";
        private RawImage m_ImageCard2;
        private RawImage ImageCard2 =>
            m_ImageCard2 = m_ImageCard2 != null ? m_ImageCard2
            : Manager.GetElement<RawImage>(LABEL_RIMG_CARD2);
        private RectTransform m_CardPos2RT;
        private RectTransform CardPos2RT =>
            m_CardPos2RT = m_CardPos2RT != null ? m_CardPos2RT
            : Manager.GetElement<RectTransform>(LABEL_RIMG_CARD2);

        private const string LABEL_CG_CARD_POS0 = "CardPos0";
        private CanvasGroup m_CardPos0;
        private CanvasGroup CardPos0 =>
            m_CardPos0 = m_CardPos0 != null ? m_CardPos0
            : Manager.GetElement<CanvasGroup>(LABEL_CG_CARD_POS0);

        private const string LABEL_CG_CARD_POS1 = "CardPos1";
        private CanvasGroup m_CardPos1;
        private CanvasGroup CardPos1 =>
            m_CardPos1 = m_CardPos1 != null ? m_CardPos1
            : Manager.GetElement<CanvasGroup>(LABEL_CG_CARD_POS1);

        private const string LABEL_CG_CARD_POS2 = "CardPos2";
        private CanvasGroup m_CardPos2;
        private CanvasGroup CardPos2 =>
            m_CardPos2 = m_CardPos2 != null ? m_CardPos2
            : Manager.GetElement<CanvasGroup>(LABEL_CG_CARD_POS2);

        #endregion

        private IEnumerator refreshInstance;
        private List<Tweener> pickupTweens = new();
        private List<Tweener> pickdownTweens = new();

        protected override void Awake()
        {
            base.Awake();
            HidePickup();
        }

        private void OnEnable()
        {
            if (refreshInstance != null)
                StartCoroutine(refreshInstance);
        }

        public void SetConfigDeck(string hint)
        {
            var configDeck = Config.GetConfigDeckName();
            var path = Program.PATH_DECK + configDeck + Program.EXPANSION_YDK;
            if (!File.Exists(path))
                SetDeck(null, hint);
            else
                SetDeck(new Deck(path), configDeck);
        }

        public void SetDeck(Deck deck,string deckName) 
        {
            TextDeckName.text = deckName;

            IEnumerator ie;
            if (deck == null)
            {
                ie = RefreshAsync();
            }
            else
            {
                ie = RefreshAsync(
                    deck.Case,
                    deck.Protector,
                    deck.Pickup.Count > 0 ? deck.Pickup[0] : 0,
                    deck.Pickup.Count > 1 ? deck.Pickup[1] : 0,
                    deck.Pickup.Count > 2 ? deck.Pickup[2] : 0);
            }

            if (gameObject.activeInHierarchy)
                StartCoroutine(ie);
            else
                refreshInstance = ie;
        }

        private IEnumerator RefreshAsync(int deckCase = 1080001, int protector = 1070001, int card0 = 0, int card1 = 0, int card2 = 0)
        {
            while (!Items.initialized)
                yield return null;

            while (TextureManager.container == null)
                yield return null;

            ImageDeck.color = Color.clear;
            ImageCard0.color =Color.clear;
            ImageCard1.color = Color.clear;
            ImageCard2.color = Color.clear;

            var load = Program.items.LoadDeckCaseIconAsync(deckCase, "_L_SD");
            while (!load.IsCompleted)
                yield return null;
            ImageDeck.sprite = load.Result;
            ImageDeck.color = Color.white;

            if (card0 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                ImageCard0.texture = null;
                ImageCard0.material = ie2.Current;
            }
            else
            {
                var matLoad = MaterialLoader.LoadCardMaterialAsync(card0);
                while (!matLoad.IsCompleted)
                    yield return null;
                var mat = matLoad.Result;
                var task = CardImageLoader.LoadCardAsync(card0, true);
                while (!task.IsCompleted)
                    yield return null;
                ImageCard0.material = mat;
                ImageCard0.texture = task.Result;
            }
            ImageCard0.color = Color.white;

            if (card1 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                ImageCard1.texture = null;
                ImageCard1.material = ie2.Current;
            }
            else
            {
                var matLoad = MaterialLoader.LoadCardMaterialAsync(card1);
                while (!matLoad.IsCompleted)
                    yield return null;
                var mat = matLoad.Result;
                var task = CardImageLoader.LoadCardAsync(card1, true);
                while (!task.IsCompleted)
                    yield return null;
                ImageCard1.material = mat;
                ImageCard1.texture = task.Result;
            }
            ImageCard1.color = Color.white;

            if (card2 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                ImageCard2.texture = null;
                ImageCard2.material = ie2.Current;
            }
            else
            {
                var matLoad = MaterialLoader.LoadCardMaterialAsync(card2);
                while (!matLoad.IsCompleted)
                    yield return null;
                var mat = matLoad.Result;
                var task = CardImageLoader.LoadCardAsync(card2, true);
                while (!task.IsCompleted)
                    yield return null;
                ImageCard2.material = mat;
                ImageCard2.texture = task.Result;
            }
            ImageCard2.color = Color.white;

            refreshInstance = null;
        }

        protected override void CallHoverOnEvent()
        {
            base.CallHoverOnEvent();
            ShowPickup();
        }

        protected override void CallHoverOffEvent()
        {
            base.CallHoverOffEvent();
            HidePickup();
        }

        private void ShowPickup()
        {
            foreach (var tween in pickdownTweens)
                if (tween.IsActive())
                    tween.Kill();
            pickdownTweens.Clear();

            var tween1 = CardPos0.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween1);
            var tween2 = CardPos1.DOFade(1f, 0.22f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween2);
            var tween3 = CardPos2.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween3);

            var tween4 = CardPos0RT.DOAnchorPos3D(new Vector3(0f, 10f, 0f), 0.2f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween4);
            var tween5 = CardPos1RT.DOAnchorPos3D(new Vector3(0f, 10f, 0f), 0.22f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween5);
            var tween6 = CardPos2RT.DOAnchorPos3D(new Vector3(0f, 10f, 0f), 0.24f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween6);

            var tween7 = CardPos0RT.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween7);
            var tween8 = CardPos2RT.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutCubic);
            pickupTweens.Add(tween8);
        }

        private void HidePickup()
        {
            foreach (var tween in pickupTweens)
                if (tween.IsActive())
                    tween.Kill();
            pickupTweens.Clear();

            var tween1 = CardPos0.DOFade(0f, 0.2f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween1);
            var tween2 = CardPos1.DOFade(0f, 0.22f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween2);
            var tween3 = CardPos2.DOFade(0f, 0.24f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween3);

            var tween4 = CardPos0RT.DOAnchorPos3D(new Vector3(0f, -40f, 0f), 0.2f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween4);
            var tween5 = CardPos1RT.DOAnchorPos3D(new Vector3(0f, -40f, 0f), 0.22f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween5);
            var tween6 = CardPos2RT.DOAnchorPos3D(new Vector3(0f, -40f, 0f), 0.24f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween6);

            var tween7 = CardPos0RT.DOLocalRotate(new Vector3(0f, 0f, -20f), 0.2f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween7);
            var tween8 = CardPos2RT.DOLocalRotate(new Vector3(0f, 0f, 20f), 0.2f).SetEase(Ease.OutCubic);
            pickdownTweens.Add(tween8);
        }


    }
}
