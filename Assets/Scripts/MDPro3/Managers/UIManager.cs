using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using MDPro3.YGOSharp;
using Spine.Unity;
using TMPro;
using UnityEngine.AddressableAssets;
using MDPro3.UI;

namespace MDPro3
{
    public class UIManager : Manager
    {
        [Header("Public Reference")]
        public CanvasGroup wallpaper;
        public Button btnExit;
        public CanvasGroup line;
        public Image blackBack;
        public Text fps;
        public RectTransform popup;
        public RectTransform transition;
        public RectTransform duelButton;

        public static string currentWallpaper;

        [Header("Source Reference")]
        public Font cnFont;
        public Font jpFont;
        public Font cnMenuFont;
        public TMP_FontAsset tmpFont;
        public TMP_FontAsset tmpFontForPhaseButton;

        public override void Initialize()
        {
            base.Initialize();

            currentWallpaper = Config.Get("Wallpaper", Program.items.wallpapers[0].id.ToString());
            ChangeWallPaper(currentWallpaper);
            InitializeLanguage();
        }
        public static void Translate(GameObject go)
        {
            foreach (var text in go.GetComponentsInChildren<Text>(true))
                if (text.name.StartsWith("#Text"))
                    text.text = InterString.Get(text.text.Replace("\r\n", "@n"));
        }

        public static void InitializeLanguage()
        {
            InterString.Initialize();
            StringHelper.Initialize();
            CardsManager.Initialize();
            Program.items.Initialize();
            Program.I().cardRenderer.SwitchLanguage();
            UIManager instance = Program.I().ui_;
            foreach (var text in instance.GetComponentsInChildren<Text>(true))
            {
                if (text.name.StartsWith("#Text"))
                {
                    text.text = InterString.Get(text.text);
                    if (text.name.EndsWith("Menu"))
                    {
                        if ((Config.Get("Language", "zh-CN") == "en-US"
                            || Config.Get("Language", "zh-CN") == "ja-JP"))
                            text.font = instance.jpFont;
                        else if (Config.Get("Language", "zh-CN") == "zh-CN")
                            text.font = instance.cnMenuFont;
                        else
                            text.font = instance.cnFont;
                    }
                }
            }
        }

        public static void ChangeLanguage()
        {
            UIManager instance = Program.I().ui_;
            foreach (var text in instance.GetComponentsInChildren<Text>(true))
                if (text.name.StartsWith("#Text"))
                    text.text = InterString.GetOriginal(text.text);
            TextureManager.ClearCache();
            InitializeLanguage();
            Program.I().cutin.Load();
            Program.I().mate.Load();
            Program.I().solo.Load();
            Online.severSelectionsInitialized = false;
        }

        IEnumerator LoadDiyWallpaperAsync(string path, Transform parent)
        {
            var ie = ABLoader.LoadFromFileAsync(path);
            while (ie.MoveNext())
                yield return null;

            GameObject dynamic = ie.Current;
            dynamic.transform.SetParent(parent, false);
        }

        public Transform ChangeWallPaper(string path)
        {
            if (wallpaper.transform.childCount > 0)
                Destroy(wallpaper.transform.GetChild(0).gameObject);
            path = Program.items.WallpaperCodeToPath(path);
            if (!path.Contains("front"))
            {
                Transform frontback = ChangeWallPaper("1130002");
                Destroy(frontback.GetChild(1).gameObject);
                StartCoroutine(LoadDiyWallpaperAsync(path, frontback));
                return frontback;
            }
            GameObject frontLoader = ABLoader.LoadFromFolder(path);
            Destroy(frontLoader);
            var front = frontLoader.transform.GetChild(0).GetComponent<RectTransform>();
            front.SetParent(wallpaper.transform, false);
            for (int i = 0; i < front.transform.childCount; i++)
                front.transform.GetChild(i).gameObject.AddComponent<RectLoopMoveY>();

            foreach (ParticleSystem p in front.GetComponentsInChildren<ParticleSystem>(true))
                p.Play();
            return front;
        }
        public static void ShowWallpaper(float time)
        {
            UIManager instance = Program.I().ui_;
            instance.wallpaper.gameObject.SetActive(true);
            DOTween.To(() => instance.wallpaper.alpha, x => instance.wallpaper.alpha = x, 1, time);
            foreach (var p in instance.wallpaper.transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                p.gameObject.SetActive(true);
                p.Play();
            }
            foreach (var skeleton in instance.wallpaper.transform.GetComponentsInChildren<SkeletonAnimation>())
            {
                //skeleton.GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
                skeleton.GetComponent<Renderer>().material.DOFade(1f, time - 0.1f).OnComplete(() => { });

            }
        }
        public static void HideWallpaper(float time)
        {
            DOTween.To(() => Program.I().ui_.wallpaper.alpha, x => Program.I().ui_.wallpaper.alpha = x, 0, time).OnComplete(() =>
                Program.I().ui_.wallpaper.gameObject.SetActive(false));
            foreach (var p in Program.I().ui_.wallpaper.transform.GetComponentsInChildren<ParticleSystem>(true))
                p.gameObject.SetActive(false);
            foreach (var skeleton in Program.I().ui_.wallpaper.transform.GetComponentsInChildren<SkeletonAnimation>())
                skeleton.GetComponent<Renderer>().material.DOFade(0f, time - 0.1f).OnComplete(() => { });
        }
        public static void ShowExitButton(float time)
        {
            Program.I().ui_.btnExit.GetComponent<RectTransform>().DOAnchorPosY(-65f, time);
        }

        public static void HideExitButton(float time)
        {
            Program.I().ui_.btnExit.GetComponent<RectTransform>().DOAnchorPosY(65f, time);
        }

        public static void ShowLine(float time)
        {
            Program.I().ui_.line.DOFade(1f, time);
        }
        public static void HideLine(float time)
        {
            Program.I().ui_.line.DOFade(0f, time);
        }
        public static void ShowFPS()
        {
            Program.I().ui_.fps.gameObject.SetActive(true);
        }
        public static void HideFPS()
        {
            Program.I().ui_.fps.gameObject.SetActive(false);
        }

        public static void ShowFPSLeft()
        {
            Program.I().ui_.fps.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            Program.I().ui_.fps.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            Program.I().ui_.fps.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, 0);
        }
        public static void ShowFPSRight()
        {
            Program.I().ui_.fps.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            Program.I().ui_.fps.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            Program.I().ui_.fps.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }

        public static void SetFpsSize(int size)
        {
            Program.I().ui_.fps.fontSize = size;
        }

        public static void ShowPopupSelection(List<string> selections, Action returnAction, Action closeAction = null)
        {
            var handle = Addressables.InstantiateAsync("PopupSelection");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                PopupSelection popupSelection = result.Result.GetComponent<PopupSelection>();
                popupSelection.selections = selections;
                popupSelection.returnAction = returnAction;
                popupSelection.closeAction = closeAction;
                popupSelection.Show();
            };
        }
        public static void ShowPopupYesOrNo(List<string> selections, Action confirmAction, Action cancelAction)
        {
            var handle = Addressables.InstantiateAsync("PopupYesOrNo");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                PopupYesOrNo popupYesOrNo = result.Result.GetComponent<PopupYesOrNo>();
                popupYesOrNo.selections = selections;
                popupYesOrNo.confirmAction = confirmAction;
                popupYesOrNo.cancelAction = cancelAction;
                popupYesOrNo.Show();
            };
        }

        public static void ShowPopupConfirm(List<string> selections)
        {
            var handle = Addressables.InstantiateAsync("PopupConfirm");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                var popupConfirm = result.Result.GetComponent<PopupConfirm>();
                popupConfirm.selections = selections;
                popupConfirm.Show();
            };
        }

        public static void ShowPopupInput(List<string> selections, Action<string> confirmAction, Action cancelAction, InputValidation.ValidationType type = InputValidation.ValidationType.None)
        {
            var handle = Addressables.InstantiateAsync("PopupInput");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                PopupInput popupInput = result.Result.GetComponent<PopupInput>();
                popupInput.selections = selections;
                popupInput.confirmAction = confirmAction;
                popupInput.cancelAction = cancelAction;
                popupInput.validationType = type;
                popupInput.Show();
            };
        }

        public static void ShowPopupFilter()
        {
            var handle = Addressables.InstantiateAsync("PopupSearchFilter");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                var popupSearchFilter = result.Result.GetComponent<PopupSearchFilter>();
                popupSearchFilter.Show();
            };
        }
        public static void ShowPopupText(List<string> selections)
        {
            var handle = Addressables.InstantiateAsync("PopupText");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                var popupText = result.Result.GetComponent<PopupText>();
                popupText.selections = selections;
                popupText.Show();
            };
        }

        public static void ShowPopupServer(List<string> selections)
        {
            var handle = Addressables.InstantiateAsync("PopupServer");
            handle.Completed += (result) =>
            {
                result.Result.transform.SetParent(Program.I().ui_.popup, false);
                var popupServer = result.Result.GetComponent<PopupServer>();
                popupServer.selections = selections;
                popupServer.Show();
            };
        }
        public static void UIBlackIn(float time)
        {
            float width = Screen.width * 1080 * 1.7f / Screen.height;
            Program.I().ui_.transition.sizeDelta = new Vector2(0, 0);
            DOTween.To(() => Program.I().ui_.transition.sizeDelta, x => Program.I().ui_.transition.sizeDelta = x, new Vector2(width, width), time);
        }
        public static void UIBlackOut(float time)
        {
            DOTween.To(() => Program.I().ui_.transition.sizeDelta, x => Program.I().ui_.transition.sizeDelta = x, new Vector2(0, 0), time);
        }

        public static Vector2 WorldToScreenPoint(Camera camera, Vector3 positon)
        {
            var screenPosition = camera.WorldToScreenPoint(positon);
            var sizeDelta = Program.I().ui_.GetComponent<RectTransform>().sizeDelta;
            return new Vector2(screenPosition.x * sizeDelta.x / Screen.width, screenPosition.y * sizeDelta.y / Screen.height);
        }
        public static Vector2 ScreenToNoScalerScreenPoint(Vector2 position)
        {
            var sizeDelta = Program.I().ui_.GetComponent<RectTransform>().sizeDelta;
            return new Vector2(position.x * Screen.width / sizeDelta.x, position.y * Screen.height / sizeDelta.y);
        }
        public static Vector3 ScreenToWorldPoint(Camera camera, Vector2 positon)
        {
            var screenPosition = ScreenToNoScalerScreenPoint(positon);
            return camera.ScreenToWorldPoint(screenPosition);
        }

        public static float ScreenLengthWithoutScalerX(float length)
        {
            var sizeDelta = Program.I().ui_.GetComponent<RectTransform>().sizeDelta;
            return length * sizeDelta.x / Screen.width;
        }
        public static float ScreenLengthWithScalerX(float length)
        {
            var sizeDelta = Program.I().ui_.GetComponent<RectTransform>().sizeDelta;
            return length * Screen.width / sizeDelta.x;
        }

        public static float ScreenLengthWithoutScalerY(float length)
        {
            var sizeDelta = Program.I().ui_.GetComponent<RectTransform>().sizeDelta;
            return length * sizeDelta.y / Screen.height;
        }

        public static Vector2 GetMousePositionToAnchorPosition()
        {
            var returnValue = Input.mousePosition;
            var uiWidth = 1080f * Screen.width / Screen.height;
            returnValue.x = returnValue.x * uiWidth / Screen.width;
            returnValue.y = returnValue.y * 1080 / Screen.height;
            returnValue.x -= uiWidth / 2f;
            returnValue.y -= 540;
            return returnValue;
        }

    }
}
