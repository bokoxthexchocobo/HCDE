namespace HCDE.MapLoader;

public static class LevelValidation
{
    public static bool TryValidate(PlayLevel level, out string? error)
    {
        error = null;
        foreach (var vertex in level.Vertices)
            if (!Coordinate(vertex.X) || !Coordinate(vertex.Y)) { error = "map-coordinate-out-of-range"; return false; }
        foreach (var side in level.Sides)
            if ((uint)side.Sector >= (uint)level.Sectors.Count) { error = "map-side-sector-invalid"; return false; }
        foreach (var line in level.Lines)
        {
            if ((uint)line.V1 >= (uint)level.Vertices.Count || (uint)line.V2 >= (uint)level.Vertices.Count)
            { error = "map-line-vertex-invalid"; return false; }
            if ((uint)line.SideFront >= (uint)level.Sides.Count || line.SideBack < -1 || line.SideBack >= level.Sides.Count)
            { error = "map-line-side-invalid"; return false; }
            if (line.X1 == line.X2 && line.Y1 == line.Y2) { error = "map-line-zero-length"; return false; }
        }
        foreach (var sector in level.Sectors)
        {
            if (!Coordinate(sector.FloorHeight) || !Coordinate(sector.CeilingHeight))
            { error = "map-sector-height-out-of-range"; return false; }
            if (sector.FloorHeight > sector.CeilingHeight) { error = "map-sector-inverted"; return false; }
        }
        foreach (var thing in level.Things)
            if (!Coordinate(thing.X) || !Coordinate(thing.Y) || !Coordinate(thing.Z) || !double.IsFinite(thing.Angle)
                || thing.Type is < 0 or > ushort.MaxValue)
            { error = "map-thing-out-of-range"; return false; }
        return true;
    }

    private static bool Coordinate(double value) => double.IsFinite(value) && value >= -32768 && value <= 32767;
}
