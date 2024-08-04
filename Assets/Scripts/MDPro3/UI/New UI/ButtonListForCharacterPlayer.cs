namespace MDPro3.UI
{
    public class ButtonListForCharacterPlayer : ButtonList
    {
        public override void SelectThis()
        {
            base.SelectThis();
            Program.I().character.SwitchPlayer(gameObject.name);
        }
    }
}
