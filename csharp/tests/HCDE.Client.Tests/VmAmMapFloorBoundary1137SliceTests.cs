namespace HCDE.Client.Tests;

public class VmAmMapFloorBoundary1137SliceTests
{
    private const int Exists = VmAmMapFloorBoundary1137Slice.FloorExists;

    [Fact]
    public void RedPolyStrokes_DrawOnlyTheShownSubsector()
    {
        Assert.Equal((255, 0, 0), VmAmMapFloorBoundary1137Slice.PolySegColor());
        var strokes = VmAmMapFloorBoundary1137Slice.RedPolyStrokes(
            2,
            new (int, double, double, double, double)[]
            {
                (1, 0, 0, 1, 1),
                (2, 5, 6, 7, 8)
            },
            true,
            (x, y) => (x + 1, y));
        Assert.Single(strokes);
        Assert.Equal((6d, 6d, 8d, 8d), strokes[0]);
    }

    [Fact]
    public void HasFloorBoundary_MatchesModelAndFlags()
    {
        var real = new VmAmMapFloorBoundary1137Slice.FloorRover(Exists, 255, 1);
        var inside = new VmAmMapFloorBoundary1137Slice.FloorRover(Exists | VmAmMapFloorBoundary1137Slice.FloorThisInside, 255, 9);
        var invisible = new VmAmMapFloorBoundary1137Slice.FloorRover(Exists, 0, 9);
        Assert.False(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(
            System.Array.Empty<VmAmMapFloorBoundary1137Slice.FloorRover>(),
            System.Array.Empty<VmAmMapFloorBoundary1137Slice.FloorRover>()));
        Assert.False(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(new[] { inside, invisible }, new[] { invisible }));
        Assert.True(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(new[] { real }, System.Array.Empty<VmAmMapFloorBoundary1137Slice.FloorRover>()));
        Assert.False(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(new[] { real }, new[] { real }));
        Assert.True(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(
            new[] { real },
            new[] { new VmAmMapFloorBoundary1137Slice.FloorRover(Exists, 255, 2) }));
        Assert.True(VmAmMapFloorBoundary1137Slice.HasFloorBoundary(
            new[] { real },
            new[] { new VmAmMapFloorBoundary1137Slice.FloorRover(Exists | 8, 255, 1) }));
    }

    [Fact]
    public void SectorActions_RequireUseOnOneSidedLinesAndSkipFriends()
    {
        var none = System.Array.Empty<VmAmMapFloorBoundary1137Slice.SectorAction>();
        var oneSided = new[]
        {
            new VmAmMapFloorBoundary1137Slice.SectorAction(0, VmAmMapFloorBoundary1137Slice.Teleport, 0, new[] { 1 }),
            new VmAmMapFloorBoundary1137Slice.SectorAction(VmAmMapFloorBoundary1137Slice.SecUse, VmAmMapFloorBoundary1137Slice.Teleport, 0, new[] { 2 })
        };
        var used = VmAmMapFloorBoundary1137Slice.TeleportBoundary(0, 0, System.Array.Empty<int>(), false, oneSided, none);
        Assert.True(used.Found);
        Assert.Equal(2, used.Args[0]);

        var friend = new[]
        {
            new VmAmMapFloorBoundary1137Slice.SectorAction(
                VmAmMapFloorBoundary1137Slice.SecUse,
                VmAmMapFloorBoundary1137Slice.Teleport,
                VmAmMapFloorBoundary1137Slice.Friendly,
                new[] { 3 })
        };
        Assert.False(VmAmMapFloorBoundary1137Slice.TeleportBoundary(0, 0, System.Array.Empty<int>(), false, friend, none).Found);
    }

    [Fact]
    public void TeleportAndExitBoundaries_PreferTheLineThenTheBackSector()
    {
        var none = System.Array.Empty<VmAmMapFloorBoundary1137Slice.SectorAction>();
        var line = VmAmMapFloorBoundary1137Slice.TeleportBoundary(
            VmAmMapFloorBoundary1137Slice.PlayerActivate, VmAmMapFloorBoundary1137Slice.TeleportNoFog, new[] { 9 }, true, none, none);
        Assert.Equal(VmAmMapFloorBoundary1137Slice.TeleportNoFog, line.Special);
        Assert.False(VmAmMapFloorBoundary1137Slice.TeleportBoundary(0, VmAmMapFloorBoundary1137Slice.Teleport, new[] { 9 }, false, none, none).Found);
        Assert.True(VmAmMapFloorBoundary1137Slice.IsTeleportSpecial(VmAmMapFloorBoundary1137Slice.TeleportZombie, System.Array.Empty<int>()));
        Assert.True(VmAmMapFloorBoundary1137Slice.IsTeleportSpecial(VmAmMapFloorBoundary1137Slice.TeleportLine, System.Array.Empty<int>()));
        Assert.False(VmAmMapFloorBoundary1137Slice.IsExitSpecial(VmAmMapFloorBoundary1137Slice.Teleport, System.Array.Empty<int>()));

        var back = new[]
        {
            new VmAmMapFloorBoundary1137Slice.SectorAction(0, VmAmMapFloorBoundary1137Slice.ExitSecret, 0, new[] { 4 })
        };
        var exit = VmAmMapFloorBoundary1137Slice.ExitBoundary(0, 0, System.Array.Empty<int>(), true, none, back);
        Assert.True(exit.Found);
        Assert.Equal(VmAmMapFloorBoundary1137Slice.ExitSecret, exit.Special);
        Assert.True(VmAmMapFloorBoundary1137Slice.IsExitSpecial(VmAmMapFloorBoundary1137Slice.TeleportNewMap, System.Array.Empty<int>()));
        Assert.True(VmAmMapFloorBoundary1137Slice.IsExitSpecial(VmAmMapFloorBoundary1137Slice.TeleportEndGame, System.Array.Empty<int>()));
        Assert.True(VmAmMapFloorBoundary1137Slice.IsExitSpecial(VmAmMapFloorBoundary1137Slice.ExitNormal, System.Array.Empty<int>()));
    }
}
