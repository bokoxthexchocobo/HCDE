namespace HCDE.Client.Tests;

public class VmReplayGainConfigurationTests
{
    private static VmReplayGainConfiguration Settings => new()
    {
        FluidPatchset = "fluid.sf2", TimidityConfig = "timidity.cfg", GusConfig = "gus.cfg",
        WildMidiConfig = "wild.cfg", AdlCustomBank = "adl.wopl", AdlBank = 12,
        OpnCustomBank = "opn.wopn", OplCore = 2
    };

    [Theory]
    [InlineData(4, "fluid.sf2")]
    [InlineData(3, "timidity.cfg")]
    [InlineData(5, "gus.cfg")]
    [InlineData(6, "wild.cfg")]
    [InlineData(7, "12")]
    [InlineData(8, "")]
    [InlineData(1, "2")]
    [InlineData(0, "")]
    public void DefaultParameterMatchesNativeSynthSelection(int playerType, string expected)
        => Assert.Equal(expected, Settings.ParameterFor(playerType));

    [Fact]
    public void CustomFmBanksOverrideNumericOrEmptyDefaults()
    {
        var settings = Settings with { AdlUseCustomBank = true, OpnUseCustomBank = true };
        Assert.Equal("adl.wopl", settings.ParameterFor(7));
        Assert.Equal("opn.wopn", settings.ParameterFor(8));
    }

    [Fact]
    public void ConfigurationFeedsDetectedMidiHashButExplicitParameterWins()
    {
        using var reader = new VmMusicByteReader("MThd"u8);
        var settings = Settings with { DefaultDevice = -4 };
        Assert.EndsWith(":5:GUS.CFG", VmReplayGainHash.Create(reader, 4, -1, "", settings));
        Assert.EndsWith(":5:EXPLICIT", VmReplayGainHash.Create(reader, 4, -1, "explicit", settings));
    }
}
