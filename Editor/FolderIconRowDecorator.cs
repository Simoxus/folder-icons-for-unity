using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FolderIconRowDecorator
{
    static FolderIconRowDecorator()
    {
        EditorApplication.projectWindowItemOnGUI -= DrawDecorations;
        EditorApplication.projectWindowItemOnGUI += DrawDecorations;
    }

    private static void DrawDecorations(string guid, Rect rowRect)
    {
        if (Event.current.type != EventType.Repaint) return;
        if (rowRect.height > 20f) return; // Only meaningful in list/tree view

        bool isListArea = Mathf.Approximately(rowRect.x, 14f);
        if (isListArea) return; // Only decorate the side tree view

        if (FolderIconSettings.ZebraStripingEnabled) DrawZebraStripe(rowRect);
        if (FolderIconSettings.HierarchyLinesEnabled) DrawHierarchyLines(rowRect);
    }

    private static void DrawZebraStripe(Rect rowRect)
    {
        // Alternate
        int rowIndex = Mathf.RoundToInt(rowRect.y / 16f);
        if (rowIndex % 2 == 0) return;

        var fullRowRect = new Rect(0f, rowRect.y, rowRect.x + rowRect.width, rowRect.height);

        EditorGUI.DrawRect(fullRowRect, new Color(0f, 0f, 0f, FolderIconSettings.ZebraStripeAlpha));
    }

    private static void DrawHierarchyLines(Rect rowRect)
    {
        int depth = Mathf.RoundToInt((rowRect.x - 16f) / 14f);
        if (depth <= 0) return;

        Color lineColor = EditorGUIUtility.isProSkin
            ? FolderIconSettings.HierarchyLineColorDark
            : FolderIconSettings.HierarchyLineColorLight;

        for (int i = 0; i < depth; i++)
        {
            float x = 9f + i * 14f;
            var lineRect = new Rect(x, rowRect.y, 1f, rowRect.height);
            EditorGUI.DrawRect(lineRect, lineColor);
        }
    }
}