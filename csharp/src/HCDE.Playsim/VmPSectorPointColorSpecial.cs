namespace HCDE.Playsim;

/// <summary>
/// Sector point, colormap, plane-light, and special transfer helpers from <c>p_sectors.cpp</c> (717–961).
/// </summary>
public static class VmPSectorPointColorSpecial
{
    public sealed class Vertex
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class MapLine
    {
        public Vertex V1 { get; } = new();
        public Vertex V2 { get; } = new();
    }

    public sealed class Plane
    {
        public bool IsSlope { get; set; }
        public double D { get; set; }
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
        public double Fd() => D;
    }

    public sealed class Colormap
    {
        public int LightColor { get; set; }
        public int Desaturation { get; set; }
        public int FadeColor { get; set; }
        public int FogDensity { get; set; }
    }

    public sealed class MapSector
    {
        public List<MapLine> Lines { get; } = new();
        public Plane CeilingPlane { get; } = new();
        public Colormap Colormap { get; } = new();
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public PlaneSlot[] Planes { get; } = { new(), new() };
        public int LightLevel { get; set; }
        public int Special { get; set; }
        public int DamageAmount { get; set; }
        public int DamageType { get; set; }
        public int DamageInterval { get; set; }
        public int LeakyDamage { get; set; }
        public uint Flags { get; set; }
        public object? LevelVertexes { get; set; }

        public int GetPlaneLight(int pos) => Planes[pos].Light;
        public uint GetFlags(int pos) => Planes[pos].Flags;
        public int ClampLight(int v) => v;
    }

    public sealed class PlaneSlot
    {
        public uint Flags { get; set; }
        public int Light { get; set; }
    }

    public const uint PlanefAbslighting = 1;
    public const uint PlanefBlocked = 2;
    public const int Floor = 0;
    public const int Ceiling = 1;
    public const uint SecfSpecialFlags = 0xffff0000;

    public sealed class SectorSpecial
    {
        public int Special { get; set; }
        public int DamageAmount { get; set; }
        public int DamageType { get; set; }
        public int DamageInterval { get; set; }
        public int LeakyDamage { get; set; }
        public uint Flags { get; set; }
    }

    public sealed class SectorPortal
    {
        public int Type { get; set; }
        public object? Skybox { get; set; }
        public uint Flags { get; set; }
        public bool PortalBlocksView(int which) => false;
        public int GetTexture(int which) => 0;
    }

    public static double FindLowestCeilingPoint(MapSector sector, Vertex? spotOut = null)
    {
        if (!sector.CeilingPlane.IsSlope)
        {
            if (spotOut is not null)
            {
                spotOut.X = sector.Lines.Count == 0 ? 0 : sector.Lines[0].V1.X;
                spotOut.Y = sector.Lines.Count == 0 ? 0 : sector.Lines[0].V1.Y;
            }

            return sector.CeilingPlane.Fd();
        }

        var height = double.MaxValue;
        Vertex? spot = null;
        foreach (var line in sector.Lines)
        {
            var probe = sector.CeilingPlane.ZAtPoint(line.V1.X, line.V1.Y);
            if (probe < height)
            {
                height = probe;
                spot = line.V1;
            }

            probe = sector.CeilingPlane.ZAtPoint(line.V2.X, line.V2.Y);
            if (probe < height)
            {
                height = probe;
                spot = line.V2;
            }
        }

        if (spotOut is not null && spot is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static void SetColor(MapSector sector, int color, int desat, Action<MapSector>? recalcLights = null)
    {
        sector.Colormap.LightColor = color;
        sector.Colormap.Desaturation = desat;
        recalcLights?.Invoke(sector);
    }

    public static void SetFade(MapSector sector, int color, Action<MapSector>? recalcLights = null)
    {
        sector.Colormap.FadeColor = color;
        recalcLights?.Invoke(sector);
    }

    public static void SetFogDensity(MapSector sector, int dens) => sector.Colormap.FogDensity = dens;

    public static void ClosestPoint(MapSector sector, double x, double y, out double ox, out double oy)
    {
        var bestDist = double.MaxValue;
        ox = 0;
        oy = 0;

        foreach (var check in sector.Lines)
        {
            var v1 = check.V1;
            var v2 = check.V2;
            var a = v2.X - v1.X;
            var b = v2.Y - v1.Y;
            var den = a * a + b * b;
            double ix, iy;

            if (den == 0)
            {
                ix = v1.X;
                iy = v1.Y;
            }
            else
            {
                var num = (x - v1.X) * a + (y - v1.Y) * b;
                var u = num / den;
                if (u <= 0)
                {
                    ix = v1.X;
                    iy = v1.Y;
                }
                else if (u >= 1)
                {
                    ix = v2.X;
                    iy = v2.Y;
                }
                else
                {
                    ix = v1.X + u * a;
                    iy = v1.Y + u * b;
                }
            }

            a = ix - x;
            b = iy - y;
            var dist = a * a + b * b;
            if (dist < bestDist)
            {
                bestDist = dist;
                ox = ix;
                oy = iy;
            }
        }
    }

    public static bool PlaneMoving(MapSector sector, int pos) =>
        pos == Floor
            ? sector.FloorData is not null || (sector.Planes[Floor].Flags & PlanefBlocked) != 0
            : sector.CeilingData is not null || (sector.Planes[Ceiling].Flags & PlanefBlocked) != 0;

    public static int GetFloorLight(MapSector sector) =>
        (sector.GetFlags(Floor) & PlanefAbslighting) != 0
            ? sector.GetPlaneLight(Floor)
            : sector.ClampLight(sector.LightLevel + sector.GetPlaneLight(Floor));

    public static int GetCeilingLight(MapSector sector) =>
        (sector.GetFlags(Ceiling) & PlanefAbslighting) != 0
            ? sector.GetPlaneLight(Ceiling)
            : sector.ClampLight(sector.LightLevel + sector.GetPlaneLight(Ceiling));

    public static SectorPortal? ValidatePortal(MapSector sector, int which, SectorPortal port, int skyFlatNum)
    {
        if (port.Type == 1 && port.Skybox is null)
            return null;
        if (port.PortalBlocksView(which))
            return null;
        if ((port.Flags & 1) != 0 && port.GetTexture(which) != skyFlatNum)
            return null;
        return port;
    }

    public static SectorSpecial GetSpecial(MapSector sector) => new()
    {
        Special = sector.Special,
        DamageAmount = sector.DamageAmount,
        DamageType = sector.DamageType,
        DamageInterval = sector.DamageInterval,
        LeakyDamage = sector.LeakyDamage,
        Flags = sector.Flags & SecfSpecialFlags
    };

    public static void SetSpecial(MapSector sector, SectorSpecial spec)
    {
        sector.Special = spec.Special;
        sector.DamageAmount = spec.DamageAmount;
        sector.DamageType = spec.DamageType;
        sector.DamageInterval = spec.DamageInterval;
        sector.LeakyDamage = spec.LeakyDamage;
        sector.Flags = (sector.Flags & ~SecfSpecialFlags) | (spec.Flags & SecfSpecialFlags);
    }

    public static void TransferSpecial(MapSector sector, MapSector model)
    {
        sector.Special = model.Special;
        sector.DamageAmount = model.DamageAmount;
        sector.DamageType = model.DamageType;
        sector.DamageInterval = model.DamageInterval;
        sector.LeakyDamage = model.LeakyDamage;
        sector.Flags = (sector.Flags & ~SecfSpecialFlags) | (model.Flags & SecfSpecialFlags);
    }
}
