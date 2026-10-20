namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfScratchSoundTests
{
    [Fact]
    public void ScratchMeleeArgs_ResolvesHitSound()
    {
        var args = VmDehackedMbfScratchSound.ScratchMeleeArgs(5, 3, (_, _) => 9);
        Assert.Equal(5, args.damage);
        Assert.Equal(9, args.hitSoundIndex);
    }

    [Fact]
    public void PlaySoundArgs_UsesAttenuationFlag()
    {
        var args = VmDehackedMbfScratchSound.PlaySoundArgs(2, 1, (id, _) => id);
        Assert.Equal(1, args.soundIndex);
        Assert.Equal(0f, args.attenuation);
    }
}
