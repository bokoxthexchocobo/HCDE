using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native linedef <c>activation</c> / <c>SPAC_*</c> subset for ACS line queries.</summary>
internal static class AcsLineActivation
{
    public const int SpacCross = 1;
    public const int SpacUse = 2;
    public const int SpacUseThrough = 64;
    public const int SpacUseBack = 1024;

    public static int Pack(LevelLine line)
    {
        var value = 0;
        if (line.PlayerCross)
            value |= SpacCross;
        if (line.PlayerUse)
            value |= SpacUse;
        if (line.UseThrough)
            value |= SpacUseThrough;
        if (line.PlayerUseBack)
            value |= SpacUseBack;
        return value;
    }

    public static void Apply(LevelLine line, int activation, int repeat)
    {
        line.PlayerCross = (activation & SpacCross) != 0;
        line.PlayerUse = (activation & SpacUse) != 0;
        line.UseThrough = (activation & SpacUseThrough) != 0;
        line.PlayerUseBack = (activation & SpacUseBack) != 0;
        if (repeat > 0)
            line.Repeat = true;
        else if (repeat == 0)
            line.Repeat = false;
    }

    public static LevelLine? FirstLineFromId(AuthoritySimulation sim, int lineId)
    {
        foreach (var line in sim.Level.Lines)
        {
            if (line.HasId(lineId))
                return line;
        }

        return null;
    }

    public static IEnumerable<LevelLine> LinesFromId(AuthoritySimulation sim, int lineId)
    {
        foreach (var line in sim.Level.Lines)
        {
            if (line.HasId(lineId))
                yield return line;
        }
    }
}
