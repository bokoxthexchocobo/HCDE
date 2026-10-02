namespace HCDE.Playsim;

internal static class WallTextureScale
{
    public const int Special = 56;
    private const int NoChange = 32767 << 16;

    public static bool? Execute(AuthoritySimulation sim, int special, int id, int x, int y, int side, int flags)
    {
        if (special != Special) return null;
        if (id == 0 || side is < 0 or > 1) return false;
        if (id == -1) return true;
        var multiply = (flags & 8) != 0;
        foreach (var line in AcsLineActivation.LinesFromId(sim, id))
        {
            var index = side == 0 ? line.SideFront : line.SideBack;
            if ((uint)index >= (uint)sim.Level.Sides.Count) continue;
            var target = sim.Level.Sides[index];
            if ((flags & (int)WallScrollParts.Top) != 0)
            {
                if (x != NoChange) target.TopTextureScaleX = Scale(target.TopTextureScaleX, x, multiply);
                if (y != NoChange) target.TopTextureScaleY = Scale(target.TopTextureScaleY, y, multiply);
            }
            if ((flags & (int)WallScrollParts.Mid) != 0)
            {
                if (x != NoChange) target.MidTextureScaleX = Scale(target.MidTextureScaleX, x, multiply);
                if (y != NoChange) target.MidTextureScaleY = Scale(target.MidTextureScaleY, y, multiply);
            }
            if ((flags & (int)WallScrollParts.Bottom) != 0)
            {
                if (x != NoChange) target.BottomTextureScaleX = Scale(target.BottomTextureScaleX, x, multiply);
                if (y != NoChange) target.BottomTextureScaleY = Scale(target.BottomTextureScaleY, y, multiply);
            }
        }
        return true;
    }

    private static double Scale(double current, int value, bool multiply) =>
        multiply ? current * (value / 65536.0) : value == 0 ? 1 : value / 65536.0;
}
