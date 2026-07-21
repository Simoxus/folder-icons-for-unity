using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FolderIconLibrary
{
    private static readonly Dictionary<string, FolderIconEntry> _iconsByName =
        new Dictionary<string, FolderIconEntry>(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, FolderIconEntry> _iconsByPath =
        new Dictionary<string, FolderIconEntry>(StringComparer.OrdinalIgnoreCase);

    private static readonly List<(System.Text.RegularExpressions.Regex Pattern, FolderIconEntry Entry)> _iconsByNamePattern =
        new List<(System.Text.RegularExpressions.Regex, FolderIconEntry)>();

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
        _iconsByNamePattern.Clear();
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

        int maxDepth = FolderIconSettings.MaxMatchDepth;
        if (maxDepth > 0 && GetFolderDepth(folderPath) > maxDepth)
        {
            entry = default;
            return false;
        }

        if (_iconsByName.TryGetValue(folderName, out entry))
        {
            return !IsExcluded(entry, folderPath);
        }

        for (int i = 0; i < _iconsByNamePattern.Count; i++)
        {
            if (!_iconsByNamePattern[i].Pattern.IsMatch(folderName)) continue;

            entry = _iconsByNamePattern[i].Entry;
            return !IsExcluded(entry, folderPath);
        }

        entry = default;
        return false;
    }

    // "Assets" itself is depth 0; "Assets/Foo" is depth 1; "Assets/Foo/Bar" is depth 2, etc.
    private static int GetFolderDepth(string folderPath)
    {
        int depth = 0;
        for (int i = 0; i < folderPath.Length; i++)
        {
            if (folderPath[i] == '/') depth++;
        }
        return depth;
    }

    // Finds the mapping asset that would match this folder (by path, name, or wildcard),
    // regardless of whether the folder is currently excluded from it.
    public static FolderIconMapping FindMatchingMapping(string folderPath, string folderName)
    {
        string trimmedPath = folderPath.TrimEnd('/');

        if (_iconsByPath.TryGetValue(trimmedPath, out FolderIconEntry pathEntry)) return pathEntry.SourceMapping;

        if (_iconsByName.TryGetValue(folderName, out FolderIconEntry nameEntry)) return nameEntry.SourceMapping;

        for (int i = 0; i < _iconsByNamePattern.Count; i++)
        {
            if (_iconsByNamePattern[i].Pattern.IsMatch(folderName))
            {
                return _iconsByNamePattern[i].Entry.SourceMapping;
            }
        }

        return null;
    }

    private static bool IsExcluded(FolderIconEntry entry, string folderPath)
    {
        if (entry.ExcludedFolderPaths == null || entry.ExcludedFolderPaths.Length == 0) return false;

        string trimmedPath = folderPath.TrimEnd('/');
        for (int i = 0; i < entry.ExcludedFolderPaths.Length; i++)
        {
            if (string.Equals(entry.ExcludedFolderPaths[i], trimmedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Supports '*' wildcards (e.g. "*.Tests" matches "MyProject.Tests")
    private static System.Text.RegularExpressions.Regex CompileWildcard(string pattern)
    {
        string regexPattern = "^" + string.Join(".*",
            pattern.Split('*').Select(System.Text.RegularExpressions.Regex.Escape)) + "$";

        return new System.Text.RegularExpressions.Regex(
            regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    public static Texture2D[] GetPackageIcons()
    {
        string relative = FolderIcon.GetPackageIconsFolderAssetPath();

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
            var bundle = FolderIcon.GetEditorAssetBundle();
            if (bundle == null)
            {
                _cachedBuiltInIcons = System.Array.Empty<Texture2D>();
                return;
            }

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

    public static Texture2D[] GetCustomIcons()
    {
        string relative = FolderIcon.CUSTOM_ICONS_FOLDER_RELATIVE_PATH;
        if (!AssetDatabase.IsValidFolder(relative)) return new Texture2D[0];

        string absolute = Path.GetFullPath(relative).Replace('\\', '/');
        if (!Directory.Exists(absolute)) return new Texture2D[0];

        return Directory.GetFiles(absolute)
            .Select(f => FolderIcon.ToProjectRelativePath(f.Replace('\\', '/')))
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
            .Where(t => t != null)
            .OrderBy(t => t.name)
            .ToArray();
    }

    private static void LoadMappingAssetsFromPackageIconsFolder()
    {
        string absoluteFolder = IconsFolderAbsolute;
        string assetFolder = FolderIcon.GetPackageIconsFolderAssetPath();

        if (string.IsNullOrEmpty(absoluteFolder) || string.IsNullOrEmpty(assetFolder)) return;
        if (!Directory.Exists(absoluteFolder)) return;

        foreach (string file in Directory.GetFiles(absoluteFolder, "*.asset"))
        {
            string relativePath = $"{assetFolder}/{Path.GetFileName(file)}";

            var mapping = AssetDatabase.LoadAssetAtPath<FolderIconMapping>(relativePath);
            if (mapping == null) continue;

            RegisterMapping(mapping);
        }
    }

    private static void LoadMappingAssetsFromSettingsFolder()
    {
        string mapsFolder = FolderIcon.MAPS_FOLDER_RELATIVE_PATH;
        if (!AssetDatabase.IsValidFolder(mapsFolder)) return;

        foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(FolderIconMapping)}", new[] { mapsFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mapping = AssetDatabase.LoadAssetAtPath<FolderIconMapping>(path);
            if (mapping == null) continue;

            RegisterMapping(mapping);
        }
    }

    private static void RegisterMapping(FolderIconMapping mapping)
    {
        string[] excludedPaths = mapping.excludeFolderPaths != null && mapping.excludeFolderPaths.Count > 0
            ? mapping.excludeFolderPaths.Select(p => p.TrimEnd('/')).ToArray()
            : null;

        var entry = new FolderIconEntry(mapping.ResolvedIcon, mapping.folderColor, excludedPaths, mapping);

        foreach (string folderName in mapping.folderNames)
        {
            if (string.IsNullOrEmpty(folderName)) continue;

            if (folderName.Contains('*'))
            {
                _iconsByNamePattern.Add((CompileWildcard(folderName), entry));
            }
            else
            {
                _iconsByName[folderName] = entry;
            }
        }

        foreach (string folderPath in mapping.folderPaths)
        {
            if (string.IsNullOrEmpty(folderPath)) continue;
            _iconsByPath[folderPath.TrimEnd('/')] = entry;
        }
    }
}