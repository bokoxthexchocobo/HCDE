namespace HCDE.Playsim;

/// <summary>Native line/sector texture specials from <c>p_lnspec.cpp</c> (2568–2765).</summary>
public static class VmPLnspecLineTextureLineSpecials
{
    public const int TextureNoChange = 32767 << 16;

    public readonly record struct SectorRotationInvoke(int Tag, double FloorDegrees, double CeilingDegrees);

    public static SectorRotationInvoke SectorSetRotation(int tag, int floorDeg, int ceilingDeg) =>
        new(tag, floorDeg, ceilingDeg);

    public static bool LineAlignCeiling(bool side) => side;

    public static int LineAlignPlane(bool ceiling) => ceiling ? 1 : 0;

    public static bool LineAlignFloor(bool side) => side;

    public static bool LineTextureReject(int lineId, int side) =>
        lineId == 0 || side < 0 || side > 1;

    public static double TextureArgFrom65536(int raw) => raw / 65536.0;

    public static bool TextureOffsetIsSetMode(int flags) => (flags & 8) == 0;

    public static bool TextureAffectsTop(int flags) => (flags & 1) != 0;

    public static bool TextureAffectsMid(int flags) => (flags & 2) != 0;

    public static bool TextureAffectsBottom(int flags) => (flags & 4) != 0;

    public static bool ShouldApplyTextureX(int raw, int flags) =>
        raw != TextureNoChange && (TextureAffectsTop(flags) || TextureAffectsMid(flags) || TextureAffectsBottom(flags));

    public static bool ShouldApplyTextureY(int raw, int flags) => ShouldApplyTextureX(raw, flags);

    public readonly record struct TextureOffsetInvoke(
        int LineId,
        int Side,
        double X,
        double Y,
        int Flags,
        bool SetMode);

    public static TextureOffsetInvoke BuildTextureOffset(int lineId, int x, int y, int side, int flags) =>
        new(lineId, side, TextureArgFrom65536(x), TextureArgFrom65536(y), flags, TextureOffsetIsSetMode(flags));

    public static TextureOffsetInvoke BuildTextureScale(int lineId, int x, int y, int side, int flags) =>
        BuildTextureOffset(lineId, x, y, side, flags);

    public static bool BlockingRejectZeroId(int lineId) => lineId == 0;

    public readonly record struct BlockingFlagEntry(int FieldIndex, int Bitmask);

    public static BlockingFlagEntry[] BlockingFlagTable() =>
    [
        new(0, 1),
        new(0, 2),
        new(0, 4),
        new(0, 8),
        new(0, 16),
        new(0, 32),
        new(0, 64),
        new(0, 128),
        new(0, 256),
        new(0, 512),
        new(0, 1024),
        new(1, 1),
    ];

    public static (int[] setFlags, int[] clearFlags) DecodeBlockingArgs(int setRaw, int clearRaw)
    {
        var set = new int[2];
        var clear = new int[2];
        var setArg = setRaw;
        var clearArg = clearRaw;
        foreach (var entry in BlockingFlagTable())
        {
            if ((setArg & 1) != 0)
                set[entry.FieldIndex] |= entry.Bitmask;
            if ((clearArg & 1) != 0)
                clear[entry.FieldIndex] |= entry.Bitmask;
            setArg >>= 1;
            clearArg >>= 1;
        }
        return (set, clear);
    }

    public static (int flags, int flags2) ApplyBlocking(int currentFlags, int currentFlags2, int[] set, int[] clear) =>
        ((currentFlags & ~clear[0]) | set[0], (currentFlags2 & ~clear[1]) | set[1]);

    public static bool CombineLineAlignResults(bool accumulated, bool next) => accumulated | next;
}
