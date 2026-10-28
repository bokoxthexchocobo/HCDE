using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class VmIntegerArithmeticOpcodeTests
{
    [Theory]
    [InlineData(VmOp.AddRr, 10, 7, 17)]
    [InlineData(VmOp.SubRr, 10, 7, 3)]
    [InlineData(VmOp.MulRr, 6, 7, 42)]
    public void IntegerBinaryOpsWriteRegister(VmOp op, int left, int right, int expected)
    {
        var function = new VmScriptFunction(
            [VmCode.Rrr(op, 2, 0, 1), VmCode.RetInt(2)],
            [], [VmValueType.Integer, VmValueType.Integer]);
        var result = VmExec.RunFunction(function, [VmValue.FromInteger(left), VmValue.FromInteger(right)]);
        Assert.Equal(expected, result.Values[0].Value);
    }

    [Fact]
    public void EqRBranchesWhenRegistersMatch()
    {
        var code = new[]
        {
            VmCode.Li(0, 5), VmCode.Li(1, 5),
            VmCode.Cmp(VmOp.EqR, 1, 0, 1), VmCode.Jmp(1), VmCode.Reti(0), VmCode.Reti(1),
        };
        Assert.Equal(new VmResult(true, 1, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void LtRrBranchesOnOrderedIntegers()
    {
        var code = new[]
        {
            VmCode.Li(0, 2), VmCode.Li(1, 9),
            VmCode.Cmp(VmOp.LtRr, 1, 0, 1), VmCode.Jmp(1), VmCode.Reti(0), VmCode.Reti(1),
        };
        Assert.Equal(new VmResult(true, 1, VmFault.None), VmExec.Run(code, []));
    }
}
