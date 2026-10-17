using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDamageRumbleTests
{
    [Fact]
    public void PainActionEmitsHapticBeforePainSound()
    {
        var sim = Room(); var player = sim.Players.Single(); var patterns = Listen(sim);
        sim.RumbleRequested += request =>
        { if (request.Pattern == "*pain") Assert.Empty(sim.PendingSoundRequests); };
        ActorDamage.Apply(player, 10);
        Assert.Equal("*pain", Assert.Single(patterns));
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound.StartsWith("*pain", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, true, 0)]
    [InlineData(true, false, 1)]
    public void DamagePreferenceIsIndependentOfWorldPreference(bool damage, bool world, int expected)
    {
        var sim = Room(); sim.DamageHapticsEnabled = damage; sim.WorldHapticsEnabled = world;
        var patterns = Listen(sim); ActorDamage.Apply(sim.Players.Single(), 10); Assert.Equal(expected, patterns.Count);
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound.StartsWith("*pain", StringComparison.Ordinal));
    }

    [Fact]
    public void NoPainDamageDoesNotRumbleMerelyBecauseHealthWasLost()
    {
        var sim = Room(); var patterns = Listen(sim); ActorDamage.Apply(sim.Players.Single(), 10, flags: DamageFlags.NoPain);
        Assert.Equal(90, sim.Players.Single().Health); Assert.Empty(patterns);
    }

    [Fact]
    public void CustomPainStateWithoutPainActionDoesNotRumble()
    {
        var sim = Room(); var player = sim.Players.Single(); var patterns = Listen(sim);
        player.States.Configure(player, [new(-1, 0), new(-1, 1)], 0); player.PainState = 1;
        ActorDamage.Apply(player, 10); Assert.Equal(1, player.States.Current); Assert.Empty(patterns);
    }

    [Fact]
    public void InvulnerabilityPreventsDamageAndPainRumble()
    {
        var sim = Room(); var player = sim.Players.Single(); var patterns = Listen(sim); player.Invulnerable = true;
        ActorDamage.Apply(player, 10); Assert.Equal(100, player.Health); Assert.Empty(patterns);
    }

    [Fact]
    public void DeathRumbleFollowsDeathStateAndPrecedesBonusCounterClearing()
    {
        var sim = Room(); var player = sim.Players.Single(); var calls = new List<string>(); player.BonusCount = 7;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, _ => calls.Add("state"))], 0);
        player.DeathState = 1; player.ExtremeDeathState = -1;
        sim.RumbleRequested += request =>
        { Assert.Equal("*death", request.Pattern); Assert.Equal(7, player.BonusCount); calls.Add("rumble"); };
        ActorDamage.Apply(player, 1000);
        Assert.Equal(new[] { "state", "rumble" }, calls); Assert.Equal(0, player.BonusCount);
    }

    [Fact]
    public void DeathHookReceivesInflictorAndPainHookReceivesSavedAttacker()
    {
        var sim = Room(); var source = sim.AddBot(100, 0); var inflictor = sim.AddBot(200, 0);
        var probe = new ProbePlayer { Simulation = sim, LastDamageSourceId = source.Id };
        ActorSoundActions.Pain(probe); Assert.Same(source, probe.HurtSource);
        ActorDamage.Apply(probe, 1000, source, inflictor: inflictor); Assert.Same(inflictor, probe.DeathSource);
    }

    [Fact]
    public void SavingRestoringAndFurtherCorpseDamageDoNotReplayDeathRumble()
    {
        var sim = Room(); var player = sim.Players.Single(); var patterns = Listen(sim);
        ActorDamage.Apply(player, 1000); Assert.Equal("*death", Assert.Single(patterns)); patterns.Clear();
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        ActorDamage.Apply(player, 10); Assert.Empty(patterns);
    }

    [Fact]
    public void DamagePreferenceDoesNotChangeGameplayChecksum()
    {
        var sim = Room(); sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        sim.DamageHapticsEnabled = false; sim.RestoreState(sim.CaptureState()); Assert.Equal(baseline, sim.Checksum);
    }

    private sealed class ProbePlayer : PlayerPawn
    {
        public Actor? HurtSource { get; private set; }
        public Actor? DeathSource { get; private set; }
        public override void PlayerHurtMakeRumble(Actor? source = null)
        { HurtSource = source; base.PlayerHurtMakeRumble(source); }
        public override void PlayerDiedMakeRumble(Actor? source = null)
        { DeathSource = source; base.PlayerDiedMakeRumble(source); }
    }

    private static List<string> Listen(AuthoritySimulation sim)
    { var patterns = new List<string>(); sim.RumbleRequested += request => patterns.Add(request.Pattern); return patterns; }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
