using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemForDeck : SuperScrollViewItem, IPointerEnterHandler, IPointerExitHandler
    {
        public string deckName;
        public int deckCase;
        public int card1;
        public int card2;
        public int card3;
        public string protector;

        public Text textName;
        public Image caseIcon;
        public RawImage cardFace1;
        public RawImage cardFace2;
        public RawImage cardFace3;

        public GameObject toggle;
        public GameObject toggleOn;
        public bool selected;
        bool onSelect;
        public void Awake()
        {
            Program.I().selectDeck.items.Add(this);
            toggle.SetActive(false);
            var defau = 1000f;
#if UNITY_ANDROID
            defau = 1500f;
#endif
            var scale = float.Parse(Config.Get("UIScale", defau.ToString())) / 1000;
            transform.localScale = Vector3.one * scale;

        }

        public override void Refresh()
        {
            StartCoroutine(RefreshAsync());
        }

        IEnumerator RefreshAsync()
        {
            textName.text = deckName;
            var casePath = deckCase.ToString();
            var load = TextureManager.LoadItemIcon(casePath);
            while (load.MoveNext())
                yield return null;
            if (load.Current != null)
                caseIcon.sprite = load.Current;
            while (Program.I().selectDeck.inTransition)
                yield return null;
            for (int i = 0; i < transform.GetSiblingIndex(); i++)
                yield return null;
            Material pMat = null;
            IEnumerator<Texture2D> ie = null;
            if (card1 != 0)
            {
                ie = Program.I().texture_.LoadCardAsync(card1, true);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                cardFace1.texture = ie.Current;
                var mat = TextureManager.GetCardMaterial(card1);
                cardFace1.material = mat;
            }
            else
            {
                if (pMat == null)
                {
                    var im = ABLoader.LoadProtectorMaterial(protector);
                    while (im.MoveNext())
                        yield return null;
                    pMat = im.Current;
                }
                cardFace1.texture = null;
                cardFace1.material = pMat;
            }
            if (card2 != 0)
            {
                ie = Program.I().texture_.LoadCardAsync(card2, true);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                cardFace2.texture = ie.Current;
                var mat = TextureManager.GetCardMaterial(card2);
                cardFace2.material = mat;
            }
            else
            {
                if (pMat == null)
                {
                    var im = ABLoader.LoadProtectorMaterial(protector);
                    while (im.MoveNext())
                        yield return null;
                    pMat = im.Current;
                }
                cardFace2.texture = null;
                cardFace2.material = pMat;
            }
            if (card3 != 0)
            {
                ie = Program.I().texture_.LoadCardAsync(card3, true);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                cardFace3.texture = ie.Current;
                var mat = TextureManager.GetCardMaterial(card3);
                cardFace3.material = mat;
            }
            else
            {
                if (pMat == null)
                {
                    var im = ABLoader.LoadProtectorMaterial(protector);
                    while (im.MoveNext())
                        yield return null;
                    pMat = im.Current;
                }
                cardFace3.texture = null;
                cardFace3.material = pMat;
            }
        }

        public override void OnClick()
        {
            AudioManager.PlaySE("SE_DUEL_SELECT");

            if (onSelect)
            {
                if (selected)
                    ToggleOff();
                else
                    ToggleOn();
            }
            else
            {
                Config.Set("DeckInUse", deckName);
                if (SelectDeck.state == SelectDeck.State.ForEdit)
                {
                    Program.I().ShiftToServant(Program.I().editDeck);
                    Program.I().editDeck.returnServant = Program.I().selectDeck;
                }
                else if (SelectDeck.state == SelectDeck.State.ForDuel)
                {
                    Program.I().ShiftToServant(Program.I().room);
                }
                else if (SelectDeck.state == SelectDeck.State.ForSolo)
                {
                    Program.I().ShiftToServant(Program.I().solo);
                    Program.I().solo.btnDeck.transform.GetChild(0).GetComponent<Text>().text = deckName;
                }
            }
        }

        public void Hover(bool hover)
        {
            cardFace1.GetComponent<Animator>().SetBool("Hover", hover);
            cardFace2.GetComponent<Animator>().SetBool("Hover", hover);
            cardFace3.GetComponent<Animator>().SetBool("Hover", hover);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Program.I().selectDeck.hoverOn)
                Hover(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Program.I().selectDeck.hoverOn)
                Hover(false);
        }

        public void ShowToggle()
        {
            toggle.SetActive(true);
            toggleOn.SetActive(false);
            onSelect = true;
        }

        public void HideToggle()
        {
            toggle.SetActive(false);
            selected = false;
            onSelect = false;
        }

        public void ToggleOn()
        {
            selected = true;
            toggleOn.SetActive(true);
        }
        public void ToggleOff()
        {
            selected = false;
            toggleOn.SetActive(false);
        }
    }
}
