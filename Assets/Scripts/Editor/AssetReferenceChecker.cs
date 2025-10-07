using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class AssetReferenceChecker : EditorWindow
{
    private string targetGuid = string.Empty;
    private string targetPath = string.Empty;
    private int referenceCount = 0;
    private List<string> referencingAssets = new List<string>();
    private Vector2 scrollPosition;
    private bool isSearching = false;
    private double searchStartTime;
    private string newGuid = string.Empty;
    private Object targetAsset;

    [MenuItem("Tools/Asset Reference Checker")]
    public static void ShowWindow()
    {
        GetWindow<AssetReferenceChecker>("资源引用查看器");
    }

    [MenuItem("Assets/检查资源引用", false, 100)]
    private static void CheckAssetReferences()
    {
        ShowWindow();
        GetWindow<AssetReferenceChecker>().FindReferencesForSelectedAsset();
    }

    [MenuItem("Assets/检查资源引用", true)]
    private static bool ValidateReplaceStringsInAsset()
    {
        return Selection.activeObject != null &&
               !AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    private void FindReferencesForSelectedAsset()
    {
        if (Selection.activeObject == null) return;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        targetGuid = AssetDatabase.AssetPathToGUID(path);
        targetPath = path;
        targetAsset = Selection.activeObject;
        // 选择新资产时，重置引用显示
        ResetReferenceDisplay();
        // 如果你希望右键菜单选择后自动开始检查，则取消下一行的注释
        // FindReferences(); 
    }

    // 新增方法：重置引用相关的显示
    private void ResetReferenceDisplay()
    {
        referenceCount = 0;
        referencingAssets.Clear();
        isSearching = false;
        newGuid = ""; // 清空之前生成的新GUID输入
    }

    private void OnGUI()
    {
        GUILayout.Label("资源引用检查器", EditorStyles.boldLabel);

        // 使用 ObjectField 替代文本字段，支持拖拽和选择
        EditorGUI.BeginChangeCheck();
        targetAsset = EditorGUILayout.ObjectField("目标资源", targetAsset, typeof(Object), false);
        // 如果目标资源字段发生了改变（用户拖入或选择了新资产）
        if (EditorGUI.EndChangeCheck())
        {
            if (targetAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(targetAsset);
                targetGuid = AssetDatabase.AssetPathToGUID(path);
                targetPath = path;
                // 重要：当目标资源改变时，立即重置引用显示
                ResetReferenceDisplay();
            }
            else
            {
                // 如果清空了目标资源，也重置所有状态
                targetGuid = "";
                targetPath = "";
                ResetReferenceDisplay();
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("GUID:", GUILayout.Width(80));
        EditorGUILayout.TextField(targetGuid ?? "");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("检查引用", GUILayout.Height(30)) && !string.IsNullOrEmpty(targetGuid))
        {
            FindReferences();
        }

        if (isSearching)
        {
            double elapsedTime = EditorApplication.timeSinceStartup - searchStartTime;
            EditorGUILayout.HelpBox($"正在检查中... 已用时: {elapsedTime:F2}秒", MessageType.Info);
            if (GUILayout.Button("取消"))
            {
                isSearching = false;
                EditorUtility.ClearProgressBar();
            }
        }

        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(targetGuid))
        {
            EditorGUILayout.BeginHorizontal();
            // 显示引用数量，如果还未检查则显示“待检查”
            GUILayout.Label($"引用数量: {(referenceCount > 0 || isSearching ? referenceCount.ToString() : "待检查")}");
            EditorGUILayout.EndHorizontal();

            // 只有在有引用结果或者正在搜索时才显示列表
            if (referencingAssets.Count > 0 || isSearching)
            {
                GUILayout.Label("引用此资源的资产:");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

                foreach (string assetPath in referencingAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(assetPath);

                    if (GUILayout.Button("定位", GUILayout.Width(40)))
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // 添加GUID修改功能
            GUILayout.Label("更改GUID (危险操作!)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("警告: 更改GUID是高风险操作，可能导致项目损坏。请确保已备份项目!", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("新GUID:", GUILayout.Width(60));
            newGuid = EditorGUILayout.TextField(newGuid);
            if (GUILayout.Button("生成", GUILayout.Width(50)))
            {
                newGuid = GenerateNewGUID();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newGuid) || newGuid == targetGuid/* || referenceCount == 0*/); // 同时在没有引用结果时禁用按钮
            if (GUILayout.Button("更改GUID并更新所有引用", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认更改GUID",
                    "这是一个高风险操作!\n\n更改GUID可能导致项目损坏。请确保已备份项目。\n\n是否继续?",
                    "继续", "取消"))
                {
                    ChangeAssetGuidAndUpdateReferences(targetGuid, newGuid, targetPath);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            // 当没有选择有效资源时给出提示
            EditorGUILayout.HelpBox("请从上方拖入或选择一个有效的资源。", MessageType.Info);
        }
    }

    private void FindReferences()
    {
        if (string.IsNullOrEmpty(targetGuid)) return;

        // 设置序列化模式为文本以便查找
        EditorSettings.serializationMode = SerializationMode.ForceText;

        // 每次检查前先重置引用显示，确保结果是最新的
        ResetReferenceDisplay();
        isSearching = true;
        searchStartTime = EditorApplication.timeSinceStartup;

        // 获取项目中所有资源路径
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<string> validPaths = new List<string>();

        // 筛选需要检查的资源类型
        foreach (string path in allAssetPaths)
        {
            if (path.StartsWith("Assets/") &&
                (path.EndsWith(".prefab") || path.EndsWith(".unity") ||
                 path.EndsWith(".mat") || path.EndsWith(".asset") ||
                 path.EndsWith(".controller") || path.EndsWith(".overrideController") ||
                 path.EndsWith(".anim") || path.EndsWith(".mask") ||
                 path.EndsWith(".physicsMaterial2D") || path.EndsWith(".physicsMaterial") ||
                 path.EndsWith(".spriteatlas") || path.EndsWith(".rendertexture") ||
                 path.EndsWith(".flare") || path.EndsWith(".font") ||
                 path.EndsWith(".guiskin") || path.EndsWith(".shadervariants") ||
                 path.EndsWith(".playable")))
            {
                validPaths.Add(path);
            }
        }

        try
        {
            for (int i = 0; i < validPaths.Count; i++)
            {
                string path = validPaths[i];

                // 跳过目标资源自身
                if (path == targetPath) continue;

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar("检查资源引用",
                    $"正在检查: {Path.GetFileName(path)}", (float)i / validPaths.Count))
                {
                    break;
                }

                try
                {
                    // 读取文件内容并检查是否包含目标GUID
                    string content = File.ReadAllText(path);
                    if (Regex.IsMatch(content, $"guid: {targetGuid}\\b"))
                    {
                        referenceCount++;
                        referencingAssets.Add(path);
                    }
                }
                catch
                {
                    // 忽略无法读取的文件
                    continue;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isSearching = false;
            Repaint();
        }
    }

    private string GenerateNewGUID()
    {
        return System.Guid.NewGuid().ToString("N");
    }

    private void ChangeAssetGuidAndUpdateReferences(string oldGuid, string newGuid, string assetPath)
    {
        if (string.IsNullOrEmpty(oldGuid) || string.IsNullOrEmpty(newGuid) || string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("无法更改GUID: 参数不能为空");
            return;
        }

        if (oldGuid == newGuid)
        {
            Debug.LogError("新GUID不能与旧GUID相同");
            return;
        }

        // 首先确保我们有最新的引用列表
        if (referencingAssets == null || referencingAssets.Count == 0)
        {
            FindReferences();
        }

        // 1. 更新所有已知引用
        UpdateKnownReferences(oldGuid, newGuid);

        // 2. 检查并更新可能包含引用的其他文件类型
        UpdateAdditionalReferences(oldGuid, newGuid);

        // 3. 然后更改目标资源的GUID
        ChangeAssetGuid(assetPath, newGuid);

        // 4. 刷新数据库
        AssetDatabase.Refresh();

        // 5. 重新加载当前资源，更新引用以避免ObjectField显示Missing
        // 重点：在刷新后重新加载资源并更新targetAsset
        targetAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (targetAsset == null)
        {
            Debug.LogWarning($"重新加载资源失败: {assetPath}。ObjectField可能仍显示为Missing。");
        }

        // 6. 更新UI显示
        targetGuid = newGuid;

        // 7. 重新查找引用以更新UI
        FindReferences();

        Debug.Log($"成功更改GUID: {oldGuid} -> {newGuid}");

        // 8. 强制重绘窗口，确保UI立即更新
        Repaint();
    }

    private void UpdateKnownReferences(string oldGuid, string newGuid)
    {
        try
        {
            for (int i = 0; i < referencingAssets.Count; i++)
            {
                string path = referencingAssets[i];

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar("更新引用",
                    $"正在处理: {Path.GetFileName(path)}", (float)i / referencingAssets.Count))
                {
                    break;
                }

                try
                {
                    // 跳过目标资源自身
                    if (path == targetPath) continue;

                    // 读取文件内容
                    string content = File.ReadAllText(path);

                    // 使用正则表达式替换GUID引用
                    string newContent = Regex.Replace(content, $"guid: {oldGuid}\\b", $"guid: {newGuid}");

                    // 如果内容有变化，则写回文件
                    if (content != newContent)
                    {
                        File.WriteAllText(path, newContent);
                        Debug.Log($"已更新引用: {path}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"处理文件 {path} 时出错: {e.Message}");
                    continue;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void UpdateAdditionalReferences(string oldGuid, string newGuid)
    {
        // 检查一些可能包含引用的额外文件类型
        string[] additionalPaths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/") &&
                       (p.EndsWith(".shader") || p.EndsWith(".compute") ||
                        p.EndsWith(".cginc") || p.EndsWith(".hlsl") ||
                        p.EndsWith(".cs") || p.EndsWith(".js") ||
                        p.EndsWith(".json") || p.EndsWith(".xml")))
            .ToArray();

        try
        {
            for (int i = 0; i < additionalPaths.Length; i++)
            {
                string path = additionalPaths[i];

                // 跳过已经处理过的文件
                if (referencingAssets.Contains(path)) continue;

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar("检查额外引用",
                    $"正在检查: {Path.GetFileName(path)}", (float)i / additionalPaths.Length))
                {
                    break;
                }

                try
                {
                    // 读取文件内容
                    string content = File.ReadAllText(path);

                    // 检查是否包含目标GUID
                    if (Regex.IsMatch(content, $"guid: {oldGuid}\\b"))
                    {
                        // 更新引用
                        string newContent = Regex.Replace(content, $"guid: {oldGuid}\\b", $"guid: {newGuid}");
                        File.WriteAllText(path, newContent);
                        Debug.Log($"发现并更新额外引用: {path}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"处理文件 {path} 时出错: {e.Message}");
                    continue;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void ChangeAssetGuid(string assetPath, string newGuid)
    {
        string metaFilePath = assetPath + ".meta";

        if (!File.Exists(metaFilePath))
        {
            Debug.LogError($"找不到meta文件: {metaFilePath}");
            return;
        }

        try
        {
            // 读取meta文件内容
            string content = File.ReadAllText(metaFilePath);

            // 使用正则表达式替换GUID
            string newContent = Regex.Replace(content, "guid: [a-f0-9]{32}", $"guid: {newGuid}");

            // 写回meta文件
            File.WriteAllText(metaFilePath, newContent);

            Debug.Log($"已更改资源GUID: {assetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"更改GUID时出错: {e.Message}");
        }
    }
}