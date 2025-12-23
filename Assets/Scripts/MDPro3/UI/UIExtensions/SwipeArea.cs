using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SwipeArea : MonoBehaviour, ICanvasRaycastFilter
    {
        public UnityEvent OnSwipeLeft;
        public UnityEvent OnSwipeRight;

        private Vector2 startTouchPosition;
        private Vector2 currentTouchPosition;
        private bool stopTouch = false;

        public float swipeRange = 50f;

        public void OnPointerDown(BaseEventData data)
        {
            startTouchPosition = UserInput.MousePos;
            stopTouch = false;
        }

        public void OnPointerUp(BaseEventData data)
        {
            stopTouch = true;
            DetectSwipe();
        }

        private void DetectSwipe()
        {
            if (stopTouch)
            {
                currentTouchPosition = UserInput.MousePos;
                Vector2 distance = currentTouchPosition - startTouchPosition;

                if (distance.magnitude > swipeRange)
                {
                    if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
                    {
                        if (distance.x > 0)
                        {
                            OnSwipeRight?.Invoke();
                        }
                        else if (distance.x < 0)
                        {
                            OnSwipeLeft?.Invoke();
                        }
                    }
                }
            }
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (UserInput.MouseLeftUp)
            {
                OnPointerUp(new BaseEventData(EventSystem.current));
            }

            return true;
        }

    }
}
