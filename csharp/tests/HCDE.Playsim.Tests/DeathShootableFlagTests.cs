using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathShootableFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageDeathClearsShootableForMonstersAndPlayers(bool player)
    {
        var sim = Room(); var actor = player ? (Actor)sim.Players.Single() : sim.AddBot(0, 0, 3004);
        Assert.True(actor.Shootable); ActorDamage.Apply(actor, 1000);
        Assert.True(actor.IsDead); Assert.False(actor.Shootable);
    }

    [Fact]
    public void CurrentSaveAndRevivalPreserveThenRestoreShootability()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); ActorDamage.Apply(actor, 1000);
        actor.States.Enter(actor, ActorStateMachine.Corpse); var bytes = SimSavegame.Write(sim);
        actor.Shootable = true; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Shootable); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(ActorRaiseActions.RaiseSelf(actor)); Assert.True(actor.Shootable);
    }

    [Fact]
    public void DamageDeathCorpseAllowsRipperPositionProbe()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); ActorDamage.Apply(actor, 1000);
        actor.Solid = true;
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.ImpBall);
        missile.Rip = true; missile.X = missile.Y = missile.Z = default;
        Assert.Null(ActorJumpActions.CheckBlock(missile, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
