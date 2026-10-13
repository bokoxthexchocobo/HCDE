namespace HCDE.Client.Tests;

public class VmMusicFmBankArgumentsTests
{
    [Theory]
    [InlineData("8", 8)]
    [InlineData("12tail", 12)]
    [InlineData("4294967296", 0)]
    [InlineData("999999999999999999999999", -1)]
    public void AdlNumericPrefixSelectsBuiltinWithoutResolver(string argument, int index)
    {
        var original = new VmMusicAdlConfiguration { Bank = new(true, true, "custom", 14) };
        var result = VmMusicFmBankArguments.ResolveAdl(original, argument, (_, _) => throw new InvalidOperationException());
        Assert.Equal(index, result.Bank.BuiltinBank);
        Assert.False(result.Bank.UseCustomBank);
        Assert.True(result.Bank.UseGenMidi);
        Assert.Equal("custom", original.Bank.CustomBank);
        Assert.True(original.Bank.UseCustomBank);
    }

    [Fact]
    public void NamedBanksUseExactFormatAndResolutionFailureDisablesCustom()
    {
        var adl = VmMusicFmBankArguments.ResolveAdl(new(), "named", (name, format) =>
        { Assert.Equal("named", name); Assert.Equal(VmMusicBankFormat.Wopl, format); return "resolved.wopl"; });
        Assert.Equal(new VmMusicAdlBankConfiguration(true, false, "resolved.wopl", 14), adl.Bank);
        var opn = VmMusicFmBankArguments.ResolveOpn(new(), "123", (name, format) =>
        { Assert.Equal("123", name); Assert.Equal(VmMusicBankFormat.Wopn, format); return null; });
        Assert.Equal(new VmMusicOpnBankConfiguration(false, ""), opn.Bank);
    }

    [Fact]
    public void EmptyArgumentsUseEnabledConfiguredBankAndDisabledBanksRemainUnchanged()
    {
        var adl = new VmMusicAdlConfiguration { Bank = new(true, false, "configured", 14) };
        Assert.Equal("configured", VmMusicFmBankArguments.ResolveAdl(adl, "").Bank.CustomBank);
        var opn = new VmMusicOpnConfiguration { Bank = new(false, "ignored") };
        Assert.Same(opn, VmMusicFmBankArguments.ResolveOpn(opn, null, (_, _) => throw new InvalidOperationException()));
        Assert.Equal(new VmMusicOpnBankConfiguration(true, "override"), VmMusicFmBankArguments.ResolveOpn(opn, "override").Bank);
    }

    [Fact]
    public void ResolverExceptionsPropagate()
    {
        var failure = new InvalidOperationException("resolve failed");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            VmMusicFmBankArguments.ResolveAdl(new(), "named", (_, _) => throw failure)));
    }
}
