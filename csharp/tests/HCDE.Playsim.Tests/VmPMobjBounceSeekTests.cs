namespace HCDE.Playsim.Tests;

public class VmPMobjBounceSeekTests
{
    [Fact]
    public void SkyDestroy_WhenNotOnSkyFlag() =>
        Assert.True(VmPMobjBounceSeek.SkyDestroy(true, VmPMobjBounceSeek.BounceNotOnSky));

    [Fact]
    public void CanSeekTarget_BlocksCantseek() =>
        Assert.False(VmPMobjBounceSeek.CanSeekTarget(true, false, false, false, false, true));

    [Fact]
    public void ClampBounceVolume() =>
        Assert.Equal(1.0, VmPMobjBounceSeek.ClampBounceVolume(2.0));
}
