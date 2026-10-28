using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class SectorChangeFlagsSpecial
{
    internal const int Special = 54;
    private const int NoModify = unchecked((int)0xC0000000);

    internal static bool? Execute(AuthoritySimulation sim, int special, int tag, int setBits, int clearBits)
    {
        if (special != Special) return null;
        if (tag == 0) return false;
        setBits &= ~NoModify;
        clearBits &= ~NoModify;
        var changed = false;
        foreach (var sector in sim.Level.Sectors.Where(candidate => candidate.MatchesTag(tag)))
        {
            sector.RuntimeFlags = (sector.RuntimeFlags | setBits) & ~clearBits;
            changed = true;
        }
        return changed;
    }
}
