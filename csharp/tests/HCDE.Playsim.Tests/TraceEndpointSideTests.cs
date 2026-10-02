using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TraceEndpointSideTests
{
    [Theory]
    [InlineData(0, 10, true)]
    [InlineData(10, 0, true)]
    [InlineData(0, -10, false)]
    [InlineData(-10, 0, false)]
    [InlineData(-10, 10, true)]
    public void EndpointCrossingUsesNativeSideClassification(double firstY, double secondY, bool hit)
    {
        var distance = CombatTrace.RayLine(0, 0, 1, 0,
            new LevelLine { X1 = 20, Y1 = firstY, X2 = 20, Y2 = secondY }, 100);
        Assert.Equal(hit, double.IsFinite(distance));
        if (hit) Assert.Equal(20, distance);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(1000000, true)]
    public void SideToleranceUsesFullTraversalDelta(double scale, bool hit)
    {
        var distance = CombatTrace.RayLine(0, 0, 1, 0,
            new LevelLine { X1 = 20, Y1 = -1e-10, X2 = 20, Y2 = 1e-10 }, scale);
        Assert.Equal(hit, double.IsFinite(distance));
        if (hit) Assert.Equal(20, distance);
    }

    [Fact]
    public void IntersectionJustBehindSourceIsNotClampedToOrigin()
    {
        Assert.True(double.IsPositiveInfinity(CombatTrace.RayLine(0, 0, 1, 0,
            new LevelLine { X1 = -1e-10, Y1 = -10, X2 = -1e-10, Y2 = 10 }, 100)));
    }
}
