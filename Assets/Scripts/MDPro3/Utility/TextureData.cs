using UnityEngine;

namespace MDPro3.Utility
{
    public class TextureData
    {
        public Texture2D texture;
        public bool persistent;
        public bool loaded;
        public int referenceCount;

        public void AddReference()
        {
            referenceCount++;
        }

        public bool Delete()
        {
            referenceCount--;
            return ((referenceCount <= 0 && !persistent));
        }
    }
}
