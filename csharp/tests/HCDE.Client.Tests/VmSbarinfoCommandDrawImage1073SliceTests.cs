namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawImage1073SliceTests
{
    [Fact]
    public void ShouldInvokeParentDrawBeforeTexture_DetectsAlternateOnFailFlag()
    {
        Assert.True(VmSbarinfoCommandDrawImage1073Slice.ShouldInvokeParentDrawBeforeTexture(
            VmSbarinfoCommandDrawImage1073Slice.DiAlternateOnFail));
        Assert.False(VmSbarinfoCommandDrawImage1073Slice.ShouldInvokeParentDrawBeforeTexture(0));
    }

    [Fact]
    public void ComputeDrawInBoxDimensions_ForceScalePicksHeightLimiter()
    {
        var (w, h) = VmSbarinfoCommandDrawImage1073Slice.ComputeDrawInBoxDimensions(
            maxWidth: 50,
            maxHeight: 40,
            textureWidth: 100,
            textureHeight: 200,
            spawnScaleX: 1,
            spawnScaleY: 1,
            flags: VmSbarinfoCommandDrawImage1073Slice.DiForceScale);
        Assert.Equal(20, w);
        Assert.Equal(40, h);
    }

    [Fact]
    public void ComputeDrawInBoxDimensions_WithoutForceScaleUsesMinScale()
    {
        var (w, h) = VmSbarinfoCommandDrawImage1073Slice.ComputeDrawInBoxDimensions(
            maxWidth: 80,
            maxHeight: 30,
            textureWidth: 100,
            textureHeight: 100,
            spawnScaleX: 1,
            spawnScaleY: 1,
            flags: VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox);
        Assert.Equal(30, w);
        Assert.Equal(30, h);
    }

    [Fact]
    public void ClassifyDrawImageKeyword_MapsNativeKeywords()
    {
        var (type, getImage, translatable) =
            VmSbarinfoCommandDrawImage1073Slice.ClassifyDrawImageKeyword("ammoicon2");
        Assert.Equal(VmSbarinfoCommandDrawImage1073Slice.DrawImageType.Ammo2, type);
        Assert.False(getImage);
        Assert.False(translatable);
    }

    [Fact]
    public void ClassifyDrawImageKeyword_TranslatableSetsFlags()
    {
        var (_, getImage, translatable) =
            VmSbarinfoCommandDrawImage1073Slice.ClassifyDrawImageKeyword("translatable");
        Assert.True(getImage);
        Assert.True(translatable);
    }

    [Fact]
    public void ResolveHexenArmorPiece_DefaultsUnknownToArmor()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawImage1073Slice.DrawImageType.HexenArmorShield,
            VmSbarinfoCommandDrawImage1073Slice.ResolveHexenArmorPiece("shield"));
        Assert.Equal(
            VmSbarinfoCommandDrawImage1073Slice.DrawImageType.HexenArmorArmor,
            VmSbarinfoCommandDrawImage1073Slice.ResolveHexenArmorPiece("mystery"));
        Assert.True(VmSbarinfoCommandDrawImage1073Slice.IsUnknownHexenArmorPiece("mystery"));
    }

    [Fact]
    public void ComputeFrameAlpha_MultipliesBlockAndCommandAlpha()
    {
        Assert.Equal(0.25, VmSbarinfoCommandDrawImage1073Slice.ComputeFrameAlpha(0.5, 0.5));
    }

    [Fact]
    public void ResolveSpawnScaledDimensions_AppliesSpawnScales()
    {
        var (w, h) = VmSbarinfoCommandDrawImage1073Slice.ResolveSpawnScaledDimensions(10, 20, 2, 0.5);
        Assert.Equal(20, w);
        Assert.Equal(10, h);
    }

    [Fact]
    public void ShouldRequireStringImageToken_MatchesGetImageGate()
    {
        Assert.True(VmSbarinfoCommandDrawImage1073Slice.ShouldRequireStringImageToken(true));
        Assert.False(VmSbarinfoCommandDrawImage1073Slice.ShouldRequireStringImageToken(false));
    }

    [Fact]
    public void ResolveInventoryImageIndex_UsesNativeSentinel()
    {
        Assert.Equal(-1, VmSbarinfoCommandDrawImage1073Slice.ResolveInventoryImageIndex());
    }
}
