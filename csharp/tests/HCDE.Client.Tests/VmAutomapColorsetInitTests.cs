namespace HCDE.Client.Tests;

public class VmAutomapColorsetInitTests
{
    [Fact]
    public void ClearModColorsets_ClearsBothFlags()
    {
        var mod = true;
        var overlay = true;
        VmAutomapColorsetInit.ClearModColorsets(ref mod, ref overlay);
        Assert.False(mod);
        Assert.False(overlay);
    }

    [Fact]
    public void ResolvePresetColorset_StrifeWhenAuto()
    {
        var set = VmAutomapColorsetInit.ResolvePresetColorset(-1, VmAutomapColorsetInit.GameKind.Strife);
        Assert.Equal(2, set);
    }

    [Fact]
    public void PlanInit_PicksStrifePalette()
    {
        var plan = VmAutomapColorsetInit.PlanInit(
            overlayed: false,
            customColors: false,
            modDefined: false,
            overlayModDefined: false,
            amColorset: -1,
            VmAutomapColorsetInit.GameKind.Strife);
        Assert.Equal(VmAutomapColorsetInit.ColorsetInitSource.PredefinedStrife, plan);
    }

    [Fact]
    public void PlanInit_OverlayUsesCustomModWhenDefined()
    {
        var plan = VmAutomapColorsetInit.PlanInit(true, true, false, true, 1, VmAutomapColorsetInit.GameKind.DoomChex);
        Assert.Equal(VmAutomapColorsetInit.ColorsetInitSource.OverlayCustomMod, plan);
    }

    [Fact]
    public void StrifePalette_MatchesNativeBackgroundTriplet()
    {
        var pal = VmAutomapColorsetInit.StrifeColors;
        Assert.Equal(0, pal[0]);
        Assert.Equal(239, pal[3]);
    }

    [Fact]
    public void PredefinedUsesRavenGrid_OnlyForSetThree()
    {
        Assert.True(VmAutomapColorsetInit.PredefinedUsesRavenGrid(3));
        Assert.False(VmAutomapColorsetInit.PredefinedUsesRavenGrid(2));
    }
}
