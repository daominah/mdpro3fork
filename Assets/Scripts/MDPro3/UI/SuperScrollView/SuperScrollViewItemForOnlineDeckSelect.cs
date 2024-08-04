using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemForOnlineDeckSelect : SuperScrollViewItem, IPointerEnterHandler, IPointerExitHandler
    {
        public string deckName;
        public string authorName;
        public string deckId;
        public int deckCase;
        public int card1;
        public int card2;
        public int card3;
        public string protector;
        public int like;
        public string lastDate;

        public Text textDeckName;
        public Text textAuthorName;
        public Image caseIcon;
        public RawImage cardFace1;
        public RawImage cardFace2;
        public RawImage cardFace3;
        public Text textLike;
        public Text textDate;

        public void Awake()
        {
            Program.I().onlineDeckViewer.items.Add(this);
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

        bool refreshed;
        IEnumerator RefreshAsync()
        {
            refreshed = false;
            textDeckName.text = deckName;
            textAuthorName.text = "By " + authorName;
            textLike.text = like.ToString();
            textDate.text = lastDate;
            var load = Program.items.LoadItemIconAsync(deckCase.ToString(), Items.ItemType.Case);
            while (load.MoveNext())
                yield return null;
            if (load.Current != null)
                caseIcon.sprite = load.Current;
            refreshed = true;
        }

        bool cardRefreshing;
        IEnumerator RefreshCardAsync()
        {
            cardRefreshing = true;
            Material pMat = null;
            if (card1 != 0)
            {
                var task = TextureManager.LoadCardAsync(card1);
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
                var task = TextureManager.LoadCardAsync(card2);
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
                var task = TextureManager.LoadCardAsync(card3);
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
            cardRefreshing = false;
        }


        public void Dispose()
        {
            StartCoroutine(DisposeAsync());
        }

        IEnumerator DisposeAsync()
        {
            gameObject.transform.SetParent(Program.I().container_2D, false);
            while (!refreshed)
                yield return null;
            while (cardRefreshing)
                yield return null;
            Destroy(gameObject);
        }

        public override void OnClick()
        {
            AudioManager.PlaySE("SE_DUEL_SELECT");
            Program.I().editDeck.onlineDeckID = deckId;
            Program.I().editDeck.SwitchCondition(EditDeck.Condition.OnlineDeck);
            Program.I().ShiftToServant(Program.I().editDeck);
        }

        public void Hover(bool hover)
        {
            cardFace1.GetComponent<Animator>().SetBool("Hover", hover);
            cardFace2.GetComponent<Animator>().SetBool("Hover", hover);
            cardFace3.GetComponent<Animator>().SetBool("Hover", hover);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartCoroutine(RefreshCardAsync());
            if (!Program.I().onlineDeckViewer.hoverOn)
                Hover(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Program.I().onlineDeckViewer.hoverOn)
                Hover(false);
        }
    }
}

