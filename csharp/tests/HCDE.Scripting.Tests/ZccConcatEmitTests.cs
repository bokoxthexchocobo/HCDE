namespace HCDE.Scripting.Tests;

public class ZccConcatEmitTests
{
    [Fact]
    public void SelectCastOpcode_IntOperand_UsesCastI2s() =>
        Assert.Equal(ZccConcatEmit.CastI2s,
            ZccConcatEmit.SelectCastOpcode(ZccConcatEmit.RegtInt, 1, false, false, false, false, false, false, false));

    [Fact]
    public void SelectCastOpcode_FloatScalar_UsesCastF2s() =>
        Assert.Equal(ZccConcatEmit.CastF2s,
            ZccConcatEmit.SelectCastOpcode(ZccConcatEmit.RegtFloat, 1, false, false, false, false, false, false, false));

    [Fact]
    public void SelectCastOpcode_TranslationId_UsesCastU2s() =>
        Assert.Equal(ZccConcatEmit.CastU2s,
            ZccConcatEmit.SelectCastOpcode(ZccConcatEmit.RegtInt, 1, false, false, false, false, false, false, true));

    [Fact]
    public void UseStringKonstReload_WhenStringKonst() =>
        Assert.True(ZccConcatEmit.UseStringKonstReload(ZccConcatEmit.RegtString, true));
}
