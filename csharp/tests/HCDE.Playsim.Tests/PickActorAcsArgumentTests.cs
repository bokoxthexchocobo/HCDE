using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickActorAcsArgumentTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static int Pick(AuthoritySimulation sim, int angle, int pitch, int assign, int? mask = null, int flags = 0)
    {
        var stack = new List<int> { 987, 0, angle, pitch, 200 * 65536, assign };
        if (mask.HasValue) stack.AddRange(new[] { mask.Value, 0, flags });
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], new AcsGlobalStrings(), AcsCallFunctions.PickActor, mask.HasValue ? 8 : 5, out var result));
        Assert.Equal(new[] { 987 }, stack);
        return result;
    }

    [Theory]
    [InlineData(0, 64, 0)]
    [InlineData(16384, 0, 64)]
    [InlineData(-16384, 0, -64)]
    [InlineData(65536, 64, 0)]
    public void AcsYawUsesFixedTurnsAndWraps(int angle, int x, int y)
    {
        var sim = Room(); var target = sim.AddBot(x, y);
        Assert.Equal(1, Pick(sim, angle, 0, 42));
        Assert.Equal(42, target.ThingId);
    }

    [Fact]
    public void AcsPitchUsesFixedTurns()
    {
        var sim = Room(); var target = sim.AddBot(0, 0); target.Z = Fixed.FromInt(80);
        Assert.Equal(1, Pick(sim, 0, -16384, 42));
        Assert.Equal(42, target.ThingId);
    }

    [Fact]
    public void ExplicitZeroMaskDoesNotUseOmittedDefault()
    {
        var sim = Room(); var target = sim.AddBot(64, 0);
        Assert.Equal(0, Pick(sim, 0, 0, 42, 0));
        Assert.Equal(0, target.ThingId);
        Assert.Equal(1, Pick(sim, 0, 0, 42));
    }

    [Theory]
    [InlineData(7, 42, 0, 1, 7)]
    [InlineData(7, 42, 2, 7, 7)]
    [InlineData(7, 42, 3, 42, 42)]
    [InlineData(7, 0, 1, 1, 0)]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0, 0, 1, 1, 0)]
    public void ForceAndReturnFlagsFollowNativeTidRules(int existing, int assign, int flags, int result, int expectedTid)
    {
        var sim = Room(); var target = sim.AddBot(64, 0); target.ThingId = existing;
        Assert.Equal(result, Pick(sim, 0, 0, assign, 4, flags));
        Assert.Equal(expectedTid, target.ThingId);
    }
}
