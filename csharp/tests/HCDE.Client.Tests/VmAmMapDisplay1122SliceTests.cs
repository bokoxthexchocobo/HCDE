namespace HCDE.Client.Tests;

public class VmAmMapDisplay1122SliceTests
{
    [Fact]
    public void ClampEmptySpaceMargin_LimitsToZeroThroughNinety()
    {
        Assert.Equal(0, VmAmMapDisplay1122Slice.ClampEmptySpaceMargin(-4));
        Assert.Equal(90, VmAmMapDisplay1122Slice.ClampEmptySpaceMargin(120));
        Assert.Equal(12, VmAmMapDisplay1122Slice.ClampEmptySpaceMargin(12));
    }

    [Fact]
    public void ToggleFollow_FlipsAndTryAddMark_RejectsNegative()
    {
        Assert.False(VmAmMapDisplay1122Slice.ToggleFollow(true));
        Assert.Null(VmAmMapDisplay1122Slice.TryAddMark(true, true, () => -1));
        Assert.Equal(3, VmAmMapDisplay1122Slice.TryAddMark(true, true, () => 3));
    }

    [Fact]
    public void ShouldToggleMap_OnlyWhenGameActionIsIdle()
    {
        Assert.True(VmAmMapDisplay1122Slice.ShouldToggleMap(VmAmMapDisplay1122Slice.GameActionNothing));
        Assert.False(VmAmMapDisplay1122Slice.ShouldToggleMap(1));
    }

    [Fact]
    public void TrySetZoomDirection_ParsesSecondArgument()
    {
        Assert.True(VmAmMapDisplay1122Slice.TrySetZoomDirection(2, "1.5", out var zoom));
        Assert.Equal(1.5f, zoom);
        Assert.False(VmAmMapDisplay1122Slice.TrySetZoomDirection(1, "1.5", out _));
    }

    [Fact]
    public void DefaultColors_IncludeWallAndOverlayWall()
    {
        var colors = VmAmMapDisplay1122Slice.DefaultColors();
        Assert.Equal(0x2c1808u, colors["wall"]);
        Assert.Equal(0x00ff00u, colors["ovwall"]);
        Assert.Equal(0.4f, VmAmMapDisplay1122Slice.DefaultSectorFillAlpha);
        Assert.Equal("AMMNUMx", new VmAmMapDisplay1122Slice.DisplayDefaults().MarkFont);
    }
}
