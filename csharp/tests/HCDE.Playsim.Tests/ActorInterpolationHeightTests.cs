namespace HCDE.Playsim.Tests;

public class ActorInterpolationHeightTests
{
    [Theory]
    [InlineData(-2, true, false, false)]
    [InlineData(-1, false, false, false)]
    [InlineData(-0.5, false, false, true)]
    [InlineData(0, false, false, true)]
    [InlineData(0.5, false, false, true)]
    [InlineData(1, false, false, false)]
    [InlineData(2, false, true, false)]
    public void HeightComparisonUsesStrictNativeEpsilon(double units, bool above, bool below, bool at)
    {
        var actor = new Actor { Z = Fixed.FromInt(10) };
        var reference = 10 + units / Fixed.Unit;
        Assert.Equal(above, actor.IsAbove(reference)); Assert.Equal(below, actor.IsBelow(reference));
        Assert.Equal(at, actor.IsAtZ(reference));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(1)]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    public void PositionInterpolationUsesPreviousAndCurrentCoordinatesWithoutClamping(double fraction)
    {
        var actor = new Actor(); actor.SetXYZ(10, 20, 30); actor.RememberPosition();
        actor.SetXYZ(14, 12, 32);
        Assert.Equal((10 + 4 * fraction, 20 - 8 * fraction, 30 + 2 * fraction), actor.InterpolatedPosition(fraction));
        Assert.Equal((14.0, 12.0, 32.0), actor.PosPlusZ(0));
        Assert.Equal(30, actor.PreviousZ.ToDouble());
    }

    [Fact]
    public void NonmovingAddZSuppressesVerticalInterpolationOnly()
    {
        var actor = new Actor(); actor.SetXYZ(10, 20, 30); actor.RememberPosition();
        actor.SetXY((14, 12)); actor.AddZ(2, moving: false);
        Assert.Equal((12.0, 16.0, 32.0), actor.InterpolatedPosition(0.5));
    }
}
