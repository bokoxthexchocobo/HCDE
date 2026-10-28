namespace HCDE.Gamedata.Tests;

public class VmSndInfoExtendedDirectivesTests
{
    [Fact]
    public void TryParseMusicVolume_ReadsFloat()
    {
        var tokens = VmLockDefTokenize.Tokenize("D_READ_M 0.5");
        var i = 0;
        var list = new List<SndInfoMusicVolume>();
        var errors = new List<string>();
        Assert.True(VmSndInfoExtendedDirectives.TryParseMusicVolume(tokens, ref i, list, errors));
        Assert.Equal(0.5f, Assert.Single(list).Volume);
    }

    [Fact]
    public void TryParseModPlayer_RejectsUnknown()
    {
        var tokens = VmLockDefTokenize.Tokenize("song badplayer");
        var i = 0;
        var errors = new List<string>();
        Assert.False(VmSndInfoExtendedDirectives.TryParseModPlayer(tokens, ref i, new List<SndInfoModPlayer>(), errors));
    }

    [Fact]
    public void ParseDocument_ReadsExtendedDirectives()
    {
        const string text = """
            $musicvolume "D_E1M1" 0.75
            $musicalias alias1 target1
            $modplayer D_E1M1 dumb
            $mididevice D_E1M1 fluidsynth
            $rumbledef test 10 0.1 0.2 0.3 0.4
            $rumble snd rumbleid
            """;
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        Assert.Equal(0.75f, data.MusicVolumes[0].Volume);
        Assert.Equal("dumb", data.ModPlayers[0].Player);
        Assert.Equal("fluidsynth", data.MidiDevices[0].Device);
        Assert.Equal(10, data.RumbleDefs[0].Duration);
        Assert.Equal("rumbleid", data.RumbleMaps[0].RumbleId);
    }
}
