namespace HCDE.Client.Tests;

public class VmMusicPlaylistLoadTests
{
    private static byte[] Bytes(string value) => System.Text.Encoding.Latin1.GetBytes(value);

    [Fact]
    public void M3uSkipsCommentsAndResolvesRelativePathsWithoutTrimmingTrailingSpaces()
    {
        var list = new VmMusicPlaylist(["old"]);
        Assert.True(list.ChangeList("lists\\mix.m3u", _ => Bytes(" #EXTM3U\r\n\n  song.mid\r\nsub\\song.ogg \n/absolute.mid\nC:\\music\\a.mid\nhttps://host/song\n"), true));
        Assert.Equal(new[] { "lists/song.mid", "lists/sub/song.ogg ", "/absolute.mid", "C:/music/a.mid", "https://host/song" },
            Enumerable.Range(0, list.Count).Select(list.GetSong));
    }

    [Fact]
    public void PlsAcceptsOnlyNativeCaseSensitiveFileAssignmentsIncludingUnnumberedFile()
    {
        var list = new VmMusicPlaylist([]);
        Assert.True(list.ChangeList("mix.pls", _ => Bytes("[playlist]\nTitle1=no\nfile1=no\nFilex=no\nFile2=b.mid\nFile=a.mid\n")));
        Assert.Equal("b.mid", list.GetSong(0));
        Assert.Equal("a.mid", list.GetSong(1));
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void FailedOpenPreservesListButOpenedEmptyListClearsIt()
    {
        var list = new VmMusicPlaylist(["a", "b"]);
        list.SetPosition(1);
        Assert.False(list.ChangeList("missing", _ => null));
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list.Position);
        Assert.False(list.ChangeList("empty", _ => Bytes("# comment\n  \n")));
        Assert.Equal(0, list.Count);
        Assert.Equal(0, list.Position);
    }

    [Fact]
    public void LongLinesSplitAtNativeReaderBudget()
    {
        var list = new VmMusicPlaylist([]);
        Assert.True(list.ChangeList("mix.m3u", _ => Bytes(new string('a', 600) + "\n")));
        Assert.Equal(511, list.GetSong(0)!.Length);
        Assert.Equal(89, list.GetSong(1)!.Length);
    }
}
