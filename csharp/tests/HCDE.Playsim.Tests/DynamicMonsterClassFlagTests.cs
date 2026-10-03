using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicMonsterClassFlagTests
{
    [Theory]
    [InlineData(3005, true, false)]
    [InlineData(3006, true, false)]
    [InlineData(71, true, false)]
    [InlineData(16, false, true)]
    [InlineData(7, false, true)]
    [InlineData(3001, false, false)]
    public void DynamicClassFlagsMatchMapSpawning(int type, bool flying, bool splashImmune)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        var mapped = Assert.Single(sim.Actors); var dynamicActor = sim.AddBot(100, 0, type);
        Assert.Equal(mapped.NoGravity, dynamicActor.NoGravity); Assert.Equal(flying, dynamicActor.NoGravity);
        Assert.Equal(mapped.Floating, dynamicActor.Floating); Assert.Equal(flying, dynamicActor.Floating);
        Assert.Equal(mapped.NoRadiusDamage, dynamicActor.NoRadiusDamage); Assert.Equal(splashImmune, dynamicActor.NoRadiusDamage);
    }
    [Theory]
    [InlineData(15, true, false)]
    [InlineData(22, false, true)]
    public void TextRemappingPreservesClassFlags(int index, bool flying, bool splashImmune)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\nHit points = 321\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var actor = sim.AddBot(0, 0, 4000);
        Assert.Equal(flying, actor.NoGravity); Assert.Equal(flying, actor.Floating);
        Assert.Equal(splashImmune, actor.NoRadiusDamage);
    }
    [Theory]
    [InlineData(6, false)]
    [InlineData(16902, true)] // SOLID | SHOOTABLE | NOGRAVITY | FLOAT
    public void ExplicitPrimaryFlagsOverrideFlyingDefaults(int bits, bool flying)
    {
        var patch = DehackedPatch.Apply($"Thing 15\nBits = {bits}\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var actor = sim.AddBot(0, 0, 3005); actor.Brain = null;
        Assert.Equal(flying, actor.NoGravity); Assert.Equal(flying, actor.Floating);
        actor.Z = Fixed.FromInt(32); actor.OnGround = false;
        sim.Tick();
        Assert.Equal(flying ? 32 : 31, actor.Z.ToDouble());
        Assert.Equal(flying ? 0 : -1, actor.VelocityZ.ToDouble());
    }
    [Theory]
    [InlineData(16)]
    [InlineData(7)]
    public void DynamicBossIgnoresNearbyArchvileSplashButTakesDirectDamage(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        var archvile = sim.AddBot(-200, 0, 64); var target = sim.AddBot(0, 0, 3004);
        var boss = sim.AddBot(0, 40, type); var health = boss.Health;
        ArchvileActions.Attack(sim, archvile, target, fireExists: true);
        Assert.Equal(health, boss.Health);
        Assert.True(target.Health < target.ResurrectionHealth);
        ActorDamage.Apply(boss, 20, archvile); Assert.Equal(health - 20, boss.Health);
    }
}