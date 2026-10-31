namespace HCDE.Playsim.Tests;

public class VmPoManMove1178SliceTests
{
    [Fact]
    public void MoveTick_StopsAtTheEndAndRescalesTheLastStep()
    {
        var level = new VmPoManRotate1177Slice.Level();
        var missing = new VmPoManRotate1177Slice.MovePoly { Dist = 10d, Speed = 3d, VelX = 3d };
        VmPoManMove1178Slice.MoveTick(level, missing);
        Assert.Equal(10d, missing.Dist);
        Assert.Empty(level.Log);

        var blocked = ArmedMove(1, out var blockedPoly);
        blockedPoly.Move = (_, _) => false;
        blocked.Dist = 10d;
        blocked.Speed = 3d;
        blocked.VelX = 3d;
        VmPoManMove1178Slice.MoveTick(level, blocked);
        Assert.Equal(10d, blocked.Dist);
        Assert.Equal(3d, blocked.VelX);
        Assert.Equal(0, blockedPoly.LinkUpdates);
        Assert.False(blocked.Destroyed);

        var cruise = ArmedMove(2, out var cruisePoly);
        cruise.Dist = 10d;
        cruise.Speed = 3d;
        cruise.Angle = 0d;
        cruise.VelX = 3d;
        VmPoManMove1178Slice.MoveTick(level, cruise);
        Assert.Equal(7d, cruise.Dist);
        Assert.Equal(3d, cruise.Speed);
        Assert.Equal(3d, cruise.VelX);
        Assert.Equal(1, cruisePoly.LinkUpdates);
        Assert.False(cruise.Destroyed);
        Assert.Empty(level.Log);

        var done = ArmedMove(3, out var donePoly);
        done.Dist = 1d;
        done.Speed = 4d;
        done.VelX = 4d;
        done.VelY = 1d;
        VmPoManMove1178Slice.MoveTick(level, done);
        Assert.Equal(-3d, done.Dist);
        Assert.Equal(4d, done.Speed);
        Assert.Equal(4d, done.VelX);
        Assert.Equal(1d, done.VelY);
        Assert.Equal(new[] { "stop:3" }, level.Log);
        Assert.True(done.Destroyed);
        Assert.Null(donePoly.SpecialData);
        Assert.Equal(1, donePoly.LinkUpdates);

        level.Log.Clear();
        var shorten = ArmedMove(4, out var shortenPoly);
        shorten.Dist = 5d;
        shorten.Speed = 3d;
        shorten.Angle = 0d;
        shorten.VelX = 9d;
        shorten.VelY = 9d;
        VmPoManMove1178Slice.MoveTick(level, shorten);
        var forward = VmPoManMove1178Slice.ToVector(0d, 2d);
        Assert.Equal(2d, shorten.Dist);
        Assert.Equal(2d, shorten.Speed);
        Assert.Equal(forward.X, shorten.VelX);
        Assert.Equal(forward.Y, shorten.VelY);
        Assert.Equal(1, shortenPoly.LinkUpdates);
        Assert.False(shorten.Destroyed);

        var reverse = ArmedMove(5, out _);
        reverse.Dist = 5d;
        reverse.Speed = -3d;
        reverse.Angle = 90d;
        VmPoManMove1178Slice.MoveTick(level, reverse);
        var back = VmPoManMove1178Slice.ToVector(90d, -2d);
        Assert.Equal(2d, reverse.Dist);
        Assert.Equal(-2d, reverse.Speed);
        Assert.Equal(back.X, reverse.VelX);
        Assert.Equal(back.Y, reverse.VelY);
    }

    [Fact]
    public void MovePoly_FlipsAngleAndStopsFastInterpolation()
    {
        var level = new VmPoManRotate1177Slice.Level();
        Assert.False(VmPoManMove1178Slice.MovePolyEvent(level, 9, 2d, 0d, 4d, false));
        Assert.Equal(new[] { "EV_MovePoly: Invalid polyobj num: 9\n" }, level.Prints);

        var busy = new VmPoManRotate1177Slice.PolyObj { Id = 1, SpecialData = "moving" };
        level.Polys[1] = busy;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { busy };
        Assert.False(VmPoManMove1178Slice.MovePolyEvent(level, 1, 2d, 0d, 4d, false));
        Assert.Equal("moving", busy.SpecialData);

        var fast = new VmPoManRotate1177Slice.PolyObj { Id = 2, SeqType = 6 };
        level.Polys[2] = fast;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { fast };
        Assert.True(VmPoManMove1178Slice.MovePolyEvent(level, 2, 2d, 0d, 4d, false));
        var fastAction = Assert.IsType<VmPoManRotate1177Slice.MovePoly>(fast.SpecialData);
        Assert.Equal(4d, fastAction.Dist);
        Assert.Equal(2d, fastAction.Speed);
        Assert.Equal(0d, fastAction.Angle);
        Assert.Equal(2d, fastAction.VelX);
        Assert.Equal(0d, fastAction.VelY);
        Assert.Null(fastAction.Interpolation);
        Assert.Equal(0, fast.LiveInterpolation!.Refs);
        Assert.Equal(6, level.Sequences[0].SeqType);
        Assert.Equal(VmPoManRotate1177Slice.SeqDoor, level.Sequences[0].Kind);
        Assert.Equal(0, level.Sequences[0].Mode);

        var left = new VmPoManRotate1177Slice.PolyObj { Id = 3, SeqType = 1 };
        var right = new VmPoManRotate1177Slice.PolyObj { Id = 4, SeqType = 2, Blocked = true };
        level.Polys[3] = left;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { left, right };
        level.Sequences.Clear();
        Assert.True(VmPoManMove1178Slice.MovePolyEvent(level, 3, 4d, 30d, 10d, false));
        var leftAction = Assert.IsType<VmPoManRotate1177Slice.MovePoly>(left.SpecialData);
        Assert.Equal(30d, leftAction.Angle);
        var leftStep = VmPoManMove1178Slice.ToVector(30d, 4d);
        Assert.Equal(leftStep.X, leftAction.VelX);
        Assert.Equal(leftStep.Y, leftAction.VelY);
        Assert.NotNull(leftAction.Interpolation);
        Assert.Null(right.SpecialData);
        Assert.True(right.Blocked);
        Assert.Single(level.Sequences);

        var neg = new VmPoManRotate1177Slice.PolyObj { Id = 5 };
        level.Polys[5] = neg;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { neg };
        Assert.True(VmPoManMove1178Slice.MovePolyEvent(level, 5, -2d, 90d, 4d, false));
        var negAction = Assert.IsType<VmPoManRotate1177Slice.MovePoly>(neg.SpecialData);
        var negStep = VmPoManMove1178Slice.ToVector(90d, -2d);
        Assert.Equal(negStep.X, negAction.VelX);
        Assert.Equal(negStep.Y, negAction.VelY);
        Assert.Null(negAction.Interpolation);

        var east = new VmPoManRotate1177Slice.PolyObj { Id = 6 };
        var west = new VmPoManRotate1177Slice.PolyObj { Id = 7 };
        level.Polys[6] = east;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { east, west };
        Assert.True(VmPoManMove1178Slice.MovePolyEvent(level, 6, 4d, 0d, 10d, true));
        var eastAction = Assert.IsType<VmPoManRotate1177Slice.MovePoly>(east.SpecialData);
        var westAction = Assert.IsType<VmPoManRotate1177Slice.MovePoly>(west.SpecialData);
        Assert.Equal(0d, eastAction.Angle);
        Assert.Equal(4d, eastAction.VelX);
        Assert.Equal(0d, eastAction.VelY);
        Assert.Equal(180d, westAction.Angle);
        var westStep = VmPoManMove1178Slice.ToVector(180d, 4d);
        Assert.Equal(westStep.X, westAction.VelX);
        Assert.Equal(westStep.Y, westAction.VelY);
        Assert.NotNull(eastAction.Interpolation);
        Assert.NotNull(westAction.Interpolation);
    }

    [Fact]
    public void MoveTo_AimsAtTheStartSpotAndReversesMirrors()
    {
        var level = new VmPoManRotate1177Slice.Level();
        var blocked = ArmedMoveTo(1, out var blockedPoly);
        blockedPoly.Move = (_, _) => false;
        blocked.Dist = 8d;
        blocked.Speed = 3d;
        blocked.VelX = 3d;
        VmPoManMove1178Slice.MoveToTick(level, blocked);
        Assert.Equal(8d, blocked.Dist);
        Assert.Equal(0, blockedPoly.LinkUpdates);

        var cruise = ArmedMoveTo(2, out var cruisePoly);
        cruise.Dist = 10d;
        cruise.Speed = 3d;
        cruise.VelX = 3d;
        cruise.VelY = 1d;
        VmPoManMove1178Slice.MoveToTick(level, cruise);
        Assert.Equal(7d, cruise.Dist);
        Assert.Equal(3d, cruise.VelX);
        Assert.Equal(1d, cruise.VelY);
        Assert.Equal(1, cruisePoly.LinkUpdates);
        Assert.False(cruise.Destroyed);

        var done = ArmedMoveTo(3, out var donePoly);
        done.Dist = 1d;
        done.Speed = 4d;
        done.VelX = 4d;
        VmPoManMove1178Slice.MoveToTick(level, done);
        Assert.Equal(-3d, done.Dist);
        Assert.Equal(new[] { "stop:3" }, level.Log);
        Assert.True(done.Destroyed);
        Assert.Equal(1, donePoly.LinkUpdates);
        Assert.Null(donePoly.SpecialData);

        var shorten = ArmedMoveTo(4, out var shortenPoly);
        shortenPoly.StartX = 1d;
        shortenPoly.StartY = 2d;
        shorten.TargetX = 8d;
        shorten.TargetY = 6d;
        shorten.Dist = 5d;
        shorten.Speed = -3d;
        shorten.VelX = 0d;
        VmPoManMove1178Slice.MoveToTick(level, shorten);
        Assert.Equal(2d, shorten.Dist);
        Assert.Equal(-2d, shorten.Speed);
        Assert.Equal(7d, shorten.VelX);
        Assert.Equal(4d, shorten.VelY);
        Assert.Equal(1, shortenPoly.LinkUpdates);
        Assert.False(shorten.Destroyed);

        Assert.False(VmPoManMove1178Slice.MovePolyToEvent(level, 40, 4d, 10d, 0d, false));
        Assert.Equal(new[] { "EV_MovePolyTo: Invalid polyobj num: 40\n" }, level.Prints);

        var origin = new VmPoManRotate1177Slice.PolyObj { Id = 10, StartX = 0d, StartY = 0d, SeqType = 5 };
        var mirror = new VmPoManRotate1177Slice.PolyObj { Id = 11, StartX = 5d, StartY = 5d, SeqType = 6 };
        level.Polys[10] = origin;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { origin, mirror };
        level.Sequences.Clear();
        Assert.True(VmPoManMove1178Slice.MovePolyToEvent(level, 10, 4d, 10d, 0d, false));
        var originAction = Assert.IsType<VmPoManRotate1177Slice.MovePolyTo>(origin.SpecialData);
        var mirrorAction = Assert.IsType<VmPoManRotate1177Slice.MovePolyTo>(mirror.SpecialData);
        Assert.Equal(10d, originAction.Dist);
        Assert.Equal(4d, originAction.Speed);
        Assert.Equal(4d, originAction.VelX);
        Assert.Equal(0d, originAction.VelY);
        Assert.Equal(10d, originAction.TargetX);
        Assert.Equal(0d, originAction.TargetY);
        Assert.Equal(10d, mirrorAction.Dist);
        Assert.Equal(-4d, mirrorAction.VelX);
        Assert.Equal(0d, mirrorAction.VelY);
        Assert.Equal(-5d, mirrorAction.TargetX);
        Assert.Equal(5d, mirrorAction.TargetY);
        Assert.NotNull(originAction.Interpolation);
        Assert.Equal(2, level.Sequences.Count);
        Assert.Equal(5, level.Sequences[0].SeqType);
        Assert.Equal(VmPoManRotate1177Slice.SeqDoor, level.Sequences[0].Kind);

        var reverse = new VmPoManRotate1177Slice.PolyObj { Id = 12, StartX = 0d, StartY = 0d };
        level.Polys[12] = reverse;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { reverse };
        Assert.True(VmPoManMove1178Slice.MovePolyToEvent(level, 12, -4d, 10d, 0d, false));
        var reverseAction = Assert.IsType<VmPoManRotate1177Slice.MovePolyTo>(reverse.SpecialData);
        Assert.Equal(-4d, reverseAction.VelX);
        Assert.Null(reverseAction.Interpolation);

        var parked = new VmPoManRotate1177Slice.PolyObj { Id = 13, StartX = 2d, StartY = 3d, SpecialData = "busy" };
        var follower = new VmPoManRotate1177Slice.PolyObj { Id = 14, StartX = 0d, StartY = 0d };
        level.Polys[13] = parked;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { parked, follower };
        Assert.False(VmPoManMove1178Slice.MovePolyToEvent(level, 13, 1d, 2d, 3d, false));
        Assert.Equal("busy", parked.SpecialData);
        Assert.Null(follower.SpecialData);

        var still = new VmPoManRotate1177Slice.PolyObj { Id = 15, StartX = 2d, StartY = 3d };
        level.Polys[15] = still;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { still };
        Assert.True(VmPoManMove1178Slice.MovePolyToEvent(level, 15, 5d, 2d, 3d, false));
        var stillAction = Assert.IsType<VmPoManRotate1177Slice.MovePolyTo>(still.SpecialData);
        Assert.Equal(0d, stillAction.Dist);
        Assert.Equal(0d, stillAction.VelX);
        Assert.Equal(0d, stillAction.VelY);
        Assert.Equal(2d, stillAction.TargetX);
        Assert.Equal(3d, stillAction.TargetY);
        Assert.Null(stillAction.Interpolation);
    }

    static VmPoManRotate1177Slice.MovePoly ArmedMove(int id, out VmPoManRotate1177Slice.PolyObj poly)
    {
        poly = new VmPoManRotate1177Slice.PolyObj { Id = id, Move = (_, _) => true };
        var action = new VmPoManRotate1177Slice.MovePoly();
        action.Construct(poly);
        poly.SpecialData = action;
        return action;
    }

    static VmPoManRotate1177Slice.MovePolyTo ArmedMoveTo(int id, out VmPoManRotate1177Slice.PolyObj poly)
    {
        poly = new VmPoManRotate1177Slice.PolyObj { Id = id, Move = (_, _) => true };
        var action = new VmPoManRotate1177Slice.MovePolyTo();
        action.Construct(poly);
        poly.SpecialData = action;
        return action;
    }
}
