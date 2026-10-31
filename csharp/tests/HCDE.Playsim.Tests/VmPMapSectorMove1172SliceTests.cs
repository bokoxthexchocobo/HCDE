using System.Linq;

namespace HCDE.Playsim.Tests;

public class VmPMapSectorMove1172SliceTests
{
    [Fact]
    public void FloorRaise_LiftsTheIntersectingThingAndRestoresARejectedPush()
    {
        var stuck = new VmPMapSectorMove1172Slice.Mover { Z = 1, FloorZ = 0, IsPlayerBody = true, ViewZ = 10 };
        var host = new VmPMapSectorMove1172Slice.MoveHost { PushUp = 1 };
        host.Apply = (thing, _) => thing.FloorZ = 5;
        var change = new VmPMapSectorMove1172Slice.Change { Instant = true };
        VmPMapSectorMove1172Slice.FloorRaise(stuck, change, host);
        Assert.Equal(5d, stuck.Z);
        Assert.Equal(4d, stuck.PrevZ);
        Assert.Equal(VmPMapSectorMove1172Slice.RfNoInterpolateView, stuck.RenderFlags);
        Assert.Equal(14d, stuck.ViewZ);
        Assert.Equal(new[] { "adjust", "clear", "crunch", "trigger:1", "render" }, host.Events);

        var bridge = new VmPMapSectorMove1172Slice.Mover { Z = 0, FloorZ = 0, Flags4 = VmPMapSectorMove1172Slice.Mf4ActLikeBridge, ViewZ = 3 };
        var blocked = new VmPMapSectorMove1172Slice.MoveHost();
        blocked.Apply = (thing, _) => thing.FloorZ = 4;
        var stop = new VmPMapSectorMove1172Slice.Change();
        VmPMapSectorMove1172Slice.FloorRaise(bridge, stop, blocked);
        Assert.True(stop.NoFit);
        Assert.Equal(0d, bridge.Z);
        Assert.Equal(3d, bridge.ViewZ);
        Assert.Equal(new[] { "adjust" }, blocked.Events);

        var same = new VmPMapSectorMove1172Slice.MoveHost();
        VmPMapSectorMove1172Slice.FloorRaise(new VmPMapSectorMove1172Slice.Mover { Z = 2, FloorZ = 2 }, new VmPMapSectorMove1172Slice.Change(), same);
        Assert.Equal(new[] { "adjust" }, same.Events);

        var flyer = new VmPMapSectorMove1172Slice.Mover
        {
            Z = 8,
            FloorZ = 0,
            Flags = VmPMapSectorMove1172Slice.MfNoGravity,
            Flags6 = VmPMapSectorMove1172Slice.Mf6RelativeToFloor,
            IsPlayerBody = true,
            ViewZ = 1,
        };
        var rejected = new VmPMapSectorMove1172Slice.MoveHost { PushUp = 2 };
        rejected.Apply = (thing, _) => thing.FloorZ = 3;
        var instant = new VmPMapSectorMove1172Slice.Change { Instant = true };
        VmPMapSectorMove1172Slice.FloorRaise(flyer, instant, rejected);
        Assert.Equal(8d, flyer.Z);
        Assert.Equal(3d, flyer.PrevZ);
        Assert.Equal(1d, flyer.ViewZ);
        Assert.DoesNotContain(rejected.Events, item => item.StartsWith("trigger", StringComparison.Ordinal));
    }

    [Fact]
    public void Ceiling_DropsOntoTheFloorAndRidesAFailedPass()
    {
        var crushed = new VmPMapSectorMove1172Slice.Mover { Z = 0, FloorZ = 0, Height = 40, CeilingZ = 30, IsPlayerBody = true, ViewZ = 4 };
        var lower = new VmPMapSectorMove1172Slice.MoveHost { PushDown = 2 };
        lower.Apply = (thing, _) =>
        {
            thing.FloorZ = 1;
            thing.CeilingZ = 20;
        };
        VmPMapSectorMove1172Slice.CeilingLower(crushed, new VmPMapSectorMove1172Slice.Change(), lower);
        Assert.Equal(1d, crushed.Z);
        Assert.Equal(5d, crushed.ViewZ);
        Assert.Contains("crunch", lower.Events);
        Assert.Contains("trigger:0", lower.Events);

        var raised = new VmPMapSectorMove1172Slice.Mover { Z = -2, FloorZ = 0, Height = 16, CeilingZ = 10, IsPlayerBody = true, ViewZ = 8 };
        var lift = new VmPMapSectorMove1172Slice.MoveHost();
        VmPMapSectorMove1172Slice.CeilingRaise(raised, new VmPMapSectorMove1172Slice.Change { MoveAmt = 4 }, lift);
        Assert.Equal(-6d, raised.Z);
        Assert.Equal(4d, raised.ViewZ);
        Assert.Contains("trigger:-2", lift.Events);

        var rider = new VmPMapSectorMove1172Slice.Mover
        {
            Z = 5,
            Height = 4,
            CeilingZ = 20,
            Flags2 = VmPMapSectorMove1172Slice.Mf2PassMobj,
        };
        var onTop = new VmPMapSectorMove1172Slice.Mover { Z = 4, Height = 8 };
        var pass = new VmPMapSectorMove1172Slice.MoveHost { AdjustOk = false, TestZOk = false, OnMover = onTop };
        VmPMapSectorMove1172Slice.CeilingRaise(rider, new VmPMapSectorMove1172Slice.Change(), pass);
        Assert.Equal(12d, rider.Z);
        Assert.Equal(new[] { "adjust", "render" }, pass.Events);

        var bridge = new VmPMapSectorMove1172Slice.Mover { Flags4 = VmPMapSectorMove1172Slice.Mf4ActLikeBridge, ViewZ = 9 };
        var skipped = new VmPMapSectorMove1172Slice.MoveHost();
        VmPMapSectorMove1172Slice.CeilingRaise(bridge, new VmPMapSectorMove1172Slice.Change(), skipped);
        Assert.Equal(9d, bridge.ViewZ);
        Assert.Equal(new[] { "adjust" }, skipped.Events);
    }

    [Fact]
    public void Change_InvertsAttachedMovementAndUpdatesFakeWater()
    {
        var room = new VmPMapSectorMove1172Slice.Sec { Name = "room" };
        var hidden = new VmPMapSectorMove1172Slice.Mover { Flags = VmPMapSectorMove1172Slice.MfNoBlockMap };
        var carried = new VmPMapSectorMove1172Slice.Mover { Flags = VmPMapSectorMove1172Slice.MfNoBlockMap, Flags5 = VmPMapSectorMove1172Slice.Mf5MoveWithSector };
        var body = new VmPMapSectorMove1172Slice.Mover();
        room.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = hidden });
        room.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = carried });
        room.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = body });
        var wet = new VmPMapSectorMove1172Slice.Sec { Name = "wet" };
        var swimmer = new VmPMapSectorMove1172Slice.Mover { Z = 10, Sector = wet };
        var visitor = new VmPMapSectorMove1172Slice.Mover { Z = 10, Sector = room };
        wet.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = swimmer });
        wet.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = visitor });
        room.FakeFloors.Add(wet);
        var host = new VmPMapSectorMove1172Slice.MoveHost();
        Assert.False(VmPMapSectorMove1172Slice.ChangeSector(room, 2, -3, 0, false, true, host));
        Assert.Equal(new[] { "floor-drop", "floor-drop" }, host.Iterators);
        Assert.Equal(("room", 0), Assert.Single(host.Portals));
        Assert.Equal(13d, Assert.Single(host.WaterZ));
        Assert.Contains("recalc:room", host.Events);
        Assert.Contains("water:wet", host.Events);
        Assert.Contains("trigger:13", host.Events);

        var linked = new VmPMapSectorMove1172Slice.Sec { Name = "link" };
        var control = new VmPMapSectorMove1172Slice.Sec { Name = "control", HeightSec = room };
        room.Attached.Add(control);
        room.Attached.Add(linked);
        linked.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = new VmPMapSectorMove1172Slice.Mover() });
        var inverted = new VmPMapSectorMove1172Slice.MoveHost();
        VmPMapSectorMove1172Slice.ChangeSector(room, 0, 2, 1, true, false, inverted);
        Assert.Equal(new[] { "recalc:control", "recalc:link", "recalc:room" }, inverted.Events.Where(item => item.StartsWith("recalc", StringComparison.Ordinal)).ToArray());
        Assert.Contains(("link", 0), inverted.Portals);
        Assert.DoesNotContain(("control", 0), inverted.Portals);
        Assert.Contains(("room", 1), inverted.Portals);
        Assert.Contains("floor-raise", inverted.Iterators);
        Assert.Contains("ceiling-raise", inverted.Iterators);
        Assert.Empty(inverted.WaterZ);

        var mid = new VmPMapSectorMove1172Slice.MoveHost();
        var midRoom = new VmPMapSectorMove1172Slice.Sec { Name = "mid" };
        midRoom.Touching.Add(new VmPMapSectorMove1172Slice.Touch { Thing = new VmPMapSectorMove1172Slice.Mover() });
        VmPMapSectorMove1172Slice.ChangeSector(midRoom, 1, 4, 2, true, false, mid);
        Assert.True(mid.MoveMidTex);
        Assert.Equal(new[] { "floor-raise", "ceiling-raise" }, mid.Iterators);
        Assert.Empty(mid.Portals);

        var bad = new VmPMapSectorMove1172Slice.MoveHost();
        var nowhere = new VmPMapSectorMove1172Slice.Sec { Name = "nope" };
        Assert.False(VmPMapSectorMove1172Slice.ChangeSector(nowhere, 0, 1, 3, false, false, bad));
        Assert.Equal(new[] { "recalc:nope" }, bad.Events);
        Assert.Empty(bad.Iterators);
    }
}
