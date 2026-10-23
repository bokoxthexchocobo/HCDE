namespace HCDE.MapLoader;

/// <summary>
/// <c>clearnodecache</c>, <c>PointOnLine</c>, and <c>MapLoader::SetRenderSector</c> from <c>glnodes.cpp</c> (1196–1396).
/// </summary>
public static class VmMapLoaderGlRenderSector
{
    public const uint SsecfDegenerate = 1u << 0;
    private const double SideEpsilon = 6.5536;
    private const string GameCacheFolder = "doom";
    private const string NodeCacheSubfolder = "nodes";

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

    /// <summary>Faithful to <c>UNSAFE_CCMD(clearnodecache)</c> (scans and deletes under <c>M_GetCachePath(false, "nodes")</c>).</summary>
    public static void ClearNodeCache()
    {
        var path = GetCachePath(create: false, NodeCacheSubfolder);
        if (!path.EndsWith('/'))
            path += "/";

        if (!TryScanDirectory(path, out var list))
            return;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.IsDirectory)
                Directory.Delete(entry.FilePath, recursive: false);
            else
                File.Delete(entry.FilePath);
        }
    }

    static string GetCachePath(bool create, string ns)
    {
        var cacheRoot = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        if (string.IsNullOrEmpty(cacheRoot))
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? "";
            cacheRoot = Path.Combine(home, ".cache");
        }
        else
            cacheRoot = ExpandHomePath(cacheRoot);

        var path = Path.Combine(cacheRoot, GameCacheFolder, ns);
        path = Path.GetFullPath(path);
        if (create)
            Directory.CreateDirectory(path);
        return path;
    }

    static string ExpandHomePath(string path)
    {
        if (path.StartsWith("$HOME/", StringComparison.Ordinal))
            return Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", path[6..]);
        if (path.StartsWith("$HOME\\", StringComparison.Ordinal))
            return Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "", path[6..]);
        return path;
    }

    readonly struct ScannedEntry
    {
        public required string FilePath { get; init; }
        public bool IsDirectory { get; init; }
    }

    static bool TryScanDirectory(string directoryWithTrailingSlash, out List<ScannedEntry> entries)
    {
        entries = new List<ScannedEntry>();
        var dir = directoryWithTrailingSlash.TrimEnd('/', '\\');
        if (!Directory.Exists(dir))
            return false;

        try
        {
            foreach (var fullPath in Directory.EnumerateFileSystemEntries(dir))
            {
                var attrs = File.GetAttributes(fullPath);
                entries.Add(new ScannedEntry
                {
                    FilePath = fullPath,
                    IsDirectory = (attrs & FileAttributes.Directory) != 0,
                });
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return true;
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
