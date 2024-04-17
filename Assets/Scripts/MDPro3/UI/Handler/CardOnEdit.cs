using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class CardOnEdit : MonoBehaviour
    {
        public Button button;
        public Image limitIcon;
        public GameObject pickup;
        float startX = -410f;
        float endX = -410f + 690f;
        float[] ys = new float[] { 246f, 136f, 26f, -84f, -253f, -419f };
        float[] ys2 = new float[] { 246f, 136f, 26f, -84f, -253f, -419f };
        public int id;

        public static float moveTime = 0.1f;

        private void Start()
        {
            var drag = button.gameObject.GetComponent<EventDrag>();
            drag.onClick = OnClick;
            drag.onClickRight = OnClickRight;
            drag.onDrag = OnDrag;
            drag.onBeginDrag = OnBeginDrag;
            drag.onEndDrag = OnEndDrag;
            drag.onPointerEnter = OnPointerEnter;
            drag.onPointerExit = OnPointerExit;
        }

        public void RefreshPosition()
        {
            GetComponent<RectTransform>().anchoredPosition = GetPosition();
            transform.localScale = Vector3.one * 1.3f;
            transform.DOScale(Vector3.one, 0.2f);
        }
        public void RefreshPositionInstant()
        {
            GetComponent<RectTransform>().anchoredPosition = GetPosition();
        }
        public Vector2 GetPosition()
        {
            if (Program.I().currentServant == Program.I().editDeck)
                startX = Program.I().editDeck.outerWidth + Program.I().editDeck.descriptionWidth + Program.I().editDeck.innerWidth - 910f;
            else
                startX = -410f;
            endX = startX + 690f;
            Vector2 position = Vector2.zero;
            int count = 0;
            if (id < 1000)
            {
                count = Program.I().editDeck.mainCount;
                if (count <= 40)
                {
                    position.x = startX + (id % 10) * (endX - startX) / 9;
                    position.y = ys[id / 10];
                }
                else
                {
                    int lineCount = (int)Math.Ceiling(count / 4f);
                    position.x = startX + (id % lineCount) * (endX - startX) / (lineCount - 1);
                    position.y = ys[id / lineCount];
                }
            }
            else if (id > 1999)
            {
                count = Program.I().editDeck.sideCount;
                if (count <= 10)
                    position.x = startX + (id - 2000) * (endX - startX) / 9;
                else
                    position.x = startX + (id - 2000) * (endX - startX) / (count - 1);
                position.y = ys[5];
            }
            else
            {
                count = Program.I().editDeck.extraCount;
                if (count <= 10)
                    position.x = startX + (id - 1000) * (endX - startX) / 9;
                else
                    position.x = startX + (id - 1000) * (endX - startX) / (count - 1);
                position.y = ys[4];
            }
            return position;
        }

        int m_code;
        public int code
        {
            get
            {
                return m_code;
            }
            set
            {
                m_code = value;
                RefreshLimitIcon();
                StartCoroutine(RefreshCard());
            }
        }

        IEnumerator RefreshCard()
        {
            GetComponent<RawImage>().texture = TextureManager.container.unknownCard.texture;
            var ie = Program.I().texture_.LoadCardAsync(code, true);
            while (ie.MoveNext())
                yield return null;
            GetComponent<RawImage>().texture = ie.Current;
            GetComponent<RawImage>().material = TextureManager.GetCardMaterial(code, true);
        }

        public void RefreshLimitIcon()
        {
            var limit = Program.I().editDeck.banlist.GetQuantity(code);
            if (limit == 3)
                limitIcon.sprite = TextureManager.container.typeNone;
            else if (limit == 2)
                limitIcon.sprite = TextureManager.container.limit2;
            else if (limit == 1)
                limitIcon.sprite = TextureManager.container.limit1;
            else
                limitIcon.sprite = TextureManager.container.banned;
        }
        public bool picked;
        public void PickUp(bool on)
        {
            Program.I().editDeck.dirty = true;
            picked = on;
            pickup.SetActive(on);
        }

        public bool dragging;

        void OnClick(PointerEventData eventData)
        {
            AudioManager.PlaySE("SE_DUEL_SELECT");

            if (Program.I().currentServant == Program.I().editDeck)
                Program.I().editDeck.Description(code, GetComponent<RawImage>().texture, GetComponent<RawImage>().material);
            else
                Program.I().appearance.PickThis(this);
        }

        void OnClickRight(PointerEventData eventData)
        {
            if (Program.I().currentServant == Program.I().editDeck)
                Program.I().editDeck.DeleteCard(this);
        }

        void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            transform.localScale = Vector3.one * 1.2f;
            transform.SetSiblingIndex(transform.parent.childCount - 1);
            button.GetComponent<Image>().raycastTarget = false;
        }
        void OnDrag(PointerEventData eventData)
        {
            var dragTarget = GetComponent<RectTransform>();
            Vector3 uiPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragTarget, eventData.position, eventData.enterEventCamera, out uiPosition);
            dragTarget.position = uiPosition;
            var anchoredPosition = dragTarget.anchoredPosition3D;
            dragTarget.anchoredPosition3D = (Vector2)anchoredPosition;
        }

        public void Move()
        {
            transform.DOScale(Vector3.one, moveTime);
            GetComponent<RectTransform>().DOAnchorPos3D(GetPosition(), moveTime);
        }


        void OnEndDrag(PointerEventData eventData)
        {
            Program.I().editDeck.RefreshCardID();
            dragging = false;
            button.GetComponent<Image>().raycastTarget = true;
        }

        public bool hover;
        void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }
        void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }
    }
}
