namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawNumber1083SliceTests
{
    [Fact]
    public void ResolveDrawNumberWhenNotZeroEarlyExit_ClearsStringAtZero()
    {
        var (abort, text) = VmSbarinfoCommandDrawNumber1083Slice.ResolveDrawNumberWhenNotZeroEarlyExit(
            whenNotZero: true,
            drawValue: 0,
            priorDisplayString: "x");
        Assert.True(abort);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void ResolveDrawNumberTranslationBand_SelectsLowAndHigh()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1083Slice.DrawNumberTranslationBand.Low,
            VmSbarinfoCommandDrawNumber1083Slice.ResolveDrawNumberTranslationBand(
                drawValue: 5,
                lowValue: 10,
                highValue: 50));
        Assert.Equal(
            VmSbarinfoCommandDrawNumber1083Slice.DrawNumberTranslationBand.High,
            VmSbarinfoCommandDrawNumber1083Slice.ResolveDrawNumberTranslationBand(
                drawValue: 60,
                lowValue: -1,
                highValue: 50));
    }

    [Fact]
    public void ClampDrawNumberWhenDigitCapped_UsesFillZeroMin()
    {
        Assert.Equal(
            -9,
            VmSbarinfoCommandDrawNumber1083Slice.ClampDrawNumberWhenDigitCapped(
                drawValue: -50,
                length: 2,
                fillZeros: true));
    }

    [Fact]
    public void ApplyDrawNumberDontCapModulo_ForcesFillZerosAtLimit()
    {
        var (value, useFill) = VmSbarinfoCommandDrawNumber1083Slice.ApplyDrawNumberDontCapModulo(
            drawValue: 100,
            length: 2,
            fillZeros: false);
        Assert.Equal(0, value);
        Assert.True(useFill);
    }

    [Fact]
    public void FormatDrawNumberDisplayString_ZeroPadsPositive()
    {
        var formatted = VmSbarinfoCommandDrawNumber1083Slice.FormatDrawNumberDisplayString(
            useFillZeros: true,
            prefixPart: "HP",
            prefixPadding: ":",
            length: 3,
            drawValue: 7);
        Assert.Equal("HP:007", formatted);
    }

    [Fact]
    public void CreateCommandDrawMugShotDefaults_MatchesNativeCtor()
    {
        var (accuracy, flags) = VmSbarinfoCommandDrawMugShot1083Slice.CreateCommandDrawMugShotDefaults();
        Assert.Equal(5, accuracy);
        Assert.Equal(VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotStateFlags.Standard, flags);
    }

    [Fact]
    public void IsCommandDrawMugShotDefaultFaceTooLong_EnforcesThreeChars()
    {
        Assert.False(VmSbarinfoCommandDrawMugShot1083Slice.IsCommandDrawMugShotDefaultFaceTooLong(3));
        Assert.True(VmSbarinfoCommandDrawMugShot1083Slice.IsCommandDrawMugShotDefaultFaceTooLong(4));
    }

    [Fact]
    public void IsCommandDrawMugShotAccuracyInRange_EnforcesOneThroughNine()
    {
        Assert.False(VmSbarinfoCommandDrawMugShot1083Slice.IsCommandDrawMugShotAccuracyInRange(0));
        Assert.True(VmSbarinfoCommandDrawMugShot1083Slice.IsCommandDrawMugShotAccuracyInRange(9));
    }

    [Fact]
    public void ApplyCommandDrawMugShotParseKeyword_StacksNativeBits()
    {
        var flags = VmSbarinfoCommandDrawMugShot1083Slice.ApplyCommandDrawMugShotParseKeyword(
            VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotStateFlags.Standard,
            VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotParseKeyword.XDeathFace);
        flags = VmSbarinfoCommandDrawMugShot1083Slice.ApplyCommandDrawMugShotParseKeyword(
            flags,
            VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotParseKeyword.DisableGrin);
        Assert.Equal(
            VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotStateFlags.XDeathFace
            | VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotStateFlags.DisableGrin,
            flags);
    }

    [Fact]
    public void ClassifyCommandDrawMugShotParseKeyword_RejectsUnknown()
    {
        var (ok, kw) = VmSbarinfoCommandDrawMugShot1083Slice.ClassifyCommandDrawMugShotParseKeyword("notaflag");
        Assert.False(ok);
        Assert.Equal(VmSbarinfoCommandDrawMugShot1083Slice.CommandDrawMugShotParseKeyword.Unknown, kw);
    }
}
