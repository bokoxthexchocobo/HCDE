namespace HCDE.MapLoader;

public static class LevelLiftMarkers
{
    /// <summary>Marks native repeatable platform sectors at level initialization.</summary>
    public static void Apply(PlayLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (level.Format == MapDataFormat.Unknown) return;
        foreach (var line in level.Lines)
        {
            int tag;
            if (level.UsesDoomSectorNumbers)
            {
                var classic = line.Special is 62 or 88 or 120 or 123;
                var generic = line.Special is >= 0x3400 and <= 0x37ff
                    && (line.Special & 1) != 0 && (line.Special & 0x300) == 0;
                if (!classic && !generic) continue;
                tag = line.Tag;
            }
            else
            {
                if (!(line.Repeat || (line.Flags & LevelLine.RepeatSpecialFlag) != 0)) continue;
                if (line.Special is not (62 or 64 or 172 or 206) && !(line.Special == 203 && line.Arg3 == 1)) continue;
                tag = line.Arg0;
            }
            if (tag != 0)
            {
                foreach (var sector in level.Sectors)
                    if (sector.HasTag(tag)) sector.IsLift = true;
            }
            else if ((uint)line.SideBack < (uint)level.Sides.Count)
            {
                var sector = level.Sides[line.SideBack].Sector;
                if ((uint)sector < (uint)level.Sectors.Count) level.Sectors[sector].IsLift = true;
            }
        }
    }
}
