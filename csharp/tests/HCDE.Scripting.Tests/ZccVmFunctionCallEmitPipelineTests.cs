namespace HCDE.Scripting.Tests;

public class ZccVmFunctionCallEmitPipelineTests
{
    [Fact]
    public void RunEmitProloguePipeline_AllowsScopeBarrier()
    {
        var input = new ZccVmFunctionCallEmit.VmFunctionCallEmitInput(
            0, false, false, false, false, 3, 0, 1, 3, 1, "fn", false);
        Assert.True(ZccVmFunctionCallEmit.RunEmitProloguePipeline(input));
    }
}
