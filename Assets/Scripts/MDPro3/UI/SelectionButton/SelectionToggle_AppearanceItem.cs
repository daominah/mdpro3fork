using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;
using MDPro3.Utility;

namespace MDPro3.UI
{
    public class SelectionToggle_AppearanceItem : SelectionToggle
    {
        #region 

        private const string LABEL_RIMG_PROTECTOR = "Protector";
        private RawImage m_Protector;
        private RawImage Protector =>
            m_Protector = m_Protector != null ? m_Protector
            : Manager.GetElement<RawImage>(LABEL_RIMG_PROTECTOR);

        private const string LABEL_IMG_ICON = "Icon";
        private Image m_Icon;
        private Image Icon =>
            m_Icon = m_Icon != null ? m_Icon
            : Manager.GetElement<Image>(LABEL_IMG_ICON);

        #endregion


        public int itemID;
        public string itemName;
        public string description;
        public string path;
        private bool loaded;

        private Coroutine refreshCoroutine;
        private Coroutine hideCoroutine;

        protected override void Awake()
        {
            base.Awake();
            HoverOff();
            exclusiveToggle = true;
            canToggleOffSelf = false;
            manuallySetNavigation = false;
        }

        public void Refresh()
        {
            refreshCoroutine = StartCoroutine(RefreshAsync());
        }

        private IEnumerator RefreshAsync()
        {
            for (int i = 0; i < index; i++)
                yield return null;

            if (path.StartsWith("Protector"))
            {
                var ie = ABLoader.LoadProtectorMaterial(itemID.ToString());
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                Protector.material = ie.Current;
                Protector.material.renderQueue = 3000;
                Protector.color = Color.white;
                Icon.gameObject.SetActive(false);
            }
            else if (path.Length > 0)
            {
                var load = Program.items.LoadItemIconAsync(itemID.ToString(), Items.ItemType.Unknown);
                while (load.MoveNext())
                    yield return null;
                Icon.sprite = load.Current;
                Icon.color = Color.white;
                if (path.StartsWith("ProfileFrame"))
                {
                    Icon.rectTransform.localScale = Vector3.one * 0.8f;
                    var ie = ABLoader.LoadFrameMaterial(itemID.ToString());
                    StartCoroutine(ie);
                    while (ie.MoveNext())
                        yield return null;
                    Material mat = ie.Current;
                    Icon.material = mat;
                    Icon.material.SetTexture("_ProfileFrameTex", Icon.sprite.texture);
                    Icon.sprite = TextureManager.container.black;
                    Icon.color = Color.white;
                }
                else if (path.StartsWith("DeckCase"))
                {
                    Icon.rectTransform.localScale = Vector3.one * 0.8f;
                    //Icon.transform.localPosition = new Vector3(0f, 15f, 0f);
                }
                Protector.gameObject.SetActive(false);
            }
            else //CrossDuel Mate
            {
                var task = CardImageLoader.LoadArtAsync(itemID, true);
                while (!task.IsCompleted)
                    yield return null;
                Icon.color = Color.white;
                Icon.sprite = TextureManager.Texture2Sprite(task.Result);
                Protector.gameObject.SetActive(false);
            }

            if (path.StartsWith("ProfileIcon"))
                Icon.material = Appearance.matForFace;

            loaded = true;
            refreshCoroutine = null;
        }

        protected override void CallHoverOnEvent()
        {
            base.CallHoverOnEvent();
            Program.instance.appearance.GetUI<AppearanceUI>().SetHoverText(itemName);
        }

        protected override void CallToggleOnEvent()
        {
            base.CallToggleOnEvent();
            CallHoverOnEvent();

            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailName(itemName);
            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailDescription(description);
            Program.instance.appearance.GetUI<AppearanceUI>().SetHoverText(itemName);
            Program.instance.appearance.lastSelectedItem = this;
            Program.instance.currentServant.lastSelectable = Selectable;
            GetSelectable().Select();

            var protector = Manager.GetElement<RawImage>("Protector");
            var icon = Manager.GetElement<Image>("Icon");


            if (Appearance.condition == Appearance.Condition.DeckEditor)
            {
                if (path.StartsWith("DeckCase"))
                {
                    if(DeckEditor.Deck.Case != itemID)
                    {
                        DeckEditor.Deck.Case = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconCase.sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("Protector"))
                {
                    if (DeckEditor.Deck.Protector != itemID)
                    {
                        DeckEditor.Deck.Protector = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconProtector.material = protector.material;
                    }
                }
                else if (path.StartsWith("FieldIcon"))
                {
                    if (DeckEditor.Deck.Field != itemID)
                    {
                        DeckEditor.Deck.Field = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconField.sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldObj"))
                {
                    if (DeckEditor.Deck.Grave != itemID)
                    {
                        DeckEditor.Deck.Grave = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconGrave.sprite = icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldAvatarBase"))
                {
                    if (DeckEditor.Deck.Stand != itemID)
                    {
                        DeckEditor.Deck.Stand = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconStand.sprite = icon.sprite;
                    }
                }
                else
                {
                    if (DeckEditor.Deck.Mate != itemID)
                    {
                        DeckEditor.Deck.Mate = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconMate.sprite = icon.sprite;
                    }
                }
            }
            else{
                if (path.StartsWith("WallPaperIcon"))
                    Config.Set("Wallpaper", itemID.ToString());
                else
                    Config.Set(Appearance.condition.ToString() + AppearanceUI.currentContent + Appearance.player, itemID.ToString());
            }

            StartCoroutine(ShowDetailAsync());
        }

        private IEnumerator ShowDetailAsync()
        {

            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(TextureManager.container.typeNone);
            while (!loaded)
                yield return null;

            var protector = Manager.GetElement<RawImage>("Protector");
            var icon = Manager.GetElement<Image>("Icon");

            if (!icon.gameObject.activeSelf)//Protector
            {
                Program.instance.appearance.GetUI<AppearanceUI>().SetDetailRawImageMaterial(protector.material);

                if (Appearance.player == "0")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector0 = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector0 = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector0 = protector.material;
                }
                else if (Appearance.player == "1")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector1 = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector1 = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector1 = protector.material;
                }
                else if (Appearance.player == "0Tag")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector0Tag = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector0Tag = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector0Tag = protector.material;
                }
                else if (Appearance.player == "1Tag")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector1Tag = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector1Tag = protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector1Tag = protector.material;
                }
            }
            else
            {
                Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(icon.sprite);
                if (path.StartsWith("ProfileIcon"))
                    Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImageMaterial(null);
                else
                    Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImageMaterial(icon.material);
                if (path.StartsWith("ProfileIcon"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace0 = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace0 = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace0 = icon.sprite;
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace1 = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace1 = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace1 = icon.sprite;
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace0Tag = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace0Tag = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace0Tag = icon.sprite;
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace1Tag = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace1Tag = icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace1Tag = icon.sprite;
                    }
                }
                else if (path.StartsWith("ProfileFrame"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.duelFace0);
                            Appearance.duelFrameMat0 = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Watch)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.watchFace0);
                            Appearance.watchFrameMat0 = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Replay)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.replayFace0);
                            Appearance.replayFrameMat0 = icon.material;
                        }
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.duelFace1);
                            Appearance.duelFrameMat1 = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Watch)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.watchFace1);
                            Appearance.watchFrameMat1 = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Replay)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.replayFace1);
                            Appearance.replayFrameMat1 = icon.material;
                        }
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.duelFace0Tag);
                            Appearance.duelFrameMat0Tag = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Watch)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.watchFace0Tag);
                            Appearance.watchFrameMat0Tag = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Replay)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.replayFace0Tag);
                            Appearance.replayFrameMat0Tag = icon.material;
                        }
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.duelFace1Tag);
                            Appearance.duelFrameMat1Tag = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Watch)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.watchFace1Tag);
                            Appearance.watchFrameMat1Tag = icon.material;
                        }
                        else if (Appearance.condition == Appearance.Condition.Replay)
                        {
                            Program.instance.appearance.GetUI<AppearanceUI>().SetDetailImage(Appearance.replayFace1Tag);
                            Appearance.replayFrameMat1Tag = icon.material;
                        }
                    }
                }
            }
        }

        protected override void OnClick()
        {
            AudioManager.PlaySE(SoundLabelClick);
            SetToggleOn();
            Program.instance.currentServant.lastSelectable = Selectable;
        }

        protected override int GetButtonsCount()
        {
            return Program.instance.appearance.GetUI<AppearanceUI>().GetCurrentGenreCount();
        }

        private GridLayoutGroup m_Grid;
        private GridLayoutGroup Grid =>
            m_Grid = m_Grid != null ? m_Grid
            : Program.instance.appearance.GetUI<AppearanceUI>().ScrollRect
            .content.GetComponent<GridLayoutGroup>();

        protected override int GetColumnsCount()
        {
            return Grid.Size().x;
        }

        public void Hide()
        {
            if (hideCoroutine != null || !gameObject.activeSelf)
                return;
            hideCoroutine = StartCoroutine(HideAsync());

            GetComponent<LayoutElement>().ignoreLayout = true;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        private IEnumerator HideAsync()
        {
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            while (!loaded)
                yield return null;
            hideCoroutine = null;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if(hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            gameObject.SetActive(true);
            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;

            GetComponent<LayoutElement>().ignoreLayout = false;
            transform.SetSiblingIndex(index);
        }

        public void Dispose()
        {
            if(refreshCoroutine != null)
                StopCoroutine(refreshCoroutine);

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            Destroy(gameObject);
        }
    }
}
