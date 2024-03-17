namespace MDPro3.UI
{
    public class ButtonListForAppearance : ButtonList
    {
        public override void SelectThis()
        {
            base.SelectThis();
            Program.I().appearance.ShowItems(gameObject.name);
        }
    }
}
