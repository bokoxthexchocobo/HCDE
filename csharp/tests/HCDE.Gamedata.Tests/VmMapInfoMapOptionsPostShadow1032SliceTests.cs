namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsPostShadow1032SliceTests
{
    [Fact]
    public void ParseOptionalBoolFlag_MatchesNativeToggleShape()
    {
        Assert.True(VmMapInfoMapOptionsPostShadow1032Slice.ParseOptionalBoolFlag(false, 0, whenOmitted: true));
        Assert.False(VmMapInfoMapOptionsPostShadow1032Slice.ParseOptionalBoolFlag(true, 0, whenOmitted: true));
    }

    [Fact]
    public void TryParseSkyRotateVector_Normalizes()
    {
        var tokens = new[] { "0", "0", "2" };
        var i = 0;
        Assert.True(VmMapInfoMapOptionsPostShadow1032Slice.TryParseSkyRotateVector(tokens, ref i, out var v));
        Assert.Equal(3, i);
        Assert.Equal(1f, v.Z, 3);
    }

    [Fact]
    public void ApplyScriptLumps_DelegatesToScriptSlice()
    {
        var info = new VmMapInfoLevelInfo();
        VmMapInfoMapOptionsPostShadow1032Slice.ApplyScriptLumps(info, "ED", "ACS", "INTRO", "OUTRO");
        Assert.Equal("ED", info.EdName);
        Assert.Equal("OUTRO", info.OutroCutscene);
    }

    [Fact]
    public void ResolveHandlerKind_MapsNativeMitTypes()
    {
        Assert.Equal(
            VmMapInfoMapOptionsPostShadow1032Slice.MapInfoFlagHandlerKind.SetFlag3,
            VmMapInfoMapOptionsPostShadow1032Slice.ResolveHandlerKind("MITYPE_SETFLAG3"));
    }

    [Fact]
    public void ResolveSetFlagMask_ReturnsDataForSetFlag()
    {
        Assert.Equal(
            VmMapInfoMapOptionsPostShadow1032Slice.LevelDoubleSky,
            VmMapInfoMapOptionsPostShadow1032Slice.ResolveSetFlagMask(
                VmMapInfoMapOptionsPostShadow1032Slice.MapInfoFlagHandlerKind.SetFlag,
                VmMapInfoMapOptionsPostShadow1032Slice.LevelDoubleSky));
    }

    [Fact]
    public void ShouldIgnoreFlagHandler_ForMitIgnore()
    {
        Assert.True(VmMapInfoMapOptionsPostShadow1032Slice.ShouldIgnoreFlagHandler(
            VmMapInfoMapOptionsPostShadow1032Slice.MapInfoFlagHandlerKind.Ignore));
    }

    [Fact]
    public void MatchesFlagHandlerKeyword_IsCaseInsensitive()
    {
        Assert.True(VmMapInfoMapOptionsPostShadow1032Slice.MatchesFlagHandlerKeyword("DoubleSky", "doublesky"));
    }
}
