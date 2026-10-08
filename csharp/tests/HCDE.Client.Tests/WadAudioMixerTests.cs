using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Client.Tests;

public class WadAudioMixerTests
{
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
