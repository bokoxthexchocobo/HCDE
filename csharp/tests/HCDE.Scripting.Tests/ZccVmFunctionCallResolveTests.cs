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

    [Fact]
    public void RunResolveGuardPipeline_FlagsMissingSelf()
    {
        var input = new ZccVmFunctionCallResolve.VmFunctionCallGuardInput(
            true, null, false, false, 0, 0, 4, false, false, false, 4, 13,
            Array.Empty<string>(), "", -1, 0, false);
        Assert.Equal(
            ZccVmFunctionCallResolve.VmFunctionCallGuardFailure.MissingSelf,
            ZccVmFunctionCallResolve.RunResolveGuardPipeline(input));
    }
}
