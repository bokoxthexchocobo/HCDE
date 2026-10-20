namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchTextTests
{
    [Fact]
    public void IsDoomMusicHack_MatchesDoom()
    {
        Assert.True(VmDehackedPatchText.IsDoomMusicHack("doom"));
        Assert.Equal("MUSIC_DOOM", VmDehackedPatchText.ResolveMusicDoomKey());
    }
}
