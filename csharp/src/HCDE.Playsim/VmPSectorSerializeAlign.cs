namespace HCDE.Playsim;

/// <summary>
/// Sector serialize, plane copy, flat align, texture replace, poly BSP hook, line adjust,
/// side lighting, and vertex height helpers from <c>p_sectors.cpp</c> (1370–1672).
/// </summary>
public static class VmPSectorSerializeAlign
{
    public sealed class SectorSpecial
    {
        public int Special { get; set; }
        public int DamageAmount { get; set; }
        public int DamageType { get; set; }
        public int DamageInterval { get; set; }
        public int LeakyDamage { get; set; }
        public uint Flags { get; set; }
    }

    public sealed class SerializerSink
    {
        private readonly Dictionary<string, object?> _fields = new();
        private readonly Stack<string> _objectStack = new();

        public bool BeginObject(string key)
        {
            _objectStack.Push(key);
            return _objectStack.Count > 0;
        }

        public SerializerSink Arc(string key, object? value)
        {
            _fields[key] = value;
            return this;
        }

        public void EndObject()
        {
            if (_objectStack.Count > 0)
                _objectStack.Pop();
        }

        public T? Get<T>(string key) where T : class =>
            _fields.TryGetValue(key, out var v) ? v as T : null;

        public int GetInt(string key) =>
            _fields.TryGetValue(key, out var v) && v is int i ? i : 0;

        public uint GetUInt(string key) =>
            _fields.TryGetValue(key, out var v) && v is uint u ? u : 0;
    }

    public static SerializerSink SerializeSectorSpecial(SerializerSink arc, string key, SectorSpecial spec)
    {
        if (arc.BeginObject(key))
        {
            arc.Arc("special", spec.Special);
            arc.Arc("damageamount", spec.DamageAmount);
            arc.Arc("damagetype", spec.DamageType);
            arc.Arc("damageinterval", spec.DamageInterval);
            arc.Arc("leakydamage", spec.LeakyDamage);
            arc.Arc("flags", spec.Flags);
            arc.EndObject();
        }

        return arc;
    }

    public static SectorSpecial ReadSectorSpecial(SerializerSink arc)
    {
        return new SectorSpecial
        {
            Special = arc.GetInt("special"),
            DamageAmount = arc.GetInt("damageamount"),
            DamageType = arc.GetInt("damagetype"),
            DamageInterval = arc.GetInt("damageinterval"),
            LeakyDamage = arc.GetInt("leakydamage"),
            Flags = arc.GetUInt("flags"),
        };
    }

    public sealed class SecPlane
    {
        public Func<(double X, double Y, double Z)> Normal { get; set; } = () => (0, 0, 1);
        public double D { get; set; }

        public double Fd() => D;

        public void CopyFrom(SecPlane other)
        {
            Normal = other.Normal;
            D = other.D;
        }
    }

    public static bool CopyPlaneIfValid(SecPlane self, SecPlane dest, SecPlane opp)
    {
        var copy = false;
        var selfN = self.Normal();
        var destN = dest.Normal();
        if (!SameNormal(selfN, destN))
            copy = true;
        else
        {
            var oppN = opp.Normal();
            var negDest = (-destN.X, -destN.Y, -destN.Z);
            if (!SameNormal(oppN, negDest))
            {
                if (self.Fd() < dest.Fd())
                    copy = true;
            }
            else if (self.Fd() < dest.Fd() && self.Fd() > -opp.Fd())
                copy = true;
        }

        if (copy)
            dest.CopyFrom(self);
        return copy;
    }

    static bool SameNormal((double X, double Y, double Z) a, (double X, double Y, double Z) b) =>
        Math.Abs(a.X - b.X) < 1e-9 && Math.Abs(a.Y - b.Y) < 1e-9 && Math.Abs(a.Z - b.Z) < 1e-9;

    public sealed class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class MapLine
    {
        public Vertex V1 { get; } = new();
        public Vertex V2 { get; } = new();
        public MapSector? FrontSector { get; set; }
        public MapSector? BackSector { get; set; }

        public (double X, double Y) Delta() => (V2.X - V1.X, V2.Y - V1.Y);

        public double[] Bbox { get; } = new double[4];

        public const int BoxLeft = 0;
        public const int BoxRight = 1;
        public const int BoxBottom = 2;
        public const int BoxTop = 3;
    }

    public sealed class MapSector
    {
        public List<MapSide> Sides { get; } = new();
        public List<MapSector> Sectors { get; } = new();
        public int Floor { get; set; }
        public int Ceiling { get; set; }
        public Func<Vertex, double> CeilingPlaneZ { get; set; } = _ => 128;
        public Func<Vertex, double> FloorPlaneZ { get; set; } = _ => 0;

        public int GetTexture(int which) => which == 0 ? Floor : Ceiling;

        public void SetTexture(int which, int id)
        {
            if (which == 0) Floor = id;
            else Ceiling = id;
        }

        public void SetBase(int fc, double dist, double negAngleRad)
        {
            BaseDist = dist;
            BaseAngleRad = negAngleRad;
            BasePlane = fc;
        }

        public int BasePlane { get; private set; }
        public double BaseDist { get; private set; }
        public double BaseAngleRad { get; private set; }
    }

    public sealed class MapSide
    {
        public int[] Textures { get; } = new int[3];

        public int GetTexture(int tier) => Textures[tier];

        public void SetTexture(int tier, int id) => Textures[tier] = id;
    }

    public sealed class LevelLocals
    {
        public List<MapLine> Lines { get; } = new();
        public List<MapSide> Sides { get; } = new();
        public List<MapSector> Sectors { get; } = new();
        public uint Flags3 { get; set; }
        public uint Flags2 { get; set; }
        public int WallHorizLight { get; set; }
        public int WallVertLight { get; set; }
        public int FakeContrast { get; set; } = 1;
    }

    public const uint Level3ForceFakeContrast = 1;
    public const uint Level2SmoothLighting = 1;
    public const uint NotTop = 1;
    public const uint NotMiddle = 2;
    public const uint NotBottom = 4;
    public const uint NotFloor = 8;
    public const uint NotCeiling = 16;

    public static bool AlignFlat(LevelLocals level, int lineNum, int side, int fc)
    {
        if (lineNum < 0 || lineNum >= level.Lines.Count)
            return false;
        var line = level.Lines[lineNum];
        var sec = side != 0 ? line.BackSector : line.FrontSector;
        if (sec is null)
            return false;

        var delta = line.Delta();
        var angle = Math.Atan2(delta.Y, delta.X);
        var norm = angle - Math.PI / 2;
        var dist = -(Math.Cos(norm) * line.V1.X + Math.Sin(norm) * line.V1.Y);
        if (side != 0)
        {
            angle += Math.PI;
            dist = -dist;
        }

        sec.SetBase(fc, dist, -angle);
        return true;
    }

    public sealed class TextureLookup
    {
        public Func<string, int>? ResolveWall { get; init; }
        public Func<string, int>? ResolveFlat { get; init; }
    }

    public static void ReplaceTextures(
        LevelLocals level,
        string? fromName,
        string? toName,
        int flags,
        TextureLookup tex)
    {
        if (fromName is null)
            return;

        if ((flags ^ (NotBottom | NotMiddle | NotTop)) != 0)
        {
            var pic1 = tex.ResolveWall?.Invoke(fromName) ?? 0;
            var pic2 = tex.ResolveWall?.Invoke(toName ?? "") ?? 0;
            uint[] bits = [NotTop, NotMiddle, NotBottom];
            foreach (var side in level.Sides)
            {
                for (var j = 0; j < 3; j++)
                {
                    if ((flags & bits[j]) == 0 && side.GetTexture(j) == pic1)
                        side.SetTexture(j, pic2);
                }
            }
        }

        if ((flags ^ (NotFloor | NotCeiling)) != 0)
        {
            var pic1 = tex.ResolveFlat?.Invoke(fromName) ?? 0;
            var pic2 = tex.ResolveFlat?.Invoke(toName ?? "") ?? 0;
            foreach (var sec in level.Sectors)
            {
                if ((flags & NotFloor) == 0 && sec.GetTexture(0) == pic1)
                    sec.SetTexture(0, pic2);
                if ((flags & NotCeiling) == 0 && sec.GetTexture(1) == pic1)
                    sec.SetTexture(1, pic2);
            }
        }
    }

    public sealed class PolyNode
    {
        public List<object> Segs { get; } = new();
        public PolyNode? PNext { get; set; }
    }

    public sealed class MiniBsp
    {
        public List<SubsectorLink> Subsectors { get; } = new();
        public List<BspSeg> Segs { get; } = new();
        public bool Dirty { get; set; }
    }

    public sealed class SubsectorLink
    {
        public MapSector? Sector { get; set; }
        public object? Section { get; set; }
    }

    public sealed class BspSeg
    {
        public object? Subsector { get; set; }
        public object? PartnerSeg { get; set; }
    }

    public sealed class Subsector
    {
        public MiniBsp? Bsp { get; set; }
        public MapSector? Sector { get; set; }
        public object? Section { get; set; }
        public int FirstLine { get; set; }
        public int NumLines { get; set; }
        public PolyNode? Polys { get; set; }
    }

    public sealed class PolyBspContext
    {
        public Action? Clear { get; init; }
        public Action<int, int>? AddSegs { get; init; }
        public Action<IReadOnlyList<object>>? AddPolySegs { get; init; }
        public Action<bool>? BuildMini { get; init; }
        public Action<MiniBsp>? ExtractMini { get; init; }
    }

    public static void BuildPolyBsp(Subsector sub, LevelLocals level, PolyBspContext ctx)
    {
        if (sub.Bsp is not null && !sub.Bsp.Dirty)
            throw new InvalidOperationException("BSP computed more than once");

        ctx.Clear?.Invoke();
        ctx.AddSegs?.Invoke(sub.FirstLine, sub.NumLines);
        for (var pn = sub.Polys; pn is not null; pn = pn.PNext)
            ctx.AddPolySegs?.Invoke(pn.Segs);

        ctx.BuildMini?.Invoke(false);
        sub.Bsp ??= new MiniBsp();
        ctx.ExtractMini?.Invoke(sub.Bsp);
        foreach (var ss in sub.Bsp.Subsectors)
        {
            ss.Sector = sub.Sector;
            ss.Section = sub.Section;
        }

        foreach (var seg in sub.Bsp.Segs)
        {
            seg.Subsector = sub;
            seg.PartnerSeg = null;
        }
    }

    public static void AdjustLine(MapLine line)
    {
        var dx = line.V2.X - line.V1.X;
        var dy = line.V2.Y - line.V1.Y;
        if (line.V1.X < line.V2.X)
        {
            line.Bbox[MapLine.BoxLeft] = line.V1.X;
            line.Bbox[MapLine.BoxRight] = line.V2.X;
        }
        else
        {
            line.Bbox[MapLine.BoxLeft] = line.V2.X;
            line.Bbox[MapLine.BoxRight] = line.V1.X;
        }

        if (line.V1.Y < line.V2.Y)
        {
            line.Bbox[MapLine.BoxBottom] = line.V1.Y;
            line.Bbox[MapLine.BoxTop] = line.V2.Y;
        }
        else
        {
            line.Bbox[MapLine.BoxBottom] = line.V2.Y;
            line.Bbox[MapLine.BoxTop] = line.V1.Y;
        }
    }

    public sealed class SideHost
    {
        public uint Flags { get; set; }
        public int Light { get; set; }
        public int[] TierLights { get; } = new int[3];
        public MapLine? LineDef { get; set; }
        public MapSector? Sector { get; set; }
    }

    public const uint WallfAbslighting = 1;
    public const uint WallfAbslightingTier = 2;
    public const uint WallfNoFakeContrast = 4;
    public const uint WallfSmoothLighting = 8;
    public const uint WallfLightFog = 16;

    public static int GetLightLevel(
        SideHost side,
        bool foggy,
        int baseLight,
        int which,
        bool is3dLight,
        LevelLocals level,
        bool captureFakeContrast,
        out int fakeContrast)
    {
        fakeContrast = 0;
        if (!is3dLight)
        {
            if ((side.Flags & (WallfAbslightingTier << which)) != 0)
                baseLight = side.TierLights[which];
            else if ((side.Flags & WallfAbslighting) != 0)
                baseLight = side.Light + side.TierLights[which];
        }

        if (!foggy || (level.Flags3 & Level3ForceFakeContrast) != 0)
        {
            if ((side.Flags & WallfNoFakeContrast) == 0 && level.FakeContrast != 0 && side.LineDef is not null)
            {
                var delta = side.LineDef.Delta();
                int rel;
                if (((level.Flags2 & Level2SmoothLighting) != 0 ||
                     (side.Flags & WallfSmoothLighting) != 0 ||
                     level.FakeContrast == 2) &&
                    delta.X != 0)
                {
                    rel = (int)Math.Round(
                        level.WallHorizLight +
                        Math.Abs(Math.Atan(delta.Y / delta.X) / 1.57079) *
                        (level.WallVertLight - level.WallHorizLight));
                }
                else
                {
                    rel = delta.X == 0 ? level.WallVertLight :
                        delta.Y == 0 ? level.WallHorizLight : 0;
                }

                if (captureFakeContrast)
                    fakeContrast = rel;
                else
                    baseLight += rel;
            }
        }

        if (!is3dLight &&
            (side.Flags & WallfAbslighting) == 0 &&
            (side.Flags & (WallfAbslightingTier << which)) == 0 &&
            (!foggy || (side.Flags & WallfLightFog) != 0))
        {
            baseLight += side.Light + side.TierLights[which];
        }

        return baseLight;
    }

    public sealed class VertexHeights
    {
        public MapSector?[] Sectors { get; set; } = [];
        public int NumSectors { get; set; }
        public float[] HeightList { get; } = new float[32];
        public int NumHeights { get; set; }
        public bool Dirty { get; set; } = true;
    }

    public static void RecalcVertexHeights(VertexHeights vtx, Vertex point)
    {
        vtx.NumHeights = 0;
        for (var i = 0; i < vtx.NumSectors; i++)
        {
            var sec = vtx.Sectors[i];
            if (sec is null) continue;
            for (var j = 0; j < 2; j++)
            {
                var height = j == 0
                    ? (float)CeilingZ(sec, point)
                    : (float)FloorZ(sec, point);

                var k = 0;
                for (; k < vtx.NumHeights; k++)
                {
                    if (Math.Abs(height - vtx.HeightList[k]) < 1e-6f)
                        break;
                    if (height < vtx.HeightList[k])
                    {
                        Array.Copy(vtx.HeightList, k, vtx.HeightList, k + 1, vtx.NumHeights - k);
                        vtx.HeightList[k] = height;
                        vtx.NumHeights++;
                        break;
                    }
                }

                if (k == vtx.NumHeights && vtx.NumHeights < vtx.HeightList.Length)
                    vtx.HeightList[vtx.NumHeights++] = height;
            }
        }

        if (vtx.NumHeights <= 2)
            vtx.NumHeights = 0;
        vtx.Dirty = false;
    }

    static double CeilingZ(MapSector sec, Vertex v) => sec.CeilingPlaneZ(v);
    static double FloorZ(MapSector sec, Vertex v) => sec.FloorPlaneZ(v);
}
