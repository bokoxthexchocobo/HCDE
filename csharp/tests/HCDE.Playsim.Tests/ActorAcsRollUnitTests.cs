using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorAcsRollUnitTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static int Call(AuthoritySimulation sim, int function, params int[] args)
    {
        var stack = new List<int> { 987 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], new AcsGlobalStrings(), function, args.Length, out var result));
        Assert.Equal(new[] { 987 }, stack);
        return result;
    }

    [Theory]
    [InlineData(88, 16384, 16384)]
    [InlineData(89, 16384, 16384)]
    [InlineData(88, -16384, 49152)]
    [InlineData(89, -16384, 49152)]
    [InlineData(88, 81920, 16384)]
    [InlineData(89, 81920, 16384)]
    [InlineData(88, 65536, 0)]
    [InlineData(89, 65536, 0)]
    public void RollCallsNormalizeFixedTurnsAcrossAllMatchingActors(int function, int value, int expected)
    {
        var sim = Room(); var first = sim.AddBot(64, 0); var second = sim.AddBot(64, 64);
        first.ThingId = second.ThingId = 7;
        Assert.Equal(0, Call(sim, function, 7, value, 1));
        Assert.Equal(unchecked((uint)expected << 16), first.Roll.Raw);
        Assert.Equal(first.Roll.Raw, second.Roll.Raw);
        Assert.Equal(expected, Call(sim, 90, 7));
        Assert.Equal(0u, sim.Players.Single().Roll.Raw);
    }

    [Fact]
    public void ActivatorSetterAndFractionalGetterUseAcsUnits()
    {
        var sim = Room(); var player = sim.Players.Single();
        Call(sim, 88, 0, 8192);
        Assert.Equal(45, player.Roll.ToDegrees());
        player.Roll = new BamAngle(0x2000ffff);
        Assert.Equal(8192, Call(sim, 90, 0));
        Assert.Equal(0, Call(sim, 90, 999));
    }
}
