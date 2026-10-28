namespace HCDE.Playsim;

/// <summary>
/// Sector rotation and line align/texture specials from <c>p_lnspec.cpp</c> (**2569–2718**); chunk **930**.
/// </summary>
public static class VmPLnspecRotationAlign930Slice
{
    public const int TextureNoChange = 32767 << 16;

    public readonly record struct SectorRotationInvoke(int Tag, double FloorDegrees, double CeilingDegrees);

    public static SectorRotationInvoke SectorSetRotation(int tag, int floorDeg, int ceilingDeg) =>
        new(tag, floorDeg, ceilingDeg);

    public static int LineAlignFlatPlane(bool ceiling) => ceiling ? 1 : 0;

    public static bool CombineLineAlignResults(bool accumulated, bool next) => accumulated | next;

    public static bool LineTextureReject(int lineId, int side) =>
        lineId == 0 || side < 0 || side > 1;

    public static double TextureArgFrom65536(int raw) => raw / 65536.0;

    public static bool TextureOffsetIsSetMode(int flags) => (flags & 8) == 0;

    public static bool TextureAffectsTop(int flags) => (flags & 1) != 0;

    public static bool TextureAffectsMid(int flags) => (flags & 2) != 0;

    public static bool TextureAffectsBottom(int flags) => (flags & 4) != 0;

    public static bool ShouldApplyTextureAxis(int raw, int flags) =>
        raw != TextureNoChange
        && (TextureAffectsTop(flags) || TextureAffectsMid(flags) || TextureAffectsBottom(flags));

    public readonly record struct TextureLineInvoke(
        int LineId,
        int Side,
        double X,
        double Y,
        int Flags,
        bool SetMode);

    public static TextureLineInvoke BuildTextureOffset(int lineId, int x, int y, int side, int flags) =>
        new(lineId, side, TextureArgFrom65536(x), TextureArgFrom65536(y), flags, TextureOffsetIsSetMode(flags));

    public static TextureLineInvoke BuildTextureScale(int lineId, int x, int y, int side, int flags) =>
        BuildTextureOffset(lineId, x, y, side, flags);

    public static bool UsesMultiplyScaleMode(int flags) => !TextureOffsetIsSetMode(flags);

    public static int CountAffectedTextureParts(int flags)
    {
        var count = 0;
        if (TextureAffectsTop(flags)) count++;
        if (TextureAffectsMid(flags)) count++;
        if (TextureAffectsBottom(flags)) count++;
        return count;
    }

    public static bool AlignCeilingFromSideArg(int sideArg) => sideArg != 0;

    public static bool AlignFloorFromSideArg(int sideArg) => sideArg != 0;

    public readonly record struct LineAlignInvoke(int LineId, bool UseBackSide, int FlatPlaneCode);

    public static LineAlignInvoke BuildLineAlignCeiling(int lineId, int sideArg) =>
        new(lineId, AlignCeilingFromSideArg(sideArg), LineAlignFlatPlane(true));

    public static LineAlignInvoke BuildLineAlignFloor(int lineId, int sideArg) =>
        new(lineId, AlignFloorFromSideArg(sideArg), LineAlignFlatPlane(false));

    public static void ForEachAffectedTexturePart(int flags, Action<int> visitPart)
    {
        if (TextureAffectsTop(flags))
            visitPart(1);
        if (TextureAffectsMid(flags))
            visitPart(2);
        if (TextureAffectsBottom(flags))
            visitPart(4);
    }

    public static (bool applyX, bool applyY) TextureAxisApplyMask(int rawX, int rawY, int flags)
    {
        var partsActive = CountAffectedTextureParts(flags) > 0;
        return (
            partsActive && rawX != TextureNoChange,
            partsActive && rawY != TextureNoChange);
    }
}
