using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MDPro3.UI
{
    public class ButtonList : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Sprite normalSprite;
        public Sprite hoverSprite;
        public Sprite selectedSprite;
        public bool defaultButton;
        public ScrollRect scrollRect;

        bool selected;
        Text text;
        Color textColor;
        public float pressColor = 0.5f;
        private void Awake()
        {
            text = transform.GetChild(0).GetComponent<Text>();
            if (text != null)
                textColor = text.color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!selected)
            {
                selected = true;
                SelectThis();
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!selected)
                GetComponent<Image>().sprite = hoverSprite;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!selected)
                GetComponent<Image>().sprite = normalSprite;
        }

        public virtual void SelectThis()
        {
            selected = true;
            GetComponent<Image>().sprite = selectedSprite;
            text.color = Color.black;
            if (scrollRect != null)
            {
                scrollRect.gameObject.SetActive(true);
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
            foreach (var btn in transform.parent.GetComponentsInChildren<ButtonList>())
            {
                if (btn != this)
                    btn.UnselectThis();
            }

        }
        public void UnselectThis()
        {
            selected = false;
            GetComponent<Image>().sprite = normalSprite;
            text.color = Color.white;
            if (scrollRect != null)
                scrollRect.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                if (text != null & !selected)
                    text.color = new Color(textColor.r * pressColor, textColor.g * pressColor, textColor.b * pressColor, 1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                if (text != null && !selected)
                    text.color = textColor;
        }
    }
}
