namespace HCDE.Client.Tests;

public class VmHcdeTauntTests
{
    [Fact]
    public void EncodeTauntDemoPayload_MatchesNativeWireFormat()
    {
        var payload = VmHcdeTaunt.EncodeTauntDemoPayload("doom");
        Assert.Equal(VmHcdeTaunt.DemoCommandTaunt, payload[0]);
        Assert.Equal(0, payload[1]);
        Assert.Equal("doom"u8.ToArray(), payload.AsSpan(2, 4).ToArray());
        Assert.Equal(0, payload[^1]);
    }

    [Fact]
    public void TryRunTauntCommand_WritesDemoCommandWhenInLevel()
    {
        var sink = new RecordingTauntSink();
        var expiry = 0UL;
        var result = VmHcdeTaunt.TryRunTauntCommand(
            new VmHcdeTauntCommandContext(true, new object(), 1000),
            "marine",
            ref expiry,
            sink);
        Assert.True(result.Succeeded);
        Assert.Equal([VmHcdeTaunt.DemoCommandTaunt, 0], sink.Bytes.Take(2));
        Assert.Contains("marine", sink.LastString);
    }

    [Fact]
    public void PlayOnPawn_InvokesSoundSink()
    {
        var sink = new RecordingSoundSink();
        VmHcdeTaunt.PlayOnPawn(new object(), "x", sink);
        Assert.Equal("*taunt-x", sink.LastSound);
    }

    private sealed class RecordingTauntSink : IVmHcdeTauntCommandSink
    {
        public List<byte> Bytes { get; } = new();
        public string? LastString { get; private set; }
        public void WriteInt8(byte value) => Bytes.Add(value);
        public void WriteString(string value) => LastString = value;
    }

    private sealed class RecordingSoundSink : IVmHcdeTauntSoundSink
    {
        public string? LastSound { get; private set; }
        public void PlayVoiceSound(object pawn, string soundName, float volume, int attenuation) => LastSound = soundName;
    }
}
