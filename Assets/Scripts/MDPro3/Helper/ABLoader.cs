using Cysharp.Threading.Tasks;
using Org.Brotli.Dec;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using Willow;
using Willow.InGameField;
using YgomGame;

namespace MDPro3
{
    public class ABLoader
    {
        public static Dictionary<string, GameObject> cachedAB = new();
        public static Dictionary<string, GameObject> cachedABFolder = new();
        public static Dictionary<string, Material> cachedPMat = new();
        private static readonly List<GameObject> tempGameObjects = new();

        private static SemaphoreSlim protectorSemaphoreSlim = new(1, 1);

        public static async UniTask<AssetBundle> CacheFromFileAsync(string path)
        {
            return await AssetBundle.LoadFromFileAsync(path);
        }

        public static GameObject LoadFromFile(string path, bool cache, bool instantiate)
        {
            if (cachedAB.TryGetValue(path, out var returnValue))
            {
                if (instantiate && returnValue != null)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return returnValue;
            }

            AssetBundle ab;
            ab = AssetBundle.LoadFromFile(Program.root + path);
            var prefabs = ab.LoadAllAssets();
            foreach (UnityEngine.Object prefab in prefabs)
            {
                if (typeof(GameObject).IsInstanceOfType(prefab))
                {
                    if (cache)
                    {
                        if (!cachedAB.TryAdd(path, prefab as GameObject))
                            Debug.LogWarning($"Failed to cache {path}");
                    }
                    else
                        tempGameObjects.Add(prefab as GameObject);
                    returnValue = prefab as GameObject;
                    break;
                }
            }

            ab.Unload(false);
            if (instantiate && returnValue != null)
                return UnityEngine.Object.Instantiate(returnValue);
            else
                return returnValue;
        }

        public static async UniTask<GameObject> LoadFromFileAsync(string path, bool cache, bool instantiate)
        {
            if (cachedAB.TryGetValue(path, out GameObject returnValue))
            {
                if (instantiate)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return returnValue;
            }

            AssetBundle ab = await AssetBundle.LoadFromFileAsync(Program.root + path);
            var prefabs = ab.LoadAllAssets();

            foreach (UnityEngine.Object prefab in prefabs)
            {
                if (typeof(GameObject).IsInstanceOfType(prefab))
                {
                    if (cache)
                    {
                        if (!cachedAB.TryAdd(path, prefab as GameObject))
                            Debug.LogWarning($"Failed to cache {path}");
                    }
                    else
                        tempGameObjects.Add(prefab as GameObject);
                    returnValue = prefab as GameObject;
                    //break;
                }
            }
            ab.Unload(false);

            if (instantiate && returnValue != null)
                return UnityEngine.Object.Instantiate(returnValue);
            else
                return returnValue;
        }

        public static GameObject LoadFromFolder<T>(string path, bool cache, bool instantiate) where T : Component
        {
            if (cachedABFolder.TryGetValue(path, out var returnValue))
            {
                if (instantiate)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return returnValue;
            }

            DirectoryInfo dir = new(Program.root + path);
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            dir = new DirectoryInfo(Path.Combine(Application.dataPath, Program.root + path));
#endif

            FileInfo[] files = dir.GetFiles("*");
            List<AssetBundle> bundles = new();
            for (int i = 0; i < files.Length; i++)
                bundles.Add(AssetBundle.LoadFromFile(files[i].FullName));
            List<GameObject> loadedPrefabs = new();
            foreach (AssetBundle bundle in bundles)
            {
                var prefabs = bundle.LoadAllAssets();
                for (int j = 0; j < prefabs.Length; j++)
                    if (typeof(GameObject).IsInstanceOfType(prefabs[j]))
                        loadedPrefabs.Add(prefabs[j] as GameObject);
            }

            foreach (var prefab in loadedPrefabs)
                if (prefab.TryGetComponent<T>(out _))
                {
                    returnValue = prefab;
                    break;
                }
            foreach (AssetBundle bundle in bundles)
                bundle.Unload(false);
            if(cache && returnValue != null)
                cachedABFolder.TryAdd(path, returnValue);
            else if(!cache)
                tempGameObjects.AddRange(loadedPrefabs);

            if (returnValue == null)
                Debug.Log($"LoadFromFolderAsync get null: {path}");

            if (instantiate)
            {
                if (returnValue != null)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return UnityEngine.Object.Instantiate(loadedPrefabs[0]);
            }
            else
            {
                if (returnValue != null)
                    return returnValue;
                else
                    return loadedPrefabs[0];
            }
        }

        public static async UniTask<GameObject> LoadFromFolderAsync<T>(string path, bool cache, bool instantiate) where T : Component
        {
            if (cachedABFolder.TryGetValue(path, out var returnValue))
            {
                if (instantiate)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return returnValue;
            }

            DirectoryInfo dir = new(Program.root + path);
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            dir = new DirectoryInfo(Path.Combine(Application.dataPath, Program.root + path));
#endif

            FileInfo[] files = dir.GetFiles("*");
            List<AssetBundle> bundles = new();
            for (int i = 0; i < files.Length; i++)
                bundles.Add(await AssetBundle.LoadFromFileAsync(files[i].FullName));

            var loadedPrefabs = new List<GameObject>();
            foreach (AssetBundle bundle in bundles)
            {
                var prefabs = bundle.LoadAllAssets();
                for (int j = 0; j < prefabs.Length; j++)
                    if (typeof(GameObject).IsInstanceOfType(prefabs[j]))
                        loadedPrefabs.Add(prefabs[j] as GameObject);
            }

            foreach (var prefab in loadedPrefabs)
            {
                if(prefab.TryGetComponent<T>(out _))
                {
                    returnValue = prefab;
                    break;
                }
            }
            foreach (AssetBundle bundle in bundles)
                bundle.Unload(false);
            if (cache && returnValue != null)
                cachedABFolder.TryAdd(path, returnValue);
            else if (!cache)
                tempGameObjects.AddRange(loadedPrefabs);

            if (returnValue == null)
                Debug.Log($"LoadFromFolderAsync get null: {path}");

            if (instantiate)
            {
                if(returnValue != null)
                    return UnityEngine.Object.Instantiate(returnValue);
                else
                    return UnityEngine.Object.Instantiate(loadedPrefabs[0]);
            }
            else
            {
                if (returnValue != null)
                    return returnValue;
                else
                    return loadedPrefabs[0];
            }
        }

        public static async UniTask<List<GameObject>> LoadsFromFolderAsync<T>(string path)
        {
            var returnValue = new List<GameObject>();

            DirectoryInfo dir = new(Program.root + path);
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            dir = new DirectoryInfo(Path.Combine(Application.dataPath, Program.root + path));
#endif

            FileInfo[] files = dir.GetFiles("*");
            List<AssetBundle> bundles = new();
            for (int i = 0; i < files.Length; i++)
                bundles.Add(await AssetBundle.LoadFromFileAsync(files[i].FullName));

            var loadedPrefabs = new List<GameObject>();
            foreach (AssetBundle bundle in bundles)
            {
                var prefabs = bundle.LoadAllAssets();
                foreach(var prefab in prefabs)
                    if (typeof(GameObject).IsInstanceOfType(prefab))
                        loadedPrefabs.Add(prefab as GameObject);
            }

            foreach (var prefab in loadedPrefabs)
                if (prefab.TryGetComponent<T>(out _))
                    returnValue.Add(prefab);

            foreach (AssetBundle bundle in bundles)
                bundle.Unload(false);

            return returnValue;
        }

        public static async UniTask<Material> LoadProtectorMaterial(string code, CancellationToken token)
        {
            await protectorSemaphoreSlim.WaitAsync(token);

            try
            {
                if (code == Items.CODE_RANDOM.ToString())
                    code = Program.items.GetRandomItem(Items.ItemType.Protector).id.ToString();

                if (cachedPMat.TryGetValue(code, out var material))
                    if (material != null)
                        return material;

                var folder = Program.root + "MasterDuel/Protector/" + code;
#if !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN)
            folder = Path.Combine(Application.dataPath, folder);
#endif
                if (!Directory.Exists(folder))
                    return null;

                var files = Directory.GetFiles(folder);

                AssetBundle matAB = null;
                List<AssetBundle> abs = new();
                foreach (var file in files)
                {
                    var ab = await AssetBundle.LoadFromFileAsync(file).WithCancellation(token);
                    abs.Add(ab);
                    if (Path.GetFileName(file) == code)
                        matAB = ab;
                }
                if (matAB == null)
                    return null;

                material = matAB.LoadAsset<Material>("PMat");
                material.renderQueue = 3000;
                foreach (var ab in abs)
                    ab.Unload(false);

                if (cachedPMat.ContainsKey(code))
                    material = cachedPMat[code];
                else
                    cachedPMat.Add(code, material);

                return material;
            }
            finally
            {
                protectorSemaphoreSlim.Release();
            }
        }

        public static async UniTask<Material> LoadFrameMaterial(string code)
        {
            if (code == Items.CODE_RANDOM.ToString())
                code = Items.lastRandomFrameID;

            var ab = await AssetBundle.LoadFromFileAsync(Program.root + "MasterDuel/Frame/ProfileFrameMat" + code);
            var material = ab.LoadAsset<Material>("ProfileFrameMat" + code);
            ab.Unload(false);
            TextureManager.ChangeProfileFrameMaterialWrapMode(material);
            return material;
        }

        public static async UniTask<Material> LoadMaterialAsync(string path, CancellationToken token)
        {
            var ab = await AssetBundle.LoadFromFileAsync(Program.root + path).WithCancellation(token);
            var matetial = ab.LoadAsset<Material>(Path.GetFileName(path));
            ab.Unload(false);
            return matetial;
        }

        public static async UniTask<Shader> LoadShaderAsync(string path, CancellationToken token)
        {
            var ab = await AssetBundle.LoadFromFileAsync(Path.Combine(Program.root, path)).WithCancellation(token);
            var shader = ab.LoadAsset<Shader>(Path.GetFileNameWithoutExtension(path));
            ab.Unload(false);
            return shader;
        }

        public static async UniTask<Mate> LoadMateAsync(int code)
        {
            Items.Item item = new();
            foreach (var mate in Program.items.mates)
            {
                if (mate.id == code)
                {
                    item = mate;
                    break;
                }
            }
            Mate.MateType type = Mate.MateType.MasterDuel;
            if (item.id == 0 && File.Exists(Program.root + "CrossDuel/" + code + ".bundle"))
                type = Mate.MateType.CrossDuel;
            Mate returnValue = null;
            if (type == Mate.MateType.CrossDuel)
            {
                var ab = await AssetBundle.LoadFromFileAsync(Program.root + "CrossDuel/" + code + ".bundle");
                var all = ab.LoadAllAssets();
                ab.Unload(false);
                foreach (var asset in all)
                {
                    if (asset is NamedAssetContainer container)
                    {
                        container.TryGet<GameObject>("prefab", out var prefab);
                        container.TryGet<NamedAssetContainer>("Timelines", out var timelines);
                        container.TryGet<ParameterContainer>("Settings", out var settings);
                        var mateGo = UnityEngine.Object.Instantiate(prefab);
                        mateGo.AddComponent<FieldParamEventController_AnimationEventReceiver>();
                        foreach (var s in timelines.AllNamedAssetNames())
                        {
                            timelines.TryGet<GameObject>(s, out var timeline);
                            var newT = UnityEngine.Object.Instantiate(timeline);
                            newT.transform.SetParent(mateGo.transform, false);
                            newT.SetActive(true);
                            for (int i = 0; i < newT.transform.childCount; i++)
                            {
                                if (newT.transform.GetChild(i).GetComponent<Volume>() != null)
                                    UnityEngine.Object.Destroy(newT.transform.GetChild(i).gameObject);
                                if (newT.transform.GetChild(i).name == "UIBattleDownAni")
                                    UnityEngine.Object.Destroy(newT.transform.GetChild(i).gameObject);
                            }
                            var controller = newT.GetComponent<CustomTimelineController>();
                            var bindTrackInfo = controller.checkReplacer.m_bindTrackInfo;
                            var director = newT.transform.GetChild(0).GetComponent<PlayableDirector>();

                            if (director == null)
                                continue;
                            Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();
                            foreach (PlayableBinding pb in director.playableAsset.outputs)
                                foreach (var bind in bindTrackInfo)
                                    if (pb.streamName == bind.m_name
                                        && director.GetGenericBinding(pb.sourceObject) == null)
                                        director.SetGenericBinding(pb.sourceObject, mateGo.GetComponent<Animator>());
                        }
                        returnValue = mateGo.AddComponent<Mate>();
                    }
                }
            }
            else
            {
                var matePath = Program.items.GetAssetPath(code.ToString(), Items.ItemType.Mate);

                GameObject mateGo;
                if (matePath.EndsWith("_Folder"))
                    mateGo = await LoadFromFolderAsync<CharacterCollision>("MasterDuel/" + matePath.Replace("_Folder", string.Empty), false, true);
                else
                    mateGo = await LoadFromFileAsync("MasterDuel/" + matePath, false, true);
                returnValue = mateGo.AddComponent<Mate>();
            }
            returnValue.type = type;
            returnValue.code = code;
            return returnValue;
        }

        public static void ClearTemp()
        {
            foreach (var go in tempGameObjects)
                UnityEngine.Object.Destroy(go);
            tempGameObjects.Clear();
        }


        #region MasterDuel

        public static bool mdCached;

        private static AssetBundle mdBundleDuel;
        private static AssetBundle mdBundleMaterials;
        private static AssetBundle mdBundleSprites;
        private static AssetBundle mdBundleTextures;

        public static async UniTask CacheMasterDuelBundles()
        {
            await CacheFromFileAsync(Program.root + "MasterDuel/Built-in/shaders");
            mdBundleMaterials = await CacheFromFileAsync(Program.root + "MasterDuel/Built-in/materials");
            mdBundleSprites = await CacheFromFileAsync(Program.root + "MasterDuel/Built-in/sprites");
            mdBundleTextures = await CacheFromFileAsync(Program.root + "MasterDuel/Built-in/textures");
            mdBundleDuel = await CacheFromFileAsync(Program.root + "MasterDuel/Built-in/duel");
            mdCached = true;
        }

        public static GameObject LoadMasterDuelGameObject(string oName)
        {
            if(mdBundleDuel == null)
            {
                Debug.LogError("MasterDuel AssetBundles not cached!");
                return null;
            }

            var prefab = mdBundleDuel.LoadAsset<GameObject>(oName);
            if(prefab == null)
            {
                Debug.LogError($"MasterDuel AssetBundle does not contain [{oName}]!");
                return null;
            }
            return Object.Instantiate(prefab);
        }

        public static Material LoadMasterDuelMaterial(string mName)
        {
            if (mdBundleMaterials == null)
            {
                Debug.LogError("MasterDuel AssetBundles not cached!");
                return null;
            }
            var mat = mdBundleMaterials.LoadAsset<Material>(mName);
            if (mat == null)
            {
                Debug.LogError($"MasterDuel AssetBundle does not contain material [{mName}]!");
                return null;
            }
            return Object.Instantiate(mat);
        }

        public static Sprite LoadMasterDuelSprite(string sName)
        {
            if (mdBundleSprites == null)
            {
                Debug.LogError("MasterDuel AssetBundles not cached!");
                return null;
            }
            var sprite = mdBundleSprites.LoadAsset<Sprite>(sName);
            if (sprite == null)
            {
                Debug.LogError($"MasterDuel AssetBundle does not contain sprite [{sName}]!");
                return null;
            }
            return sprite;
        }

        public static Texture2D LoadMasterDuelTexture(string tName)
        {
            if (mdBundleTextures == null)
            {
                Debug.LogError("MasterDuel AssetBundles not cached!");
                return null;
            }
            var tex = mdBundleTextures.LoadAsset<Texture2D>(tName);
            if (tex == null)
            {
                Debug.LogError($"MasterDuel AssetBundle does not contain texture [{tName}]!");
                return null;
            }
            return tex;
        }

        #endregion
    }
}