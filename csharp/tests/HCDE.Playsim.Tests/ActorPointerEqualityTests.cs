using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class ActorPointerEqualityTests
{
    [Theory]
    [InlineData(0u)]
    [InlineData(42u)]
    public void SeparateActorsWithSameIdAreNotSamePointer(uint id)
    {
        var first = new Actor { Id = id }; var second = new Actor { Id = id };
        Assert.False(AcsActorPointer.PointersEqual(first, second)); Assert.True(AcsActorPointer.PointersEqual(first, first));
    }

    [Fact]
    public void NullEqualityMatchesNativePointers()
    {
        var actor = new Actor(); Assert.True(AcsActorPointer.PointersEqual(null, null));
        Assert.False(AcsActorPointer.PointersEqual(actor, null)); Assert.False(AcsActorPointer.PointersEqual(null, actor));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 1)]
    public void AcsOptionalTidDispatchMatchesNativeArgumentCount(int count, int expected)
    {
        var sim = Room(); var actor = sim.AddBot(80, 0, thingId: 9);
        int[] all = [AcsActorPointer.Default, AcsActorPointer.Default, 9, 0, 123];
        Assert.Equal(expected, Check(sim, sim.Players.Single(), all[..count]));
    }

    [Fact]
    public void AcsNullPointersCompareEqualWhenOriginsDiffer()
    {
        var sim = Room(); sim.AddBot(80, 0, thingId: 9);
        Assert.Equal(1, Check(sim, sim.Players.Single(), [AcsActorPointer.Null, AcsActorPointer.Null, 9, 0]));
    }

    [Fact]
    public void AcsDefaultPointersDoNotAliasUnassignedActorIds()
    {
        var sim = Room(); var origin = new Actor();
        // Static player resolution must compare object identity even when an external origin reuses its id.
        var player = sim.Players.Single(); origin = new Actor { Id = player.Id };
        Assert.Equal(0, Check(sim, origin, [AcsActorPointer.Default, AcsActorPointer.Player1]));
    }

    private static int Check(AuthoritySimulation sim, Actor actor, int[] args)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], new(),
            AcsCallFunctions.IsPointerEqual, args.Length, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
