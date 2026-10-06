using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontFallTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeathClearsNoGravityUnlessDontFall(bool dontFall)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); actor.Brain = null;
        actor.NoGravity = true; actor.Z = Fixed.FromInt(64);
        ActorPropertyActions.ChangeFlag(actor, "DONTFALL", dontFall); ActorDamage.Apply(actor, 1000);
        Assert.Equal(dontFall, actor.NoGravity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(dontFall ? 0 : -1, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void DontFallDoesNotEnableGravityFreeMovement()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); actor.DontFall = true;
        ActorDamage.Apply(actor, 1000); Assert.False(actor.NoGravity);
    }

    [Fact]
    public void LivingSavePreservesFlagForLaterDeath()
    {
        var sim = Room(); var actor = sim.Players.Single(); var legacy = SimSavegame.Write(sim);
        actor.DontFall = true; actor.NoGravity = true; var bytes = SimSavegame.Write(sim);
        var loaded = Room(); SimSavegame.Apply(loaded, bytes); var player = loaded.Players.Single();
        Assert.True(player.DontFall); Assert.Equal(bytes, SimSavegame.Write(loaded));
        ActorDamage.Apply(player, 1000); Assert.True(player.NoGravity);
        SimSavegame.Apply(loaded, legacy); Assert.False(player.DontFall);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
