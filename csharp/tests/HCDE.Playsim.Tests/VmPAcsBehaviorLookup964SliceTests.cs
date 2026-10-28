using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim.Tests;

public class VmPAcsBehaviorLookup964SliceTests
{
    [Fact]
    public void ResolveGetFunction_LocalFunction()
    {
        var result = VmPAcsBehaviorLookup964Slice.ResolveGetFunction(
            0, 1, new VmPAcsBehaviorLookup964Slice.ScriptFunctionDef(42, 0), Array.Empty<object?>(), null);
        Assert.True(result.Found);
        Assert.True(result.SelfModule);
        Assert.Equal(42, result.ResolvedAddress);
    }

    [Fact]
    public void ResolveGetFunction_ImportRedirect()
    {
        var result = VmPAcsBehaviorLookup964Slice.ResolveGetFunction(
            0,
            1,
            new VmPAcsBehaviorLookup964Slice.ScriptFunctionDef(7, 1),
            new object?[] { new object() },
            (_, _) => new VmPAcsBehaviorLookup964Slice.GetFunctionResult(true, 0, 99, false));
        Assert.True(result.Found);
        Assert.Equal(99, result.ResolvedAddress);
    }

    [Fact]
    public void FindStringInChunk_MatchesName()
    {
        var chunk = new byte[20];
        BinaryPrimitives.WriteInt32LittleEndian(chunk.AsSpan(8), 1);
        BinaryPrimitives.WriteInt32LittleEndian(chunk.AsSpan(12), 0);
        Encoding.ASCII.GetBytes("foo").CopyTo(chunk.AsSpan(8 + 0));
        chunk[11] = 0;
        Assert.Equal(0, VmPAcsBehaviorLookup964Slice.FindStringInChunk(chunk, "FOO"));
    }

    [Fact]
    public void GetArrayVal_ReturnsZeroWhenOutOfRange()
    {
        var arrays = new VmPAcsBehaviorLookup964Slice.ArrayInfo?[]
        {
            new(2, new[] { 5, 6 }),
        };
        Assert.Equal(0, VmPAcsBehaviorLookup964Slice.GetArrayVal(0, 5, 1, arrays));
        Assert.Equal(6, VmPAcsBehaviorLookup964Slice.GetArrayVal(0, 1, 1, arrays));
    }

    [Fact]
    public void CopyStringToArray_WritesTerminator()
    {
        var arrays = new VmPAcsBehaviorLookup964Slice.ArrayInfo?[]
        {
            new(8, new int[8]),
        };
        Assert.True(VmPAcsBehaviorLookup964Slice.CopyStringToArray(0, 0, 4, "hi", 1, arrays));
        Assert.Equal('h', arrays[0]!.Value.Elements[0]);
        Assert.Equal('i', arrays[0]!.Value.Elements[1]);
        Assert.Equal(0, arrays[0]!.Value.Elements[2]);
    }

    [Fact]
    public void FindChunkOffset_WalksLinkedChunks()
    {
        var data = new byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0), 0x12345678);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(16), 0x12345678);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(20), 4);
        Assert.Equal(0, VmPAcsBehaviorLookup964Slice.FindChunkOffset(data, data.Length, 0x12345678));
        Assert.Equal(16, VmPAcsBehaviorLookup964Slice.NextChunkOffset(data, data.Length, 0));
    }

    [Fact]
    public void LookupUsesStringPool_DetectsPoolIndex()
    {
        var index = (uint)VmPAcsBehaviorLookup964Slice.StrpoolLibraryId << VmPAcsBehaviorLookup964Slice.LibraryIdShift;
        Assert.True(VmPAcsBehaviorLookup964Slice.LookupUsesStringPool(index));
    }

    [Fact]
    public void OldFormatStringInRange_RequiresValidTable()
    {
        var data = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), 3);
        Assert.True(VmPAcsBehaviorLookup964Slice.OldFormatStringInRange(
            VmPAcsBehaviorLookup964Slice.AcsOld, 8, 2, data));
        Assert.False(VmPAcsBehaviorLookup964Slice.OldFormatStringInRange(
            VmPAcsBehaviorLookup964Slice.AcsOld, 8, 3, data));
    }
}
