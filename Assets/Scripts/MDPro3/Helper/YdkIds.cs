using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using MDPro3.YGOSharp;

namespace MDPro3
{
    public static class YdkIds
    {
        const string ydkIdsPath = "Data/YdkIds.txt";
        const string pattern = @"<card mrk='(\d+)'/>";

        static bool initialized;
        static Dictionary<int, int> ydkIds = new Dictionary<int, int>();
        static void Initialize()
        {
            var texts = File.ReadAllText(ydkIdsPath);
            var lines = texts.Replace("\r", string.Empty).Split('\n');
            foreach (var line in lines)
            {
                var pair = line.Split(' ');
                if (pair.Length > 1)
                {
                    try
                    {
                        if (!ydkIds.ContainsKey(int.Parse(pair[1])))
                            ydkIds.Add(int.Parse(pair[1]), int.Parse(pair[0]));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Read YdkIds.txt Error: " + e);
                    }
                }
            }

            initialized = true;
        }

        static string EvaluatorGetNameFromNumber(Match match)
        {
            string numberString = match.Groups[1].Value;
            int cardCode = int.Parse(numberString);
            ydkIds.TryGetValue(cardCode, out var code);
            if (code != 0)
                return CardsManager.Get(code).Name;
            else
                return CardsManager.Get(cardCode).Name;
        }


        public static string ReplaceWithCardName(string origin)
        {
            if(!initialized)
                Initialize();

            origin = origin.Replace(" get=\'name\'", string.Empty);
            return Regex.Replace(origin, pattern, EvaluatorGetNameFromNumber);
        }

    }
}
