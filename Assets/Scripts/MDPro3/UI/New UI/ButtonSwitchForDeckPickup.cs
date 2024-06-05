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
            if(Program.I().currentServant == Program.I().selectDeck)
                Program.I().selectDeck.hoverOn = true;
            else
                Program.I().onlineDeckViewer.hoverOn = true;
        }

        public override void OnSwitchOff()
        {
            base.OnSwitchOff();
            if (Program.I().currentServant == Program.I().selectDeck)
                Program.I().selectDeck.hoverOn = false;
            else
                Program.I().onlineDeckViewer.hoverOn = false;
        }
    }
}
