namespace HCDE.Playsim.Tests;

public class VmPMapUtlLineOpeningTests
{
    [Fact]
    public void InterceptVector_ParallelReturnsZero()
    {
        var v1 = new VmPMapUtlLineOpening.Divline(0, 0, 1, 1);
        var v2 = new VmPMapUtlLineOpening.Divline(0, 1, 2, 2);
        Assert.Equal(0, VmPMapUtlLineOpening.InterceptVector(v2, v1));
    }

    [Fact]
    public void LineOpening_OneSidedClearsRange()
    {
        var open = new VmPMapUtlLineOpening.LineOpeningState();
        VmPMapUtlLineOpening.LineOpening(
            open, hasActor: false, twoSided: false, lineFlags: 0,
            posX: 0, posY: 0, refX: null, refY: null, flags: 0,
            frontCeilZ: 64, backCeilZ: 64, frontFloorZ: 0, backFloorZ: 0,
            frontCeilPortal: false, backCeilPortal: false,
            frontFloorPortal: false, backFloorPortal: false,
            frontFloorSlope: false, backFloorSlope: false,
            nextLowestFloorAt: null, applyXFloors: null, apply3dMidtex: null);
        Assert.Equal(0, open.Range);
    }
}
