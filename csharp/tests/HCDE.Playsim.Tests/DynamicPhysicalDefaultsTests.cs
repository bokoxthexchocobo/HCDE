using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicPhysicalDefaultsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(98304, 1.5)]
    [InlineData(1048576, 16)]
    public void DynamicDimensionsMatchPatchedMapSpawn(int encoded, double expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nWidth = {encoded}\nHeight = {encoded}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var mapActor = Assert.Single(sim.Actors); var bot = sim.AddBot(100, 0);
        Assert.Equal(expected, bot.Radius.ToDouble()); Assert.Equal(expected, bot.Height.ToDouble());
        Assert.Equal(mapActor.Radius, bot.Radius); Assert.Equal(mapActor.Height, bot.Height);
    }
    [Theory]
    [InlineData("8", 2)]
    [InlineData("98304", 0.375)]
    [InlineData("-4", 0)]
    [InlineData("2147483647", 30)]
    public void DynamicSpeedUsesConvertedEncodingAndManagedLimit(string encoded, double expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nSpeed = {encoded}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var bot = sim.AddBot(100, 0);
        Assert.Equal(expected, bot.ChaseSpeed);
        Assert.Equal(sim.Actors[0].ChaseSpeed, bot.ChaseSpeed);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(123)]
    [InlineData(-1)]
    public void DynamicPainChanceUsesPatchedSignedValue(int chance)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nPain chance = {chance}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var bot = sim.AddBot(100, 0);
        Assert.Equal(chance, bot.PainChance); Assert.Equal(sim.Actors[0].PainChance, bot.PainChance);
    }
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
}