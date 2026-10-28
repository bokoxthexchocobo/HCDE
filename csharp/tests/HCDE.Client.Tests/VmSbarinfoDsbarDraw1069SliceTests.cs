namespace HCDE.Client.Tests;

public class VmSbarinfoDsbarDraw1069SliceTests
{
    [Fact]
    public void ResolveHudBarIndexForDrawState_CoversFullscreenAndOther()
    {
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.FullscreenHudBarIndex,
            VmSbarinfoDsbarDraw1069Slice.ResolveHudBarIndexForDrawState(
                VmSbarinfoDsbarDraw1069Slice.PrimaryDrawHudState.Fullscreen, false, false));
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.NoneHudBarIndex,
            VmSbarinfoDsbarDraw1069Slice.ResolveHudBarIndexForDrawState(
                VmSbarinfoDsbarDraw1069Slice.PrimaryDrawHudState.Other, true, true));
    }

    [Fact]
    public void ResolveMainHudDrawOffsets_UsesPopupDisplacementWhenNeeded()
    {
        var (x, y) = VmSbarinfoDsbarDraw1069Slice.ResolveMainHudDrawOffsets(true, 4, -2);
        Assert.Equal(4, x);
        Assert.Equal(-2, y);
    }

    [Fact]
    public void ShouldDrawInventoryBar_RequiresTicsAndAllowedLevel()
    {
        Assert.True(VmSbarinfoDsbarDraw1069Slice.ShouldDrawInventoryBar(
            5, levelAllowsInventoryBar: true, VmSbarinfoDsbarDraw1069Slice.PrimaryDrawHudState.Fullscreen));
        Assert.False(VmSbarinfoDsbarDraw1069Slice.ShouldDrawInventoryBar(
            0, true, VmSbarinfoDsbarDraw1069Slice.PrimaryDrawHudState.Fullscreen));
    }

    [Fact]
    public void MapActivePopupToHudBarIndex_MapsLogKeysStatus()
    {
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.PopupKeysHudBarIndex,
            VmSbarinfoDsbarDraw1069Slice.MapActivePopupToHudBarIndex(VmSbarinfoDsbarDraw1069Slice.PopKeys));
    }

    [Fact]
    public void ResolveShowPopupIntent_ClosesWhenPopupActive()
    {
        var (pending, close) = VmSbarinfoDsbarDraw1069Slice.ResolveShowPopupIntent(
            VmSbarinfoDsbarDraw1069Slice.PopLog, currentPopup: VmSbarinfoDsbarDraw1069Slice.PopKeys);
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.PopLog, pending);
        Assert.True(close);
    }

    [Fact]
    public void CommitImmediatePopupWhenIdle_PromotesPending()
    {
        var (active, pending) = VmSbarinfoDsbarDraw1069Slice.CommitImmediatePopupWhenIdle(
            VmSbarinfoDsbarDraw1069Slice.PopStatus, currentPopup: VmSbarinfoDsbarDraw1069Slice.PopNone);
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.PopStatus, active);
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.PopNone, pending);
    }

    [Fact]
    public void ResolveProtrusionOffset_WalksAscendingFactors()
    {
        ReadOnlySpan<(double, int)> data = [(0.5, 10), (1.0, 20), (1.5, 30)];
        Assert.Equal(20, VmSbarinfoDsbarDraw1069Slice.ResolveProtrusionOffset(data, 1.0));
        Assert.Equal(0, VmSbarinfoDsbarDraw1069Slice.ResolveProtrusionOffset(data, 0.25));
    }

    [Fact]
    public void ShouldSkipDrawGraphic_WhenTextureMissing()
    {
        Assert.True(VmSbarinfoDsbarDraw1069Slice.ShouldSkipDrawGraphic(null));
        Assert.False(VmSbarinfoDsbarDraw1069Slice.ShouldSkipDrawGraphic(new object()));
    }

    [Fact]
    public void ShouldAdvancePendingPopupAfterClose_WhenClosedAndSettled()
    {
        Assert.True(VmSbarinfoDsbarDraw1069Slice.ShouldAdvancePendingPopupAfterClose(false, doneMoving: true));
    }

    [Fact]
    public void ApplyInventoryTicsWhenBarEmpty_ClearsWhenNoCommands()
    {
        Assert.Equal(0, VmSbarinfoDsbarDraw1069Slice.ApplyInventoryTicsWhenBarEmpty(12, false, true));
        Assert.Equal(12, VmSbarinfoDsbarDraw1069Slice.ApplyInventoryTicsWhenBarEmpty(12, true, true));
    }

    [Fact]
    public void SelectInventoryHudBarIndex_SwitchesForFullscreen()
    {
        Assert.Equal(VmSbarinfoDsbarDraw1069Slice.InventoryFullscreenHudBarIndex,
            VmSbarinfoDsbarDraw1069Slice.SelectInventoryHudBarIndex(
                VmSbarinfoDsbarDraw1069Slice.PrimaryDrawHudState.Fullscreen));
    }
}
