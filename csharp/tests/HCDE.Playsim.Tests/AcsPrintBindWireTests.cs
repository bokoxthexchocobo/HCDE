using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPrintBindWireTests
{
    [Fact]
    public void PrintBindWire_AppendsUnknownCommandPlaceholder()
    {
        var sim = Room();
        Run(sim,
        [
            3, 0, (int)AcsPcode.BeginPrint,
            3, 0, AcsPcodeWire.PrintBind,
            (int)AcsPcode.EndPrint,
            (int)AcsPcode.Terminate,
        ], ["+forward"]);
        Assert.Contains("??? (+forward)", sim.AcsPrintBuffer, StringComparison.Ordinal);
    }

    private static AuthoritySimulation Room()
    {
        var level = new PlayLevel
        {
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
