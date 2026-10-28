namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::DoCheckActorTexture</c> same-sector floor/ceiling flat match.</summary>
public static class AcsCheckActorTextureService
{
    public static bool TryCheck(
        AuthoritySimulation sim,
        int tid,
        Actor? activator,
        string textureName,
        bool floor)
    {
        var actor = tid == 0 ? activator : AcsActorTid.SingleFromTid(sim, tid);
        if (actor is null || actor.Destroyed || string.IsNullOrEmpty(textureName))
            return false;

        var sectorIndex = actor.SectorIndex;
        if (sectorIndex < 0 || sectorIndex >= sim.Level.Sectors.Count)
            return false;

        var sector = sim.Level.Sectors[sectorIndex];
        var flat = floor ? sector.FloorPic : sector.CeilingPic;
        return string.Equals(flat, textureName, StringComparison.OrdinalIgnoreCase);
    }
}
