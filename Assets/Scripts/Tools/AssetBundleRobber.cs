using AssetStudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

public class AssetBundleRobber : MonoBehaviour
{
    public Text text;
    string masterDuelAssetBundlePath;
    string masterDuelWindowsAssetBundlePath = "../../../Game/Steam/steamapps/common/Yu-Gi-Oh!  Master Duel/LocalData/16165626/0000/";
    string masterDuelAndroidAssetBundlePath = "../../../Game/Steam/steamapps/common/Yu-Gi-Oh!  Master Duel/LocalData/Android/0000/";
    string workingPlace;
    string androindWorkingPlace = "Android/Robber/";
    string windowsWorkingPlace = "StandaloneWindows64/Robber/";
    public static int fileCount;
    public static int currentFileCount;
    public static Dictionary<string, string> ydkIds = new Dictionary<string, string>();

    bool fullCopy;
    public struct AssetbundleInfo
    {
        public string path;
        public string name;
        public List<string> dependencies;
    }
    public static List<AssetbundleInfo> files = new List<AssetbundleInfo>();
    public static List<AssetbundleInfo> newFiles = new List<AssetbundleInfo>();

    void Start()
    {
        Application.targetFrameRate = 0;

        masterDuelAssetBundlePath = masterDuelWindowsAssetBundlePath;
        workingPlace = windowsWorkingPlace;
        masterDuelAssetBundlePath = masterDuelAndroidAssetBundlePath;
        workingPlace = androindWorkingPlace;

        //fullCopy = true;
        fullCopy = false;

        Initialize();

        //StartCoroutine(RefreshFileResources());
        Copy("fdea3f9a");
    }
            



    void Copy(string path)
    {
        foreach (var file in files)
        {
            if (file.path == path)
            {
                Directory.CreateDirectory(workingPlace + path);
                File.Copy(GetFullPath(file.path), workingPlace + path + "/" + file.path);
                foreach (var dep in file.dependencies)
                {
                    File.Copy(GetFullPath(dep), workingPlace + path + "/" + dep);

                }
            }

        }
        Debug.Log(path + ": Copy Done!");
    }

    void Initialize()
    {
        if(!Directory.Exists(workingPlace))
            Directory.CreateDirectory(workingPlace);
        var fullText = "";
        if(File.Exists(workingPlace + "FileList.txt"))
            fullText = File.ReadAllText(workingPlace + "FileList.txt");
        var lines = fullText.Replace("\r", "").Split('\n');
        AssetbundleInfo file = new AssetbundleInfo();
        file.dependencies = new List<string>();
        foreach (var line in lines)
        {
            if (!line.StartsWith("-"))
            {
                if (file.name != null)
                {
                    files.Add(file);
                    file = new AssetbundleInfo
                    {
                        dependencies = new List<string>(),
                        path = line
                    };
                }
                else
                    file.path = line;
            }
            else if (line.StartsWith("--"))
                file.dependencies.Add(line.Replace("--", ""));
            else
                file.name = line.Replace("-", "");
        }
        text.text = "Load FileList Complete.";
        Debug.Log("加载文件数量：" + files.Count);
        fullText = File.ReadAllText("data/YdkIds.txt");
        lines = fullText.Replace("\r", "").Split('\n');
        foreach(var line in lines)
        {
            var pair = Regex.Split(line, " ");
            if(pair.Length == 2 && !ydkIds.ContainsKey(pair[1]))
                ydkIds.Add(pair[1], pair[0]);
        }
    }

    IEnumerator RefreshFileResources()
    {
        var assetManager = GetComponent<AssetStudio.AssetsManager>();
        var ie = assetManager.LoadFolderAsync(masterDuelAssetBundlePath);
        StartCoroutine(ie);
        while(ie.MoveNext())
        {
            text.text = "LoadResources: " + currentFileCount + "/" + fileCount;
            yield return null;
        }
        bool modified = false;
        int count = 0;
        foreach (var file in assetManager.assetsFileList)
        {
            count++;
            string filePath = file.originalPath.Substring(file.originalPath.Length - 8);
            string fileName = "";
            foreach (var obj in file.Objects)
                if (obj is AssetStudio.AssetBundle assetBundle)
                    foreach (var pair in assetBundle.m_Container)
                    {
                        fileName = pair.Key;
                        break;
                    }
            bool contained = false;
            foreach(var f in files)
                if (f.path == filePath)
                {
                    contained = true;
                    break;
                }
            if (!contained)
            {
                modified = true;
                var filestruct = new AssetbundleInfo();
                filestruct.path = filePath;
                filestruct.name = fileName;
                filestruct.dependencies = GetDependencies(filePath);
                files.Add(filestruct);
                newFiles.Add(filestruct);
                text.text = "GetDependencies: " + count + "/" + assetManager.assetsFileList.Count;
                yield return null;
            }
        }
        if(modified)
        {
            var all = "";
            foreach (var file in files)
            {
                all += file.path + "\r\n";
                all += "-" + file.name + "\r\n";
                foreach (var depend in file.dependencies)
                    all += "--" + depend + "\r\n";
            }
            File.WriteAllText(workingPlace + "FileList.txt", all);
            Debug.Log("FileList更新完成，新文件：" + newFiles.Count);
        }
        else
            Debug.Log("没有新文件。");
        StartCoroutine(CopyBundles());
    }

    IEnumerator CopyBundles()
    {
        fileCount = files.Count;
        currentFileCount = 0;
        List<AssetbundleInfo> targetFiles;
        if (fullCopy)
            targetFiles = files;
        else
            targetFiles = newFiles;

        foreach (var file in targetFiles)
        {
            currentFileCount++;
            var type = GetAssetType(file.name);
            if (type == AssetType.Avatarstand)
            {
                if (!Directory.Exists(workingPlace + "Avatarstand"))
                    Directory.CreateDirectory(workingPlace + "Avatarstand");
                if(!File.Exists(workingPlace + "Avatarstand/" + Path.GetFileName(file.name).Replace(".prefab", "")))
                    File.Copy(GetFullPath(file.path), workingPlace + "Avatarstand/" +
                        Path.GetFileName(file.name).Replace(".prefab", ""));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Frame)
            {
                if (!Directory.Exists(workingPlace + "Frame"))
                    Directory.CreateDirectory(workingPlace + "Frame");
                File.Copy(GetFullPath(file.path), workingPlace + "Frame/" +
                    Path.GetFileName(file.name).Replace(".mat", "").Replace("profileframemat", "ProfileFrameMat"));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Grave)
            {
                if (!Directory.Exists(workingPlace + "Grave"))
                    Directory.CreateDirectory(workingPlace + "Grave");
                if (!File.Exists(workingPlace + "Grave/" + Path.GetFileName(file.name).Replace(".prefab", "")))
                    File.Copy(GetFullPath(file.path), workingPlace + "Grave/" +
                        Path.GetFileName(file.name).Replace(".prefab", ""));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Mat)
            {
                if (!Directory.Exists(workingPlace + "Mat"))
                    Directory.CreateDirectory(workingPlace + "Mat");
                if(!File.Exists(workingPlace + "Mat/" + Path.GetFileName(file.name).Replace(".prefab", "")))
                    File.Copy(GetFullPath(file.path), workingPlace + "Mat/" +
                        Path.GetFileName(file.name).Replace(".prefab", ""));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Mate)
            {
                if (!Directory.Exists(workingPlace + "Mate"))
                    Directory.CreateDirectory(workingPlace + "Mate");
                if (!File.Exists(workingPlace + "Mate/" + Path.GetFileName(file.name).Replace(".prefab", "")))
                    File.Copy(GetFullPath(file.path), workingPlace + "Mate/" +
                        Path.GetFileName(file.name).Replace(".prefab", ""));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Protector)
            {
                if (!Directory.Exists(workingPlace + "Protector"))
                    Directory.CreateDirectory(workingPlace + "Protector");
                string subDir = "107" + Regex.Split(file.name, "/")[4];
                if (!Directory.Exists(workingPlace + "Protector/" + subDir))
                    Directory.CreateDirectory(workingPlace + "Protector/" + subDir);
                File.Copy(GetFullPath(file.path), workingPlace + "Protector/" +
                    subDir + "/" + Path.GetFileName(file.name).Replace(".mat", "").Replace(".png", "")
                    .Replace("protectoricon", "").Replace("pmat", "PMat"));
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Wallpaper)
            {
                if (!Directory.Exists(workingPlace + "Wallpaper"))
                    Directory.CreateDirectory(workingPlace + "Wallpaper");
                string subDir = Path.GetFileName(file.name).Replace(".prefab", "");
                if (!Directory.Exists(workingPlace + "Wallpaper/" + subDir))
                    Directory.CreateDirectory(workingPlace + "Wallpaper/" + subDir);
                File.Copy(GetFullPath(file.path), workingPlace + "Wallpaper/" + subDir + "/" + subDir);
                List<string> depens = new List<string>();
                try
                {
                    depens = GetDependencies(file.path);
                }
                catch(Exception e)
                {
                    Debug.Log(file.path);
                    Debug.LogException(e);
                }
                foreach (string depen in depens)
                {
                    if (File.Exists(GetFullPath(depen)))
                        File.Copy(GetFullPath(depen), workingPlace + "Wallpaper/" + subDir + "/" + depen);
                    else
                        Debug.Log("未找到" + file.path + "的依赖：" + depen);
                }
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.Card)
            {
                if (!Directory.Exists(workingPlace + "Card"))
                    Directory.CreateDirectory(workingPlace + "Card");
                string subDir = int.Parse(Regex.Split(file.name, "/")[6].Replace("ef", "")).ToString();
                subDir = GetYdkID(subDir);
                if (file.name.Contains("/highend_hd/"))
                    subDir = "HD" + subDir;
                else if (file.name.Contains("/sd/"))
                    subDir = "SD" + subDir;
                if (!Directory.Exists(workingPlace + "Card/" + subDir))
                    Directory.CreateDirectory(workingPlace + "Card/" + subDir);
                File.Copy(GetFullPath(file.path), workingPlace + "Card/" +
                    subDir + "/" + file.path);
                List<string> depens = new List<string>();
                try
                {
                    depens = GetDependencies(file.path);
                }
                catch (Exception e)
                {
                    Debug.Log(file.path);
                    Debug.LogException(e);
                }
                foreach (string depen in depens)
                {
                    if (File.Exists(GetFullPath(depen)))
                    {
                        if(!File.Exists(workingPlace + "Card/" + subDir + "/" + depen))
                            File.Copy(GetFullPath(depen), workingPlace + "Card/" + subDir + "/" + depen);
                    }
                    else
                        Debug.Log("未找到" + file.path + "的依赖：" + depen);
                }
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
            else if (type == AssetType.MonsterCutin)
            {
                if (file.name.Contains("/sd/"))
                    if (workingPlace.Contains("Windows"))
                        continue;
                if (!Directory.Exists(workingPlace + "MonsterCutin"))
                    Directory.CreateDirectory(workingPlace + "MonsterCutin");
                string subDir = Regex.Split(file.name, "/")[7].Replace("p", "");
                subDir = GetYdkID(subDir);
                if (!Directory.Exists(workingPlace + "MonsterCutin/" + subDir))
                    Directory.CreateDirectory(workingPlace + "MonsterCutin/" + subDir);
                File.Copy(GetFullPath(file.path), workingPlace + "MonsterCutin/" +
                    subDir + "/" + file.path);
                List<string> depens = new List<string>();
                try
                {
                    depens = GetDependencies(file.path);
                }
                catch (Exception e)
                {
                    Debug.Log("查找" + file.path + "的依赖失败：");
                    Debug.LogException(e);
                }
                foreach (string depen in depens)
                {
                    if (File.Exists(GetFullPath(depen)))
                    {
                        if (!File.Exists(workingPlace + "MonsterCutin/" + subDir + "/" + depen))
                            File.Copy(GetFullPath(depen), workingPlace + "MonsterCutin/" + subDir + "/" + depen);
                    }
                    else
                        Debug.Log("未找到" + file.path + "的依赖：" + depen + ": " + GetFullPath(depen));
                }
                text.text = "Copying: " + currentFileCount + "/" + fileCount;
                yield return null;
            }
        }
        text.text = "Copy Complete.";
    }

    AssetType GetAssetType(string name)
    {
        if (name.Contains("assets/resourcesassetbundle/duel/bg/avatarstand/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.Avatarstand;
        }
        else if (name.Contains("assets/resourcesassetbundle/images/profileframe/"))
        {
            if (name.Contains(".mat"))
                return AssetType.Frame;
        }
        else if (name.Contains("/grave/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.Grave;
        }
        else if (name.Contains("/mat/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.Mat;
        }
        else if (name.Contains("/mate/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.Mate;
        }
        else if (name.Contains("/protector/"))
        {
            if (!name.Contains("/protector/shaders/"))
                return AssetType.Protector;
        }
        else if (name.Contains("/outgamebg/front/")
            || name.Contains("/wallpaper/wallpaper")
            )
        {
            if (name.Contains(".prefab"))
                return AssetType.Wallpaper;
        }
        else if (name.Contains("/duel/timeline/card/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.Card;
        }
        else if (name.Contains("/duel/timeline/duel/monstercutin/"))
        {
            if (name.Contains(".prefab"))
                return AssetType.MonsterCutin;
        }
        return AssetType.None;
    }
    public enum AssetType
    {
        None,
        Avatarstand,
        Card,
        Frame,
        Grave,
        Mat,
        Mate,
        MonsterCutin,
        Protector,
        Wallpaper
    }


    List<string> GetDependencies(string fileName, List<string> parentDepends = null)
    {
        byte[] bytes = Decompress(fileName);
        List<int> dependencyPositions = new List<int>();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0x2F)
                if(i + 9 < bytes.Length)
                    if (bytes[i + 2] == bytes[i - 1])
                        if (bytes[i + 1] == bytes[i - 2])
                        {
                            bool check = true;
                            if (bytes[i + 9] != 0x0)
                                check = false;
                            if (check)
                            {
                                for (int j = 1; j < 9; j++)
                                {
                                    if (bytes[i + j] >= 48 && bytes[i + j] <= 57
                                        || bytes[i + j] >= 65 && bytes[i + j] <= 90
                                        || bytes[i + j] >= 97 && bytes[i + j] <= 122
                                        )
                                    {
                                    }
                                    else
                                        check = false;
                                }
                            }
                            if (check)
                                dependencyPositions.Add(i);
                        }
        }

        List<string> dependencies = new List<string>();
        for (int i = 0; i < dependencyPositions.Count; i++)
        {
            List<byte> temp = new List<byte>();
            for (int j = dependencyPositions[i] + 1; j < dependencyPositions[i] + 9; j++)
            {
                temp.Add(bytes[j]);
            }
            var s = Encoding.UTF8.GetString(temp.ToArray());
            if(s != fileName)
            {
                if(parentDepends != null)
                {
                    if(!parentDepends.Contains(s))
                        dependencies.Add(s);
                }
                else
                    dependencies.Add(s);
            }
        }

        List<string> newParentDepends = new List<string>();
        foreach (var dependency in dependencies)
            newParentDepends.Add(dependency);
        if(parentDepends != null)
            foreach (var dependency in parentDepends)
                if(!newParentDepends.Contains(dependency))
                    newParentDepends.Add(dependency);

        List<string> subdepends = new List<string>();
        foreach(var value in dependencies)
        {
            var ss = GetDependencies(value, newParentDepends);
            foreach(var s in ss)
                if(!subdepends.Contains(s))
                    subdepends.Add(s);
        }
        foreach(var value in subdepends)
            if(!dependencies.Contains(value))
                dependencies.Add(value);
        return dependencies;
    }

    byte[] Decompress(string path)
    {
        var manager = new AssetsTools.NET.Extra.AssetsManager();
        if (!File.Exists(GetFullPath(path)))
        {
            Debug.Log("未找到" + path);
            return new byte[0];
        }
        BundleFileInstance bundleInst = manager.LoadBundleFile(GetFullPath(path), false);
        AssetBundleFile bundle = bundleInst.file;
        MemoryStream bundleStream = new MemoryStream();
        bundle.Unpack(new AssetsFileWriter(bundleStream));
        return bundleStream.GetBuffer();
    }

    string GetYdkID(string mdID)
    {
        if(!ydkIds.TryGetValue(mdID, out var ydkID))
            ydkID = mdID;
        return ydkID;
    }

    string GetFullPath(string path)
    {
        return masterDuelAssetBundlePath + path.Substring(0, 2) + "/" + path;
    }
}
