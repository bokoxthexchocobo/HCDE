namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsMedia1028SliceTests
{
    [Fact]
    public void ApplyTitlePatchFlags_SetsHideAuthorOnNewFormatComma()
    {
        var flags = VmMapInfoMapOptionsMedia1028Slice.ApplyTitlePatchFlags(0, newFormat: true, hasTrailingComma: true, trailingNumber: 1);
        Assert.True((flags & VmMapInfoMapOptionsMedia1028Slice.Level3HideAuthorName) != 0);
    }

    [Fact]
    public void StoreMapInterMusic_InsertsPair()
    {
        var table = new Dictionary<string, (string Music, int Order)>();
        VmMapInfoMapOptionsMedia1028Slice.StoreMapInterMusic(table, "MAP01", "FIGHTER", 3);
        Assert.Equal(("FIGHTER", 3), table["MAP01"]);
    }

    [Fact]
    public void TryApplyOption_WaveAndDuelLimitsClamp()
    {
        var draft = new VmMapInfoMapOptionsMedia1028Slice.LevelScalarsDraft();
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("wavelimit", draft, false, false, 0, "", 9000, 0));
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("duellimit", draft, false, false, 0, "", 400, 0));
        Assert.Equal(3000, draft.InvasionWaveLimit);
        Assert.Equal(255, draft.DuelLimit);
    }

    [Fact]
    public void TryApplyOption_WallShadesHalveAndClamp()
    {
        var draft = new VmMapInfoMapOptionsMedia1028Slice.LevelScalarsDraft();
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("vertwallshade", draft, false, false, 0, "", 200, 0));
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("horizwallshade", draft, false, false, 0, "", -300, 0));
        Assert.Equal(100, draft.WallVertLight);
        Assert.Equal(-128, draft.WallHorizLight);
    }

    [Fact]
    public void TryApplyOption_SkyboxClearsSpeed()
    {
        var draft = new VmMapInfoMapOptionsMedia1028Slice.LevelScalarsDraft { SkySpeed1 = 5 };
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("skybox", draft, false, false, 0, "SKY1", 0, 0));
        Assert.Equal("SKY1", draft.SkyPic1);
        Assert.Equal(0, draft.SkySpeed1);
    }

    [Fact]
    public void TryApplyOption_SkymistyScaleClamps()
    {
        var draft = new VmMapInfoMapOptionsMedia1028Slice.LevelScalarsDraft();
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryApplyOption("skymistyscale", draft, false, false, 0, "", 0, 900));
        Assert.Equal(544, draft.SkyMistyScale);
    }

    [Fact]
    public void TryParseCdIdHex_ParsesHex()
    {
        Assert.True(VmMapInfoMapOptionsMedia1028Slice.TryParseCdIdHex("ff00", out var id));
        Assert.Equal(0xff00u, id);
    }

    [Fact]
    public void ApplyEvenLighting_ZerosWallLights()
    {
        var draft = new VmMapInfoMapOptionsMedia1028Slice.LevelScalarsDraft { WallVertLight = 9, WallHorizLight = -3 };
        VmMapInfoMapOptionsMedia1028Slice.ApplyEvenLighting(draft);
        Assert.Equal(0, draft.WallVertLight);
        Assert.Equal(0, draft.WallHorizLight);
    }
}
