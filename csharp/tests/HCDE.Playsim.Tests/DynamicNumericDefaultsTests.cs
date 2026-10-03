using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicNumericDefaultsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(75)]
    [InlineData(300)]
    [InlineData(0)]
    [InlineData(-1)]
    public void DynamicHealthMatchesMapSpawnAndResurrectionBaseline(int health)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nHit points = {health}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        var mapActor = Assert.Single(sim.Actors); var bot = sim.AddBot(100, 0);
        Assert.Equal(mapActor.Health, bot.Health); Assert.Equal(health, bot.Health);
        Assert.Equal(mapActor.ResurrectionHealth, bot.ResurrectionHealth);
        if (health > 0) Assert.Equal(mapActor.GibHealth, bot.GibHealth);
        Assert.Equal(health <= 0, bot.IsDead);
    }
    [Fact]
    public void DynamicSpawnAppliesReactionMassAndDamageWithRuntimeEffect()
    {
        var patch = DehackedPatch.Apply("Thing 2\nReaction time = 7\nMass = 321\nMissile damage = 17\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch); var bot = sim.AddBot(0, 0);
        Assert.Equal(7, bot.ReactionTime); Assert.Equal(321, bot.Mass); Assert.Equal(17, bot.Damage);
        sim.Tick(); Assert.Equal(6, bot.ReactionTime);
    }
    [Fact]
    public void OtherClassRetainsCatalogHealthAndMass()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 75\nMass = 321\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch); var bot = sim.AddBot(0, 0, 3001);
        Assert.Equal(DoomActorCatalog.Find(3001)!.Health, bot.Health);
        Assert.Equal(DoomActorCatalog.MassOf(3001), bot.Mass);
    }
}