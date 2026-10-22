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
    public void CheckMove_AllowsWhenNoclip()
    {
        var tm = new VmPMapCheckMoveFakePush.CheckPositionTm { FloorZ = 0, CeilingZ = 200 };
        var actor = new VmPMapCheckMoveFakePush.CheckMoveActor { Height = 56, Z = 0, Flags = VmPMapCheckMoveFakePush.MfNoclip };
        var s = new VmPMapCheckMoveFakePush.CheckMoveState
        {
            Actor = actor,
            Tm = tm,
            Flags = 0,
            CheckPosition = () => true,
        };
        Assert.True(VmPMapCheckMoveFakePush.CheckMove(s));
    }

    [Fact]
    public void FakeZMovement_FloatsTowardTarget()
    {
        var mo = new VmPMapCheckMoveFakePush.CheckMoveActor
        {
            Z = 10,
            VelZ = 0,
            FloorZ = 0,
            CeilingZ = 128,
            Height = 40,
            Flags = VmPMapCheckMoveFakePush.MfFloat,
            FloatSpeed = 2,
            PosX = 0,
            PosY = 0,
            Target = new VmPMapCheckMoveFakePush.FloatTargetSample { X = 10, Y = 0, CenterZ = 20 },
        };
        VmPMapCheckMoveFakePush.FakeZMovement(mo);
        Assert.True(mo.Z > 10);
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

    [Fact]
    public void CheckForPushSpecial_ActivatesImpact()
    {
        var hit = false;
        var ctx = new VmPMapCheckMoveFakePush.PushLineContext
        {
            Special = 1,
            Side = 1,
            ThingFlags2 = VmPMapCheckMoveFakePush.Mf2Impact,
            ActivateImpact = _ => hit = true,
        };
        Assert.True(VmPMapCheckMoveFakePush.CheckForPushSpecial(ctx));
        Assert.True(hit);
    }
}
