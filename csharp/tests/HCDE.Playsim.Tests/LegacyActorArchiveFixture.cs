namespace HCDE.Playsim.Tests;

internal static class LegacyActorArchiveFixture
{
    // These fixtures isolate version-specific layouts through version 27.
    // Version 28 round trips are covered by BlastEligibilityArchiveTests.
    internal static byte[] Write(AuthoritySimulation sim) => Write(sim.CaptureState());

    internal static byte[] Write(SimSaveState state)
    {
        foreach (var pose in state.Actors) { pose.BlastEligibilityFlags = null; pose.ThruActorsFlags = null; pose.MissileThruSpeciesFlags = null; pose.ThruSpeciesFlags = null; pose.ThruBits = null; pose.GhostFlags = null; pose.NonShootableFlags = null; pose.HitOwnerFlags = null; pose.SpectralFlags = null; }
        return SimSavegame.Write(state);
    }
}