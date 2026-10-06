namespace HCDE.Playsim.Tests;

public class FlyingCeilingCollisionTests
{
    [Theory]
    [InlineData(true, true, 100, false)]
    [InlineData(true, false, 100, true)]
    [InlineData(false, true, 100, true)]
    [InlineData(true, true, 40, true)]
    public void CeilingHuggerMovementChecksOriginalTopDuringActiveFlight(bool fly, bool noGravity, int z, bool passes)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 96);
        var actor = Actor(fly, noGravity, z); actor.X = Fixed.FromInt(-40); actor.SectorIndex = 0;
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, 40, 80, out _));
        Assert.Equal(passes ? 40 : -40, actor.X.ToDouble());
        Assert.Equal(passes ? 40 : z, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(true, true, 100, false)]
    [InlineData(true, false, 100, true)]
    [InlineData(false, true, 100, true)]
    [InlineData(true, true, 40, true)]
    public void MovementProbeUsesTheSameFlightCeilingGate(bool fly, bool noGravity, int z, bool passes)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 96);
        var actor = Actor(fly, noGravity, z);
        Assert.Equal(passes, ActorPhysics.FlatMoveProbeFits(sim, actor));
        Assert.Equal(z, actor.Z.ToDouble());
    }

    private static Actor Actor(bool fly, bool noGravity, int z) => new()
    {
        X = Fixed.FromInt(40), Y = Fixed.FromInt(80), Z = Fixed.FromInt(z), SectorIndex = 1,
        Fly = fly, NoGravity = noGravity, CeilingHugger = true, AllowDropOff = true, Height = Fixed.FromInt(56)
    };
}
