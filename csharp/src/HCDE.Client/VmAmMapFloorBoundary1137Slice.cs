namespace HCDE.Client;

/// <summary>
/// Red poly-seg strokes, 3D-floor boundaries, and teleport/exit probes from <c>am_map.cpp</c>
/// (<b>2401–2550</b>). Chunk <b>1137</b>. Trigger and lock probes begin at <b>2555</b>.
/// </summary>
public static class VmAmMapFloorBoundary1137Slice
{
    public const int FloorExists = 0x1;
    public const int FloorThisInside = 0x40000000;
    public const int SecUse = 16;
    public const int SecUseWall = 32;
    public const int Friendly = 0x08000000;
    public const int PlayerActivate = 1243;
    public const int Teleport = 70;
    public const int TeleportNoFog = 71;
    public const int TeleportZombie = 39;
    public const int TeleportLine = 215;
    public const int TeleportNewMap = 74;
    public const int TeleportEndGame = 75;
    public const int ExitNormal = 243;
    public const int ExitSecret = 244;

    public readonly record struct FloorRover(int Flags, int Alpha, int Model);

    public readonly record struct SectorAction(int Health, int Special, int Flags, int[] Args);

    public readonly record struct BoundaryHit(bool Found, int Special, int[] Args);

    public static (byte R, byte G, byte B) PolySegColor()
    {
        return (255, 0, 0);
    }

    public static List<(double Ax, double Ay, double Bx, double By)> RedPolyStrokes(
        int shownSubsector,
        IReadOnlyList<(int Subsector, double Ax, double Ay, double Bx, double By)> segs,
        bool rotate,
        Func<double, double, (double X, double Y)> rotatePoint)
    {
        var drawn = new List<(double Ax, double Ay, double Bx, double By)>();
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            if (seg.Subsector != shownSubsector)
                continue;
            var line = VmAmMapPolyShade1136Slice.OrientedSeg(seg.Ax, seg.Ay, seg.Bx, seg.By, rotate, rotatePoint);
            drawn.Add((line.Ax, line.Ay, line.Bx, line.By));
        }
        return drawn;
    }

    public static bool HasFloorBoundary(IReadOnlyList<FloorRover> front, IReadOnlyList<FloorRover> back)
    {
        if (front.Count == 0 && back.Count == 0)
            return false;
        if (RealFloorCount(front) != RealFloorCount(back))
            return true;
        for (var i = 0; i < front.Count; i++)
        {
            var rover = front[i];
            if (!IsRealFloor(rover))
                continue;
            if (!BackHasMatch(back, rover))
                return true;
        }
        return false;
    }

    public static BoundaryHit FindSectorAction(
        IReadOnlyList<SectorAction> chain, bool needUseActivated, Func<int, int[], bool> matches)
    {
        for (var i = 0; i < chain.Count; i++)
        {
            var action = chain[i];
            var useOk = (action.Health & (SecUse | SecUseWall)) != 0 || !needUseActivated;
            if (useOk && matches(action.Special, action.Args) && (action.Flags & Friendly) == 0)
                return new BoundaryHit(true, action.Special, action.Args);
        }
        return new BoundaryHit(false, 0, System.Array.Empty<int>());
    }

    public static BoundaryHit CheckSpecialBoundary(
        int activation,
        int lineSpecial,
        int[] lineArgs,
        bool hasBackSector,
        IReadOnlyList<SectorAction> front,
        IReadOnlyList<SectorAction> back,
        Func<int, int[], bool> matches)
    {
        if ((activation & PlayerActivate) != 0 && matches(lineSpecial, lineArgs))
            return new BoundaryHit(true, lineSpecial, lineArgs);
        var frontHit = FindSectorAction(front, !hasBackSector, matches);
        if (frontHit.Found)
            return frontHit;
        if (hasBackSector)
            return FindSectorAction(back, false, matches);
        return new BoundaryHit(false, 0, System.Array.Empty<int>());
    }

    public static bool IsTeleportSpecial(int special, int[] args)
    {
        if (special == Teleport || special == TeleportNoFog || special == TeleportZombie || special == TeleportLine)
            return true;
        return false;
    }

    public static bool IsExitSpecial(int special, int[] args)
    {
        if (special == TeleportNewMap || special == TeleportEndGame || special == ExitNormal || special == ExitSecret)
            return true;
        return false;
    }

    public static BoundaryHit TeleportBoundary(
        int activation, int lineSpecial, int[] lineArgs, bool hasBackSector,
        IReadOnlyList<SectorAction> front, IReadOnlyList<SectorAction> back)
    {
        return CheckSpecialBoundary(activation, lineSpecial, lineArgs, hasBackSector, front, back, IsTeleportSpecial);
    }

    public static BoundaryHit ExitBoundary(
        int activation, int lineSpecial, int[] lineArgs, bool hasBackSector,
        IReadOnlyList<SectorAction> front, IReadOnlyList<SectorAction> back)
    {
        return CheckSpecialBoundary(activation, lineSpecial, lineArgs, hasBackSector, front, back, IsExitSpecial);
    }

    private static int RealFloorCount(IReadOnlyList<FloorRover> rovers)
    {
        var count = 0;
        for (var i = 0; i < rovers.Count; i++)
        {
            if (IsRealFloor(rovers[i]))
                count++;
        }
        return count;
    }

    private static bool IsRealFloor(FloorRover rover)
    {
        if ((rover.Flags & FloorThisInside) != 0)
            return false;
        if ((rover.Flags & FloorExists) == 0)
            return false;
        if (rover.Alpha == 0)
            return false;
        return true;
    }

    private static bool BackHasMatch(IReadOnlyList<FloorRover> back, FloorRover rover)
    {
        for (var j = 0; j < back.Count; j++)
        {
            var other = back[j];
            if (!IsRealFloor(other))
                continue;
            if (rover.Model == other.Model && rover.Flags == other.Flags)
                return true;
        }
        return false;
    }
}
