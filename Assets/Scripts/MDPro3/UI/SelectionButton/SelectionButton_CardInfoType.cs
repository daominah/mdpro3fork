using UnityEngine;
using UnityEngine.Events;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;
using TMPro;
using MDPro3.Net;

namespace MDPro3.UI
{
    public class SelectionButton_CardInfoType : SelectionButton
    {
        #region Elements

        private const string LABEL_GO_INFO0 = "IconInfoSwitching0";
        private GameObject m_Info0;
        private GameObject Info0 =>
            m_Info0 = m_Info0 != null ? m_Info0
            : Manager.GetElement(LABEL_GO_INFO0);

        private const string LABEL_GO_INFO1 = "IconInfoSwitching1";
        private GameObject m_Info1;
        private GameObject Info1 =>
            m_Info1 = m_Info1 != null ? m_Info1
            : Manager.GetElement(LABEL_GO_INFO1);

        private const string LABEL_GO_INFO2 = "IconInfoSwitching2";
        private GameObject m_Info2;
        private GameObject Info2 =>
            m_Info2 = m_Info2 != null ? m_Info2
            : Manager.GetElement(LABEL_GO_INFO2);

        private const string LABEL_GO_INFO3 = "IconInfoSwitching3";
        private GameObject m_Info3;
        private GameObject Info3 =>
            m_Info3 = m_Info3 != null ? m_Info3
            : Manager.GetElement(LABEL_GO_INFO3);

        private const string LABEL_TXT_GP = "TextGenesysPoint";
        private TextMeshProUGUI m_TextGP;
        private TextMeshProUGUI TextGP =>
            m_TextGP = m_TextGP != null ? m_TextGP
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_GP);

        #endregion

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
            var type = (DeckEditorUI.CardInfoType)(((int)DeckEditorUI.cardInfoType + 1) % 4);
            Program.instance.deckEditor.GetUI<DeckEditorUI>().SetCardInfoType(type);
            SetCardInfoTypeIcon(type);
        }

        public void SetCardInfoTypeIcon(DeckEditorUI.CardInfoType type)
        {
            Info0.SetActive(type == DeckEditorUI.CardInfoType.None);
            Info1.SetActive(type == DeckEditorUI.CardInfoType.Detail);
            Info2.SetActive(type == DeckEditorUI.CardInfoType.Pool);
            Info3.SetActive(type == DeckEditorUI.CardInfoType.Genesys);
            TextGP.gameObject.SetActive(type == DeckEditorUI.CardInfoType.Genesys);
        }

        public static void SetGenesysPoints(int gp)
        {
            if (instance == null)
                return;
            instance.TextGP.text = gp.ToString();
            instance.TextGP.color = OnlineService.GetGenesysPointsColor(gp);
        }
    }
}
