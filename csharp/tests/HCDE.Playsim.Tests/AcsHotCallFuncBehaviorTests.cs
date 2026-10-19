using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsHotCallFuncBehaviorTests
{
    [Fact]
    public void SpawnForcedCreatesActorAtFixedCoordinates()
    {
        var sim = Room();
        var stack = new List<int> { 0, 0, 64 * 65536, 0, 7, 0 };
        var strings = new[] { "DoomImp" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), strings, new(), AcsCallFunctionWireIds.NameToWire["SpawnForced"], 6, out var spawned));
        Assert.Equal(1, spawned);
        Assert.Single(sim.Actors, actor => actor.ThingId == 7);
    }

    [Fact]
    public void UserVariableRoundTripOnActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var setStack = new List<int> { 0, 0, 42 };
        var strings = new[] { "ammo" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, setStack, new() { Value = player }, strings, new(),
            AcsCallFunctions.SetUserVariable, 3, out _));
        var getStack = new List<int> { 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, getStack, new() { Value = player }, strings, new(),
            AcsCallFunctions.GetUserVariable, 2, out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetThingUdmfReadsTidAndSpecialArgs()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.ThingId = 9;
        imp.Special = 12;
        imp.SpecialArgs[1] = 5;
        var stack = new List<int> { 9, 0 };
        var strings = new[] { "arg1" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), strings, new(), AcsCallFunctions.GetThingUdmfInt, 2, out var value));
        Assert.Equal(5, value);
    }

    [Fact]
    public void AirSupplyRoundTripForPlayerZero()
    {
        var sim = Room();
        var setStack = new List<int> { 0, 140 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, setStack, new(), [], new(), AcsCallFunctions.SetAirSupply, 2, out var setResult));
        Assert.Equal(1, setResult);
        var getStack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, getStack, new(), [], new(), AcsCallFunctions.GetAirSupply, 1, out var remaining));
        Assert.Equal(140, remaining);
    }

    [Fact]
    public void SpawnSpotForcedAtActivatorPosition()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0, 9, 0 };
        var strings = new[] { "DoomImp" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, strings, new(),
            AcsCallFunctions.SpawnSpotForced, 4, out var spawned));
        Assert.Equal(1, spawned);
        Assert.Contains(sim.Actors, a => a.ThingId == 9 && a.DoomEdNum == 3001);
    }

    [Fact]
    public void UserArrayIndexRoundTrip()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var setStack = new List<int> { 0, 0, 2, 99 };
        var strings = new[] { "wave" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, setStack, new() { Value = imp }, strings, new(),
            AcsCallFunctions.SetUserArray, 4, out var updated));
        Assert.Equal(1, updated);
        var getStack = new List<int> { 0, 0, 2 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, getStack, new() { Value = imp }, strings, new(),
            AcsCallFunctions.GetUserArray, 3, out var value));
        Assert.Equal(99, value);
    }

    [Fact]
    public void WireDispatchRoutesSpawnForcedSlot()
    {
        var sim = Room();
        var stack = new List<int> { 0, 0, 0, 0, 3, 0 };
        var strings = new[] { "DoomImp" };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), strings, new(), 36, 6, out var result));
        Assert.Equal(1, result);
    }

    private static AuthoritySimulation Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = 0, Y = 0 }, new LevelThing { Type = 3001, X = 64, Y = 0 }],
        });
    }
}
