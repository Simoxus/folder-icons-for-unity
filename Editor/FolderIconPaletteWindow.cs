using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FolderIconPaletteWindow : EditorWindow
{
    private const float GRID_SCROLLBAR_WIDTH = 16f;
    private const float CUSTOM_ICONS_SCROLL_HEIGHT = CELL_SIZE + 15f;
    private const float PACKAGE_ICONS_SCROLL_HEIGHT = 140f;
    private const float BUILT_IN_ICONS_SCROLL_HEIGHT = 140f;
    private const float BOTTOM_PADDING = 8f;
    private const int ICON_COLUMNS = 5;
    private const float CELL_SIZE = 32f;
    private const float CELL_SPACING = 3f;
    private const double SAVE_DEBOUNCE_SECONDS = 0.3;

    private float _minWindowHeight;

    private string _folderPath;
    private string _folderName;
    private FolderIconMapping _mapping;

    private bool _usePath;
    private Color _folderColor = Color.white;

    private Texture2D[] _customIcons;
    private Vector2 _scrollCustom;

    private Texture2D[] _packageIcons;
    private string _packageSearch = string.Empty;
    private Texture2D _selectedPackageIcon;

    private Texture2D[] _builtInIcons;
    private string _builtInSearch = string.Empty;
    private string _selectedBuiltInIconName;

    private Vector2 _scrollPackage;
    private Vector2 _scrollBuiltIn;

    private double _pendingSaveTime = -1;

    private void OnEnable()
    {
        EditorApplication.update += CheckPendingSave;
    }

    private void OnDisable()
    {
        EditorApplication.update -= CheckPendingSave;
    }

    private void CheckPendingSave()
    {
        if (_pendingSaveTime < 0) return;
        if (EditorApplication.timeSinceStartup < _pendingSaveTime) return;

        _pendingSaveTime = -1;

        if (_selectedPackageIcon != null)
        {
            ApplyAndSave();
        }
    }

    public static void Open(string folderPath, Vector2 screenPosition)
    {
        var window = CreateInstance<FolderIconPaletteWindow>();
        window.Setup(folderPath);

        // Rough initial estimate
        float estimatedHeight =
            8 + 20 + 6 + 20 + 6 + 18 + 6
          + 18 + 2 + CUSTOM_ICONS_SCROLL_HEIGHT + 8
          + 18 + 2 + PACKAGE_ICONS_SCROLL_HEIGHT + 40   // padding
          + 8
          + 18 + 2 + BUILT_IN_ICONS_SCROLL_HEIGHT + 40  // paddington
          + 8;

        window._minWindowHeight = estimatedHeight;

        window.ShowAsDropDown(new Rect(screenPosition, Vector2.zero),
            new Vector2(ICON_COLUMNS * (CELL_SIZE + CELL_SPACING) + 24 + GRID_SCROLLBAR_WIDTH + 8, estimatedHeight));
    }

    private void Setup(string folderPath)
    {
        _folderPath = folderPath;
        _folderName = Path.GetFileName(folderPath);

        _customIcons = FolderIconLibrary.GetCustomIcons();
        _packageIcons = FolderIconLibrary.GetPackageIcons();
        _builtInIcons = FolderIconLibrary.GetBuiltInIcons();

        _mapping = FindExistingMapping(_folderName);

        if (_mapping != null)
        {
            _folderColor = _mapping.folderColor;
            _usePath = _mapping.folderPaths != null && _mapping.folderPaths.Count > 0
                       && (_mapping.folderNames == null || _mapping.folderNames.Count == 0);

            if (!string.IsNullOrEmpty(_mapping.builtInIconName))
            {
                _selectedBuiltInIconName = _mapping.builtInIconName;
                _selectedPackageIcon = System.Array.Find(_builtInIcons, t => t.name == _mapping.builtInIconName);
            }
            else
            {
                _selectedBuiltInIconName = null;
                _selectedPackageIcon = _mapping.icon;
            }
        }
    }

    private static FolderIconMapping FindExistingMapping(string folderName)
    {
        string targetName = $"{folderName} Mapping";

        foreach (string guid in AssetDatabase.FindAssets("t:FolderIconMapping"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.Equals(Path.GetFileNameWithoutExtension(path), targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<FolderIconMapping>(path);
            }
        }

        return null;
    }

    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Space(8);

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(8);

                EditorGUILayout.LabelField(_folderName, EditorStyles.boldLabel);
                GUILayout.Space(6);

                EditorGUI.BeginChangeCheck();
                _folderColor = EditorGUILayout.ColorField("Folder Color", _folderColor);
                if (EditorGUI.EndChangeCheck())
                {
                    _pendingSaveTime = EditorApplication.timeSinceStartup + SAVE_DEBOUNCE_SECONDS;
                }

                GUILayout.Space(6);

                EditorGUI.BeginChangeCheck();
                _usePath = EditorGUILayout.ToggleLeft(
                    new GUIContent("Match By Path",
                        $"On: Only this folder gets the icon.\nOff: Any folder named \"{_folderName}\" gets it."),
                    _usePath);
                bool boolsChanged = EditorGUI.EndChangeCheck();

                if (!_usePath && _mapping != null)
                {
                    if (GUILayout.Button(new GUIContent("Exclude Folder from Mapping",
                        "Prevents this specific folder from getting the icon, even though it matches by name.")))
                    {
                        ExcludeFolderFromMapping();
                    }
                }

                GUILayout.Space(6);

                bool customChanged = false;
                EditorGUILayout.LabelField(new GUIContent("Custom Icons", "Icons placed directly in the Folder Icons settings folder."), EditorStyles.boldLabel);
                GUILayout.Space(2);
                if (_customIcons != null && _customIcons.Length > 0)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        _scrollCustom = EditorGUILayout.BeginScrollView(
                            _scrollCustom,
                            alwaysShowHorizontal: true,
                            alwaysShowVertical: false,
                            horizontalScrollbar: GUI.skin.horizontalScrollbar,
                            verticalScrollbar: GUIStyle.none,
                            background: GUI.skin.scrollView,
                            GUILayout.Height(CUSTOM_ICONS_SCROLL_HEIGHT));
                        customChanged = DrawIconGrid(_customIcons, "No custom icons found.", isBuiltIn: false, maxColumns: int.MaxValue);
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"No custom icons found. Add some in {FolderIcon.CUSTOM_ICONS_FOLDER_RELATIVE_PATH}.", MessageType.Info);
                }
                GUILayout.Space(8);

                EditorGUILayout.LabelField(new GUIContent("Package Icons", "Icons bundled with Folder Icons."), EditorStyles.boldLabel);
                GUILayout.Space(2);
                _packageSearch = EditorGUILayout.TextField(_packageSearch, EditorStyles.toolbarSearchField);
                bool packageChanged;
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    _scrollPackage = EditorGUILayout.BeginScrollView(
                        _scrollPackage,
                        alwaysShowHorizontal: false,
                        alwaysShowVertical: false,
                        horizontalScrollbar: GUIStyle.none,
                        verticalScrollbar: GUI.skin.verticalScrollbar,
                        background: GUI.skin.scrollView,
                        GUILayout.Height(PACKAGE_ICONS_SCROLL_HEIGHT));
                    Texture2D[] filteredPackage = FilterIcons(_packageIcons, _packageSearch);
                    packageChanged = DrawIconGrid(filteredPackage, "No icons found.", isBuiltIn: false);
                    EditorGUILayout.EndScrollView();
                }

                GUILayout.Space(8);

                EditorGUILayout.LabelField(new GUIContent("Built-In Icons", "Icons that either from the package or under 32x32 are excluded."), EditorStyles.boldLabel);
                GUILayout.Space(2);
                _builtInSearch = EditorGUILayout.TextField(_builtInSearch, EditorStyles.toolbarSearchField);
                bool builtInChanged;
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    _scrollBuiltIn = EditorGUILayout.BeginScrollView(
                        _scrollBuiltIn,
                        alwaysShowHorizontal: false,
                        alwaysShowVertical: false,
                        horizontalScrollbar: GUIStyle.none,
                        verticalScrollbar: GUI.skin.verticalScrollbar,
                        background: GUI.skin.scrollView,
                        GUILayout.Height(BUILT_IN_ICONS_SCROLL_HEIGHT));
                    Texture2D[] filteredBuiltIn = FilterIcons(_builtInIcons, _builtInSearch);
                    builtInChanged = DrawIconGrid(filteredBuiltIn, "No built-in icons found.", isBuiltIn: true);
                    EditorGUILayout.EndScrollView();
                }

                bool iconChanged = packageChanged || builtInChanged || customChanged;

                GUILayout.Space(8);

                if ((boolsChanged || iconChanged))
                {
                    _pendingSaveTime = -1;
                    ApplyAndSave();
                }
            }

            GUILayout.Space(8);
        }

        if (Event.current.type == EventType.Repaint)
        {
            FitWindowToContent();
        }
    }

    private void FitWindowToContent()
    {
        Rect contentRect = GUILayoutUtility.GetLastRect();
        float desiredHeight = Mathf.Max(contentRect.yMax + BOTTOM_PADDING, _minWindowHeight);

        if (Mathf.Abs(desiredHeight - position.height) > 0.5f)
        {
            position = new Rect(position.x, position.y, position.width, desiredHeight);
        }
    }

    // Returns true if the selection changed this frame
    private bool DrawIconGrid(Texture2D[] icons, string emptyMessage, bool isBuiltIn, int maxColumns = ICON_COLUMNS)
    {
        bool changed = false;
        int column = 0;
        EditorGUILayout.BeginHorizontal();

        // "None"
        if (!isBuiltIn)
        {
            Rect noneCellRect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE));
            bool noneSelected = _selectedPackageIcon == null && _selectedBuiltInIconName == null;

            if (noneSelected)
            {
                Rect highlightRect = new Rect(noneCellRect.x + 1, noneCellRect.y + 1, noneCellRect.width - 2, noneCellRect.height - 2);
                EditorGUI.DrawRect(highlightRect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            }
            else if (noneCellRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(noneCellRect, new Color(1f, 1f, 1f, 0.06f));
            }

            GUI.Label(noneCellRect, new GUIContent("None", "No Icon"), EditorStyles.label);

            if (Event.current.type == EventType.MouseDown && noneCellRect.Contains(Event.current.mousePosition))
            {
                _selectedPackageIcon = null;
                _selectedBuiltInIconName = null;
                changed = true;
                Event.current.Use();
                Repaint();
            }

            GUILayout.Space(CELL_SPACING);
            column++;
        }

        if (icons == null || icons.Length == 0)
        {
            EditorGUILayout.EndHorizontal();
            if (isBuiltIn) EditorGUILayout.HelpBox(emptyMessage, MessageType.Info);
            return changed;
        }

        foreach (Texture2D icon in icons)
        {
            if (column >= maxColumns)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                column = 0;
            }

            Rect cellRect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE));

            bool isSelected = icon == _selectedPackageIcon;
            if (isSelected)
            {
                Rect highlightRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.width - 2, cellRect.height - 2);
                EditorGUI.DrawRect(highlightRect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            }
            else if (cellRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 1f, 0.06f));
            }

            Rect iconRect = new Rect(cellRect.x + 4, cellRect.y + 4, cellRect.width - 8, cellRect.height - 8);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.Label(cellRect, new GUIContent(string.Empty, icon.name));

            if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
            {
                _selectedPackageIcon = icon;
                _selectedBuiltInIconName = isBuiltIn ? icon.name : null;
                changed = true;
                Event.current.Use();
                Repaint();
            }

            GUILayout.Space(CELL_SPACING);
            column++;
        }

        EditorGUILayout.EndHorizontal();
        return changed;
    }

    private static Texture2D[] FilterIcons(Texture2D[] icons, string search)
    {
        if (icons == null) return System.Array.Empty<Texture2D>();
        if (string.IsNullOrEmpty(search)) return icons;

        return icons
            .Where(t => t.name.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();
    }

    private void ExcludeFolderFromMapping()
    {
        if (_mapping == null) return;

        if (!_mapping.excludeFolderPaths.Contains(_folderPath))
        {
            _mapping.excludeFolderPaths.Add(_folderPath);
        }

        EditorUtility.SetDirty(_mapping);
        AssetDatabase.SaveAssets();

        FolderIconLibrary.Rebuild();
        Close();
    }

    private void ApplyAndSave()
    {
        if (_mapping == null && _selectedPackageIcon == null && _selectedBuiltInIconName == null && _folderColor == Color.white)
        {
            return;
        }

        if (_mapping == null)
        {
            _mapping = CreateInstance<FolderIconMapping>();
            string targetFolder = FolderIcon.EnsureSettingsFolderExists();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{_folderName} Mapping.asset");
            AssetDatabase.CreateAsset(_mapping, assetPath);
        }

        if (_selectedBuiltInIconName != null)
        {
            _mapping.icon = null;
            _mapping.builtInIconName = _selectedBuiltInIconName;
        }
        else
        {
            _mapping.icon = _selectedPackageIcon;
            _mapping.builtInIconName = null;
        }
        _mapping.folderColor = _folderColor;

        if (_usePath)
        {
            if (!_mapping.folderPaths.Contains(_folderPath))
            {
                _mapping.folderPaths.Add(_folderPath);
            }
        }
        else
        {
            if (!_mapping.folderNames.Contains(_folderName))
            {
                _mapping.folderNames.Add(_folderName);
            }
        }

        EditorUtility.SetDirty(_mapping);
        AssetDatabase.SaveAssets();

        FolderIconLibrary.Rebuild();
    }
}