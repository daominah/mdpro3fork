using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace MDPro3.UI
{
    public class PopupDuel : Popup
    {
        [Header("Popup Duel Reference")]
        public Button btnHide;
        [HideInInspector]
        public bool exitable;
        bool hided;

        public override void Initialize()
        {
            base.Initialize();
            Program.I().ocgcore.allGameObjects.Add(gameObject);
            if (btnHide != null)
                btnHide.onClick.AddListener(FieldView);
            Program.I().ocgcore.currentPopup = this;
            if (!exitable)
            {
                if (btnHide != null)
                    Program.I().ocgcore.returnAction = FieldView;
                else
                    Program.I().ocgcore.returnAction = () => { };
            }
        }
        public float tempHideHeight = -390;
        public virtual void FieldView()
        {
            if (hided)
            {
                hided = false;
                window.DOAnchorPosY(0, transitionTime);
                AudioManager.PlaySE("SE_MENU_SLIDE_03");
                btnHide.transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                hided = true;
                window.DOAnchorPosY(tempHideHeight, transitionTime);
                AudioManager.PlaySE("SE_MENU_SLIDE_04");
                btnHide.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        public override void Show()
        {
            Initialize();
            window.DOAnchorPos(Vector2.zero, transitionTime);
            if (shadow != null)
            {
                shadow.DOFade(0.8f, transitionTime);
                shadow.raycastTarget = true;
            }
        }
        public override void Hide()
        {
            if (shadow != null)
                shadow.DOFade(0f, transitionTime);
            window.DOAnchorPos(new Vector2(0f, -1100f), transitionTime).OnComplete(() =>
            {
                Destroy(gameObject);
                Program.I().ocgcore.returnAction = null;
                whenQuitDo?.Invoke();
            });
            Program.I().ocgcore.Sleep((int)(transitionTime * 100));
            Program.I().ocgcore.currentPopup = null;
        }
        public override void OnConfirm()
        {
            AudioManager.PlaySE("SE_DUEL_DECIDE");
        }
        public override void OnCancel()
        {
            AudioManager.PlaySE("SE_DUEL_CANCEL");
        }
    }
}
