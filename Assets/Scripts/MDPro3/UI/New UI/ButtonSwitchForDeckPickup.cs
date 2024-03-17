using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3.UI
{
    public class ButtonSwitchForDeckPickup : ButtonSwitch
    {
        public override void OnSwitchOn()
        {
            base.OnSwitchOn();
            Program.I().selectDeck.hoverOn = true;
        }

        public override void OnSwitchOff()
        {
            base.OnSwitchOff();
            Program.I().selectDeck.hoverOn = false;
        }
    }
}
