namespace HCDE.Gamedata.Tests;

public class VmInfoLoadAltHud1042SliceTests
{
    [Fact]
    public void CreateSpriteDefDraft_ZeroFrames()
    {
        var draft = VmInfoLoadAltHud1042Slice.CreateSpriteDefDraft("PLAY");
        Assert.Equal("PLAY", draft.Name);
        Assert.Equal(0, draft.NumFrames);
        Assert.Equal(0, draft.SpriteFramesPointer);
    }

    [Fact]
    public void ClassifyAltHudKeyword_HealthBerserkOrItem()
    {
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudKeywordKind.Health,
            VmInfoLoadAltHud1042Slice.ClassifyAltHudKeyword("health"));
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudKeywordKind.Berserk,
            VmInfoLoadAltHud1042Slice.ClassifyAltHudKeyword("Berserk"));
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudKeywordKind.InventoryClass,
            VmInfoLoadAltHud1042Slice.ClassifyAltHudKeyword("Clip"));
    }

    [Fact]
    public void ResolveDefaultAltHudPicNames_MatchesGameTypeSwitch()
    {
        var doom = VmInfoLoadAltHud1042Slice.ResolveDefaultAltHudPicNames(VmGameTypeHud.Doom);
        Assert.Equal("MEDIA0", doom.HealthTexture);
        Assert.Equal("PSTRA0", doom.BerserkTexture);

        var heretic = VmInfoLoadAltHud1042Slice.ResolveDefaultAltHudPicNames(VmGameTypeHud.Heretic);
        Assert.Equal("ARTIPTN2", heretic.HealthTexture);
        Assert.Null(heretic.BerserkTexture);
    }

    [Fact]
    public void IsExplicitNullAltHudTextureToken_RecognizesSentinels()
    {
        Assert.True(VmInfoLoadAltHud1042Slice.IsExplicitNullAltHudTextureToken("0"));
        Assert.True(VmInfoLoadAltHud1042Slice.IsExplicitNullAltHudTextureToken("NULL"));
        Assert.True(VmInfoLoadAltHud1042Slice.IsExplicitNullAltHudTextureToken(""));
        Assert.False(VmInfoLoadAltHud1042Slice.IsExplicitNullAltHudTextureToken("MEDIA0"));
    }

    [Fact]
    public void EvaluateAltHudInventoryClass_UnknownOrNotInventory()
    {
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudInventoryClassStatus.UnknownClass,
            VmInfoLoadAltHud1042Slice.EvaluateAltHudInventoryClass(false, false));
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudInventoryClassStatus.NotInventory,
            VmInfoLoadAltHud1042Slice.EvaluateAltHudInventoryClass(true, false));
        Assert.Equal(
            VmInfoLoadAltHud1042Slice.AltHudInventoryClassStatus.Ok,
            VmInfoLoadAltHud1042Slice.EvaluateAltHudInventoryClass(true, true));
    }

    [Fact]
    public void BuildAltHudInventoryApply_NullTokenClearsIcon()
    {
        var clear = VmInfoLoadAltHud1042Slice.BuildAltHudInventoryApply(
            VmInfoLoadAltHud1042Slice.AltHudInventoryClassStatus.Ok,
            "0");
        Assert.True(clear.ApplyIcon);
        Assert.True(clear.IconExplicitlyInvalid);

        var tex = VmInfoLoadAltHud1042Slice.BuildAltHudInventoryApply(
            VmInfoLoadAltHud1042Slice.AltHudInventoryClassStatus.Ok,
            "ICONA0");
        Assert.Equal("ICONA0", tex.IconTextureName);
    }

    [Fact]
    public void FormatAltHudClassMessages_IncludeClassName()
    {
        Assert.Contains("Unknown", VmInfoLoadAltHud1042Slice.FormatUnknownAltHudClassMessage("Foo"));
        Assert.Contains("Invalid", VmInfoLoadAltHud1042Slice.FormatInvalidAltHudClassMessage("Bar"));
    }

    [Fact]
    public void NeedsDefaultSpriteSeed_OnlyWhenEmpty()
    {
        Assert.True(VmInfoLoadAltHud1042Slice.NeedsDefaultSpriteSeed(0));
        Assert.False(VmInfoLoadAltHud1042Slice.NeedsDefaultSpriteSeed(3));
    }

    [Fact]
    public void DefaultSpriteSeedNames_MatchesStaticInit()
    {
        Assert.Equal(["TNT1", "----", "####"], VmInfoLoadAltHud1042Slice.DefaultSpriteSeedNames);
    }

    [Fact]
    public void RunRegisterIdsForAllActorClasses_InvokesEach()
    {
        var calls = 0;
        VmInfoLoadAltHud1042Slice.RunRegisterIdsForAllActorClasses([() => calls++, () => calls++]);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void StaticInitPostActorLoadSteps_IncludesAltHud()
    {
        Assert.Contains("LoadAltHudStuff", VmInfoLoadAltHud1042Slice.StaticInitPostActorLoadSteps);
    }

    [Fact]
    public void ShouldAddActorClassToRegistry_RequiresActorDescendant()
    {
        Assert.True(VmInfoLoadAltHud1042Slice.ShouldAddActorClassToRegistry(true));
        Assert.False(VmInfoLoadAltHud1042Slice.ShouldAddActorClassToRegistry(false));
    }
}
