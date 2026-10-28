using HCDE.Scripting;

namespace HCDE.Scripting.Tests;

public class VmPointerArithmeticOpcodeTests
{
    [Fact]
    public void AddARrAdvancesBytePointerIntoArray()
    {
        var memory = new byte[8];
        var basePointer = new VmMemoryPointer(memory, 2);
        var result = VmExec.Run(
            [
                VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Li(2, 3),
                VmCode.Rrr(VmOp.AddARr, 1, 0, 2), VmCode.RetObject(1),
            ],
            [], [], [], objectConstants: [basePointer]);
        Assert.Equal(VmFault.None, result.Fault);
        var advanced = Assert.IsType<VmMemoryPointer>(result.ObjectValue);
        Assert.Equal(5, advanced.ByteOffset);
    }

    [Fact]
    public void AddARkUsesConstantOffsetAndNullPointerStaysNull()
    {
        var memory = new byte[4];
        var result = VmExec.Run(
            [
                VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(VmOp.AddARk, 1, 0, 0),
                VmCode.Rrr(VmOp.Lkp, 2, 1, 0), VmCode.Rrr(VmOp.AddARk, 3, 2, 1),
                VmCode.RetObject(1),
            ],
            [3, 99], [], [], objectConstants: [new VmMemoryPointer(memory, 0), null]);
        Assert.Equal(VmFault.None, result.Fault);
        Assert.Equal(3, Assert.IsType<VmMemoryPointer>(result.ObjectValue).ByteOffset);
    }

    [Fact]
    public void SubAReportsByteDistanceBetweenPointers()
    {
        var memory = new byte[16];
        var left = new VmMemoryPointer(memory, 10);
        var right = new VmMemoryPointer(memory, 3);
        var result = VmExec.Run(
            [
                VmCode.Rrr(VmOp.Lkp, 0, 0, 0), VmCode.Rrr(VmOp.Lkp, 1, 1, 0),
                VmCode.Rrr(VmOp.SubA, 2, 0, 1), VmCode.RetInt(2),
            ],
            [], [], [], objectConstants: [left, right]);
        Assert.Equal(VmFault.None, result.Fault);
        Assert.Equal(7, result.Value);
    }
}
