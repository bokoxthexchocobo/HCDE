namespace HCDE.Playsim.Tests;

public class VmScopeBarrier1207SliceTests
{
    [Fact]
    public void Sides_MapFlagsAndBlockUiReads()
    {
        Assert.Equal(VmScopeBarrier1207Slice.SideUi, VmScopeBarrier1207Slice.SideFromFlags(VmScopeBarrier1207Slice.VarfUi | VmScopeBarrier1207Slice.VarfPlay));
        Assert.Equal(VmScopeBarrier1207Slice.SideVirtual, VmScopeBarrier1207Slice.SideFromFlags(VmScopeBarrier1207Slice.VarfVirtualScope));
        Assert.Equal(VmScopeBarrier1207Slice.SideClear, VmScopeBarrier1207Slice.SideFromFlags(VmScopeBarrier1207Slice.VarfClearScope));
        Assert.Equal(VmScopeBarrier1207Slice.SidePlain, VmScopeBarrier1207Slice.SideFromFlags(0));
        Assert.Equal(VmScopeBarrier1207Slice.SideUi, VmScopeBarrier1207Slice.SideFromObjectFlags(VmScopeBarrier1207Slice.ScopeUi | VmScopeBarrier1207Slice.ScopePlay));
        Assert.Equal(0, VmScopeBarrier1207Slice.FlagsFromSide(99));
        Assert.Equal(VmScopeBarrier1207Slice.ScopeAll, VmScopeBarrier1207Slice.ObjectFlagsFromSide(VmScopeBarrier1207Slice.SideVirtual));
        Assert.Equal("virtualscope", VmScopeBarrier1207Slice.StringFromSide(VmScopeBarrier1207Slice.SideVirtual));
        Assert.Equal("clearscope", VmScopeBarrier1207Slice.StringFromSide(VmScopeBarrier1207Slice.SideClear));
        Assert.Equal("unknown", VmScopeBarrier1207Slice.StringFromSide(9));

        int kept = VmScopeBarrier1207Slice.VarfReadOnly | VmScopeBarrier1207Slice.VarfUi | VmScopeBarrier1207Slice.VarfNoRollback | 1;
        Assert.Equal(VmScopeBarrier1207Slice.VarfReadOnly | VmScopeBarrier1207Slice.VarfPlay | 1, VmScopeBarrier1207Slice.ChangeSideInFlags(kept, VmScopeBarrier1207Slice.SidePlay));
        Assert.Equal(VmScopeBarrier1207Slice.ScopePlay | 4, VmScopeBarrier1207Slice.ChangeSideInObjectFlags(VmScopeBarrier1207Slice.ScopeUi | 4, VmScopeBarrier1207Slice.SidePlay));
        Assert.Equal(0, VmScopeBarrier1207Slice.ChangeSideInObjectFlags(VmScopeBarrier1207Slice.ScopeUi, VmScopeBarrier1207Slice.SidePlain));

        var same = new VmScopeBarrier1207Slice(VmScopeBarrier1207Slice.VarfPlay, VmScopeBarrier1207Slice.VarfPlay, "hp");
        Assert.True(same.Readable && same.Writable && same.Callable);
        Assert.Equal("", same.ReadError);

        var blocked = new VmScopeBarrier1207Slice(VmScopeBarrier1207Slice.VarfPlay, VmScopeBarrier1207Slice.VarfUi, "hud");
        Assert.False(blocked.Readable || blocked.Writable || blocked.Callable);
        Assert.Equal("Can't read ui field hud from play context", blocked.ReadError);
        Assert.Contains("(not readable)", blocked.WriteError);
        Assert.Contains("(not readable)", blocked.CallError);

        var readOnly = new VmScopeBarrier1207Slice(VmScopeBarrier1207Slice.VarfUi, VmScopeBarrier1207Slice.VarfPlay | VmScopeBarrier1207Slice.VarfReadOnly, "query");
        Assert.True(readOnly.Readable && readOnly.Callable);
        Assert.False(readOnly.Writable);
        Assert.Equal("Can't write play field query from ui context", readOnly.WriteError);
        Assert.Equal("", readOnly.CallError);
    }

    [Fact]
    public void Chain_FreezesAfterTheFirstBanAndConstructionChecksTheClassSide()
    {
        var chain = new VmScopeBarrier1207Slice();
        chain.AddFlags(VmScopeBarrier1207Slice.VarfPlay, 0, "plain");
        chain.AddFlags(VmScopeBarrier1207Slice.VarfPlay, VmScopeBarrier1207Slice.VarfUi, "nested");
        Assert.False(chain.Readable);
        Assert.Contains("nested", chain.ReadError);
        chain.AddFlags(VmScopeBarrier1207Slice.VarfPlay, VmScopeBarrier1207Slice.VarfPlay, "later");
        Assert.DoesNotContain("later", chain.ReadError);

        var quiet = new VmScopeBarrier1207Slice(VmScopeBarrier1207Slice.VarfPlay, VmScopeBarrier1207Slice.VarfUi, null);
        Assert.False(quiet.Readable);
        Assert.Equal("", quiet.ReadError);

        Assert.True(VmScopeBarrier1207Slice.CheckSidesForFunctionPointer(VmScopeBarrier1207Slice.SideUi, -1));
        Assert.True(VmScopeBarrier1207Slice.CheckSidesForFunctionPointer(VmScopeBarrier1207Slice.SideClear, VmScopeBarrier1207Slice.SideUi));
        Assert.False(VmScopeBarrier1207Slice.CheckSidesForFunctionPointer(VmScopeBarrier1207Slice.SideUi, VmScopeBarrier1207Slice.SidePlay));
        Assert.True(VmScopeBarrier1207Slice.CheckSidesForFunctionPointer(VmScopeBarrier1207Slice.SidePlain, VmScopeBarrier1207Slice.SidePlay));

        var invalid = Assert.Throws<VmScopeBarrier1207Slice.ScriptError>(() => VmScopeBarrier1207Slice.ValidateNew(false, "Bad", 0, VmScopeBarrier1207Slice.SidePlain));
        Assert.Equal("VM execution aborted: Cannot instantiate invalid class Bad", invalid.Detail);
        VmScopeBarrier1207Slice.ValidateNew(true, "Data", 0, VmScopeBarrier1207Slice.SideUi);
        var construct = Assert.Throws<VmScopeBarrier1207Slice.ScriptError>(() => VmScopeBarrier1207Slice.ValidateNew(true, "Hud", VmScopeBarrier1207Slice.ScopeUi, VmScopeBarrier1207Slice.SidePlain));
        Assert.Equal("VM execution aborted: Cannot construct ui class Hud from data context", construct.Detail);
        Assert.Throws<NullReferenceException>(() => VmScopeBarrier1207Slice.ValidateCall(false, VmScopeBarrier1207Slice.ScopePlay, "Tick", VmScopeBarrier1207Slice.SideUi));
        var call = Assert.Throws<VmScopeBarrier1207Slice.ScriptError>(() => VmScopeBarrier1207Slice.ValidateCall(true, VmScopeBarrier1207Slice.ScopePlay, "Tick", VmScopeBarrier1207Slice.SideUi));
        Assert.Equal("VM execution aborted: Cannot call play function Tick from ui context", call.Detail);
        VmScopeBarrier1207Slice.ValidateCall(true, VmScopeBarrier1207Slice.ScopePlay, "Tick", VmScopeBarrier1207Slice.SidePlay);
    }
}
