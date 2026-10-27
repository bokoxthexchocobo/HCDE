namespace HCDE.Playsim;

/// <summary>
/// Sector link, scroll, damage, and gravity specials from <c>p_lnspec.cpp</c> (**2269–2418**); chunk **928**.
/// </summary>
public static class VmPLnspecScrollSector928Slice
{
    public const int ScrollZeroId = 0;

    public static bool SectorSetLinkRequiresControlTag(int controlTag) => controlTag != 0;

    public readonly record struct SectorLinkInvoke(int ControlTag, int LinkTag, int Plane, int MoveType);

    public static SectorLinkInvoke SectorSetLink(int controlTag, int linkTag, int plane, int moveType) =>
        new(controlTag, linkTag, plane, moveType);

    public static bool SectorLinkFoundControl(int controlSectorIndex) => controlSectorIndex >= 0;

    public readonly record struct WallScrollInvoke(int Id, int SideChoice, double Dx, double Dy, int Where);

    public static (int id, int sideChoice) NormalizeTextureScrollId(int id)
    {
        if (id < 0)
            return (-id, 1);
        return (id, 0);
    }

    public static WallScrollInvoke ScrollTextureBoth(int id, int left, int right, int up, int down)
    {
        if (id == ScrollZeroId)
            return new(0, 0, 0, 0, 0);
        var (normId, side) = NormalizeTextureScrollId(id);
        var dx = (left - right) / 64.0;
        var dy = (down - up) / 64.0;
        return new(normId, side, dx, dy, 0);
    }

    public static bool ScrollTextureBothSucceeded(int id) => id != ScrollZeroId;

    public static bool ScrollWallRejectZeroId(int id) => id == ScrollZeroId;

    public static WallScrollInvoke ScrollWall(int id, int x, int y, bool side, int scrollPos) =>
        new(id, side ? 1 : 0, x / 65536.0, y / 65536.0, scrollPos);

    public readonly record struct FloorScrollInvoke(int Tag, double Dx, double Dy, int Mode);

    public static FloorScrollInvoke ScrollFloor(int tag, int xMove, int yMove, int mode)
    {
        var dx = xMove / 32.0;
        var dy = yMove / 32.0;
        return new(tag, dx, dy, mode);
    }

    public static bool ScrollFloorMovesFloor(int mode) => mode == 0 || mode == 2;

    public static bool ScrollFloorEnablesCarry(int mode) => mode > 0;

    public static (double floorDx, double floorDy, double carryDx, double carryDy) ScrollFloorDeltas(int mode, double dx, double dy) =>
        ScrollFloorMovesFloor(mode)
            ? (-dx, dy, ScrollFloorEnablesCarry(mode) ? dx : 0, ScrollFloorEnablesCarry(mode) ? dy : 0)
            : (0, 0, ScrollFloorEnablesCarry(mode) ? dx : 0, ScrollFloorEnablesCarry(mode) ? dy : 0);

    public static (double scrollerDx, double scrollerDy) ScrollFloorScroller(int mode, double dx, double dy)
    {
        var deltas = ScrollFloorDeltas(mode, dx, dy);
        return (deltas.floorDx, deltas.floorDy);
    }

    public static (double carryDx, double carryDy) ScrollFloorCarry(int mode, double dx, double dy)
    {
        var deltas = ScrollFloorDeltas(mode, dx, dy);
        return (deltas.carryDx, deltas.carryDy);
    }

    public static FloorScrollInvoke ScrollCeiling(int tag, int xMove, int yMove)
    {
        var dx = xMove / 32.0;
        var dy = yMove / 32.0;
        return new(tag, -dx, dy, 0);
    }

    public static bool PointPushAlwaysFails() => true;

    public readonly record struct SectorDamagePatch(short Amount, int DamageType, short Interval, short Leaky);

    public static SectorDamagePatch NormalizeLegacyLeakiness(int amount, int interval, int leaky)
    {
        if (interval > 0)
            return new((short)amount, 0, (short)interval, (short)leaky);
        if (amount < 20)
            return new((short)amount, 0, 32, 0);
        if (amount < 50)
            return new((short)amount, 0, 32, 5);
        return new((short)amount, 0, 1, 256);
    }

    public static SectorDamagePatch ApplyDamageMod(int amount, int mod, int interval, int leaky) =>
        NormalizeLegacyLeakiness(amount, interval, leaky) with { DamageType = mod };

    public static int ClampGravityFraction(int frac) => frac > 99 ? 99 : frac;

    public static double SectorGravity(int intPart, int fracPart)
    {
        var frac = ClampGravityFraction(fracPart);
        return intPart + frac * 0.01;
    }
}
