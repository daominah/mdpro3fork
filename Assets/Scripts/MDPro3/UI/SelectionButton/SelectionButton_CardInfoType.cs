using UnityEngine;
using UnityEngine.Events;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;

namespace MDPro3.UI
{
    public class SelectionButton_CardInfoType : SelectionButton
    {
        public static SelectionButton_CardInfoType instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            SetCardInfoTypeIcon(DeckEditorUI.cardInfoType);
            SetClickEvent(ClickEvent);
        }

        private void ClickEvent()
        {
            var type = (DeckEditorUI.CardInfoType)(((int)DeckEditorUI.cardInfoType + 1) % 3);
            Program.instance.deckEditor.GetUI<DeckEditorUI>().SetCardInfoType(type);
            SetCardInfoTypeIcon(type);
        }

        public void SetCardInfoTypeIcon(DeckEditorUI.CardInfoType type)
        {
            switch (type)
            {
                case DeckEditorUI.CardInfoType.None:
                    Manager.GetElement("IconInfoSwitching0").SetActive(true);
                    Manager.GetElement("IconInfoSwitching1").SetActive(false);
                    Manager.GetElement("IconInfoSwitching2").SetActive(false);
                    break;
                case DeckEditorUI.CardInfoType.Detail:
                    Manager.GetElement("IconInfoSwitching0").SetActive(false);
                    Manager.GetElement("IconInfoSwitching1").SetActive(true);
                    Manager.GetElement("IconInfoSwitching2").SetActive(false);
                    break;
                case DeckEditorUI.CardInfoType.Pool:
                    Manager.GetElement("IconInfoSwitching0").SetActive(false);
                    Manager.GetElement("IconInfoSwitching1").SetActive(false);
                    Manager.GetElement("IconInfoSwitching2").SetActive(true);
                    break;                
            }
        }
    }
}
