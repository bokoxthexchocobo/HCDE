namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchFrameTests
{
    [Fact]
    public void GetDefaultTics_UsesShotgunFlashDefaults()
    {
        Assert.Equal(5, VmDehackedPatchFrame.GetDefaultTics(47, 1));
        Assert.Equal(4, VmDehackedPatchFrame.GetDefaultTics(48, 1));
    }

    [Fact]
    public void TryApplyProperty_SetsDuration()
    {
        var frame = new VmDehackedFramePatchTarget();
        Assert.True(VmDehackedPatchFrame.TryApplyProperty("Duration", "10", frame));
        Assert.Equal(10, frame.Tics);
    }
}
