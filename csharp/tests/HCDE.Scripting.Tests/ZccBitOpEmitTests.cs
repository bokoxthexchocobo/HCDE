namespace HCDE.Scripting.Tests;

public class ZccBitOpEmitTests
{
    [Fact]
    public void BaseInstruction_MapsBitwiseOperators()
    {
        Assert.Equal(ZccBitOpEmit.OpAndRr, ZccBitOpEmit.BaseInstruction('&'));
        Assert.Equal(ZccBitOpEmit.OpXorRr, ZccBitOpEmit.BaseInstruction('^'));
    }

    [Fact]
    public void EmitOpcode_AddsOneWhenRightKonst() =>
        Assert.Equal(ZccBitOpEmit.OpOrRr + 1, ZccBitOpEmit.EmitOpcode(ZccBitOpEmit.OpOrRr, true));
}
