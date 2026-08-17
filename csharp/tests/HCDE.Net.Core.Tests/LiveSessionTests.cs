using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class LiveSessionTests
{
    [Fact]
    public void AuthorityAndGuestExchangeClientInputWithUserCmd()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        guest.Pump(now);
        authority.PumpClient(now + 1000, guestEndpoint, clientSlot: 1);

        Assert.True(authority.TryReceiveClientInput(guestEndpoint, out var header, out var players));
        Assert.Equal((byte)1, header.PlayerCount);
        Assert.Single(players);
        Assert.Equal((byte)1, players[0].PlayerNum);
    }

    [Fact]
    public void AuthorityPump_ReceivesClientInputAndAdvancesGameTic()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.TrackClient(guestEndpoint, clientSlot: 1);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        guest.Pump(now);
        authority.Pump(now + 1000);

        Assert.Equal(1u, authority.GameTic);
    }

    [Fact]
    public void AuthorityPump_SendsWorldStateTailViaPump()
    {
        var wad = HCDE.MapLoader.Tests.TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        Assert.True(AuthorityMapLoadBootstrap.TryBootstrapAuthorityWorldState(
            authority,
            wad,
            "MAP01",
            out _,
            rngSeed: 9,
            replicateSectorMetadata: true));
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum, rngSeed: 9);

        var now = (ulong)Environment.TickCount64;
        authority.Pump(now);

        Assert.Equal(1u, authority.GameTic);
        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(guestStore.Sectors.TryGetValue(0, out var sector));
        Assert.Equal(160, sector.LightLevel);
        Assert.True(guestStore.Players.TryGetValue(0, out var player));
        Assert.Equal(100, player.Health);
        Assert.True(guestStore.Actors.TryGetValue(1, out var actor));
        Assert.Equal(1, actor.ClassId);
        Assert.True(guestChecksum.Ring.TryFind((int)tailSections!.Value.ChecksumGameTic, out _));
    }

    [Fact]
    public void GuestReceivesServerSnapshotWithMinimalTail()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        authority.PumpClient(now, guestEndpoint, clientSlot: 1);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out var header, out var players, out var tailSections));
        Assert.Equal((byte)1, header.PlayerCount);
        Assert.Single(players);
        Assert.NotNull(tailSections);
        Assert.Equal(1u, tailSections.Value.WorldDelta.GameTic);
        Assert.Null(tailSections.Value.InvasionSnapshot);
        Assert.False(tailSections.Value.HasChecksum);
    }

    [Fact]
    public void AuthorityPumpAllClients_SendsSameGameTicToTrackedGuests()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guest1Transport = new UdpTransport();
        using var guest2Transport = new UdpTransport();
        authorityTransport.Bind(0);
        guest1Transport.Bind(0);
        guest2Transport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guest1Transport.SetNonBlocking(true);
        guest2Transport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guest1Endpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guest1Transport.BoundPort);
        var guest2Endpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guest2Transport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.TrackClient(guest1Endpoint, clientSlot: 1);
        authority.TrackClient(guest2Endpoint, clientSlot: 2);

        var guest1 = new LiveGuestSession(guest1Transport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        var guest2 = new LiveGuestSession(guest2Transport, gameId, authorityEndpoint, guestPlayerSlot: 2, authoritySlot: 0, maxClients: 4);

        var now = (ulong)Environment.TickCount64;
        authority.PumpAllClients(now);

        Assert.Equal(1u, authority.GameTic);
        Assert.True(guest1.TryReceiveAuthorityControl(out _));
        Assert.True(guest2.TryReceiveAuthorityControl(out _));
        Assert.True(guest1.TryReceiveServerSnapshot(out _, out _, out var tail1));
        Assert.True(guest2.TryReceiveServerSnapshot(out _, out _, out var tail2));
        Assert.NotNull(tail1);
        Assert.NotNull(tail2);
        Assert.Equal(1u, tail1.Value.WorldDelta.GameTic);
        Assert.Equal(1u, tail2.Value.WorldDelta.GameTic);
    }

    [Fact]
    public void GuestAppliesQuitterPrefixFromServerSnapshot()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        var now = (ulong)Environment.TickCount64;
        gameplay.TrySendServerSnapshot(
            guestEndpoint,
            roomId: 0,
            gameTic: 1,
            playerNum: 1,
            includeMinimalTail: true,
            quitterPlayerSlots: new byte[] { 2, 3 });

        Assert.True(guest.TryReceiveServerSnapshot(out var header, out _, out _));
        Assert.Equal((byte)NetCommandFlags.Quitters, header.ControlFlags);
        Assert.False(guest.PeerSlots.IsConnected(2));
        Assert.False(guest.PeerSlots.IsConnected(3));
        Assert.True(guest.PeerSlots.IsConnected(1));
    }

    [Fact]
    public void AuthorityPump_SendsCoopDeadSpawnTailViaPump()
    {
        var wad = HCDE.MapLoader.Tests.TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var authorityStore = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", authorityStore, out _));
        authority.SetAuthorityWorldState(
            authorityStore,
            new SnapshotChecksumSession(),
            rngSeed: 9,
            replicateSectorMetadata: true);
        authorityStore.QueueCoopDeadSpawn(77);
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum, rngSeed: 9);

        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.NotNull(tailSections!.Value.CoopDeadSpawnIndices);
        Assert.Equal(new uint[] { 77 }, tailSections.Value.CoopDeadSpawnIndices);
        Assert.Contains(77u, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void AuthorityPump_SendsInvasionTailViaPump()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.SetAuthorityInvasionSnapshot(new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 1,
            wave: 4,
            maxWaves: 12,
            waveBudget: 8,
            waveSpawned: 2,
            waveCleared: 0,
            activeMonsters: 3));
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);

        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.NotNull(tailSections!.Value.InvasionSnapshot);
        Assert.Equal(4u, tailSections.Value.InvasionSnapshot!.Value.Wave);
    }

    [Fact]
    public void GuestAppliesPresentationEchoWhenWorldStateWired()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum);

        Span<byte> tail = stackalloc byte[512];
        var cursor = 0;
        cursor += WorldDeltaChunkCodec.WriteEmpty(tail[cursor..], gameTic: 1);
        cursor += ActorDeltasCodec.WriteEmpty(tail[cursor..]);
        cursor += PresentationEchoCodec.Write(tail[cursor..], PresentationEchoCodec.CreateExampleBlock());

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: 1,
            playerNum: 1,
            externalTail: tail[..cursor]);

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(guest.PresentationEchoState);
        Assert.NotNull(guest.PresentationEchoState!.LastInventoryItems);
        Assert.NotEmpty(guest.PresentationEchoState.LastInventoryItems!);
    }

    [Fact]
    public void AuthorityPump_SendsAuthorityEventTailViaPump()
    {
        var wad = HCDE.MapLoader.Tests.TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        var authorityStore = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", authorityStore, out _));
        authorityStore.QueueAuthorityEvent(AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 55));
        authority.SetAuthorityWorldState(authorityStore, new SnapshotChecksumSession());
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);

        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.NotNull(tailSections!.Value.AuthorityEventRecords);
        Assert.Single(tailSections.Value.AuthorityEventRecords!);
        Assert.Equal(55u, tailSections.Value.AuthorityEventRecords![0].ActorId);
    }

    [Fact]
    public void GuestAppliesInvasionSnapshotWhenWorldStateWired()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.SetGuestWorldState(guestStore, new SnapshotChecksumSession());

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.SetAuthorityInvasionSnapshot(new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateCountdown,
            stateTics: 5,
            wave: 2,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 1,
            waveCleared: 0,
            activeMonsters: 4));
        authority.TrackClient(guestEndpoint, clientSlot: 1);
        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.NotNull(guest.InvasionState);
        Assert.Equal(1, guest.InvasionState!.ApplyMirrorCalls);
        Assert.Equal(2, guest.InvasionState.MirrorState.Wave);
        Assert.Equal(LiveConstants.InvasionStateCountdown, guest.InvasionState.MirrorState.State);
    }

    [Fact]
    public void GuestAppliesInvasionSpawnDirectoryWhenWorldStateWired()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.SetGuestWorldState(guestStore, new SnapshotChecksumSession());

        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.SetAuthorityInvasionSnapshot(new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 2,
            wave: 3,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 5,
            waveCleared: 1,
            activeMonsters: 6,
            spawnSpotCount: 8,
            activeSpawnSpotCount: 2,
            spawnPlanBudget: 12,
            spawnActiveTag: 77,
            spawnFlags: LiveConstants.InvasionSnapshotSpawnFlagUsingFallback,
            spawnFallbackSource: LiveConstants.InvasionSpawnSourceDeathmatch));
        authority.TrackClient(guestEndpoint, clientSlot: 1);
        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.NotNull(guest.InvasionState?.SpawnDirectory);
        Assert.Equal(8, guest.InvasionState!.SpawnDirectory!.Value.TotalSpotCount);
        Assert.Equal(2, guest.InvasionState.SpawnDirectory!.Value.ActiveSpotCount);
        Assert.Equal(77u, guest.InvasionState.SpawnDirectory!.Value.ActiveTag);
        Assert.True(guest.InvasionState.SpawnDirectory!.Value.UsingFallback);
    }

    [Fact]
    public void AuthorityInvasionPump_IncludesChecksumWhenWorldStateWired()
    {
        var wad = HCDE.MapLoader.Tests.TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int rngSeed = 17;

        using var authorityTransport = new UdpTransport();
        using var guestTransport = new UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authorityStore = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", authorityStore, out _));
        var authorityChecksum = new SnapshotChecksumSession();
        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.SetAuthorityWorldState(authorityStore, authorityChecksum, rngSeed);
        authority.SetAuthorityInvasionSnapshot(new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 1,
            wave: 2,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 1,
            waveCleared: 0,
            activeMonsters: 3));
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            authorityEndpoint,
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.SetGuestWorldState(new GuestWorldStateStore(), new SnapshotChecksumSession(), rngSeed);

        authority.Pump((ulong)Environment.TickCount64);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.NotNull(tailSections!.Value.InvasionSnapshot);
        Assert.True(tailSections.Value.HasChecksum);
        Assert.NotNull(tailSections.Value.ChecksumHashes);
        Assert.True(authorityChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out var authorityHashes));
        Assert.Equal(authorityHashes, tailSections.Value.ChecksumHashes);
    }
}
