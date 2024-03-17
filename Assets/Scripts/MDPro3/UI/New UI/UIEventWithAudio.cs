using UnityEngine;
using UnityEngine.EventSystems;

namespace MDPro3.UI
{
    public class UIEventWithAudio : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public AudioClip previewClip;
        public string enterAudio;
        public string clickAudio;
        public string exitAudio;

        public enum AudioType
        {
            SE,
            BGM,
            Voice
        }
        public AudioType audioType = AudioType.SE;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                PlayAudio(clickAudio);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayAudio(enterAudio);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayAudio(exitAudio);
        }

        private void PlayAudio(string path)
        {
            if (path == "")
                return;
            if (audioType == AudioType.SE)
                AudioManager.PlaySE(path);
            else if (audioType == AudioType.Voice)
                AudioManager.PlayVoice(path);
        }

    }
}
