using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AutomaticNativeMovementTests
{
    [Fact]
    public void AutomaticTickUsesNativeDirectionSelectionWhenEnabled()
    {
        var (sim, actor) = Room(true);
        sim.Tick();
        Assert.Equal(Fixed.FromInt(8), actor.X);
        Assert.Equal(default, actor.VelocityX);
        Assert.NotNull(sim.CaptureState().TryWalkRandomState);
    }

    [Fact]
    public void DefaultAutomaticTickRetainsExistingMovementPolicy()
    {
        var (sim, actor) = Room(false);
        sim.Tick();
        Assert.True(actor.X.Raw > 0);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
        Assert.False(sim.NativeMonsterMovement);
    }

    [Fact]
    public void MovementPolicyParticipatesInChecksum()
    {
        var (native, _) = Room(true);
        var (legacy, _) = Room(false);
        native.RestoreState(native.CaptureState());
        legacy.RestoreState(legacy.CaptureState());
        Assert.NotEqual(native.Checksum, legacy.Checksum);
    }

    [Fact]
    public void AutomaticNativeMovementContinuesAfterFreshSimulationLoad()
    {
        var (sim, _) = Room(true);
        sim.Tick();
        var saved = SimSavegame.Write(sim, true);
        for (var i = 0; i < 5; i++) sim.Tick();
        var expected = SimSavegame.Write(sim, true);
        var (restored, _) = Room(true);
        SimSavegame.Apply(restored, saved);
        for (var i = 0; i < 5; i++) restored.Tick();
        Assert.Equal(expected, SimSavegame.Write(restored, true));
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation, Actor) Room(bool native)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, NoAttack = true }],
            Things = [new LevelThing { Type = 1, X = 300 }]
        }, spawnOptions: new SpawnOptions(NativeMonsterMovement: native));
        var actor = sim.AddBot(0, 0);
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0;
        actor.MovementSpeed = Fixed.FromInt(8);
        actor.SpawnState = actor.IdleState = 0;
        actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
