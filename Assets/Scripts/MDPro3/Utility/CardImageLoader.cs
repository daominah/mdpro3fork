using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using zFramework.Internal;
using YgomGame.Duel;
using MDPro3.Duel.YGOSharp;
using System.Xml.Linq;

namespace MDPro3.Utility
{
    public class CardImageLoader
    {

        private static readonly ConcurrentDictionary<int, CacheEntry> cachedArts = new();
        private static readonly ConcurrentDictionary<int, CacheEntry> cachedCards = new();
        private static readonly ConcurrentDictionary<int, CacheEntry> cachedCardNames = new();
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> artLoadingLocks = new();
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> cardLoadingLocks = new();
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> cardNameLoadingLocks = new();

#if UNITY_ANDROID
        private static readonly int maxArtLoads = 1;
        public static readonly int maxCardLoads = 1;
#else
        private static readonly int maxArtLoads = 2;
        public static readonly int maxCardLoads = 2;
#endif
        private static readonly SemaphoreSlim artSemaphore = new(maxArtLoads, maxArtLoads);
        private static readonly SemaphoreSlim cardSemaphore = new(maxCardLoads, maxCardLoads);

        #region Public

        public static bool lastCardFoundArt;
        public static bool lastCardRenderSucceed;

        public static async Task<Texture2D> LoadArtAsync(
            int code,
            bool persistent = false,
            CancellationToken token = default)
        {
            while (true)
            {
                if (cachedArts.TryGetValue(code, out var entry))
                {
                    if (entry.LoadingTask != null)
                    {
                        await entry.LoadingTask;
                        continue;
                    }
                    Interlocked.Increment(ref entry.ReferenceCount);
                    entry.IsPersistent |= persistent;
                    return entry.Texture;
                }

                CacheEntry newEntry = new()
                {
                    LoadingTask = InternalLoadArtAsync(code, token)
                };

                if (cachedArts.TryAdd(code, newEntry))
                {
                    try
                    {
                        newEntry.Texture = await newEntry.LoadingTask;
                        newEntry.ReferenceCount = 1;
                        newEntry.IsPersistent = persistent;
                        return newEntry.Texture;
                    }
                    catch (OperationCanceledException)
                    {
                        cachedArts.TryRemove(code, out _);
                        return null;
                    }
                    catch (AggregateException)
                    {
                        cachedArts.TryRemove(code, out _);
                        return null;
                    }
                    catch (Exception e)
                    {
                        cachedArts.TryRemove(code, out _);
                        Debug.LogError($"Load art failed: {e.GetType()} {e.Message}");
                        return null;
                    }
                    finally
                    {
                        newEntry.LoadingTask = null;
                    }
                }
            }
        }

        public static void ReleaseArt(int code)
        {
            if (!cachedArts.TryGetValue(code, out var entry)) return;

            var newCount = Interlocked.Decrement(ref entry.ReferenceCount);

            if (newCount == 0 && !entry.IsPersistent)
                if (cachedArts.TryRemove(code, out _))
                    UnityEngine.Object.Destroy(entry.Texture);
        }

        public static async Task<Texture2D> LoadCardAsync(
            int code,
            bool persistent = false,
            CancellationToken token = default)
        {
            while (true)
            {
                if (cachedCards.TryGetValue(code, out var entry))
                {
                    if (entry.LoadingTask != null)
                    {
                        await entry.LoadingTask;
                        continue;
                    }
                    Interlocked.Increment(ref entry.ReferenceCount);
                    entry.IsPersistent |= persistent;
                    return entry.Texture;
                }

                CacheEntry newEntry = new()
                {
                    LoadingTask = InternalLoadCardAsync(code, token)
                };

                if (cachedCards.TryAdd(code, newEntry))
                {
                    try
                    {
                        newEntry.Texture = await newEntry.LoadingTask;
                        newEntry.ReferenceCount = 1;
                        newEntry.IsPersistent = persistent;
                        return newEntry.Texture;
                    }
                    catch (OperationCanceledException)
                    {
                        cachedCards.TryRemove(code, out _);
                        return null;
                    }
                    catch (AggregateException)
                    {
                        cachedCards.TryRemove(code, out _);
                        return null;
                    }
                    catch (Exception e)
                    {
                        cachedCards.TryRemove(code, out _);
                        Debug.LogError($"Load card failed: {e.GetType()} {e.Message}");
                        return null;
                    }
                    finally
                    {
                        newEntry.LoadingTask = null;
                    }
                }
            }
        }

        public static void ReleaseCard(int code)
        {
            if (!cachedCards.TryGetValue(code, out var entry)) return;

            var newCount = Interlocked.Decrement(ref entry.ReferenceCount);

            if (newCount == 0 && !entry.IsPersistent)
                if (cachedCards.TryRemove(code, out _))
                    UnityEngine.Object.Destroy(entry.Texture);
        }

        public static async Task<Texture2D> LoadCardNameAsync(
            int code,
            CancellationToken token = default)
        {
            while (true)
            {
                if (cachedCardNames.TryGetValue(code, out var entry))
                {
                    if (entry.LoadingTask != null)
                    {
                        await entry.LoadingTask;
                        continue;
                    }
                    Interlocked.Increment(ref entry.ReferenceCount);
                    entry.IsPersistent = true;
                    return entry.Texture;
                }

                CacheEntry newEntry = new()
                {
                    LoadingTask = InternalLoadCardNameAsync(code, token)
                };

                if (cachedCardNames.TryAdd(code, newEntry))
                {
                    try
                    {
                        newEntry.Texture = await newEntry.LoadingTask;
                        newEntry.ReferenceCount = 1;
                        newEntry.IsPersistent = true;
                        return newEntry.Texture;
                    }
                    catch (OperationCanceledException)
                    {
                        cachedCardNames.TryRemove(code, out _);
                        return null;
                    }
                    catch (AggregateException)
                    {
                        cachedCardNames.TryRemove(code, out _);
                        return null;
                    }
                    catch (Exception e)
                    {
                        cachedCardNames.TryRemove(code, out _);
                        Debug.LogError($"Load card name failed: {e.GetType()} {e.Message}");
                        return null;
                    }
                    finally
                    {
                        newEntry.LoadingTask = null;
                    }
                }
            }
        }

        public static void ClearCache()
        {
            foreach (var card in cachedCards.Values)
                UnityEngine.Object.Destroy(card.Texture);
            cachedCards.Clear();

            foreach (var cardName in cachedCardNames.Values)
                UnityEngine.Object.Destroy(cardName.Texture);
            cachedCardNames.Clear();
        }

        #endregion

        #region Private

        private class CacheEntry
        {
            public Texture2D Texture;
            public int ReferenceCount;
            public bool IsPersistent;
            public Task<Texture2D> LoadingTask;
        }

        private static async Task<Texture2D> InternalLoadArtAsync(
            int code,
            CancellationToken token)
        {
            await artSemaphore.WaitAsync(token);
            lastCardFoundArt = true;

            try
            {
                var lockObj = artLoadingLocks.GetOrAdd(code, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(token);

                try
                {
                    if (cachedArts.TryGetValue(code, out var entry))
                        if (entry.Texture != null)
                            return entry.Texture;

                    var path = GetArtFilePath(code);
                    if (string.IsNullOrEmpty(path))
                    {
                        bool needCrop = false;
                        var zipLoad = LoadArtFromZip("art", code, token);
                        await TaskUtility.WaitUntil(() => zipLoad.IsCompleted);
                        if (zipLoad.Result == null)
                        {
                            needCrop = true;
                            zipLoad = LoadArtFromZip("pics", code, token);
                            await TaskUtility.WaitUntil(() => zipLoad.IsCompleted);
                        }
                        if (zipLoad.Result == null)
                        {
                            lastCardFoundArt = false;
                            return null;
                        }
                        else
                        {
                            if (needCrop)
                                return CropCardToArt(zipLoad.Result, code);
                            else
                                return zipLoad.Result;
                        }
                    }

                    using var request = UnityWebRequestTexture.GetTexture(path);
                    var option = request.SendWebRequest();
                    using (token.Register(() => request.Abort()))
                    {
                        while (!option.isDone)
                        {
                            await TaskUtility.WaitOneFrame();
                            token.ThrowIfCancellationRequested();
                        }
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"º”‘ÿ ß∞‹: {request.error}");
                        lastCardFoundArt = false;
                        return null;
                    }
                    return DownloadHandlerTexture.GetContent(request);
                }
                finally
                {
                    lockObj.Release();
                    artLoadingLocks.TryRemove(code, out _);
                }
            }
            finally { artSemaphore.Release(); }
        }

        private static async Task<Texture2D> InternalLoadCardAsync(
            int code,
            CancellationToken token)
        {
            lastCardRenderSucceed = true;

            await TaskUtility.WaitWhile(() => TextureManager.container == null);
            await cardSemaphore.WaitAsync(token);

            try
            {
                var lockObj = cardLoadingLocks.GetOrAdd(code, _ => new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(token);

                try
                {
                    if (cachedCards.TryGetValue(code, out var entry))
                        if (entry.Texture != null)
                            return entry.Texture;

                    var data = CardsManager.Get(code, true);
                    if (data.Id == 0)
                    {
                        lastCardRenderSucceed = false;
                        return TextureManager.container.unknownCard.texture;
                    }

                    var task = LoadArtAsync(code, false, token);
                    await TaskUtility.WaitUntil(() => task.IsCompleted);

                    var art = task.Result;
                    if (art == null)
                        art = TextureManager.container.unknownArt.texture;
                    if (!Program.instance.cardRenderer.RenderCard(code, art))
                    {
                        lastCardRenderSucceed = false;
                        return TextureManager.container.unknownCard.texture;
                    }

                    var returnValue = new Texture2D(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.RGB24, true);
                    returnValue.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
                    returnValue.Apply();
                    returnValue.name = "Card_" + code;
                    await TaskUtility.WaitOneFrame();

                    ReleaseArt(code);
                    return returnValue;
                }
                finally
                {
                    lockObj.Release();
                    cardLoadingLocks.TryRemove(code, out _);
                }
            }
            finally { cardSemaphore.Release(); }
        }

        private static async Task<Texture2D> InternalLoadCardNameAsync(
            int code,
            CancellationToken token)
        {
            await cardSemaphore.WaitAsync(token);
            try
            {
                var lockObj = cardNameLoadingLocks.GetOrAdd(code, _ = new SemaphoreSlim(1, 1));
                await lockObj.WaitAsync(token);

                try
                {
                    if (cachedCardNames.TryGetValue(code, out var entry))
                        if (entry.Texture != null)
                            return entry.Texture;

                    RenderTexture.active = Program.instance.cardRenderer.renderTexture;
                    Program.instance.cardRenderer.RenderName(code);
                    var result = new Texture2D(RenderTexture.active.width, 203, TextureFormat.RGBA32, false);
                    var rect = new Rect(0, Program.instance.cardRenderer.renderTexture.height - 203
                        , Program.instance.cardRenderer.renderTexture.width, 203);
                    result.ReadPixels(rect, 0, 0);
                    result.Apply();
                    result.wrapMode = TextureWrapMode.Clamp;
                    await TaskUtility.WaitOneFrame();
                    return result;
                }
                finally
                {
                    lockObj.Release();
                    cardNameLoadingLocks.TryRemove(code, out _);
                }
            }
            finally { cardSemaphore.Release(); }
        }

        private static string GetArtFilePath(int code)
        {
            var path = Program.altArtPath + code;
            if (File.Exists(path + Program.jpgExpansion))
                path += Program.jpgExpansion;
            else if (File.Exists(path + Program.pngExpansion))
                path += Program.pngExpansion;
            else if (File.Exists(Program.artPath + code.ToString() + Program.jpgExpansion))
                path = Program.artPath + Program.slash + code.ToString() + Program.jpgExpansion;
            else
                path = string.Empty;

            if (!string.IsNullOrEmpty(path))
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                path = Path.Combine("file://" + Application.persistentDataPath, path);
#else
                path = Path.Combine(Environment.CurrentDirectory, path);
#endif
            }

            return path;
        }

        private static async Task<Texture2D> LoadArtFromZip(
            string folder,
            int code,
            CancellationToken token)
        {
            //await Loom.ToOtherThread;
            //token.ThrowIfCancellationRequested();

            MemoryStream stream = null;
            var targetPNG = $"{folder.ToLower()}/{code}{Program.pngExpansion.ToLower()}";
            var targetJPG = $"{folder.ToLower()}/{code}{Program.jpgExpansion.ToLower()}";

            foreach (var zip in ZipHelper.zips)
            {
                if (zip.Name.ToLower().EndsWith("script.zip"))
                    continue;
                foreach (var file in zip.EntryFileNames)
                {
                    if (file.ToLower() == targetPNG || file.ToLower() == targetJPG)
                    {
                        stream = new();
                        var entry = zip[file];
                        entry.Extract(stream);
                        await TaskUtility.WaitOneFrame();
                        token.ThrowIfCancellationRequested();
                        break;
                    }
                }
            }

            //await Loom.ToMainThread;
            //token.ThrowIfCancellationRequested();

            if (stream != null)
            {
                Texture2D returnValue = new(0, 0);
                returnValue.LoadImage(stream.ToArray());
                await TaskUtility.WaitOneFrame();
                token.ThrowIfCancellationRequested();
                return returnValue;
            }
            else
                return null;
        }

        private static Texture2D CropCardToArt(Texture2D pic, int code)
        {
            var data = CardsManager.Get(code);
            if (code >= 120000000 && code < 130000000)
            {
                if (data.HasType(CardType.Monster))
                    return GetArtFromRushDuelMonsterCard(pic);
                else
                    return GetArtFromRushDuelSpellCard(pic);
            }
            else if (data.HasType(CardType.Pendulum))
                return GetArtFromPendulumCard(pic);
            else
                return GetArtFromCard(pic);
        }

        #endregion

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
            var pix = new Color32[returnValue.width * returnValue.height];
            var index = 0;
            for (var y = startY; y < height; y++)
                for (var x = startX; x < width; x++)
                    pix[index++] = texture.GetPixel(x, y);

            returnValue.SetPixels32(pix);
            returnValue.Apply();
            return returnValue;
        }

        #endregion

    }
}