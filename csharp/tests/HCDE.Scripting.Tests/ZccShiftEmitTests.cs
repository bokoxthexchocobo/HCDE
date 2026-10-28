namespace HCDE.Scripting.Tests;

public class ZccShiftEmitTests
{
    [Fact]
    public void SelectInstruction_UsesImmediateColumnWhenRightIsImmediate()
    {
        Assert.Equal(2, ZccShiftEmit.SelectInstruction(0, false, true));
    }

    [Fact]
    public void SelectInstruction_UsesKrColumnWhenLeftKonst() =>
        Assert.Equal(ZccShiftEmit.InstrMap[1][1], ZccShiftEmit.SelectInstruction(1, true, false));
}
