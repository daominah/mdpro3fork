using MDPro3.Net;
using MDPro3.UI;
using MDPro3.UI.ServantUI;
using MDPro3.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static MDPro3.CardRenderer;
using static MDPro3.Net.NetUtil;

namespace MDPro3.Servant
{
    public class SettingServant : Servant
    {
        [Header("Setting Servant")]
        [HideInInspector] public SelectionToggle_Setting lastSelectedToggle;
        [HideInInspector] public SelectionButton_Setting lastSelectedButton;

        private string unsafeDownloadUrl;

        #region Servant

        public override int Depth => 1;
        protected override bool ShowLine => false;
        protected override float BlackAlpha => 0.6f;
        protected override float SubBlackAlpha => 0.9f;

        public override void Initialize()
        {
            returnServant = Program.instance.menu;
            base.Initialize();

            InitializeSettings();
        }

        protected override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            RefreshCharacterName();

            if (preDepth <= Depth)
            {
                servantUI.SelectDefaultSelectable();
            }

            if (Program.instance.currentServant == Program.instance.ocgcore)
            {
                Program.instance.currentSubServant = this;
                UIManager.ShowFPSRight();
            }
        }

        public override void Select(bool forced = false)
        {
            if (!forced && !UserInput.NeedDefaultSelect())
                return;

            if (lastSelectable != null)
            {
                if (lastSelectable.TryGetComponent<SelectionButton_Setting>(out _))
                    EventSystem.current.SetSelectedGameObject(lastSelectable.gameObject);
                else if (lastSelectable.TryGetComponent<SelectionToggle_Setting>(out _))
                    EventSystem.current.SetSelectedGameObject(lastSelectable.gameObject);
                else
                    servantUI.SelectDefaultSelectable();
            }
            else
                servantUI.SelectDefaultSelectable();
        }

        public override void OnReturn()
        {
            if (inTransition) return;
            if(returnAction != null)
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
            else if (selected.TryGetComponent<SelectionButton_Setting>(out _))
            {
                if (lastSelectedToggle != null)
                    lastSelectedToggle.GetSelectable().Select();
                else
                    servantUI.SelectDefaultSelectable();
            }
            else
                OnExit();
        }

        #endregion

        private void InitializeSettings()
        {
            QualitySettings.vSyncCount = 0;

            AudioManager.SetBGMVol(Config.GetFloat("BgmVol", 0.7f));
            AudioManager.SetSeVol(Config.GetFloat("SEVol", 0.7f));
            AudioManager.SetVoiceVol(Config.GetFloat("VoiceVol", 0.7f));

#if UNITY_ANDROID || UNITY_IOS
            if (Config.Have("Resolution"))
            {
                var resolution = Config.Get("Resolution", "1920 x 1080");
                Screen.SetResolution(int.Parse(Regex.Split(resolution, " x ")[0]), int.Parse(Regex.Split(resolution, " x ")[1]), FullScreenMode.FullScreenWindow);
            }
#endif

            Program.instance.camera_.urpAsset.renderScale = SettingServantUI.GetScale();
            // TODO FAA
            SettingServantUI.ChangeAAA(Config.GetFloat("AAA", 0f));
            SettingServantUI.ChangeShadow(Config.GetFloat("Shadow", 0f));
            SetFpsToConfig();
            SettingServantUI.ChangeShowFPS();
            Program.instance.background_.Change(int.Parse(Config.Get("Background", "0")));
        }

        public static void SetFpsToConfig()
        {
            SettingServantUI.ChangeFPS(SettingServantUI.GetFPS());
        }

        public float GetBGMVolum()
        {
            if (servantUI == null)
                return Config.GetFloat("BgmVol", 0.7f);
            else
                return GetUI<SettingServantUI>().GetBGMVolum();
        }

        public void RefreshCharacterName()
        {
            if (servantUI == null)
                return;

            GetUI<SettingServantUI>().RefreshCharacterName();
        }

        private bool checkingPrereleaseUpdate;

        public void UpdatePrerelease()
        {
            if (!checkingPrereleaseUpdate)
            {
                checkingPrereleaseUpdate = true;
                StartCoroutine(UpdatePrereleaseAsync());
            }
        }

        private IEnumerator UpdatePrereleaseAsync()
        {
            var filePath = Path.Combine(Program.PATH_EXPANSIONS, Path.GetFileName(Settings.Data.PrereleasePackUrl));
            if (!File.Exists(filePath) || Language.GetConfig() != Language.GetPrereleaseConfig())
            {
                Config.Set("Prerelease", Config.STRING_NO);
                Config.Save();
            }

            var www = UnityWebRequest.Get(Settings.GetPrereleasePackVersionUrl());
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
                GetUI<SettingServantUI>().ButtonUpdatePrerelease.SetModeText(InterString.Get("检查更新中"));
            }
            if (www.result == UnityWebRequest.Result.Success)
            {
                var result = www.downloadHandler.text;
                var lines = result.Replace("\r", string.Empty).Split('\n');
                if (Config.Get("Prerelease", Config.STRING_NO) != lines[0])
                {
                    if (!Directory.Exists(Program.PATH_EXPANSIONS))
                        Directory.CreateDirectory(Program.PATH_EXPANSIONS);
                    var download = UnityWebRequest.Get(Settings.GetPrereleasePackUrl());
                    download.SendWebRequest();
                    MessageManager.Cast(InterString.Get("正在更新，请耐心等待更待更新完成再进行其他操作。"));
                    while (!download.isDone)
                    {
                        yield return null;
                        GetUI<SettingServantUI>().ButtonUpdatePrerelease
                            .SetModeText((download.downloadProgress * 100f).ToString("0.##") + "%");
                    }
                    if (download.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(filePath, download.downloadHandler.data);
                        MessageManager.Cast(InterString.Get("先行卡更新成功。"));
                        Config.Set("Prerelease", lines[0]);
                        Language.SetPrereleaseConfig(Language.GetConfig());
                        Config.Save();
                        Program.instance.InitializeForDataChange();
                    }
                    else
                        MessageManager.Cast(InterString.Get("先行卡更新失败。") + download.error);
                }
                else
                    MessageManager.Cast(InterString.Get("先行卡已是最新版。"));
            }
            else
                MessageManager.Cast(InterString.Get("检查更新失败！"));
            GetUI<SettingServantUI>().ButtonUpdatePrerelease.SetModeText(string.Empty);
            checkingPrereleaseUpdate = false;
        }

        private bool downloadingYPK;

        public void DownloadYPK()
        {
            if (!downloadingYPK)
            {
                var selections = new List<string>()
                {
                    InterString.Get("下载卡包@n请输入卡包的下载地址"),
                    string.Empty
                };
                UIManager.ShowPopupInput(selections, DownloadYPK, null, TmpInputValidation.ValidationType.None);
            }
            else
                MessageManager.Toast(InterString.Get("正在下载中，请稍后再试。"));
        }

        private void DownloadYPK(string url)
        {
            string[] supportedExts = new string[] { ".zip", ".ypk" };
            if (!NetUtil.IsValidDownloadUrl(url, supportedExts))
            {
                MessageManager.Toast(InterString.Get("无效的下载地址。"));
                return;
            }
            StartCoroutine(DownloadYpkAsync(url));
        }


        private IEnumerator DownloadYpkAsync(string url)
        {
            downloadingYPK = true;
            using var request = UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
            {
                GetUI<SettingServantUI>().ButtonDownloadYPK.SetModeText((request.downloadProgress * 100f).ToString("0.##") + "%");
                yield return new WaitForSeconds(0.3f);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                var filePath = Path.Combine(Program.PATH_EXPANSIONS, Path.GetFileName(url));
                if (!Directory.Exists(Program.PATH_EXPANSIONS))
                    Directory.CreateDirectory(Program.PATH_EXPANSIONS);
                File.WriteAllBytes(filePath, request.downloadHandler.data);
                MessageManager.Cast(InterString.Get("下载成功。"));
                Program.instance.InitializeForDataChange();
            }
            else
            {
                //MessageManager.Cast(InterString.Get("下载失败。"));

                string errorMessage = string.Empty;

                // 1. 证书安全问题（SSL错误）
                if (request.result == UnityWebRequest.Result.ConnectionError &&
                    (request.error.Contains("SSL") || request.error.Contains("certificate")))
                {
                    List<string> selections = new()
                    {
                        InterString.Get("下载链接不安全"),
                        InterString.Get("安全证书验证失败！连接可能不安全。@n是否继续下载？"),
                        InterString.Get("确认"),
                        InterString.Get("取消"),
                        Config.STRING_YES
                    };
                    unsafeDownloadUrl = url;
                    UIManager.ShowPopupYesOrNo(selections, DownloadYpkUnsafe, null);
                }
                // 2. 网络连接错误
                else if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    errorMessage = InterString.Get("网络连接失败：") + request.error;
                }
                // 3. HTTP协议错误（404, 500等）
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    errorMessage = InterString.Get("服务器返回错误：") + request.responseCode;
                }
                // 4. 数据处理错误
                else if (request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    errorMessage = InterString.Get("数据处理错误：") + request.error;
                }
                // 5. 其他未知错误
                else
                {
                    errorMessage = InterString.Get("未知错误：") + request.error;
                }

                if(errorMessage != string.Empty)
                    MessageManager.Cast(errorMessage);
            }
            GetUI<SettingServantUI>().ButtonDownloadYPK.SetModeText(string.Empty);
            downloadingYPK = false;
        }

        private void DownloadYpkUnsafe()
        {
            StartCoroutine(DownloadYpkUnsafeAsync(unsafeDownloadUrl));
        }

        private IEnumerator DownloadYpkUnsafeAsync(string url)
        {
            downloadingYPK = true;
            using var request = UnityWebRequest.Get(url);
            request.certificateHandler = new AcceptAllCertificateHandler();
            request.SendWebRequest();
            while (!request.isDone)
            {
                GetUI<SettingServantUI>().ButtonDownloadYPK.SetModeText((request.downloadProgress * 100f).ToString("0.##") + "%");
                yield return new WaitForSeconds(0.3f);
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                var filePath = Path.Combine(Program.PATH_EXPANSIONS, Path.GetFileName(url));
                if (!Directory.Exists(Program.PATH_EXPANSIONS))
                    Directory.CreateDirectory(Program.PATH_EXPANSIONS);
                File.WriteAllBytes(filePath, request.downloadHandler.data);
                MessageManager.Cast(InterString.Get("下载成功。"));
                Program.instance.InitializeForDataChange();
            }
            else
                MessageManager.Cast(InterString.Get("下载失败。"));
            GetUI<SettingServantUI>().ButtonDownloadYPK.SetModeText(string.Empty);
            downloadingYPK = false;
        }
    }
}
