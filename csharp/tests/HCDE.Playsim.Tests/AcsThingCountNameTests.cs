using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsThingCountNameTests
{
    [Fact]
    public void ThingCountName_ResolvesDoomImp()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            ],
        });
        sim.Acs.Add(ExitWhen(
        [
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.ThingCountName,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 44,
        ], ["DoomImp"]));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCountName_ReturnsZeroForUnknownClassNames()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 80, Y = 64 }],
        });
        sim.Acs.Add(ExitWhen(
        [
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.ThingCountName,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 44,
        ], ["NotARealClass"]));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ThingCountNameSector_RespectsSectorTag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 80, Y = 64 },
            ],
        });
        sim.Acs.Add(ExitWhen(
        [
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 7,
            (int)AcsPcode.ThingCountNameSector,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 52,
        ], ["DoomImp"]));
        sim.Acs.Enqueue(1);
        sim.Tick();
        Assert.True(sim.Exited);
    }

    private static AcsProgram ExitWhen(int[] leading, string[] stringTable)
    {
        var words = new List<int>(leading)
        {
            (int)AcsPcode.Terminate,
            (int)AcsPcode.Lspec1Direct,
            LineSpecials.ExitNormal,
            0,
        };
        return new AcsProgram { Number = 1, Code = Words(words.ToArray()), StringTable = stringTable };
    }

    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
}
