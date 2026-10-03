namespace HCDE.Playsim;

public static class SectorGravity
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, int tag, int integer, int fraction)
    {
        if (special != 216) return null;
        var gravity = integer + Math.Min(fraction, 99) * 0.01;
        foreach (var sector in sim.Level.Sectors)
            if (sector.MatchesTag(tag)) sector.Gravity = gravity;
        return true;
    }
}