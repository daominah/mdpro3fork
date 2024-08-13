using DG.Tweening;
using MDPro3.UI;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace MDPro3
{
    public class Setting : Servant
    {
        public ButtonList defaultButton;
        public Button btnSurrender;

        [Header("System")]
        public Slider bgmVol;
        public Slider seVol;
        public Slider voiceVol;
        public Slider fps;
        public Text fpsValue;
        public Slider quality;
        public Text qualityValue;
        public Slider faa;
        public Text faaValue;
        public Slider aaa;
        public Text aaaValue;
        public Slider shadow;
        public Text shadowValue;
        public Button showFPS;
        public Text showFPSValue;
        public Button screen;
        public Text screenEx;
        public Text screenValue;
        public Button resolution;
        public Text resolutionValue;
        public Slider scale;
        public Text scaleValue;
        public Button confirm;
        public Text confirmValue;
        public Slider uiScale;
        public Text uiScaleValue;
        public Button background;
        public Text backgroundValue;
        public Button bgmBy;
        public Text bgmByValue;
        public Button cardLanguage;
        public Text cardLanguageValue;
        public Button language;
        public Text languageValue;

        [Header("Duel")]
        public Button duelAppearance;
        public Text duelAppearanceValue;
        public Button duelCharacter;
        public Text duelCharacterValue;
        public Button duelVoice;
        public Text duelVoiceValue;
        public Button duelCloseup;
        public Text duelCloseupValue;
        public Button duelSummon;
        public Text duelSummonValue;
        public Button duelPendulum;
        public Text duelPendulumValue;
        public Button duelCutin;
        public Text duelCutinValue;
        public Button duelEffect;
        public Text duelEffectValue;
        public Button duelChain;
        public Text duelChainValue;
        public Button duelDice;
        public Text duelDiceValue;
        public Button duelCoin;
        public Text duelCoinValue;
        public Button duelAutoInfo;
        public Text duelAutoInfoValue;
        public Button duelPlayerMessage;
        public Text duelPlayerMessageValue;
        public Button duelSystemMessage;
        public Text duelSystemMessageValue;
        public Slider duelAcc;
        public Text duelAccValue;
        public Button duelAutoAcc;
        public Text duelAutoAccValue;
        public Button duelFaceDown;
        public Text duelFaceDownValue;

        public Button timing;
        public Text timingValue;
        public Button autoRPS;
        public Text autoRPSValue;

        [Header("Watch")]
        public Button watchAppearance;
        public Text watchAppearanceValue;
        public Button watchCharacter;
        public Text watchCharacterValue;
        public Button watchVoice;
        public Text watchVoiceValue;
        public Button watchCloseup;
        public Text watchCloseupValue;
        public Button watchSummon;
        public Text watchSummonValue;
        public Button watchPendulum;
        public Text watchPendulumValue;
        public Button watchCutin;
        public Text watchCutinValue;
        public Button watchEffect;
        public Text watchEffectValue;
        public Button watchChain;
        public Text watchChainValue;
        public Button watchDice;
        public Text watchDiceValue;
        public Button watchCoin;
        public Text watchCoinValue;
        public Button watchAutoInfo;
        public Text watchAutoInfoValue;
        public Button watchPlayerMessage;
        public Text watchPlayerMessageValue;
        public Button watchSystemMessage;
        public Text watchSystemMessageValue;
        public Slider watchAcc;
        public Text watchAccValue;
        public Button watchAutoAcc;
        public Text watchAutoAccValue;
        public Button watchFaceDown;
        public Text watchFaceDownValue;

        [Header("Replay")]
        public Button replayAppearance;
        public Text replayAppearanceValue;
        public Button replayCharacter;
        public Text replayCharacterValue;
        public Button replayVoice;
        public Text replayVoiceValue;
        public Button replayCloseup;
        public Text replayCloseupValue;
        public Button replaySummon;
        public Text replaySummonValue;
        public Button replayPendulum;
        public Text replayPendulumValue;
        public Button replayCutin;
        public Text replayCutinValue;
        public Button replayEffect;
        public Text replayEffectValue;
        public Button replayChain;
        public Text replayChainValue;
        public Button replayDice;
        public Text replayDiceValue;
        public Button replayCoin;
        public Text replayCoinValue;
        public Button replayAutoInfo;
        public Text replayAutoInfoValue;
        public Button replayPlayerMessage;
        public Text replayPlayerMessageValue;
        public Button replaySystemMessage;
        public Text replaySystemMessageValue;
        public Slider replayAcc;
        public Text replayAccValue;
        public Button replayAutoAcc;
        public Text replayAutoAccValue;
        public Button replayFaceDown;
        public Text replayFaceDownValue;

        [Header("Port")]
        public Button import;
        public Button importBG;
        public Button exportDeck;
        public Button exportReplay;
        public Button exportPicture;
        public Button clearPicture;

        [Header("Expansions")]
        public Button supportExpansions;
        public Text supportExpansionsValue;
        public Button clearExpansions;
        public Button updatePrerelease;
        public Text updatePrereleaseValue;

        public override void Initialize()
        {
            depth = 1;
            haveLine = false;
            blackAlpha = 0.6f;
            subBlackAlpha = 0.9f;
            returnServant = Program.I().menu;
            base.Initialize();

            QualitySettings.vSyncCount = 0;

            bgmVol.onValueChanged.AddListener(OnBgmVolChange);
            seVol.onValueChanged.AddListener(OnSeVolChange);
            voiceVol.onValueChanged.AddListener(OnVoiceVolChange);
            fps.onValueChanged.AddListener(OnFpsChange);
            scale.onValueChanged.AddListener(OnScaleChange);
            uiScale.onValueChanged.AddListener(OnUIScaleChange);
            quality.onValueChanged.AddListener(OnQualityChange);
            faa.onValueChanged.AddListener(OnFAAChange);
            aaa.onValueChanged.AddListener(OnAAAChange);
            shadow.onValueChanged.AddListener(OnShadowChange);
            showFPS.onClick.AddListener(OnShowFPSClicked);
            screen.onClick.AddListener(OnScreenModeChange);
            resolution.onClick.AddListener(OnResolutionChange);
            background.onClick.AddListener(OnBackground);
            cardLanguage.onClick.AddListener(OnCardLanguageChange);
            language.onClick.AddListener(OnLanguageChange);
            confirm.onClick.AddListener(OnConfirmClicked);
            autoRPS.onClick.AddListener(OnAutoRPS);
            bgmBy.onClick.AddListener(OnBgmByClicked);

            duelAppearance.onClick.AddListener(OnDuelAppearcanceClick);
            watchAppearance.onClick.AddListener(OnWatchAppearcanceClick);
            replayAppearance.onClick.AddListener(OnReplayAppearcanceClick);
            duelCharacter.onClick.AddListener(OnDuelCharacterClick);
            watchCharacter.onClick.AddListener(OnWatchCharacterClick);
            replayCharacter.onClick.AddListener(OnReplayCharacterClick);
            duelVoice.onClick.AddListener(OnDuelVoiceClick);
            watchVoice.onClick.AddListener(OnWatchVoiceClick);
            replayVoice.onClick.AddListener(OnReplayVoiceClick);
            duelCloseup.onClick.AddListener(OnDuelCloseupClick);
            watchCloseup.onClick.AddListener(OnWatchCloseupClick);
            replayCloseup.onClick.AddListener(OnReplayCloseupClick);
            duelSummon.onClick.AddListener(OnDuelSummonClick);
            watchSummon.onClick.AddListener(OnWatchSummonClick);
            replaySummon.onClick.AddListener(OnReplaySummonClick);
            duelPendulum.onClick.AddListener(OnDuelPendulumClick);
            watchPendulum.onClick.AddListener(OnWatchPendulumClick);
            replayPendulum.onClick.AddListener(OnReplayPendulumClick);
            duelCutin.onClick.AddListener(OnDuelCutinClick);
            watchCutin.onClick.AddListener(OnWatchCutinClick);
            replayCutin.onClick.AddListener(OnReplayCutinClick);
            duelEffect.onClick.AddListener(OnDuelEffectClick);
            watchEffect.onClick.AddListener(OnWatchEffectClick);
            replayEffect.onClick.AddListener(OnReplayEffectClick);
            duelChain.onClick.AddListener(OnDuelChainClick);
            watchChain.onClick.AddListener(OnWatchChainClick);
            replayChain.onClick.AddListener(OnReplayChainClick);
            duelDice.onClick.AddListener(OnDuelDiceClick);
            watchDice.onClick.AddListener(OnWatchDiceClick);
            replayDice.onClick.AddListener(OnReplayDiceClick);
            duelCoin.onClick.AddListener(OnDuelCoinClick);
            watchCoin.onClick.AddListener(OnWatchCoinClick);
            replayCoin.onClick.AddListener(OnReplayCoinClick);
            duelAutoInfo.onClick.AddListener(OnDuelAutoInfoClick);
            watchAutoInfo.onClick.AddListener(OnWatchAutoInfoClick);
            replayAutoInfo.onClick.AddListener(OnReplayAutoInfoClick);
            duelPlayerMessage.onClick.AddListener(OnDuelPlayerMessageClick);
            watchPlayerMessage.onClick.AddListener(OnWatchPlayerMessageClick);
            replayPlayerMessage.onClick.AddListener(OnReplayPlayerMessageClick);
            duelSystemMessage.onClick.AddListener(OnDuelSystemMessageClick);
            watchSystemMessage.onClick.AddListener(OnWatchSystemMessageClick);
            replaySystemMessage.onClick.AddListener(OnReplaySystemMessageClick);
            duelAcc.onValueChanged.AddListener(OnDuelAccChange);
            watchAcc.onValueChanged.AddListener(OnWatchAccChange);
            replayAcc.onValueChanged.AddListener(OnReplayAccChange);
            duelAutoAcc.onClick.AddListener(OnDuelAutoAccClick);
            watchAutoAcc.onClick.AddListener(OnWatchAutoAccClick);
            replayAutoAcc.onClick.AddListener(OnReplayAutoAccClick);
            duelFaceDown.onClick.AddListener(OnDuelFaceDownClick);
            watchFaceDown.onClick.AddListener(OnWatchFaceDownClick);
            replayFaceDown.onClick.AddListener(OnReplayFaceDownClick);

            timing.onClick.AddListener(OnTimingClick);

            import.onClick.AddListener(OnImport);
            importBG.onClick.AddListener(OnImportBG);

            exportDeck.onClick.AddListener(OnExportDecks);
            exportReplay.onClick.AddListener(OnExportReplays);
            exportPicture.onClick.AddListener(OnExportPictures);
            clearPicture.onClick.AddListener(OnClearPictures);
            clearExpansions.onClick.AddListener(OnClearExpansions);
            supportExpansions.onClick.AddListener(OnSupportExpansions);
            updatePrerelease.onClick.AddListener(OnUpdatePrerelease);

            bgmVol.value = Config.GetFloat("BgmVol", 0.7f);
            OnBgmVolChange(bgmVol.value);
            seVol.value = Config.GetFloat("SeVol", 0.7f);
            OnSeVolChange(seVol.value);
            voiceVol.value = Config.GetFloat("VoiceVol", 0.7f);
            OnVoiceVolChange(voiceVol.value);
            fps.value = Config.GetFloat("FPS", 60f);
            OnFpsChange(fps.value);

            var defau = 1f;
#if UNITY_ANDROID
            defau = 0.5f;
#endif
            scale.value = Config.GetFloat("Scale", defau);
            OnScaleChange(scale.value);

            defau = 1f;
#if UNITY_ANDROID
            defau = 1.5f;
#endif
            uiScale.value = Config.GetFloat("UIScale", defau);
            quality.value = Config.GetFloat("Quality", 3f);
            OnQualityChange(quality.value);
            faa.value = Config.GetFloat("FAA", 1);
            OnFAAChange(faa.value);
            aaa.value = Config.GetFloat("AAA", 0);
            OnAAAChange(aaa.value);
            shadow.value = Config.GetFloat("Shadow", 0);
            OnShadowChange(shadow.value);

            duelAcc.value = Config.GetFloat("DuelAcc", 2f);
            OnDuelAccChange(duelAcc.value);
            watchAcc.value = Config.GetFloat("WatchAcc", 2f);
            OnWatchAccChange(watchAcc.value);
            replayAcc.value = Config.GetFloat("ReplayAcc", 2f);
            OnReplayAccChange(replayAcc.value);

            InitializeShowFPS();
            InitializeScreenMode();
            InitializeResolution();
            InitializeConfirm();
            InitializeBackground();
            InitializeCardLanguage();
            InitializeLanguage();
            InitializeSwitches();
            InitializeBgmBy();
        }

        public override void Show(int preDepth)
        {
            base.Show(preDepth);
            if (Program.I().currentServant == Program.I().ocgcore)
            {
                Program.I().currentSubServant = this;
                UIManager.ShowFPSRight();
                btnSurrender.gameObject.SetActive(true);
            }
            else
                btnSurrender.gameObject.SetActive(false);
        }
        public override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            if (preDepth <= depth)
                defaultButton.SelectThis();
            RefreshCharacterName();
        }

        public override void OnExit()
        {
            base.OnExit();
            Save();
            if (Program.I().currentServant == Program.I().ocgcore)
                UIManager.ShowFPSLeft();

        }

        #region setting

        public void RefreshCharacterName()
        {
            if (Program.I().character.characters == null)
                return;

            var character = Config.Get("DuelCharacter0", VoiceHelper.defaultCharacter);
            duelCharacterValue.text = Program.I().character.characters.GetName(character);
            character = Config.Get("WatchCharacter0", VoiceHelper.defaultCharacter);
            watchCharacterValue.text = Program.I().character.characters.GetName(character);
            character = Config.Get("ReplayCharacter0", VoiceHelper.defaultCharacter);
            replayCharacterValue.text = Program.I().character.characters.GetName(character);
        }

        public void Save()
        {
            Config.SetFloat("BgmVol", bgmVol.value);
            Config.SetFloat("SeVol", seVol.value);
            Config.SetFloat("VoiceVol", voiceVol.value);
            Config.SetFloat("FPS", fps.value);
            Config.SetFloat("Scale", scale.value);
            Config.SetFloat("UIScale", uiScale.value);
            Config.SetFloat("Quality", quality.value);
            Config.SetFloat("FAA", faa.value);
            Config.SetFloat("AAA", aaa.value);
            Config.SetFloat("Shadow", shadow.value);
            Config.Set("ShowFPS", SaveBool(showFPSValue.text));
            Config.Set("ScreenMode", SaveScreenMode(screenValue.text));
            Config.Set("Resolution", resolutionValue.text);
            Config.Set("CardLanguage", InterString.GetOriginal(cardLanguageValue.text));
            Config.Set("Language", InterString.GetOriginal(languageValue.text));
            Config.Set("Confirm", SaveBool(confirmValue.text));

            Config.Set("DuelSummon", SaveBool(duelSummonValue.text));
            Config.Set("WatchSummon", SaveBool(watchSummonValue.text));
            Config.Set("ReplaySummon", SaveBool(replaySummonValue.text));
            Config.Set("DuelPendulum", SaveBool(duelPendulumValue.text));
            Config.Set("WatchPendulum", SaveBool(watchPendulumValue.text));
            Config.Set("ReplayPendulum", SaveBool(replayPendulumValue.text));
            Config.Set("DuelCutin", SaveBool(duelCutinValue.text));
            Config.Set("WatchCutin", SaveBool(watchCutinValue.text));
            Config.Set("ReplayCutin", SaveBool(replayCutinValue.text));
            Config.Set("DuelEffect", SaveBool(duelEffectValue.text));
            Config.Set("WatchEffect", SaveBool(watchEffectValue.text));
            Config.Set("ReplayEffect", SaveBool(replayEffectValue.text));
            Config.Set("DuelChain", SaveBool(duelChainValue.text));
            Config.Set("WatchChain", SaveBool(watchChainValue.text));
            Config.Set("ReplayChain", SaveBool(replayChainValue.text));
            Config.Set("DuelDice", SaveBool(duelDiceValue.text));
            Config.Set("WatchDice", SaveBool(watchDiceValue.text));
            Config.Set("ReplayDice", SaveBool(replayDiceValue.text));
            Config.Set("DuelCoin", SaveBool(duelCoinValue.text));
            Config.Set("WatchCoin", SaveBool(watchCoinValue.text));
            Config.Set("ReplayCoin", SaveBool(replayCoinValue.text));
            Config.Set("DuelAutoInfo", SaveBool(duelAutoInfoValue.text));
            Config.Set("WatchAutoInfo", SaveBool(watchAutoInfoValue.text));
            Config.Set("ReplayAutoInfo", SaveBool(replayAutoInfoValue.text));
            Config.Set("DuelPlayerMessage", SaveBool(duelPlayerMessageValue.text));
            Config.Set("WatchPlayerMessage", SaveBool(watchPlayerMessageValue.text));
            Config.Set("ReplayPlayerMessage", SaveBool(replayPlayerMessageValue.text));
            Config.Set("DuelSystemMessage", SaveBool(duelSystemMessageValue.text));
            Config.Set("WatchSystemMessage", SaveBool(watchSystemMessageValue.text));
            Config.Set("ReplaySystemMessage", SaveBool(replaySystemMessageValue.text));
            Config.Set("DuelAutoAcc", SaveBool(duelAutoAccValue.text));
            Config.Set("WatchAutoAcc", SaveBool(watchAutoAccValue.text));
            Config.Set("ReplayAutoAcc", SaveBool(replayAutoAccValue.text));

            Config.Set("Timing", SaveBool(timingValue.text));
            Config.Set("Expansions", SaveBool(supportExpansionsValue.text));

            Config.Save();
        }
        public string SaveBool(string value)
        {
            string returnValue = Config.stringNo;
            if (value == InterString.Get("开"))
                returnValue = Config.stringYes;
            if (value == InterString.Get("有"))
                returnValue = Config.stringYes;
            if (value == InterString.Get("左"))
                returnValue = Config.stringYes;
            if (value == InterString.Get("是"))
                returnValue = Config.stringYes;
            return returnValue;
        }
        public void OnBgmVolChange(float vol)
        {
            AudioManager.SetBGMVol(vol);
        }
        public void OnSeVolChange(float vol)
        {
            AudioManager.SetSeVol(vol);
        }
        public void OnVoiceVolChange(float vol)
        {
            AudioManager.SetVoiceVol(vol);
        }
        public void OnFpsChange(float value)
        {
            QualitySettings.vSyncCount = 0;
            if (value > 0f && value < 30f)
                value = 30f;
            Application.targetFrameRate = (int)value;
            fpsValue.text = ((int)value).ToString();
        }

        public void OnScaleChange(float vol)
        {
            string value = vol.ToString();
            value = value.Length > 4 ? value.Substring(0, 4) : value;
            scaleValue.text = value;
            Program.I().camera_.urpAsset.renderScale = float.Parse(value);
        }
        public void OnUIScaleChange(float vol)
        {
            string value = vol.ToString();
            value = value.Length > 4 ? value.Substring(0, 4) : value;
            uiScaleValue.text = value;
        }

        public void OnQualityChange(float value)
        {
            string qualityText;
            switch ((int)value)
            {
                case 0:
                    qualityText = InterString.Get("非常低");
                    break;
                case 1:
                    qualityText = InterString.Get("低");
                    break;
                case 2:
                    qualityText = InterString.Get("中等");
                    break;
                case 3:
                    qualityText = InterString.Get("高");
                    break;
                case 4:
                    qualityText = InterString.Get("非常高");
                    break;
                case 5:
                    qualityText = InterString.Get("极致");
                    break;
                default:
                    qualityText = InterString.Get("中等");
                    break;
            }
            Config.SetFloat("Quality", (int)value);
            qualityValue.text = qualityText;
        }
        public void OnFAAChange(float value)
        {
            switch ((int)value)
            {
                case 1:
                    faaValue.text = InterString.Get("Off");
                    Program.I().camera_.urpAsset.msaaSampleCount = 1;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 1;
                    break;
                case 2:
                    faaValue.text = "MSAA 2x";
                    Program.I().camera_.urpAsset.msaaSampleCount = 2;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 2;
                    break;
                case 3:
                    faaValue.text = "MSAA 4x";
                    Program.I().camera_.urpAsset.msaaSampleCount = 4;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 4;
                    break;
                case 4:
                    faaValue.text = "MSAA 8x";
                    Program.I().camera_.urpAsset.msaaSampleCount = 8;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 8;
                    break;
            }
        }
        public void OnAAAChange(float value)
        {
            var cameraData3D = Program.I().camera_.cameraMain.GetUniversalAdditionalCameraData();
            var cameraData2D = Program.I().camera_.camera2D.GetUniversalAdditionalCameraData();

            OnFAAChange(faa.value);

            switch ((int)value)
            {
                case 0:
                    aaaValue.text = InterString.Get("Off");
                    cameraData3D.antialiasing = AntialiasingMode.None;
                    break;
                case 1:
                    aaaValue.text = "FAA";
                    cameraData3D.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case 2:
                    aaaValue.text = "SMAA Low";
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.Low;
                    break;
                case 3:
                    aaaValue.text = "SMAA Medium";
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.Medium;
                    break;
                case 4:
                    aaaValue.text = "SMAA High";
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.High;
                    break;
                case 5:
                    aaaValue.text = "TAA";
                    cameraData3D.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                    Program.I().camera_.urpAsset.msaaSampleCount = 1;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 1;
                    break;
            }
        }
        public void OnShadowChange(float value)
        {
            SROptions sr = new SROptions();
            switch ((int)value)
            {
                case 0:
                    shadowValue.text = InterString.Get("非常低");
                    sr.MainLightShadowResolution = ShadowResolution._256;
                    sr.SupportsSoftShadows = false;
                    break;
                case 1:
                    shadowValue.text = InterString.Get("低");
                    sr.MainLightShadowResolution = ShadowResolution._512;
                    sr.SupportsSoftShadows = false;
                    break;
                case 2:
                    shadowValue.text = InterString.Get("中等");
                    sr.MainLightShadowResolution = ShadowResolution._1024;
                    sr.SupportsSoftShadows = false;
                    break;
                case 3:
                    shadowValue.text = InterString.Get("高");
                    sr.MainLightShadowResolution = ShadowResolution._2048;
                    sr.SupportsSoftShadows = true;
                    break;
                case 4:
                    shadowValue.text = InterString.Get("非常高");
                    sr.MainLightShadowResolution = ShadowResolution._4096;
                    sr.SupportsSoftShadows = true;
                    break;
            }
        }
        public void InitializeShowFPS()
        {
            var value = Config.GetBool("ShowFPS", true);
            if (value)
            {
                showFPSValue.text = InterString.Get("开");
                UIManager.ShowFPS();
            }
            else
            {
                showFPSValue.text = InterString.Get("关");
                UIManager.HideFPS();
            }
        }
        public void OnShowFPSClicked()
        {
            if (showFPSValue.text == InterString.Get("开"))
            {
                showFPSValue.text = InterString.Get("关");
                UIManager.HideFPS();
            }
            else
            {
                showFPSValue.text = InterString.Get("开");
                UIManager.ShowFPS();
            }
        }
        public string SaveScreenMode(string value)
        {
            string returnValue = "1";
            if (value == InterString.Get("独占全屏"))
                returnValue = "0";
            else if (value == InterString.Get("窗口全屏"))
                returnValue = "1";
            else if (value == InterString.Get("窗口化"))
                returnValue = "2";
            return returnValue;
        }
        public void InitializeScreenMode()
        {
            string value = Config.Get("ScreenMode", "1");

            if (value == "0")
            {
                screenEx.text = InterString.Get("独占全屏");
                screenValue.text = InterString.Get("独占全屏");
            }
            else if (value == "1")
            {
                screenEx.text = InterString.Get("窗口全屏");
                screenValue.text = InterString.Get("窗口全屏");
            }
            else
            {
                screenEx.text = InterString.Get("窗口化（仅桌面端有效）");
                screenValue.text = InterString.Get("窗口化");
            }
        }
        public void OnScreenModeChange()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("显示模式"),
                InterString.Get("独占全屏"),
                InterString.Get("窗口全屏"),
                InterString.Get("窗口化")
            };
            UIManager.ShowPopupSelection(selections, OnScreenModeSelection);
        }
        public void OnScreenModeSelection()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            if (selected == InterString.Get("独占全屏"))
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.ExclusiveFullScreen);
                screenEx.text = InterString.Get("独占全屏（仅Windows端有效）");
                screenValue.text = InterString.Get("独占全屏");
            }
            else if (selected == InterString.Get("窗口全屏"))
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
                screenEx.text = InterString.Get("窗口全屏");
                screenValue.text = InterString.Get("窗口全屏");
            }
            else
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, false);
                screenEx.text = InterString.Get("窗口化（仅桌面端有效）");
                screenValue.text = InterString.Get("窗口化");
            }
        }
        public void InitializeResolution()
        {
            string resolution = $"{Screen.width} x {Screen.height}";

#if UNITY_ANDROID
            if (Config.Have("Resolution"))
                resolution = Config.Get("Resolution", "1920 x 1080");
            Screen.SetResolution(int.Parse(Regex.Split(resolution, " x ")[0]), int.Parse(Regex.Split(resolution, " x ")[1]), FullScreenMode.FullScreenWindow);
#endif
            resolutionValue.text = resolution;
        }
        public void OnResolutionChange()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("分辨率")
            };
            foreach (var resolution in Screen.resolutions)
            {
                string selection = Regex.Split(resolution.ToString(), " @ ")[0];
#if !UNITY_EDITOR && UNITY_ANDROID
                int height = int.Parse(Regex.Split(selection, " x ")[0]);
                int width = int.Parse(Regex.Split(selection, " x ")[1]);
                if (height > width)
                {
                    var cache = height;
                    height = width;
                    width = cache;
                }
                if (height > 540)
                {
                    string r = (width * 540 / height).ToString() + " x " + 540.ToString();
                    if(!selections.Contains(r))
                        selections.Add(r);
                }
                if(height > 720)
                {
                    string r = (width * 720 / height).ToString() + " x " + 720.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                if (height > 1080)
                {
                    string r = (width * 1080 / height).ToString() + " x " + 1080.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                if (height > 1200)
                {
                    string r = (width * 1200 / height).ToString() + " x " + 1200.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                if (height > 1440)
                {
                    string r = (width * 1440 / height).ToString() + " x " + 1440.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                if (height > 1600)
                {
                    string r = (width * 1600 / height).ToString() + " x " + 1600.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                if (height > 2160)
                {
                    string r = (width * 2160 / height).ToString() + " x " + 2160.ToString();
                    if (!selections.Contains(r))
                        selections.Add(r);
                }
                selection = width.ToString() + " x " + height.ToString();
#endif
                if (!selections.Contains(selection))
                    selections.Add(selection);
            }
            UIManager.ShowPopupSelection(selections, OnResolutioSelection);
        }
        public void OnResolutioSelection()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            Screen.SetResolution(int.Parse(Regex.Split(selected, " x ")[0]), int.Parse(Regex.Split(selected, " x ")[1]), Screen.fullScreen);
            resolutionValue.text = selected;
        }

        public void InitializeBackground()
        {
            var id = int.Parse(Config.Get("Background", "0"));
            var value = InterString.Get("随机");
            if (id != 0)
                if (!BackgroundManager.backgrounds.TryGetValue(id, out value))
                {
                    id = 1;
                    value = "Classic";
                }
            if (string.IsNullOrEmpty(value))
                value = InterString.Get("随机");
            backgroundValue.text = value;
            Program.I().background_.Change(id);
        }

        public void OnBackground()
        {
            List<string> selections = new List<string>
            {
                InterString.Get("更换背景"),
                InterString.Get("随机"),
            };
            foreach (var background in BackgroundManager.backgrounds)
                selections.Add(background.Value);

            UIManager.ShowPopupSelection(selections, OnBackgroundSelection);
        }

        void OnBackgroundSelection()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                    currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            var id = Program.I().background_.GetIDByName(selected);
            Config.Set("Background", id.ToString());
            InitializeBackground();
        }

        public void InitializeCardLanguage()
        {
            string lan = Config.Get("CardLanguage", "zh-CN");
            cardLanguageValue.text = InterString.Get(lan);
        }
        public void OnCardLanguageChange()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能更改此选项。"));
                return;
            }

            List<string> selections = new List<string>
            {
                InterString.Get("卡图语言")
            };
            DirectoryInfo[] infos = new DirectoryInfo(Program.localesPath).GetDirectories();
            foreach (DirectoryInfo info in infos)
                selections.Add(InterString.Get(info.Name));
            UIManager.ShowPopupSelection(selections, OnCardLanguageSelection);
        }
        public void OnCardLanguageSelection()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                    currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            cardLanguageValue.text = selected;
            Config.Set("CardLanguage", InterString.GetOriginal(selected));
            UIManager.ChangeLanguage();
        }
        public void InitializeLanguage()
        {
            string lan = Config.Get("Language", "zh-CN");
            languageValue.text = InterString.Get(lan);
        }
        public void OnLanguageChange()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能更改此选项。"));
                return;
            }

            List<string> selections = new List<string>
            {
                InterString.Get("语言")
            };
            DirectoryInfo[] infos = new DirectoryInfo(Program.localesPath).GetDirectories();
            foreach (DirectoryInfo info in infos)
                selections.Add(InterString.Get(info.Name));
            UIManager.ShowPopupSelection(selections, OnLanguageSelection);
        }
        public void OnLanguageSelection()
        {
            string selected = UnityEngine.EventSystems.EventSystem.current.
                    currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
            languageValue.text = selected;
            Config.Set("Language", InterString.GetOriginal(selected));
            UIManager.ChangeLanguage();
        }

        public void InitializeConfirm()
        {
            var value = Config.GetBool("Confirm", true);
            if (value)
                confirmValue.text = InterString.Get("左");
            else
                confirmValue.text = InterString.Get("右");
        }
        public void OnConfirmClicked()
        {
            if (confirmValue.text == InterString.Get("右"))
            {
                confirmValue.text = InterString.Get("左");
                Config.SetBool("Confirm", true);
            }
            else
            {
                confirmValue.text = InterString.Get("右");
                Config.SetBool("Confirm", false);
            }
        }

        public void InitializeBgmBy()
        {
            var value = Config.GetBool("BGMbyMySide", true);
            if (value)
                bgmByValue.text = InterString.Get("我方");
            else
                bgmByValue.text = InterString.Get("对方");
        }

        public void OnBgmByClicked()
        {
            var value = Config.GetBool("BGMbyMySide", true);

            if (value)
            {
                bgmByValue.text = InterString.Get("对方");
                Config.SetBool("BGMbyMySide", false);
            }
            else
            {
                bgmByValue.text = InterString.Get("我方");
                Config.SetBool("BGMbyMySide", true);
            }
        }

        public void OnAutoRPS()
        {
            if (autoRPSValue.text == InterString.Get("关"))
            {
                autoRPSValue.text = InterString.Get("开");
                Config.SetBool("AutoRPS", true);
            }
            else
            {
                autoRPSValue.text = InterString.Get("关");
                Config.SetBool("AutoRPS", false);
            }
        }
        public void InitializeSwitches()
        {
            duelAppearanceValue.text = Config.Get("DuelPlayerName0", "@ui");
            watchAppearanceValue.text = Config.Get("WatchPlayerName0", "@ui");
            replayAppearanceValue.text = Config.Get("ReplayPlayerName0", "@ui");

            var value = Config.GetBool("DuelVoice", false);
            if (value)
                duelVoiceValue.text = InterString.Get("开");
            else
                duelVoiceValue.text = InterString.Get("关");
            value = Config.GetBool("WatchVoice", false);
            if (value)
                watchVoiceValue.text = InterString.Get("开");
            else
                watchVoiceValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayVoice", false);
            if (value)
                replayVoiceValue.text = InterString.Get("开");
            else
                replayVoiceValue.text = InterString.Get("关");

            value = Config.GetBool("DuelCloseup", true);
            if (value)
                duelCloseupValue.text = InterString.Get("开");
            else
                duelCloseupValue.text = InterString.Get("关");
            value = Config.GetBool("WatchCloseup", true);
            if (value)
                watchCloseupValue.text = InterString.Get("开");
            else
                watchCloseupValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayCloseup", true);
            if (value)
                replayCloseupValue.text = InterString.Get("开");
            else
                replayCloseupValue.text = InterString.Get("关");

            value = Config.GetBool("DuelSummon", true);
            if (value)
                duelSummonValue.text = InterString.Get("开");
            else
                duelSummonValue.text = InterString.Get("关");
            value = Config.GetBool("WatchSummon", true);
            if (value)
                watchSummonValue.text = InterString.Get("开");
            else
                watchSummonValue.text = InterString.Get("关");
            value = Config.GetBool("ReplaySummon", true);
            if (value)
                replaySummonValue.text = InterString.Get("开");
            else
                replaySummonValue.text = InterString.Get("关");

            value = Config.GetBool("DuelPendulum", true);
            if (value)
                duelPendulumValue.text = InterString.Get("开");
            else
                duelPendulumValue.text = InterString.Get("关");
            value = Config.GetBool("WatchPendulum", true);
            if (value)
                watchPendulumValue.text = InterString.Get("开");
            else
                watchPendulumValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayPendulum", true);
            if (value)
                replayPendulumValue.text = InterString.Get("开");
            else
                replayPendulumValue.text = InterString.Get("关");

            value = Config.GetBool("DuelCutin", true);
            if (value)
                duelCutinValue.text = InterString.Get("开");
            else
                duelCutinValue.text = InterString.Get("关");
            value = Config.GetBool("WatchCutin", true);
            if (value)
                watchCutinValue.text = InterString.Get("开");
            else
                watchCutinValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayCutin", true);
            if (value)
                replayCutinValue.text = InterString.Get("开");
            else
                replayCutinValue.text = InterString.Get("关");

            value = Config.GetBool("DuelEffect", true);
            if (value)
                duelEffectValue.text = InterString.Get("开");
            else
                duelEffectValue.text = InterString.Get("关");
            value = Config.GetBool("WatchEffect", true);
            if (value)
                watchEffectValue.text = InterString.Get("开");
            else
                watchEffectValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayEffect", true);
            if (value)
                replayEffectValue.text = InterString.Get("开");
            else
                replayEffectValue.text = InterString.Get("关");

            value = Config.GetBool("DuelChain", true);
            if (value)
                duelChainValue.text = InterString.Get("开");
            else
                duelChainValue.text = InterString.Get("关");
            value = Config.GetBool("WatchChain", true);
            if (value)
                watchChainValue.text = InterString.Get("开");
            else
                watchChainValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayChain", true);
            if (value)
                replayChainValue.text = InterString.Get("开");
            else
                replayChainValue.text = InterString.Get("关");

            value = Config.GetBool("DuelDice", true);
            if (value)
                duelDiceValue.text = InterString.Get("开");
            else
                duelDiceValue.text = InterString.Get("关");
            value = Config.GetBool("WatchDice", true);
            if (value)
                watchDiceValue.text = InterString.Get("开");
            else
                watchDiceValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayDice", true);
            if (value)
                replayDiceValue.text = InterString.Get("开");
            else
                replayDiceValue.text = InterString.Get("关");

            value = Config.GetBool("DuelCoin", true);
            if (value)
                duelCoinValue.text = InterString.Get("开");
            else
                duelCoinValue.text = InterString.Get("关");
            value = Config.GetBool("WatchCoin", true);
            if (value)
                watchCoinValue.text = InterString.Get("开");
            else
                watchCoinValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayCoin", true);
            if (value)
                replayCoinValue.text = InterString.Get("开");
            else
                replayCoinValue.text = InterString.Get("关");

            value = Config.GetBool("DuelAutoInfo", true);
            if (value)
                duelAutoInfoValue.text = InterString.Get("开");
            else
                duelAutoInfoValue.text = InterString.Get("关");
            value = Config.GetBool("WatchAutoInfo", true);
            if (value)
                watchAutoInfoValue.text = InterString.Get("开");
            else
                watchAutoInfoValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayAutoInfo", true);
            if (value)
                replayAutoInfoValue.text = InterString.Get("开");
            else
                replayAutoInfoValue.text = InterString.Get("关");

            value = Config.GetBool("DuelPlayerMessage", true);
            if (value)
                duelPlayerMessageValue.text = InterString.Get("开");
            else
                duelPlayerMessageValue.text = InterString.Get("关");
            value = Config.GetBool("WatchPlayerMessage", true);
            if (value)
                watchPlayerMessageValue.text = InterString.Get("开");
            else
                watchPlayerMessageValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayPlayerMessage", true);
            if (value)
                replayPlayerMessageValue.text = InterString.Get("开");
            else
                replayPlayerMessageValue.text = InterString.Get("关");

            value = Config.GetBool("DuelSystemMessage", true);
            if (value)
                duelSystemMessageValue.text = InterString.Get("开");
            else
                duelSystemMessageValue.text = InterString.Get("关");
            value = Config.GetBool("WatchSystemMessage", true);
            if (value)
                watchSystemMessageValue.text = InterString.Get("开");
            else
                watchSystemMessageValue.text = InterString.Get("关");
            value = Config.GetBool("ReplaySystemMessage", true);
            if (value)
                replaySystemMessageValue.text = InterString.Get("开");
            else
                replaySystemMessageValue.text = InterString.Get("关");

            value = Config.GetBool("DuelAutoAcc", false);
            if (value)
                duelAutoAccValue.text = InterString.Get("开");
            else
                duelAutoAccValue.text = InterString.Get("关");
            value = Config.GetBool("WatchAutoAcc", false);
            if (value)
                watchAutoAccValue.text = InterString.Get("开");
            else
                watchAutoAccValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayAutoAcc", false);
            if (value)
                replayAutoAccValue.text = InterString.Get("开");
            else
                replayAutoAccValue.text = InterString.Get("关");

            value = Config.GetBool("DuelFaceDown", true);
            if (value)
                duelFaceDownValue.text = InterString.Get("开");
            else
                duelFaceDownValue.text = InterString.Get("关");
            value = Config.GetBool("WatchFaceDown", true);
            if (value)
                watchFaceDownValue.text = InterString.Get("开");
            else
                watchFaceDownValue.text = InterString.Get("关");
            value = Config.GetBool("ReplayFaceDown", true);
            if (value)
                replayFaceDownValue.text = InterString.Get("开");
            else
                replayFaceDownValue.text = InterString.Get("关");

            value = Config.GetBool("Timing", true);
            if (value)
                timingValue.text = InterString.Get("开");
            else
                timingValue.text = InterString.Get("关");

            value = Config.GetBool("AutoRPS", false);
            if (value)
                autoRPSValue.text = InterString.Get("开");
            else
                autoRPSValue.text = InterString.Get("关");

            value = Config.GetBool("Expansions", true);
            if (value)
                supportExpansionsValue.text = InterString.Get("是");
            else
                supportExpansionsValue.text = InterString.Get("否");
        }
        public void OnDuelAppearcanceClick()
        {
            Program.I().appearance.SwitchCondition(Appearance.Condition.Duel);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnWatchAppearcanceClick()
        {
            Program.I().appearance.SwitchCondition(Appearance.Condition.Watch);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnReplayAppearcanceClick()
        {
            Program.I().appearance.SwitchCondition(Appearance.Condition.Replay);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnDuelCharacterClick()
        {
            Program.I().character.SwitchCondition(SelectCharacter.Condition.Duel);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().character);
            else
                Program.I().ShiftToServant(Program.I().character);
        }
        public void OnWatchCharacterClick()
        {
            Program.I().character.SwitchCondition(SelectCharacter.Condition.Watch);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().character);
            else
                Program.I().ShiftToServant(Program.I().character);
        }
        public void OnReplayCharacterClick()
        {
            Program.I().character.SwitchCondition(SelectCharacter.Condition.Replay);
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().character);
            else
                Program.I().ShiftToServant(Program.I().character);
        }
        public void OnDuelVoiceClick()
        {
            if (duelVoiceValue.text == InterString.Get("开"))
                duelVoiceValue.text = InterString.Get("关");
            else
                duelVoiceValue.text = InterString.Get("开");
            Config.Set("DuelVoice", SaveBool(duelVoiceValue.text));

            Program.I().ocgcore.CheckCharaFace();
        }
        public void OnWatchVoiceClick()
        {
            if (watchVoiceValue.text == InterString.Get("开"))
                watchVoiceValue.text = InterString.Get("关");
            else
                watchVoiceValue.text = InterString.Get("开");
            Config.Set("WatchVoice", SaveBool(watchVoiceValue.text));

            Program.I().ocgcore.CheckCharaFace();
        }
        public void OnReplayVoiceClick()
        {
            if (replayVoiceValue.text == InterString.Get("开"))
                replayVoiceValue.text = InterString.Get("关");
            else
                replayVoiceValue.text = InterString.Get("开");
            Config.Set("ReplayVoice", SaveBool(replayVoiceValue.text));

            Program.I().ocgcore.CheckCharaFace();
        }
        public void OnDuelCloseupClick()
        {
            if (duelCloseupValue.text == InterString.Get("开"))
                duelCloseupValue.text = InterString.Get("关");
            else
                duelCloseupValue.text = InterString.Get("开");
            Config.Set("DuelCloseup", SaveBool(duelCloseupValue.text));
            Program.I().ocgcore.RefreshAllCardsLabel();
        }
        public void OnWatchCloseupClick()
        {
            if (watchCloseupValue.text == InterString.Get("开"))
                watchCloseupValue.text = InterString.Get("关");
            else
                watchCloseupValue.text = InterString.Get("开");
            Config.Set("WatchCloseup", SaveBool(watchCloseupValue.text));
            Program.I().ocgcore.RefreshAllCardsLabel();
        }
        public void OnReplayCloseupClick()
        {
            if (replayCloseupValue.text == InterString.Get("开"))
                replayCloseupValue.text = InterString.Get("关");
            else
                replayCloseupValue.text = InterString.Get("开");
            Config.Set("ReplayCloseup", SaveBool(replayCloseupValue.text));
            Program.I().ocgcore.RefreshAllCardsLabel();
        }
        public void OnDuelSummonClick()
        {
            if (duelSummonValue.text == InterString.Get("开"))
                duelSummonValue.text = InterString.Get("关");
            else
                duelSummonValue.text = InterString.Get("开");
        }
        public void OnWatchSummonClick()
        {
            if (watchSummonValue.text == InterString.Get("开"))
                watchSummonValue.text = InterString.Get("关");
            else
                watchSummonValue.text = InterString.Get("开");
        }
        public void OnReplaySummonClick()
        {
            if (replaySummonValue.text == InterString.Get("开"))
                replaySummonValue.text = InterString.Get("关");
            else
                replaySummonValue.text = InterString.Get("开");
        }
        public void OnDuelPendulumClick()
        {
            if (duelPendulumValue.text == InterString.Get("开"))
                duelPendulumValue.text = InterString.Get("关");
            else
                duelPendulumValue.text = InterString.Get("开");
        }
        public void OnWatchPendulumClick()
        {
            if (watchPendulumValue.text == InterString.Get("开"))
                watchPendulumValue.text = InterString.Get("关");
            else
                watchPendulumValue.text = InterString.Get("开");
        }
        public void OnReplayPendulumClick()
        {
            if (replayPendulumValue.text == InterString.Get("开"))
                replayPendulumValue.text = InterString.Get("关");
            else
                replayPendulumValue.text = InterString.Get("开");
        }
        public void OnDuelCutinClick()
        {
            if (duelCutinValue.text == InterString.Get("开"))
                duelCutinValue.text = InterString.Get("关");
            else
                duelCutinValue.text = InterString.Get("开");
        }
        public void OnWatchCutinClick()
        {
            if (watchCutinValue.text == InterString.Get("开"))
                watchCutinValue.text = InterString.Get("关");
            else
                watchCutinValue.text = InterString.Get("开");
        }
        public void OnReplayCutinClick()
        {
            if (replayCutinValue.text == InterString.Get("开"))
                replayCutinValue.text = InterString.Get("关");
            else
                replayCutinValue.text = InterString.Get("开");
        }
        public void OnDuelEffectClick()
        {
            if (duelEffectValue.text == InterString.Get("开"))
                duelEffectValue.text = InterString.Get("关");
            else
                duelEffectValue.text = InterString.Get("开");
        }
        public void OnWatchEffectClick()
        {
            if (watchEffectValue.text == InterString.Get("开"))
                watchEffectValue.text = InterString.Get("关");
            else
                watchEffectValue.text = InterString.Get("开");
        }
        public void OnReplayEffectClick()
        {
            if (replayEffectValue.text == InterString.Get("开"))
                replayEffectValue.text = InterString.Get("关");
            else
                replayEffectValue.text = InterString.Get("开");
        }
        public void OnDuelChainClick()
        {
            if (duelChainValue.text == InterString.Get("开"))
                duelChainValue.text = InterString.Get("关");
            else
                duelChainValue.text = InterString.Get("开");
        }
        public void OnWatchChainClick()
        {
            if (watchChainValue.text == InterString.Get("开"))
                watchChainValue.text = InterString.Get("关");
            else
                watchChainValue.text = InterString.Get("开");
        }
        public void OnReplayChainClick()
        {
            if (replayChainValue.text == InterString.Get("开"))
                replayChainValue.text = InterString.Get("关");
            else
                replayChainValue.text = InterString.Get("开");
        }
        public void OnDuelDiceClick()
        {
            if (duelDiceValue.text == InterString.Get("开"))
                duelDiceValue.text = InterString.Get("关");
            else
                duelDiceValue.text = InterString.Get("开");
        }
        public void OnWatchDiceClick()
        {
            if (watchDiceValue.text == InterString.Get("开"))
                watchDiceValue.text = InterString.Get("关");
            else
                watchDiceValue.text = InterString.Get("开");
        }
        public void OnReplayDiceClick()
        {
            if (replayDiceValue.text == InterString.Get("开"))
                replayDiceValue.text = InterString.Get("关");
            else
                replayDiceValue.text = InterString.Get("开");
        }
        public void OnDuelCoinClick()
        {
            if (duelCoinValue.text == InterString.Get("开"))
                duelCoinValue.text = InterString.Get("关");
            else
                duelCoinValue.text = InterString.Get("开");
        }
        public void OnWatchCoinClick()
        {
            if (watchCoinValue.text == InterString.Get("开"))
                watchCoinValue.text = InterString.Get("关");
            else
                watchCoinValue.text = InterString.Get("开");
        }
        public void OnReplayCoinClick()
        {
            if (replayCoinValue.text == InterString.Get("开"))
                replayCoinValue.text = InterString.Get("关");
            else
                replayCoinValue.text = InterString.Get("开");
        }

        public void OnDuelAutoInfoClick()
        {
            if (duelAutoInfoValue.text == InterString.Get("开"))
                duelAutoInfoValue.text = InterString.Get("关");
            else
                duelAutoInfoValue.text = InterString.Get("开");
        }
        public void OnWatchAutoInfoClick()
        {
            if (watchAutoInfoValue.text == InterString.Get("开"))
                watchAutoInfoValue.text = InterString.Get("关");
            else
                watchAutoInfoValue.text = InterString.Get("开");
        }
        public void OnReplayAutoInfoClick()
        {
            if (replayAutoInfoValue.text == InterString.Get("开"))
                replayAutoInfoValue.text = InterString.Get("关");
            else
                replayAutoInfoValue.text = InterString.Get("开");
        }

        public void OnDuelPlayerMessageClick()
        {
            if (duelPlayerMessageValue.text == InterString.Get("开"))
                duelPlayerMessageValue.text = InterString.Get("关");
            else
                duelPlayerMessageValue.text = InterString.Get("开");
        }
        public void OnWatchPlayerMessageClick()
        {
            if (watchPlayerMessageValue.text == InterString.Get("开"))
                watchPlayerMessageValue.text = InterString.Get("关");
            else
                watchPlayerMessageValue.text = InterString.Get("开");
        }
        public void OnReplayPlayerMessageClick()
        {
            if (replayPlayerMessageValue.text == InterString.Get("开"))
                replayPlayerMessageValue.text = InterString.Get("关");
            else
                replayPlayerMessageValue.text = InterString.Get("开");
        }

        public void OnDuelSystemMessageClick()
        {
            if (duelSystemMessageValue.text == InterString.Get("开"))
                duelSystemMessageValue.text = InterString.Get("关");
            else
                duelSystemMessageValue.text = InterString.Get("开");
        }
        public void OnWatchSystemMessageClick()
        {
            if (watchSystemMessageValue.text == InterString.Get("开"))
                watchSystemMessageValue.text = InterString.Get("关");
            else
                watchSystemMessageValue.text = InterString.Get("开");
        }
        public void OnReplaySystemMessageClick()
        {
            if (replaySystemMessageValue.text == InterString.Get("开"))
                replaySystemMessageValue.text = InterString.Get("关");
            else
                replaySystemMessageValue.text = InterString.Get("开");
        }

        public void OnDuelAccChange(float value)
        {
            string result = value.ToString();
            duelAccValue.text = result.Length > 4 ? result.Substring(0, 4) : result;
            Config.SetFloat("DuelAcc", value);
            if (Program.I().ocgcore.isShowed)
                if (Program.I().ocgcore.condition == OcgCore.Condition.Duel)
                    if (Program.I().ocgcore.accing)
                        Program.I().ocgcore.OnAcc();
        }

        public void OnWatchAccChange(float value)
        {
            string result = value.ToString();
            watchAccValue.text = result.Length > 4 ? result.Substring(0, 4) : result;
            Config.SetFloat("WatchAcc", value);
            if (Program.I().ocgcore.isShowed)
                if (Program.I().ocgcore.condition == OcgCore.Condition.Watch)
                    if (Program.I().ocgcore.accing)
                        Program.I().ocgcore.OnAcc();
        }

        public void OnReplayAccChange(float value)
        {
            string result = value.ToString();
            replayAccValue.text = result.Length > 4 ? result.Substring(0, 4) : result;
            Config.SetFloat("ReplayAcc", value);
            if (Program.I().ocgcore.isShowed)
                if (Program.I().ocgcore.condition == OcgCore.Condition.Replay)
                    if (Program.I().ocgcore.accing)
                        Program.I().ocgcore.OnAcc();
        }

        public void OnDuelAutoAccClick()
        {
            if (duelAutoAccValue.text == InterString.Get("开"))
                duelAutoAccValue.text = InterString.Get("关");
            else
                duelAutoAccValue.text = InterString.Get("开");
        }
        public void OnWatchAutoAccClick()
        {
            if (watchAutoAccValue.text == InterString.Get("开"))
                watchAutoAccValue.text = InterString.Get("关");
            else
                watchAutoAccValue.text = InterString.Get("开");
        }
        public void OnReplayAutoAccClick()
        {
            if (replayAutoAccValue.text == InterString.Get("开"))
                replayAutoAccValue.text = InterString.Get("关");
            else
                replayAutoAccValue.text = InterString.Get("开");
        }

        public void OnDuelFaceDownClick()
        {
            if (duelFaceDownValue.text == InterString.Get("开"))
            {
                duelFaceDownValue.text = InterString.Get("关");
                Config.SetBool("DuelFaceDown", false);
            }
            else
            {
                duelFaceDownValue.text = InterString.Get("开");
                Config.SetBool("DuelFaceDown", true);
            }
            foreach(var card in Program.I().ocgcore.cards)
                card.ShowFaceDownCardOrNot(card.NeedShowFaceDownCard());
        }
        public void OnWatchFaceDownClick()
        {
            if (watchFaceDownValue.text == InterString.Get("开"))
            {
                watchFaceDownValue.text = InterString.Get("关");
                Config.SetBool("WatchFaceDown", false);
            }
            else
            {
                watchFaceDownValue.text = InterString.Get("开");
                Config.SetBool("WatchFaceDown", true);
            }
            foreach (var card in Program.I().ocgcore.cards)
                card.ShowFaceDownCardOrNot(card.NeedShowFaceDownCard());
        }
        public void OnReplayFaceDownClick()
        {
            if (replayFaceDownValue.text == InterString.Get("开"))
            {
                replayFaceDownValue.text = InterString.Get("关");
                Config.SetBool("ReplayFaceDown", false);
            }
            else
            {
                replayFaceDownValue.text = InterString.Get("开");
                Config.SetBool("ReplayFaceDown", true);
            }
            foreach (var card in Program.I().ocgcore.cards)
                card.ShowFaceDownCardOrNot(card.NeedShowFaceDownCard());
        }

        public void OnTimingClick()
        {
            if (timingValue.text == InterString.Get("开"))
                timingValue.text = InterString.Get("关");
            else
                timingValue.text = InterString.Get("开");
        }

        void OnImport()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能进行此操作。"));
                return;
            }

            PortHelper.ImportFiles();
        }
        void OnImportBG()
        {
            PortHelper.ImportBG();
        }
        void OnExportDecks()
        {
            PortHelper.ExportAllDecks();
        }
        void OnExportReplays()
        {
            PortHelper.ExportAllReplays();
        }
        void OnExportPictures()
        {
            PortHelper.ExportAllPictures();
        }
        void OnClearPictures()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能进行此操作。"));
                return;
            }

            var selections = new List<string>
            {
                InterString.Get("确定清空"),
                InterString.Get("是否确认删除所有导入的卡图？"),
                InterString.Get("确认"),
                InterString.Get("取消")
            };
            UIManager.ShowPopupYesOrNo(selections, () =>
            {
                if (!Directory.Exists(Program.altArtPath))
                    Directory.CreateDirectory(Program.altArtPath);
                foreach (var file in Directory.GetFiles(Program.altArtPath))
                    File.Delete(file);
            }, null);
        }
        void OnClearExpansions()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能进行此操作。"));
                return;
            }

            var selections = new List<string>
            {
                InterString.Get("确定清空"),
                InterString.Get("是否确认删除所有导入的扩展卡包？"),
                InterString.Get("确认"),
                InterString.Get("取消")
            };
            UIManager.ShowPopupYesOrNo(selections, () =>
            {
                ZipHelper.Dispose();
                if(!Directory.Exists(Program.expansionsPath))
                    Directory.CreateDirectory(Program.expansionsPath);
                foreach (var file in Directory.GetFiles(Program.expansionsPath))
                    File.Delete(file);
                Program.I().InitializeForDataChange();
            }, null);
        }

        void OnSupportExpansions()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能更改此选项。"));
                return;
            }

            if (supportExpansionsValue.text == InterString.Get("否"))
            {
                supportExpansionsValue.text = InterString.Get("是");
                Config.SetBool("Expansions", true);
            }
            else
            {
                supportExpansionsValue.text = InterString.Get("否");
                Config.SetBool("Expansions", false);
            }
            Program.I().InitializeForDataChange();
        }

        bool checking;
        void OnUpdatePrerelease()
        {
            if (Program.I().ocgcore.isShowed)
            {
                MessageManager.Cast(InterString.Get("决斗中不能进行此操作。"));
                return;
            }

            if (!checking)
            {
                checking = true;
                StartCoroutine(UpdatePrereleaseAsync());
            }
        }

        public static readonly string prereleaseVersionUrl = "https://cdn02.moecube.com:444/ygopro-super-pre/data/version.txt";
        public static readonly string prereleasePackUrl = "https://cdn02.moecube.com:444/ygopro-super-pre/archive/ygopro-super-pre.ypk";

        IEnumerator UpdatePrereleaseAsync()
        {
            var filePath = Path.Combine(Program.expansionsPath, Path.GetFileName(prereleasePackUrl));
            if (!File.Exists(filePath))
            {
                Config.Set("Prerelease", "0");
                Config.Save();
            }

            var www = UnityWebRequest.Get(prereleaseVersionUrl);
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
                updatePrereleaseValue.text = InterString.Get("检查更新中");
            }
            if (www.result == UnityWebRequest.Result.Success)
            {
                var result = www.downloadHandler.text;
                var lines = result.Replace("\r", "").Split('\n');
                if (Config.Get("Prerelease", "0") != lines[0])
                {
                    if(!Directory.Exists(Program.expansionsPath))
                        Directory.CreateDirectory(Program.expansionsPath);
                    var download = UnityWebRequest.Get(prereleasePackUrl);
                    download.SendWebRequest();
                    MessageManager.Cast(InterString.Get("正在更新，请耐心等待更待更新完成再进行其他操作。"));
                    while (!download.isDone)
                    {
                        yield return null;
                        updatePrereleaseValue.text = (download.downloadProgress * 100f).ToString("0.##") + "%";
                    }
                    if(download.result == UnityWebRequest.Result.Success)
                    {
                        ZipHelper.Dispose();
                        File.WriteAllBytes(filePath, download.downloadHandler.data);
                        MessageManager.Cast(InterString.Get("先行卡更新成功。"));
                        Config.Set("Prerelease", lines[0]);
                        Config.Save();
                        Program.I().InitializeForDataChange();
                    }
                    else
                        MessageManager.Cast(InterString.Get("先行卡更新失败。"));
                }
                else
                    MessageManager.Cast(InterString.Get("先行卡已是最新版。"));
            }
            else
                MessageManager.Cast(InterString.Get("检查更新失败！"));
            updatePrereleaseValue.text = string.Empty;
            checking = false;
        }

#endregion

        public void OnAboutGame()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("AboutGame");
            handle.Completed += (result) =>
            {
                var selections = new List<string>()
                {
                    InterString.Get("关于游戏"),
                    result.Result.text
                };
                UIManager.ShowPopupText(selections);
            };
        }

        public void OnAboutVersion()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("AboutVersion");
            handle.Completed += (result) =>
            {
                var selections = new List<string>()
                {
                    InterString.Get("关于版本号"),
                    result.Result.text
                };
                UIManager.ShowPopupText(selections, TMPro.HorizontalAlignmentOptions.Left);
            };
        }
        public void OnAboutUpdate()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("AboutUpdate");
            handle.Completed += (result) =>
            {
                var selections = new List<string>()
                {
                    InterString.Get("关于更新"),
                    result.Result.text
                };
                UIManager.ShowPopupText(selections);
            };
        }
        public void OnUpdateContent()
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("UpdateContent");
            handle.Completed += (result) =>
            {
                var selections = new List<string>()
                {
                    InterString.Get("更新内容"),
                    result.Result.text
                };
                UIManager.ShowPopupText(selections, TMPro.HorizontalAlignmentOptions.Left);
            };
        }

    }


    public partial class SROptions
    {
        private UniversalRenderPipelineAsset urpa;
        private Type universalRenderPipelineAssetType;
        private FieldInfo mainLightShadowmapResolutionFieldInfo;
        private FieldInfo supportsSoftShadowsFieldInfo;

        private void InitializeShadowMapFieldInfo()
        {
            urpa = Resources.Load<UniversalRenderPipelineAsset>("Settings/URPAsset");
            universalRenderPipelineAssetType = urpa.GetType();
            mainLightShadowmapResolutionFieldInfo = universalRenderPipelineAssetType.GetField("m_MainLightShadowmapResolution", BindingFlags.Instance | BindingFlags.NonPublic);
            supportsSoftShadowsFieldInfo = universalRenderPipelineAssetType.GetField("m_SoftShadowsSupported", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public ShadowResolution MainLightShadowResolution
        {
            get
            {
                if (mainLightShadowmapResolutionFieldInfo == null)
                {
                    InitializeShadowMapFieldInfo();
                }
                return (ShadowResolution)mainLightShadowmapResolutionFieldInfo.GetValue(urpa);
            }
            set
            {
                if (mainLightShadowmapResolutionFieldInfo == null)
                {
                    InitializeShadowMapFieldInfo();
                }
                mainLightShadowmapResolutionFieldInfo.SetValue(urpa, value);
            }
        }
        public bool SupportsSoftShadows
        {
            get
            {
                if (mainLightShadowmapResolutionFieldInfo == null)
                {
                    InitializeShadowMapFieldInfo();
                }
                return (bool)supportsSoftShadowsFieldInfo.GetValue(urpa);
            }
            set
            {
                if (mainLightShadowmapResolutionFieldInfo == null)
                {
                    InitializeShadowMapFieldInfo();
                }
                supportsSoftShadowsFieldInfo.SetValue(urpa, value);
            }
        }
    }
}
