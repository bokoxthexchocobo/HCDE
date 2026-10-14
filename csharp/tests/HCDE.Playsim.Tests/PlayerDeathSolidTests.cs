using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDeathSolidTests
{
    [Fact]
    public void DamageDeathClearsSolidBeforeDeathStateAction()
    {
        var sim = Room(); var player = sim.Players.Single(); bool? observed = null;
        player.States.Configure(player, [new(-1, 0), new(-1, 1, a => observed = a.Solid)], 0); player.DeathState = 1;
        ActorDamage.Apply(player, 1000);
        Assert.False(player.Solid); Assert.False(observed);
    }

    [Fact]
    public void MissingDeathStateStillClearsPlayerSolid()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DeathState = -1;
        ActorDamage.Apply(player, 1000); Assert.False(player.Solid); Assert.False(player.Destroyed);
    }

    [Fact]
    public void MonsterDamageDeathDoesNotClearSolidEarly()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0);
        ActorDamage.Apply(actor, 1000); Assert.True(actor.Solid);
    }

    [Fact]
    public void SurvivingPlayerDamageKeepsSolid()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorDamage.Apply(player, 1); Assert.True(player.Solid);
    }

    [Fact]
    public void DeadPlayerSaveRestoresNonSolidFlag()
    {
        var sim = Room(); var player = sim.Players.Single(); ActorDamage.Apply(player, 1000);
        var saved = SimSavegame.Write(sim); player.Solid = true;
        SimSavegame.Apply(sim, saved); Assert.False(player.Solid); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RespawnRestoresClassDefaultSolidity(bool solid)
    {
        var sim = Room(); sim.AllowSinglePlayerRespawn = true; var player = sim.Players.Single();
        player.ResurrectionCollisionFlags = solid ? 3 : 2;
        ActorDamage.Apply(player, 1000); Assert.False(player.Solid);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead); Assert.Equal(solid, player.Solid);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
