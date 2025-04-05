using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class DuelErrorLog : MonoBehaviour
    {
        private CanvasGroup cg;
        private CanvasGroup CG =>
            cg = cg != null ? cg : GetComponent<CanvasGroup>();

        private ScrollRect scrollRect;
        private ScrollRect ScrollRect =>
            scrollRect = scrollRect != null ? scrollRect
            : GetComponent<ScrollRect>();

        public TextMeshProUGUI text;

        private string lastMessage;

        public void Hide()
        {
            CG.alpha = 0f;
            cg.blocksRaycasts = false;
        }

        public void Show(string log)
        {
            lastMessage = text.text;
            text.text = lastMessage + "\r\n" + log;

            CG.alpha = 1f;
            cg.blocksRaycasts = true;
            ScrollRect.DOVerticalNormalizedPos(0f, 0.1f);
        }

    }
}