namespace HCDE.Playsim.Tests;

public class MovementOpeningBoundsTests
{
    [Theory]
    [InlineData(0, false, false, 0)]
    [InlineData(24, false, false, 24)]
    [InlineData(48, true, false, 48)]
    [InlineData(0, false, true, 24)]
    public void DestinationBoundsUseAdjacentPlane(short floor, bool floorHugger, bool ceilingHugger, int z)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 80);
        var actor = sim.AddBot(-40, 80);
        actor.AllowDropOff = true;
        actor.Height = Fixed.FromInt(56);
        // Leave enough room for the high floor-hugger case.
        if (floorHugger) actor.Height = Fixed.FromInt(32);
        actor.FloorHugger = floorHugger;
        actor.CeilingHugger = ceilingHugger;
        Assert.True(ActorPhysics.TryMove(sim, actor, -1, 80, out _));
        Assert.Equal(z, actor.Z.ToDouble());
        Assert.Equal(0, actor.SectorIndex);
    }

    [Fact]
    public void OrdinaryMissileRejectsAdjacentRaisedFloor()
    {
        var sim = GameplayFoundationTests.TwoRooms(1, 128);
        var actor = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        actor.X = Fixed.FromInt(-40); actor.Y = Fixed.FromInt(80); actor.Z = default;
        Assert.False(ActorPhysics.TryMove(sim, actor, -1, 80, out _));
        Assert.Equal(-40, actor.X.ToDouble());
        Assert.Equal(0, actor.Z.ToDouble());
    }
}
