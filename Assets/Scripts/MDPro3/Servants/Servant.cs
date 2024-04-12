using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;

namespace MDPro3 
{
    public class Servant : MonoBehaviour
    {
        [HideInInspector]
        public bool isShowed;
        [HideInInspector]
        public int depth;
        [HideInInspector]
        public bool haveLine;
        [HideInInspector]
        public float transitionTime = 0.4f;
        [HideInInspector]
        public float blackAlpha = 0f;
        [HideInInspector]
        public float subBlackAlpha = 0f;
        [HideInInspector]
        public bool inTransition;
        [HideInInspector]
        public CanvasGroup cg;
        [HideInInspector]
        public Servant returnServant;
        public Action returnAction;

        float startX;

        //右键、ESC、退出按钮等触发OnReturn,
        //returnAction为空时OnReturn触发OnExit退出， 不为空时执行returnAction，
        //OnExit控制单独退出(Hide)还是切换Servant
        //Hide -> ApplyHideArrangement

        //Program.ShiftServant 调用show 和 hide

        public virtual void Initialize()
        {
            if (gameObject != null)
            {
                startX = GetComponent<RectTransform>().anchoredPosition.x;
                cg = GetComponent<CanvasGroup>();
                if (cg == null) return;
                if (depth == 0)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    Program.I().currentServant = this;
                    Program.I().depth = 0;
                    isShowed = true;
                    Program.I().ui_.btnExit.GetComponent<RectTransform>().anchoredPosition = new Vector2(65, 65);
                    Program.I().ui_.line.alpha = 0f;
                }
                else
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
            }
        }

        public virtual void Show(int preDepth)
        {
            if (!isShowed)
            {
                isShowed = true;
                ApplyShowArrangement(preDepth);
            }
        }
        public virtual void ApplyShowArrangement(int preDepth)
        {
            bool blackTransition = false;
            if (Program.I().currentServant == this && preDepth == -1)
                blackTransition = true;

            if (blackTransition)
            {
                if (cg != null)
                {
                    DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                    {
                        cg.alpha = 1f;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                        if (depth <= 0 || (this == Program.I().editDeck && Program.I().editDeck.condition == EditDeck.EditDeckCondition.ChangeSide))
                            UIManager.HideExitButton(0);
                        else
                            UIManager.ShowExitButton(0);
                        if (haveLine)
                            UIManager.ShowLine(0);
                        else
                            UIManager.HideLine(0);
                        RectTransform t = GetComponent<RectTransform>();
                        t.eulerAngles = Vector3.zero;
                        t.DOAnchorPosX(startX, 0);
                    });
                }
            }
            else
            {
                inTransition = true;
                if (cg != null)
                {
                    cg.interactable = true;
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 1, transitionTime);
                    RectTransform t = GetComponent<RectTransform>();

                    if (depth < preDepth)
                    {
                        t.eulerAngles = new Vector3(0f, 30f, 0f);
                        t.anchoredPosition = new Vector2(startX - 300, t.anchoredPosition.y);
                        t.DORotate(new Vector3(0f, 0f, 0f), transitionTime).SetEase(Ease.OutCubic);
                        t.DOAnchorPosX(startX, transitionTime).SetEase(Ease.OutCubic);
                    }
                    else
                    {
                        t.eulerAngles = new Vector3(0f, -30f, 0f);
                        t.anchoredPosition = new Vector2(startX + 300, t.anchoredPosition.y);
                        t.DORotate(new Vector3(0f, 0f, 0f), transitionTime).SetEase(Ease.OutCubic);
                        t.DOAnchorPosX(startX, transitionTime).SetEase(Ease.OutCubic);
                    }
                }
                if (depth <= 0)
                    UIManager.HideExitButton(transitionTime);
                else
                    UIManager.ShowExitButton(transitionTime);
                if (haveLine)
                    UIManager.ShowLine(transitionTime);
                else
                    UIManager.HideLine(transitionTime);
                Program.I().ui_.blackBack.DOFade(Program.I().currentServant.depth == -1 ? subBlackAlpha : blackAlpha, transitionTime)
                    .OnComplete(() =>
                    {
                        inTransition = false;
                        if (cg != null)
                        {
                            cg.blocksRaycasts = true;
                        }
                        if (depth > 0)
                            Program.I().ui_.blackBack.raycastTarget = true;
                    });
            }
        }


        public virtual void Hide(int preDepth)
        {
            if (isShowed)
            {
                isShowed = false;
                ApplyHideArrangement(preDepth);
            }
        }

        public virtual void ApplyHideArrangement(int preDepth)
        {
            bool blackTransition = false;
            if (preDepth == -1)
                blackTransition = true;

            if (blackTransition)
            {
                if (cg != null)
                {
                    DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                    {
                        cg.alpha = 0f;
                        cg.interactable = false;
                        cg.blocksRaycasts = false;
                    });
                }
            }
            else
            {
                inTransition = true;
                if (cg != null)
                {
                    cg.blocksRaycasts = false;
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0, transitionTime);

                    RectTransform t = GetComponent<RectTransform>();
                    if (depth < preDepth)
                    {
                        t.DORotate(new Vector3(0f, 30f, 0f), transitionTime);
                        t.DOAnchorPosX(startX - 300, transitionTime);
                    }
                    else
                    {
                        t.DORotate(new Vector3(0f, -30f, 0f), transitionTime);
                        t.DOAnchorPosX(startX + 300, transitionTime);
                    }
                }
                DOTween.To(v => { }, 0, 0, transitionTime).OnComplete(() =>
                {
                    if (cg != null)
                    {
                        inTransition = false;
                        cg.interactable = false;
                    }
                });
            }
            if (preDepth <= 0)
            {
                UIManager.HideExitButton(transitionTime);
                Program.I().ui_.blackBack.DOFade(0, transitionTime).OnComplete(() =>
                {
                    Program.I().ui_.blackBack.raycastTarget = false;
                });
            }
        }

        [HideInInspector]
        public int exitPressedTime;

        public virtual void PerFrameFunction()
        {
            if (isShowed)
            {
                if (
                    Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
                    //|| Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame)
                    || Input.GetKeyDown(KeyCode.Escape) //|| Input.GetKeyDown(KeyCode.Backspace)
                    || Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame
                    )
                {
                    exitPressedTime = Program.TimePassed();
                }
                if (
                Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame
                //|| Keyboard.current != null && (Keyboard.current.escapeKey.wasReleasedThisFrame || Keyboard.current.backspaceKey.wasReleasedThisFrame)
                || Input.GetKeyUp(KeyCode.Escape) //|| Input.GetKeyUp(KeyCode.Backspace)
                || Gamepad.current != null && Gamepad.current.bButton.wasReleasedThisFrame
                )
                {
                    if (Program.TimePassed() - exitPressedTime < 300)
                        OnReturn();
                }

            }
        }

        public virtual void OnReturn()
        {
            if (inTransition) return;
            AudioManager.PlaySE("SE_MENU_CANCEL");
            if (returnAction != null)
            {
                returnAction.Invoke();
                return;
            }
            else
                OnExit();
        }

        public virtual void OnExit()
        {
            if (Program.I().currentSubServant == this)
            {
                if (returnServant == Program.I().menu)//Setting
                {
                    Hide(0);
                    Program.I().currentSubServant = null;
                }
                else//Appearance
                    Program.I().ShowSubServant(returnServant);
            }
            else
                Program.I().ShiftToServant(returnServant);
        }
    }

}

