using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActivationCompatSideTests
{
    [Theory]
    [InlineData(0, 1, 0, 0, true)]
    [InlineData(0, -1, 0, 0, false)]
    [InlineData(1, 0, 0, 0, false)]
    [InlineData(-1, 0, 0, 0, true)]
    [InlineData(0, 1, 1, 0, false)]
    [InlineData(1, 0, 0, 1, true)]
    [InlineData(1, 1, 0.5, 0.5, true)]
    [InlineData(1, 1, 0.5, 0.499, false)]
    [InlineData(1, 1, 0.5, 0.501, true)]
    [InlineData(0.001, 0.001, 1, 0, true)]
    public void VanillaClassificationRetainsAxisAndFixedPointRules(double dx, double dy, double x, double y, bool expected)
    {
        var line = new LevelLine { X2 = dx, Y2 = dy, Flags = LevelLine.CompatSideFlag };
        Assert.Equal(expected, LineSpecials.IsBackSide(line, x, y));
    }

    [Fact]
    public void FlagSelectsVanillaTieInsteadOfPreciseFront()
    {
        var precise = new LevelLine { X2 = 1, Y2 = 1 };
        var vanilla = new LevelLine { X2 = 1, Y2 = 1, Flags = LevelLine.CompatSideFlag };
        Assert.False(LineSpecials.IsBackSide(precise, 0.5, 0.5));
        Assert.True(LineSpecials.IsBackSide(vanilla, 0.5, 0.5));
    }
}
