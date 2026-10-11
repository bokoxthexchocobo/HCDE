using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseActiveSoundTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlockedChaseTurnsOnlyWhenStopIfBlockedIsAbsent(bool stop)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ReactionTime = 0;
        var flags = MonsterChaseFlags.NoPlayActive | (stop ? MonsterChaseFlags.StopIfBlocked : 0);
        for (var i = 0; i < 4; i++) monster.Brain!.Tick(sim, monster, flags);
        Assert.True(monster.VelocityX.Raw > 0);
        Assert.Equal(!stop, monster.VelocityY.Raw != 0);
        Assert.Null(sim.CaptureState().ChaseRandomState);
    }

    [Fact]
    public void ClearingStopIfBlockedRestoresObstacleSteeringOnNextCall()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ReactionTime = 0;
        for (var i = 0; i < 4; i++) monster.Brain!.Tick(sim, monster,
            MonsterChaseFlags.StopIfBlocked | MonsterChaseFlags.NoPlayActive);
        Assert.Equal(0, monster.VelocityY.Raw);
        monster.Brain!.Tick(sim, monster, MonsterChaseFlags.NoPlayActive);
        Assert.NotEqual(0, monster.VelocityY.Raw);
    }

    [Fact]
    public void ImmediateSoundCallbackCannotReenterChaseOrConsumeAnotherDraw()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = "idle" };
        var state = sim.CaptureState(); state.ChaseRandomState = 0; sim.RestoreState(state);
        monster = sim.Actors.Single(a => a.DoomEdNum == 3004); monster.ReactionTime = 0;
        var callbacks = 0;
        sim.SoundRequestsReady += () =>
        {
            callbacks++; var before = sim.CaptureState().ChaseRandomState;
            monster.Brain!.Tick(sim, monster);
            Assert.Equal(before, sim.CaptureState().ChaseRandomState);
        };
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(1, callbacks); Assert.Single(sim.PendingSoundRequests);
    }

    [Fact]
    public void ChaseGuardClearsWhenImmediateSoundCallbackThrows()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = "idle" };
        var state = sim.CaptureState(); state.ChaseRandomState = 0; sim.RestoreState(state);
        monster = sim.Actors.Single(a => a.DoomEdNum == 3004); monster.ReactionTime = 0;
        Action fail = () => throw new InvalidOperationException("callback failure");
        sim.SoundRequestsReady += fail;
        Assert.Throws<InvalidOperationException>(() => monster.Brain!.Tick(sim, monster));
        sim.SoundRequestsReady -= fail;
        var before = sim.CaptureState().ChaseRandomState;
        monster.Brain!.Tick(sim, monster);
        Assert.NotEqual(before, sim.CaptureState().ChaseRandomState);
    }

    [Theory]
    [InlineData(MonsterChaseFlags.None)]
    [InlineData(MonsterChaseFlags.DontLookAllAround)]
    public void VisualAcquisitionSkipsNoTargetPlayerUntilProtectionClears(MonsterChaseFlags flags)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var player = sim.Players.Single(); player.NoTarget = true; monster.ReactionTime = 0;
        monster.Brain!.Tick(sim, monster, flags);
        Assert.Null(monster.Brain.TargetId); Assert.Equal(MonsterMode.Idle, monster.Brain.Mode);
        Assert.Null(sim.CaptureState().ChaseRandomState);
        player.NoTarget = false; monster.Brain.Tick(sim, monster, flags);
        Assert.Equal(player.Id, monster.Brain.TargetId);
    }

    [Fact]
    public void NoTargetDiscardsAlreadyAcquiredPlayer()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        monster.Brain!.Tick(sim, monster); player.NoTarget = true;
        monster.Brain.Tick(sim, monster); Assert.Null(monster.Brain.TargetId);
    }

    [Theory]
    [InlineData(0, 500, true)]
    [InlineData(90, 500, true)]
    [InlineData(270, 500, true)]
    [InlineData(180, 500, false)]
    [InlineData(180, 84, true)]
    [InlineData(180, 85, false)]
    public void RestrictedLookUsesFrontHalfAndActorRadiusCloseException(double facing, int distance, bool acquired)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var player = sim.Players.Single(); player.X = Fixed.FromInt(distance);
        monster.Radius = Fixed.FromInt(20); monster.MeleeRange = Fixed.FromInt(64);
        monster.Angle = BamAngle.FromDegrees(facing); monster.ReactionTime = 0;
        monster.Brain!.Tick(sim, monster, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(acquired ? player.Id : (uint?)null, monster.Brain.TargetId);
    }

    [Fact]
    public void RestrictedLookRetainsAlreadyAcquiredPlayerBehindMonster()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ReactionTime = 0; monster.Brain!.Tick(sim, monster);
        monster.Angle = BamAngle.FromDegrees(180);
        monster.Brain.Tick(sim, monster, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(sim.Players.Single().Id, monster.Brain.TargetId);
    }

    [Theory]
    [InlineData(MonsterChaseFlags.None)]
    [InlineData(MonsterChaseFlags.NoPlayActive)]
    [InlineData(MonsterChaseFlags.DontMove)]
    [InlineData(MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.DontMove)]
    public void ChaseControlsMovementAndSoundDrawIndependently(MonsterChaseFlags flags)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = "idle" };
        var state = sim.CaptureState(); state.ChaseRandomState = 0; sim.RestoreState(state);
        monster = sim.Actors.Single(a => a.DoomEdNum == 3004); monster.ReactionTime = 0;
        monster.Brain!.Tick(sim, monster, flags);
        Assert.Equal(MonsterMode.Chase, monster.Brain.Mode);
        Assert.Equal((flags & MonsterChaseFlags.DontMove) == 0, monster.VelocityX.Raw != 0);
        var silent = (flags & MonsterChaseFlags.NoPlayActive) != 0;
        Assert.Equal(silent ? 0 : 1, sim.PendingSoundRequests.Count);
        Assert.Equal(silent, sim.CaptureState().ChaseRandomState == 0);
    }

    [Fact]
    public void UnsupportedChaseFlagsRejectBeforeChangingActorOrStream()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        var reaction = monster.ReactionTime;
        Assert.Throws<NotSupportedException>(() => monster.Brain!.Tick(sim, monster, (MonsterChaseFlags)1));
        Assert.Equal(reaction, monster.ReactionTime); Assert.Null(sim.CaptureState().ChaseRandomState);
    }

    [Fact]
    public void ChaseDrawTriggersActiveSoundAndSaveRestoresSequence()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Active] = "idle" };
        var state = sim.CaptureState(); state.ChaseRandomState = 0; sim.RestoreState(state);
        monster = sim.Actors.Single(a => a.DoomEdNum == 3004); monster.ReactionTime = 0;
        monster.Brain!.Tick(sim, monster);
        Assert.Equal(MonsterMode.Chase, monster.Brain.Mode);
        Assert.Equal("idle", Assert.Single(sim.DrainSoundRequests()).Sound);
        var savedRandom = sim.CaptureState().ChaseRandomState;
        Assert.NotEqual(0UL, savedRandom);
        var bytes = SimSavegame.Write(sim);
        var expected = Enumerable.Range(0, 8).Select(_ => sim.NextChaseByte()).ToArray();
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(savedRandom, sim.CaptureState().ChaseRandomState);
        Assert.Equal(expected, Enumerable.Range(0, 8).Select(_ => sim.NextChaseByte()).ToArray());
    }

    [Fact]
    public void ReactionCountdownStillDrawsAndLegacyRestoreClearsChaseStream()
    {
        var sim = Room(); var legacy = sim.CaptureState();
        var monster = sim.Actors.Single(a => a.DoomEdNum == 3004);
        monster.ReactionTime = 1; monster.Brain!.Tick(sim, monster);
        Assert.NotNull(sim.CaptureState().ChaseRandomState);
        monster.Brain.Tick(sim, monster);
        Assert.NotNull(sim.CaptureState().ChaseRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().ChaseRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = true }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }],
    });
}
