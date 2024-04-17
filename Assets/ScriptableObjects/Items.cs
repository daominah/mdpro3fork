using MDPro3.YGOSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MDPro3
{
    [CreateAssetMenu]
    public class Items : ScriptableObject
    {
        public static string nullString = "coming soon";
        [Serializable]
        public struct Item
        {
            public int id;
            public string m_name;
            public string name
            {
                get
                {
                    if (!diy)
                        m_name = instace.GetName(id);
                    return m_name;
                }
                set
                {
                    m_name = value;
                }
            }
            public string m_description;
            public string description
            {
                get
                {
                    if (!diy)
                        m_description = instace.GetDescription(id);
                    return m_description;
                }
                set
                {
                    m_description = value;
                }
            }
            public string path;
            public bool functional;
            public bool secondFace;
            public bool diy;
        }

        public List<Item> wallpapers;
        public List<Item> faces;
        public List<Item> frames;
        public List<Item> protectors;
        public List<Item> mats;
        public List<Item> graves;
        public List<Item> stands;
        public List<Item> mates;
        public List<Item> cases;

        public List<List<Item>> kinds;

        static string language = "";
        Dictionary<int, string> names = new Dictionary<int, string>();
        Dictionary<int, string> descriptions = new Dictionary<int, string>();

        static Items instace;
        public void Initialize()
        {
            instace = this;
            kinds = new List<List<Item>>()
        {
            wallpapers,
            faces,
            frames,
            protectors,
            mats,
            graves,
            stands,
            mates,
            cases,
        };
            if (language != Config.Get("Language", "zh-CN"))
            {
                language = Config.Get("Language", "zh-CN");
                LoadNames();
                LoadDescriptions();
            }
        }
        void LoadNames()
        {
            LoadData(0, Program.localesPath + Program.slash + language + "/IDS_ITEM.bytes");
        }
        void LoadDescriptions()
        {
            LoadData(1, Program.localesPath + Program.slash + language + "/IDS_ITEMDESC.bytes");
        }
        void LoadData(int type, string path)
        {
            var dictionary = new Dictionary<int, string>();
            var bytes = File.ReadAllBytes(path);
            List<int> positions = new List<int>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0xA9)
                    if (bytes[i + 1] == 0x49)
                        if (bytes[i + 2] == 0x44)
                            positions.Add(i);
            }
            for (int i = 0; i < positions.Count; i++)
            {
                var b_key = new List<byte>();
                var b_value = new List<byte>();
                for (int j = positions[i] + 3; j < positions[i] + 10; j++)
                    b_key.Add(bytes[j]);
                if (i == positions.Count - 1)
                {
                    for (int j = positions[i] + 10; j < bytes.Length; j++)
                        b_value.Add(bytes[j]);
                }
                else
                {
                    for (int j = positions[i] + 10; j < positions[i + 1]; j++)
                        b_value.Add(bytes[j]);
                }
                if (b_value.Count > 0)
                {
                    var blank = b_value[0];
                    b_value.RemoveAt(0);
                    if (blank >= 0xC2)
                        b_value.RemoveAt(0);
                    if (blank >= 0xE0)
                        b_value.RemoveAt(0);
                    if (blank >= 0xF0)
                        b_value.RemoveAt(0);
                }
                var key = int.Parse(Encoding.UTF8.GetString(b_key.ToArray()));
                var value = Encoding.UTF8.GetString(b_value.ToArray());
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, value);
            }
            if (type == 0)
                names = dictionary;
            else if (type == 1)
                descriptions = dictionary;
        }
        string GetName(int code)
        {
            names.TryGetValue(code, out var returnValue);
            if (string.IsNullOrEmpty(returnValue))
                returnValue = nullString;
            return returnValue;
        }
        string GetDescription(int code)
        {
            descriptions.TryGetValue(code, out var returnValue);
            if (string.IsNullOrEmpty(returnValue))
                return nullString;
            returnValue = returnValue.Replace(" get=\'name\'", string.Empty);
            string pattern = @"<card mrk='(\d+)'/>";
            return Regex.Replace(returnValue, pattern, EvaluatorGetNameFromNumber);
        }
        string EvaluatorGetNameFromNumber(Match match)
        {
            string numberString = match.Groups[1].Value;
            int cardCode = 0;
            switch (numberString)
            {
                case "18799":
                    cardCode = 27015862;
                    break;
                case "14648":
                    cardCode = 40441990;
                    break;
                case "15250":
                    cardCode = 20129614;
                    break;
                case "13670":
                    cardCode = 26077387;
                    break;
                case "19196":
                    cardCode = 80845034;
                    break;
                case "13982":
                    cardCode = 79698395;
                    break;
                case "10191":
                    cardCode = 14001430;
                    break;
                case "10793":
                    cardCode = 99795159;
                    break;
                case "15573":
                    cardCode = 34572613;
                    break;
                case "18003":
                    cardCode = 25550531;
                    break;
                case "16200":
                    cardCode = 24639891;
                    break;
            }
            return CardsManager.Get(cardCode).Name;
        }

        public string WallpaperCodeToPath(string code)
        {
            string returnValue = "Wallpaper/front0001";
            foreach (var item in wallpapers)
            {
                if (item.id.ToString() == code)
                {
                    returnValue = item.path;
                    break;
                }
            }
            return returnValue;
        }
        public string CodeToPath(string code, ItemType type)
        {
            string returnValue = "";
            foreach (var kind in kinds)
                foreach (var item in kind)
                    if (item.id.ToString() == code)
                        return item.path;
            switch (type)
            {
                case ItemType.Wallpaper:
                    return wallpapers[0].path;
                case ItemType.Face:
                    return faces[0].path;
                case ItemType.Frame:
                    return frames[0].path;
                case ItemType.Protector:
                    return protectors[0].path;
                case ItemType.Mat:
                    return mats[0].path;
                case ItemType.Grave:
                    return graves[0].path;
                case ItemType.Stand:
                    return stands[0].path;
                case ItemType.Mate:
                    return mates[0].path;
                case ItemType.Case:
                    return cases[0].path;
                default:
                    return mats[0].path;
            }
        }

        public enum ItemType
        {
            Wallpaper,
            Face,
            Frame,
            Protector,
            Mat,
            Grave,
            Stand,
            Mate,
            Case
        }
    }
}
