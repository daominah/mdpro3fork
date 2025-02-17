using MDPro3.UI.ServantUI;
using Percy;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SelectionToggle_Replay : SelectionToggle_ScrollRectItem
    {
        public string replayName;
        private YRP yrp;

        public override void Refresh()
        {
            Manager.GetElement<TextMeshProUGUI>("Title").text = replayName;
            yrp = Program.instance.replay.GetUI<ReplaySelectorUI>().CacheYRP(replayName);

            Manager.GetElement("NumBadge").SetActive(false);
            Manager.GetElement("TextClear").SetActive(false);

            base.Refresh();
        }

        protected override IEnumerator RefreshAsync()
        {
            refreshed = false;
            while (TextureManager.container == null)
                yield return null;

            var face = Manager.GetElement<RawImage>("Image");
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
            face.texture = task.Result;

            enumerator = null;
            refreshed = true;
        }

        protected override void CallToggleOnEvent()
        {
            base.CallToggleOnEvent();
            Program.instance.replay.lastSelectedReplayItem = this;
            Program.instance.replay.GetUI<ReplaySelectorUI>().superScrollView.selected = index;

            var ui = Program.instance.replay.GetUI<ReplaySelectorUI>();
            if (yrp == null)
            {
                ui.TextOverview.text = string.Empty;
                ui.ButtonPlayer0.gameObject.SetActive(false);
                ui.ButtonPlayer1.gameObject.SetActive(false);
                ui.ButtonPlayer2.gameObject.SetActive(false);
                ui.ButtonPlayer3.gameObject.SetActive(false);
            }
            else
            {
                ui.ButtonPlayer0.gameObject.SetActive(true);
                ui.ButtonPlayer1.gameObject.SetActive(true);
                ui.ButtonPlayer2.gameObject.SetActive(true);
                ui.ButtonPlayer3.gameObject.SetActive(true);

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

                ui.ButtonPlayer0.SetButtonText(yrp.playerData[0].name);
                ui.ButtonPlayer1.SetButtonText(yrp.playerData[1].name);
                if (tag)
                {
                    ui.ButtonPlayer2.SetButtonText(yrp.playerData[2].name);
                    ui.ButtonPlayer3.SetButtonText(yrp.playerData[3].name);
                }
                else
                {
                    ui.ButtonPlayer2.gameObject.SetActive(false);
                    ui.ButtonPlayer3.gameObject.SetActive(false);
                }

                ui.TextOverview.text = description;
            }
        }

        protected override void CallSubmitEvent()
        {
            Program.instance.replay.GetUI<ReplaySelectorUI>().KF_Replay(replayName);
        }

        protected override void OnNavigation(AxisEventData eventData)
        {
            base.OnNavigation(eventData);

            if (eventData.moveDir == MoveDirection.Right)
            {
                UserInput.NextSelectionIsAxis = true;
                var deckButton = Program.instance.replay.GetUI<ReplaySelectorUI>().ButtonPlayer0;
                if (deckButton.gameObject.activeSelf)
                    deckButton.GetSelectable().Select();
                else
                    Program.instance.replay.GetUI<ReplaySelectorUI>().ButtonGodView.GetSelectable().Select();
            }
        }
    }
}
