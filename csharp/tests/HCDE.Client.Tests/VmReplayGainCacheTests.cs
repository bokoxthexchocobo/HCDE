namespace HCDE.Client.Tests;

public class VmReplayGainCacheTests
{
    [Fact]
    public void LoadRunsOnceAndLaterDuplicateKeysReplaceEarlierValues()
    {
        var cache = new VmReplayGainCache();
        cache.ReadOnce(() => new[] { new KeyValuePair<string, float>("hash", 1), new("hash", 2), new("HASH", 3) });
        cache.ReadOnce(() => throw new InvalidOperationException());
        Assert.True(cache.TryGet("hash", out var gain));
        Assert.Equal(2, gain);
        Assert.True(cache.TryGet("HASH", out gain));
        Assert.Equal(3, gain);
        Assert.False(cache.TryGet("missing", out _));
    }

    [Fact]
    public void NativeReadOnceFlagIsSetEvenWhenProviderFails()
    {
        var cache = new VmReplayGainCache();
        Assert.Throws<InvalidOperationException>(() => cache.ReadOnce(() => throw new InvalidOperationException()));
        cache.ReadOnce(() => throw new Exception("unexpected retry"));
    }

    [Fact]
    public void SaveEmitsInvariantSixDecimalValuesAndAnIndependentSnapshot()
    {
        var cache = new VmReplayGainCache();
        cache.Store("hash", -1.25f);
        IReadOnlyList<KeyValuePair<string, string>>? saved = null;
        cache.Save(entries => saved = entries);
        cache.Store("hash", 2);
        Assert.Equal(new KeyValuePair<string, string>("hash", "-1.250000"), Assert.Single(saved!));
    }

    [Fact]
    public void ManualGainStoresCappedValueAndPersistsBeforeApplyingAmplitude()
    {
        var cache = new VmReplayGainCache();
        var saved = false;
        cache.SetManualGain("hash", 20, entries =>
        {
            Assert.Equal("10.000000", Assert.Single(entries).Value);
            saved = true;
        }, amplitude => { Assert.True(saved); Assert.Equal(VmReplayGain.DecibelsToAmplitude(10), amplitude); });
        Assert.True(cache.TryGet("hash", out var gain));
        Assert.Equal(10, gain);
    }
}
