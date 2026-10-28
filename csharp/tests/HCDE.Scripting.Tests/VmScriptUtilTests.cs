namespace HCDE.Scripting.Tests;

public class VmScriptUtilTests
{
    [Fact]
    public void Exec_CachesResolvedFunctionAndReturnsVmResult()
    {
        var lookups = 0;
        var util = new VmScriptUtil(name =>
        {
            lookups++;
            return new VmFunction((_, requested) =>
            {
                Assert.Equal(1, requested);
                return new[] { VmValue.FromInteger(99) };
            }, "ScriptUtil.Test");
        });

        var value = util.Exec(
            "Test",
            [VmScriptUtilParameterStack.TypeInt, VmScriptUtilParameterStack.TypeEnd],
            [7]);
        Assert.Equal(99, value);
        util.Exec(
            "Test",
            [VmScriptUtilParameterStack.TypeInt, VmScriptUtilParameterStack.TypeEnd],
            [3]);
        Assert.Equal(1, lookups);
    }

    [Fact]
    public void Exec_UndefinedFunctionThrowsLikeNative()
    {
        var util = new VmScriptUtil(_ => null);
        Assert.Throws<InvalidOperationException>(() => util.Exec(
            "Missing",
            [VmScriptUtilParameterStack.TypeEnd],
            Array.Empty<object?>()));
    }

    [Fact]
    public void Clear_DropsCacheAndParameters()
    {
        var util = new VmScriptUtil(_ => new VmFunction((_, _) => new[] { VmValue.FromInteger(1) }, "x"));
        util.Exec("x", [VmScriptUtilParameterStack.TypeEnd], Array.Empty<object?>());
        util.Clear();
        util.Exec("x", [VmScriptUtilParameterStack.TypeEnd], Array.Empty<object?>());
    }
}
