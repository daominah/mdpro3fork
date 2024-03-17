using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MDPro3.UI;

namespace MDPro3
{
    public class BackgroundManager : Manager
    {
        static GameObject back;

        public override void Initialize()
        {
            base.Initialize();
            back = ABLoader.LoadFromFile("wallpaper/back/back0007");
            if (back == null)
                return;
            back.AddComponent<AutoScale>();
            Tools.ChangeLayer(back, "2D");
            back.transform.SetParent(transform);
        }

        public static void Change(int id)
        {
            var back = ABLoader.LoadFromFile("wallpaper/back/back000" + id);
            if (back == null) return;
            else
            {
                Object.Destroy(BackgroundManager.back);
                BackgroundManager.back = back;
            }
        }
    }
}
