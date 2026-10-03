using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class ParsedMonsterRemappingTests
{
    [Theory]
    [InlineData(13, 3002, 58)]
    [InlineData(14, 58, 3002)]
    [InlineData(16, 3003, 69)]
    [InlineData(18, 69, 3003)]
    public void TextPatchRemapsMapAndDynamicMonstersWithoutLosingFamily(int index, int original, int relative)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\nHit points = 321\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 4000 }] }, dehacked: patch);
        var mapped = Assert.Single(sim.Actors); var dynamicMapped = sim.AddBot(100, 0, 4000);
        var sibling = sim.AddBot(200, 0, relative);
        foreach (var actor in new[] { mapped, dynamicMapped })
        {
            Assert.Equal(321, actor.Health); Assert.Equal(4000, actor.DoomEdNum);
            Assert.True(actor.IsSameSpecies(sibling));
            ActorDamage.Apply(actor, 5, sibling); Assert.Equal(321, actor.Health);
            actor.DoHarmSpecies = true; ActorDamage.Apply(actor, 5, sibling); Assert.Equal(316, actor.Health);
        }
        Assert.Equal(original, patch.Actors.Single(a => a.Index == index).OriginalDoomEdNum);
    }
}