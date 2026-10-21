namespace HCDE.Playsim.Tests;

public class VmSoundEngineStartSoundTests
{
    [Fact]
    public void PlanStartSound_RejectsWhenSfxDisabled()
    {
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: true, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: 0, limitReached: false, sourceIsListener: false,
            channel: 0, incomingFlags: 0, channelUsed: false, soundPaused: false, attenuation: 1);
        Assert.True(plan.Reject);
    }

    [Fact]
    public void PlanStartSound_MarksLimitEvict()
    {
        var plan = VmSoundEngineStartSound.PlanStartSound(
            validSound: true, volume: 1, nosfx: false, soundEnabled: true, blockNewSounds: false,
            sourceType: 1, hasPoint: true, sfxVolume: 1, sfxSingular: false, requestSingular: false,
            singularAlreadyPlaying: false, nearLimit: 2, limitReached: true, sourceIsListener: false,
            channel: 0, incomingFlags: VmSoundEngineStartSound.ChanfLoop, channelUsed: false,
            soundPaused: false, attenuation: 1);
        Assert.False(plan.Reject);
        Assert.True((plan.ChanFlags & VmSoundEngineStartSound.ChanfEvicted) != 0);
    }
}
