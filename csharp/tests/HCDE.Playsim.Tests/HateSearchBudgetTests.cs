using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HateSearchBudgetTests
{
    [Theory]
    [InlineData(0, 18, 32, 33)]
    [InlineData(1, 23, 34, 30)]
    [InlineData(12345, 23, 23, 15)]
    public void NativeNamedStreamBudgetVectors(int seed, int a, int b, int c)
    {
        var sim = Room(seed); Assert.Equal(new[] { a, b, c }, Enumerable.Range(0, 3).Select(_ => sim.NextHateSearchBudget()));
    }

    [Fact]
    public void GlobalSearchStopsAtBudgetAndResumesAfterCursor()
    {
        var sim = DenseRoom(); var hunter = Hunter(sim); var hatees = sim.Actors.Where(a => a.ThingId == 7).ToArray();
        Search(sim); Assert.Null(hunter.TargetId); Assert.Equal(hatees[17].Id, hunter.LastLookActorId);
        Search(sim); Assert.Null(hunter.TargetId); Assert.Equal(hatees[49].Id, hunter.LastLookActorId);
    }

    [Fact]
    public void SaveRestoresNextBudgetAndCursor()
    {
        var sim = DenseRoom(); Search(sim); var saved = SimSavegame.Write(sim, true);
        Search(sim); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Assert.Equal(saved, SimSavegame.Write(sim, true));
        Search(sim); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void OlderSnapshotResetsCursorAndRandomUsage()
    {
        var sim = DenseRoom(); var saved = SimSavegame.Write(sim, true); Search(sim);
        Assert.NotNull(Hunter(sim).LastLookActorId); SimSavegame.Apply(sim, saved);
        Assert.Null(Hunter(sim).LastLookActorId); Assert.Null(sim.CaptureState().HateSearchRandomState);
    }

    [Fact]
    public void LocalDiscoverySetsCursorWithoutConsumingGlobalRandomness()
    {
        var sim = Room(); sim.Level.Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []);
        var hunter = sim.AddBot(40, 40); hunter.TidToHate = 7;
        var hatee = sim.AddBot(80, 40, 3001); hatee.ThingId = 7;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(hatee.Id, hunter.LastLookActorId);
        Assert.Null(sim.CaptureState().HateSearchRandomState);
    }

    [Fact]
    public void ChangedTidInvalidatesSearchCursor()
    {
        var sim = DenseRoom(); Search(sim);
        foreach (var actor in sim.Actors.Where(a => a.ThingId == 7)) actor.ThingId = 9;
        Search(sim); Assert.Null(Hunter(sim).LastLookActorId);
    }

    [Fact]
    public void RecursiveArchiveHeaderIsRejected()
    {
        var sim = DenseRoom(); Search(sim); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 232);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-hatesearch-header", error);
    }

    [Fact]
    public void FriendshipCopyIncludesSearchCursor()
    {
        var sim = DenseRoom(); Search(sim); var source = Hunter(sim); var actor = sim.AddBot(400, 0);
        ActorPropertyActions.CopySupportedFriendship(actor, source); Assert.Equal(source.LastLookActorId, actor.LastLookActorId);
    }

    private static void Search(AuthoritySimulation sim) => Hunter(sim).Brain!.Tick(sim, Hunter(sim), MonsterChaseFlags.DontLookAllAround);
    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3004);
    private static AuthoritySimulation DenseRoom()
    {
        var sim = Room(); var hunter = sim.AddBot(0, 0); hunter.TidToHate = 7;
        for (var i = 0; i < 60; i++) sim.AddBot(-500 - i * 100, 0, 3001).ThingId = 7;
        return sim;
    }
    private static AuthoritySimulation Room(int seed = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 800 }]
    }, rngSeed: seed);
}
