namespace HCDE.Playsim.Tests;

public class VmSoundEngineLoadSoundTests
{
    [Fact]
    public void IsVocHeader_DetectsCreativeVoice()
    {
        var data = "Creative Voice File\x0"u8.ToArray();
        Assert.True(VmSoundEngineLoadSound.IsVocHeader(data));
    }

    [Fact]
    public void CanShareLumpBuffer_MatchesLumpAndRate()
    {
        Assert.True(VmSoundEngineLoadSound.CanShareLumpBuffer(
            otherValid: true, otherLink: -1, otherLump: 42, lumpnum: 42, loadRaw: true, rawRate: 11025, otherRawRate: 11025));
        Assert.False(VmSoundEngineLoadSound.CanShareLumpBuffer(
            otherValid: true, otherLink: -1, otherLump: 42, lumpnum: 42, loadRaw: true, rawRate: 22050, otherRawRate: 11025));
    }

    [Fact]
    public void IsDmxHeader_ValidatesDmxPrefix()
    {
        var data = new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        Assert.True(VmSoundEngineLoadSound.IsDmxHeader(data, dmxLen: 2, size: data.Length));
    }
}
