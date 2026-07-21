// Inspired by: vFolders 2 and Rainbow Folders 2

using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class FolderIcon
{
    public const string SETTINGS_PARENT_RELATIVE_PATH = "Assets/Settings";
    public const string SETTINGS_FOLDER_RELATIVE_PATH = SETTINGS_PARENT_RELATIVE_PATH + "/Folder Icons";
    public const float GENERATION_BUDGET_MILLISECONDS = 2;

    public static string EnsureSettingsFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(SETTINGS_PARENT_RELATIVE_PATH))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        if (!AssetDatabase.IsValidFolder(SETTINGS_FOLDER_RELATIVE_PATH))
        {
            AssetDatabase.CreateFolder(SETTINGS_PARENT_RELATIVE_PATH, "Folder Icons");
        }

        return SETTINGS_FOLDER_RELATIVE_PATH;
    }

    public static string GetPackageAssetPath([CallerFilePath] string callerPath = null)
    {
        PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(FolderIcon).Assembly);
        if (packageInfo != null) return packageInfo.assetPath;

        string scriptFolder = Path.GetDirectoryName(callerPath)?.Replace('\\', '/');
        string packageFolder = Path.GetDirectoryName(scriptFolder)?.Replace('\\', '/');
        return ToProjectRelativePath(packageFolder);
    }

    // The bundled icons folder, as a Unity asset path
    public static string GetPackageIconsFolderAssetPath()
    {
        string packagePath = GetPackageAssetPath();
        return string.IsNullOrEmpty(packagePath) ? null : $"{packagePath}/Internal";
    }

    // The bundled icons folder
    public static string GetPackageIconsFolder()
    {
        string assetPath = GetPackageIconsFolderAssetPath();
        if (string.IsNullOrEmpty(assetPath)) return null;
        return Path.GetFullPath(assetPath).Replace('\\', '/');
    }

    public static string ToProjectRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return null;

        string normalized = absolutePath.Replace('\\', '/').TrimEnd('/');
        string dataPath = Application.dataPath; // Assets
        string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);

        return normalized.StartsWith(projectRoot) ? normalized.Substring(projectRoot.Length) : null;
    }

    public static bool IsFolderEmpty(string projectPath)
    {
        return AssetDatabase.FindAssets(string.Empty, new[] { projectPath }).Length == 0;
    }

    public static Color32 GetRowBackgroundColor(Rect rowRect, bool isSelected)
    {
        if (isSelected)
        {
            return EditorGUIUtility.isProSkin
                ? new Color32(73, 73, 73, 255)
                : new Color32(55, 108, 167, 255);
        }

        return EditorGUIUtility.isProSkin
            ? FolderIconSettings.ClearerRowsBackgroundDark
            : FolderIconSettings.ClearerRowsBackgroundLight;
    }

    public static int GetQuantizedLargeSize(Rect cellRect)
    {
        float rawSize = Mathf.Min(cellRect.width, cellRect.height - 14f);
        int quantized = Mathf.CeilToInt(rawSize / FolderIconSettings.SizeStep) * FolderIconSettings.SizeStep;
        return Mathf.Clamp(quantized, FolderIconSettings.MinLargeSize, FolderIconSettings.MaxLargeSize);
    }

    public static Rect SmallIconRect(Rect rowRect)
    {
        bool isListArea = Mathf.Approximately(rowRect.x, 14f);
        float x = rowRect.x + (isListArea ? 3f : 0f);
        return new Rect(x, rowRect.y, 16, 16);
    }

    public static Rect LargeIconRect(Rect cellRect)
    {
        float size = Mathf.Min(cellRect.width, cellRect.height - 14f);
        float x = cellRect.x + (cellRect.width - size) * 0.5f;
        return new Rect(x, cellRect.y + 1f, size, size);
    }

    public static Texture2D ResolveBaseIcon(bool isEmpty)
    {
        string targetName = isEmpty ? "FolderEmpty Icon" : "Folder Icon";
        string relative = GetPackageIconsFolderAssetPath();

        if (!string.IsNullOrEmpty(relative) && AssetDatabase.IsValidFolder(relative))
        {
            foreach (string guid in AssetDatabase.FindAssets($"{targetName} t:Texture2D", new[] { relative }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.Equals(Path.GetFileNameWithoutExtension(path), targetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) return texture;
            }
        }

        return EditorGUIUtility.FindTexture(targetName);
    }

    private static AssetBundle _cachedEditorAssetBundle;
    private static System.Reflection.MethodInfo _getEditorAssetBundleMethod;

    public static AssetBundle GetEditorAssetBundle()
    {
        if (_cachedEditorAssetBundle != null) return _cachedEditorAssetBundle;

        try
        {
            _getEditorAssetBundleMethod ??= typeof(EditorGUIUtility).GetMethod(
                "GetEditorAssetBundle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            _cachedEditorAssetBundle = (AssetBundle)_getEditorAssetBundleMethod.Invoke(null, null);
        }
        catch
        {
            _cachedEditorAssetBundle = null;
        }

        return _cachedEditorAssetBundle;
    }

    public static Texture2D ResolveBuiltInIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return null;

        Texture2D found = EditorGUIUtility.FindTexture(iconName);
        if (found != null) return found;

        AssetBundle bundle = GetEditorAssetBundle();
        if (bundle == null) return null;

        try
        {
            string path = Array.Find(bundle.GetAllAssetNames(),
                n => Path.GetFileNameWithoutExtension(n) == iconName);

            return string.IsNullOrEmpty(path) ? null : bundle.LoadAsset<Texture2D>(path);
        }
        catch
        {
            return null;
        }
    }

    public static Texture2D DownsampleTexture(Texture2D source, int targetSize)
    {
        if (source == null) return null;
        if (source.width <= targetSize && source.height <= targetSize) return source;

        RenderTexture previousActive = RenderTexture.active;

        int width = source.width;
        int height = source.height;

        var previousFilterMode = source.filterMode;
        source.filterMode = FilterMode.Bilinear;

        RenderTexture current = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        Graphics.Blit(source, current);

        while (width > targetSize * 2 && height > targetSize * 2)
        {
            int nextWidth = Mathf.Max(targetSize, width / 2);
            int nextHeight = Mathf.Max(targetSize, height / 2);

            RenderTexture next = RenderTexture.GetTemporary(nextWidth, nextHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(current, next);

            RenderTexture.ReleaseTemporary(current);
            current = next;
            width = nextWidth;
            height = nextHeight;
        }

        RenderTexture final = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        Graphics.Blit(current, final);
        RenderTexture.ReleaseTemporary(current);

        source.filterMode = previousFilterMode;

        RenderTexture.active = final;
        var result = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.DontSave,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0
        };
        result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
        result.Apply(false, false);

        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(final);

        return result;
    }
}