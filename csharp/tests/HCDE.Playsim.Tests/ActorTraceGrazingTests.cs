using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTraceGrazingTests
{
    [Theory]
    [InlineData(-20, false)]
    [InlineData(20, true)]
    [InlineData(-19, true)]
    [InlineData(19, true)]
    [InlineData(-21, false)]
    [InlineData(21, false)]
    public void HorizontalGrazingUsesNativeEndpointSides(int y, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(100, y); target.Radius = Fixed.FromInt(20);
        Assert.Equal(expected, ReferenceEquals(target, CombatTrace.TraceLineAttack(sim, sim.Players.Single(),
            new BamAngle(0), new BamAngle(0), 256).Victim));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(19, true)]
    [InlineData(20, false)]
    [InlineData(21, false)]
    public void VerticalRayEntryUsesStrictInsideBoxRule(int x, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(x, 0); target.Radius = Fixed.FromInt(20); target.Z = Fixed.FromInt(80);
        Assert.Equal(expected, ReferenceEquals(target, CombatTrace.PickActor(sim, sim.Players.Single(),
            new BamAngle(0), BamAngle.FromDegrees(-90), 100)));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    });
}
