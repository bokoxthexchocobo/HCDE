using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLightQueryTests
{
    [Theory]
    [InlineData(340, 0, 160)]
    [InlineData(340, 42, 160)]
    [InlineData(340, 99, 0)]
    [InlineData(281, 7, 160)]
    [InlineData(281, 99, -1)]
    public void LightQueryUsesNativeDefaults(int opcode, int target, int expected)
    {
        var sim = Room(); AssertQuery(sim, opcode, target, expected);
    }

    [Theory]
    [InlineData(340, 0)]
    [InlineData(281, 7)]
    public void QueriesSeeAnimatedLightInsteadOfMapTemplate(int opcode, int target)
    {
        var sim = Room(77); sim.Tick(); Assert.Equal(0, sim.LightOf(0));
        AssertQuery(sim, opcode, target, 0);
        Assert.Equal(160, sim.Level.Sectors[0].LightLevel);
    }

    [Theory]
    [InlineData(340)]
    [InlineData(281)]
    public void MissingArgumentStopsFollowingAction(int opcode)
    {
        var sim = Room(); Run(sim, [opcode, 10, 112, 7, 35, 1]);
        Assert.Equal(160, sim.LightOf(0));
    }

    [Fact]
    public void SectorQueryUsesFirstMatchingSectorAndZeroTag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { LightLevel = 73 }, new LevelSector { Index = 1, LightLevel = 99 }],
        });
        Run(sim, [3, 0, 3, 0, 281, 5, 112, 1]);
        Assert.Equal(73, sim.LightOf(0)); Assert.Equal(73, sim.LightOf(1));
    }

    private static void AssertQuery(AuthoritySimulation sim, int opcode, int target, int expected)
    {
        Run(sim, [3, 7, 3, target, opcode, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }
    private static void Run(AuthoritySimulation sim, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.FirstOrDefault())); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room(short special = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, Special = special, LightLevel = 160, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Id = 42 }],
    });
}
