using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace MDPro3.UI
{
    public class Popup : MonoBehaviour
    {
        [Header("Popup Reference")]
        public Image shadow;
        public RectTransform window;
        public Text title;
        public float transitionTime = 0.3f;
        public Button btnConfirm;
        public Button btnCancel;
        public float buttomOffest = 180;

        [HideInInspector]
        public List<string> selections;

        public Action whenQuitDo;

        public virtual void Initialize()
        {
            if (shadow != null)
                shadow.color = new Color(0f, 0f, 0f, 0);
            window.anchoredPosition = new Vector2(0f, -1100f);
            UIManager.Translate(gameObject);

            if (btnConfirm != null && btnCancel != null)
            {
                bool confirmOnLeft = Config.Get("Confirm", "1") == "1";
                float height = btnConfirm.GetComponent<RectTransform>().anchoredPosition.y;
                if (!confirmOnLeft)
                {
                    btnConfirm.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttomOffest, height);
                    btnCancel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-buttomOffest, height);
                }
            }
            if (btnConfirm != null)
                btnConfirm.onClick.AddListener(OnConfirm);
            if (btnCancel != null)
                btnCancel.onClick.AddListener(OnCancel);
            Program.I().currentServant.returnAction = Hide;
            InitializeSelections();
        }
        public virtual void InitializeSelections()
        {
            if (title != null)
                title.text = selections[0];
        }

        public virtual void Show()
        {
            Initialize();
            window.DOAnchorPos(Vector2.zero, transitionTime);
            if (shadow != null)
            {
                shadow.DOFade(0.8f, transitionTime);
                shadow.raycastTarget = true;
            }
        }

        public virtual void Hide()
        {
            if (shadow != null)
                shadow.DOFade(0f, transitionTime);
            var servant = Program.I().currentServant;
            window.DOAnchorPos(new Vector2(0f, -1100f), transitionTime).OnComplete(() =>
            {
                Destroy(gameObject);
                servant.returnAction = null;
                whenQuitDo?.Invoke();
            });
        }
        public virtual void OnConfirm()
        {
            AudioManager.PlaySE("SE_MENU_DECIDE");
        }

        public virtual void OnCancel()
        {
            AudioManager.PlaySE("SE_MENU_CANCEL");
        }
    }
}
