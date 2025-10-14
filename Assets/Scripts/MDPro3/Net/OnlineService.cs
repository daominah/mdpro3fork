using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Percy;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MDPro3.Net
{
    public static class OnlineService
    {

        public static void Initialize()
        {
            _ = InitializeGenesysLflist();
            _ = InitializeMyCardAppsAsync();
        }

        #region Genesys lflist

        private const string URL_GENESYS_LFLIST = "https://cdntx.moecube.com/ygopro-genesys/lflist.conf";
        private const string PATH_GENESYS_LFLIST = "Data/lflist_genesys.conf";
        private static readonly List<int> genesysBannedCards = new();
        private static readonly List<GenesysPoint> genesysPoints = new();
        private static int officialGenesysLimit = 100;

        private static async UniTask InitializeGenesysLflist()
        {
            var eTag = await GetETagAsync(URL_GENESYS_LFLIST);
            if (!string.IsNullOrEmpty(eTag))
            {
                var configTag = Config.Get(GetLocalETagKey(URL_GENESYS_LFLIST), Config.EMPTY_STRING);
                if(!string.Equals(eTag, configTag, StringComparison.Ordinal))
                {
                    Program.Debug("Update Genesys Lflist.");
                    await DownloadGenesysLflist(eTag);
                }
                else
                    Program.Debug("Genesys Lflist do not need update.");
            }

            ParseGenesysLflist();
        }

        private static bool GenesysRequiresDownload()
        {
            if (!File.Exists(PATH_GENESYS_LFLIST))
                return true;

            var lastWriteTime = File.GetLastWriteTimeUtc(PATH_GENESYS_LFLIST);
            var now = DateTime.UtcNow;
            var updateTime = new DateTime(now.Year, now.Month, now.Day, 20, 0, 0, DateTimeKind.Utc);

            return now > updateTime && lastWriteTime < updateTime;
        }

        private static async UniTask DownloadGenesysLflist(string ETag)
        {
            using var request = UnityWebRequest.Get(URL_GENESYS_LFLIST);
            request.timeout = 15;

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(PATH_GENESYS_LFLIST, request.downloadHandler.text);
                Config.Set(GetLocalETagKey(URL_GENESYS_LFLIST), ETag);
                Config.Save();
            }
            else
                MessageManager.Cast(InterString.Get("下载Genesys禁卡表失败。"));
        }

        private static void ParseGenesysLflist()
        {
            if (!File.Exists(PATH_GENESYS_LFLIST))
                return;

            try
            {
                var lines = File.ReadAllLines(PATH_GENESYS_LFLIST);
                var currentType = string.Empty;

                foreach(var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if(string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$"))
                    {
                        currentType = line;
                        continue;
                    }

                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if(parts.Length == 2 && parts[1] == "0")
                    {
                        if (int.TryParse(parts[0], out int code))
                            genesysBannedCards.Add(code);
                    }
                    else if(parts.Length >= 3)
                    {
                        var commentIndex = Array.FindIndex(parts, p => p.StartsWith("--"));
                        var dataLength = commentIndex > 0 ? commentIndex : parts.Length;

                        if(dataLength >= 3 && int.TryParse(parts[0], out var code))
                        {
                            var gp = new GenesysPoint
                            {
                                code = code,
                                banType = currentType,
                            };
                            if(int.TryParse(parts[2], out var result))
                                gp.point = result;
                            else
                                gp.point = 0;
                            genesysPoints.Add(gp);
                        }
                    }
                }

            }
            catch (Exception e) 
            {
                Program.Debug(e.Message);
            }
        }

        public static int GetGenesysPoint(int code)
        {
            if (genesysBannedCards.Contains(code))
                return -1;
            foreach(var gp in genesysPoints)
                if(gp.code == code)
                    return gp.point;
            return 0;
        }

        public static string GetGenesysPointString(int code)
        {
            var gp = GetGenesysPoint(code);
            if (gp < 0)
                return "X";
            return gp.ToString();
        }


        /// <summary>
        /// color for Genesys Points one card score
        /// </summary>
        public static Color GetGenesysPointColor(int gp)
        {
            if (gp < 0)
                return Color.red;
            if (gp == 0)
                return Color.gray;
            if (gp <= officialGenesysLimit / 10)
                return Color.green;
            if (gp <= officialGenesysLimit / 2)
                return Color.yellow;
            if (gp <= officialGenesysLimit)
                return Color.magenta;
            return Color.red;
        }

        /// <summary>
        /// color for Genesys Points total score
        /// </summary>
        public static Color GetGenesysPointsColor(int gp)
        {
            if (gp <= officialGenesysLimit)
                return Color.white;
            return Color.red;
        }

        #endregion

        #region MyCard Apps

        private const string URL_MYCARD_APPS = "https://cdntx.moecube.com/apps.json";
        private const string PATH_MYCARD_APPS = "Data/mycard_apps.json";
        public static MyCardNews myCardNews;

        private static async UniTask InitializeMyCardAppsAsync()
        {
            var eTag = await GetETagAsync(URL_MYCARD_APPS);
            if (!string.IsNullOrEmpty(eTag))
            {
                var configTag = Config.Get(GetLocalETagKey(URL_MYCARD_APPS), Config.EMPTY_STRING);
                if (!string.Equals(eTag, configTag, StringComparison.Ordinal))
                {
                    Program.Debug("Update MyCard Apps.");
                    await DownloadMyCardApps(eTag);
                }
                else
                    Program.Debug("MyCard Apps do not need update.");
            }

            ParseMyCardNews();
        }

        private static async UniTask DownloadMyCardApps(string ETag)
        {
            using var request = UnityWebRequest.Get(URL_MYCARD_APPS);
            request.timeout = 15;

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(PATH_MYCARD_APPS, request.downloadHandler.text);
                Config.Set(GetLocalETagKey(URL_MYCARD_APPS), ETag);
                Config.Save();
            }
            else
                Program.Debug("下载MyCard apps.json失败。");
        }

        private static void ParseMyCardNews()
        {
            if (!File.Exists(PATH_MYCARD_APPS))
                return;

            var json = File.ReadAllText(PATH_MYCARD_APPS);
            json = json.Replace("\"news\":[]", "\"news\":{}");
            var apps = JsonConvert.DeserializeObject<MyCardApp[]>(json);
            foreach (var app in apps)
                if (app.id == "ygopro")
                {
                    myCardNews = app.news;
                    return;
                }
        }

        #endregion


        #region Online Tools

        private static string GetLocalETagKey(string url) => $"ETag_{url.GetHashCode()}";

        public static async UniTask<string> GetETagAsync(string url)
        {
            using var headRequest = UnityWebRequest.Head(url);
            await headRequest.SendWebRequest();

            if(headRequest.result != UnityWebRequest.Result.Success)
            {
                Program.Debug($"HEAD({url})请求失败：{headRequest.error}");
                return null;
            }

            var onlineETag = headRequest.GetResponseHeader("ETag")?.Trim();
            if (string.IsNullOrEmpty(onlineETag))
            {
                Program.Debug($"未找到ETag({url})，服务器可能未启用缓存");
                return null;
            }
            return onlineETag;
        }

        #endregion

    }

    public struct GenesysPoint
    {
        public int code;
        public string banType;
        public int point;
    }
}