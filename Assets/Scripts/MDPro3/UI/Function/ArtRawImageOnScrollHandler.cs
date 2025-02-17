using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MDPro3.Utility;

namespace MDPro3.UI
{
    public class ArtRawImageOnScrollHandler : ArtRawImageHandler
    {
        protected override IEnumerator LoadArtAsync()
        {
            m_Refreshed = false;

            RawImage.texture = TextureManager.container.unknownArt.texture;

            yield return null;
            while (OnScrollSetFreeze.Freeze)
                yield return null;

            var task = TextureLoader.LoadArtAsync(code, cache);
            while (!task.IsCompleted)
                yield return null;
            RawImage.texture = task.Result;

            artLoadCoroutine = null;
            m_Refreshed = true;
        }
    }
}