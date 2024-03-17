using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MDPro3.UI
{
    public class ToggleForDeckDelete : Toggle
    {
        public override void SwitchOn()
        {
            base.SwitchOn();
            Program.I().selectDeck.DeckDelete();
        }
        public override void SwitchOff()
        {
            base.SwitchOff();
            Program.I().selectDeck.DeckDelete();
        }


    }
}
