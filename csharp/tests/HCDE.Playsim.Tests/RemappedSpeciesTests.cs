using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class RemappedSpeciesTests
{
    [Theory]
    [InlineData(2, 3004)]
    [InlineData(12, 3001)]
    [InlineData(9, 67)]
    public void RemappedAndOriginalEditorNumbersRetainSameClassSpecies(int index, int original)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 4000 }] }, dehacked: patch);
        var remapped = Assert.Single(sim.Actors); var originalActor = sim.AddBot(100, 0, original);
        Assert.NotEqual(remapped.DoomEdNum, originalActor.DoomEdNum);
        Assert.True(remapped.IsSameSpecies(originalActor)); Assert.True(originalActor.IsSameSpecies(remapped));
        Assert.True(remapped.ProjectileImmune(originalActor));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageGateUsesClassSpeciesAndHonorsDoHarmSpecies(bool harm)
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch);
        var target = sim.AddBot(0, 0, 4000); var source = sim.AddBot(100, 0, 3001);
        target.DoHarmSpecies = harm;
        var health = target.Health; ActorDamage.Apply(target, 5, source);
        Assert.Equal(harm ? health - 5 : health, target.Health);
    }
    [Fact]
    public void DifferentClassesAndPlayersRemainDistinct()
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        var remapped = sim.AddBot(0, 0, 4000); var zombie = sim.AddBot(100, 0, 3004);
        Assert.False(remapped.IsSameSpecies(zombie)); Assert.False(remapped.ProjectileImmune(zombie));
        Assert.False(remapped.IsSameSpecies(Assert.Single(sim.Players)));
        Assert.False(Assert.Single(sim.Players).IsSameSpecies(Assert.Single(sim.Players)));
    }
}