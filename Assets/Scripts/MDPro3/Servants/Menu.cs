using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Networking;
using YgomSystem.LocalFileSystem.Internal;

namespace MDPro3
{
    public class Menu : Servant
    {
        public Text title;
        //public Text debugText;
        public override void Initialize()
        {
            depth = 0;
            haveLine = false;
            base.Initialize();
            title.text = "MDPro3 v" + Application.version;
            StartCoroutine(CheckUpdate());
        }

        private IEnumerator CheckUpdate()
        {
            yield return new WaitForSeconds(1);
            var www = UnityWebRequest.Get("https://code.mycard.moe/sherry_chaos/MDPro3/-/raw/master/Version.txt");
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();
            try
            {
                var result = www.downloadHandler.text;
                var lines = result.Replace("\r", "").Split('\n');
                if (Application.version != lines[0])
                    MessageManager.Cast(InterString.Get("检测到新版本[[?]]。", lines[0]));
            }
            catch
            {
                MessageManager.Cast(InterString.Get("检查更新失败！"));
            }
        }


        public void OnSolo()
        {
            Program.I().ShiftToServant(Program.I().solo);
        }
        public void OnOnline()
        {
            Program.I().ShiftToServant(Program.I().online);
        }
        public void OnPuzzle()
        {
            Program.I().ShiftToServant(Program.I().puzzle);
        }
        public void OnReplay()
        {
            Program.I().ShiftToServant(Program.I().replay);
        }
        public void OnCutin()
        {
            Program.I().ShiftToServant(Program.I().cutin);
        }
        public void OnMate()
        {
            Program.I().ShiftToServant(Program.I().mate);
        }
        public void OnEditDeck()
        {
            Program.I().ShiftToServant(Program.I().selectDeck);
        }
        public void OnSetting()
        {
            Program.I().ShiftToServant(Program.I().setting);
        }
        public override void OnExit()
        {
            List<string> selections = new List<string>
        {
            InterString.Get("确认退出"),
            InterString.Get("即将退出应用程序，@n是否确认？"),
            InterString.Get("确认"),
            InterString.Get("取消")
        };
            UIManager.ShowPopupYesOrNo(selections, GameQuit, null);
        }

        public static void GameQuit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

        }

        public override void ApplyShowArrangement(int preDepth)
        {
            base.ApplyShowArrangement(preDepth);
            UIManager.ShowWallpaper(transitionTime);
        }
        public override void ApplyHideArrangement(int preDepth)
        {
            base.ApplyHideArrangement(preDepth);
            UIManager.HideWallpaper(transitionTime);
        }
        public override void PerFrameFunction()
        {
            if (isShowed)
            {
                if (
                    Input.GetKeyDown(KeyCode.Escape) //|| Input.GetKeyDown(KeyCode.Backspace)
                    || Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame
                    )
                {
                    exitPressedTime = Program.TimePassed();
                }
                if (
                Input.GetKeyUp(KeyCode.Escape) //|| Input.GetKeyUp(KeyCode.Backspace)
                || Gamepad.current != null && Gamepad.current.bButton.wasReleasedThisFrame
                )
                {
                    if (Program.TimePassed() - exitPressedTime < 300)
                        OnReturn();
                }
            }
        }
    }
}
