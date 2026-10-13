namespace HCDE.Client.Tests;

public class VmMusicPlaylistCommandsTests
{
    [Fact]
    public void LoadPositionIsZeroBasedButPositionCommandIsOneBasedAndAcceptsNumericPrefix()
    {
        var list = new VmMusicPlaylist([]);
        var activations = new List<bool>();
        var commands = new VmMusicPlaylistCommands(list, activations.Add, _ => { });
        Assert.True(commands.Load("mix", "1tail", _ => System.Text.Encoding.ASCII.GetBytes("a\nb\nc\n"), () => 0));
        Assert.Equal(1, list.Position);
        commands.Position("  +3suffix");
        Assert.Equal(2, list.Position);
        commands.Position("invalid");
        Assert.Equal(0, list.Position);
        Assert.Equal(new[] { false, false, false }, activations);
    }

    [Fact]
    public void NavigationWrapsAndStatusReportsSelectedSong()
    {
        var list = new VmMusicPlaylist(["a", "b"]);
        var activations = new List<bool>();
        var messages = new List<string>();
        var commands = new VmMusicPlaylistCommands(list, activations.Add, messages.Add);
        commands.Previous();
        commands.Status();
        commands.Next();
        Assert.Equal(new[] { true, false }, activations);
        Assert.Equal("Song 2 of 2:\nb\n", Assert.Single(messages));
        Assert.Equal(0, list.Position);
    }

    [Fact]
    public void EmptyCommandsReportWithoutActivationAndFailedLoadPreservesState()
    {
        var list = new VmMusicPlaylist([]);
        var messages = new List<string>();
        var commands = new VmMusicPlaylistCommands(list, _ => throw new InvalidOperationException(), messages.Add);
        commands.Position(null);
        commands.Next();
        commands.Previous();
        commands.Status();
        Assert.Equal(4, messages.Count);
        Assert.All(messages, message => Assert.Equal("No playlist is playing.\n", message));
        Assert.False(commands.Load("missing", null, _ => null, () => 0));
        Assert.Equal("Could not load playlist: missing", messages[^1]);
    }

    [Fact]
    public void LoadShuffleIsCaseInsensitiveAndActivatesAfterPositionReset()
    {
        var list = new VmMusicPlaylist([]);
        var calls = 0;
        var commands = new VmMusicPlaylistCommands(list, backwards =>
        {
            Assert.False(backwards);
            Assert.Equal(0, list.Position);
            Assert.Equal("b", list.GetSong(0));
            calls++;
        }, _ => { });
        Assert.True(commands.Load("mix", "ShUfFlE", _ => System.Text.Encoding.ASCII.GetBytes("a\nb\n"), () => 1));
        Assert.Equal(1, calls);
    }
}
