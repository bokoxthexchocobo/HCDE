using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnStompClassificationTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void MonsterFlagControlsStompIndependentlyOfBrain(bool isMonster, bool hasBrain)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Coop = true }],
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        var player = Assert.Single(sim.Players);
        var body = sim.AddBot(0, 0);
        body.Brain!.Enabled = false;
        if (!hasBrain) body.Brain = null;
        body.IsMonster = isMonster; body.Solid = false;
        var health = body.Health;
        ActorDamage.Apply(player, 1000);
        for (var i = 0; i < GameTicClock.TicRate; i++) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.False(player.IsDead);
        Assert.Equal(isMonster, body.IsDead);
        if (!isMonster) Assert.Equal(health, body.Health);
    }
}
