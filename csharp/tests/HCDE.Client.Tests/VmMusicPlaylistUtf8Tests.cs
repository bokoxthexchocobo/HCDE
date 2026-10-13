namespace HCDE.Client.Tests;

public class VmMusicPlaylistUtf8Tests
{
    [Theory]
    [InlineData("a\u00e9.mid")]
    [InlineData("sub/\u65e5\u672c.mid")]
    [InlineData("https://host/\u00e9.mid")]
    public void Utf8NamesDecodeAfterBytePathRules(string name)
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.m3u", _ => System.Text.Encoding.UTF8.GetBytes(name + "\n")));
        Assert.Equal(name, playlist.GetSong(0));
    }

    [Fact]
    public void NonAsciiPlaylistDirectoryIsNotReencodedAsLatin1()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("\u65e5\u672c/mix.m3u", _ => System.Text.Encoding.UTF8.GetBytes("a\u00e9.mid\n")));
        Assert.Equal("\u65e5\u672c/a\u00e9.mid", playlist.GetSong(0));
    }

    [Fact]
    public void PlsNamesBeginningWithNonAsciiDecodeAfterFileAssignment()
    {
        var playlist = new VmMusicPlaylist([]);
        Assert.True(playlist.ChangeList("mix.pls", _ => System.Text.Encoding.UTF8.GetBytes("[playlist]\nFile1=\u00e9.mid\n")));
        Assert.Equal("\u00e9.mid", playlist.GetSong(0));
    }

    [Fact]
    public void DecodedPlaylistSongOpensActualUnicodeFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "hcde-playlist-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var name = "a\u00e9.mid";
            File.WriteAllBytes(Path.Combine(directory, name), [42]);
            var playlist = new VmMusicPlaylist([]);
            Assert.True(playlist.ChangeList(Path.Combine(directory, "mix.m3u"),
                _ => System.Text.Encoding.UTF8.GetBytes(name + "\n")));
            var resources = new VmMusicResources(_ => -1, _ => -1, _ => 0, _ => [],
                _ => throw new InvalidOperationException());
            using var reader = resources.Open(playlist.GetSong(0)!);
            Assert.NotNull(reader);
            var bytes = new byte[1];
            Assert.Equal(1, reader.Read(bytes));
            Assert.Equal(42, bytes[0]);
        }
        finally
        {
            File.Delete(Path.Combine(directory, "a\u00e9.mid"));
            Directory.Delete(directory);
        }
    }
}
