namespace HCDE.Playsim;

internal static class SectorTextureScale
{
    public const int CeilingFixed = 170;
    public const int FloorFixed = 171;
    public const int Ceiling = 188;
    public const int Floor = 189;

    public static bool? Execute(AuthoritySimulation sim, int special, int tag, int arg1, int arg2, int arg3, int arg4)
    {
        if (special is not (CeilingFixed or FloorFixed or Ceiling or Floor)) return null;
        var fixedPoint = special is CeilingFixed or FloorFixed;
        var x = fixedPoint ? arg1 / 65536.0 : arg1 + arg2 / 100.0;
        var y = fixedPoint ? arg2 / 65536.0 : arg3 + arg4 / 100.0;
        var floor = special is Floor or FloorFixed;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            // Native scale actions invert nonzero factors; zero leaves that axis unchanged.
            if (floor)
            {
                if (x != 0) sector.FloorTextureScaleX = 1 / x;
                if (y != 0) sector.FloorTextureScaleY = 1 / y;
            }
            else
            {
                if (x != 0) sector.CeilingTextureScaleX = 1 / x;
                if (y != 0) sector.CeilingTextureScaleY = 1 / y;
            }
        }
        return true;
    }
}
