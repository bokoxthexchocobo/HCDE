namespace HCDE.Playsim.Tests;

public class VmSoundEngineStartSoundMid1102SliceTests
{
    [Fact]
    public void ResolveRolloffMinDistance_UsesGlobalWhenUnset()
    {
        var min = VmSoundEngineStartSoundMid1102Slice.ResolveRolloffMinDistance(
            forcedPresent: false,
            forcedMin: 0f,
            currentMin: 0f,
            globalMin: 256f);
        Assert.Equal(256f, min);
    }

    [Fact]
    public void RunSingularAndLimitGates_SetsEvictedWhenSingularPlaying()
    {
        var ctx = new VmSoundEngineStartSoundMid1102Slice.SingularLimitContext
        {
            SfxSingular = true,
            SoundId = 3,
            CheckSingular = _ => true
        };
        VmSoundEngineStartSoundMid1102Slice.RunSingularAndLimitGates(ctx);
        Assert.True((ctx.ChanFlags & VmSoundEngineStartSoundMid1102Slice.ChanfEvicted) != 0);
    }

    [Fact]
    public void ShouldAbortEvictedNonLoop_MatchesNativeGate()
    {
        Assert.True(VmSoundEngineStartSoundMid1102Slice.ShouldAbortEvictedNonLoop(
            VmSoundEngineStartSoundMid1102Slice.ChanfEvicted));
        Assert.False(VmSoundEngineStartSoundMid1102Slice.ShouldAbortEvictedNonLoop(
            VmSoundEngineStartSoundMid1102Slice.ChanfEvicted | VmSoundEngineStartSoundMid1102Slice.ChanfLoop));
    }

    [Fact]
    public void RunStopMatchingChannels_StopsUnattachedPointMatch()
    {
        var stopped = false;
        VmSoundEngineStartSoundMid1102Slice.RunStopMatchingChannels(
            new VmSoundEngineStartSoundMid1102Slice.StopChannelScanContext
            {
                Type = StartSoundContext.SourceUnattached,
                Channel = 0,
                ChannelInUse = true,
                Source = new object(),
                TargetX = 1f,
                TargetY = 2f,
                TargetZ = 3f,
                ActiveChannels = new[]
                {
                    new VmSoundEngineStartSoundMid1102Slice.ChannelSlot
                    {
                        SourceType = StartSoundContext.SourceUnattached,
                        EntChannel = 0,
                        PointX = 1f,
                        PointY = 2f,
                        PointZ = 3f
                    }
                },
                StopChannel = _ => stopped = true
            });
        Assert.True(stopped);
    }

    [Fact]
    public void MergePostStartChannelFlags_Sets3DWhenAttenuated()
    {
        var flags = VmSoundEngineStartSoundMid1102Slice.MergePostStartChannelFlags(0, 1f, 1);
        Assert.True((flags & VmSoundEngineStartSoundMid1102Slice.ChanfIs3D) != 0);
    }
}
