using System.Net;
using System.Text;
using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class InvasionMultiClientTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EveryClientReceivesTheSameEventsCountsAndActorPosesForATic(bool invasion)
    {
        using var server = Open();
        using var first = Open();
        using var second = Open();
        var gameId = new byte[] { 1, 3, 5, 7, 2, 4, 6, 8 };
        var authority = new LiveAuthoritySession(server, gameId, 0, 4);
        var source = new GuestWorldStateStore();
        // Exceeds the former 512-byte tail and 1024-byte packet scaffolds.
        for (uint id = 1; id <= 40; id++)
        {
            source.SeedActor(id, 3004, 30, (byte)ReplicatedActorCategory.Monster, LiveConstants.ActorDeltaFlagLive);
            source.Actors[id].HasPose = true;
            source.Actors[id].PosX = id * 10;
            source.Actors[id].PosY = 64;
        }
        source.QueueAuthorityEvent(new AuthorityEventRecord(
            AuthorityEventType.Spawn, ReplicatedActorSource.Invasion, ReplicatedActorCategory.Monster,
            0, 1, 1, 3004, 30, 2, Encoding.UTF8.GetBytes("Zombieman"),
            10, 64, 0, 0, 0, 0, 0, 0));
        authority.SetAuthorityWorldState(source, new SnapshotChecksumSession());
        if (invasion) authority.SetAuthorityInvasionSnapshot(new InvasionSnapshotHeader(
            0, LiveConstants.InvasionStateCleanup, 0, 2, 8, 40, 40, 0, 40));
        authority.TrackClient(Endpoint(first), 1);
        authority.TrackClient(Endpoint(second), 2);
        var guests = new[]
        {
            new LiveGuestSession(first, gameId, Endpoint(server), 1, 0, 4),
            new LiveGuestSession(second, gameId, Endpoint(server), 2, 0, 4),
        };
        foreach (var guest in guests)
            guest.SetGuestWorldState(new GuestWorldStateStore(), new SnapshotChecksumSession());

        authority.PumpAllClients(100);
        var sections = new List<ServerSnapshotTailSections>();
        foreach (var guest in guests)
        {
            Assert.True(guest.TryReceiveAuthorityControl(out _));
            Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tail));
            Assert.NotNull(tail);
            sections.Add(tail!.Value);
            Assert.Single(tail.Value.AuthorityEventRecords!);
            Assert.Equal(40, tail.Value.ActorDeltaRecords!.Count);
            if (invasion) Assert.Equal(40u, guest.InvasionState!.ActiveMonsters);
            Assert.Equal(400, guest.GuestWorldState!.Actors[40].PosX);
            Assert.Equal(64, guest.GuestWorldState.Actors[40].PosY);
            Assert.True(guest.LastChecksumApplyValid);
            Assert.True(guest.LastChecksumApplyState.Compared);
        }
        Assert.Equal(sections[0].ChecksumHashes, sections[1].ChecksumHashes);
        Assert.Equal(guests[0].GuestWorldState!.AuthorityEventRollingHash, guests[1].GuestWorldState!.AuthorityEventRollingHash);
        Assert.Equal(guests[0].GuestWorldState!.ActorDeltaRollingHash, guests[1].GuestWorldState!.ActorDeltaRollingHash);
    }

    [Fact]
    public void InputSequenceIsIndependentPerPeerAndRepeatedCommandsAdvance()
    {
        using var server = Open();
        using var first = Open();
        using var second = Open();
        var gameId = new byte[] { 1, 3, 5, 7, 2, 4, 6, 8 };
        var authority = new LiveAuthoritySession(server, gameId, 0, 4);
        authority.TrackClient(Endpoint(first), 1);
        authority.TrackClient(Endpoint(second), 2);
        var sink = new Commands();
        authority.SetClientInputSink(sink);
        var one = new LiveGameplayEndpoint(first, gameId);
        var two = new LiveGameplayEndpoint(second, gameId);
        var buffer = new byte[NetConstants.MaxTransmitSize];
        void Send(LiveGameplayEndpoint sender, byte slot, uint tic)
        {
            Assert.True(sender.TrySendClientInput(Endpoint(server), 0, tic, slot, new UserCmd(1, 0, 0, 0, 0, 0, 0)));
            Assert.True(server.TryReceive(buffer, out var count, out var remote, TimeSpan.FromSeconds(1)));
            Assert.True(authority.TryApplyClientInputPacket(buffer.AsSpan(0, count), remote));
            // Duplicate transport delivery must not execute another shot.
            Assert.True(authority.TryApplyClientInputPacket(buffer.AsSpan(0, count), remote));
        }
        Send(one, 1, 1);
        Send(two, 2, 1);
        Send(one, 1, 2);
        Send(two, 2, 2);
        Assert.Equal(new[] { (1, 1), (2, 1), (1, 2), (2, 2) }, sink.Applied);
    }

    private sealed class Commands : IClientInputCommandSink
    {
        public List<(int Slot, int Sequence)> Applied { get; } = new();
        public bool ApplyCommand(int clientSlot, byte playerNum, int sequence, UserCmd command, ReadOnlyMemory<byte> eventRecords)
        {
            Assert.Equal(1u, command.Buttons);
            Applied.Add((clientSlot, sequence));
            return true;
        }
    }
    private static UdpTransport Open()
    {
        var socket = new UdpTransport();
        socket.Bind(0); socket.SetNonBlocking(true);
        return socket;
    }
    private static NetworkEndpoint Endpoint(UdpTransport socket) => new(IPAddress.Loopback, socket.BoundPort);
}
