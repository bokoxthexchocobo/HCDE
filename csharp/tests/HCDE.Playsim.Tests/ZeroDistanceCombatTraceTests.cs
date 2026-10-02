using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ZeroDistanceCombatTraceTests
{
    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(15, 0, 0, true)]
    [InlineData(-15, 0, 0, true)]
    [InlineData(0, 15, 0, true)]
    [InlineData(16, 0, 0, false)]
    [InlineData(0, -16, 0, false)]
    [InlineData(0, 0, 28, true)]
    [InlineData(0, 0, -28, true)]
    [InlineData(0, 0, 29, false)]
    [InlineData(0, 0, -29, false)]
    public void ZeroDistanceSelectsOnlyStrictlyContainingBoxAtOriginHeight(int x, int y, int z, bool expected)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(x, y);
        target.Radius = Fixed.FromInt(16); target.Height = Fixed.FromInt(56); target.Z = Fixed.FromInt(z);
        var random = sim.CombatRandomState; var health = target.Health; var count = sim.Actors.Count;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, BamAngle.FromDegrees(-45), 0);
        Assert.Equal(expected, hit.Hit); Assert.Equal(expected ? target : null, hit.Victim);
        if (expected) { Assert.Equal(0, hit.X); Assert.Equal(0, hit.Y); Assert.Equal(28, hit.Z); }
        Assert.Equal(expected ? target : null, CombatTrace.PickActor(sim, source, source.Angle, BamAngle.FromDegrees(-45), 0));
        Assert.Equal(random, sim.CombatRandomState); Assert.Equal(health, target.Health); Assert.Equal(count, sim.Actors.Count);
        var stack = new List<int> { 987, 0, 0, -8192, 0, 42 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.PickActor, 5, out var result));
        Assert.Equal(expected ? 1 : 0, result); Assert.Equal(expected ? 42 : 0, target.ThingId);
        Assert.Equal(new[] { 987 }, stack);
        Assert.Equal(expected ? 1 : 0, AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 0));
        Assert.Equal(expected ? health - 7 : health, target.Health);
    }

    [Fact]
    public void ZeroDistanceDoesNotHitCoincidentLineOrPlane()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 0, X2 = 0, Y1 = 128, Y2 = -128,
                SideFront = 0, SideBack = -1, Health = 100 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(-28);
        Assert.False(CombatTrace.TraceLineAttack(sim, source, source.Angle, BamAngle.FromDegrees(45), 0).Hit);
        Assert.Equal(0, AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 0));
        Assert.Equal(100, sim.Level.Lines[0].Health); Assert.Equal(100, sim.Level.Sectors[0].HealthFloor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
