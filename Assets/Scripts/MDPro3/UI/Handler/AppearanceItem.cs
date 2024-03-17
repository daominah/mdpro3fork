using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class AppearanceItem : MonoBehaviour, IPointerEnterHandler
    {
        public Image icon;
        public RawImage protector;
        public Button button;
        public GameObject checkMark;

        public Material materialForFace;

        public int id;
        public int itemID;
        public string itemName;
        public string description;
        public string path;

        public bool selected;
        bool loaded;

        void Start()
        {
            button.onClick.AddListener(SelectThis);
            StartCoroutine(Refresh());
        }

        IEnumerator Refresh()
        {
            for (int i = 0; i < id; i++)
                yield return null;
            if (path.StartsWith("Protector"))
            {
                IEnumerator<Material> ie = ABLoader.LoadProtectorMaterial(itemID.ToString());
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                protector.material = ie.Current;
                protector.material.renderQueue = 3000;
                protector.color = Color.white;
                Destroy(icon.gameObject);
            }
            else if (path.Length > 0)
            {
                var load = TextureManager.LoadItemIcon(itemID.ToString());
                while (load.MoveNext())
                    yield return null;
                icon.sprite = load.Current;
                icon.color = Color.white;
                if (path.StartsWith("ProfileFrame"))
                {
                    icon.rectTransform.localScale = Vector3.one * 0.8f;
                    var ie = ABLoader.LoadFrameMaterial(itemID.ToString());
                    StartCoroutine(ie);
                    while (ie.MoveNext())
                        yield return null;
                    Material mat = ie.Current;
                    icon.material = mat;
                    icon.material.SetTexture("_ProfileFrameTex", icon.sprite.texture);
                    icon.sprite = TextureManager.container.black;
                    icon.color = Color.white;
                }
                else if (path.StartsWith("DeckCase"))
                {
                    icon.rectTransform.localScale = Vector3.one * 0.8f;
                }
                Destroy(protector.gameObject);
            }
            else //CrossDuel Mate
            {
                var loading = Program.I().texture_.LoadArtAsync(itemID, true);
                StartCoroutine(loading);
                while (loading.MoveNext())
                    yield return null;
                var texture = loading.Current;
                icon.sprite = Sprite.Create(texture, new Rect(0, texture.height - texture.width, texture.width, texture.width), new Vector2(0.5f, 0.5f));
                icon.color = Color.white;
            }

            if (path.StartsWith("ProfileIcon"))
            {
                icon.material = Appearance.matForFace;

            }
            loaded = true;
        }

        public void SelectThis()
        {
            selected = true;
            checkMark.SetActive(true);
            foreach (var item in transform.parent.GetComponentsInChildren<AppearanceItem>())
                if (item != this)
                    item.UnselectThis();
            Program.I().appearance.detailTitle.text = itemName;
            Program.I().appearance.description.text = description;
            Program.I().appearance.hover.text = itemName;
            StartCoroutine(ShowDetail());

            if (Appearance.type == Appearance.AppearanceType.Deck)
            {
                if (path.StartsWith("DeckCase"))
                {
                    if (Program.I().editDeck.deck.Case[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Case[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconCase").sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("Protector"))
                {
                    if (Program.I().editDeck.deck.Protector[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Protector[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconProtector").material = protector.material;
                    }
                }
                else if (path.StartsWith("FieldIcon"))
                {
                    if (Program.I().editDeck.deck.Field[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Field[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconField").sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldObj"))
                {
                    if (Program.I().editDeck.deck.Grave[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Grave[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconGrave").sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldAvatarBase"))
                {
                    if (Program.I().editDeck.deck.Stand[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Stand[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconStand").sprite = icon.sprite;
                    }
                }
                else
                {
                    if (Program.I().editDeck.deck.Mate[0] != itemID)
                    {
                        Program.I().editDeck.dirty = true;
                        Program.I().editDeck.deck.Mate[0] = itemID;
                        Program.I().editDeck.manager.GetElement<Image>("IconMate").sprite = icon.sprite;
                    }
                }
            }
            else
            {
                if (path.StartsWith("Wallpaper"))
                    Config.Set("Wallpaper", itemID.ToString());
                else
                    Config.Set(Appearance.type.ToString() + Appearance.currentContent + Appearance.player, itemID.ToString());
            }
        }
        public IEnumerator ShowDetail()
        {
            Program.I().appearance.detailImage.sprite = TextureManager.container.typeNone;
            Program.I().appearance.detailImage.color = Color.clear;
            Program.I().appearance.detailProtector.color = Color.clear;
            while (!loaded)
                yield return null;
            Program.I().appearance.detailImage.color = Color.white;
            Program.I().appearance.detailProtector.color = Color.white;
            if (icon == null)//Protector
            {
                Program.I().appearance.detailImage.gameObject.SetActive(false);
                Program.I().appearance.detailProtector.gameObject.SetActive(true);
                Program.I().appearance.detailProtector.material = protector.material;
                if (Appearance.player == "0")
                {
                    if (Appearance.type == Appearance.AppearanceType.Duel)
                        Appearance.duelProtector0 = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Watch)
                        Appearance.watchProtector0 = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Replay)
                        Appearance.replayProtector0 = protector.material;
                }
                else if (Appearance.player == "1")
                {
                    if (Appearance.type == Appearance.AppearanceType.Duel)
                        Appearance.duelProtector1 = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Watch)
                        Appearance.watchProtector1 = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Replay)
                        Appearance.replayProtector1 = protector.material;
                }
                else if (Appearance.player == "0Tag")
                {
                    if (Appearance.type == Appearance.AppearanceType.Duel)
                        Appearance.duelProtector0Tag = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Watch)
                        Appearance.watchProtector0Tag = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Replay)
                        Appearance.replayProtector0Tag = protector.material;
                }
                else if (Appearance.player == "1Tag")
                {
                    if (Appearance.type == Appearance.AppearanceType.Duel)
                        Appearance.duelProtector1Tag = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Watch)
                        Appearance.watchProtector1Tag = protector.material;
                    else if (Appearance.type == Appearance.AppearanceType.Replay)
                        Appearance.replayProtector1Tag = protector.material;
                }
            }
            else
            {
                Program.I().appearance.detailImage.gameObject.SetActive(true);
                Program.I().appearance.detailProtector.gameObject.SetActive(false);
                Program.I().appearance.detailImage.sprite = icon.sprite;
                if (path.StartsWith("ProfileIcon"))
                    Program.I().appearance.detailImage.material = null;
                else
                    Program.I().appearance.detailImage.material = icon.material;
                if (path.StartsWith("ProfileIcon"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                            Appearance.duelFace0 = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                            Appearance.watchFace0 = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                            Appearance.replayFace0 = icon.sprite;
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                            Appearance.duelFace1 = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                            Appearance.watchFace1 = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                            Appearance.replayFace1 = icon.sprite;
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                            Appearance.duelFace0Tag = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                            Appearance.watchFace0Tag = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                            Appearance.replayFace0Tag = icon.sprite;
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                            Appearance.duelFace1Tag = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                            Appearance.watchFace1Tag = icon.sprite;
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                            Appearance.replayFace1Tag = icon.sprite;
                    }
                }
                else if (path.StartsWith("ProfileFrame"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.duelFace0;
                            Appearance.duelFrameMat0 = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.watchFace0;
                            Appearance.watchFrameMat0 = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.replayFace0;
                            Appearance.replayFrameMat0 = icon.material;
                        }
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.duelFace1;
                            Appearance.duelFrameMat1 = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.watchFace1;
                            Appearance.watchFrameMat1 = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.replayFace1;
                            Appearance.replayFrameMat1 = icon.material;
                        }
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.duelFace0Tag;
                            Appearance.duelFrameMat0Tag = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.watchFace0Tag;
                            Appearance.watchFrameMat0Tag = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.replayFace0Tag;
                            Appearance.replayFrameMat0Tag = icon.material;
                        }
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.type == Appearance.AppearanceType.Duel)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.duelFace1Tag;
                            Appearance.duelFrameMat1Tag = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Watch)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.watchFace1Tag;
                            Appearance.watchFrameMat1Tag = icon.material;
                        }
                        else if (Appearance.type == Appearance.AppearanceType.Replay)
                        {
                            Program.I().appearance.detailImage.sprite = Appearance.replayFace1Tag;
                            Appearance.replayFrameMat1Tag = icon.material;
                        }
                    }
                }
            }
        }
        public void Show()
        {
            var group = GetComponent<CanvasGroup>();
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        public IEnumerator Hide()
        {
            var group = GetComponent<CanvasGroup>();
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
            while (!loaded)
                yield return null;
            gameObject.SetActive(false);
        }

        public IEnumerator Dispose()
        {
            while (!loaded)
                yield return null;
            Destroy(gameObject);
        }

        public void UnselectThis()
        {
            selected = false;
            checkMark.SetActive(false);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            Program.I().appearance.hover.text = itemName;
        }
    }
}
