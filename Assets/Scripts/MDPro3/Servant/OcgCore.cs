using DG.Tweening;
using MDPro3.Duel;
using MDPro3.Duel.BG;
using MDPro3.Duel.YGOSharp;
using MDPro3.Net;
using MDPro3.UI;
using MDPro3.UI.ServantUI;
using MDPro3.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;
using YgomGame.Bg;
using YgomSystem.Effect;
using YgomSystem.ElementSystem;
using static YgomGame.Bg.BgEffectSettingInner;

namespace MDPro3.Servant
{
    public class OcgCore : Servant
    {
        #region Reference
        [Header("OcgCore")]
        public MeshRenderer greenBackground;

        [HideInInspector] public DuelPrefabContainer container;
        [HideInInspector] public ElementObjectManager myDeck;
        [HideInInspector] public ElementObjectManager myExtra;
        [HideInInspector] public ElementObjectManager opDeck;
        [HideInInspector] public ElementObjectManager opExtra;
        [HideInInspector] public Material myProtector;
        [HideInInspector] public Material opProtector;
        [HideInInspector] public BgEffectManager field0Manager;
        [HideInInspector] public BgEffectManager field1Manager;
        [HideInInspector] public BgEffectManager grave0Manager;
        [HideInInspector] public BgEffectManager grave1Manager;
        [HideInInspector] public BgEffectManager stand0Manager;
        [HideInInspector] public BgEffectManager stand1Manager;
        [HideInInspector] public PopupDuel currentPopup;
        [HideInInspector] public GameObject fieldSummonRightInfo;

        [HideInInspector] public bool inAi;
        [HideInInspector] public bool isTag;
        [HideInInspector] public bool mycardDuel;
        [HideInInspector] public Deck deck { get; set; }
        [HideInInspector] public Deck sideReference = new();
        [HideInInspector] public float handOffset;
        [HideInInspector] public float lastHandOffset;
        [HideInInspector] public bool clickingHandCard;
        [HideInInspector] public bool handCardDraged;
        public static bool inputMode;
        public static bool Accing;
        public static bool CurrentReplayUseYRP2;
        public static Condition condition = Condition.N;
        public static ChainCondition chainCondition = ChainCondition.Smart;
        public Action startCard;

        private static float mate0ClickIntime;
        private static float mate1ClickIntime;
        private float clickInPosition;

        private static float handCellX = 30f;
        private int handCount;
        private bool mate0Random;
        private bool mate1Random;

        private GameObject field0;
        private GameObject field1;
        private GameObject grave0;
        private GameObject grave1;
        private GameObject stand0;
        private GameObject stand1;
        private Mate mate0;
        private Mate mate1;
        private GameObject phaseButton;
        private GameObject timer;
        private GameObject attackLine;
        private GameObject targetLine;
        private GameObject equipLine;
        private GameObject myDice;
        private GameObject opDice;
        private GameObject duelFinalBlow;
        private ElementObjectManager timerManager;
        private TimerHandler timerHandler;
        private PlayableGuide playableGuide;
        private DuelButton btnConfirm;
        private DuelButton btnCancel;

        #endregion

        #region Servants

        public override int Depth => -1;
        protected override bool ShowLine => false;

        public override void Initialize()
        {
            base.Initialize();
            SystemEvent.OnResolutionChange += RefreshHandCardPositionInstant;
            StartCoroutine(InitializeAsync());
        }

        private IEnumerator InitializeAsync()
        {
            var handle = Addressables.LoadAssetAsync<DuelPrefabContainer>("ScriptableObjects/DuelPrefabs.asset");
            while (!handle.IsDone)
                yield return null;
            container = handle.Result;

            btnConfirm = Instantiate(container.duelButton).GetComponent<DuelButton>();
            btnConfirm.response.Add(-4);
            btnConfirm.hint = InterString.Get("确认");
            btnConfirm.type = ButtonType.Decide;
            btnConfirm.Hide();

            btnCancel = Instantiate(container.duelButton).GetComponent<DuelButton>();
            btnCancel.response.Add(-5);
            btnCancel.hint = InterString.Get("取消");
            btnCancel.type = ButtonType.Cancel;
            btnCancel.Hide();
        }

        protected override void ApplyShowArrangement(int preDepth)
        {
            servantUI.gameObject.SetActive(true);
            servantUI.Show(true);
            StartCoroutine(LoadAssets());
            CameraBack();
            returnAction = null;
        }

        protected override void ApplyHideArrangement(int preDepth)
        {
            StartCoroutine(ExitDuel());
        }

        public override void OnExit()
        {
            base.OnExit();
            CloseConnection();
            GetUI<OcgCoreUI>().OnNor();
        }

        public void ReturnTo()
        {
            if (returnServant != null)
                Program.instance.ShiftToServant(returnServant);
            else
                Program.instance.ShiftToServant(Program.instance.online);
        }

        public override void PerFrameFunction()
        {
            if (!showing)
                return;

            if (packages.Count > 0)
                Sibyl();

            if (TimelineManager.skippable
                && UserInput.MouseLeftDown)
            {
                Program.instance.timeline_.Skip();
                return;
            }

            if (speaking && UserInput.MouseLeftDown)
            {
                speaking = false;
                speakBreaking = true;
            }

            if (!EventSystem.current.IsPointerOverGameObject()
                && UserInput.HoverObject == null
                && UserInput.MouseLeftUp)
            {
                GetUI<OcgCoreUI>().CardDescription.Hide();
                GetUI<OcgCoreUI>().CardList.Hide();
            }

            #region Background

            if (field0 != null && UserInput.HoverObject == field0 && UserInput.MouseLeftUp)
                field0Manager.PlayTapAnimation();
            if (field1 != null && UserInput.HoverObject == field1 && UserInput.MouseLeftUp)
                field1Manager.PlayTapAnimation();
            if (mate0 != null && UserInput.HoverObject == mate0.gameObject && UserInput.MouseLeftDown)
                mate0ClickIntime = Time.time;

            if (mate0 != null && UserInput.HoverObject == mate0.gameObject && UserInput.MouseLeftUp)
            {
                if (Time.time - mate0ClickIntime < 0.3f)
                    mate0.Play(Mate.MateAction.Tap);
                else
                {
                    if (cameraState == CameraState.Main)
                        CameraZoomToMate0();
                    else
                        CameraBack();
                }
            }

            if (mate1 != null && UserInput.HoverObject == mate1.gameObject && UserInput.MouseLeftDown)
                mate1ClickIntime = Time.time;

            if (mate1 != null && UserInput.HoverObject == mate1.gameObject && UserInput.MouseLeftUp)
            {
                if (Time.time - mate1ClickIntime < 0.3f)
                    mate1.Play(Mate.MateAction.Tap);
                else
                {
                    if (cameraState == CameraState.Main)
                        CameraZoomToMate1();
                    else
                        CameraBack();
                }
            }

            if (mate0 != null && mate0Random)
            {
                mate0.Play(Mate.MateAction.Random);
                mate0Random = false;
                DOTween.To(v => { }, 0, 0, UnityEngine.Random.Range(8, 16)).OnComplete(() =>
                {
                    mate0Random = true;
                });
            }
            if (mate1 != null && mate1Random)
            {
                mate1.Play(Mate.MateAction.Random);
                mate1Random = false;
                DOTween.To(v => { }, 0, 0, UnityEngine.Random.Range(8, 16)).OnComplete(() =>
                {
                    mate1Random = true;
                });
            }

            #endregion

            #region HandOffset

            if (GetMyHandCount() > 10)
            {
                if (UserInput.HoverObject != null
                    && UserInput.HoverObject.name == "CardModel"
                    && UserInput.HoverObject.GetComponent<GameCardMono>().cookieCard.p.controller == 0
                    && (UserInput.HoverObject.GetComponent<GameCardMono>().cookieCard.p.location & (uint)CardLocation.Hand) > 0
                    && UserInput.MouseLeftDown
                    )
                {
                    clickInPosition = UserInput.MousePos.x;
                    clickingHandCard = true;
                    handCount = GetMyHandCount();
                }
                if (clickingHandCard && UserInput.MouseLeftPressing)
                {
                    var currentOffset = lastHandOffset + UserInput.MousePos.x - clickInPosition;
                    var currentHandCellX = UIManager.ScreenLengthWithScalerX(handCellX);
                    handOffset = currentOffset > (handCount * currentHandCellX) ?
                        handCount * currentHandCellX :
                        Math.Abs(currentOffset) > (handCount * currentHandCellX) ?
                        -(handCount * currentHandCellX) :
                        currentOffset;
                }
                if (UserInput.MouseLeftUp)
                {
                    handCardDraged = false;
                    if (clickingHandCard)
                    {
                        clickingHandCard = false;
                        if (lastHandOffset != handOffset)
                        {
                            handCardDraged = true;
                            lastHandOffset = handOffset;
                        }
                    }
                }
            }
            else
            {
                if (handOffset != 0)
                {
                    handOffset = 0;
                    lastHandOffset = 0;
                    RefreshHandCardPositionInstant();
                }
            }

            #endregion

            #region Hot Key

            if (UserInput.MouseRightDown || UserInput.WasCancelPressed)
            {
                returnAction?.Invoke();
            }
            if (UserInput.MouseLeftDown)
            {
                if (equipLine != null)
                    equipLine.SetActive(false);
                foreach (var line in targetLines)
                    Destroy(line);
            }

            if ((UserInput.WasCancelPressed
                || UserInput.WasSubmitPressed))
            {
                ToChat();
            }

            if (Program.instance.ui_.chatPanel.showing || inputMode)
                return;

            if (Keyboard.current != null)
            {
                // Mate Viewer
                if (Keyboard.current.qKey.wasPressedThisFrame)
                    CameraZoomToMate0();
                else if (Keyboard.current.eKey.wasPressedThisFrame)
                    CameraZoomToMate1();
                else if (Keyboard.current.wKey.wasPressedThisFrame)
                    CameraBack();

                // Timing
                if (Keyboard.current.aKey.wasPressedThisFrame)
                {
                    chainCondition = ChainCondition.All - 1;
                    GetUI<OcgCoreUI>().OnTiming();
                }
                else if (Keyboard.current.sKey.wasPressedThisFrame)
                {
                    chainCondition = ChainCondition.No - 1;
                    GetUI<OcgCoreUI>().OnTiming();
                }
                else if (Keyboard.current.dKey.wasPressedThisFrame)
                {
                    chainCondition = ChainCondition.Smart - 1;
                    GetUI<OcgCoreUI>().OnTiming();
                }

                // Log
                if (Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    GetUI<OcgCoreUI>().OnLog();
                }

                // Green
                if (Keyboard.current.gKey.wasPressedThisFrame)
                {
                    if (greenOn)
                        GreenBackgroundOff();
                    else
                        GreenBackgroundOn();
                }

                if (greenOn)
                {
                    if (Keyboard.current.numpad0Key.wasPressedThisFrame
                        || Keyboard.current.digit0Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.black;
                    }
                    else if (Keyboard.current.numpad1Key.wasPressedThisFrame
                        || Keyboard.current.digit1Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.red;
                    }
                    else if (Keyboard.current.numpad2Key.wasPressedThisFrame
                        || Keyboard.current.digit2Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = new Color(1f, 0.5f, 0f);
                    }
                    else if (Keyboard.current.numpad3Key.wasPressedThisFrame
                        || Keyboard.current.digit3Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = new Color(1f, 1f, 0f);
                    }
                    else if (Keyboard.current.numpad4Key.wasPressedThisFrame
                        || Keyboard.current.digit4Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.green;
                    }
                    else if (Keyboard.current.numpad5Key.wasPressedThisFrame
                        || Keyboard.current.digit5Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = new Color(0f, 1f, 1f);
                    }
                    else if (Keyboard.current.numpad6Key.wasPressedThisFrame
                        || Keyboard.current.digit6Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.blue;
                    }
                    else if (Keyboard.current.numpad7Key.wasPressedThisFrame
                        || Keyboard.current.digit7Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = new Color(0.6f, 0f, 1f);
                    }
                    else if (Keyboard.current.numpad8Key.wasPressedThisFrame
                        || Keyboard.current.digit8Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.gray;
                    }
                    else if (Keyboard.current.numpad9Key.wasPressedThisFrame
                        || Keyboard.current.digit9Key.wasPressedThisFrame)
                    {
                        greenBackground.material.color = Color.white;
                    }
                }
            }

            #endregion

        }

        bool greenOn;
        public void GreenBackgroundOn()
        {
            greenBackground.gameObject.SetActive(true);
            GetUI<OcgCoreUI>().CG.alpha = 0f;
            GetUI<OcgCoreUI>().CG.blocksRaycasts = false;
            CameraManager.DuelOverlay3DPlus();

            greenOn = true;
        }

        public void GreenBackgroundOff()
        {
            greenBackground.gameObject.SetActive(false);
            GetUI<OcgCoreUI>().CG.alpha = 1f;
            GetUI<OcgCoreUI>().CG.blocksRaycasts = true;
            CameraManager.DuelOverlay3DMinus();

            greenOn = false;
        }

        public void ToChat()
        {
            if (condition == Condition.Replay || inAi)
                return;
            Program.instance.ui_.chatPanel.Switch();
        }

        private enum CameraState
        {
            Main,
            Mate0,
            Mate1
        }

        CameraState cameraState = CameraState.Main;

        public void CameraZoomToMate0()
        {
            if (Program.root == Program.PATH_ROOT_ANDROID)
                return;

            if (Config.Get("MateViewTips", "0") == "0")
            {
                MessageManager.Cast(InterString.Get("长按宠物再松开后即可返回正常视角。"));
                Config.Set("MateViewTips", "1");
                Config.Save();
            }

            cameraState = CameraState.Mate0;
            if (mate0.huge)
            {
                Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37), 0.3f).SetEase(Ease.InOutSine);
                Program.instance.camera_.cameraMain.transform.DORotate(new Vector3(60, -50, -10), 0.3f).SetEase(Ease.InOutSine);
            }
            else
            {
                Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(-16, 36, -30), 0.3f).SetEase(Ease.InOutSine);
                Program.instance.camera_.cameraMain.transform.DORotate(new Vector3(37, -26, 0), 0.3f).SetEase(Ease.InOutSine);
            }
        }

        public void CameraZoomToMate1()
        {
            if (Program.root == Program.PATH_ROOT_ANDROID)
                return;

            if (Config.Get("MateViewTips", "0") == "0")
            {
                MessageManager.Cast(InterString.Get("长按宠物再松开后即可返回正常视角。"));
                Config.Set("MateViewTips", "1");
                Config.Save();
            }

            cameraState = CameraState.Mate1;
            if (mate1.huge)
            {
                Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37), 0.3f).SetEase(Ease.InOutSine);
                Program.instance.camera_.cameraMain.transform.DORotate(new Vector3(60, 33, 10), 0.3f).SetEase(Ease.InOutSine);
            }
            else
            {
                Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(23, 24, -17), 0.3f).SetEase(Ease.InOutSine);
                Program.instance.camera_.cameraMain.transform.DORotate(new Vector3(25, 23, 0), 0.3f).SetEase(Ease.InOutSine);
            }
        }

        public void CameraBack()
        {
            cameraState = CameraState.Main;
            Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37), 0.3f).SetEase(Ease.InOutSine);
            Program.instance.camera_.cameraMain.transform.DORotate(new Vector3(70, 0, 0), 0.3f).SetEase(Ease.InOutSine);
        }

        private IEnumerator ExitDuel()
        {
            ClearResponse();
            CameraManager.BlackOut(0f, 0.3f);
            CameraBack();
            UIManager.UIBlackIn(TransitionTime);
            TimelineManager.inSummonMaterial = false;
            GetUI<OcgCoreUI>().CloseHint();
            attackLine.SetActive(false);
            Destroy(duelFinalBlow, 0.5f);
            yield return new WaitForSeconds(TransitionTime);
            servantUI.ShutDown();
            speaking = false;
            speakBreaking = false;
            waitForNoWaitingVoice = false;

            packages.Clear();
            allPackages.Clear();
            AudioManager.ResetSESource();
            mycardDuel = false;
            CloseCharaFace();

            foreach (var o in turnEndDeleteObjects)
                Destroy(o);
            turnEndDeleteObjects.Clear();
            foreach (var o in allGameObjects)
                Destroy(o);
            allGameObjects.Clear();

            foreach (var card in cards)
                card.Dispose();
            cards.Clear();
            pause = false;
            nextMoveAction = null;
            cachedCharaFaces.Clear();
            GC.Collect();
            CameraManager.ShiftTo2D();
            GetUI<OcgCoreUI>().Buttons.SetActive(false);
            GetUI<OcgCoreUI>().DuelLog.ClearLog();
            Program.instance.ui_.chatPanel.Hide();
            yield return new WaitForSeconds(0.3f);
            UIManager.UIBlackOut(TransitionTime);
            yield return new WaitForSeconds(TransitionTime);
            UIManager.ShowFPSRight();
            AudioManager.PlayBGM(AudioManager.BGM_MENU_MAIN);
        }

#if UNITY_STANDALONE_OSX || UNITY_IOS
        private const string TIMER_TEXTURE = "_SampleTexture2D_4791db607d671180b2a839392ec5ea21_Texture_1_Texture2D";
#else
        private const string TIMER_TEXTURE = "_SampleTexture2D_4791db607d671180b2a839392ec5ea21_Texture_1";
#endif

        private IEnumerator LoadAssets()
        {
            GetUI<OcgCoreUI>().CG.alpha = 0f;
            GetUI<OcgCoreUI>().CG.blocksRaycasts = false;

            messagePass = false;
            mate0Random = false;
            mate1Random = false;
            deck = null;
            var deckName = Config.GetConfigDeckName();

            if (condition == Condition.Duel && inAi == false && File.Exists(Program.PATH_DECK + deckName + Program.EXPANSION_YDK))
                deck = new Deck(Program.PATH_DECK + deckName + Program.EXPANSION_YDK);
            UIManager.UIBlackIn(TransitionTime);
            yield return new WaitForSeconds(TransitionTime);
            while (!Appearance.loaded)
                yield return null;
            CameraManager.ShiftTo3D();
            UIManager.HideExitButton(0);
            UIManager.HideLine(0);
            AudioManager.StopBGM();

            #region Attack Line
            if (attackLine == null)
            {
                var ie = ABLoader.LoadFromFileAsync("MasterDuel/Effects/Other/fxp_atk_select_arrow_001");
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                attackLine = ie.Current;

                var lineManager = attackLine.GetComponent<ElementObjectManager>();
                var line1 = lineManager.GetElement<LineRenderer>("arrowlimeRollover");
                var line2 = lineManager.GetElement<LineRenderer>("arrowRollover");
                line1.sortingLayerName = "DuelEffect_High";
                line2.sortingLayerName = "DuelEffect_High";
                line1.material.renderQueue = 4000;
                line2.material.renderQueue = 4000;

#if UNITY_STANDALONE_OSX || UNITY_IOS
                var suffix = "_Texture2D";
#endif
                var exposed = "_Texture2DAsset_b6d1fd99174c608f800b61fcd5471719_Out_0";
#if UNITY_STANDALONE_OSX || UNITY_IOS
                exposed += suffix;
#endif
                line1.material.SetTexture(exposed, TextureManager.container.fxt_Arrow_003);
                line2.material.SetTexture(exposed, TextureManager.container.fxt_Arrow_002);

                exposed = "_Texture2DAsset_866488b0fc8d338ca1244df079d54189_Out_0";
#if UNITY_STANDALONE_OSX || UNITY_IOS
                exposed += suffix;
#endif
                line1.material.SetTexture(exposed, TextureManager.container.fxt_Arrow_002);

                exposed = "_Texture2DAsset_7258d16c8bba4ee4a9ec0071720a13ad_Out_0";
#if UNITY_STANDALONE_OSX || UNITY_IOS
                exposed += suffix;
#endif
                line1.material.SetTexture(exposed, TextureManager.container.fxt_msk_005);

                attackLine.SetActive(false);
            }
#endregion

            #region Target Line
            if (targetLine == null)
            {
                var ie = ABLoader.LoadFromFileAsync("MasterDuel/Effects/Other/fxp_target_arrow_001");
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                targetLine = ie.Current;
                var line = targetLine.transform.GetChild(0).GetComponent<LineRenderer>();
                line.sortingLayerName = "DuelEffect_High";
                line.material.renderQueue = 4000;
                line.material.SetTexture("_Texture2DAsset_b6d1fd99174c608f800b61fcd5471719_Out_0", TextureManager.container.fxt_Arrow);
                line.material.SetTexture("_Texture2DAsset_46a0b6b632b7ad8a9a0dbeab8e0a7fa5_Out_0", TextureManager.container.fxt_Arrow_004);
                line.material.SetTexture("_Texture2DAsset_4f4a26709ec6ff8d9e69ef02918507d2_Out_0", TextureManager.container.fxt_Arrow);
                line.material.SetTexture("_Texture2DAsset_ba8237ebbd5d078c896d47d3e15b10dc_Out_0", TextureManager.container.fxt_Arrow);
                targetLine.SetActive(false);
            }
            #endregion

            #region Equip Line
            if (equipLine == null)
            {
                var ie = ABLoader.LoadFromFileAsync("MasterDuel/Effects/Other/fxp_equip_arrow_001");
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                equipLine = ie.Current;
                var line = equipLine.transform.GetChild(0).GetComponent<LineRenderer>();
                line.sortingLayerName = "DuelEffect_High";
                line.material.renderQueue = 4000;
                line.material.SetTexture("_Texture2DAsset_5b426b3b88fc4e3c873ed973f68902bd_Out_0", TextureManager.container.fxt_Arrow);
                line.material.SetTexture("_Texture2DAsset_32775df679384275b23b5efed70b243e_Out_0", TextureManager.container.fxt_Arrow_004);
                equipLine.SetActive(false);
            }
            #endregion

            #region Dice
            if (myDice == null)
            {
                var ie = ABLoader.LoadFromFolderAsync("MasterDuel/Timeline/DuelDice");
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                myDice = ie.Current;
                Destroy(myDice);
                if (myDice.transform.GetChild(0).GetComponent<PlayableDirector>() == null)
                    myDice = myDice.transform.GetChild(1).gameObject;
                else
                    myDice = myDice.transform.GetChild(0).gameObject;
                myDice.transform.SetParent(Program.instance.container_3D, false);
                myDice.gameObject.SetActive(false);
            }
            if (opDice == null)
            {
                var ie = ABLoader.LoadFromFolderAsync("MasterDuel/Timeline/DuelDiceEn");
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                opDice = ie.Current;
                Destroy(opDice);
                if (opDice.transform.GetChild(0).GetComponent<PlayableDirector>() == null)
                    opDice = opDice.transform.GetChild(1).gameObject;
                else
                    opDice = opDice.transform.GetChild(0).gameObject;
                opDice.transform.SetParent(Program.instance.container_3D, false);
                opDice.gameObject.SetActive(false);
            }
            #endregion

            #region FieldSummonRightInfo
            if (fieldSummonRightInfo == null)
            {
                var handle = Addressables.InstantiateAsync("Prefab/FieldSummonRightInfo.prefab");
                handle.Completed += (result) =>
                {
                    fieldSummonRightInfo = result.Result;
                    fieldSummonRightInfo.SetActive(false);
                    fieldSummonRightInfo.transform.SetParent(Program.instance.container_3D);
                };
            }
            #endregion

            #region 场地
            var path = Program.items.GetAssetPath(
                Config.Get(condition.ToString() + "Field0",
                Program.items.mats[0].id.ToString()), Items.ItemType.Mat);
            if (deck != null && !Config.GetBool("OverrideDeckAppearance", false))
                path = Program.items.GetAssetPath(deck.Field.ToString(), Items.ItemType.Mat);
            path = "MasterDuel/" + path;
            var enumerator = ABLoader.LoadFromFileAsync(path + "_near");
            while (enumerator.MoveNext())
                yield return null;
            field0 = enumerator.Current;
            field0.transform.SetParent(Program.instance.container_3D, false);

            enumerator = ABLoader.LoadFromFileAsync("MasterDuel/" +
                Program.items.GetAssetPath(Config.Get(condition.ToString() + "Field1",
                Program.items.mats[0].id.ToString()), Items.ItemType.Mat, 1) + "_far");
            while (enumerator.MoveNext())
                yield return null;
            field1 = enumerator.Current;
            field1.transform.SetParent(Program.instance.container_3D, false);

            allGameObjects.Add(field0);
            allGameObjects.Add(field1);


            var collider = field0.AddComponent<BoxCollider>();
            collider.center = new Vector3(38, 5, -10);
            collider.size = new Vector3(10, 10, 10);
            collider = field1.AddComponent<BoxCollider>();
            collider.center = new Vector3(-38, 5, 10);
            collider.size = new Vector3(10, 10, 10);

            field0Manager = field0.GetComponent<BgEffectManager>();
            field1Manager = field1.GetComponent<BgEffectManager>();

            Transform pos_Grave_near = field0.transform.GetChildByName("POS_Grave_near");
            Transform pos_Grave_far = field1.transform.GetChildByName("POS_Grave_far");
            Transform pos_AvatarStand_near = field0.transform.GetChildByName("POS_AvatarStand_near");
            Transform pos_AvatarStand_far = field1.transform.GetChildByName("POS_AvatarStand_far");
            Transform pos_Avatar_near = field0.transform.GetChildByName("POS_Avatar_near");
            Transform pos_Avatar_far = field1.transform.GetChildByName("POS_Avatar_far");
            #endregion

            #region 墓地
            path = Program.items.GetAssetPath(
                Config.Get(condition.ToString() + "Grave0",
                Program.items.graves[0].id.ToString()), Items.ItemType.Grave);
            if (deck != null && !Config.GetBool("OverrideDeckAppearance", false))
                path = Program.items.GetAssetPath(deck.Grave.ToString(), Items.ItemType.Grave);
            path = "MasterDuel/" + path;
            enumerator = ABLoader.LoadFromFileAsync(path + "_near");
            while (enumerator.MoveNext())
                yield return null;
            grave0 = enumerator.Current;
            grave0.transform.SetParent(pos_Grave_near, false);
            enumerator = ABLoader.LoadFromFileAsync("MasterDuel/" +
                Program.items.GetAssetPath(Config.Get(condition.ToString() + "Grave1",
                Program.items.graves[0].id.ToString()), Items.ItemType.Grave, 1) + "_far");
            while (enumerator.MoveNext())
                yield return null;
            grave1 = enumerator.Current;
            grave1.transform.SetParent(pos_Grave_far, false);

            Tools.PlayAnimation(grave0.transform, "StartToPhase1");
            Tools.PlayAnimation(grave1.transform, "StartToPhase1");

            graves.Clear();
            grave0Manager = grave0.GetComponent<BgEffectManager>();
            grave1Manager = grave1.GetComponent<BgEffectManager>();
            var g0 = grave0.AddComponent<GraveBehaviour>();
            g0.controller = 0;
            graves.Add(g0);
            var g1 = grave1.AddComponent<GraveBehaviour>();
            g1.controller = 1;
            graves.Add(g1);
            #endregion

            #region 站台
            var standConfig = Config.Get(condition.ToString() + "Stand0", Program.items.stands[0].id.ToString());
            if (standConfig != Items.CODE_NONE.ToString() || deck != null)
            {
                path = Program.items.GetAssetPath(standConfig, Items.ItemType.Stand);
                if (deck != null && !Config.GetBool("OverrideDeckAppearance", false))
                    path = Program.items.GetAssetPath(deck.Stand.ToString(), Items.ItemType.Stand);
                path = "MasterDuel/" + path;
                enumerator = ABLoader.LoadFromFileAsync(path + "_near");
                while (enumerator.MoveNext())
                    yield return null;
                stand0 = enumerator.Current;
                stand0.transform.SetParent(pos_AvatarStand_near, false);

                pos_Avatar_near = stand0.transform.GetChildByName("POS_Avatar_near");
                Tools.PlayAnimation(stand0.transform, "StartToPhase1");
                stand0Manager = stand0.GetComponent<BgEffectManager>();
            }

            standConfig = Config.Get(condition.ToString() + "Stand1", Program.items.stands[0].id.ToString());
            if (standConfig != Items.CODE_NONE.ToString())
            {
                enumerator = ABLoader.LoadFromFileAsync("MasterDuel/" +
                Program.items.GetAssetPath(standConfig, Items.ItemType.Stand, 1) + "_far");
                while (enumerator.MoveNext())
                    yield return null;
                stand1 = enumerator.Current;
                stand1.transform.SetParent(pos_AvatarStand_far, false);

                pos_Avatar_far = stand1.transform.GetChildByName("POS_Avatar_far");
                Tools.PlayAnimation(stand1.transform, "StartToPhase1");
                stand1Manager = stand1.GetComponent<BgEffectManager>();
            }

            #endregion

            #region 宠物
            var mateConfig = Config.Get(condition.ToString() + "Mate0", Program.items.mates[0].id.ToString());
            if (mateConfig != Items.CODE_NONE.ToString() || deck != null)
            {
                int mateCode = int.Parse(mateConfig);
                if (deck != null && !Config.GetBool("OverrideDeckAppearance", false))
                    mateCode = deck.Mate;
                var mateLoader = ABLoader.LoadMateAsync(mateCode);
                while (mateLoader.MoveNext())
                    yield return null;
                if(mateLoader.Current != null)
                {
                    mate0 = mateLoader.Current;
                    mate0.parent = pos_Avatar_near;
                }
            }

            mateConfig = Config.Get(condition.ToString() + "Mate1", Program.items.mates[0].id.ToString());
            if (mateConfig != Items.CODE_NONE.ToString())
            {
                var mateLoader = ABLoader.LoadMateAsync(int.Parse(Config.Get(condition.ToString() + "Mate1", Program.items.mates[0].id.ToString())));
                while (mateLoader.MoveNext())
                    yield return null;
                if (mateLoader.Current != null)
                {
                    mate1 = mateLoader.Current;
                    mate1.parent = pos_Avatar_far;
                }
            }
            #endregion

            #region 场地背景
            enumerator = ABLoader.LoadFromFileAsync("MasterDuel/BG/CelestialSphere_c001");
            while (enumerator.MoveNext())
                yield return null;
            var matBack = enumerator.Current;
            matBack.transform.SetParent(Program.instance.container_3D, false);
            matBack.transform.localScale = Vector3.one * 2;
            allGameObjects.Add(matBack);
            #endregion

            #region 阶段按钮
            if (field1.name.StartsWith("Mat_013"))
            {
                enumerator = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PhaseButton_013");
                while (enumerator.MoveNext())
                    yield return null;
                phaseButton = enumerator.Current;
                phaseButton.GetComponent<Animator>().SetTrigger("Start");
                Tools.PlayAnimation(phaseButton.transform, "StartToPhase1");
            }
            else
            {
                enumerator = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/PhaseButton_c001");
                while (enumerator.MoveNext())
                    yield return null;
                phaseButton = enumerator.Current;
                Transform playerPart = phaseButton.transform.Find("PlayerPart");
                Texture texture = playerPart.GetComponent<Renderer>().material.GetTexture("_Texture2D");
                playerPart.GetComponent<Renderer>().material.SetTexture(TIMER_TEXTURE, texture);
                Transform opponentPart = phaseButton.transform.Find("OpponentPart");
                opponentPart.GetComponent<Renderer>().material.SetTexture(TIMER_TEXTURE, texture);
            }
            phaseButton.transform.SetParent(Program.instance.container_3D, false);
            allGameObjects.Add(phaseButton);
            phaseButton.AddComponent<PhaseButtonHandler>();
            #endregion

            #region Timer
            if (condition == Condition.Duel)
            {
                IEnumerator<GameObject> ie;
                if (field1.name.StartsWith("Mat_013"))
                    ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/Timer_013", true);
                else
                    ie = ABLoader.LoadFromFileAsync("MasterDuel/BG/Timer/Timer_c001", true);
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                timer = ie.Current;
                timerManager = timer.GetComponent<ElementObjectManager>();
                timerHandler = timer.AddComponent<TimerHandler>();
                timer.transform.SetParent(Program.instance.container_3D, false);
                timerHandler.timeLimit = timeLimit;
                timerHandler.time = timeLimit;
                allGameObjects.Add(timer);

                if (!field1.name.StartsWith("Mat_013"))
                {
                    var timerRenderer = timerManager.GetElement<Renderer>("Timer");
                    timerRenderer.material.SetTexture(TIMER_TEXTURE
                        , timerRenderer.material.GetTexture("_Texture2D"));
                }
            }
            #endregion

            #region Playable Guide

            playableGuide = Create<PlayableGuide>();
            playableGuide.Load(field0.name, field1.name);
            while (!playableGuide.loaded)
                yield return null;
            if(DeviceInfo.OnAndroid())
                playableGuide.SetHeight(0.6f);

            #endregion

            #region 卡组
            var deckLoad = ABLoader.LoadFromFileAsync("MasterDuel/Timeline/DuelDeckAppearance", true);
            StartCoroutine(deckLoad);
            while (deckLoad.MoveNext())
                yield return null;
            myDeck = deckLoad.Current.GetComponent<ElementObjectManager>();
            var sideManager = myDeck.GetElement<ElementObjectManager>("CardShuffleTop");
            sideManager.GetElement<MeshRenderer>("CardModel01_side").material = MaterialLoader.cardMatSide;
            sideManager.GetElement<MeshRenderer>("CardModel02_side").material = MaterialLoader.cardMatSide;
            sideManager.GetElement<MeshRenderer>("CardModel03_side").material = MaterialLoader.cardMatSide;
            sideManager.GetElement<MeshRenderer>("CardModel04_side").material = MaterialLoader.cardMatSide;

            myExtra = Instantiate(deckLoad.Current).GetComponent<ElementObjectManager>();
            opDeck = Instantiate(deckLoad.Current).GetComponent<ElementObjectManager>();
            opExtra = Instantiate(deckLoad.Current).GetComponent<ElementObjectManager>();

            myDeck.transform.SetParent(field0.transform, false);
            opDeck.transform.SetParent(field1.transform, false);
            myExtra.transform.SetParent(field0.transform, false);
            opExtra.transform.SetParent(field1.transform, false);
            myDeck.transform.localPosition = new Vector3(26.6f, 1.5f, -23.5f);
            myDeck.transform.localEulerAngles = new Vector3(0, -20f, 0);
            myExtra.transform.localPosition = new Vector3(-26.6f, 1.5f, -23.5f);
            myExtra.transform.localEulerAngles = new Vector3(0, 20f, 0);
            opDeck.transform.localPosition = new Vector3(-26.6f, 1.5f, 23.5f);
            opDeck.transform.localEulerAngles = new Vector3(0, 160f, 0);
            opExtra.transform.localPosition = new Vector3(26.6f, 1.5f, 23.5f);
            opExtra.transform.localEulerAngles = new Vector3(0, -160f, 0);
            allGameObjects.Add(myDeck.gameObject);
            allGameObjects.Add(opDeck.gameObject);
            allGameObjects.Add(myExtra.gameObject);
            allGameObjects.Add(opExtra.gameObject);

            var deckMat = Appearance.duelProtector0;
            if (deck != null && !Config.GetBool("OverrideDeckAppearance", false))
            {
                var ie = ABLoader.LoadProtectorMaterial(deck.Protector.ToString());
                StartCoroutine(ie);
                while (ie.MoveNext())
                    yield return null;
                if (ie.Current != null)
                    deckMat = ie.Current;
            }

            if (condition == Condition.Duel)
                myProtector = deckMat;
            else if (condition == Condition.Watch)
                myProtector = Appearance.watchProtector0;
            else if (condition == Condition.Replay)
                myProtector = Appearance.replayProtector0;

            foreach (var r in myDeck.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = myProtector;
            foreach (var r in myExtra.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = myProtector;

            if (condition == Condition.Duel)
                opProtector = Appearance.duelProtector1;
            else if (condition == Condition.Watch)
                opProtector = Appearance.watchProtector1;
            else if (condition == Condition.Replay)
                opProtector = Appearance.replayProtector1;

            foreach (var r in opDeck.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = opProtector;
            foreach (var r in opExtra.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = opProtector;

            myDeck.gameObject.SetActive(false);
            myExtra.gameObject.SetActive(false);
            opDeck.gameObject.SetActive(false);
            opExtra.gameObject.SetActive(false);
            #endregion

            #region 位置选择
            places.Clear();
            for (uint c = 0; c < 2; c++)
            {
                GPS gps = new GPS();
                gps.controller = c;
                gps.location = (uint)CardLocation.Deck;
                CreatePlaceSelector(gps);

                gps = new GPS();
                gps.controller = c;
                gps.location = (uint)CardLocation.Extra;
                CreatePlaceSelector(gps);

                for (uint s = 0; s < (c == 0 ? 7 : 5); s++)
                {
                    gps = new GPS();
                    gps.controller = c;
                    gps.location = (uint)CardLocation.MonsterZone;
                    gps.sequence = s;
                    CreatePlaceSelector(gps);
                }
                for (uint s = 0; s < 6; s++)
                {
                    gps = new GPS();
                    gps.controller = c;
                    gps.location = (uint)CardLocation.SpellZone;
                    gps.sequence = s;
                    CreatePlaceSelector(gps);
                }
            }
            #endregion

            #region Quit Load

            GC.Collect();
            Preload();
            yield return new WaitForSeconds(0.3f);
            //退出加载

            if (NeedVoice())
            {
                GetUI<OcgCoreUI>().CG.alpha = 1;
                GetUI<OcgCoreUI>().CG.blocksRaycasts = true;
            }
            GetUI<OcgCoreUI>().Buttons.SetActive(false);

            UIManager.ShowFPSLeft();
            UIManager.HideBlackBack(0f);
            UIManager.UIBlackOut(TransitionTime);
            yield return new WaitForSeconds(TransitionTime);

            backgroundFieldInitialize = false;
            BackgroundFieldInitialize();
            messagePass = true;

            if (condition == Condition.Duel && Config.GetBool("DuelAutoAcc", false)
                || condition == Condition.Watch && Config.GetBool("WatchAutoAcc", false)
                || condition == Condition.Replay && Config.GetBool("ReplayAutoAcc", false))
                GetUI<OcgCoreUI>().OnAcc();

            #endregion

        }

        private bool backgroundFieldInitialize = false;
        private void BackgroundFieldInitialize()
        {
            if (field0 == null || field1 == null)
                return;

            if (backgroundFieldInitialize)
            {
                field0.SetActive(false);
                field1.SetActive(false);
                field0.SetActive(true);
                field1.SetActive(true);
            }

            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1);
            grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1);
            //field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1Extra);
            //grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1Extra);
            bgPhase0 = 1;
            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1);
            grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1);
            //field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1Extra);
            //grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.StartToPhase1Extra);
            bgPhase1 = 1;
            if (mate0 != null)
            {
                mate0.gameObject.SetActive(true);
                mate0.Play(Mate.MateAction.Entry);
            }
            if (mate1 != null)
            {
                mate1.gameObject.SetActive(true);
                mate1.Play(Mate.MateAction.Entry);
            }
            if (timerHandler != null)
                timerHandler.DuelStart();
            DOTween.To(v => { }, 0, 0, UnityEngine.Random.Range(8, 16)).OnComplete(() =>
            {
                mate0Random = true;
            });
            DOTween.To(v => { }, 0, 0, UnityEngine.Random.Range(8, 16)).OnComplete(() =>
            {
                mate1Random = true;
            });

            backgroundFieldInitialize = true;
        }

#endregion

        #region Button Function

        public void OnAnnounceCard(string input)
        {
            var datas = CardsManager.AnnounceSearch(input, ES_searchCodes);
            var max = datas.Count;
            if (max > 49)
                max = 40;
            List<GameCard> cards = new List<GameCard>();
            for (var i = 0; i < max; i++)
            {
                var p = new GPS
                {
                    controller = 0,
                    location = (uint)CardLocation.Search,
                    sequence = (uint)i,
                    position = 0
                };
                var card = GCS_Create(p);
                card.SetData(datas[i]);
                cards.Add(card);
            }
            currentPopup.whenQuitDo = () => 
            { 
                GetUI<OcgCoreUI>().ShowPopupSelectCard
                (InterString.Get("请选择需要宣言的卡片。"), cards, 1, 1, true, false); 
            };
        }

        public void ClearAnnounceCards()
        {
            List<GameCard> needClean = new List<GameCard>();
            foreach (var card in cards)
                if (card.p.location == (uint)CardLocation.Search)
                    needClean.Add(card);
            foreach (var card in needClean)
            {
                cards.Remove(card);
                card.Dispose();
            }
        }

        public void CloseConnection()
        {
            if (TcpHelper.tcpClient != null)
            {
                if (TcpHelper.tcpClient.Connected)
                {
                    TcpHelper.tcpClient.Client.Shutdown(0);
                    TcpHelper.tcpClient.Close();
                }
                TcpHelper.tcpClient = null;
            }
        }

        public void OnDuelResultConfirmed(bool manual = false)
        {
            RoomServant.JoinWithReconnect = false;

            if(condition != Condition.Watch)
            {
                if (Program.instance.room.duelEnded
                    || surrendered
                    || TcpHelper.tcpClient == null
                    || !TcpHelper.tcpClient.Connected)
                {
                    surrendered = false;
                    Program.instance.room.duelEnded = false;
                    RoomServant.NeedSide = false;
                    RoomServant.SideWaitingObserver = false;
                    if (Program.instance.currentSubServant != null)
                    {
                        Program.instance.currentSubServant.Hide(-1);
                        Program.instance.currentSubServant = null;
                    }
                    OnExit();
                    return;
                }
            }

            if (RoomServant.NeedSide)
            {
                RoomServant.NeedSide = false;
                MessageManager.Cast(InterString.Get("卡片历史中为您准备了对手上一局使用过的卡。"));
                //returnServant = Program.instance.editDeck;
                //Program.instance.editDeck.SwitchCondition(EditDeck.Condition.ChangeSide);
                returnServant = Program.instance.deckEditor;
                Program.instance.deckEditor.SwitchCondition(DeckEditor.Condition.ChangeSide);
                ReturnTo();
                return;
            }

            if (condition == Condition.Watch)
            {
                if (manual)
                {
                    surrendered = false;
                    Program.instance.room.duelEnded = false;
                    RoomServant.NeedSide = false;
                    RoomServant.SideWaitingObserver = false;
                    if (Program.instance.currentSubServant != null)
                    {
                        Program.instance.currentSubServant.Hide(-1);
                        Program.instance.currentSubServant = null;
                    }
                    TcpHelper.CtosMessage_LeaveGame();
                    OnExit();
                }
                else
                {
                    if (duelEnded)
                    {
                        if (TcpHelper.tcpClient == null
                            || !TcpHelper.tcpClient.Connected)
                            OnExit();
                        else
                            Hide(0);
                    }
                    else
                    {
                        field0.SetActive(false);
                        field1.SetActive(false);
                        field0.SetActive(true);
                        field1.SetActive(true);
                    }
                }
                return;
            }

            var selections = new List<string>
            {
                InterString.Get("投降"),
                InterString.Get("您确定要投降吗？"),
                InterString.Get("是"),
                InterString.Get("否")
            };
            UIManager.ShowPopupYesOrNo(selections, ActionSurrender, null);
        }

        private void ActionSurrender()
        {
            surrendered = true;
            if (TcpHelper.tcpClient != null && TcpHelper.tcpClient.Connected)
            {
                TcpHelper.CtosMessage_Surrender();
                Program.instance.ExitCurrentServant();
                if (RoomServant.Mode == 2 && !tagSurrendered)
                    MessageManager.Cast(InterString.Get("您发起了投降。"));
            }
            else
                OnExit();
        }

        #endregion

        #region Message

        [HideInInspector] public List<GameCard> cards = new();
        [HideInInspector] public List<GameCard> tempCards = new();
        [HideInInspector] public string name_0 = "";
        [HideInInspector] public string name_0_c = "";
        [HideInInspector] public string name_0_tag = "";
        [HideInInspector] public string name_1 = "";
        [HideInInspector] public string name_1_c = "";
        [HideInInspector] public string name_1_tag = "";
        [HideInInspector] public int MasterRule;
        [HideInInspector] public int life0;
        [HideInInspector] public int life1;
        [HideInInspector] public int timeLimit = 180;
        [HideInInspector] public int lpLimit = 8000;
        [HideInInspector] public int turns;
        [HideInInspector] public bool isFirst;
        [HideInInspector] public bool isObserver;
        [HideInInspector] public bool myTurn = true;
        [HideInInspector] public DuelPhase phase = DuelPhase.Draw;
        [HideInInspector] public delegate void ResponseHandler(byte[] buffer);
        [HideInInspector] public List<GameObject> allGameObjects = new();
        [HideInInspector] public int mySummonCount;
        [HideInInspector] public int mySpSummonCount;
        [HideInInspector] public int opSummonCount;
        [HideInInspector] public int opSpSummonCount;

        private int md5Maker;
        private int bgPhase0 = 1;
        private int bgPhase1 = 1;
        private int playerType;
        private List<GameObject> turnEndDeleteObjects = new();


        public static bool messagePass;
        public static bool pause;
        public static bool speaking;
        private static bool speakBreaking;
        private static bool waitForNoWaitingVoice;

        private List<Package> packages = new List<Package>();
        private readonly List<Package> allPackages = new List<Package>();
        [HideInInspector] public GameMessage currentMessage = GameMessage.Waiting;
        [HideInInspector] public GameMessage lastMessage = GameMessage.Waiting;
        private int currentMessageIndex = -1;
        public static int MessageBeginTime;
        public ResponseHandler handler = null;

        [HideInInspector] public bool surrendered;
        [HideInInspector] public bool tagSurrendered;
        private bool deckReserved;
        [HideInInspector] public bool cantCheckGrave;
        private readonly List<int> keys = new List<int>();
        private DuelResult result = DuelResult.DisLink;
        private int cookie_matchKill;
        private string winReason = "";
        [HideInInspector] public List<string> confirmedCards = new List<string>();
        [HideInInspector] public GameCard summonCard;
        [HideInInspector] public GameCard lastMoveCard;
        [HideInInspector] public GameCard lastConfirmedCard;
        [HideInInspector] public GameCard attackingCard;
        private Vector3 myPosition = new(0, 15, -25);
        private Vector3 opPosition = new(0, 15, 25);

        private List<GameCard> chainCards = new();

        [HideInInspector] public List<GameCard> materialCards = new List<GameCard>();
        [HideInInspector] public List<GameCard> cardsInChain = new List<GameCard>();
        [HideInInspector] public GameCard currentSolvingCard;
        [HideInInspector] public List<int> codesInChain = new List<int>();
        [HideInInspector] public List<uint> controllerInChain = new List<uint>();
        [HideInInspector] public List<int> negatedInChain = new List<int>();
        [HideInInspector] public List<GameCard> cardsBeTarget = new List<GameCard>();
        [HideInInspector] public List<GameCard> cardsInSelection = new List<GameCard>();
        [HideInInspector] public List<GameCard> cardsMustBeSelected = new List<GameCard>();
        [HideInInspector] public List<int> myActivated = new List<int>();
        [HideInInspector] public List<int> opActivated = new List<int>();


        private string ES_hint;
        [HideInInspector] public int ES_max;
        [HideInInspector] public int ES_min;
        [HideInInspector] public int ES_level;
        [HideInInspector] public bool ES_overFlow;
        [HideInInspector] public string ES_selectHint;
        [HideInInspector] public int Es_selectMSGHintData;
        [HideInInspector] public int Es_selectMSGHintPlayer;
        [HideInInspector] public int Es_selectMSGHintType;
        [HideInInspector] public List<int> ES_searchCodes = new();
        [HideInInspector] public string ES_selectUnselectHint = string.Empty;
        [HideInInspector] public bool ES_selectCardFromFieldFirstFlag = false;
        [HideInInspector] public int ES_sortSum;
        [HideInInspector] public string ES_turnString = string.Empty;

        [HideInInspector] public bool duelEnded;
        //For single duel end
        //Program.instance.room.duelEnded: For match End;

        private bool needDamageResponseInstant;
        public Action endingAction;
        public Action nextMoveAction;
        public Action nextNegateAction;
        public Action nextNegateAction_Additional;
        public float nextNegateAction_AdditionalTime;
        public ElementObjectManager nextNegateAction_AdditionalManager;
        private Renderer nextMoveActionTargetRenderer;

        [HideInInspector] public int lastSelectedCard = 0;
        [HideInInspector] public ElementObjectManager nextMoveManager;
        [HideInInspector] public float nextMoveTime = 0f;
        [HideInInspector] public bool ignoreNextMoveLog;

        public void CoreReset()
        {
            if (cards.Count > 0)
                foreach (GameCard card in cards)
                    card.Dispose();
            cards.Clear();
            tempCards.Clear();
            sideReference = new Deck();
            pause = false;
            duelEnded = false;
            Program.instance.room.duelEnded = false;
            turns = 0;
            handOffset = 0;
            lastHandOffset = 0;
            myPreHandCards.Clear();
            opPreHandCards.Clear();
            needRefreshHand0 = true;
            needRefreshHand1 = true;
            materialCards.Clear();
            cardsInChain.Clear();
            codesInChain.Clear();
            controllerInChain.Clear();
            negatedInChain.Clear();
            cardsBeTarget.Clear();
            cardsInSelection.Clear();
            cardsMustBeSelected.Clear();
            myActivated.Clear();
            opActivated.Clear();
            GetUI<OcgCoreUI>().CardDescription.Hide();
            GetUI<OcgCoreUI>().CardList.Hide();
            surrendered = false;
            tagSurrendered = false;
            deckReserved = false;
            cantCheckGrave = false;
            cookie_matchKill = 0;
            needDamageResponseInstant = false;
            Config.Set("MateViewTips", "0");
            if (condition == Condition.Duel)
            {
                if (Config.GetBool(Config.LABEL_TIMING, false))
                {
                    chainCondition = ChainCondition.All - 1;
                    GetUI<OcgCoreUI>().OnTiming();
                }
                else
                {
                    chainCondition = ChainCondition.Smart - 1;
                    GetUI<OcgCoreUI>().OnTiming();
                }
            }
            GetUI<OcgCoreUI>().HidePlaceCount();
            mySummonCount = 0;
            mySpSummonCount = 0;
            opSummonCount = 0;
            opSpSummonCount = 0;
            Program.instance.room.duelEnded = false;
            RoomServant.JoinWithReconnect = false;
            endingAction = null;
            nextMoveAction = null;
            GetUI<OcgCoreUI>().DuelLog.ClearLog();
            GetUI<OcgCoreUI>().DuelLog.showing = true;
            GetUI<OcgCoreUI>().OnLog(true);

            GetUI<OcgCoreUI>().DuelLog.chainSolving = 0;
            ignoreNextMoveLog = false;
            GetUI<OcgCoreUI>().DuelLog.psum = false;

            greenBackground.gameObject.SetActive(false);
            inputMode = false;
            returnAction = null;
            movingToGrave = 0;
            movingToExclude = 0;
        }

        public void AddPackage(Package p)
        {
            TcpHelper.AddRecordLine(p);
            packages.Add(p);
            allPackages.Add(p);
        }
        public void FlushPackages(List<Package> packages)
        {
            this.packages.Clear();
            this.packages = null;
            this.packages = packages;
            allPackages.Clear();
            foreach (Package p in packages)
                allPackages.Add(p);
        }

        public void SendReturn(byte[] buffer)
        {
            handler?.Invoke(buffer);
            ClearResponse();
        }

        public void Sleep(int framesIn100)
        {
            var illustion = (int)(Program.TimePassed() + framesIn100 * 10f);
            if (illustion > MessageBeginTime) MessageBeginTime = illustion;
        }

        public void OnResend()
        {
            var binaryMaster = new BinaryMaster();
            binaryMaster.writer.Write(-1);
            SendReturn(binaryMaster.Get());
        }

        public void StocMessage_TeammateSurrender()
        {
            if (surrendered)
                return;
            tagSurrendered = true;
            MessageManager.Cast(InterString.Get("队友发起了投降。"));
        }

        public void StocMessage_TimeLimit(BinaryReader r)
        {
            int player = LocalPlayer(r.ReadByte());
            r.ReadByte();
            int timeLimit = r.ReadInt16();
            TcpHelper.CtosMessage_TimeConfirm();

            if (timerHandler == null)
                return;
            timerHandler.time = timeLimit;
            timerHandler.player = player;
        }

        public void StocMessage_Error(string error)
        {
            StartCoroutine(ShowErrorMessageAsync(error));
        }
        
        private IEnumerator ShowErrorMessageAsync(string error)
        {
            while (servantUI == null)
                yield return null;
            GetUI<OcgCoreUI>().DuelErrorLog.Show(error);
        }

        private void SetPlayableGuide(bool me)
        {
            if (playableGuide == null) return;

            playableGuide.Set(me);
        }

        private bool GetMessageConfig(int player)
        {
            if (player < 4 || player == 7)
            {
                if (condition == Condition.Duel && !Config.GetBool("DuelPlayerMessage", true))
                    return false;
                if (condition == Condition.Watch && !Config.GetBool("WatchPlayerMessage", true))
                    return false;
                if (condition == Condition.Replay && !Config.GetBool("ReplayPlayerMessage", true))
                    return false;
            }
            else
            {
                if (condition == Condition.Duel && !Config.GetBool("DuelSystemMessage", true))
                    return false;
                if (condition == Condition.Watch && !Config.GetBool("WatchSystemMessage", true))
                    return false;
                if (condition == Condition.Replay && !Config.GetBool("ReplaySystemMessage", true))
                    return false;
            }
            return true;
        }

        public void ForceMSquit()
        {
            var p = new Package
            {
                Function = (int)GameMessage.sibyl_quit
            };
            packages.Add(p);
        }

        public bool InIgnoranceReplay()
        {
            return condition != Condition.Duel;
        }

        public Package GetNextPackage()
        {
            int target = 1;
            while (packages.Count > target)
            {
                if (packages[target].Function != (int)GameMessage.UpdateData
                    && packages[target].Function != (int)GameMessage.UpdateCard)
                    return packages[target];
                target++;
            }
            return null;
        }

        public static int movingToGrave = 0;
        public static int movingToExclude = 0;

        public bool NextMessageIsMovingTo(CardLocation location, uint player)
        {
            var p = GetNextPackage();
            if (p == null)
                return false;

            if (p.Function == (int)GameMessage.Move)
            {
                var r = p.Data.reader;
                r.BaseStream.Seek(0, 0);
                r.ReadInt32();
                r.ReadGPS();
                var to = r.ReadGPS();
                if (player == to.controller && (to.location & (uint)location) > 0)
                    return true;
            }
            return false;
        }

        public bool NextMessageIsMovingFrom(CardLocation location)
        {
            var p = GetNextPackage();
            if (p == null)
                return false;

            if (p.Function == (int)GameMessage.Move)
            {
                var r = p.Data.reader;
                r.BaseStream.Seek(0, 0);
                r.ReadInt32();
                var from = r.ReadGPS();
                if ((from.location & (uint)location) > 0)
                    return true;
            }
            return false;
        }

        public bool NextMessageIsMovingToGrave(uint player)
        {
            if (movingToGrave >= 5)
                return false;
            if (NextMessageIsMovingTo(CardLocation.Grave, player))
                return true;
            return false;
        }
        public bool NextMessageIsMovingToExclude(uint player)
        {
            if (movingToExclude >= 5)
                return false;
            if (NextMessageIsMovingTo(CardLocation.Removed, player))
                return true;
            return false;
        }
        private void Sibyl()
        {
            try
            {
                var messageIsHandled = false;
                GetConfirmedCard();

                while (!pause && messagePass && !speaking && !speakBreaking && !waitForNoWaitingVoice)
                {
                    if (packages.Count == 0) break;
                    var currentPackage = packages[0];
                    currentMessage = (GameMessage)currentPackage.Function;

                    if (IfMessageImportant(currentPackage))
                        if (Program.TimePassed() < MessageBeginTime)
                            break;

                    messageIsHandled = true;

                    try
                    {
                        VoiceMessage(packages[0]);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }

                    lastMessage = currentMessage;
                    packages.RemoveAt(0);
                }
                if (messageIsHandled)
                    if (condition == Condition.Replay)
                        if (packages.Count == 0)
                            MessageManager.Cast(InterString.Get("回放播放结束。"));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        #endregion

        #region PracticalizeTools

        public int LocalPlayer(int p)
        {
            if (p == 0 || p == 1)
            {
                if (isFirst)
                    return p;
                return 1 - p;
            }
            return p;
        }
        public static int[] GetSelectLevelSum(List<GameCard> cards)
        {
            var sum1 = 0;
            foreach (var card in cards)
                sum1 += card.levelForSelect_1;
            var sum2 = 0;
            foreach (var card in cards)
                sum2 += card.levelForSelect_2;
            return new int[] { sum1, sum2 };
        }

        public static bool CheckSelectableInSum(List<GameCard> cards, GameCard card, List<GameCard> selectedCards, int max)
        {
            if (selectedCards.Count >= max)
                return false;
            bool returnValue = false;
            var sum = GetSelectLevelSum(selectedCards);
            if (sum[0] + card.levelForSelect_1 == Program.instance.ocgcore.ES_level || sum[1] + card.levelForSelect_2 == Program.instance.ocgcore.ES_level)
                return true;
            if (sum[0] + card.levelForSelect_1 > Program.instance.ocgcore.ES_level || sum[1] + card.levelForSelect_2 > Program.instance.ocgcore.ES_level)
                return false;

            var newSelectedCards = new List<GameCard>(selectedCards) { card };
            foreach (var c in cards)
                if (!newSelectedCards.Contains(c))
                {
                    returnValue = CheckSelectableInSum(cards, c, newSelectedCards, max);
                    if (returnValue)
                        return true;
                }
            return returnValue;
        }

        public static bool TypeMatchReason(int type, int reason)
        {
            if ((type & (uint)CardType.Ritual) > 0 && (reason & (uint)CardReason.Ritual) > 0)
                return true;
            if ((type & (uint)CardType.Fusion) > 0 && (reason & (uint)CardReason.Fusion) > 0)
                return true;
            if ((type & (uint)CardType.Synchro) > 0 && (reason & (uint)CardReason.Synchro) > 0)
                return true;
            if ((type & (uint)CardType.Xyz) > 0 && (reason & (uint)CardReason.Xyz) > 0)
                return true;
            if ((type & (uint)CardType.Link) > 0 && (reason & (uint)CardReason.Link) > 0)
                return true;
            return false;
        }

        public GameCard GCS_Create(GPS p, bool temp = false)
        {
            GameCard c = null;
            for (var i = 0; i < cards.Count; i++)
                if (cards[i].md5 == md5Maker)
                {
                    c = cards[i];
                    c.p = p;
                }

            if (c == null)
            {
                c = Program.instance.container_3D.gameObject.AddComponent<GameCard>();
                c.p = p;
                c.md5 = md5Maker;
                cards.Add(c);

                if (temp)
                    tempCards.Add(c);
            }

            md5Maker++;
            return c;
        }

        public GameCard GCS_Get(GPS p)
        {
            GameCard c = null;
            if ((p.location & (uint)CardLocation.Overlay) > 0)
            {
                for (var i = 0; i < cards.Count; i++)
                    if (cards[i].p.location == p.location)
                        if (cards[i].p.controller == p.controller)
                            if (cards[i].p.sequence == p.sequence)
                                if (cards[i].p.position == p.position)
                                {
                                    c = cards[i];
                                    break;
                                }
            }
            else
            {
                for (var i = 0; i < cards.Count; i++)
                    if (cards[i].p.location == p.location)
                        if (cards[i].p.controller == p.controller)
                            if (cards[i].p.sequence == p.sequence)
                            {
                                c = cards[i];
                                break;
                            }
            }

            if (p.location == 0) c = null;
            return c;
        }
        public List<GameCard> GCS_GetLocationCards(int controller, int location)
        {
            var cardsInLocation = new List<GameCard>();
            for (var i = 0; i < cards.Count; i++)
                if (!tempCards.Contains(cards[i]))
                    if (cards[i].p.location == location)
                        if (cards[i].p.controller == controller)
                            cardsInLocation.Add(cards[i]);
            return cardsInLocation;
        }
        public List<GameCard> GCS_GetOverlays(GameCard c)
        {
            var overlays = new List<GameCard>();
            if (c != null)
                if ((c.p.location & (uint)CardLocation.Overlay) == 0)
                    for (var i = 0; i < cards.Count; i++)
                        if ((cards[i].p.location & (uint)CardLocation.Overlay) > 0)
                            if (cards[i].p.controller == c.p.controller)
                                if ((cards[i].p.location | (uint)CardLocation.Overlay) ==
                                    (c.p.location | (uint)CardLocation.Overlay))
                                    if (cards[i].p.sequence == c.p.sequence)
                                        overlays.Add(cards[i]);
            return overlays;
        }
        private void GCS_CreateBundle(int count, int controller, CardLocation location)
        {
            for (var i = 0; i < count; i++)
            {
                GCS_Create(new GPS
                {
                    controller = (uint)controller,
                    location = (uint)location,
                    position = (int)CardPosition.FaceDownAttack,
                    sequence = (uint)i
                });
            }
        }
        private List<GameCard> GCS_ResizeBundle(int count, int player, CardLocation location)
        {
            var cardBow = new List<GameCard>();
            var waterOutOfBow = new List<GameCard>();
            for (var i = 0; i < cards.Count; i++)
                if ((cards[i].p.location & (uint)location) > 0)
                    if (cards[i].p.controller == player)
                    {
                        if (cardBow.Count < count)
                            cardBow.Add(cards[i]);
                        else
                            waterOutOfBow.Add(cards[i]);
                    }
            foreach (var card in waterOutOfBow)
            {
                cards.Remove(card);
                if ((card.p.location & (uint)CardLocation.Hand) > 0)
                    card.AnimationShuffle(0.15f);
                else
                    card.Dispose();
            }
            while (cardBow.Count < count)
            {
                var card = GCS_Create(new GPS
                {
                    controller = (uint)player,
                    location = (uint)location,
                    position = (int)CardPosition.FaceDownAttack,
                    sequence = (uint)cardBow.Count
                });
                cardBow.Add(card);
            }
            foreach (var card in cardBow)
            {
                card.EraseData();
                card.p.position = (int)CardPosition.FaceDownAttack;
            }
            return cardBow;
        }
        public void ArrangeCards()
        {
            //sort 
            cards.Sort((left, right) =>
            {
                var a = 1;
                if (left.p.controller > right.p.controller)
                {
                    a = 1;
                }
                else if (left.p.controller < right.p.controller)
                {
                    a = -1;
                }
                else
                {
                    if (left.p.location == (uint)CardLocation.Hand && right.p.location != (uint)CardLocation.Hand)
                    {
                        a = -1;
                    }
                    else if (left.p.location != (uint)CardLocation.Hand && right.p.location == (uint)CardLocation.Hand)
                    {
                        a = 1;
                    }
                    else
                    {
                        if ((left.p.location | (uint)CardLocation.Overlay) >
                            (right.p.location | (uint)CardLocation.Overlay))
                        {
                            a = -1;
                        }
                        else if ((left.p.location | (uint)CardLocation.Overlay) <
                                 (right.p.location | (uint)CardLocation.Overlay))
                        {
                            a = 1;
                        }
                        else
                        {
                            if (left.p.sequence > right.p.sequence)
                            {
                                a = 1;
                            }
                            else if (left.p.sequence < right.p.sequence)
                            {
                                a = -1;
                            }
                            else
                            {
                                if ((left.p.location & (uint)CardLocation.Overlay) >
                                    (right.p.location & (uint)CardLocation.Overlay))
                                {
                                    a = -1;
                                }
                                else if ((left.p.location & (uint)CardLocation.Overlay) <
                                         (right.p.location & (uint)CardLocation.Overlay))
                                {
                                    a = 1;
                                }
                                else
                                {
                                    if (left.p.position > right.p.position)
                                        a = 1;
                                    else if (left.p.position < right.p.position) a = -1;
                                }
                            }
                        }
                    }
                }
                return a;
            });

            /////rebuild
            uint preController = 9999;
            uint preLocation = 9999;
            uint preSequence = 9999;

            uint sequenceWriter = 0;
            var positionWriter = 0;

            for (var i = 0; i < cards.Count; i++)
                if (cards[i])
                {
                    if (preController != cards[i].p.controller) sequenceWriter = 0;
                    if ((preLocation | (uint)CardLocation.Overlay) != (cards[i].p.location | (uint)CardLocation.Overlay))
                        sequenceWriter = 0;
                    if (preSequence != cards[i].p.sequence) positionWriter = 0;

                    if ((cards[i].p.location & (uint)CardLocation.MonsterZone) == 0)
                        if ((cards[i].p.location & (uint)CardLocation.SpellZone) == 0)
                            cards[i].p.sequence = sequenceWriter;


                    if ((cards[i].p.location & (uint)CardLocation.Overlay) > 0)
                    {
                        cards[i].p.position = positionWriter;
                        positionWriter++;
                    }
                    else
                    {
                        sequenceWriter++;
                    }

                    preController = cards[i].p.controller;
                    preLocation = cards[i].p.location;
                    preSequence = cards[i].p.sequence;
                }
        }

        [HideInInspector] public bool needRefreshHand0 = true;
        [HideInInspector] public bool needRefreshHand1 = true;
        [HideInInspector] public List<GameCard> myHandCards = new();
        [HideInInspector] public List<GameCard> opHandCards = new();
        private List<GameCard> myPreHandCards = new();
        private List<GameCard> opPreHandCards = new();

        public int GetMyHandCount()
        {
            if (needRefreshHand0)
            {
                myHandCards = new List<GameCard>(myPreHandCards);
                foreach (var card in cards)
                    if (card.p.controller == 0)
                        if ((card.p.location & (uint)CardLocation.Hand) > 0)
                            if (!myHandCards.Contains(card))
                                myHandCards.Add(card);
                needRefreshHand0 = false;
            }
            return myHandCards.Count;
        }
        public int GetOpHandCount()
        {
            if (needRefreshHand1)
            {
                opHandCards = new List<GameCard>(opPreHandCards);
                foreach (var card in cards)
                    if (card.p.controller != 0)
                        if ((card.p.location & (uint)CardLocation.Hand) > 0)
                            if (!opHandCards.Contains(card))
                                opHandCards.Add(card);
                needRefreshHand1 = false;
            }
            return opHandCards.Count;
        }

        public int GetLocationCardCount(CardLocation location, uint controller)
        {
            int count = 0;
            foreach (var card in cards)
                if ((card.p.location & (uint)location) > 0 && card.p.controller == controller)
                    count++;
            return count;
        }
        public Package GetNamePacket()
        {
            var p__ = new Package
            {
                Function = (int)GameMessage.sibyl_name,
                Data = new BinaryMaster()
            };
            p__.Data.writer.WriteUnicode(name_0, 50);
            p__.Data.writer.WriteUnicode(name_0_tag, 50);
            p__.Data.writer.WriteUnicode(name_0_c != "" ? name_0_c : name_0, 50);
            p__.Data.writer.WriteUnicode(name_1, 50);
            p__.Data.writer.WriteUnicode(name_1_tag, 50);
            p__.Data.writer.WriteUnicode(name_1_c != "" ? name_1_c : name_1, 50);
            p__.Data.writer.Write(Program.instance.ocgcore.MasterRule);
            return p__;
        }

        public bool GetAutoInfo()
        {
            if (condition == Condition.Duel
                && Config.Get("DuelAutoInfo", "0") == "0")
                return false;
            if (condition == Condition.Watch
                && Config.Get("WatchAutoInfo", "0") == "0")
                return false;
            if (condition == Condition.Replay
                && Config.Get("ReplayAutoInfo", "0") == "0")
                return false;

            return true;
        }

        public void RefreshAllCardsLabel()
        {
            if (!showing)
                return;
            foreach (var card in cards)
            {
                card.RefreshLabel();
            }
        }

        bool CurrentChainDisabled(int currentChain)
        {
            for (int i = 0; i < packages.Count; i++)
            {
                if ((GameMessage)packages[i].Function == GameMessage.ChainDisabled)
                {
                    var r = packages[i].Data.reader;
                    r.BaseStream.Seek(0, 0);
                    if (r.ReadByte() == currentChain)
                        return true;
                }
                if ((GameMessage)packages[i].Function == GameMessage.ChainSolved)
                    return false;
            }
            return false;
        }

        private bool IfMessageImportant(Package package)
        {
            var r = package.Data.reader;
            r.BaseStream.Seek(0, 0);
            var msg = (GameMessage)packages[0].Function;
            switch (msg)
            {
                case GameMessage.Start:
                case GameMessage.Win:
                case GameMessage.ConfirmDecktop:
                case GameMessage.ConfirmCards:
                case GameMessage.ShuffleDeck:
                case GameMessage.ShuffleHand:
                case GameMessage.SwapGraveDeck:
                case GameMessage.ShuffleSetCard:
                case GameMessage.ReverseDeck:
                case GameMessage.DeckTop:
                case GameMessage.NewTurn:
                case GameMessage.NewPhase:
                case GameMessage.Move:
                case GameMessage.PosChange:
                case GameMessage.Swap:
                case GameMessage.RandomSelected:
                case GameMessage.BecomeTarget:
                case GameMessage.Draw:
                case GameMessage.Recover:
                case GameMessage.PayLpCost:
                case GameMessage.TossCoin:
                case GameMessage.TossDice:
                case GameMessage.TagSwap:
                case GameMessage.ReloadField:
                case GameMessage.FlipSummoning:
                case GameMessage.Summoning:
                case GameMessage.SpSummoning:
                case GameMessage.Chaining:
                case GameMessage.Chained:
                case GameMessage.ChainSolving:
                case GameMessage.ChainSolved:
                case GameMessage.ChainEnd:
                case GameMessage.ChainNegated:
                case GameMessage.ChainDisabled:
                    return true;
                case GameMessage.Damage:
                    if (needDamageResponseInstant)
                        return false;
                    else
                        return true;
                case GameMessage.Hint:
                    int type = r.ReadChar();
                    if (type == 8) return true;
                    if (type == 10) return true;
                    return false;
                case GameMessage.CardHint:
                    r.ReadGPS();
                    int ctype = r.ReadByte();
                    if (ctype == 1) return true;
                    return false;
                case GameMessage.SelectBattleCmd:
                case GameMessage.SelectIdleCmd:
                case GameMessage.SelectEffectYn:
                case GameMessage.SelectYesNo:
                case GameMessage.SelectOption:
                case GameMessage.SelectCard:
                case GameMessage.SelectPosition:
                case GameMessage.SelectTribute:
                case GameMessage.SortChain:
                case GameMessage.SelectCounter:
                case GameMessage.SelectSum:
                case GameMessage.SortCard:
                case GameMessage.AnnounceRace:
                case GameMessage.AnnounceAttrib:
                case GameMessage.AnnounceCard:
                case GameMessage.AnnounceNumber:
                case GameMessage.SelectDisfield:
                case GameMessage.SelectPlace:
                case GameMessage.SelectChain:
                case GameMessage.Attack:
                    return true;
            }
            return false;
        }

        private void ClearResponse()
        {
            var myMaxDeck = GetLocationCardCount(CardLocation.Deck, 0);
            var opMaxDeck = GetLocationCardCount(CardLocation.Deck, 1);
            foreach (var card in cards)
            {
                card.effects.Clear();
                card.ClearButtons();
                if ((card.p.location & (uint)CardLocation.Deck) > 0)
                {
                    if (deckReserved)
                    {
                        if (card.p.controller == 0 && card.p.sequence != myMaxDeck - 1)
                            card.EraseData();
                        if (card.p.controller == 1 && card.p.sequence != opMaxDeck - 1)
                            card.EraseData();
                    }
                    else
                    {
                        card.EraseData();
                    }
                }
            }
            foreach (var place in places)
            {
                place.StopResponse();
                place.HideHint();
                place.ClearButtons();
            }
            foreach (var grave in graves)
            {
                grave.ClearGraveButtons();
                grave.ClearExcludeButtons();
            }

            PhaseButtonHandler.battlePhase = false;
            PhaseButtonHandler.main2Phase = false;
            PhaseButtonHandler.endPhase = false;
            PhaseButtonHandler.CloseHint();

            CloseBgHint();
            FieldSelectReset();
            ES_selectHint = string.Empty;
        }

        private void GetConfirmedCard()
        {
            if (nextMoveAction == null || nextMoveActionTargetRenderer == null)
                return;
            for (int i = 0; i < packages.Count; i++)
            {
                if ((GameMessage)packages[i].Function == GameMessage.ConfirmCards)
                {
                    var r = packages[i].Data.reader;
                    r.BaseStream.Seek(0, 0);
                    r.ReadByte();
                    if (condition != Condition.Replay || CurrentReplayUseYRP2)
                        r.ReadByte();
                    r.ReadByte();

                    int nextConfirmedCard = r.ReadInt32();
                    StartCoroutine(Program.instance.texture_.LoadCardToRendererWithMaterialAsync(nextMoveActionTargetRenderer, nextConfirmedCard, true));
                    nextMoveActionTargetRenderer = null;
                    lastMoveCard.SetCode(nextConfirmedCard);
                }
            }
        }

        #endregion

        #region Practicalize
        private void PracticalizeMessage(Package p)
        {
            currentMessageIndex++;
            var r = p.Data.reader;
            r.BaseStream.Seek(0, 0);
            var player = 0;
            var code = 0;
            var count = 0;
            var min = 0;
            var max = 0;
            var cancelable = false;
            var location = 0;
            var sequence = 0;
            var data = 0;
            var type = 0;
            var desc = "";
            var sleep = 0f;
            uint available;
            GPS gps;
            GameCard card;
            GPS from;
            GPS to;
            int val;
            string name;
            surrendered = false;
            var length_of_message = r.BaseStream.Length;
            BinaryMaster binaryMaster;
            List<string> selections;
            //if ((GameMessage)p.Function != GameMessage.UpdateData)
            //    Debug.Log("----------" + (GameMessage)p.Function);
            //else
            //    Debug.Log("|||||||||||" + (GameMessage)p.Function);
            switch ((GameMessage)p.Function)
            {
                case GameMessage.sibyl_chat:
                    player = r.ReadInt32();
                    if (!GetMessageConfig(player))
                        break;
                    name = string.Empty;
                    if (isTag)
                    {
                        switch (player)
                        {
                            case 0:
                                name = name_0;
                                if (playerType < 7 &&
                                    ((playerType < 2 && !isFirst) || (playerType >= 2 && isFirst)))
                                    name = name_1;
                                break;
                            case 1:
                                name = name_0_tag;
                                if (playerType < 7 &&
                                    ((playerType < 2 && !isFirst) || (playerType >= 2 && isFirst)))
                                    name = name_1_tag;
                                break;
                            case 2:
                                name = name_1;
                                if (playerType < 7 &&
                                    ((playerType < 2 && !isFirst) || (playerType >= 2 && isFirst)))
                                    name = name_0;
                                break;
                            case 3:
                                name = name_1_tag;
                                if (playerType < 7 &&
                                    ((playerType < 2 && !isFirst) || (playerType >= 2 && isFirst)))
                                    name = name_0_tag;
                                break;
                        }
                    }
                    else
                    {
                        switch (player)
                        {
                            case 0:
                                name = name_0;
                                if (playerType < 7 &&
                                    ((playerType == 0 && !isFirst) || (playerType == 1 && isFirst)))
                                    name = name_1;
                                break;
                            case 1:
                                name = name_1;
                                if (playerType < 7 &&
                                    ((playerType == 0 && !isFirst) || (playerType == 1 && isFirst)))
                                    name = name_0;
                                break;
                        }
                    }
                    if (player == 7)
                        name = InterString.Get("观战者");
                    if (name != string.Empty)
                        name += ": ";
                    var content = r.ReadALLUnicode();
                    MessageManager.Cast(name + content);
                    break;
                case GameMessage.sibyl_name:
                    name_0 = r.ReadUnicode(50);
                    name_0_tag = r.ReadUnicode(50);
                    name_0_c = r.ReadUnicode(50);
                    name_1 = r.ReadUnicode(50);
                    name_1_tag = r.ReadUnicode(50);
                    name_1_c = r.ReadUnicode(50);

                    isTag = !(name_0_tag == "---" && name_1_tag == "---" && name_0 == name_0_c && name_1 == name_1_c);

                    if (Config.Get("ReplayPlayerName0", Config.EMPTY_STRING).Length > 0)
                        name_0 = Config.Get("ReplayPlayerName0", Config.EMPTY_STRING);
                    if (Config.Get("ReplayPlayerName1", Config.EMPTY_STRING).Length > 0)
                        name_1 = Config.Get("ReplayPlayerName1", Config.EMPTY_STRING);
                    if (Config.Get("ReplayPlayerName0Tag", Config.EMPTY_STRING).Length > 0)
                        name_0_tag = Config.Get("ReplayPlayerName0Tag", Config.EMPTY_STRING);
                    if (Config.Get("ReplayPlayerName1Tag", Config.EMPTY_STRING).Length > 0)
                        name_1_tag = Config.Get("ReplayPlayerName1Tag", Config.EMPTY_STRING);
                    if (isTag)
                    {
                        if (isFirst)
                        {
                            name_0_c = name_0;
                            name_1_c = name_1_tag;
                        }
                        else
                        {
                            name_0_c = name_0_tag;
                            name_1_c = name_1;
                        }
                    }
                    else
                    {
                        name_0_c = name_0;
                        name_1_c = name_1;
                    }
                    GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0_c;
                    GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1_c;
                    SetFace();
                    if (r.BaseStream.Position < r.BaseStream.Length)
                        MasterRule = r.ReadInt32();
                    else
                        MasterRule = 3;
                    break;
                case GameMessage.sibyl_quit:
                    duelEnded = true;
                    Program.instance.room.duelEnded = true;
                    result = DuelResult.DisLink;
                    break;
                case GameMessage.Retry:
                    MessageManager.Cast("Error!");
                    break;
                case GameMessage.ShowHint:
                    int length = r.ReadUInt16();
                    var buffer = r.ReadToEnd();
                    var n = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    MessageManager.Cast(n);
                    break;
                case GameMessage.AiName:
                    length = r.ReadUInt16();
                    buffer = r.ReadBytes(length + 1);
                    n = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    name_0 = Config.Get("DuelPlayerName0", Config.EMPTY_STRING);
                    name_0_c = name_0;
                    name_1 = n;
                    name_1_c = name_1;
                    GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0_c;
                    GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1_c;
                    isTag = false;
                    SetFace();
                    break;
                case GameMessage.Win:
                    deckReserved = false;
                    cantCheckGrave = false;
                    duelEnded = true;
                    GetUI<OcgCoreUI>().CardDescription.Hide();
                    ClearResponse();

                    if (currentPopup != null)
                    {
                        currentPopup.whenQuitDo = null;
                        currentPopup.Hide();
                        returnAction = null;
                    }
                    player = LocalPlayer(r.ReadByte());
                    int winType = r.ReadByte();
                    keys.Insert(0, currentMessageIndex);
                    AudioManager.StopBGM();
                    GameObject duelText;
                    string endingReason = string.Empty;
                    if (player == 2)
                    {
                        result = DuelResult.Draw;
                        duelText = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelTextDraw", true);
                    }
                    else if (player == 0 || winType == 4)
                    {
                        result = DuelResult.Win;
                        duelText = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelTextWin", true);
                        if (cookie_matchKill > 0)
                        {
                            winReason = CardsManager.Get(cookie_matchKill).Name;
                            endingReason = InterString.Get("比赛胜利，卡片：[?]", winReason);
                        }
                        else
                        {
                            winReason = StringHelper.Get("victory", winType);
                            endingReason = InterString.Get("游戏胜利，原因：[?]", winReason);
                        }
                    }
                    else
                    {
                        result = DuelResult.Lose;
                        duelText = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelTextLose", true);
                        if (cookie_matchKill > 0)
                        {
                            winReason = CardsManager.Get(cookie_matchKill).Name;
                            endingReason = InterString.Get("比赛败北，卡片：[?]", winReason);
                        }
                        else
                        {
                            winReason = StringHelper.Get("victory", winType);
                            endingReason = InterString.Get("游戏败北，原因：[?]", winReason);
                        }
                    }
                    allGameObjects.Add(duelText);
                    if (timerHandler != null)
                        timerHandler.DuelEnd();
                    if (playableGuide != null)
                    {
                        playableGuide.End();
                    }
                    //防止对方在更换副卡组时拔螺丝
                    UIManager.UIBlackOut(TransitionTime);
                    GetUI<OcgCoreUI>().CG.blocksRaycasts = true;
                    GetUI<OcgCoreUI>().CG.alpha = 1f;
                    GetUI<OcgCoreUI>().Buttons.SetActive(true);

                    duelText.SetActive(false);
                    endingAction = () =>
                    {
                        duelText.SetActive(true);
                        var mono = duelText.AddComponent<DoWhenPlayableDirectorStop>();
                        mono.action = () =>
                        {
                            PrintDuelLog(endingReason);
                            if (condition != Condition.Replay)
                            {
                                GetUI<OcgCoreUI>().ShowSaveReplay();
                                Destroy(mono.gameObject);
                            }
                        };
                        if (result == DuelResult.Win)
                        {
                            bgPhase1 = 4;
                            var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE4_R";
                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd, seLabel);

                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndWin);
                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndLose);

                            if (stand1Manager != null)
                                stand1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                            if (mate0 != null)
                                mate0.Play(Mate.MateAction.Victory);
                            if (mate1 != null)
                                mate1.Play(Mate.MateAction.Defeat);
                        }
                        else if (result == DuelResult.Lose)
                        {
                            bgPhase0 = 4;
                            var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE4_P";
                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd, seLabel);

                            field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndWin);
                            field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndLose);

                            if (stand0Manager != null)
                                stand0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                            if (mate0 != null)
                                mate0.Play(Mate.MateAction.Defeat);
                            if (mate1 != null)
                                mate1.Play(Mate.MateAction.Victory);
                        }
                    };
                    if (cookie_matchKill > 0)
                    {
                        PlayCommonSpecialWin(new int[] { cookie_matchKill });
                    }
                    else if (winType >= 0x10)
                    {
                        if (winType == 0x10)//被封印的艾克佐迪亚
                        {
                            ElementObjectManager mner = PlaySpecialWin("33396948");
                            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard"), 33396948, 0, true));
                            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard2"), 7902349, 0, true));
                            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard3"), 70903634, 0, true));
                            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard4"), 44519536, 0, true));
                            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard5"), 8124921, 0, true));
                        }
                        else if (winType == 0x11)//终焉的倒计时
                            PlayCommonSpecialWin(new int[] { 95308449 });
                        else if (winType == 0x12)//毒蛇神 维诺米纳迦
                            PlayCommonSpecialWin(new int[] { 8062132 });
                        else if (winType == 0x13)//光之创造神 哈拉克提
                            PlayCommonSpecialWin(new int[] { 10000040 });
                        else if (winType == 0x14)//究极封印神 艾克佐迪奥斯
                            PlayCommonSpecialWin(new int[] { 13893596 });
                        else if (winType == 0x15)//通灵盘
                            PlaySpecialWin("40771118");
                        else if (winType == 0x16)//最终一战！
                            PlayCommonSpecialWin(new int[] { 28566710 });
                        else if (winType == 0x17)//No.88 机关傀儡-命运狮子
                            PlayCommonSpecialWin(new int[] { 48995978 });
                        else if (winType == 0x18)//混沌No.88 机关傀儡-灾厄狮子
                            PlayCommonSpecialWin(new int[] { 6165656 });
                        else if (winType == 0x19)//头奖壶7
                            PlayCommonSpecialWin(new int[] { 81171949, 81171949, 81171949 });
                        else if (winType == 0x1A)//魂之接力
                            PlayCommonSpecialWin(new int[] { 42776960 });
                        else if (winType == 0x1B)//鬼计惰天使
                            PlaySpecialWin("53334641");
                        else if (winType == 0x1C)//幻煌龙的天涡
                            PlayCommonSpecialWin(new int[] { 97795930 });
                        else if (winType == 0x1D)//方程式运动员胜利团队
                            PlayCommonSpecialWin(new int[] { 69553552 });
                        else if (winType == 0x1E)//飞行象
                            PlayCommonSpecialWin(new int[] { 66765023 });
                        else if (winType == 0x1F)//守护神 艾克佐迪亚
                            PlayCommonSpecialWin(new int[] { 5008836 });
                        else if (winType == 0x20)//真艾克佐迪亚
                            PlayCommonSpecialWin(new int[] { 37984331 });
                        else if (winType == 0x21)//混沌虚数No.1000 梦幻虚光神 原数天灵·原数天地
                            PlayCommonSpecialWin(new int[] { 15862758 });
                        else if (winType == 0x22)//席取-六双丸
                            PlaySpecialWin("96637156");
                        else if (winType == 0x23)//火器的祝台
                            PlayCommonSpecialWin(new int[] { 77751766 });
                        else
                        {
                            MessageManager.Cast(InterString.Get("请联系开发者修复这张特殊胜利的卡。"));
                            endingAction.Invoke();
                        }
                    }
                    else
                        endingAction.Invoke();
                    break;
                case GameMessage.Start:
                    CoreReset();
                    BackgroundFieldInitialize();
                    md5Maker = 0;
                    messagePass = false;
                    playerType = r.ReadByte();
                    isFirst = (playerType & 0xF) == 0;
                    RoomServant.CoreShowing = 2;
                    isObserver = (playerType & 0xF0) > 0;
                    if (r.BaseStream.Length > 17)
                        MasterRule = r.ReadByte();
                    life0 = r.ReadInt32();
                    life1 = r.ReadInt32();
                    lpLimit = life0;
                    GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0;
                    GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1;
                    if (RoomServant.Mode == 2)
                    {
                        if (isFirst)
                            GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1_tag;
                        else
                            GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0_tag;
                        isTag = true;
                    }
                    else
                        isTag = false;
                    SetFace();
                    if (preload)
                        break;
                    GCS_CreateBundle(r.ReadInt16(), LocalPlayer(0), CardLocation.Deck);
                    GCS_CreateBundle(r.ReadInt16(), LocalPlayer(0), CardLocation.Extra);
                    GCS_CreateBundle(r.ReadInt16(), LocalPlayer(1), CardLocation.Deck);
                    GCS_CreateBundle(r.ReadInt16(), LocalPlayer(1), CardLocation.Extra);
                    ArrangeCards();
                    RefreshBgState();
                    SetLP(0, 0, true);
                    DOTween.To(v => { }, 0, 0, TransitionTime).OnComplete(() =>
                    {
                        myDeck.gameObject.SetActive(true);
                        myExtra.gameObject.SetActive(true);
                        opDeck.gameObject.SetActive(true);
                        opExtra.gameObject.SetActive(true);
                        var mono = myDeck.gameObject.AddComponent<DoWhenPlayableDirectorStop>();
                        mono.action = () =>
                        {
                            var effect = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelTextStart", true);
                            var mono = effect.AddComponent<DoWhenPlayableDirectorStop>();
                            mono.action = () =>
                            {
                                Destroy(effect);
                                GetUI<OcgCoreUI>().CG.alpha = 1;
                                GetUI<OcgCoreUI>().CG.blocksRaycasts = true;
                                GetUI<OcgCoreUI>().Buttons.SetActive(true);
                                messagePass = true;
                                AudioManager.PlayBgmNormal(Config.GetBool("BGMbyMySide", true) ? field0.name : field1.name);
                            };
                        };
                    });
                    break;
                case GameMessage.ReloadField:
                    CoreReset();

                    if (inAi)
                        myTurn = true;
                    PhaseButtonHandler.TurnChange(myTurn, 1);

                    MasterRule = r.ReadByte() + 1;
                    if (MasterRule > 255) MasterRule -= 255;

                    keys.Insert(0, currentMessageIndex);

                    md5Maker = 0;

                    for (var p_ = 0; p_ < 2; p_++)
                    {
                        player = LocalPlayer(p_);
                        if (player == 0)
                            life0 = r.ReadInt32();
                        else
                            life1 = r.ReadInt32();
                        for (int i = 0; i < 7; i++)
                        {
                            val = r.ReadByte();
                            if (val > 0)
                            {
                                gps = new GPS
                                {
                                    controller = (uint)player,
                                    location = (uint)CardLocation.MonsterZone,
                                    position = r.ReadByte(),
                                    sequence = (uint)i
                                };
                                GCS_Create(gps);
                                val = r.ReadByte();
                                for (var xyz = 0; xyz < val; ++xyz)
                                {
                                    var overlay = new GPS
                                    {
                                        controller = gps.controller,
                                        location = (uint)CardLocation.MonsterZone | (uint)CardLocation.Overlay,
                                        position = xyz,
                                        sequence = gps.sequence
                                    };
                                    GCS_Create(overlay);
                                }
                            }
                        }
                        for (var i = 0; i < 8; i++)
                        {
                            val = r.ReadByte();
                            if (val > 0)
                            {
                                gps = new GPS
                                {
                                    controller = (uint)player,
                                    location = (uint)CardLocation.SpellZone,
                                    position = r.ReadByte(),
                                    sequence = (uint)i
                                };
                                GCS_Create(gps);
                            }
                        }
                        val = r.ReadByte();
                        for (var i = 0; i < val; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Deck,
                                position = (int)CardPosition.FaceDownAttack,
                                sequence = (uint)i
                            };
                            GCS_Create(gps);
                        }
                        val = r.ReadByte();
                        for (var i = 0; i < val; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Hand,
                                position = (int)CardPosition.FaceDownAttack,
                                sequence = (uint)i
                            };
                            GCS_Create(gps);
                        }
                        val = r.ReadByte();
                        for (var i = 0; i < val; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Grave,
                                position = (int)CardPosition.FaceUpAttack,
                                sequence = (uint)i
                            };
                            GCS_Create(gps);
                        }
                        val = r.ReadByte();
                        for (var i = 0; i < val; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Removed,
                                position = (int)CardPosition.FaceUpAttack,
                                sequence = (uint)i
                            };
                            GCS_Create(gps);
                        }
                        val = r.ReadByte();
                        int val_up = r.ReadByte();
                        for (var i = 0; i < val - val_up; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Extra,
                                position = (int)CardPosition.FaceDownAttack,
                                sequence = (uint)i
                            };
                            GCS_Create(gps);
                        }
                        for (var i = 0; i < val_up; i++)
                        {
                            gps = new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Extra,
                                position = (int)CardPosition.FaceUpAttack,
                                sequence = (uint)(val + i)
                            };
                            GCS_Create(gps);
                        }
                    }
                    UpdateBgEffect(0, true);
                    UpdateBgEffect(1, true);
                    SetLP(0, 0, true);
                    ArrangeCards();
                    RefreshBgState();
                    myDeck.gameObject.SetActive(true);
                    myExtra.gameObject.SetActive(true);
                    opDeck.gameObject.SetActive(true);
                    opExtra.gameObject.SetActive(true);

                    var mono = myDeck.gameObject.AddComponent<DoWhenPlayableDirectorStop>();
                    mono.action = () =>
                    {
                        GetUI<OcgCoreUI>().CG.alpha = 1;
                        GetUI<OcgCoreUI>().CG.blocksRaycasts = true;
                        GetUI<OcgCoreUI>().Buttons.SetActive(true);
                        AudioManager.PlayBgmNormal(Config.GetBool("BGMbyMySide", true) ? field0.name : field1.name);
                    };

                    for (var i = 0; i < cards.Count; i++)
                        cards[i].Move(cards[i].p, true);

                    Sleep((int)(TransitionTime * 100 + 10));
                    break;
                case GameMessage.UpdateData:
                    player = LocalPlayer(r.ReadChar());
                    location = r.ReadChar();
                    try
                    {
                        while (true)
                        {
                            var len = r.ReadInt32();
                            if (len == 4) continue;
                            var pos = r.BaseStream.Position;
                            r.ReadCardData();
                            r.BaseStream.Position = pos + len - 4;
                        }
                    }
                    catch { }
                    myPreHandCards.Clear();
                    opPreHandCards.Clear();
                    RefreshHandCardPosition();
                    RefreshBgState();
                    break;
                case GameMessage.UpdateCard:
                    gps = r.ReadShortGPS();
                    var cardToRefresh = GCS_Get(gps);
                    r.ReadUInt32();
                    r.ReadCardData(cardToRefresh);
                    break;
                case GameMessage.Move:
                    keys.Insert(0, currentMessageIndex);
                    code = r.ReadInt32();
                    from = r.ReadGPS();
                    to = r.ReadGPS();
                    uint reason = r.ReadUInt32();

                    card = GCS_Get(from);
                    if (card != null)
                    {
                        card.CacheData();
                    }
                    else
                    {
                        Debug.LogFormat("GCS_Get: not found, location: {0:X}, sequence: {1:X}, position: {2:X}", from.location, from.sequence, from.position);
                        card = GCS_Create(from);
                    }
                    card.SetCode(code);
                    to.reason = reason;
                    if (Settings.Data.BatchMove)
                        Sleep((int)(card.Move(to) * 100));
                    else
                        Sleep((int)(card.Move_Backup(to) * 100));
                    break;
                case GameMessage.PosChange:
                    ES_hint = StringHelper.GetUnsafe(1600);//卡片改变了表示形式
                    code = r.ReadInt32();
                    from = r.ReadGPS();
                    to = from;
                    to.position = r.ReadByte();
                    card = GCS_Get(from);
                    if (card != null)
                    {
                        card.ShowFaceDownCardOrNot(false);
                        card.SetCode(code);
                        sleep = card.Move(to);
                        var delay = sleep;
                        if ((to.position & (uint)CardPosition.FaceUp) > 0
                            && (to.location & (uint)CardLocation.MonsterZone) > 0)
                        {
                            card.AnimationPositon(delay);
                            sleep = 0.3f;
                        }
                        Sleep((int)(sleep * 100));
                    }
                    break;
                case GameMessage.Set:
                    ES_hint = StringHelper.GetUnsafe(1601);//盖放了卡片
                    var effect = ABLoader.LoadFromFile("MasterDuel/Effects/Summon/fxp_som_mgctrpfld_001", true);
                    effect.transform.position = lastMoveCard.model.transform.position;
                    Destroy(effect, 3f);
                    AudioManager.PlaySE("SE_LAND_MT_SET");
                    break;
                case GameMessage.Swap:
                    ES_hint = StringHelper.GetUnsafe(1602);//卡的控制权改变了
                    code = r.ReadInt32();
                    from = r.ReadGPS();
                    code = r.ReadInt32();
                    to = r.ReadGPS();
                    var from2 = new GPS
                    {
                        controller = from.controller,
                        location = from.location,
                        sequence = from.sequence,
                        position = to.position
                    };
                    var to2 = new GPS
                    {
                        controller = to.controller,
                        location = to.location,
                        sequence = to.sequence,
                        position = from.position
                    };
                    card = GCS_Get(from);
                    var card_2 = GCS_Get(to);
                    if (card != null)
                        Sleep((int)(card.Move(to2) * 100));
                    if (card_2 != null)
                        Sleep((int)(card_2.Move(from2) * 100));
                    break;
                case GameMessage.Summoning:
                    cardsInSelection.Clear();
                    code = r.ReadInt32();
                    gps = r.ReadGPS();
                    card = GCS_Get(gps);
                    if (gps.controller == 0)
                        mySummonCount++;
                    else
                        opSummonCount++;
                    effect = ABLoader.LoadFromFile("MasterDuel/Effects/Summon/fxp_somldg/Hand/fxp_somldg_hand_001", true);
                    effect.transform.localPosition = GameCard.GetCardPosition(gps);
                    if ((gps.position & (uint)CardPosition.Attack) > 0)
                        Destroy(effect.transform.GetChild(1).gameObject);
                    else
                        Destroy(effect.transform.GetChild(0).gameObject);
                    Destroy(effect, 10);
                    string se = "";
                    string tail = "";

                    se = "SE_LAND_NORMAL";
                    if (card != null)
                    {
                        card.SetCode(code);
                        card.AddStringTail(InterString.Get("通常召唤登场"));
                        card.AnimationPositon();
                        ES_hint = InterString.Get("「[?]」通常召唤宣言时", card.GetData().Name);
                        if (card.GetData().Level > 6)
                        {
                            effect = ABLoader.LoadFromFolder("MasterDuel/Effects/Summon/fxp_somldg/Advance_s2", "Advance_s2", true);
                            effect.transform.localPosition = GameCard.GetCardPosition(gps);
                            Destroy(effect, 10);
                            se = "SE_LAND_ADVANCE_HIGH";
                            CameraManager.ShakeCamera(true);
                        }
                        else if (card.GetData().Level > 4)
                        {
                            effect = ABLoader.LoadFromFolder("MasterDuel/Effects/Summon/fxp_somldg/Advance_s1", "Advance_s1", true);
                            effect.transform.localPosition = GameCard.GetCardPosition(gps);
                            Destroy(effect, 10);
                            se = "SE_LAND_ADVANCE_MIDDLE";
                            CameraManager.ShakeCamera();
                        }
                        if (GetAutoInfo())
                            GetUI<OcgCoreUI>().CardDescription.Show(card, card.GetMaterial());
                    }
                    AudioManager.PlaySE(se);
                    foreach (var c in cards)
                        c.AnimationLandShake(card, card.GetData().Level > 6);
                    materialCards.Clear();
                    Sleep(100);
                    break;
                case GameMessage.Summoned:
                    ES_hint = StringHelper.GetUnsafe(1604);//怪兽召唤成功
                    break;
                case GameMessage.SpSummoning:
                    cardsInSelection.Clear();
                    code = r.ReadInt32();
                    gps = r.ReadGPS();
                    card = GCS_Get(gps);
                    if (gps.controller == 0)
                        mySpSummonCount++;
                    else
                        opSpSummonCount++;
                    if (card.GetData().HasType(CardType.Token))
                        goto TokenPasss;

                    effect = ABLoader.LoadFromFile("MasterDuel/Effects/Summon/fxp_somldg/Hand/fxp_somldg_hand_001", true);
                    effect.transform.localPosition = GameCard.GetCardPosition(gps);
                    if ((gps.position & (uint)CardPosition.Attack) > 0)
                        Destroy(effect.transform.GetChild(1).gameObject);
                    else
                        Destroy(effect.transform.GetChild(0).gameObject);
                    Destroy(effect, 10);
                    se = "SE_LAND_NORMAL";
                    tail = "";
                    if (card != null)
                    {
                        card.SetCode(code);
                        card.AnimationPositon();
                        ES_hint = InterString.Get("「[?]」特殊召唤宣言时", card.GetData().Name);

                        if (materialCards.Count > 0
                            //&& (card.GetData().Reason & (uint)CardReason.Link) > 0)
                            && card.GetData().HasType(CardType.Link))
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Link_s1";
                            se = "SE_LAND_LINK_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Link;
                        }
                        else if (materialCards.Count > 0
                            //&& (card.GetData().Reason & (uint)CardReason.Fusion) > 0)
                            && card.GetData().HasType(CardType.Fusion))
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Fusion_s1";
                            se = "SE_LAND_FUSION_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Fusion;
                        }
                        else if (materialCards.Count > 0
                            //&& (card.GetData().Reason & (uint)CardReason.Synchro) > 0)
                            && card.GetData().HasType(CardType.Synchro))
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Synchro_s1";
                            se = "SE_LAND_SYNCHRO_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Synchro;
                        }
                        else if (materialCards.Count > 0
                            //&& (card.GetData().Reason & (uint)CardReason.Xyz) > 0)
                            && card.GetData().HasType(CardType.Xyz))
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Xyz_s1";
                            se = "SE_LAND_XYZ_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Xyz;
                        }
                        else if (materialCards.Count > 0
                            //&& (card.GetData().Reason & (uint)CardReason.Ritual) > 0)
                            && card.GetData().HasType(CardType.Ritual))
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Ritual_s1";
                            se = "SE_LAND_RITUAL_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Ritual;
                        }
                        else if (GetUI<OcgCoreUI>().DuelLog.psum)
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Pendulum_s1";
                            se = "SE_LAND_PENDULUM_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = (uint)CardReason.Pendulum;
                        }
                        else
                        {
                            tail = "MasterDuel/Effects/Summon/fxp_somldg/Special_s1";
                            se = "SE_LAND_ADVANCE_MIDDLE";
                            GetUI<OcgCoreUI>().DuelLog.lastSpSummonReason = 0;
                        }
                        if (GameCard.NeedStrongSummon(card.GetData()))
                        {
                            tail = tail.Replace("_s1", "_s2");
                            se = se.Replace("_MIDDLE", "_HIGH");
                        }

                        if (!string.IsNullOrEmpty(tail))
                        {
                            effect = ABLoader.LoadFromFolder(tail, tail, true);
                            CameraManager.Overlay3DReset();
                            effect.transform.localPosition = GameCard.GetCardPosition(gps);
                            if ((gps.position & (uint)CardPosition.Defence) > 0)
                                effect.transform.localEulerAngles = new Vector3(0, 90, 0);
                            Destroy(effect, 10);
                        }
                        AudioManager.PlaySE(se);
                        if (se.EndsWith("HIGH"))
                            CameraManager.ShakeCamera(true);
                        else
                            CameraManager.ShakeCamera();
                        if (GetAutoInfo())
                            GetUI<OcgCoreUI>().CardDescription.Show(card, card.GetMaterial());
                    }

                    foreach (var c in cards)
                        c.AnimationLandShake(card, GameCard.NeedStrongSummon(card.GetData()));
                    TokenPasss:
                    if (card.GetData().HasType(CardType.Token))
                        Sleep(20);
                    else
                        Sleep(100);

                    materialCards.Clear();
                    break;
                case GameMessage.SpSummoned:
                    ES_hint = StringHelper.GetUnsafe(1606);//怪兽特殊召唤成功
                    break;
                case GameMessage.FlipSummoning:
                    cardsInSelection.Clear();
                    code = r.ReadInt32();
                    card = GCS_Get(r.ReadShortGPS());
                    if (card != null)
                    {
                        card.SetCode(code);
                        card.p.position = (int)CardPosition.FaceUpAttack;
                        var delay = card.Move(card.p);
                        card.RefreshData();
                        card.AnimationPositon(delay);
                        ES_hint = InterString.Get("「[?]」反转召唤宣言时", card.GetData().Name);
                        if (GetAutoInfo())
                            GetUI<OcgCoreUI>().CardDescription.Show(card, card.GetMaterial());
                    }
                    materialCards.Clear();
                    Sleep(100);
                    break;
                case GameMessage.FlipSummoned:
                    ES_hint = StringHelper.GetUnsafe(1608);//怪兽反转召唤成功
                    break;
                case GameMessage.Chaining:
                    code = r.ReadInt32();
                    gps = r.ReadGPS();
                    card = GCS_Get(gps);
                    if (card != null)
                    {
                        card.SetCode(code);
                        cardsInChain.Add(card);
                        codesInChain.Add(code);
                        controllerInChain.Add(card.p.controller);
                        card.AnimationActivate();
                        Sleep(100);
                        ES_hint = InterString.Get("「[?]」被发动时", card.GetData().Name);
                    }
                    if (gps.controller == 0)
                    {
                        if (!myActivated.Contains(code))
                            myActivated.Add(code);
                    }
                    else
                    {
                        if (!opActivated.Contains(code))
                            opActivated.Add(code);
                    }
                    if (card != null && GetAutoInfo())
                        GetUI<OcgCoreUI>().CardDescription.Show(card, null);
                    break;
                case GameMessage.Chained:
                    var currentChainCard = cardsInChain[cardsInChain.Count - 1];
                    currentChainCard.AddChain(cardsInChain.Count);
                    ShowChainStack();
                    int sleepIn100 = 0;
                    if (CheckChain())
                    {
                        if (cardsInChain.Count > 1)
                            sleepIn100 = 200;
                        if (cardsInChain.Count > 3)
                            sleepIn100 = 230;
                    }
                    Sleep(sleepIn100);
                    break;
                case GameMessage.ChainSolving:
                    var id = (int)r.ReadByte();
                    currentSolvingCard = cardsInChain[id - 1];
                    messagePass = false;
                    DOTween.To(v => { }, 0, 0, ShowChainResolve(id)).OnComplete(() =>
                    {
                        if (id <= cardsInChain.Count)
                        {
                            bool needPlay = true;
                            card = cardsInChain[id - 1];
                            if (card == null)
                                needPlay = false;
                            else
                                card.ResolveChain(id);

                            if (needPlay && card.GetData().Id != codesInChain[id - 1])
                                needPlay = false;
                            if (needPlay && (negatedInChain.Contains(id) || card.negated))
                            {
                                needPlay = false;
                                card.AnimationNegate();
                                Sleep(100);
                                messagePass = true;
                                return;
                            }
                            if (needPlay && card.Disabled)
                            {
                                needPlay = false;
                                messagePass = true;
                                return;
                            }
                            if (needPlay && CurrentChainDisabled(id))
                            {
                                needPlay = false;
                                messagePass = true;
                                return;
                            }
                            if (needPlay && card.disabledInChain)
                            {
                                needPlay = false;
                                messagePass = true;
                                return;
                            }

                            if (condition == Condition.Duel
                                && Config.Get("DuelEffect", "1") == "0")
                                needPlay = false;
                            if (condition == Condition.Watch
                                && Config.Get("WatchEffect", "1") == "0")
                                needPlay = false;
                            if (condition == Condition.Replay
                                && Config.Get("ReplayEffect", "1") == "0")
                                needPlay = false;

                            if (needPlay)
                            {
                                code = card.GetData().Alias > 0 ? card.GetData().Alias : card.GetData().Id;
                                if (card.GetData().Id == 83764719)//死者苏生 异画
                                    code = 83764719;
                                if (card.GetData().Id == 63166096)//闪刀起动-交闪 异画
                                    code = 63166096;
                                if (card.GetData().Id == 32807848)//闪刀起动-交闪 异画
                                    code = 32807848;
                                var targetFolder = Program.root + "MasterDuel/Card/" + code.ToString();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                                targetFolder = Path.Combine(Application.dataPath, targetFolder);
#endif
                                if (Directory.Exists(targetFolder))
                                {
                                    effect = ABLoader.LoadFromFolder("MasterDuel/Card/" + code.ToString(), "CardEffect" + code.ToString(), true);
                                    allGameObjects.Add(effect);
                                    for (int i = 0; i < effect.transform.childCount; i++)
                                    {
                                        if (effect.transform.GetChild(i).TryGetComponent<PlayableDirector>(out _))
                                        {
                                            mono = effect.transform.GetChild(i).gameObject.AddComponent<DoWhenPlayableDirectorStop>();
                                            mono.action = () =>
                                            {
                                                messagePass = true;
                                                Destroy(effect);
                                            };
                                        }
                                        else
                                        {
                                            Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 旋风
                                    if (code == 5318639)
                                    {
                                        if (card.effectTargets.Count > 0 && card.effectTargets[0].model != null)
                                        {
                                            AudioManager.PlaySE("SE_EV_CYCLONE");
                                            effect.transform.localPosition = card.effectTargets[0].model.transform.position;
                                            if (card.p.controller != 0)
                                                effect.transform.localEulerAngles = new Vector3(0, 180, 0);
                                        }
                                        else
                                        {
                                            messagePass = true;
                                            Destroy(effect);
                                        }
                                    }
                                    // 月女神之镞
                                    else if (code == 2263869)
                                    {
                                        if (card.effectTargets.Count > 0 && card.effectTargets[0].model != null)
                                            AudioManager.PlaySE("SE_EV_ULTIMATE_SLAYER");
                                        else
                                        {
                                            messagePass = true;
                                            Destroy(effect);
                                        }
                                    }
                                    // 雷击
                                    else if (code == 12580477)
                                    {
                                        AudioManager.PlaySE("SE_EV_RAIGEKI");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name.StartsWith("Ef04343_Near"))
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name.StartsWith("Ef04343_Far"))
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        DOTween.To(v => { }, 0, 0, 0.4f).OnComplete(() =>
                                        {
                                            CameraManager.ShakeCamera(true);
                                        });
                                    }
                                    // 灰流丽
                                    else if (code == 14558127)
                                    {
                                        int order = 0;
                                        for (int i = 0; i < cardsInChain.Count; i++)
                                            if (cardsInChain[i] == card)
                                            {
                                                order = i;
                                                break;
                                            }
                                        if (order > 0)
                                        {
                                            AudioManager.PlaySE("SE_EV_ASH_BLOSSOM_v2");
                                            effect.transform.localPosition = GameCard.GetCardPosition(cardsInChain[order - 1].p);
                                        }
                                        else
                                        {
                                            messagePass = true;
                                            Destroy(effect);
                                        }
                                    }
                                    // 鹰身女妖的羽毛扫
                                    else if (code == 18144506)
                                    {
                                        AudioManager.PlaySE("SE_EV_HARPIESFEATHER_DUSTER_3D");
                                        foreach (var child in effect.transform.GetComponentsInChildren<Transform>(true))
                                            if (child.name == "DistPlane")
                                                Destroy(child.gameObject);
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04678Op(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04678(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 红色重启
                                    else if (code == 23002292)
                                    {
                                        AudioManager.PlaySE("SE_EV_REDREBOOT");
                                    }
                                    // 墓穴的指名者
                                    else if (code == 24224830)
                                    {
                                        AudioManager.PlaySE("SE_EV_CALLED_GRAVE");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef13619Op(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef13619(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 禁忌的一滴
                                    else if (code == 24299458)
                                    {
                                        AudioManager.PlaySE("SE_EV_FORBIDDEN_DROPLET");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef15299_Near(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef15299_Far(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 三战之才
                                    else if (code == 25311006)
                                    {
                                        AudioManager.PlaySE("SE_EV_TRIPLETACTICS_TALENT");
                                    }
                                    // 神之宣告
                                    else if (code == 41420027)
                                    {
                                        AudioManager.PlaySE("SE_EV_SOLEMNJUDGMENT");
                                    }
                                    // 神圣防护罩 -反射镜力-
                                    else if (code == 44095762)
                                    {
                                        AudioManager.PlaySE("SE_EV_MIRRORFORCE");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04887Op(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04887(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 黑洞
                                    else if (code == 53129443)
                                    {
                                        AudioManager.PlaySE("SE_EV_BLACKHOLE");
                                    }
                                    // 冥王结界波
                                    else if (code == 54693926)
                                    {
                                        AudioManager.PlaySE("SE_EV_DARKRULER_NOMORE");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef14742Op(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef14742(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 王宫的敕命
                                    else if (code == 61740673)
                                    {
                                        AudioManager.PlaySE("SE_EV_IMPERIAL_ORDER");
                                    }
                                    // 魔法筒
                                    else if (code == 62279055)
                                    {
                                        Tools.ChangeLayer(effect, "Default");
                                        AudioManager.PlaySE("SE_EV_MAGIC_CYLINDER");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef05124_far(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef05124_near(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 千把刀
                                    else if (code == 63391643)
                                    {
                                        if (card.effectTargets.Count > 0 && card.effectTargets[0].model != null)
                                        {
                                            AudioManager.PlaySE("SE_EV_THOUSANDKNIVES");
                                            effect.transform.localPosition = card.effectTargets[0].model.transform.position;
                                            if (card.p.controller != 0)
                                                effect.transform.localEulerAngles = new Vector3(0, 180, 0);
                                        }
                                        else
                                        {
                                            messagePass = true;
                                            Destroy(effect);
                                        }
                                    }
                                    // 抹杀之指名者
                                    else if (code == 65681983)
                                    {
                                        AudioManager.PlaySE("SE_EV_CROSSOUT_DESIGNATOR");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef14627_Near(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef14627_Far(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 光之护封剑
                                    else if (code == 72302403)
                                    {
                                        AudioManager.PlaySE("SE_EV_GOFUKEN");
                                        if (card.p.controller == 0)
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04354Op(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).name == "Ef04354(Clone)")
                                                    Destroy(effect.transform.GetChild(i).gameObject);
                                        }
                                    }
                                    // 封印之黄金柜
                                    else if (code == 75500286)
                                    {
                                        AudioManager.PlaySE("SE_EV_GOLD_SARCOPHAGUS");
                                    }
                                    // 死者苏生
                                    else if (code == 83764718 || code == 83764719)
                                    {
                                        AudioManager.PlaySE("SE_EV_MONSTER_REBORN");
                                    }
                                    // 闪刀起动-交闪
                                    else if (code == 63166095 || code == 63166096)//ENGAGE
                                    {
                                        Destroy(effect);
                                        messagePass = true;
                                        nextMoveAction = () =>
                                        {
                                            var effect = ABLoader.LoadFromFolder("MasterDuel/Card/" + code, "CardEffect" + code, true);
                                            allGameObjects.Add(effect);
                                            var manager = effect.transform.GetChild(0).GetComponent<ElementObjectManager>();
                                            nextMoveManager = manager;
                                            nextMoveTime = 0.3f;
                                            var mono = manager.gameObject.AddComponent<DoWhenPlayableDirectorStop>();
                                            mono.action = () =>
                                            {
                                                Destroy(effect);
                                            };
                                            nextMoveActionTargetRenderer = manager.GetElement<Renderer>("SummonPosDummy");
                                        };
                                    }
                                    // 大风暴
                                    else if (code == 19613556)
                                    {
                                        AudioManager.PlaySE("SE_EV_HEAVY_STORM");
                                    }
                                    // 天霆号 阿宙斯
                                    else if (code == 90448279)
                                    {
                                        AudioManager.PlaySE("SE_EV_AZEUS");
                                    }
                                    // 增援
                                    else if (code == 32807846)
                                    {
                                        AudioManager.PlaySE("SE_EV_039_NORMAL");
                                    }
                                    // 增援 异画
                                    else if (code == 32807848)
                                    {
                                        AudioManager.PlaySE("SE_EV_039_SPECIAL");
                                    }
                                    // 幽鬼兔
                                    else if (code == 59438930)
                                    {
                                        int order = 0;
                                        for (int i = 0; i < cardsInChain.Count; i++)
                                            if (cardsInChain[i] == card)
                                                order = i;
                                        if (order > 0)
                                        {
                                            AudioManager.PlaySE("SE_EV_038_NORMAL");
                                            ElementObjectManager manager = null;
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).TryGetComponent(out manager))
                                                    break;
                                            var cardEffect = manager.GetElement<Transform>("EffectOffset");
                                            cardEffect.localPosition = GameCard.GetCardPosition(cardsInChain[order - 1].p);
                                            cardEffect.localEulerAngles = GameCard.GetCardRotation(cardsInChain[order - 1].p);
                                            cardEffect.localScale = GameCard.GetCardScale(cardsInChain[order - 1].p);
                                            manager.GetNestedElement<MeshRenderer>("CardOffset/DummyCard/DummyCardModel_front")
                                                .material = cardsInChain[order - 1].GetMaterial();
                                        }
                                        else
                                        {
                                            messagePass = true;
                                            Destroy(effect);
                                        }
                                    }
                                    // 欢聚友伴·抖抖海月水母
                                    else if (code == 84192580)
                                    {
                                        AudioManager.PlaySE("SE_EV_040_NORMAL");
                                        ElementObjectManager manager = null;
                                        for (int i = 0; i < effect.transform.childCount; i++)
                                            if (effect.transform.GetChild(i).TryGetComponent(out manager))
                                                break;
                                        Destroy(manager.GetElement(card.p.controller == 0 ? "Hand01" : "EnHand01"));
                                    }
                                    // 欢聚友伴·茸茸长尾山雀
                                    else if (code == 42141493)
                                    {
                                        AudioManager.PlaySE("SE_EV_041_NORMAL");
                                        ElementObjectManager manager = null;
                                        for (int i = 0; i < effect.transform.childCount; i++)
                                            if (effect.transform.GetChild(i).TryGetComponent(out manager))
                                                break;

                                        if (card.p.controller == 0)
                                        {
                                            Destroy(manager.GetElement("MainDeck"));
                                            Destroy(manager.GetElement("ExDeck"));
                                            DuelEffectUtil.SetDeckModelAppearance(manager.GetElement<ElementObjectManager>("EnMainDeck")
                                                , GetLocationCardCount(CardLocation.Deck, 1), opDeck);
                                            DuelEffectUtil.SetDeckModelAppearance(manager.GetElement<ElementObjectManager>("EnExDeck")
                                                , GetLocationCardCount(CardLocation.Extra, 1), opExtra);
                                        }
                                        else
                                        {
                                            Destroy(manager.GetElement("EnMainDeck"));
                                            Destroy(manager.GetElement("EnExDeck"));

                                            DuelEffectUtil.SetDeckModelAppearance(manager.GetElement<ElementObjectManager>("MainDeck")
                                                , GetLocationCardCount(CardLocation.Deck, 0), myDeck);
                                            DuelEffectUtil.SetDeckModelAppearance(manager.GetElement<ElementObjectManager>("ExDeck")
                                                , GetLocationCardCount(CardLocation.Extra, 0), myExtra);
                                        }
                                    }
                                    // 欢聚友伴·喵喵豹猫
                                    else if (code == 87126721)
                                    {
                                        AudioManager.PlaySE("SE_EV_042_NORMAL");
                                        foreach(var sr in effect.transform.GetComponentsInChildren<SpriteRenderer>(true))
                                            sr.maskInteraction = SpriteMaskInteraction.None;

                                        if (card.p.controller == 0)
                                            Destroy(effect.transform.GetChildByName("GraveSet").gameObject);
                                        else
                                            Destroy(effect.transform.GetChildByName("EnGraveSet").gameObject);
                                    }
                                    // 灵王的波动
                                    else if (code == 40366667)
                                    {
                                        int order = 0;
                                        for (int i = 0; i < cardsInChain.Count; i++)
                                            if (cardsInChain[i] == card)
                                            {
                                                order = i;
                                                break;
                                            }
                                        messagePass = true;
                                        effect.SetActive(false);

                                        if (order > 0)
                                        {
                                            effect.transform.DestroyChildByName("BG");
                                            ElementObjectManager manager = null;
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).TryGetComponent(out manager))
                                                    break;
                                            manager.GetNestedElement("CardOffset/DummyCard").SetActive(false);
                                            var targetEff = cardsInChain[order].setOverTurn ? "CardOffset/nomalEf" : "CardOffset/handEf";
                                            manager.GetNestedElement(targetEff).SetActive(true);
                                            manager.gameObject.SetActive(false);
                                            nextNegateAction_AdditionalManager = manager;
                                            nextNegateAction_AdditionalTime = 0.5f;
                                            nextNegateAction_Additional = () =>
                                            {
                                                manager.gameObject.SetActive(true);
                                                AudioManager.PlaySE("SE_EV_044_NORMAL");

                                                Destroy(effect);
                                                Destroy(manager.gameObject, 2f);
                                            };
                                        }
                                        else
                                        {
                                            Destroy(effect);
                                        }
                                    }
                                    // 圣王的粉碎
                                    else if (code == 97045737)
                                    {
                                        int order = 0;
                                        for (int i = 0; i < cardsInChain.Count; i++)
                                            if (cardsInChain[i] == card)
                                            {
                                                order = i;
                                                break;
                                            }
                                        messagePass = true;
                                        effect.SetActive(false);

                                        if (order > 0)
                                        {
                                            effect.transform.DestroyChildByName("BG");
                                            ElementObjectManager manager = null;
                                            for (int i = 0; i < effect.transform.childCount; i++)
                                                if (effect.transform.GetChild(i).TryGetComponent(out manager))
                                                    break;
                                            manager.GetElement("CardOffset").SetActive(false);
                                            manager.gameObject.SetActive(false);
                                            nextNegateAction_AdditionalManager = manager;
                                            nextNegateAction_AdditionalTime = 0.8f;
                                            nextNegateAction_Additional = () =>
                                            {
                                                manager.gameObject.SetActive(true);
                                                AudioManager.PlaySE("SE_EV_043_NORMAL");

                                                Destroy(effect);
                                                Destroy(manager.gameObject, 2f);
                                            };
                                        }
                                        else
                                        {
                                            Destroy(effect);
                                        }
                                    }
                                }
                                else
                                {
                                    // 技能抽取
                                    if (code == 82732705)
                                    {
                                        if (card.model == null)
                                        {
                                            messagePass = true;
                                            return;
                                        }

                                        AudioManager.PlaySE("SE_EV_SKILLDRAIN");
                                        var effArea = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_05740_Area", "fxp_05740_Area", true);
                                        var effCard = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_05740_Card", "fxp_05740_Card", true);
                                        effCard.SetActive(false);
                                        effCard.transform.position = card.model.transform.position;
                                        DOTween.To(v => { }, 0, 0, 0.5f).OnComplete(() =>
                                        {
                                            effCard.SetActive(true);
                                        });
                                        DOTween.To(v => { }, 0, 0, 1.5f).OnComplete(() =>
                                        {
                                            Destroy(effArea);
                                            Destroy(effCard);
                                            messagePass = true;
                                        });
                                    }
                                    // 无限泡影
                                    else if (code == 10045474)
                                    {
                                        if (card.effectTargets.Count == 0 || card.effectTargets[0].model == null)
                                        {
                                            messagePass = true;
                                            return;
                                        }

                                        var time = 1.5f;

                                        AudioManager.PlaySE("SE_EV_INFINITE_IMPERMANENCE");
                                        var effCard = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_13631_Card", "fxp_13631_Card", true);
                                        effCard.transform.position = card.effectTargets[0].model.transform.position;
                                        if ((card.effectTargets[0].p.position & (uint)CardPosition.Attack) > 0)
                                            Destroy(effCard.transform.GetChild(0).GetChild(1).gameObject);
                                        else
                                            Destroy(effCard.transform.GetChild(0).GetChild(0).gameObject);

                                        if (card.setOverTurn)
                                        {
                                            GameObject effArea = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_13631_Area", "fxp_13631_Area", true);
                                            GameObject effAreaLoop = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_13631_Area_Loop", "fxp_13631_Area_Loop", true);
                                            foreach (var place in Program.instance.ocgcore.places)
                                            {
                                                if (place.InTheSameLine(card.p))
                                                {
                                                    var area = Instantiate(effArea);
                                                    area.transform.position = place.transform.position;
                                                    Destroy(area, time);
                                                    var loop = Instantiate(effAreaLoop);
                                                    loop.transform.SetParent(place.transform, false);
                                                    if ((place.p.location & (uint)CardLocation.MonsterZone) > 0)
                                                        loop.transform.localScale = new Vector3(1f, 1f, 1.1f);
                                                    allGameObjects.Add(loop);
                                                    turnEndDeleteObjects.Add(loop);
                                                }
                                            }
                                            Destroy(effArea);
                                            Destroy(effAreaLoop);
                                        }

                                        DOTween.To(v => { }, 0, 0, time).OnComplete(() =>
                                        {
                                            Destroy(effCard);
                                            messagePass = true;
                                        });
                                    }
                                    // 闪电风暴
                                    else if (code == 14532163)
                                    {
                                        var eff = ABLoader.LoadFromFolder("MasterDuel/Effects/MagicTrapEffects/fxp_14876", "fxp_14876", true);

                                        if (card.p.controller == 0)
                                        {
                                            AudioManager.PlaySE("SE_EV_LIGHTNINGSTORM_P");
                                            Destroy(eff.transform.GetChild(0).GetChild(0).GetChild(2).gameObject);
                                        }
                                        else
                                        {
                                            AudioManager.PlaySE("SE_EV_LIGHTNINGSTORM_R");
                                            Destroy(eff.transform.GetChild(0).GetChild(0).GetChild(0).gameObject);
                                        }
                                        DOTween.To(v => { }, 0, 0, 1f).OnComplete(() =>
                                        {
                                            Destroy(eff);
                                            messagePass = true;
                                        });
                                    }
                                    // 效果遮蒙者
                                    else if (code == 97268402)
                                    {
                                        if (card.effectTargets.Count == 0 || card.effectTargets[0].model == null)
                                        {
                                            messagePass = true;
                                            return;
                                        }
                                        AudioManager.PlaySE("SE_EV_EFFECT_VEILER");
                                        var eff = ABLoader.LoadFromFolder("MasterDuel/Effects/MonsterEffectProcess/fxp_mep08933_01", "fxp_mep08933_01", true);
                                        eff.transform.position = card.effectTargets[0].model.transform.position;
                                        DOTween.To(v => { }, 0, 0, 1f).OnComplete(() =>
                                        {
                                            Destroy(eff);
                                            messagePass = true;
                                        });
                                    }
                                    // 屋敷童
                                    else if (code == 73642296)
                                    {
                                        int order = 0;
                                        for (int i = 0; i < cardsInChain.Count; i++)
                                            if (cardsInChain[i] == card)
                                            {
                                                order = i;
                                                break;
                                            }
                                        messagePass = true;

                                        if (order > 0)
                                        {
                                            nextNegateAction = () =>
                                            {
                                                AudioManager.PlaySE("SE_EV_GHOSTBELLE");

                                                var eff = ABLoader.LoadFromFolder("MasterDuel/Effects/MonsterEffectProcess/ef13587", "ef13587", true);
                                                eff.transform.localPosition = GameCard.GetCardPosition(cardsInChain[order - 1].p);
                                                Tools.ChangeLayer(eff, "DuelOverlay3D");
                                                CameraManager.DuelOverlay3DPlus();

                                                if(card.GetData().Id == 73642297)
                                                {
                                                    AudioManager.PlaySE("SE_EV_037_SPECIAL");
                                                    var model = ABLoader.LoadFromFolder("MasterDuel/Card/73642297", "CardEffect" + code.ToString(), true);
                                                    model.transform.localPosition = GameCard.GetCardPosition(cardsInChain[order - 1].p);
                                                    Tools.ChangeLayer(model, "DuelOverlay3D");

                                                    if (cardsInChain[order - 1].p.controller == 0)
                                                        Destroy(model.transform.GetChild(1).gameObject);
                                                    else
                                                        Destroy(model.transform.GetChild(0).gameObject);
                                                    DOTween.To(v => { }, 0, 0, 2f).OnComplete(() =>
                                                    {
                                                        Destroy(model);
                                                    });
                                                }

                                                DOTween.To(v => { }, 0, 0, 2f).OnComplete(() =>
                                                {
                                                    Destroy(eff);
                                                    CameraManager.DuelOverlay3DMinus();
                                                });

                                            };
                                        }
                                    }
                                    else
                                        messagePass = true;
                                }
                            }
                            else
                                messagePass = true;
                        }
                        else
                            messagePass = true;
                    });
                    break;
                case GameMessage.ChainSolved:
                    id = r.ReadByte();
                    if (id <= cardsInChain.Count)
                    {
                        card = cardsInChain[id - 1];
                        card.RemoveChain(id);
                    }
                    materialCards.Clear();
                    break;
                case GameMessage.ChainEnd:
                    //For log
                    GetUI<OcgCoreUI>().DuelLog.chainSolving = cardsInChain.Count;
                    foreach (var c in cardsInChain)
                    {
                        c.negated = false;
                        c.disabledInChain = false;
                        c.RemoveAllChain();
                        c.effectTargets.Clear();
                    }
                    cardsBeTarget.Clear();
                    cardsInChain.Clear();
                    codesInChain.Clear();
                    controllerInChain.Clear();
                    negatedInChain.Clear();
                    materialCards.Clear();
                    currentSolvingCard = null;
                    foreach (var c in tempCards)
                        c.Dispose();
                    tempCards.Clear();
                    break;
                case GameMessage.ChainNegated:
                    id = r.ReadByte();
                    if (id <= cardsInChain.Count)
                    {
                        negatedInChain.Add(id);
                        card = cardsInChain[id - 1];
                        card.negated = true;
                    }
                    break;
                case GameMessage.ChainDisabled:
                    id = r.ReadByte();
                    if (id <= cardsInChain.Count)
                    {
                        card = cardsInChain[id - 1];
                        card.disabledInChain = true;
                        if (!card.negated)
                        {
                            card.AnimationNegate();
                            Sleep(100);
                        }
                    }
                    break;
                case GameMessage.Attack:
                    var gps1 = r.ReadGPS();
                    var gps2 = r.ReadGPS();
                    var attackCard = GCS_Get(gps1);
                    if (attackCard != null)
                    {
                        attackingCard = attackCard;
                        ES_hint = InterString.Get("「[?]」攻击时", attackCard.GetData().Name);
                        var endPosition = opPosition;
                        var attacked = GCS_Get(gps2);
                        bool finalBlow = false;
                        if (attacked != null)
                        {
                            endPosition = attacked.model.transform.localPosition;
                            if ((attacked.p.position & (uint)CardPosition.Attack) > 0)
                            {
                                var differ = attackCard.GetData().Attack - attacked.GetData().Attack;
                                if (attackCard.p.controller == 0)
                                {
                                    if (differ >= life1)
                                        finalBlow = true;
                                }
                                else
                                {
                                    if (differ >= life0)
                                        finalBlow = true;
                                }
                            }
                        }
                        else
                        {
                            if (attackCard.p.controller == 0)
                            {
                                endPosition = opPosition;
                                if (attackCard.GetData().Attack >= life1)
                                    finalBlow = true;
                            }
                            else
                            {
                                endPosition = myPosition;
                                if (attackCard.GetData().Attack >= life0)
                                    finalBlow = true;
                            }
                            effect = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelDirectAtk00", true);
                            mono = effect.AddComponent<DoWhenPlayableDirectorStop>();
                            mono.action = () =>
                            {
                                Destroy(effect);
                            };
                            AudioManager.PlaySE("SE_DA_TEXT");
                        }
                        ShowAttackLine(attackingCard.model.transform.localPosition, endPosition);
                        if (finalBlow)
                        {
                            if (duelFinalBlow != null)
                                Destroy(duelFinalBlow);
                            duelFinalBlow = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelText/DuelFinalBlow", true);
                        }
                        Sleep(20);
                    }
                    else
                        attackingCard = null;
                    break;
                case GameMessage.AttackDisabled:
                    ES_hint = InterString.Get("攻击被无效时");
                    attackLine.SetActive(false);
                    Destroy(duelFinalBlow, 0.5f);
                    break;
                case GameMessage.DamageStepStart:
                    break;
                case GameMessage.DamageStepEnd:
                    break;
                case GameMessage.BeChainTarget:
                    break;
                case GameMessage.CreateRelation:
                    MessageManager.Cast("CreateRelation");
                    break;
                case GameMessage.ReleaseRelation:
                    MessageManager.Cast("ReleaseRelation");
                    break;
                case GameMessage.Battle:
                    var gpsAttacker = r.ReadShortGPS();
                    r.ReadByte();
                    attackCard = GCS_Get(gpsAttacker);
                    if (attackCard != null)
                    {
                        var data2 = attackCard.GetData();
                        data2.Attack = r.ReadInt32();
                        data2.Defense = r.ReadInt32();
                        attackCard.SetData(data2);
                    }
                    else
                    {
                        r.ReadInt32();
                        r.ReadInt32();
                    }

                    r.ReadByte();
                    var gpsAttacked = r.ReadShortGPS();
                    r.ReadByte();
                    var attackedCard = GCS_Get(gpsAttacked);
                    if (attackCard != null && gpsAttacked.location != 0)
                    {
                        var data2 = attackedCard.GetData();
                        data2.Attack = r.ReadInt32();
                        data2.Defense = r.ReadInt32();
                        attackedCard.SetData(data2);
                    }
                    else
                    {
                        r.ReadInt32();
                        r.ReadInt32();
                    }

                    r.ReadByte();
                    var attackTransform = attackCard.manager.GetElement<Transform>("CardPlane");
                    var attackPosition = attackTransform.position;
                    var attackAngle = attackTransform.eulerAngles;

                    Vector3 attackedPosition;
                    int directAttack = 0;
                    if (attackedCard == null || gpsAttacked.location == 0)
                    {
                        if (gpsAttacker.controller == 0)
                        {
                            attackedPosition = opPosition;
                            directAttack = 1;
                        }
                        else
                        {
                            attackedPosition = myPosition;
                            directAttack = -1;
                        }
                    }
                    else
                        attackedPosition = attackedCard.model.transform.position;


                    attackLine.SetActive(false);
                    bool isFinalAttack = false;
                    if (duelFinalBlow != null)
                    {
                        isFinalAttack = true;
                        Destroy(duelFinalBlow);
                    }
                    needDamageResponseInstant = true;
                    messagePass = false;
                    if (isFinalAttack && GetSpecialFinalAttackType(attackCard, attackedPosition) != FinalAttackType.Normal)
                        break;

                    GameObject tailObj = null;
                    GameObject hitObj = null;
                    string hit = string.Empty;
                    tail = string.Empty;
                    string sound1 = string.Empty;
                    string sound2 = string.Empty;
                    if ((attackCard.GetData().Attribute & (uint)CardAttribute.Dark) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkdak_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitdak_S2_001";
                        sound1 = "SE_ATTACK_A_DARK_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_DARK_SPECIAL_02";
                    }
                    else if ((attackCard.GetData().Attribute & (uint)CardAttribute.Earth) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkeah_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hiteah_S2_001";
                        sound1 = "SE_ATTACK_A_EARTH_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_EARTH_SPECIAL_02";
                    }
                    else if ((attackCard.GetData().Attribute & (uint)CardAttribute.Fire) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkfie_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitfie_S2_001";
                        sound1 = "SE_ATTACK_A_FIRE_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_FIRE_SPECIAL_02";
                    }
                    else if ((attackCard.GetData().Attribute & (uint)CardAttribute.Light) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atklit_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitlit_S2_001";
                        sound1 = "SE_ATTACK_A_LIGHT_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_LIGHT_SPECIAL_02";
                    }
                    else if ((attackCard.GetData().Attribute & (uint)CardAttribute.Water) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkwtr_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitwtr_S2_001";
                        sound1 = "SE_ATTACK_A_WIND_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_WIND_SPECIAL_02";
                    }
                    else if ((attackCard.GetData().Attribute & (uint)CardAttribute.Wind) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkwid_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitwid_S2_001";
                        sound1 = "SE_ATTACK_A_DARK_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_DARK_SPECIAL_02";
                    }
                    else// if ((attackCard.GetData().Attribute & (uint)CardAttribute.Divine) > 0)
                    {
                        tail = "MasterDuel/Effects/Attack/fxp_atkdve_S2_001";
                        hit = "MasterDuel/Effects/Hit/fxp_hitdve_S2_001";
                        sound1 = "SE_ATTACK_A_DIVINE_SPECIAL_01";
                        sound2 = "SE_ATTACK_A_DIVINE_SPECIAL_02";

                        if (attackCard.GetData().Attack < 2000)
                        {
                            tail = tail.Replace("_S2_", "_S1_");
                            hit = hit.Replace("_S2_", "_S1_");
                        }
                        tailObj = ABLoader.LoadFromFolder(tail, Path.GetFileName(tail), true);
                        hitObj = ABLoader.LoadFromFolder(hit, Path.GetFileName(hit), true);
                        hitObj.SetActive(false);
                    }

                    if (attackCard.GetData().Attack < 2000)
                    {
                        tail = tail.Replace("_S2_", "_S1_");
                        hit = hit.Replace("_S2_", "_S1_");
                    }

                    if (directAttack == 0)
                    {
                        attackTransform.LookAt(attackedCard.model.transform);
                        if ((attackedCard.p.position & (uint)CardPosition.Defence) > 0)
                            if (attackedCard.GetData().Defense >= attackCard.GetData().Attack)
                            {
                                hit = "MasterDuel/Effects/Hit/fxp_hit_guard_001";
                                sound2 = "SE_ATTACK_GUARD";
                            }
                        if ((attackedCard.p.position & (uint)CardPosition.Attack) > 0)
                            if (attackedCard.GetData().Attack > attackCard.GetData().Attack)
                            {
                                hit = "MasterDuel/Effects/Hit/fxp_hit_guard_001";
                                sound2 = "SE_ATTACK_GUARD";
                            }
                    }
                    else
                    {
                        GameObject dummy = new GameObject();
                        dummy.transform.position = attackedPosition;
                        attackTransform.LookAt(dummy.transform);
                        if (directAttack == 1)
                        {
                            hit = "MasterDuel/Effects/Hit/fxp_dithit_far_001";
                            sound2 = "SE_DIRECT_ATTACK_RIVAL";
                        }
                        else
                        {
                            hit = "MasterDuel/Effects/Hit/fxp_dithit_near_001";
                            sound2 = "SE_DIRECT_ATTACK_PLAYER";
                        }
                        Destroy(dummy);
                    }

                    if (tailObj == null)
                        tailObj = ABLoader.LoadFromFile(tail, true);
                    tailObj.transform.SetParent(attackTransform, false);
                    tailObj.SetActive(false);

                    Vector3 v = attackedPosition - attackPosition;
                    v.y = 0;

                    Vector3 faceAngle = attackTransform.eulerAngles;
                    faceAngle.x = 0;
                    attackTransform.eulerAngles = attackAngle;

                    Sequence quence = DOTween.Sequence();
                    if (attackCard.GetData().Attack < 2000)
                    {
                        faceAngle.z = faceAngle.y >= 0 && faceAngle.y < 180 ? -20f : 20f;
                        quence.Append(attackTransform.DOMove(attackPosition + new Vector3(0f, 10f, 0f) - v * 0.3f, 0.3f).SetEase(Ease.InOutCubic).OnComplete(() =>
                        {
                            tailObj.SetActive(true);
                            foreach (Transform t in tailObj.GetComponentsInChildren<Transform>(true))
                                t.gameObject.SetActive(true);
                        }));
                        quence.Join(attackTransform.DORotate(faceAngle, 0.3f).SetEase(Ease.InOutCubic));
                        quence.Append(attackTransform.DOMove(attackPosition + (attackedPosition - attackPosition) * 0.8f + new Vector3(0f, 0f, 0f), 0.1f).SetEase(Ease.InSine));
                        faceAngle.z = 0;
                        quence.Join(attackTransform.DORotate(faceAngle, 0.1f).SetEase(Ease.InSine));
                        quence.Join(Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37 + directAttack * 5), 0.1f));
                        quence.AppendCallback(() =>
                        {
                            CameraManager.ShakeCamera();
                            messagePass = true;
                            if (hitObj == null)
                                hitObj = ABLoader.LoadFromFile(hit, true);
                            else
                                hitObj.SetActive(true);
                            attackedPosition.y += 5;
                            hitObj.transform.position = attackedPosition;
                            Destroy(hitObj, 5f);
                            AudioManager.PlaySE(sound2);
                        });
                        quence.AppendInterval(0.3f);
                        quence.Append(attackTransform.DOMove(attackPosition, 0.3f).SetEase(Ease.InQuad));
                        quence.Join(Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37), 0.3f));
                        quence.Join(attackTransform.DORotate(attackAngle, 0.3f).SetEase(Ease.InQuad));
                        Sleep(100);
                    }
                    else
                    {
                        faceAngle.z = faceAngle.y >= 0 && faceAngle.y < 180 ? -30f : 30f;
                        quence.Append(attackTransform.DOMove(attackPosition + new Vector3(0f, 10f, 0f) - v * 0.4f, 0.5f).SetEase(Ease.InOutCubic));
                        quence.Join(attackTransform.DORotate(faceAngle + new Vector3(45f, 0f, 0f), 0.5f).SetEase(Ease.InOutCubic));
                        quence.InsertCallback(0.4f, () =>
                        {
                            tailObj.SetActive(true);
                            foreach (Transform t in tailObj.GetComponentsInChildren<Transform>(true))
                                t.gameObject.SetActive(true);
                        });

                        quence.Append(attackTransform.DOMove(attackPosition + (attackedPosition - attackPosition) * 0.8f + new Vector3(0f, 0f, 0f), 0.15f).SetEase(Ease.InSine));
                        faceAngle.z = 0;
                        quence.Join(attackTransform.DORotate(faceAngle, 0.15f));
                        quence.Join(Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37 + directAttack * 5), 0.15f));
                        quence.AppendCallback(() =>
                        {
                            CameraManager.ShakeCamera(true);
                            messagePass = true;
                            if (hitObj == null)
                                hitObj = ABLoader.LoadFromFile(hit, true);
                            else
                                hitObj.SetActive(true);
                            attackedPosition.y += 5;
                            hitObj.transform.position = attackedPosition;
                            Destroy(hitObj, 5f);
                            AudioManager.PlaySE(sound2);
                        });
                        quence.AppendInterval(0.3f);
                        quence.Append(attackTransform.DOMove(attackPosition, 0.3f).SetEase(Ease.InQuad));
                        quence.Join(Program.instance.camera_.cameraMain.transform.DOMove(new Vector3(0, 95, -37), 0.3f));
                        quence.Join(attackTransform.DORotate(attackAngle, 0.3f).SetEase(Ease.InQuad));
                        Sleep(125);
                    }
                    quence.OnComplete(() =>
                    {
                        needDamageResponseInstant = false;
                    });
                    AudioManager.PlaySE(sound1);
                    Destroy(tailObj, 3f);
                    break;
                case GameMessage.Damage:
                    player = LocalPlayer(r.ReadByte());
                    val = r.ReadInt32();
                    ES_hint = player == 0 ? InterString.Get("我方受到伤害时") : InterString.Get("对方受到伤害时");
                    if (player == 0)
                    {
                        life0 -= val;
                        ES_hint = InterString.Get("我方受到伤害时");
                    }
                    else
                    {
                        life1 -= val;
                        ES_hint = InterString.Get("对方受到伤害时");
                    }
                    if (life0 <= 0 || life1 <= 0)
                    {
                        AudioManager.StopBGM();
                        GetUI<OcgCoreUI>().OnNor();
#if UNITY_EDITOR
                        Program.instance.timeScaleForEditor = 0.1f;
                        DOTween.To(() => Program.instance.timeScaleForEditor, x => Program.instance.timeScaleForEditor = x, 1f, 0.85f).SetEase(Ease.InQuad);
#else
                        Program.instance.TimeScale = 0.1f;
                        DOTween.To(() => Program.instance.TimeScale, x => Program.instance.TimeScale = x, 1f, 0.85f).SetEase(Ease.InQuad);
#endif

                        if (life0 <= 0)
                        {
                            hitObj = ABLoader.LoadFromFile("MasterDuel/Effects/Hit/fxp_dithit_fin_near_001");
                            hitObj.transform.position = new Vector3(0, 15, -25);
                            Destroy(hitObj, 10);
                        }
                        if (life1 <= 0)
                        {
                            hitObj = ABLoader.LoadFromFile("MasterDuel/Effects/Hit/fxp_dithit_fin_far_001");
                            hitObj.transform.position = new Vector3(0, 15, 25);
                            Destroy(hitObj, 10);
                        }
                    }
                    UpdateBgEffect(player);
                    SetLP(player, -val);
                    Sleep(50);
                    break;
                case GameMessage.PayLpCost:
                    player = LocalPlayer(r.ReadByte());
                    val = r.ReadInt32();
                    if (player == 0)
                        life0 -= val;
                    else
                        life1 -= val;
                    UpdateBgEffect(player);
                    SetLP(player, -val);
                    Sleep(50);
                    break;
                case GameMessage.Recover:
                    player = LocalPlayer(r.ReadByte());
                    val = r.ReadInt32();
                    ES_hint = player == 0 ? InterString.Get("我方生命值回复时") : InterString.Get("对方生命值回复时");
                    if (player == 0)
                    {
                        life0 += val;
                    }
                    else
                    {
                        life1 += val;
                    }
                    SetLP(player, val);
                    Sleep(50);
                    break;
                case GameMessage.LpUpdate:
                    player = LocalPlayer(r.ReadByte());
                    val = r.ReadInt32();
                    if (player == 0)
                    {
                        GetUI<OcgCoreUI>().DuelLog.cacheLp = val - life0;
                        life0 = val;
                    }
                    else
                    {
                        GetUI<OcgCoreUI>().DuelLog.cacheLp = val - life1;
                        life1 = val;
                    }
                    if (life0 <= 0 || life1 <= 0)
                    {
#if UNITY_EDITOR
                        Program.instance.timeScaleForEditor = 0.1f;
                        DOTween.To(() => Program.instance.timeScaleForEditor, x => Program.instance.timeScaleForEditor = x, 1, 0.8f).SetEase(Ease.InQuad);
#else
                        Program.instance.TimeScale = 0.1f;
                        DOTween.To(() => Program.instance.TimeScale, x => Program.instance.TimeScale = x, 1, 0.8f).SetEase(Ease.InQuad);
#endif
                        if (life0 <= 0)
                        {
                            hitObj = ABLoader.LoadFromFile("MasterDuel/Effects/Hit/fxp_dithit_fin_near_001");
                            hitObj.transform.position = new Vector3(0, 15, -25);
                            Destroy(hitObj, 10);
                        }
                        if (life1 <= 0)
                        {
                            hitObj = ABLoader.LoadFromFile("MasterDuel/Effects/Hit/fxp_dithit_fin_far_001");
                            hitObj.transform.position = new Vector3(0, 15, 25);
                            Destroy(hitObj, 10);
                        }
                    }
                    UpdateBgEffect(player);
                    SetLP(player, GetUI<OcgCoreUI>().DuelLog.cacheLp);
                    Sleep(50);
                    break;
                case GameMessage.TossCoin:
                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    bool config = true;
                    if (condition == Condition.Duel
                        && Config.Get("DuelCoin", "1") == "0")
                        config = false;
                    if (condition == Condition.Watch
                        && Config.Get("WatchCoin", "1") == "0")
                        config = false;
                    if (condition == Condition.Replay
                        && Config.Get("ReplayCoin", "1") == "0")
                        config = false;

                    if (config)
                    {
                        AudioManager.PlaySE("SE_COIN_THROW");
                        for (var i = 0; i < count; i++)
                        {
                            var coin = ABLoader.LoadFromFolder("MasterDuel/Timeline/DuelCoinToss01", "DuelCoinToss", true);
                            var manager = coin.transform.GetChild(0).GetComponent<ElementObjectManager>();
                            manager.GetComponent<PlayableDirector>().Play();
                            Destroy(coin, 3f);
                            var x = -(count - 1) * 8 + i * 16;
                            coin.transform.localPosition = new Vector3(x, 0, 0);
                            GameObject targetCoin;
                            if (player == 0)
                                targetCoin = manager.GetElement("Blue");
                            else
                                targetCoin = manager.GetElement("Red");
                            targetCoin.SetActive(true);
                            data = r.ReadByte();
                            if (data == 0)
                            {
                                DOTween.To(v => { }, 0, 0, 2f).OnComplete(() =>
                                {
                                    AudioManager.PlaySE("SE_COIN_DECIDE_02");
                                });
                                quence = DOTween.Sequence();
                                quence.AppendInterval(1f);
                                quence.Append(targetCoin.transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f));
                            }
                            else
                            {
                                DOTween.To(v => { }, 0, 0, 2f).OnComplete(() =>
                                {
                                    AudioManager.PlaySE("SE_COIN_DECIDE");
                                });
                            }
                        }
                        Sleep(300);
                    }
                    else
                    {
                        for (var i = 0; i < count; i++)
                        {
                            data = r.ReadByte();
                            if (data == 1)
                                MessageManager.Cast(InterString.Get("硬币正面"));
                            else
                                MessageManager.Cast(InterString.Get("硬币反面"));
                        }
                    }
                    break;
                case GameMessage.TossDice:
                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    config = true;
                    if (condition == Condition.Duel
                        && Config.Get("DuelDice", "1") == "0")
                        config = false;
                    if (condition == Condition.Watch
                        && Config.Get("WatchDice", "1") == "0")
                        config = false;
                    if (condition == Condition.Replay
                        && Config.Get("ReplayDice", "1") == "0")
                        config = false;
                    if (config)
                    {
                        AudioManager.PlaySE("SE_DICE_ROLL");
                        DOTween.To(v => { }, 0, 0, 0.6f).OnComplete(() =>
                        {
                            AudioManager.PlaySE("SE_DICE_DECIDE");
                        });
                        for (var i = 0; i < count; i++)
                        {
                            var instance = Instantiate(player == 0 ? myDice : opDice);
                            instance.SetActive(true);
                            instance.GetComponent<PlayableDirector>().enabled = true;
                            instance.GetComponent<ScreenEffect>().enabled = true;
                            Destroy(instance, 2f);
                            var diceNumber = instance.GetComponent<ElementObjectManager>().
                                GetElement<Transform>("DiceNumber");
                            data = r.ReadByte();
                            switch (data)
                            {
                                case 1:
                                    diceNumber.localEulerAngles = Vector3.zero;
                                    break;
                                case 2:
                                    diceNumber.localEulerAngles = new Vector3(270, 0, 0);
                                    break;
                                case 3:
                                    diceNumber.localEulerAngles = new Vector3(0, 0, 270);
                                    break;
                                case 4:
                                    diceNumber.localEulerAngles = new Vector3(0, 0, 90);
                                    break;
                                case 5:
                                    diceNumber.localEulerAngles = new Vector3(90, 0, 0);
                                    break;
                                case 6:
                                    diceNumber.localEulerAngles = new Vector3(180, 90, 0);
                                    break;
                            }

                            var x = -(count - 1) * 5 + i * 10;
                            instance.transform.localPosition = new Vector3(x, 0, 0);
                        }
                        Sleep(200);
                    }
                    else
                    {
                        for (var i = 0; i < count; i++)
                        {
                            data = r.ReadByte();
                            MessageManager.Cast(InterString.Get("骰子结果：[?]", data.ToString()));
                        }
                    }
                    break;
                case GameMessage.HandResult:
                    MessageManager.Cast("HandResult");
                    break;
                case GameMessage.Draw:
                    player = LocalPlayer(r.ReadByte());
                    ES_hint = player == 0 ? InterString.Get("我方抽卡时") : InterString.Get("对方抽卡时");
                    count = r.ReadByte();
                    var deckCount = GetLocationCardCount(CardLocation.Deck, (uint)player);
                    var handCount = GetLocationCardCount(CardLocation.Hand, (uint)player);
                    sleep = 0;
                    List<GameCard> preHands = new List<GameCard>();
                    for (var i = 0; i < count; i++)
                    {
                        card = GCS_Get(
                            new GPS
                            {
                                controller = (uint)player,
                                location = (uint)CardLocation.Deck,
                                sequence = (uint)(deckCount - 1 - i),
                            });
                        card.SetCode(r.ReadInt32() & 0x7fffffff);
                        preHands.Add(card);
                    }
                    if (player == 0)
                    {
                        needRefreshHand0 = true;
                        myPreHandCards = preHands;
                    }
                    else
                    {
                        needRefreshHand1 = true;
                        opPreHandCards = preHands;
                    }
                    for (var i = 0; i < preHands.Count; i++)
                        sleep = preHands[i].Move(
                                new GPS
                                {
                                    controller = (uint)player,
                                    location = (uint)CardLocation.Hand,
                                    sequence = (uint)(handCount + i),
                                });
                    Sleep((int)(sleep * 100));
                    break;
                case GameMessage.TagSwap:
                    player = LocalPlayer(r.ReadByte());
                    if (player == 0)
                    {
                        if (GetUI<OcgCoreUI>().TextPlayer0Name.text == name_0)
                            GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0_tag;
                        else
                            GetUI<OcgCoreUI>().TextPlayer0Name.text = name_0;
                    }
                    else
                    {
                        if (GetUI<OcgCoreUI>().TextPlayer1Name.text == name_1)
                            GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1_tag;
                        else
                            GetUI<OcgCoreUI>().TextPlayer1Name.text = name_1;
                    }
                    SetFace();

                    int mainCount = r.ReadByte();
                    int extraCount = r.ReadByte();
                    int pendulumCount = r.ReadByte();
                    int handsCount = r.ReadByte();
                    var cardsInDeck = GCS_ResizeBundle(mainCount, player, CardLocation.Deck);
                    var cardsInExtra = GCS_ResizeBundle(extraCount, player, CardLocation.Extra);
                    var cardsInHand = GCS_ResizeBundle(handsCount, player, CardLocation.Hand);
                    if (cardsInDeck.Count > 0)
                        cardsInDeck[cardsInDeck.Count - 1].SetCode(r.ReadInt32());
                    for (int i = 0; i < cardsInHand.Count; i++)
                        cardsInHand[i].SetCode(r.ReadInt32());
                    for (int i = 0; i < cardsInExtra.Count; i++)
                        cardsInExtra[i].SetCode(r.ReadInt32() & 0x7FFFFFFF);
                    for (int i = 0; i < pendulumCount; i++)
                        if (cardsInExtra.Count - 1 - i > 0)
                            cardsInExtra[cardsInExtra.Count - 1 - i].p.position = (int)CardPosition.FaceUpAttack;
                    ArrangeCards();
                    needRefreshHand0 = true;
                    needRefreshHand1 = true;
                    RefreshBgState();
                    foreach (var c in cardsInHand)
                    {
                        c.AnimationShuffle(0.15f);
                        c.EraseData();
                    }
                    break;
                case GameMessage.MatchKill:
                    cookie_matchKill = r.ReadInt32();
                    break;
                case GameMessage.PlayerHint:
                    player = LocalPlayer(r.ReadByte());
                    int ptype = r.ReadByte();
                    var pvalue = r.ReadInt32();
                    var valstring = StringHelper.Get(pvalue);
                    if (pvalue == 38723936)
                    {
                        valstring = InterString.Get("不能确认墓地中的卡");
                        if (player == 0)
                        {
                            if (ptype == 6)
                            {
                                cantCheckGrave = true;
                                GetUI<OcgCoreUI>().CardList.Hide();
                            }
                            if (ptype == 7)
                                cantCheckGrave = false;
                        }
                    }
                    if (ptype == 6)
                    {
                        if (player == 0)
                            PrintDuelLog(InterString.Get("我方状态：[?]", valstring));
                        else
                            PrintDuelLog(InterString.Get("对方状态：[?]", valstring));
                    }
                    else if (ptype == 7)
                    {
                        if (player == 0)
                            PrintDuelLog(InterString.Get("我方状态结束：[?]", valstring));
                        else
                            PrintDuelLog(InterString.Get("对方状态结束：[?]", valstring));
                    }
                    break;
                case GameMessage.CardHint:
                    card = GCS_Get(r.ReadGPS());
                    int ctype = r.ReadByte();
                    var value = r.ReadInt32();
                    if (card != null)
                    {
                        switch (ctype)
                        {
                            case 1:
                                card.RemoveStringTail(InterString.Get("数字记录："));
                                card.AddStringTail(InterString.Get("数字记录：") + value);
                                break;
                            case 2:
                                card.RemoveStringTail(InterString.Get("卡片记录："));
                                card.AddStringTail(InterString.Get("卡片记录：") + CardsManager.Get(value).Name);
                                break;
                            case 3:
                                card.RemoveStringTail(InterString.Get("种族记录："));
                                card.AddStringTail(InterString.Get("种族记录：") + StringHelper.Race(value));
                                break;
                            case 4:
                                card.RemoveStringTail(InterString.Get("属性记录："));
                                card.AddStringTail(InterString.Get("属性记录：") + StringHelper.Attribute(value));
                                break;
                            case 5:
                                card.RemoveStringTail(InterString.Get("数字记录："));
                                card.AddStringTail(InterString.Get("数字记录：") + value);
                                break;
                            case 6:
                                card.AddStringTail(StringHelper.Get(value));
                                break;
                            case 7:
                                card.RemoveStringTail(StringHelper.Get(value));
                                break;
                        }
                    }
                    break;
                case GameMessage.Hint:
                    Es_selectMSGHintType = r.ReadChar();
                    Es_selectMSGHintPlayer = LocalPlayer(r.ReadChar());
                    Es_selectMSGHintData = r.ReadInt32();
                    type = Es_selectMSGHintType;
                    player = Es_selectMSGHintPlayer;
                    data = Es_selectMSGHintData;
                    if (type == 1)
                        ES_hint = StringHelper.Get(data);
                    if (type == 2)
                        PrintDuelLog(StringHelper.Get(data));
                    if (type == 3)
                        ES_selectHint = StringHelper.Get(data);
                    if (type == 4)
                        PrintDuelLog(InterString.Get("效果选择：[?]", StringHelper.Get(data)));
                    if (type == 5)
                        PrintDuelLog(StringHelper.Get(data));
                    if (type == 6)
                        PrintDuelLog(InterString.Get("种族选择：[?]", StringHelper.Race(data)));
                    if (type == 7)
                        PrintDuelLog(InterString.Get("属性选择：[?]", StringHelper.Attribute(data)));
                    if (type == 8)
                    {
                        Program.instance.message_.CastCard(data);
                        lastDuelLog = InterString.Get("宣言卡片：[?]", CardsManager.Get(data).Name);
                    }
                    if (type == 9)
                        PrintDuelLog(InterString.Get("数字选择：[?]", data.ToString()));
                    if (type == 10)
                    {
                        Program.instance.message_.CastCard(data);
                        lastDuelLog = InterString.Get("效果适用：[?]", CardsManager.Get(data).Name);
                    }
                    if (type == 11)
                    {
                        if (player == 1)
                            data = (data >> 16) | (data << 16);
                        PrintDuelLog(InterString.Get("区域选择：[?]", StringHelper.Zone(data)));
                    }
                    ES_selectCardFromFieldFirstFlag = (type == 3 && data == 575);

                    break;
                case GameMessage.MissedEffect:
                    //TODO
                    break;
                case GameMessage.NewTurn:
                    cardsInSelection.Clear();
                    myActivated.Clear();
                    opActivated.Clear();
                    mySummonCount = 0;
                    mySpSummonCount = 0;
                    opSummonCount = 0;
                    opSpSummonCount = 0;
                    turns++;
                    myTurn = isFirst ? (turns % 2 != 0) : (turns % 2 == 0);
                    PhaseButtonHandler.TurnChange(myTurn, turns);
                    TurnChangeBanner(myTurn ? 0 : 1);
                    PhaseButtonHandler.SetTextMain("");
                    foreach (var c in cards)
                        c.ShowDisquiet();
                    foreach (var o in turnEndDeleteObjects)
                        Destroy(o);
                    turnEndDeleteObjects.Clear();

                    SetPlayableGuide(myTurn);

                    break;
                case GameMessage.NewPhase:
                    attackLine.SetActive(false);
                    Destroy(duelFinalBlow, 0.5f);

                    cardsInSelection.Clear();
                    var ph = r.ReadUInt16();
                    player = myTurn ? 0 : 1;
                    if (ph == 0x01)
                    {
                        phase = DuelPhase.Draw;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("Draw");
                    }
                    else if (ph == 0x02)
                    {
                        phase = DuelPhase.Standby;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("Standby");
                    }
                    else if (ph == 0x04)
                    {
                        phase = DuelPhase.Main1;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("Main1");
                    }
                    else if (ph == 0x08)
                    {
                        phase = DuelPhase.BattleStart;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("Battle");
                        if (myTurn && GetAllAtk(true) >= life1)
                            AudioManager.PlayBgmClimax();
                        else if (!myTurn && GetAllAtk(false) >= life0)
                            AudioManager.PlayBgmClimax();
                    }
                    else if (ph == 0x10)
                    {
                        phase = DuelPhase.BattleStep;
                        PhaseButtonHandler.SetTextBelow("01");
                    }
                    else if (ph == 0x20)
                    {
                        phase = DuelPhase.Damage;
                        PhaseButtonHandler.SetTextBelow("02");
                    }
                    else if (ph == 0x40)
                    {
                        phase = DuelPhase.DamageCal;
                        PhaseButtonHandler.SetTextBelow("03");
                    }
                    else if (ph == 0x80)
                    {
                        phase = DuelPhase.Battle;
                        PhaseButtonHandler.SetTextBelow("");
                    }
                    else if (ph == 0x100)
                    {
                        phase = DuelPhase.Main2;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("Main2");
                    }
                    else if (ph == 0x200)
                    {
                        phase = DuelPhase.End;
                        GetUI<OcgCoreUI>().ShowPhaseBanner(player, phase);
                        Sleep(100);
                        PhaseButtonHandler.SetTextMain("End");
                    }
                    break;
                case GameMessage.ConfirmDecktop:
                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    var countOfDeck = GetLocationCardCount(CardLocation.Deck, (uint)player);
                    for (int i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        gps = new GPS
                        {
                            controller = (uint)player,
                            location = (uint)CardLocation.Deck,
                            sequence = (uint)(countOfDeck - 1 - i)
                        };
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.AnimationConfirmDeckTop(i);
                        }
                    }
                    var camera = Program.instance.camera_.cameraMain.transform;
                    quence = DOTween.Sequence();
                    if (player == 0)
                        quence.Append(camera.DOLocalMove(new Vector3(0, 95, -40), 0.25f));
                    else
                        quence.Append(camera.DOLocalMove(new Vector3(0, 95, -31), 0.25f));
                    quence.Join(camera.DOLocalRotate(new Vector3(70, 0, 0), 0.25f));
                    quence.AppendInterval(count);
                    quence.Append(camera.DOLocalMove(new Vector3(0, 95, -37), 0.25f));
                    quence.Join(camera.DOLocalRotate(new Vector3(70, 0, 0), 0.25f));
                    Sleep(count * 100 + 50);
                    break;
                case GameMessage.ConfirmCards:
                    player = LocalPlayer(r.ReadByte());
                    if(condition != Condition.Replay || CurrentReplayUseYRP2)
                        r.ReadByte();
                    count = r.ReadByte();
                    var listShow = false;
                    if (count > 3 && condition == Condition.Duel)
                        listShow = true;
                    var confirmCards = new List<GameCard>();
                    for (int i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            if (listShow)
                                confirmCards.Add(card);
                            else
                                card.AnimationConfirm(i);
                        }
                    }
                    if (listShow)
                    {
                        messagePass = false;
                        GetUI<OcgCoreUI>().ShowPopupSelectCard
                            (InterString.Get("确认卡片：[?]张。", count.ToString()), confirmCards, 0, 0, true, true);
                    }
                    else
                        Sleep(100 * count + 10);
                    break;
                case GameMessage.DeckTop:
                    player = LocalPlayer(r.ReadByte());
                    countOfDeck = GetLocationCardCount(CardLocation.Deck, (uint)player);
                    gps = new GPS
                    {
                        controller = (uint)player,
                        location = (uint)CardLocation.Deck,
                        sequence = (uint)(countOfDeck - 1 - r.ReadByte())
                    };
                    code = r.ReadInt32();
                    card = GCS_Get(gps);
                    if (card != null)
                    {
                        card.SetCode(code);
                        PrintDuelLog(InterString.Get("确认卡片：[?]", CardsManager.Get(code).Name));
                    }
                    break;
                case GameMessage.RefreshDeck:
                case GameMessage.ShuffleDeck:
                    player = LocalPlayer(r.ReadByte());
                    if (GetLocationCardCount(CardLocation.Deck, (uint)player) > 0)
                    {
                        for (var i = 0; i < cards.Count; i++)
                            if (cards[i].gameObject.activeInHierarchy)
                                if ((cards[i].p.location & (uint)CardLocation.Deck) > 0)
                                    if (cards[i].p.controller == player)
                                        cards[i].EraseData();
                        Animator animator;
                        if (player == 0)
                            animator = myDeck.GetElement<Animator>("CardShuffleTop");
                        else
                            animator = opDeck.GetElement<Animator>("CardShuffleTop");
                        animator.speed = 2;
                        animator.SetTrigger("Shuffle");
                        Tools.ChangeLayer(animator.gameObject, "DuelOverlay3D");
                        CameraManager.DuelOverlay3DPlus();
                        DOTween.To(v => { }, 0, 0, 0.5f).OnComplete(() =>
                        {
                            animator.SetTrigger("Idle");
                            Tools.ChangeLayer(animator.gameObject, "Default");
                            CameraManager.DuelOverlay3DMinus();
                        });
                        Program.instance.audio_.PlayShuffleSE();
                        Sleep(50);
                    }
                    break;
                case GameMessage.ShuffleHand:
                    player = LocalPlayer(r.ReadByte());
                    for (var i = 0; i < cards.Count; i++)
                        if (cards[i].gameObject.activeInHierarchy)
                            if ((cards[i].p.location & (uint)CardLocation.Hand) > 0)
                                if (cards[i].p.controller == player)
                                {
                                    cards[i].AnimationShuffle(0.15f);
                                    cards[i].EraseData();
                                }
                    Program.instance.audio_.PlayShuffleSE();
                    Sleep(30);
                    messagePass = false;
                    break;
                case GameMessage.SwapGraveDeck:
                    player = LocalPlayer(r.ReadByte());
                    foreach (var c in cards)
                    {
                        if (c.p.controller == player)
                        {
                            if ((c.p.location & (uint)CardLocation.Deck) > 0)
                                c.p.location = (uint)CardLocation.Grave;
                            else if ((c.p.location & (uint)CardLocation.Grave) > 0)
                            {
                                if (c.GetData().IsExtraCard())
                                    c.p.location = (uint)CardLocation.Extra;
                                else
                                    c.p.location = (uint)CardLocation.Deck;
                            }
                        }
                    }

                    break;
                case GameMessage.ShuffleSetCard:
                    messagePass = false;
                    location = r.ReadByte();
                    count = r.ReadByte();
                    var gpss = new List<GPS>();
                    var cardsToShuffle = new List<GameCard>();
                    for (int i = 0; i < count; i++)
                    {
                        gps = r.ReadGPS();
                        gpss.Add(gps);
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.EraseData();
                            cardsToShuffle.Add(card);
                        }
                    }
                    for (int i = 0; i < count; i++)
                    {
                        var targetCard = cardsToShuffle[i];
                        var newGPS = r.ReadGPS();
                        var oldGPS = targetCard.p;
                        oldGPS.reason = 0;
                        targetCard.model.transform.DOLocalMove(new Vector3(0f, 5f, targetCard.p.controller == 0 ? -12 : 12), 0.2f).OnComplete(() =>
                        {
                            targetCard.Move(newGPS.location > 0 ? newGPS : oldGPS);
                        });
                    }
                    Sleep(40);
                    break;
                case GameMessage.ReverseDeck:
                    deckReserved = !deckReserved;
                    break;
                case GameMessage.FieldDisabled:
                    var disabledField = r.ReadUInt32();
                    foreach (var place in places)
                        place.SetDisabled(disabledField);
                    break;
                case GameMessage.CardSelected:
                    MessageManager.Cast("CardSelected");
                    break;
                case GameMessage.RandomSelected:
                    int targetTime = 0;
                    GetUI<OcgCoreUI>().DuelLog.psum = false;
                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        gps = r.ReadGPS();
                        card = GCS_Get(gps);
                        if (card == null)
                            continue;
                        cardsBeTarget.Add(card);
                        targetTime += 50;
                        card.AnimationTarget();
                    }
                    Sleep(targetTime);
                    break;
                case GameMessage.BecomeTarget:
                    targetTime = 0;
                    GetUI<OcgCoreUI>().DuelLog.psum = false;
                    count = r.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        gps = r.ReadGPS();
                        card = GCS_Get(gps);

                        if (card != null)
                        {
                            if (cardsInChain.Count > 0)
                                cardsInChain[^1].AddEffectTarget(card);
                            if (cardsInChain.Count == 0
                                && card.InPendulumZone())
                                targetTime += 0;
                            else
                            {
                                targetTime += 50;
                                card.AnimationTarget();
                            }
                            cardsBeTarget.Add(card);
                        }
                        if (phase == DuelPhase.Main1 || phase == DuelPhase.Main2)
                            if (cardsInChain.Count == 0)
                                if (cardsBeTarget.Count == 2)
                                    if (cardsBeTarget[0].InPendulumZone())
                                        if (cardsBeTarget[1].InPendulumZone())
                                            if (cardsBeTarget[0].p.controller == cardsBeTarget[1].p.controller)
                                                GetUI<OcgCoreUI>().DuelLog.psum = true;

                        config = true;
                        if (GetUI<OcgCoreUI>().DuelLog.psum)
                        {
                            switch (condition)
                            {
                                case Condition.Duel:
                                    if (Config.Get("DuelPendulum", "1") == "0")
                                        config = false;
                                    break;
                                case Condition.Watch:
                                    if (Config.Get("WatchPendulum", "1") == "0")
                                        config = false;
                                    break;
                                case Condition.Replay:
                                    if (Config.Get("ReplayPendulum", "1") == "0")
                                        config = false;
                                    break;
                            }
                        }

                        if (GetUI<OcgCoreUI>().DuelLog.psum && config)
                        {
                            GetUI<OcgCoreUI>().CardDescription.Hide();
                            targetTime = 366;
                            GameObject pendulum = ABLoader.LoadFromFolder("MasterDuel/Timeline/Summon/SummonPendulum/SummonPendulum01", "SummonPendulum", true);
                            ElementObjectManager manager = null;
                            for (int j = 0; j < pendulum.transform.childCount; j++)
                            {
                                if (pendulum.transform.GetChild(j).GetComponent<ElementObjectManager>() != null)
                                    manager = pendulum.transform.GetChild(j).GetComponent<ElementObjectManager>();
                                else
                                    Destroy(pendulum.transform.GetChild(j).gameObject);
                            }
                            manager = manager.GetElement<ElementObjectManager>("SummonPendulumShowCard");
                            pendulum.transform.SetParent(Program.instance.container_3D, false);
                            Tools.ChangeLayer(pendulum, "DuelOverlay3D");

                            var card1 = manager.GetElement<ElementObjectManager>("DummyCard01");
                            var card2 = manager.GetElement<ElementObjectManager>("DummyCard02");
                            var i1 = Program.instance.texture_.LoadDummyCard(card1, cardsBeTarget[0].GetData().Id, cardsBeTarget[0].p.controller);
                            StartCoroutine(i1);
                            var i2 = Program.instance.texture_.LoadDummyCard(card2, cardsBeTarget[1].GetData().Id, cardsBeTarget[1].p.controller);
                            StartCoroutine(i2);
                            var scale1 = cardsBeTarget[0].GetData().LScale;
                            var scale2 = cardsBeTarget[1].GetData().RScale;
                            if (scale1 < 10)
                            {
                                Destroy(manager.GetElement("LPendulumNum00Ones"));
                                Destroy(manager.GetElement("LPendulumNum00Tens"));
                                Destroy(manager.GetElement("LPendulumNum00OnesA"));
                                Destroy(manager.GetElement("LPendulumNum00TensA"));
                                var handle = Addressables.LoadAssetAsync<Texture>("LPendulumNum0" + scale1);
                                handle.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("LPendulumNum00Digit").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("LPendulumNum00DigitA").material.mainTexture = result.Result;
                                };
                            }
                            else
                            {
                                Destroy(manager.GetElement("LPendulumNum00Digit"));
                                Destroy(manager.GetElement("LPendulumNum00DigitA"));
                                var handle = Addressables.LoadAssetAsync<Texture>("LPendulumNum01");
                                handle.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("LPendulumNum00Tens").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("LPendulumNum00TensA").material.mainTexture = result.Result;
                                };
                                var handle2 = Addressables.LoadAssetAsync<Texture>("LPendulumNum0" + (scale1 - 10));
                                handle2.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("LPendulumNum00Ones").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("LPendulumNum00OnesA").material.mainTexture = result.Result;
                                };
                            }
                            if (scale2 < 10)
                            {
                                Destroy(manager.GetElement("RPendulumNum00Ones"));
                                Destroy(manager.GetElement("RPendulumNum00Tens"));
                                Destroy(manager.GetElement("RPendulumNum00OnesA"));
                                Destroy(manager.GetElement("RPendulumNum00TensA"));
                                var handle = Addressables.LoadAssetAsync<Texture>("RPendulumNum0" + scale2);
                                handle.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("RPendulumNum00Digit").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("RPendulumNum00DigitA").material.mainTexture = result.Result;
                                };
                            }
                            else
                            {
                                Destroy(manager.GetElement("RPendulumNum00Digit"));
                                Destroy(manager.GetElement("RPendulumNum00DigitA"));
                                var handle = Addressables.LoadAssetAsync<Texture>("RPendulumNum01");
                                handle.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("RPendulumNum00Tens").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("RPendulumNum00TensA").material.mainTexture = result.Result;
                                };
                                var handle2 = Addressables.LoadAssetAsync<Texture>("RPendulumNum0" + (scale2 - 10));
                                handle2.Completed += (result) =>
                                {
                                    manager.GetElement<MeshRenderer>("RPendulumNum00Ones").material.mainTexture = result.Result;
                                    manager.GetElement<MeshRenderer>("RPendulumNum00OnesA").material.mainTexture = result.Result;
                                };
                            }
                            Destroy(pendulum, 4f);

                            if (MasterRule >= 4)
                            {
                                var pendulumSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/Summon/SummonPendulum/SummonPendulumScaleSet", "PendulumSet", true);
                                pendulumSet.transform.SetParent(Program.instance.container_3D);
                                ElementObjectManager setManager = null;
                                for (int j = 0; j < pendulumSet.transform.childCount; j++)
                                {
                                    if (pendulumSet.transform.GetChild(j).GetComponent<ElementObjectManager>() != null)
                                        setManager = pendulumSet.transform.GetChild(j).GetComponent<ElementObjectManager>();
                                    else
                                        Destroy(pendulumSet.transform.GetChild(j).gameObject);
                                }
                                var dummy1 = setManager.transform.GetChild(0).GetComponent<ElementObjectManager>();
                                var dummy2 = setManager.transform.GetChild(1).GetComponent<ElementObjectManager>();
                                var ie = Program.instance.texture_.LoadDummyCard(dummy1, cardsBeTarget[0].GetData().Id, cardsBeTarget[0].p.controller, true);
                                StartCoroutine(ie);
                                ie = Program.instance.texture_.LoadDummyCard(dummy2, cardsBeTarget[1].GetData().Id, cardsBeTarget[1].p.controller, true);
                                StartCoroutine(ie);
                                if (cardsBeTarget[0].p.controller != 0)
                                    setManager.transform.localEulerAngles = new Vector3(0, 180, 0);
                                Destroy(pendulumSet, 4f);
                            }
                        }
                    }
                    Sleep(targetTime);
                    break;
                case GameMessage.CardTarget:
                    from = r.ReadGPS();
                    to = r.ReadGPS();
                    var cardFrom = GCS_Get(from);
                    var cardTo = GCS_Get(to);
                    if (cardFrom != null && cardTo != null)
                        cardFrom.AddTarget(cardTo);
                    break;
                case GameMessage.Equip:
                    from = r.ReadGPS();
                    to = r.ReadGPS();
                    cardFrom = GCS_Get(from);
                    cardTo = GCS_Get(to);
                    if (cardFrom != null && cardTo != null)
                        cardFrom.equipedCard = cardTo;
                    break;
                case GameMessage.CancelTarget:
                    from = r.ReadGPS();
                    card = GCS_Get(from);
                    if (card != null)
                        card.targets.Clear();
                    break;
                case GameMessage.Unequip:
                    from = r.ReadGPS();
                    card = GCS_Get(from);
                    if (card != null)
                        card.equipedCard = null;
                    break;
                case GameMessage.AddCounter:
                    type = r.ReadUInt16();
                    gps = r.ReadShortGPS();
                    card = GCS_Get(gps);
                    count = r.ReadUInt16();
                    if (card != null)
                        card.AddCounter(type, count);
                    break;
                case GameMessage.RemoveCounter:
                    type = r.ReadUInt16();
                    gps = r.ReadShortGPS();
                    card = GCS_Get(gps);
                    count = r.ReadUInt16();
                    if (card != null)
                        card.RemoveCounter(type, count);
                    break;
                case GameMessage.Waiting:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(false);
                    break;
                case GameMessage.RequestDeck:
                    break;

                case GameMessage.AnnounceRace:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    ES_min = r.ReadByte();
                    available = r.ReadUInt32();
                    selections = new List<string>() { InterString.Get("宣言种族") };
                    var responses = new List<int>();
                    for (int i = 0; i < (uint)CardRace.Count; i++)
                    {
                        if ((available & (1 << i)) > 0)
                        {
                            selections.Add(StringHelper.GetUnsafe(1020 + i));
                            responses.Add(1 << i);
                        }
                    }
                    GetUI<OcgCoreUI>().ShowPopupSelection(selections, responses);
                    break;
                case GameMessage.AnnounceAttrib:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    ES_min = r.ReadByte();
                    available = r.ReadUInt32();
                    selections = new List<string>() { InterString.Get("宣言属性") };
                    responses = new List<int>();
                    for (int i = 0; i < (uint)CardAttribute.Count; i++)
                    {
                        if ((available & (1 << i)) > 0)
                        {
                            selections.Add(StringHelper.GetUnsafe(1010 + i));
                            responses.Add(1 << i);
                        }
                    }
                    GetUI<OcgCoreUI>().ShowPopupSelection(selections, responses);
                    break;
                case GameMessage.AnnounceNumber:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    ES_min = 1;
                    selections = new List<string>() { InterString.Get("宣言数字") };
                    responses = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        selections.Add(r.ReadUInt32().ToString());
                        responses.Add(i);
                    }
                    GetUI<OcgCoreUI>().ShowPopupSelection(selections, responses);
                    break;
                case GameMessage.AnnounceCard:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    ES_searchCodes.Clear();
                    count = r.ReadByte();
                    for (int i = 0; i < count; i++)
                        ES_searchCodes.Add(r.ReadInt32());
                    selections = new List<string>()
                    {
                        InterString.Get("请输入关键字："),
                        InterString.Get("搜索"),
                        string.Empty,
                        string.Empty
                    };
                    GetUI<OcgCoreUI>().ShowPopupInput(selections, OnAnnounceCard, null);
                    break;
                case GameMessage.SelectIdleCmd:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadChar());
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.AddButton((i << 16) + 0, InterString.Get("召唤"), ButtonType.Summon);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            if ((card.p.location & (uint)CardLocation.SpellZone) > 0
                                && card.GetData().HasType(CardType.Pendulum)
                                )
                                card.AddButton((i << 16) + 1, InterString.Get("灵摆召唤"), ButtonType.PenSummon);
                            else
                                card.AddButton((i << 16) + 1, InterString.Get("特殊召唤"), ButtonType.SpSummon);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            if ((card.p.position & (uint)CardPosition.Defence) > 0)
                                card.AddButton((i << 16) + 2, InterString.Get("变为攻击表示"), ButtonType.ToAttackPosition);
                            else
                                card.AddButton((i << 16) + 2, InterString.Get("变为守备表示"), ButtonType.ToDefensePosition);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.AddButton((i << 16) + 3, InterString.Get("设置"), ButtonType.SetMonster);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.AddButton((i << 16) + 4, InterString.Get("设置"), ButtonType.SetSpell);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        var descP = r.ReadInt32();
                        desc = StringHelper.Get(descP);
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            if (descP == 1160)
                                card.AddButton((i << 16) + 5, InterString.Get("灵摆发动"), ButtonType.SetPendulum);
                            else
                            {
                                var eff = new Effect();
                                eff.ptr = (i << 16) + 5;
                                eff.desc = desc;
                                card.effects.Add(eff);
                                card.AddButton((i << 16) + 5, InterString.Get("发动效果"), ButtonType.Activate);
                            }
                        }
                    }
                    foreach (var c in cards)
                        c.CreateButtons();
                    int buttonsCount = 0;
                    foreach (var c in cards)
                        buttonsCount += c.buttons.Count;
                    if (buttonsCount == 0)
                        PhaseButtonHandler.SetHint();

                    var bp = r.ReadByte();
                    var ep = r.ReadByte();
                    var shuffle = r.ReadByte();
                    if (bp == 1)
                        PhaseButtonHandler.battlePhase = true;
                    if (ep == 1)
                        PhaseButtonHandler.endPhase = true;

                    ShowBgHint();

                    break;
                case GameMessage.SelectBattleCmd:
                    attackLine.SetActive(false);
                    Destroy(duelFinalBlow, 0.5f);

                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadChar());
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        desc = StringHelper.Get(r.ReadInt32());
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            var eff = new Effect();
                            eff.ptr = (i << 16) + 0;
                            eff.desc = desc;
                            card.effects.Add(eff);
                            card.AddButton((i << 16) + 0, InterString.Get("发动效果"), ButtonType.Activate);
                        }
                    }
                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        r.ReadByte();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.AddButton((i << 16) + 1, InterString.Get("攻击"), ButtonType.Battle);
                        }
                    }
                    foreach (var c in cards)
                        c.CreateButtons();
                    buttonsCount = 0;
                    foreach (var c in cards)
                        buttonsCount += c.buttons.Count;
                    if (buttonsCount == 0)
                        PhaseButtonHandler.SetHint();

                    var mp2 = r.ReadByte();
                    ep = r.ReadByte();
                    if (mp2 == 1)
                        PhaseButtonHandler.main2Phase = true;
                    if (ep == 1)
                        PhaseButtonHandler.endPhase = true;

                    ShowBgHint();

                    break;
                case GameMessage.SelectYesNo:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    desc = StringHelper.Get(r.ReadInt32());
                    var title = InterString.Get("选择");
                    //if(cardsInChain.Count > 0)
                    //{
                    //    title = StringHelper.Get(95);//是否使用[%ls]的效果？
                    //    var forReplaceFirst = new Regex("\\[%ls\\]");
                    //    title = forReplaceFirst.Replace(title, "「" + cardsInChain[currentChainNumber - 1].GetData().Name + "」", 1);
                    //}
                    selections = new List<string>
                    {
                        title,
                        desc,
                        InterString.Get("是"),
                        InterString.Get("否")
                    };
                    Action yes = () =>
                    {
                        var binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(1);
                        SendReturn(binaryMaster.Get());
                    };
                    Action no = () =>
                    {
                        var binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(0);
                        SendReturn(binaryMaster.Get());
                    };
                    GetUI<OcgCoreUI>().ShowPopupYesOrNo(selections, yes, no);

                    break;
                case GameMessage.SelectEffectYn:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    code = r.ReadInt32();
                    gps = r.ReadShortGPS();
                    r.ReadByte();
                    var cr = r.ReadInt32();
                    card = GCS_Get(gps);
                    if (card != null)
                    {
                        var displayname = "「" + card.GetData().Name + "」";
                        var forReplaceFirst = new Regex("\\[%ls\\]");
                        if (cr == 0)
                        {
                            desc = StringHelper.Get(200);//是否在[%ls]发动[%ls]的效果？
                            desc = forReplaceFirst.Replace(desc, StringHelper.FormatLocation(gps), 1);
                            desc = ES_hint + "，" + forReplaceFirst.Replace(desc, displayname, 1);
                        }
                        else if (cr == 221)
                        {
                            desc = StringHelper.Get(221);//是否在[%ls]发动[%ls]的诱发类效果？
                            desc = forReplaceFirst.Replace(desc, StringHelper.FormatLocation(gps), 1);
                            desc = forReplaceFirst.Replace(desc, displayname, 1);
                            desc = ES_hint + "，" + desc + "\n" + StringHelper.Get(223);//稍后将询问其他可以发动的效果。
                        }
                        else
                        {
                            desc = StringHelper.Get(cr);
                            desc = forReplaceFirst.Replace(desc, displayname, 1);
                        }

                        List<GameCard> oneCardToSend = new List<GameCard>() { card };
                        GetUI<OcgCoreUI>().ShowPopupSelectCard(desc, oneCardToSend, 1, 1, true, false);
                    }
                    break;
                case GameMessage.SelectChain:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadChar());
                    count = r.ReadByte();
                    int spcount = r.ReadByte();
                    var hint0 = r.ReadInt32();
                    var hint1 = r.ReadInt32();
                    chainCards = new List<GameCard>();
                    var forceCount = 0;
                    for (var i = 0; i < count; i++)
                    {
                        var flag = r.ReadChar();
                        var forced = r.ReadByte();
                        forceCount += forced;
                        code = r.ReadInt32() % 1000000000;
                        gps = r.ReadGPS();
                        desc = StringHelper.Get(r.ReadInt32());
                        card = GCS_Get(gps);
                        if (card == null)
                            card = GCS_Create(gps, true);

                        if (!chainCards.Contains(card))
                            chainCards.Add(card);
                        card.SetCode(code);
                        var eff = new Effect();
                        eff.flag = flag;
                        eff.ptr = i;
                        eff.desc = desc;
                        eff.forced = forced > 0;
                        card.effects.Add(eff);
                    }

                    var handleFlag = 0;

                    // 无强制发动的卡
                    if (forceCount == 0)
                    {
                        // 无关键卡
                        if (spcount == 0)
                        {
                            if (chainCondition == ChainCondition.No)
                            {
                                handleFlag = 0;
                            }
                            else if (chainCondition == ChainCondition.All)
                            {
                                if (chainCards.Count == 0)
                                {
                                    handleFlag = -1;
                                }
                                else
                                {
                                    if (chainCards.Count == 1 && chainCards[0].effects.Count == 1)
                                        handleFlag = 1;
                                    else
                                        handleFlag = 2;
                                }
                            }
                            else if (chainCondition == ChainCondition.Smart)
                            {
                                handleFlag = 0;
                            }
                        }
                        // 有关键卡
                        else
                        {
                            if (chainCards.Count == 0)
                            {
                                handleFlag = 0;
                                if (chainCondition == ChainCondition.All)
                                    handleFlag = -1;
                            }
                            else if (chainCondition == ChainCondition.No)
                            {
                                handleFlag = 0;
                            }
                            else
                            {
                                if (chainCards.Count == 1 && chainCards[0].effects.Count == 1)
                                    handleFlag = 1;
                                else
                                    handleFlag = 2;
                            }
                        }
                    }
                    // 有强制发动的卡
                    else
                    {
                        if (chainCards.Count == 1 && chainCards[0].effects.Count == 1)
                        {
                            handleFlag = 3;
                        }
                        else
                        {
                            handleFlag = 4;
                        }
                    }

                    //Debug.Log($"ChainCondition: {chainCondition} spcount: {spcount} handleFlag: {handleFlag}");

                    // handleFlag
                    // -1   无卡 需要提醒
                    // 0    无卡 直接回应
                    // 1    一张卡需要处理
                    // 2    多张卡需要处理
                    // 3    一张卡需要处理（强制发动）
                    // 4    多张卡需要处理（强制发动）

                    switch (handleFlag)
                    {
                        case 1:
                        case 2:
                            GetUI<OcgCoreUI>().ShowPopupSelectCard(InterString.Get("[?]，是否连锁？", ES_hint), chainCards, 1, 1, true, false);
                            break;
                        case 3:
                        case 4:
                            GetUI<OcgCoreUI>().ShowPopupSelectCard(InterString.Get("[?]，请选择效果发动。", ES_hint), chainCards, 1, 1, false, false);
                            break;
                        default:
                            OnResend();
                            break;
                    }

                    break;
                case GameMessage.SelectCard:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    cancelable = r.ReadByte() != 0;
                    ES_min = r.ReadByte();
                    ES_max = r.ReadByte();
                    ES_level = 0;
                    count = r.ReadByte();
                    cardsInSelection.Clear();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.selectPtr = i;
                            cardsInSelection.Add(card);
                        }
                    }
                    if (ES_selectCardFromFieldFirstFlag && cancelable)
                    {
                        ES_selectCardFromFieldFirstFlag = false;
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(-1);
                        SendReturn(binaryMaster.Get());
                        break;
                    }

                    if (ES_min == 1 && count == 1)
                    {
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write((byte)count);
                        foreach (var c in cardsInSelection)
                        {
                            lastSelectedCard = c.GetData().Id;
                            binaryMaster.writer.Write(c.selectPtr);
                        }
                        SendReturn(binaryMaster.Get());
                        break;
                    }
                    bool allOnfield = true;
                    foreach (var c in cardsInSelection)
                        if ((c.p.location & (uint)CardLocation.Onfield) == 0 || (c.p.location & (uint)CardLocation.Overlay) > 0)
                        {
                            allOnfield = false;
                            break;
                        }
                    if (allOnfield)
                        FieldSelect(ES_selectHint, cardsInSelection, ES_min, ES_max, cancelable, false);
                    else
                        GetUI<OcgCoreUI>().ShowPopupSelectCard(ES_selectHint, cardsInSelection, ES_min, ES_max, cancelable, false);
                    break;
                case GameMessage.SelectUnselect:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    var finishable = r.ReadByte() != 0;
                    cancelable = r.ReadByte() != 0 || finishable;
                    ES_min = r.ReadByte();
                    ES_max = r.ReadByte();
                    ES_level = 0;
                    count = r.ReadByte();
                    cardsInSelection.Clear();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.selectPtr = i;
                            cardsInSelection.Add(card);
                        }
                    }
                    allOnfield = true;
                    foreach (var c in cardsInSelection)
                        if ((c.p.location & (uint)CardLocation.Onfield) == 0 || (c.p.location & (uint)CardLocation.Overlay) > 0)
                        {
                            allOnfield = false;
                            break;
                        }
                    if (!string.IsNullOrEmpty(ES_selectHint))
                        ES_selectUnselectHint = ES_selectHint;
                    if (string.IsNullOrEmpty(ES_selectUnselectHint))
                        ES_selectUnselectHint = InterString.Get("请选择卡片");

                    if (allOnfield)
                        FieldSelect(ES_selectUnselectHint, cardsInSelection, 1, 1, cancelable, finishable);
                    else
                        GetUI<OcgCoreUI>().ShowPopupSelectCard(ES_selectUnselectHint, cardsInSelection, 1, 1, cancelable, finishable);
                    break;
                case GameMessage.SelectSum:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    ES_overFlow = r.ReadByte() != 0;
                    player = LocalPlayer(r.ReadByte());
                    ES_level = r.ReadInt32();
                    ES_min = r.ReadByte();
                    ES_max = r.ReadByte();
                    if (ES_min < 1) ES_min = 1;
                    if (ES_max < 1) ES_max = 99;
                    cardsInSelection.Clear();
                    cardsMustBeSelected.Clear();

                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        var para = r.ReadInt32();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.selectPtr = i;
                            card.levelForSelect_1 = para & 0xffff;
                            card.levelForSelect_2 = para >> 16;
                            if ((para & 0x80000000) > 0)
                            {
                                card.levelForSelect_1 = para & 0x7fffffff;
                                card.levelForSelect_2 = card.levelForSelect_1;
                            }
                            if (card.levelForSelect_2 == 0)
                                card.levelForSelect_2 = card.levelForSelect_1;
                            cardsInSelection.Add(card);
                            cardsMustBeSelected.Add(card);
                        }
                    }

                    bool sendable = false;
                    var level = 0;
                    foreach (var c in cardsMustBeSelected)
                        level += c.levelForSelect_1;
                    if (level == ES_level)
                        sendable = true;
                    if (!sendable)
                    {
                        level = 0;
                        foreach (var c in cardsMustBeSelected)
                            level += c.levelForSelect_2;
                        if (level == ES_level)
                            sendable = true;
                    }
                    if (sendable)
                    {
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(cardsMustBeSelected.Count);
                        for (var i = 0; i < cardsMustBeSelected.Count; i++)
                            binaryMaster.writer.Write(i);
                        SendReturn(binaryMaster.Get());
                        break;
                    }

                    count = r.ReadByte();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        var para = r.ReadInt32();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.selectPtr = i;
                            card.levelForSelect_1 = para & 0xffff;
                            card.levelForSelect_2 = para >> 16;
                            if ((para & 0x80000000) > 0)
                            {
                                card.levelForSelect_1 = para & 0x7fffffff;
                                card.levelForSelect_2 = card.levelForSelect_1;
                            }
                            if (card.levelForSelect_2 == 0)
                                card.levelForSelect_2 = card.levelForSelect_1;
                            cardsInSelection.Add(card);
                        }
                    }

                    level = 0;
                    foreach (var c in cardsInSelection)
                        level += c.levelForSelect_1;
                    if (level == ES_level)
                        sendable = true;
                    if (!sendable)
                    {
                        level = 0;
                        foreach (var c in cardsInSelection)
                            level += c.levelForSelect_2;
                        if (level == ES_level)
                            sendable = true;
                    }
                    if (sendable)
                    {
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write((byte)cardsInSelection.Count);
                        foreach (var c in cardsMustBeSelected)
                            binaryMaster.writer.Write((byte)c.selectPtr);
                        foreach (var c in cardsInSelection)
                            if (!cardsMustBeSelected.Contains(c))
                                binaryMaster.writer.Write((byte)c.selectPtr);
                        SendReturn(binaryMaster.Get());
                        break;
                    }

                    allOnfield = true;
                    foreach (var c in cardsInSelection)
                        if ((c.p.location & (uint)CardLocation.Onfield) == 0 || (c.p.location & (uint)CardLocation.Overlay) > 0)
                        {
                            allOnfield = false;
                            break;
                        }
                    if (allOnfield)
                        FieldSelect(ES_selectHint, cardsInSelection, ES_min, ES_max, false, false);
                    else
                        GetUI<OcgCoreUI>().ShowPopupSelectCard(ES_selectHint, cardsInSelection, ES_min, ES_max, false, false);
                    break;
                case GameMessage.SelectTribute:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    cancelable = r.ReadByte() != 0;
                    ES_min = r.ReadByte();
                    ES_max = r.ReadByte();
                    ES_level = 0;
                    count = r.ReadByte();
                    cardsInSelection.Clear();
                    for (var i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.selectPtr = i;
                            int para = r.ReadByte();
                            card.levelForSelect_1 = para;
                            card.levelForSelect_2 = para;
                            cardsInSelection.Add(card);
                        }
                    }
                    allOnfield = true;
                    foreach (var c in cardsInSelection)
                        if ((c.p.location & (uint)CardLocation.Onfield) == 0 || (c.p.location & (uint)CardLocation.Overlay) > 0)
                        {
                            allOnfield = false;
                            break;
                        }
                    if (allOnfield)
                        FieldSelect(ES_selectHint, cardsInSelection, ES_min, ES_max, cancelable, false);
                    else
                        GetUI<OcgCoreUI>().ShowPopupSelectCard(ES_selectHint, cardsInSelection, ES_min, ES_max, cancelable, false);
                    break;
                case GameMessage.SelectOption:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    count = r.ReadByte();
                    if (count > 1)
                    {
                        selections = new List<string>() { InterString.Get("效果选择") };
                        responses = new List<int> { };
                        for (var i = 0; i < count; i++)
                        {
                            desc = StringHelper.Get(r.ReadInt32());
                            selections.Add(desc);
                            responses.Add(i);
                        }
                        GetUI<OcgCoreUI>().ShowPopupSelection(selections, responses);
                    }
                    else
                    {
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(0);
                        SendReturn(binaryMaster.Get());
                    }
                    break;
                case GameMessage.SelectPlace:
                case GameMessage.SelectDisfield:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = r.ReadByte();
                    min = r.ReadByte();
                    cancelable = false;
                    if (min == 0)
                    {
                        cancelable = true;
                        min = 1;
                    }
                    ES_min = min;
                    var filter = ~r.ReadUInt32();
                    foreach (var place in places)
                        place.HighlightThisZone(filter, min);

                    if (currentMessage == GameMessage.SelectPlace)
                    {
                        if (Es_selectMSGHintType == 3)
                        {
                            if (Es_selectMSGHintPlayer == 0)
                                ES_selectHint = InterString.Get("请为我方的「[?]」选择位置。", CardsManager.Get(Es_selectMSGHintData).Name);
                            else
                                ES_selectHint = InterString.Get("请为对方的「[?]」选择位置。", CardsManager.Get(Es_selectMSGHintData).Name);
                        }
                    }
                    else if (ES_selectHint == "")
                        ES_selectHint = StringHelper.GetUnsafe(570);//请选择要变成不能使用的卡片区域
                    GetUI<OcgCoreUI>().SetHint(ES_selectHint);
                    break;
                case GameMessage.SelectPosition:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    code = r.ReadInt32();
                    int positions = r.ReadByte();
                    var op1 = 0x1;
                    var op2 = 0x4;
                    if (positions == 0x1 || positions == 0x2 || positions == 0x4 || positions == 0x8)
                    {
                        binaryMaster = new BinaryMaster();
                        binaryMaster.writer.Write(positions);
                        SendReturn(binaryMaster.Get());
                    }
                    else if (positions == (0x1 | 0x4 | 0x8))
                    {
                        GetUI<OcgCoreUI>().ShowPopupPosition(code, 3);
                    }
                    else
                    {
                        if ((positions & 0x1) > 0)
                            op1 = 0x1;
                        if ((positions & 0x2) > 0)
                            op1 = 0x2;
                        if ((positions & 0x4) > 0)
                            op2 = 0x4;
                        if ((positions & 0x8) > 0)
                        {
                            if ((positions & 0x4) > 0)
                                op1 = 0x4;
                            op2 = 0x8;
                        }
                        GetUI<OcgCoreUI>().ShowPopupPosition(code, 2, op1, op2);
                    }
                    break;
                case GameMessage.SelectCounter:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    var version1033b = (length_of_message - 5) % 8 == 0;
                    player = LocalPlayer(r.ReadByte());
                    r.ReadInt16();
                    if (version1033b)
                        ES_min = r.ReadByte();
                    else
                        ES_min = r.ReadUInt16();
                    count = r.ReadByte();
                    cardsInSelection.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        var pew = 0;
                        if (version1033b)
                            pew = r.ReadByte();
                        else
                            pew = r.ReadUInt16();
                        if (card != null)
                        {
                            card.SetCode(code);
                            card.counterCanCount = pew;
                            card.counterSelected = 0;
                            card.selectPtr = i;
                            cardsInSelection.Add(card);
                        }
                    }
                    FieldSelect(InterString.Get("请取除指示物"), cardsInSelection, ES_min, ES_min, true, false);
                    break;
                case GameMessage.RockPaperScissors:
                    //TODO
                    break;
                case GameMessage.CustomMsg:
                    break;
                case GameMessage.DuelWinner:
                    break;
                case GameMessage.SortCard:
                case GameMessage.SortChain:
                    if (InIgnoranceReplay()) break;
                    SetPlayableGuide(true);

                    player = LocalPlayer(r.ReadByte());
                    ES_sortSum = 0;
                    count = r.ReadByte();
                    List<GameCard> sortingCards = new List<GameCard>();
                    for (int i = 0; i < count; i++)
                    {
                        code = r.ReadInt32();
                        gps = r.ReadShortGPS();
                        card = GCS_Get(gps);
                        if (card != null)
                        {
                            card.SetCode(code);
                            if (!sortingCards.Contains(card))
                                sortingCards.Add(card);
                            ES_sortSum++;
                        }
                    }
                    GetUI<OcgCoreUI>().ShowPopupSelectCard(InterString.Get("请为卡片排序。"), sortingCards, sortingCards.Count, sortingCards.Count, false, false);
                    break;
            }
        }
        public void Chat(int player, string content)
        {
            if (!GetMessageConfig(player))
                return;
            if (player == 7 || player < 4)
                MessageManager.Cast(ChatPanel.GetPlayerName(player) + ": " + content);
            else
                MessageManager.Cast(content);
        }

        private bool preload;
        private void Preload()
        {
            preload = true;
            GetUI<OcgCoreUI>().TextPlayer0LP.text = string.Empty;
            GetUI<OcgCoreUI>().TextPlayer1LP.text = string.Empty;
            for (int i = 0; i < packages.Count; i++)
            {
                if ((GameMessage)packages[i].Function == GameMessage.Start
                    || (GameMessage)packages[i].Function == GameMessage.AiName
                    || (GameMessage)packages[i].Function == GameMessage.sibyl_name
                    )
                {
                    PracticalizeMessage(packages[i]);
                    break;
                }
                else
                    continue;
            }

            preload = false;
        }

        public static string lastDuelLog;
        private static void PrintDuelLog(string content)
        {
            lastDuelLog = content;
            MessageManager.Cast(content);
        }

        private void SetFace()
        {
            if (condition == Condition.Duel)
            {
                var selfType = RoomServant.SelfType;
                if (GetUI<OcgCoreUI>().TextPlayer0Name.text == name_0)
                {
                    if (isTag)
                    {
                        if (selfType == 0 || selfType == 2)
                        {
                            GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.duelFrameMat0;
                            SetFaceWhenCharaOff(Appearance.duelFace0, 0);
                        }
                        else
                        {
                            GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.duelFrameMat0Tag;
                            SetFaceWhenCharaOff(Appearance.duelFace0Tag, 0);
                        }
                    }
                    else
                    {
                        GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.duelFrameMat0;
                        SetFaceWhenCharaOff(Appearance.duelFace0, 0);
                    }
                }
                else
                {
                    if (selfType == 0 || selfType == 2)
                    {
                        GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.duelFrameMat0Tag;
                        SetFaceWhenCharaOff(Appearance.duelFace0Tag, 0);
                    }
                    else
                    {
                        GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.duelFrameMat0;
                        SetFaceWhenCharaOff(Appearance.duelFace0, 0);
                    }
                }
                if (GetUI<OcgCoreUI>().TextPlayer1Name.text == name_1)
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.duelFrameMat1;
                    SetFaceWhenCharaOff(Appearance.duelFace1, 1);
                }
                else
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.duelFrameMat1Tag;
                    SetFaceWhenCharaOff(Appearance.duelFace1Tag, 1);
                }
            }
            else if (condition == Condition.Watch)
            {
                if (GetUI<OcgCoreUI>().TextPlayer0Name.text == name_0)
                {
                    GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.watchFrameMat0;
                    SetFaceWhenCharaOff(Appearance.watchFace0, 0);
                }
                else
                {
                    GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.watchFrameMat0Tag;
                    SetFaceWhenCharaOff(Appearance.watchFace0Tag, 0);
                }
                if (GetUI<OcgCoreUI>().TextPlayer1Name.text == name_1)
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.watchFrameMat1;
                    SetFaceWhenCharaOff(Appearance.watchFace1, 1);
                }
                else
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.watchFrameMat1Tag;
                    SetFaceWhenCharaOff(Appearance.watchFace1Tag, 1);
                }
            }
            else if (condition == Condition.Replay)
            {
                if (GetUI<OcgCoreUI>().TextPlayer0Name.text == name_0)
                {
                    GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.replayFrameMat0;
                    SetFaceWhenCharaOff(Appearance.replayFace0, 0);
                }
                else
                {
                    GetUI<OcgCoreUI>().AvatarPlayer0.material = Appearance.replayFrameMat0Tag;
                    SetFaceWhenCharaOff(Appearance.replayFace0Tag, 0);
                }
                if (GetUI<OcgCoreUI>().TextPlayer1Name.text == name_1)
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.replayFrameMat1;
                    SetFaceWhenCharaOff(Appearance.replayFace1, 1);
                }
                else
                {
                    GetUI<OcgCoreUI>().AvatarPlayer1.material = Appearance.replayFrameMat1Tag;
                    SetFaceWhenCharaOff(Appearance.replayFace1Tag, 1);
                }
            }

            StartCoroutine(SetMyCardFace());
        }

        private IEnumerator SetMyCardFace()
        {
            if (MyCard.account == null || !mycardDuel)
                yield break;

            var task = MyCard.GetAvatarAsync(GetUI<OcgCoreUI>().TextPlayer0Name.text);
            while (!task.IsCompleted)
                yield return null;
            if (task.Result != null)
                SetFaceWhenCharaOff(TextureManager.Texture2Sprite(task.Result), 0);

            task = MyCard.GetAvatarAsync(GetUI<OcgCoreUI>().TextPlayer1Name.text);
            while (!task.IsCompleted)
                yield return null;
            if (task.Result != null)
                SetFaceWhenCharaOff(TextureManager.Texture2Sprite(task.Result), 1);
        }


        private Sprite mySprite;
        private Sprite opSprite;

        private void SetFaceWhenCharaOff(Sprite sprite, int player)
        {
            if (player == 0)
                mySprite = sprite;
            else
                opSprite = sprite;

            if (!charaFaceSetting)
            {
                GetUI<OcgCoreUI>().AvatarPlayer0.sprite = mySprite;
                GetUI<OcgCoreUI>().AvatarPlayer1.sprite = opSprite;
            }
        }

        private void CloseCharaFace()
        {
            charaFaceSetting = false;
            SetFaceWhenCharaOff(mySprite, 0);
        }

        public void CheckCharaFace()
        {
            if (!showing)
                return;

            if (NeedVoice())
                SetCharacterDefaultFace();
            else
                CloseCharaFace();
        }

        private void SetLP(int player, int val, bool first = false)
        {
            if (first)
            {
                GetUI<OcgCoreUI>().TextPlayer0LP.text = life0.ToString();
                GetUI<OcgCoreUI>().TextPlayer1LP.text = life1.ToString();
            }
            else
            {
                AnimationLpChange(player, val);
            }
        }

        private void AnimationLpChange(int player, int val)
        {
            TextMeshProUGUI text;
            int targetLP;
            if (player == 0)
            {
                text = GetUI<OcgCoreUI>().TextPlayer0LP;
                targetLP = life0;
            }
            else
            {
                text = GetUI<OcgCoreUI>().TextPlayer1LP;
                targetLP = life1;
            }

            int origin = targetLP - val;
            var sequence = DOTween.Sequence();
            var obj = Instantiate(container.duelLpText);
            obj.GetComponent<TextMeshProUGUI>().text = Math.Abs(val).ToString();
            obj.transform.SetParent(GetUI<OcgCoreUI>().RectPopup, false);
            var uiWidth = Screen.width * (float)1080 / Screen.height;
            Color color = DuelLog.damageColor;
            string seType = "COUNT";
            float fontSize = 120;
            if (val > 0)
            {
                color = DuelLog.recoverColor;
                seType = "RECOVERY";
            }
            else
            {
                if (!needDamageResponseInstant)
                    AudioManager.PlaySE("SE_COST_DAMAGE");
            }
            obj.GetComponent<TextMeshProUGUI>().color = color;
            if (player == 0)
            {
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -400);
                obj.transform.localScale = Vector3.zero;
                float targetX = -(uiWidth / 2 - 325);
                sequence.Append(obj.transform.DOScale(1, 0.1f));
                sequence.AppendInterval(0.6f);
                sequence.Append(obj.GetComponent<RectTransform>().DOAnchorPosX(targetX, 0.2f));
                sequence.Join(DOTween.To(() => fontSize, x =>
                {
                    fontSize = x;
                    obj.GetComponent<TextMeshProUGUI>().fontSize = (int)fontSize;
                }, 40, 0.2f));
                sequence.Append(obj.GetComponent<RectTransform>().DOAnchorPosY(-490, 0.2f));
            }
            else
            {
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 400);
                obj.transform.localScale = Vector3.zero;
                float targetX = uiWidth / 2 - 225;
                sequence.Append(obj.transform.DOScale(1, 0.1f));
                sequence.AppendInterval(0.6f);
                sequence.Append(obj.GetComponent<RectTransform>().DOAnchorPosX(targetX, 0.2f));
                sequence.Join(DOTween.To(() => fontSize, x =>
                {
                    fontSize = x;
                    obj.GetComponent<TextMeshProUGUI>().fontSize = (int)fontSize;
                }, 40, 0.2f));
                sequence.Append(obj.GetComponent<RectTransform>().DOAnchorPosY(450, 0.2f));
            }
            sequence.Join(obj.GetComponent<TextMeshProUGUI>().DOFade(0, 0.2f).OnComplete(() =>
            {
                AudioManager.PlaySE("SE_LP_" + seType + (player == 0 ? "_PLAYER" : "_RIVAL"));
                float flp = origin;
                DOTween.To(() => flp, x => { flp = x; text.text = ((int)flp).ToString(); }, targetLP < 0 ? 0 : targetLP, 1.2f);
                Destroy(obj);
            }));

            sequence.Append(text.DOColor(color, 0.1f));
            sequence.Join(text.transform.DOScale(1.3f, 0.2f));
            sequence.AppendInterval(0.8f);
            sequence.Append(text.transform.DOScale(1f, 0.2f));
            sequence.Join(text.DOColor(Color.white, 0.2f));
        }


        void TurnChangeBanner(int controller)
        {
            if (turns == 1)
                return;
            GameObject phaseObject = Instantiate(controller == 0 ? container.duelTurnChangeNear : container.duelTurnChangeFar);
            Destroy(phaseObject, 2f);
            Sleep(200);
        }

        enum FinalAttackType
        {
            BlueEyes,
            DarkM,
            RedEyes,
            Slifer,
            Obelisk,
            Ra,
            Normal
        }

        FinalAttackType GetSpecialFinalAttackType(GameCard attackCard, Vector3 attackedPosition)
        {
            var data = attackCard.GetData();
            var returnValue = FinalAttackType.Normal;
            if (Settings.Data.FinalAttackBlueEyes.Contains(data.Id) || Settings.Data.FinalAttackBlueEyes.Contains(data.Alias))
                returnValue = FinalAttackType.BlueEyes;
            if (Settings.Data.FinalAttackDarkM.Contains(data.Id) || Settings.Data.FinalAttackDarkM.Contains(data.Alias))
                returnValue = FinalAttackType.DarkM;
            if (Settings.Data.FinalAttackRedEyes.Contains(data.Id) || Settings.Data.FinalAttackRedEyes.Contains(data.Alias))
                returnValue = FinalAttackType.RedEyes;
            if (Settings.Data.FinalAttackObelisk.Contains(data.Id) || Settings.Data.FinalAttackObelisk.Contains(data.Alias))
                returnValue = FinalAttackType.Obelisk;
            if (Settings.Data.FinalAttackRa.Contains(data.Id) || Settings.Data.FinalAttackRa.Contains(data.Alias))
                returnValue = FinalAttackType.Ra;
            if (Settings.Data.FinalAttackSlifer.Contains(data.Id) || Settings.Data.FinalAttackSlifer.Contains(data.Alias))
                returnValue = FinalAttackType.Slifer;


            AnimationFinalAttack(returnValue, attackCard, attackedPosition);
            return returnValue;
        }

        void AnimationFinalAttack(FinalAttackType type, GameCard attackCard, Vector3 attackedPosition)
        {
            if (type == FinalAttackType.Normal)
                return;
            if (type == FinalAttackType.BlueEyes)
                AnimationFinalAttack_BlueEyes(attackCard, attackedPosition);
            else if (type == FinalAttackType.DarkM)
                AnimationFinalAttack_DarkM(attackCard, attackedPosition);
            else if (type == FinalAttackType.RedEyes)
                AnimationFinalAttack_RedEyes(attackCard, attackedPosition);
            else if (type == FinalAttackType.Ra)
                AnimationFinalAttack_Ra(attackCard, attackedPosition);
            else if (type == FinalAttackType.Slifer)
                AnimationFinalAttack_Slifer(attackCard, attackedPosition);
            else if (type == FinalAttackType.Obelisk)
                AnimationFinalAttack_Obelisk(attackCard, attackedPosition);
        }

        void AnimationFinalAttack_BlueEyes(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);

            CameraManager.Duel3DOverlayStickWithMain(true);
            AudioManager.PlaySE("SE_MONSTERATTACK_BE_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/BlueEyes/CardSet", "BlueEyes CardSet", true);
            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            attackCard.model.SetActive(false);
            Tools.ChangeLayer(cardSet, "DuelOverlay3D");
            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/BlueEyes/ScreenEffect", "BlueEyes ScreenEffect", true);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);

            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/BlueEyes/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "BlueEyes Hit", true);
            hit.transform.position = attackedPosition;
            hit.SetActive(false);

            var beam = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/BlueEyes/Beam", "BlueEyes Beam", true);
            Destroy(beam);
            beam = beam.transform.GetChild(0).gameObject;
            beam.transform.SetParent(cardSetManager.transform, false);
            beam.transform.localPosition = new Vector3(0f, 1f, 0f);
            beam.GetComponent<PlayableDirector>().enabled = true;
            beam.GetComponent<PlayableDirector>().playOnAwake = true;
            beam.SetActive(false);

            var offset = new Vector3(0f, 5f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;
            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = faceAngle.y >= 0 && faceAngle.y < 180 ? -60f : 60f;
            offset = new Vector3(0f, 5f, -15f);
            if (attackCard.p.controller != 0)
                offset.z = 15f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.6f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(45f, 0f, 0f), 0.6f).SetEase(Ease.InOutCubic));

            offset = new Vector3(0f, 5f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.3f).SetEase(Ease.InOutCubic));
            faceAngle.z = 0f;
            quence.Join(attackTransform.DORotate(faceAngle, 0.3f).SetEase(Ease.InOutCubic));

            quence.InsertCallback(0.7f, () =>
            {
                beam.SetActive(true);
            });

            offset = new Vector3(0f, 3f, 8f);
            if (attackCard.p.controller != 0)
                offset.z = -8f;
            quence.Append(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.1f));
            quence.InsertCallback(1.05f, () =>
            {
                hit.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_BE_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });
            quence.AppendInterval(1f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
            });
        }
        void AnimationFinalAttack_DarkM(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);
            CameraManager.Duel3DOverlayStickWithMain(true);
            CameraManager.DuelOverlay3DPlus();

            AudioManager.PlaySE("SE_MONSTERATTACK_BM_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/CardSet", "DarkM CardSet", true);
            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/ScreenEffect", "DarkM ScreenEffect", true);
            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "DarkM Hit", true);
            //var lineRenderer = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/LineRenderer", "DarkM LineRenderer", true);
            var lineRendererDA = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/LineRendererDA", "DarkM LineRendererDA", true);
            var targetPoint = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/DarkM/TargetPoint", "DarkM TargetPoint", true);

            Tools.ChangeLayer(cardSet, "DuelOverlay3D");
            Tools.ChangeLayer(screenEffect, "DuelOverlay3D");
            Tools.ChangeLayer(hit, "DuelOverlay3D");
            Tools.ChangeLayer(lineRendererDA, "DuelOverlay3D");
            Tools.ChangeLayer(targetPoint, "DuelOverlay3D");

            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            attackCard.model.SetActive(false);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);

            hit.transform.position = attackedPosition;
            hit.SetActive(false);
            targetPoint.transform.position = attackedPosition;
            targetPoint.transform.GetChild(0).GetComponent<PlayableDirector>().playOnAwake = true;
            targetPoint.transform.LookAt(attackCard.model.transform);
            targetPoint.transform.GetChild(0).transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            targetPoint.SetActive(false);


            var lineManager = lineRendererDA.transform.GetChild(0).GetComponent<ElementObjectManager>();
            var line1 = lineManager.GetElement<LineRenderer>("Line01");
            var line2 = lineManager.GetElement<LineRenderer>("Line02");
            lineRendererDA.SetActive(false);

            var offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;

            var positions = new Vector3[2]
{
                attackTransform.position + new Vector3(0f, 1f, 0f),
                attackedPosition
};
            line1.SetPositions(positions);
            line2.SetPositions(positions);

            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = faceAngle.y >= 0 && faceAngle.y < 180 ? -60f : 60f;
            offset = new Vector3(0f, 40f, -15f);
            if (attackCard.p.controller != 0)
                offset.z = 15f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.8f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(45f, 0f, 0f), 0.8f).SetEase(Ease.InOutCubic));

            offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.3f).SetEase(Ease.InOutCubic));
            faceAngle.z = 0f;
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(30f, 0f, 0f), 0.3f).SetEase(Ease.InOutCubic));

            quence.InsertCallback(0.9f, () =>
            {
                lineRendererDA.SetActive(true);
                Destroy(lineRendererDA, 0.58f);
            });

            offset = new Vector3(0f, 3f, 8f);
            if (attackCard.p.controller != 0)
                offset.z = -8f;
            quence.Append(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.1f));
            quence.InsertCallback(1.25f, () =>
            {
                hit.SetActive(true);
                targetPoint.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_BM_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });
            quence.AppendInterval(0.5f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                Destroy(targetPoint);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
                CameraManager.DuelOverlay3DMinus();
            });
        }
        void AnimationFinalAttack_RedEyes(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);
            CameraManager.Duel3DOverlayStickWithMain(true);
            CameraManager.DuelOverlay3DPlus();

            AudioManager.PlaySE("SE_MONSTERATTACK_RE_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/RedEyes/CardSet", "RedEyes CardSet", true);
            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/RedEyes/ScreenEffect", "RedEyes ScreenEffect", true);
            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/RedEyes/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "RedEyes Hit", true);
            var bless = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/RedEyes/Bless", "RedEyes Bless", true);
            bless.SetActive(false);

            Tools.ChangeLayer(cardSet, "DuelOverlay3D");

            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            cardSetManager.GetComponent<PlayableDirector>().Play();
            attackCard.model.SetActive(false);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);

            hit.transform.position = attackedPosition;
            hit.SetActive(false);

            var offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;

            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = faceAngle.y >= 0 && faceAngle.y < 180 ? -60f : 60f;
            offset = new Vector3(0f, 40f, -15f);
            if (attackCard.p.controller != 0)
                offset.z = 15f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.6f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(45f, 0f, 0f), 0.6f).SetEase(Ease.InOutCubic));

            offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.3f).SetEase(Ease.InOutCubic));
            faceAngle.z = 0f;
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(30f, 0f, 0f), 0.3f).SetEase(Ease.InOutCubic));


            offset = new Vector3(0f, 3f, 8f);
            if (attackCard.p.controller != 0)
                offset.z = -8f;
            quence.Append(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.3f));
            quence.InsertCallback(0.95f, () =>
            {
                bless.SetActive(true);
                bless.transform.position = attackTransform.position;
                bless.transform.LookAt(attackedPosition);
                bless.transform.DOMove(attackedPosition, 0.3f);
            });

            quence.InsertCallback(1.2f, () =>
            {
                hit.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_RE_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });
            quence.AppendInterval(0.5f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                Destroy(bless);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
                CameraManager.DuelOverlay3DMinus();
            });
        }
        void AnimationFinalAttack_Ra(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);
            CameraManager.Duel3DOverlayStickWithMain(true);
            CameraManager.DuelOverlay3DPlus();

            AudioManager.PlaySE("SE_MONSTERATTACK_RA_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Ra/CardSet", "Ra CardSet", true);
            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Ra/ScreenEffect", "Ra ScreenEffect", true);
            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Ra/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "Ra Hit", true);
            var bless = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Ra/Bless", "Ra Bless", true);
            Tools.SetParticleSystemSimulationSpeed(bless.transform, 0.5f);
            bless.SetActive(false);

            Tools.ChangeLayer(cardSet, "DuelOverlay3D");

            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            cardSetManager.GetComponent<PlayableDirector>().Play();
            attackCard.model.SetActive(false);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);

            hit.transform.position = attackedPosition;
            hit.SetActive(false);

            var offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;

            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = 0f;
            offset = new Vector3(0f, 40f, -15f);
            if (attackCard.p.controller != 0)
                offset.z = 15f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 1f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(-30f, 0f, 0f), 1f).SetEase(Ease.InOutCubic));

            quence.InsertCallback(1f, () =>
            {
                bless.SetActive(true);
                bless.transform.position = attackTransform.position;
                bless.transform.LookAt(attackedPosition);

                bless.transform.DOMove(attackedPosition, 0.3f);

                offset = new Vector3(0f, 3f, 8f);
                if (attackCard.p.controller != 0)
                    offset.z = -8f;
                Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.3f);
            });

            offset = new Vector3(0f, 20f, 0f);
            if (attackCard.p.controller != 0)
                offset.z = 0f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.3f).SetEase(Ease.OutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(30f, 0f, 0f), 0.3f).SetEase(Ease.OutCubic));


            quence.InsertCallback(1.3f, () =>
            {
                hit.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_RA_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });
            quence.AppendInterval(0.5f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                Destroy(bless);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
                CameraManager.DuelOverlay3DMinus();
            });
        }
        void AnimationFinalAttack_Slifer(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);
            CameraManager.Duel3DOverlayStickWithMain(true);
            CameraManager.DuelOverlay3DPlus();
            AudioManager.PlaySE("SE_MONSTERATTACK_SLIFER_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Slifer/CardSet", "Slifer CardSet", true);
            Tools.ChangeLayer(cardSet, "DuelOverlay3D");
            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            attackCard.model.SetActive(false);

            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Slifer/ScreenEffect", "Slifer ScreenEffect", true);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);
            screenEffect.SetActive(false);

            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Slifer/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "Slifer Hit", true);
            hit.transform.position = attackedPosition;
            hit.SetActive(false);

            var beam = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Slifer/Beam", "Slifer Beam", true);
            Destroy(beam);
            beam = beam.transform.GetChild(0).gameObject;
            beam.transform.SetParent(cardSetManager.transform, false);
            beam.transform.localPosition = new Vector3(0f, 1f, 0f);
            beam.GetComponent<PlayableDirector>().enabled = true;
            beam.GetComponent<PlayableDirector>().playOnAwake = true;
            beam.SetActive(false);

            var offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;
            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = 0;
            offset = new Vector3(0f, 40f, -15f);
            if (attackCard.p.controller != 0)
                offset.z = 15f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.8f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(faceAngle + new Vector3(30f, 0f, 0f), 0.8f).SetEase(Ease.InOutCubic));

            offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.3f).SetEase(Ease.InOutCubic));
            faceAngle.z = 0f;
            quence.Join(attackTransform.DORotate(faceAngle, 0.3f).SetEase(Ease.InOutCubic));

            quence.InsertCallback(0.4f, () =>
            {
                screenEffect.SetActive(true);
            });

            quence.InsertCallback(0.9f, () =>
            {
                beam.SetActive(true);
            });

            quence.InsertCallback(1.1f, () =>
            {
                offset = new Vector3(0f, 3f, 8f);
                if (attackCard.p.controller != 0)
                    offset.z = -8f;
                Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.1f);
            });

            quence.InsertCallback(1.2f, () =>
            {
                hit.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_SLIFER_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });
            quence.AppendInterval(0.6f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
                CameraManager.DuelOverlay3DMinus();
            });

        }
        void AnimationFinalAttack_Obelisk(GameCard attackCard, Vector3 attackedPosition)
        {
            var attackRotation = attackCard.p.controller == 0 ? Vector3.zero : new Vector3(0f, 180f, 0f);
            CameraManager.Duel3DOverlayStickWithMain(true);
            CameraManager.DuelOverlay3DPlus();

            AudioManager.PlaySE("SE_MONSTERATTACK_OBELISK_01");

            var cardSet = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Obelisk/CardSet", "Obelisk CardSet", true);
            var screenEffect = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Obelisk/ScreenEffect", "Obelisk ScreenEffect", true);
            var hit = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Obelisk/Hit" + (attackCard.p.controller == 0 ? "Far" : "Near"), "Obelisk Hit", true);
            var punch = ABLoader.LoadFromFolder("MasterDuel/Timeline/FinalAttack/Obelisk/Punch", "Obelisk Punch", true);
            Destroy(punch);
            punch = Tools.GetPlayableDirectorInChildren(punch.transform).gameObject;
            punch.transform.SetParent(null, false);
            punch.GetComponent<PlayableDirector>().enabled = true;
            if (attackCard.p.controller != 0)
                punch.transform.eulerAngles = new Vector3(0f, 180f, 0f);

            Tools.ChangeLayer(cardSet, "DuelOverlay3D");
            Tools.ChangeLayer(punch, "DuelOverlay3D");

            var attackTransform = cardSet.transform;
            var cardSetManager = attackTransform.GetChild(0).GetComponent<ElementObjectManager>();
            var subManager = cardSetManager.GetElement<ElementObjectManager>("Card");
            StartCoroutine(Program.instance.texture_.LoadDummyCard(subManager, attackCard.GetData().Id, attackCard.p.controller));
            cardSetManager.GetComponent<PlayableDirector>().Play();
            attackCard.model.SetActive(false);
            screenEffect.transform.SetParent(Program.instance.camera_.cameraDuelOverlay3D.transform, true);

            hit.transform.position = attackedPosition;
            hit.SetActive(false);
            punch.transform.position = attackedPosition;
            punch.SetActive(false);

            var offset = new Vector3(0f, 5f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            var attackPosition = attackCard.model.transform.position;
            attackTransform.position = attackPosition + offset;

            attackTransform.LookAt(attackedPosition);
            var faceAngle = attackTransform.eulerAngles;
            faceAngle.x = 0;
            attackTransform.eulerAngles = attackRotation;
            attackTransform.position = attackPosition;

            Sequence quence = DOTween.Sequence();
            faceAngle.z = 0f;
            offset = new Vector3(5f, 40f, -15f);
            if (attackCard.p.controller != 0)
            {
                offset.x = -5f;
                offset.z = 15f;
            }
            quence.Append(attackTransform.DOMove(attackPosition + offset, 1.5f).SetEase(Ease.InOutCubic));
            offset = new Vector3(-30f, 35f, 0f);
            quence.Join(attackTransform.DORotate(faceAngle + offset, 1.5f).SetEase(Ease.InOutCubic));
            quence.InsertCallback(1f, () =>
            {
                punch.SetActive(true);
            });

            offset = new Vector3(0f, 20f, -8f);
            if (attackCard.p.controller != 0)
                offset.z = 8f;
            quence.Append(attackTransform.DOMove(attackPosition + offset, 0.4f).SetEase(Ease.InOutCubic));
            faceAngle.z = 0f;
            offset = new Vector3(20f, 0f, 0f);
            quence.Join(attackTransform.DORotate(faceAngle + offset, 0.4f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.GetChild(0).DOLocalMoveZ(10f, 0.4f));
            quence.Join(attackTransform.GetChild(0).DOLocalRotate(new Vector3(0f, -30f, 0f), 0.4f));
            offset = new Vector3(0f, 3f, 8f);
            if (attackCard.p.controller != 0)
                offset.z = -8f;
            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition + offset, 0.4f));

            quence.InsertCallback(1.8f, () =>
            {
                hit.SetActive(true);
                AudioManager.PlaySE("SE_MONSTERATTACK_OBELISK_02");
                messagePass = true;
                CameraManager.ShakeCamera(true);
            });

            quence.AppendInterval(0.5f);
            quence.Append(attackTransform.DOMove(attackPosition, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.DORotate(attackRotation, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.GetChild(0).DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic));
            quence.Join(attackTransform.GetChild(0).DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic));

            quence.Join(Program.instance.camera_.cameraMain.transform.DOLocalMove(CameraManager.mainCameraDefaultPosition, 0.2f));

            quence.OnComplete(() =>
            {
                Destroy(cardSet);
                Destroy(hit);
                Destroy(screenEffect);
                Destroy(punch);
                needDamageResponseInstant = false;
                attackCard.model.SetActive(true);
                CameraManager.Duel3DOverlayStickWithMain(false);
                CameraManager.DuelOverlay3DMinus();
            });
        }

        void RefreshHandCardPositionInstant()
        {
            if (showing)
                foreach (var card in cards)
                    card.SetHandDefault();
        }
        public void RefreshHandCardPosition()
        {
            if (showing)
                foreach (var card in cards)
                    card.SetHandToDefault();
        }


        [HideInInspector] public List<PlaceSelector> places = new();
        private readonly List<GraveBehaviour> graves = new();
        private void CreatePlaceSelector(GPS p)
        {
            var go = new GameObject("PlaceSelector");
            var mono = go.AddComponent<PlaceSelector>();
            mono.p = p;
            go.transform.SetParent(Program.instance.container_3D);
            allGameObjects.Add(go);
            places.Add(mono);
        }

        string fieldHint;
        int fieldMin;
        int fieldMax;
        bool fieldExitable;
        bool fieldSendable;
        [HideInInspector] public int fieldCounterCount;

        void FieldSelect(string hint, List<GameCard> cards, int min, int max, bool exitable, bool sendable)
        {
            foreach (var place in places)
                place.InitializeSelectCardInThisZone(cards);
            fieldHint = string.IsNullOrEmpty(hint) ? InterString.Get("请选择卡片") : hint;
            fieldMin = min;
            fieldMax = max;
            fieldExitable = exitable;
            fieldSendable = sendable;
            fieldCounterCount = 0;

            if (currentMessage == GameMessage.SelectCard
                || currentMessage == GameMessage.SelectCounter)
                GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + 0 + Program.STRING_SLASH + fieldMax);
            else if (currentMessage == GameMessage.SelectSum)
            {
                if (!ES_overFlow)
                    foreach (var place in places)
                        if (place.cardSelecting)
                            if (!place.cardSelected)
                                if (CheckSelectableInSum(cardsInSelection, place.cookieCard, cardsMustBeSelected, ES_max + cardsMustBeSelected.Count))
                                    place.CardInThisZoneSelectable();
                                else
                                    place.CardInThisZoneUnselectable();
                GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + GetSelectLevelSum(cardsMustBeSelected)[0] + Program.STRING_SLASH + ES_level);
            }
            else
            {
                if (!string.IsNullOrEmpty(fieldHint))
                    GetUI<OcgCoreUI>().SetHint(fieldHint);
            }

            RefreshButton();
        }

        public void FieldSelectRefresh(GameCard card)
        {
            var selected = new List<GameCard>();
            foreach (var place in places)
                if (place.cardSelecting)
                    if (place.cardSelected)
                        selected.Add(place.cookieCard);
            if (currentMessage == GameMessage.SelectSum)
            {
                var sum = GetSelectLevelSum(selected);
                if ((ES_overFlow && (ES_level <= sum[0] || ES_level <= sum[1]))
                    ||
                    (!ES_overFlow && (ES_level == sum[0] || ES_level == sum[1])))
                    fieldSendable = true;
                else
                    fieldSendable = false;
                if (!ES_overFlow)
                {
                    if (sum[0] == ES_level || sum[1] == ES_level)
                    {
                        FieldSelectedSend();
                        return;
                    }
                    else
                    {
                        foreach (var place in places)
                            if (place.cardSelecting)
                                if (!place.cardSelected)
                                    if (CheckSelectableInSum(cardsInSelection, place.cookieCard, selected, ES_max + cardsMustBeSelected.Count))
                                        place.CardInThisZoneSelectable();
                                    else
                                        place.CardInThisZoneUnselectable();
                    }
                }
                RefreshButton();
                GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + GetSelectLevelSum(selected)[0] + Program.STRING_SLASH + ES_level);
            }
            else if (currentMessage == GameMessage.SelectCounter)
            {
                fieldCounterCount++;
                card.counterSelected++;
                GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + fieldCounterCount + Program.STRING_SLASH + fieldMax);
                if (fieldCounterCount == ES_min)
                {
                    FieldSelectedSend();
                    return;
                }
                foreach (var place in places)
                    if (place.cardSelecting)
                    {
                        if (place.cookieCard.counterCanCount > place.cookieCard.counterSelected)
                            place.CardInThisZoneSelectable();
                        else
                            place.CardInThisZoneUnselectable();
                    }
            }
            else if (currentMessage == GameMessage.SelectTribute)
            {
                var sum = 0;
                foreach (var c in selected)
                    sum += c.levelForSelect_1;
                if (selected.Count >= fieldMax)
                    FieldSelectedSend();
                else if (sum >= fieldMin)
                {
                    fieldSendable = true;
                    RefreshButton();
                }
            }
            else
            {
                if (selected.Count >= fieldMin)
                    fieldSendable = true;
                else
                    fieldSendable = false;
                if (selected.Count >= fieldMax)
                    FieldSelectedSend();
                else
                {
                    foreach (var place in places)
                        if (place.cardSelecting)
                            if (!place.cardSelected)
                                place.CardInThisZoneSelectable();
                    RefreshButton();
                }
                if (currentMessage == GameMessage.SelectCard)
                    GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + selected.Count + Program.STRING_SLASH + fieldMax);
            }
        }

        void RefreshButton()
        {
            if (fieldSendable)
            {
                btnConfirm.Show();
                if (currentMessage == GameMessage.SelectUnselect)
                    btnCancel.Hide();
            }
            else
                btnConfirm.Hide();
            if (fieldExitable)
            {
                if (currentMessage == GameMessage.SelectUnselect && fieldSendable)
                {
                }
                else
                    btnCancel.Show();
            }
            else
                btnCancel.Hide();
        }

        private void FieldSelectReset()
        {
            foreach (var place in places)
                place.StopResponse();
            btnConfirm.Hide();
            btnCancel.Hide();
            GetUI<OcgCoreUI>().CloseHint();
        }

        public void FieldSelectedSend()
        {
            var selected = new List<GameCard>();
            foreach (var place in places)
                if (place.cardSelecting)
                    if (place.cardSelected)
                        selected.Add(place.cookieCard);
            var binaryMaster = new BinaryMaster();
            if (currentMessage == GameMessage.SelectUnselect && selected.Count == 0)
                binaryMaster.writer.Write(-1);
            else if (currentMessage == GameMessage.SelectCounter)
                for (var i = 0; i < cardsInSelection.Count; i++)
                    binaryMaster.writer.Write((short)cardsInSelection[i].counterSelected);
            else if (currentMessage == GameMessage.SelectSum)
            {
                binaryMaster.writer.Write((byte)selected.Count);
                foreach (var card in cardsMustBeSelected)
                    binaryMaster.writer.Write((byte)card.selectPtr);
                foreach (var card in selected)
                    if (!cardsMustBeSelected.Contains(card))
                        binaryMaster.writer.Write((byte)card.selectPtr);
            }
            else
            {
                binaryMaster.writer.Write((byte)selected.Count);
                foreach (var card in selected)
                    binaryMaster.writer.Write((byte)card.selectPtr);
            }
            SendReturn(binaryMaster.Get());
        }

        public void FieldSelectedCancel()
        {
            if (currentMessage == GameMessage.SelectCounter)
            {
                foreach (var card in cardsInSelection)
                    card.counterSelected = 0;
                fieldCounterCount = 0;
                GetUI<OcgCoreUI>().SetHint(fieldHint + ": " + 0 + Program.STRING_SLASH + fieldMax);
                foreach (var place in places)
                    if (place.cardSelecting)
                        place.CardInThisZoneSelectable();
            }
            else
            {
                var binaryMaster = new BinaryMaster();
                binaryMaster.writer.Write(-1);
                SendReturn(binaryMaster.Get());
            }
        }


        public void SetDeckTop(GameCard card)
        {
            var deck = card.p.controller == 0 ? myExtra : opExtra;
            Material targetMat = null;
            if ((card.p.position & (uint)CardPosition.FaceUp) > 0)
                targetMat = card.GetMaterial();
            if (targetMat == null)
                targetMat = card.p.controller == 0 ? myProtector : opProtector;
            foreach (var r in deck.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = targetMat;
        }
        public IEnumerator UpdateDeckTop(uint controller, GameCard card = null)
        {
            var deck = controller == 0 ? myExtra : opExtra;
            GameCard topCard = card;
            if (topCard == null)
            {
                var extraCount = GetLocationCardCount(CardLocation.Extra, controller);
                foreach (var c in cards)
                    if (c.p.controller == controller)
                        if ((c.p.location & (uint)CardLocation.Extra) > 0)
                            if ((c.p.position & (uint)CardPosition.FaceUp) > 0)
                                if (c.p.sequence == extraCount - 1)
                                {
                                    topCard = c;
                                    break;
                                }
            }
            var targetMat = controller == 0 ? myProtector : opProtector;
            if (topCard != null)
            {
                var code = topCard.GetData().Id;

                var matLoad = MaterialLoader.LoadCardMaterialAsync(code);
                while (!matLoad.IsCompleted)
                    yield return null;
                targetMat = matLoad.Result;

                var task = CardImageLoader.LoadCardAsync(code, true);
                while (!task.IsCompleted)
                    yield return null;
                targetMat.mainTexture = task.Result;
            }
            foreach (var r in deck.transform.GetComponentsInChildren<Renderer>(true))
                if (r.name.EndsWith("back"))
                    r.material = targetMat;
        }

        public void RefreshBgState()
        {
            var myDeckCount = GetLocationCardCount(CardLocation.Deck, 0);
            var deckSetOffset = myDeck.GetElement<Transform>("CardShuffleTop");
            if (myDeckCount == 0)
                deckSetOffset.localScale = Vector3.zero;
            else
                deckSetOffset.localScale = new Vector3(0.9f, myDeckCount, 0.9f);
            var myExtraCount = GetLocationCardCount(CardLocation.Extra, 0);
            deckSetOffset = myExtra.GetElement<Transform>("CardShuffleTop");
            if (myExtraCount == 0)
                deckSetOffset.localScale = Vector3.zero;
            else
                deckSetOffset.localScale = new Vector3(0.9f, myExtraCount, 0.9f);
            var opDeckCount = GetLocationCardCount(CardLocation.Deck, 1);
            deckSetOffset = opDeck.GetElement<Transform>("CardShuffleTop");
            if (opDeckCount == 0)
                deckSetOffset.localScale = Vector3.zero;
            else
                deckSetOffset.localScale = new Vector3(0.9f, opDeckCount, 0.9f);
            var opExtraCount = GetLocationCardCount(CardLocation.Extra, 1);
            deckSetOffset = opExtra.GetElement<Transform>("CardShuffleTop");
            if (opExtraCount == 0)
                deckSetOffset.localScale = Vector3.zero;
            else
                deckSetOffset.localScale = new Vector3(0.9f, opExtraCount, 0.9f);

            if (GetLocationCardCount(CardLocation.Grave, 0) > 20)
            {
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS3").Play();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Grave, 0) > 10)
            {
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS2").Play();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Grave, 0) > 0)
            {
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS1").Play();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else
            {
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 0);
            }

            if (GetLocationCardCount(CardLocation.Removed, 0) > 20)
            {
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Play();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Removed, 0) > 10)
            {
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Play();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Removed, 0) > 0)
            {
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Play();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else
            {
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave0Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave0Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 0);
            }

            if (GetLocationCardCount(CardLocation.Grave, 1) > 20)
            {
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS3").Play();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Grave, 1) > 10)
            {
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS2").Play();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Grave, 1) > 0)
            {
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS1").Play();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 1);
            }
            else
            {
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("GraveIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_GraveCardExist", 0);
            }

            if (GetLocationCardCount(CardLocation.Removed, 1) > 20)
            {
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Play();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Removed, 1) > 10)
            {
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Play();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else if (GetLocationCardCount(CardLocation.Removed, 1) > 0)
            {
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Play();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 1);
            }
            else
            {
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS1").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS2").Stop();
                grave1Manager.GetElement<ParticleSystem>("ExcludeIdleS3").Stop();
                grave1Manager.GetElement<Renderer>("Material01").material.SetFloat("_ExcludeCardExist", 0);
            }
        }

        public void SetBgTimeScale(float timeScale)
        {
            Tools.SetAnimatorTimescale(field0.transform, timeScale);
            Tools.SetAnimatorTimescale(field1.transform, timeScale);
            Tools.SetAnimatorTimescale(phaseButton.transform, timeScale);
            if (timer != null)
                Tools.SetAnimatorTimescale(timer.transform, timeScale);
            Tools.SetParticleSystemSimulationSpeed(field0.transform, timeScale);
            Tools.SetParticleSystemSimulationSpeed(field1.transform, timeScale);
        }

        public void GraveBgEffect(GPS p, bool cardIn)
        {
            ElementObjectManager manager;
            if (p.controller == 0)
                manager = grave0Manager;
            else
                manager = grave1Manager;
            if (manager == null)
                return;

            BgEffectSetting effect;
            BgEffectSetting effectEnd;
            string audio = "";
            if ((p.location & (uint)CardLocation.Grave) > 0)
            {
                if (cardIn)
                {
                    effect = manager.GetElement<BgEffectSetting>("GraveIn");
                    effectEnd = manager.GetElement<BgEffectSetting>("GraveInend");
                    audio = "SE_CEMETARY_ABSORB";
                }
                else
                {
                    effect = manager.GetElement<BgEffectSetting>("GraveOut");
                    effectEnd = manager.GetElement<BgEffectSetting>("GraveOutend");
                    audio = "SE_CEMETARY_GOOUT";
                }
            }
            else
            {
                if (cardIn)
                {
                    effect = manager.GetElement<BgEffectSetting>("ExcludeIn");
                    effectEnd = manager.GetElement<BgEffectSetting>("ExcludeInend");
                    audio = "SE_EXCLUSION_ABSORB";
                }
                else
                {
                    effect = manager.GetElement<BgEffectSetting>("ExcludeOut");
                    effectEnd = manager.GetElement<BgEffectSetting>("ExcludeOutend");
                    audio = "SE_EXCLUSION_GOOUT";
                }
            }
            DOTween.To(v => { }, 0, 0, effect.delay).OnComplete(() =>
            {
                effect.particle.Play();
                AudioManager.PlaySE(audio);
            });
            DOTween.To(v => { }, 0, 0, effectEnd.delay).OnComplete(() =>
            {
                effectEnd.particle.Play();
            });
        }

        void ShowBgHint()
        {
            bool haveHint = false;
            foreach (var card in cards)
                if ((card.p.location & (uint)CardLocation.Grave) > 0)
                    if (card.p.controller == 0)
                        if (card.buttons.Count > 0)
                        {
                            var effect = ABLoader.LoadFromFile("MasterDuel/Effects/Hitghlight/fxp_HL_active/fxp_HL_active_grave_001", true);
                            effect.transform.SetParent(grave0Manager.GetElement<Transform>("GraveHighlightNear"), false);
                            Destroy(effect, 3f);
                            grave0Manager.GetElement<Animator>("GraveHighlightNear").SetBool("On", true);
                            haveHint = true;
                            break;
                        }
            foreach (var card in cards)
                if ((card.p.location & (uint)CardLocation.Removed) > 0)
                    if (card.p.controller == 0)
                        if (card.buttons.Count > 0)
                        {
                            var effect = ABLoader.LoadFromFile("MasterDuel/Effects/Hitghlight/fxp_HL_active/fxp_HL_active_exclude_001", true);
                            effect.transform.SetParent(grave0Manager.GetElement<Transform>("ExcludeHighlightNear"), false);
                            Destroy(effect, 3f);
                            grave0Manager.GetElement<Animator>("ExcludeHighlightNear").SetBool("On", true);
                            haveHint = true;
                            break;
                        }
            foreach (var card in cards)
                if ((card.p.location & (uint)CardLocation.Extra) > 0)
                    if (card.p.controller == 0)
                        if (card.buttons.Count > 0)
                        {
                            var effect = ABLoader.LoadFromFile("MasterDuel/Effects/Hitghlight/fxp_HL_active/fxp_HL_active_Exdeck_001", true);
                            effect.transform.SetParent(myExtra.transform, false);
                            effect.transform.position = Tools.GetDeckModelTopPosition(myExtra);
                            foreach (var place in places)
                                place.ShowHint((uint)CardLocation.Extra, 0u);
                            Destroy(effect, 3f);
                            haveHint = true;
                            break;
                        }
            foreach (var card in cards)
                if ((card.p.location & (uint)CardLocation.Deck) > 0)
                    if (card.p.controller == 0)
                        if (card.buttons.Count > 0)
                        {
                            var effect = ABLoader.LoadFromFile("MasterDuel/Effects/Hitghlight/fxp_HL_active/fxp_HL_active_Exdeck_001", true);
                            effect.transform.SetParent(myDeck.transform, false);
                            effect.transform.position = Tools.GetDeckModelTopPosition(myDeck);
                            foreach (var place in places)
                                place.ShowHint((uint)CardLocation.Deck, 0u);
                            Destroy(effect, 3f);
                            haveHint = true;
                            break;
                        }

            if (haveHint)
                AudioManager.PlaySE("SE_DUEL_ACTIVE_POSSIBLE");
        }

        void CloseBgHint()
        {
            grave0Manager.GetElement<Animator>("GraveHighlightNear").SetBool("On", false);
            grave0Manager.GetElement<Animator>("ExcludeHighlightNear").SetBool("On", false);
        }


        public int GetAllAtk(bool mySide)
        {
            int allAttack = 0;
            var monsters = GCS_GetLocationCards(mySide ? 0 : 1, (int)CardLocation.MonsterZone);
            foreach (var card in monsters)
                if ((card.p.position & (uint)CardPosition.FaceUpAttack) > 0)
                    allAttack += card.GetData().Attack;
            return allAttack;
        }

        private bool PlayerLosing()
        {
            if (myTurn)
            {
                if (GetAllAtk(true) - GetAllAtk(false) > life1)
                {
                    var defenseCount = 0;
                    var monsters = GCS_GetLocationCards(1, (int)CardLocation.MonsterZone);
                    foreach (var card in monsters)
                        if ((card.p.position & (uint)CardPosition.Defence) > 0)
                            defenseCount++;
                    if (defenseCount == 0)
                        return true;
                }
            }
            else if (!myTurn)
            {
                if (GetAllAtk(false) - GetAllAtk(true) > life0)
                {
                    var defenseCount = 0;
                    var monsters = GCS_GetLocationCards(0, (int)CardLocation.MonsterZone);
                    foreach (var card in monsters)
                        if ((card.p.position & (uint)CardPosition.Defence) > 0)
                            defenseCount++;
                    if (defenseCount == 0)
                        return true;
                }
            }
            return false;
        }

        void ShowAttackLine(Vector3 end, Vector3 start)
        {
            var lineManager = attackLine.GetComponent<ElementObjectManager>();
            var line1 = lineManager.GetElement<LineRenderer>("arrowlimeRollover");
            var line2 = lineManager.GetElement<LineRenderer>("arrowRollover");
            var posArr = new Vector3[9]
            {
                new Vector3(start.x, 5, start.z),
                new Vector3(start.x + (end.x - start.x) * 0.125f, 5.8f, start.z + (end.z - start.z) * 0.125f),
                new Vector3(start.x + (end.x - start.x) * 0.25f, 6.3f, start.z + (end.z - start.z) * 0.25f),
                new Vector3(start.x + (end.x - start.x) * 0.375f, 6.5f, start.z + (end.z - start.z) * 0.375f),
                new Vector3(start.x + (end.x - start.x) * 0.5f, 6.5f, start.z + (end.z - start.z) * 0.5f),
                new Vector3(start.x + (end.x - start.x) * 0.625f, 6.5f, start.z + (end.z - start.z) * 0.625f),
                new Vector3(start.x + (end.x - start.x) * 0.75f, 6.3f, start.z + (end.z - start.z) * 0.75f),
                new Vector3(start.x + (end.x - start.x) * 0.875f, 5.8f, start.z + (end.z - start.z) * 0.875f),
                new Vector3(end.x, 5, end.z),
            };
            line1.SetPositions(posArr);
            line2.SetPositions(posArr);
            attackLine.SetActive(true);
        }

        public void ShowEquipLine(Vector3 start, Vector3 end)
        {
            var line = equipLine.transform.GetChild(0).GetComponent<LineRenderer>();
            var posArr = new Vector3[9]
            {
                new(start.x, 1f, start.z),
                new(start.x + (end.x - start.x) * 0.125f, 1.5f, start.z + (end.z - start.z) * 0.125f),
                new(start.x + (end.x - start.x) * 0.25f, 2f, start.z + (end.z - start.z) * 0.25f),
                new(start.x + (end.x - start.x) * 0.375f, 2.5f, start.z + (end.z - start.z) * 0.375f),
                new(start.x + (end.x - start.x) * 0.5f, 2.8f, start.z + (end.z - start.z) * 0.5f),
                new(start.x + (end.x - start.x) * 0.625f, 2.5f, start.z + (end.z - start.z) * 0.625f),
                new(start.x + (end.x - start.x) * 0.75f, 2f, start.z + (end.z - start.z) * 0.75f),
                new(start.x + (end.x - start.x) * 0.875f, 1.5f, start.z + (end.z - start.z) * 0.875f),
                new(end.x, 1f, end.z),
            };
            line.SetPositions(posArr);
            equipLine.SetActive(true);
        }

        private readonly List<GameObject> targetLines = new();

        public void ShowTargetLines(Vector3 start, List<GameCard> targets)
        {
            foreach (var card in targets)
            {
                if ((card.p.location & (uint)CardLocation.Onfield) > 0)
                {
                    var newLine = Instantiate(targetLine);
                    newLine.SetActive(true);
                    var line = newLine.transform.GetChild(0).GetComponent<LineRenderer>();
                    var end = card.model.transform.position;
                    var posArr = new Vector3[9]
                    {
                        new(start.x, 1f, start.z),
                        new(start.x + (end.x - start.x) * 0.125f, 5f, start.z + (end.z - start.z) * 0.125f),
                        new(start.x + (end.x - start.x) * 0.25f, 9f, start.z + (end.z - start.z) * 0.25f),
                        new(start.x + (end.x - start.x) * 0.375f, 11f, start.z + (end.z - start.z) * 0.375f),
                        new(start.x + (end.x - start.x) * 0.5f, 12f, start.z + (end.z - start.z) * 0.5f),
                        new(start.x + (end.x - start.x) * 0.625f, 11f, start.z + (end.z - start.z) * 0.625f),
                        new(start.x + (end.x - start.x) * 0.75f, 9f, start.z + (end.z - start.z) * 0.75f),
                        new(start.x + (end.x - start.x) * 0.875f, 5f, start.z + (end.z - start.z) * 0.875f),
                        new(end.x, 1f, end.z),
                    };
                    line.SetPositions(posArr);
                    targetLines.Add(newLine);
                }
            }
        }



        bool CheckChain()
        {
            bool config = true;
            if (condition == Condition.Duel && Config.Get("DuelChain", "1") == "0")
                config = false;
            else if (condition == Condition.Watch && Config.Get("WatchChain", "1") == "0")
                config = false;
            else if (condition == Condition.Replay && Config.Get("ReplayChain", "1") == "0")
                config = false;
            return config;
        }
        void ShowChainStack()
        {
            int chain = cardsInChain.Count;
            if (chain == 1)
                return;
            if (!CheckChain())
                return;

            GameObject animation;
            if (chain < 3)
                animation = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelChain/DuelChainStack01", true);
            else
            {
                animation = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelChain/DuelChainStack02", true);
                DOTween.To(v => { }, 0, 0, 0.0166f).OnComplete(() =>
                {
                    AudioManager.PlaySE("SE_DUELCHAIN_STACK02");
                });
                DOTween.To(v => { }, 0, 0, 0.767f).OnComplete(() =>
                {
                    if (chain == 3)
                        AudioManager.PlaySE("SE_DUEL_CHAIN_NUMEFF_01");
                    else if (chain == 4)
                        AudioManager.PlaySE("SE_DUEL_CHAIN_NUMEFF_02");
                    else
                        AudioManager.PlaySE("SE_DUEL_CHAIN_NUMEFF_03");
                });
            }
            var director = animation.GetComponent<PlayableDirector>();
            var mono = animation.AddComponent<DoWhenPlayableDirectorStop>();
            mono.action = () =>
            {
                Destroy(animation);
            };
            var manager = animation.GetComponent<ElementObjectManager>();

            ElementObjectManager targetCardD;
            if (controllerInChain[chain - 1] == 0)
            {
                targetCardD = manager.GetElement<ElementObjectManager>("DummyChainCardDL");
                manager.GetElement("ChainCardSetDROffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Tens"),
                    chain);
            }
            else
            {
                targetCardD = manager.GetElement<ElementObjectManager>("DummyChainCardDR");
                manager.GetElement("ChainCardSetDLOffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Tens"),
                    chain);
            }
            StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardD, codesInChain[chain - 1], 0, true));

            if (controllerInChain[chain - 1] == controllerInChain[chain - 2])
            {
                manager.GetElement("ChainStraightCLtoDR").SetActive(false);
                manager.GetElement("ChainStraightCRtoDL").SetActive(false);
            }
            else
            {
                if (controllerInChain[chain - 1] == 0)
                    manager.GetElement("ChainStraightCLtoDR").SetActive(false);
                else
                    manager.GetElement("ChainStraightCRtoDL").SetActive(false);
            }
            ElementObjectManager targetCardC;
            if (controllerInChain[chain - 2] == 0)
            {
                targetCardC = manager.GetElement<ElementObjectManager>("DummyChainCardCL");
                manager.GetElement("ChainCardSetCROffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumCL_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumCL_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumCL_Tens"),
                    chain - 1);
            }
            else
            {
                targetCardC = manager.GetElement<ElementObjectManager>("DummyChainCardCR");
                manager.GetElement("ChainCardSetCLOffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumCR_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumCR_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumCR_Tens"),
                    chain - 1);
            }
            StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardC, codesInChain[chain - 2], 0, true));

            if (chain > 2)
            {
                if (controllerInChain[chain - 2] == controllerInChain[chain - 3])
                {
                    manager.GetElement("ChainStraightBLtoCR").SetActive(false);
                    manager.GetElement("ChainStraightBRtoCL").SetActive(false);
                }
                else
                {
                    if (controllerInChain[chain - 2] == 0)
                        manager.GetElement("ChainStraightBLtoCR").SetActive(false);
                    else
                        manager.GetElement("ChainStraightBRtoCL").SetActive(false);
                }
                ElementObjectManager targetCardB;
                if (controllerInChain[chain - 3] == 0)
                {
                    targetCardB = manager.GetElement<ElementObjectManager>("DummyChainCardBL");
                    manager.GetElement("ChainCardSetBROffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Tens"),
                        chain - 2);
                }
                else
                {
                    targetCardB = manager.GetElement<ElementObjectManager>("DummyChainCardBR");
                    manager.GetElement("ChainCardSetBLOffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Tens"),
                        chain - 2);
                }
                StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardB, codesInChain[chain - 3], 0, true));

                if (chain > 3)
                {
                    if (controllerInChain[chain - 3] == controllerInChain[chain - 4])
                    {
                        manager.GetElement("ChainStraightALtoBR").SetActive(false);
                        manager.GetElement("ChainStraightARtoBL").SetActive(false);
                    }
                    else
                    {
                        if (controllerInChain[chain - 3] == 0)
                            manager.GetElement("ChainStraightALtoBR").SetActive(false);
                        else
                            manager.GetElement("ChainStraightARtoBL").SetActive(false);
                    }
                    ElementObjectManager targetCardA;
                    if (controllerInChain[chain - 4] == 0)
                    {
                        targetCardA = manager.GetElement<ElementObjectManager>("DummyChainCardAL");
                        manager.GetElement("ChainCardSetAROffset").SetActive(false);
                        ChangeChainNumber(
                            manager.GetElement<SpriteRenderer>("ChainNumAL_Digit"),
                            manager.GetElement<SpriteRenderer>("ChainNumAL_Ones"),
                            manager.GetElement<SpriteRenderer>("ChainNumAL_Tens"),
                            chain - 3);
                    }
                    else
                    {
                        targetCardA = manager.GetElement<ElementObjectManager>("DummyChainCardAR");
                        manager.GetElement("ChainCardSetALOffset").SetActive(false);
                        ChangeChainNumber(
                            manager.GetElement<SpriteRenderer>("ChainNumAR_Digit"),
                            manager.GetElement<SpriteRenderer>("ChainNumAR_Ones"),
                            manager.GetElement<SpriteRenderer>("ChainNumAR_Tens"),
                            chain - 3);
                    }
                    StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardA, codesInChain[chain - 4], 0, true));
                }
                else
                {
                    manager.GetElement("ChainStraightALtoBR").SetActive(false);
                    manager.GetElement("ChainStraightARtoBL").SetActive(false);
                    manager.GetElement("ChainCardSetALOffset").SetActive(false);
                    manager.GetElement("ChainCardSetAROffset").SetActive(false);
                }
            }
        }
        float ShowChainResolve(int chain)
        {
            if (cardsInChain.Count == 1)
                return 0;
            if (!CheckChain())
                return 0;

            GameObject animation;
            if (chain == 1)
                animation = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelChain/DuelChainResolve01", true);
            else if (chain == 2)
                animation = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelChain/DuelChainResolve02", true);
            else
                animation = ABLoader.LoadFromFile("MasterDuel/Timeline/DuelChain/DuelChainResolve03", true);
            var director = animation.GetComponent<PlayableDirector>();
            var mono = animation.AddComponent<DoWhenPlayableDirectorStop>();
            mono.action = () =>
            {
                Destroy(animation);
            };
            var manager = animation.GetComponent<ElementObjectManager>();

            ElementObjectManager targetCardD;
            if (controllerInChain[chain - 1] == 0)
            {
                targetCardD = manager.GetElement<ElementObjectManager>("DummyChainCardDL");
                manager.GetElement("ChainCardSetDROffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumDL_Tens"),
                    chain);
            }
            else
            {
                targetCardD = manager.GetElement<ElementObjectManager>("DummyChainCardDR");
                manager.GetElement("ChainCardSetDLOffset").SetActive(false);
                ChangeChainNumber(
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Digit"),
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Ones"),
                    manager.GetElement<SpriteRenderer>("ChainNumDR_Tens"),
                    chain);
            }
            StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardD, codesInChain[chain - 1], 0, true));

            if (chain > 1)
            {
                if (chain != cardsInChain.Count)
                {
                    manager.GetComponent<PlayableDirector>().time = 0.83f;
                    manager.GetElement("ResolveTextSet").SetActive(false);
                }
                if (controllerInChain[chain - 1] == controllerInChain[chain - 2])
                {
                    manager.GetElement("ChainStraightCLtoDR").SetActive(false);
                    manager.GetElement("ChainStraightCRtoDL").SetActive(false);
                }
                else
                {
                    if (controllerInChain[chain - 1] == 0)
                        manager.GetElement("ChainStraightCLtoDR").SetActive(false);
                    else
                        manager.GetElement("ChainStraightCRtoDL").SetActive(false);
                }

                ElementObjectManager targetCardC;
                if (controllerInChain[chain - 2] == 0)
                {
                    targetCardC = manager.GetElement<ElementObjectManager>("DummyChainCardCL");
                    manager.GetElement("ChainCardSetCROffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumCL_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumCL_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumCL_Tens"),
                        chain - 1);
                }
                else
                {
                    targetCardC = manager.GetElement<ElementObjectManager>("DummyChainCardCR");
                    manager.GetElement("ChainCardSetCLOffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumCR_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumCR_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumCR_Tens"),
                        chain - 1);
                }
                StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardC, codesInChain[chain - 2], 0, true));
            }

            if (chain > 2)
            {
                if (controllerInChain[chain - 2] == controllerInChain[chain - 3])
                {
                    manager.GetElement("ChainStraightBLtoCR").SetActive(false);
                    manager.GetElement("ChainStraightBRtoCL").SetActive(false);
                }
                else
                {
                    if (controllerInChain[chain - 2] == 0)
                        manager.GetElement("ChainStraightBLtoCR").SetActive(false);
                    else
                        manager.GetElement("ChainStraightBRtoCL").SetActive(false);
                }

                ElementObjectManager targetCardB;
                if (controllerInChain[chain - 3] == 0)
                {
                    targetCardB = manager.GetElement<ElementObjectManager>("DummyChainCardBL");
                    manager.GetElement("ChainCardSetBROffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumBL_Tens"),
                        chain - 2);
                }
                else
                {
                    targetCardB = manager.GetElement<ElementObjectManager>("DummyChainCardBR");
                    manager.GetElement("ChainCardSetBLOffset").SetActive(false);
                    ChangeChainNumber(
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Digit"),
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Ones"),
                        manager.GetElement<SpriteRenderer>("ChainNumBR_Tens"),
                        chain - 2);
                }
                StartCoroutine(Program.instance.texture_.LoadDummyCard(targetCardB, codesInChain[chain - 3], 0, true));
            }

            if (chain == 1)
                return 0.95f;
            else if (chain == cardsInChain.Count)
                return 1.84f;
            else
                return 1f;
        }

        void ChangeChainNumber(SpriteRenderer digit, SpriteRenderer one, SpriteRenderer ten, int number)
        {
            if (number < 10)
            {
                one.gameObject.SetActive(false);
                ten.gameObject.SetActive(false);
                digit.sprite = TextureManager.container.GetChainNumSprite(number);
            }
            else
            {
                digit.gameObject.SetActive(false);
                one.sprite = TextureManager.container.GetChainNumSprite(number % 10);
                ten.sprite = TextureManager.container.GetChainNumSprite((number / 10) % 10);
            }
        }


        private void UpdateBgEffect(int player, bool first = false)
        {
            if (player == 0)
            {
                field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.PhaseToDamagePhaseAll);
                //field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhaseToNextPhaseAll);
                if (mate0 != null && !first)
                    mate0.Play(Mate.MateAction.GetDamage);
                if (bgPhase0 == 1 && life0 < (lpLimit * 0.75f))
                {
                    bgPhase0++;
                    var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE1_P";
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2, seLabel);
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase1ToPhase2);
                    grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                    if (stand0Manager != null)
                        stand0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                }
                if (bgPhase0 == 2 && life0 < (lpLimit * 0.5f))
                {
                    bgPhase0++;
                    var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE2_P";
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3, seLabel);
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase2ToPhase3);
                    grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                    if (stand0Manager != null)
                        stand0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                }
                if (bgPhase0 == 3 && life0 < (lpLimit * 0.25f))
                {
                    bgPhase0++;
                    var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE3_P";
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4, seLabel);
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase3ToPhase4);
                    grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                    if (stand0Manager != null)
                        stand0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                    AudioManager.PlayBgmClimax();
                }
                if (bgPhase0 == 4 && life0 <= 0)
                {
                    //bgPhase0++;
                    //var seLabel = "SE_FIELD_MAT" + field0Manager.name.Substring(4, 3) + "_PHASE4_P";
                    //field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd, seLabel);
                    //field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndLose);
                    //field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndWin);
                    //grave0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                    //if (stand0Manager != null)
                    //    stand0Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                }
            }
            else
            {
                field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.PhaseToDamagePhaseAll);
                //field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhaseToNextPhaseAll);
                if (mate1 != null && !first)
                    mate1.Play(Mate.MateAction.GetDamage);
                if (bgPhase1 == 1 && life1 < (lpLimit * 0.75f))
                {
                    bgPhase1++;
                    var seLabel = "SE_FIELD_MAT" + field1Manager.name.Substring(4, 3) + "_PHASE1_P";
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2, seLabel);
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase1ToPhase2);
                    grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                    if (stand1Manager != null)
                        stand1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase1ToPhase2);
                }
                if (bgPhase1 == 2 && life1 < (lpLimit * 0.5f))
                {
                    bgPhase1++;
                    var seLabel = "SE_FIELD_MAT" + field1Manager.name.Substring(4, 3) + "_PHASE2_P";
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3, seLabel);
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase2ToPhase3);
                    grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                    if (stand1Manager != null)
                        stand1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase2ToPhase3);
                }
                if (bgPhase1 == 3 && life1 < (lpLimit * 0.25f))
                {
                    bgPhase1++;
                    var seLabel = "SE_FIELD_MAT" + field1Manager.name.Substring(4, 3) + "_PHASE3_P";
                    field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4, seLabel);
                    field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.OtherSideDamagePhase3ToPhase4);
                    grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                    if (stand1Manager != null)
                        stand1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase3ToPhase4);
                    AudioManager.PlayBgmClimax();
                }
                if (bgPhase1 == 4 && life1 <= 0)
                {
                    //bgPhase1++;
                    //var seLabel = "SE_FIELD_MAT" + field1Manager.name.Substring(4, 3) + "_PHASE4_P";
                    //field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd, seLabel);
                    //field0Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndWin);
                    //field1Manager.PlayAnimatorTrigger(TriggerLabelDefine.EndLose);
                    //grave1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                    //if (stand1Manager != null)
                    //    stand1Manager.PlayAnimatorTrigger(TriggerLabelDefine.DamagePhase4ToEnd);
                }
            }
        }
        void PlayCommonSpecialWin(int[] code)
        {
            var count = code.Length;
            var go = ABLoader.LoadFromFolder("MasterDuel/Timeline/SpecialWin/SpecialWinCommonCard0" + count);
            allGameObjects.Add(go);
            ElementObjectManager mner = null;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                mner = go.transform.GetChild(i).GetComponent<ElementObjectManager>();
                if (mner == null)
                    Destroy(go.transform.GetChild(i).gameObject);
            }
            foreach (var child in mner.transform.GetComponentsInChildren<Transform>(true))
                if (child.name == "White")
                {
                    //var newWhite = Instantiate(child.gameObject);
                    //newWhite.transform.SetParent(child.transform, false);
                    //newWhite.transform.localScale = Vector3.one;
                    //newWhite.GetComponent<SpriteRenderer>().color = Color.clear;
                    child.gameObject.SetActive(false);
                }
            StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard01"), code[0], 0, true));
            mner.GetElement<ElementObjectManager>("DummyCard01").GetElement<Renderer>("DummyCardModel_front").material.renderQueue = 4000;
            if (count > 1)
                StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard02"), code[1], 0, true));
            if (count > 2)
                StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard03"), code[2], 0, true));
            if (count > 3)
                StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard04"), code[3], 0, true));
            if (count > 4)
                StartCoroutine(Program.instance.texture_.LoadDummyCard(mner.GetElement<ElementObjectManager>("DummyCard05"), code[4], 0, true));
            mner.GetComponent<PlayableDirector>().Play();
            var mono = mner.gameObject.AddComponent<DoWhenPlayableDirectorStop>();
            mono.action = () =>
            {
                Destroy(go);
            };
        }
        ElementObjectManager PlaySpecialWin(string path)
        {
            var go = ABLoader.LoadFromFolder("MasterDuel/Timeline/SpecialWin/" + path);
            allGameObjects.Add(go);
            ElementObjectManager manager = null;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                manager = go.transform.GetChild(i).GetComponent<ElementObjectManager>();
                if (manager == null)
                    Destroy(go.transform.GetChild(i).gameObject);
            }
            var mono = manager.gameObject.AddComponent<DoWhenPlayableDirectorStop>();
            mono.action = () => { Destroy(go); };
            return manager;
        }

        #endregion

        #region Voice

        ChatItemHandler duelChat0;
        ChatItemHandler duelChat1;

        private void VoiceMessage(Package p)
        {
            speaking = true;
            var voiceData = VoiceHelper.GetVoiceDatas(p);
            if (NeedVoice())
            {
                if (!charaFaceSetting)
                    SetCharacterDefaultFace();

                if (voiceData == null || voiceData.Count == 0)
                {
                    speaking = false;
                    PracticalizeMessage(p);
                    GetUI<OcgCoreUI>().DuelLog.LogMessage(p);
                    return;
                }
            }
            else
            {
                CloseCharaFace();

                speaking = false;
                PracticalizeMessage(p);
                GetUI<OcgCoreUI>().DuelLog.LogMessage(p);
                return;
            }

            StartCoroutine(PlayVoiceAsync(p, voiceData));
        }

        private bool NeedVoice()
        {
            return Config.GetBool(condition + "Voice", false);
        }

        IEnumerator PlayVoiceAsync(Package p, List<VoiceHelper.VoiceData> data)
        {
            var paths = VoiceHelper.GetVoicePaths(data);
            var clips = new List<AudioClip>[paths.Length];
            for (int i = 0; i < clips.Length; i++)
                clips[i] = new List<AudioClip>();

            for (int i = 0; i < paths.Length; i++)
            {
                for (int j = 0; j < paths[i].Count; j++)
                {
                    var ie = AudioManager.LoadAudioFileAsync(paths[i][j], AudioType.OGGVORBIS);
                    while (ie.MoveNext())
                        yield return null;
                    clips[i].Add(ie.Current);
                }
            }

            for (int i = 0; i < clips.Length; i++)
            {
                if (!speaking && !waitForNoWaitingVoice)
                    break;
                for (int j = 0; j < clips[i].Count; j++)
                {
                    if (!speaking && !waitForNoWaitingVoice)
                        break;

                    var line = VoiceHelper.GetLine(Path.GetFileNameWithoutExtension(paths[i][j]), data[i].me);

                    if (j == 0)
                        yield return new WaitForSeconds(data[i].delay);

                    if (line != null)
                    {
                        var item = Instantiate(data[i].me ? container.duelChatItemMe : container.duelChatItemOp);
                        item.transform.SetParent(transform.GetChild(0), false);
                        var handler = item.GetComponent<ChatItemHandler>();
                        handler.text = line.text;
                        if (clips[i][j] == null)
                        {
                            Debug.LogError("Voice File " + paths[i][j] + " not Found!");

                            speaking = false;
                            speakBreaking = false;
                            PracticalizeMessage(p);
                            GetUI<OcgCoreUI>().DuelLog.LogMessage(p);
                            yield break;
                        }
                        handler.time = clips[i][j].length;
                        handler.frame = line.frame;
                        if (data[i].me)
                        {
                            if (duelChat0 != null)
                                duelChat0.BeGray();
                            duelChat0 = handler;
                        }
                        else
                        {
                            if (duelChat1 != null)
                                duelChat1.BeGray();
                            duelChat1 = handler;
                        }
                        StartCoroutine(SetCharacterFaceAsync(data[i].me ? VoiceHelper.hero : VoiceHelper.rival, line.face, data[i].me, 0f));
                        StartCoroutine(SetCharacterFaceAsync(data[i].me ? VoiceHelper.hero : VoiceHelper.rival, 1, data[i].me, clips[i][j].length - 0.1f));
                    }

                    AudioManager.PlayVoice(clips[i][j]);
                    if (data[i].wait)
                    {
                        var timePassed = 0f;
                        while (timePassed <= clips[i][j].length && speaking)
                        {
                            yield return null;
                            timePassed += Time.deltaTime;
                        }
                    }
                    else
                    {
                        if (i == 0 && j == 0 && !waitForNoWaitingVoice)
                        {
                            speaking = false;
                            speakBreaking = false;
                            PracticalizeMessage(p);
                            GetUI<OcgCoreUI>().DuelLog.LogMessage(p);
                            waitForNoWaitingVoice = true;
                        }
                        if (waitForNoWaitingVoice)
                        {
                            var timePassed = 0f;
                            while (timePassed <= clips[i][j].length)
                            {
                                yield return null;
                                timePassed += Time.deltaTime;
                            }
                        }
                    }
                }
            }

            if (!waitForNoWaitingVoice)
            {
                speaking = false;
                speakBreaking = false;
                PracticalizeMessage(p);
                GetUI<OcgCoreUI>().DuelLog.LogMessage(p);
            }
            else
                waitForNoWaitingVoice = false;
        }

        private bool charaFaceSetting;
        Dictionary<string, Sprite> cachedCharaFaces = new Dictionary<string, Sprite>();
        private IEnumerator SetCharacterFaceAsync(string chara, int id, bool isMe, float delay = 0f)
        {
            charaFaceSetting = true;
            yield return new WaitForSeconds(delay);

            if (id == 0)
                id = 1;

            var address = "sn" + chara + "_3_" + id;
            if (!cachedCharaFaces.TryGetValue(address, out var sprite))
            {
                var handle = Addressables.LoadAssetAsync<Sprite>("sn" + chara + "_3_" + id);
                yield return handle;

                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    if (handle.Result.texture.width != handle.Result.texture.height)
                    {
                        var texture = TextureManager.GetCroppingTex(handle.Result.texture, 80, 0, 320, 240);
                        TextureManager.ReplaceTransparentPixelsWithColor(texture, Color.black);
                        sprite = TextureManager.Texture2Sprite(texture);
                    }
                    else
                    {
                        var texture = TextureManager.CreateCenteredTexture(handle.Result.texture, 280, 0, 10);
                        TextureManager.ReplaceTransparentPixelsWithColor(texture, Color.black);
                        sprite = TextureManager.Texture2Sprite(texture);
                    }
                    cachedCharaFaces[address] = sprite;
                }
                else
                    yield break;
            }

            if (isMe)
                GetUI<OcgCoreUI>().AvatarPlayer0.sprite = sprite;
            else
                GetUI<OcgCoreUI>().AvatarPlayer1.sprite = sprite;
        }

        void SetCharacterDefaultFace()
        {
            var hero = Config.Get(condition + "Character0", VoiceHelper.defaultCharacter);
            var rival = Config.Get(condition + "Character1", VoiceHelper.defaultCharacter);
            StartCoroutine(SetCharacterFaceAsync(hero, 1, true, 0f));
            StartCoroutine(SetCharacterFaceAsync(rival, 1, false, 0f));
        }

        #endregion

        #region Enum

        private enum DuelResult
        {
            DisLink,
            Win,
            Lose,
            Draw
        }
        public enum Condition
        {
            N,
            Duel,
            Watch,
            Replay
        }

        public enum ChainCondition
        {
            No = 0,
            All = 1,
            Smart = 2,
        }

        #endregion

        #region Tools

        private T Create<T>(bool addToList = true) where T : MonoBehaviour
        {
            GameObject obj = new(typeof(T).Name);
            T component = obj.AddComponent<T>();
            if (addToList)
                allGameObjects.Add(obj);
            return component;
        }

        #endregion
    }
}
