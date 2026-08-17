using HCDE.MapLoader;

namespace HCDE.Net.Core.Tests;

public class GuestWorldStateBootstrapTests
{
    [Fact]
    public void SeedFromMapSectors_PopulatesGuestSectorState()
    {
        var sectors = new[]
        {
            new MapSectorRecord(0, 128, "FLOOR1_1", "CEIL1_1", 160, 0, 1),
            new MapSectorRecord(-8, 192, "FLOOR1_2", "CEIL1_2", 144, 3, 2),
        };

        var store = new GuestWorldStateStore();
        Assert.Equal(2, GuestWorldStateBootstrap.SeedFromMapSectors(store, sectors));

        Assert.True(store.Sectors.TryGetValue(0, out var first));
        Assert.Equal(0, first.Floor);
        Assert.Equal(128, first.Ceiling);
        Assert.Equal(160, first.LightLevel);
        Assert.Equal(0, first.Special);

        Assert.True(store.Sectors.TryGetValue(1, out var second));
        Assert.Equal(-8, second.Floor);
        Assert.Equal(192, second.Ceiling);
        Assert.Equal(144, second.LightLevel);
        Assert.Equal(3, second.Special);
    }

    [Fact]
    public void SeedFromMapSectors_EnablesWorldStateTailBuilder()
    {
        var sectors = new[]
        {
            new MapSectorRecord(0, 128, "FLOOR1_1", "CEIL1_1", 160, 0, 1),
        };

        var store = new GuestWorldStateStore();
        GuestWorldStateBootstrap.SeedFromMapSectors(store, sectors);

        Assert.True(WorldStateTailBuilder.HasWorldDeltaPayload(store));
        Span<byte> tail = stackalloc byte[256];
        var written = WorldStateTailBuilder.WriteCoopTailFromStore(tail, store, gameTic: 5);
        Assert.True(written > 0);
    }

    [Fact]
    public void SeedPlayersFromMapThings_SeedsDoomPlayerStarts()
    {
        var things = new[]
        {
            new MapThingRecord(100, 200, 90, type: 1, options: 7),
            new MapThingRecord(0, 0, 0, type: 3004, options: 0),
            new MapThingRecord(50, 50, 0, type: 2, options: 0),
        };

        var store = new GuestWorldStateStore();
        Assert.Equal(2, GuestWorldStateBootstrap.SeedPlayersFromMapThings(store, things));
        Assert.True(store.Players.TryGetValue(0, out var player0));
        Assert.True(store.Players.TryGetValue(1, out var player1));
        Assert.Equal(100, player0.Health);
        Assert.Equal(100, player1.Health);
    }

    [Fact]
    public void SeedPlayersFromMapThings_SeedsActorDeltasForPlayerStarts()
    {
        var things = new[] { new MapThingRecord(100, 200, 90, type: 1, options: 7) };
        var store = new GuestWorldStateStore();
        GuestWorldStateBootstrap.SeedPlayersFromMapThings(store, things);
        Assert.True(store.Actors.TryGetValue(1, out var actor));
        Assert.Equal(1, actor.ClassId);
        Assert.Equal(100, actor.Health);
    }
}
