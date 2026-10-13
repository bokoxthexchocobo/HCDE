namespace HCDE.Client.Tests;

public class VmMusicReaderOwnershipTests
{
    private sealed record Song(VmMusicByteReader Reader);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OwningReaderSurvivesOpeningAndFailedStartUntilSongCloses(bool startSucceeds)
    {
        var state = new VmMusicPlaybackState<Song>("", null);
        var resources = new VmMusicResources(_ => 0, _ => -1, _ => 1, _ => [42], _ => { });
        Assert.Equal(startSucceeds, VmMusicChangeOpen.OpenAndStart(state, resources, "new", 0, false, 1, true,
            _ => { }, _ => new VmMusicResolvedSettings(-1, "", 0, 1),
            _ => { }, (_, _, _) => { }, _ => { }, (reader, _, _) => new Song(reader),
            (_, _, _, _) => startSucceeds, _ => throw new InvalidOperationException(), () => { }, () => { },
            transferReaderOwnership: true));
        var reader = Assert.IsType<Song>(state.Song).Reader;
        var bytes = new byte[1];
        Assert.Equal(1, reader.Read(bytes));
        Assert.Equal(42, bytes[0]);
        state.Stop(true, 0, () => { }, () => { }, _ => { }, song => song.Reader.Dispose());
        Assert.Throws<ObjectDisposedException>(() => reader.Tell());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedOpenAlwaysClosesUntransferredReader(bool throws)
    {
        var state = new VmMusicPlaybackState<Song>("", null);
        var resources = new VmMusicResources(_ => 0, _ => -1, _ => 1, _ => [42], _ => { });
        VmMusicByteReader? retained = null;
        bool Change() => VmMusicChangeOpen.OpenAndStart(state, resources, "new", 0, false, 1, true,
            _ => { }, (reader, _) =>
            {
                retained = reader;
                if (throws) throw new InvalidOperationException("open failed");
                return null;
            }, (_, _, _, _) => throw new InvalidOperationException(), _ => { }, () => { }, () => { },
            transferReaderOwnership: true);
        if (throws) Assert.Throws<InvalidOperationException>(() => Change());
        else Assert.False(Change());
        Assert.Throws<ObjectDisposedException>(() => retained!.Tell());
        Assert.Null(state.Song);
    }
}
