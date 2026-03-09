using MDPro3.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;
using Cysharp.Threading.Tasks;

namespace MDPro3.UI
{
    public class SelectionToggle_Watch : SelectionToggle_ScrollRectItem
    {
        [Header("SelectionToggle Watch")]


        public string roomId;
        public string roomTitile;
        public string player0Name;
        public string player1Name;
        public string arena;
        public MyCardRoomOptions options = new();
        public override void Refresh()
        {
            base.Refresh();

            var player0Label = Manager.GetElement<TextMeshProUGUI>("Player0Name");
            var player1Label = Manager.GetElement<TextMeshProUGUI>("Player1Name");

            if (player0Label != null)
                player0Label.text = GetSafeDisplayName(player0Name, player0Label);
            if (player1Label != null)
                player1Label.text = GetSafeDisplayName(player1Name, player1Label);
        }

        private static string GetSafeDisplayName(string name, TextMeshProUGUI label)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            if (label == null || label.font == null)
                return name;

            if (label.font.HasCharacters(name, out uint[] missingCharacters, true, true))
                return name;

            if (missingCharacters == null || missingCharacters.Length == 0)
                return name;

            var missingSet = new HashSet<uint>(missingCharacters);
            var builder = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                var code = name[i];
                builder.Append(missingSet.Contains(code) ? '?' : name[i]);
            }
            return builder.ToString();
        }

        protected override async UniTask RefreshAsync()
        {
            refreshed = false;

            Manager.GetElement<RawImage>("Face0").texture = Appearance.defaultFace0.texture;
            Manager.GetElement<RawImage>("Face1").texture = Appearance.defaultFace1.texture;

            Manager.GetElement<RawImage>("Face0").texture = await MyCard.GetAvatarAsync(player0Name);
            Manager.GetElement<RawImage>("Face1").texture = await MyCard.GetAvatarAsync(player1Name);

            refreshed = true;
        }

        protected override void CallToggleOnEvent()
        {
            base.CallToggleOnEvent();
            Program.instance.online.lastSelectedWatchItem = this;
        }

        protected override void CallSubmitEvent()
        {
            AudioManager.PlaySE("SE_MENU_DECIDE");
            base.CallSubmitEvent();
            _ = WaitPasswordToJoin();
        }

        private async UniTask WaitPasswordToJoin()
        {
            var password = await MyCard.GetJoinRoomPassword(options, roomId, MyCard.account.user.id);
            TcpHelper.LinkStart(MyCard.duelUrl, MyCard.account.user.username, MyCard.athleticPort.ToString(), password, false, null);
        }

        protected override void OnClick()
        {
            Program.instance.online.lastSelectedWatchItem = this;
            CallSubmitEvent();
        }

        protected override void ToggleOn()
        {
            isOn = true;
        }
        public override void ToggleOnNow()
        {
            isOn = true;
        }
        protected override void ToggleOff()
        {
            isOn = false;
        }
        public override void ToggleOffNow()
        {
            isOn = false;
        }

        protected override void OnNavigation(AxisEventData eventData)
        {
            var selfIndex = index;
            if (selfIndex < 0)
                selfIndex = transform.GetSiblingIndex();

            var count = Program.instance.online.GetUI<OnlineServantUI>()
                .PageMyCard.WatchList.superScrollView.items.Count;
            var columes = Program.instance.online.GetUI<OnlineServantUI>()
                .PageMyCard.WatchList.superScrollView.GetColumnCount();

            var targetIndex = selfIndex + 1;

            if (eventData.moveDir == MoveDirection.Left)
            {
                if (selfIndex % columes == 0)
                {
                    Program.instance.online.GetUI<OnlineServantUI>()
                        .PageMyCard.ButtonDeckSelector.GetSelectable().Select();
                    return;
                }
                targetIndex = index - 1;
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                if (selfIndex % columes == columes - 1 || index == count - 1)
                {
                    Program.instance.online.GetUI<OnlineServantUI>().PageMyCard.SelectDefault();
                    return;
                }
            }
            else if (eventData.moveDir == MoveDirection.Up)
                targetIndex = selfIndex - columes;
            else if (eventData.moveDir == MoveDirection.Down)
            {
                var lastLineLeft = count % columes;
                var bound = count - lastLineLeft - 1;
                if (lastLineLeft == 0)
                    bound -= columes;
                if (selfIndex > bound)
                    return;
                targetIndex = index + columes;
            }

            if (targetIndex < 0)
                return;
            if (targetIndex >= count)
                targetIndex = count - 1;
            if (targetIndex == index)
                return;

            for (int i = 0; i < transform.parent.childCount; i++)
            {
                var child = transform.parent.GetChild(i);
                if (!child.gameObject.activeSelf)
                    continue;

                var buttonIndex = child.GetComponent<SelectionButton>().index;
                if (buttonIndex < 0)
                    buttonIndex = i;

                if (buttonIndex == targetIndex)
                {
                    UserInput.NextSelectionIsAxis = true;
                    EventSystem.current.SetSelectedGameObject(transform.parent.GetChild(i).gameObject);
                    break;
                }
            }

        }
    }
}

