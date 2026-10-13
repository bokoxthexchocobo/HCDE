namespace HCDE.Client.Tests;

public class VmMusicMidiDevicesTests
{
    [Fact]
    public void MenuPreservesProviderOrderDuplicatesAndAppendsBothSurfaces()
    {
        var options = new List<VmMusicMidiOption> { new("existing", 0) };
        var commands = new List<VmMusicMidiMenuCommand>();
        VmMusicMidiDevices.BuildMenu([new(12, "first", 7), new(-5, "default", 2), new(12, "duplicate", 1)],
            options.Add, commands.Add);
        Assert.Equal(new[] { "existing", "first", "default", "duplicate" }, options.Select(x => x.Text));
        Assert.Equal(new[] { 0f, 12f, -5f, 12f }, options.Select(x => x.Value));
        Assert.Equal(new[] { "snd_mididevice 12", "snd_mididevice -5", "snd_mididevice 12" }, commands.Select(x => x.Command));
        Assert.All(commands, x => Assert.True(x.Centered));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EitherMenuSurfaceCanBeAbsent(bool optionsOnly)
    {
        var calls = 0;
        VmMusicMidiDevices.BuildMenu([new(1, "one", 1)],
            optionsOnly ? _ => calls++ : null, optionsOnly ? null : _ => calls++);
        Assert.Equal(1, calls);
        VmMusicMidiDevices.BuildMenu([new(1, "one", 1)], null, null);
    }

    [Theory]
    [InlineData(1, "MIDIPORT")]
    [InlineData(2, "SYNTH")]
    [InlineData(3, "SQSYNTH")]
    [InlineData(4, "FMSYNTH")]
    [InlineData(5, "MAPPER")]
    [InlineData(6, "WAVETABLE")]
    [InlineData(7, "SWSYNTH")]
    [InlineData(65535, "")]
    public void ListingUsesNativeTechnologyLabelsAndSelectedColor(ushort technology, string label)
    {
        var lines = new List<string>();
        VmMusicMidiDevices.List([new(-5, "default", technology), new(0, "zero", technology), new(12, "twelve", technology)], -5, lines.Add);
        Assert.Equal(new[] { $"\u001c+-5. default : {label}\u001c-\n",
            $" 0. zero : {label}\u001c-\n", $" 12. twelve : {label}\u001c-\n" }, lines);
    }

    [Fact]
    public void DispatcherListsLiveDevicesRegardlessOfMusicEnablementAndIgnoresExtraArguments()
    {
        var selected = 1;
        var messages = new List<string>();
        var console = new VmMusicConsole<object>(new("", null), new([]),
            () => throw new InvalidOperationException(), (_, _) => false, _ => { }, _ => { }, messages.Add,
            listMidiDevices: () => VmMusicMidiDevices.List([new(1, "one", 1)], selected, messages.Add));
        Assert.True(console.TryExecute(["SND_LISTMIDIDEVICES", "ignored"]));
        selected = -5;
        Assert.True(console.TryExecute(["snd_listmididevices"]));
        Assert.Equal(new[] { "\u001c+ 1. one : MIDIPORT\u001c-\n", " 1. one : MIDIPORT\u001c-\n" }, messages);
    }

    [Fact]
    public void EmptyDeviceListDoesNotEmitOrAppendAnything()
    {
        VmMusicMidiDevices.List([], 0, _ => throw new InvalidOperationException());
        VmMusicMidiDevices.BuildMenu([], _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
    }
}
