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

    [Fact]
    public void FloorBounceDestroyed_WhenSky()
    {
        var ctx = new VmPMobjBounceSeek.FloorBounceContext(0, VmPMobjBounceSeek.BounceNotOnSky, 0, 0, true, false, false, false, 0);
        Assert.True(VmPMobjBounceSeek.FloorBounceDestroyed(ctx));
    }

    [Fact]
    public void SeekerTracksTarget_WhenTracerSet()
    {
        var ctx = new VmPMobjBounceSeek.SeekerContext(false, true, 1, 1, 2, 1, 1, 0, false, 0, false);
        Assert.True(VmPMobjBounceSeek.SeekerTracksTarget(ctx));
    }

}
