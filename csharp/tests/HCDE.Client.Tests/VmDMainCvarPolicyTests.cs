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
}
