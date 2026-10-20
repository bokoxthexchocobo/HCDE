namespace HCDE.Client.Tests;

public class VmConfigFileTests
{
    [Fact]
    public void ReadConfig_ParsesSectionAndKey()
    {
        var config = new VmConfigFile();
        config.LoadFromText("[General]\nfoo=bar\n");
        Assert.True(config.SetSection("General"));
        Assert.Equal("bar", config.GetValueForKey("foo"));
    }

    [Fact]
    public void WriteConfig_RoundTripsSingleLineValue()
    {
        var config = new VmConfigFile();
        config.LoadFromText("[General]\nfoo=bar\n");
        var text = config.WriteToText();
        var roundTrip = new VmConfigFile();
        roundTrip.LoadFromText(text);
        Assert.True(roundTrip.SetSection("General"));
        Assert.Equal("bar", roundTrip.GetValueForKey("foo"));
    }
}
