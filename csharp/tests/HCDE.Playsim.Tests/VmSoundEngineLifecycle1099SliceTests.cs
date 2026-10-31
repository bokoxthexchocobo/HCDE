namespace HCDE.Playsim.Tests;

public class VmSoundEngineLifecycle1099SliceTests
{
    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, false, false)]
    public void ResolveSoundEnabled_MatchesNativeGate(bool enabled, bool noSound, bool noSfx, bool expected)
    {
        Assert.Equal(expected, VmSoundEngineLifecycle1099Slice.ResolveSoundEnabled(enabled, noSound, noSfx));
    }

    [Fact]
    public void RunCacheMarkedSounds_CachesUsedAndUnloadsUnlinked()
    {
        var used = new bool[4];
        var cached = new List<int>();
        var unloaded = new List<int>();
        var ctx = new VmSoundEngineLifecycle1099Slice.CacheMarkedSoundsContext
        {
            SfxCount = 4,
            IsUsed = i => used[i],
            ReadLink = _ => VmSoundEngineLifecycle1099Slice.SfxNoLink,
            CacheUsed = i => cached.Add(i),
            UnloadUnused = i => unloaded.Add(i),
            ChannelHead = () => null,
            MarkPlayingSound = _ => true,
            NextChannel = _ => null
        };
        used[2] = true;
        VmSoundEngineLifecycle1099Slice.RunCacheMarkedSounds(ctx);
        Assert.Equal([2], cached);
        Assert.Equal([1, 3], unloaded);
    }

    [Fact]
    public void RunInit_DrainsActiveChannelsBeforeCurveAssign()
    {
        var head = new object();
        var returns = 0;
        var ctx = new VmSoundEngineLifecycle1099Slice.InitContext
        {
            ActiveHead = head,
            ReturnChannel = _ =>
            {
                returns++;
                return null;
            },
            IncomingCurve = [1, 2],
            AssignCurve = _ => { }
        };
        VmSoundEngineLifecycle1099Slice.RunInit(ctx);
        Assert.Equal(1, returns);
        Assert.Null(ctx.ActiveHead);
    }

    [Fact]
    public void DefaultPitch_MatchesNativeEnum()
    {
        Assert.Equal(128, VmSoundEngineLifecycle1099Slice.DefaultPitch);
    }
}
