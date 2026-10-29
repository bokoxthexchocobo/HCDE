namespace HCDE.Client.Tests;

public class VmAmMapOverlayColor1123SliceTests
{
    [Fact]
    public void DefaultOverlayColors_MatchLockedThingAndPortal()
    {
        var colors = VmAmMapOverlayColor1123Slice.DefaultOverlayColors();
        Assert.Equal(0x008844u, colors["ovlocked"]);
        Assert.Equal(0xe88800u, colors["ovthing"]);
        Assert.Equal(0x004022u, colors["ovportal"]);
        Assert.Equal(0.2f, VmAmMapOverlayColor1123Slice.DefaultOverlaySectorFillAlpha);
    }

    [Fact]
    public void ForceOpaque_SetsAlphaAndInvalidRgbStaysRejected()
    {
        Assert.Equal(0xFF2c1808u, VmAmMapOverlayColor1123Slice.ForceOpaque(0x2c1808));
        Assert.False(VmAmMapOverlayColor1123Slice.IsValidColor(0));
        Assert.True(VmAmMapOverlayColor1123Slice.IsValidColor(VmAmMapOverlayColor1123Slice.PackOpaqueRgb(1, 2, 3)));
    }

    [Fact]
    public void DeriveAlmostBackground_WrapsChannelsBelowZero()
    {
        var (r, g, b) = VmAmMapOverlayColor1123Slice.DeriveAlmostBackground(0x000008);
        Assert.Equal((16, 16, 24), (r, g, b));
    }

    [Fact]
    public void InitFromCvars_PaintsAlmostBackgroundAndEnablesLocks()
    {
        var inputs = new uint[VmAmMapOverlayColor1123Slice.AlmostBackgroundIndex];
        inputs[0] = 0x6c5440;
        var colors = VmAmMapOverlayColor1123Slice.InitFromCvars(
            inputs, 0.4f, out var alpha, out var locks, out var forceBackground);
        Assert.Equal(0xFF6c5440u, colors[0]);
        Assert.Equal(VmAmMapOverlayColor1123Slice.PackOpaqueRgb(92, 68, 48), colors[25]);
        Assert.Equal(0.4f, alpha);
        Assert.True(locks);
        Assert.False(forceBackground);
    }

    [Fact]
    public void ColorNames_EndAtAlmostBackground()
    {
        var names = VmAmMapOverlayColor1123Slice.ColorNames();
        Assert.Equal(VmAmMapOverlayColor1123Slice.ColorCount, names.Count);
        Assert.Equal("AlmostBackgroundColor", names[VmAmMapOverlayColor1123Slice.AlmostBackgroundIndex]);
    }
}
