namespace HCDE.Gamedata.Tests;

public class DehackedFrameNumericPrefixTests
{
    [Theory]
    [InlineData("+17suffix", 17)]
    [InlineData("-5.75", -1)]
    [InlineData("40000suffix", 32767)]
    [InlineData("4294967295suffix", -1)]
    [InlineData("4294967296suffix", 0)]
    [InlineData("0x10", 0)]
    [InlineData("invalid", 0)]
    public void DurationNarrowsDecimalPrefixBeforeClamping(string text, int expected)
    {
        var patch = DehackedPatch.Apply($"Frame 47\nDuration = {text}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(expected, patch.States.Single(state => state.Index == 47).Tics);
    }

    [Fact]
    public void PrefixAssignmentsPreserveSpriteFlagsAndChainedBaseline()
    {
        var first = DehackedPatch.Apply("Frame 47\nUnknown 1 = -5suffix\nUnknown 2 = 4294967295suffix\nSprite number = 3suffix\nNext frame = 48suffix\nSprite subnumber = 32770suffix\n");
        var second = DehackedPatch.Apply("Frame 47\nDuration = 12suffix\n", first);
        Assert.Empty(first.Errors);
        Assert.Empty(second.Errors);
        var state = second.States.Single(state => state.Index == 47);
        Assert.Equal(-5, state.Misc1);
        Assert.Equal(-1, state.Misc2);
        Assert.Equal(3, state.Sprite);
        Assert.Equal(48, state.NextState);
        Assert.Equal(2, state.Frame);
        Assert.True(state.FullBright);
        Assert.True(state.Patched);
        Assert.Equal(12, state.Tics);
        Assert.Equal(5, first.States.Single(state => state.Index == 47).Tics);
    }
}
