namespace HCDE.Playsim.Tests;

public class ActorPositionMethodTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddZUpdatesPreviousZOnlyWhenNotMoving(bool moving)
    {
        var actor = Setup(); actor.AddZ(-2.5, moving);
        Assert.Equal(27.5, actor.Z.ToDouble());
        Assert.Equal(moving ? 30 : 27.5, actor.PreviousZ.ToDouble());
        Assert.Equal(10, actor.PreviousX.ToDouble()); Assert.Equal(20, actor.PreviousY.ToDouble());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetZRetainsPreviousPositionRegardlessOfMovingParameter(bool moving)
    {
        var actor = Setup(); actor.SetZ(12.5, moving);
        Assert.Equal(12.5, actor.Z.ToDouble()); Assert.Equal(30, actor.PreviousZ.ToDouble());
    }

    [Fact]
    public void PositionOverloadsPreservePreviousPositionAndVelocity()
    {
        var actor = Setup(); actor.VelocityZ = Fixed.FromInt(7);
        actor.SetXY((-1.5, 2.25)); Assert.Equal((-1.5, 2.25, 30.0), actor.PosAtZ(actor.Z.ToDouble()));
        actor.SetXYZ(1, 2, 3); Assert.Equal((1.0, 2.0, 3.0), actor.PosPlusZ(0));
        actor.SetXYZ((-2, -3, -4)); Assert.Equal((-2.0, -3.0, -4.0), actor.PosPlusZ(0));
        Assert.Equal(10, actor.PreviousX.ToDouble()); Assert.Equal(20, actor.PreviousY.ToDouble());
        Assert.Equal(30, actor.PreviousZ.ToDouble()); Assert.Equal(7, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(56)]
    [InlineData(0)]
    [InlineData(12.5)]
    public void GeometryQueriesUseCurrentHeightAndDoNotMoveActor(double height)
    {
        var actor = Setup(); actor.Height = Fixed.FromDouble(height);
        Assert.Equal(30 + height, actor.Top()); Assert.Equal(height / 2, actor.CenterOffset());
        Assert.Equal(30 + height / 2, actor.Center());
        Assert.Equal((10.0, 20.0, 27.5), actor.PosPlusZ(-2.5));
        Assert.Equal((10.0, 20.0, -10.0), actor.PosAtZ(-10)); Assert.Equal(30, actor.Z.ToDouble());
    }

    private static Actor Setup()
    {
        var actor = new Actor { X = Fixed.FromInt(10), Y = Fixed.FromInt(20), Z = Fixed.FromInt(30) };
        actor.RememberPosition(); return actor;
    }
}
