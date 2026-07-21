using UnityEditor;
using UnityEngine;

internal static class FolderIconSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        var provider = new SettingsProvider("Preferences/Folder Icons", SettingsScope.User)
        {
            label = "Folder Icons",
            guiHandler = (searchContext) =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(10); // Top

                    using (new GUILayout.VerticalScope())
                    {
                        EditorGUILayout.Space(8); // Left

                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.LabelField("Row Decorations", EditorStyles.boldLabel);
                        FolderIconSettings.ClearerRowsEnabled = EditorGUILayout.Toggle(
                            new GUIContent("Clearer Rows", "Removes the folder icon from the tree view in the Project window. Slightly scuffed! (no reflection yet :P)"),
                            FolderIconSettings.ClearerRowsEnabled
                        );
                        if (FolderIconSettings.ClearerRowsEnabled)
                        {
                            FolderIconSettings.ClearerRowsBackgroundLight = EditorGUILayout.ColorField("Clearer Rows Background (Light)", FolderIconSettings.ClearerRowsBackgroundLight);
                            FolderIconSettings.ClearerRowsBackgroundDark = EditorGUILayout.ColorField("Clearer Rows Background (Dark)", FolderIconSettings.ClearerRowsBackgroundDark);
                        }
                        FolderIconSettings.ZebraStripingEnabled = EditorGUILayout.Toggle(
                            new GUIContent("Zebra Striping", "Alternates row alpha on adjacent rows to be easier on the eyes."),
                            FolderIconSettings.ZebraStripingEnabled
                        );
                        if (FolderIconSettings.ZebraStripingEnabled)
                        {
                            FolderIconSettings.ZebraStripeAlpha = EditorGUILayout.Slider("Zebra Stripe Alpha", FolderIconSettings.ZebraStripeAlpha, 0f, 0.2f);
                        }
                        FolderIconSettings.HierarchyLinesEnabled = EditorGUILayout.Toggle(
                            new GUIContent("Hierarchy Lines", "Draws branches, like a tree :D"),
                            FolderIconSettings.HierarchyLinesEnabled
                        );
                        if (FolderIconSettings.HierarchyLinesEnabled)
                        {
                            FolderIconSettings.HierarchyLineColorLight = EditorGUILayout.ColorField("Hierarchy Line Color (Light)", FolderIconSettings.HierarchyLineColorLight);
                            FolderIconSettings.HierarchyLineColorDark = EditorGUILayout.ColorField("Hierarchy Line Color (Dark)", FolderIconSettings.HierarchyLineColorDark);
                        }

                        EditorGUILayout.Space(8);

                        EditorGUILayout.LabelField("Sizing", EditorStyles.boldLabel);
                        FolderIconSettings.SmallOverlayScale = EditorGUILayout.Slider("Small Overlay Scale", FolderIconSettings.SmallOverlayScale, 0.1f, 1f);
                        FolderIconSettings.LargeOverlayScale = EditorGUILayout.Slider("Large Overlay Scale", FolderIconSettings.LargeOverlayScale, 0.1f, 1f);
                        FolderIconSettings.CornerOffset = EditorGUILayout.Slider("Corner Offset", FolderIconSettings.CornerOffset, 0f, 0.3f);

                        EditorGUILayout.Space(8);

                        EditorGUILayout.LabelField("Shadow", EditorStyles.boldLabel);
                        FolderIconSettings.ShadowLayers = EditorGUILayout.IntSlider("Shadow Layers", FolderIconSettings.ShadowLayers, 1, 12);
                        FolderIconSettings.ShadowMaxSpread = EditorGUILayout.Slider("Shadow Max Spread", FolderIconSettings.ShadowMaxSpread, 0f, 0.5f);
                        FolderIconSettings.ShadowMaxAlpha = EditorGUILayout.Slider("Shadow Max Alpha", FolderIconSettings.ShadowMaxAlpha, 0f, 1f);
                        FolderIconSettings.ShadowOffset = EditorGUILayout.Slider("Shadow Offset", FolderIconSettings.ShadowOffset, 0f, 0.3f);

                        EditorGUILayout.Space(8);

                        EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
                        FolderIconSettings.SmallOutlineAlphaMultiplier = EditorGUILayout.Slider("Small Outline Alpha Multiplier", FolderIconSettings.SmallOutlineAlphaMultiplier, 0f, 1f);
                        FolderIconSettings.OutlineColorLight = EditorGUILayout.ColorField("Outline Color (Light)", FolderIconSettings.OutlineColorLight);
                        FolderIconSettings.OutlineColorDark = EditorGUILayout.ColorField("Outline Color (Dark)", FolderIconSettings.OutlineColorDark);

                        EditorGUILayout.Space(8);

                        EditorGUILayout.LabelField("Render", EditorStyles.boldLabel);
                        FolderIconSettings.SizeStep = EditorGUILayout.IntSlider(
                            new GUIContent("Size Step", "Rounds large icon sizes to optimize caching."),
                            FolderIconSettings.SizeStep, 4, 64
                        );
                        FolderIconSettings.MinLargeSize = EditorGUILayout.IntSlider("Min Large Size", FolderIconSettings.MinLargeSize, 8, 128);
                        FolderIconSettings.MaxLargeSize = EditorGUILayout.IntSlider("Max Large Size", FolderIconSettings.MaxLargeSize, 64, 512);

                        if (EditorGUI.EndChangeCheck())
                        {
                            FolderIconDrawer.ClearCompositedCache();
                            EditorApplication.RepaintProjectWindow();
                        }
                    }
                }
            }
        };

        return provider;
    }
}