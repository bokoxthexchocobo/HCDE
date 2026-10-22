namespace HCDE.Playsim.Tests;

public class VmPMapCheckMoveFakePushTests
{
    [Fact]
    public void CheckMove_BlocksLowCeiling()
    {
        var tm = new VmPMapCheckMoveFakePush.CheckPositionTm { FloorZ = 0, CeilingZ = 40 };
        var actor = new VmPMapCheckMoveFakePush.CheckMoveActor { Height = 56, Z = 0 };
        var s = new VmPMapCheckMoveFakePush.CheckMoveState
        {
            Actor = actor,
            Tm = tm,
            Flags = 0,
            CheckPosition = () => true,
        };
        Assert.False(VmPMapCheckMoveFakePush.CheckMove(s));
    }

    [Fact]
    public void FakeZMovement_ClipsToFloor()
    {
        var mo = new VmPMapCheckMoveFakePush.CheckMoveActor
        {
            Z = -5,
            VelZ = 0,
            FloorZ = 0,
            CeilingZ = 128,
            Height = 40,
        };
        VmPMapCheckMoveFakePush.FakeZMovement(mo);
        Assert.Equal(0, mo.Z);
    }

    [Fact]
    public void CheckForPushSpecial_ActivatesPushWall()
    {
        var side = -1;
        var ctx = new VmPMapCheckMoveFakePush.PushLineContext
        {
            Special = 1,
            Side = 0,
            ThingFlags2 = VmPMapCheckMoveFakePush.Mf2Pushwall,
            ActivatePush = s => side = s,
        };
        Assert.True(VmPMapCheckMoveFakePush.CheckForPushSpecial(ctx));
        Assert.Equal(0, side);
    }
}
