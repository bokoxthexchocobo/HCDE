namespace HCDE.Playsim.Tests;

public class VmADecalSpreadTests
{
    [Fact]
    public void Length_MatchesHypot()
    {
        Assert.Equal(5, VmADecalSpread.Length(3, 4));
    }

    [Fact]
    public void GetWallStuff_FlipsDeltaWhenNotSide0()
    {
        VmADecalSpread.GetWallStuff(false, 0, 0, 1, 0, 10, 0, out var ldx, out _);
        Assert.Equal(-10, ldx);
    }

    [Fact]
    public void Spread_CallsLeftAndRight()
    {
        var left = false;
        var right = false;
        VmADecalSpread.Spread(
            1,
            1,
            _ => 32,
            _ => 0,
            _ => true,
            new object(),
            5,
            0,
            10,
            null,
            true,
            0,
            0,
            10,
            0,
            (_, _, _, _, _, _) => left = true,
            (_, _, _, _, _) => right = true);
        Assert.True(left);
        Assert.True(right);
    }
}
