namespace HCDE.Client;

/// <summary>
/// Trigger and lock probes, plus the automap wall pass through the Boom secret color,
/// from <c>am_map.cpp</c> (<b>2551–2700</b>). Chunk <b>1138</b>.
/// </summary>
public static class VmAmMapWallPass1138Slice
{
    public const int ExitSecret = 244;
    public const int DoorClose = 10;
    public const int DoorOpen = 11;
    public const int DoorRaise = 12;
    public const int DoorLockedRaise = 13;
    public const int DoorAnimated = 14;
    public const int AcsLockedExecute = 83;
    public const int AcsLockedExecuteDoor = 85;
    public const int FsExecute = 158;
    public const int GenericDoor = 202;
    public const int DoorCloseWaitOpen = 249;
    public const int PolyObjectWall = 64;
    public const int DontDraw = 0x80;
    public const int Mapped = 0x100;
    public const int AutomapStyleCount = 14;

    public readonly record struct SpecialInfo(bool Known, int MaxArgs);

    public enum WallChoice
    {
        Hidden,
        DontDraw,
        Style,
        Portal,
        SecretSector,
        Continue
    }

    public static bool ExitSecretArm(int special)
    {
        if (special == ExitSecret)
            return true;
        return false;
    }

    public static bool IsShownTrigger(int showTriggerLines, int special, SpecialInfo info)
    {
        if (!info.Known || info.MaxArgs < 0)
            return false;
        if (showTriggerLines == 1 && IsPlainDoor(special))
            return false;
        return true;
    }

    public static bool IsLockSpecial(int special, int[] args)
    {
        if (special == DoorLockedRaise || special == AcsLockedExecute || special == AcsLockedExecuteDoor)
            return true;
        if (special == DoorAnimated && args[3] != 0)
            return true;
        if (special == GenericDoor && args[4] != 0)
            return true;
        if (special == FsExecute && args[2] != 0)
            return true;
        return false;
    }

    public static int LockArgument(int special, int[] args)
    {
        if (special == FsExecute)
            return args[2];
        if (special == DoorAnimated || special == DoorLockedRaise)
            return args[3];
        return args[4];
    }

    public static (bool Found, int Lock) LockBoundary(
        int lockNumber,
        int activation,
        int lineSpecial,
        int[] lineArgs,
        bool hasBackSector,
        IReadOnlyList<VmAmMapFloorBoundary1137Slice.SectorAction> front,
        IReadOnlyList<VmAmMapFloorBoundary1137Slice.SectorAction> back)
    {
        if (lockNumber != 0)
            return (true, lockNumber);
        var hit = VmAmMapFloorBoundary1137Slice.CheckSpecialBoundary(
            activation, lineSpecial, lineArgs, hasBackSector, front, back, IsLockSpecial);
        if (!hit.Found)
            return (false, 0);
        return (true, LockArgument(hit.Special, hit.Args));
    }

    public static int PortalGroupCount(bool portalOverlay, int displacementCount)
    {
        if (portalOverlay)
            return displacementCount;
        return 0;
    }

    public static List<int> PortalPasses(int groupCount, int mapPortalGroup)
    {
        var passes = new List<int>();
        for (var pass = groupCount - 1; pass >= -1; pass--)
        {
            if (pass == mapPortalGroup)
                continue;
            passes.Add(pass);
        }
        return passes;
    }

    public static bool IsPolyObjectWall(int sideFlags)
    {
        if ((sideFlags & PolyObjectWall) != 0)
            return true;
        return false;
    }

    public static int LinePortalGroup(bool polyObject, int midpointGroup, int frontGroup)
    {
        if (polyObject)
            return midpointGroup;
        return frontGroup;
    }

    public static (bool Draw, double X, double Y, bool PortalMode) WallOffset(
        int pass,
        int lineGroup,
        int mapGroup,
        bool portalOverlay,
        int portalGroupCount,
        double offsetX,
        double offsetY)
    {
        var portalMode = portalGroupCount > 0 && lineGroup != mapGroup;
        if (lineGroup == pass)
            return (true, offsetX, offsetY, portalMode);
        if (pass == -1 && (lineGroup == mapGroup || !portalOverlay))
            return (true, 0, 0, portalMode);
        return (false, 0, 0, portalMode);
    }

    public static (double Ax, double Ay, double Bx, double By) PlacedWall(
        double v1x, double v1y, double v2x, double v2y,
        double offsetX, double offsetY,
        bool rotate,
        Func<double, double, (double X, double Y)> rotatePoint)
    {
        var line = VmAmMapPolyShade1136Slice.OrientedSeg(
            v1x + offsetX, v1y + offsetY, v2x + offsetX, v2y + offsetY, rotate, rotatePoint);
        return (line.Ax, line.Ay, line.Bx, line.By);
    }

    public static bool WallIsMapped(int cheat, int lineFlags)
    {
        if (cheat != 0 || (lineFlags & Mapped) != 0)
            return true;
        return false;
    }

    public static bool SkipDontDraw(int lineFlags, int cheat, bool showAllEnabled, bool cheatModeBlocks)
    {
        if ((lineFlags & DontDraw) != 0 && (cheat == 0 || cheat >= 4))
        {
            if (!showAllEnabled || cheatModeBlocks)
                return true;
        }
        return false;
    }

    public static bool UseAutomapStyle(int style, int cheat)
    {
        if (style > 0 && style < AutomapStyleCount && (cheat == 0 || cheat >= 4))
            return true;
        return false;
    }

    public static WallChoice ChooseWall(bool mapped, bool skipDontDraw, bool useStyle, bool portalMode, int secretResult)
    {
        if (!mapped)
            return WallChoice.Hidden;
        if (skipDontDraw)
            return WallChoice.DontDraw;
        if (useStyle)
            return WallChoice.Style;
        if (portalMode)
            return WallChoice.Portal;
        if (secretResult == 1)
            return WallChoice.SecretSector;
        return WallChoice.Continue;
    }

    private static bool IsPlainDoor(int special)
    {
        if (special == DoorOpen || special == DoorClose || special == DoorCloseWaitOpen
            || special == DoorRaise || special == DoorAnimated || special == GenericDoor)
            return true;
        return false;
    }
}
