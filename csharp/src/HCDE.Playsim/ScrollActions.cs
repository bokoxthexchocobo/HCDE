using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>Scroll_Floor</c> carry subset and <c>Scroll_Ceiling</c> activation stub. Texture scroll and displacement <c>DScroller</c> thinkers are absent; carry rates are stored and summed each tic.</summary>
internal static class ScrollActions
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, int tag, int xMove, int yMove, int mode,
        LevelLine? line = null)
    {
        if (special == LineSpecials.ScrollCeiling)
        {
            var touched = false;
            foreach (var _ in LineSpecials.TargetSectors(sim, line, tag))
                touched = true;
            return touched;
        }
        if (special != LineSpecials.ScrollFloor) return null;
        var dx = xMove / 32.0;
        var dy = yMove / 32.0;
        var any = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag))
        {
            any = true;
            if (mode > 0)
                sim.SetSectorScroll(sector, dx, dy);
            else
                sim.SetSectorScroll(sector, 0, 0);
        }
        return any;
    }
}
