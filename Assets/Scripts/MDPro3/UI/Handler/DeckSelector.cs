using MDPro3.YGOSharp;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class DeckSelector : MonoBehaviour
    {
        public Image imageCase;
        public Text textDeckName;
        public RawImage rawImageCard1;
        public RawImage rawImageCard2;
        public RawImage rawImageCard3;

        public Servant servant;

        IEnumerator refreshInstance;

        private void Awake()
        {
            GetComponent<ButtonHover>().hoverIn = () => ShowPickUp();
            GetComponent<ButtonHover>().hoverOut = () => HidePickUp();

        }

        private void OnEnable()
        {
            if(refreshInstance != null)
                StartCoroutine(refreshInstance);
        }

        public void SetDeck(Deck deck, string deckName)
        {
            textDeckName.text = deckName;

            if (deck == null)
            {
                var ie = RefreshAsync();
                if (gameObject.activeInHierarchy)
                    StartCoroutine(ie);
                else
                    refreshInstance= ie;
            }
            else
            {
                var ie = RefreshAsync(
                    deck.Case[0],
                    deck.Protector[0],
                    deck.Pickup.Count > 0 ? deck.Pickup[0] : 0,
                    deck.Pickup.Count > 1 ? deck.Pickup[1] : 0,
                    deck.Pickup.Count > 2 ? deck.Pickup[2] : 0
                    );
                if (gameObject.activeInHierarchy)
                    StartCoroutine(ie);
                else
                    refreshInstance = ie;
            }
        }

        IEnumerator RefreshAsync(int deckCase = 1080001, int protector = 1070001, int card1 = 0, int card2 = 0, int card3 = 0)
        {
            servant.refreshingCount++;

            if (!Items.initialized)
                yield return null;

            if(TextureManager.container == null)
                yield return null;

            imageCase.color = Color.clear;
            rawImageCard1.color = Color.clear;
            rawImageCard2.color = Color.clear;
            rawImageCard3.color = Color.clear;

            var ie = TextureManager.LoadItemIcon(deckCase.ToString(), Items.ItemType.Case);
            while (ie.MoveNext())
                yield return null;
            imageCase.sprite = ie.Current;
            imageCase.color = Color.white;

            if(card1 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard1.texture = null;
                rawImageCard1.material = ie2.Current;
                rawImageCard1.color = Color.white;
            }
            else
            {
                var mat = TextureManager.GetCardMaterial(card1);
                var ie2 = Program.I().texture_.LoadCardAsync(card1);
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard1.material = mat;
                rawImageCard1.texture = ie2.Current;
                rawImageCard1.color = Color.white;
            }

            if (card2 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard2.texture = null;
                rawImageCard2.material = ie2.Current;
                rawImageCard2.color = Color.white;
            }
            else
            {
                var mat = TextureManager.GetCardMaterial(card2);
                var ie2 = Program.I().texture_.LoadCardAsync(card2);
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard2.material = mat;
                rawImageCard2.texture = ie2.Current;
                rawImageCard2.color = Color.white;
            }

            if (card3 == 0)
            {
                var ie2 = ABLoader.LoadProtectorMaterial(protector.ToString());
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard3.texture = null;
                rawImageCard3.material = ie2.Current;
                rawImageCard3.color = Color.white;
            }
            else
            {
                var mat = TextureManager.GetCardMaterial(card3);
                var ie2 = Program.I().texture_.LoadCardAsync(card3);
                while (ie2.MoveNext())
                    yield return null;
                rawImageCard3.material = mat;
                rawImageCard3.texture = ie2.Current;
                rawImageCard3.color = Color.white;
            }

            servant.refreshingCount--;
        }

        Animator animator1;
        Animator animator2;
        Animator animator3;
        public void ShowPickUp()
        {
            if (animator1 == null)
                animator1 = rawImageCard1.GetComponent<Animator>();
            if (animator2 == null)
                animator2 = rawImageCard2.GetComponent<Animator>();
            if (animator3 == null)
                animator3 = rawImageCard3.GetComponent<Animator>();

            rawImageCard1.GetComponent<Animator>().SetBool("Hover", true);
            rawImageCard2.GetComponent<Animator>().SetBool("Hover", true);
            rawImageCard3.GetComponent<Animator>().SetBool("Hover", true);
        }
        public void HidePickUp()
        {
            if (animator1 == null)
                animator1 = rawImageCard1.GetComponent<Animator>();
            if (animator2 == null)
                animator2 = rawImageCard2.GetComponent<Animator>();
            if (animator3 == null)
                animator3 = rawImageCard3.GetComponent<Animator>();

            rawImageCard1.GetComponent<Animator>().SetBool("Hover", false);
            rawImageCard2.GetComponent<Animator>().SetBool("Hover", false);
            rawImageCard3.GetComponent<Animator>().SetBool("Hover", false);
        }

    }
}
