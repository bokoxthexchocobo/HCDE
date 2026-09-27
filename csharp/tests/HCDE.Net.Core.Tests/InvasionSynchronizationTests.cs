using System.Net;
using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class InvasionSynchronizationTests
{
    [Fact]
    public void GuestKeepsWaveTimerAndEnemiesFromSameFreshSnapshot()
    {
        using var server = new UdpTransport();
        using var client = new UdpTransport();
        server.Bind(0);
        client.Bind(0);
        server.SetNonBlocking(true);
        client.SetNonBlocking(true);
        var gameId = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var endpoint = new NetworkEndpoint(IPAddress.Loopback, client.BoundPort);
        var guest = new LiveGuestSession(client, gameId,
            new NetworkEndpoint(IPAddress.Loopback, server.BoundPort), 1, 0, 4);
        var invasion = new GuestInvasionState();
        guest.SetApplySinks(null, null, invasionSink: invasion);
        var sender = new LiveGameplayEndpoint(server, gameId);

        void Send(uint tic, uint wave, uint timer, uint spawned, uint cleared, uint alive, byte state)
        {
            var tail = new byte[256];
            var cursor = WorldDeltaChunkCodec.WriteEmpty(tail, tic);
            cursor += InvasionSnapshotCodec.WriteV2(tail.AsSpan(cursor), new InvasionSnapshotHeader(
                flags: 0, state: state, stateTics: timer, wave: wave, maxWaves: 10,
                waveBudget: 20, waveSpawned: spawned, waveCleared: cleared, activeMonsters: alive));
            cursor += PresentationEchoCodec.WriteMinimal(tail.AsSpan(cursor));
            Assert.True(sender.TrySendServerSnapshotWithExternalTail(endpoint, 0, tic, 1, tail.AsSpan(0, cursor)));
            Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        }

        // A late join during wave 5's countdown must show the upcoming wave.
        Send(100, 4, 70, 0, 0, 0, LiveConstants.InvasionStateCountdown);
        Assert.Equal(5u, invasion.PendingWave);
        Assert.Equal(70u, invasion.StateTics);
        Send(101, 5, 35, 10, 4, 6, LiveConstants.InvasionStateSpawning);
        Assert.Equal(0u, invasion.PendingWave);
        Assert.Equal(6u, invasion.ActiveMonsters);

        // Fresh transport packets replaying older or identical tics cannot rewind HUD state.
        Send(99, 3, 350, 20, 0, 20, LiveConstants.InvasionStateCountdown);
        Send(101, 3, 350, 20, 0, 20, LiveConstants.InvasionStateCountdown);
        Assert.Equal(2, invasion.ApplyMirrorCalls);
        Assert.Equal(5, invasion.MirrorState.Wave);
        Assert.Equal(35u, invasion.StateTics);
        Assert.Equal(6u, invasion.ActiveMonsters);

        // A later authority correction replaces all counts, even for the same wave number.
        Send(102, 5, 20, 6, 2, 4, LiveConstants.InvasionStateSpawning);
        Assert.Equal(3, invasion.ApplyMirrorCalls);
        Assert.Equal(20u, invasion.StateTics);
        Assert.Equal(6u, invasion.MirrorState.WaveSpawned);
        Assert.Equal(2u, invasion.MirrorState.WaveCleared);
        Assert.Equal(4u, invasion.ActiveMonsters);
        Assert.Equal(10u, invasion.MaxWaves);
    }
}
