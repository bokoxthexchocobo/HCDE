namespace HCDE.Client;

/// <summary><c>GetCaps</c> from <c>d_main.cpp</c> (974–1000).</summary>
public static class VmDMainRendererCaps
{
    public const uint RffUnclippedTex = 1u << 0;
    public const uint RffTrueColor = 1u << 1;
    public const uint RffColormap = 1u << 2;
    public const uint RffFlatSprites = 1u << 3;
    public const uint RffModels = 1u << 4;
    public const uint RffSlope3dFloors = 1u << 5;
    public const uint RffTiltPitch = 1u << 6;
    public const uint RffRollSprites = 1u << 7;
    public const uint RffPolygonal = 1u << 8;
    public const uint RffMatShader = 1u << 9;
    public const uint RffPostShader = 1u << 10;
    public const uint RffBrightmap = 1u << 11;
    public const uint RffVoxels = 1u << 12;

    public const int GlTonemapPalette = 5;

    public static uint BuildSoftwareCaps(bool trueColor) =>
        RffUnclippedTex | (trueColor ? RffTrueColor : RffColormap);

    public static uint BuildHardwareCaps(bool paletteTonemap, bool drawVoxels)
    {
        var flags = RffFlatSprites | RffModels | RffSlope3dFloors | RffTiltPitch | RffRollSprites
            | RffPolygonal | RffMatShader | RffPostShader | RffBrightmap;
        if (!paletteTonemap)
            flags |= RffTrueColor;
        if (drawVoxels)
            flags |= RffVoxels;
        return flags;
    }
}
