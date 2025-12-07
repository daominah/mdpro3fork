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
using Cysharp.Threading.Tasks;

namespace MDPro3.UI
{
    public class SelectionToggle_AppearanceItem : SelectionToggle
    {
        #region Elements

        private CanvasGroup m_CG;
        private CanvasGroup CG =>
            m_CG = m_CG != null ? m_CG
            : GetComponent<CanvasGroup>();

        private const string LABEL_RIMG_PROTECTOR = "Protector";
        private RawImage m_Protector;
        private RawImage Protector =>
            m_Protector = m_Protector != null ? m_Protector
            : Manager.GetElement<RawImage>(LABEL_RIMG_PROTECTOR);

        private const string LABEL_IMG_WALLPAPER_BG = "WallpaperBG";
        private Image m_WallpaperBG;
        private Image WallpaperBG =>
            m_WallpaperBG = m_WallpaperBG != null ? m_WallpaperBG
            : Manager.GetElement<Image>(LABEL_IMG_WALLPAPER_BG);

        private GridLayoutGroup m_Grid;
        private GridLayoutGroup Grid =>
            m_Grid = m_Grid != null ? m_Grid
            : Program.instance.appearance.GetUI<AppearanceUI>().ScrollRect
            .content.GetComponent<GridLayoutGroup>();

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
            _ = RefreshAsync();
        }

        private async UniTask RefreshAsync()
        {
            for (int i = 0; i < index; i++)
                await UniTask.Yield();

            if (path.StartsWith("Protector"))
            {
                Protector.material = await ABLoader.LoadProtectorMaterial(itemID.ToString(), destroyCancellationToken);
                Protector.material.renderQueue = 3000;
                Protector.color = Color.white;
                Icon.gameObject.SetActive(false);
            }
            else if (path.Length > 0)
            {
                Icon.sprite = await Program.items.LoadItemIconAsync(itemID.ToString(), Items.ItemType.Unknown);
                if (Manager == null)
                    return;
                Icon.color = Color.white;
                if (path.StartsWith("ProfileFrame"))
                {
                    Icon.rectTransform.localScale = Vector3.one * 0.8f;
                    Icon.material = await ABLoader.LoadFrameMaterial(itemID.ToString());
                    Icon.material.SetTexture("_ProfileFrameTex", Icon.sprite.texture);
                    Icon.sprite = TextureManager.container.black;
                    Icon.color = Color.white;
                }
                else if (path.StartsWith("DeckCase"))
                {
                    Icon.transform.localPosition = new Vector3(0f, 15f, 0f);
                }
                else if (path.StartsWith("WallPaperIcon"))
                {
                    WallpaperBG.gameObject.SetActive(true);
                }
                Protector.gameObject.SetActive(false);
            }
            else //CrossDuel Mate
            {
                var art = await CardImageLoader.LoadArtAsync(itemID, true, destroyCancellationToken);
                Icon.color = Color.white;
                Icon.sprite = TextureManager.Texture2Sprite(art);
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

            Program.instance.appearance.GetUI<AppearanceUI>().Detail.SetItem(itemID, itemName, description, path == string.Empty);

            Program.instance.appearance.GetUI<AppearanceUI>().SetHoverText(itemName);
            Program.instance.appearance.lastSelectedItem = this;
            Program.instance.currentServant.lastSelectable = Selectable;
            GetSelectable().Select();

            if (Appearance.condition == Appearance.Condition.DeckEditor)
            {
                if (path.StartsWith("DeckCase"))
                {
                    if(DeckEditor.Deck.Case != itemID)
                    {
                        DeckEditor.Deck.Case = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconCase.sprite = Icon.sprite;
                    }
                }
                else if (path.StartsWith("Protector"))
                {
                    if (DeckEditor.Deck.Protector != itemID)
                    {
                        DeckEditor.Deck.Protector = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconProtector.material = Protector.material;
                    }
                }
                else if (path.StartsWith("FieldIcon"))
                {
                    if (DeckEditor.Deck.Field != itemID)
                    {
                        DeckEditor.Deck.Field = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconField.sprite = Icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldObj"))
                {
                    if (DeckEditor.Deck.Grave != itemID)
                    {
                        DeckEditor.Deck.Grave = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconGrave.sprite = Icon.sprite;
                    }
                }
                else if (path.StartsWith("FieldAvatarBase"))
                {
                    if (DeckEditor.Deck.Stand != itemID)
                    {
                        DeckEditor.Deck.Stand = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconStand.sprite = Icon.sprite;
                    }
                }
                else
                {
                    if (DeckEditor.Deck.Mate != itemID)
                    {
                        DeckEditor.Deck.Mate = itemID;
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().DeckView.SetDirty(true);
                        Program.instance.deckEditor.GetUI<DeckEditorUI>().IconMate.sprite = Icon.sprite;
                    }
                }
            }
            else
            {
                if (AppearanceUI.currentContent == "Wallpaper")
                    Config.Set("Wallpaper", itemID.ToString());
                else
                    Config.Set(Appearance.condition.ToString() + AppearanceUI.currentContent + Appearance.player, itemID.ToString());
            }

            StartCoroutine(ConfigSetAsync());
        }

        private IEnumerator ConfigSetAsync()
        {
            while (!loaded)
                yield return null;

            if (!Icon.gameObject.activeSelf)//Protector
            {
                if (Appearance.player == "0")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector0 = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector0 = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector0 = Protector.material;
                }
                else if (Appearance.player == "1")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector1 = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector1 = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector1 = Protector.material;
                }
                else if (Appearance.player == "0Tag")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector0Tag = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector0Tag = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector0Tag = Protector.material;
                }
                else if (Appearance.player == "1Tag")
                {
                    if (Appearance.condition == Appearance.Condition.Duel)
                        Appearance.duelProtector1Tag = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Watch)
                        Appearance.watchProtector1Tag = Protector.material;
                    else if (Appearance.condition == Appearance.Condition.Replay)
                        Appearance.replayProtector1Tag = Protector.material;
                }
            }
            else
            {
                if (path.StartsWith("ProfileIcon"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace0 = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace0 = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace0 = Icon.sprite;
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace1 = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace1 = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace1 = Icon.sprite;
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace0Tag = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace0Tag = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace0Tag = Icon.sprite;
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFace1Tag = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFace1Tag = Icon.sprite;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFace1Tag = Icon.sprite;
                    }
                }
                else if (path.StartsWith("ProfileFrame"))
                {
                    if (Appearance.player == "0")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFrameMat0 = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFrameMat0 = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFrameMat0 = Icon.material;
                    }
                    else if (Appearance.player == "1")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFrameMat1 = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFrameMat1 = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFrameMat1 = Icon.material;
                    }
                    else if (Appearance.player == "0Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFrameMat0Tag = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFrameMat0Tag = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFrameMat0Tag = Icon.material;
                    }
                    else if (Appearance.player == "1Tag")
                    {
                        if (Appearance.condition == Appearance.Condition.Duel)
                            Appearance.duelFrameMat1Tag = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Watch)
                            Appearance.watchFrameMat1Tag = Icon.material;
                        else if (Appearance.condition == Appearance.Condition.Replay)
                            Appearance.replayFrameMat1Tag = Icon.material;
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
            CG.alpha = 0f;
            CG.blocksRaycasts = false;
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
            CG.alpha = 1f;
            CG.blocksRaycasts = true;

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
