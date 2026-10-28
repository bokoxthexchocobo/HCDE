namespace HCDE.Scripting.Tests;

public class VmScriptUtilParameterStackTests
{
    [Fact]
    public void AppendFromTypeStream_PushesIntPointerAndFloat()
    {
        var stack = new VmScriptUtilParameterStack();
        stack.AppendFromTypeStream(
            [VmScriptUtilParameterStack.TypeInt, VmScriptUtilParameterStack.TypePointer, VmScriptUtilParameterStack.TypeFloat, VmScriptUtilParameterStack.TypeEnd],
            [42, "ptr", 3.5]);
        Assert.Equal(3, stack.Count);
    }

    [Fact]
    public void Clamp_RestoresStackToParamStart()
    {
        var stack = new VmScriptUtilParameterStack();
        var start = stack.BeginFrame();
        stack.AppendFromTypeStream([VmScriptUtilParameterStack.TypeInt, VmScriptUtilParameterStack.TypeEnd], [1]);
        stack.Clamp(start);
        Assert.Equal(0, stack.Count);
    }
}
