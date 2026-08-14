namespace HCDE.Net.Core.Tests;

public class NetcodeCrossLanguageTests
{
    [Fact]
    public void SkipsUnlessNetcodeSoakConfigured()
    {
        var serverPath = Environment.GetEnvironmentVariable("HCDE_HCDESERV_PATH");
        var iwadPath = Environment.GetEnvironmentVariable("HCDE_IWAD_PATH");
        if (string.IsNullOrWhiteSpace(serverPath) || string.IsNullOrWhiteSpace(iwadPath))
            return;

        Assert.True(File.Exists(serverPath));
        Assert.True(File.Exists(iwadPath));
    }
}

public class GuestChecksumApplyIntegrationTests
{
    private sealed class RecordingMismatchSink : ISnapshotChecksumMismatchSink
    {
        public List<SnapshotChecksumMismatch> Reported { get; } = new();

        public void ReportMismatch(SnapshotChecksumMismatch mismatch, uint remoteTic) => Reported.Add(mismatch);
    }

    [Fact]
    public void GuestReceive_ReportsChecksumMismatchFromTail()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var checksumSession = new SnapshotChecksumSession();
        var localHashes = new uint[] { 10, 20, 30, 40, 50, 60 };
        checksumSession.Ring.Store(7, localHashes);

        var mismatchSink = new RecordingMismatchSink();
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetChecksumSession(checksumSession, mismatchSink);

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        var remoteHashes = localHashes.ToArray();
        remoteHashes[(int)SnapshotChecksumCategory.Actors] = 999;
        Assert.True(gameplay.TrySendServerSnapshot(
            guestEndpoint,
            roomId: 0,
            gameTic: 7,
            playerNum: 1,
            includeMinimalTail: true,
            checksumHashes: remoteHashes));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(tailSections.Value.HasChecksum);
        Assert.Single(mismatchSink.Reported);
        Assert.Equal(SnapshotChecksumCategory.Actors, mismatchSink.Reported[0].Category);
    }
}
