namespace HCDE.Client.Tests;

public class VmMusicPlaylistSignedBytesTests
{
    [Fact]
    public void SignedLeadingBytesAreSkippedBeforeSongName()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.m3u", _ => [0xef, 0xbb, 0xbf, 32, (byte)'a', 10]));
        Assert.Equal("a", playlist.GetSong(0));
    }

    [Fact]
    public void SignedPrefixCanExposeCommentAndPlsHeader()
    {
        var prefix = new byte[] { 0xff, (byte)'#', (byte)'x', 10, 0xfe };
        var data = prefix.Concat(System.Text.Encoding.ASCII.GetBytes("[playlist]\nTitle1=ignored\nFile1=song\n")).ToArray();
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.pls", _ => data));
        Assert.Equal(1, playlist.Count);
        Assert.Equal("song", playlist.GetSong(0));
    }

    [Fact]
    public void AllHighBytesLineIsSkippedWithoutEndingPlaylist()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.m3u", _ => [0x80, 0xff, 10, (byte)'a']));
        Assert.Equal(1, playlist.Count);
        Assert.Equal("a", playlist.GetSong(0));
    }

    [Fact]
    public void InternalHighBytesRemainInBytePreservingRepresentation()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.m3u", _ => [(byte)'a', 0xe9, 10], decodeUtf8: false));
        Assert.Equal("a\u00e9", playlist.GetSong(0));
    }

    [Fact]
    public void UnsignedCharBuildCanRetainLeadingHighBytes()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.m3u", _ => [0xe9, (byte)'a', 10], signedCharacters: false, decodeUtf8: false));
        Assert.Equal("\u00e9a", playlist.GetSong(0));
    }
}
