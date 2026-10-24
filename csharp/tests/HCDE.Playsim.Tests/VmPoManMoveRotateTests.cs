namespace HCDE.Playsim.Tests;

public class VmPoManMoveRotateTests
{
    [Fact]
    public void MovePolyobj_RollsBackWhenBlocked()
    {
        var ctx = new VmPoManMoveRotate.MovePolyobjContext
        {
            DeltaX = 4,
            DeltaY = 0,
            SideDefIndices = { 0 },
            Vertices = { (0, 0) },
            PrevPts = { (0, 0) },
            CheckMobjBlocking = _ => true,
        };
        Assert.False(VmPoManMoveRotate.MovePolyobj(ctx));
        Assert.Equal(0, ctx.Vertices[0].X);
    }

    [Fact]
    public void MovePolyobj_AppliesDeltaWhenClear()
    {
        var ctx = new VmPoManMoveRotate.MovePolyobjContext
        {
            DeltaX = 2,
            DeltaY = 3,
            Vertices = { (1, 1) },
            PrevPts = { (1, 1) },
            Force = true,
        };
        Assert.True(VmPoManMoveRotate.MovePolyobj(ctx));
        Assert.Equal((3, 4), ctx.Vertices[0]);
    }

    [Fact]
    public void RotatePolyobj_RestoresVerticesWhenBlocked()
    {
        var ctx = new VmPoManMoveRotate.RotatePolyobjContext
        {
            DeltaDegrees = 15,
            Vertices = { (10, 0) },
            PrevPts = { (10, 0) },
            OriginalPts = { (10, 0) },
            SideDefIndices = { 0 },
            CheckMobjBlocking = _ => true,
        };
        Assert.False(VmPoManMoveRotate.RotatePolyobj(ctx));
        Assert.Equal((10, 0), ctx.Vertices[0]);
    }

    [Fact]
    public void EvMovePoly_RejectsBusyPoly()
    {
        var ctx = new VmPoManMoveRotate.EvMovePolyContext { PolyBusy = _ => true };
        Assert.False(VmPoManMoveRotate.EvMovePoly(ctx, 1));
    }

    [Fact]
    public void EvMovePoly_SpawnsThinkerWhenFree()
    {
        var spawned = false;
        var ctx = new VmPoManMoveRotate.EvMovePolyContext
        {
            Speed = 2,
            AngleDegrees = 0,
            Distance = 5,
            PolyBusy = _ => false,
            SpawnMoveThinker = (_, _, _, _) => spawned = true,
        };
        Assert.True(VmPoManMoveRotate.EvMovePoly(ctx, 3));
        Assert.True(spawned);
    }

    [Fact]
    public void EvRotatePoly_SkipsLinkedPortalPoly()
    {
        var ctx = new VmPoManMoveRotate.EvRotatePolyContext
        {
            ResolveMirror = _ => new VmPoManMoveRotate.RotatePolyState { PolyId = 1, PortalLinkMode = 2 },
        };
        Assert.False(VmPoManMoveRotate.EvRotatePoly(ctx, 1));
    }

    [Fact]
    public void RotatePolyDistance_ByteAngleMapsToDegrees()
    {
        Assert.Equal(45 * (90.0 / 64), VmPoManMoveRotate.RotatePolyDistance(45), 3);
    }
}
