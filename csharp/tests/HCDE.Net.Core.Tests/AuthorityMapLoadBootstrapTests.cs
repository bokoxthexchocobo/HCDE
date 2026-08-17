using HCDE.MapLoader;
using HCDE.MapLoader.Tests;

namespace HCDE.Net.Core.Tests;

public class AuthorityMapLoadBootstrapTests
{
    [Fact]
    public void TryBootstrapAuthorityWorldState_SeedsAuthorityStoreFromWad()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01");
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var transport = new HCDE.Net.Transport.UdpTransport();
        transport.Bind(0);

        var authority = new LiveAuthoritySession(transport, gameId, authoritySlot: 0, maxClients: 4);
        Assert.True(AuthorityMapLoadBootstrap.TryBootstrapAuthorityWorldState(
            authority,
            wad,
            "MAP01",
            out _,
            rngSeed: 9,
            replicateSectorMetadata: true));

        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        guestTransport.Bind(0);
        transport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);
        authority.TrackClient(guestEndpoint, clientSlot: 1);

        var guestStore = new GuestWorldStateStore();
        var guestChecksum = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(
            guestTransport,
            gameId,
            new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, transport.BoundPort),
            guestPlayerSlot: 1,
            authoritySlot: 0,
            maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum, rngSeed: 9);

        authority.PumpClient((ulong)Environment.TickCount64, guestEndpoint, clientSlot: 1);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(guestStore.Sectors.TryGetValue(0, out var sector));
        Assert.Equal(160, sector.LightLevel);
        Assert.True(guestChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out _));
    }
}
