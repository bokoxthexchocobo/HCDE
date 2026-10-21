namespace HCDE.Scripting.Tests;

public class ZccStackVariableEmitTests
{
    [Fact]
    public void StackOffsetRegister_ZeroMeansDefault() =>
        Assert.Equal(-1, ZccStackVariableEmit.StackOffsetRegister(0));

    [Fact]
    public void StackLoadOpForMember_RewritesLoToLp() =>
        Assert.Equal("OP_LP", ZccStackVariableEmit.StackLoadOpForMember("OP_LO"));

    [Fact]
    public void StructMemberMetaReadOnly_ForMetaFlag() =>
        Assert.True(ZccStackVariableEmit.StructMemberMetaReadOnly(0x10000));

    [Fact]
    public void StructMemberWritable_RequiresScopeBarrier() =>
        Assert.False(ZccStackVariableEmit.StructMemberWritable(true, true, 0, 0, false, false, false));

    [Fact]
    public void StackVariableWritable_RequiresCtx() =>
        Assert.False(ZccStackVariableEmit.StackVariableWritable(true, false, 0, 0));

    [Fact]
    public void StackUseBitLoad_WhenBitSet() =>
        Assert.True(ZccStackVariableEmit.StackUseBitLoad(2));

    [Fact]
    public void StructMemberResolve_RejectsUnreadable() =>
        Assert.False(ZccStackVariableEmit.StructMemberResolveReadable(false));

    [Fact]
    public void StructMemberResolve_StaticReadonlyPromotesGlobal() =>
        Assert.True(ZccStackVariableEmit.StaticReadonlyPromotesToGlobal(0x4 | 0x8));
}
