using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemForDeckSelect : SuperScrollViewItem, IPointerEnterHandler, IPointerExitHandler
    {
        public string deckName;
        public int deckCase;
        public int card1;
        public int card2;
        public int card3;
        public string protector;
        public string deckId;

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
            var scale = Config.GetUIScale();
            transform.localScale = Vector3.one * scale;

        }

        public override void Refresh()
        {
            StartCoroutine(RefreshAsync());
        }

        bool refreshed;
        IEnumerator RefreshAsync()
        {
            if (selected)
                ToggleOn();
            else
                ToggleOff();

            refreshed = false;
            textName.text = deckName;
            var casePath = deckCase.ToString();
            var load = Program.items.LoadItemIconAsync(casePath, Items.ItemType.Case);
            while (load.MoveNext())
                yield return null;
            if (load.Current != null)
                caseIcon.sprite = load.Current;
            while (Program.I().selectDeck.inTransition)
                yield return null;
            for (int i = 0; i < transform.GetSiblingIndex(); i++)
                yield return null;
            Material pMat = null;
            if (card1 != 0)
            {
                var task = TextureManager.LoadCardAsync(card1, true);
                while (!task.IsCompleted)
                    yield return null;
                cardFace1.texture = task.Result;
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
                var task = TextureManager.LoadCardAsync(card2, true);
                while (!task.IsCompleted)
                    yield return null;
                cardFace2.texture = task.Result;
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
                var task = TextureManager.LoadCardAsync(card3, true);
                while (!task.IsCompleted)
                    yield return null;
                cardFace3.texture = task.Result;
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
            refreshed = true;
        }

        public void Dispose()
        {
            StartCoroutine(DisposeAsync());
        }

        IEnumerator DisposeAsync()
        {
            while(!refreshed)
                yield return null;
            Destroy(gameObject);
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
                if (SelectDeck.condition == SelectDeck.Condition.ForEdit)
                {
                    Program.I().editDeck.SwitchCondition(EditDeck.Condition.EditDeck);
                    Program.I().ShiftToServant(Program.I().editDeck);
                }
                else if (SelectDeck.condition == SelectDeck.Condition.MyCard)
                {
                    //Program.I().editDeck.SwitchCondition(EditDeck.Condition.EditOnlineDeck, textName.text);
                    //Program.I().ShiftToServant(Program.I().editDeck);
                    Program.I().ShiftToServant(Program.I().online);
                }
                else if (SelectDeck.condition == SelectDeck.Condition.ForDuel)
                {
                    Program.I().ShiftToServant(Program.I().room);
                }
                else if (SelectDeck.condition == SelectDeck.Condition.ForSolo)
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
            handler.items[id].args[6] = "1";
        }
        public void ToggleOff()
        {
            selected = false;
            toggleOn.SetActive(false);
            handler.items[id].args[6] = "0";
        }
    }
}
