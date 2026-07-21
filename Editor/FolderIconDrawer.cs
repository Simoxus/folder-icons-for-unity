using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FolderIconDrawer
{
    private static readonly Dictionary<IconCacheKey, Texture2D> _compositedIcons = new Dictionary<IconCacheKey, Texture2D>();
    private static readonly Queue<Action> _generationQueue = new Queue<Action>();

    private static readonly HashSet<IconCacheKey> _pendingKeys = new HashSet<IconCacheKey>();
    private static readonly Stopwatch _budgetStopwatch = new Stopwatch();

    static FolderIconDrawer()
    {
        EditorApplication.projectWindowItemOnGUI -= DrawFolderIcon;
        EditorApplication.projectWindowItemOnGUI += DrawFolderIcon;

        EditorApplication.update -= ProcessGenerationQueue;
        EditorApplication.update += ProcessGenerationQueue;

        AssemblyReloadEvents.beforeAssemblyReload -= ClearCompositedCache;
        AssemblyReloadEvents.beforeAssemblyReload += ClearCompositedCache;
    }

    private static void DrawFolderIcon(string guid, Rect rect)
    {
        if (Event.current.type != EventType.Repaint) return;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path)) return;

        string folderName = Path.GetFileName(path);
        if (!FolderIconLibrary.TryGetEntry(path, folderName, out FolderIconEntry entry)) return;

        bool isSmallRow = rect.height <= 20f; // list view vs tile view
        int size = isSmallRow ? 16 : FolderIcon.GetQuantizedLargeSize(rect);
        bool isEmpty = FolderIcon.IsFolderEmpty(path);

        var cacheKey = new IconCacheKey(path, size, isEmpty);

        if (!_compositedIcons.TryGetValue(cacheKey, out Texture2D composited))
        {
            if (_pendingKeys.Add(cacheKey))
            {
                _generationQueue.Enqueue(() =>
                {
                    _compositedIcons[cacheKey] = FolderIconCompositor.Composite(entry, size, isEmpty);
                    _pendingKeys.Remove(cacheKey);
                });
            }

            return; // Draw it onec it's ready
        }

        Rect iconRect = isSmallRow ? FolderIcon.SmallIconRect(rect) : FolderIcon.LargeIconRect(rect);

        if (FolderIconSettings.ClearerRowsEnabled && isSmallRow)
        {
            int instanceId = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            bool isSelected = Selection.instanceIDs.Contains(instanceId);

            EditorGUI.DrawRect(iconRect, FolderIcon.GetRowBackgroundColor(rect, isSelected));
        }

        GUI.DrawTexture(iconRect, composited);
    }

    private static void ProcessGenerationQueue()
    {
        if (_generationQueue.Count == 0) return;

        double budgetMilliseconds = FolderIcon.GENERATION_BUDGET_MILLISECONDS;

        _budgetStopwatch.Restart();

        do
        {
            _generationQueue.Dequeue().Invoke();
        }
        while (_generationQueue.Count > 0 && _budgetStopwatch.Elapsed.TotalMilliseconds < budgetMilliseconds);

        EditorApplication.RepaintProjectWindow();
    }

    public static void ClearCompositedCache()
    {
        foreach (Texture2D texture in _compositedIcons.Values)
        {
            DestroyIfOwned(texture);
        }

        _compositedIcons.Clear();
        _generationQueue.Clear();
        _pendingKeys.Clear();
    }

    private static void DestroyIfOwned(Texture2D texture)
    {
        if (texture != null && texture.hideFlags == HideFlags.DontSave)
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private readonly struct IconCacheKey : IEquatable<IconCacheKey>
    {
        private readonly string _path;
        private readonly int _size;
        private readonly bool _isEmpty;

        public IconCacheKey(string path, int size, bool isEmpty)
        {
            _path = path;
            _size = size;
            _isEmpty = isEmpty;
        }

        public bool Equals(IconCacheKey other) =>
            _size == other._size &&
            _isEmpty == other._isEmpty &&
            string.Equals(_path, other._path, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is IconCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = _path != null ? StringComparer.Ordinal.GetHashCode(_path) : 0;
                hash = (hash * 397) ^ _size;
                hash = (hash * 397) ^ _isEmpty.GetHashCode();
                return hash;
            }
        }
    }
}