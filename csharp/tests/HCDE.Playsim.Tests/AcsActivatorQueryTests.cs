using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActivatorQueryTests
{
    [Fact]
    public void ActivatorTid_ReportsTheTriggeringActor()
    {
        var sim = Room(playerThingId: 42);
        var player = sim.Players.Single();
        Assert.Equal(42, player.ThingId);
        Run(sim, player,
            (int)AcsPcode.ActivatorTid,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PlayerHealth_ReportsActivatorHealth()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Health = 77;
        Run(sim, player,
            (int)AcsPcode.PlayerHealth,
            (int)AcsPcode.PushNumber, 77,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void LineSide_ReportsBackSideActivation()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var line = new LevelLine { Index = 0, X1 = 0, Y1 = 0, X2 = 0, Y2 = 64 };
        Run(sim, player, line, backSide: true,
            (int)AcsPcode.LineSide,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 24,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void Run(
        AuthoritySimulation sim,
        Actor activator,
        params int[] words) =>
        Run(sim, activator, triggerLine: null, backSide: false, words);

    private static void Run(
        AuthoritySimulation sim,
        Actor activator,
        LevelLine? triggerLine,
        bool backSide,
        params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator, triggerLine, backSide));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room(int playerThingId = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Id = playerThingId, X = 32, Y = 64 }],
    });
}
