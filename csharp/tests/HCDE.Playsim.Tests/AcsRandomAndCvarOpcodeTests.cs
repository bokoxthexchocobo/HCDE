using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsRandomAndCvarOpcodeTests
{
    [Fact]
    public void RandomDirectUsesNativeAcsStream()
    {
        var sim = Room(rngSeed: 42);
        Run(sim, 1, 58, 1, 6, 26, 0, 3, 7, 29, 0, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void RandomStackOpcodeUsesInclusiveRange()
    {
        var sim = Room(rngSeed: 7);
        Run(sim, 1, 3, 4, 3, 9, (int)AcsPcode.Random, 26, 0, 3, 7, 29, 0, 5, 112, 1);
        Assert.InRange(sim.LightOf(0), 4, 9);
    }

    [Fact]
    public void GetCvarReadsIntegerCvarOntoStack()
    {
        var sim = Room();
        sim.CVars.Set("sv_allowfreelook", ConsoleVariable.IntValue(42));
        sim.Acs.Add(new AcsProgram
        {
            Number = 1,
            Code = Encode(3, 0, (int)AcsPcode.GetCvar, 26, 0, 3, 7, 29, 0, 5, 112, 1, 1),
            StringTable = ["sv_allowfreelook"],
        });
        sim.Acs.TryExecute(1, []);
        sim.Acs.Tick(sim);
        Assert.Equal(42, sim.LightOf(0));
    }

    [Fact]
    public void GetCvarEncodesFloatCvarsAsFixedPoint()
    {
        var sim = Room();
        sim.CVars.Set("sv_gravity", ConsoleVariable.FloatValue(800f));
        var expected = AcsFixedPoint.FromDouble(800d);
        sim.Acs.Add(new AcsProgram
        {
            Number = 1,
            Code = Encode(3, 0, (int)AcsPcode.GetCvar, 3, expected, 19, 26, 0, 3, 7, 29, 0, 5, 112, 1, 1),
            StringTable = ["sv_gravity"],
        });
        sim.Acs.TryExecute(1, []);
        sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void PolyWaitDoesNotStopScript()
    {
        var sim = Room();
        Run(sim, 1, 63, 5, 1);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room(int rngSeed = 1) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
            Format = MapDataFormat.HexenBinary,
        }, rngSeed: rngSeed);

    private static void Run(AuthoritySimulation sim, int number, params int[] words)
    {
        sim.Acs.Add(new AcsProgram { Number = number, Code = Encode(words) });
        Assert.True(sim.Acs.Enqueue(number));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static byte[] Encode(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
}
