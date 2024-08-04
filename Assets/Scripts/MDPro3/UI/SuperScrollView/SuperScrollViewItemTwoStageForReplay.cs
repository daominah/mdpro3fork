using Percy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MDPro3.SelectPuzzle;

namespace MDPro3.UI
{
    public class SuperScrollViewItemTwoStageForReplay : SuperScrollViewItemTwoStage
    {
        public Text title;
        public RawImage face;
        public string replayName;
        YRP yrp;

        public override void OnSelected()
        {
            base.OnSelected();
            Program.I().replay.superScrollView.selected = id;
            Program.I().replay.buttons.SetActive(true);
            if (yrp == null)
            {
                Program.I().replay.description.text = "";
                Program.I().replay.btnPlayer1.gameObject.SetActive(false);
                Program.I().replay.btnPlayer2.gameObject.SetActive(false);
                Program.I().replay.btnPlayer3.gameObject.SetActive(false);
                Program.I().replay.btnPlayer4.gameObject.SetActive(false);
            }
            else
            {
                Program.I().replay.btnPlayer1.gameObject.SetActive(true);
                Program.I().replay.btnPlayer2.gameObject.SetActive(true);
                Program.I().replay.btnPlayer3.gameObject.SetActive(true);
                Program.I().replay.btnPlayer4.gameObject.SetActive(true);

                var description = "";
                bool tag = false;
                if ((yrp.opt & 0x20) > 0)
                {
                    description += StringHelper.GetUnsafe(1246) + "\r\n";//双打模式
                    tag = true;
                }
                description += StringHelper.GetUnsafe(1259 + (yrp.opt >> 16)) + "\r\n";//规则
                description += StringHelper.GetUnsafe(1231) + yrp.StartLp + "\r\n";//初始基本分：
                description += StringHelper.GetUnsafe(1232) + yrp.StartHand + "\r\n";//初始手卡数：
                description += StringHelper.GetUnsafe(1233) + yrp.DrawCount + "\r\n";//每回合抽卡：
                if ((yrp.opt & 0x10) > 0)
                    description += StringHelper.GetUnsafe(1230) + "\r\n";

                Program.I().replay.btnPlayer1.transform.GetChild(0).GetComponent<Text>().text = yrp.playerData[0].name;
                Program.I().replay.btnPlayer2.transform.GetChild(0).GetComponent<Text>().text = yrp.playerData[1].name;
                if (tag)
                {
                    Program.I().replay.btnPlayer3.transform.GetChild(0).GetComponent<Text>().text = yrp.playerData[2].name;
                    Program.I().replay.btnPlayer4.transform.GetChild(0).GetComponent<Text>().text = yrp.playerData[3].name;
                }
                else
                {
                    Program.I().replay.btnPlayer3.gameObject.SetActive(false);
                    Program.I().replay.btnPlayer4.gameObject.SetActive(false);
                }

                Program.I().replay.description.text = description;
            }
        }

        public override void Refresh()
        {
            title.text = replayName;
            yrp = Program.I().replay.CacheYRP(replayName);
            action = () =>
            {
                Program.I().replay.KF_Replay(replayName);
            };
            base.Refresh();
        }

        public override IEnumerator RefreshAsync()
        {
            while (TextureManager.container == null)
                yield return null;

            face.texture = TextureManager.container.black.texture;

            if (yrp == null)
            {
                face.texture = TextureManager.container.unknownArt.texture;
                face.color = Color.white;
                enumerator = null;
                yield break;
            }

            var task = TextureManager.LoadArtAsync(yrp.playerData[0].main[0], true);
            while (!task.IsCompleted)
                yield return null;
            face.color = Color.white;
            face.texture = task.Result;

            enumerator = null;
            refreshed = true;
        }
    }
}
