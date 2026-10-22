namespace HCDE.Playsim.Tests;

public class VmPPSpriteOverlayTranslationTests
{
    [Fact]
    public void OverlayTranslation_EmptyNameResetsTranslation()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel { Translation = 9 };
        var ctx = new VmPPSpriteOverlayTranslation.OverlayActionContext
        {
            CallFromPsprite = true,
            PspIndex = 5,
            FindPsprite = (_, layer) => layer == 5 ? pspr : null
        };
        VmPPSpriteOverlayTranslation.OverlayTranslation(ctx, new object(), 0, "", _ => 3);
        Assert.Equal(VmPPSpriteOverlayTranslation.NoTranslation, pspr.Translation);
    }

    [Fact]
    public void OverlayTranslation_SkipsWhenNotFromPsprite()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel { Translation = 2 };
        var ctx = new VmPPSpriteOverlayTranslation.OverlayActionContext
        {
            CallFromPsprite = false,
            PspIndex = 1,
            FindPsprite = (_, _) => pspr
        };
        VmPPSpriteOverlayTranslation.OverlayTranslation(ctx, new object(), 0, "foo", _ => 9);
        Assert.Equal(2, pspr.Translation);
    }

    [Fact]
    public void OverlayX_ReturnsPositionFromPspIndex()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel { X = 12, Y = 34 };
        var player = new VmPPSpriteOverlayTranslation.OverlayPlayerModel
        {
            FindPsprite = _ => pspr
        };
        var ctx = new VmPPSpriteOverlayTranslation.OverlayActionContext { CallFromPsprite = true, PspIndex = 3 };
        Assert.Equal(12, VmPPSpriteOverlayTranslation.OverlayX(ctx, player, 0));
        Assert.Equal(34, VmPPSpriteOverlayTranslation.OverlayY(ctx, player, 0));
        Assert.Equal(3, VmPPSpriteOverlayTranslation.OverlayId(ctx));
    }

    [Fact]
    public void OverlayAlpha_ClampsAndOverlayCreatesLayer()
    {
        var pspr = new VmPPSpriteLayers.PspriteModel();
        var ctx = new VmPPSpriteOverlayTranslation.OverlayActionContext
        {
            CallFromPsprite = true,
            PspIndex = 1,
            FindPsprite = (_, _) => pspr
        };
        VmPPSpriteOverlayTranslation.OverlayAlpha(ctx, new object(), 0, 2);
        Assert.Equal(1, pspr.Alpha);

        var created = new VmPPSpriteLayers.PspriteModel();
        var player = new VmPPSpriteOverlayTranslation.OverlayPlayerModel();
        var state = new VmPPSpriteSetState.StateModel();
        var ok = VmPPSpriteOverlayTranslation.Overlay(
            player,
            new object(),
            7,
            state,
            false,
            (_, _, _) => created,
            (p, s) => p.State = (VmPPSpriteSetState.StateModel?)s);
        Assert.True(ok);
        Assert.Same(state, created.State);
    }
}
