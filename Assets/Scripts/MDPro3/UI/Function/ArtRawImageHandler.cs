using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MDPro3.Utility;

namespace MDPro3.UI
{
    public class ArtRawImageHandler : MonoBehaviour
    {
        public bool cache = true;

        protected int code = 0;
        protected bool m_Refreshed;
        public bool Refreshed => m_Refreshed;

        private RawImage m_RawImage;
        public RawImage RawImage =>
            m_RawImage = m_RawImage != null ? m_RawImage
            : GetComponent<RawImage>();

        protected Coroutine artLoadCoroutine;

        protected void OnDisable()
        {
            if(artLoadCoroutine != null)
                StopCoroutine(artLoadCoroutine);
        }

        protected void OnDestroy()
        {
            DeleteArt();
        }

        private void DeleteArt()
        {
            if(code != 0)
            {
                TextureLoader.DeleteArt(code);
                code = 0;
            }
        }

        public void SetArt(int art)
        {
            if (code == art)
                return;

            DeleteArt();
            code = art;

            if(artLoadCoroutine != null)
                StopCoroutine(artLoadCoroutine);
            artLoadCoroutine = StartCoroutine(LoadArtAsync());
        }

        protected virtual IEnumerator LoadArtAsync()
        {
            m_Refreshed = false;

            RawImage.texture = TextureManager.container.unknownArt.texture;

            var task = TextureLoader.LoadArtAsync(code, cache);
            while(!task.IsCompleted)
                yield return null;
            RawImage.texture = task.Result;

            artLoadCoroutine = null;
            m_Refreshed = true;
        }
    }
}
