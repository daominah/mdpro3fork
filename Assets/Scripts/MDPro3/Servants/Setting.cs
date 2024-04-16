using DG.Tweening;
using MDPro3.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public Button autoRPS;
        public Text autoRPSValue;
        public Slider uiScale;
        public Text uiScaleValue;
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
        public Button timing;
        public Text timingValue;

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
        [Header("Port")]
        public Button import;
        public Button exportDeck;
        public Button exportReplay;
        public Button exportPicture;
        public Button clearPicture;
        public Button clearExpansions;

        public override void Initialize()
        {
            depth = 1;
            haveLine = false;
            blackAlpha = 0.6f;
            subBlackAlpha = 0.9f;
            returnServant = Program.I().menu;
            base.Initialize();


            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

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
            language.onClick.AddListener(OnLanguageChange);
            confirm.onClick.AddListener(OnConfirmClicked);
            autoRPS.onClick.AddListener(OnAutoRPS);

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
            timing.onClick.AddListener(OnTimingClick);
            import.onClick.AddListener(OnImport);
            exportDeck.onClick.AddListener(OnExportDecks);
            exportReplay.onClick.AddListener(OnExportReplays);
            exportPicture.onClick.AddListener(OnExportPictures);
            clearPicture.onClick.AddListener(OnClearPictures);
            clearExpansions.onClick.AddListener(OnClearExpansions);

            bgmVol.value = int.Parse(Config.Get("BgmVol", "700")) / (float)1000;
            seVol.value = int.Parse(Config.Get("SeVol", "700")) / (float)1000;
            voiceVol.value = int.Parse(Config.Get("VoiceVol", "700")) / (float)1000;
            fps.value = int.Parse(Config.Get("FPS", "60"));
            scale.value = int.Parse(Config.Get("Scale", "1000")) / (float)1000;

            var defau = "1000";
#if UNITY_ANDROID
            defau = "1500";
#endif
            uiScale.value = int.Parse(Config.Get("UIScale", defau)) / (float)1000;
            quality.value = int.Parse(Config.Get("Quality", "3"));
            faa.value = int.Parse(Config.Get("FAA", "1"));
            aaa.value = int.Parse(Config.Get("AAA", "0"));
            shadow.value = int.Parse(Config.Get("Shadow", "0"));
            InitializeShowFPS();
            InitializeScreenMode();
            InitializeResolution();
            InitializeConfirm();
            InitializeLanguage();
            InitializeSwitches();
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

        public override void OnExit()
        {
            base.OnExit();
            Save();
            if (Program.I().currentServant == Program.I().ocgcore)
                UIManager.ShowFPSLeft();

        }
        public override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            if (preDepth <= depth)
                defaultButton.SelectThis();
        }

        #region setting
        public void Save()
        {
            Config.Set("BgmVol", ((int)(bgmVol.value * 1000)).ToString());
            Config.Set("SeVol", ((int)(seVol.value * 1000)).ToString());
            Config.Set("VoiceVol", ((int)(voiceVol.value * 1000)).ToString());
            Config.Set("FPS", fpsValue.text);
            Config.Set("Scale", ((int)(scale.value * 1000)).ToString());
            Config.Set("UIScale", ((int)(uiScale.value * 1000)).ToString());
            Config.Set("Quality", quality.value.ToString());
            Config.Set("FAA", faa.value.ToString());
            Config.Set("AAA", aaa.value.ToString());
            Config.Set("Shadow", shadow.value.ToString());
            Config.Set("ShowFPS", SaveBool(showFPSValue.text));
            Config.Set("ScreenMode", SaveScreenMode(screenValue.text));
            Config.Set("Resolution", resolutionValue.text);
            Config.Set("Language", InterString.GetOriginal(languageValue.text));
            Config.Set("Confirm", SaveBool(confirmValue.text));

            Config.Set("DuelVoice", SaveBool(duelVoiceValue.text));
            Config.Set("WatchVoice", SaveBool(watchVoiceValue.text));
            Config.Set("ReplayVoice", SaveBool(replayVoiceValue.text));
            Config.Set("DuelCloseup", SaveBool(duelCloseupValue.text));
            Config.Set("WatchCloseup", SaveBool(watchCloseupValue.text));
            Config.Set("ReplayCloseup", SaveBool(replayCloseupValue.text));
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
            Config.Set("Timing", SaveBool(timingValue.text));
            Config.Save();
        }
        public string SaveBool(string value)
        {
            string returnValue = "0";
            if (value == InterString.Get("开"))
                returnValue = "1";
            if (value == InterString.Get("有"))
                returnValue = "1";
            if (value == InterString.Get("左"))
                returnValue = "1";
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
            Config.Set("Quality", ((int)value).ToString());
            qualityValue.text = qualityText;
        }
        public void OnFAAChange(float value)
        {
            switch ((int)value)
            {
                case 1:
                    faaValue.text = InterString.Get("关闭");
                    Program.I().camera_.urpAsset.msaaSampleCount = 1;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 1;
                    break;
                case 2:
                    faaValue.text = InterString.Get("MSAA 2x");
                    Program.I().camera_.urpAsset.msaaSampleCount = 2;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 2;
                    break;
                case 3:
                    faaValue.text = InterString.Get("MSAA 4x");
                    Program.I().camera_.urpAsset.msaaSampleCount = 4;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 4;
                    break;
                case 4:
                    faaValue.text = InterString.Get("MSAA 8x");
                    Program.I().camera_.urpAsset.msaaSampleCount = 8;
                    Program.I().camera_.urpAssetForUI.msaaSampleCount = 8;
                    break;
            }
        }
        public void OnAAAChange(float value)
        {
            var cameraData3D = Program.I().camera_.cameraMain.GetUniversalAdditionalCameraData();
            var cameraData2D = Program.I().camera_.camera2D.GetUniversalAdditionalCameraData();

            switch ((int)value)
            {
                case 0:
                    aaaValue.text = InterString.Get("无");
                    cameraData3D.antialiasing = AntialiasingMode.None;
                    cameraData2D.antialiasing = AntialiasingMode.None;
                    break;
                case 1:
                    aaaValue.text = InterString.Get("FAA");
                    cameraData3D.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    cameraData2D.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case 2:
                    aaaValue.text = InterString.Get("SMAA Low");
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.Low;
                    cameraData2D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData2D.antialiasingQuality = AntialiasingQuality.Low;
                    break;
                case 3:
                    aaaValue.text = InterString.Get("SMAA Medium");
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.Medium;
                    cameraData2D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData2D.antialiasingQuality = AntialiasingQuality.Medium;
                    break;
                case 4:
                    aaaValue.text = InterString.Get("SMAA High");
                    cameraData3D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData3D.antialiasingQuality = AntialiasingQuality.High;
                    cameraData2D.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    cameraData2D.antialiasingQuality = AntialiasingQuality.High;
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
            string value = Config.Get("ShowFPS", "1");
            if (value == "1")
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
            string resolution;
            if (Config.Have("Resolution"))
                resolution = Config.Get("Resolution", "1920 x 1080");
            else
                resolution = Regex.Split(Screen.currentResolution.ToString(), " @ ")[0];
            string fullScreenMode = Config.Get("ScreenMode", "1");
            if (fullScreenMode == "0")
                Screen.SetResolution(int.Parse(Regex.Split(resolution, " x ")[0]), int.Parse(Regex.Split(resolution, " x ")[1]), FullScreenMode.ExclusiveFullScreen);
            else if (fullScreenMode == "2")
                Screen.SetResolution(int.Parse(Regex.Split(resolution, " x ")[0]), int.Parse(Regex.Split(resolution, " x ")[1]), FullScreenMode.Windowed);
            else
                Screen.SetResolution(int.Parse(Regex.Split(resolution, " x ")[0]), int.Parse(Regex.Split(resolution, " x ")[1]), FullScreenMode.FullScreenWindow);
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
            string value = Config.Get("Confirm", "1");
            if (value == "0")
                confirmValue.text = InterString.Get("右");
            else
                confirmValue.text = InterString.Get("左");
        }
        public void OnConfirmClicked()
        {
            if (confirmValue.text == InterString.Get("右"))
            {
                confirmValue.text = InterString.Get("左");
                Config.Set("Confirm", "1");
            }
            else
            {
                confirmValue.text = InterString.Get("右");
                Config.Set("Confirm", "0");
            }
        }

        public void OnAutoRPS()
        {
            if (autoRPSValue.text == InterString.Get("关"))
            {
                autoRPSValue.text = InterString.Get("开");
                Config.Set("AutoRPS", "1");
            }
            else
            {
                autoRPSValue.text = InterString.Get("关");
                Config.Set("AutoRPS", "0");
            }
        }
        public void InitializeSwitches()
        {
            duelAppearanceValue.text = Config.Get("DuelPlayerName0", "@ui");
            watchAppearanceValue.text = Config.Get("WatchPlayerName0", "@ui");
            replayAppearanceValue.text = Config.Get("ReplayPlayerName0", "@ui");

            string value = Config.Get("DuelVoice", "0");
            if (value == "0")
                duelVoiceValue.text = InterString.Get("关");
            else
                duelVoiceValue.text = InterString.Get("开");
            value = Config.Get("WatchVoice", "0");
            if (value == "0")
                watchVoiceValue.text = InterString.Get("关");
            else
                watchVoiceValue.text = InterString.Get("开");
            value = Config.Get("ReplayVoice", "0");
            if (value == "0")
                replayVoiceValue.text = InterString.Get("关");
            else
                replayVoiceValue.text = InterString.Get("开");

            value = Config.Get("DuelCloseup", "1");
            if (value == "0")
                duelCloseupValue.text = InterString.Get("关");
            else
                duelCloseupValue.text = InterString.Get("开");
            value = Config.Get("WatchCloseup", "1");
            if (value == "0")
                watchCloseupValue.text = InterString.Get("关");
            else
                watchCloseupValue.text = InterString.Get("开");
            value = Config.Get("ReplayCloseup", "1");
            if (value == "0")
                replayCloseupValue.text = InterString.Get("关");
            else
                replayCloseupValue.text = InterString.Get("开");

            value = Config.Get("DuelSummon", "1");
            if (value == "0")
                duelSummonValue.text = InterString.Get("关");
            else
                duelSummonValue.text = InterString.Get("开");
            value = Config.Get("WatchSummon", "1");
            if (value == "0")
                watchSummonValue.text = InterString.Get("关");
            else
                watchSummonValue.text = InterString.Get("开");
            value = Config.Get("ReplaySummon", "1");
            if (value == "0")
                replaySummonValue.text = InterString.Get("关");
            else
                replaySummonValue.text = InterString.Get("开");

            value = Config.Get("DuelPendulum", "1");
            if (value == "0")
                duelPendulumValue.text = InterString.Get("关");
            else
                duelPendulumValue.text = InterString.Get("开");
            value = Config.Get("WatchPendulum", "1");
            if (value == "0")
                watchPendulumValue.text = InterString.Get("关");
            else
                watchPendulumValue.text = InterString.Get("开");
            value = Config.Get("ReplayPendulum", "1");
            if (value == "0")
                replayPendulumValue.text = InterString.Get("关");
            else
                replayPendulumValue.text = InterString.Get("开");

            value = Config.Get("DuelCutin", "1");
            if (value == "0")
                duelCutinValue.text = InterString.Get("关");
            else
                duelCutinValue.text = InterString.Get("开");
            value = Config.Get("WatchCutin", "1");
            if (value == "0")
                watchCutinValue.text = InterString.Get("关");
            else
                watchCutinValue.text = InterString.Get("开");
            value = Config.Get("ReplayCutin", "1");
            if (value == "0")
                replayCutinValue.text = InterString.Get("关");
            else
                replayCutinValue.text = InterString.Get("开");

            value = Config.Get("DuelEffect", "1");
            if (value == "0")
                duelEffectValue.text = InterString.Get("关");
            else
                duelEffectValue.text = InterString.Get("开");
            value = Config.Get("WatchEffect", "1");
            if (value == "0")
                watchEffectValue.text = InterString.Get("关");
            else
                watchEffectValue.text = InterString.Get("开");
            value = Config.Get("ReplayEffect", "1");
            if (value == "0")
                replayEffectValue.text = InterString.Get("关");
            else
                replayEffectValue.text = InterString.Get("开");

            value = Config.Get("DuelChain", "1");
            if (value == "0")
                duelChainValue.text = InterString.Get("关");
            else
                duelChainValue.text = InterString.Get("开");
            value = Config.Get("WatchChain", "1");
            if (value == "0")
                watchChainValue.text = InterString.Get("关");
            else
                watchChainValue.text = InterString.Get("开");
            value = Config.Get("ReplayChain", "1");
            if (value == "0")
                replayChainValue.text = InterString.Get("关");
            else
                replayChainValue.text = InterString.Get("开");

            value = Config.Get("DuelDice", "1");
            if (value == "0")
                duelDiceValue.text = InterString.Get("关");
            else
                duelDiceValue.text = InterString.Get("开");
            value = Config.Get("WatchDice", "1");
            if (value == "0")
                watchDiceValue.text = InterString.Get("关");
            else
                watchDiceValue.text = InterString.Get("开");
            value = Config.Get("ReplayDice", "1");
            if (value == "0")
                replayDiceValue.text = InterString.Get("关");
            else
                replayDiceValue.text = InterString.Get("开");

            value = Config.Get("DuelCoin", "1");
            if (value == "0")
                duelCoinValue.text = InterString.Get("关");
            else
                duelCoinValue.text = InterString.Get("开");
            value = Config.Get("WatchCoin", "1");
            if (value == "0")
                watchCoinValue.text = InterString.Get("关");
            else
                watchCoinValue.text = InterString.Get("开");
            value = Config.Get("ReplayCoin", "1");
            if (value == "0")
                replayCoinValue.text = InterString.Get("关");
            else
                replayCoinValue.text = InterString.Get("开");

            value = Config.Get("DuelAutoInfo", "1");
            if (value == "0")
                duelAutoInfoValue.text = InterString.Get("关");
            else
                duelAutoInfoValue.text = InterString.Get("开");
            value = Config.Get("WatchAutoInfo", "1");
            if (value == "0")
                watchAutoInfoValue.text = InterString.Get("关");
            else
                watchAutoInfoValue.text = InterString.Get("开");
            value = Config.Get("ReplayAutoInfo", "1");
            if (value == "0")
                replayAutoInfoValue.text = InterString.Get("关");
            else
                replayAutoInfoValue.text = InterString.Get("开");

            value = Config.Get("Timing", "0");
            if (value == "0")
                timingValue.text = InterString.Get("关");
            else
                timingValue.text = InterString.Get("开");

            value = Config.Get("AutoRPS", "0");
            if (value == "0")
                autoRPSValue.text = InterString.Get("关");
            else
                autoRPSValue.text = InterString.Get("开");
        }
        public void OnDuelAppearcanceClick()
        {
            Appearance.type = Appearance.AppearanceType.Duel;
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnWatchAppearcanceClick()
        {
            Appearance.type = Appearance.AppearanceType.Watch;
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnReplayAppearcanceClick()
        {
            Appearance.type = Appearance.AppearanceType.Replay;
            if (Program.I().currentSubServant == this)
                Program.I().ShowSubServant(Program.I().appearance);
            else
                Program.I().ShiftToServant(Program.I().appearance);
        }
        public void OnDuelCharacterClick()
        {

        }
        public void OnWatchCharacterClick()
        {

        }
        public void OnReplayCharacterClick()
        {

        }
        public void OnDuelVoiceClick()
        {
            if (duelVoiceValue.text == InterString.Get("开"))
                duelVoiceValue.text = InterString.Get("关");
            else
                duelVoiceValue.text = InterString.Get("开");
        }
        public void OnWatchVoiceClick()
        {
            if (watchVoiceValue.text == InterString.Get("开"))
                watchVoiceValue.text = InterString.Get("关");
            else
                watchVoiceValue.text = InterString.Get("开");
        }
        public void OnReplayVoiceClick()
        {
            if (replayVoiceValue.text == InterString.Get("开"))
                replayVoiceValue.text = InterString.Get("关");
            else
                replayVoiceValue.text = InterString.Get("开");
        }
        public void OnDuelCloseupClick()
        {
            if (duelCloseupValue.text == InterString.Get("开"))
                duelCloseupValue.text = InterString.Get("关");
            else
                duelCloseupValue.text = InterString.Get("开");
        }
        public void OnWatchCloseupClick()
        {
            if (watchCloseupValue.text == InterString.Get("开"))
                watchCloseupValue.text = InterString.Get("关");
            else
                watchCloseupValue.text = InterString.Get("开");
        }
        public void OnReplayCloseupClick()
        {
            if (replayCloseupValue.text == InterString.Get("开"))
                replayCloseupValue.text = InterString.Get("关");
            else
                replayCloseupValue.text = InterString.Get("开");
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
                UIManager.ShowPopupText(selections);
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
