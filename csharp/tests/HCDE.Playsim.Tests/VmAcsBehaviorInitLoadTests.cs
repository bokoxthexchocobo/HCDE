namespace HCDE.Playsim.Tests;

public class VmAcsBehaviorInitLoadTests
{
    [Fact]
    public void ParseAcsFormatByte_MapsEnhancedMarkers()
    {
        Assert.Equal(AcsFormat.Enhanced, VmAcsBehaviorInitLoad.ParseAcsFormatByte((byte)'E'));
        Assert.Equal(AcsFormat.LittleEnhanced, VmAcsBehaviorInitLoad.ParseAcsFormatByte((byte)'e'));
        Assert.Equal(AcsFormat.Unknown, VmAcsBehaviorInitLoad.ParseAcsFormatByte((byte)'?'));
    }

    [Fact]
    public void ResolveModuleName_FromFileUsesShortName() =>
        Assert.Equal("MAP01", VmAcsBehaviorInitLoad.ResolveModuleName(fromFile: true, fileShortName: "MAP01"));

    [Fact]
    public void ShouldUpgradeOldEnhanced_DetectsAcSe() =>
        Assert.True(VmAcsBehaviorInitLoad.ShouldUpgradeOldEnhanced(32, AcsChunkIds.AcSe, out var fmt) && fmt == AcsFormat.LittleEnhanced);

    [Fact]
    public void ShouldLocalizeHexenHack_MatchesHexenWad() =>
        Assert.True(VmAcsBehaviorInitLoad.ShouldLocalizeHexenHack(true, true, -1, 1, "hexen.wad"));

    [Fact]
    public void LoadMiniChunkVars_WritesSpan() =>
        Assert.Equal(2, VmAcsBehaviorInitLoad.LoadMiniChunkVars(new int[8], firstVar: 3, values: [9, 10]));

    [Fact]
    public void LoadFunctionsFromChunkNative_CopiesMetadata()
    {
        var src = new[] { new VmAcsBehaviorInitLoad.ScriptFunctionInFile { ArgCount = 2, Address = 9 } };
        var loaded = VmAcsBehaviorInitLoad.LoadFunctionsFromChunkNative(src);
        Assert.Equal(2, loaded[0].ArgCount);
        Assert.Equal(9, loaded[0].Address);
    }
}
