using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1454SliceTests
{
    [Fact]
    public void ReadyPresetAndStatus()
    {
        var idle = new VmNet1454Slice.World();
        VmNet1454Slice.Ready(idle);
        Assert.Empty(idle.Writes);
        idle.Netgame = true;
        idle.Console = 1;
        VmNet1454Slice.Ready(idle);
        Assert.True(idle.Readied);
        Assert.Equal(VmNet1454Slice.Readied, Assert.Single(idle.Writes));
        VmNet1454Slice.ResetTimer(idle);
        Assert.True(idle.StartedCutscene);
        idle.Countdown = 12;
        Assert.Equal(12, VmNet1454Slice.ReadyTimer(idle));

        idle.ControlOk = false;
        VmNet1454Slice.Victory(idle);
        Assert.Equal("Invasion control rejected (authority + sv_gametype 4 required)", Assert.Single(idle.Logs));
        idle.ControlOk = true;
        idle.Logs.Clear();
        VmNet1454Slice.NextWave(idle);
        Assert.Equal("nextwave", Assert.Single(idle.Actions));

        idle.DmFlags = VmNet1454Slice.FastMonsters;
        VmNet1454Slice.ApplyPreset(idle, null);
        Assert.Contains("Invasion preset rejected", Assert.Single(idle.Logs));
        idle.LocalService = true;
        idle.Logs.Clear();
        VmNet1454Slice.Standard(idle);
        Assert.Equal(4, idle.GameType);
        Assert.Equal("invasion-standard-alias", idle.TraceReason);
        Assert.Equal(0, idle.DmFlags & VmNet1454Slice.FastMonsters);
        Assert.Equal(8, idle.Waves);
        Assert.Contains("waves=8 explicit-wavelimit=0 use-map-settings=1 exit-victory=1 budget=24+8/wave, per-player=6", Assert.Single(idle.Logs));

        idle.Logs.Clear();
        idle.StateName = "spawning";
        idle.StateTics = 9;
        idle.Wave = 2;
        idle.MaxWaves = 8;
        idle.Speed = 1.5;
        idle.HasMap = true;
        idle.MapWaveLimit = 4000;
        idle.MapDuelLimit = -2;
        idle.DuelLimit = 300;
        idle.ResolvedWaves = 8;
        idle.CanOverride = true;
        VmNet1454Slice.Status(idle);
        Assert.Contains("state=spawning (9 tics) wave=2/8", idle.Logs[0]);
        Assert.Contains("speedscale=1.50", idle.Logs[0]);
        Assert.Contains("map-wavelimit=3000", idle.Logs[1]);
        Assert.Contains("map-duellimit=0", idle.Logs[1]);
        Assert.Contains("legacy-duellimit=255", idle.Logs[1]);
        Assert.Contains("allow-offline-overrides=1", idle.Logs[1]);
    }

    [Fact]
    public void SpotsPingsAndKick()
    {
        var world = new VmNet1454Slice.World
        {
            TotalSpots = 1,
            ActiveSpots = 1,
            FallbackSource = "map",
            Wave = 3,
            GameTic = 10,
            Spots = { new VmNet1454Slice.Spot { Source = "player", Tag = 4, X = 1.26, Y = 2, Z = 3, NextTic = 10, Spawned = 1 } },
        };
        VmNet1454Slice.Spots(world);
        Assert.Contains("total=1 active=1", world.Logs[0]);
        Assert.Contains("source=map wave=3", world.Logs[0]);
        Assert.Contains("#0 src=player tid=4 pos=(1.3, 2.0, 3.0)", world.Logs[1]);
        Assert.Contains("ready=1", world.Logs[1]);

        world.Logs.Clear();
        VmNet1454Slice.Pings(world);
        Assert.Contains("only be used when playing in a net game", Assert.Single(world.Logs));
        world.Netgame = true;
        world.Clients.Add(0);
        world.Logs.Clear();
        VmNet1454Slice.Pings(world);
        Assert.Empty(world.Logs);
        world.Clients.Add(2);
        world.Authority[0] = true;
        world.Latency[2] = 18;
        world.Names[2] = "ada";
        VmNet1454Slice.Pings(world);
        Assert.Equal("18ms ada [2]", Assert.Single(world.Logs));

        world.Logs.Clear();
        world.LocalService = false;
        VmNet1454Slice.Kick(world, "2");
        Assert.Contains("only accessible to the authority", Assert.Single(world.Logs));
        world.LocalService = true;
        world.Console = 1;
        world.InGame[2] = true;
        world.Logs.Clear();
        VmNet1454Slice.Kick(world, "nope", "2", "2", "1", "80");
        Assert.Contains(world.Logs, line => line.Contains("Bad client number nope"));
        Assert.Contains(world.Logs, line => line.Contains("Bad client number 80"));
        Assert.Equal(new[] { VmNet1454Slice.KickCommand, 2 }, world.Writes);
    }
}
