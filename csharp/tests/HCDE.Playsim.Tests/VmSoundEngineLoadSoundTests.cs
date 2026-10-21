namespace HCDE.Playsim.Tests;

public class VmSoundEngineLoadSoundTests
{
    [Fact]
    public void IsVocHeader_DetectsCreativeVoice()
    {
        var data = "Creative Voice File\x0"u8.ToArray();
        Assert.True(VmSoundEngineLoadSound.IsVocHeader(data));
    }
}
