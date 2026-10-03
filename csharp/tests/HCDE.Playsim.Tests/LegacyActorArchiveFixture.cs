namespace HCDE.Playsim.Tests;

internal static class LegacyActorArchiveFixture
{
    // These fixtures isolate version-specific layouts through version 27.
    // Current-format round trips are covered by BlastEligibilityArchiveTests.
    internal static byte[] Write(AuthoritySimulation sim) => Write(sim.CaptureState());

    internal static byte[] Write(SimSaveState state)
    {
        foreach (var pose in state.Actors) pose.BlastEligibilityFlags = null;
        return SimSavegame.Write(state);
    }
}