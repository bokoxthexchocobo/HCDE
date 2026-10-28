namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawInventoryBar1087SliceTests
{
    [Fact]
    public void ApplyInventoryBarTranslucentBackgroundAlpha_ScalesWithHexenShadow()
    {
        var alpha = VmSbarinfoCommandDrawInventoryBar1087Slice.ApplyInventoryBarTranslucentBackgroundAlpha(
            blockAlpha: 1.0,
            translucent: true);
        Assert.Equal(1.0 * VmSbarinfoCommandDrawInventoryBar1087Slice.HexenShadowAlphaFactor, alpha, 5);
    }

    [Fact]
    public void ComputeInventoryBarSlotOrigin_AdvancesHorizontalSlots()
    {
        var (rx, ry) = VmSbarinfoCommandDrawInventoryBar1087Slice.ComputeInventoryBarSlotOrigin(
            baseX: 10,
            baseY: 20,
            slotIndex: 2,
            spacing: 31,
            vertical: false);
        Assert.Equal(72, rx);
        Assert.Equal(20, ry);
    }

    [Fact]
    public void ResolveInventorySelectionOverlay_MapsHereticSelectBox()
    {
        var (graphic, dx, dy) = VmSbarinfoCommandDrawInventoryBar1087Slice.ResolveInventorySelectionOverlay(
            VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle.Heretic);
        Assert.Equal(VmSbarinfoCommandDrawInventoryBar1087Slice.InventorySelectionGraphic.SelectBox, graphic);
        Assert.Equal(29, dy);
    }

    [Fact]
    public void ResolveInventoryBarRightArrowPlacement_UsesStrifeFormula()
    {
        var (offset, _) = VmSbarinfoCommandDrawInventoryBar1087Slice.ResolveInventoryBarRightArrowPlacement(
            VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle.Strife,
            barSize: 7,
            vertical: false);
        Assert.Equal(7 * 35 - 4, offset);
    }

    [Fact]
    public void TryParseInventoryBarStyleKeyword_PrefersHexenStrictBeforeHexen()
    {
        var (ok, style) = VmSbarinfoCommandDrawInventoryBar1087Slice.TryParseInventoryBarStyleKeyword("HexenStrict");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle.HexenStrict, style);
    }

    [Fact]
    public void ClassifyInventoryBarParseKeyword_MapsTranslucent()
    {
        var (ok, kw) = VmSbarinfoCommandDrawInventoryBar1087Slice.ClassifyInventoryBarParseKeyword("translucent");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarParseKeyword.Translucent, kw);
    }

    [Fact]
    public void ResolveInventoryBarDefaultCounterPosition_Applies2622Offsets()
    {
        var (cx, cy) = VmSbarinfoCommandDrawInventoryBar1087Slice.ResolveInventoryBarDefaultCounterPosition(0, 0);
        Assert.Equal(26, cx);
        Assert.Equal(22, cy);
    }

    [Fact]
    public void ResolveInventoryBarCounterSpacingFromBoxDimension_AdjustsForStrife()
    {
        Assert.Equal(
            31,
            VmSbarinfoCommandDrawInventoryBar1087Slice.ResolveInventoryBarCounterSpacingFromBoxDimension(
                boxDimension: 32,
                VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle.Strife));
    }

    [Fact]
    public void ShouldDrawInventoryBarLeftArrow_RequiresPrevItem()
    {
        Assert.True(VmSbarinfoCommandDrawInventoryBar1087Slice.ShouldDrawInventoryBarLeftArrow(
            noArrows: false,
            hasFirstItem: true,
            hasPreviousItem: true));
    }

    [Fact]
    public void MissingArtiboxSpacingFallback_MatchesNativeNullBox()
    {
        Assert.Equal(32, VmSbarinfoCommandDrawInventoryBar1087Slice.MissingArtiboxSpacingFallback);
    }
}
