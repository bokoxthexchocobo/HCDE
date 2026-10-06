using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathFloatFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageDeathClearsFloatRegardlessOfDontFall(bool dontFall)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3005);
        actor.DontFall = dontFall; Assert.True(actor.Floating);
        ActorDamage.Apply(actor, 1000);
        Assert.False(actor.Floating); Assert.Equal(dontFall, actor.NoGravity);
    }

    [Fact]
    public void SaveRestoresClearedFloatAndResurrectionRestoresClassDefault()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3005); ActorDamage.Apply(actor, 1000);
        actor.States.Enter(actor, ActorStateMachine.Corpse);
        var bytes = SimSavegame.Write(sim); actor.Floating = true; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Floating); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(ActorRaiseActions.RaiseSelf(actor));
        Assert.True(actor.Floating); Assert.True(actor.NoGravity);
    }

    [Fact]
    public void DirectHealthAssignmentDoesNotRunDieMovementFlags()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3005); actor.Health = 0;
        Assert.True(actor.Floating); Assert.True(actor.NoGravity);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
