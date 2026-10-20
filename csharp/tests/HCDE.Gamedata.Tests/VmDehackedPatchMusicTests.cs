namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMusicTests
{
    [Fact]
    public void TryParseMusicLine_BuildsMusicKey()
    {
        Assert.True(VmDehackedPatchMusic.TryParseMusicLine("d_e1m1", "  newtrack  ", out var key, out var name));
        Assert.Equal("MUSIC_d_e1m1", key);
        Assert.Equal("newtrack", name);
    }
}
