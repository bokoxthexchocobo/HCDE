namespace HCDE.Client.Tests;

public class VmReplayGainLookupTests
{
    [Fact]
    public void CachedGainResetsVolumeAndRefreshesGainsBeforeLoadingAndApplying()
    {
        var cache = new VmReplayGainCache();
        var calls = new List<string>();
        Assert.True(VmReplayGainLookup.ApplyCached(true, cache,
            value => calls.Add(value == 1 ? "reset" : "apply"), () => calls.Add("fluid"), () => calls.Add("module"),
            () => { calls.Add("hash"); return "key"; },
            () => { calls.Add("read"); cache.Store("key", -20); },
            hash => { Assert.Equal("key", hash); calls.Add("set-hash"); }));
        Assert.Equal(new[] { "reset", "fluid", "module", "read", "hash", "set-hash", "apply" }, calls);
    }

    [Fact]
    public void FailedMusicOpenSkipsCacheAndHashWork()
    {
        Assert.False(VmReplayGainLookup.ApplyCached(true, new(), _ => { }, () => { }, () => { },
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), () => false));
    }

    [Fact]
    public void DisabledGainOnlyResetsAndUpdatesSynthGains()
    {
        var levels = new List<float>();
        Assert.False(VmReplayGainLookup.ApplyCached(false, new(), levels.Add, () => { }, () => { },
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()));
        Assert.Equal(new float[] { 1 }, levels);
    }

    [Theory]
    [InlineData("")]
    [InlineData("missing")]
    public void MissingGainRetainsUnityAndOnlyNonemptyHashUpdatesCurrentHash(string hash)
    {
        var levels = new List<float>();
        var hashes = new List<string>();
        Assert.False(VmReplayGainLookup.ApplyCached(true, new(), levels.Add, () => { }, () => { },
            () => hash, () => { }, hashes.Add));
        Assert.Equal(new float[] { 1 }, levels);
        Assert.Equal(hash.Length == 0 ? Array.Empty<string>() : new[] { hash }, hashes);
    }
}
