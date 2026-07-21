using UnityEngine;

// What FolderIconLibrary hands to the compositor for a matched folder
public readonly struct FolderIconEntry
{
    public readonly Texture2D Icon;
    public readonly Color FolderColor;

    public bool HasFolderColor => FolderColor != Color.white;

    public FolderIconEntry(Texture2D icon, Color folderColor = default)
    {
        Icon = icon;
        FolderColor = folderColor == default ? Color.white : folderColor;
    }
}