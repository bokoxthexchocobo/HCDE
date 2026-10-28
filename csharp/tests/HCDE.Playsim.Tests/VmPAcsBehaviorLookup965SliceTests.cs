using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim.Tests;

public class VmPAcsBehaviorLookup965SliceTests
{
    [Fact]
    public void NormalizeHexenPrintToken_StripsPunctuationAndSpaces()
    {
        Assert.Equal("HELLO", VmPAcsBehaviorLookup965Slice.NormalizeHexenPrintToken("Hello, world!"));
    }

    [Fact]
    public void BuildHexenLocalizationLabel_MatchesNativePattern()
    {
        var label = VmPAcsBehaviorLookup965Slice.BuildHexenLocalizationLabel("MAP33", 6, "You da...");
        Assert.Equal("TXT_ACS_MAP33_6_YOUDA", label);
    }

    [Fact]
    public void ResolveOldLookupString_UsesTranslationWhenPresent()
    {
        var data = new byte[32];
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), 1);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), 16);
        Encoding.ASCII.GetBytes("Hi").CopyTo(data.AsSpan(16));
        data[18] = 0;
        var result = VmPAcsBehaviorLookup965Slice.ResolveOldLookupString(
            data,
            8,
            0,
            shouldLocalize: true,
            forPrint: true,
            "MAP1",
            key => key.StartsWith("TXT_ACS_MAP1_0_") ? "translated" : null);
        Assert.Equal("translated", result);
    }

    [Fact]
    public void ResolveEnhancedLookupString_ReadsRelativeOffset()
    {
        var data = new byte[40];
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), 0);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), 1);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16), 0);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(20), 16);
        Encoding.ASCII.GetBytes("ok").CopyTo(data.AsSpan(8 + 16));
        data[8 + 18] = 0;
        var result = VmPAcsBehaviorLookup965Slice.ResolveEnhancedLookupString(data, 8, 0);
        Assert.Equal("ok", result);
    }

    [Fact]
    public void ResolveTypedScriptTypeName_FallsBackLikeNative()
    {
        Assert.Equal("Enter", VmPAcsBehaviorLookup965Slice.ResolveTypedScriptTypeName(4));
        Assert.Equal("Unknown", VmPAcsBehaviorLookup965Slice.ResolveTypedScriptTypeName(999));
    }

    [Fact]
    public void EnumerateModuleTypedScriptStarts_MatchesTypeAndFlags()
    {
        var scripts = new[]
        {
            new VmPAcsBehaviorLookup965Slice.TypedScriptEntry(4, 10),
            new VmPAcsBehaviorLookup965Slice.TypedScriptEntry(3, 11),
            new VmPAcsBehaviorLookup965Slice.TypedScriptEntry(4, 12),
        };
        var starts = VmPAcsBehaviorLookup965Slice.EnumerateModuleTypedScriptStarts(4, always: true, runNow: false, scripts);
        Assert.Equal(2, starts.Count);
        Assert.All(starts, p => Assert.Equal(VmPAcsBehaviorLookup965Slice.AcsAlways, p.StartFlags));
        Assert.Equal(new[] { 10, 12 }, starts.Select(p => p.ScriptNumber).ToArray());
    }

    [Fact]
    public void CollectStopMyScriptThinkers_OrdersServerThenClient()
    {
        var server = new object();
        var client = new object();
        var thinkers = VmPAcsBehaviorLookup965Slice.CollectStopMyScriptThinkers(server, client);
        Assert.Equal(new[] { server, client }, thinkers);
        Assert.Empty(VmPAcsBehaviorLookup965Slice.CollectStopMyScriptThinkers(null, null));
    }

    [Fact]
    public void ShouldWriteRunningScriptsArray_OnlyWhenWritingWithEntries()
    {
        Assert.True(VmPAcsBehaviorLookup965Slice.ShouldWriteRunningScriptsArray(true, 2));
        Assert.False(VmPAcsBehaviorLookup965Slice.ShouldWriteRunningScriptsArray(false, 2));
        Assert.False(VmPAcsBehaviorLookup965Slice.ShouldWriteRunningScriptsArray(true, 0));
    }

    [Fact]
    public void CollectRunningScriptsForWrite_SkipsNullValues()
    {
        var rows = VmPAcsBehaviorLookup965Slice.CollectRunningScriptsForWrite(
            new Dictionary<int, object?> { [1] = new object(), [2] = null, [3] = new object() });
        Assert.Equal(2, rows.Count);
        Assert.Equal(1, rows[0].ScriptNum);
        Assert.Equal(3, rows[1].ScriptNum);
    }
}
