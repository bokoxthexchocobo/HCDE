using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRadiusSelfTests
{
    [Theory]
    [InlineData(0, 0, 0, 100)]
    [InlineData(50, 0, 0, 50)]
    [InlineData(0, 0, 50, 50)]
    [InlineData(30, 40, 0, 50)]
    [InlineData(100, 0, 0, 0)]
    [InlineData(101, 0, 0, 0)]
    public void TargetDamageUsesStrictThreeDimensionalFalloff(int x, int y, int z, int expected)
    {
        var sim = Room(); var source = sim.AddBot(0, 0); var target = sim.Players.Single(); target.ThingId = 7; target.X = Fixed.FromInt(x); target.Y = Fixed.FromInt(y);
        target.Z = Fixed.FromInt(z); source.Brain!.SetTargetThingId(sim, 7); target.Health = 500;
        ActorRadiusSelfActions.Apply(source, 100, 100);
        Assert.Equal(500 - expected, target.Health);
    }

    [Fact]
    public void FrameActionAppliesDamageAndSaveRestoresHealth()
    {
        var sim = Room(); var source = sim.AddBot(0, 0); var target = sim.Players.Single(); target.ThingId = 7; target.X = Fixed.FromInt(50);
        target.Health = 500; source.Brain!.SetTargetThingId(sim, 7);
        source.States.Configure(source, [new(-1, 0), new(-1, 0, Action: self => ActorRadiusSelfActions.Apply(self, 100, 100))], 0);
        source.States.Enter(source, 1); Assert.Equal(450, target.Health);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        target.Health = 400; sim.RestoreState(state); Assert.Equal(450, target.Health);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BfgRollsReplayWithSavedCombatState()
    {
        var sim = Room(); var source = sim.AddBot(0, 0); var target = sim.Players.Single(); target.ThingId = 7; target.X = Fixed.FromInt(50);
        source.Brain!.SetTargetThingId(sim, 7); target.Health = 500;
        var state = sim.CaptureState(); ActorRadiusSelfActions.Apply(source, 10, 100, 1);
        var health = target.Health; Assert.InRange(500 - health, 5, 40);
        sim.RestoreState(state); ActorRadiusSelfActions.Apply(source, 10, 100, 1); Assert.Equal(health, target.Health);
    }

    [Fact]
    public void MissingTargetAndInvalidParametersDoNotDamage()
    {
        var actor = Room().AddBot(0, 0);
        ActorRadiusSelfActions.Apply(actor);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorRadiusSelfActions.Apply(actor, distance: double.NaN));
        Assert.Throws<NotSupportedException>(() => ActorRadiusSelfActions.Apply(actor, flags: 2));
        Assert.Equal(20, actor.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
