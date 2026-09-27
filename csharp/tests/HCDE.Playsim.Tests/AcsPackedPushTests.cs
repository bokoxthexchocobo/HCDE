using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPackedPushTests
{
    [Theory]
    [InlineData(167, 1)]
    [InlineData(176, 2)]
    [InlineData(177, 3)]
    [InlineData(178, 4)]
    [InlineData(179, 5)]
    [InlineData(175, 0)]
    [InlineData(175, 1)]
    [InlineData(175, 5)]
    [InlineData(175, 255)]
    public void PushesUnsignedBytesInOrderAndContinuesAtExactByteOffset(int opcode, int count)
    {
        var code = new List<byte>(); Word(code, 3); Word(code, 7); Word(code, opcode);
        if (opcode == 175) code.Add((byte)count);
        var values = Enumerable.Range(0, count).Select(i => (byte)(255 - i)).ToArray();
        code.AddRange(values);
        // Pop values in reverse order; any mismatch skips to a failing light assignment.
        var branches = new List<int>();
        foreach (var value in values.AsEnumerable().Reverse())
        {
            Word(code, 3); Word(code, value); Word(code, 19); Word(code, 79);
            branches.Add(code.Count); Word(code, 0);
        }
        Word(code, 3); Word(code, 1); Word(code, 5); Word(code, 112); Word(code, 1);
        var failure = code.Count; Word(code, 10); Word(code, 112); Word(code, 7); Word(code, 0); Word(code, 1);
        var bytes = code.ToArray();
        foreach (var branch in branches) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(branch), failure);
        var sim = Room(); Start(sim, bytes);
        for (var i = 0; i < 40 && sim.Acs.RunningCount > 0; i++) sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(167)]
    [InlineData(175)]
    [InlineData(176)]
    [InlineData(177)]
    [InlineData(178)]
    [InlineData(179)]
    public void TruncatedOperandsTerminateWithoutMutation(int opcode)
    {
        var code = new List<byte>(); Word(code, opcode);
        var sim = Room(); Start(sim, code.ToArray()); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void TruncatedCountedPayloadTerminates()
    {
        var code = new List<byte>(); Word(code, 175); code.AddRange([3, 1, 2]);
        var sim = Room(); Start(sim, code.ToArray()); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Word(List<byte> code, int value)
    {
        var bytes = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value); code.AddRange(bytes);
    }
    private static void Start(AuthoritySimulation sim, byte[] bytes)
    {
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); Assert.True(sim.Acs.TryExecute(1, []));
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
    });
}
