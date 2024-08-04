namespace MDPro3.UI
{
    public class ButtonListForCharacter : ButtonList
    {
        public override void SelectThis()
        {
            base.SelectThis();
            Program.I().character.ShowCharacters(gameObject.name);
        }
    }
}
