namespace HCDE.Playsim.Tests;

public class VmSoundEngineCacheChannel1100SliceTests
{
    [Fact]
    public void RunCacheSound_LoadsSingleWhenNotRandom()
    {
        var loaded = false;
        VmSoundEngineCacheChannel1100Slice.RunCacheSound(new VmSoundEngineCacheChannel1100Slice.CacheSoundContext
        {
            BackendPresent = true,
            Tentative = false,
            RandomHeader = false,
            LoadSingle = () => loaded = true,
            MarkUsed = () => { }
        });
        Assert.True(loaded);
    }

    [Fact]
    public void RunUnloadSound_UnloadsWhenValidAndAlwaysClears()
    {
        var unloaded = false;
        var cleared = false;
        VmSoundEngineCacheChannel1100Slice.RunUnloadSound(new VmSoundEngineCacheChannel1100Slice.UnloadSoundContext
        {
            DataValid = true,
            UnloadBackend = () => unloaded = true,
            ClearData = () => cleared = true
        });
        Assert.True(unloaded);
        Assert.True(cleared);
    }

    [Fact]
    public void ShouldPersistActiveChannel_ExcludesUiAndTransient()
    {
        Assert.False(VmSoundEngineCacheChannel1100Slice.ShouldPersistActiveChannel(
            VmSoundEngineCacheChannel1100Slice.ChanfUi));
        Assert.True(VmSoundEngineCacheChannel1100Slice.ShouldPersistActiveChannel(0));
    }

    [Fact]
    public void ListChannelsShouldInclude_SkipsEvicted()
    {
        Assert.False(VmSoundEngineCacheChannel1100Slice.ListChannelsShouldInclude(
            VmSoundEngineRestartChannel.ChanfEvicted));
    }
}
