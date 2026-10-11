using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsInfiniteAmmoPowerQueryTests
{
    [Theory]
    [InlineData(0, 41)]
    [InlineData(9, 73)]
    [InlineData(999, 0)]
    public void ScriptTimerQueryUsesActivatorOrExactTid(int tid, int expected)
    {
        var sim = Room(); var players = sim.Players.ToArray();
        players[0].GivePowerInfiniteAmmo(41); players[1].GivePowerInfiniteAmmo(73);
        int[] words = [(int)AcsPcode.PushNumber, tid,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, AcsCallFunctions.GetActorPowerupTics,
            (int)AcsPcode.PushNumber, expected, (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code, StringTable = ["powerinfiniteammo"] });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, players[0])); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(41, players[0].PowerInfiniteAmmoTics); Assert.Equal(73, players[1].PowerInfiniteAmmoTics);
    }

    [Fact]
    public void QueryReturnsItemTimerRatherThanServerSetting()
    {
        var sim = Room(true); var player = sim.Players.First();
        Assert.True(player.HasInfiniteAmmo);
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerInfiniteAmmo"));
        player.GivePowerInfiniteAmmo(55);
        Assert.Equal(55, AcsActorPowerups.RemainingTics(player, "POWERINFINITEAMMO"));
        ActorInventoryActions.TakeInventory(player, "PowerInfiniteAmmo");
        Assert.True(player.HasInfiniteAmmo);
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerInfiniteAmmo"));
    }

    [Fact]
    public void QueryRejectsMissingAndDestroyedOwners()
    {
        var player = Room().Players.First(); player.GivePowerInfiniteAmmo(55);
        Assert.Equal(0, AcsActorPowerups.RemainingTics(null, "PowerInfiniteAmmo"));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(new Actor(), "PowerInfiniteAmmo"));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, null));
        player.Destroy(); Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerInfiniteAmmo"));
    }

    private static AuthoritySimulation Room(bool infinite = false) => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, Id = 9, X = 400 }] },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative, InfiniteAmmo: infinite));
}
