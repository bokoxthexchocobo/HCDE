namespace HCDE.Client;

/// <summary>
/// Wall colors after the portal branch, the 2D automap rotation, and glyph-point scale
/// from <c>am_map.cpp</c> (<b>2701–2850</b>). Chunk <b>1139</b>.
/// </summary>
public static class VmAmMapWallColor1139Slice
{
    public const int LineSecret = 0x20;
    public const int DontDraw = 0x80;
    public const int Revealed = 0x20000000;

    public enum WallColor
    {
        None,
        SecretSector,
        UnexploredSecret,
        SecretWall,
        Wall,
        IntraTeleport,
        InterTeleport,
        LockFromMap,
        Locked,
        Trigger,
        OneSided,
        FloorDiff,
        CeilingDiff,
        ExtraFloor,
        TwoSided,
        NotSeen
    }

    public readonly record struct WallPaint(WallColor Color, byte R, byte G, byte B);

    public readonly record struct Spin(double Sin, double Cos);

    public static WallPaint ChooseWall(
        bool mapped,
        int secretResult,
        int lineFlags,
        int cheat,
        bool hasBack,
        bool teleportBoundary,
        bool intraColorValid,
        bool exitBoundary,
        bool interColorValid,
        bool lockBoundary,
        bool displayLocks,
        int lockMapColor,
        int showTriggerLines,
        bool specialColorValid,
        bool triggerBoundary,
        bool floorsDiffer,
        bool ceilingsDiffer,
        bool extraFloor,
        bool allMap,
        bool showAllEnabled,
        bool cheatModeBlocks)
    {
        if (mapped)
        {
            if (secretResult == 1)
                return Plain(WallColor.SecretSector);
            if (secretResult == 2)
                return Plain(WallColor.UnexploredSecret);
            if ((lineFlags & LineSecret) != 0)
            {
                if (cheat != 0 && cheat < 4 && hasBack)
                    return Plain(WallColor.SecretWall);
                return Plain(WallColor.Wall);
            }
            if (teleportBoundary && intraColorValid)
                return Plain(WallColor.IntraTeleport);
            if (exitBoundary && interColorValid)
                return Plain(WallColor.InterTeleport);
            if (lockBoundary)
                return LockPaint(displayLocks, lockMapColor);
            if (showTriggerLines != 0 && specialColorValid && triggerBoundary)
                return Plain(WallColor.Trigger);
            if (!hasBack)
                return Plain(WallColor.OneSided);
            if (floorsDiffer)
                return Plain(WallColor.FloorDiff);
            if (ceilingsDiffer)
                return Plain(WallColor.CeilingDiff);
            if (extraFloor)
                return Plain(WallColor.ExtraFloor);
            if (cheat > 0 && cheat < 4)
                return Plain(WallColor.TwoSided);
            return Plain(WallColor.None);
        }

        if (allMap || (lineFlags & Revealed) != 0)
        {
            if (SkipUnseen(lineFlags, cheat, showAllEnabled, cheatModeBlocks))
                return Plain(WallColor.None);
            return Plain(WallColor.NotSeen);
        }
        return Plain(WallColor.None);
    }

    public static Spin UpdateSpin(bool angleChanged, Spin saved, double nextSin, double nextCos)
    {
        if (angleChanged)
            return new Spin(nextSin, nextCos);
        return saved;
    }

    public static (double X, double Y) ApplySpin(double x, double y, Spin spin)
    {
        var nextX = (x * spin.Cos) - (y * spin.Sin);
        var nextY = (x * spin.Sin) + (y * spin.Cos);
        return (nextX, nextY);
    }

    public static (double X, double Y) RotatePoint(
        double x, double y, double mapX, double mapY, double mapW, double mapH, Spin spin)
    {
        var pivotX = mapX + mapW / 2.0;
        var pivotY = mapY + mapH / 2.0;
        var spun = ApplySpin(x - pivotX, y - pivotY, spin);
        return (spun.X + pivotX, spun.Y + pivotY);
    }

    public static (double X, double Y) ScaleGlyphPoint(double x, double y, double scale)
    {
        if (scale != 0)
            return (x * scale, y * scale);
        return (x, y);
    }

    private static WallPaint LockPaint(bool displayLocks, int lockMapColor)
    {
        if (displayLocks && lockMapColor >= 0)
        {
            return new WallPaint(
                WallColor.LockFromMap,
                (byte)((lockMapColor >> 16) & 0xff),
                (byte)((lockMapColor >> 8) & 0xff),
                (byte)(lockMapColor & 0xff));
        }
        return Plain(WallColor.Locked);
    }

    private static bool SkipUnseen(int lineFlags, int cheat, bool showAllEnabled, bool cheatModeBlocks)
    {
        if ((lineFlags & DontDraw) != 0 && (cheat == 0 || cheat >= 4))
        {
            if (!showAllEnabled || cheatModeBlocks)
                return true;
        }
        return false;
    }

    private static WallPaint Plain(WallColor color)
    {
        return new WallPaint(color, 0, 0, 0);
    }
}
