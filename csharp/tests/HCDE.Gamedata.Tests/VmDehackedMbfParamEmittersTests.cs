namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfParamEmittersTests
{
    [Fact]
    public void ScratchSoundIndex_OffsetsByOne()
    {
        Assert.Equal(4, VmDehackedMbfParamEmitters.ScratchSoundIndex(5));
    }

    [Fact]
    public void TurnAngleDegrees_PassesThrough()
    {
        Assert.Equal(45f, VmDehackedMbfParamEmitters.TurnAngleDegrees(45));
    }
}
