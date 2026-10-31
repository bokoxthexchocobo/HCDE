namespace HCDE.Playsim.Tests;

public class VmSoundEngineVolumePitchQuery1103SliceTests
{
    [Fact]
    public void ClampUnitVolume_ClampsHighAndLow()
    {
        Assert.Equal(0f, VmSoundEngineVolumePitchQuery1103Slice.ClampUnitVolume(-1f));
        Assert.Equal(1f, VmSoundEngineVolumePitchQuery1103Slice.ClampUnitVolume(2f));
    }

    [Fact]
    public void RunSetVolume_AppliesClampedValue()
    {
        float applied = -1f;
        VmSoundEngineVolumePitchQuery1103Slice.RunSetVolume(
            new VmSoundEngineVolumePitchQuery1103Slice.SetVolumeContext
            {
                RequestedVolume = 2f,
                ApplyBackend = v => applied = v,
                AssignChannelVolume = v => applied = v
            });
        Assert.Equal(1f, applied);
    }

    [Fact]
    public void CountPlayingInfo_CountsMatchingOrgId()
    {
        var source = new object();
        var count = VmSoundEngineVolumePitchQuery1103Slice.CountPlayingInfo(
            new VmSoundEngineVolumePitchQuery1103Slice.PlayingInfoContext
            {
                SourceType = 1,
                Source = source,
                ChannelFilter = -1,
                OrgSoundId = 5,
                ValidSoundId = true,
                Channels = new[]
                {
                    new VmSoundEngineVolumePitchQuery1103Slice.PlayingInfoRow
                    {
                        SourceType = 1,
                        Source = source,
                        OrgId = 5,
                        EntChannel = 0
                    }
                }
            });
        Assert.Equal(1, count);
    }

    [Fact]
    public void IsChannelUsed_UsesSeenBitmask()
    {
        var ctx = new VmSoundEngineVolumePitchQuery1103Slice.ChannelUsedContext
        {
            Channel = 2,
            SeenMask = 1 << 2
        };
        Assert.True(VmSoundEngineVolumePitchQuery1103Slice.IsChannelUsed(ctx));
    }

    [Fact]
    public void RunEvictChannel_SetsAbstimeWhenStopping()
    {
        var ctx = new VmSoundEngineVolumePitchQuery1103Slice.EvictChannelContext
        {
            ChanFlags = 0,
            HasSysChannel = true,
            BackendPresent = true,
            ReadPosition = () => 3.5f
        };
        VmSoundEngineVolumePitchQuery1103Slice.RunEvictChannel(ctx);
        Assert.True((ctx.ChanFlagsOut & VmSoundEngineVolumePitchQuery1103Slice.ChanfEvicted) != 0);
        Assert.Equal(3.5f, ctx.StartTime);
    }
}
