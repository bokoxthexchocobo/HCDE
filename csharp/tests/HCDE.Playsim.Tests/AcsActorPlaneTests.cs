using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorPlaneTests
{
    [Theory]
    [InlineData(259, -81920)]
    [InlineData(282, 8437760)]
    public void PlaneQueriesUseFractionalSectorHeights(int opcode, int expected)
    {
        var sim = Room(); AssertQuery(sim, opcode, expected);
    }

    [Theory]
    [InlineData(0u, 0)]
    [InlineData(0x40000000u, 16384)]
    [InlineData(0x80000000u, 32768)]
    [InlineData(0xc0000000u, 49152)]
    [InlineData(0xffffffffu, 65535)]
    [InlineData(0x12345678u, 4660)]
    public void AngleReturnsUnsignedAcsTurns(uint raw, int expected)
    {
        var sim = Room(); sim.Players.Single().Angle = new BamAngle(raw);
        AssertQuery(sim, 260, expected);
    }

    [Fact]
    public void FloorQuerySeesCompletedMovement()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
            { Special = 23, Arg0 = 7, Arg1 = 8, Arg2 = 2, PlayerUse = true }, true));
        sim.Tick(); sim.Tick(); AssertQuery(sim, 259, 49152);
    }

    [Theory]
    [InlineData(259)]
    [InlineData(260)]
    [InlineData(282)]
    public void MissingActorReturnsZero(int opcode)
    {
        var sim = Room(); AssertQuery(sim, opcode, 0, tid: 999);
    }

    private static void AssertQuery(AuthoritySimulation sim, int opcode, int expected, int tid = 0)
    {
        int[] words = [3, 7, 3, tid, opcode, 3, expected, 19, 5, 112, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, FloorHeight = -1.25, CeilingHeight = 128.75, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
