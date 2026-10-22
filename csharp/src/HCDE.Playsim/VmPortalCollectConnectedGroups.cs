namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CollectConnectedGroups</c> from <c>portal.cpp</c> (1049–1184).</summary>
public static class VmPortalCollectConnectedGroups
{
    public const int PgaNoSectorPortals = 1;
    public const int PgaFull3D = 2;
    public const int Upper = 1 << 16;
    public const int Lower = 1 << 17;
    public const int Flat = 1 << 18;

    public sealed class PortalGroupArray
    {
        public int Method { get; init; }
        public bool Inited { get; set; }
        public List<int> Groups { get; } = new();
        public void Add(int g) => Groups.Add(g);
    }

    public sealed class LinkedPortal
    {
        public int OriginGroup { get; init; }
        public int DestinationGroup { get; init; }
        public double BoxMinX { get; init; }
        public double BoxMaxX { get; init; }
        public double BoxMinY { get; init; }
        public double BoxMaxY { get; init; }
        public bool InRange { get; init; }
        public bool OnLineSidePass { get; init; }
    }

    public sealed class CollectContext
    {
        public int DisplacementSize { get; init; } = 1;
        public int StartGroup { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double PosZ { get; init; }
        public double UpperZ { get; init; }
        public double CheckRadius { get; init; }
        public int Method { get; init; }
        public bool HasLinkedPortals { get; init; }
        public bool HasLinkedSectorPortals { get; init; }
        public IReadOnlyList<LinkedPortal> LinkedPortals { get; init; } = Array.Empty<LinkedPortal>();
        public HashSet<int> ProcessMask { get; } = new();
        public List<LinkedPortal> FoundPortals { get; } = new();
        public Func<(double X, double Y)>? GetDisplacementOffset { get; init; }
        public Func<double, double, object?>? PointInSector { get; init; }
        public Func<object, int, bool>? PortalBlocksMovement { get; init; }
        public Func<object, int, double>? GetPortalPlaneZ { get; init; }
        public Func<object, int, int>? GetOppositePortalGroup { get; init; }
    }

    public static bool CollectConnectedGroups(CollectContext ctx, PortalGroupArray outGroups)
    {
        var retval = false;
        outGroups.Inited = true;

        if (ctx.DisplacementSize == 1)
        {
            ctx.ProcessMask.Add(ctx.StartGroup);
            return false;
        }

        if (!ctx.HasLinkedPortals)
            return WalkSectorPortals(ctx, outGroups, ref retval);

        ctx.ProcessMask.Add(ctx.StartGroup);
        foreach (var lp in ctx.LinkedPortals)
        {
            if (!lp.InRange || !lp.OnLineSidePass)
                continue;
            ctx.FoundPortals.Add(lp);
        }

        var foundone = true;
        while (foundone)
        {
            foundone = false;
            for (var i = ctx.FoundPortals.Count - 1; i >= 0; i--)
            {
                var p = ctx.FoundPortals[i];
                if (ctx.ProcessMask.Contains(p.OriginGroup) && !ctx.ProcessMask.Contains(p.DestinationGroup))
                {
                    ctx.ProcessMask.Add(p.DestinationGroup);
                    outGroups.Add(p.DestinationGroup);
                    foundone = true;
                    retval = true;
                    ctx.FoundPortals.RemoveAt(i);
                }
            }
        }

        if (ctx.Method != PgaNoSectorPortals)
            retval |= WalkSectorPortals(ctx, outGroups, ref retval);

        return retval;
    }

    private static bool WalkSectorPortals(CollectContext ctx, PortalGroupArray outGroups, ref bool retval)
    {
        var sec = ctx.PointInSector?.Invoke(ctx.PosX, ctx.PosY);
        if (sec == null)
            return retval;

        retval |= WalkVerticalPortal(ctx, outGroups, sec, sectorCeiling: true);
        retval |= WalkVerticalPortal(ctx, outGroups, sec, sectorCeiling: false);
        return retval;
    }

    private static bool WalkVerticalPortal(CollectContext ctx, PortalGroupArray outGroups, object sec, bool sectorCeiling)
    {
        var plane = sectorCeiling ? 1 : 0;
        if (ctx.PortalBlocksMovement?.Invoke(sec, plane) == true)
            return false;

        var zGate = sectorCeiling
            ? ctx.UpperZ > (ctx.GetPortalPlaneZ?.Invoke(sec, plane) ?? 0)
            : ctx.PosZ < (ctx.GetPortalPlaneZ?.Invoke(sec, plane) ?? 0);
        if (!zGate)
            return false;

        var other = ctx.GetOppositePortalGroup?.Invoke(sec, plane) ?? 0;
        if (ctx.ProcessMask.Contains(other))
            return false;

        ctx.ProcessMask.Add(other);
        outGroups.Add(other | (sectorCeiling ? Upper : Lower));
        return true;
    }

    public static bool SingleGroupDisplacement(int size) => size == 1;

    public static bool ShouldScanLinkedLinePortals(CollectContext ctx) =>
        ctx.HasLinkedPortals && ctx.DisplacementSize > 1;
}
