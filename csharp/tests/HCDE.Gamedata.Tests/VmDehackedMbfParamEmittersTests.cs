namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfParamEmittersTests
{
    [Fact]
    public void ScratchSoundIndex_OffsetsByOne()
    {
        Assert.Equal(4, VmDehackedMbfParamEmitters.ScratchSoundIndex(5));
    }
}
