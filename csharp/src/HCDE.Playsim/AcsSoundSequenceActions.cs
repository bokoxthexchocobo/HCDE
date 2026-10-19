using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native ACS <c>ACSF_SoundSequence*</c> from <c>p_acs.cpp</c> (name storage subset).</summary>
internal static class AcsSoundSequenceActions
{
    internal static void StartOnActor(AuthoritySimulation sim, int tid, string sequenceName, Actor? activator)
    {
        if (string.IsNullOrEmpty(sequenceName)) return;
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                activator.AcsSoundSequenceName = sequenceName;
            return;
        }
        foreach (var actor in sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid))
            actor.AcsSoundSequenceName = sequenceName;
    }

    internal static void StartOnSectors(AuthoritySimulation sim, int tag, string sequenceName)
    {
        if (string.IsNullOrEmpty(sequenceName)) return;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sim.SetSectorAcsSoundSequence(sector.Index, sequenceName);
        }
    }

    internal static void StartOnPolyobj(AuthoritySimulation sim, int polyId, string sequenceName)
    {
        if (string.IsNullOrEmpty(sequenceName)) return;
        sim.SetPolyobjAcsSoundSequence(polyId, sequenceName);
    }
}
