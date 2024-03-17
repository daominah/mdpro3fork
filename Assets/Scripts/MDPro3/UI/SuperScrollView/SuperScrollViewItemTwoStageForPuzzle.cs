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

        public bool refreshed;
        IEnumerator enumerator;
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
            refreshed = false;
            title.text = puzzle.name;
            if (enumerator != null)
                StopCoroutine(enumerator);
            enumerator = RefreshFace();
            StartCoroutine(enumerator);
            action = () =>
            {
                Program.I().puzzle.StartPuzzle(puzzle.name);
            };
        }

        IEnumerator RefreshFace()
        {
            while (TextureManager.container == null)
                yield return null;
            face.texture = TextureManager.container.black.texture;
            IEnumerator ie = Program.I().texture_.LoadArtAsync(int.Parse(puzzle.firstCard), true);
            StartCoroutine(ie);
            while (ie.MoveNext())
                yield return null;
            face.texture = ie.Current as Texture2D;
            face.color = Color.white;
            enumerator = null;
            refreshed = true;
        }
    }
}
