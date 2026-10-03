using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicMonsterCatalogTests
{
    [Theory]
    [InlineData(3004, 20, 56, 2, 200)]
    [InlineData(9, 20, 56, 2, 170)]
    [InlineData(65, 20, 56, 2, 170)]
    [InlineData(84, 20, 56, 2, 170)]
    [InlineData(3001, 20, 56, 2, 200)]
    [InlineData(3002, 30, 56, 2.5, 180)]
    [InlineData(58, 30, 56, 2.5, 180)]
    [InlineData(3005, 31, 56, 2, 128)]
    [InlineData(3003, 24, 64, 2, 50)]
    [InlineData(69, 24, 64, 2, 50)]
    [InlineData(64, 20, 56, 3.75, 10)]
    [InlineData(66, 20, 56, 2.5, 100)]
    [InlineData(67, 48, 64, 2, 80)]
    [InlineData(68, 64, 64, 3, 128)]
    [InlineData(71, 31, 56, 2, 128)]
    [InlineData(3006, 16, 56, 2, 256)]
    [InlineData(16, 40, 110, 4, 20)]
    [InlineData(7, 128, 100, 3, 40)]
    public void DynamicMonstersUseNativeCatalogDimensionsSpeedAndPain(int type, int radius, int height, double chaseSpeed, int pain)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        var mapped = Assert.Single(sim.Actors); var actor = sim.AddBot(500, 0, type);
        Assert.Equal(radius, actor.Radius.ToDouble()); Assert.Equal(height, actor.Height.ToDouble());
        Assert.Equal(chaseSpeed, actor.ChaseSpeed); Assert.Equal(pain, actor.PainChance);
        Assert.Equal(mapped.Radius, actor.Radius); Assert.Equal(mapped.Height, actor.Height);
        Assert.Equal(mapped.ChaseSpeed, actor.ChaseSpeed); Assert.Equal(mapped.PainChance, actor.PainChance);
    }
    [Fact]
    public void UnknownDynamicClassRetainsFallbackDimensionsAndSpeed()
    {
        var actor = AuthoritySimulation.Start(new PlayLevel()).AddBot(0, 0, 4000);
        Assert.Equal(20, actor.Radius.ToDouble()); Assert.Equal(56, actor.Height.ToDouble());
        Assert.Equal(1, actor.ChaseSpeed); Assert.Equal(256, actor.PainChance);
    }
}