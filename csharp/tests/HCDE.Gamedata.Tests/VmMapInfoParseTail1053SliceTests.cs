namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseTail1053SliceTests
{
    [Fact]
    public void ApplyLevelInfoResetTail_SetsHcdeShadowUnsetAndSkyRotate()
    {
        var tail = new VmMapInfoLevelResetTail1053();
        VmMapInfoParseTail1053Slice.ApplyLevelInfoResetTail(tail);
        Assert.Equal(VmMapInfoParseTail1053Slice.UnsetMapinfoToggle, tail.HcdeShadowQualityCap);
        Assert.Equal(1f, tail.SkyRotate.Z);
        Assert.Equal(VmMapInfoTonemapMode.None, tail.Tonemap);
        Assert.True(tail.SpecialActionsCleared);
    }

    [Fact]
    public void ResolveLookupLevelName_ReturnsPlainWhenFlagUnset()
    {
        var name = VmMapInfoParseTail1053Slice.ResolveLookupLevelName(0, "MyLevel", "MAP01", _ => "1: Entry");
        Assert.Equal("MyLevel", name);
    }

    [Fact]
    public void ResolveLookupLevelName_StripsMapHeaderFromLocalizedString()
    {
        const uint lookupFlag = VmMapInfoParseTail1053Slice.LevelLookupLevelNameFlag;
        var name = VmMapInfoParseTail1053Slice.ResolveLookupLevelName(
            lookupFlag,
            "$OBLIGATORY",
            "MAP01",
            _ => "1: Entryway");
        Assert.Equal("Entryway", name);
    }

    [Fact]
    public void BuildLocalizedHeaderPrefix_FormatsEpisodeMap()
    {
        Assert.Equal("E1M1: ", VmMapInfoParseTail1053Slice.BuildLocalizedHeaderPrefix("E1M1"));
    }

    [Fact]
    public void ShouldEvaluateInventoryRedirect_RequiresNamedType()
    {
        Assert.True(VmMapInfoParseTail1053Slice.ShouldEvaluateInventoryRedirect(1));
        Assert.False(VmMapInfoParseTail1053Slice.ShouldEvaluateInventoryRedirect(
            VmMapInfoParseTail1053Slice.NameNone));
    }

    [Fact]
    public void ShouldEvaluateCvarRedirect_RejectsIgnoredBool()
    {
        Assert.False(VmMapInfoParseTail1053Slice.ShouldEvaluateCvarRedirect(1, true, cvarIgnored: true));
        Assert.True(VmMapInfoParseTail1053Slice.ShouldEvaluateCvarRedirect(1, true, cvarIgnored: false));
    }

    [Fact]
    public void UsesPerPlayerCvarRedirect_DetectsUserInfoFlag()
    {
        Assert.True(VmMapInfoParseTail1053Slice.UsesPerPlayerCvarRedirect(cvarFlags: 0x40, cvarUserInfoFlag: 0x40));
    }

    [Fact]
    public void IsLevelInfoValid_AllowsDefaultRecordWithoutMapName()
    {
        Assert.True(VmMapInfoParseTail1053Slice.IsLevelInfoValid("", isDefaultLevelRecord: true));
        Assert.False(VmMapInfoParseTail1053Slice.IsLevelInfoValid("", isDefaultLevelRecord: false));
    }
}
