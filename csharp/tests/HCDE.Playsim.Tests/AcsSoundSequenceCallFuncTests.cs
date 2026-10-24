using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSoundSequenceCallFuncTests
{
    [Fact]
    public void SoundSequenceOnActorSetsActivatorName()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, ["Water"],
            new AcsGlobalStrings(), AcsCallFunctions.SoundSequenceOnActor, 2, out _));
        Assert.Equal("Water", player.AcsSoundSequenceName);
    }

    [Fact]
    public void SoundSequenceOnSectorTagsAllMatchingSectors()
    {
        var sim = TaggedRoom();
        var stack = new List<int> { 7, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), ["Drip"],
            new AcsGlobalStrings(), AcsCallFunctions.SoundSequenceOnSector, 2, out _));
        Assert.Equal("Drip", sim.SectorAcsSoundSequence(0));
        Assert.Equal("Drip", sim.SectorAcsSoundSequence(1));
        Assert.Null(sim.SectorAcsSoundSequence(2));
    }

    [Fact]
    public void SoundSequenceOnPolyobjStoresSequenceName()
    {
        var sim = Room();
        var stack = new List<int> { 4, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), ["PolyLoop"],
            new AcsGlobalStrings(), AcsCallFunctions.SoundSequenceOnPolyobj, 2, out _));
        Assert.Equal("PolyLoop", sim.PolyobjAcsSoundSequence(4));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static AuthoritySimulation TaggedRoom() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors =
        [
            new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 2, Tag = 3, CeilingHeight = 128 },
        ],
        Things = [new LevelThing { Type = 1 }],
    });
}
