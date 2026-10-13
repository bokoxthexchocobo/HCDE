namespace HCDE.Client.Tests;

public class VmMusicCommandArgumentsTests
{
    [Theory]
    [InlineData(" \t-12tail", -12)]
    [InlineData("+7", 7)]
    [InlineData("nonsense", 0)]
    [InlineData("0x10", 0)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void ParsesSignedAsciiDecimalPrefixAndIgnoresExtraArguments(string value, int expected)
    {
        var playlist = new VmMusicPlaylist(["listed"]);
        var calls = 0;
        VmMusicCommands.ChangeArguments(new VmMusicPlaybackState<object>("", null), playlist, true,
            ["changemus", "track", value, "ignored"], (name, order) =>
            {
                Assert.Equal(0, playlist.Count);
                Assert.Equal("track", name);
                Assert.Equal(expected, order);
                calls++;
                return false;
            }, _ => throw new InvalidOperationException());
        Assert.Equal(1, calls);
    }

    [Fact]
    public void MissingOrderDefaultsToZeroAndMissingSongReportsStatus()
    {
        var state = new VmMusicPlaybackState<object>("old", null);
        var playlist = new VmMusicPlaylist(["listed"]);
        var messages = new List<string>();
        VmMusicCommands.ChangeArguments(state, playlist, true, ["changemus"],
            (_, _) => throw new InvalidOperationException(), messages.Add);
        Assert.Equal(new[] { "currently playing old\n" }, messages);
        Assert.Equal(1, playlist.Count);
        VmMusicCommands.ChangeArguments(state, playlist, true, ["changemus", "track"],
            (_, order) => { Assert.Equal(0, order); return true; }, messages.Add);
    }

    [Fact]
    public void DisabledMusicSkipsOverflowParsingAndPreservesPlaylist()
    {
        var playlist = new VmMusicPlaylist(["listed"]);
        var messages = new List<string>();
        VmMusicCommands.ChangeArguments(new VmMusicPlaybackState<object>("", null), playlist, false,
            ["changemus", "track", "999999999999999"], (_, _) => throw new InvalidOperationException(), messages.Add);
        Assert.Equal(new[] { "Music is disabled\n" }, messages);
        Assert.Equal(1, playlist.Count);
    }

    [Fact]
    public void OverflowSurfacesAfterPlaylistClearWithoutCallingChange()
    {
        var playlist = new VmMusicPlaylist(["listed"]);
        Assert.Throws<OverflowException>(() => VmMusicCommands.ChangeArguments(
            new VmMusicPlaybackState<object>("", null), playlist, true, ["changemus", "track", "2147483648"],
            (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException()));
        Assert.Equal(0, playlist.Count);
    }
}
