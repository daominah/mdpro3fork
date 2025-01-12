using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using MDPro3.YGOSharp;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using MDPro3.Utility;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;
using LibJpegTurboUnity;

namespace MDPro3
{
    public class TextureLoader : MonoBehaviour
    {
        public static TextureLoader Instance;

        private static readonly ConcurrentDictionary<int, TextureData> cachedArts = new();
        private static readonly ConcurrentDictionary<int, TextureData> cachedCards = new();
        private static readonly ConcurrentDictionary<int, Texture2D> cachedNames = new();

        public const int MAX_LOADPICTURE_THREADS = 2;
        private const int MAX_RENDER_THREADS = 2;
        private const int MAX_LOADZIP_THREADS = 1;

        private static SemaphoreSlim semaphoreLoadPicture;
        private static SemaphoreSlim semaphoreRender;
        private static SemaphoreSlim semaphoreLoadZip;

        private void Awake()
        {
            Instance = this;
            semaphoreLoadPicture = new SemaphoreSlim(MAX_LOADPICTURE_THREADS);
            semaphoreLoadZip = new SemaphoreSlim(MAX_LOADZIP_THREADS);
            semaphoreRender = new SemaphoreSlim(MAX_RENDER_THREADS);
        }

        public static async Task<Texture2D> LoadPicFromFileAsync(string path)
        {
            if (!File.Exists(path))
                return null;

            await semaphoreLoadPicture.WaitAsync();

            string fullPath;
#if !UNITY_EDITOR && UNITY_ANDROID
            fullPath = "file://" + Application.persistentDataPath + Program.slash + path;
#else
            fullPath = Environment.CurrentDirectory + Program.slash + path;
#endif

            using var request = UnityWebRequestTexture.GetTexture(fullPath);
            var send = request.SendWebRequest();
            await TaskUtility.WaitUntil(() => send.isDone);

            semaphoreLoadPicture.Release();

            if (request.result == UnityWebRequest.Result.Success)
                return DownloadHandlerTexture.GetContent(request);
            else
            {
                Debug.LogWarningFormat("Pic File [{0}] not fount.", path);
                return null;
            }
        }

        public static async Task<Texture2D> LoadArtAsync(int code, bool cache)
        {
            await TaskUtility.WaitWhile(() => TextureManager.container == null);

            if (cachedArts.TryGetValue(code, out var textureData))
                return await WaitTextureLoaded(textureData, cache);

            textureData = new TextureData()
            {
                texture = null,
                loaded = false,
                notDelete = cache,
                referenceCount = 1,
            };
            if (!cachedArts.TryAdd(code, textureData))
                return await WaitTextureLoaded(cachedArts[code], cache);

            if (!Directory.Exists(Program.artPath))
                Directory.CreateDirectory(Program.artPath);
            if (!Directory.Exists(Program.altArtPath))
                Directory.CreateDirectory(Program.altArtPath);

            var path = Program.altArtPath + code;

            if (File.Exists(path + Program.jpgExpansion))
                path += Program.jpgExpansion;
            else if (File.Exists(path + Program.pngExpansion))
                path += Program.pngExpansion;
            else if (File.Exists(Program.artPath + code.ToString() + Program.jpgExpansion))
                path = Program.artPath + Program.slash + code.ToString() + Program.jpgExpansion;
            else
            {
                Task<Texture2D> loadTask;
                loadTask = LoadArtFromZipArt(code);
                await TaskUtility.WaitUntil(() => loadTask.IsCompleted);
                if (loadTask.Result == null)
                {
                    loadTask = LoadArtFromZipPics(code);
                    await TaskUtility.WaitUntil(() => loadTask.IsCompleted);
                }

                textureData.texture = loadTask.Result;
                textureData.loaded = true;
                return loadTask.Result;
            }

            var task = LoadPicFromFileAsync(path);
            await TaskUtility.WaitUntil(() => task.IsCompleted);

            textureData.texture = task.Result;
            textureData.loaded = true;
            return task.Result;
        }

        public static async Task<Texture2D> LoadCardAsync(int code, bool cache = false)
        {
            await TaskUtility.WaitWhile(() => TextureManager.container == null);

            if (cachedCards.TryGetValue(code, out var textureData))
                return await WaitTextureLoaded(textureData, cache);

            textureData = new TextureData()
            {
                texture = null,
                loaded = false,
                notDelete = cache,
                referenceCount = 1,
            };
            if (!cachedCards.TryAdd(code, textureData))
                return await WaitTextureLoaded(cachedCards[code], cache);

            var data = CardsManager.Get(code, true);
            if (data.Id == 0)
                return TextureManager.container.unknownCard.texture;

            var task = LoadArtAsync(code, false);
            await TaskUtility.WaitUntil(() => task.IsCompleted);

            await semaphoreRender.WaitAsync();
            var art = task.Result;
            if(art == null)
                art = TextureManager.container.unknownArt.texture;
            if (!Program.instance.cardRenderer.RenderCard(code, art))
                return TextureManager.container.unknownCard.texture;
            var returnValue = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.RGB24, true);
            returnValue.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            returnValue.Apply();
            returnValue.name = "Card_" + code;
            await TaskUtility.WaitOneFrame();

            textureData.texture = returnValue;
            textureData.loaded = true;

            semaphoreRender.Release();
            DeleteArt(code);
            return returnValue;
        }

        public static async Task<Texture2D> LoadCardNameAsync(int code)
        {
            if (cachedNames.TryGetValue(code, out var result))
                return result;

            await semaphoreRender.WaitAsync();

            RenderTexture.active = Program.instance.cardRenderer.renderTexture;
            Program.instance.cardRenderer.RenderName(code);
            result = new Texture2D(RenderTexture.active.width, 203, TextureFormat.RGBA32, false);
            var rect = new Rect(0, Program.instance.cardRenderer.renderTexture.height - 203
                , Program.instance.cardRenderer.renderTexture.width, 203);
            result.ReadPixels(rect, 0, 0);
            result.Apply();
            result.wrapMode = TextureWrapMode.Clamp;

            if (!cachedNames.TryAdd(code, result))
                Destroy(result);
            semaphoreRender.Release();
            return cachedNames[code];
        }

        public static void DeleteArt(int code)
        {
            if (cachedArts.TryGetValue(code, out var art))
                if (art.Delete())
                {
                    cachedArts.TryRemove(code, out _);
                    DestroyImmediate(art.texture);
                }
        }

        public static void DeleteCard(int code)
        {
            if (cachedCards.TryGetValue(code, out var card))
            {
                if (card.Delete())
                {
                    cachedCards.TryRemove(code, out _);
                    DestroyImmediate(card.texture);
                }
            }
        }

        public static void ClearCache()
        {
            foreach (var art in cachedArts.Values)
                Destroy(art.texture);
            foreach (var card in cachedCards.Values)
                Destroy(card.texture);
            foreach (var name in cachedNames.Values)
                Destroy(name);
            cachedArts.Clear();
            cachedCards.Clear();
            cachedNames.Clear();
        }

        private static async Task<Texture2D> LoadArtFromZipArt(int code)
        {
            await semaphoreLoadZip.WaitAsync();

            foreach (var zip in ZipHelper.zips)
            {
                if (zip.Name.ToLower().EndsWith("script.zip"))
                    continue;
                foreach (var file in zip.EntryFileNames)
                {
                    foreach (var extName in new[] { Program.pngExpansion, Program.jpgExpansion })
                    {
                        var picPath = $"art/{code}{extName}";
                        if (file.ToLower() == picPath)
                        {
                            MemoryStream stream = new();
                            var entry = zip[picPath];
                            entry.Extract(stream);
                            Texture2D returnValue = new(0, 0);
                            returnValue.LoadImage(stream.ToArray());
                            semaphoreLoadZip.Release();
                            return returnValue;
                        }
                    }
                }
            }

            semaphoreLoadZip.Release();
            return null;
        }

        private static async Task<Texture2D> LoadArtFromZipPics(int code)
        {
            await semaphoreLoadZip.WaitAsync();

            foreach (var zip in ZipHelper.zips)
            {
                if (zip.Name.ToLower().EndsWith("script.zip"))
                    continue;
                foreach (var file in zip.EntryFileNames)
                {
                    foreach (var extName in new[] { Program.pngExpansion, Program.jpgExpansion })
                    {
                        var picPath = $"pics/{code}{extName}";
                        if (file.ToLower() == picPath)
                        {
                            var data = CardsManager.Get(code);
                            MemoryStream stream = new MemoryStream();
                            var entry = zip[picPath];
                            entry.Extract(stream);
                            await TaskUtility.WaitOneFrame();

                            Texture2D returnValue = new(0, 0);
                            returnValue.LoadImage(stream.ToArray());

                            semaphoreLoadZip.Release();

                            if (code >= 120000000 && code < 130000000)
                            {
                                if (data.HasType(CardType.Monster))
                                    return GetArtFromRushDuelMonsterCard(returnValue);
                                else
                                    return GetArtFromRushDuelSpellCard(returnValue);
                            }
                            else if (data.HasType(CardType.Pendulum))
                                return GetArtFromPendulumCard(returnValue);
                            else
                                return GetArtFromCard(returnValue);
                        }
                    }
                }
            }

            semaphoreLoadZip.Release();
            return null;
        }

        private static async Task<Texture2D> WaitTextureLoaded(TextureData data, bool cache)
        {
            await TaskUtility.WaitUntil(() => data.loaded);
            data.AddReference();
            if (cache)
                data.notDelete = true;
            return data.texture;
        }

        #region Crop Texture
        private static Texture2D GetArtFromCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.13f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.3f);
            var width = Mathf.CeilToInt(cardPic.width * 0.87f);
            var height = Mathf.CeilToInt(cardPic.height * 0.81f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        private static Texture2D GetArtFromPendulumCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.38f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.81f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        private static Texture2D GetArtFromRushDuelMonsterCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.29f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.90f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }
        private static Texture2D GetArtFromRushDuelSpellCard(Texture2D cardPic)
        {
            var startX = Mathf.CeilToInt(cardPic.width * 0.067f);
            var startY = Mathf.CeilToInt(cardPic.height * 0.29f);
            var width = Mathf.CeilToInt(cardPic.width * 0.933f);
            var height = Mathf.CeilToInt(cardPic.height * 0.90f);
            return GetCroppingTex(cardPic, startX, startY, width, height);
        }

        private static Texture2D GetCroppingTex(Texture2D texture, int startX, int startY, int width, int height)
        {
            var returnValue = new Texture2D(width - startX, height - startY);
            var pix = new Color[returnValue.width * returnValue.height];
            var index = 0;
            for (var y = startY; y < height; y++)
                for (var x = startX; x < width; x++)
                    pix[index++] = texture.GetPixel(x, y);

            returnValue.SetPixels(pix);
            returnValue.Apply();
            return returnValue;
        }

        #endregion
    }
}
