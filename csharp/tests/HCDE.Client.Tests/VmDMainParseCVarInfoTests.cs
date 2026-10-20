namespace HCDE.Client.Tests;

public class VmDMainParseCVarInfoTests
{
    [Fact]
    public void NormalizeConfigOnlyFlags_ClearsServerUser()
    {
        var flags = VmDMainParseCVarInfo.CvarServerInfo | VmDMainParseCVarInfo.CvarConfigOnly;
        var normalized = VmDMainParseCVarInfo.NormalizeConfigOnlyFlags(flags);
        Assert.Equal(VmDMainParseCVarInfo.CvarConfigOnly, normalized);
    }

    [Fact]
    public void HasValidScopeFlags_RejectsMissingScope()
    {
        Assert.False(VmDMainParseCVarInfo.HasValidScopeFlags(VmDMainParseCVarInfo.CvarModArchive));
    }

    [Fact]
    public void ApplyAttributeFlag_Server_SetsServerInfo()
    {
        var flags = VmDMainParseCVarInfo.ApplyAttributeFlag("server", VmDMainParseCVarInfo.CvarModArchive, out _, out _);
        Assert.True((flags & VmDMainParseCVarInfo.CvarServerInfo) != 0);
    }

    [Fact]
    public void IsPatchExtension_MatchesDeh()
    {
        Assert.True(VmDMainParseCVarInfo.IsPatchExtension("foo.deh"));
    }
}

public class VmDMainDebugCcmdsTests
{
    [Fact]
    public void ShouldIgnoreDebugRestart_DuringNetgame()
    {
        Assert.True(VmDMainDebugCcmds.ShouldIgnoreDebugRestart(true, false));
    }

    [Fact]
    public void StripTitleColorEscapes_RemovesBracketSequence()
    {
        var stripped = VmDMainDebugCcmds.StripTitleColorEscapes("Plain\x1c[cf00]Title");
        Assert.Equal("PlainTitle", stripped);
    }

    [Fact]
    public void FormatBrandedWindowTitle_PrefixesHcde()
    {
        Assert.Equal("HCDE - Doom", VmDMainDebugCcmds.FormatBrandedWindowTitle("Doom"));
    }

    [Fact]
    public void ShouldMarkFsDirEntryHidden_WhenShadowed()
    {
        Assert.True(VmDMainDebugCcmds.ShouldMarkFsDirEntryHidden(1, 0));
    }
}

public class VmAutomapCustomCvarCallbacksTests
{
    [Fact]
    public void ClampAutomapCheat_BlocksNetWithoutCheats()
    {
        Assert.Equal(0, VmAutomapCustomCvarCallbacks.ClampAutomapCheat(true, false, 1));
    }

    [Fact]
    public void ShouldDeactivateOverlay_WhenOverlayZero()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldDeactivateOverlay(true, true, 0));
    }

    [Fact]
    public void ClampEmptySpaceMargin_CapsAtNinety()
    {
        Assert.Equal(90, VmAutomapCustomCvarCallbacks.ClampEmptySpaceMargin(120));
    }

    [Fact]
    public void DefaultShowSecrets_IsTrue()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.DefaultShowSecrets);
    }

    [Fact]
    public void ShouldUpdateShowAllLines_WhenAutomapPresent()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldUpdateShowAllLines(true));
    }

    [Fact]
    public void ShouldRefreshResolution_WhenStatusBarAndAutomap()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldRefreshResolution(true, true));
    }

    [Fact]
    public void DefaultOverlaySectorFillAlpha_MatchesNative()
    {
        Assert.Equal(0.2f, VmAutomapCustomCvarCallbacks.DefaultOverlaySectorFillAlpha);
    }

    [Fact]
    public void ShouldInitColorFromCvar_SkipsAlmostBackground()
    {
        Assert.False(VmAutomapCustomCvarCallbacks.ShouldInitColorFromCvar(
            VmAutomapCustomCvarCallbacks.AlmostBackgroundColorIndex,
            VmAutomapCustomCvarCallbacks.AlmostBackgroundColorIndex));
    }

    [Fact]
    public void StandardCvarTableLength_MatchesNative()
    {
        Assert.Equal(25, VmAutomapCustomCvarCallbacks.StandardCvarTableLength);
    }

    [Fact]
    public void OverlayCvarTableLength_MatchesNative()
    {
        Assert.Equal(25, VmAutomapCustomCvarCallbacks.OverlayCvarTableLength);
    }
}
