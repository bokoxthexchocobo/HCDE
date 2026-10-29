namespace HCDE.Playsim.Tests;

public class VmPMapTryMove1174SliceTests
{
    [Fact]
    public void Link_PlacesTheActorAndTriggersEyes()
    {
        var thing = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            X = 1,
            Y = 2,
            Sector = "home",
            Flags2 = VmPMapTryMove1174Slice.Mf2FloorClip,
        };
        var tm = new VmPMapTryMove1173Slice.Trace
        {
            FloorZ = 4,
            CeilingZ = 200,
            DropoffZ = 1,
            FloorPic = 3,
            FloorTerrain = "rock",
            FloorSector = "tile",
            CeilingPic = 8,
            CeilingSector = "sky",
        };
        var link = new VmPMapTryMove1174Slice.LinkHost
        {
            NewHasHeightSec = true,
            NewHasAction = true,
            NewFakeFloorZ = 10,
            NewFakeCeilingZ = 20,
        };
        link.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 1, Special = 5, Side = 1, OldSide = 0 });
        link.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 2, Special = 9, Side = 1, OldSide = 0 });
        link.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 3, Special = 4, Side = 1, OldSide = 1 });
        var host = new VmPMapTryMove1173Slice.CrossingHost
        {
            OldHasHeightSec = true,
            OldFakeFloorZ = 100,
            OldFakeCeilingZ = 200,
        };
        var moved = VmPMapTryMove1174Slice.TryMove(thing, 6, 7, 1, false, 0, tm, false, host, link);
        Assert.True(moved);
        Assert.Equal(6d, thing.X);
        Assert.Equal(7d, thing.Y);
        Assert.Equal(4d, thing.FloorZ);
        Assert.Equal(200d, thing.CeilingZ);
        Assert.Equal(1d, thing.DropoffZ);
        Assert.Equal(3, thing.FloorPic);
        Assert.Equal("rock", thing.FloorTerrain);
        Assert.Equal("tile", thing.FloorSector);
        Assert.Equal(8, thing.CeilingPic);
        Assert.Equal("sky", thing.CeilingSector);
        Assert.Equal("home", link.LinkedFromSector);
        Assert.Equal(0, thing.Flags6);
        Assert.Equal(
            new[]
            {
                "unlink",
                "link",
                "floorclip",
                "cross:1:2",
                "cross:1:1",
                "eyes:" + VmPMapTryMove1174Slice.SecEyesSurface,
                "eyes:" + VmPMapTryMove1174Slice.SecEyesAboveC,
                "transition:home",
            },
            link.Events);

        var diver = new VmPMapTryMove1173Slice.Actor { IsPlayer = true, ViewHeight = 41, Sector = "home" };
        var diveLink = new VmPMapTryMove1174Slice.LinkHost
        {
            NewHasHeightSec = true,
            NewHasAction = true,
            NewFakeFloorZ = 50,
            NewFakeCeilingZ = 80,
            HeightMoreFlags = VmPMapTryMove1174Slice.SecMfFakeFloorOnly,
        };
        var diveHost = new VmPMapTryMove1173Slice.CrossingHost { OldHasHeightSec = true, OldFakeFloorZ = 10, OldFakeCeilingZ = 20 };
        Assert.True(VmPMapTryMove1174Slice.TryMove(diver, 1, 1, 1, false, 0, Room(), false, diveHost, diveLink));
        Assert.Contains("eyes:" + VmPMapTryMove1174Slice.SecEyesDive, diveLink.Events);
        Assert.DoesNotContain(diveLink.Events, item => item.Contains("eyes:" + VmPMapTryMove1174Slice.SecEyesBelowC, StringComparison.Ordinal));

        var porter = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = false,
            Flags = VmPMapTryMove1173Slice.MfNoClip,
            FloorZ = 3,
            Flags2 = VmPMapTryMove1174Slice.Mf2FloorClip,
        };
        var gate = new VmPMapTryMove1173Slice.CrossingHost();
        gate.Portals.Add(new VmPMapTryMove1173Slice.PortalHit { Id = 1, Frac = 0.2, DispX = 20, DispY = 4 });
        var portLink = new VmPMapTryMove1174Slice.LinkHost();
        var portTm = new VmPMapTryMove1173Slice.Trace { FloorZ = 9 };
        Assert.True(VmPMapTryMove1174Slice.TryMove(porter, 5, 0, 0, false, 0, portTm, false, gate, portLink));
        Assert.Equal(25d, porter.X);
        Assert.Equal(4d, porter.Y);
        Assert.Equal(3d, porter.FloorZ);
        Assert.Equal(new[] { "floorclip", "transition:home" }, portLink.Events);

        var predicting = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            Cheats = VmPMapTryMove1173Slice.CfPredicting,
            Sector = "home",
        };
        var predictLink = new VmPMapTryMove1174Slice.LinkHost { RelX = 9, RelY = 9, RelZ = 9, RelSector = "far", RelGroup = 4, NewHasHeightSec = true, NewHasAction = true };
        predictLink.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 4, Special = 2, Side = 1, OldSide = 0 });
        var predictHost = new VmPMapTryMove1173Slice.CrossingHost { CheckSetsPortalStep = true, OldHasHeightSec = true };
        Assert.True(VmPMapTryMove1174Slice.TryMove(predicting, 2, 3, 1, false, 0, Room(), false, predictHost, predictLink));
        Assert.Equal(2d, predicting.X);
        Assert.Equal(3d, predicting.Y);
        Assert.Equal(0d, predicting.Z);
        Assert.Equal(0, predicting.Flags6);
        Assert.Contains("predict:1:4", predictLink.Events);
        Assert.DoesNotContain(predictLink.Events, item => item.StartsWith("transition", StringComparison.Ordinal));
        Assert.DoesNotContain(predictLink.Events, item => item.StartsWith("eyes:", StringComparison.Ordinal));
        Assert.DoesNotContain("floorceil", predictLink.Events);
    }

    [Fact]
    public void Push_RestoresZAndPushesSpecialsInReverse()
    {
        var blasted = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            Z = 5,
            Flags2 = VmPMapTryMove1173Slice.Mf2Blasted,
            Mass = 100,
        };
        var link = new VmPMapTryMove1174Slice.LinkHost();
        link.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 1, Side = 0 });
        link.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 2, Side = 1 });
        var host = new VmPMapTryMove1173Slice.CrossingHost { PositionOk = false };
        Assert.False(VmPMapTryMove1174Slice.TryMove(blasted, 4, 0, 0, true, 9, Room(), false, host, link));
        Assert.Equal(5d, blasted.Z);
        Assert.Equal(0, blasted.Flags6);
        Assert.Equal(new[] { "damage:3:melee", "push:2:1", "push:1:0" }, link.Events);

        var predicting = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            Z = 5,
            Cheats = VmPMapTryMove1173Slice.CfPredicting,
            Flags2 = VmPMapTryMove1173Slice.Mf2Blasted,
            Mass = 100,
        };
        var quiet = new VmPMapTryMove1174Slice.LinkHost();
        quiet.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 8, Side = 1 });
        Assert.False(VmPMapTryMove1174Slice.TryMove(predicting, 1, 0, 0, true, 9, Room(), false, new VmPMapTryMove1173Slice.CrossingHost { PositionOk = false }, quiet));
        Assert.Equal(5d, predicting.Z);
        Assert.Empty(quiet.Events);

        var ghost = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            Z = 2,
            Flags = VmPMapTryMove1173Slice.MfNoClip,
            Flags2 = VmPMapTryMove1173Slice.Mf2Blasted,
        };
        var skipped = new VmPMapTryMove1174Slice.LinkHost();
        skipped.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 3, Side = 1 });
        Assert.False(VmPMapTryMove1174Slice.TryMove(ghost, 1, 0, 0, false, 0, Room(), false, new VmPMapTryMove1173Slice.CrossingHost { PositionOk = false }, skipped));
        Assert.Equal(2d, ghost.Z);
        Assert.Empty(skipped.Events);

        var monster = new VmPMapTryMove1173Slice.Actor { IsPlayer = false };
        var lines = new VmPMapTryMove1174Slice.LinkHost();
        lines.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 11, Special = 5, Side = 1, OldSide = 0 });
        lines.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 10, Special = VmPMapTryMove1174Slice.Teleport, Side = 1, OldSide = 0 });
        Assert.True(VmPMapTryMove1174Slice.TryMove(monster, 1, 1, 1, false, 0, Room(), false, new VmPMapTryMove1173Slice.CrossingHost(), lines));
        Assert.Equal(
            new[] { "unlink", "link", "mcross:4:10", "any:128:11", "transition:home" },
            lines.Events);

        var shot = new VmPMapTryMove1173Slice.Actor { IsPlayer = false, Flags2 = VmPMapTryMove1174Slice.Mf2PCross };
        var shotLink = new VmPMapTryMove1174Slice.LinkHost();
        shotLink.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 12, Special = VmPMapTryMove1174Slice.TeleportLine, Side = 1, OldSide = 0 });
        Assert.True(VmPMapTryMove1174Slice.TryMove(shot, 1, 0, 1, false, 0, Room(), false, new VmPMapTryMove1173Slice.CrossingHost(), shotLink));
        Assert.Contains("pcross:32:12", shotLink.Events);

        var silent = new VmPMapTryMove1173Slice.Actor { IsPlayer = false, Flags6 = VmPMapTryMove1174Slice.Mf6NoTrigger };
        var silentLink = new VmPMapTryMove1174Slice.LinkHost();
        silentLink.Specs.Add(new VmPMapTryMove1174Slice.SpecHit { Id = 13, Special = 9, Side = 1, OldSide = 0 });
        Assert.True(VmPMapTryMove1174Slice.TryMove(silent, 1, 0, 1, false, 0, Room(), false, new VmPMapTryMove1173Slice.CrossingHost(), silentLink));
        Assert.DoesNotContain(silentLink.Events, item => item.Contains(":13", StringComparison.Ordinal));
    }

    [Fact]
    public void ShortMove_ForwardsDropoffAndLinks()
    {
        var placed = new VmPMapTryMove1173Slice.Actor
        {
            IsPlayer = true,
            Flags = VmPMapTryMove1173Slice.MfNoClip,
            FloorSector = "other",
            X = 0,
            Y = 0,
            Z = 1,
        };
        var link = new VmPMapTryMove1174Slice.LinkHost { RelX = 8, RelY = 9, RelZ = 10, RelSector = "far", RelGroup = 4 };
        var host = new VmPMapTryMove1173Slice.CrossingHost { CheckSetsPortalStep = true };
        Assert.True(VmPMapTryMove1174Slice.TryMove(placed, 3, 4, 0, true, 6, false, host, link));
        Assert.Equal(8d, placed.X);
        Assert.Equal(9d, placed.Y);
        Assert.Equal(10d, placed.Z);
        Assert.Equal(5d, placed.PrevX);
        Assert.Equal(5d, placed.PrevY);
        Assert.Equal(4d, placed.PrevZ);
        Assert.Equal("far", placed.Sector);
        Assert.Equal(4, link.PrevPortalGroup);
        Assert.Contains("floorceil", link.Events);
        Assert.Contains("transition:home", link.Events);
        Assert.Equal(0, placed.Flags6);

        var blocked = new VmPMapTryMove1173Slice.Actor { IsPlayer = true, MaxDropOffHeight = -1, Z = 4 };
        var blockedLink = new VmPMapTryMove1174Slice.LinkHost();
        Assert.False(VmPMapTryMove1174Slice.TryMove(blocked, 1, 1, 0, false, 0, false, new VmPMapTryMove1173Slice.CrossingHost(), blockedLink));
        Assert.Equal(4d, blocked.Z);
        Assert.DoesNotContain("link", blockedLink.Events);

        var missile = new VmPMapTryMove1173Slice.Actor { IsPlayer = true, MaxDropOffHeight = -1, FloorSector = "other" };
        var missileLink = new VmPMapTryMove1174Slice.LinkHost();
        Assert.True(VmPMapTryMove1174Slice.TryMove(missile, 2, 2, 0, true, 5, true, new VmPMapTryMove1173Slice.CrossingHost(), missileLink));
        Assert.Equal(2d, missile.X);
        Assert.Equal(5d, missile.Z);
        Assert.Contains("link", missileLink.Events);
    }

    static VmPMapTryMove1173Slice.Trace Room()
    {
        return new VmPMapTryMove1173Slice.Trace { FloorZ = 0, CeilingZ = 200 };
    }
}
