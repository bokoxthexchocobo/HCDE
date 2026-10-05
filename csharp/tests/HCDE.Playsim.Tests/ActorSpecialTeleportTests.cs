using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialTeleportTests
{
    [Theory]
    [InlineData(70, false)]
    [InlineData(70, true)]
    [InlineData(154, false)]
    [InlineData(154, true)]
    public void TeleportMovesActivatorWithNativeVelocityPolicy(int special, bool death)
    {
        var sim = Room(); var player = sim.Players.Single(); var thing = sim.Actors[1];
        Set(thing, special, 7); player.VelocityX = Fixed.FromInt(3); player.ReactionTime = 7;
        if (death) ActorDamage.Apply(thing, 1000, player);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, thing, player));
        Assert.Equal(200, player.X.ToDouble()); Assert.Equal(90, player.Angle.ToDegrees());
        Assert.Equal(special == 154 ? 3 : 0, player.VelocityX.ToDouble());
        Assert.Equal(special == 154 ? 7 : 18, player.ReactionTime);
        Assert.Equal(20, thing.X.ToDouble()); Assert.Equal(death ? 0 : special, thing.Special);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FailedTeleportPreservesExplicitSpecial(int failure)
    {
        var sim = Room(); var player = sim.Players.Single(); var thing = sim.Actors[1];
        Set(thing, 70, failure == 0 ? 99 : 7); thing.ActivationType = 32;
        player.NoTeleport = failure == 1;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, thing, failure == 2 ? null : player));
        Assert.Equal(70, thing.Special); Assert.Equal(0, player.X.ToDouble());
    }

    [Fact]
    public void ThingActsTeleportsThingInsteadOfTrigger()
    {
        var sim = Room(); var thing = sim.Actors[1]; Set(thing, 70, 7); thing.ActivationType = 1;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, thing, sim.Players.Single()));
        Assert.Equal(200, thing.X.ToDouble()); Assert.Equal(0, sim.Players.Single().X.ToDouble());
    }

    private static void Set(Actor actor, int special, int tid)
    {
        actor.Special = special; actor.SpecialArgs[0] = tid;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 20 },
            new LevelThing { Type = 14, Id = 7, X = 200, Angle = 90 }],
    });
}
