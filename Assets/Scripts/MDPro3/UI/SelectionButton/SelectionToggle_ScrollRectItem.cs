using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MDPro3.UI
{
    public class SelectionToggle_ScrollRectItem : SelectionToggle
    {
        [HideInInspector] public bool refreshed;
        protected float switchTime = 0.2f;
        protected bool simpleMove = true;

        protected CancellationTokenSource cts;

        protected override void OnDisable()
        {
            base.OnDisable();
            Cancel();
        }

        #region Input Reaction

        protected override void Awake()
        {
            base.Awake();
            exclusiveToggle = true;
            canToggleOffSelf = false;
            toggleWhenSelected = true;
        }

        protected override void ToggleOn()
        {
            base.ToggleOn();
            Manager.GetElement<RectTransform>("Offset").DOAnchorPosX(48f, switchTime).SetEase(Ease.OutQuart);
        }

        protected override void ToggleOff()
        {
            base.ToggleOff();
            Manager.GetElement<RectTransform>("Offset").DOAnchorPosX(0f, switchTime).SetEase(Ease.OutQuart);
        }

        #endregion

        #region Public Function

        public virtual void ToggleOnNow()
        {
            isOn = true;
            Manager.GetElement<RectTransform>("Offset").DOAnchorPosX(48f, 0f);
        }

        public virtual void ToggleOffNow()
        {
            isOn = false;
            Manager.GetElement<RectTransform>("Offset").DOAnchorPosX(0f, 0f);
        }

        public virtual void Dispose()
        {
            Destroy(gameObject);
        }

        public virtual void Refresh()
        {
            Cancel();

            if (gameObject.activeInHierarchy)
            {
                cts = new();
                _ = RefreshAsync();
            }
        }

        protected virtual async UniTask RefreshAsync()
        {
            refreshed = false;
            await UniTask.Yield();
            refreshed = true;
        }

        private void Cancel()
        {
            try
            {
                cts?.Cancel();
                cts?.Dispose();
            }
            finally
            {
                cts = null;
            }
        }

        #endregion

        protected override void OnNavigation(AxisEventData eventData)
        {
            if (simpleMove)
            {
                if (eventData.moveDir == MoveDirection.Up)
                {
                    for (int i = 0; i < transform.parent.childCount; i++)
                    {
                        if (transform.parent.GetChild(i).GetComponent<SelectionToggle_ScrollRectItem>().index == index - 1)
                        {
                            UserInput.NextSelectionIsAxis = true;
                            EventSystem.current.SetSelectedGameObject(transform.parent.GetChild(i).gameObject);
                            break;
                        }
                    }
                }
                else if (eventData.moveDir == MoveDirection.Down)
                {
                    for (int i = 0; i < transform.parent.childCount; i++)
                    {
                        if (transform.parent.GetChild(i).GetComponent<SelectionToggle_ScrollRectItem>().index == index + 1)
                        {
                            UserInput.NextSelectionIsAxis = true;
                            EventSystem.current.SetSelectedGameObject(transform.parent.GetChild(i).gameObject);
                            break;
                        }
                    }
                }
            }
            else
                base.OnNavigation(eventData);
        }

    }
}
