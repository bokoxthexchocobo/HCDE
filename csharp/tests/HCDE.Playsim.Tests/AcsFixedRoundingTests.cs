using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsFixedRoundingTests
{
    [Theory]
    [InlineData(0.5, 0)]
    [InlineData(1.5, 2)]
    [InlineData(2.5, 2)]
    [InlineData(-0.5, 0)]
    [InlineData(-1.5, -2)]
    [InlineData(-2.5, -2)]
    [InlineData(0.75, 1)]
    [InlineData(-0.25, 0)]
    public void CameraQueryUsesNativeNearestEvenFixedConversion(double fixedUnits, int expected)
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); actor.CameraHeight = fixedUnits / 65536;
        Assert.Equal(expected, Invoke(sim, actor, AcsCallFunctions.GetActorViewHeight, [0]));
    }

    [Theory]
    [InlineData(2, 2, 3)]
    [InlineData(-2, -2, 3)]
    [InlineData(65536, 65536, 92682)]
    public void VectorLengthRoundsFractionalFixedResults(int x, int y, int expected)
    {
        var sim = Room(); Assert.Equal(expected, Invoke(sim, sim.Players.Single(), AcsCallFunctions.VectorLength, [x, y]));
    }

    private static int Invoke(AuthoritySimulation sim, Actor actor, int function, int[] args)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], new(), function, args.Length, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
