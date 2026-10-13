namespace HCDE.Client.Tests;

public class VmReplayGainIniWriteTests
{
    [Fact]
    public void MergeUpdatesFirstCaseInsensitiveKeyAndRetainsOtherSectionsAndUnknownGains()
    {
        var text = "[Other]\nsetting=yes\n[Gains]\nHASH=1\nunknown=2\n[Last]\nvalue=3\n";
        var merged = VmReplayGainIni.Merge(text, [new("hash", "4.000000"), new("new", "5.000000")]);
        Assert.Contains("[Other]\nsetting=yes\n", merged);
        Assert.Contains("hash=4.000000\nunknown=2\nnew=5.000000\n[Last]\nvalue=3\n", merged);
    }

    [Fact]
    public void DiskSaveCreatesFileAndRoundTripsCache()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ini");
        try
        {
            var cache = new VmReplayGainCache();
            cache.Store("hash", -1.25f);
            VmReplayGainIni.SaveFile(cache, path);
            var loaded = new VmReplayGainCache();
            VmReplayGainIni.ReadFileOnce(loaded, path);
            Assert.True(loaded.TryGet("hash", out var gain));
            Assert.Equal(-1.25f, gain);
            cache.Store("hash", 2);
            VmReplayGainIni.SaveFile(cache, path);
            Assert.Single(VmReplayGainIni.Parse(File.ReadAllText(path)));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void UnrepresentableKeyIsRejectedBeforeWriting()
        => Assert.Throws<InvalidDataException>(() => VmReplayGainIni.Merge("", [new("bad\nkey", "1")]));
}
