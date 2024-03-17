using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3.UI
{
    public class ButtonListForAppearancePlayer : ButtonList
    {
        public override void SelectThis()
        {
            base.SelectThis();
            Program.I().appearance.SwitchPlayer(gameObject.name);
        }
    }
}
