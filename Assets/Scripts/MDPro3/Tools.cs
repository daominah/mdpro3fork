using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace MDPro3
{
    public class Tools
    {
        public static Transform GetChildByName(Transform parent, string childName)
        {
            foreach (var t in parent.GetComponentsInChildren<Transform>())
            { if (t.name == childName) return t; }
            return null;
        }

        public static void ChangeLayer(GameObject go, string layer, bool setAllChildrenActivate = false)
        {
            //Debug.Log("Change Layer: " + go.name + "-> " + layer);
            foreach (Transform t in go.transform.GetComponentsInChildren<Transform>(true))
            {
                if (setAllChildrenActivate) t.gameObject.SetActive(true);
                t.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
        public static void ChangeLayer(GameObject go, int layerMask, bool setAllChildrenActivate = false)
        {
            //Debug.Log("Change Layer: " + go.name + "-> " + layer);
            foreach (Transform t in go.transform.GetComponentsInChildren<Transform>(true))
            {
                if (setAllChildrenActivate) t.gameObject.SetActive(true);
                t.gameObject.layer = layerMask;
            }
        }

        public static void ChangeSortingLayer(GameObject go, string sortingLayer)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                renderer.sortingLayerName = sortingLayer;
        }

        public static void ChangeMaterialRenderQueue(GameObject root, int queue)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                renderer.material.renderQueue = queue;
        }

        public static void PlayAnimation(Transform animationContainer, string animationName)
        {
            if (animationContainer == null) return;
            Animator[] animators = animationContainer.GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                animator.SetTrigger(animationName);
            }
        }

        public static void PlayParticle(Transform particleContainer, string particleName)
        {
            if (particleContainer == null) return;
            foreach (var child in particleContainer.GetComponentsInChildren<Transform>(true))
                if (child.name.ToLower().Contains(particleName.ToLower()))
                    foreach (var p in child.GetComponentsInChildren<ParticleSystem>(true))
                        p.Play();
        }
        public static void SetAnimatorTimescale(Transform container, float timeScale)
        {
            foreach (var animator in container.GetComponentsInChildren<Animator>(true))
                animator.speed = timeScale;
        }
        public static void SetParticleSystemSimulationSpeed(Transform container, float timeScale)
        {
            foreach(var particle in container.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particle.main;
                main.simulationSpeed = timeScale;
            }
        }

        public static void SetPlayableDirectorUnscaledGameTime(Transform container)
        {
            foreach (var director in container.GetComponentsInChildren<PlayableDirector>(true))
                director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        }

        public static PlayableDirector GetPlayableDirectorInChildren(Transform container)
        {
            PlayableDirector returnValue = null;
            for (int i = 0 ; i < container.childCount; i++)
            {
                if (container.GetChild(i).GetComponent<PlayableDirector>() != null)
                    returnValue = container.GetChild(i).GetComponent<PlayableDirector>();
                else
                    UnityEngine.Object.Destroy(container.GetChild(i).gameObject);
            }
            return returnValue;
        }

        public static int CompareTime(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var xInfo = (FileInfo)x;
            var yInfo = (FileInfo)y;
            return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);
        }

        public static int CompareName(object x, object y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var xInfo = (FileInfo)x;
            var yInfo = (FileInfo)y;
            return xInfo.FullName.CompareTo(yInfo.FullName);
        }

        public static string GetTimeString()
        {
            return DateTime.Now.ToString("MM-dd¡¸HH£ºmm£ºss¡¹");
        }

        public static List<string> GetLocalIPv4()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var returnValue = new List<string >();
            foreach(var address in  host.AddressList)
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    returnValue.Add(address.ToString() ?? "127.0.0.1");
            return returnValue;
        }

        public static string[] SplitWithPreservedQuotes(string input)
        {
            List<string> result = new List<string>();
            int start = 0;
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (input[i] == ' ' && !inQuotes)
                {
                    result.Add(input.Substring(start, i - start));
                    start = i + 1;
                }
            }
            result.Add(input.Substring(start));
            for (int i = 0; i < result.Count; i++)
                result[i] = result[i].Replace("\"", "");
            return result.ToArray();
        }

        public static KeyValuePair<TKey, TValue> GetNthElement<TKey, TValue>(Dictionary<TKey, TValue> dic, int n)
        {
            if (n < 0)
                n = 0;
            if (n >= dic.Count)
                n = dic.Count - 1;
            var enumerator = dic.GetEnumerator();
            for (int i = 0; i < n + 1; i++)
                enumerator.MoveNext();
            return enumerator.Current;
        }

        public static bool StringIsAlphaNumeric(string input)
        {
            Regex regex = new Regex("^[A-Za-z0-9]+$");
            return regex.IsMatch(input);
        }
        public static bool StringIsLowerAlphaNumeric(string input)
        {
            Regex regex = new Regex("^[a-z0-9]+$");
            return regex.IsMatch(input);
        }

        public static int GetLocalDeckCount()
        {
            return Directory.GetFiles(Program.deckPath, "*.ydk").Length;
        }

        public static DateTime GetLocalDeckLastEditTime()
        {
            DateTime dateTime = DateTime.MinValue;
            foreach(var file in Directory.GetFiles(Program.deckPath, "*.ydk"))
            {
                var fileInfo = new FileInfo(file);
                if(fileInfo.LastWriteTime > dateTime)
                    dateTime = fileInfo.LastWriteTime;
            }
            return dateTime;
        }

        public static Color[] ResizePixels(Color[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight)
        {
            Color[] newPixels = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int origX = (int)((float)x / newWidth * originalWidth);
                    int origY = (int)((float)y / newHeight * originalHeight);

                    newPixels[y * newWidth + x] = originalPixels[origY * originalWidth + origX];
                }
            }

            return newPixels;
        }

        public static Sprite Texture2Sprite(Texture2D texture)
        {
            if(texture == null)
                return null;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        #region Online
        public static async Task<Texture2D> DownloadImageAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(url);
                    var imageStream = new MemoryStream(imageBytes);
                    var returenValue = new Texture2D(0, 0);
                    returenValue.LoadImage(imageStream.ToArray());
                    return returenValue;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Error downloading image: {ex.Message}");
                return null;
            }
        }
        #endregion



    }
}
