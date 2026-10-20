namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchTextTests
{
    [Fact]
    public void IsDoomMusicHack_MatchesDoom()
    {
        Assert.True(VmDehackedPatchText.IsDoomMusicHack("doom"));
        Assert.Equal("MUSIC_DOOM", VmDehackedPatchText.ResolveMusicDoomKey());
    }

    [Fact]
    public void TryReplaceSpriteName_RequiresFourCharSprite()
    {
        Assert.True(VmDehackedPatchText.TryReplaceSpriteName(4, "ABCD", "WXYZ", out var o, out var n));
        Assert.Equal("ABCD", o);
        Assert.Equal("WXYZ", n);
    }
}
