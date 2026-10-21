namespace HCDE.Net.Core.Tests;

public class VmDNetBlackboxTests
{
    [Fact]
    public void ClampSizeMbCvarNative_EnforcesBounds()
    {
        Assert.Equal(4, VmDNetBlackbox.ClampSizeMbCvarNative(1));
        Assert.Equal(256, VmDNetBlackbox.ClampSizeMbCvarNative(999));
    }

    [Fact]
    public void ShouldAutoSaveNative_RespectsCooldown()
    {
        Assert.False(VmDNetBlackbox.ShouldAutoSaveNative(1000, 0));
        Assert.True(VmDNetBlackbox.ShouldAutoSaveNative(6000, 0));
    }
}
