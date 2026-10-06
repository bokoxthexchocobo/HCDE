namespace HCDE.Playsim.Tests;

public class TouchedFloorSupportTests
{
    [Theory]
    [InlineData(-24, 0)]
    [InlineData(24, 24)]
    public void TouchedFloorRemainsGroundSupport(short floor, int z)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 128);
        var actor = sim.AddBot(-1, 80);
        actor.AllowDropOff = true;
        actor.Brain = null;
        actor.VelocityX = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(1, actor.SectorIndex);
        Assert.Equal(z, actor.Z.ToDouble());
        Assert.True(actor.OnGround);
        Assert.False(actor.OnMobj);
        Assert.Equal(0, actor.VelocityZ.ToDouble());
    }

    [Fact]
    public void FullBoundsClearanceStartsLedgeGravity()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var actor = sim.AddBot(19, 80);
        actor.AllowDropOff = true;
        actor.Brain = null;
        actor.Z = default;
        actor.SectorIndex = 1;
        actor.VelocityX = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(0, actor.Z.ToDouble());
        Assert.False(actor.OnGround);
        Assert.Equal(-2, actor.VelocityZ.ToDouble());
    }
}
