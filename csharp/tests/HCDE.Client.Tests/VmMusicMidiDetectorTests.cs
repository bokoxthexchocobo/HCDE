namespace HCDE.Client.Tests;

public class VmMusicMidiDetectorTests
{
    [Theory]
    [InlineData("MThd", VmMusicMidiType.Midi)]
    [InlineData("HMI-MIDISONG", VmMusicMidiType.Hmi)]
    [InlineData("HMIMIDIP", VmMusicMidiType.Hmi)]
    [InlineData("FORM1234XDIR", VmMusicMidiType.Xmi)]
    [InlineData("FORM1234XMID", VmMusicMidiType.Xmi)]
    [InlineData("CAT 1234XMID", VmMusicMidiType.Xmi)]
    [InlineData("RIFF1234MIDS", VmMusicMidiType.Mids)]
    [InlineData("RIFF1234WAVE", VmMusicMidiType.None)]
    [InlineData("mthd", VmMusicMidiType.None)]
    [InlineData("HMI", VmMusicMidiType.None)]
    public void RecognizesNativeSignaturesAndRejectsUnmatchedOrShortHeaders(string header, VmMusicMidiType expected)
        => Assert.Equal(expected, VmMusicMidiDetector.Identify(System.Text.Encoding.ASCII.GetBytes(header)));

    [Fact]
    public void MusSearchIncludesFinalPossibleOffsetAndTakesPrecedence()
    {
        var header = new byte[32];
        "MThd"u8.CopyTo(header);
        "MUS\u001a"u8.CopyTo(header.AsSpan(28));
        Assert.Equal(VmMusicMidiType.Mus, VmMusicMidiDetector.Identify(header));
        Assert.Equal(VmMusicMidiType.None, VmMusicMidiDetector.Identify([]));
    }

    [Fact]
    public void ReplayGainHashUsesConvertedDetectionWithoutInjectedProbe()
    {
        using var reader = new VmMusicByteReader("MThd"u8);
        var key = VmReplayGainHash.Create(reader, 4, 4, "bank", 0, _ => throw new InvalidOperationException());
        Assert.EndsWith(":4:BANK", key);
        Assert.Equal(0, reader.Tell());
    }
}
