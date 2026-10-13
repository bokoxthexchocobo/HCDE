using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Client.Tests;

public class WadAudioMixerTests
{
    [Fact]
    public void ClientSoundLengthUsesConfiguredArchiveReturnsSecondsAndSurvivesRestore()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundAliases = new Dictionary<string, string> { ["alias"] = "sound" },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["missing"] = "ABSENT" } };
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "missing", "alias" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(1000), out var error), error);
        foreach (var sound in new[] { "SOUND", "alias", "group" })
        {
            Assert.True(host.TryGetSoundLength(sound, out var seconds, out error), error);
            Assert.Equal(0.002, seconds);
        }
        var snapshot = host.Simulation.CaptureState(); host.Simulation.RestoreState(snapshot);
        Assert.True(host.TryGetSoundLength("alias", out var duration, out error), error);
        Assert.Equal(0.002, duration); Assert.Equal(0, host.GetSoundPlayingCount());
        Assert.True(host.TryGetSoundLength("unknown", out duration, out error), error); Assert.Equal(0, duration);
    }

    [Fact]
    public void ClientSoundLengthReportsMissingArchiveAndCorruptSoundWithoutPlayback()
    {
        var level = Level();
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.False(host.TryGetSoundLength("sound", out var seconds, out var error));
        Assert.Equal(0, seconds); Assert.Equal("audio-sound-archive-missing", error);
        var wad = Wad(1000); wad[12] = 2;
        Assert.True(host.TrySetSoundArchive(wad, out error), error);
        Assert.False(host.TryGetSoundLength("sound", out seconds, out error));
        Assert.Equal(0, seconds); Assert.Equal("sound-format-unsupported", error);
        Assert.Equal(0, host.GetSoundPlayingCount());
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.TryGetSoundLength("sound", out _, out _));
    }

    [Theory]
    [InlineData("sound")]
    [InlineData("alias")]
    [InlineData("group")]
    public void DifferentlyCasedSoundSettingsApplyVolumePitchAndRolloff(string sound)
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "alias" } };
        var rolloff = new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
            { ["SOUND"] = new(Volume: 0.5f, DefPitch: 2, Rolloff: rolloff),
              ["ALIAS"] = new(Volume: 0.5f, DefPitch: 2, Rolloff: rolloff),
              ["GROUP"] = new(Volume: 0.5f, DefPitch: 2, Rolloff: rolloff) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, sound, out var handle, out var error,
            nextRandom: () => 0, loop: true), error);
        Assert.True(mixer.TryGetChannelRolloff(handle, out var actual)); Assert.Equal(rolloff, actual);
        Assert.Equal(new short[] { -16384, -16384 }, mixer.Mix(2));
        Assert.True(mixer.TryGetSoundLengthMilliseconds(Wad(1000), level, sound, out var duration, out error), error);
        Assert.Equal(2u, duration);
    }

    [Theory]
    [InlineData("SOUND")]
    [InlineData("AlIaS")]
    [InlineData("GROUP")]
    public void CanonicalSoundNamesResolveAcrossCaseSensitiveMappings(string sound)
    {
        var level = Level();
        level.SoundAliases = new Dictionary<string, string> { ["alias"] = "SOUND" };
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "ALIAS" } };
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
            { ["sound"] = new(Volume: 0.5f), ["alias"] = new(Volume: 0.5f), ["group"] = new(Volume: 0.5f) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryGetSoundLengthMilliseconds(Wad(1000), level, sound, out var duration, out var error), error);
        Assert.Equal(2u, duration);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, sound, out _, out error, nextRandom: () => 0), error);
        Assert.Equal(1, mixer.GetSoundPlayingCount(sound.ToLowerInvariant()));
        Assert.Equal(-16384, mixer.Mix(1)[0]);
    }

    [Theory]
    [InlineData("unknown", 0)]
    [InlineData("empty", 0)]
    [InlineData("nested", 2)]
    public void DurationQueriesHandleInvalidNamesAndNestedOrEmptyRandomGroups(string sound, uint expected)
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>
            { ["empty"] = Array.Empty<string>(), ["inner"] = new[] { "unknown", "alias" },
              ["nested"] = new[] { "empty", "inner", "unknown" } };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryGetSoundLengthMilliseconds(Wad(1000), level, sound, out var length, out var error), error);
        Assert.Equal(expected, length); Assert.Null(error);
        Assert.Equal(0, mixer.GetSoundPlayingCount());
        Assert.False(HCDE.MapLoader.SoundResourceResolver.TryResolve(level, "unknown", out _, out error));
        Assert.Equal("sound-resource-missing: unknown", error);
    }

    [Theory]
    [InlineData("sound", 2)]
    [InlineData("alias", 2)]
    [InlineData("group", 2)]
    [InlineData("missing", 0)]
    public void SoundLengthResolvesAliasesAndLongestRandomChoiceWithoutPlayback(string sound, uint expected)
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>
            { ["group"] = new[] { "missing", "alias", "alias" } };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryGetSoundLengthMilliseconds(Wad(1000), level, sound, out var length, out var error), error);
        Assert.Equal(expected, length); Assert.Equal(0, mixer.GetSoundPlayingCount());
    }

    [Fact]
    public void SoundLengthRejectsRandomAliasCycleAndBadEncoding()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "alias" } };
        level.SoundAliases = new Dictionary<string, string> { ["alias"] = "group" };
        var mixer = new AudioMixer();
        Assert.False(mixer.TryGetSoundLengthMilliseconds(Wad(1000), level, "group", out var length, out var error));
        Assert.Equal(0u, length); Assert.Equal("sound-length-cycle", error);
        var wad = Wad(1000); wad[12] = 2;
        Assert.False(mixer.TryGetSoundLengthMilliseconds(wad, Level(), "sound", out length, out error));
        Assert.Equal(0u, length); Assert.Equal("sound-format-unsupported", error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoreReconcilesAudioCameraAndKeepsConsolePlayer(bool cameraInSnapshot)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 20 }] };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.GamePaused = true;
        Assert.True(host.TrySetAudioConsolePlayer(1, out var error), error);
        HCDE.Playsim.Actor? camera = cameraInSnapshot ? host.Simulation.AddBot(50, 0) : null;
        var snapshot = host.Simulation.CaptureState();
        camera ??= host.Simulation.AddBot(50, 0);
        Assert.True(host.TrySetAudioListener(camera.Id, out error), error);
        host.Tick(default); Assert.Equal(50, host.AudioListenerPosition!.Value.X);
        if (!cameraInSnapshot) camera.Destroy();
        host.Simulation.RestoreState(snapshot);
        Assert.Null(host.AudioListenerPosition);
        host.Tick(default); Assert.Equal(cameraInSnapshot ? 50 : 20, host.AudioListenerPosition!.Value.X);
    }

    [Fact]
    public void LevelExitClearsReportedAudioListenerPosition()
    {
        using var host = ClientHost.DemoRoom();
        host.GamePaused = true;
        host.Tick(default); Assert.NotNull(host.AudioListenerPosition);
        host.Simulation.MarkExited(false);
        Assert.Null(host.AudioListenerPosition); Assert.Equal(0, host.GetSoundPlayingCount());
        host.Tick(default); Assert.Null(host.AudioListenerPosition);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AudioConsolePlayerSelectionRoutesLocalViewAndPreservesSelectionOnError(bool inertCamera)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 },
            new LevelThing { Type = 2, X = 20 }, new LevelThing { Type = 14, X = 50 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var first = host.Simulation.Players.Single(p => p.PlayerNum == 0);
        var selected = host.Simulation.Players.Single(p => p.PlayerNum == 1);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TrySetAudioConsolePlayer(1, out error), error);
        if (inertCamera)
            Assert.True(host.TrySetAudioListener(host.Simulation.Actors.Single(a => a.DoomEdNum == 14).Id, out error), error);
        HCDE.Playsim.ActorSoundActions.StartSound(first, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(0, host.GetSoundPlayingCount());
        HCDE.Playsim.ActorSoundActions.StartSound(selected, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(1, host.GetSoundPlayingCount());
        host.Audio.StopAllChannels();
        Assert.False(host.TrySetAudioConsolePlayer(200, out error));
        Assert.Equal("audio-console-player-missing", error);
        HCDE.Playsim.ActorSoundActions.StartSound(selected, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(1, host.GetSoundPlayingCount());
        host.Audio.StopAllChannels();
        selected.Destroy();
        HCDE.Playsim.ActorSoundActions.StartSound(first, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(0, host.GetSoundPlayingCount());
        Assert.True(host.TrySetAudioConsolePlayer(null, out error), error);
        HCDE.Playsim.ActorSoundActions.StartSound(first, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(1, host.GetSoundPlayingCount()); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Fact]
    public void AudioConsolePlayerSelectionSetsDefaultListenerPosition()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 20 }] };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.GamePaused = true;
        Assert.True(host.TrySetAudioConsolePlayer(1, out var error), error);
        host.Tick(default); Assert.Equal(20, host.AudioListenerPosition!.Value.X);
        host.Simulation.Players.Single(p => p.PlayerNum == 1).Destroy();
        host.Tick(default); Assert.Null(host.AudioListenerPosition);
        Assert.True(host.TrySetAudioConsolePlayer(null, out error), error);
        host.Tick(default); Assert.Equal(0, host.AudioListenerPosition!.Value.X);
    }

    [Theory]
    [InlineData(14, false, true)]
    [InlineData(3004, false, false)]
    [InlineData(14, true, false)]
    public void PlayerLocalSoundsRemainAudibleThroughInertCameraOnly(int cameraType, bool destroyed, bool audible)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = cameraType }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var player = host.Simulation.Players.Single(); var camera = host.Simulation.Actors.Single(a => a.Id != player.Id);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TrySetAudioListener(camera.Id, out error), error);
        if (destroyed) camera.Destroy();
        HCDE.Playsim.ActorSoundActions.StartSound(player, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(audible ? 1 : 0, host.GetSoundPlayingCount());
        Assert.Equal(audible ? -32768 : 0, host.Audio.Mix(1)[0]);
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Fact]
    public void SelectedAudioCameraControlsDistanceAndCanResetToPlayer()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.GamePaused = true;
        var source = host.Simulation.AddBot(20, 0); var camera = host.Simulation.AddBot(20, 0);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out _, out var error,
            loop: true, noPause: true, entityChannel: 4), error);
        host.Tick(default); Assert.Equal(0, host.LastMix[0]);
        Assert.True(host.TrySetAudioListener(camera.Id, out error), error);
        host.Tick(default); Assert.Equal(-32768, host.LastMix[0]);
        Assert.Equal(20, host.AudioListenerPosition!.Value.X);
        Assert.False(host.TrySetAudioListener(uint.MaxValue, out error));
        Assert.Equal("audio-listener-actor-missing", error);
        host.Tick(default); Assert.Equal(-32768, host.LastMix[0]);
        camera.Destroy(); host.Tick(default);
        Assert.Null(host.AudioListenerPosition); Assert.Equal(0, host.LastMix[0]);
        Assert.True(host.TrySetAudioListener(null, out error), error);
        host.Tick(default); Assert.NotNull(host.AudioListenerPosition); Assert.Equal(0, host.LastMix[0]);
    }

    [Fact]
    public void LocalSoundRequestsFollowSelectedCamera()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var camera = host.Simulation.AddBot(20, 0); var player = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TrySetAudioListener(camera.Id, out error), error);
        HCDE.Playsim.ActorSoundActions.StartSound(player, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(0, host.GetSoundPlayingCount());
        HCDE.Playsim.ActorSoundActions.StartSound(camera, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Local);
        Assert.Equal(1, host.GetSoundPlayingCount());
        Assert.Equal(-32768, host.Audio.Mix(1)[0]); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(5, 20, true)]
    [InlineData(20, 5, false)]
    public void NearbySoundLimitReadsActorMovementBeforeNextAudioTick(int initial, int moved, bool starts)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
                { ["sound"] = new(NearLimit: 1, LimitRange: 100) },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 100) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.AddBot(initial, 0); var target = host.Simulation.AddBot(0, 0);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var original,
            out var error, loop: true, entityChannel: 4), error);
        source.X = HCDE.Playsim.Fixed.FromInt(moved);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", target.Id, out var next,
            out error, entityChannel: 4), error);
        Assert.Equal(starts, next != Guid.Empty);
        Assert.True(host.Audio.IsPlaying(original));
        Assert.Equal(starts ? 2 : 1, host.GetSoundPlayingCount("sound"));
    }

    [Theory]
    [InlineData(false, false, 4, false)]
    [InlineData(false, true, 4, true)]
    [InlineData(false, false, 0, true)]
    [InlineData(true, false, 4, false)]
    [InlineData(true, true, 4, true)]
    public void PausedOneShotReplacesSelectedChannelBeforeSuppression(bool positioned, bool overlap,
        int channel, bool survives)
    {
        using var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        var origin = positioned ? new HCDE.Playsim.SimulationSoundOrigin(1, 2, 3, -1)
            : (HCDE.Playsim.SimulationSoundOrigin?)null;
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var original,
            out var error, loop: true, attenuation: 0, entityChannel: channel, detachedOrigin: origin), error);
        host.Audio.Paused = true;
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "alias", actor.Id, out var suppressed,
            out error, attenuation: 0, entityChannel: channel, overlap: overlap, detachedOrigin: origin), error);
        Assert.Equal(Guid.Empty, suppressed); Assert.Equal(survives, host.Audio.IsPlaying(original));
        host.Audio.Paused = false;
        Assert.Equal(survives ? -32768 : 0, host.Audio.Mix(1)[0]);
    }

    [Theory]
    [InlineData(4, false, false, false)]
    [InlineData(4, true, false, true)]
    [InlineData(0, false, false, true)]
    [InlineData(1, false, true, false)]
    public void ActorPlaybackReplacesDirectMixerChannelsWithNativeOverlapRules(int channel, bool overlap,
        bool magic, bool survives)
    {
        using var host = ClientHost.DemoRoom();
        var actor = host.Simulation.Players.Single();
        var level = Level();
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error,
            loop: true, attenuation: 0, sourceActorId: actor.Id, entityChannel: channel), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var second, out error,
            loop: true, attenuation: 0, sourceActorId: actor.Id, entityChannel: channel), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var otherActor, out error,
            loop: true, attenuation: 0, sourceActorId: actor.Id + 1, entityChannel: channel), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var otherChannel, out error,
            loop: true, attenuation: 0, sourceActorId: actor.Id, entityChannel: 2), error);
        host.MagicSilenceCompatibility = magic;
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "alias", actor.Id, out var replacement,
            out error, loop: true, attenuation: 0, entityChannel: magic ? 4 : channel, overlap: overlap), error);
        Assert.Equal(survives, host.Audio.IsPlaying(first)); Assert.Equal(survives, host.Audio.IsPlaying(second));
        Assert.True(host.Audio.IsPlaying(replacement)); Assert.True(host.Audio.IsPlaying(otherActor));
        Assert.True(host.Audio.IsPlaying(otherChannel));
        Assert.Equal(survives ? 4 : 2, host.GetActorSoundPlayingCount(actor.Id));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PositionalStartsWithoutListenerUseZeroOriginAndKeepSourceHeight(bool positioned, bool listenerHeight)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.AddBot(0, 0); source.Z = HCDE.Playsim.Fixed.FromInt(15);
        host.Simulation.Players.Single().Destroy();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var handle,
            out var error, loop: true, listenerHeight: listenerHeight, entityChannel: 4,
            detachedOrigin: positioned ? new HCDE.Playsim.SimulationSoundOrigin(0, 15, 0, -1) : null), error);
        Assert.NotEqual(Guid.Empty, handle);
        Assert.Equal(-16384, host.Audio.Mix(1)[0]);
        Assert.Equal(positioned ? 0 : 1, host.GetActorSoundPlayingCount(source.Id));
        Assert.Equal(positioned, host.IsPositionedSoundPlaying(4));
        Assert.True(host.Audio.StopChannel(handle));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out error), error);
        HCDE.Playsim.ActorSoundActions.StartSoundAt(host.Simulation, 0, 0, 15, "sound", 4,
            HCDE.Playsim.SimulationSoundFlags.Loop | HCDE.Playsim.SimulationSoundFlags.ListenerZ);
        Assert.Empty(host.LastSoundRequestErrors); Assert.Equal(-16384, host.Audio.Mix(1)[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingListenerUpdatesActorSourcesAgainstNativeZeroOrigin(bool listenerHeight)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1, X = 0, Y = 0 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.GamePaused = true;
        var source = host.Simulation.AddBot(15, 0);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var handle, out var error,
            loop: true, noPause: true, sourceActorId: source.Id, entityChannel: 4), error);
        Assert.True(host.TryBindAudioSource(handle, source.Id, out error, listenerHeight), error);
        host.Simulation.Players.Single().Destroy();
        host.Tick(default); Assert.Null(host.AudioListenerPosition); Assert.Equal(-16384, host.LastMix[0]);
        source.X = HCDE.Playsim.Fixed.FromInt(20);
        host.Tick(default); Assert.Equal(0, host.LastMix[0]);
        source.X = HCDE.Playsim.Fixed.FromInt(0); source.Z = HCDE.Playsim.Fixed.FromInt(20);
        host.Tick(default); Assert.Equal(0, host.LastMix[0]);
        source.Z = HCDE.Playsim.Fixed.FromInt(0);
        host.Tick(default); Assert.Equal(-32768, host.LastMix[0]);
        Assert.True(host.Audio.IsPlaying(handle));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectMixerSourcesFollowActorMovementAndRelinkedDestination(bool paused)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1, X = 0, Y = 0 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.GamePaused = paused;
        var source = host.Simulation.AddBot(15, 0);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var handle, out var error,
            loop: true, noPause: true, sourceActorId: source.Id, entityChannel: 4), error);
        host.Tick(default);
        Assert.Equal(paused ? -16384 : -15384, host.LastMix[0]);
        source.X = HCDE.Playsim.Fixed.FromInt(20);
        host.Tick(default); Assert.Equal(paused ? 0 : 1000, host.LastMix[0]);
        var target = host.Simulation.Players.Single();
        Assert.True(host.TryRelinkActorSounds(source.Id, target.Id, out var moved, out error), error);
        Assert.Equal(1, moved);
        host.Tick(default); Assert.Equal(paused ? -32768 : -31768, host.LastMix[0]);
        Assert.True(host.Audio.IsPlaying(handle));
        Assert.Equal(1, host.GetActorSoundPlayingCount(target.Id));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    public void DirectMixerActorSoundsDetachOnRelinkOrDestruction(bool loop, bool cutoff, bool destroy)
    {
        using var host = ClientHost.DemoRoom();
        host.SoundCutoffCompatibility = cutoff;
        var actor = host.Simulation.AddBot(100, 0);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out var handle, out var error,
            loop: loop, attenuation: 0, sourceActorId: actor.Id, entityChannel: 4), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out var unrelated, out error,
            loop: true, sourceActorId: host.Simulation.Players.First().Id, entityChannel: 4), error);
        if (destroy) actor.Destroy();
        else
        {
            Assert.True(host.TryRelinkActorSounds(actor.Id, null, out var count, out error), error);
            Assert.Equal(1, count);
            Assert.True(host.TryRelinkActorSounds(actor.Id, null, out count, out error), error);
            Assert.Equal(0, count);
        }
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(!loop && !cutoff, host.Audio.IsPlaying(handle));
        Assert.Equal(!loop && !cutoff, host.IsPositionedSoundPlaying(4, "sound"));
        Assert.True(host.Audio.IsPlaying(unrelated));
    }

    [Fact]
    public void DirectMixerRelinkTransfersAllChannelsAndDestinationDestructionStopsLoops()
    {
        using var host = ClientHost.DemoRoom();
        var source = host.Simulation.AddBot(100, 0); var target = host.Simulation.AddBot(200, 0);
        foreach (var channel in new[] { 2, 4 })
            Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out _, out var error,
                loop: true, attenuation: 0, sourceActorId: source.Id, entityChannel: channel), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out var existing, out var otherError,
            loop: true, attenuation: 0, sourceActorId: target.Id, entityChannel: 4), otherError);
        Assert.False(host.TryRelinkActorSounds(source.Id, uint.MaxValue, out var moved, out otherError));
        Assert.Equal(0, moved); Assert.Equal("audio-destination-actor-missing", otherError);
        Assert.Equal(2, host.GetActorSoundPlayingCount(source.Id));
        Assert.True(host.TryRelinkActorSounds(source.Id, target.Id, out moved, out otherError), otherError);
        Assert.Equal(2, moved); Assert.Equal(0, host.GetActorSoundPlayingCount(source.Id));
        Assert.Equal(3, host.GetActorSoundPlayingCount(target.Id));
        source.Destroy(); Assert.Equal(3, host.GetActorSoundPlayingCount(target.Id));
        target.Destroy(); Assert.False(host.Audio.IsPlaying(existing)); Assert.Equal(0, host.GetSoundPlayingCount());
    }

    [Fact]
    public void BindingUnownedPlaybackEstablishesActorOwnershipForQueriesAndDestruction()
    {
        using var host = ClientHost.DemoRoom();
        var actor = host.Simulation.AddBot(100, 0);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out var handle, out var playbackError,
            loop: true, attenuation: 0), playbackError);
        Assert.True(host.TryBindAudioSource(handle, actor.Id, out var error), error);
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id));
        Assert.False(host.IsPositionedSoundPlaying());
        actor.Destroy(); Assert.False(host.Audio.IsPlaying(handle));
    }

    [Theory]
    [InlineData(0, false, 1)]
    [InlineData(4, false, 1)]
    [InlineData(-2, false, 3)]
    [InlineData(4, true, 3)]
    public void DirectActorChannelsParticipateInQueriesAndChannelStops(int channel, bool magic, int stopped)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        foreach (var slot in new[] { 0, 2, 4 })
            Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error,
                loop: true, sourceActorId: actor.Id, entityChannel: slot), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var unrelated, out var otherError,
            loop: true, sourceActorId: actor.Id + 1, entityChannel: 4), otherError);
        host.MagicSilenceCompatibility = magic;
        Assert.Equal(3, host.GetActorSoundPlayingCount(actor.Id, "SOUND"));
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id, entityChannel: 0));
        Assert.True(host.IsActorSoundPlaying(actor.Id, 0, "SOUND"));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "missing"));
        Assert.Equal(stopped, host.StopActorSound(actor.Id, channel));
        Assert.Equal(3 - stopped, host.GetActorSoundPlayingCount(actor.Id));
        Assert.True(host.Audio.IsPlaying(unrelated));
    }

    [Theory]
    [InlineData(4, 2, false, 2)]
    [InlineData(4, 2, true, 2)]
    [InlineData(0, 0, false, 3)]
    [InlineData(0, 0, true, 3)]
    [InlineData(8, 10, false, 0)]
    public void DirectActorRangeStopsUseNativeBounds(int minimum, int maximum, bool magic, int stopped)
    {
        using var host = ClientHost.DemoRoom();
        var actor = host.Simulation.Players.First();
        foreach (var slot in new[] { 0, 2, 4 })
            Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out _, out var error,
                loop: true, sourceActorId: actor.Id, entityChannel: slot), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), Level(), "sound", out var unrelated, out var otherError,
            loop: true, entityChannel: 4), otherError);
        host.MagicSilenceCompatibility = magic;
        Assert.Equal(stopped, host.StopActorSounds(actor.Id, minimum, maximum));
        Assert.Equal(3 - stopped, host.GetActorSoundPlayingCount(actor.Id));
        Assert.True(host.Audio.IsPlaying(unrelated));
    }

    [Fact]
    public void ActorLoopDuplicateGuardSeesDirectMixerPlayback()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var handle, out error,
            loop: true, attenuation: 0, sourceActorId: actor.Id, entityChannel: 4), error);
        QueueAcsSound(host, actor, 4, loop: true);
        host.Tick(default);
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id));
        Assert.True(host.Audio.IsPlaying(handle));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false, -12288)]
    [InlineData(true, -4096)]
    public void ActorVolumeActionIncludesDirectMixerChannels(bool magic, int expected)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); var other = host.Simulation.AddBot(100, 0);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        foreach (var channel in new[] { 4, 5 })
            Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out _, out error,
                loop: true, volume: 0.25f, attenuation: 0, sourceActorId: actor.Id, entityChannel: channel), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out _, out error,
            loop: true, volume: 0.125f, attenuation: 0, sourceActorId: other.Id, entityChannel: 4), error);
        host.MagicSilenceCompatibility = magic;
        HCDE.Playsim.ActorSoundActions.SoundVolume(actor, 4, 0);
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(expected, host.Audio.Mix(1)[0]);
    }

    [Fact]
    public void ActorVolumeSelectionClampsAndRejectsInvalidVolumeBeforeMutation()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out _, out var error,
            loop: true, volume: 0.25f, sourceActorId: 1, entityChannel: 4), error);
        Assert.False(mixer.TrySetActorSoundVolume(1, -1, float.NaN, out var changed, out error));
        Assert.Equal(0, changed); Assert.Equal("audio-volume-invalid", error);
        Assert.Equal(-8192, mixer.Mix(1)[0]);
        Assert.True(mixer.TrySetActorSoundVolume(1, 5, 0, out changed, out error), error);
        Assert.Equal(0, changed);
        Assert.True(mixer.TrySetActorSoundVolume(1, 4, -1, out changed, out error), error);
        Assert.Equal(1, changed); Assert.Equal(0, mixer.Mix(1)[0]);
        Assert.True(mixer.TrySetActorSoundVolume(1, -1, 2, out changed, out error), error);
        Assert.Equal(1, changed); Assert.Equal(-32768, mixer.Mix(1)[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NamedActorStopIgnoresChannelAndPreservesOtherSources(bool magic)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["other"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); var other = host.Simulation.AddBot(100, 0);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        for (var channel = 4; channel <= 6; channel++)
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, channel == 6 ? "other" : "sound", actor.Id,
                out _, out error, loop: true, attenuation: 0, entityChannel: channel), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", other.Id, out var otherHandle,
            out error, loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", 0, out var positioned,
            out error, loop: true, attenuation: 0, entityChannel: 4,
            detachedOrigin: new HCDE.Playsim.SimulationSoundOrigin(1, 2, 3, -1)), error);
        host.MagicSilenceCompatibility = magic;
        HCDE.Playsim.ActorSoundActions.StopSound(actor, 6, "SOUND");
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id, "sound"));
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id, "other"));
        Assert.True(host.Audio.IsPlaying(otherHandle)); Assert.True(host.Audio.IsPlaying(positioned));
        Assert.Equal(0, host.StopActorSound(actor.Id, 4, "missing"));
        Assert.Equal(1, host.StopActorSound(actor.Id, 4, "other"));
        Assert.Equal(2, host.GetSoundPlayingCount());
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("SOUND", 2)]
    [InlineData("missing", 0)]
    public void NativeActorPitchSelectorUsesExactChannelOrSoundAcrossChannels(string? sound, int expected)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["other"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); var other = host.Simulation.AddBot(100, 0);
        for (var channel = 4; channel <= 6; channel++)
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, channel == 6 ? "other" : "sound", actor.Id,
                out _, out var error, loop: true, attenuation: 0, entityChannel: channel), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", other.Id, out _, out var otherError,
            loop: true, attenuation: 0, entityChannel: 4), otherError);
        Assert.True(host.Audio.TrySetActorSoundPitch(actor.Id, 4, 2, out var changed, out otherError, sound), otherError);
        Assert.Equal(expected, changed);
        Assert.False(host.Audio.TrySetActorSoundPitch(actor.Id, 4, float.NaN, out changed, out otherError, sound));
        Assert.Equal(0, changed); Assert.Equal("audio-pitch-invalid", otherError);
        Assert.Equal(3, host.GetActorSoundPlayingCount(actor.Id)); Assert.Equal(1, host.GetActorSoundPlayingCount(other.Id));
    }

    [Fact]
    public void NamedActorPitchActionReachesClientAndChangesLoopSamples()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out error,
            loop: true, attenuation: 0, entityChannel: 5), error);
        HCDE.Playsim.ActorSoundActions.SoundPitch(actor, 4, 2, "sound");
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal((short)-31768, host.LastMix[0]); Assert.Equal((short)-31768, host.LastMix[1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NonActorQueriesDistinguishSourceTypesAndDetachedPlayback(bool loop)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out var error,
            loop: loop, attenuation: 0, entityChannel: 4), error);
        Assert.False(host.IsUnpositionedSoundPlaying()); Assert.False(host.IsPositionedSoundPlaying());
        Assert.True(host.TryRelinkActorSounds(actor.Id, null, out _, out error), error);
        Assert.Equal(!loop, host.IsPositionedSoundPlaying(4, "SOUND"));
        Assert.False(host.IsPositionedSoundPlaying(5)); Assert.False(host.IsPositionedSoundPlaying(-1));
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out _, out error,
            loop: true, entityChannel: 5), error);
        Assert.True(host.IsUnpositionedSoundPlaying()); Assert.True(host.IsUnpositionedSoundPlaying(5, "SOUND"));
        Assert.False(host.IsUnpositionedSoundPlaying(4)); Assert.False(host.IsUnpositionedSoundPlaying(5, "other"));
        host.StopUnpositionedSound(); Assert.False(host.IsUnpositionedSoundPlaying());
        Assert.Equal(!loop, host.IsPositionedSoundPlaying());
        host.Audio.Mix(2); Assert.False(host.IsPositionedSoundPlaying());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LocalNoStopFindsDirectMixerUnpositionedPlayback(bool magic)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { MagicSilenceCompatibility = magic };
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var original, out error,
            loop: true, volume: 0.25f, entityChannel: 4), error);
        HCDE.Playsim.ActorSoundActions.StartSound(host.Simulation.Players.Single(), "sound", 4,
            HCDE.Playsim.SimulationSoundFlags.Local | HCDE.Playsim.SimulationSoundFlags.NoStop);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.True(host.Audio.IsPlaying(original)); Assert.Equal((short)-7192, host.LastMix[0]);
        Assert.Equal(1, host.GetSoundPlayingCount("sound"));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void CrushPainFindsPlaybackAcrossChannels(int existingChannel)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 64 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string>(actor.ActorSounds)
        { [HCDE.Playsim.ActorSoundType.CrushPain] = "sound" };
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original,
            out error, loop: true, attenuation: 0, entityChannel: existingChannel), error);
        HCDE.Playsim.ActorSoundActions.CrushPain(actor);
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors); Assert.True(host.Audio.IsPlaying(original));
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id, "sound"));
    }

    [Theory]
    [InlineData(0, null, 3)]
    [InlineData(4, null, 2)]
    [InlineData(0, "SOUND", 2)]
    [InlineData(4, "SOUND", 1)]
    public void NativeUnpositionedStopFiltersSourceChannelAndName(int channel, string? sound, int stopped)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["other"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var actorHandle,
            out error, loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", 0, out var positioned,
            out error, loop: true, attenuation: 0, entityChannel: 4,
            detachedOrigin: new HCDE.Playsim.SimulationSoundOrigin(1, 2, 3, -1)), error);
        HCDE.Playsim.ActorSoundActions.StartGlobalSound(host.Simulation, "sound", 4, HCDE.Playsim.SimulationSoundFlags.Loop);
        HCDE.Playsim.ActorSoundActions.StartGlobalSound(host.Simulation, "sound", 5, HCDE.Playsim.SimulationSoundFlags.Loop);
        HCDE.Playsim.ActorSoundActions.StartGlobalSound(host.Simulation, "other", 4, HCDE.Playsim.SimulationSoundFlags.Loop);
        Assert.Equal(5, host.GetSoundPlayingCount());
        Assert.Equal(stopped, host.StopUnpositionedSound(channel, sound));
        Assert.True(host.Audio.IsPlaying(actorHandle)); Assert.True(host.Audio.IsPlaying(positioned));
        Assert.Equal(5 - stopped, host.GetSoundPlayingCount());
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(0, host.StopUnpositionedSound(channel, sound));
        Assert.Equal(sound is null ? 0 : 1, host.StopSoundByName("other"));
        Assert.Equal(channel == 0 ? 2 : 3, host.StopSoundByName("sound"));
        Assert.False(host.Audio.IsPlaying(actorHandle)); Assert.False(host.Audio.IsPlaying(positioned));
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id)); Assert.Equal(0, host.GetSoundPlayingCount());
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(16, false)]
    [InlineData(16, true)]
    [InlineData(16384, false)]
    [InlineData(16384, true)]
    public void NativeGlobalStartSoundNeedsNoActorOrPosition(int flags, bool loop)
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, Silent = true }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { SilentPickupCompatibility = true };
        Assert.Empty(host.Simulation.Players);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        HCDE.Playsim.ActorSoundActions.StartGlobalSound(host.Simulation, "sound",
            flags: (HCDE.Playsim.SimulationSoundFlags)flags | (loop ? HCDE.Playsim.SimulationSoundFlags.Loop : 0));
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal((short)-31768, host.LastMix[0]);
        Assert.Equal(loop ? 1 : 0, host.GetSoundPlayingCount("sound", 4));
        host.Tick(default); Assert.Equal(loop ? (short)-31768 : (short)1000, host.LastMix[0]);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(16, true)]
    [InlineData(16384, false)]
    [InlineData(16384, true)]
    [InlineData(16400, false)]
    [InlineData(16400, true)]
    public void PositionedStartSoundIgnoresActorLocalAndPickupGates(int flags, bool silentPickup)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { SilentPickupCompatibility = silentPickup };
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        HCDE.Playsim.ActorSoundActions.StartSoundAt(host.Simulation, 0, 0, 0, "sound",
            flags: (HCDE.Playsim.SimulationSoundFlags)flags | HCDE.Playsim.SimulationSoundFlags.Loop, attenuation: 0);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal((short)-31768, host.LastMix[0]);
        Assert.Equal(1, host.GetSoundPlayingCount("sound", 4));
        Assert.Equal(0, host.GetActorSoundPlayingCount(host.Simulation.Players.Single().Id));
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(false, true, 0)]
    [InlineData(true, false, 0)]
    [InlineData(true, true, 0)]
    [InlineData(false, false, 4)]
    [InlineData(false, true, 4)]
    [InlineData(true, false, 4)]
    [InlineData(true, true, 4)]
    public void PositionedReplacementUsesExactOriginChannelAndOverlap(bool samePosition, bool overlap, int channel)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var origin = new HCDE.Playsim.SimulationSoundOrigin(0, 0, 0, -1);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", 0, out var first, out var error,
            loop: true, attenuation: 0, entityChannel: channel, detachedOrigin: origin), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", host.Simulation.Players.Single().Id,
            out var actorSound, out error, loop: true, attenuation: 0, entityChannel: channel), error);
        var nextOrigin = samePosition ? origin : origin with { X = 1 };
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", 0, out var second, out error,
            loop: true, attenuation: 0, entityChannel: channel, overlap: overlap, detachedOrigin: nextOrigin), error);
        var replaced = samePosition && !overlap && channel != 0;
        Assert.Equal(!replaced, host.Audio.IsPlaying(first));
        Assert.True(host.Audio.IsPlaying(second)); Assert.True(host.Audio.IsPlaying(actorSound));
        Assert.Equal(replaced ? 2 : 3, host.GetSoundPlayingCount("sound", channel));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NativeStartSoundAtPlaysWithoutActorAndPreservesPositionedLoops(bool loop, bool magic)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 128, Silent = true }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { MagicSilenceCompatibility = magic };
        HCDE.Playsim.ActorSoundActions.StartSoundAt(host.Simulation, 12, 34, 56, "sound", channel: 4,
            flags: loop ? HCDE.Playsim.SimulationSoundFlags.Loop : 0, attenuation: 0);
        var request = Assert.Single(host.Simulation.PendingSoundRequests);
        Assert.Equal(new HCDE.Playsim.SimulationSoundOrigin(12, 56, 34, -1), request.DetachedOrigin);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal((short)-31768, host.LastMix[0]);
        Assert.Equal(loop ? 1 : 0, host.GetSoundPlayingCount("sound", 4));
        Assert.Equal(0, host.GetActorSoundPlayingCount(host.Simulation.Players.Single().Id));
        host.Tick(default); Assert.Equal(loop ? (short)-31768 : (short)1000, host.LastMix[0]);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void NullRelinkDestinationUsesNativePositionedSoundRules(bool loop, bool cutoff)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { SoundCutoffCompatibility = cutoff };
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var handle,
            out var error, loop: loop, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryRelinkActorSounds(actor.Id, null, out var count, out error), error);
        Assert.Equal(1, count); Assert.False(actor.Destroyed);
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(!loop && !cutoff, host.Audio.IsPlaying(handle));
        Assert.Equal(!loop && !cutoff ? 1 : 0, host.GetSoundPlayingCount("sound", 4));
        Assert.Equal(0, host.StopActorSound(actor.Id, 4));
        Assert.True(host.TryRelinkActorSounds(actor.Id, null, out count, out error), error); Assert.Equal(0, count);
        var samples = host.Audio.Mix(2);
        Assert.Equal(!loop && !cutoff ? short.MinValue : (short)0, samples[0]);
        Assert.Equal(0, host.GetSoundPlayingCount());
    }

    [Fact]
    public void NullRelinkRejectsUnknownSourceWithoutChangingPlayback()
    {
        using var host = ClientHost.DemoRoom();
        var handle = host.Audio.PlayChannel(new short[] { 1, 2 }, loop: true);
        Assert.False(host.TryRelinkActorSounds(uint.MaxValue, null, out var count, out var error));
        Assert.Equal(0, count); Assert.Equal("audio-source-actor-missing", error);
        Assert.True(host.Audio.IsPlaying(handle));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GlobalSoundCountRetainsDetachedPlaybackUntilCompletion(bool cutoff)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level))
        { SoundCutoffCompatibility = cutoff };
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var positioned,
            out var error, attenuation: 0, entityChannel: 4), error);
        var raw = host.Audio.PlayChannel(new short[] { 1, 2 }, loop: true);
        Assert.Equal(2, host.GetSoundPlayingCount());
        Assert.Equal(1, host.GetSoundPlayingCount("SOUND"));
        Assert.Equal(1, host.GetSoundPlayingCount(entityChannel: 0));
        Assert.Equal(1, host.GetSoundPlayingCount("sound", 4));
        Assert.Equal(0, host.GetSoundPlayingCount("missing"));
        actor.Destroy();
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(cutoff ? 1 : 2, host.GetSoundPlayingCount());
        Assert.Equal(cutoff ? 0 : 1, host.GetSoundPlayingCount("sound", 4));
        Assert.Equal(!cutoff, host.Audio.IsPlaying(positioned));
        host.Audio.Mix(2);
        Assert.Equal(1, host.GetSoundPlayingCount()); Assert.Equal(0, host.GetSoundPlayingCount("sound"));
        Assert.True(host.Audio.IsPlaying(raw));
        host.Dispose(); Assert.Equal(0, host.GetSoundPlayingCount());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeActorSoundCountIncludesOverlapAndFiltersExactChannels(bool magicSilence)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["other"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); var otherActor = host.Simulation.AddBot(100, 0);
        for (var index = 0; index < 2; index++)
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out var error,
                loop: true, attenuation: 0, entityChannel: 4, overlap: true), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "other", actor.Id, out _, out var otherError,
            loop: true, attenuation: 0, entityChannel: 5), otherError);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", otherActor.Id, out _, out otherError,
            loop: true, attenuation: 0, entityChannel: 4), otherError);
        host.MagicSilenceCompatibility = magicSilence;
        Assert.Equal(3, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(2, host.GetActorSoundPlayingCount(actor.Id, "SOUND"));
        Assert.Equal(2, host.GetActorSoundPlayingCount(actor.Id, "sound", 4));
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id, "sound", 5));
        Assert.Equal(1, host.GetActorSoundPlayingCount(actor.Id, entityChannel: 5));
        Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id, entityChannel: 0));
        Assert.Equal(0, host.GetActorSoundPlayingCount(uint.MaxValue));
        Assert.True(host.IsActorSoundPlaying(actor.Id, 0));
        Assert.Equal(magicSilence, host.IsActorSoundPlaying(actor.Id, 5, "sound"));
        Assert.Equal(1, host.GetActorSoundPlayingCount(otherActor.Id));
        actor.Destroy(); Assert.Equal(0, host.GetActorSoundPlayingCount(actor.Id));
        Assert.Equal(1, host.GetActorSoundPlayingCount(otherActor.Id));
    }

    [Theory]
    [InlineData(false, 4, false, true)]
    [InlineData(false, 5, false, false)]
    [InlineData(false, 5, true, true)]
    [InlineData(true, 4, false, true)]
    [InlineData(true, 5, false, false)]
    [InlineData(true, 5, true, true)]
    public void BodySoundActionsUseNativeQueryGuard(bool howl, int originalChannel, bool magicSilence, bool suppressed)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string>
        { [howl ? HCDE.Playsim.ActorSoundType.Howl : HCDE.Playsim.ActorSoundType.Push] = "sound" };
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original,
            out error, loop: true, volume: 0.25f, attenuation: 0, entityChannel: originalChannel), error);
        host.MagicSilenceCompatibility = magicSilence;
        if (howl) HCDE.Playsim.ActorSoundActions.Howl(actor);
        else HCDE.Playsim.ActorSoundActions.PlayPushSound(actor);
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors); Assert.True(host.Audio.IsPlaying(original));
        Assert.Equal(suppressed ? (short)-7192 : (short)-31768, host.LastMix[0]);
    }

    [Theory]
    [InlineData(4, false, true)]
    [InlineData(5, false, false)]
    [InlineData(5, true, true)]
    public void LegacyLoopSoundChecksMatchingSoundOnNativeChannelScope(int originalChannel, bool magicSilence, bool suppressed)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original,
            out error, loop: true, volume: 0.25f, attenuation: 0, entityChannel: originalChannel), error);
        host.MagicSilenceCompatibility = magicSilence;
        HCDE.Playsim.ActorSoundActions.PlaySoundEx(actor, "sound", "Body", true, 1);
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors); Assert.True(host.Audio.IsPlaying(original));
        Assert.Equal(suppressed ? (short)-7192 : (short)-31768, host.LastMix[0]);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ProjectileImpactSoundSurvivesSourceRemovalAndDelayedArchive(bool delayed, bool silent)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 128, Silent = silent }],
            SoundDefinitions = new Dictionary<string, string> { ["weapons/rocklx"] = "DSPISTOL" },
        };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        host.Simulation.Players.Single().Invulnerable = true;
        var missile = host.Simulation.SpawnProjectile(host.Simulation.Players.Single(), HCDE.Playsim.ProjectileKind.Rocket);
        host.Simulation.DrainSoundRequests();
        missile.Z = HCDE.Playsim.Fixed.FromInt(1); missile.VelocityZ = HCDE.Playsim.Fixed.FromInt(-10);
        missile.FullVolumeDeath = true;
        if (!delayed) Assert.True(host.TrySetSoundArchive(Wad(11025), out var archiveError), archiveError);
        host.Tick(default); Assert.True(missile.Destroyed);
        if (delayed)
        {
            var origin = Assert.Single(host.Simulation.PendingSoundRequests).DetachedOrigin;
            var position = missile.SoundPos();
            Assert.Equal(new HCDE.Playsim.SimulationSoundOrigin(position.X, position.Y, position.Z, 0), origin);
            Assert.True(host.TrySetSoundArchive(Wad(11025), out var archiveError), archiveError);
            host.Tick(default);
        }
        Assert.Empty(host.LastSoundRequestErrors); Assert.Empty(host.Simulation.PendingSoundRequests);
        if (silent) Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
        else Assert.Contains(host.LastMix, sample => sample != 1000);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PitchRoundTripWithoutMixingDoesNotLosePlaybackPosition(bool loop)
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out var channel, out var error, loop: loop), error);
        Assert.Equal(new short[] { -32768 }, mixer.Mix(1));
        for (var change = 0; change < 8; change++)
        {
            Assert.True(mixer.TrySetChannelPitch(channel, 2, out error), error);
            Assert.True(mixer.TrySetChannelPitch(channel, 1, out error), error);
        }
        Assert.Equal(new short[] { 32512 }, mixer.Mix(1));
        Assert.Equal(loop, mixer.IsPlaying(channel));
    }

    [Fact]
    public void LivePitchChangesPreserveSourcePositionAndOriginalSamples()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out var channel, out var error, loop: true), error);
        Assert.Equal(new short[] { -32768 }, mixer.Mix(1));
        Assert.True(mixer.TrySetChannelPitch(channel, 0.5f, out error), error);
        Assert.Equal((short)32512, mixer.Mix(1)[0]);
        Assert.True(mixer.TrySetChannelPitch(channel, 1, out error), error);
        Assert.Equal((short)32512, mixer.Mix(1)[0]);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.False(mixer.TrySetChannelPitch(channel, float.NaN, out error)); Assert.Equal("audio-pitch-invalid", error);
        Assert.True(mixer.TrySetChannelPitch(channel, -1, out error), error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PitchStateActionUsesExactChannelWithoutMagicSilenceExpansion(bool magic)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var channel, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        host.MagicSilenceCompatibility = magic;
        HCDE.Playsim.ActorSoundActions.SoundPitch(actor, 2, 2);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0,
            a => HCDE.Playsim.ActorSoundActions.SoundPitch(a, 4, 2))], 0);
        Assert.Equal(new short[] { -32768, -32768 }, host.Audio.Mix(2));
        Assert.True(host.Audio.IsPlaying(channel)); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeOneShotSoundOffsetClampsToBeginning(bool local)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        HCDE.Playsim.ActorSoundActions.StartSound(host.Simulation.Players.Single(), "sound",
            flags: local ? HCDE.Playsim.SimulationSoundFlags.Local : 0, startTime: -1);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out error, startTime: -1), error);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.False(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out _, out error, loop: true, startTime: -1));
        Assert.Equal("audio-start-time-invalid", error);
        Assert.Throws<NotSupportedException>(() => HCDE.Playsim.ActorSoundActions.StartSound(
            host.Simulation.Players.Single(), "sound", flags: HCDE.Playsim.SimulationSoundFlags.Loop, startTime: -1));
        Assert.Empty(host.Simulation.PendingSoundRequests);
    }

    [Theory]
    [InlineData(false, false, 1.1f, 1, 32512)]
    [InlineData(true, false, 3.1f, 1, 32512)]
    [InlineData(true, true, 3.1f, 1, 32512)]
    [InlineData(false, false, 3.1f, 1, 0)]
    [InlineData(false, false, 1.1f, 2, -32768)]
    public void ActorSoundStartOffsetsUseSourceTimeAndLoopWrapping(bool loop, bool local, float offsetSamples, float pitch, short expected)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var flags = (loop ? HCDE.Playsim.SimulationSoundFlags.Loop : 0) |
            (local ? HCDE.Playsim.SimulationSoundFlags.Local : 0);
        HCDE.Playsim.ActorSoundActions.StartSound(host.Simulation.Players.Single(), "sound", flags: flags,
            pitch: pitch, startTime: offsetSamples / 11025);
        Assert.Equal(new short[] { expected }, host.Audio.Mix(1)); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ActorSoundActionsPassExplicitPitchToPositionedAndLocalPlayback(bool local, bool legacy)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0, a =>
        {
            if (legacy) HCDE.Playsim.ActorSoundActions.PlaySound(a, "sound", looping: true, local: local, pitch: 2);
            else HCDE.Playsim.ActorSoundActions.StartSound(a, "sound", flags: HCDE.Playsim.SimulationSoundFlags.Loop |
                (local ? HCDE.Playsim.SimulationSoundFlags.Local : 0), pitch: 2);
        })], 0);
        Assert.Equal(new short[] { -32768, -32768, -32768 }, host.Audio.Mix(3));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyPlaySoundLoopKeepsPositionAndLocalUsesListenerOnly(bool local)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var player = host.Simulation.Players.Single();
        HCDE.Playsim.ActorSoundActions.PlaySound(player, "sound", looping: true, local: local);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.PlaySound(player, "sound", looping: true, local: local);
        Assert.Equal(new short[] { 32512 }, host.Audio.Mix(1));
        var other = host.Simulation.Actors.Single(a => a.Id != player.Id);
        if (local) HCDE.Playsim.ActorSoundActions.PlaySound(other, "sound", local: true);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false, 8128)]
    [InlineData(true, 32767)]
    public void WeaponSoundActionUsesWeaponChannelAndFullVolumeOption(bool fullVolume, short expected)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 500 }],
            SoundDefinitions = new Dictionary<string, string> { ["weapon"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 20) };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Actors.Single(a => a.DoomEdNum == 3004);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "weapon", actor.Id, out var previous, out error,
            loop: true, volume: 0.5f, attenuation: 0, entityChannel: 1), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "weapon", actor.Id, out var body, out error,
            loop: true, volume: 0.25f, attenuation: 0, entityChannel: 4), error);
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0,
            a => HCDE.Playsim.ActorSoundActions.PlayWeaponSound(a, "weapon", fullVolume))], 0);
        Assert.False(host.Audio.IsPlaying(previous)); Assert.True(host.Audio.IsPlaying(body));
        Assert.True(host.IsActorSoundPlaying(actor.Id, 1, "weapon"));
        Assert.Equal(expected, host.Audio.Mix(2)[1]); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(2, false, 32512)]
    [InlineData(10, false, 32767)]
    [InlineData(-1, false, 0)]
    [InlineData(2, true, 0)]
    public void SoundVolumeActionUsesExactChannelOrMagicSilenceAll(int channel, bool magic, short expected)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        foreach (var slot in new[] { 2, 4 })
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out error,
                loop: true, attenuation: 0, entityChannel: slot), error);
        host.MagicSilenceCompatibility = magic;
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0,
            a => HCDE.Playsim.ActorSoundActions.SoundVolume(a, channel, -1))], 0);
        Assert.Equal(expected, host.Audio.Mix(2)[1]);
        HCDE.Playsim.ActorSoundActions.SoundVolume(actor, -1, 2);
        Assert.Equal((short)32767, host.Audio.Mix(2)[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => HCDE.Playsim.ActorSoundActions.SoundVolume(actor, 2, float.NaN));
        Assert.Empty(host.Simulation.PendingSoundRequests); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(2, 4, false, false, false)]
    [InlineData(4, 2, true, false, false)]
    [InlineData(4, 4, true, true, false)]
    [InlineData(0, 0, false, false, false)]
    [InlineData(10, 8, false, true, true)]
    public void StopSoundRangePreservesNativeBoundsAndIgnoresMagicSilence(int minimum, int maximum,
        bool magic, bool voiceSurvives, bool bodySurvives)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); var other = host.Simulation.Actors.Single(a => a.Id != actor.Id);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var voice, out error,
            loop: true, attenuation: 0, entityChannel: 2), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var body, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", other.Id, out var unrelated, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        host.MagicSilenceCompatibility = magic;
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0, a =>
        {
            if (minimum == 0 && maximum == 0) HCDE.Playsim.ActorSoundActions.StopAllSounds(a);
            else HCDE.Playsim.ActorSoundActions.StopSounds(a, minimum, maximum);
        })], 0);
        Assert.Equal(voiceSurvives, host.Audio.IsPlaying(voice)); Assert.Equal(bodySurvives, host.Audio.IsPlaying(body));
        Assert.True(host.Audio.IsPlaying(unrelated)); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Fact]
    public void HowlKeepsPlayingCopyAndRestartsAfterCompletion()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["howl"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Howl] = "howl" };
        HCDE.Playsim.ActorSoundActions.Howl(actor);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.Howl(actor);
        Assert.Equal(new short[] { 32512 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.Howl(actor);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdleActiveSoundWaitsForAnyVoiceSoundButNotBodySound(bool sameSound)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["idle"] = "DSPISTOL", ["other"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Active] = "idle" };
        HCDE.Playsim.ActorSoundActions.StartSound(actor, sameSound ? "idle" : "other", 2);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.PlayActiveSound(actor);
        Assert.Equal(new short[] { 32512 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.StartSound(actor, "other", 4, volume: 0);
        HCDE.Playsim.ActorSoundActions.PlayActiveSound(actor);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Fact]
    public void PushSoundPreservesMatchingPlaybackAndReplacesDifferentBodySound()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["push"] = "DSPISTOL", ["other"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var actor = host.Simulation.Players.Single();
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Push] = "push" };
        HCDE.Playsim.ActorSoundActions.PlayPushSound(actor);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.PlayPushSound(actor);
        Assert.Equal(new short[] { 32512 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.PlayPushSound(actor);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.StartSound(actor, "other", 4);
        Assert.Equal(new short[] { -32768 }, host.Audio.Mix(1));
        HCDE.Playsim.ActorSoundActions.PlayPushSound(actor);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
        Assert.Empty(host.LastSoundRequestErrors);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(10)]
    public void StopSoundStateActionUsesDefaultOrExactExplicitChannel(int channel)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        using var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var voice, out error,
            loop: true, attenuation: 0, entityChannel: 2), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var body, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        actor.States.Configure(actor, [new HCDE.Playsim.ActorFrame(-1, 0, a =>
        {
            if (channel == 2) HCDE.Playsim.ActorSoundActions.StopSound(a);
            else HCDE.Playsim.ActorSoundActions.StopSound(a, channel);
        })], 0);
        Assert.Equal(channel != 2, host.Audio.IsPlaying(voice));
        Assert.Equal(channel != 4, host.Audio.IsPlaying(body));
        Assert.Empty(host.Simulation.PendingSoundRequests); Assert.Empty(host.LastSoundRequestErrors);
    }

    [Fact]
    public void ExtremeScreamStateStartsSoundBeforeImmediateRemoval()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }],
            SoundDefinitions = new Dictionary<string, string> { ["misc/gibbed"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var monster = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 3004);
        monster.States.Configure(monster, [new HCDE.Playsim.ActorFrame(0, -1, HCDE.Playsim.ActorSoundActions.XScream)], 0);
        Assert.True(monster.Destroyed); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
    }

    [Fact]
    public void ActiveSoundActionReplacesPreviousVoiceChannelSound()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["active"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "active", actor.Id, out var previous, out error,
            loop: true, attenuation: 0, entityChannel: 2), error);
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Active] = "active" };
        HCDE.Playsim.ActorSoundActions.ActiveSound(actor);
        Assert.False(host.Audio.IsPlaying(previous)); Assert.True(host.IsActorSoundPlaying(actor.Id, 2, "active"));
        Assert.Empty(host.LastSoundRequestErrors); Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
    }

    [Fact]
    public void DefaultDeathFramePlaysBossSoundWithoutDistanceAttenuation()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 500 }],
            SoundDefinitions = new Dictionary<string, string> { ["monster/death"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 20) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var monster = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 3004); monster.Boss = true;
        monster.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Death] = "monster/death" };
        monster.States.Enter(monster, HCDE.Playsim.ActorStateMachine.Death);
        Assert.Empty(host.LastSoundRequestErrors); Assert.True(host.IsActorSoundPlaying(monster.Id, 2, "monster/death"));
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
    }

    [Fact]
    public void DefaultMonsterPainFrameStartsClientSound()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }],
            SoundDefinitions = new Dictionary<string, string> { ["monster/pain"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var monster = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 3004);
        monster.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [HCDE.Playsim.ActorSoundType.Pain] = "monster/pain" };
        monster.States.Enter(monster, HCDE.Playsim.ActorStateMachine.Pain);
        Assert.True(host.IsActorSoundPlaying(monster.Id, 2, "monster/pain"));
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
    }

    [Fact]
    public void IceShatterSoundStartsBeforeCorpseDisappears()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14 }],
            SoundDefinitions = new Dictionary<string, string> { ["misc/icebreak"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var corpse = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        corpse.Health = 0; corpse.IceCorpse = true; corpse.Shootable = true;
        HCDE.Playsim.ActorDamage.Apply(corpse, 1);
        Assert.True(corpse.Destroyed); Assert.Empty(host.Simulation.PendingSoundRequests);
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { -32768, 32512 }, host.Audio.Mix(2));
    }

    [Fact]
    public void FreezeDeathSoundActionReachesClientPlayback()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["misc/freeze"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        HCDE.Playsim.ActorFreezeActions.FreezeDeath(host.Simulation.Players.Single());
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal((short)-31768, host.LastMix[0]); Assert.Empty(host.Simulation.PendingSoundRequests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LevelExitStopsPlaybackAndNotifiesOnce(bool secret)
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single(); var notifications = 0;
        host.Simulation.LevelExited += () => notifications++;
        var raw = host.Audio.PlayChannel(new short[] { 1, 2 }, loop: true);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var owned, out var error,
            loop: true, attenuation: 0), error);
        host.Simulation.MarkExited(secret); host.Simulation.MarkExited(secret);
        Assert.Equal(1, notifications); Assert.Equal(secret, host.Simulation.SecretExit);
        Assert.False(host.Audio.IsPlaying(raw)); Assert.False(host.Audio.IsPlaying(owned));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var afterExit, out error), error);
        Assert.Equal(Guid.Empty, afterExit);
        host.Tick(default); Assert.Equal(new short[] { 0, 0, 0, 0 }, host.LastMix);
    }

    [Fact]
    public void LevelExitDiscardsPendingAndLaterSoundRequests()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 4); Assert.NotEmpty(host.Simulation.PendingSoundRequests);
        host.Simulation.MarkExited(false); Assert.Empty(host.Simulation.PendingSoundRequests);
        QueueAcsSound(host, actor, 4); host.Tick(default);
        Assert.Empty(host.Simulation.PendingSoundRequests); Assert.Equal(new short[] { 0, 0, 0, 0 }, host.LastMix);
    }

    [Fact]
    public void RestoringSimulationClearsOldPlaybackAndRequestsButKeepsSoundArchive()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        var snapshot = host.Simulation.CaptureState();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var handle, out error,
            loop: true, attenuation: 0), error);
        QueueAcsSound(host, actor, 4); Assert.NotEmpty(host.Simulation.PendingSoundRequests);
        host.Simulation.RestoreState(snapshot);
        Assert.False(host.Audio.IsPlaying(handle)); Assert.Empty(host.Simulation.PendingSoundRequests);
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "sound"));
        QueueAcsSound(host, host.Simulation.Players.Single(), 4); host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors); Assert.Equal((short)-31768, host.LastMix[0]);
    }

    [Fact]
    public void RejectedRestorePreservesActiveSound()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var handle, out var error,
            loop: true, attenuation: 0), error);
        var originalSounds = actor.ActorSounds;
        actor.ActorSounds = new Dictionary<HCDE.Playsim.ActorSoundType, string> { [(HCDE.Playsim.ActorSoundType)99] = "bad" };
        var snapshot = host.Simulation.CaptureState();
        actor.ActorSounds = originalSounds;
        Assert.Throws<InvalidOperationException>(() => host.Simulation.RestoreState(snapshot));
        Assert.True(host.Audio.IsPlaying(handle));
    }

    [Fact]
    public void ClientShutdownStopsOwnedAndRawChannelsAndRejectsPlayback()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        var raw = host.Audio.PlayChannel(new short[] { 1, 2 }, loop: true);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var owned, out var error,
            loop: true, attenuation: 0), error);
        host.Dispose(); host.Dispose();
        Assert.False(host.Audio.IsPlaying(raw)); Assert.False(host.Audio.IsPlaying(owned));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "sound"));
        Assert.Throws<ObjectDisposedException>(() => host.Tick(default));
        Assert.Throws<ObjectDisposedException>(() => host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out _, out _));
        actor.Destroy(); Assert.Equal(new short[] { 0, 0 }, host.Audio.Mix(2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DetachedOneShotRetainsListenerHeightModifier(bool listenerHeight)
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        source.Z = HCDE.Playsim.Fixed.FromInt(100);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var handle, out var error,
            listenerHeight: listenerHeight), error);
        source.Destroy(); Assert.True(host.Audio.IsPlaying(handle));
        Assert.True(host.Audio.TryUpdateListenerPosition(new(0, 50, 0), out error), error);
        Assert.Equal(new short[] { listenerHeight ? (short)-32768 : (short)0 }, host.Audio.Mix(1));
        Assert.True(host.Audio.TryUpdateListenerPosition(new(0, -50, 0), out error), error);
        Assert.Equal(new short[] { listenerHeight ? (short)32512 : (short)0 }, host.Audio.Mix(1));
    }

    [Fact]
    public void DestructionCallbackDetachesOneShotAtCurrentActorPosition()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var handle, out var error), error);
        source.X = HCDE.Playsim.Fixed.FromInt(20); source.Destroy();
        Assert.True(host.Audio.IsPlaying(handle)); Assert.False(host.IsActorSoundPlaying(source.Id, 0, "sound"));
        source.X = HCDE.Playsim.Fixed.FromInt(0);
        Assert.True(host.Audio.TryUpdateListenerPosition(default, out error), error);
        Assert.Equal(new short[] { 0, 0 }, host.Audio.Mix(2));
    }

    [Fact]
    public void DestructionCallbackRunsOnceAndStopsLoopImmediately()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single(); var notifications = 0;
        host.Simulation.ActorDestroyed += _ => notifications++;
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var handle, out var error,
            loop: true, attenuation: 0), error);
        actor.Destroy(); Assert.False(host.Audio.IsPlaying(handle));
        actor.Destroy(); Assert.Equal(1, notifications);
    }

    [Fact]
    public void RelinkingActorSoundsTransfersOwnershipWithoutReplacingDestinationSounds()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14); var target = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var moved, out var error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", target.Id, out var existing, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryRelinkActorSounds(source.Id, target.Id, out var count, out error), error); Assert.Equal(1, count);
        Assert.False(host.IsActorSoundPlaying(source.Id, 4, "sound")); Assert.True(host.IsActorSoundPlaying(target.Id, 4, "sound"));
        source.Destroy(); host.Tick(default); Assert.True(host.Audio.IsPlaying(moved)); Assert.True(host.Audio.IsPlaying(existing));
        Assert.Equal(2, host.StopActorSound(target.Id, 4));
    }

    [Fact]
    public void InvalidRelinkDestinationPreservesSourceOwnership()
    {
        var host = ClientHost.DemoRoom(); var source = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", source.Id, out var handle, out var error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.False(host.TryRelinkActorSounds(source.Id, uint.MaxValue, out var count, out error));
        Assert.Equal(0, count); Assert.Equal("audio-destination-actor-missing", error);
        Assert.True(host.Audio.IsPlaying(handle)); Assert.True(host.IsActorSoundPlaying(source.Id, 4, "sound"));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    public void ActorDisappearanceDetachesOneShotsAndStopsLoopsOrCutoffSounds(bool loop, bool cutoff, bool removeListener)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000),
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)) { SoundCutoffCompatibility = cutoff };
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var handle, out var error,
            loop: loop, attenuation: 0, entityChannel: 4), error);
        source.Destroy();
        if (removeListener) host.Simulation.Players.Single().Destroy();
        host.Tick(default);
        Assert.Equal(loop || cutoff ? (short)1000 : (short)-31768, host.LastMix[0]);
        Assert.False(host.Audio.IsPlaying(handle)); Assert.False(host.IsActorSoundPlaying(source.Id, 4, "sound"));
        if (removeListener) Assert.Null(host.AudioListenerPosition);
    }

    [Fact]
    public void DetachingVirtualLoopStopsItsRetainedHandle()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.IsChannelVirtual(waiting)); Assert.True(mixer.DetachChannelSource(waiting));
        Assert.False(mixer.IsPlaying(waiting)); Assert.False(mixer.DetachChannelSource(waiting));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MagicSilenceRoutesActorPlaybackToWeaponChannel(bool compatibility)
    {
        var host = ClientHost.DemoRoom(); host.MagicSilenceCompatibility = compatibility;
        var actor = host.Simulation.Players.Single(); var level = Level();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var first, out var error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var second, out error,
            loop: true, attenuation: 0, entityChannel: 5), error);
        Assert.Equal(!compatibility, host.Audio.IsPlaying(first)); Assert.True(host.Audio.IsPlaying(second));
        Assert.Equal(compatibility ? 1 : 0, host.StopActorSound(actor.Id, 1));
    }

    [Fact]
    public void MagicSilenceQueriesAndStopsAllExistingActorChannels()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single(); var level = Level();
        for (var channel = 4; channel <= 5; channel++)
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out var error,
                loop: true, attenuation: 0, entityChannel: channel), error);
        host.MagicSilenceCompatibility = true;
        Assert.True(host.IsActorSoundPlaying(actor.Id, 7, "sound"));
        Assert.Equal(2, host.StopActorSound(actor.Id, 7));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 7, "sound"));
    }

    [Fact]
    public void MagicSilenceNoStopChecksAllActorChannels()
    {
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" } };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        host.MagicSilenceCompatibility = true; QueueAcsSound(host, actor, 4096 | 5);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors); Assert.True(host.Audio.IsPlaying(original));
        Assert.Equal((short)-31768, host.LastMix[0]); Assert.Equal(1, host.StopActorSound(actor.Id, 7));
    }

    [Fact]
    public void MagicSilenceVolumeRequestUpdatesAllActorChannels()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single(); var level = Level();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        for (var channel = 4; channel <= 5; channel++)
            Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out error,
                loop: true, attenuation: 0, entityChannel: channel), error);
        host.MagicSilenceCompatibility = true;
        int[] words = [3, 0, 3, 7, 3, 16384, 351, 3, 70, 1]; var bytes = new byte[words.Length * 4];
        for (var index = 0; index < words.Length; index++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(index * 4), words[index]);
        host.Simulation.Acs.Add(new HCDE.Playsim.AcsProgram { Number = 7, Code = bytes });
        Assert.True(host.Simulation.Acs.Enqueue(7, ReadOnlySpan<int>.Empty, actor)); host.Simulation.Acs.Tick(host.Simulation);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors); Assert.Equal((short)-15384, host.LastMix[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SilentSectorSuppressesActorRequestsIncludingLocalSounds(bool local)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 128, Silent = true }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single(); Assert.Equal(0, actor.SectorIndex);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 4, local: local);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
    }

    [Fact]
    public void SilentSectorActorPlaybackRejectsBeforeDecodingResources()
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 128, Silent = true }],
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound([], level, "sound", actor.Id, out var handle, out var error, loop: true), error);
        Assert.Equal(Guid.Empty, handle); Assert.Null(error);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void MaybeLocalSoundHonorsSilentPickupCompatibility(bool compatibility, bool listenerSource, bool audible)
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000),
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)) { SilentPickupCompatibility = compatibility };
        var actor = listenerSource ? host.Simulation.Players.First() : host.Simulation.Players.Last();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, (int)HCDE.Playsim.SimulationSoundFlags.MaybeLocal | 4);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(audible ? (short)-31768 : (short)1000, host.LastMix[0]);
    }

    [Fact]
    public void DefinedSingularBlockedLoopRetainsHandleAndNativeSelfCheck()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(Singular: true) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true), error);
        Assert.NotEqual(Guid.Empty, waiting); Assert.True(mixer.IsChannelVirtual(waiting));
        mixer.StopChannel(first);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
        Assert.True(mixer.IsPlaying(waiting)); Assert.True(mixer.IsChannelVirtual(waiting));
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var suppressed, out error), error);
        Assert.Equal(Guid.Empty, suppressed);
        Assert.True(mixer.StopChannel(waiting));
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var replacement, out error, loop: true), error);
        Assert.False(mixer.IsChannelVirtual(replacement));
    }

    [Fact]
    public void RequestOnlySingularBlockedLoopRestartsWithoutDefinedSingularRecheck()
    {
        var level = Level(); var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true, singular: true), error);
        Assert.True(mixer.IsChannelVirtual(waiting));
        mixer.Mix(1);
        Assert.True(mixer.IsPlaying(first)); Assert.False(mixer.IsChannelVirtual(waiting));
    }

    [Fact]
    public void DefinedSingularAliasUsesOriginalIdentityDuringRestoration()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(Singular: true) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var alias, out error, loop: true), error);
        Assert.True(mixer.IsChannelVirtual(alias));
        mixer.StopChannel(first);
        Assert.Equal(new short[] { -32768 }, mixer.Mix(1)); Assert.False(mixer.IsChannelVirtual(alias));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VirtualLoopRestorationHonorsPausePolicy(bool noPause)
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var blocker, out var error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true, noPause: noPause,
            sourcePosition: default(System.Numerics.Vector3)), error);
        mixer.Paused = true; mixer.StopChannel(blocker);
        Assert.Equal(new short[] { noPause ? (short)-32768 : (short)0 }, mixer.Mix(1));
        Assert.Equal(!noPause, mixer.IsChannelVirtual(waiting));
        mixer.Paused = false;
        Assert.Equal(new short[] { noPause ? (short)32512 : (short)-32768 }, mixer.Mix(1));
        Assert.False(mixer.IsChannelVirtual(waiting));
    }

    [Fact]
    public void BlockedLoopKeepsHandleAndResumesAfterBlockerStops()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var blocker, out var error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.IsPlaying(waiting)); Assert.True(mixer.IsChannelVirtual(waiting));
        Assert.Equal(new short[] { -32768 }, mixer.Mix(1)); Assert.True(mixer.IsChannelVirtual(waiting));
        Assert.True(mixer.StopChannel(blocker));
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.False(mixer.IsChannelVirtual(waiting)); Assert.True(mixer.IsPlaying(waiting));
    }

    [Fact]
    public void VirtualLoopsDoNotConsumeLimitsAndResumeOldestFirst()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer(); var handles = new List<Guid>();
        for (var index = 0; index < 3; index++)
        {
            Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var handle, out var error, loop: true,
                sourcePosition: default(System.Numerics.Vector3)), error);
            handles.Add(handle);
        }
        Assert.True(mixer.IsChannelVirtual(handles[1])); Assert.True(mixer.IsChannelVirtual(handles[2]));
        mixer.StopChannel(handles[0]); mixer.Mix(1);
        Assert.False(mixer.IsChannelVirtual(handles[1])); Assert.True(mixer.IsChannelVirtual(handles[2]));
        mixer.StopChannel(handles[1]); mixer.Mix(1); Assert.False(mixer.IsChannelVirtual(handles[2]));
    }

    [Fact]
    public void VirtualLoopResumesAfterSourceMovesOutsideLimitRange()
    {
        var level = Level(); level.GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(NearLimit: 1, LimitRange: 100) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TrySetChannelPosition(waiting, new(20, 0, 0), new(20, 0, 0), out error), error);
        mixer.Mix(1); Assert.False(mixer.IsChannelVirtual(waiting));
    }

    [Fact]
    public void VirtualLoopCanBeStoppedAndCompletionUnblocksOnNextBuffer()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var waiting, out error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        mixer.Mix(2); Assert.True(mixer.IsChannelVirtual(waiting));
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2)); Assert.False(mixer.IsChannelVirtual(waiting));
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var stopped, out error, loop: true,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.IsChannelVirtual(stopped)); Assert.True(mixer.StopChannel(stopped));
        Assert.False(mixer.IsPlaying(stopped));
    }

    [Theory]
    [InlineData(10, 1, true)]
    [InlineData(11, 1, false)]
    [InlineData(1000, 0, true)]
    [InlineData(14, 0.5f, true)]
    public void NearbySoundLimitUsesSquaredRangeAndAttenuation(float distance, float attenuation, bool blocked)
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["sound"] = new(NearLimit: 1, LimitRange: 100) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error,
            sourcePosition: default(System.Numerics.Vector3), sourceActorId: 1, attenuation: attenuation), error);
        Assert.NotEqual(Guid.Empty, first);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var second, out error,
            sourcePosition: new System.Numerics.Vector3(distance, 0, 0), sourceActorId: 2), error);
        Assert.Equal(blocked, second == Guid.Empty);
        Assert.True(mixer.StopChannel(first));
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var resumed, out error,
            sourcePosition: default(System.Numerics.Vector3), sourceActorId: 3), error);
        Assert.NotEqual(Guid.Empty, resumed);
    }

    [Fact]
    public void NearbySoundLimitSharesResolvedAliasesAndAllowsSameActorChannelRestart()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(NearLimit: -1), ["sound"] = new(NearLimit: 1, LimitRange: 100) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out _, out var error,
            sourcePosition: default(System.Numerics.Vector3), sourceActorId: 1, entityChannel: 4), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var blocked, out error,
            sourcePosition: default(System.Numerics.Vector3), sourceActorId: 2, entityChannel: 4), error);
        Assert.Equal(Guid.Empty, blocked);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var restart, out error,
            sourcePosition: default(System.Numerics.Vector3), sourceActorId: 1, entityChannel: 4), error);
        Assert.NotEqual(Guid.Empty, restart);
    }

    [Fact]
    public void NearbySoundLimitExemptsListenerAndUnpositionedSounds()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var unpositioned, out error), error);
        Assert.NotEqual(Guid.Empty, unpositioned);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var listener, out error,
            sourcePosition: default(System.Numerics.Vector3), listenerSource: true), error);
        Assert.NotEqual(Guid.Empty, listener);
    }

    [Fact]
    public void ActorPlaybackAppliesNearbyLimitBeforeReplacingChannels()
    {
        var level = new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2 }],
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        level.GlobalSoundRolloff = new(MinDistance: 1, MaxDistance: 1000);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(NearLimit: 1) };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Players.Last(); Assert.NotEqual(host.Simulation.Players.First().Id, source.Id);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var first, out var error, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var blocked, out error, entityChannel: 5), error);
        Assert.Equal(Guid.Empty, blocked); Assert.True(host.Audio.IsPlaying(first));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", source.Id, out var restart, out error, entityChannel: 4), error);
        Assert.NotEqual(Guid.Empty, restart); Assert.False(host.Audio.IsPlaying(first));
    }

    [Fact]
    public void ExplicitAliasLimitZeroOverridesReferencedLimit()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(NearLimit: 0), ["sound"] = new(NearLimit: 1) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out _, out var error,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var alias, out error,
            sourcePosition: default(System.Numerics.Vector3)), error);
        Assert.NotEqual(Guid.Empty, alias);
    }

    [Fact]
    public void DefaultNearbyLimitReleasesCompletedSounds()
    {
        var mixer = new AudioMixer(); var level = Level();
        for (var index = 0; index < 2; index++)
        {
            Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error,
                sourcePosition: default(System.Numerics.Vector3)), error);
            Assert.NotEqual(Guid.Empty, channel);
        }
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var blocked, out var failure,
            sourcePosition: default(System.Numerics.Vector3)), failure);
        Assert.Equal(Guid.Empty, blocked);
        mixer.Mix(2);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var resumed, out failure,
            sourcePosition: default(System.Numerics.Vector3)), failure);
        Assert.NotEqual(Guid.Empty, resumed);
    }

    [Fact]
    public void ChannelVolumeChangesCanRestoreOriginalSamples()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var channel, out var error, loop: true, volume: 0.25f), error);
        Assert.Equal(new short[] { -8192, 8128 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelVolume(channel, 2, out error), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.False(mixer.TrySetChannelVolume(channel, float.NaN, out error)); Assert.Equal("audio-volume-invalid", error);
        Assert.True(mixer.TrySetChannelVolume(channel, -1, out error), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2)); Assert.True(mixer.IsPlaying(channel));
    }

    [Fact]
    public void AcsVolumeRequestUpdatesOwnedChannelBeforeMixing()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 256 | 4);
        int[] words = [3, 0, 3, 4, 3, 32768, 351, 3, 70, 1];
        var bytes = new byte[words.Length * 4];
        for (var index = 0; index < words.Length; index++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(index * 4), words[index]);
        host.Simulation.Acs.Add(new HCDE.Playsim.AcsProgram { Number = 2, Code = bytes });
        Assert.True(host.Simulation.Acs.Enqueue(2, ReadOnlySpan<int>.Empty, actor)); host.Simulation.Acs.Tick(host.Simulation);
        Assert.True(host.Simulation.PendingSoundRequests.Last().ChangeVolume);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { -15384, 17256, -15384, 17256 }, host.LastMix);
    }

    [Fact]
    public void PausePreservesChannelProgressAndNoPauseKeepsPlaying()
    {
        var mixer = new AudioMixer();
        var channel = mixer.PlayChannel(new short[] { 100, 200 });
        Assert.Equal(new short[] { 100 }, mixer.Mix(1));
        mixer.Paused = true;
        mixer.PlayChannel(new short[] { 50 }, noPause: true);
        Assert.Equal(new short[] { 50 }, mixer.Mix(1)); Assert.True(mixer.IsPlaying(channel));
        mixer.Paused = false; Assert.Equal(new short[] { 200 }, mixer.Mix(1)); Assert.False(mixer.IsPlaying(channel));
    }

    [Theory]
    [InlineData(0, false, 1000)]
    [InlineData(32, true, -31768)]
    [InlineData(64, true, -31768)]
    [InlineData(65536, true, 1000)]
    [InlineData(256, true, 1000)]
    public void PausedAcsPlaybackHonorsUiNoPauseForceAndLoop(int flags, bool resumes, int firstSample)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        host.Audio.Paused = true;
        QueueAcsSound(host, host.Simulation.Players.Single(), flags | 4);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        // Demo PCM also pauses, leaving only exempt sounds audible.
        Assert.Equal(firstSample == 1000 ? 0 : -32768, host.LastMix[0]);
        host.Audio.Paused = false; host.Tick(default);
        Assert.Equal(resumes && firstSample == 1000 ? -30768 : 2000, host.LastMix[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ListenerHeightFlagRemovesVerticalSeparation(bool listenerHeight)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 }, new LevelThing { Type = 14, X = 15 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        source.Z = HCDE.Playsim.Fixed.FromInt(100);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, source, 256 | 4 | (listenerHeight ? 8 : 0));
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(listenerHeight ? -15384 : 1000, host.LastMix[0]);
        source.Z = HCDE.Playsim.Fixed.FromInt(200);
        host.Tick(default); Assert.Equal(listenerHeight ? -15384 : 1000, host.LastMix[0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitSingularRequestSuppressesDuplicateAcrossEntityChannels(bool local)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.Equal(-32768, host.Audio.Mix(1)[0]);
        QueueAcsSound(host, actor, 131072 | 1, local);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.True(host.Audio.IsPlaying(original)); Assert.Equal(32767, host.LastMix[0]);
    }

    [Fact]
    public void ExplicitSingularPlaybackResumesAfterPreviousSoundStops()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var duplicate, out error, singular: true), error);
        Assert.Equal(Guid.Empty, duplicate); mixer.StopChannel(first);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var replacement, out error, singular: true), error);
        Assert.NotEqual(Guid.Empty, replacement);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out error, singular: true), error);
    }

    [Theory]
    [InlineData(4096)]
    [InlineData(256)]
    public void ExplicitNoStopAndLoopFlagsHaveDistinctRestartBehavior(int flag)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level)); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var original, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        QueueAcsSound(host, actor, flag | 4);
        host.Tick(default); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(flag == 4096, host.Audio.IsPlaying(original));
    }

    [Fact]
    public void OverlapFlagKeepsChannelSoundsUntilNonoverlapReplacement()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single(); var level = Level();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var first, out var error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var overlap, out error,
            loop: true, attenuation: 0, entityChannel: 4, overlap: true), error);
        Assert.True(host.Audio.IsPlaying(first)); Assert.True(host.Audio.IsPlaying(overlap));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var replacement, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.False(host.Audio.IsPlaying(first)); Assert.False(host.Audio.IsPlaying(overlap)); Assert.True(host.Audio.IsPlaying(replacement));
    }

    [Fact]
    public void AcsLoopRecognizesDirectActorPlaybackAndOriginalAliasName()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
            SoundAliases = new Dictionary<string, string> { ["alias"] = "sound" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var handle, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.IsActorSoundPlaying(actor.Id, 0, "SOUND"));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 1, "sound"));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "alias"));
        Assert.False(host.IsActorSoundPlaying(uint.MaxValue, 0, "sound"));
        Assert.Equal(-32768, host.Audio.Mix(1)[0]);
        QueueAcsSound(host, actor, 4, loop: true);
        host.Tick(default); Assert.True(host.Audio.IsPlaying(handle)); Assert.Equal(32767, host.LastMix[0]);
        Assert.Equal(1, host.StopActorSound(actor.Id, 4));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "sound"));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "alias", actor.Id, out _, out error, attenuation: 0), error);
        Assert.True(host.IsActorSoundPlaying(actor.Id, 0, "ALIAS"));
        Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "sound"));
        host.Audio.Mix(2); Assert.False(host.IsActorSoundPlaying(actor.Id, 0, "alias"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RepeatedLoopRequestDoesNotRestartOrOverlapExistingSound(bool local)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 4, local, loop: true);
        host.Tick(default);
        Assert.Equal(-32768, host.Audio.Mix(1)[0]);
        QueueAcsSound(host, actor, 0, local, loop: true);
        host.Tick(default);
        Assert.Equal(32767, host.LastMix[0]);
        Assert.Empty(host.LastSoundRequestErrors);
        host.Audio.StopAllChannels();
        QueueAcsSound(host, actor, 0, local, loop: true);
        host.Tick(default); Assert.Equal(-31768, host.LastMix[0]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LocalAcsPlaybackFiltersToListenerAndHasNoActorOwnership(bool listenerSource)
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 }, new LevelThing { Type = 14, X = 100 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = listenerSource ? host.Simulation.Players.Single() : host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 4, local: true);
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(listenerSource ? -31768 : 1000, host.LastMix[0]);
        Assert.Equal(0, host.StopActorSound(actor.Id, -1));
    }

    [Fact]
    public void AcsStopRequestStopsItsActorChannelInQueueOrder()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 4);
        int[] words = [3, 0, 351, 1, 62, 1];
        var bytes = new byte[words.Length * 4];
        for (var index = 0; index < words.Length; index++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(index * 4), words[index]);
        host.Simulation.Acs.Add(new HCDE.Playsim.AcsProgram { Number = 2, Code = bytes });
        Assert.True(host.Simulation.Acs.Enqueue(2, ReadOnlySpan<int>.Empty, actor));
        host.Simulation.Acs.Tick(host.Simulation);
        Assert.True(host.Simulation.PendingSoundRequests.Last().Stop);
        host.Tick(default);
        Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
    }

    [Fact]
    public void ActorStopIncludesAutoOverlapAndCanStopAllChannels()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var auto, out var error, loop: true, attenuation: 0), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var body, out error, loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.Equal(1, host.StopActorSound(actor.Id, 0));
        Assert.False(host.Audio.IsPlaying(auto)); Assert.True(host.Audio.IsPlaying(body));
        Assert.Equal(0, host.StopActorSound(uint.MaxValue, -1));
        Assert.Equal(1, host.StopActorSound(actor.Id, -1)); Assert.False(host.Audio.IsPlaying(body));
        Assert.Equal(0, host.StopActorSound(actor.Id, -1));
    }

    [Fact]
    public void ActorEntityChannelsReplaceOnlyMatchingActorAndChannel()
    {
        var host = ClientHost.DemoRoom(); var level = Level(); var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var first, out var error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var other, out error,
            loop: true, attenuation: 0, entityChannel: 1), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var replacement, out error,
            loop: true, attenuation: 0, entityChannel: 4), error);
        Assert.False(host.Audio.IsPlaying(first)); Assert.True(host.Audio.IsPlaying(other)); Assert.True(host.Audio.IsPlaying(replacement));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "missing", actor.Id, out var empty, out error,
            attenuation: 0, entityChannel: 4), error);
        Assert.Equal(Guid.Empty, empty); Assert.True(host.Audio.IsPlaying(replacement));
        Assert.False(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out _, out error, entityChannel: 8));
        Assert.Equal("audio-entity-channel-invalid", error); Assert.True(host.Audio.IsPlaying(replacement));
    }

    [Fact]
    public void AutoEntityChannelAllowsOverlap()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var first, out var error,
            loop: true, attenuation: 0), error);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out var second, out error,
            loop: true, attenuation: 0), error);
        Assert.True(host.Audio.IsPlaying(first)); Assert.True(host.Audio.IsPlaying(second));
    }

    [Fact]
    public void AcsSoundRequestPlaysOnceOnConfiguredClientTick()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Players.Single();
        QueueAcsSound(host, actor, 4);
        host.Tick(default); Assert.Single(host.Simulation.PendingSoundRequests);
        var wad = Wad(11025); Assert.True(host.TrySetSoundArchive(wad, out var error), error);
        Array.Clear(wad);
        host.Tick(default);
        Assert.Empty(host.Simulation.PendingSoundRequests); Assert.Empty(host.LastSoundRequestErrors);
        Assert.Equal(new short[] { -31768, 32767, 1000, 1000 }, host.LastMix);
        host.Tick(default); Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
    }

    [Fact]
    public void UnsupportedAcsFlagsAreReportedWithoutPlayback()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        Assert.True(host.TrySetSoundArchive(Wad(11025), out var error), error);
        QueueAcsSound(host, actor, 131);
        host.Tick(default);
        Assert.Equal("audio-request-flags-unsupported: sound", Assert.Single(host.LastSoundRequestErrors));
        Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
        Assert.False(host.TrySetSoundArchive(new byte[] { 1 }, out error)); Assert.NotNull(error);
    }

    [Fact]
    public void ActorPlaybackQueuesAndBindsInOneOperation()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 }, new LevelThing { Type = 14, X = 15 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["missing"] = "ABSENT" },
        };
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var actor = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "sound", actor.Id, out var channel, out var error, loop: true), error);
        host.Tick(default); Assert.Equal(-15384, host.LastMix[0]);
        actor.X = HCDE.Playsim.Fixed.FromInt(20);
        host.Tick(default); Assert.Equal(1000, host.LastMix[0]);
        Assert.True(host.Audio.IsPlaying(channel));
        Assert.True(host.TryPlayActorWadSound(Wad(11025), level, "missing", actor.Id, out var missing, out error), error);
        Assert.Equal(Guid.Empty, missing);
    }

    [Fact]
    public void ActorPlaybackFailureDoesNotLeaveQueuedSound()
    {
        var host = ClientHost.DemoRoom(); var actor = host.Simulation.Players.Single();
        Assert.False(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", uint.MaxValue, out var channel, out var error));
        Assert.Equal("audio-source-actor-missing", error); Assert.Equal(Guid.Empty, channel);
        Assert.False(host.TryPlayActorWadSound(Wad(11025), Level(), "sound", actor.Id, out channel, out error));
        Assert.Equal("audio-rolloff-required", error); Assert.Equal(Guid.Empty, channel);
        Assert.Equal(new short[] { 0, 0 }, host.Audio.Mix(2));
    }

    [Fact]
    public void BoundAudioSourceFollowsActorOnClientTicks()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1 }, new LevelThing { Type = 14, X = 15 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        var source = host.Simulation.Actors.Single(actor => actor.DoomEdNum == 14);
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, loop: true), error);
        Assert.True(host.TryBindAudioSource(channel, source.Id, out error), error);
        host.Tick(default); Assert.Equal(-15384, host.LastMix[0]);
        source.X = HCDE.Playsim.Fixed.FromInt(20);
        host.Tick(default); Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
        source.X = HCDE.Playsim.Fixed.FromInt(0);
        host.Tick(default); Assert.Equal(-31768, host.LastMix[0]);
        source.Destroy(); source.X = HCDE.Playsim.Fixed.FromInt(100);
        host.Tick(default); Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
        Assert.False(host.Audio.IsPlaying(channel));
        host.Audio.StopChannel(channel);
        host.Tick(default); Assert.Equal(new short[] { 1000, 1000, 1000, 1000 }, host.LastMix);
        Assert.False(host.TryBindAudioSource(channel, host.Simulation.Players.Single().Id, out error)); Assert.Equal("audio-channel-missing", error);
        Assert.False(host.TryBindAudioSource(channel, uint.MaxValue, out error)); Assert.Equal("audio-source-actor-missing", error);
    }

    [Fact]
    public void ClientTickMovesAudioListenerAndUpdatesPositionedPlayback()
    {
        var level = new PlayLevel
        {
            Things = new[] { new LevelThing { Type = 1, X = 0, Y = 0 } },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL" },
        };
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var host = new ClientHost(HCDE.Playsim.AuthoritySimulation.Start(level));
        Assert.True(host.Audio.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, loop: true), error);
        Assert.True(host.Audio.TrySetChannelPosition(channel, new(15, 0, 0), new(), out error), error);
        host.Tick(default);
        Assert.Equal(new System.Numerics.Vector3(), host.AudioListenerPosition);
        Assert.Equal(new short[] { -15384, 17256, -15384, 17256 }, host.LastMix);
        host.Simulation.Players.Single().X = HCDE.Playsim.Fixed.FromInt(15);
        host.Tick(default);
        Assert.Equal(new System.Numerics.Vector3(15, 0, 0), host.AudioListenerPosition);
        Assert.Equal(new short[] { -31768, 32767, -31768, 32767 }, host.LastMix);
    }

    [Fact]
    public void ListenerMovementUpdatesRetainedSourcesAndLeavesUnpositionedAudio()
    {
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var second, out error, loop: true), error);
        Assert.True(mixer.TrySetChannelPosition(first, new(0, 0, 0), new(), out error), error);
        Assert.True(mixer.TrySetChannelPosition(second, new(40, 0, 0), new(), out error), error);
        Assert.True(mixer.TryUpdateListenerPosition(new(20, 0, 0), out error), error);
        mixer.Play(new short[] { 100 }); Assert.Equal(new short[] { 100 }, mixer.Mix(1));
        Assert.True(mixer.TryUpdateListenerPosition(new(40, 0, 0), out error), error);
        Assert.Equal(new short[] { 32512 }, mixer.Mix(1));
        Assert.True(mixer.TrySetChannelDistance(second, null, out error), error);
        Assert.True(mixer.TryUpdateListenerPosition(new(100, 0, 0), out error), error);
        Assert.Equal(new short[] { -32768 }, mixer.Mix(1));
    }

    [Fact]
    public void FailedListenerUpdateRestoresAllPriorGains()
    {
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var second, out error, loop: true, attenuation: float.MaxValue), error);
        Assert.True(mixer.TrySetChannelPosition(first, new(), new(15, 0, 0), out error), error);
        Assert.True(mixer.TrySetChannelPosition(second, new(), new(), out error), error);
        Assert.False(mixer.TryUpdateListenerPosition(new(20, 0, 0), out error));
        Assert.Equal("audio-distance-invalid", error);
        mixer.StopChannel(second); Assert.Equal(new short[] { -16384 }, mixer.Mix(1));
        Assert.False(mixer.TryUpdateListenerPosition(new(float.NaN, 0, 0), out error));
        Assert.Equal("audio-position-invalid", error);
    }

    [Fact]
    public void PositionUpdatesUseAllAxesAndListenerMovement()
    {
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, loop: true), error);
        var source = new System.Numerics.Vector3(14, 17, 22);
        Assert.True(mixer.TrySetChannelPosition(channel, source, new(5, 5, 22), out error), error);
        Assert.Equal(new short[] { -16384, 16256 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelPosition(channel, source, source, out error), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelPosition(channel, source, new(14, 17, 2), out error), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.MaxValue)]
    public void InvalidOrOverflowingPositionsPreservePlayback(float value)
    {
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error), error);
        Assert.False(mixer.TrySetChannelPosition(channel, new(value, 0, 0), new(-value, 0, 0), out error));
        Assert.Equal("audio-position-invalid", error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.False(mixer.TrySetChannelPosition(channel, new(), new(), out error));
        Assert.Equal("audio-channel-missing", error);
    }

    [Fact]
    public void WadCustomCurveFeedsDistancePlaybackAndIsCopied()
    {
        var original = Wad(11025);
        var wad = new byte[58]; original.AsSpan(0, 22).CopyTo(wad);
        wad[22] = 127; wad[23] = 0;
        original.AsSpan(22, 16).CopyTo(wad.AsSpan(24));
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(4), 2);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(8), 24);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(40), 22);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(44), 2);
        "SNDCURVE"u8.CopyTo(wad.AsSpan(48));
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Custom, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(wad, level, "sound", out var channel, out var error, loop: true), error);
        wad[23] = 127;
        Assert.True(mixer.TrySetChannelDistance(channel, 15, out error), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelDistance(channel, 15, out error, new byte[] { 127, 127 }), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Fact]
    public void DistanceUpdatesScalePlaybackWithoutDestroyingLoopSamples()
    {
        var level = Level(); level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, loop: true, attenuation: 2), error);
        Assert.True(mixer.TrySetChannelDistance(channel, 7.5f, out error), error);
        Assert.Equal(new short[] { -16384, 16256 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelDistance(channel, 10, out error), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2)); Assert.True(mixer.IsPlaying(channel));
        Assert.True(mixer.TrySetChannelDistance(channel, null, out error), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void NonpositiveAttenuationBypassesDistanceCurve(float attenuation)
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var channel, out var error, attenuation: attenuation), error);
        Assert.True(mixer.TrySetChannelDistance(channel, 100, out error), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Fact]
    public void CustomDistanceGainIsAppliedAndClampedLikeBackendGain()
    {
        var mixer = new AudioMixer(); var level = Level();
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Custom, 10, 20);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, loop: true), error);
        Assert.True(mixer.TrySetChannelDistance(channel, 15, out error, new byte[] { 127, 0 }), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
        Assert.True(mixer.TrySetChannelDistance(channel, 15, out error, new byte[] { 127, 255 }), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Fact]
    public void FailedDistanceUpdatesPreservePriorGain()
    {
        var mixer = new AudioMixer(); var level = Level();
        level.GlobalSoundRolloff = new(HCDE.Gamedata.SoundRolloffType.Linear, 10, 20);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error), error);
        Assert.True(mixer.TrySetChannelDistance(channel, 15, out error), error);
        Assert.False(mixer.TrySetChannelDistance(channel, float.NaN, out error)); Assert.Equal("audio-distance-invalid", error);
        Assert.False(mixer.TrySetChannelDistance(channel, -1, out error));
        Assert.Equal(new short[] { -16384, 16256 }, mixer.Mix(2));
        Assert.False(mixer.TrySetChannelDistance(channel, 1, out error)); Assert.Equal("audio-channel-missing", error);
        var direct = mixer.PlayChannel(new short[] { 100 });
        Assert.False(mixer.TrySetChannelDistance(direct, 1, out error)); Assert.Equal("audio-rolloff-required", error);
        Assert.Equal(new short[] { 100 }, mixer.Mix(1));
    }

    [Theory]
    [InlineData(5f, true)]
    [InlineData(-5f, true)]
    [InlineData(0f, false)]
    public void ForcedRolloffOverridesOnlyWithNonzeroMinimum(float minimum, bool overrides)
    {
        var level = Level();
        var inherited = new HCDE.Gamedata.SoundRolloff(MinDistance: 10, MaxDistance: 100);
        var forced = new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Linear, minimum, 40);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(Rolloff: inherited) };
        level.GlobalSoundRolloff = new(MinDistance: 20);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var channel, out var error, forcedRolloff: forced), error);
        Assert.True(mixer.TryGetChannelRolloff(channel, out var actual)); Assert.Equal(overrides ? forced : inherited, actual);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out error, forcedRolloff: forced), error);
    }

    [Fact]
    public void ZeroForcedRolloffAllowsGlobalFallback()
    {
        var level = Level(); var global = new HCDE.Gamedata.SoundRolloff(MinDistance: 10, MaxDistance: 100);
        level.GlobalSoundRolloff = global;
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var channel, out var error, forcedRolloff: new(MinDistance: 0, MaxDistance: 50)), error);
        Assert.True(mixer.TryGetChannelRolloff(channel, out var actual)); Assert.Equal(global, actual);
    }

    [Theory]
    [InlineData(float.NaN, 1f)]
    [InlineData(1f, float.PositiveInfinity)]
    public void InvalidForcedRolloffDoesNotResolveOrQueue(float minimum, float maximum)
    {
        var mixer = new AudioMixer(); var draws = 0;
        Assert.False(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var channel, out var error,
            () => { draws++; return 0; }, forcedRolloff: new(MinDistance: minimum, MaxDistance: maximum)));
        Assert.Equal("audio-rolloff-invalid", error); Assert.Equal(Guid.Empty, channel); Assert.Equal(0, draws);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(5f)]
    [InlineData(-5f)]
    public void RolloffInheritanceUsesZeroMinimumAsUnset(float minimum)
    {
        var level = Level();
        var original = new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Linear, minimum, 40);
        var target = new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Log, 10, 2);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(NearLimit: 2, Rolloff: original), ["sound"] = new(Rolloff: target) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var channel, out var error), error);
        Assert.True(mixer.TryGetChannelRolloff(channel, out var actual));
        Assert.Equal(minimum == 0 ? target : original, actual);
        mixer.Mix(2);
        Assert.False(mixer.TryGetChannelRolloff(channel, out _));
    }

    [Fact]
    public void RolloffFallsBackToGlobalAndRetainsChannelSnapshot()
    {
        var level = Level();
        var global = new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Custom, 20, 100);
        level.GlobalSoundRolloff = global;
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["sound"] = new(Rolloff: new(MinDistance: 0, MaxDistance: 50)) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var channel, out var error), error);
        level.GlobalSoundRolloff = new(MinDistance: 30);
        Assert.True(mixer.TryGetChannelRolloff(channel, out var actual));
        Assert.Equal(global, actual);
        var direct = mixer.PlayChannel(new short[] { 1 });
        Assert.True(mixer.TryGetChannelRolloff(direct, out var unset)); Assert.Null(unset);
    }

    [Fact]
    public void RolloffSkipsConsecutiveRandomHeaders()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>>
        { ["group"] = new[] { "nested", "missing" }, ["nested"] = new[] { "sound", "missing" } };
        var target = new HCDE.Gamedata.SoundRolloff(MinDistance: 10, MaxDistance: 100);
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["nested"] = new(Rolloff: new(MinDistance: 99)), ["sound"] = new(Rolloff: target) };
        var mixer = new AudioMixer(); var calls = 0;
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var channel, out var error, () => { calls++; return 0; }), error);
        Assert.Equal(2, calls);
        Assert.True(mixer.TryGetChannelRolloff(channel, out var actual)); Assert.Equal(target, actual);
    }

    [Fact]
    public void AttenuationUsesResolvedSoundAndSkipsAliasSetting()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["alias"] = new(Attenuation: 100), ["sound"] = new(Attenuation: 2) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var channel, out var error, attenuation: 0.5f), error);
        Assert.True(mixer.TryGetChannelDistanceScale(channel, out var scale)); Assert.Equal(1, scale);
        mixer.StopChannel(channel); Assert.False(mixer.TryGetChannelDistanceScale(channel, out _));
    }

    [Fact]
    public void AttenuationSkipsConsecutiveRandomHeaders()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "nested", "missing" }, ["nested"] = new[] { "sound", "missing" } };
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["group"] = new(Attenuation: 2), ["nested"] = new(Attenuation: 100), ["sound"] = new(Attenuation: 3) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var channel, out var error, () => 0, attenuation: 0.5f), error);
        Assert.True(mixer.TryGetChannelDistanceScale(channel, out var scale)); Assert.Equal(3, scale);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void FiniteSignedAttenuationIsRetained(float value)
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var channel, out var error, attenuation: value), error);
        Assert.True(mixer.TryGetChannelDistanceScale(channel, out var scale)); Assert.Equal(value, scale);
    }

    [Fact]
    public void InvalidAttenuationDoesNotQueueAudio()
    {
        var mixer = new AudioMixer();
        Assert.False(mixer.TryPlayWadSound(Wad(11025), Level(), "sound", out var channel, out var error, attenuation: float.NaN));
        Assert.Equal(Guid.Empty, channel); Assert.Equal("audio-attenuation-invalid", error);
    }

    [Fact]
    public void ConsecutiveRandomGroupsSkipIntermediatePitchDefaults()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "nested", "missing" }, ["nested"] = new[] { "sound", "missing" } };
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["group"] = new(NearLimit: -1), ["nested"] = new(NearLimit: 2, DefPitch: 0.5f), ["sound"] = new(DefPitch: 2) };
        var mixer = new AudioMixer(); var calls = 0;
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var error, () => { calls++; return 0; }), error);
        Assert.Equal(2, calls); Assert.Equal(new short[] { -32768, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void AliasBetweenRandomGroupsCreatesSeparateInheritanceStep()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "bridge", "missing" }, ["nested"] = new[] { "sound", "missing" } };
        level.SoundAliases = new Dictionary<string, string> { ["bridge"] = "nested" };
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["group"] = new(NearLimit: -1), ["bridge"] = new(NearLimit: -1), ["nested"] = new(NearLimit: 2, DefPitch: 0.5f), ["sound"] = new(DefPitch: 2) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var error, () => 0), error);
        Assert.Equal(new short[] { -32768, -128, 32512, 32512 }, mixer.Mix(4));
    }

    [Fact]
    public void AutomaticPitchInheritsThroughAliasesUnlessLimitIsExplicit()
    {
        var level = Level();
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(NearLimit: -1, DefPitch: 0.5f), ["sound"] = new(DefPitch: 2) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var error), error);
        Assert.Equal(new short[] { -32768, 0 }, mixer.Mix(2));
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(NearLimit: 2, DefPitch: 0.5f), ["sound"] = new(DefPitch: 2) };
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out error), error);
        Assert.Equal(new short[] { -32768, -128, 32512, 32512 }, mixer.Mix(4));
    }

    [Fact]
    public void ExplicitPitchOverridesAutomaticSettingsWithoutDraws()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(PitchMask: 7, DefPitch: 1, DefPitchMax: 2) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var error, pitch: 1,
            nextPitchFloat: () => throw new InvalidOperationException()), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Fact]
    public void AutomaticRandomPitchRequiresAndConsumesProvider()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(DefPitch: 1, DefPitchMax: 3) };
        var mixer = new AudioMixer();
        Assert.False(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var error));
        Assert.Equal("audio-pitch-random-provider-required", error);
        var calls = 0;
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out error, nextPitchFloat: () => { calls++; return 0.5f; }), error);
        Assert.Equal(1, calls); Assert.Equal(new short[] { -32768, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void ExplicitPitchChangesWadPlaybackAndLoopDuration()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out var channel, out var error, loop: true, pitch: 2), error);
        Assert.Equal(new short[] { -32768, -32768, -32768 }, mixer.Mix(3));
        Assert.True(mixer.StopChannel(channel));
        Assert.False(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out channel, out error, pitch: float.NaN));
        Assert.Equal(Guid.Empty, channel); Assert.Equal("audio-pitch-invalid", error);
    }

    [Fact]
    public void SingularDirectSoundSuppressesDuplicateUntilStoppedOrCompleted()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>(StringComparer.OrdinalIgnoreCase) { ["sound"] = new(Singular: true) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var first, out var error, loop: true), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var suppressed, out error), error);
        Assert.Equal(Guid.Empty, suppressed); Assert.True(mixer.IsPlaying(first));
        mixer.StopChannel(first);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var next, out error), error);
        Assert.NotEqual(Guid.Empty, next); mixer.Mix(2);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out next, out error), error);
        Assert.NotEqual(Guid.Empty, next);
    }

    [Fact]
    public void SingularResolvedSoundUsesNativeOriginalChannelIdentity()
    {
        var level = Level(); level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings> { ["sound"] = new(Singular: true) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var alias, out var error), error);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var second, out error), error);
        Assert.NotEqual(Guid.Empty, second);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "sound", out var direct, out error), error);
        Assert.NotEqual(Guid.Empty, direct);
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var suppressed, out error), error);
        Assert.Equal(Guid.Empty, suppressed); Assert.True(mixer.IsPlaying(alias));
    }

    [Fact]
    public void DecodedWadSoundLoopsUntilStopped()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out var channel, out var error, loop: true), error);
        Assert.Equal(new short[] { -32768, 32512, -32768 }, mixer.Mix(3));
        Assert.True(mixer.IsPlaying(channel)); Assert.True(mixer.StopChannel(channel));
    }

    [Theory]
    [InlineData(0.5f, 1f, -16384, 16256)]
    [InlineData(0.5f, 0.5f, -8192, 8128)]
    [InlineData(2f, 1f, -32768, 32512)]
    [InlineData(0f, 1f, 0, 0)]
    [InlineData(-1f, 1f, 0, 0)]
    public void PlaybackVolumeMultipliesAndClamps(float setting, float volume, int first, int second)
    {
        var level = Level();
        level.SoundSettings = new Dictionary<string, HCDE.Gamedata.SndInfoSoundSettings>
        { ["alias"] = new(Volume: setting), ["sound"] = new(Volume: 0) };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "alias", out var error, volume: volume), error);
        Assert.Equal(new short[] { (short)first, (short)second }, mixer.Mix(2));
    }

    [Fact]
    public void SilentVolumeSkipsRandomSelection()
    {
        var level = Level(); level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "alias", "missing" } };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var error,
            () => throw new InvalidOperationException("Unexpected random draw"), volume: 0), error);
        Assert.Equal(new short[] { 0 }, mixer.Mix(1));
    }

    [Fact]
    public void InvalidVolumeFailsWithoutQueuedAudio()
    {
        var mixer = new AudioMixer();
        Assert.False(mixer.TryPlayWadSound(Wad(11025), Level(), "alias", out var error, volume: float.NaN));
        Assert.Equal("audio-volume-invalid", error); Assert.Equal(new short[] { 0 }, mixer.Mix(1));
    }

    [Fact]
    public void AliasedDecodedSoundReachesMixerAndIsCopied()
    {
        var wad = Wad(11025);
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(wad, Level(), "alias", out var channel, out var error), error);
        Assert.True(mixer.IsPlaying(channel));
        wad[20] = 128;
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
        Assert.False(mixer.IsPlaying(channel));
    }

    [Fact]
    public void RandomGroupSelectionReachesMixer()
    {
        var level = Level();
        level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = new[] { "missing", "alias" } };
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), level, "group", out var error, () => 1), error);
        Assert.Equal(new short[] { -32768, 32512 }, mixer.Mix(2));
    }

    [Fact]
    public void MismatchedRateConvertsBeforeEnqueuing()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(22050), Level(), "alias", out var error), error);
        Assert.Equal(new short[] { -32768, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void MatchingCustomRateAndMutePreserveExistingBehavior()
    {
        var mixer = new AudioMixer { SampleRate = 22050, Muted = true };
        Assert.True(mixer.TryPlayWadSound(Wad(22050), Level(), "alias", out var error), error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    [Fact]
    public void MissingResourceIsSilentAndBadEncodingFails()
    {
        var mixer = new AudioMixer();
        Assert.True(mixer.TryPlayWadSound(Wad(11025), Level(), "missing", out var error), error);
        var wad = Wad(11025); wad[12] = 2;
        Assert.False(mixer.TryPlayWadSound(wad, Level(), "alias", out error));
        Assert.Equal("sound-format-unsupported", error);
        Assert.Equal(new short[] { 0, 0 }, mixer.Mix(2));
    }

    private static void QueueAcsSound(ClientHost host, HCDE.Playsim.Actor actor, int channel, bool local = false, bool loop = false)
    {
        int[] words = local || loop
            ? [3, 0, 3, 0, 3, channel, 3, 65536, 3, loop ? 1 : 0, 3, 65536, 3, local ? 1 : 0, 351, 7, 61, 1]
            : [3, 0, 3, 0, 3, channel, 351, 3, 61, 1];
        var bytes = new byte[words.Length * 4];
        for (var index = 0; index < words.Length; index++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(index * 4), words[index]);
        host.Simulation.Acs.Add(new HCDE.Playsim.AcsProgram { Number = 1, StringTable = ["sound"], Code = bytes });
        Assert.True(host.Simulation.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, actor));
        host.Simulation.Acs.Tick(host.Simulation);
    }

    private static PlayLevel Level() => new()
    {
        SoundAliases = new Dictionary<string, string> { ["alias"] = "sound" },
        SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["missing"] = "ABSENT" },
    };

    private static byte[] Wad(ushort frequency)
    {
        var wad = new byte[38];
        "PWAD"u8.CopyTo(wad);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(8), 22);
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan(12), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan(14), frequency);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(16), 2);
        wad[20] = 0; wad[21] = 255;
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(22), 12);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(26), 10);
        "DSPISTOL"u8.CopyTo(wad.AsSpan(30));
        return wad;
    }
}
