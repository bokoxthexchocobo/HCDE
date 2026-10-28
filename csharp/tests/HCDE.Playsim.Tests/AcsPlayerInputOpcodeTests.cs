using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPlayerInputOpcodeTests
{
    [Fact]
    public void OpcodeReadsBufferedForwardMoveForPlayerZero()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 42 });
        sim.Tick();
        Add(sim, 1, (int)AcsPcode.PushNumber, 0, (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.GetPlayerInput, (int)AcsPcode.PushNumber, 42, (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64, (int)AcsPcode.Lspec2Direct, 112, 7, 35, 1,
            (int)AcsPcode.Terminate);
        sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void OpcodeUsesActivatorWhenPlayerNumberNegative()
    {
        var sim = Room();
        sim.QueueCommand(0, new PlayerCommand { Use = true });
        sim.Tick();
        Add(sim, 1, (int)AcsPcode.PushNumber, -1, (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.GetPlayerInput, (int)AcsPcode.PushNumber, 2, (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64, (int)AcsPcode.Terminate);
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ClassifyActorOpcodeReportsPlayerAlive()
    {
        var sim = Room();
        Add(sim, 1, (int)AcsPcode.PushNumber, 0, (int)AcsPcode.ClassifyActor,
            (int)AcsPcode.PushNumber, 0x22, (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56, (int)AcsPcode.Lspec2Direct, 112, 7, 35, 1,
            (int)AcsPcode.Terminate);
        sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
        Assert.True(sim.Acs.Enqueue(number, ReadOnlySpan<int>.Empty, sim.Players.First()));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
