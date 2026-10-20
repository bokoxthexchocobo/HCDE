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

    [Fact]
    public void ApplyIntProperty_RejectsBadNumber()
    {
        var stats = new VmEMapInfoStats();
        var value = 0;
        Assert.False(VmEMapInfoApplyHelpers.ApplyIntProperty(stats, "MAP01", "gravity", "x", 1, ref value));
        Assert.Equal(1, stats.Invalid);
    }

    [Fact]
    public void ApplyBossSpecials_ParsesE1M8()
    {
        var flags = 0;
        var flags3 = 0u;
        Assert.True(VmEMapInfoApplyHelpers.ApplyBossSpecials(ref flags, ref flags3, ["E1M8"]));
        Assert.NotEqual(0u, flags3);
    }

    [Fact]
    public void ApplyDefaultEnvironment_RecordsUnsupportedWhenMissing()
    {
        var stats = new VmEMapInfoStats();
        Assert.True(VmEMapInfoApplyHelpers.ApplyDefaultEnvironment(
            stats, "MAP01", "defaultenvironment", "1 2", 4, _ => null, out var packed, out _));
        Assert.Equal(0x0102, packed);
        Assert.Equal(1, stats.Unsupported);
    }

    [Fact]
    public void ApplyDefaultEnvironment_ResolvesKnownReverb()
    {
        var stats = new VmEMapInfoStats();
        Assert.True(VmEMapInfoApplyHelpers.ApplyDefaultEnvironment(
            stats, "MAP01", "defaultenvironment", "0 1", 2, _ => 0x0001, out var packed, out var id));
        Assert.Equal(0x0001, packed);
        Assert.Equal(0x0001, id);
        Assert.Equal(0, stats.Unsupported);
    }

    [Fact]
    public void ApplyDoubleProperty_RejectsBadToken()
    {
        var stats = new VmEMapInfoStats();
        var value = 0.0;
        Assert.False(VmEMapInfoApplyHelpers.ApplyDoubleProperty(stats, "MAP01", "gravity", "not-a-float", 1, ref value));
        Assert.Equal(1, stats.Invalid);
    }
}
