using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableAutoImporter : AssetPostprocessor
{
    private const string PATH_ROOT = "Assets/Addressables/";
    private const string PATH_AB_ROOT = "Assets/AddressableAssetsData/";
    private const string NAME_ICON_GROUP = "Icon";
    private const string NAME_SOUND_GROUP = "Sound";
    private const string NAME_TEXT_GROUP = "Text";

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedToAssets,
        string[] movedFromAssets)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        bool dirty = false;

        foreach (var assetPath in importedAssets)
        {
            if (IsFolder(assetPath))
                continue;
            if (!assetPath.StartsWith(PATH_ROOT)) continue;
            if (assetPath.StartsWith(PATH_AB_ROOT))
            {
                Debug.Log("Skip");
                continue;
            }

            var group = GetGroup(settings, assetPath);
            if(group == null)
            {
                Debug.LogError($"Auto Addressables: Find Group Faild: {assetPath}.");
                continue;
            }

            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            entry.address = GetAddress(assetPath);
            dirty = true;
        }

        if(dirty)
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
    }

    private static AddressableAssetGroup GetGroup(AddressableAssetSettings setting, string path)
    {
        if (path.Contains("/Icon/"))
            return setting.FindGroup(NAME_ICON_GROUP);
        else if (path.Contains("/Sound/"))
            return setting.FindGroup(NAME_SOUND_GROUP);
        else if(path.Contains("/Text/"))
            return setting.FindGroup(NAME_TEXT_GROUP);
        return null;
    }

    private static string GetAddress(string path)
    {
        var value = Path.GetFileNameWithoutExtension(path);
        if (path.Contains("/SD/"))
            value += "_SD";
        if (path.Contains("/HD/"))
            value += "_HD";
        return value;
    }

    private static bool IsFolder(string path)
    {
        return AssetDatabase.IsValidFolder(path);
    }

    private void OnPostprocessTexture(Texture2D texture)
    {
        if (!assetPath.StartsWith(PATH_ROOT)) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        if (importer == null) return;

        if(importer.textureType != TextureImporterType.Sprite)
        {
            EditorApplication.delayCall += () =>
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                if (TextureIsCharacterAvatar(assetPath))
                {
                    importer.isReadable = true;
                    var defaultSettings = importer.GetDefaultPlatformTextureSettings();
                    defaultSettings.format = TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(defaultSettings);
                }
                importer.SaveAndReimport();
            };
        }
    }

    private static readonly Regex Pattern = new(@"^sn\d{4}_3_\d+$");
    private static bool TextureIsCharacterAvatar(string path)
    {
        return Pattern.IsMatch(Path.GetFileNameWithoutExtension(path));
    }

}