using System.Collections;
using DG.Tweening;
using MDPro3.Duel.YGOSharp;
using UnityEngine;
using MDPro3.Utility;

namespace MDPro3.UI
{
    public class CardRawImageOnScrollHandler : CardRawImageHandler
    {
        protected override IEnumerator LoadCardPicAsync()
        {
            m_Refreshed = false;

            if (normalMat == null)
                normalMat = TextureManager.GetCardMaterial(-1);
            normalMat.SetTexture("_LoadingTex", TextureManager.container
                .GetCardLoadingTexture(CardsManager.Get(card.Id)));

            if (matTweener != null && matTweener.IsActive())
                matTweener.Kill();
            normalMat.SetFloat("_LoadingBlend", 1f);
            RawImage.material = normalMat;

            yield return null;
            while (OnScrollSetFreeze.Freeze)
                yield return null;

            if (tempMat != null)
                DestroyImmediate(tempMat);

            var task = TextureLoader.LoadCardAsync(card.Id, cache);
            while (!task.IsCompleted)
                yield return null;
            RawImage.texture = task.Result;

            if (CardRarity.GetRarity(card.Id) == CardRarity.Rarity.Normal)
                matTweener = normalMat.DOFloat(0f, "_LoadingBlend", 0.1f);
            else
            {
                if (matLoadCoroutine != null)
                    StopCoroutine(matLoadCoroutine);

                var coroutine = LoadMatAsync(0.1f);
                matLoadCoroutine = StartCoroutine(coroutine);
                while (coroutine.MoveNext())
                    yield return null;
            }

            picLoadCoroutine = null;
            m_Refreshed = true;
        }
    }
}