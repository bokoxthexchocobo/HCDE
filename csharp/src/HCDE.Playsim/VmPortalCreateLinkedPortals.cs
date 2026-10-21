namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CreateLinkedPortals</c> from <c>portal.cpp</c> (851–1039).</summary>
public static class VmPortalCreateLinkedPortals
{
    public sealed class CreateLinkedPortalsContext
    {
        public List<int> LinkedSectorPortalIds { get; } = new();
        public IList<PortalSector> Sectors { get; set; } = Array.Empty<PortalSector>();
        public IList<PortalLine> LinePortals { get; set; } = Array.Empty<PortalLine>();
        public int NextGroupId { get; set; } = 1;
        public bool Bogus { get; set; }

        public Action<int, bool, int>? ClearSlopedPortal { get; init; }
        public Func<int, int, bool>? CollectSectors { get; init; }
        public Action<int>? CreateDisplacements { get; init; }
        public Action<int>? AddDisplacementForSectorPortal { get; init; }
        public Action<int>? AddDisplacementForLinePortal { get; init; }
        public Func<int, int, (bool Set, double X, double Y)>? GetDisplacement { get; init; }
        public Action<int, int>? ReportMismatch { get; init; }
    }

    public readonly record struct PortalSector(int Index, int FloorPortalType, int CeilingPortalType, bool FloorSlope, bool CeilingSlope, int PortalGroup, int LinkedPortalId);

    public readonly record struct PortalLine(int Index, int Type, bool DestinationMissing, int OriginLineIndex);

    public static void CreateLinkedPortals(CreateLinkedPortalsContext ctx)
    {
        for (var i = 0; i < ctx.LinkedSectorPortalIds.Count; i++)
        {
        }

        foreach (var id in ctx.LinkedSectorPortalIds)
        {
            _ = id;
        }

        if (ctx.LinkedSectorPortalIds.Count > 0)
        {
            for (var s = 0; s < ctx.Sectors.Count; s++)
            {
                var sec = ctx.Sectors[s];
                for (var plane = 0; plane < 2; plane++)
                {
                    var portalType = plane == 0 ? sec.FloorPortalType : sec.CeilingPortalType;
                    if (portalType != 1)
                        continue;
                    var sloped = plane == 0 ? sec.FloorSlope : sec.CeilingSlope;
                    if (sloped)
                        ctx.ClearSlopedPortal?.Invoke(s, plane == 0, s);
                }
            }

            for (var i = 0; i < ctx.LinkedSectorPortalIds.Count; i++)
            {
                if (ctx.CollectSectors?.Invoke(ctx.NextGroupId, i) == true)
                    ctx.NextGroupId++;
                if (ctx.CollectSectors?.Invoke(ctx.NextGroupId, i + 1000) == true)
                    ctx.NextGroupId++;
            }
        }

        for (var i = 0; i < ctx.LinePortals.Count; i++)
        {
            var lp = ctx.LinePortals[i];
            if (lp.Type != 2)
                continue;
            if (lp.DestinationMissing)
            {
                continue;
            }

            if (ctx.CollectSectors?.Invoke(ctx.NextGroupId, lp.OriginLineIndex) == true)
                ctx.NextGroupId++;
            if (ctx.CollectSectors?.Invoke(ctx.NextGroupId, lp.OriginLineIndex + 1) == true)
                ctx.NextGroupId++;
        }

        ctx.CreateDisplacements?.Invoke(ctx.NextGroupId);

        for (var s = 0; s < ctx.Sectors.Count; s++)
        {
            var sec = ctx.Sectors[s];
            for (var plane = 0; plane < 2; plane++)
            {
                var portalType = plane == 0 ? sec.FloorPortalType : sec.CeilingPortalType;
                if (portalType == 1 && sec.PortalGroup == 0 && sec.LinkedPortalId >= 0)
                    ctx.CollectSectors?.Invoke(sec.LinkedPortalId, s);
            }
        }

        for (var i = 0; i < ctx.LinkedSectorPortalIds.Count; i++)
            ctx.AddDisplacementForSectorPortal?.Invoke(i);

        for (var i = 0; i < ctx.LinePortals.Count; i++)
        {
            if (ctx.LinePortals[i].Type == 2)
                ctx.AddDisplacementForLinePortal?.Invoke(i);
        }

        for (var x = 1; x < ctx.NextGroupId; x++)
        {
            for (var y = x + 1; y < ctx.NextGroupId; y++)
            {
                var dispxy = ctx.GetDisplacement?.Invoke(x, y) ?? (false, 0, 0);
                var dispyx = ctx.GetDisplacement?.Invoke(y, x) ?? (false, 0, 0);
                if (dispxy.Set && dispyx.Set && (dispxy.X != -dispyx.X || dispxy.Y != -dispyx.Y))
                {
                    ctx.ReportMismatch?.Invoke(x, y);
                    ctx.Bogus = true;
                }
            }
        }
    }
}
