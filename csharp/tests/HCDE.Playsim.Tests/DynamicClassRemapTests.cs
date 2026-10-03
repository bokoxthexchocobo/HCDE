using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicClassRemapTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(12)]
    [InlineData(9)]
    [InlineData(4)]
    public void RemappedDynamicMonsterKeepsOriginalClassBehavior(int index)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 4000 }] }, dehacked: patch);
        var map = Assert.Single(sim.Actors); var bot = sim.AddBot(100, 0, 4000, 17);
        Assert.Equal(4000, bot.DoomEdNum); Assert.Equal(17, bot.ThingId);
        Assert.Equal(map.Brain!.Attack, bot.Brain!.Attack);
        Assert.Equal(map.RaiseDuration, bot.RaiseDuration); Assert.Equal(map.Mass, bot.Mass);
        Assert.Equal(map.Damage, bot.Damage); Assert.Equal(map.Health, bot.Health);
        Assert.Equal(map.Radius, bot.Radius); Assert.Equal(map.Height, bot.Height);
    }
    [Fact]
    public void ExplicitNumericOverridesStillWinAfterRemapping()
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\nMass = 321\nMissile damage = 17\nReaction time = 7\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch); var bot = sim.AddBot(0, 0, 4000);
        Assert.Equal(MonsterAttack.Fireball, bot.Brain!.Attack);
        Assert.Equal(321, bot.Mass); Assert.Equal(17, bot.Damage); Assert.Equal(7, bot.ReactionTime);
    }
    [Fact]
    public void OriginalEditorNumberDoesNotSelectRemappedPatch()
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\nHit points = 999\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch); var bot = sim.AddBot(0, 0, 3001);
        Assert.Equal(60, bot.Health); Assert.Equal(3001, bot.DoomEdNum);
        Assert.Equal(MonsterAttack.Fireball, bot.Brain!.Attack);
    }
}