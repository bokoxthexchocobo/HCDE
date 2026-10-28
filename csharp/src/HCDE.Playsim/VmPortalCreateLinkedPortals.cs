namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CreateLinkedPortals</c> from <c>portal.cpp</c> (851–1039).</summary>
public static class VmPortalCreateLinkedPortals
{
    public const int LinkedSectorPortalType = 1;
    public const int LinkedLinePortalType = 2;
    public const int FloorPlane = 0;
    public const int CeilingPlane = 1;

    public sealed class CreateLinkedPortalsContext
    {
        public IList<LinkedPortalOrg> LinkedOrgs { get; set; } = Array.Empty<LinkedPortalOrg>();
        public IList<PortalSector> Sectors { get; set; } = Array.Empty<PortalSector>();
        public IList<PortalLine> LinePortals { get; set; } = Array.Empty<PortalLine>();
        public int NextGroupId { get; set; } = 1;
        public bool Bogus { get; set; }
        public bool RejectMatrixReset { get; set; }

        public Action<int, bool, int>? ClearSlopedPortal { get; init; }
        public Func<int, int, bool>? CollectSectors { get; init; }
        public Action<int>? CreateDisplacements { get; init; }
        public Action<int>? AddDisplacementForSectorPortal { get; init; }
        public Action<int>? AddDisplacementForLinePortal { get; init; }
        public Func<int, int, (bool Set, double X, double Y)>? GetDisplacement { get; init; }
        public Action<int, int>? ReportMismatch { get; init; }
        public Action<int>? DisableUnconnectedLinePortal { get; init; }
        public Action<int, int>? CollectOrphanOneSidedLine { get; init; }
        public Action<int>? ReportUnassignedSector { get; init; }
        public Action<int>? NormalizeNegativePortalGroup { get; init; }
        public Func<bool>? ConnectPortalGroups { get; init; }
        public Action<int, bool>? MarkPortalPlaneLinked { get; init; }
        public Action<int>? MarkSectorLinesPortalConnect { get; init; }
    }

    public readonly record struct LinkedPortalOrg(int OriginSector, int DestinationSector);

    public readonly record struct PortalSector(
        int Index,
        int FloorPortalType,
        int CeilingPortalType,
        bool FloorSlope,
        bool CeilingSlope,
        int PortalGroup,
        int LinkedPortalId,
        bool FloorPortalLinked,
        bool CeilingPortalLinked,
        double CeilingPortalZ,
        double FloorPortalZ,
        double CeilingPlaneD,
        double FloorPlaneD);

    public readonly record struct PortalLine(int Index, int Type, bool DestinationMissing, int OriginLineIndex);

    public static int PortalTypeForPlane(PortalSector sec, int plane) =>
        plane == FloorPlane ? sec.FloorPortalType : sec.CeilingPortalType;

    public static bool PlaneIsSloped(PortalSector sec, int plane) =>
        plane == FloorPlane ? sec.FloorSlope : sec.CeilingSlope;

    public static void ClearSlopedLinkedPortals(CreateLinkedPortalsContext ctx)
    {
        for (var s = 0; s < ctx.Sectors.Count; s++)
        {
            var sec = ctx.Sectors[s];
            for (var plane = 0; plane < 2; plane++)
            {
                if (PortalTypeForPlane(sec, plane) != LinkedSectorPortalType)
                    continue;
                if (!PlaneIsSloped(sec, plane))
                    continue;
                ctx.ClearSlopedPortal?.Invoke(s, plane == FloorPlane, s);
            }
        }
    }

    public static int CollectFromLinkedOrgs(CreateLinkedPortalsContext ctx, int startId)
    {
        var id = startId;
        for (var i = 0; i < ctx.LinkedOrgs.Count; i++)
        {
            var org = ctx.LinkedOrgs[i];
            if (ctx.CollectSectors?.Invoke(id, org.OriginSector) == true)
                id++;
            if (ctx.CollectSectors?.Invoke(id, org.DestinationSector) == true)
                id++;
        }

        return id;
    }

    public static int ProcessLineLinkedPortals(CreateLinkedPortalsContext ctx, int startId)
    {
        var id = startId;
        for (var i = 0; i < ctx.LinePortals.Count; i++)
        {
            var lp = ctx.LinePortals[i];
            if (lp.Type != LinkedLinePortalType)
                continue;
            if (lp.DestinationMissing)
            {
                ctx.DisableUnconnectedLinePortal?.Invoke(i);
                continue;
            }

            if (ctx.CollectSectors?.Invoke(id, lp.OriginLineIndex) == true)
                id++;
            if (ctx.CollectSectors?.Invoke(id, lp.OriginLineIndex + 1) == true)
                id++;
        }

        return id;
    }

    public static void CollectLeftoverLinkedSectors(CreateLinkedPortalsContext ctx)
    {
        for (var s = 0; s < ctx.Sectors.Count; s++)
        {
            var sec = ctx.Sectors[s];
            for (var plane = 0; plane < 2; plane++)
            {
                if (PortalTypeForPlane(sec, plane) != LinkedSectorPortalType)
                    continue;
                if (sec.PortalGroup != 0)
                    continue;
                if (sec.LinkedPortalId < 0)
                    continue;
                ctx.CollectSectors?.Invoke(sec.LinkedPortalId, s);
            }
        }
    }

    public static void AddDisplacementEntries(CreateLinkedPortalsContext ctx)
    {
        for (var i = 0; i < ctx.LinkedOrgs.Count; i++)
            ctx.AddDisplacementForSectorPortal?.Invoke(i);

        for (var i = 0; i < ctx.LinePortals.Count; i++)
        {
            if (ctx.LinePortals[i].Type == LinkedLinePortalType)
                ctx.AddDisplacementForLinePortal?.Invoke(i);
        }
    }

    public static bool DisplacementPairMismatches(
        (bool Set, double X, double Y) forward,
        (bool Set, double X, double Y) reverse) =>
        forward.Set && reverse.Set && (forward.X != -reverse.X || forward.Y != -reverse.Y);

    public static void RunDisplacementConsistencyPass(CreateLinkedPortalsContext ctx)
    {
        for (var x = 1; x < ctx.NextGroupId; x++)
        {
            for (var y = x + 1; y < ctx.NextGroupId; y++)
            {
                var dispxy = ctx.GetDisplacement?.Invoke(x, y) ?? (false, 0, 0);
                var dispyx = ctx.GetDisplacement?.Invoke(y, x) ?? (false, 0, 0);
                if (!DisplacementPairMismatches(dispxy, dispyx))
                    continue;
                ctx.ReportMismatch?.Invoke(x, y);
                ctx.Bogus = true;
            }
        }
    }

    public static void RunOrphanSectorCleanup(CreateLinkedPortalsContext ctx, IList<(int LineIndex, int FrontSector, bool OneSided)> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!line.OneSided)
                continue;
            var front = ctx.Sectors.Count > line.FrontSector ? ctx.Sectors[line.FrontSector] : default;
            if (front.PortalGroup != 0)
                continue;
            ctx.CollectOrphanOneSidedLine?.Invoke(-1, line.FrontSector);
        }

        for (var s = 0; s < ctx.Sectors.Count; s++)
        {
            var sec = ctx.Sectors[s];
            if (sec.PortalGroup == 0)
                ctx.ReportUnassignedSector?.Invoke(s);
            else if (sec.PortalGroup < 0)
                ctx.NormalizeNegativePortalGroup?.Invoke(s);
        }
    }

    public static int ResolveCeilingBelowFloorClearPlane(PortalSector sec)
    {
        if (sec.CeilingPortalZ >= sec.FloorPortalZ)
            return -1;
        if (sec.CeilingPlaneD < sec.FloorPlaneD || sec.FloorPortalZ == sec.FloorPlaneD)
            return CeilingPlane;
        return FloorPlane;
    }

    public static void FinalizeLinkedSectorPlanes(CreateLinkedPortalsContext ctx)
    {
        for (var s = 0; s < ctx.Sectors.Count; s++)
        {
            var sec = ctx.Sectors[s];
            if (sec.FloorPortalLinked)
                ctx.MarkPortalPlaneLinked?.Invoke(s, true);
            if (sec.CeilingPortalLinked)
                ctx.MarkPortalPlaneLinked?.Invoke(s, false);
            if (sec.FloorPortalLinked || sec.CeilingPortalLinked)
                ctx.MarkSectorLinesPortalConnect?.Invoke(s);

            if (!sec.FloorPortalLinked || !sec.CeilingPortalLinked)
                continue;
            var clear = ResolveCeilingBelowFloorClearPlane(sec);
            if (clear >= 0)
                ctx.ClearSlopedPortal?.Invoke(s, clear == FloorPlane, s);
        }
    }

    public static bool ShouldResetRejectMatrix(int displacementTableSize) => displacementTableSize > 1;

    public static void CreateLinkedPortals(CreateLinkedPortalsContext ctx)
    {
        if (ctx.LinkedOrgs.Count > 0)
        {
            ClearSlopedLinkedPortals(ctx);
            ctx.NextGroupId = CollectFromLinkedOrgs(ctx, ctx.NextGroupId);
        }

        ctx.NextGroupId = ProcessLineLinkedPortals(ctx, ctx.NextGroupId);
        ctx.CreateDisplacements?.Invoke(ctx.NextGroupId);
        CollectLeftoverLinkedSectors(ctx);
        AddDisplacementEntries(ctx);
        RunDisplacementConsistencyPass(ctx);

        if (ctx.ConnectPortalGroups?.Invoke() == true)
            ctx.Bogus = true;

        if (ShouldResetRejectMatrix(ctx.NextGroupId))
            ctx.RejectMatrixReset = true;

        FinalizeLinkedSectorPlanes(ctx);
    }
}
