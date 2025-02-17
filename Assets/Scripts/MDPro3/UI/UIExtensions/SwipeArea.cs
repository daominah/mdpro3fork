using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SwipeArea : MonoBehaviour
    {
        public UnityEvent OnSwipeLeft;
        public UnityEvent OnSwipeRight;

        private Vector2 startTouchPosition;
        private Vector2 currentTouchPosition;
        private bool stopTouch = false;

        private float swipeRange = 50f; // »¬¶¯·¶Î§
        private float tapRange = 10f; // Çá´¥·¶Î§

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
    }
}
