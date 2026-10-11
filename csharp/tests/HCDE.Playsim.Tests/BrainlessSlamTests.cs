using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BrainlessSlamTests
{
    [Theory]
    [InlineData(false, false, 1, 89)]
    [InlineData(true, false, 2, 100)]
    [InlineData(false, true, 0, 100)]
    public void DefaultSlamSupportsOrdinaryActorDamageAndStates(bool dormant, bool dead, int state, int health)
    {
        var sim = Room(); var target = sim.Players.Single(); target.NoPain = true;
        var actor = new Actor
        {
            Simulation = sim, Level = sim.Level, Health = dead ? 0 : 100, Dormant = dormant,
            VelocityX = Fixed.FromInt(3), VelocityY = Fixed.FromInt(4), VelocityZ = Fixed.FromInt(5),
            SeeState = 1, IdleState = 2, DamageFunction = _ => 11,
        };
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(7, 1), new ActorFrame(9, 2)], 0);
        Assert.False(actor.Slam(target)); Assert.Null(actor.Brain);
        Assert.Equal(default, actor.VelocityX); Assert.Equal(default, actor.VelocityY); Assert.Equal(default, actor.VelocityZ);
        Assert.Equal(state, actor.States.Current); Assert.Equal(health, target.Health);
        if (dormant) Assert.Equal(-1, actor.States.RemainingTics);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void OnlySlamSolidPassPreservesBrainlessActorVelocity()
    {
        var sim = Room(); var target = sim.Players.Single(); target.Solid = target.Shootable = false;
        var actor = new Actor { OnlySlamSolid = true, VelocityX = Fixed.FromInt(3) };
        Assert.True(actor.Slam(target)); Assert.Equal(Fixed.FromInt(3), actor.VelocityX);
        Assert.Equal(100, target.Health); Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
