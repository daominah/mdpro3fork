using MDPro3.Servant;
using MDPro3.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MDPro3
{
    [CreateAssetMenu(fileName = "Items", menuName = "Scriptable Objects/Items")]
    public class Items : ScriptableObject
    {
        [Serializable]
        public struct Item
        {
            public int id;
            public string m_name;
            bool nameLoaded;
            bool descriptionLoaded;
            public string name
            {
                get
                {
                    if (!nameLoaded)
                    {
                        var listName = instance.GetName(id, m_name);
                        if(listName != STRING_NULL || string.IsNullOrEmpty(m_name))
                            m_name = listName;
                        nameLoaded = true;
                    }
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
                    if (!diy && !descriptionLoaded)
                    {
                        var listDescription = instance.GetDescription(id);
                        if(listDescription != STRING_NULL)
                            m_description = listDescription;
                        if(string.IsNullOrEmpty(m_description))
                            m_description = STRING_NULL;
                        descriptionLoaded = true;
                    }
                    if(diy && !descriptionLoaded)
                    {
                        if(m_description.Contains("@"))
                            m_description = InterString.Get("”…°∏[?]°πÕ∂∏Â°£", m_description);
                    }
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

        public enum ItemType
        {
            Unknown,
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

        private static string language = string.Empty;
        private static Items instance;
        private readonly Dictionary<string, int> idsMap = new();
        private Dictionary<int, string> names = new();
        private Dictionary<int, string> descriptions = new();
        private readonly Dictionary<string, Sprite> cachedIcons = new();

        private const string PATH_MAP = "Data/items.txt";
        public const string STRING_NULL = "coming soon";
        public const int CODE_RANDOM = 9999;
        public const int CODE_SAME = 8888;
        public const int CODE_NONE = 0;
        public const int CODE_DIY = 9998;
        public const string PATH_ICON_RANDOM = "Menu-Random";
        public const string PATH_ICON_SAME = "Menu-Same";
        public const string PATH_ICON_NONE = "Menu-NoImage";
        public const string PATH_ICON_DIY = "Menu-DIY";

        public static bool initialized = false;

        private const string ADDRESS_DEFAULT_DECK_CASE = "DeckCase0001_L";

        public void Initialize()
        {
            if (!initialized)
            {
                instance = this;
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
                var all = File.ReadAllText(PATH_MAP);
                var lines = all.Replace("\r", string.Empty).Split('\n');
                foreach(var line in lines)
                {
                    var pair = line.Split(' ');
                    if (pair.Length > 1)
                    {
                        try
                        {
                            idsMap.Add(pair[1], int.Parse(pair[0]));
                        }
                        catch(Exception e)
                        {
                            Debug.LogError("Read items.txt Error: " + e);
                        }
                    }
                }

                initialized = true;
            }
            var currentLanguage = Language.GetConfig();
            if (language != currentLanguage)
            {
                language = currentLanguage;
                Load();
            }
        }

        private void Load()
        {
            LoadData(Program.localesPath + language + "/IDS_ITEM.bytes");
            LoadData(Program.localesPath + language + "/IDS_ITEMDESC.bytes");
        }

        private void LoadData(string path)
        {
            int type = 0;
            if (path.EndsWith("IDS_ITEMDESC.bytes"))
                type = 1;

            var bytes = File.ReadAllBytes(path);
            var languageBytes = Encoding.UTF8
                .GetBytes(Language.GetMasterDuelLanguage(language));
            int start = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                bool pass = true;
                for (int j = 0; j < languageBytes.Length; j++)
                {
                    if (bytes[i + j] != languageBytes[j])
                    {
                        pass = false;
                        break;
                    }
                }
                if (pass)
                {
                    start = i + 5;
                    break;
                }
            }

            bool isID = true;
            var ids = new List<string>();
            var values = new List<string>();
            for (int i = start; i < bytes.Length;)
            {
                int length = 0;
                if (bytes[i] == 0xDA)
                {
                    length = (bytes[i + 1] << 8) | bytes[i + 2];
                }
                else if (bytes[i] == 0xD9)
                {
                    length = bytes[i + 1];
                }
                else if (bytes[i] > 0xA0 && bytes[i] < 0xB0)
                {
                    length = bytes[i] - 0xA0;
                }
                else if (bytes[i] >= 0xB0 && bytes[i] < 0xC0)
                {
                    length = bytes[i] - 0xB0 + 16;
                }
                else
                {
                    Debug.LogErrorFormat("Items Load: Unknown Lentgh {0:X} at {1}", bytes[i], i);
                }

                var offset = 1;
                if (length > 31)
                    offset = 2;
                if (length > 255)
                    offset = 3;

                var newBytes = new byte[length];
                Array.Copy(bytes, i + offset, newBytes, 0, length);
                var content = Encoding.UTF8.GetString(newBytes);
                if (isID)
                    ids.Add(content);
                else
                    values.Add(content);
                isID = !isID;
                i += length + offset;
            }

            var dic = new Dictionary<int, string>();
            for (int i = 0; i < ids.Count && i < values.Count; i++)
                if (idsMap.TryGetValue(ids[i], out var id))
                    dic.Add(id, values[i]);

            if (type == 0)
                names = dic;
            else if(type == 1)
                descriptions = dic;
        }

        private string GetName(int code, string mName)
        {
            names.TryGetValue(code, out var returnValue);
            if (string.IsNullOrEmpty(returnValue))
            {
                if(string.IsNullOrEmpty(mName))
                    returnValue = STRING_NULL;
                else 
                    returnValue = mName;
            }
            return Cid2Ydk.ReplaceWithCardName(returnValue);
        }

        private string GetDescription(int code)
        {
            descriptions.TryGetValue(code, out var returnValue);
            if (string.IsNullOrEmpty(returnValue))
                return STRING_NULL;
            return Cid2Ydk.ReplaceWithCardName(returnValue);
        }

        public string WallpaperCodeToPath(string code)
        {
            string returnValue = "Wallpaper/Front0001";
            if(code == CODE_RANDOM.ToString())
                return GetRandomItem(ItemType.Wallpaper).path;

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

        public Item GetRandomItem(ItemType type)
        {
            switch (type)
            {
                case ItemType.Wallpaper:
                    return wallpapers[UnityEngine.Random.Range(0, wallpapers.Count)];
                case ItemType.Face:
                    return faces[UnityEngine.Random.Range(0, faces.Count)];
                case ItemType.Frame:
                    return frames[UnityEngine.Random.Range(0, frames.Count)];
                case ItemType.Protector:
                    return protectors[UnityEngine.Random.Range(0, protectors.Count)];
                case ItemType.Mat:
                    return mats[UnityEngine.Random.Range(0, mats.Count)];
                case ItemType.Grave:
                    return graves[UnityEngine.Random.Range(0, graves.Count)];
                case ItemType.Stand:
                    return stands[UnityEngine.Random.Range(0, stands.Count)];
                case ItemType.Mate:
                    return mates[UnityEngine.Random.Range(0, mates.Count)];
                case ItemType.Case:
                    return cases[UnityEngine.Random.Range(0, cases.Count)];
                default:
                    return mats[UnityEngine.Random.Range(0, mats.Count)];
            }
        }

        public string GetSameCode(ItemType type, string mapCode)
        {
            if (mapCode.Length != 7)
                mapCode = "1090001";
            if (mapCode == "1098001" && type != ItemType.Mat)
                mapCode = "1090009";
            if (mapCode == "1098002" && type != ItemType.Mat)
                mapCode = "1090003";

            if (type == ItemType.Grave)
                return "110" + mapCode[3..];
            else if (type == ItemType.Stand)
                return "111" + mapCode[3..];
            else if (type == ItemType.Mat)
            {
                lastMat1 = lastMat0;
                return lastMat0;
            }
            else
                return mapCode;
        }

        private string lastMat0;
        private string lastMat1;
        public string GetPathByCode(string code, ItemType type, int player = 0)
        {
            if(code == CODE_RANDOM.ToString())
            {
                var item = GetRandomItem(type);
                if (type == ItemType.Mat)
                {
                    if(player == 0)
                        lastMat0 = item.id.ToString();
                    else
                        lastMat1 = item.id.ToString();
                }
                return item.path;
            }
            if(type == ItemType.Mat)
            {
                if (player == 0)
                    lastMat0 = code;
                else
                    lastMat1 = code;
            }
            if (code == CODE_SAME.ToString())
                code = GetSameCode(type, player == 0 ? lastMat0 : lastMat1);

            if (type == ItemType.Unknown)
                return CodeToIconPath(code);
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

        public static string CodeToIconPath(string id)
        {
            if (id == CODE_RANDOM.ToString())
                return PATH_ICON_RANDOM;
            if(id == CODE_SAME.ToString())
                return PATH_ICON_SAME;
            if (id == CODE_NONE.ToString())
                return PATH_ICON_NONE;
            if (id == CODE_DIY.ToString())
                return PATH_ICON_DIY;

            var currentContent = id[..3];
            string pathPrefix = string.Empty;
            string pathSuffix = string.Empty;
            switch (currentContent)
            {
                case "113":
                    pathPrefix = "WallPaperIcon";
                    pathSuffix = string.Empty;
                    break;
                case "101":
                    pathPrefix = "ProfileIcon";
                    pathSuffix = "_L";
                    break;
                case "103":
                    pathPrefix = "ProfileFrame";
                    pathSuffix = "_L";
                    break;
                case "107":
                    pathPrefix = "ProtectorIcon";
                    pathSuffix = string.Empty;
                    break;
                case "109":
                    pathPrefix = "FieldIcon";
                    pathSuffix = string.Empty;
                    break;
                case "110":
                    pathPrefix = "FieldObjIcon";
                    pathSuffix = string.Empty;
                    break;
                case "111":
                    pathPrefix = "FieldAvatarBaseIcon";
                    pathSuffix = string.Empty;
                    break;
                case "100":
                    pathPrefix = string.Empty;
                    pathSuffix = string.Empty;
                    break;
                case "108":
                    pathPrefix = "DeckCase";
                    pathSuffix = "_L";
                    break;
                default:
                    pathPrefix = string.Empty;
                    pathSuffix = string.Empty;
                    break;
            }

            if(currentContent == "108")
                return pathPrefix + id.Substring(3) + pathSuffix;
            else
                return pathPrefix + id + pathSuffix;
        }

        public IEnumerator<Sprite> LoadItemIconAsync(string id, ItemType type)
        {
            lock (cachedIcons)
            {
                if (cachedIcons.ContainsKey(id))
                {
                    yield return cachedIcons[id];
                    yield break;
                }
            }

            var handle = Addressables.LoadAssetAsync<Sprite>(CodeToIconPath(id));
            while (!handle.IsDone)
                yield return null;

            if (handle.Result == null)
                yield break;

            Sprite returnValue;
            lock (cachedIcons)
            {
                if(cachedIcons.ContainsKey(id))
                {
                    returnValue = cachedIcons[id];
                    Addressables.Release(handle);
                }
                else
                {
                    returnValue = handle.Result;
                    cachedIcons.Add(id, returnValue);
                }
            }

            yield return returnValue;
        }

        public static string lastRandomFrameID;
        public IEnumerator<Sprite> LoadConcreteItemIconAsync(string id, ItemType type, int player = 0)
        {
            if(id == CODE_RANDOM.ToString())
            {
                id = GetRandomItem(type).id.ToString();
                if(type == ItemType.Frame)
                    lastRandomFrameID = id;
            }

            if(id == CODE_DIY.ToString())
            {
                var path = Program.diyPath;
                switch (player)
                {
                    case 0:
                        path += Appearance.meString;
                        break;
                    case 1:
                        path += Appearance.opString;
                        break;
                    case 2:
                        path += Appearance.meTagString;
                        break;
                    case 3:
                        path += Appearance.opTagString;
                        break;
                }
                if(File.Exists(path + Program.pngExpansion))
                {
                    var task = TextureManager.LoadPicFromFileAsync(path + Program.pngExpansion);
                    while(!task.IsCompleted)
                        yield return null;
                    yield return TextureManager.Texture2Sprite(task.Result);
                    yield break;
                }
                else if (File.Exists(path + Program.jpgExpansion))
                {
                    var task = TextureManager.LoadPicFromFileAsync(path + Program.jpgExpansion);
                    while (!task.IsCompleted)
                        yield return null;
                    yield return TextureManager.Texture2Sprite(task.Result);
                    yield break;
                }
                else
                    id = faces[0].id.ToString();
            }

            var ie = LoadItemIconAsync(id, type);
            while(ie.MoveNext())
                yield return null;
            yield return ie.Current;
        }

        public bool ListHaveRandom(List<Item> target)
        {
            if(target == wallpapers) 
                return true;
            else if (target == faces)
                return true;
            else if (target == frames)
                return true;
            else if (target == protectors)
                return true;
            else if (target == mats)
                return true;
            else if (target == graves)
                return true;
            else if (target == stands)
                return true;
            else if (target == mates)
                return true;
            else if (target == cases)
                return true;
            else
                return false;
        }

        public bool ListHaveSame(List<Item> target)
        {
            if (target == graves)
                return true;
            else if (target == stands)
                return true;
            else
                return false;
        }

        public bool ListHaveNone(List<Item> target)
        {
            if (target == wallpapers)
                return true;
            else if (target == faces)
                return false;
            else if (target == frames)
                return false;
            else if (target == protectors)
                return false;
            else if (target == mats)
                return false;
            else if (target == graves)
                return false;
            else if (target == stands)
                return true;
            else if (target == mates)
                return true;
            else if (target == cases)
                return false;
            else
                return false;
        }

        public bool ListHaveDIY(List<Item> target)
        {
            if (target == faces)
                return true;
            else
                return false;
        }

        public async Task<Sprite> LoadDeckCaseIconAsync(int code, string suffix)
        {
            try
            {
                var load = LoadAddressables($"DeckCase{code.ToString()[3..]}{suffix}");
                while (load.MoveNext())
                    await TaskUtility.WaitOneFrame();
                return load.Current;
            }
            catch
            {
                Debug.LogError("Addressables Not Found: " + $"DeckCase{code.ToString()[3..]}{suffix}");
                var load = LoadAddressables(ADDRESS_DEFAULT_DECK_CASE);
                while (load.MoveNext())
                    await TaskUtility.WaitOneFrame();
                return load.Current;
            }
        }

        private IEnumerator<Sprite> LoadAddressables(string address)
        {
            var load = Addressables.LoadAssetAsync<Sprite>(address);
            while (!load.IsDone)
                yield return null;
            yield return load.Result;
        }
    }
}
