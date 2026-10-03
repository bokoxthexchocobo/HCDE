namespace HCDE.Gamedata.Tests;
public class DehackedMonsterTableTests
{
    [Theory]
    [InlineData(13, "Demon", 3002, 150)]
    [InlineData(14, "Spectre", 58, 150)]
    [InlineData(15, "Cacodemon", 3005, 400)]
    [InlineData(16, "BaronOfHell", 3003, 1000)]
    [InlineData(18, "HellKnight", 69, 500)]
    [InlineData(19, "LostSoul", 3006, 100)]
    [InlineData(20, "SpiderMastermind", 7, 3000)]
    [InlineData(21, "Arachnotron", 68, 500)]
    [InlineData(22, "Cyberdemon", 16, 4000)]
    [InlineData(23, "PainElemental", 71, 400)]
    [InlineData(24, "WolfensteinSS", 84, 50)]
    public void NativeIndicesRetainCatalogDefaultsAndIdentityWhenPatched(int index, string name, int type, int health)
    {
        var baseline = DehackedPatch.CreateVanillaActors().Single(a => a.Index == index);
        Assert.Equal(name, baseline.Name); Assert.Equal(type, baseline.DoomEdNum); Assert.Equal(health, baseline.Health);
        var definition = DoomActorCatalog.Find(type)!;
        Assert.Equal((double)definition.Radius, baseline.Radius); Assert.Equal((double)definition.Height, baseline.Height);
        Assert.Equal((double)definition.Speed, baseline.Speed); Assert.Equal(definition.PainChance, baseline.PainChance);
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 4000\nHit points = 321\n");
        Assert.Empty(patch.Errors);
        var actor = patch.Actors.Single(a => a.Index == index);
        Assert.True(actor.Patched); Assert.Equal(4000, actor.DoomEdNum);
        Assert.Equal(type, actor.OriginalDoomEdNum); Assert.Equal(321, actor.Health);
        Assert.False(baseline.Patched); Assert.Equal(health, baseline.Health);
        Assert.Equal("BaronBall", patch.Actors.Single(a => a.Index == 17).Name);
        Assert.Equal(-1, patch.Actors.Single(a => a.Index == 17).DoomEdNum);
    }
}