using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLightingTests
{
    [Theory]
    [InlineData(110, 200, 328)]
    [InlineData(111, 200, -72)]
    [InlineData(112, 35, 35)]
    public void DirectTwoArgumentLightingUsesFullSignedValues(int special, int value, int expected)
    {
        var sim = Room(); Run(sim, 10, special, 7, value, 1);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void DirectFadeAndStopWorkWithoutPlayerActivator()
    {
        var sim = Room(); Run(sim, 11, 113, 7, 228, 4, 1);
        foreach (var light in new[] { 128, 153, 178 }) { sim.Tick(); Assert.Equal(light, sim.LightOf(0)); }
        Run(sim, 9, 117, 7, 1);
        for (var i = 0; i < 10; i++) { sim.Tick(); Assert.Equal(178, sim.LightOf(0)); }
    }

    [Fact]
    public void DirectFourArgumentGlowPreservesArgumentOrder()
    {
        var sim = Room(); Run(sim, 12, 114, 7, 200, 100, 2, 1);
        foreach (var light in new[] { 200, 150, 100, 150 }) { sim.Tick(); Assert.Equal(light, sim.LightOf(0)); }
    }

    [Fact]
    public void DirectFiveArgumentStrobeUsesBothDurations()
    {
        var sim = Room(); Run(sim, 13, 116, 7, 220, 40, 2, 3, 1);
        foreach (var light in new[] { 40, 40, 40, 220, 220, 40 }) { sim.Tick(); Assert.Equal(light, sim.LightOf(0)); }
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    public void TruncatedDirectOperandsNeverApplyPartialAction(int opcode)
    {
        var sim = Room();
        var words = new List<int> { opcode, 112 };
        words.AddRange(new[] { 7, 35, 0, 0, 0 }.Take(opcode - 9)); // One required operand missing.
        Run(sim, words.ToArray());
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void UnsupportedMultiArgumentActionStopsBeforeFollowingMutation()
    {
        var sim = Room(); Run(sim, 10, 999, 7, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void MissingArgumentsAreZeroAndNextInstructionRemainsAligned()
    {
        var sim = Room();
        Run(sim, 9, 112, 7, 10, 110, 7, 20, 1);
        Assert.Equal(20, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
