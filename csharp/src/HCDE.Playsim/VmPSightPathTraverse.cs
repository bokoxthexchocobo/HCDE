namespace HCDE.Playsim;

/// <summary><c>SightCheck::P_SightPathTraverse</c> from <c>p_sight.cpp</c> (619–825).</summary>
public static class VmPSightPathTraverse
{
    public const int SfIgnoreWaterBoundary = 1;
    public const int PortalDirFloor = 0;
    public const int PortalDirCeiling = 1;
    public const int FfExists = 1;
    public const int FfSeethrough = 1 << 1;
    public const int FfSolid = 1 << 2;

    public sealed class SightPathTraverseContext
    {
        public double SightStartX { get; set; }
        public double SightStartY { get; set; }
        public double SightStartZ { get; set; }
        public double SightEndX { get; set; }
        public double SightEndY { get; set; }
        public double TraceDx { get; set; }
        public double TraceDy { get; set; }
        public double StartFrac { get; set; }
        public double TopSlope { get; set; }
        public double BottomSlope { get; set; }
        public int PortalDir { get; set; } = -1;
        public int Flags { get; set; }
        public bool MySeethrough { get; set; }
        public int PortalGroup { get; set; }
        public int SeeingThingPortalGroup { get; set; }
        public double BmapOrgX { get; set; }
        public double BmapOrgY { get; set; }
        public int ValidCount { get; set; }
        public List<SightPortalTask> Portals { get; } = new();
        public IReadOnlyList<SightFFloorBand> StartSectorFFloors { get; set; } = Array.Empty<SightFFloorBand>();
        public bool CeilingPortalOpen { get; set; }
        public bool FloorPortalOpen { get; set; }
        public int OppositePortalGroupCeiling { get; set; }
        public int OppositePortalGroupFloor { get; set; }

        public Func<int, int, bool>? IsValidBlock { get; init; }
        public Func<int, int, int>? SightBlockLinesIterator { get; init; }
        public Func<bool>? SightTraverseIntercepts { get; init; }
    }

    public readonly record struct SightPortalTask(
        double Frac,
        double TopSlope,
        double BottomSlope,
        int PortalDir,
        int PortalGroup);

    public readonly record struct SightFFloorBand(
        bool Exists,
        int Flags,
        double BottomZAtStart,
        double TopZAtStart);

    public static bool SightPathTraverse(SightPathTraverseContext ctx)
    {
        ctx.ValidCount++;
        var x1 = ctx.SightStartX + ctx.StartFrac * ctx.TraceDx;
        var y1 = ctx.SightStartY + ctx.StartFrac * ctx.TraceDy;
        var x2 = ctx.SightEndX;
        var y2 = ctx.SightEndY;

        var checkFloor = true;
        var checkCeiling = true;
        foreach (var rover in ctx.StartSectorFFloors)
        {
            if (!rover.Exists) continue;
            if ((ctx.Flags & SfIgnoreWaterBoundary) != 0 && (rover.Flags & FfSolid) == 0) continue;

            if (ctx.SightStartZ < rover.TopZAtStart) checkCeiling = false;
            if (ctx.SightStartZ >= rover.BottomZAtStart) checkFloor = false;

            if (ctx.SightStartZ < rover.TopZAtStart && ctx.SightStartZ >= rover.BottomZAtStart)
            {
                ctx.MySeethrough = (rover.Flags & FfSeethrough) != 0;
                break;
            }
        }

        if (ctx.PortalDir != PortalDirFloor && checkCeiling && ctx.CeilingPortalOpen)
        {
            ctx.Portals.Add(new SightPortalTask(0, ctx.TopSlope, ctx.BottomSlope, PortalDirCeiling, ctx.OppositePortalGroupCeiling));
        }

        if (ctx.PortalDir != PortalDirCeiling && checkFloor && ctx.FloorPortalOpen)
        {
            ctx.Portals.Add(new SightPortalTask(0, ctx.TopSlope, ctx.BottomSlope, PortalDirFloor, ctx.OppositePortalGroupFloor));
        }

        x1 -= ctx.BmapOrgX;
        y1 -= ctx.BmapOrgY;
        var xt1 = x1 / MapBlockUnits;
        var yt1 = y1 / MapBlockUnits;

        x2 -= ctx.BmapOrgX;
        y2 -= ctx.BmapOrgY;
        var xt2 = x2 / MapBlockUnits;
        var yt2 = y2 / MapBlockUnits;

        var mapx = RoundDown(xt1);
        var mapy = RoundDown(yt1);
        var mapex = RoundDown(xt2);
        var mapey = RoundDown(yt2);

        int mapxstep, mapystep;
        double xstep, ystep, partialx, partialy, xintercept, yintercept;

        if (mapex > mapx)
        {
            mapxstep = 1;
            partialx = 1.0 - xt1 + RoundDown(xt1);
            ystep = (y2 - y1) / Math.Abs(x2 - x1);
        }
        else if (mapex < mapx)
        {
            mapxstep = -1;
            partialx = xt1 - RoundDown(xt1);
            ystep = (y2 - y1) / Math.Abs(x2 - x1);
        }
        else
        {
            mapxstep = 0;
            partialx = 1.0;
            ystep = 256;
        }

        yintercept = yt1 + partialx * ystep;

        if (mapey > mapy)
        {
            mapystep = 1;
            partialy = 1.0 - yt1 + RoundDown(yt1);
            xstep = (x2 - x1) / Math.Abs(y2 - y1);
        }
        else if (mapey < mapy)
        {
            mapystep = -1;
            partialy = yt1 - RoundDown(yt1);
            xstep = (x2 - x1) / Math.Abs(y2 - y1);
        }
        else
        {
            mapystep = 0;
            partialy = 1.0;
            xstep = 256;
        }

        xintercept = xt1 + partialy * xstep;

        if (Math.Abs(xstep) == 1.0 && Math.Abs(ystep) == 1.0)
        {
            if (ystep < 0) partialx = 1.0 - partialx;
            if (xstep < 0) partialy = 1.0 - partialy;
            if (partialx == partialy)
            {
                xintercept = xt1;
                yintercept = yt1;
            }
        }

        var itres = -1;
        for (var count = 0; count < 1000; count++)
        {
            if (ctx.IsValidBlock?.Invoke(mapx, mapy) != true)
                break;

            itres = ctx.SightBlockLinesIterator?.Invoke(mapx, mapy) ?? -1;
            if (itres == 0)
                return false;

            if (itres == -1 || (mapxstep | mapystep) == 0)
                break;

            switch ((RoundDown(yintercept) == mapy ? 2 : 0) | (RoundDown(xintercept) == mapx ? 1 : 0))
            {
                case 0:
                    return false;
                case 1:
                    xintercept += xstep;
                    mapy += mapystep;
                    if (mapy == mapey) mapystep = 0;
                    break;
                case 2:
                    yintercept += ystep;
                    mapx += mapxstep;
                    if (mapx == mapex) mapxstep = 0;
                    break;
                case 3:
                    if (ctx.SightBlockLinesIterator?.Invoke(mapx + mapxstep, mapy) == 0
                        || ctx.SightBlockLinesIterator?.Invoke(mapx, mapy + mapystep) == 0)
                    {
                        return false;
                    }

                    xintercept += xstep;
                    yintercept += ystep;
                    mapx += mapxstep;
                    mapy += mapystep;
                    if (mapx == mapex) mapxstep = 0;
                    if (mapy == mapey) mapystep = 0;
                    break;
            }
        }

        var traverseRes = ctx.SightTraverseIntercepts?.Invoke() == true;
        if (itres == -1) return false;
        if (ctx.PortalGroup != ctx.SeeingThingPortalGroup) return false;
        return traverseRes;
    }

    public const double MapBlockUnits = 128.0;

    public static int RoundDown(double v) => (int)Math.Floor(v);
}
