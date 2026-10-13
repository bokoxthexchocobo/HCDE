namespace HCDE.Client.Tests;

public class VmMusicPlaylistShuffleTests
{
    [Fact]
    public void ShuffleUsesForwardNativeModuloSelectionAndResetsPosition()
    {
        var list = new VmMusicPlaylist(["a", "b", "c", "d"]);
        list.SetPosition(2);
        var random = new Queue<int>([7, 4, 3, int.MaxValue]);
        list.Shuffle(() => random.Dequeue());
        Assert.Empty(random);
        Assert.Equal(new[] { "d", "c", "a", "b" }, Enumerable.Range(0, list.Count).Select(list.GetSong));
        Assert.Equal(0, list.Position);
    }

    [Fact]
    public void EmptyListConsumesNoRandomAndSingletonConsumesOne()
    {
        var list = new VmMusicPlaylist([]);
        list.Shuffle(() => throw new InvalidOperationException("unexpected random request"));
        Assert.Equal(0, list.Position);
        list = new VmMusicPlaylist(["only"]);
        var calls = 0;
        list.Shuffle(() => { calls++; return 42; });
        Assert.Equal(1, calls);
        Assert.Equal("only", list.GetSong(0));
    }

    [Fact]
    public void ShufflePreservesDuplicateEntries()
    {
        var list = new VmMusicPlaylist(["a", "a", "b"]);
        list.Shuffle(() => int.MaxValue);
        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { "a", "a", "b" }, Enumerable.Range(0, list.Count).Select(list.GetSong).Order());
    }

    [Fact]
    public void InvalidRandomValueFailsBeforeFirstSwapOrPositionReset()
    {
        var list = new VmMusicPlaylist(["a", "b"]);
        list.SetPosition(1);
        Assert.Throws<InvalidOperationException>(() => list.Shuffle(() => -1));
        Assert.Equal("a", list.GetSong(0));
        Assert.Equal("b", list.GetSong(1));
        Assert.Equal(1, list.Position);
    }
}
