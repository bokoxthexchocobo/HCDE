using HCDE.MapLoader;
using HCDE.MapLoader.Tests;

namespace HCDE.Net.Core.Tests;

public class MapLoadBootstrapIntegrationTests
{
    [Fact]
    public void TrySeedGuestWorldState_LoadsSectorsFromMinimalWad()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        var store = new GuestWorldStateStore();

        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", store, out _));
        Assert.Single(store.Sectors);
        Assert.True(store.Sectors.TryGetValue(0, out var sector));
        Assert.Equal(0, sector.Floor);
        Assert.Equal(128, sector.Ceiling);
        Assert.Equal(160, sector.LightLevel);
    }

    [Fact]
    public void AuthoritySend_MapBootstrapGuestReceive_MatchesSectorChecksum()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        const int rngSeed = 17;
        var gameId = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var authorityStore = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", authorityStore, out _));

        var authorityChecksum = new SnapshotChecksumSession();
        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.TrackClient(guestEndpoint, clientSlot: 1);
        authority.SetAuthorityWorldState(authorityStore, authorityChecksum, rngSeed);

        var guestStore = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", guestStore, out _));
        var guestChecksum = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum, rngSeed);

        var now = (ulong)Environment.TickCount64;
        authority.PumpClient(now, guestEndpoint, clientSlot: 1);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(tailSections.Value.HasChecksum);
        Assert.NotNull(tailSections.Value.WorldDeltaSectors);
        Assert.Single(tailSections.Value.WorldDeltaSectors!);
        Assert.True(guestStore.Sectors.TryGetValue(0, out var guestSector));
        Assert.Equal(0, guestSector.Floor);
        Assert.Equal(128, guestSector.Ceiling);
        Assert.Equal(160, guestSector.LightLevel);
        Assert.True(authorityChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out var authorityHashes));
        Assert.True(guestChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out var guestHashes));
        Assert.Equal(authorityHashes, guestHashes);
        Assert.Equal(authorityHashes, tailSections.Value.ChecksumHashes);
    }
}
