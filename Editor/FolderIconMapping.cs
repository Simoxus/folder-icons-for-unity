using System.Collections.Generic;
using UnityEngine;

public class FolderIconMapping : ScriptableObject
{
    public Texture2D icon;
    public string builtInIconName;

    public Color folderColor = Color.white;

    public List<string> folderNames = new List<string>();
    public List<string> folderPaths = new List<string>();

    public Texture2D ResolvedIcon => icon != null ? icon : FolderIcon.ResolveBuiltInIcon(builtInIconName);

    private void OnValidate()
    {
        FolderIconLibrary.Rebuild();
    }
}