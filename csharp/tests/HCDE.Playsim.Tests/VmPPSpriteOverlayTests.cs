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
}
