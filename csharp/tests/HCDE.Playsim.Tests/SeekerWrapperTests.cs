using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SeekerWrapperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerOwnerReplacesMissingOrNonplayerTracer(bool hasTracer)
    {
        var (sim, missile, target) = Room(playerOwner: true);
        missile.TracerId = hasTracer ? target.Id : null;
        ActorSeekerActions.SeekerMissile(missile, 9999, 9999);
        Assert.Equal(sim.Players.Single().Id, missile.TracerId);
        Assert.Equal(0, missile.Angle.ToDegrees(), 5);
    }

    [Fact]
    public void ExistingPlayerTracerStaysSelected()
    {
        var (sim, missile, _) = Room(playerOwner: true, secondPlayer: true); var first = sim.Players.First();
        var other = sim.Players.Last();
        missile.TracerId = other.Id;
        ActorSeekerActions.SeekerMissile(missile, 9999, 9999);
        Assert.Equal(other.Id, missile.TracerId); Assert.NotEqual(first.Id, missile.TracerId);
        Assert.Equal(45, missile.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(-1, 20, 20)]
    [InlineData(10, -1, 0)]
    [InlineData(9999, 9999, 90)]
    public void ArgumentsClampOnlyAtLowerBound(int threshold, int cap, double expected)
    {
        var (_, missile, _) = Room();
        ActorSeekerActions.SeekerMissile(missile, threshold, cap);
        Assert.Equal(expected, missile.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(ActorSeekerFlags.Precise)]
    [InlineData(ActorSeekerFlags.CurrentSpeed)]
    [InlineData(ActorSeekerFlags.Precise | ActorSeekerFlags.CurrentSpeed)]
    public void FlagsDispatchToNativeSteeringModes(ActorSeekerFlags flags)
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(100);
        missile.VelocityX = Fixed.FromInt(3); missile.VelocityY = Fixed.FromInt(4); missile.VelocityZ = Fixed.FromInt(12);
        ActorSeekerActions.SeekerMissile(missile, 0, 20, flags);
        var expected = (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ);
        missile.Angle = default; missile.VelocityX = Fixed.FromInt(3); missile.VelocityY = Fixed.FromInt(4); missile.VelocityZ = Fixed.FromInt(12);
        ActorSeekerActions.Seek(missile, 0, 20, (flags & ActorSeekerFlags.CurrentSpeed) != 0, (flags & ActorSeekerFlags.Precise) != 0);
        Assert.Equal(expected, (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ));
    }

    [Fact]
    public void FailedSeekingWithoutLookPreservesUnseekableTracer()
    {
        var (_, missile, target) = Room(); target.CantSeek = true;
        ActorSeekerActions.SeekerMissile(missile, 10, 20); Assert.Equal(target.Id, missile.TracerId);
        target.CantSeek = false; target.Shootable = false;
        ActorSeekerActions.SeekerMissile(missile, 10, 20); Assert.Null(missile.TracerId);
    }

    [Theory]
    [InlineData(ActorSeekerFlags.Look)]
    [InlineData(ActorSeekerFlags.Look | ActorSeekerFlags.Precise)]
    public void LookKeepsExistingTracerWithoutApplyingStickyPlayerRule(ActorSeekerFlags flags)
    {
        var (sim, missile, target) = Room(playerOwner: true); missile.TracerId = target.Id;
        ActorSeekerActions.SeekerMissile(missile, 10, 20, flags);
        Assert.Equal(target.Id, missile.TracerId); Assert.Equal(20, missile.Angle.ToDegrees(), 5);
        Assert.Null(sim.CaptureState().SeekerMissileRandomState);
    }

    [Fact]
    public void UnknownFlagsFailBeforeMutation()
    {
        var (_, missile, target) = Room(playerOwner: true); missile.TracerId = target.Id;
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorSeekerActions.SeekerMissile(missile, 10, 20, (ActorSeekerFlags)8));
        Assert.Equal(target.Id, missile.TracerId);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room(bool playerOwner = false, bool secondPlayer = false)
    {
        var things = new List<LevelThing> { new() { Type = 1, X = 100 } };
        if (secondPlayer) things.Add(new LevelThing { Type = 2, X = 100, Y = 100 });
        var level = new PlayLevel { Things = things,
            Sectors = [new LevelSector { CeilingHeight = 256 }] };
        var sim = AuthoritySimulation.Start(level);
        Actor owner = playerOwner ? sim.Players.First() : sim.AddBot(-100, 0);
        var target = sim.AddBot(0, 100); var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        missile.SetXYZ(0, 0, 0); missile.Angle = default; missile.TracerId = target.Id;
        return (sim, missile, target);
    }
}
