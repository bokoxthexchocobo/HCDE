namespace HCDE.Playsim.Tests;

public class VmPortalClipTranslateTests
{
    [Fact]
    public void ClipLineToPortal_RejectsLineBehindPortal()
    {
        var clip = VmPortalClipTranslate.ClipLineToPortal(
            0, 10, 0, 20,
            0, 0, 10, 0,
            10, 0,
            5, 5);
        Assert.True(clip);
    }

    [Fact]
    public void TranslatePortalXy_RotatesAndOffsets()
    {
        var ctx = new VmPortalClipTranslate.TranslatePortalContext
        {
            HasPortal = true,
            SrcV1X = 0,
            SrcV1Y = 0,
            DestV2X = 100,
            DestV2Y = 0,
            CosRot = 0,
            SinRot = 1,
        };
        var x = 1.0;
        var y = 0.0;
        VmPortalClipTranslate.TranslatePortalXy(ctx, ref x, ref y);
        Assert.Equal(100, x, 3);
        Assert.Equal(1, y, 3);
    }
}
