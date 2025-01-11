using MDPro3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public const string LABEL_LEFT = "Left";
        public const string LABEL_RIGHT = "Right";
        public const string LABEL_REMOVEDECK = "RemoveDeck";
        public const string LABEL_ADDBOOKMARK = "AddBookmark";
        public const string LABEL_CANNOTADDBOOKMARK = "CanNotAddBookmark";
        public const string LABEL_MAINDECK = "MainDeck";
        public const string LABEL_EXTRADECK = "ExtraDeck";
        public const string LABEL_SIDEDECK = "SideDeck";

        public static string HoveringLabel;

        [SerializeField] private float alpha = 0.2f;
        [SerializeField] private string label;

        private bool _hover;
        public bool Hover => _hover;

        private Image m_Image;
        private Image Image => m_Image = m_Image != null 
            ? m_Image : GetComponent<Image>();

        private void OnDisable()
        {
            Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            if(UserInput.Draging && Image != null)
            {
                Image.color = new Color(1f, 1f, 1f, alpha);
                HoveringLabel = label;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            if (Image != null)
            {
                Image.color = Color.clear;
                if(HoveringLabel == label)
                    HoveringLabel = string.Empty;
            }
        }

        public void Hide()
        {
            if (Image != null)
                Image.color = Color.clear;
        }
    }
}
