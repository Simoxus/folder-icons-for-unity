using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FolderIconPaletteHandler
{
    static FolderIconPaletteHandler()
    {
        EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect rect)
    {
        Event e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0 || !e.alt) return;
        if (!rect.Contains(e.mousePosition)) return;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path)) return;

        e.Use();

        FolderIconPaletteWindow.Open(path, GUIUtility.GUIToScreenPoint(e.mousePosition));
    }
}