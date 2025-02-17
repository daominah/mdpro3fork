using MDPro3.UI;
using MDPro3.UI.ServantUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MDPro3.Servant
{
    public class PuzzleSelector : Servant
    {

        [HideInInspector] public string currentPuzzle;
        [HideInInspector] public SelectionToggle_Puzzle lastPuzzleItem;
        private PercyOCG percy;

        #region Servant

        public override int Depth => 1;
        protected override bool ShowLine => false;

        public override void Initialize()
        {
            returnServant = Program.instance.menu;
            base.Initialize();
        }

        protected override void FirstLoadEvent()
        {
            base.FirstLoadEvent();
            StartCoroutine(Program.instance.texture_.SetCommonShopButtonMaterial
                (GetUI<PuzzleSelectorUI>().ImageOut, false));
            StartCoroutine(Program.instance.texture_.SetCommonShopButtonMaterial
                (GetUI<PuzzleSelectorUI>().ImageHover, true));
        }

        public override void Select()
        {
            EventSystem.current.SetSelectedGameObject(lastPuzzleItem.gameObject);
        }

        public override void OnExit()
        {
            if(Program.exitOnReturn)
                Program.GameQuit();
            else
                Program.instance.ShiftToServant(returnServant);
        }

        #endregion

        public void PrintPuzzles()
        {
            if (servantUI == null)
                return;

            GetUI<PuzzleSelectorUI>().Print();
        }

        public void StartCurrentPuzzle()
        {
            StartPuzzle(currentPuzzle);
        }

        public void StartPuzzle(string puzzle)
        {
            percy?.Dispose();
            percy = new PercyOCG();
            percy.StartPuzzle(puzzle + ".lua");
        }

        public void SelectLastPuzzleItem()
        {
            UserInput.NextSelectionIsAxis = true;
            Select();
        }
    }
}
