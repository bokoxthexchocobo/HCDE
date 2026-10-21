namespace HCDE.Scripting.Tests;

public class ZccUnarySignCodegenTests
{
    [Fact]
    public void PlusSignOperandAllowed_AcceptsVectors() =>
        Assert.True(ZccUnarySignCodegen.PlusSignOperandAllowed(false, true, false));

    [Fact]
    public void NegateEmitOpcode_PicksVectorOpcode() =>
        Assert.Equal(ZccUnarySignOpcodes.OpNegV3, ZccUnarySignCodegen.NegateEmitOpcode(false, 3));

    [Fact]
    public void MinusSignShouldCastBoolToInt_WhenNonConstantBool() =>
        Assert.True(ZccUnarySignCodegen.MinusSignShouldCastBoolToInt(false, true));

    [Fact]
    public void UnaryNotBoolean_FoldsConstants() =>
        Assert.True(ZccUnarySignCodegen.FoldUnaryNotBoolean(false));
}
