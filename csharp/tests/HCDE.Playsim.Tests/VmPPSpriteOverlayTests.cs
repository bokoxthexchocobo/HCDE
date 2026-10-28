namespace HCDE.Playsim.Tests;

public class VmPPSpriteOverlayTests
{
    [Fact]
    public void HandleOverlayRelative_RotatesOffset()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite { RotationCos = 0, RotationSin = 1 };
        double x = 1, y = 0;
        VmPPSpriteOverlay.HandleOverlayRelative(psp, ref x, ref y);
        Assert.Equal(0, x);
        Assert.Equal(1, y);
    }

    [Fact]
    public void OverlayOffset_AdditiveRelativeUpdatesPosition()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite { RotationCos = 1, X = 1, Y = 2 };
        VmPPSpriteOverlay.OverlayOffset(psp, 3, 4, VmPPSpriteOverlay.WofAdd | VmPPSpriteOverlay.WofRelative);
        Assert.Equal(4, psp.X);
        Assert.Equal(-2, psp.Y);
    }

    [Fact]
    public void OverlayScale_ZeroYCopiesX()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite();
        Assert.True(VmPPSpriteOverlay.OverlayScale(psp, 2, 0, 0));
        Assert.Equal(2, psp.ScaleX);
        Assert.Equal(2, psp.ScaleY);
    }

    [Fact]
    public void OverlayVertexOffset_RejectsInvalidIndex()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite();
        Assert.False(VmPPSpriteOverlay.OverlayVertexOffset(psp, 4, 1, 1, 0));
    }

    [Fact]
    public void OverlayRotate_SyncsTrig()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite();
        VmPPSpriteOverlay.OverlayRotate(psp, 90, 0);
        Assert.Equal(0, psp.RotationCos, 3);
        Assert.Equal(1, psp.RotationSin, 3);
    }

    [Fact]
    public void OverlayFlags_ClearsInterpolationWhenUnset()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite { InterpolateTic = true, Flags = VmPPSpriteOverlay.PspfInterpolate };
        VmPPSpriteOverlay.OverlayFlags(psp, VmPPSpriteOverlay.PspfInterpolate, set: false);
        Assert.False(psp.InterpolateTic);
    }

    [Fact]
    public void WeaponOffsetAction_UsesWeaponLayer()
    {
        var psp = new VmPPSpriteOverlay.OverlayPsprite();
        VmPPSpriteOverlay.WeaponOffsetAction(psp, 5, 6, 0);
        Assert.Equal(5, psp.X);
        Assert.Equal(6, psp.Y);
    }
}
