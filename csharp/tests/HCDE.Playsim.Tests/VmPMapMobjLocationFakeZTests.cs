namespace HCDE.Playsim.Tests;

public class VmPMapMobjLocationFakeZTests
{
    [Fact]
    public void TestMobjLocation_AcceptsWhenZInBounds()
    {
        var m = new VmPMapMobjLocationFakeZ.MobjProbe
        {
            Z = 16,
            Top = 32,
            FloorZ = 0,
            CeilingZ = 64,
        };
        Assert.True(VmPMapMobjLocationFakeZ.TestMobjLocation(m, _ => true));
    }

    [Fact]
    public void FakeZMovement_ClipsToFloor()
    {
        var m = new VmPMapMobjLocationFakeZ.MobjProbe { Z = -8, Top = 8, FloorZ = 0, CeilingZ = 64, Height = 16, VelZ = -16 };
        VmPMapMobjLocationFakeZ.FakeZMovement(m);
        Assert.Equal(0, m.Z);
    }

    [Fact]
    public void CheckForPushSpecial_ActivatesPushWall()
    {
        var side = -1;
        var ctx = new VmPMapMobjLocationFakeZ.PushLineContext
        {
            HasSpecial = true,
            PushWall = true,
            ActivatePush = s => side = s,
        };
        VmPMapMobjLocationFakeZ.CheckForPushSpecial(ctx, 1, 32, 0);
        Assert.Equal(1, side);
    }

    [Fact]
    public void PortalCrossSide_DetectsForwardCross() =>
        Assert.True(VmPMapMobjLocationFakeZ.PortalCrossSide(0, 1));

    [Fact]
    public void CheckForPushSpecial_SkipsWhenWindowNotBlocking()
    {
        var activated = false;
        var ctx = new VmPMapMobjLocationFakeZ.PushLineContext
        {
            HasSpecial = true,
            PushWindowCompat = true,
            HasBackSector = true,
            FrontCeil = 64,
            FrontFloor = 0,
            BackCeil = 64,
            BackFloor = 0,
            PushWall = true,
            ActivatePush = _ => activated = true,
        };
        VmPMapMobjLocationFakeZ.CheckForPushSpecial(ctx, 1, 32, 16);
        Assert.False(activated);
    }

    [Fact]
    public void ShouldSkipPushWhenWindowOpen_WithoutMidtexBlock() =>
        Assert.True(VmPMapMobjLocationFakeZ.ShouldSkipPushWhenWindowOpen(
            new VmPMapMobjLocationFakeZ.PushLineContext
            {
                PushWindowCompat = true,
                HasBackSector = true,
                FrontCeil = 64,
                FrontFloor = 0,
                BackCeil = 64,
                BackFloor = 0,
            },
            32,
            16));
}
