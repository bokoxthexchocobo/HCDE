namespace HCDE.Playsim;

internal static class SectorTexturePanning
{
    public const int Ceiling = 186;
    public const int Floor = 187;

    public static bool? Execute(AuthoritySimulation sim, int special, int tag, int xInteger, int xFraction, int yInteger, int yFraction)
    {
        if (special is not (Floor or Ceiling)) return null;
        var x = xInteger + xFraction / 100.0;
        var y = yInteger + yFraction / 100.0;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            if (special == Floor)
            {
                sector.FloorTextureOffsetX = x;
                sector.FloorTextureOffsetY = y;
            }
            else
            {
                sector.CeilingTextureOffsetX = x;
                sector.CeilingTextureOffsetY = y;
            }
        }
        return true;
    }
}
