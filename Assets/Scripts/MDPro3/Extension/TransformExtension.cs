using UnityEngine;

namespace MDPro3
{
    public static class TransformExtension
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
                Object.Destroy(transform.GetChild(i).gameObject);
        }

        public static Transform GetChildByName(this Transform parent, string childName)
        {
            foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                if (t.name == childName)
                    return t;
            return null;
        }

        public static void DestroyChildByName(this Transform parent, string childName)
        {
            Object.Destroy(parent.GetChildByName(childName).gameObject);
        }

    }
}
