namespace HCDE.Gamedata.Tests;

public class VmXlatExpressionEvalTests
{
    [Fact]
    public void Eval_AddsAndUsesTag()
    {
        var state = new VmXlatExprState { Tag = 7 };
        Assert.Equal(9, VmXlatExpressionEval.Eval(2, 4, 5, state));
        Assert.Equal(7, VmXlatExpressionEval.Eval(1, 0, 0, state));
    }
}
