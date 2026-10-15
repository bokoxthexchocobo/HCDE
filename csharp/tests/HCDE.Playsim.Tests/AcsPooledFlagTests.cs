using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsPooledFlagTests
{
    [Theory]
    [InlineData("NOGRAVITY")]
    [InlineData("NOCLIP")]
    [InlineData("SHOOTABLE")]
    public void PooledSetterAndCheckerRoundTripFlags(string name)
    {
        var sim = Room(); var player = sim.Players.Single(); var pool = new AcsGlobalStrings(); var id = pool.Add(name.ToLowerInvariant());
        foreach (var value in new[] { 1, 0, -1 })
        {
            Assert.Equal(1, Call(sim, player, pool, AcsCallFunctions.SetActorFlag, [0, id, value]));
            Assert.Equal(value != 0 ? 1 : 0, Call(sim, player, pool, AcsCallFunctions.CheckFlag, [0, id]));
        }
    }

    [Fact]
    public void SetterUpdatesAllMatchingTidsAndReportsCount()
    {
        var sim = Room(); var first = sim.AddBot(60, 0, thingId: 9); var second = sim.AddBot(100, 0, thingId: 9);
        var pool = new AcsGlobalStrings(); var id = pool.Add("NOGRAVITY");
        Assert.Equal(2, Call(sim, null, pool, AcsCallFunctions.SetActorFlag, [9, id, 1]));
        Assert.True(first.NoGravity); Assert.True(second.NoGravity); Assert.False(sim.Players.Single().NoGravity);
    }

    [Theory]
    [InlineData("UnknownFlag")]
    [InlineData("")]
    public void UnsupportedPooledFlagsDoNotMutateActors(string name)
    {
        var sim = Room(); var player = sim.Players.Single(); var pool = new AcsGlobalStrings(); var id = pool.Add(name);
        Assert.Equal(0, Call(sim, player, pool, AcsCallFunctions.SetActorFlag, [0, id, 1]));
        Assert.Equal(0, Call(sim, player, pool, AcsCallFunctions.CheckFlag, [0, id])); Assert.False(player.NoGravity);
    }

    [Fact]
    public void PooledNoGravityAffectsMotionAndSurvivesSave()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Z = Fixed.FromInt(32);
        var pool = new AcsGlobalStrings(); var id = pool.Add("NOGRAVITY");
        Call(sim, player, pool, AcsCallFunctions.SetActorFlag, [0, id, 1]);
        var saved = SimSavegame.Write(sim, true); sim.Tick(); Assert.Equal(Fixed.FromInt(32), player.Z);
        Call(sim, player, pool, AcsCallFunctions.SetActorFlag, [0, id, 0]); SimSavegame.Apply(sim, saved);
        Assert.Equal(1, Call(sim, player, pool, AcsCallFunctions.CheckFlag, [0, id]));
    }

    private static int Call(AuthoritySimulation sim, Actor? actor, AcsGlobalStrings strings, int function, int[] args)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], strings, function, args.Length, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
