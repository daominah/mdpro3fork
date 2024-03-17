using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3.UI
{
    public class ToggleForSearchFilter : Toggle
    {
        public int code;
        public int subCode;
        public int group;
        public long filterCode;

        private void Start()
        {
            if (code != 0)
            {
                label.text = StringHelper.GetUnsafe(code);
                if (subCode != 0)
                {
                    if (subCode == 9999)
                        label.text = InterString.Get("[?]Че", label.text);
                    else
                        label.text += StringHelper.GetUnsafe(subCode);
                }
            }
        }

        public override void OnClickOn()
        {
            base.OnClickOn();
            //AudioManager.PlaySE("SE_MENU_S_DECIDE_01");
        }
        public override void OnClickOff()
        {
            base.OnClickOff();
            //AudioManager.PlaySE("SE_MENU_S_DECIDE_02");
        }
    }
}
