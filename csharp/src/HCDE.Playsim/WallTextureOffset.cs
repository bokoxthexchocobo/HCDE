using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class WallTextureOffset
{
    public const int Special = 53;
    private const int NoChange = 32767 << 16;

    public static bool? Execute(AuthoritySimulation sim, int special, int id, int x, int y, int side, int flags)
    {
        if (special != Special) return null;
        if (id == 0 || side is < 0 or > 1) return false;
        if (id == -1) return true; // Native line-ID manager excludes the unset sentinel.
        foreach (var line in AcsLineActivation.LinesFromId(sim, id))
        {
            var index = side == 0 ? line.SideFront : line.SideBack;
            if ((uint)index >= (uint)sim.Level.Sides.Count) continue;
            Apply(sim.Level.Sides[index], x, y, flags);
        }
        return true;
    }

    private static void Apply(LevelSide side, int x, int y, int flags)
    {
        var add = (flags & 8) != 0;
        if ((flags & (int)WallScrollParts.Top) != 0)
        {
            if (x != NoChange) side.TopTextureOffsetX = Offset(side.TopTextureOffsetX, x, add);
            if (y != NoChange) side.TopTextureOffsetY = Offset(side.TopTextureOffsetY, y, add);
        }
        if ((flags & (int)WallScrollParts.Mid) != 0)
        {
            if (x != NoChange) side.MidTextureOffsetX = Offset(side.MidTextureOffsetX, x, add);
            if (y != NoChange) side.MidTextureOffsetY = Offset(side.MidTextureOffsetY, y, add);
        }
        if ((flags & (int)WallScrollParts.Bottom) != 0)
        {
            if (x != NoChange) side.BottomTextureOffsetX = Offset(side.BottomTextureOffsetX, x, add);
            if (y != NoChange) side.BottomTextureOffsetY = Offset(side.BottomTextureOffsetY, y, add);
        }
    }

    private static double Offset(double current, int value, bool add) =>
        (add ? current : 0) + value / 65536.0;
}
