using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native runtime plane scroll actions; map-start flags are separate.</summary>
internal static class ScrollActions
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, int tag, int xMove, int yMove, int mode,
        LevelLine? line = null)
    {
        if (special is not (LineSpecials.ScrollFloor or LineSpecials.ScrollCeiling)) return null;
        var dx = xMove / 32.0;
        var dy = yMove / 32.0;
        if (special == LineSpecials.ScrollCeiling)
            sim.SetTaggedScroller(tag, -dx, dy, SectorTextureScrollPlane.Ceiling);
        else
        {
            sim.SetTaggedScroller(tag, mode is 0 or 2 ? -dx : 0,
                mode is 0 or 2 ? dy : 0, SectorTextureScrollPlane.Floor);
            sim.SetTaggedScroller(tag, mode > 0 ? dx : 0, mode > 0 ? dy : 0, null);
        }
        return true;
    }
}
