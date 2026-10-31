namespace HCDE.Playsim.Tests;

public class VmSoundEngineListResolveStart1101SliceTests
{
    [Fact]
    public void RunListSoundChannels_SkipsEvictedAndCounts()
    {
        var appended = 0;
        var count = VmSoundEngineListResolveStart1101Slice.RunListSoundChannels(
            new VmSoundEngineListResolveStart1101Slice.ListSoundChannelsContext
            {
                ChannelFlags = new[] { 0, VmSoundEngineRestartChannel.ChanfEvicted, 0 },
                AppendListedChannel = _ => appended++
            });
        Assert.Equal(2, count);
        Assert.Equal(2, appended);
    }

    [Fact]
    public void RunResolveSound_AppliesRandomAttenuation()
    {
        var ctx = new VmSoundEngineListResolveStart1101Slice.ResolveSoundContext
        {
            RandomHeader = true,
            Attenuation = 2f,
            HeaderAttenuation = 0.5f,
            CurrentSoundId = 7,
            PickReplacement = id => id + 1
        };
        var resolved = VmSoundEngineListResolveStart1101Slice.RunResolveSound(ctx);
        Assert.Equal(8, resolved);
        Assert.Equal(1f, ctx.Attenuation);
    }

    [Fact]
    public void CalcPitch_UsesDefPitchRangeWhenSet()
    {
        var pitch = VmSoundEngineListResolveStart1101Slice.CalcPitch(
            0,
            0.8f,
            1.2f,
            sndPitched: false,
            randomPitch: () => 1);
        Assert.Equal(1.2f, pitch, 3);
    }

    [Fact]
    public void TryRunStartSoundPrologue_RejectsInvalidVolume()
    {
        var ok = VmSoundEngineListResolveStart1101Slice.TryRunStartSoundPrologue(
            new VmSoundEngineListResolveStart1101Slice.StartSoundPrologueContext
            {
                SoundId = 1,
                Volume = 0,
                IsValidSoundId = _ => true,
                SoundEnabled = () => true
            });
        Assert.False(ok);
    }

    [Fact]
    public void TryAdvanceSfxLink_AbortsWhenResolveReturnsSameSfx()
    {
        var ctx = new VmSoundEngineListResolveStart1101Slice.SfxLinkStepContext
        {
            CurrentSoundId = 1,
            ResolvedSoundId = 2,
            SameSfxAfterResolve = true,
            IsValidSoundId = _ => true,
            ResolveLinkedSound = c =>
            {
                c.ResolvedSoundId = 2;
                return true;
            }
        };
        Assert.False(VmSoundEngineListResolveStart1101Slice.TryAdvanceSfxLink(ctx));
    }
}
