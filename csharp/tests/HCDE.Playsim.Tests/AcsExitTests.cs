using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsExitTests
{
    [Theory]
    [InlineData(false, false, 243)]
    [InlineData(false, false, 244)]
    [InlineData(false, true, 243)]
    [InlineData(false, true, 244)]
    [InlineData(true, false, 243)]
    [InlineData(true, false, 244)]
    [InlineData(true, true, 243)]
    [InlineData(true, true, 244)]
    public void ExitDispatchPreservesActivatorAndWorldExemption(bool stack, bool world, int special)
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.Acs.Add(Program(1, stack ? [3, 0, 4, special, 1] : [9, special, 0, 1]));
        Assert.True(sim.Acs.TryExecute(1, [], world ? null : player)); sim.Acs.Tick(sim);
        Assert.Equal(world, sim.Exited); Assert.Equal(!world, player.IsDead);
        if (world) Assert.Equal(special == 244, sim.SecretExit);
    }

    [Theory]
    [InlineData(80)]
    [InlineData(226)]
    public void MapTriggerAndNestedExecuteKeepActivator(int executeSpecial)
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.Acs.Add(Program(1, [13, executeSpecial, 2, 0, 0, 0, 0, 1]));
        sim.Acs.Add(Program(2, [9, 243, 0, 1]));
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine
            { Special = executeSpecial, Arg0 = 1, PlayerUse = true }, true));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.False(sim.Exited); Assert.True(player.IsDead);
    }

    [Fact]
    public void ActiveActivatorIdentityChangesChecksum()
    {
        var sim = Room(); sim.Acs.Add(Program(1, [9, 243, 0, 1]));
        var world = new AcsVm(); world.Add(Program(1, [9, 243, 0, 1]));
        Assert.True(world.TryExecute(1, []));
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        Assert.NotEqual(world.Checksum, sim.Acs.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResumeKeepsOriginalActivatorAndDestroyedActivatorBecomesWorld(bool destroyed)
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.Acs.Add(Program(1, [2, 9, 243, 0, 1]));
        Assert.True(sim.Acs.TryExecute(1, [], player)); sim.Acs.Tick(sim);
        if (destroyed) player.Destroy();
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(destroyed, sim.Exited);
        if (!destroyed) Assert.True(player.IsDead);
    }

    private static AcsProgram Program(int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch), noExit: true);
}
