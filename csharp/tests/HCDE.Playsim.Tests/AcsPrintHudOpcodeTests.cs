using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPrintHudOpcodeTests
{
    [Fact]
    public void BeginPrint_EndPrint_FlushesToBuffer()
    {
        var sim = Room();
        Run(sim,
        [
            3, 0, (int)AcsPcode.BeginPrint,
            3, 42, (int)AcsPcode.PrintNumber,
            (int)AcsPcode.EndPrint,
            (int)AcsPcode.Terminate,
        ], ["ignored"]);
        Assert.Equal("42", sim.AcsPrintBuffer);
    }

    [Fact]
    public void PrintString_UsesStringTable()
    {
        var sim = Room();
        Run(sim,
        [
            3, 0, (int)AcsPcode.BeginPrint,
            3, 0, (int)AcsPcode.PrintString,
            (int)AcsPcode.EndPrint,
            (int)AcsPcode.Terminate,
        ], ["Hello"]);
        Assert.Equal("Hello", sim.AcsPrintBuffer);
    }

    [Fact]
    public void EndHudMessage_RecordsHudPayload()
    {
        var sim = Room();
        Run(sim,
        [
            3, 0, (int)AcsPcode.BeginPrint,
            3, 0, (int)AcsPcode.PrintString,
            3, 0, 3, 1, 3, 2, 3, 3, 3, 4, 3, 5, 3, 6,
            (int)AcsPcode.EndHudMessage,
            (int)AcsPcode.Terminate,
        ], ["Ping"]);
        var message = Assert.Single(sim.DrainAcsHudMessages());
        Assert.Equal("Ping", message.Text);
        Assert.True(message.HoldTime > 0);
    }

    [Fact]
    public void PrintBinaryAndHex_UsePrintBuilder()
    {
        var sim = Room();
        Run(sim,
        [
            3, 0, (int)AcsPcode.BeginPrint,
            3, 5, (int)AcsPcode.PrintBinary,
            3, 255, (int)AcsPcode.PrintHex,
            (int)AcsPcode.EndPrint,
            (int)AcsPcode.Terminate,
        ], []);
        Assert.Equal("101FF", sim.AcsPrintBuffer);
    }

    private static AuthoritySimulation Room()
    {
        var level = new PlayLevel
        {
            MapName = "TEST",
            Sectors = [new LevelSector { Index = 0 }],
            Sides = [new LevelSide { Index = 0, Sector = 0 }],
            Lines =
            [
                new LevelLine
                {
                    Index = 0, V1 = 0, V2 = 1, SideFront = 0, SideBack = -1,
                    X1 = 0, Y1 = 0, X2 = 64, Y2 = 0,
                },
            ],
            Vertices = [new LevelVertex { Index = 0 }, new LevelVertex { Index = 1 }],
            Things = [new LevelThing { Type = 1 }],
        };
        return AuthoritySimulation.Start(level);
    }

    private static void Run(AuthoritySimulation sim, int[] words, string[] strings)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = strings });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
    }
}
