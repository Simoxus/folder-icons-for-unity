using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class FolderIconLibrary
{
    private static readonly Dictionary<string, FolderIconEntry> _iconsByName =
        new Dictionary<string, FolderIconEntry>(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, FolderIconEntry> _iconsByPath =
        new Dictionary<string, FolderIconEntry>(StringComparer.OrdinalIgnoreCase);

    private static string IconsFolderAbsolute => FolderIcon.GetPackageIconsFolder();

    private static Texture2D[] _cachedBuiltInIcons;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        Rebuild();
    }

    public static void Rebuild()
    {
        _iconsByName.Clear();
        _iconsByPath.Clear();
        _cachedBuiltInIcons = null;
        LoadMappingAssetsFromPackageIconsFolder();
        LoadMappingAssetsFromSettingsFolder();

        FolderIconDrawer.ClearCompositedCache();
        EditorApplication.RepaintProjectWindow();
    }

    // Path matches are more specific than name matches, so they take priority
    public static bool TryGetEntry(string folderPath, string folderName, out FolderIconEntry entry)
    {
        if (_iconsByPath.TryGetValue(folderPath, out entry)) return true;
        return _iconsByName.TryGetValue(folderName, out entry);
    }

    public static Texture2D[] GetPackageIcons()
    {
        string relative = FolderIcon.ToProjectRelativePath(IconsFolderAbsolute);

        if (string.IsNullOrEmpty(relative) || !AssetDatabase.IsValidFolder(relative))
        {
            return new Texture2D[0];
        }

        return AssetDatabase.FindAssets("t:Texture2D", new[] { relative })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
            .Where(t => t != null)
            .OrderBy(t => t.name)
            .ToArray();
    }

    public static Texture2D[] GetBuiltInIcons()
    {
        LoadBuiltInIcons();
        return _cachedBuiltInIcons;
    }

    private static void LoadBuiltInIcons()
    {
        if (_cachedBuiltInIcons != null) return;

        try
        {
            var method = typeof(EditorGUIUtility).GetMethod(
                "GetEditorAssetBundle",
                BindingFlags.NonPublic | BindingFlags.Static);

            var bundle = (AssetBundle)method.Invoke(null, null);

            var packageIconNames = new HashSet<string>(
                GetPackageIcons().Select(t => "d_" + t.name),
                StringComparer.OrdinalIgnoreCase);

            _cachedBuiltInIcons = bundle.GetAllAssetNames()
                .Select(n => Path.GetFileNameWithoutExtension(n))
                .Where(name => name.StartsWith("d_")
                            && !name.EndsWith("@2x")
                            && !name.EndsWith("Icon")
                            && !packageIconNames.Contains(name))
                .Select(n => bundle.LoadAsset<Texture2D>(n))
                .Where(t => t != null && t.width > 32 && t.height > 32)
                .GroupBy(t => t.name)
                .Select(g => g.First())
                .OrderBy(t => t.name)
                .ToArray();
        }
        catch
        {
            _cachedBuiltInIcons = System.Array.Empty<Texture2D>();
        }
    }

    private static void LoadMappingAssetsFromPackageIconsFolder()
    {
        if (!Directory.Exists(IconsFolderAbsolute)) return;

        foreach (string file in Directory.GetFiles(IconsFolderAbsolute, "*.asset"))
        {
            string relativePath = FolderIcon.ToProjectRelativePath(file);
            if (relativePath == null) continue;

            var mapping = AssetDatabase.LoadAssetAtPath<FolderIconMapping>(relativePath);
            if (mapping == null) continue;

            RegisterMapping(mapping);
        }
    }

    private static void LoadMappingAssetsFromSettingsFolder()
    {
        string settingsFolder = FolderIcon.SETTINGS_FOLDER_RELATIVE_PATH;
        if (!AssetDatabase.IsValidFolder(settingsFolder)) return;

        foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(FolderIconMapping)}", new[] { settingsFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mapping = AssetDatabase.LoadAssetAtPath<FolderIconMapping>(path);
            if (mapping == null) continue;

            RegisterMapping(mapping);
        }
    }

    private static void RegisterMapping(FolderIconMapping mapping)
    {
        var entry = new FolderIconEntry(mapping.ResolvedIcon, mapping.folderColor);

        foreach (string folderName in mapping.folderNames)
        {
            if (string.IsNullOrEmpty(folderName)) continue;
            _iconsByName[folderName] = entry;
        }

        foreach (string folderPath in mapping.folderPaths)
        {
            if (string.IsNullOrEmpty(folderPath)) continue;
            _iconsByPath[folderPath.TrimEnd('/')] = entry;
        }
    }
}