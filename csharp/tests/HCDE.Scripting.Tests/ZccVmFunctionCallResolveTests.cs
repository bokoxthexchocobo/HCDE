namespace HCDE.Scripting.Tests;

public class ZccVmFunctionCallResolveTests
{
    [Fact]
    public void RequiresSelfPointerNative_WhenMethodWithoutSelf() =>
        Assert.True(ZccVmFunctionCallResolve.RequiresSelfPointerNative(isMethod: true, self: null));

    [Fact]
    public void TooManyArgumentsNative_WhenOverProto() =>
        Assert.True(ZccVmFunctionCallResolve.TooManyArgumentsNative(5, implicitArgs: 1, protoArgCount: 4, isVarArg: false));

    [Fact]
    public void RelaxedNamedArgsNative_At413() =>
        Assert.True(ZccVmFunctionCallResolve.RelaxedNamedArgsNative(4, 13));
}
