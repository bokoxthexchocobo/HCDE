namespace HCDE.Client.Tests;

public class VmDMainCvarPolicyTests
{
    [Fact]
    public void ClampTimeScaleForNetGame_ForcesOne()
    {
        Assert.Equal(1.0f, VmDMainCvarPolicy.ClampTimeScaleForNetGame(true, 0.5f));
    }

    [Fact]
    public void ClampVidRenderMode_RemapsSoftpoly()
    {
        Assert.Equal(0, VmDMainCvarPolicy.ClampVidRenderMode(2));
    }

    [Fact]
    public void IsTimeScaleAllowed_RejectsTooSmall()
    {
        Assert.False(VmDMainCvarPolicy.IsTimeScaleAllowed(0.04f));
        Assert.True(VmDMainCvarPolicy.IsTimeScaleAllowed(0.05f));
    }

    [Fact]
    public void ClampWipeType_ClampsInvalid()
    {
        Assert.Equal(1, VmDMainCvarPolicy.ClampWipeType(99, 0, 5, 1));
    }
}
