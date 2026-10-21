namespace HCDE.Scripting.Tests;

public class ZccVmFunctionCallEmitTests
{
    [Fact]
    public void IsStaticCallNative_WhenFinal() =>
        Assert.True(ZccVmFunctionCallEmit.IsStaticCallNative(fnPtrCall: false, isFinal: true, noVirtualIndex: false, noVirtualFlag: false));

    [Fact]
    public void IsDecorateCastKludgeNative_MatchesInternalNames() =>
        Assert.True(ZccVmFunctionCallEmit.IsDecorateCastKludgeNative("__decorate_internal_int__"));
}
