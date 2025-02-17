using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MDPro3.Servant;
using MDPro3.UI.ServantUI;

namespace MDPro3.UI
{
    public class SelectionToggle_Puzzle : SelectionToggle_ScrollRectItem
    {
        public PuzzleSelectorUI.Puzzle puzzle;

        public override void Refresh()
        {
            base.Refresh();
            Manager.GetElement<TextMeshProUGUI>("Title").text = puzzle.name;

            Manager.GetElement("NumBadge").SetActive(!Config.GetBool(Program.puzzlePath + puzzle.name + "_Enter", false));
            Manager.GetElement("TextClear").SetActive(Config.GetBool(Program.puzzlePath + puzzle.name + "_Clear", false));
        }

        protected override IEnumerator RefreshAsync()
        {
            refreshed = false;
            while (TextureManager.container == null)
                yield return null;

            var face = Manager.GetElement<RawImage>("Image");
            face.texture = TextureManager.container.black.texture;
            var task = TextureManager.LoadArtAsync(int.Parse(puzzle.firstCard), true);
            while (!task.IsCompleted)
                yield return null;
            face.texture = task.Result;

            if (Program.instance.puzzle.currentPuzzle == Program.puzzlePath + puzzle.name)
                CallToggleOnEvent();

            enumerator = null;
            refreshed = true;
        }

        protected override void CallToggleOnEvent()
        {
            base.CallToggleOnEvent();

            Program.instance.puzzle.GetUI<PuzzleSelectorUI>().superScrollView.selected = index;
            Program.instance.puzzle.GetUI<PuzzleSelectorUI>().SetOverview(puzzle.description + "\r\n" + puzzle.solution);

            Program.instance.puzzle.GetUI<PuzzleSelectorUI>().Art.SetArt(int.Parse(puzzle.firstCard));
            Program.instance.puzzle.currentPuzzle = Program.puzzlePath + puzzle.name;
            Program.instance.puzzle.lastPuzzleItem = this;
        }

        protected override void CallSubmitEvent()
        {
            base.CallSubmitEvent();
            Program.instance.puzzle.StartCurrentPuzzle();
        }

        protected override void OnNavigation(AxisEventData eventData)
        {
            base.OnNavigation(eventData);

            if (eventData.moveDir == MoveDirection.Right)
            {
                UserInput.NextSelectionIsAxis = true;
                Program.instance.puzzle.GetUI<PuzzleSelectorUI>()
                    .ButtonPlay.GetSelectable().Select();
            }
        }
    }
}
