using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.IO;
using MDPro3.Duel.YGOSharp;
using MDPro3.UI.ServantUI;

namespace MDPro3.Servant
{
    public class Appearance : Servant
    {

        [Header("Appearance")]
        [HideInInspector] public SelectionToggle_AppearanceGenre lastSelectedToggle;
        [HideInInspector] public SelectionToggle_AppearanceItem lastSelectedItem;

        #region Assets

        public static Sprite duelFace0;
        public static Sprite duelFace1;
        public static Sprite watchFace0;
        public static Sprite watchFace1;
        public static Sprite replayFace0;
        public static Sprite replayFace1;
        public static Sprite duelFace0Tag;
        public static Sprite duelFace1Tag;
        public static Sprite watchFace0Tag;
        public static Sprite watchFace1Tag;
        public static Sprite replayFace0Tag;
        public static Sprite replayFace1Tag;
        public static Sprite defaultFace0;
        public static Sprite defaultFace1;

        public static Material duelFrameMat0;
        public static Material duelFrameMat1;
        public static Material watchFrameMat0;
        public static Material watchFrameMat1;
        public static Material replayFrameMat0;
        public static Material replayFrameMat1;
        public static Material duelFrameMat0Tag;
        public static Material duelFrameMat1Tag;
        public static Material watchFrameMat0Tag;
        public static Material watchFrameMat1Tag;
        public static Material replayFrameMat0Tag;
        public static Material replayFrameMat1Tag;

        public static Material duelProtector0;
        public static Material duelProtector1;
        public static Material watchProtector0;
        public static Material watchProtector1;
        public static Material replayProtector0;
        public static Material replayProtector1;
        public static Material duelProtector0Tag;
        public static Material duelProtector1Tag;
        public static Material watchProtector0Tag;
        public static Material watchProtector1Tag;
        public static Material replayProtector0Tag;
        public static Material replayProtector1Tag;

        public static Material matForFace;
        public static string player = "0";
        public const string meString = "Me";
        public const string opString = "Op";
        public const string meTagString = "MeTag";
        public const string opTagString = "OpTag";

        [HideInInspector] public static GameObject appearanceItem;

        #endregion

        public enum Condition
        {
            Duel,
            Watch,
            Replay,
            DeckEditor
        }
        public static Condition condition = Condition.Duel;
        public void SwitchCondition(Condition condition)
        {
            Appearance.condition = condition;
        }

        #region Servant

        public override int Depth => 6;
        protected override bool ShowLine => false;
        protected override float SubBlackAlpha => 0.9f;
        public override void Initialize()
        {
            base.Initialize();

            var handle = Addressables.LoadAssetAsync<GameObject>("ItemAppearance");
            handle.Completed += (result) =>
            {
                appearanceItem = result.Result;
            };

            StartCoroutine(LoadSettingAssets());
        }

        public override void OnReturn()
        {
            if (inTransition) return;
            if (returnAction != null)
            {
                returnAction.Invoke();
                return;
            }
            AudioManager.PlaySE("SE_MENU_CANCEL");
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if (selected == null)
                OnExit();
            else if (Cursor.lockState == CursorLockMode.None)
                OnExit();
            else if (selected.TryGetComponent<SelectionToggle_AppearanceItem>(out _) 
                || selected == GetUI<AppearanceUI>().InputPlayerName.gameObject)
            {
                if (lastSelectedToggle != null)
                    lastSelectedToggle.GetSelectable().Select();
                else
                    servantUI.SelectDefaultSelectable();
            }
            else
                OnExit();
        }

        public override void OnExit()
        {
            if (condition != Condition.DeckEditor)
            {
                if (Program.instance.currentSubServant == this)
                    Program.instance.ShowSubServant(Program.instance.setting);
                else
                    Program.instance.ShiftToServant(Program.instance.setting);
            }
            else
            {
                Program.instance.ShiftToServant(Program.instance.deckEditor);
            }
        }

        public override void PerFrameFunction()
        {
            if (NeedResponseInput())
            {
                if (UserInput.WasLeftShoulderPressed)
                    if (GetUI<AppearanceUI>().CanSwitchPlayer())
                        GetUI<AppearanceUI>().OnPlayerLeft();
                if (UserInput.WasRightShoulderPressed)
                    if (GetUI<AppearanceUI>().CanSwitchPlayer())
                        GetUI<AppearanceUI>().OnPlayerRight();

                if (UserInput.MouseRightDown || UserInput.WasCancelPressed)
                    OnReturn();
            }
        }

        public override void Select(bool forced = false)
        {
            if (!forced && !UserInput.NeedDefaultSelect())
                return;

            if (lastSelectable != null)
            {
                if (lastSelectable.TryGetComponent<SelectionToggle_CharacterItem>(out _)
                    || lastSelectable.TryGetComponent<SelectionToggle_CharacterSeries>(out _))
                    lastSelectable.Select();
                else
                    servantUI.SelectDefaultSelectable();
            }
            else
                servantUI.SelectDefaultSelectable();
        }

        #endregion

        public static bool loaded;
        public IEnumerator LoadSettingAssets()
        {
            loaded = false;

            var ab = AssetBundle.LoadFromFileAsync(Program.root + "MasterDuel/Frame/ProfileFrameMat1030001");
            matForFace = ab.assetBundle.LoadAsset<Material>("ProfileFrameMat1030001");
            ab.assetBundle.Unload(false);

            #region Face
            var ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 0);
            while (ie.MoveNext())
                yield return null;
            duelFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 1);
            while (ie.MoveNext())
                yield return null;
            duelFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 2);
            while (ie.MoveNext())
                yield return null;
            duelFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face, 3);
            while (ie.MoveNext())
                yield return null;
            duelFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            watchFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace0", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace1", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace1 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace0Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace0Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFace1Tag", Program.items.faces[0].id.ToString()), Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            replayFace1Tag = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync("1010039", Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            defaultFace0 = ie.Current;

            ie = Program.items.LoadConcreteItemIconAsync("1010001", Items.ItemType.Face);
            while (ie.MoveNext())
                yield return null;
            defaultFace1 = ie.Current;

            #endregion

            #region Frame

            Sprite duelFrame0;
            Sprite duelFrame1;
            Sprite watchFrame0;
            Sprite watchFrame1;
            Sprite replayFrame0;
            Sprite replayFrame1;
            Sprite duelFrame0Tag;
            Sprite duelFrame1Tag;
            Sprite watchFrame0Tag;
            Sprite watchFrame1Tag;
            Sprite replayFrame0Tag;
            Sprite replayFrame1Tag;

            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame0 = ie.Current;

            var im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat0 = im.Current;
            duelFrameMat0.SetTexture("_ProfileFrameTex", duelFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat1 = im.Current;
            duelFrameMat1.SetTexture("_ProfileFrameTex", duelFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat0Tag = im.Current;
            duelFrameMat0Tag.SetTexture("_ProfileFrameTex", duelFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("DuelFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            duelFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("DuelFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelFrameMat1Tag = im.Current;
            duelFrameMat1Tag.SetTexture("_ProfileFrameTex", duelFrame1Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame0 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat0 = im.Current;
            watchFrameMat0.SetTexture("_ProfileFrameTex", watchFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat1 = im.Current;
            watchFrameMat1.SetTexture("_ProfileFrameTex", watchFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat0Tag = im.Current;
            watchFrameMat0Tag.SetTexture("_ProfileFrameTex", watchFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("WatchFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            watchFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("WatchFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchFrameMat1Tag = im.Current;
            watchFrameMat1Tag.SetTexture("_ProfileFrameTex", watchFrame1Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame0", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame0 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame0", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat0 = im.Current;
            replayFrameMat0.SetTexture("_ProfileFrameTex", replayFrame0.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame1", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame1 = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame1", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat1 = im.Current;
            replayFrameMat1.SetTexture("_ProfileFrameTex", replayFrame1.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame0Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame0Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame0Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat0Tag = im.Current;
            replayFrameMat0Tag.SetTexture("_ProfileFrameTex", replayFrame0Tag.texture);


            ie = Program.items.LoadConcreteItemIconAsync(Config.Get("ReplayFrame1Tag", Program.items.frames[0].id.ToString()), Items.ItemType.Frame);
            while (ie.MoveNext())
                yield return null;
            replayFrame1Tag = ie.Current;

            im = ABLoader.LoadFrameMaterial(Config.Get("ReplayFrame1Tag", Program.items.frames[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayFrameMat1Tag = im.Current;
            replayFrameMat1Tag.SetTexture("_ProfileFrameTex", replayFrame1Tag.texture);

            #endregion

            #region Protector
            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("DuelProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            duelProtector1Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("WatchProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            watchProtector1Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector0", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector0 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector1", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector1 = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector0Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector0Tag = im.Current;

            im = ABLoader.LoadProtectorMaterial(Config.Get("ReplayProtector1Tag", Program.items.protectors[0].id.ToString()));
            while (im.MoveNext())
                yield return null;
            replayProtector1Tag = im.Current;

            #endregion

            loaded = true;
        }


    }
}
