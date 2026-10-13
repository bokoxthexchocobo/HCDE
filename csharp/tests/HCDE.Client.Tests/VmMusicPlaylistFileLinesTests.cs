namespace HCDE.Client.Tests;

public class VmMusicPlaylistFileLinesTests
{
    private static VmMusicPlaylist Load(string text)
    {
        var playlist = new VmMusicPlaylist([]);
        playlist.ChangeList("mix.m3u", _ => System.Text.Encoding.ASCII.GetBytes(text));
        return playlist;
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("song.mid", "song.mid")]
    [InlineData("song.mid\r\n", "song.mid")]
    [InlineData("so\rng.mid\n", "so\rng.mid")]
    public void FileLinesRetainFinalByteAndInternalCarriageReturns(string text, string expected)
    {
        var playlist = Load(text);
        Assert.Equal(1, playlist.Count);
        Assert.Equal(expected, playlist.GetSong(0));
    }

    [Fact]
    public void EmbeddedNullTerminatesVisibleStringButConsumesRestOfPhysicalLine()
    {
        var playlist = Load("first\0hidden\nsecond\n");
        Assert.Equal(2, playlist.Count);
        Assert.Equal("first", playlist.GetSong(0));
        Assert.Equal("second", playlist.GetSong(1));
    }

    [Fact]
    public void NullAtStartSkipsWholePhysicalLineRatherThanEndingPlaylist()
    {
        var playlist = Load("\0hidden\nlast");
        Assert.Equal(1, playlist.Count);
        Assert.Equal("last", playlist.GetSong(0));
    }

    [Fact]
    public void FullBufferFollowedByOneByteAtEofRetainsBothChunks()
    {
        var playlist = Load(new string('a', 512));
        Assert.Equal(2, playlist.Count);
        Assert.Equal(new string('a', 511), playlist.GetSong(0));
        Assert.Equal("a", playlist.GetSong(1));
    }
}
