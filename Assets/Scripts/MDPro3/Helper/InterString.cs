using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace MDPro3
{
    public static class InterString
    {
        private static readonly Dictionary<string, string> translations = new Dictionary<string, string>();
        private static string path;

        public static void Initialize()
        {
            string language = Config.Get("Language", "zh-CN");
            path = Program.localesPath + Program.slash + language + "/translation.conf";
            if (!File.Exists(path))
                File.Create(path).Close();

            var txtString = File.ReadAllText(path);
            var lines = txtString.Replace("\r", "").Split('\n');
            translations.Clear();
            for (var i = 0; i < lines.Length; i++)
            {
                var mats = Regex.Split(lines[i], "->");
                if (mats.Length == 2)
                    if (!translations.ContainsKey(mats[0]))
                        translations.Add(mats[0], mats[1]);
            }
        }

        public static string Get(string original)
        {
            var returnValue = original;
            if (translations.TryGetValue(original, out returnValue))
                return returnValue.Replace("@n", "\r\n").Replace("@ui", "");

            if (original != "")
            {
                try
                {
                    File.AppendAllText(path, original + "->" + original + "\r\n");
                }
                catch
                {
                    Program.noAccess = true;
                }

                translations.Add(original, original);
                return original.Replace("@n", "\r\n").Replace("@ui", "");
            }
            return original;
        }

        public static string Get(string original, string replace)
        {
            return Get(original).Replace("[?]", replace);
        }
        public static string GetOriginal(string value)
        {
            var returnValue = value;
            foreach (var translation in translations)
            {
                if (translation.Value == value)
                {
                    returnValue = translation.Key;
                    break;
                }
            }
            return returnValue;
        }

    }
}
