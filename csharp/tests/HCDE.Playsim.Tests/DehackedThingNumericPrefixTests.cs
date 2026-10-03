using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedThingNumericPrefixTests
{
    [Theory]
    [InlineData("17 trailing", 17)]
    [InlineData("+17suffix", 17)]
    [InlineData("-5.75", -5)]
    [InlineData("0x10", 0)]
    [InlineData("12e3", 12)]
    [InlineData("invalid", 0)]
    [InlineData("+", 0)]
    [InlineData("- 5", 0)]
    [InlineData("4294967295suffix", -1)]
    public void DecimalPrefixControlsSpawnedReaction(string text, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nReaction time = {text}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(expected, Spawn(patch).ReactionTime);
    }

    [Theory]
    [InlineData("4294967296suffix")]
    [InlineData("-2147483649suffix")]
    [InlineData("999999999999999999999999suffix")]
    public void OutOfRangePrefixPreservesPreviousAssignment(string text)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nReaction time = 17\nReaction time = {text}\n");
        Assert.Single(patch.Errors);
        Assert.Equal(17, Spawn(patch).ReactionTime);
    }

    [Theory]
    [InlineData("2147483648", 32768.0)]
    [InlineData("4294967295", 65535.99998474121)]
    public void UnsignedFixedValuesRemainPositiveBeforeFloatingConversion(string text, double expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nHeight = {text}\nWidth = {text}\nSpeed = {text}\n");
        Assert.Empty(patch.Errors);
        var record = patch.Actors.Single(actor => actor.Index == 2);
        Assert.Equal(expected, record.Height);
        Assert.Equal(expected, record.Radius);
        Assert.Equal(expected, record.Speed);
    }

    [Fact]
    public void RepresentableFixedPrefixesReachSpawnedDimensions()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHeight = 98304suffix\nWidth = 98304suffix\nSpeed = 98304suffix\n");
        Assert.Empty(patch.Errors);
        var actor = Spawn(patch);
        Assert.Equal(1.5, actor.Height.ToDouble());
        Assert.Equal(1.5, actor.Radius.ToDouble());
        Assert.Equal(1.5, actor.MovementSpeed.ToDouble());
    }

    [Fact]
    public void PrefixParsingAlsoAppliesToSoundAndStateAssignments()
    {
        var patch = DehackedPatch.Apply("Thing 2\nAlert sound = 3suffix\nInitial frame = 47suffix\nPain chance = 65792suffix\n");
        Assert.Empty(patch.Errors);
        var record = patch.Actors.Single(actor => actor.Index == 2);
        Assert.Equal(3, record.SeeSound);
        Assert.Equal(47, record.SpawnState);
        Assert.Equal(256, record.PainChance);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
