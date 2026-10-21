namespace HCDE.Playsim.Tests;

public class VmSoundEngineStartSoundTests
{
    [Fact]
    public void PlanStartSound_RejectsWhenSfxDisabled()
    {
        var limitRange = 0f;
        var defPitch = 0f;
        var defPitchMax = 0f;
        var rolloffMin = 0f;
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: true, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: 0, limitReached: false, sourceIsListener: false,
            channel: 0, incomingFlags: 0, soundPaused: false, attenuation: 1,
            sfxLinkFrames: Array.Empty<VmSoundEngineStartSound.SfxLinkResolveFrame>(),
            sfxAttenuation: 1f,
            ref limitRange, ref defPitch, ref defPitchMax, ref rolloffMin,
            forcedRolloffMin: 0, forcedRolloffPresent: false, globalRolloffMin: 256f);
        Assert.True(plan.Reject);
    }

    [Fact]
    public void PlanStartSound_MarksLimitEvict()
    {
        var limitRange = 0f;
        var defPitch = 0f;
        var defPitchMax = 0f;
        var rolloffMin = 0f;
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: false, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: 2, limitReached: true, sourceIsListener: false,
            channel: 0, incomingFlags: VmSoundEngineStartSound.ChanfLoop, soundPaused: false, attenuation: 1,
            sfxLinkFrames: Array.Empty<VmSoundEngineStartSound.SfxLinkResolveFrame>(),
            sfxAttenuation: 1f,
            ref limitRange, ref defPitch, ref defPitchMax, ref rolloffMin,
            forcedRolloffMin: 0, forcedRolloffPresent: false, globalRolloffMin: 256f);
        Assert.False(plan.Reject);
        Assert.True((plan.ChanFlags & VmSoundEngineStartSound.ChanfEvicted) != 0);
    }

    [Fact]
    public void PlanStartSound_InheritsNearLimitFromLinkedSfx()
    {
        var limitRange = 0f;
        var defPitch = 0f;
        var defPitchMax = 0f;
        var rolloffMin = 0f;
        var frames = new[]
        {
            new VmSoundEngineStartSound.SfxLinkResolveFrame(
                ValidSoundId: true,
                SfxChanged: true,
                NearLimit: 3,
                LimitRange: 512f,
                DefPitch: 0.9f,
                DefPitchMax: 1.1f,
                RolloffMinDistance: 128f),
        };
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: false, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: -1, limitReached: false, sourceIsListener: false,
            channel: 0, incomingFlags: 0, soundPaused: false, attenuation: 2f,
            sfxLinkFrames: frames, sfxAttenuation: 0.5f,
            ref limitRange, ref defPitch, ref defPitchMax, ref rolloffMin,
            forcedRolloffMin: 0, forcedRolloffPresent: false, globalRolloffMin: 256f);
        Assert.False(plan.Reject);
        Assert.Equal(3, plan.NearLimit);
        Assert.Equal(512f, limitRange);
        Assert.Equal(128f, rolloffMin);
        Assert.Equal(0.9f, defPitch);
    }

    [Fact]
    public void PlanStartSound_RejectsStalledLink()
    {
        var limitRange = 0f;
        var defPitch = 0f;
        var defPitchMax = 0f;
        var rolloffMin = 0f;
        var frames = new[]
        {
            new VmSoundEngineStartSound.SfxLinkResolveFrame(
                ValidSoundId: true, SfxChanged: false, 0, 0, 0, 0, 0),
        };
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: false, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: 0, limitReached: false, sourceIsListener: false,
            channel: 0, incomingFlags: 0, soundPaused: false, attenuation: 1,
            sfxLinkFrames: frames, sfxAttenuation: 1f,
            ref limitRange, ref defPitch, ref defPitchMax, ref rolloffMin,
            forcedRolloffMin: 0, forcedRolloffPresent: false, globalRolloffMin: 256f);
        Assert.True(plan.Reject);
    }

    [Fact]
    public void PickRolloffMinDistance_PrefersForced()
    {
        Assert.Equal(128f, VmSoundEngineStartSound.PickRolloffMinDistance(true, 128f, 64f, 256f));
        Assert.Equal(256f, VmSoundEngineStartSound.PickRolloffMinDistance(false, 0f, 0f, 256f));
    }
}
