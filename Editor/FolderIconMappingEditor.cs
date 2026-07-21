using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FolderIconMapping))]
public class FolderIconMappingEditor : Editor
{
    private SerializedProperty _icon;
    private SerializedProperty _builtInIconName;
    private SerializedProperty _folderColor;
    private SerializedProperty _folderNames;
    private SerializedProperty _folderPaths;

    private void OnEnable()
    {
        _icon = serializedObject.FindProperty("icon");
        _builtInIconName = serializedObject.FindProperty("builtInIconName");
        _folderColor = serializedObject.FindProperty("folderColor");
        _folderNames = serializedObject.FindProperty("folderNames");
        _folderPaths = serializedObject.FindProperty("folderPaths");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIconSettings();
        EditorGUILayout.Space();
        DrawColorSettings();
        EditorGUILayout.Space();
        DrawFolderMatchSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIconSettings()
    {
        EditorGUILayout.PropertyField(_icon);
        EditorGUILayout.PropertyField(_builtInIconName);
    }

    private void DrawColorSettings()
    {
        EditorGUILayout.PropertyField(_folderColor);
    }

    private void DrawFolderMatchSettings()
    {
        EditorGUILayout.PropertyField(_folderNames, true);
        DrawFolderPathsList();
    }

    private void DrawFolderPathsList()
    {
        EditorGUILayout.PropertyField(_folderPaths, false); // just the foldout/label, no children

        if (!_folderPaths.isExpanded) return;

        EditorGUI.indentLevel++;

        for (int i = 0; i < _folderPaths.arraySize; i++)
        {
            SerializedProperty element = _folderPaths.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            element.stringValue = EditorGUILayout.TextField(element.stringValue);

            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    string relative = FolderIcon.ToProjectRelativePath(selected);
                    if (!string.IsNullOrEmpty(relative))
                    {
                        element.stringValue = relative;
                    }
                }
            }

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                _folderPaths.DeleteArrayElementAtIndex(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            _folderPaths.InsertArrayElementAtIndex(_folderPaths.arraySize);
            _folderPaths.GetArrayElementAtIndex(_folderPaths.arraySize - 1).stringValue = "";
        }

        EditorGUI.indentLevel--;
    }
}