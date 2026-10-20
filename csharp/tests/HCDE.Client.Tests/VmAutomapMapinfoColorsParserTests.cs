namespace HCDE.Client.Tests;

public class VmAutomapMapinfoColorsParserTests
{
    [Fact]
    public void TryParseBasePreset_AcceptsDoom()
    {
        Assert.True(VmAutomapMapinfoColorsParser.TryParseBasePreset("Doom", out var p, out _));
        Assert.Equal(VmAutomapColorBasePreset.Doom, p);
    }

    [Fact]
    public void TryParseBasePreset_RejectsUnknown()
    {
        Assert.False(VmAutomapMapinfoColorsParser.TryParseBasePreset("hexen", out _, out var err));
        Assert.Contains("Raven", err);
    }

    [Fact]
    public void ValidateBaseKeyOrder_RejectsLateBase()
    {
        var msg = VmAutomapMapinfoColorsParser.ValidateBaseKeyOrder("base", colorsAlreadyAssigned: true);
        Assert.NotNull(msg);
    }

    [Fact]
    public void TryResolveColorNameIndex_FindsWallColor()
    {
        Assert.True(VmAutomapMapinfoColorsParser.TryResolveColorNameIndex("wallcolor", out var idx));
        Assert.Equal(VmAutomapColorsetIndex.WallColor, idx);
    }

    [Fact]
    public void TryParseShowLocks_FalseLiteralHidesLocks()
    {
        VmAutomapMapinfoColorsParser.TryParseShowLocks(falseLiteral: true, out var show);
        Assert.False(show);
    }

    [Fact]
    public void UsesRavenGrid_OnlyForRavenBase()
    {
        Assert.True(VmAutomapMapinfoColorsParser.UsesRavenGridForBase(VmAutomapColorBasePreset.Raven));
        Assert.False(VmAutomapMapinfoColorsParser.UsesRavenGridForBase(VmAutomapColorBasePreset.Strife));
    }
}
