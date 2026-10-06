namespace HCDE.Playsim.Tests;

internal static class LegacyActorArchiveFixture
{
    // These fixtures isolate version-specific layouts through version 27.
    // Version 28 round trips are covered by BlastEligibilityArchiveTests.
    internal static byte[] Write(AuthoritySimulation sim) => Write(sim.CaptureState());

    internal static byte[] Write(SimSaveState state)
    {
        state.Lights = null;
        state.JumpRandomState = null;
        state.LightAnimation = null;
        foreach (var pose in state.Actors) pose.Killed = false;
        foreach (var pose in state.Actors) pose.ReactionTime = null;
        foreach (var pose in state.Actors) pose.AllowDropOffFlag = null;
        foreach (var pose in state.Actors) pose.TargetMemory = null;
        foreach (var pose in state.Actors) pose.GravityRaw = null;
        foreach (var pose in state.Actors) pose.ChaseThreshold = null;
        foreach (var pose in state.Actors) pose.DefenseProperties = null;
        foreach (var pose in state.Actors) pose.Tuning = null;
        foreach (var pose in state.Actors) pose.TeleFog = null;
        foreach (var pose in state.Actors) pose.Size = null;
        foreach (var pose in state.Actors) pose.SpeciesOverride = null;
        foreach (var pose in state.Actors) pose.Scale = null;
        foreach (var pose in state.Actors) pose.FloatBobPhase = null;
        foreach (var pose in state.Actors) pose.SpriteOrientation = null;
        foreach (var pose in state.Actors) { pose.BlastEligibilityFlags = null; pose.ThruActorsFlags = null; pose.MissileThruSpeciesFlags = null; pose.ThruSpeciesFlags = null; pose.ThruBits = null; pose.GhostFlags = null; pose.NonShootableFlags = null; pose.HitOwnerFlags = null; pose.SpectralFlags = null; }
        return SimSavegame.Write(state);
    }
}
