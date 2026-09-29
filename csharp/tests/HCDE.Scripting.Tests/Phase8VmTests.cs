using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class Phase8VmTests
{
    [Fact]
    public void Multiply_Returns42()
    {
        var code = new[]
        {
            VmCode.Li(1, 6),
            VmCode.Li(2, 7),
            VmCode.Rrr(VmOp.MulRr, 0, 1, 2),
            VmCode.RetInt(0),
        };

        var result = VmExec.Run(code, []);

        Assert.Equal(new VmResult(true, 42, VmFault.None), result);
    }

    [Fact]
    public void DivideByZero_Stops()
    {
        var code = new[]
        {
            VmCode.Li(0, 1),
            VmCode.Li(1, 0),
            VmCode.Rrr(VmOp.DivRr, 2, 0, 1),
            VmCode.RetInt(2),
        };

        var result = VmExec.Run(code, []);

        Assert.Equal(VmFault.DivisionByZero, result.Fault);
        Assert.False(result.Returned);
    }

    [Fact]
    public void Equal_JumpsOverTheFallThrough()
    {
        var code = new[]
        {
            VmCode.Li(0, 3),
            VmCode.Li(1, 3),
            VmCode.Cmp(VmOp.EqR, 1, 0, 1),
            VmCode.Jmp(1),
            VmCode.Reti(0),
            VmCode.Reti(1),
        };

        Assert.Equal(new VmResult(true, 1, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void LessThan_FallsThroughWhenEqual()
    {
        var code = new[]
        {
            VmCode.Li(0, 5),
            VmCode.Li(1, 5),
            VmCode.Cmp(VmOp.LtRr, 1, 0, 1),
            VmCode.Jmp(1),
            VmCode.Reti(0),
            VmCode.Reti(1),
        };

        Assert.Equal(new VmResult(true, 0, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void AddConstant_AndImmediate()
    {
        var code = new[]
        {
            VmCode.Li(0, 2),
            VmCode.Rrr(VmOp.AddRk, 1, 0, 0),
            VmCode.Addi(2, 1, -2),
            VmCode.RetInt(2),
        };

        Assert.Equal(new VmResult(true, 40, VmFault.None), VmExec.Run(code, [40]));
    }

    [Fact]
    public void Loop_CountsToThree()
    {
        var code = new[]
        {
            VmCode.Li(0, 0),
            VmCode.Li(1, 1),
            VmCode.Rrr(VmOp.AddRr, 0, 0, 1),
            VmCode.Li(2, 3),
            VmCode.Cmp(VmOp.EqR, 1, 0, 2),
            VmCode.Jmp(1),
            VmCode.Jmp(-5),
            VmCode.RetInt(0),
        };

        Assert.Equal(new VmResult(true, 3, VmFault.None), VmExec.Run(code, []));
    }

    [Fact]
    public void Shifts_KeepTheSignOnlyForArithmetic()
    {
        var arithmetic = new[]
        {
            VmCode.Li(0, -8),
            VmCode.Rrr(VmOp.SraRi, 1, 0, 1),
            VmCode.RetInt(1),
        };
        var logical = new[]
        {
            VmCode.Li(0, -1),
            VmCode.Rrr(VmOp.SrlRi, 1, 0, 1),
            VmCode.RetInt(1),
        };

        Assert.Equal(new VmResult(true, -4, VmFault.None), VmExec.Run(arithmetic, []));
        Assert.Equal(new VmResult(true, int.MaxValue, VmFault.None), VmExec.Run(logical, []));
    }

    [Fact]
    public void UnknownOpcode_Stops()
    {
        var result = VmExec.Run([0xFF], []);

        Assert.Equal(VmFault.UnknownOpcode, result.Fault);
        Assert.False(result.Returned);
    }
}
