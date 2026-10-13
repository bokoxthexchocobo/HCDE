namespace HCDE.Client.Tests;

public class VmReplayGainIniTests
{
    [Fact]
    public void GainsParseCommentsSectionsAndNativeDecimalPrefixes()
    {
        var entries = VmReplayGainIni.Parse("#comment\n[other]\na=9\n[gAiNs]\n hash = -1.25tail\nexp=2e1suffix\nbad=oops\n//comment\n");
        Assert.Equal(new[] { new KeyValuePair<string, float>("hash", -1.25f), new("exp", 20), new("bad", 0) }, entries);
    }

    [Fact]
    public void DiskLoadReplacesDuplicateKeysAndRunsOnlyOnce()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "[Gains]\nhash=1\nhash=2\n");
            var cache = new VmReplayGainCache();
            VmReplayGainIni.ReadFileOnce(cache, path);
            File.WriteAllText(path, "[Gains]\nhash=3\n");
            VmReplayGainIni.ReadFileOnce(cache, path);
            Assert.True(cache.TryGet("hash", out var gain));
            Assert.Equal(2, gain);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void MissingFileLoadsAnEmptyCache()
    {
        var cache = new VmReplayGainCache();
        VmReplayGainIni.ReadFileOnce(cache, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ini"));
        Assert.False(cache.TryGet("hash", out _));
    }

    [Fact]
    public void SpecialFloatPrefixesAreParsedWithoutAcceptingUnicodeDigits()
    {
        var entries = VmReplayGainIni.Parse("[Gains]\na=-infinitytail\nb=+nan(payload)\nc=\u0661\n");
        Assert.Equal(float.NegativeInfinity, entries[0].Value);
        Assert.True(float.IsNaN(entries[1].Value));
        Assert.Equal(0, entries[2].Value);
    }

    [Fact]
    public void UnsupportedMultilineAndUnsectionedEntriesReportErrors()
    {
        Assert.Throws<InvalidDataException>(() => VmReplayGainIni.Parse("hash=1"));
        Assert.Throws<InvalidDataException>(() => VmReplayGainIni.Parse("[Gains]\nhash=<<<END\n"));
    }
}
