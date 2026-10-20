namespace HCDE.Client.Tests;

public class VmDoomMusicPathRulesTests
{
    [Fact]
    public void IsKnownMusicExtension_MatchesNativeAllowlist()
    {
        Assert.True(VmDoomMusicPathRules.IsKnownMusicExtension("ogg"));
        Assert.True(VmDoomMusicPathRules.IsKnownMusicExtension("MUS"));
        Assert.False(VmDoomMusicPathRules.IsKnownMusicExtension("bsp"));
    }

    [Fact]
    public void HasMusicPathSegment_DetectsMusicFolder()
    {
        Assert.True(VmDoomMusicPathRules.HasMusicPathSegment("flats/music/d_e1m1.ogg"));
        Assert.False(VmDoomMusicPathRules.HasMusicPathSegment("sounds/ pistol.wav"));
    }

    [Fact]
    public void MusicPathMatchesName_ComparesStemCaseInsensitive()
    {
        Assert.True(VmDoomMusicPathRules.MusicPathMatchesName(@"music\D_E1M1.OGG", "d_e1m1"));
        Assert.False(VmDoomMusicPathRules.MusicPathMatchesName("music/d_e1m1.ogg", "e1m1"));
    }
}
