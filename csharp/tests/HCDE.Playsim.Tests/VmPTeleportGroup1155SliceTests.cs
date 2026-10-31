namespace HCDE.Playsim.Tests;

public class VmPTeleportGroup1155SliceTests
{
    [Fact]
    public void Other_RequiresBothTidsAndDropsKeepOrientationWhenFogging()
    {
        var blocked = VmPTeleportGroup1155Slice.TeleportOther(0, 4, true, new[] { true });
        Assert.False(blocked.Did);
        Assert.Equal(0, blocked.Calls);

        var quiet = VmPTeleportGroup1155Slice.TeleportOther(3, 4, false, Array.Empty<bool>());
        Assert.False(quiet.Did);
        Assert.Equal(VmPTeleportGroup1155Slice.KeepOrientation, quiet.Flags);
        Assert.Equal(0, quiet.Calls);

        var fogged = VmPTeleportGroup1155Slice.TeleportOther(3, 4, true, new[] { false, true });
        Assert.True(fogged.Did);
        Assert.Equal(VmPTeleportGroup1155Slice.DestFog | VmPTeleportGroup1155Slice.SourceFog, fogged.Flags);
        Assert.Equal(2, fogged.Calls);
        Assert.Equal(0, VmPTeleportGroup1155Slice.Normalize360(360));
        Assert.Equal(270, VmPTeleportGroup1155Slice.Normalize360(-90));
        Assert.NotEqual(370, VmPTeleportGroup1155Slice.Normalize360(370));
    }

    [Fact]
    public void MoveOne_RotatesAroundTheSourceAndNormalizesYaw()
    {
        var held = VmPTeleportGroup1155Slice.MoveOne(3, 4, 2, 30, 1, 1, 1, 10, 10, 20, 5, 10, false, false, true);
        Assert.Equal(12, held.X);
        Assert.Equal(23, held.Y);
        Assert.Equal(6, held.Z);
        Assert.Equal(VmPTeleportGroup1155Slice.KeepOrientation, held.Flags);
        Assert.Equal(VmPTeleportGroup1155Slice.Normalize360(30), held.Yaw);
        Assert.True(held.Did);

        var wrapped = VmPTeleportGroup1155Slice.MoveOne(0, 0, 0, 30, 0, 0, 0, 10, 0, 0, 0, 350, false, false, true);
        Assert.Equal(VmPTeleportGroup1155Slice.Normalize360(370), wrapped.Yaw);

        var turned = VmPTeleportGroup1155Slice.MoveOne(3, 4, 2, 0, 0, 0, 0, 0, 10, 20, 5, 90, true, true, false);
        Assert.Equal(6, turned.X, 9);
        Assert.Equal(23, turned.Y, 9);
        Assert.Equal(VmPTeleportGroup1155Slice.OnFloorZ, turned.Z);
        Assert.Equal(
            VmPTeleportGroup1155Slice.DestFog | VmPTeleportGroup1155Slice.SourceFog | VmPTeleportGroup1155Slice.KeepOrientation,
            turned.Flags);
        Assert.False(turned.Did);
        Assert.Equal(VmPTeleportGroup1155Slice.Normalize360(90), turned.Yaw);
    }

    [Fact]
    public void GroupAndSector_FallBackAndFilterTouches()
    {
        var missing = VmPTeleportGroup1155Slice.TeleportGroup(
            5, 9, false, 0, 0, 0, 0, false, 0, 0, 0, 0, false, false, new VmPTeleportGroup1155Slice.ActorSpot(0, 0, 0, 0, false),
            true, true, true, new[] { new VmPTeleportGroup1155Slice.ActorSpot(1, 0, 0, 0, true) });
        Assert.True(missing.FellBack);
        Assert.True(missing.Did);
        Assert.Equal(VmPTeleportGroup1155Slice.DestFog | VmPTeleportGroup1155Slice.SourceFog, missing.Flags);
        Assert.False(missing.SourceMoved);

        var noDest = VmPTeleportGroup1155Slice.TeleportGroup(
            5, 9, true, 0, 0, 0, 0, false, 0, 0, 0, 0, false, true, new VmPTeleportGroup1155Slice.ActorSpot(1, 2, 3, 4, true),
            true, false, true, Array.Empty<VmPTeleportGroup1155Slice.ActorSpot>());
        Assert.False(noDest.Did);
        Assert.Equal(0, noDest.Calls);

        var direct = VmPTeleportGroup1155Slice.TeleportGroup(
            0, 9, true, 0, 0, 1, 10, true, 8, 9, 4, 10, false, true, new VmPTeleportGroup1155Slice.ActorSpot(3, 4, 2, 30, true),
            true, true, false, new[] { new VmPTeleportGroup1155Slice.ActorSpot(99, 99, 99, 99, false) });
        Assert.False(direct.FellBack);
        Assert.True(direct.FloorZ);
        Assert.Single(direct.Actors);
        Assert.Equal(11, direct.Actors[0].X);
        Assert.Equal(13, direct.Actors[0].Y);
        Assert.Equal(
            VmPTeleportGroup1155Slice.DestFog | VmPTeleportGroup1155Slice.SourceFog | VmPTeleportGroup1155Slice.KeepOrientation,
            direct.Actors[0].Flags);
        Assert.True(direct.SourceMoved);
        Assert.Equal(VmPTeleportGroup1155Slice.KeepOrientation, direct.SourceFlags);
        Assert.Equal(VmPTeleportGroup1155Slice.OnFloorZ, direct.SourceZ);
        Assert.Equal(10, direct.SourceYaw);
        Assert.Equal(2, direct.Calls);
        Assert.True(direct.Did);

        var idle = VmPTeleportGroup1155Slice.TeleportGroup(
            4, 9, true, 0, 0, 0, 0, true, 1, 1, 1, 1, true, false, new VmPTeleportGroup1155Slice.ActorSpot(0, 0, 0, 0, false),
            true, false, true, new[] { new VmPTeleportGroup1155Slice.ActorSpot(2, 2, 2, 2, false) });
        Assert.False(idle.FloorZ);
        Assert.False(idle.Did);
        Assert.False(idle.SourceMoved);
        Assert.Equal(1, idle.Calls);

        var sector = VmPTeleportGroup1155Slice.TeleportSector(
            true, 0, 0, 0, 0, true, 5, 6, 7, 0, true, false, 7,
            new[]
            {
                new VmPTeleportGroup1155Slice.Touch(false, 7, 1, 1, 1, 1, true),
                new VmPTeleportGroup1155Slice.Touch(true, 3, 1, 0, 0, 0, true),
                new VmPTeleportGroup1155Slice.Touch(true, 7, 2, 0, 1, 0, false),
            });
        Assert.False(sector.FloorZ);
        Assert.Equal(1, sector.Calls);
        Assert.False(sector.Did);
        Assert.Equal(7, sector.Actors[0].X);
        Assert.Equal(6, sector.Actors[0].Y);
        Assert.Equal(8, sector.Actors[0].Z);

        var open = VmPTeleportGroup1155Slice.TeleportSector(
            false, 0, 0, 0, 0, true, 0, 0, 0, 0, false, false, 0, Array.Empty<VmPTeleportGroup1155Slice.Touch>());
        Assert.False(open.Did);
        Assert.Empty(open.Actors);
    }
}
