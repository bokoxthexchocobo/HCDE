namespace HCDE.Playsim.Tests;

public class DestinationDropOffRangeTests
{
    [Theory]
    [InlineData(19, false, false)]
    [InlineData(20, false, true)]
    [InlineData(21, false, true)]
    [InlineData(40, false, true)]
    [InlineData(40, true, false)]
    public void OnlyDestinationTouchedFloorsAndActorSupportLimitDrop(int destinationX, bool onMobj, bool passes)
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 256);
        var actor = sim.AddBot(-40, 80);
        actor.Radius = Fixed.FromInt(20);
        actor.AllowDropOff = false;
        actor.Floating = false;
        actor.NoDropOff = true;
        actor.OnMobj = onMobj;
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, destinationX, 80, out _));
        Assert.Equal(passes ? destinationX : -40, actor.X.ToDouble());
        Assert.Equal(0, actor.Z.ToDouble());
        Assert.Equal(passes ? 1 : 0, actor.SectorIndex);
    }

    [Fact]
    public void ActorSupportAtExactDropLimitPasses()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 256);
        var actor = sim.AddBot(-40, 80);
        actor.AllowDropOff = false;
        actor.NoDropOff = true;
        actor.OnMobj = true;
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 80, out _));
    }
}
