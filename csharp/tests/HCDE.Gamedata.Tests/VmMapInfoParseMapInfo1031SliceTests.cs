namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapInfo1031SliceTests
{
    [Fact]
    public void ShouldSkipCmpgninfBlock_WhenMapEmptyOrNoLimits()
    {
        var empty = new VmCmpgninfBlock { MapName = "" };
        empty.HasWaveLimit = true;
        Assert.True(VmMapInfoParseMapInfo1031Slice.ShouldSkipCmpgninfBlock(empty));

        var noLimits = new VmCmpgninfBlock { MapName = "MAP01" };
        Assert.True(VmMapInfoParseMapInfo1031Slice.ShouldSkipCmpgninfBlock(noLimits));
    }

    [Fact]
    public void ApplyCmpgninfLimits_WritesPresentLimitsOnly()
    {
        var block = new VmCmpgninfBlock { HasWaveLimit = true, WaveLimit = 120, HasDuelLimit = true, DuelLimit = 9 };
        var level = new VmMapInfoParseMapInfo1031Slice.CmpgninfLevelDraft();
        VmMapInfoParseMapInfo1031Slice.ApplyCmpgninfLimits(block, level);
        Assert.Equal(120, level.InvasionWaveLimit);
        Assert.Equal(9, level.DuelLimit);
    }

    [Fact]
    public void MergeComplvlIntoDefaults_OrMasksPresets()
    {
        var defaults = new VmMapInfoParseMapInfo1031Slice.GameDefaultsCompatDraft
        {
            CompatFlags = 1,
            CompatMask = 2,
            CompatFlags2 = 4,
            CompatMask2 = 8
        };
        VmMapInfoParseMapInfo1031Slice.MergeComplvlIntoDefaults(
            defaults, new VmMapInfoComplvlFlags(0x10, 0x20));
        Assert.Equal(0x11u, defaults.CompatFlags);
        Assert.Equal(0x12u, defaults.CompatMask);
        Assert.Equal(0x24u, defaults.CompatFlags2);
        Assert.Equal(0x28u, defaults.CompatMask2);
    }

    [Fact]
    public void TryLoadComplvlPresets_DelegatesToExistingParser()
    {
        Assert.True(VmMapInfoParseMapInfo1031Slice.TryLoadComplvlPresets("mbf21", out var flags));
        Assert.NotEqual(0u, flags.Flags1);
    }

    [Fact]
    public void ShouldFatalOnBasemapinfoAddonOverride_WhenContainerPositive()
    {
        Assert.True(VmMapInfoParseMapInfo1031Slice.ShouldFatalOnBasemapinfoAddonOverride(1));
        Assert.False(VmMapInfoParseMapInfo1031Slice.ShouldFatalOnBasemapinfoAddonOverride(0));
    }

    [Fact]
    public void ShouldSkipNamedMapinfoLump_SkipsMapinfoWhenZmapinfoInWad()
    {
        Assert.True(VmMapInfoParseMapInfo1031Slice.ShouldSkipNamedMapinfoLump(
            VmMapInfoLumpPrecedence.LumpIndexMapinfo,
            sameWadHasZmapinfo: true,
            sameWadHasMapinfo: false,
            sameWadHasUmapinfo: false));
    }

    [Fact]
    public void ShouldProbeZmapinfoWhenClassicMapinfo_OnlyForIndexZero()
    {
        Assert.True(VmMapInfoParseMapInfo1031Slice.ShouldProbeZmapinfoWhenClassicMapinfo(0));
        Assert.False(VmMapInfoParseMapInfo1031Slice.ShouldProbeZmapinfoWhenClassicMapinfo(2));
    }

    [Fact]
    public void TryApplyCmpgninfBlockProperty_ClampsWaveLimit()
    {
        var block = new VmCmpgninfBlock();
        Assert.True(VmMapInfoParseMapInfo1031Slice.TryApplyCmpgninfBlockProperty(
            block, "wavelimit", "", 5000, out _));
        Assert.Equal(3000, block.WaveLimit);
        Assert.True(block.HasWaveLimit);
    }

    [Fact]
    public void NeedsSyntheticLevelForCmpgninf_WhenMissing()
    {
        Assert.True(VmMapInfoParseMapInfo1031Slice.NeedsSyntheticLevelForCmpgninf(levelAlreadyExists: false));
    }
}
