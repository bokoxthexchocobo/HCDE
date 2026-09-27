using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGotoStackTests
{
    [Fact]
    public void DecodedModuleJumpTableRelocatesAndExecutes()
    {
        var original = Program(1);
        var chunk = new byte[16]; "JUMP"u8.CopyTo(chunk);
        BinaryPrimitives.WriteInt32LittleEndian(chunk.AsSpan(4), 8);
        for (var i = 0; i < 2; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(chunk.AsSpan(8 + i * 4), (uint)(64 + original.JumpPoints[i]));
        Assert.True(MapBehaviorJumpTableCodec.TryReadChunks(chunk, out var targets, out var parseError), parseError);
        Assert.True(MapBehaviorJumpTableCodec.TryRelocate(targets!, 64, original.Code.Length, out var points, out var bindError), bindError);
        var sim = Room(); sim.Acs.Add(new AcsProgram { Number = 1, Code = original.Code, JumpPoints = points! });
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void IndexSelectsTargetAndPreservesLowerStack(int padding)
    {
        var sim = Room(); var program = Program(padding);
        sim.Acs.Add(program); sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void InvalidIndexStopsBeforeFollowingAction(int index)
    {
        var program = Program(1); BinaryPrimitives.WriteInt32LittleEndian(program.Code.AsSpan(12), index);
        var sim = Room(); sim.Acs.Add(program); sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void EmptyStackStopsSafely()
    {
        var sim = Room(); sim.Acs.Add(new AcsProgram { Number = 1, Code = Words(363, 10, 112, 7, 35, 1) });
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void RegistrationOwnsJumpTable()
    {
        var program = Program(1); var sim = Room(); sim.Acs.Add(program);
        program.JumpPoints[1] = 0;
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(3)]
    public void InvalidTargetRejectsReplacementWithoutChangingRegistration(int target)
    {
        var sim = Room(); sim.Acs.Add(Program(1)); var checksum = sim.Acs.Checksum;
        Assert.Throws<ArgumentOutOfRangeException>(() => sim.Acs.Add(new AcsProgram
            { Number = 1, Code = Words(1), JumpPoints = [target] }));
        Assert.Equal(checksum, sim.Acs.Checksum);
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void RunningFiberRetainsTableWhenProgramIsReplaced()
    {
        var sim = Room(); sim.Acs.Add(Program(1)); sim.Acs.Enqueue(1);
        var replacement = Program(1); replacement.JumpPoints[1] = replacement.JumpPoints[0];
        sim.Acs.Add(replacement); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void TableParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); var changed = Program(1);
        changed.JumpPoints[1] = changed.JumpPoints[0];
        first.Acs.Add(Program(1)); second.Acs.Add(changed);
        Assert.NotEqual(first.Acs.Checksum, second.Acs.Checksum);
    }

    private static AcsProgram Program(int padding)
    {
        var code = new List<byte>(Words(3, 7, 3, 1, 363, 1));
        code.AddRange(new byte[padding]); var target = code.Count;
        code.AddRange(Words(3, 35, 5, 112, 1));
        return new AcsProgram { Number = 1, Code = code.ToArray(), JumpPoints = [20, target] };
    }
    private static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        return bytes;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
