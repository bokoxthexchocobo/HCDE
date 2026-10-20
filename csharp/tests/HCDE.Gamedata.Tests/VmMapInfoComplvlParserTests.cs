namespace HCDE.Gamedata.Tests;

public class VmMapInfoComplvlParserTests
{
    [Fact]
    public void TryParseComplvl_VanillaPreset()
    {
        Assert.True(VmMapInfoComplvlParser.TryParseComplvl("vanilla", out var flags));
        Assert.Equal(2_853_780_231u, flags.Flags1);
        Assert.Equal(16_650u, flags.Flags2);
    }

    [Fact]
    public void TryParseComplvl_BoomPreset()
    {
        Assert.True(VmMapInfoComplvlParser.TryParseComplvl("boom", out var flags));
        Assert.Equal(2_148_116_480u, flags.Flags1);
    }

    [Fact]
    public void TryParseComplvl_RejectsUnknown()
    {
        Assert.False(VmMapInfoComplvlParser.TryParseComplvl("zdoom", out _));
    }
}
