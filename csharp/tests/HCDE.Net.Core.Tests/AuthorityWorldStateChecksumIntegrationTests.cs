namespace HCDE.Net.Core.Tests;

public class AuthorityWorldStateChecksumIntegrationTests
{
    [Fact]
    public void AuthoritySend_GuestReceive_MatchesWorldStoreChecksum()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int rngSeed = 11;

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var worldState = new GuestWorldStateStore();
        worldState.ApplyPose(
            recipientClientSlot: 1,
            new PlayerPoseWorldDelta(
                1,
                LiveConstants.ServerWorldDeltaPoseHasActor,
                health: 75,
                armor: 0,
                posX: 0,
                posY: 0,
                posZ: 0,
                velX: 0,
                velY: 0,
                velZ: 0,
                yawBams: 0,
                pitchBams: 0),
            sequenceAck: 0);

        var authorityChecksum = new SnapshotChecksumSession();
        var authority = new LiveAuthoritySession(authorityTransport, gameId, authoritySlot: 0, maxClients: 4);
        authority.TrackClient(guestEndpoint, clientSlot: 1);
        authority.SetAuthorityWorldState(worldState, authorityChecksum, rngSeed);

        var guestChecksum = new SnapshotChecksumSession();
        var guestStore = new GuestWorldStateStore();
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestChecksum, rngSeed);

        var now = (ulong)Environment.TickCount64;
        authority.PumpClient(now, guestEndpoint, clientSlot: 1);

        Assert.True(guest.TryReceiveAuthorityControl(out _));
        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(tailSections.Value.HasChecksum);
        Assert.NotNull(tailSections.Value.WorldDeltaPoses);
        Assert.Single(tailSections.Value.WorldDeltaPoses!);
        Assert.True(guestStore.Players.ContainsKey(1));
        Assert.Equal(75, guestStore.Players[1].Health);
        Assert.True(authorityChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out var authorityHashes));
        Assert.True(guestChecksum.Ring.TryFind((int)tailSections.Value.ChecksumGameTic, out var guestHashes));
        Assert.Equal(authorityHashes, guestHashes);
        Assert.Equal(authorityHashes, tailSections.Value.ChecksumHashes);
    }
}
