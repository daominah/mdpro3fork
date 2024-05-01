using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MDPro3.UI;

namespace MDPro3
{
    public class BackgroundManager : Manager
    {
        GameObject back;

        public static readonly Dictionary<int, string> backgrounds = new Dictionary<int, string>()
        {
            {1, "Classic" },
            {2, "Classic2" },
            {3, "PurpleDarkFantasy" },
            {5, "ClassicPurple" },
            {4, "ClassicPurpleShine" },
            //{6, "ClassicWhite" },
            {7, "WCS" },
            {8, "Shop" },
            {9, "Knowledge" },
            {10, "DeepDarkFantasy" },
        };


        public void Change(int id)
        {
            Destroy(back);
            if (id == 0)
            {
                var random = Random.Range(0, backgrounds.Count);
                id = Tools.GetNthElement(backgrounds, random).Key;
            }

            var endString = id.ToString("D4");
            back = ABLoader.LoadFromFolder("wallpaper/back" + endString, "Background" + endString, true);
            back.transform.GetChild(0).gameObject.AddComponent<AutoScale>();
            Tools.ChangeLayer(back, "2D");
            back.transform.SetParent(transform, false);
        }
        public int GetIDByName(string bgName)
        {
            var id = 0;
            if(bgName == InterString.Get("Ëæ»ú"))
                return 0;
            foreach (var background in backgrounds)
            {
                if(bgName == background.Value)
                {
                    id = background.Key;
                    break;
                }
            }
            return id;
        }
    }
}
