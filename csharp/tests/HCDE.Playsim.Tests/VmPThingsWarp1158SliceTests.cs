namespace HCDE.Playsim.Tests;

public class VmPThingsWarp1158SliceTests
{
    [Fact]
    public void Proximity_SkipsTheReferenceAndStopsOnceTheCountIsPassed()
    {
        var none = VmPThingsWarp1158Slice.CheckProximity(
            false, true, 10, 0, 0, false, 7, 0, 16, 1, 2, 3, 4, 5, 6, Array.Empty<VmPThingsWarp1158Slice.Neighbor>());
        Assert.Equal(0, none.Value);
        Assert.Equal(1, none.SelfTarget);

        var closed = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 0, 0, 0, false, 7, 0, 16, 1, 2, 3, 4, 5, 6, Array.Empty<VmPThingsWarp1158Slice.Neighbor>());
        Assert.Equal(0, closed.Value);

        var empty = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 0, 0, false, 7, 0, 16, 1, 2, 3, 4, 5, 6, Array.Empty<VmPThingsWarp1158Slice.Neighbor>());
        Assert.Equal(1, empty.Value);
        Assert.Equal(0, empty.Seen);

        var neighbors = new[]
        {
            Near(7, exact: true, distance: 1),
            Near(8, exact: true, distance: 1, unmorphed: true),
            Near(9, exact: false, kind: true, distance: 1),
            Near(10, exact: true, distance: 1, map: false),
            Near(11, exact: true, distance: 10),
            Near(12, exact: true, distance: 1, z: 100),
            Near(13, exact: true, distance: 1, killed: true),
            Near(14, exact: true, distance: 4),
            Near(15, exact: true, distance: 3),
            Near(16, exact: true, distance: 2),
        };
        var stopped = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, 0, false, 7, 0, 16, 1, 2, 3, 4, 5, 6, neighbors);
        Assert.Equal(1, stopped.Value);
        Assert.Equal(2, stopped.Seen);
        Assert.Equal(-1, stopped.ChosenId);
        Assert.False(stopped.Wrote);
        Assert.Equal(1, stopped.SelfTarget);

        var ancestor = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.Ancestor, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(9, exact: false, kind: true, distance: 1) });
        Assert.Equal(1, ancestor.Value);

        var sight = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.CheckSight, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(20, exact: true, distance: 1, sight: false), Near(21, exact: true, distance: 1) });
        Assert.Equal(1, sight.Seen);
        Assert.Equal(1, sight.Value);

        var noZ = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.NoZ, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(12, exact: true, distance: 1, z: 100) });
        Assert.Equal(1, noZ.Value);

        var dead = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.DeadOnly | VmPThingsWarp1158Slice.SetTarget, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(31, exact: true, distance: 1, killed: true), Near(30, exact: true, distance: 1) });
        Assert.Equal(1, dead.Seen);
        Assert.Equal(31, dead.SelfTarget);

        var countedDead = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.CountDead, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(31, exact: true, distance: 1, killed: true) });
        Assert.Equal(1, countedDead.Value);
    }

    [Fact]
    public void Proximity_KeepsSearchingForClosestAndWritesThePointer()
    {
        var closest = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 0, VmPThingsWarp1158Slice.Closest | VmPThingsWarp1158Slice.SetTarget, false,
            7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(40, exact: true, distance: 5), Near(41, exact: true, distance: 4), Near(42, exact: true, distance: 1) });
        Assert.Equal(1, closest.Value);
        Assert.Equal(3, closest.Seen);
        Assert.Equal(42, closest.ChosenId);
        Assert.Equal(42, closest.SelfTarget);
        Assert.Equal(4, closest.RefTarget);
        Assert.True(closest.Wrote);

        var both = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 20, 2, VmPThingsWarp1158Slice.Closest | VmPThingsWarp1158Slice.Farthest | VmPThingsWarp1158Slice.SetMaster,
            false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(50, exact: true, distance: 1), Near(51, exact: true, distance: 5) });
        Assert.Equal(1, both.Value);
        Assert.Equal(51, both.ChosenId);
        Assert.Equal(51, both.SelfMaster);
        Assert.Equal(1, both.SelfTarget);

        var under = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 3, VmPThingsWarp1158Slice.LessOrEqual, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(60, exact: true, distance: 1) });
        Assert.Equal(1, under.Value);
        var exactUnder = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 3, VmPThingsWarp1158Slice.LessOrEqual | VmPThingsWarp1158Slice.Exact, false,
            7, 0, 16, 1, 2, 3, 4, 5, 6, new[] { Near(60, exact: true, distance: 1) });
        Assert.Equal(0, exactUnder.Value);
        var bareUnder = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 3, 0, false, 7, 0, 16, 1, 2, 3, 4, 5, 6, new[] { Near(60, exact: true, distance: 1) });
        Assert.Equal(0, bareUnder.Value);

        var over = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 0, VmPThingsWarp1158Slice.LessOrEqual, false, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(70, exact: true, distance: 1), Near(71, exact: true, distance: 1) });
        Assert.Equal(0, over.Value);
        Assert.Equal(1, over.Seen);

        var counting = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 0, VmPThingsWarp1158Slice.SetTarget, true, 7, 0, 16, 1, 2, 3, 4, 5, 6,
            new[] { Near(80, exact: true, distance: 1), Near(81, exact: true, distance: 2), Near(82, exact: true, distance: 3) });
        Assert.Equal(3, counting.Value);
        Assert.Equal(80, counting.SelfTarget);
        Assert.Equal(3, counting.Seen);

        var onPtr = VmPThingsWarp1158Slice.CheckProximity(
            true, true, 10, 1, VmPThingsWarp1158Slice.SetTracer | VmPThingsWarp1158Slice.SetOnPtr, false,
            7, 0, 16, 1, 2, 3, 4, 5, 6, new[] { Near(90, exact: true, distance: 1) });
        Assert.Equal(1, onPtr.Value);
        Assert.Equal(90, onPtr.RefTracer);
        Assert.Equal(3, onPtr.SelfTracer);
    }

    [Fact]
    public void Warp_RotatesLeftAndRestoresAFailedOrTestOnlyMove()
    {
        var caller = Body(1, 1, 2, 3, yaw: 10, pitch: 4, velX: 9, velY: 8, velZ: 7, prevX: 1, prevY: 1, prevZ: 1);
        var reference = Body(2, 10, 20, 30, yaw: 0, pitch: 1, velX: 4, velY: 5, velZ: 6, height: 8, radius: 2);

        var missing = VmPThingsWarp1158Slice.Warp(
            true, false, caller, reference, 3, 4, 1, 0, 0, 0, 0, 0, true, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, false, 0, 0);
        Assert.False(missing.Warped);
        Assert.Equal(1, missing.Caller.X);

        var moved = Warp(caller, reference, 3, 4, 1, 0, 0);
        Assert.True(moved.Warped);
        Assert.True(moved.TriedMove);
        Assert.Equal(13, moved.Caller.X);
        Assert.Equal(16, moved.Caller.Y);
        Assert.Equal(31, moved.Caller.Z);
        Assert.Equal(0, moved.Caller.Yaw);
        Assert.Equal(4, moved.Caller.Pitch);
        Assert.Equal(9, moved.Caller.VelX);
        Assert.Equal(13, moved.Caller.PrevX);
        Assert.Equal(0, moved.Caller.PrevYaw);
        Assert.Equal(9, moved.Caller.PortalGroup);
        Assert.Equal(2, moved.Reference.Id);
        Assert.Equal(1, moved.Caller.Id);

        var blocked = Warp(caller, reference, 3, 4, 1, 0, 0, fits: false);
        Assert.False(blocked.Warped);
        Assert.False(blocked.TriedMove);
        Assert.Equal(1, blocked.Caller.X);
        Assert.Equal(2, blocked.Caller.Y);
        Assert.Equal(3, blocked.Caller.Z);
        Assert.Equal(10, blocked.Caller.Yaw);
        Assert.Equal(2, blocked.Caller.PortalGroup);

        var testOnly = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.TestOnly);
        Assert.True(testOnly.Warped);
        Assert.False(testOnly.TriedMove);
        Assert.Equal(1, testOnly.Caller.X);
        Assert.Equal(10, testOnly.Caller.Yaw);
        Assert.Equal(1, testOnly.Caller.PrevX);

        var absoluteOffset = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.AbsoluteOffset);
        Assert.Equal(13, absoluteOffset.Caller.X);
        Assert.Equal(24, absoluteOffset.Caller.Y);

        var absolute = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.AbsolutePosition, radiusOffset: 1);
        Assert.Equal(5, absolute.Caller.X);
        Assert.Equal(4, absolute.Caller.Y);
        Assert.Equal(1, absolute.Caller.Z);

        var shifted = Warp(caller, reference, 3, 4, 1, 0, 0, portalX: 100, portalY: 200);
        Assert.Equal(113, shifted.Caller.X);
        Assert.Equal(216, shifted.Caller.Y);

        var callerAngle = Warp(caller, reference, 3, 4, 1, 20, VmPThingsWarp1158Slice.UseCallerAngle | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(30, callerAngle.Caller.Yaw);
        Assert.Equal(3, callerAngle.Caller.X);

        var refAngle = Warp(caller, reference with { Yaw = 15 }, 3, 4, 1, 20, VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(35, refAngle.Caller.Yaw);

        var frozen = Warp(caller, reference with { Yaw = 15 }, 3, 4, 1, 20, VmPThingsWarp1158Slice.AbsoluteAngle | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(20, frozen.Caller.Yaw);

        var raised = Warp(caller, reference, 0, 0, 1, 0, 0, heightOffset: 0.5);
        Assert.Equal(10, raised.Caller.X);
        Assert.Equal(20, raised.Caller.Y);
        Assert.Equal(35, raised.Caller.Z);

        var floored = Warp(caller, reference, 0, 0, 1, 0, VmPThingsWarp1158Slice.ToFloor, heightOffset: 0.5, floorZ: 100);
        Assert.Equal(105, floored.Caller.Z);

        var forced = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.NoCheckPosition, fits: false);
        Assert.True(forced.Warped);
        Assert.Equal(13, forced.Caller.X);

        var swapped = Warp(caller, reference, 5, 0, 0, 0, VmPThingsWarp1158Slice.MovePtr | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(2, swapped.Caller.Id);
        Assert.Equal(5, swapped.Caller.X);
        Assert.Equal(0, swapped.Caller.Y);
        Assert.Equal(0, swapped.Caller.Z);
        Assert.Equal(1, swapped.Reference.Id);
        Assert.Equal(1, swapped.Reference.X);
    }

    [Fact]
    public void Warp_CopiesPitchThenClampsThePlayerAndBobs()
    {
        var caller = Body(1, 1, 2, 3, yaw: 10, pitch: 4, velX: 9, velY: 8, velZ: 7, prevX: 1, prevY: 1, prevZ: 1);
        var reference = Body(2, 10, 20, 30, yaw: 0, pitch: 40, velX: 4, velY: 5, velZ: 6, height: 8, radius: 2, flags2: VmPThingsWarp1158Slice.FloatBob, bobPhase: 16, bobFactor: 1, bobStrength: 1);

        var copied = Warp(caller, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.CopyVelocity | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(4, copied.Caller.VelX);
        Assert.Equal(6, copied.Caller.VelZ);

        var stopped = Warp(caller, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.CopyVelocity | VmPThingsWarp1158Slice.Stop | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(0, stopped.Caller.VelX);
        Assert.Equal(0, stopped.Caller.VelZ);

        var pitched = Warp(caller, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.CopyPitch | VmPThingsWarp1158Slice.AbsolutePosition, pitch: 5);
        Assert.Equal(45, pitched.Caller.Pitch);

        var player = caller with { Player = true, MinPitch = -10, MaxPitch = 10, Pitch = 0 };
        var clamped = Warp(player, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.CopyPitch | VmPThingsWarp1158Slice.AbsolutePosition, pitch: -5);
        Assert.Equal(5, clamped.Caller.Pitch);
        var added = Warp(player, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.AbsolutePosition, pitch: 40);
        Assert.Equal(10, added.Caller.Pitch);

        var bobbed = Warp(caller, reference, 0, 0, 0, 0, VmPThingsWarp1158Slice.Bob | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(8, bobbed.Caller.Z, 9);
        var still = Warp(caller, reference with { BobPhase = 0 }, 0, 0, 1, 0, VmPThingsWarp1158Slice.Bob | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(1, still.Caller.Z);
        var quiet = Warp(caller, reference with { Flags2 = 0, BobPhase = 16 }, 0, 0, 1, 0, VmPThingsWarp1158Slice.Bob | VmPThingsWarp1158Slice.AbsolutePosition);
        Assert.Equal(1, quiet.Caller.Z);

        var held = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.Interpolate);
        Assert.Equal(13, held.Caller.X);
        Assert.Equal(1, held.Caller.PrevX);
        Assert.Equal(10, held.Caller.PrevYaw);

        var warped = Warp(caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.WarpInterpolation, oldToNewX: 2);
        Assert.Equal(11, warped.Caller.PrevX);
        Assert.Equal(15, warped.Caller.PrevY);
        Assert.Equal(29, warped.Caller.PrevZ);
        Assert.Equal(10, warped.Caller.PrevYaw);
        Assert.Equal(9, warped.Caller.PrevPortalGroup);

        var copiedPrev = Warp(
            caller, reference, 3, 4, 1, 0, VmPThingsWarp1158Slice.CopyInterpolation, refPrevX: 5, refGroupX: 1);
        Assert.Equal(17, copiedPrev.Caller.PrevX);
        Assert.Equal(16, copiedPrev.Caller.PrevY);
        Assert.Equal(31, copiedPrev.Caller.PrevZ);
        Assert.Equal(10, copiedPrev.Caller.PrevYaw);

        var slid = Warp(caller, reference, 3, 4, 1, 0, 0, applyTry: true, tryX: 99, tryY: 98);
        Assert.True(slid.TriedMove);
        Assert.Equal(99, slid.Caller.X);
        Assert.Equal(98, slid.Caller.Y);
        Assert.Equal(31, slid.Caller.Z);
    }

    private static VmPThingsWarp1158Slice.WarpResult Warp(
        VmPThingsWarp1158Slice.Body caller,
        VmPThingsWarp1158Slice.Body reference,
        double xofs,
        double yofs,
        double zofs,
        double angle,
        int flags,
        bool fits = true,
        double portalX = 0,
        double portalY = 0,
        double heightOffset = 0,
        double radiusOffset = 0,
        double floorZ = 0,
        double pitch = 0,
        double oldToNewX = 0,
        double refPrevX = 0,
        double refGroupX = 0,
        bool applyTry = false,
        double tryX = 0,
        double tryY = 0)
    {
        return VmPThingsWarp1158Slice.Warp(
            true, true, caller, reference, xofs, yofs, zofs, angle, flags, heightOffset, radiusOffset, pitch,
            fits, portalX, portalY, floorZ, 9, oldToNewX, 0, refPrevX, 0, refGroupX, 0, 0, applyTry, tryX, tryY);
    }

    private static VmPThingsWarp1158Slice.Neighbor Near(
        int id,
        bool exact,
        double distance,
        bool unmorphed = false,
        bool kind = false,
        bool map = true,
        double z = 0,
        double height = 16,
        bool killed = false,
        bool sight = true)
    {
        return new VmPThingsWarp1158Slice.Neighbor(id, unmorphed, kind, exact, map, distance, z, height, killed, sight);
    }

    private static VmPThingsWarp1158Slice.Body Body(
        int id,
        double x,
        double y,
        double z,
        double yaw = 0,
        double pitch = 0,
        double velX = 0,
        double velY = 0,
        double velZ = 0,
        double height = 10,
        double radius = 5,
        double prevX = 0,
        double prevY = 0,
        double prevZ = 0,
        int flags2 = 0,
        double bobPhase = 0,
        double bobFactor = 1,
        double bobStrength = 1)
    {
        return new VmPThingsWarp1158Slice.Body(
            id, x, y, z, velX, velY, velZ, yaw, pitch, height, radius,
            prevX, prevY, prevZ, yaw, pitch, 2, 2, false, -89, 89, flags2, bobPhase, bobFactor, bobStrength);
    }
}
