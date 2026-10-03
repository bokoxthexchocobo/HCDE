using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class VanillaSpeciesFamilyTests
{
    [Theory]
    [InlineData(58, 3002, false)]
    [InlineData(3002, 58, false)]
    [InlineData(69, 3003, false)]
    [InlineData(3003, 69, false)]
    [InlineData(58, 3002, true)]
    [InlineData(69, 3003, true)]
    public void RelatedMonstersShareDamageImmunityUnlessTargetHarmsSpecies(int targetType, int sourceType, bool harm)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        var target = sim.AddBot(0, 0, targetType); var source = sim.AddBot(100, 0, sourceType);
        Assert.True(target.IsSameSpecies(source)); Assert.True(source.IsSameSpecies(target));
        target.DoHarmSpecies = harm;
        Assert.Equal(!harm, target.ProjectileImmune(source));
        var health = target.Health; ActorDamage.Apply(target, 5, source);
        Assert.Equal(harm ? health - 5 : health, target.Health);
    }
    [Theory]
    [InlineData(58, 3002)]
    [InlineData(69, 3003)]
    public void ResolvedRemappedClassRetainsMonsterParentSpecies(int child, int parent)
    {
        // Supply a resolved catalog entry; the text parser's vanilla table does not yet include these classes.
        var patch = new DehackedPatchResult { Actors = [new DehackedActor {
            DoomEdNum = 4000, OriginalDoomEdNum = child, Patched = true, Health = 150 }] };
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 4000 }] }, dehacked: patch);
        var mapped = Assert.Single(sim.Actors); var dynamicMapped = sim.AddBot(100, 0, 4000);
        var original = sim.AddBot(200, 0, parent);
        Assert.True(mapped.IsSameSpecies(original)); Assert.True(dynamicMapped.IsSameSpecies(original));
        var health = mapped.Health; ActorDamage.Apply(mapped, 5, original); Assert.Equal(health, mapped.Health);
    }
    [Theory]
    [InlineData(58, 69)]
    [InlineData(3002, 3003)]
    public void DifferentMonsterFamiliesCanDamageEachOther(int targetType, int sourceType)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        var target = sim.AddBot(0, 0, targetType); var source = sim.AddBot(100, 0, sourceType);
        Assert.False(target.IsSameSpecies(source));
        var health = target.Health; ActorDamage.Apply(target, 5, source); Assert.Equal(health - 5, target.Health);
    }
}