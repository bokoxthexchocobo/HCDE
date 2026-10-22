namespace HCDE.Playsim.Tests;

public class VmPMapUseLinesPuzzleTests
{
    [Fact]
    public void UseLines_PlaysFailWhenBlocked()
    {
        var fail = false;
        var s = new VmPMapUseLinesPuzzle.UseLinesState
        {
            Player = new VmPMapUseLinesPuzzle.UseLinesPlayer
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
        VmPMapUseLinesPuzzle.UseLines(s);
        Assert.True(fail);
    }

    [Fact]
    public void UsePuzzleItem_RejectsWrongType()
    {
        var s = new VmPMapUseLinesPuzzle.PuzzleItemState
        {
            User = new object(),
            PuzzleItemType = 1,
            UsePuzzleItemLineSpecial = 999,
            Next = _ => new VmPMapUseLinesPuzzle.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseLinesPuzzle.LineSample { Special = 999, Args = new[] { 9, 12, 0, 0, 0 } },
            },
            PointOnLineSide = (_, _, _) => 0,
        };
        Assert.False(VmPMapUseLinesPuzzle.UsePuzzleItem(s));
    }

    [Fact]
    public void UsePuzzleItem_MatchesLineArgs()
    {
        var scripted = false;
        var s = new VmPMapUseLinesPuzzle.PuzzleItemState
        {
            User = new object(),
            PuzzleItemType = 7,
            UsePuzzleItemLineSpecial = 999,
            Next = _ => new VmPMapUseLinesPuzzle.Intercept
            {
                IsLine = true,
                Line = new VmPMapUseLinesPuzzle.LineSample { Special = 999, Args = new[] { 7, 12, 0, 0, 0 } },
            },
            PointOnLineSide = (_, _, _) => 0,
            StartScript = (_, _, _, _) => scripted = true,
        };
        Assert.True(VmPMapUseLinesPuzzle.UsePuzzleItem(s));
        Assert.True(scripted);
    }
}
