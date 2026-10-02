using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackAcsAngleTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static void Attack(AuthoritySimulation sim, int tid, int yaw, int pitch, int? range = null)
    {
        var stack = new List<int> { 987, tid, yaw, pitch, 5 };
        if (range.HasValue) stack.AddRange(new[] { 0, 0, range.Value });
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, range.HasValue ? 7 : 4, out _));
        Assert.Equal(new[] { 987 }, stack);
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(16384, 0, 100)]
    [InlineData(-16384, 0, -100)]
    [InlineData(65536, 100, 0)]
    public void FixedTurnYawIsAbsoluteAndWraps(int yaw, int x, int y)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Angle = BamAngle.FromDegrees(135);
        var target = sim.AddBot(x, y); var health = target.Health;
        Attack(sim, 0, yaw, 0);
        Assert.Equal(health - 5, target.Health);
    }

    [Fact]
    public void VerticalPitchIsAbsoluteAndUsesFixedTurns()
    {
        var sim = Room(); var player = sim.Players.Single(); player.PitchDegrees = 45;
        var target = sim.AddBot(0, 0); target.Z = Fixed.FromInt(80); var health = target.Health;
        Attack(sim, 0, 0, -16384);
        Assert.Equal(health - 5, target.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OmittedOrZeroRangeUsesNativeMissileRange(bool explicitZero)
    {
        var sim = Room(); var target = sim.AddBot(1000, 0); var health = target.Health;
        Attack(sim, 0, 0, 0, explicitZero ? 0 : null);
        Assert.Equal(health - 5, target.Health);
    }

    [Fact]
    public void EveryMatchingSourceTidFires()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 100, Y1 = 200, X2 = 100, Y2 = -200,
                SideFront = 0, SideBack = -1, Health = 1000 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var first = sim.Players.Single(); first.ThingId = 7;
        var second = sim.AddBot(0, 100); second.ThingId = 7;
        Attack(sim, 7, 0, 0);
        Assert.Equal(990, sim.Level.Lines[0].Health);
    }
}
