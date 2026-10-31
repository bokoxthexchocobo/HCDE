namespace HCDE.Playsim.Tests;

public class VmPlaylist1206SliceTests
{
    [Fact]
    public void ChangeList_JoinsRelativeSongsAndKeepsAFailedOpen()
    {
        var list = new VmPlaylist1206Slice();
        list.Songs.Add("kept");
        Assert.False(list.ChangeList("music/list.m3u", null));
        Assert.Equal("kept", Assert.Single(list.Songs));

        const string m3u = "# comment\n  song a.ogg\n\n/abs/b.ogg\nhttp://example/c.ogg\nsub\\d.ogg\n[Playlist]\n";
        Assert.True(list.ChangeList("music/list.m3u", m3u));
        Assert.Equal(new[]
        {
            "music/song a.ogg",
            "/abs/b.ogg",
            "http://example/c.ogg",
            "music/sub/d.ogg",
            "music/[Playlist]",
        }, list.Songs);

        Assert.False(list.ChangeList("music/list.m3u", "# only\n\n"));
        Assert.Empty(list.Songs);
        Assert.Equal(0u, list.Position);

        const string pls = "[playlist]\nFile1=one.ogg\nTitle1=Nope\nFile2=two.ogg\nFile=\n";
        Assert.True(list.ChangeList("sets/a.pls", pls));
        Assert.Equal(new[] { "sets/one.ogg", "sets/two.ogg", "sets/" }, list.Songs);
        Assert.True(list.ChangeList("flat.pls", "[playlist]\nFile=\nFile1=only.ogg\n"));
        Assert.Equal("only.ogg", Assert.Single(list.Songs));

        var bounds = Assert.Throws<VmPlaylist1206Slice.ScriptError>(() => list.ChangeList("sets/a.pls", "[playlist]\nFile\n"));
        Assert.Equal("VM execution aborted: array access out of bounds. string index 4 out of bounds (string length is 4)", bounds.Detail);
        Assert.True(list.ChangeList("music/list.m3u", "\u00A0hidden.ogg\n"));
        Assert.Equal("music/hidden.ogg", list.Songs[0]);
        Assert.True(list.ChangeList("dir/x.m3u", new string('A', 511) + "B\n"));
        Assert.Equal("dir/" + new string('A', 511), list.Songs[0]);
        Assert.Equal("dir/B", list.Songs[1]);
    }

    [Fact]
    public void Cursor_WrapsAndShuffleUsesRand()
    {
        var list = new VmPlaylist1206Slice();
        Assert.True(list.ChangeList("dir/x.m3u", "a\nb\n"));
        Assert.Equal(0, list.SetPosition(5));
        Assert.Equal(0, list.SetPosition(-1));
        Assert.Equal(1, list.SetPosition(1));
        Assert.Equal(0, list.Advance());
        Assert.Equal(0, list.GetPosition());
        Assert.Equal(1, list.Backup());
        Assert.Equal("dir/b", list.GetSong(1));
        Assert.Null(list.GetSong(-1));
        Assert.Null(list.GetSong(2));
        Assert.Equal(2, list.GetNumSongs());

        var empty = new VmPlaylist1206Slice();
        Assert.Equal(0, empty.Advance());
        Assert.Equal(-1, empty.Backup());

        var rolls = new Queue<int>(new[] { 2, 0, 1 });
        VmPlaylist1206Slice.NextRand = () => rolls.Dequeue();
        try
        {
            var three = new VmPlaylist1206Slice();
            Assert.True(three.ChangeList("z.m3u", "a\nb\nc\n"));
            three.Shuffle();
            Assert.Equal(new[] { "c", "b", "a" }, three.Songs);
            Assert.Equal(0, three.GetPosition());
        }
        finally
        {
            VmPlaylist1206Slice.ResetRand();
        }
    }
}
