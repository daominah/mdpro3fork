using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

namespace MDPro3
{
    public static class Config
    {
        public static uint ClientVersion = 0x1360;

        private static readonly List<OneString> translations = new List<OneString>();

        private static string path;

        public static void Initialize(string path)
        {
            Config.path = path;
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                if (Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    Set("Language", "zh-CN");
                else if (Application.systemLanguage == SystemLanguage.ChineseTraditional)
                    Set("Language", "zh-TW");
                else if (Application.systemLanguage == SystemLanguage.Spanish)
                    Set("Language", "es-ES");
                else if (Application.systemLanguage == SystemLanguage.Japanese)
                    Set("Language", "ja-JP");
                else if (Application.systemLanguage == SystemLanguage.Korean)
                    Set("Language", "ko-KR");
                else
                    Set("Language", "en-US");
                Save();
            }
            var txtString = File.ReadAllText(path);
            var lines = txtString.Replace("\r", "").Split('\n');
            translations.Clear();
            for (var i = 0; i < lines.Length; i++)
            {
                var mats = Regex.Split(lines[i], "->");
                if (mats.Length == 2)
                {
                    var s = new OneString();
                    s.original = mats[0];
                    s.translated = mats[1];
                    translations.Add(s);
                }
            }
        }

        internal static float GetFloat(string v, string defaul)
        {
            var getted = 0;
            try
            {
                getted = int.Parse(Get(v, defaul));
            }
            catch (Exception)
            {
            }

            return getted / 100000f;
        }

        internal static void SetFloat(string v, float f)
        {
            Set(v, ((int)(f * 100000f)).ToString());
        }

        public static bool Have(string original)
        {
            var found = false;
            for (var i = 0; i < translations.Count; i++)
                if (translations[i].original == original)
                {
                    found = true;
                    break;
                }
            return found;
        }

        public static string Get(string original, string defau)
        {
            var return_value = defau;
            var found = false;
            for (var i = 0; i < translations.Count; i++)
                if (translations[i].original == original)
                {
                    return_value = translations[i].translated;
                    found = true;
                    break;
                }

            if (found == false)
                if (path != null)
                {
                    File.AppendAllText(path, original + "->" + defau + "\r\n");
                    var s = new OneString
                    {
                        original = original,
                        translated = defau
                    };
                    return_value = defau;
                    translations.Add(s);
                }

            return return_value.Replace("@ui", "");
        }
        public static void Set(string original, string setted)
        {
            var found = false;
            for (var i = 0; i < translations.Count; i++)
                if (translations[i].original == original)
                {
                    found = true;
                    translations[i].translated = setted;
                }

            if (found == false)
            {
                var s = new OneString();
                s.original = original;
                s.translated = setted;
                translations.Add(s);
            }
        }
        public static void Save()
        {
            var all = "";
            for (var i = 0; i < translations.Count; i++)
                all += translations[i].original + "->" + translations[i].translated + "\r\n";

            try
            {
                File.WriteAllText(path, all);
            }
            catch (Exception e)
            {
                Program.noAccess = true;
                Debug.Log(e);
            }
        }

        private class OneString
        {
            public string original = "";
            public string translated = "";
        }
    }
}
