using MDPro3.Duel.YGOSharp;
using MDPro3.Servant;
using MDPro3.Utility;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YgomSystem.ElementSystem;

namespace MDPro3.UI
{
    public class DeckPickup : MonoBehaviour
    {

        #region Elements

        private ElementObjectManager m_Manager;
        private ElementObjectManager Manager =>
            m_Manager = m_Manager != null ? m_Manager 
            : GetComponent<ElementObjectManager>();

        private const string LABEL_IMG_DECKCASE = "DeckCase";
        private Image m_DeckCase;
        private Image DeckCase =>
            m_DeckCase = m_DeckCase != null ? m_DeckCase
            : Manager.GetElement<Image>(LABEL_IMG_DECKCASE);

        private const string LABEL_CRH_CARD1 = "Card1";
        private CardRawImageHandler m_Card1;
        private CardRawImageHandler Card1 =>
            m_Card1 = m_Card1 != null ? m_Card1
            : Manager.GetElement<CardRawImageHandler>(LABEL_CRH_CARD1);

        private const string LABEL_CRH_CARD2 = "Card2";
        private CardRawImageHandler m_Card2;
        private CardRawImageHandler Card2 =>
            m_Card2 = m_Card2 != null ? m_Card2
            : Manager.GetElement<CardRawImageHandler>(LABEL_CRH_CARD2);

        private const string LABEL_CRH_CARD3 = "Card3";
        private CardRawImageHandler m_Card3;
        private CardRawImageHandler Card3 =>
            m_Card3 = m_Card3 != null ? m_Card3
            : Manager.GetElement<CardRawImageHandler>(LABEL_CRH_CARD3);

        #endregion

        public async void SetDeck(Deck deck)
        {

            if (deck.Pickup.Count > 0)
                Card1.SetCard(deck.Pickup[0]);
            else
                Card1.SetProtector(deck.Protector);
            if (deck.Pickup.Count > 1)
                Card2.SetCard(deck.Pickup[1]);
            else
                Card2.SetProtector(deck.Protector);
            if (deck.Pickup.Count > 2)
                Card3.SetCard(deck.Pickup[2]);
            else
                Card3.SetProtector(deck.Protector);

            await LoadDeckCaseAsync(deck.Case);
        }

        private async Task LoadDeckCaseAsync(int deckCase)
        {
            var load = Program.items.LoadDeckCaseIconAsync(deckCase, "_Open_L");
            while (!load.IsCompleted)
                await TaskUtility.WaitOneFrame();
            DeckCase.sprite = load.Result;
        }

    }
}
