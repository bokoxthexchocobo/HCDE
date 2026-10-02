using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SignedLineAttackRangeTests
{
    [Theory]
    [InlineData(false, 80, true)]
    [InlineData(false, 84, false)]
    [InlineData(true, 80, true)]
    [InlineData(true, 84, false)]
    public void NegativeRangeSelectsBackwardActorAtInclusiveBoxEntry(bool reverse, int distance, bool expected)
    {
        var sign = reverse ? 1 : -1;
        var sim = Room(reverse); var source = sim.Players.Single();
        var target = sim.AddBot(sign * distance, 0); target.Radius = Fixed.FromInt(16);
        var health = target.Health;
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, BamAngle.FromDegrees(0), -64);
        Assert.Equal(expected, hit.Hit);
        Assert.Equal(expected ? target : null, hit.Victim);
        if (expected) { Assert.Equal(sign * 64, hit.X, 5); Assert.Equal(28, hit.Z, 5); }
        Assert.Equal(expected ? target : null, CombatTrace.PickActor(sim, source, source.Angle, BamAngle.FromDegrees(0), -64));
        Invoke(sim, source, reverse ? 32768 : 0, 0, -64);
        Assert.Equal(expected ? health - 7 : health, target.Health);
    }

    [Theory]
    [InlineData(false, -8192)]
    [InlineData(false, 8192)]
    [InlineData(true, -8192)]
    [InlineData(true, 8192)]
    public void NegativeRangeHitsBackwardWallWithoutFlatPlaneFallback(bool reverse, int pitch)
    {
        var sign = reverse ? 1 : -1;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 64, HealthFloor = 100, HealthCeiling = 100 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = sign * 60, X2 = sign * 60, Y1 = sign * 128,
                Y2 = -sign * 128, SideFront = 0, SideBack = -1, Health = 100 }],
            Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
        });
        var source = sim.Players.Single();
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(unchecked((uint)(pitch << 16))), -128);
        Assert.Same(sim.Level.Lines[0], hit.Wall); Assert.Equal(-1, hit.PlaneSector);
        Assert.Equal(sign * 60, hit.X, 5); Assert.Equal(pitch < 0 ? -32 : 88, hit.Z, 5);
        Invoke(sim, source, reverse ? 32768 : 0, pitch, -128);
        Assert.Equal(93, sim.Level.Lines[0].Health);
        Assert.Equal(100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(100, sim.Level.Sectors[0].HealthCeiling);
    }

    [Theory]
    [InlineData(-45, -200)]
    [InlineData(-45, 200)]
    [InlineData(45, -200)]
    [InlineData(45, 200)]
    public void NegativeRangeRetainsOriginalPitchForHeightAdjustment(double pitch, int targetZ)
    {
        var sim = Room(false); var source = sim.Players.Single(); var target = sim.AddBot(-64, 0);
        target.Z = Fixed.FromInt(targetZ); target.Radius = Fixed.FromInt(256);
        Assert.False(CombatTrace.TraceLineAttack(sim, source, source.Angle, BamAngle.FromDegrees(pitch), -128).Hit);
        Assert.Null(CombatTrace.PickActor(sim, source, source.Angle, BamAngle.FromDegrees(pitch), -128));
    }

    private static AuthoritySimulation Room(bool reverse) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, Angle = reverse ? 180 : 0 }],
    });

    private static void Invoke(AuthoritySimulation sim, Actor source, int yaw, int pitch, int range)
    {
        var stack = new List<int> { 987, 0, yaw, pitch, 7, 0, 0, range << 16 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 7, out var result));
        Assert.Equal(0, result); Assert.Equal(new[] { 987 }, stack);
    }
}
