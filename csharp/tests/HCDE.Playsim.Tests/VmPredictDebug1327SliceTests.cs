using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPredictDebug1327SliceTests
{
    [Fact]
    public void Gates_LogPingAndHoldTheWindow()
    {
        var off = new VmPredictDebug1327 { Level = 0, NetGame = true, NowMs = 5000 };
        off.NetworkClients.Add(0);
        off.Tick(1, 2, 3);
        Assert.Empty(off.Pings);
        Assert.Equal(int.MaxValue, off.MinAvailable);

        var blocked = Ready();
        blocked.NetGame = false;
        blocked.Tick(1, 2, 3);
        blocked.NetGame = true;
        blocked.GameState = 1;
        blocked.Tick(1, 2, 3);
        blocked.GameState = VmPredictDebug1327.GsLevel;
        blocked.DemoPlayback = true;
        blocked.Tick(1, 2, 3);
        blocked.DemoPlayback = false;
        blocked.LocalAuthority = true;
        blocked.Tick(1, 2, 3);
        Assert.Empty(blocked.Pings);

        var host = Ready();
        host.NowMs = 1000;
        host.ClientTic = 10;
        host.GameTic = 4;
        host.TicDup = 2;
        host.ExtraTic = true;
        host.ConsolePlayer = 0;
        host.Interval = 15;
        host.NetworkClients.Add(0);
        host.NetworkClients.Add(1);
        host.Clients[0].AverageLatency = 40;
        host.Clients[1].AverageLatency = 12;
        host.Clients[1].CurrentSequence = 8;
        host.AvailableTics = 5;
        host.ServerActive = 0;
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef { Missing = true });
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef { ObjectFlags = VmPredictDebug1327.OfEuthanizeMe });
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef { Corpse = true });
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef());
        host.Tick(3, 5, 1);
        Assert.Equal(2, host.Pings.Count);
        Assert.Equal(new VmPredictDebug1327.PingSample(0, 3, 171, 40, 2, 1, 6), host.Pings[0]);
        Assert.Equal(new VmPredictDebug1327.PingSample(1, 6, 342, 12, 2, 1, 6), host.Pings[1]);
        Assert.Equal(5, host.MinAvailable);
        Assert.Equal(6, host.MaxBacklog);
        Assert.False(host.CsvHeaderProbed);
        Assert.Empty(host.DesyncWarnings);
        Assert.Equal(1000ul, host.MismatchStartMs);

        host.NowMs = 1500;
        host.Tick(3, 2, 1);
        Assert.Equal(2, host.Pings.Count);
        Assert.Equal(2, host.MinAvailable);
        Assert.Equal(6, host.MaxBacklog);

        host.NowMs = 1999;
        host.Tick(3, 2, 1);
        Assert.Empty(host.DesyncWarnings);
        host.NowMs = 2000;
        host.Tick(3, 2, 1);
        Assert.Equal(4, host.Pings.Count);
        Assert.Equal(2000ul, host.LastPingMs);
        Assert.Single(host.DesyncWarnings);
        Assert.Equal(
            "[MIRROR PARITY DESYNC] mismatch for >1s: server active monsters=0, client blocking mirrors=1, gametic=4",
            host.DesyncWarnings[0]);

        host.ServerActive = 1;
        host.NowMs = 4000;
        host.Tick(3, 2, 1);
        Assert.Equal(0ul, host.MismatchStartMs);
        Assert.Single(host.DesyncWarnings);
    }

    [Fact]
    public void Sample_WritesTheCsvAndRatesTheSoftWarn()
    {
        var host = Ready();
        host.Level = 3;
        host.NowMs = 5000;
        host.AppData = "app";
        host.GameTic = 40;
        host.ClientTic = 50;
        host.TicDup = 1;
        host.Interval = 15;
        host.TotalTics = 2;
        host.AvailableTics = 0;
        host.LowestSequence = 7;
        host.AuthoritySlot = 2;
        host.EnterTic = 80;
        host.LastGameUpdate = 70;
        host.ConsolePlayer = 0;
        host.InvasionName = "wave";
        host.LagName = "run";
        host.Wave = 2;
        host.ServerActive = 1;
        host.Predicting = true;
        host.HudDebug = true;
        host.Room = 9;
        host.AckLagThreshold = 1;
        host.MirrorThreshold = 5;
        host.PassiveThreshold = 3;
        host.LifetimePassiveClient = 9;
        host.LifetimePassiveAuth = 4;
        host.HealthRepairs = 4;
        host.StateRepairs = 1;
        host.RespawnRepairs = 1;
        host.DeathRepairs = 1;
        host.FaultReports = 6;
        host.Clients[2].CurrentSequence = 11;
        host.Clients[2].SequenceAck = 8;
        host.Peers[2].SnapshotReceived = 7;
        host.Peers[2].ClientCommandSent = 8;
        host.Players[0] = new VmPredictDebug1327.PlayerSlot
        {
            Cheats = VmPredictDebug1327.CfPredicting,
            Bob = 1.25,
            ViewHeight = 41,
            ViewZ = 50,
            HasActor = true,
            X = 1,
            Y = 2,
            Z = 30,
            VelX = 0.5,
            VelY = 0,
            VelZ = -1,
            Health = 80,
            Weapon = new VmPredictDebug1327.PSprite { X = 4.5, Y = 32.25, Alive = true },
        };
        host.Commands[49] = new VmPredictDebug1327.UserCmd
        {
            Buttons = VmPredictDebug1327.BtAttack,
            Forward = 1,
        };
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef { Projectile = true });
        host.Mirrors.Add(new VmPredictDebug1327.MirrorRef
        {
            Action = VmPredictDebug1327.ActionMissile,
            VisualTargetTic = 1,
        });
        host.Files["app/hcde_predict_metrics.csv"] = "kept";
        host.Tick(2, 0, 7);

        string csv = host.Files["app/hcde_predict_metrics.csv"];
        Assert.StartsWith("kept", csv);
        Assert.DoesNotContain(VmPredictDebug1327.CsvHeader, csv);
        Assert.Contains("5000,40,50,2,0,10,7,2,11,8,3,0,0,0,7,8,9,4,5,2,6,0,3,39,10,wave,2,1,2,1,1,1,0,10,run,1,1,1,1,1,0,0,0x00000001,1.00,2.00,30.00,0.500,0.000,-1.000,1.2500,80,41.00,20.00,4.50,32.25,1,9\n", csv);
        Assert.Equal(5000ul, host.LastSoftWarnMs);
        Assert.Equal(5000ul, host.LastTraceDumpMs);
        Assert.Contains("triggers=[ack-lag available-low stale-attack passive-storm hard-churn ]", host.Files["app/hcde_prediction_softwarn.log"]);
        Assert.Contains("passive=9 invasion=wave wave=2 triggers=ack-lag available-low stale-attack passive-storm hard-churn ", host.SoftWarnings[0]);
        Assert.EndsWith("passive=9\n", host.HudLines[0]);
        Assert.Equal("app/hcde_prediction_softwarn_trace_5000.txt", host.Traces[0].Path);
        Assert.Equal("net", host.Traces[0].Channel);
        Assert.Equal("Debug", host.Traces[0].Severity);
        Assert.Equal(0, host.MinAvailable);
        Assert.Equal(3, host.MaxAckLag);
        Assert.Equal(39, host.MaxStaleVisual);
        Assert.Equal(10, host.MaxBacklog);
        Assert.Equal(40, host.LastSampleTic);
        Assert.Equal(9ul, host.WindowPassiveClient);
        Assert.Equal(5ul, host.WindowLocalRepairs);

        host.NowMs = 5200;
        host.Tick(2, 4, 7);
        Assert.Single(host.SoftWarnings);
        Assert.Equal(0, host.MinAvailable);

        host.NowMs = 5300;
        host.GameTic = 55;
        host.Tick(2, 4, 7);
        Assert.Equal(2, host.SoftWarnings.Count);
        Assert.Single(host.Traces);

        var ackOnly = Ready();
        ackOnly.Level = 3;
        ackOnly.NowMs = 8000;
        ackOnly.AppData = "app";
        ackOnly.GameTic = 20;
        ackOnly.ClientTic = 20;
        ackOnly.AvailableTics = 5;
        ackOnly.AuthoritySlot = 0;
        ackOnly.Clients[0].CurrentSequence = 9;
        ackOnly.Clients[0].SequenceAck = 1;
        ackOnly.AckLagThreshold = 2;
        ackOnly.MirrorThreshold = 99;
        ackOnly.PassiveThreshold = 99;
        ackOnly.EnterTic = 1;
        ackOnly.PauseGraceUntil = 0;
        ackOnly.InvasionName = "idle";
        ackOnly.LagName = "ok";
        ackOnly.Tick(1, 5, 0);
        Assert.Single(ackOnly.SoftWarnings);
        Assert.Contains("triggers=ack-lag ", ackOnly.SoftWarnings[0]);
        Assert.Empty(ackOnly.Traces);
        Assert.StartsWith(VmPredictDebug1327.CsvHeader, ackOnly.Files["app/hcde_predict_metrics.csv"]);

        var grace = Ready();
        grace.Level = 2;
        grace.NowMs = 9000;
        grace.AppData = "app";
        grace.GameTic = 20;
        grace.ClientTic = 30;
        grace.AvailableTics = 0;
        grace.EnterTic = 1;
        grace.PauseGraceUntil = 10;
        grace.AckLagThreshold = 0;
        grace.InvasionName = "idle";
        grace.LagName = "ok";
        grace.Tick(1, 0, 0);
        Assert.Empty(grace.SoftWarnings);
        Assert.True(grace.CsvHeaderProbed);
        Assert.Equal(20, grace.LastSampleTic);
    }

    static VmPredictDebug1327 Ready()
    {
        return new VmPredictDebug1327
        {
            Level = 1,
            NetGame = true,
            GameState = VmPredictDebug1327.GsLevel,
        };
    }
}
