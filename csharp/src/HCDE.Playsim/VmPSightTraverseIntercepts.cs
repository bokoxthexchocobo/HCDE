namespace HCDE.Playsim;

/// <summary><c>SightCheck::P_SightTraverseIntercepts</c> from <c>p_sight.cpp</c> (534–605).</summary>
public static class VmPSightTraverseIntercepts
{
    public sealed class SightIntercept
    {
        public bool IsLine { get; set; }
        public double Frac { get; set; } = double.MaxValue;
        public object? Line { get; set; }
    }

    public sealed class SightTraverseInterceptsContext
    {
        public double StartFrac { get; set; }
        public double TraceX { get; set; }
        public double TraceY { get; set; }
        public double TraceDx { get; set; }
        public double TraceDy { get; set; }
        public double SightStartZ { get; set; }
        public double TopSlope { get; set; }
        public double BottomSlope { get; set; }
        public double LastZTop { get; set; }
        public double LastZBottom { get; set; }
        public bool MySeethrough { get; set; }
        public int Flags { get; set; }
        public bool TargetInLastSector { get; set; }
        public IReadOnlyList<SightFFloorCrossing> TargetSectorFFloors { get; set; } = Array.Empty<SightFFloorCrossing>();
        public List<SightIntercept> Intercepts { get; } = new();

        public Func<SightIntercept, double>? InterceptFracFromLine { get; init; }
        public Func<SightIntercept, bool>? PtrSightTraverse { get; init; }
    }

    public readonly record struct SightFFloorCrossing(
        bool Exists,
        int Flags,
        double BottomZ,
        double TopZ);

    public const int SfIgnoreWaterBoundary = 1;
    public const int FfSeethrough = 1 << 1;
    public const int FfSolid = 1 << 2;
    public const int FfExists = 1;

    public static bool SightTraverseIntercepts(SightTraverseInterceptsContext ctx)
    {
        var count = ctx.Intercepts.Count;
        for (var scanpos = 0; scanpos < ctx.Intercepts.Count; scanpos++)
        {
            var scan = ctx.Intercepts[scanpos];
            if (!scan.IsLine) continue;
            scan.Frac = ctx.InterceptFracFromLine?.Invoke(scan) ?? double.MaxValue;
            if (scan.Frac < ctx.StartFrac)
            {
                scan.Frac = double.MaxValue;
                count--;
            }
        }

        SightIntercept? inIntercept = null;
        while (count-- > 0)
        {
            var dist = double.MaxValue;
            foreach (var scan in ctx.Intercepts)
            {
                if (scan.Frac < dist)
                {
                    dist = scan.Frac;
                    inIntercept = scan;
                }
            }

            if (inIntercept is null) continue;
            if (ctx.PtrSightTraverse?.Invoke(inIntercept) != true)
                return false;
            inIntercept.Frac = double.MaxValue;
        }

        if (!ctx.TargetInLastSector || ctx.TargetSectorFFloors.Count == 0)
            return true;

        var topz = ctx.TopSlope + ctx.SightStartZ;
        var bottomz = ctx.BottomSlope + ctx.SightStartZ;
        foreach (var rover in ctx.TargetSectorFFloors)
        {
            if ((rover.Flags & FfSeethrough) == (ctx.MySeethrough ? FfSeethrough : 0) || (rover.Flags & FfExists) == 0)
                continue;
            if ((ctx.Flags & SfIgnoreWaterBoundary) != 0 && (rover.Flags & FfSolid) == 0)
                continue;

            if (ctx.LastZTop <= rover.BottomZ && topz > rover.BottomZ && ctx.LastZBottom <= rover.BottomZ && bottomz > rover.BottomZ)
                return false;
            if (ctx.LastZBottom >= rover.TopZ && bottomz < rover.TopZ && ctx.LastZTop >= rover.TopZ && topz < rover.TopZ)
                return false;
        }

        return true;
    }
}
