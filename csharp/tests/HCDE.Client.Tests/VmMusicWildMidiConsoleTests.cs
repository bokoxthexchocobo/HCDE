using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConsoleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResourceCommandResolvesCurrentSongAndRestoresAfterExportOrFailure(bool fail)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        var state = new VmMusicPlaybackState<object>("current", new(), 2, true);
        var calls = new List<string>();
        VmMusicWildMidiManagedRenderer? renderer = null;
        var bytes = VmMusicSmf.Serialize(480, 500000, [new(0, 0x643c90), new(120, 0x3c80)]);
        var console = new VmMusicConsole<object>(state, new([]), () => true, (_, _) => true,
            _ => { calls.Add("stop"); state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { }); },
            _ => { }, _ => Assert.Fail("unexpected report"));
        console.ConfigureWildMidiWaveExport(-1,
            name => { Assert.Equal("current", name); calls.Add("resolve"); return 0; },
            _ => -1, _ => bytes,
            request =>
            {
                calls.Add("allocate");
                Assert.Equal(6, request.Device);
                Assert.Equal("patch.cfg", request.Soundfont);
                if (fail) throw new FileNotFoundException("missing configuration");
                return renderer = new(22050, _ => null, _ => 0);
            },
            (name, order, loop, force) =>
            {
                calls.Add("restore");
                Assert.Equal("current", name); Assert.Equal(2, order); Assert.True(loop); Assert.True(force);
                return true;
            });
        try
        {
            string[] arguments = ["writewave", "*", path, "0", "22050", "wildmidi", "patch.cfg"];
            if (fail) Assert.Throws<FileNotFoundException>(() => console.TryExecute(arguments));
            else
            {
                Assert.True(console.TryExecute(arguments));
                Assert.True(new FileInfo(path).Length > 68);
                Assert.Throws<ObjectDisposedException>(() => renderer!.GetVoiceCount());
            }
            Assert.Equal(new[] { "resolve", "stop", "allocate", "restore" }, calls);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OtherSynthRequestDoesNotAllocateWildMidiRenderer()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        Assert.Throws<NotSupportedException>(() => VmMusicWildMidiWaveDump.Write(source,
            new(7, null, "unused.wav", 0, 44100), _ => throw new InvalidOperationException("unexpected allocation")));
    }
}
