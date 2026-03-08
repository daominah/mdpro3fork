using Ionic.Zip;
using MDPro3.Servant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDPro3.Duel.YGOSharp
{
    public static class BanlistManager
    {
        public static List<Banlist> Banlists { get; private set; }
        public static string EmptyBanlistName = "N/A";
        private static bool localServerLflistInjected;
        private static string localServerLflistBackupContent;
        private const string LfListFileName = "lflist.conf";

        public static void Initialize()
        {
            Banlists = new List<Banlist>();
            StreamReader reader = null;
            if (Config.GetBool("Expansions", true))
            {
                var confPath = Program.PATH_EXPANSIONS + "lflist.conf";
                if(File.Exists(confPath))
                {
                    reader = new StreamReader(confPath);
                    InitializeFromReader(reader);
                    reader.Close();
                }
                foreach (var zip in ZipHelper.zips)
                {
                    if (zip.Name.ToLower().EndsWith("script.zip"))
                        continue;
                    foreach (var file in zip.EntryFileNames)
                    {
                        if (file.ToLower().EndsWith("lflist.conf"))
                        {
                            var e = zip[file];
                            if (!Directory.Exists(Program.PATH_TEMP_FOLDER))
                                Directory.CreateDirectory(Program.PATH_TEMP_FOLDER);
                            var tempFile = Path.Combine(Path.GetFullPath(Program.PATH_TEMP_FOLDER), file);
                            e.Extract(Path.GetFullPath(Program.PATH_TEMP_FOLDER), ExtractExistingFileAction.OverwriteSilently);
                            reader = new StreamReader(tempFile);
                            InitializeFromReader(reader);
                            reader.Close();
                            File.Delete(tempFile);
                        }
                    }
                }
            }

            Banlist current = null;
            reader = new StreamReader(Program.PATH_LFLIST);
            InitializeFromReader(reader);
            reader.Close();
            current = new();
            current.Name = EmptyBanlistName;
            Banlists.Add(current);
        }

        public static void PrepareLocalServerLflist()
        {
            if (localServerLflistInjected)
                return;
            if (!Config.GetBool("Expansions", true))
                return;

            try
            {
                if (!File.Exists(Program.PATH_LFLIST))
                    return;

                localServerLflistBackupContent = File.ReadAllText(Program.PATH_LFLIST);
                var mergedContent = BuildMergedLflistContent();

                if (string.IsNullOrEmpty(mergedContent) || mergedContent == localServerLflistBackupContent)
                {
                    localServerLflistBackupContent = null;
                    return;
                }

                File.WriteAllText(Program.PATH_LFLIST, mergedContent);
                localServerLflistInjected = true;
            }
            catch (Exception e)
            {
                localServerLflistBackupContent = null;
                localServerLflistInjected = false;
                UnityEngine.Debug.LogError("Failed to prepare LAN lflist.conf.");
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void RestoreLocalServerLflist()
        {
            if (!localServerLflistInjected)
                return;

            try
            {
                if (localServerLflistBackupContent != null)
                    File.WriteAllText(Program.PATH_LFLIST, localServerLflistBackupContent);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed to restore original lflist.conf.");
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                localServerLflistBackupContent = null;
                localServerLflistInjected = false;
            }
        }

        private static string BuildMergedLflistContent()
        {
            StringBuilder builder = new StringBuilder();
            var confPath = Program.PATH_EXPANSIONS + LfListFileName;
            AppendLflistFromFile(builder, confPath);

            foreach (var zip in ZipHelper.zips)
            {
                if (zip.Name.ToLower().EndsWith("script.zip"))
                    continue;
                foreach (var file in zip.EntryFileNames)
                {
                    if (!file.ToLower().EndsWith(LfListFileName))
                        continue;

                    var entry = zip[file];
                    using (var memoryStream = new MemoryStream())
                    {
                        entry.Extract(memoryStream);
                        memoryStream.Position = 0;
                        using (var reader = new StreamReader(memoryStream))
                            AppendLflistText(builder, reader.ReadToEnd());
                    }
                }
            }

            AppendLflistFromFile(builder, Program.PATH_LFLIST);
            return builder.ToString();
        }

        private static void AppendLflistFromFile(StringBuilder builder, string path)
        {
            if (!File.Exists(path))
                return;
            AppendLflistText(builder, File.ReadAllText(path));
        }

        private static void AppendLflistText(StringBuilder builder, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (builder.Length > 0 && builder[builder.Length - 1] != '\n')
                builder.AppendLine();
            builder.Append(text);
            if (builder.Length > 0 && builder[builder.Length - 1] != '\n')
                builder.AppendLine();
        }

        public static void InitializeFromReader(StreamReader reader)
        {
            Banlist current = null;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                try
                {
                    if (line == null)
                        continue;
                    if (line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("!"))
                    {
                        current = new Banlist();
                        current.Name = line.Substring(1, line.Length - 1);
                        Banlists.Add(current);
                        continue;
                    }
                    if (line.StartsWith("$"))
                    {
                        if (current != null && line.Equals("$whitelist", StringComparison.OrdinalIgnoreCase))
                            current.EnableWhitelistMode();
                        continue;
                    }
                    if (!line.Contains(" "))
                        continue;
                    if (current == null)
                        continue;
                    string[] data = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    int id = int.Parse(data[0]);
                    int count = int.Parse(data[1]);
                    current.Add(id, count);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.Log(line);
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        public static int GetIndex(uint hash)
        {
            for (int i = 0; i < Banlists.Count; i++)
                if (Banlists[i].Hash == hash)
                    return i;
            return 0;
        }
        public static int GetIndexByName(string name)
        {
            for (int i = 0; i < Banlists.Count; i++)
                if (Banlists[i].Name == name)
                    return i;
            return 0;
        }

        public static string GetName(uint hash)    
        {
            for (int i = 0; i < Banlists.Count; i++)
                if (Banlists[i].Hash == hash)
                    return Banlists[i].Name;
            return InterString.Get("未知卡表");
        }

        public static List<string> GetAllName()
        {
            List<string> returnValue = new List<string>();
            foreach (var item in Banlists)
            {
                returnValue.Add(item.Name);
            }
            return returnValue;
        }

        public static Banlist GetByName(string name)
        {
            Banlist returnValue = Banlists[Banlists.Count - 1];
            foreach (var item in Banlists)
            {
                if (item.Name == name)
                {
                    returnValue = item;
                }
            }
            return returnValue;
        }

        public static Banlist GetByHash(uint hash)
        {
            Banlist returnValue = Banlists[Banlists.Count - 1];
            foreach (var item in Banlists)
            {
                if (item.Hash == hash)
                {
                    returnValue = item;
                }
            }
            return returnValue;
        }
    }
}
