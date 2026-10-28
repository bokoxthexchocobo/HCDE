namespace HCDE.Client.Tests;

public class VmSbarinfoCommandGameMode1085SliceTests
{
    [Fact]
    public void TryResolveGameModeKeyword_MapsTeamgameIndex()
    {
        var (ok, idx) = VmSbarinfoCommandGameMode1085Slice.TryResolveGameModeKeyword("teamgame");
        Assert.True(ok);
        Assert.Equal(3, idx);
    }

    [Fact]
    public void MergeGameModeKeywordIntoMask_StacksBitsVia1084Helper()
    {
        var merged = VmSbarinfoCommandGameMode1085Slice.MergeGameModeKeywordIntoMask(
            modes: 0,
            modeIndex: 2);
        Assert.Equal((int)VmSbarinfoCommandGameMode1085Slice.GameModeFlag.DeathMatch, merged);
    }

    [Fact]
    public void ResolveUnknownGameModeWarningState_EmitsOnce()
    {
        var (warn, next) = VmSbarinfoCommandGameMode1085Slice.ResolveUnknownGameModeWarningState(
            recognizedKeyword: false,
            warnUnknownStillActive: true);
        Assert.True(warn);
        Assert.False(next);
    }

    [Fact]
    public void ResolveGameModeFlowControlTruth_MatchesCooperativeCase()
    {
        Assert.True(VmSbarinfoCommandGameMode1085Slice.ResolveGameModeFlowControlTruth(
            modes: (int)VmSbarinfoCommandGameMode1085Slice.GameModeFlag.Cooperative,
            multiplayer: true,
            deathmatch: false,
            teamplay: false));
    }

    [Fact]
    public void ResolveUsesAmmoFlowControlTruth_OrPrimarySecondary()
    {
        Assert.True(VmSbarinfoCommandUsesAmmo1085Slice.ResolveUsesAmmoFlowControlTruth(
            hasPrimaryAmmo: false,
            hasSecondaryAmmo: true));
    }

    [Fact]
    public void ResolveUsesSecondaryAmmoFlowControlTruth_RequiresDistinctSecondary()
    {
        Assert.False(VmSbarinfoCommandUsesAmmo1085Slice.ResolveUsesSecondaryAmmoFlowControlTruth(
            hasSecondaryAmmo: true,
            secondarySameAsPrimary: true));
    }

    [Fact]
    public void ResolveInventoryBarNotVisibleTruth_WhenTicsExpired()
    {
        Assert.True(VmSbarinfoCommandInventoryBar1085Slice.ResolveInventoryBarNotVisibleTruth(
            inventoryTics: 0,
            levelDisablesInventoryBar: false));
    }

    [Fact]
    public void TryParseAspectRatioKeyword_Maps169()
    {
        var (ok, kind) = VmSbarinfoCommandAspectRatio1085Slice.TryParseAspectRatioKeyword("16:9");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_9, kind);
    }

    [Fact]
    public void RegisterAspectRatioSelfMapping_WritesDiagonalEntry()
    {
        var map = new int[VmSbarinfoCommandAspectRatio1085Slice.AspectRatioMapLength];
        VmSbarinfoCommandAspectRatio1085Slice.RegisterAspectRatioSelfMapping(
            map,
            VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio5_4);
        Assert.Equal(4, map[4]);
    }

    [Fact]
    public void ResolveAspectRatioFlowControlTruth_ComparesMappedRatio()
    {
        Assert.True(VmSbarinfoCommandAspectRatio1085Slice.ResolveAspectRatioFlowControlTruth(
            mappedActiveRatio: 1,
            configuredRatio: 1));
    }
}
