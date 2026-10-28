namespace HCDE.Playsim;

/// <summary>
/// Ceiling surround, texture height, model sector, light, and floor high-point queries from
/// <c>p_sectors.cpp</c> (453–711).
/// </summary>
public static class VmPSectorCeilingTextureModel
{
    public const int MlTwosided = 4;
    public const int CompatfShorttex = 1 << 2;

    public const int SideBottom = 0;
    public const int SideTop = 1;

    public sealed class SideDef
    {
        public int BottomTextureId { get; set; } = -1;
        public int TopTextureId { get; set; } = -1;

        public int GetTexture(int which) => which == SideTop ? TopTextureId : BottomTextureId;
    }

    public sealed class TextureLine
    {
        public VmPSectorSurroundHeights.Vertex V1 { get; } = new();
        public VmPSectorSurroundHeights.Vertex V2 { get; } = new();
        public LightSector? FrontSector { get; set; }
        public LightSector? BackSector { get; set; }
        public int Flags { get; set; }
        public SideDef Side0 { get; } = new();
        public SideDef Side1 { get; } = new();

        public SideDef GetSide(int index) => index == 0 ? Side0 : Side1;
    }

    public sealed class SectorLevel
    {
        public int CompatFlags { get; set; }
        public Func<int, bool>? TextureIdIsValid { get; set; }
        public Func<int, bool>? TextureIdIsNull { get; set; }
        public Func<int, double>? TextureDisplayHeight { get; set; }
        public double FallbackTextureHeight { get; set; } = 128;
    }

    public sealed class LightSector
    {
        public int LightLevel { get; set; }
        public SectorLevel Level { get; } = new();
        public bool FloorIsSlope { get; set; }
        public double FloorPlaneD { get; set; }
        public VmPSectorSurroundHeights.Plane FloorPlane { get; } = new();
        public List<TextureLine> Lines { get; } = new();
    }

    public static LightSector? GetNextLightSector(TextureLine line, LightSector from)
    {
        if (line.FrontSector == from)
            return line.BackSector;
        if (line.BackSector == from)
            return line.FrontSector;
        return null;
    }

    public static double FindLowestCeilingSurrounding(VmPSectorSurroundHeights.MapSector sector, VmPSectorSurroundHeights.Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: true);

        var spot = sector.Lines[0].V1;
        var height = double.PositiveInfinity;

        foreach (var check in sector.Lines)
        {
            var other = VmPSectorSurroundHeights.GetNextSector(check, sector);
            if (other is null)
                continue;

            var oceil = other.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (oceil < height)
            {
                height = oceil;
                spot = check.V1;
            }

            oceil = other.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (oceil < height)
            {
                height = oceil;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static double FindHighestCeilingSurrounding(VmPSectorSurroundHeights.MapSector sector, VmPSectorSurroundHeights.Vertex? spotOut = null)
    {
        if (sector.Lines.Count == 0)
            return sector.GetPlaneTexZ(ceiling: true);

        var spot = sector.Lines[0].V1;
        var height = double.NegativeInfinity;

        foreach (var check in sector.Lines)
        {
            var other = VmPSectorSurroundHeights.GetNextSector(check, sector);
            if (other is null)
                continue;

            var oceil = other.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y);
            if (oceil > height)
            {
                height = oceil;
                spot = check.V1;
            }

            oceil = other.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y);
            if (oceil > height)
            {
                height = oceil;
                spot = check.V2;
            }
        }

        if (spotOut is not null)
        {
            spotOut.X = spot.X;
            spotOut.Y = spot.Y;
        }

        return height;
    }

    public static void CheckShortestTex(SectorLevel level, int texnum, ref double minsize)
    {
        var isValid = level.TextureIdIsValid?.Invoke(texnum) ?? texnum >= 0;
        var isNull = level.TextureIdIsNull?.Invoke(texnum) ?? texnum < 0;
        if (isValid || (isNull && (level.CompatFlags & CompatfShorttex) != 0))
        {
            var h = level.TextureDisplayHeight?.Invoke(texnum);
            if (h is not null && h.Value < minsize)
                minsize = h.Value;
        }
    }

    public static double FindShortestTextureAround(LightSector sec)
    {
        var minsize = double.PositiveInfinity;
        foreach (var check in sec.Lines)
        {
            if ((check.Flags & MlTwosided) == 0)
                continue;

            CheckShortestTex(sec.Level, check.Side0.GetTexture(SideBottom), ref minsize);
            CheckShortestTex(sec.Level, check.Side1.GetTexture(SideBottom), ref minsize);
        }

        return minsize < double.PositiveInfinity ? minsize : sec.Level.FallbackTextureHeight;
    }

    public static double FindShortestUpperAround(LightSector sec)
    {
        var minsize = double.PositiveInfinity;
        foreach (var check in sec.Lines)
        {
            if ((check.Flags & MlTwosided) == 0)
                continue;

            CheckShortestTex(sec.Level, check.Side0.GetTexture(SideTop), ref minsize);
            CheckShortestTex(sec.Level, check.Side1.GetTexture(SideTop), ref minsize);
        }

        return minsize < double.PositiveInfinity ? minsize : sec.Level.FallbackTextureHeight;
    }

    public static VmPSectorSurroundHeights.MapSector? FindModelFloorSector(
        VmPSectorSurroundHeights.MapSector sect,
        double floordestheight)
    {
        foreach (var check in sect.Lines)
        {
            var sec = VmPSectorSurroundHeights.GetNextSector(check, sect);
            if (sec is null)
                continue;

            if (sec.FloorPlane.ZAtPoint(check.V1.X, check.V1.Y) == floordestheight
                || sec.FloorPlane.ZAtPoint(check.V2.X, check.V2.Y) == floordestheight)
                return sec;
        }

        return null;
    }

    public static VmPSectorSurroundHeights.MapSector? FindModelCeilingSector(
        VmPSectorSurroundHeights.MapSector sect,
        double ceilingdestheight)
    {
        foreach (var check in sect.Lines)
        {
            var sec = VmPSectorSurroundHeights.GetNextSector(check, sect);
            if (sec is null)
                continue;

            if (sec.CeilingPlane.ZAtPoint(check.V1.X, check.V1.Y) == ceilingdestheight
                || sec.CeilingPlane.ZAtPoint(check.V2.X, check.V2.Y) == ceilingdestheight)
                return sec;
        }

        return null;
    }

    public static int FindMinSurroundingLight(LightSector sector, int min)
    {
        foreach (var line in sector.Lines)
        {
            var check = GetNextLightSector(line, sector);
            if (check is not null && check.LightLevel < min)
                min = check.LightLevel;
        }

        return min;
    }

    public static double FindHighestFloorPoint(LightSector sector, VmPSectorSurroundHeights.Vertex? spotOut = null)
    {
        if (!sector.FloorIsSlope)
        {
            if (spotOut is not null)
            {
                if (sector.Lines.Count == 0)
                {
                    spotOut.X = 0;
                    spotOut.Y = 0;
                }
                else
                {
                    spotOut.X = sector.Lines[0].V1.X;
                    spotOut.Y = sector.Lines[0].V1.Y;
                }
            }

            return -sector.FloorPlaneD;
        }

        var height = double.NegativeInfinity;
        VmPSectorSurroundHeights.Vertex? spot = null;

        foreach (var line in sector.Lines)
        {
            var probeheight = sector.FloorPlane.ZAtPoint(line.V1.X, line.V1.Y);
            if (probeheight > height)
            {
                height = probeheight;
                spot = line.V1;
            }

            probeheight = sector.FloorPlane.ZAtPoint(line.V2.X, line.V2.Y);
            if (probeheight > height)
            {
                height = probeheight;
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
}
