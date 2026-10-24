namespace HCDE.Gamedata.Tests;

public class VmEMapInfoMusicNormalizeTests
{
    [Fact]
    public void NormalizeMusicName_PrefixesDoomTrack()
    {
        Assert.Equal("D_INTRO", VmEMapInfoMusicNormalize.NormalizeMusicName("intro", VmEMapInfoGameType.Doom));
    }

    [Fact]
    public void NormalizeMusicName_PreservesExplicitPath()
    {
        Assert.Equal("music/track.ogg", VmEMapInfoMusicNormalize.NormalizeMusicName("music/track.ogg", VmEMapInfoGameType.Doom));
    }
}
