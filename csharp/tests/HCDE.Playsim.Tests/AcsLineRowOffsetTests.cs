using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLineRowOffsetTests
{
    [Theory]
    [InlineData(12.75, false, 12)]
    [InlineData(-12.75, false, -12)]
    [InlineData(12.75, true, 12)]
    [InlineData(-12.75, true, -12)]
    public void QueryUsesFrontMiddleOffsetAndTruncatesTowardZero(double offset, bool back, int expected)
    {
        var sim = Room(offset);
        sim.Acs.Add(Program(1, Query(expected)));
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideFront = 0, SideBack = 1 }, backSide: back));
        sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(2)]
    public void MissingOrInvalidFrontSideReturnsZero(int? side)
    {
        var sim = Room(12.75); sim.Acs.Add(Program(1, Query(0)));
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: side.HasValue ? new LevelLine { SideFront = side.Value } : null));
        sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void NestedScriptInheritsMapTrigger()
    {
        var sim = Room(12.75);
        sim.Acs.Add(Program(1, [13, 226, 2, 0, 0, 0, 0, 1]));
        sim.Acs.Add(Program(2, Query(12)));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(),
            new LevelLine { Special = 80, Arg0 = 1, PlayerUse = true, SideFront = 0, SideBack = 1 }, true));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void ResumedScriptKeepsOriginalFrontSide()
    {
        var sim = Room(12.75); sim.Acs.Add(Program(1, [2, .. Query(12)]));
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideFront = 0 }));
        sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(1, [], triggerLine: new LevelLine { SideFront = 1 }));
        sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void OffsetParticipatesInSimulationChecksum()
    {
        Assert.NotEqual(Room(12.75).Checksum, Room(12.5).Checksum);
    }

    private static int[] Query(int expected) => [3, 7, 258, 3, expected, 19, 5, 112, 1];

    private static AcsProgram Program(int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return new AcsProgram { Number = number, Code = bytes };
    }

    private static AuthoritySimulation Room(double offset) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Sides = [new LevelSide { MidTextureOffsetY = offset }, new LevelSide { Index = 1, MidTextureOffsetY = 99 }],
    });
}
