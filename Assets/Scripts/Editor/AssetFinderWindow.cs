using UnityEngine;
using UnityEditor;

public class AssetFinderWindow : EditorWindow
{
    private string searchGUID = string.Empty;
    private UnityEngine.Object foundAsset;
    private string searchMessage = string.Empty;
    private MessageType messageType;

    [MenuItem("Tools/Asset Finder by GUID")]
    public static void ShowWindow()
    {
        GetWindow<AssetFinderWindow>("GUID Finder");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Asset Finder by GUID", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 显示当前选中资源的GUID
        DrawSelectedAssetInfo();

        GUILayout.Space(20);
        EditorGUILayout.LabelField("查找资源", EditorStyles.boldLabel);

        // 输入GUID
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输入GUID:", GUILayout.Width(80));
        searchGUID = EditorGUILayout.TextField(searchGUID);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 查找按钮
        if (GUILayout.Button("通过GUID查找资源", GUILayout.Height(30)))
        {
            FindAssetByGUID();
        }

        // 显示查找消息
        if (!string.IsNullOrEmpty(searchMessage))
        {
            EditorGUILayout.HelpBox(searchMessage, messageType);
        }

        // 显示找到的资源
        if (foundAsset != null)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("找到的资源:");
            EditorGUILayout.ObjectField(foundAsset, foundAsset.GetType(), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("在Project窗口中定位"))
            {
                Selection.activeObject = foundAsset;
                EditorGUIUtility.PingObject(foundAsset);
            }

            if (GUILayout.Button("复制资源路径"))
            {
                string path = AssetDatabase.GetAssetPath(foundAsset);
                EditorGUIUtility.systemCopyBuffer = path;
                ShowNotification(new GUIContent("路径已复制到剪贴板"));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSelectedAssetInfo()
    {
        if (Selection.activeObject != null && AssetDatabase.Contains(Selection.activeObject))
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string guid = AssetDatabase.AssetPathToGUID(path);

            EditorGUILayout.LabelField("当前选中资源:");
            EditorGUILayout.ObjectField(Selection.activeObject, Selection.activeObject.GetType(), false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GUID:", GUILayout.Width(40));
            EditorGUILayout.TextField(guid);

            if (GUILayout.Button("复制", GUILayout.Width(40)))
            {
                EditorGUIUtility.systemCopyBuffer = guid;
                ShowNotification(new GUIContent("GUID已复制到剪贴板"));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("路径:", GUILayout.Width(40));
            EditorGUILayout.TextField(path);

            if (GUILayout.Button("复制", GUILayout.Width(40)))
            {
                EditorGUIUtility.systemCopyBuffer = path;
                ShowNotification(new GUIContent("路径已复制到剪贴板"));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void FindAssetByGUID()
    {
        if (string.IsNullOrEmpty(searchGUID))
        {
            searchMessage = "请输入GUID";
            messageType = MessageType.Error;
            return;
        }

        // 移除可能的前后空格
        searchGUID = searchGUID.Trim();

        // 验证GUID格式
        if (searchGUID.Length != 32)
        {
            searchMessage = "GUID格式不正确，应为32个字符";
            messageType = MessageType.Error;
            foundAsset = null;
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(searchGUID);

        if (string.IsNullOrEmpty(assetPath))
        {
            searchMessage = "未找到对应GUID的资源";
            messageType = MessageType.Error;
            foundAsset = null;
            return;
        }

        foundAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

        if (foundAsset != null)
        {
            searchMessage = "资源查找成功!";
            messageType = MessageType.Info;
        }
        else
        {
            searchMessage = "找到路径但无法加载资源";
            messageType = MessageType.Error;
        }
    }
}