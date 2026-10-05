using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialExitTests
{
    [Theory]
    [InlineData(243, false)]
    [InlineData(244, false)]
    [InlineData(243, true)]
    [InlineData(244, true)]
    public void DeathExitPreservesKillerAndWorldNoExitRules(int special, bool world)
    {
        var sim = Room(true); var victim = sim.Actors[1]; var player = sim.Players.Single();
        victim.Special = special;
        ActorDamage.Apply(victim, 1000, world ? null : player);
        Assert.Equal(world, sim.Exited); Assert.Equal(!world, player.IsDead);
        Assert.Equal(world && special == 244, sim.SecretExit);
        Assert.Equal(0, victim.Special);
    }

    [Theory]
    [InlineData(243, false)]
    [InlineData(244, false)]
    [InlineData(243, true)]
    [InlineData(244, true)]
    public void ExplicitExitClearsOnlySuccessfulSpecial(int special, bool noExit)
    {
        var sim = Room(noExit); var thing = sim.Actors[1]; var player = sim.Players.Single();
        thing.Special = special; thing.ActivationType = 32;
        Assert.Equal(!noExit, ActorSpecialActions.ActivateSpecial(sim, thing, player));
        Assert.Equal(!noExit, sim.Exited); Assert.Equal(noExit, player.IsDead);
        Assert.Equal(noExit ? special : 0, thing.Special);
        Assert.Equal(!noExit && special == 244, sim.SecretExit);
    }

    private static AuthoritySimulation Room(bool noExit) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch), noExit: noExit);
}
