namespace HCDE.Playsim.Tests;

public class VmPMapUseTraverseTests
{
    [Fact]
    public void UseTraverse_ActivatesUseLine()
    {
        var activated = false;
        var s = new VmPMapUseTraverse.UseTraverseState
        {
            Usething = new object(),
            NextIntercept = () => new VmPMapUseTraverse.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseTraverse.LineSample { Special = 1, Activation = VmPMapUseTraverse.SpacUse },
            },
            PointOnLineSide = (_, _, _) => 0,
            ActivateLine = (_, _, _) => activated = true,
        };
        Assert.True(VmPMapUseTraverse.UseTraverse(s));
        Assert.True(activated);
    }

    [Fact]
    public void UseTraverse_SetsFoundLineWhenBlocked()
    {
        var s = new VmPMapUseTraverse.UseTraverseState
        {
            Usething = new object(),
            NextIntercept = () => new VmPMapUseTraverse.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseTraverse.LineSample(),
            },
            LineOpeningRange = (_, _, _, _) => 0,
            HasPlayer = true,
        };
        Assert.True(VmPMapUseTraverse.UseTraverse(s));
        Assert.False(s.FoundLine);
    }

    [Fact]
    public void NoWayTraverse_BlocksWhenRangeZero()
    {
        var s = new VmPMapUseTraverse.NoWayState
        {
            ActorZ = 0,
            ActorTop = 40,
            MaxStepHeight = 24,
            NextLine = () => new VmPMapUseTraverse.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseTraverse.LineSample(),
            },
            LineOpening = (_, _, _, _) => 0,
        };
        Assert.True(VmPMapUseTraverse.NoWayTraverse(s));
    }

    [Fact]
    public void UseLines_PlaysFailWhenBlocked()
    {
        var fail = false;
        var s = new VmPMapUseTraverse.UseLinesState
        {
            Player = new VmPMapUseTraverse.UseLinesPlayer
            {
                Mo = new object(),
                UseRange = 64,
                Height = 40,
                YawDegrees = 0,
            },
            PortalTransition = _ => (0, 0),
            UseTraverse = (_, _) => false,
            NoWayTraverse = (_, _, _) => true,
            PlayUseFail = () => fail = true,
        };
        VmPMapUseTraverse.UseLines(s);
        Assert.True(fail);
    }

    [Fact]
    public void UsePuzzleItem_RejectsWrongType()
    {
        var s = new VmPMapUseTraverse.PuzzleItemState
        {
            User = new object(),
            PuzzleItemType = 1,
            UsePuzzleItemLineSpecial = 999,
            Next = _ => new VmPMapUseTraverse.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseTraverse.LineSample { Special = 999, Args = new[] { 9, 12, 0, 0, 0 } },
            },
            PointOnLineSide = (_, _, _) => 0,
        };
        Assert.False(VmPMapUseTraverse.UsePuzzleItem(s));
    }

    [Fact]
    public void UsePuzzleItem_MatchesLineArgs()
    {
        var scripted = false;
        var s = new VmPMapUseTraverse.PuzzleItemState
        {
            User = new object(),
            PuzzleItemType = 7,
            UsePuzzleItemLineSpecial = 999,
            Next = _ => new VmPMapUseTraverse.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseTraverse.LineSample { Special = 999, Args = new[] { 7, 12, 0, 0, 0 } },
            },
            PointOnLineSide = (_, _, _) => 0,
            StartScript = (_, _, _, _) => scripted = true,
        };
        Assert.True(VmPMapUseTraverse.UsePuzzleItem(s));
        Assert.True(scripted);
    }
}
