namespace HCDE.Playsim.Tests;

public class VmPoManPolyThinkerBandsTests
{
    [Fact]
    public void PolyActionSerialize_WritesCoreFields()
    {
        var fields = new List<string>();
        var ctx = new VmPoManPolyThinkerBands.PolyActionSerializeContext
        {
            WriteField = (k, _) => fields.Add(k),
        };
        ctx.State.Speed = 3;
        VmPoManPolyThinkerBands.PolyActionSerialize(ctx);
        Assert.Contains("polyobj", fields);
        Assert.Contains("speed", fields);
        Assert.Contains("dist", fields);
        Assert.Contains("interpolation", fields);
    }

    [Fact]
    public void PolyActionConstruct_ClearsSpeedAndDist()
    {
        var ctx = new VmPoManPolyThinkerBands.PolyActionConstructContext();
        VmPoManPolyThinkerBands.PolyActionConstruct(ctx, new object());
        Assert.Equal(0, ctx.State.Speed);
        Assert.Equal(0, ctx.State.Dist);
    }

    [Fact]
    public void PolyActionOnDestroy_ClearsSpecialData()
    {
        var state = new VmPoManPolyThinkerBands.PolyActionState();
        state.SpecialDataOnPoly = state;
        VmPoManPolyThinkerBands.PolyActionOnDestroy(state, null, null);
        Assert.Null(state.SpecialDataOnPoly);
    }

    [Fact]
    public void PolyActionStop_InvokesCallbacks()
    {
        var stopped = false;
        VmPoManPolyThinkerBands.PolyActionStop(() => stopped = true, null);
        Assert.True(stopped);
    }

    [Fact]
    public void PolyActionSetInterpolation_CreatesObject()
    {
        var state = new VmPoManPolyThinkerBands.PolyActionState();
        VmPoManPolyThinkerBands.PolyActionSetInterpolation(state, () => 42);
        Assert.Equal(42, state.Interpolation);
    }

    [Fact]
    public void PolyActionStopInterpolation_ReleasesHandle()
    {
        var state = new VmPoManPolyThinkerBands.PolyActionState { Interpolation = 1 };
        object? released = null;
        VmPoManPolyThinkerBands.PolyActionStopInterpolation(state, h => released = h);
        Assert.Equal(1, released);
        Assert.Null(state.Interpolation);
    }

    [Fact]
    public void MovePolySerialize_WritesAngleAndSpeed()
    {
        var keys = new List<string>();
        var move = new VmPoManPolyThinkerBands.MovePolyState { AngleDegrees = 90 };
        var ctx = new VmPoManPolyThinkerBands.PolyActionSerializeContext
        {
            WriteField = (k, _) => keys.Add(k),
        };
        VmPoManPolyThinkerBands.MovePolySerialize(ctx, move);
        Assert.Contains("angle", keys);
        Assert.Contains("speedv", keys);
    }

    [Fact]
    public void MovePolyConstruct_ZerosMotionFields()
    {
        var move = new VmPoManPolyThinkerBands.MovePolyState { AngleDegrees = 5, SpeedVx = 1 };
        var ctx = new VmPoManPolyThinkerBands.PolyActionConstructContext();
        VmPoManPolyThinkerBands.MovePolyConstruct(ctx, null, move);
        Assert.Equal(0, move.AngleDegrees);
        Assert.Equal(0, move.SpeedVx);
    }

    [Fact]
    public void MovePolyToSerialize_WritesTarget()
    {
        var keys = new List<string>();
        var move = new VmPoManPolyThinkerBands.MovePolyToState { TargetX = 10, TargetY = 20 };
        var ctx = new VmPoManPolyThinkerBands.PolyActionSerializeContext
        {
            WriteField = (k, _) => keys.Add(k),
        };
        VmPoManPolyThinkerBands.MovePolyToSerialize(ctx, move);
        Assert.Contains("target", keys);
    }

    [Fact]
    public void MovePolyToConstruct_ZerosVectors()
    {
        var move = new VmPoManPolyThinkerBands.MovePolyToState { TargetX = 1, SpeedVx = 2 };
        var ctx = new VmPoManPolyThinkerBands.PolyActionConstructContext();
        VmPoManPolyThinkerBands.MovePolyToConstruct(ctx, null, move);
        Assert.Equal(0, move.TargetX);
        Assert.Equal(0, move.SpeedVx);
    }

    [Fact]
    public void PolyDoorSerialize_WritesDoorFields()
    {
        var keys = new List<string>();
        var door = new VmPoManPolyThinkerBands.PolyDoorState { DoorType = 2 };
        var ctx = new VmPoManPolyThinkerBands.PolyActionSerializeContext
        {
            WriteField = (k, _) => keys.Add(k),
        };
        VmPoManPolyThinkerBands.PolyDoorSerialize(ctx, door);
        Assert.Contains("type", keys);
        Assert.Contains("close", keys);
    }

    [Fact]
    public void PolyDoorConstruct_SetsDefaults()
    {
        var door = new VmPoManPolyThinkerBands.PolyDoorState();
        var ctx = new VmPoManPolyThinkerBands.PolyActionConstructContext();
        VmPoManPolyThinkerBands.PolyDoorConstruct(ctx, null, door, 1);
        Assert.Equal(1, door.DoorType);
        Assert.False(door.Close);
    }

    [Fact]
    public void FPolyObjConstruct_ClearsDefaults()
    {
        var poly = new VmPoManPolyThinkerBands.FPolyObjState { Tag = 9, Blocked = true };
        VmPoManPolyThinkerBands.FPolyObjConstruct(poly);
        Assert.Equal(0, poly.Tag);
        Assert.False(poly.Blocked);
    }

    [Fact]
    public void FPolyObjGetMirror_ReturnsMirrorNum()
    {
        var poly = new VmPoManPolyThinkerBands.FPolyObjState { MirrorNum = 7 };
        Assert.Equal(7, VmPoManPolyThinkerBands.FPolyObjGetMirror(poly));
    }

    [Fact]
    public void RotatePt_RotatesAboutStart()
    {
        double x = 1, y = 0;
        VmPoManPolyThinkerBands.RotatePt(90, ref x, ref y, 10, 20);
        Assert.Equal(10, x, 3);
        Assert.Equal(21, y, 3);
    }
}
