using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class VmPAcsBehaviorStringValidate963SliceTests
{
    [Fact]
    public void CompareScriptNumbers_OrdersAscending()
    {
        Assert.True(VmPAcsBehaviorStringValidate963Slice.CompareScriptNumbers(1, 2) < 0);
        Assert.Equal(0, VmPAcsBehaviorStringValidate963Slice.CompareScriptNumbers(5, 5));
    }

    [Fact]
    public void UnencryptStringChunks_RetagsStreToStrl()
    {
        Span<byte> payload = stackalloc byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(payload, VmPAcsBehaviorStringValidate963Slice.StreFourCc);
        BinaryPrimitives.WriteInt32LittleEndian(payload.Slice(4), 24);
        BinaryPrimitives.WriteInt32LittleEndian(payload.Slice(12), 1);
        BinaryPrimitives.WriteInt32LittleEndian(payload.Slice(20), 0);
        payload[28] = 0x55;

        var data = payload.ToArray();
        var count = VmPAcsBehaviorStringValidate963Slice.UnencryptStringChunks(
            data,
            (_, _) => data,
            _ => null);
        Assert.Equal(1, count);
        Assert.Equal(VmPAcsBehaviorStringValidate963Slice.StrlFourCc, BinaryPrimitives.ReadUInt32LittleEndian(data));
    }

    [Fact]
    public void UnescapeStringTable_UnpaddedTableDecodesEscapes()
    {
        var bytes = new byte[] { 1, 0, 0, 0, 8, 0, 0, 0, (byte)'\\', (byte)'n', 0 };
        VmPAcsBehaviorStringValidate963Slice.UnescapeStringTable(bytes, bytes, hasPadding: false);
        Assert.Equal((byte)'\n', bytes[8]);
    }

    [Fact]
    public void IsGood_RejectsUnknownFormat()
    {
        var check = new VmPAcsBehaviorStringValidate963Slice.BehaviorGoodCheck(
            VmPAcsBehaviorStringValidate963Slice.AcsUnknown,
            Array.Empty<VmPAcsBehaviorStringValidate963Slice.FunctionDef>(),
            Array.Empty<object?>(),
            "TEST",
            null);
        Assert.False(VmPAcsBehaviorStringValidate963Slice.IsGood(in check));
    }

    [Fact]
    public void IsGood_RejectsMissingImport()
    {
        var check = new VmPAcsBehaviorStringValidate963Slice.BehaviorGoodCheck(
            1,
            new[] { new VmPAcsBehaviorStringValidate963Slice.FunctionDef(1, 0) },
            new object?[] { null },
            "TEST",
            null);
        Assert.False(VmPAcsBehaviorStringValidate963Slice.IsGood(in check));
    }

    [Fact]
    public void FindScriptIndex_PrefersEarlierDuplicate()
    {
        var scripts = new[]
        {
            new VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot(1, 10),
            new VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot(2, 20),
            new VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot(2, 30),
        };
        var idx = VmPAcsBehaviorStringValidate963Slice.FindScriptIndex(scripts, 2);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void FindScriptModuleIndex_ScansModulesInOrder()
    {
        var modules = new IReadOnlyList<VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot>[]
        {
            new[] { new VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot(1, 1) },
            new[] { new VmPAcsBehaviorStringValidate963Slice.ScriptPtrSlot(9, 9) },
        };
        Assert.Equal(1, VmPAcsBehaviorStringValidate963Slice.FindScriptModuleIndex(modules, 9));
    }
}
