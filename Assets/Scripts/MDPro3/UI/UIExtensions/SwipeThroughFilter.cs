using UnityEngine;
using UnityEngine.EventSystems;

namespace MDPro3.UI
{
    public class SwipeThroughFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        private SwipeArea parent;

        private void Start()
        {
            parent = GetComponentInParent<SwipeArea>();
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (UserInput.MouseLeftDown)
            {
                if (parent != null)
                    parent.OnPointerDown(new BaseEventData(EventSystem.current));
            }
            else if (UserInput.MouseLeftUp)
            {
                if (parent != null)
                    parent.OnPointerUp(new BaseEventData(EventSystem.current));
            }

            return true;
        }

    }
}