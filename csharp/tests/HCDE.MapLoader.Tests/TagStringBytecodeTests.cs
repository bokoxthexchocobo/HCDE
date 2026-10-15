using System.Buffers.Binary;
namespace HCDE.MapLoader.Tests;

public class TagStringBytecodeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WalkerRecognizesTagStringWithoutOperands(bool compact)
    {
        byte[] code;
        if (compact) code = [(byte)AcsPcode.TagString, (byte)AcsPcode.Terminate];
        else
        {
            code = new byte[8]; BinaryPrimitives.WriteInt32LittleEndian(code, (int)AcsPcode.TagString);
            BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(4), (int)AcsPcode.Terminate);
        }
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(code, compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld,
            0, out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal((int)AcsPcode.TagString, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandByteCount); Assert.Equal(2, instructions.Count);
    }
}
