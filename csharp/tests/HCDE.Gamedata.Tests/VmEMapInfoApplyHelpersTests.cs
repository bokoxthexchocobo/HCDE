namespace HCDE.Gamedata.Tests;

public class VmEMapInfoApplyHelpersTests
{
    [Fact]
    public void ApplyBoolProperty_RecordsInvalidOnBadToken()
    {
        var stats = new VmEMapInfoStats();
        var result = false;
        Assert.False(VmEMapInfoApplyHelpers.ApplyBoolProperty(stats, "MAP01", "nosound", "maybe", 3, ref result));
        Assert.Equal(1, stats.Invalid);
        Assert.Single(stats.UnsupportedSamples);
    }

    [Fact]
    public void TryParseDefaultEnvironmentPair_AcceptsTwoInts()
    {
        Assert.True(VmEMapInfoApplyHelpers.TryParseDefaultEnvironmentPair("1 2", out var a, out var b));
        Assert.Equal(1, a);
        Assert.Equal(2, b);
        Assert.Equal(0x0102, VmEMapInfoApplyHelpers.PackEnvironmentId(a, b));
    }
}
