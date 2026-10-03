using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullDamagePropertyTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(0)]
    [InlineData(-5)]
    public void ChargeCollisionUsesActorDamage(int damage)
    {
        var sim = Room();
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        var player = sim.Players.Single();
        soul.Damage = damage;
        soul.Brain!.StartCharge(soul, player);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) sim.Tick();
        Assert.False(soul.Brain.Charging);
        var dealt = 100 - player.Health;
        if (damage <= 0) Assert.Equal(0, dealt);
        else { Assert.InRange(dealt, damage, damage * 8); Assert.Equal(0, dealt % damage); }
    }

    [Fact]
    public void AllSupportedSoulSpawnPathsInitializeDamageThree()
    {
        var sim = Room();
        Assert.Equal(3, sim.Actors.Single(actor => actor.DoomEdNum == 3006).Damage);
        Assert.Equal(3, sim.AddBot(400, 300, doomEdNum: 3006).Damage);
        var parent = sim.AddBot(800, 300, doomEdNum: 71);
        Assert.Equal(3, sim.SpawnLostSoul(parent, null, 0)!.Damage);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }],
    });
}
