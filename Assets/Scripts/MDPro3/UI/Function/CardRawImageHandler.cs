using DG.Tweening;
using MDPro3.Duel.YGOSharp;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.Utility;
using System.Threading;
using System.Threading.Tasks;

namespace MDPro3.UI
{
    [RequireComponent(typeof(RawImage))]
    public class CardRawImageHandler : MonoBehaviour
    {
        public bool cache;

        public Card card;

        public int protectorCode = 1070001;

        protected bool m_Refreshed;
        public bool Refreshed => m_Refreshed;

        private RawImage m_RawImage;
        public RawImage RawImage =>
            m_RawImage = m_RawImage != null ? m_RawImage
            : GetComponent<RawImage>();

        protected Material normalMat;
        protected Material tempMat;

        private CancellationTokenSource cts;

        protected Tween matTweener;

        protected void OnDestroy()
        {
            Destroy(normalMat);
            Destroy(tempMat);
            DeleteCard();
        }

        private void DeleteCard()
        {
            if (card != null)
            {
                CardImageLoader.ReleaseCard(card.Id);
                card = null;
            }
        }

        public void SetCard(int code)
        {
            if (code <= 0)
            {
                card = null;
                SetProtector(protectorCode);
            }
            else
                SetCard(CardsManager.Get(code));
        }

        public void SetCard(Card data)
        {
            if (card != null && card.Id == data.Id)
                return;
            DeleteCard();
            card = data;

            _ = LoadCardPicAsync();
        }

        private async Task LoadCardPicAsync()
        {
            m_Refreshed = false;

            if(normalMat == null)
            {
                var matLoad = MaterialLoader.LoadCardMaterialAsync(-1);
                while (!matLoad.IsCompleted)
                    await TaskUtility.WaitOneFrame(gameObject);
                normalMat = matLoad.Result;
            }
            normalMat.SetTexture("_LoadingTex", TextureManager.container
                .GetCardLoadingTexture(CardsManager.Get(card.Id)));

            if (matTweener != null && matTweener.IsActive())
                matTweener.Kill();
            normalMat.SetFloat("_LoadingBlend", 1f);
            RawImage.material = normalMat;

            if (tempMat != null)
                Destroy(tempMat);

            CancelLoading();
            cts = new CancellationTokenSource();
            var task = CardImageLoader.LoadCardAsync(card.Id, cache, cts.Token);

            while (!task.IsCompleted)
                await TaskUtility.WaitOneFrame(gameObject, cts.Token);
            RawImage.texture = task.Result;

            if (CardRarity.GetRarity(card.Id) == CardRarity.Rarity.Normal)
                matTweener = normalMat.DOFloat(0f, "_LoadingBlend", 0.1f);
            else
                await LoadMatAsync(0.1f);

            m_Refreshed = true;
        }

        public void RefreshRarity(int code)
        {
            if (card.Id != code)
                return;
            if(tempMat != null)
                DestroyImmediate(tempMat);
            if (CardRarity.GetRarity(card.Id) == CardRarity.Rarity.Normal)
            {
                normalMat.SetFloat("_LoadingBlend", 0f);
                RawImage.material = normalMat;
            }
            else
                _ = LoadMatAsync(0f);
        }

        protected async Task LoadMatAsync(float fadeTime)
        {
            var task = MaterialLoader.LoadCardMaterialAsync(card.Id);
            while (!task.IsCompleted)
                await TaskUtility.WaitOneFrame(gameObject);

            tempMat = task.Result;
            tempMat.SetFloat("_LoadingBlend", 1f);
            tempMat.SetTexture("_LoadingTex"
                , normalMat.GetTexture("_LoadingTex"));
            RawImage.material = tempMat;
            if(matTweener != null && matTweener.IsActive())
                matTweener.Kill();
            matTweener = tempMat.DOFloat(0f, "_LoadingBlend", fadeTime);
        }

        public void SetProtector(int code)
        {
            protectorCode = code;
            _ = LoadProtectorAsync(code);
        }

        private async Task LoadProtectorAsync(int code)
        {
            m_Refreshed = false;

            var im = ABLoader.LoadProtectorMaterial(code.ToString());
            while (im.MoveNext())
                await TaskUtility.WaitOneFrame(gameObject);

            RawImage.material = im.Current;
            m_Refreshed = true;

            RawImage.texture = null;
            DeleteCard();

            m_Refreshed = true;
        }

        private void CancelLoading()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}
