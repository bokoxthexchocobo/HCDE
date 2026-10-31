namespace HCDE.Client.Tests;

public class VmAmMapPaletteInit1125SliceTests
{
    [Fact]
    public void DoomColors_WallIsRedAndSpecialIsUnused()
    {
        var doom = VmAmMapPaletteInit1125Slice.DoomColors();
        Assert.Equal(VmAmMapPaletteInit1125Slice.ColorCount * 3, doom.Length);
        Assert.Equal((byte)0xfc, VmAmMapPaletteInit1125Slice.Triplet(doom, 2).R);
        Assert.True(VmAmMapPaletteInit1125Slice.IsUnused(doom, 13));
        Assert.Equal((byte)0x40, VmAmMapPaletteInit1125Slice.Triplet(doom, 25).R);
    }

    [Fact]
    public void StrifeAndRaven_SpotCheckThingAndPortal()
    {
        var strife = VmAmMapPaletteInit1125Slice.StrifeColors();
        Assert.Equal((239, 239, 0), VmAmMapPaletteInit1125Slice.Triplet(strife, 1));
        Assert.Equal((187, 59, 0), VmAmMapPaletteInit1125Slice.Triplet(strife, 7));
        var raven = VmAmMapPaletteInit1125Slice.RavenColors();
        Assert.Equal((0x6c, 0x54, 0x40), VmAmMapPaletteInit1125Slice.Triplet(raven, 0));
        Assert.Equal((0x50, 0x50, 0x50), VmAmMapPaletteInit1125Slice.Triplet(raven, 25));
        Assert.False(VmAmMapPaletteInit1125Slice.IsUnused(raven, 16));
    }

    [Fact]
    public void ClearColorsets_ClearsBothDefinedFlags()
    {
        var (overlay, standard) = VmAmMapPaletteInit1125Slice.ClearColorsets();
        Assert.False(overlay);
        Assert.False(standard);
    }

    [Fact]
    public void ChooseInit_PrefersOverlayModThenGamePalette()
    {
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.CopyOverlayMod,
            VmAmMapPaletteInit1125Slice.ChooseInit(true, true, true, false, -1, VmAmMapPaletteInit1125Slice.GameDoom));
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.OverlayCvars,
            VmAmMapPaletteInit1125Slice.ChooseInit(true, true, false, true, 1, VmAmMapPaletteInit1125Slice.GameDoom));
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.DoomPalette,
            VmAmMapPaletteInit1125Slice.ChooseInit(false, false, false, false, -1, VmAmMapPaletteInit1125Slice.GameDoomChex));
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.StrifePalette,
            VmAmMapPaletteInit1125Slice.ChooseInit(false, true, false, false, -1, VmAmMapPaletteInit1125Slice.GameStrife));
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.RavenResolvedPastHull,
            VmAmMapPaletteInit1125Slice.ChooseInit(false, false, false, false, -1, VmAmMapPaletteInit1125Slice.GameRaven));
        Assert.Equal(
            VmAmMapPaletteInit1125Slice.InitKind.StandardCvars,
            VmAmMapPaletteInit1125Slice.ChooseInit(false, false, false, false, 0, VmAmMapPaletteInit1125Slice.GameDoom));
        Assert.False(VmAmMapPaletteInit1125Slice.PaletteShowLocks(VmAmMapPaletteInit1125Slice.InitKind.DoomPalette));
        Assert.False(VmAmMapPaletteInit1125Slice.PaletteShowLocks(VmAmMapPaletteInit1125Slice.InitKind.StrifePalette));
    }
}
