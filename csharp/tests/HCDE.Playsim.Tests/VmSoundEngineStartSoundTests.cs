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
            Type = 1,
            Attenuation = 0,
            SoundId = 1,
            Volume = 1,
            SfxVolume = 1,
            ChanFlags = StartSoundContext.ChanfLoop,
            NearLimit = 2,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
            CheckSoundLimit = () => true,
            LoadSoundLump = _ => 2,
            SfxEmptyLump = 1,
            GetChannel = _ => chan,
            StartSound2D = _ => chan,
        };
        Assert.NotNull(VmSoundEngineStartSound.StartSound(ctx));
        Assert.True((ctx.ChanFlags & StartSoundContext.ChanfEvicted) != 0);
    }

    [Fact]
    public void StartSound_InheritsNearLimitFromLinkedSfx()
    {
        var step = 0;
        var ctx = new StartSoundContext
        {
            Type = 1,
            SoundId = 1,
            Volume = 1,
            SfxVolume = 1,
            NearLimit = -1,
            Attenuation = 0,
            SfxAttenuation = 1f,
            IsValidSoundId = _ => true,
            SoundEnabled = () => true,
            ValidatePosVel = () => true,
            LoadSoundLump = _ => 2,
            SfxEmptyLump = 1,
            StartSound2D = _ => new object(),
        };
        ctx.SfxHasLink = () => step == 0;
        ctx.ResolveNextSfxLink = () =>
        {
            step++;
            ctx.NearLimit = 3;
            ctx.LimitRange = 512f;
            ctx.DefPitch = 0.9f;
            ctx.RolloffMinDistance = 128f;
            return true;
        };
        Assert.NotNull(VmSoundEngineStartSound.StartSound(ctx));
        Assert.Equal(3, ctx.NearLimit);
        Assert.Equal(512f, ctx.LimitRange);
        Assert.Equal(0f, ctx.Attenuation);
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
            SfxHasLink = () => true,
            ResolveNextSfxLink = () => false,
        };
        Assert.Null(VmSoundEngineStartSound.StartSound(ctx));
    }
}
