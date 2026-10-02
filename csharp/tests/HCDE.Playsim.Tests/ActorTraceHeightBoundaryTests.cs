using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTraceHeightBoundaryTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void HorizontalRayIncludesExactActorTop(int heightDelta, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(100, 0);
        target.Height = new Fixed(28 * 65536 + heightDelta);
        var hit = CombatTrace.TraceLineAttack(sim, sim.Players.Single(), new BamAngle(0), new BamAngle(0), 256);
        Assert.Equal(expected, ReferenceEquals(target, hit.Victim));
        if (expected) Assert.Equal(28, hit.Z);
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void HorizontalRayIncludesExactActorBottom(int zDelta, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(100, 0);
        target.Z = new Fixed(28 * 65536 + zDelta);
        Assert.Equal(expected, ReferenceEquals(target, CombatTrace.TraceLineAttack(sim, sim.Players.Single(),
            new BamAngle(0), new BamAngle(0), 256).Victim));
    }

    [Theory]
    [InlineData(19.9999847412109375, false)]
    [InlineData(20, true)]
    [InlineData(20.0000152587890625, true)]
    public void DescendingRayIncludesActorTopAtExactRange(double range, bool expected)
    {
        var sim = Room(); var source = sim.Players.Single(); source.Z = Fixed.FromInt(128);
        var target = sim.AddBot(0, 0); target.Z = Fixed.FromInt(80); target.Height = Fixed.FromInt(56);
        var hit = CombatTrace.TraceLineAttack(sim, source, new BamAngle(0), BamAngle.FromDegrees(90), range);
        Assert.Equal(expected, ReferenceEquals(target, hit.Victim));
        if (expected) Assert.Equal(136, hit.Z);
    }

    [Fact]
    public void PickActorIncludesTopSurfaceWithoutChangingHealth()
    {
        var sim = Room(); var target = sim.AddBot(100, 0); target.Height = Fixed.FromInt(28);
        var health = target.Health;
        Assert.Same(target, CombatTrace.PickActor(sim, sim.Players.Single(), new BamAngle(0), new BamAngle(0), 256));
        Assert.Equal(health, target.Health);
    }
}
