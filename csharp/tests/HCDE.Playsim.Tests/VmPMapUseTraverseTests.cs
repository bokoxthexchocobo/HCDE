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
        Assert.True(s.FoundLine);
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
}
