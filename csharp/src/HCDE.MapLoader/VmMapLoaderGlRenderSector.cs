namespace HCDE.MapLoader;

/// <summary>
/// <c>clearnodecache</c>, <c>PointOnLine</c>, and <c>MapLoader::SetRenderSector</c> from <c>glnodes.cpp</c> (1196–1396).
/// </summary>
public static class VmMapLoaderGlRenderSector
{
    public const uint SsecfDegenerate = 1u << 0;
    private const double SideEpsilon = 6.5536;

    public sealed class CacheEntry
    {
        public required string FilePath { get; init; }
        public bool IsDirectory { get; init; }
    }

    public sealed class RenderSeg
    {
        public int V1X { get; set; }
        public int V1Y { get; set; }
        public int V2X { get; set; }
        public int V2Y { get; set; }
        public bool HasLinedef { get; set; }
        public RenderSide? Sidedef { get; set; }
        public int? PartnerIndex { get; set; }
        public RenderSubsector? Subsector { get; set; }
    }

    public sealed class RenderSide
    {
        public RenderSector? Sector { get; set; }
    }

    public sealed class RenderSector
    {
        public int Index { get; set; }
    }

    public sealed class RenderSubsector
    {
        public uint Flags { get; set; }
        public int NumLines { get; set; }
        public int FirstSegIndex { get; set; }
        public RenderSector? Sector { get; set; }
        public RenderSector? RenderSector { get; set; }
    }

    public sealed class SetRenderSectorLevel
    {
        public List<RenderSeg> Segs { get; } = new();
        public List<RenderSubsector> Subsectors { get; } = new();
    }

    public enum ClearNodeCacheResult
    {
        Cleared,
        ScanFailed,
    }

    public static string CombineNodeCacheDirectory(string cacheRoot, string subfolder) =>
        $"{cacheRoot.TrimEnd('/', '\\')}/doom/{subfolder}";

    public static ClearNodeCacheResult ClearNodeCache(
        IReadOnlyList<CacheEntry>? directoryListing,
        Action<string>? removeDirectory = null,
        Action<string>? removeFile = null)
    {
        removeDirectory ??= static _ => { };
        removeFile ??= static _ => { };

        if (directoryListing is null)
            return ClearNodeCacheResult.ScanFailed;

        for (var i = directoryListing.Count - 1; i >= 0; i--)
        {
            var entry = directoryListing[i];
            if (entry.IsDirectory)
                removeDirectory(entry.FilePath);
            else
                removeFile(entry.FilePath);
        }

        return ClearNodeCacheResult.Cleared;
    }

    public static bool PointOnLine(int x, int y, int x1, int y1, int dx, int dy)
    {
        var dDx = (double)dx;
        var dDy = (double)dy;
        var dX = (double)x;
        var dY = (double)y;
        var dX1 = (double)x1;
        var dY1 = (double)y1;

        var sNum = (dY1 - dY) * dDx - (dX1 - dX) * dDy;

        if (Math.Abs(sNum) < 17179869184.0)
        {
            var l = Math.Sqrt(dDx * dDx + dDy * dDy);
            var dist = Math.Abs(sNum) / l;
            if (dist < SideEpsilon)
                return true;
        }

        return false;
    }

    public static void SetRenderSector(SetRenderSectorLevel level)
    {
        ValidatePartnerSegs(level.Segs);

        var undetermined = new List<RenderSubsector>();

        foreach (var ss in level.Subsectors)
        {
            ss.Flags |= SsecfDegenerate;
            var first = level.Segs[ss.FirstSegIndex];
            for (var j = 2u; j < ss.NumLines; j++)
            {
                var seg = level.Segs[ss.FirstSegIndex + (int)j];
                if (!PointOnLine(
                        seg.V1X,
                        seg.V1Y,
                        first.V1X,
                        first.V1Y,
                        first.V2X - first.V1X,
                        first.V2Y - first.V1Y))
                {
                    ss.Flags &= ~SsecfDegenerate;
                    break;
                }
            }

            var picked = false;
            for (var j = 0; j < ss.NumLines; j++)
            {
                var seg = level.Segs[ss.FirstSegIndex + j];
                var partner = seg.PartnerIndex is int pi ? level.Segs[pi] : null;
                if (seg.Sidedef is not null
                    && (partner is null
                        || (partner.Sidedef is not null
                            && seg.Sidedef.Sector != partner.Sidedef.Sector)))
                {
                    ss.RenderSector = seg.Sidedef!.Sector;
                    picked = true;
                    break;
                }
            }

            if (!picked)
                undetermined.Add(ss);
        }

        while (undetermined.Count > 0)
        {
            var deleted = false;
            for (var i = undetermined.Count - 1; i >= 0; i--)
            {
                var ss = undetermined[i];
                for (var j = 0; j < ss.NumLines; j++)
                {
                    var seg = level.Segs[ss.FirstSegIndex + j];
                    if (seg.PartnerIndex is int pi)
                    {
                        var partner = level.Segs[pi];
                        var backSec = partner.Subsector?.RenderSector;
                        if (backSec is not null)
                        {
                            ss.RenderSector = backSec;
                            undetermined.RemoveAt(i);
                            deleted = true;
                            break;
                        }
                    }
                }
            }

            if (!deleted)
            {
                foreach (var ss in undetermined)
                    ss.RenderSector = ss.Sector;
                break;
            }
        }
    }

    static void ValidatePartnerSegs(List<RenderSeg> segs)
    {
        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            if (seg.PartnerIndex is not int pi)
                continue;

            if (pi < 0 || pi >= segs.Count)
            {
                seg.PartnerIndex = null;
                continue;
            }

            var partner = segs[pi];
            if (seg.HasLinedef && seg.PartnerIndex is not null && !partner.HasLinedef)
            {
                seg.PartnerIndex = null;
                partner.PartnerIndex = null;
            }
        }

        for (var i = 0; i < segs.Count; i++)
        {
            var seg = segs[i];
            if (seg.PartnerIndex is int pi && segs[pi].PartnerIndex != i)
                seg.PartnerIndex = null;
        }
    }
}
