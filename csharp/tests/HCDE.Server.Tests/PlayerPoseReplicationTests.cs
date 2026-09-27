using HCDE.MapLoader;
using HCDE.Net.Core;
using HCDE.Playsim;

namespace HCDE.Server.Tests;

public class PlayerPoseReplicationTests
{
    [Fact]
    public void AnimatedLightPublishesEachAuthoritativeStep()
    {
        var simulation = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        Assert.True(LineSpecials.ActivateMapLine(simulation, simulation.Players.Single(),
            new LevelLine { Special = 113, Arg0 = 7, Arg1 = 328, Arg2 = 2, PlayerUse = true }, true));
        var store = new GuestWorldStateStore();
        foreach (var expected in new[] { 128, 228, 328 })
        {
            simulation.Tick(); SimSnapshotPublisher.Publish(simulation, store);
            var buffer = new byte[4096];
            var written = WorldStateTailBuilder.WriteCoopTailFromStore(buffer, store, 1, replicateSectorMetadata: true);
            Assert.True(ServerSnapshotTailWalker.TryWalk(buffer.AsSpan(0, written), out var sections, out _, out var error), error);
            Assert.Equal(expected, Assert.Single(sections.WorldDeltaSectors!).LightLevel);
        }
    }

    [Theory]
    [InlineData(110, 328)]
    [InlineData(111, -72)]
    public void ChangedSectorLightReachesSnapshotWithoutClampingToByte(int special, int expected)
    {
        var simulation = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        Assert.True(LineSpecials.ActivateMapLine(simulation, simulation.Players.Single(),
            new LevelLine { Special = special, Arg0 = 7, Arg1 = 200, PlayerUse = true }, true));
        var store = new GuestWorldStateStore(); SimSnapshotPublisher.Publish(simulation, store);
        var buffer = new byte[4096];
        var written = WorldStateTailBuilder.WriteCoopTailFromStore(buffer, store, 1, replicateSectorMetadata: true);
        Assert.True(ServerSnapshotTailWalker.TryWalk(buffer.AsSpan(0, written), out var sections, out _, out var error), error);
        Assert.Equal(expected, Assert.Single(sections.WorldDeltaSectors!).LightLevel);
    }

    [Fact]
    public void FractionalSectorHeightsCarryPlayerAndReachSnapshot()
    {
        var simulation = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { FloorHeight = 0.125, CeilingHeight = 128.875 }],
            Things = [new LevelThing { Type = 1 }],
        });
        simulation.Tick();
        Assert.Equal(0.125, simulation.Players.Single().Z.ToDouble());
        var store = new GuestWorldStateStore(); SimSnapshotPublisher.Publish(simulation, store);
        var buffer = new byte[4096];
        var written = WorldStateTailBuilder.WriteCoopTailFromStore(buffer, store, 1);
        Assert.True(ServerSnapshotTailWalker.TryWalk(buffer.AsSpan(0, written), out var sections, out _, out var error), error);
        var sector = Assert.Single(sections.WorldDeltaSectors!);
        Assert.Equal(0.125, sector.Floor); Assert.Equal(128.875, sector.Ceiling);
    }

    [Theory]
    [InlineData(-45, false)]
    [InlineData(45, false)]
    [InlineData(-89, true)]
    [InlineData(89, true)]
    public void ArmorAndSignedPitchReachGuest(double pitch, bool invasion)
    {
        var simulation = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = simulation.Players.Single();
        player.PitchDegrees = pitch;
        player.Inventory.Armor = 137;
        var authority = new GuestWorldStateStore();
        SimSnapshotPublisher.Publish(simulation, authority);
        var buffer = new byte[4096];
        var written = invasion
            ? WorldStateTailBuilder.TryBuildMergedInvasionCoopTail(buffer, authority, null, 1,
                new InvasionSnapshotHeader(0, LiveConstants.InvasionStateSpawning, 1, 1, 10, 8, 1, 0, 1)).BytesWritten
            : WorldStateTailBuilder.WriteCoopTailFromStore(buffer, authority, 1);
        Assert.True(written > 0);
        Assert.True(ServerSnapshotTailWalker.TryWalk(buffer.AsSpan(0, written), out var sections, out _, out var error), error);
        var pose = Assert.Single(sections.WorldDeltaPoses!);
        var guest = new GuestWorldStateStore();
        Assert.True(guest.ApplyPose(0, pose, 0));
        Assert.Equal(137, guest.Players[0].Armor);
        Assert.Equal(pitch, unchecked((int)guest.Players[0].PitchBams) * (360.0 / 4294967296), 5);
    }

    [Fact]
    public void LargeModHealthSaturatesInsteadOfBecomingNegativeOnWire()
    {
        var simulation = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = simulation.Players.Single();
        player.Health = 100000;
        player.Inventory.Armor = 100000;
        var store = new GuestWorldStateStore();
        SimSnapshotPublisher.Publish(simulation, store);
        Assert.Equal(short.MaxValue, store.Players[0].Health);
        Assert.Equal(short.MaxValue, store.Players[0].Armor);
        Assert.Equal(short.MaxValue, store.Actors[player.Id].Health);
        Assert.Equal(100000, player.Health);
    }
}
