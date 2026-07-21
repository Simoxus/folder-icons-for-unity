using UnityEditor;
using UnityEngine;

public static class FolderIconSettings
{
    private const string PREFIX = "FolderIcon.";

    // Behavior
    private const string ENABLED_KEY = PREFIX + "Enabled";
    private const string MAX_MATCH_DEPTH_KEY = PREFIX + "MaxMatchDepth";

    // Row Decorations
    private const string CLEARER_ROWS_ENABLED_KEY = PREFIX + "ClearerRowsEnabled";
    private const string CLEARER_ROWS_BACKGROUND_LIGHT_KEY = PREFIX + "ClearerRowsBackgroundLight";
    private const string CLEARER_ROWS_BACKGROUND_DARK_KEY = PREFIX + "ClearerRowsBackgroundDark";
    private const string ZEBRA_STRIPING_ENABLED_KEY = PREFIX + "ZebraStripingEnabled";
    private const string ZEBRA_STRIPE_ALPHA_KEY = PREFIX + "ZebraStripeAlpha";
    private const string HIERARCHY_LINES_ENABLED_KEY = PREFIX + "HierarchyLinesEnabled";
    private const string HIERARCHY_LINE_COLOR_LIGHT_KEY = PREFIX + "HierarchyLineColorLight";
    private const string HIERARCHY_LINE_COLOR_DARK_KEY = PREFIX + "HierarchyLineColorDark";

    // Sizing
    private const string SMALL_OVERLAY_SCALE_KEY = PREFIX + "SmallOverlayScale";
    private const string LARGE_OVERLAY_SCALE_KEY = PREFIX + "LargeOverlayScale";
    private const string CORNER_OFFSET_KEY = PREFIX + "CornerOffset";

    // Shadow
    private const string SHADOW_LAYERS_KEY = PREFIX + "ShadowLayers";
    private const string SHADOW_MAX_SPREAD_KEY = PREFIX + "ShadowMaxSpread";
    private const string SHADOW_MAX_ALPHA_KEY = PREFIX + "ShadowMaxAlpha";
    private const string SHADOW_OFFSET_KEY = PREFIX + "ShadowOffset";

    // Outline
    private const string SMALL_OUTLINE_ALPHA_MULTIPLIER_KEY = PREFIX + "SmallOutlineAlphaMultiplier";
    private const string OUTLINE_COLOR_LIGHT_KEY = PREFIX + "OutlineColorLight";
    private const string OUTLINE_COLOR_DARK_KEY = PREFIX + "OutlineColorDark";

    // Render
    private const string SIZE_STEP_KEY = PREFIX + "SizeStep";
    private const string MIN_LARGE_SIZE_KEY = PREFIX + "MinLargeSize";
    private const string MAX_LARGE_SIZE_KEY = PREFIX + "MaxLargeSize";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(ENABLED_KEY, true);
        set => EditorPrefs.SetBool(ENABLED_KEY, value);
    }

    public static int MaxMatchDepth
    {
        get => EditorPrefs.GetInt(MAX_MATCH_DEPTH_KEY, 0);
        set => EditorPrefs.SetInt(MAX_MATCH_DEPTH_KEY, value);
    }

    public static bool ClearerRowsEnabled
    {
        get => EditorPrefs.GetBool(CLEARER_ROWS_ENABLED_KEY, false);
        set => EditorPrefs.SetBool(CLEARER_ROWS_ENABLED_KEY, value);
    }

    public static Color32 ClearerRowsBackgroundLight
    {
        get => GetColor32(CLEARER_ROWS_BACKGROUND_LIGHT_KEY, new Color32(201, 201, 201, 255));
        set => SetColor32(CLEARER_ROWS_BACKGROUND_LIGHT_KEY, value);
    }

    public static Color32 ClearerRowsBackgroundDark
    {
        get => GetColor32(CLEARER_ROWS_BACKGROUND_DARK_KEY, new Color32(56, 56, 56, 255));
        set => SetColor32(CLEARER_ROWS_BACKGROUND_DARK_KEY, value);
    }

    public static bool ZebraStripingEnabled
    {
        get => EditorPrefs.GetBool(ZEBRA_STRIPING_ENABLED_KEY, false);
        set => EditorPrefs.SetBool(ZEBRA_STRIPING_ENABLED_KEY, value);
    }

    public static float ZebraStripeAlpha
    {
        get => EditorPrefs.GetFloat(ZEBRA_STRIPE_ALPHA_KEY, 0.08f);
        set => EditorPrefs.SetFloat(ZEBRA_STRIPE_ALPHA_KEY, value);
    }

    public static bool HierarchyLinesEnabled
    {
        get => EditorPrefs.GetBool(HIERARCHY_LINES_ENABLED_KEY, true);
        set => EditorPrefs.SetBool(HIERARCHY_LINES_ENABLED_KEY, value);
    }

    public static Color32 HierarchyLineColorLight
    {
        get => GetColor32(HIERARCHY_LINE_COLOR_LIGHT_KEY, new Color32(0, 0, 0, 40));
        set => SetColor32(HIERARCHY_LINE_COLOR_LIGHT_KEY, value);
    }

    public static Color32 HierarchyLineColorDark
    {
        get => GetColor32(HIERARCHY_LINE_COLOR_DARK_KEY, new Color32(255, 255, 255, 30));
        set => SetColor32(HIERARCHY_LINE_COLOR_DARK_KEY, value);
    }

    public static float SmallOverlayScale
    {
        get => EditorPrefs.GetFloat(SMALL_OVERLAY_SCALE_KEY, 0.534f);
        set => EditorPrefs.SetFloat(SMALL_OVERLAY_SCALE_KEY, value);
    }

    public static float LargeOverlayScale
    {
        get => EditorPrefs.GetFloat(LARGE_OVERLAY_SCALE_KEY, 0.4f);
        set => EditorPrefs.SetFloat(LARGE_OVERLAY_SCALE_KEY, value);
    }

    public static float CornerOffset
    {
        get => EditorPrefs.GetFloat(CORNER_OFFSET_KEY, 0.05f);
        set => EditorPrefs.SetFloat(CORNER_OFFSET_KEY, value);
    }

    public static int ShadowLayers
    {
        get => EditorPrefs.GetInt(SHADOW_LAYERS_KEY, 5);
        set => EditorPrefs.SetInt(SHADOW_LAYERS_KEY, value);
    }

    public static float ShadowMaxSpread
    {
        get => EditorPrefs.GetFloat(SHADOW_MAX_SPREAD_KEY, 0.2547f);
        set => EditorPrefs.SetFloat(SHADOW_MAX_SPREAD_KEY, value);
    }

    public static float ShadowMaxAlpha
    {
        get => EditorPrefs.GetFloat(SHADOW_MAX_ALPHA_KEY, 0.069f);
        set => EditorPrefs.SetFloat(SHADOW_MAX_ALPHA_KEY, value);
    }

    public static float ShadowOffset
    {
        get => EditorPrefs.GetFloat(SHADOW_OFFSET_KEY, 0.0154f);
        set => EditorPrefs.SetFloat(SHADOW_OFFSET_KEY, value);
    }

    public static float SmallOutlineAlphaMultiplier
    {
        get => EditorPrefs.GetFloat(SMALL_OUTLINE_ALPHA_MULTIPLIER_KEY, 1f);
        set => EditorPrefs.SetFloat(SMALL_OUTLINE_ALPHA_MULTIPLIER_KEY, value);
    }

    public static Color32 OutlineColorLight
    {
        get => GetColor32(OUTLINE_COLOR_LIGHT_KEY, new Color32(191, 191, 191, 14));
        set => SetColor32(OUTLINE_COLOR_LIGHT_KEY, value);
    }

    public static Color32 OutlineColorDark
    {
        get => GetColor32(OUTLINE_COLOR_DARK_KEY, new Color32(0, 0, 0, 75));
        set => SetColor32(OUTLINE_COLOR_DARK_KEY, value);
    }

    public static int SizeStep
    {
        get => EditorPrefs.GetInt(SIZE_STEP_KEY, 16);
        set => EditorPrefs.SetInt(SIZE_STEP_KEY, value);
    }

    public static int MinLargeSize
    {
        get => EditorPrefs.GetInt(MIN_LARGE_SIZE_KEY, 64);
        set => EditorPrefs.SetInt(MIN_LARGE_SIZE_KEY, value);
    }

    public static int MaxLargeSize
    {
        get => EditorPrefs.GetInt(MAX_LARGE_SIZE_KEY, 256);
        set => EditorPrefs.SetInt(MAX_LARGE_SIZE_KEY, value);
    }

    private static Color32 GetColor32(string key, Color32 defaultValue)
    {
        return UnpackColor32(EditorPrefs.GetInt(key, PackColor32(defaultValue)));
    }

    private static void SetColor32(string key, Color32 value)
    {
        EditorPrefs.SetInt(key, PackColor32(value));
    }

    private static int PackColor32(Color32 c)
    {
        return (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
    }

    private static Color32 UnpackColor32(int packed)
    {
        byte r = (byte)((packed >> 24) & 0xFF);
        byte g = (byte)((packed >> 16) & 0xFF);
        byte b = (byte)((packed >> 8) & 0xFF);
        byte a = (byte)(packed & 0xFF);
        return new Color32(r, g, b, a);
    }
}