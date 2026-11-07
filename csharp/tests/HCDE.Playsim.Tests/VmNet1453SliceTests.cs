using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1453SliceTests
{
    [Fact]
    public void LatencyCountdownAndClassicMap()
    {
        var world = new VmNet1453Slice.World { ClientTic = 6, GameTic = 0, Console = 2 };
        world.Latency[2] = 40;
        Assert.Equal((3, 171, 40), VmNet1453Slice.Latency(world));
        world.ClientTic = 5;
        Assert.Equal(2, VmNet1453Slice.Latency(world).Severity);
        world.ClientTic = 3;
        Assert.Equal(1, VmNet1453Slice.Latency(world).Severity);
        world.ClientTic = 0;
        world.Console = -1;
        Assert.Equal((0, 0, 0), VmNet1453Slice.Latency(world));

        world.State = 2;
        world.StateTics = 10;
        world.Cutscene = 40;
        world.LocalService = true;
        Assert.Equal(40, VmNet1453Slice.StateTics(world));
        world.StateTics = 50;
        Assert.Equal(50, VmNet1453Slice.StateTics(world));
        world.Netgame = true;
        world.LocalService = false;
        world.StateTics = -4;
        Assert.Equal(0, VmNet1453Slice.StateTics(world));
        Assert.Equal("countdown", VmNet1453Slice.StateName(world));
        Assert.Equal(5, VmNet1453Slice.ClassicState(world));
        world.State = 4;
        Assert.Equal(6, VmNet1453Slice.ClassicState(world));
        world.State = 7;
        Assert.Equal(9, VmNet1453Slice.ClassicState(world));
        world.State = 1;
        Assert.Equal(0, VmNet1453Slice.ClassicState(world));
    }

    [Fact]
    public void CountsArchvilesAndControl()
    {
        var world = new VmNet1453Slice.World
        {
            Wave = -2,
            MaxWaves = 8,
            Budget = 3,
            Spots = -1,
            WaveFlags = VmNet1453Slice.Boss,
            Monsters =
            {
                new VmNet1453Slice.Monster { Health = 10, Archvile = true },
                new VmNet1453Slice.Monster { Health = 0, Archvile = true },
                new VmNet1453Slice.Monster { Health = 4, Archvile = true, Euthanized = true },
                new VmNet1453Slice.Monster { Health = 9 },
            },
            ReplicatedMonsters = 6,
            ReplicatedArchviles = 2,
            Netgame = true,
        };
        Assert.Equal(0, VmNet1453Slice.Wave(world));
        Assert.Equal(8, VmNet1453Slice.ExportMaxWaves(world));
        Assert.Equal(6, VmNet1453Slice.ActiveMonsters(world));
        Assert.Equal(2, VmNet1453Slice.Archviles(world));
        world.LocalService = true;
        Assert.Equal(4, VmNet1453Slice.ExportActive(world));
        Assert.Equal(1, VmNet1453Slice.Archviles(world));
        Assert.True(VmNet1453Slice.BossWave(world));
        Assert.Equal(0, VmNet1453Slice.ExportSpots(world));
        Assert.Equal(3, VmNet1453Slice.ExportBudget(world));

        Assert.Equal(0, VmNet1453Slice.Control(world, 1, null));
        world.GameType = 4;
        Assert.Equal(1, VmNet1453Slice.Control(world, 1, null));
        Assert.Equal("api-nextwave", Assert.Single(world.Calls));
        Assert.Equal(1, VmNet1453Slice.Control(world, 3, "custom"));
        Assert.Equal("custom", world.Calls[1]);
        Assert.Equal(0, VmNet1453Slice.Control(world, 9, "nope"));
        world.Netgame = true;
        world.LocalService = false;
        Assert.Equal(0, VmNet1453Slice.Control(world, 2, "api-victory"));
    }
}
