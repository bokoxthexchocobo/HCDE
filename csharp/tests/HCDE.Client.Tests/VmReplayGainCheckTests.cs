namespace HCDE.Client.Tests;

public class VmReplayGainCheckTests
{
    [Fact]
    public void CacheHitSkipsCalculationAndSave()
    {
        var cache = new VmReplayGainCache();
        cache.Store("key", -20);
        var levels = new List<float>();
        Assert.True(VmReplayGainCheck.Check(true, true, cache, levels.Add, () => { }, () => { }, () => true,
            () => { }, () => "key", _ => { }, () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException()));
        Assert.Equal(new float[] { 1, .1f }, levels);
    }

    [Fact]
    public void CacheMissCalculatesAfterHashSelectionAndSavesAppliedResult()
    {
        var cache = new VmReplayGainCache();
        var calls = new List<string>();
        Assert.True(VmReplayGainCheck.Check(true, true, cache, _ => calls.Add("volume"),
            () => { }, () => { }, () => true, () => { }, () => "key", _ => calls.Add("hash"),
            () => { calls.Add("calculate"); return 20; }, () =>
            {
                Assert.True(cache.TryGet("key", out var gain));
                Assert.Equal(20, gain);
                calls.Add("save");
            }));
        Assert.Equal(new[] { "volume", "hash", "calculate", "volume", "save" }, calls);
    }

    [Theory]
    [InlineData(false, true, true, "key")]
    [InlineData(true, false, true, "key")]
    [InlineData(true, true, false, "key")]
    [InlineData(true, true, true, "")]
    public void BypassConditionsDoNotCalculate(bool enabled, bool calculate, bool opens, string hash)
    {
        Assert.False(VmReplayGainCheck.Check(enabled, calculate, new(), _ => { }, () => { }, () => { },
            () => opens, () => { }, () => hash, _ => { }, () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException()));
    }

    [Fact]
    public void UnavailableCalculationLeavesUnityAndDoesNotCreateCacheEntry()
    {
        var cache = new VmReplayGainCache();
        var levels = new List<float>();
        Assert.False(VmReplayGainCheck.Check(true, true, cache, levels.Add, () => { }, () => { }, () => true,
            () => { }, () => "key", _ => { }, () => null, () => throw new InvalidOperationException()));
        Assert.Equal(new float[] { 1 }, levels);
        Assert.False(cache.TryGet("key", out _));
    }
}
