using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class SuperScrollViewItemTwoStageForPuzzle : SuperScrollViewItemTwoStage
    {
        public Text title;
        public RawImage face;
        public SelectPuzzle.Puzzle puzzle;

        public override void OnSelected()
        {
            base.OnSelected();
            Program.I().puzzle.superScrollView.selected = id;
            Program.I().puzzle.description.text = puzzle.description + "\r\n" + puzzle.solution;
            Program.I().puzzle.description.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Program.I().puzzle.cardImage.texture = face.texture;
        }

        public override void Refresh()
        {
            base.Refresh();
            title.text = puzzle.name;
            action = () =>
            {
                Program.I().puzzle.StartPuzzle(Program.puzzlePath + puzzle.name);
            };
        }

        public override IEnumerator RefreshAsync()
        {
            while (TextureManager.container == null)
                yield return null;

            face.texture = TextureManager.container.black.texture;

            var task = TextureManager.LoadArtAsync(int.Parse(puzzle.firstCard), true);
            while(!task.IsCompleted)
                yield return null;
            face.color = Color.white;
            face.texture = task.Result;

            enumerator = null;
            refreshed = true;
        }
    }
}
