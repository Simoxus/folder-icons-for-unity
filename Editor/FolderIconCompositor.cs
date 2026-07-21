using UnityEditor;
using UnityEngine;

public static class FolderIconCompositor
{
    private static readonly Rect FullSourceRect = new Rect(0f, 0f, 1f, 1f);

    private static readonly Vector2[] OutlineDirections =
    {
        new Vector2(-1f, -1f),
        new Vector2(-1f, 1f),
        new Vector2(1f, 1f),
        new Vector2(1f, -1f)
    };

    public static Texture2D Composite(FolderIconEntry entry, int size, bool isEmpty)
    {
        Texture2D baseIcon = FolderIcon.ResolveBaseIcon(isEmpty);
        if (baseIcon == null) return entry.Icon;

        // Just show only the icon
        if (FolderIconSettings.ClearerRowsEnabled && size <= 16)
        {
            return entry.Icon != null ? entry.Icon : baseIcon;
        }

        // Nothing to composite and nothing to color
        if (entry.Icon == null && !entry.HasFolderColor) return baseIcon;

        RenderTexture renderTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, Color.clear);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, size, size, 0);

        DrawBaseIcon(baseIcon, size, entry);

        if (entry.Icon != null)
        {
            DrawOverlayIcon(entry.Icon, size);
        }

        GL.PopMatrix();

        Texture2D result = CaptureToTexture(size);

        // Clean up resources
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(renderTexture);

        return result;
    }

    private static void DrawBaseIcon(Texture2D baseIcon, int size, FolderIconEntry entry)
    {
        var previousFilterMode = baseIcon.filterMode;
        baseIcon.filterMode = FilterMode.Bilinear;

        var baseRect = new Rect(0, 0, size, size);
        if (entry.HasFolderColor)
        {
            Texture2D colored = RecolorTexture(baseIcon, entry.FolderColor);
            Graphics.DrawTexture(baseRect, colored);

            // Clean up recolored texture
            if (colored != baseIcon)
            {
                Object.DestroyImmediate(colored, allowDestroyingAssets: true);
            }
        }
        else
        {
            Graphics.DrawTexture(baseRect, baseIcon);
        }

        baseIcon.filterMode = previousFilterMode;
    }

    private static Texture2D RecolorTexture(Texture2D source, Color color)
    {
        try
        {
            Color32[] sourcePixels = source.GetPixels32();
            var resultPixels = new Color32[sourcePixels.Length];
            Color32 solid = color;

            for (int i = 0; i < sourcePixels.Length; i++)
            {
                byte alpha = (byte)(sourcePixels[i].a * solid.a / 255);
                resultPixels[i] = new Color32(solid.r, solid.g, solid.b, alpha);
            }

            var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear
            };
            result.SetPixels32(resultPixels);
            result.Apply(false, false);
            return result;
        }
        catch
        {
            return source;
        }
    }

    private static void DrawOverlayIcon(Texture2D icon, int size)
    {
        int overlaySize = Mathf.Max(1, Mathf.RoundToInt(size * (size <= 16 ? FolderIconSettings.SmallOverlayScale : FolderIconSettings.LargeOverlayScale)));
        Texture2D downsampledOverlay = FolderIcon.DownsampleTexture(icon, overlaySize);

        try
        {
            DrawOverlayInCorner(downsampledOverlay, size);
        }
        finally
        {
            if (downsampledOverlay != icon)
            {
                Object.DestroyImmediate(downsampledOverlay);
            }
        }
    }

    private static void DrawOverlayInCorner(Texture2D overlay, int size)
    {
        bool isSmall = size <= 16;

        float overlaySize = overlay.width;
        float offset = size * FolderIconSettings.CornerOffset;

        float x = size - overlaySize - offset;
        float y = size - overlaySize - offset;
        var iconRect = new Rect(x, y, overlaySize, overlaySize);

        if (!isSmall)
        {
            DrawShadow(overlay, iconRect);
        }

        DrawOutline(overlay, iconRect, isSmall);

        Graphics.DrawTexture(iconRect, overlay);
    }

    private static void DrawShadow(Texture2D overlay, Rect iconRect)
    {
        var offset = new Vector2(iconRect.width * FolderIconSettings.ShadowOffset, iconRect.height * FolderIconSettings.ShadowOffset);

        for (int i = FolderIconSettings.ShadowLayers; i >= 1; i--)
        {
            float t = i / (float)FolderIconSettings.ShadowLayers;
            float spread = iconRect.width * FolderIconSettings.ShadowMaxSpread * t;
            float alpha = FolderIconSettings.ShadowMaxAlpha * (1f - t * 0.5f);

            Rect shadowRect = new Rect(
                iconRect.x + offset.x - spread * 0.5f,
                iconRect.y + offset.y - spread * 0.5f,
                iconRect.width + spread,
                iconRect.height + spread);

            Graphics.DrawTexture(shadowRect, overlay, FullSourceRect, 0, 0, 0, 0, new Color(0f, 0f, 0f, alpha));
        }
    }

    private static void DrawOutline(Texture2D overlay, Rect iconRect, bool isSmall)
    {
        Color outlineColor = EditorGUIUtility.isProSkin ? FolderIconSettings.OutlineColorDark : FolderIconSettings.OutlineColorLight;

        if (isSmall)
        {
            outlineColor.a *= FolderIconSettings.SmallOutlineAlphaMultiplier;
        }

        foreach (Vector2 direction in OutlineDirections)
        {
            Rect offsetRect = new Rect(iconRect.x + direction.x, iconRect.y + direction.y, iconRect.width, iconRect.height);
            Graphics.DrawTexture(offsetRect, overlay, FullSourceRect, 0, 0, 0, 0, outlineColor);
        }
    }

    private static Texture2D CaptureToTexture(int size)
    {
        var result = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.DontSave,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0,
            wrapMode = TextureWrapMode.Clamp
        };
        result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        result.Apply(false, false);
        return result;
    }
}