using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNet1442SliceTests
{
    static VmNet1442Slice.Packet P(int client, params byte[] bytes)
    {
        return new VmNet1442Slice.Packet { Client = client, Bytes = bytes };
    }

    [Fact]
    public void ControlPackets()
    {
        Assert.Equal(5848, VmNet1442Slice.BandStart);
        Assert.Equal(6227, VmNet1442Slice.BandEnd);
        var world = new VmNet1442Slice.World { GameTic = 8, ClientTic = 3, Room = 4, Now = 50 };
        world.Inbox.Add(P(2, VmNet1442Slice.Exit, 9));
        world.Arbitrator = 0;
        VmNet1442Slice.GetPackets(world);
        Assert.Equal((2, -1), world.Quits[0]);
        Assert.Contains("Received exit packet from client 2 at gametic=8 clienttic=3 room=4", world.Logs[0]);
        Assert.Equal(50ul, world.Peers[2].LastPacket);
        Assert.Equal(1, world.Blackboxes);

        world = new VmNet1442Slice.World { Arbitrator = 0 };
        world.Inbox.Add(P(0, VmNet1442Slice.Exit, 6));
        VmNet1442Slice.GetPackets(world);
        Assert.Equal((0, 6), world.Quits[0]);

        world = new VmNet1442Slice.World();
        world.Inbox.Add(P(1, VmNet1442Slice.Setup));
        VmNet1442Slice.GetPackets(world);
        Assert.Equal(1, world.Setups);
        Assert.Equal(1, world.Connects);

        world = new VmNet1442Slice.World();
        world.Inbox.Add(P(2, VmNet1442Slice.Latency, 3));
        world.Inbox.Add(P(2, VmNet1442Slice.Latency, 9));
        world.Inbox.Add(P(4, VmNet1442Slice.Latency, 1));
        VmNet1442Slice.GetPackets(world);
        Assert.Equal(2, world.Sent.Count);
        Assert.Equal(new byte[] { VmNet1442Slice.LatencyAck, 3 }, world.Sent[0].Bytes);
        Assert.Equal(2, world.Sent[0].Client);
        Assert.Equal(new byte[] { VmNet1442Slice.LatencyAck, 1 }, world.Sent[1].Bytes);

        world = new VmNet1442Slice.World { Now = 99 };
        world.Peers[1].CurrentLatency = 35;
        world.Peers[1].NewLatency = false;
        world.Inbox.Add(P(1, VmNet1442Slice.LatencyAck, 35));
        world.Inbox.Add(P(1, VmNet1442Slice.LatencyAck, 1));
        VmNet1442Slice.GetPackets(world);
        Assert.Equal(99ul, world.Peers[1].RecvTime[0]);
        Assert.Equal(36, world.Peers[1].CurrentLatency);
        Assert.True(world.Peers[1].NewLatency);

        world = new VmNet1442Slice.World { Room = 4, Arbitrator = 1 };
        world.Inbox.Add(P(1, VmNet1442Slice.LevelReady, 4, 1, 2));
        world.Inbox.Add(P(2, VmNet1442Slice.LevelReady, 4, 9, 9));
        world.Inbox.Add(P(1, VmNet1442Slice.LevelReady, 5, 1, 2));
        VmNet1442Slice.GetPackets(world);
        Assert.Equal((1, 258), world.LevelStarts[0]);
        Assert.Equal((2, 0), world.LevelStarts[1]);
        Assert.Equal(2, world.LevelStarts.Count);

        world = new VmNet1442Slice.World();
        world.Inbox.Add(new VmNet1442Slice.Packet { Client = 3, Bytes = new byte[] { 0 }, Live = true, LiveOk = true });
        world.Inbox.Add(new VmNet1442Slice.Packet { Client = 3, Bytes = new byte[] { 0 }, Live = true, LiveOk = false });
        world.Inbox.Add(new VmNet1442Slice.Packet { Client = 3, Bytes = new byte[] { 0 }, SetupBusy = true });
        world.Inbox.Add(new VmNet1442Slice.Packet { Client = 3, Bytes = new byte[] { 0 }, Enforce = true, UsesService = true });
        world.Inbox.Add(new VmNet1442Slice.Packet { Client = 4, Bytes = new byte[] { 0 }, Enforce = true });
        VmNet1442Slice.GetPackets(world);
        Assert.Equal(new[] { "header-too-short" }, world.Reasons);
        Assert.Equal("raw-ncmd-from-hcde-peer", world.Rejects[0].Reason);
        Assert.Equal("raw-ncmd-from-non-hcde-peer", world.Rejects[1].Reason);
        Assert.Equal(VmNet1442Slice.MissingSeq, world.Peers[3].Flags);
        Assert.Equal(0, world.Peers[4].Flags);
    }

    [Fact]
    public void GameplayFieldsAndStrikes()
    {
        var kick = new VmNet1442Slice.World { LocalAuthority = true, Arbitrator = 0, GameTic = 11, ClientTic = 2, Room = 6, Now = 1000 };
        kick.Peers[3].Name = "Ace";
        for (int i = 0; i < 6; i++)
            kick.Inbox.Add(P(3, 0));
        VmNet1442Slice.GetPackets(kick);
        Assert.Equal(VmNet1442Slice.Quit | VmNet1442Slice.MissingSeq, kick.Peers[3].Flags);
        Assert.Equal(0, kick.Peers[3].Strikes);
        Assert.Contains("Disconnecting client 3 'Ace' for repeated malformed gameplay packets at gametic=11 clienttic=2 room=6", kick.Logs[0]);

        var self = new VmNet1442Slice.World { LocalAuthority = true, Arbitrator = 0, Now = 1000 };
        for (int i = 0; i < 6; i++)
            self.Inbox.Add(P(0, 0));
        VmNet1442Slice.GetPackets(self);
        Assert.Equal(VmNet1442Slice.MissingSeq, self.Peers[0].Flags);
        Assert.Empty(self.Logs);

        var window = new VmNet1442Slice.World { Now = 10 };
        window.Inbox.Add(P(1, 0));
        VmNet1442Slice.GetPackets(window);
        Assert.Equal(1, window.Peers[1].Strikes);
        window.Now = 5010;
        window.Inbox.Add(P(1, 0));
        VmNet1442Slice.GetPackets(window);
        Assert.Equal(2, window.Peers[1].Strikes);
        window.Now = 5011;
        window.Inbox.Add(P(1, 0));
        VmNet1442Slice.GetPackets(window);
        Assert.Equal(1, window.Peers[1].Strikes);
        Assert.Equal(5011ul, window.Peers[1].Window);

        var stale = new VmNet1442Slice.World { Now = 40, Room = 2 };
        stale.Peers[1].Strikes = 2;
        stale.Peers[1].Window = 40;
        var stalePacket = P(1, new byte[10]);
        stalePacket.Stale = true;
        stale.Inbox.Add(stalePacket);
        VmNet1442Slice.GetPackets(stale);
        Assert.Equal(2, stale.Peers[1].Strikes);
        Assert.Equal(0, stale.Peers[1].Flags);
        Assert.Single(stale.Headers);
        Assert.Equal(0, stale.Headers[0].SequenceAck);

        var clear = new VmNet1442Slice.World { Now = 5 };
        clear.Peers[1].Strikes = 4;
        clear.Peers[1].Window = 5;
        clear.Inbox.Add(P(1, new byte[14]));
        VmNet1442Slice.GetPackets(clear);
        Assert.Equal(0, clear.Peers[1].Strikes);
        Assert.Equal(0ul, clear.Peers[1].Window);

        var quitting = new VmNet1442Slice.World { GameTic = 4, ClientTic = 1, Room = 3 };
        var quitPacket = new byte[16];
        quitPacket[0] = VmNet1442Slice.Quitters;
        quitPacket[10] = 1;
        quitPacket[11] = 4;
        quitting.Inbox.Add(P(2, quitPacket));
        VmNet1442Slice.GetPackets(quitting);
        Assert.Equal(4, quitting.Disconnects[0]);
        Assert.Contains("Authority reported client 4 quit at gametic=4 clienttic=1 room=3", quitting.Logs[0]);
        Assert.Equal(0, quitting.Peers[2].Flags);

        var shortQuit = new VmNet1442Slice.World();
        var shortPacket = new byte[12];
        shortPacket[0] = VmNet1442Slice.Quitters;
        shortPacket[10] = 2;
        shortPacket[11] = 4;
        shortQuit.Inbox.Add(P(2, shortPacket));
        VmNet1442Slice.GetPackets(shortQuit);
        Assert.Equal(new[] { 4 }, shortQuit.Disconnects);
        Assert.Equal("quitters-entry-malformed", shortQuit.Reasons[0]);
        Assert.Equal(VmNet1442Slice.MissingSeq, shortQuit.Peers[2].Flags);
        Assert.Equal(1, shortQuit.Peers[2].Strikes);

        var played = new VmNet1442Slice.World { Arbitrator = 1 };
        var body = new byte[23];
        body[5] = 7;
        body[9] = 9;
        body[10] = 1;
        body[11] = 2;
        body[18] = 2;
        body[20] = VmNet1442Slice.EmptyCmd;
        body[21] = 1;
        body[22] = VmNet1442Slice.EmptyCmd;
        played.Inbox.Add(P(5, body));
        VmNet1442Slice.GetPackets(played);
        Assert.Equal(9, played.Peers[2].ConsistencyAck);
        Assert.Equal(1, played.Peers[2].CurrentSequence);
        Assert.Equal(1, played.Peers[2].SequenceAck);
        Assert.Equal(2, played.Commands.Count);
        Assert.Equal(new byte[] { VmNet1442Slice.EmptyCmd }, played.Commands[0].Payload);
        Assert.Equal(0, played.Commands[0].Sequence);
        Assert.Equal(1, played.Commands[1].Sequence);

        var gap = new VmNet1442Slice.World();
        gap.Peers[2].CurrentSequence = 5;
        var gapBody = new byte[21];
        gapBody[10] = 1;
        gapBody[11] = 1;
        gapBody[15] = 7;
        gapBody[18] = 2;
        gapBody[20] = VmNet1442Slice.EmptyCmd;
        gap.Inbox.Add(P(5, gapBody));
        VmNet1442Slice.GetPackets(gap);
        Assert.Empty(gap.Commands);
        Assert.Equal(5, gap.Peers[2].CurrentSequence);
        Assert.Equal(VmNet1442Slice.MissingSeq, gap.Peers[5].Flags);

        var dup = new VmNet1442Slice.World();
        dup.Peers[2].CurrentSequence = 5;
        var dupBody = new byte[21];
        dupBody[10] = 1;
        dupBody[11] = 1;
        dupBody[15] = 5;
        dupBody[18] = 2;
        dupBody[20] = VmNet1442Slice.EmptyCmd;
        dup.Inbox.Add(P(5, dupBody));
        VmNet1442Slice.GetPackets(dup);
        Assert.Empty(dup.Commands);
        Assert.Equal(0, dup.Peers[5].Flags);

        var cons = new VmNet1442Slice.World();
        var consBody = new byte[25];
        consBody[10] = 1;
        consBody[12] = 2;
        consBody[18] = 2;
        consBody[21] = 9;
        consBody[22] = 1;
        cons.Inbox.Add(P(5, consBody));
        VmNet1442Slice.GetPackets(cons);
        Assert.Equal(9, cons.Peers[2].NetConsistency[0]);
        Assert.Equal(0, cons.Peers[2].CurrentNetConsistency);
        Assert.Equal(VmNet1442Slice.MissingCon, cons.Peers[5].Flags);

        var ahead = new VmNet1442Slice.World { Arbitrator = 1 };
        var aheadBody = new byte[17];
        aheadBody[10] = 1;
        aheadBody[13] = 6;
        aheadBody[14] = 2;
        aheadBody[15] = 1;
        aheadBody[16] = 2;
        ahead.Inbox.Add(P(1, aheadBody));
        VmNet1442Slice.GetPackets(ahead);
        Assert.Equal(6, ahead.CommandsAhead);
        Assert.Equal(258, ahead.Peers[2].AverageLatency);
        Assert.Equal((2, 1), ahead.Ensured[0]);

        var local = new VmNet1442Slice.World { Arbitrator = 1, LocalAuthority = true };
        local.Inbox.Add(P(1, (byte[])aheadBody.Clone()));
        VmNet1442Slice.GetPackets(local);
        Assert.Equal(6, local.CommandsAhead);
        Assert.Equal(0, local.Peers[2].AverageLatency);
        Assert.Equal(-1, local.Peers[2].CurrentSequence);
        Assert.Equal(-1, local.Peers[2].ConsistencyAck);

        var stable = new VmNet1442Slice.World { LocalAuthority = true, Arbitrator = 0 };
        var stableBody = new byte[15];
        stableBody[10] = 1;
        stableBody[13] = 8;
        stableBody[14] = 2;
        stable.Inbox.Add(P(5, stableBody));
        VmNet1442Slice.GetPackets(stable);
        Assert.Equal(8, stable.Peers[5].Stability);
        Assert.Equal(0, stable.CommandsAhead);

        var badIndex = new VmNet1442Slice.World { Now = 8 };
        badIndex.Peers[5].Strikes = 3;
        badIndex.Peers[5].Window = 8;
        var badBody = new byte[15];
        badBody[10] = 1;
        badBody[14] = 200;
        badIndex.Inbox.Add(P(5, badBody));
        VmNet1442Slice.GetPackets(badIndex);
        Assert.Equal(4, badIndex.Peers[5].Strikes);
        Assert.Equal("packet-fields-malformed", badIndex.Reasons[0]);
        Assert.Contains(badIndex.Logs, line => line.Contains("player index=200"));

        var badCmd = new VmNet1442Slice.World { LocalAuthority = true, Arbitrator = 0, Now = 3 };
        badCmd.Peers[5].Strikes = 5;
        badCmd.Peers[5].Window = 3;
        badCmd.Peers[5].Name = "Zoe";
        var cmdBody = new byte[21];
        cmdBody[10] = 1;
        cmdBody[11] = 1;
        cmdBody[18] = 2;
        cmdBody[20] = 0;
        badCmd.Inbox.Add(P(5, cmdBody));
        VmNet1442Slice.GetPackets(badCmd);
        Assert.Equal(VmNet1442Slice.Quit | VmNet1442Slice.MissingSeq, badCmd.Peers[5].Flags);
        Assert.Equal(0, badCmd.Peers[5].Strikes);
        Assert.Equal(0ul, badCmd.Peers[5].Window);
        Assert.Equal("command-record-malformed", badCmd.Reasons[0]);
    }
}
