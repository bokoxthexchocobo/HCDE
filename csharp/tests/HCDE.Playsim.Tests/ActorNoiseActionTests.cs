using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorNoiseActionTests
{
    [Theory]
    [InlineData(AlertMonsterFlags.None, false)]
    [InlineData(AlertMonsterFlags.TargetEmitter, true)]
    [InlineData(AlertMonsterFlags.TargetNonPlayer, true)]
    [InlineData(AlertMonsterFlags.EmitFromTarget, false)]
    public void MonsterAlertSelectsNativeTargetKinds(AlertMonsterFlags flags, bool alerts)
    {
        var (sim, emitter, target, listener) = Room(); emitter.SetTargetPointer(target);
        ActorNoiseActions.AlertMonsters(emitter, flags: flags);
        var expected = (flags & AlertMonsterFlags.TargetEmitter) != 0 ? emitter.Id : target.Id;
        Assert.Equal(alerts ? expected : (uint?)null, listener.LastHeardTargetId);
    }

    [Fact]
    public void EmitFromTargetChangesRangeOrigin()
    {
        var (_, emitter, target, listener) = Room(); emitter.SetTargetPointer(target);
        ActorNoiseActions.AlertMonsters(emitter, 100, AlertMonsterFlags.TargetNonPlayer);
        Assert.Equal(target.Id, listener.LastHeardTargetId); listener.LastHeardTargetId = null;
        ActorNoiseActions.AlertMonsters(emitter, 100, AlertMonsterFlags.TargetNonPlayer | AlertMonsterFlags.EmitFromTarget);
        Assert.Null(listener.LastHeardTargetId);
    }

    [Fact]
    public void NoiseAlertUsesTargetAndWorksAsFrameAction()
    {
        var (_, emitter, target, listener) = Room(); emitter.SetTargetPointer(target);
        emitter.States.Configure(emitter, [new(-1, 0, ActorNoiseActions.NoiseAlert)], 0);
        Assert.Equal(target.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void MissingTargetDoesNotClearExistingNoise()
    {
        var (_, emitter, target, listener) = Room(); listener.LastHeardTargetId = target.Id;
        ActorNoiseActions.NoiseAlert(emitter); ActorNoiseActions.AlertMonsters(emitter);
        Assert.Equal(target.Id, listener.LastHeardTargetId);
    }

    [Fact]
    public void SoundAlertWrapperPassesSplashFilter()
    {
        var (_, emitter, target, listener) = Room(); listener.NoSplashAlert = true;
        ActorNoiseActions.SoundAlert(emitter, target, splash: true); Assert.Null(listener.LastHeardTargetId);
        ActorNoiseActions.SoundAlert(emitter, target); Assert.Equal(target.Id, listener.LastHeardTargetId);
    }

    private static (AuthoritySimulation Sim, Actor Emitter, Actor Target, Actor Listener) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 1000 },
                new LevelThing { Type = 3002, X = 100 }] });
        return (sim, sim.Actors[0], sim.Actors[1], sim.Actors[2]);
    }
}
