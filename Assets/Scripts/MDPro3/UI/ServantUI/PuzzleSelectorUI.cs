using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using MDPro3.UI.PropertyOverride;

namespace MDPro3.UI.ServantUI
{
    public class PuzzleSelectorUI : ServantUI
    {

        #region Elements

        private const string LABEL_SBN_PLAY = "ButtonPlay";
        private SelectionButton m_ButtonPlay;
        public SelectionButton ButtonPlay =>
            m_ButtonPlay = m_ButtonPlay != null ? m_ButtonPlay
            : Manager.GetElement<SelectionButton>(LABEL_SBN_PLAY);

        private const string LABEL_SR = "ScrollRect";
        private ScrollRect m_ScrollRect;
        private ScrollRect ScrollRect =>
            m_ScrollRect = m_ScrollRect != null ? m_ScrollRect
            : Manager.GetElement<ScrollRect>(LABEL_SR);

        private const string LABEL_TXT_OVERVIEW = "TextOverview";
        private TextMeshProUGUI m_TextOverview;
        private TextMeshProUGUI TextOverview =>
            m_TextOverview = m_TextOverview != null ? m_TextOverview
            : Manager.GetElement<TextMeshProUGUI>(LABEL_TXT_OVERVIEW);

        private const string LABEL_MONO_ART = "ArtImage";
        private ArtRawImageHandler m_Art;
        public ArtRawImageHandler Art =>
            m_Art = m_Art != null ? m_Art
            : Manager.GetElement<ArtRawImageHandler>(LABEL_MONO_ART);

        private const string LABEL_IMG_HOVER = "ButtonHover";
        private Image m_ImageHover;
        public Image ImageHover =>
            m_ImageHover = m_ImageHover != null ? m_ImageHover
            : Manager.GetElement<Image>(LABEL_IMG_HOVER);

        private const string LABEL_IMG_OUT = "ButtonOut";
        private Image m_ImageOut;
        public Image ImageOut =>
            m_ImageOut = m_ImageOut != null ? m_ImageOut
            : Manager.GetElement<Image>(LABEL_IMG_OUT);

        #endregion

        public struct Puzzle
        {
            public string name;
            public string firstCard;
            public string description;
            public string solution;
        }
        private List<Puzzle> puzzles;
        public SuperScrollView superScrollView;

        private void Awake()
        {
            GetPuzzles();
            Print();
        }

        private void GetPuzzles()
        {
            puzzles = new List<Puzzle>();
            if (!Directory.Exists(Program.PATH_PUZZLE))
                Directory.CreateDirectory(Program.PATH_PUZZLE);
            FileInfo[] fileInfos = new DirectoryInfo(Program.PATH_PUZZLE).GetFiles("*.lua");
            foreach (FileInfo fileInfo in fileInfos)
            {
                string text = File.ReadAllText(fileInfo.FullName);
                string st = text.Replace("\r", "");
                string[] lines = st.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string card = "0";
                int messageStart = 0;
                int messageEnd = 0;
                int solutionStart = 0;
                int solutionEnd = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("Debug.AddCard(") && card == "0")
                        card = lines[i].Replace("Debug.AddCard(", "").Split(',')[0];
                    else if (lines[i].StartsWith("--[[message"))
                        messageStart = i + 1;
                    else if (lines[i].StartsWith("Solution:"))
                        solutionStart = i;
                    else if (lines[i].StartsWith("]]"))
                    {
                        if (messageEnd == 0)
                            messageEnd = i;
                        else
                            solutionEnd = i;
                    }
                }
                string description = "";
                string solution = "";
                if (messageStart != 0 && messageEnd != 0)
                    for (int i = messageStart; i < messageEnd; i++)
                        description += lines[i] + Program.STRING_LINE_BREAK;
                if (solutionStart != 0 && solutionEnd != 0)
                    for (int i = solutionStart; i < solutionEnd; i++)
                        solution += lines[i] + Program.STRING_LINE_BREAK;
                description = description.Replace("\r\n\t\r\n\t", "\r\n\t");
                Puzzle puzzle = new()
                {
                    name = fileInfo.Name.Replace(Program.EXPANSION_LUA, string.Empty),
                    firstCard = card,
                    description = description,
                    solution = solution,
                };
                puzzles.Add(puzzle);
            }
        }


        public void Print()
        {
            superScrollView?.Clear();
            List<string[]> tasks = new();

            for (int i = 0; i < puzzles.Count; i++)
            {
                string[] task = new string[]
                {
                    i.ToString(),
                    puzzles[i].name,
                    puzzles[i].firstCard,
                    puzzles[i].description,
                    puzzles[i].solution
                };
                tasks.Add(task);
            }
            var handle = Addressables.LoadAssetAsync<GameObject>("UI/ItemPuzzle.prefab");
            handle.Completed += (result) =>
            {
                var itemHeight = PropertyOverrider.NeedMobileLayout() ? 180f : 150f;
                float topPadding = PropertyOverrider.NeedMobileLayout() ? 148f : 134f;
                float space = itemHeight - (PropertyOverrider.NeedMobileLayout() ? 152f : 122f);
                float bottomPadding = (PropertyOverrider.NeedMobileLayout() ? 64f : 54f) - space;
                superScrollView = new SuperScrollView(
                    1,
                    700,
                    itemHeight,
                    topPadding,
                    bottomPadding,
                    result.Result,
                    ItemOnListRefresh,
                    ScrollRect);
                superScrollView.Print(tasks);
                SelectFirst();
            };
        }

        private void SelectFirst()
        {
            if (superScrollView == null || superScrollView.gameObjects.Count == 0)
                return;

            var item0 = superScrollView.items[0].gameObject.GetComponent<SelectionToggle_Puzzle>();
            item0.SetToggleOn();
        }

        private void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SelectionToggle_Puzzle>();
            Puzzle puzzle = new()
            {
                name = task[1],
                firstCard = task[2],
                description = task[3],
                solution = task[4],
            };
            handler.puzzle = puzzle;
            handler.Refresh();
        }

        public void SetOverview(string overview)
        {
            TextOverview.text = overview;
            TextOverview.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        public void OnPlayPuzzle()
        {
            Program.instance.puzzle.StartCurrentPuzzle();
        }

        public void SelectLastPuzzleItem()
        {
            UserInput.NextSelectionIsAxis = true;
            Program.instance.puzzle.lastPuzzleItem.GetSelectable().Select();
        }
    }
}