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
        state.ChaseRandomState = null;
        state.MissileDamageRandomState = null;
        state.PoisonDamageRandomState = null;
        state.PainChanceRandomState = null;
        state.SkelFistRandomState = null;
        state.SargAttackRandomState = null;
        state.TroopAttackRandomState = null;
        state.HeadAttackRandomState = null;
        state.BruisAttackRandomState = null;
        state.PosAttackRandomState = null;
        state.SPosAttackRandomState = null;
        state.CPosAttackRandomState = null;
        state.CPosRefireRandomState = null;
        foreach (var pose in state.Actors) pose.DeathDamageAmount = 0;
        foreach (var pose in state.Actors) pose.RepeatAttack = null;
        foreach (var pose in state.Actors) pose.ProfileAttack = null;
        foreach (var pose in state.Actors) pose.RevenantAttack = null;
        foreach (var pose in state.Actors) pose.ArchvileAttack = null;
        foreach (var pose in state.Actors) pose.LostSoulAttack = null;
        foreach (var pose in state.Actors) pose.OnlySlamSolid = false;
        foreach (var pose in state.Actors) pose.CausePain = false;
        foreach (var pose in state.Actors) pose.AllowPain = false;
        foreach (var pose in state.Actors) pose.NoDamage = false;
        foreach (var pose in state.Actors) pose.PlayerDrainStrength = 0;
        foreach (var pose in state.Actors) pose.PlayerGodMode2 = false;
        foreach (var pose in state.Actors) pose.PlayerBuddha2 = false;
        foreach (var pose in state.Actors) pose.PlayerGodMode = false;
        foreach (var pose in state.Actors) pose.PlayerDamageCount = 0;
        foreach (var pose in state.Actors) pose.LaxTelefragDamage = false;
        foreach (var pose in state.Actors) pose.SelfDamageFactor = 65536;
        foreach (var pose in state.Actors) pose.PainType = null;
        foreach (var pose in state.Actors) pose.RetargetAfterSlam = false;
        foreach (var pose in state.Actors) pose.BrainDisabled = false;
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
