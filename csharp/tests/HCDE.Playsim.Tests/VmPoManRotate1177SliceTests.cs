namespace HCDE.Playsim.Tests;

public class VmPoManRotate1177SliceTests
{
    [Fact]
    public void Lifecycle_RegistersClassesAndClearsThePoly()
    {
        Assert.Equal(
            new[]
            {
                "DPolyAction",
                "DRotatePoly",
                "DMovePoly",
                "DMovePolyTo",
                "DPolyDoor",
            },
            VmPoManRotate1177Slice.Registered.Select(row => row.Name).ToArray());
        Assert.False(VmPoManRotate1177Slice.Registered[0].IsAbstract);
        Assert.True(VmPoManRotate1177Slice.Registered[0].Pointers);
        Assert.All(VmPoManRotate1177Slice.Registered.Skip(1), row => Assert.False(row.Pointers));
        Assert.Equal(new[] { "m_Interpolation" }, VmPoManRotate1177Slice.PointerFields);
        Assert.Contains("DoMovePolyobj", VmPoManRotate1177Slice.PrivatePrototypes);
        Assert.Null(VmPoManRotate1177Slice.FreePolyNodes);

        var poly = new VmPoManRotate1177Slice.PolyObj { Id = 4 };
        var rotate = new VmPoManRotate1177Slice.RotatePoly();
        rotate.Construct(poly);
        Assert.Same(poly, rotate.Poly);
        Assert.Equal(0d, rotate.Speed);
        Assert.Equal(0d, rotate.Dist);
        Assert.NotNull(rotate.Interpolation);
        Assert.Same(rotate.Interpolation, poly.LiveInterpolation);
        Assert.Equal(1, rotate.Interpolation!.Refs);

        var arc = new VmPoManRotate1177Slice.Archive();
        rotate.Serialize(arc);
        Assert.Equal(new[] { "polyobj", "speed", "dist", "interpolation" }, arc.Entries.Select(entry => entry.Key).ToArray());
        Assert.Equal("4", arc.Entries[0].Value);
        Assert.Equal("0", arc.Entries[1].Value);
        Assert.Equal("set", arc.Entries[3].Value);

        var firstToken = rotate.Interpolation;
        var stranger = new VmPoManRotate1177Slice.RotatePoly();
        stranger.Construct(poly);
        poly.SpecialData = stranger;
        rotate.OnDestroy();
        Assert.Same(stranger, poly.SpecialData);
        Assert.Null(rotate.Interpolation);
        Assert.Equal(0, firstToken!.Refs);
        Assert.True(rotate.SuperDestroyed);
        Assert.Equal(1, stranger.Interpolation!.Refs);

        var kept = stranger.Interpolation;
        stranger.StopInterpolation();
        Assert.Null(stranger.Interpolation);
        Assert.Equal(0, kept!.Refs);
        stranger.StopInterpolation();
        Assert.Equal(0, kept.Refs);

        var level = new VmPoManRotate1177Slice.Level();
        poly.SpecialData = stranger;
        stranger.Destroyed = false;
        stranger.Construct(poly);
        stranger.Stop(level);
        Assert.Equal(new[] { "stop:4" }, level.Log);
        Assert.Null(poly.SpecialData);
        Assert.True(stranger.Destroyed);
        Assert.True(stranger.SuperDestroyed);

        var move = new VmPoManRotate1177Slice.MovePoly();
        move.Construct(poly);
        Assert.Equal(0d, move.Angle);
        Assert.Equal(0d, move.VelX);
        Assert.Equal(0d, move.VelY);
        var moveArc = new VmPoManRotate1177Slice.Archive();
        move.Serialize(moveArc);
        Assert.Equal(new[] { "polyobj", "speed", "dist", "interpolation", "angle", "speedv" }, moveArc.Entries.Select(entry => entry.Key).ToArray());
        Assert.Equal("0,0", moveArc.Entries[5].Value);

        var moveTo = new VmPoManRotate1177Slice.MovePolyTo();
        moveTo.Construct(poly);
        Assert.Equal(0d, moveTo.TargetX);
        Assert.Equal(0d, moveTo.TargetY);
        var toArc = new VmPoManRotate1177Slice.Archive();
        moveTo.Serialize(toArc);
        Assert.Equal(new[] { "polyobj", "speed", "dist", "interpolation", "speedv", "target" }, toArc.Entries.Select(entry => entry.Key).ToArray());
        Assert.Equal("0,0", toArc.Entries[5].Value);

        var door = new VmPoManRotate1177Slice.PolyDoor();
        door.Construct(poly, 1);
        Assert.Equal(1, door.Type);
        Assert.Equal(0d, door.Direction);
        Assert.Equal(0d, door.TotalDist);
        Assert.Equal(0, door.Tics);
        Assert.Equal(0, door.WaitTics);
        Assert.False(door.Close);
        Assert.Equal(0d, door.Angle);
        var doorArc = new VmPoManRotate1177Slice.Archive();
        door.Serialize(doorArc);
        Assert.Equal(
            new[] { "polyobj", "speed", "dist", "interpolation", "angle", "speedv", "type", "direction", "totaldist", "tics", "waittics", "close" },
            doorArc.Entries.Select(entry => entry.Key).ToArray());
        Assert.Equal("1", doorArc.Entries[6].Value);
        Assert.Equal("false", doorArc.Entries[11].Value);
    }

    [Fact]
    public void RotateTick_ClampsSpeedAndStopsAtZero()
    {
        var level = new VmPoManRotate1177Slice.Level();
        var missing = new VmPoManRotate1177Slice.RotatePoly { Dist = 10d, Speed = 3d };
        missing.Tick(level);
        Assert.Equal(10d, missing.Dist);
        Assert.Equal(3d, missing.Speed);
        Assert.Empty(level.Log);

        var steady = Poly(1);
        double seen = double.NaN;
        steady.Rotate = angle =>
        {
            seen = angle;
            return true;
        };
        var action = new VmPoManRotate1177Slice.RotatePoly();
        action.Construct(steady);
        steady.SpecialData = action;
        action.Dist = 10d;
        action.Speed = 3d;
        action.Tick(level);
        Assert.Equal(3d, seen);
        Assert.Equal(3d, action.Speed);
        Assert.Equal(7d, action.Dist);
        Assert.False(action.Destroyed);
        Assert.Empty(level.Log);
        Assert.Same(action, steady.SpecialData);

        var finish = Armed(2, out var finishPoly);
        finish.Dist = 2d;
        finish.Speed = 5d;
        finish.Tick(level);
        Assert.Equal(2d, finish.Speed);
        Assert.Equal(0d, finish.Dist);
        Assert.Equal(new[] { "stop:2" }, level.Log);
        Assert.True(finish.Destroyed);
        Assert.Null(finishPoly.SpecialData);
        Assert.Null(finish.Interpolation);
        Assert.Equal(0, finishPoly.LiveInterpolation!.Refs);

        level.Log.Clear();
        var negative = Armed(3, out var negativePoly);
        negative.Dist = 2d;
        negative.Speed = -5d;
        negative.Tick(level);
        Assert.Equal(-2d, negative.Speed);
        Assert.Equal(0d, negative.Dist);
        Assert.True(negative.Destroyed);
        Assert.Null(negativePoly.SpecialData);

        level.Log.Clear();
        var perpetual = Armed(4, out _);
        perpetual.Dist = -1d;
        perpetual.Speed = 4d;
        perpetual.Tick(level);
        Assert.Equal(4d, perpetual.Speed);
        Assert.Equal(-1d, perpetual.Dist);
        Assert.False(perpetual.Destroyed);
        Assert.Empty(level.Log);

        var failedSeen = 0d;
        var failedPoly = Poly(5);
        failedPoly.Rotate = angle =>
        {
            failedSeen = angle;
            return false;
        };
        var failed = new VmPoManRotate1177Slice.RotatePoly();
        failed.Construct(failedPoly);
        failedPoly.SpecialData = failed;
        failed.Dist = 2d;
        failed.Speed = 5d;
        failed.Tick(level);
        Assert.Equal(2d, failedSeen);
        Assert.Equal(2d, failed.Speed);
        Assert.Equal(2d, failed.Dist);
        Assert.False(failed.Destroyed);
        Assert.Same(failed, failedPoly.SpecialData);

        level.Log.Clear();
        var exact = Armed(6, out _);
        exact.Dist = 3d;
        exact.Speed = -3d;
        exact.Tick(level);
        Assert.Equal(-3d, exact.Speed);
        Assert.Equal(0d, exact.Dist);
        Assert.True(exact.Destroyed);
    }

    [Fact]
    public void RotatePoly_RejectsBusyMirrorsAndScalesTheAngle()
    {
        var level = new VmPoManRotate1177Slice.Level();
        Assert.False(VmPoManRotate1177Slice.RotatePolyEvent(level, 9, 1, 64, 1, false));
        Assert.Equal(new[] { "EV_RotatePoly: Invalid polyobj num: 9\n" }, level.Prints);
        Assert.Empty(level.Thinkers);

        var busy = Poly(1);
        busy.SpecialData = "already";
        level.Polys[1] = busy;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { busy, Poly(2) };
        Assert.False(VmPoManRotate1177Slice.RotatePolyEvent(level, 1, 2, 64, -1, false));
        Assert.Empty(level.Thinkers);
        Assert.Equal("already", busy.SpecialData);

        var blocked = Poly(3);
        blocked.Blocked = true;
        level.Polys[3] = blocked;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { blocked };
        Assert.False(VmPoManRotate1177Slice.RotatePolyEvent(level, 3, 2, 64, 1, false));
        Assert.True(blocked.Blocked);

        var portal = Poly(4);
        portal.HasPortals = 2;
        level.Polys[4] = portal;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { portal };
        Assert.False(VmPoManRotate1177Slice.RotatePolyEvent(level, 4, 2, 64, 1, false));
        Assert.Empty(level.Thinkers);

        var first = Poly(5);
        first.SeqType = 7;
        first.HasPortals = 1;
        var second = Poly(6);
        second.HasPortals = 2;
        second.SeqType = 8;
        level.Polys[5] = first;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { first, second };
        Assert.True(VmPoManRotate1177Slice.RotatePolyEvent(level, 5, 2, 64, -1, false));
        var started = Assert.IsType<VmPoManRotate1177Slice.RotatePoly>(first.SpecialData);
        Assert.Equal(90d, started.Dist);
        Assert.Equal(2 * -1 * VmPoManRotate1177Slice.SpeedScale, started.Speed);
        Assert.False(first.Blocked);
        Assert.Null(second.SpecialData);
        Assert.Single(level.Sequences);
        Assert.Equal(7, level.Sequences[0].SeqType);
        Assert.Equal(VmPoManRotate1177Slice.SeqDoor, level.Sequences[0].Kind);
        Assert.Equal(0, level.Sequences[0].Mode);
        Assert.Same(first, level.Sequences[0].Poly);

        var left = Poly(10);
        left.SeqType = 3;
        var right = Poly(11);
        right.SeqType = 4;
        right.Blocked = true;
        level.Polys[10] = left;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { left, right };
        level.Sequences.Clear();
        Assert.True(VmPoManRotate1177Slice.RotatePolyEvent(level, 10, 2, 255, -1, true));
        var leftAction = Assert.IsType<VmPoManRotate1177Slice.RotatePoly>(left.SpecialData);
        var rightAction = Assert.IsType<VmPoManRotate1177Slice.RotatePoly>(right.SpecialData);
        Assert.Equal(-1d, leftAction.Dist);
        Assert.Equal(-1d, rightAction.Dist);
        Assert.Equal(2 * -1 * VmPoManRotate1177Slice.SpeedScale, leftAction.Speed);
        Assert.Equal(2 * 1 * VmPoManRotate1177Slice.SpeedScale, rightAction.Speed);
        Assert.False(right.Blocked);
        Assert.Equal(2, level.Sequences.Count);
        Assert.Equal(4, level.Sequences[1].SeqType);

        var full = Poly(12);
        level.Polys[12] = full;
        level.MirrorOrder = null;
        Assert.True(VmPoManRotate1177Slice.RotatePolyEvent(level, 12, 1, 0, 1, false));
        var fullTurn = Assert.IsType<VmPoManRotate1177Slice.RotatePoly>(full.SpecialData);
        Assert.Equal(360d, fullTurn.Dist);
        Assert.Equal(1 * 1 * VmPoManRotate1177Slice.SpeedScale, fullTurn.Speed);

        var replaced = Poly(13);
        var previous = new VmPoManRotate1177Slice.RotatePoly();
        previous.Construct(replaced);
        replaced.SpecialData = previous;
        var later = Poly(14);
        later.HasPortals = 2;
        later.SpecialData = "busy";
        level.Polys[13] = replaced;
        level.MirrorOrder = new List<VmPoManRotate1177Slice.PolyObj> { replaced, later };
        Assert.True(VmPoManRotate1177Slice.RotatePolyEvent(level, 13, 4, 32, 1, true));
        var replacement = Assert.IsType<VmPoManRotate1177Slice.RotatePoly>(replaced.SpecialData);
        Assert.NotSame(previous, replacement);
        Assert.False(previous.Destroyed);
        Assert.Equal(45d, replacement.Dist);
        Assert.Equal(4 * VmPoManRotate1177Slice.SpeedScale, replacement.Speed);
        Assert.Equal("busy", later.SpecialData);
    }

    static VmPoManRotate1177Slice.PolyObj Poly(int id)
    {
        return new VmPoManRotate1177Slice.PolyObj
        {
            Id = id,
            Rotate = _ => true,
        };
    }

    static VmPoManRotate1177Slice.RotatePoly Armed(int id, out VmPoManRotate1177Slice.PolyObj poly)
    {
        poly = Poly(id);
        var action = new VmPoManRotate1177Slice.RotatePoly();
        action.Construct(poly);
        poly.SpecialData = action;
        return action;
    }
}
