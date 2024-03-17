using MDPro3.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace MDPro3
{
    public class SelectPuzzle : Servant
    {
        public ScrollRect scrollView;
        public Text description;
        public RawImage cardImage;
        static List<Puzzle> puzzles;
        public SuperScrollViewTwoStage superScrollView;
        List<string[]> tasks = new List<string[]>();
        public override void Initialize()
        {
            depth = 1;
            haveLine = true;
            returnServant = Program.I().menu;
            base.Initialize();
            GetPuzzles();
            Print();
            StartCoroutine(SelectZero());
        }
        IEnumerator SelectZero()
        {
            while (superScrollView == null)
                yield return null;

            var item0 = superScrollView.items[0].gameObject.GetComponent<SuperScrollViewItemTwoStageForPuzzle>();
            while (!item0.refreshed)
                yield return null;
            item0.ToStage1();
        }

        public struct Puzzle
        {
            public string name;
            public string firstCard;
            public string description;
            public string solution;
        }

        void GetPuzzles()
        {
            puzzles = new List<Puzzle>();
            if (!Directory.Exists("Puzzle"))
                Directory.CreateDirectory("Puzzle");
            FileInfo[] fileInfos = new DirectoryInfo("Puzzle").GetFiles("*.lua");
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
                        description += lines[i] + "\r\n";
                if (solutionStart != 0 && solutionEnd != 0)
                    for (int i = solutionStart; i < solutionEnd; i++)
                        solution += lines[i] + "\r\n";
                description = description.Replace("\r\n\t\r\n\t", "\r\n\t");
                Puzzle puzzle = new Puzzle
                {
                    name = fileInfo.Name.Replace(".lua", ""),
                    firstCard = card,
                    description = description,
                    solution = solution,
                };
                puzzles.Add(puzzle);
            }
        }

        void Print()
        {
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
            var handle = Addressables.LoadAssetAsync<GameObject>("ButtonForTwoStage");
            handle.Completed += (result) =>
            {
                superScrollView = new SuperScrollViewTwoStage
                    (
                    1,
                    700,
                    140,
                    0,
                    -10,
                    result.Result,
                    ItemOnListRefresh,
                    scrollView,
                    30
                    );
                superScrollView.Print(tasks);
            };
        }

        void ItemOnListRefresh(string[] task, GameObject item)
        {
            var handler = item.GetComponent<SuperScrollViewItemTwoStageForPuzzle>();
            handler.id = int.Parse(task[0]);
            Puzzle puzzle = new Puzzle
            {
                name = task[1],
                firstCard = task[2],
                description = task[3],
                solution = task[4],
            };
            handler.puzzle = puzzle;
            handler.Refresh();
        }
        private PercyOCG percy;
        public void StartPuzzle(string puzzle)
        {
            if (percy != null)
                percy.Dispose();
            percy = new PercyOCG();
            percy.StartPuzzle("puzzle/" + puzzle + ".lua");
        }
    }
}
