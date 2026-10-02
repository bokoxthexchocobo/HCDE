using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorBoxTraceTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(45)]
    [InlineData(135)]
    [InlineData(225)]
    [InlineData(315)]
    public void DiagonalRayEntersSquareBeforeCircularSurface(double yaw)
    {
        var sim = Room(); var source = sim.Players.Single();
        var radians = yaw * Math.PI / 180;
        var target = sim.AddBot(100 * Math.Sign(Math.Cos(radians)), 100 * Math.Sign(Math.Sin(radians)));
        target.Radius = Fixed.FromInt(20);
        var hit = CombatTrace.TraceLineAttack(sim, source, BamAngle.FromDegrees(yaw), new BamAngle(0), 115);
        Assert.Same(target, hit.Victim);
        Assert.Equal(80, Math.Abs(hit.X), 5); Assert.Equal(80, Math.Abs(hit.Y), 5);
    }

    [Theory]
    [InlineData(110, false)]
    [InlineData(115, true)]
    public void DiagonalBoxEntryRespectsThreeDimensionalRange(double range, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(100, 100); target.Radius = Fixed.FromInt(20);
        var hit = CombatTrace.TraceLineAttack(sim, sim.Players.Single(), BamAngle.FromDegrees(45), new BamAngle(0), range);
        Assert.Equal(expected, ReferenceEquals(target, hit.Victim));
    }

    [Fact]
    public void VerticalRayInsideSquareCornerHitsTopOrBottom()
    {
        var sim = Room(); var target = sim.AddBot(15, 15);
        target.Radius = Fixed.FromInt(20); target.Z = Fixed.FromInt(80);
        var hit = CombatTrace.TraceLineAttack(sim, sim.Players.Single(), new BamAngle(0), BamAngle.FromDegrees(-90), 100);
        Assert.Same(target, hit.Victim); Assert.Equal(80, hit.Z);
    }

    [Fact]
    public void RayOutsideBoxDoesNotHit()
    {
        var sim = Room(); var target = sim.AddBot(100, 21); target.Radius = Fixed.FromInt(20);
        Assert.Null(CombatTrace.TraceLineAttack(sim, sim.Players.Single(), new BamAngle(0), new BamAngle(0), 256).Victim);
    }

    [Fact]
    public void PickActorSharesBoxIntersection()
    {
        var sim = Room(); var target = sim.AddBot(100, 100); target.Radius = Fixed.FromInt(20);
        Assert.Same(target, CombatTrace.PickActor(sim, sim.Players.Single(), BamAngle.FromDegrees(45), new BamAngle(0), 115));
    }
}
