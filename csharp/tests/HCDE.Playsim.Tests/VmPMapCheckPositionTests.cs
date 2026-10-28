namespace HCDE.Playsim.Tests;

public class VmPMapCheckPositionTests
{
    [Fact]
    public void CheckPosition_NoclipReturnsTrue()
    {
        var tm = new VmPMapCheckPosition.CheckPositionTm();
        Assert.True(VmPMapCheckPosition.CheckPosition(new VmPMapCheckPosition.CheckPositionState
        {
            Tm = tm,
            ThingFlags = VmPMapCheckPosition.MfNoclip,
            PointInSector = (_, _) => new VmPMapCheckPosition.SectorSample { FloorZ = 0, CeilingZ = 128 },
        }));
    }

    [Fact]
    public void CheckPosition_HeightFailWhenCrushed()
    {
        var tm = new VmPMapCheckPosition.CheckPositionTm();
        Assert.False(VmPMapCheckPosition.CheckPosition(new VmPMapCheckPosition.CheckPositionState
        {
            Tm = tm,
            ThingHeight = 56,
            PointInSector = (_, _) => new VmPMapCheckPosition.SectorSample
            {
                PortalBlocksCeiling = true,
                PortalBlocksFloor = true,
                FloorZ = 0,
                CeilingZ = 40,
            },
            IterateLines = () => Array.Empty<VmPMapCheckPosition.LineIteratorResult>(),
            PitCheckLine = (_, _) => true,
        }));
    }

    [Fact]
    public void ShouldDealDamage_WhenForcePain() =>
        Assert.True(VmPMapCheckPosition.ShouldDealDamage(0, 1, 0));
}
