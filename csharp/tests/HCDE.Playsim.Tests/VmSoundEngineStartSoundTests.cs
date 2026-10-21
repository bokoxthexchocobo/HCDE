namespace HCDE.Playsim.Tests;

public class VmSoundEngineStartSoundTests
{
    [Fact]
    public void StartSound_RejectsWhenSfxDisabled()
    {
        var ctx = new StartSoundContext
        {
            SoundId = 1,
            Volume = 1,
            NoSfx = true,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
        };
        Assert.Null(VmSoundEngineStartSound.StartSound(ctx));
    }

    [Fact]
    public void StartSound_MarksLimitEvictButAllowsLooped()
    {
        var chan = new object();
        var ctx = new StartSoundContext
        {
            SoundId = 1,
            Volume = 1,
            SfxVolume = 1,
            ChanFlags = StartSoundContext.ChanfLoop,
            NearLimit = 2,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
            CheckSoundLimit = () => true,
            LoadSoundLump = _ => 1,
            StartSound2D = _ => chan,
        };
        Assert.NotNull(VmSoundEngineStartSound.StartSound(ctx));
        Assert.True((ctx.ChanFlags & StartSoundContext.ChanfEvicted) != 0);
    }

    [Fact]
    public void StartSound_InheritsNearLimitFromLinkedSfx()
    {
        var ctx = new StartSoundContext
        {
            SoundId = 1,
            Volume = 1,
            SfxVolume = 1,
            NearLimit = -1,
            Attenuation = 2f,
            SfxAttenuation = 0.5f,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
            LoadSoundLump = _ => 1,
            StartSound2D = _ => new object(),
        };
        ctx.SfxLinkFrames.Add(new SfxLinkResolveFrame(true, true, 3, 512f, 0.9f, 1.1f, 128f));
        Assert.NotNull(VmSoundEngineStartSound.StartSound(ctx));
        Assert.Equal(3, ctx.NearLimit);
        Assert.Equal(512f, ctx.LimitRange);
        Assert.Equal(1f, ctx.Attenuation);
    }

    [Fact]
    public void StartSound_RejectsStalledLink()
    {
        var ctx = new StartSoundContext
        {
            SoundId = 1,
            Volume = 1,
            SfxVolume = 1,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
        };
        ctx.SfxLinkFrames.Add(new SfxLinkResolveFrame(true, false, 0, 0, 0, 0, 0));
        Assert.Null(VmSoundEngineStartSound.StartSound(ctx));
    }
}
