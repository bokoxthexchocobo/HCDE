using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::LineFromID</c> / <c>SideFromID</c> (<c>p_acs.cpp</c>).</summary>
internal static class AcsLineSideFromId
{
    public static int LineFromId(AuthoritySimulation sim, int lineId)
    {
        if (lineId == 0)
            return sim.AcsTriggerLine?.Index ?? -1;
        return AcsLineActivation.FirstLineFromId(sim, lineId)?.Index ?? -1;
    }

    public static int SideFromId(AuthoritySimulation sim, int lineId, int side)
    {
        if (side is not 0 and not 1)
            return -1;
        LevelLine? line;
        if (lineId == 0)
            line = sim.AcsTriggerLine;
        else
            line = AcsLineActivation.FirstLineFromId(sim, lineId);
        if (line is null)
            return -1;
        var sideIndex = side == 0 ? line.SideFront : line.SideBack;
        if (sideIndex < 0 || sideIndex >= sim.Level.Sides.Count)
            return -1;
        return sim.Level.Sides[sideIndex].Index;
    }
}
