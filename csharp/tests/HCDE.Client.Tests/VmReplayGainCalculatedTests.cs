namespace HCDE.Client.Tests;

public class VmReplayGainCalculatedTests
{
    [Fact]
    public void CalculatedGainIsUncappedAndStoredBeforeVolumeAndSave()
    {
        var cache = new VmReplayGainCache();
        var applied = false;
        var result = VmReplayGainCalculated.Apply("hash", 20, cache, amplitude =>
        {
            Assert.True(cache.TryGet("hash", out var gain));
            Assert.Equal(20, gain);
            Assert.Equal(10, amplitude);
            applied = true;
        }, () => Assert.True(applied));
        Assert.Equal(10, result);
    }

    [Fact]
    public void SaveFailureRetainsAlreadyAppliedVolumeAndCache()
    {
        var cache = new VmReplayGainCache();
        var applied = false;
        Assert.Throws<InvalidOperationException>(() => VmReplayGainCalculated.Apply("hash", -20, cache,
            _ => applied = true, () => throw new InvalidOperationException("save failed")));
        Assert.True(applied);
        Assert.True(cache.TryGet("hash", out var gain));
        Assert.Equal(-20, gain);
    }

    [Fact]
    public void DecoderAnalysisFeedsCacheAndPreservesNativeInsufficientSampleResult()
    {
        var cache = new VmReplayGainCache();
        var saves = 0;
        var amplitude = VmReplayGainCalculated.AnalyzeAndApply("empty", new(800, 8000, 1, false),
            _ => throw new InvalidOperationException(), _ => false, cache, value => Assert.Equal(0, value), () => saves++);
        Assert.Equal(0, amplitude);
        Assert.True(cache.TryGet("empty", out var gain));
        Assert.Equal(-24601, gain);
        Assert.Equal(1, saves);
    }

    [Fact]
    public void DecoderFailureDoesNotStoreApplyOrSave()
    {
        var cache = new VmReplayGainCache();
        Assert.Throws<InvalidOperationException>(() => VmReplayGainCalculated.AnalyzeAndApply("hash",
            new(800, 8000, 1, false), _ => throw new Exception("unexpected float"),
            _ => throw new InvalidOperationException("decode failed"), cache,
            _ => throw new Exception("unexpected apply"), () => throw new Exception("unexpected save")));
        Assert.False(cache.TryGet("hash", out _));
    }
}
