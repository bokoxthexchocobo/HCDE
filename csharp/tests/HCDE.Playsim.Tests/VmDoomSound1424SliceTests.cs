using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1424SliceTests
{
    [Fact]
    public void MusicLookupInitAndShutdown()
    {
        Assert.Equal(342, VmDoomSound1424Slice.BandEnd - VmDoomSound1424Slice.BandStart + 1);
        Assert.Equal(232, VmDoomSound1424Slice.PathEnd);
        Assert.Equal(575, VmDoomSound1424Slice.ActorSound);
        Assert.Equal(16, VmDoomSound1424Slice.MusicNamespace);
        var nested = new VmDoomSound1424Slice.State
        {
            Files = { "sprites/dog.ogg", "music/d_e1m1.ogg", @"MUSIC\D_E1M1.mid", "music/notes.txt", null },
        };
        Assert.Equal(2, VmDoomSound1424Slice.FindNested(nested, "d_e1m1"));
        Assert.Contains("music:FindMusic nested match 'd_e1m1' -> 'MUSIC\\D_E1M1.mid' lump=2", nested.Traces);
        Assert.Equal(-1, VmDoomSound1424Slice.FindNested(nested, null));
        Assert.Equal(-1, VmDoomSound1424Slice.FindNested(nested, "missing"));
        var named = new VmDoomSound1424Slice.State { Names = { ["d_e1m1"] = 9 }, Files = { "music/d_e1m1.ogg" } };
        Assert.Equal(9, VmDoomSound1424Slice.FindMusic(named, "d_e1m1"));
        Assert.Empty(named.Traces);
        var fallen = new VmDoomSound1424Slice.State { Files = { "music/d_e1m1.ogg" }, Full = { ["d_e1m1"] = 4 } };
        Assert.Equal(0, VmDoomSound1424Slice.FindMusic(fallen, "D_E1M1"));
        var full = new VmDoomSound1424Slice.State { Full = { ["maps/song"] = 4 } };
        Assert.Equal(4, VmDoomSound1424Slice.FindMusic(full, "maps/song"));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.FindMusic(new VmDoomSound1424Slice.State(), null));
        var init = new VmDoomSound1424Slice.State { CurveLump = 3, Curve = new byte[] { 9, 8 } };
        VmDoomSound1424Slice.Init(init);
        Assert.True(init.Callbacks);
        Assert.True(init.HasEngine);
        Assert.Equal(1, init.Created);
        Assert.Equal(1, init.SoundInits);
        Assert.False(init.MusicDisabled);
        Assert.Equal(1, init.MusicInits);
        Assert.Equal(1, init.VolumeCallbacks);
        Assert.Equal(new byte[] { 9, 8 }, init.CurveLoaded);
        Assert.Equal(1, init.EngineInits);
        Assert.Contains("sound:S_Init called", init.Traces);
        var quiet = new VmDoomSound1424Slice.State { HasEngine = true, NoSound = true };
        VmDoomSound1424Slice.Init(quiet);
        Assert.Equal(0, quiet.Created);
        Assert.True(quiet.MusicDisabled);
        var blocked = new VmDoomSound1424Slice.State { NoMusic = true, ArgsMissing = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Init(blocked));
        Assert.Equal(1, blocked.Created);
        Assert.Equal(1, blocked.SoundInits);
        Assert.Equal(0, blocked.MusicInits);
        var down = new VmDoomSound1424Slice.State { HasEngine = true, GSnd = true, Levels = new List<int> { 1, 2 }, LastSong = "run" };
        VmDoomSound1424Slice.Shutdown(down);
        Assert.True(down.MusicStopped);
        Assert.Equal(2, down.Sequences);
        Assert.False(down.HasEngine);
        Assert.Equal(1, down.EngineShutdowns);
        Assert.Equal("", down.LastSong);
        Assert.Equal(1, down.SoundClosed);
        var bare = new VmDoomSound1424Slice.State { HasEngine = false, GSnd = false, Levels = new() };
        VmDoomSound1424Slice.Shutdown(bare);
        Assert.Equal(0, bare.EngineShutdowns);
        Assert.Equal(0, bare.SoundClosed);
        Assert.Equal("", bare.LastSong);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Shutdown(new VmDoomSound1424Slice.State { Levels = null }));
    }

    [Fact]
    public void LevelStartPrecacheAndPlayback()
    {
        var reload = new VmDoomSound1424Slice.State
        {
            GSnd = true,
            HasEngine = true,
            SoundInfo = "local.snd",
            SndSeq = "local.seq",
            Full = { ["local.snd"] = 6, ["local.seq"] = 7 },
        };
        VmDoomSound1424Slice.Start(reload);
        Assert.Equal(1, reload.StoppedChannels);
        Assert.Equal(1, reload.Unloaded);
        Assert.True(reload.InfoGlobal);
        Assert.Equal(6, reload.AddedLocal);
        Assert.Equal(7, reload.SeqLump);
        Assert.Equal("local.snd", reload.LastInfo);
        Assert.Equal("local.seq", reload.LastSeq);
        var again = new VmDoomSound1424Slice.State
        {
            GSnd = true,
            HasEngine = true,
            SoundInfo = "LOCAL.SND",
            SndSeq = "local.seq",
            LastInfo = "local.snd",
            LastSeq = "LOCAL.SEQ",
        };
        VmDoomSound1424Slice.Start(again);
        Assert.Equal(0, again.Unloaded);
        Assert.Equal(-2, again.SeqLump);
        Assert.Equal("LOCAL.SND", again.LastInfo);
        var seq = new VmDoomSound1424Slice.State
        {
            GSnd = true,
            HasEngine = true,
            SoundInfo = "same",
            SndSeq = "next",
            LastInfo = "same",
            LastSeq = "old",
            Full = { ["next"] = -1 },
        };
        VmDoomSound1424Slice.Start(seq);
        Assert.Equal(0, seq.Unloaded);
        Assert.Equal(-1, seq.SeqLump);
        var missing = new VmDoomSound1424Slice.State { GSnd = true, HasEngine = true, HasInfo = false, LastInfo = "" };
        VmDoomSound1424Slice.Start(missing);
        Assert.Equal(1, missing.Unloaded);
        Assert.Equal(-1, missing.SeqLump);
        Assert.Equal("", missing.LastInfo);
        var dead = new VmDoomSound1424Slice.State { GSnd = true, HasEngine = true, LevelMissing = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Start(dead));
        Assert.Equal(1, dead.StoppedChannels);
        var idle = new VmDoomSound1424Slice.State { GSnd = false, HasEngine = true, LevelMissing = true };
        VmDoomSound1424Slice.Start(idle);
        Assert.Equal(0, idle.StoppedChannels);
        var cache = new VmDoomSound1424Slice.State
        {
            GSnd = true,
            Actors = new List<bool> { true, false, true },
            GameSounds = new List<int> { 4 },
            MapSounds = new List<int> { 8, 9 },
        };
        VmDoomSound1424Slice.Precache(cache);
        Assert.Equal(1, cache.MarkedUnused);
        Assert.Equal(2, cache.VirtualMarks);
        Assert.Equal(new[] { 4, 8, 9 }, cache.Used);
        Assert.Equal(1, cache.CachedMarked);
        var other = new VmDoomSound1424Slice.State { GSnd = true, SameLevel = false };
        VmDoomSound1424Slice.Precache(other);
        Assert.Equal(0, other.MarkedUnused);
        var nullLevel = new VmDoomSound1424Slice.State { GSnd = true, LevelNull = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Precache(nullLevel));
        Assert.Equal(1, nullLevel.MarkedUnused);
        var nullMap = new VmDoomSound1424Slice.State { GSnd = true, GameSounds = new List<int> { 1 }, MapSounds = null };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Precache(nullMap));
        Assert.Equal(new[] { 1 }, nullMap.Used);
        Assert.Equal(0, nullMap.CachedMarked);
        var data = new VmDoomSound1424Slice.State { LastInfo = "a", LastSeq = "b" };
        VmDoomSound1424Slice.InitData(data);
        Assert.Equal("", data.LastInfo);
        Assert.Equal("", data.LastSeq);
        Assert.False(data.InfoGlobal);
        Assert.Equal(-1, data.SeqLump);
        var heard = new VmDoomSound1424Slice.State { HasEngine = true };
        VmDoomSound1424Slice.SoundPitch(heard, 4, 32, 3, 0.5f, 1f, 1.25f, 2f);
        VmDoomSound1424Slice.Sound(heard, 1, 0, 5, 1f, 0f);
        Assert.Equal(0, VmDoomSound1424Slice.ActionSound(heard, 7, 39, 1, 1, 0.5, 0));
        Assert.Equal(0, VmDoomSound1424Slice.ActionStart(heard, 8, 8, 16, 1, 1.5, 1.25, 0.25));
        Assert.Equal(0, VmDoomSound1424Slice.ActionStartAt(heard, 1, 2, 3, 9, 4, 32, 1.5, 3, 1.25, 0.5));
        Assert.Equal(VmDoomSound1424Slice.SourceNone, heard.Plays[0].Source);
        Assert.Equal(1.25f, heard.Plays[0].Pitch);
        Assert.Equal(2f, heard.Plays[0].Start);
        Assert.Equal(0f, heard.Plays[1].Pitch);
        Assert.Equal(0f, heard.Plays[1].Start);
        Assert.Equal(7, heard.Plays[2].Channel);
        Assert.Equal(32, heard.Plays[2].Flags);
        Assert.Equal(8, heard.Plays[3].Channel);
        Assert.Equal(16, heard.Plays[3].Flags);
        var at = heard.Plays[4];
        Assert.Equal(VmDoomSound1424Slice.SourceUnattached, at.Source);
        Assert.Equal(new[] { 1f, 3f, 2f }, new[] { at.X, at.Z, at.Y });
        Assert.True(at.Position);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Sound(new VmDoomSound1424Slice.State(), 0, 0, 0, 0, 0));
        var reserved = new VmDoomSound1424Slice.State { SfxFlag = VmDoomSound1424Slice.PlayerReserve, Ent = "player" };
        VmDoomSound1424Slice.Cache(reserved);
        Assert.Equal(0, reserved.BaseCaches);
        Assert.Equal(3, VmDoomSound1424Slice.Resolve(reserved, 3));
        Assert.Equal("player", reserved.Skinned);
        var sector = new VmDoomSound1424Slice.State { SfxFlag = 1, Type = 3, Ent = "nope" };
        Assert.Equal(4, VmDoomSound1424Slice.Resolve(sector, 4));
        Assert.Null(sector.Skinned);
        var plain = new VmDoomSound1424Slice.State { SfxFlag = 0 };
        VmDoomSound1424Slice.Cache(plain);
        Assert.Equal(1, plain.BaseCaches);
        Assert.Equal(5, VmDoomSound1424Slice.Resolve(plain, 5));
        Assert.Equal(1, plain.BaseResolves);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Cache(new VmDoomSound1424Slice.State()));
        Assert.False(VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State { EntNull = true, SectorNull = true }));
        Assert.False(VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State { Euthanize = true, SectorNull = true }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State { SectorNull = true }));
        Assert.False(VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State { SectorSilent = true }));
        Assert.False(VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State { SameLevel = false }));
        Assert.False(VmDoomSound1424Slice.Verify(new VmDoomSound1424Slice.State
        {
            Flags = VmDoomSound1424Slice.MaybeLocal,
            Compat = VmDoomSound1424Slice.SilentPickup,
            Listener = false,
        }));
        var magic = new VmDoomSound1424Slice.State { Compat = VmDoomSound1424Slice.MagicSilence, Channel = 4, Listener = true, Flags = VmDoomSound1424Slice.MaybeLocal };
        Assert.True(VmDoomSound1424Slice.Verify(magic));
        Assert.Equal(VmDoomSound1424Slice.Weapon, magic.Channel);
        var stop = new VmDoomSound1424Slice.State();
        VmDoomSound1424Slice.StopChannel(stop);
        Assert.Null(stop.Source);
        Assert.Equal(1, stop.BaseStops);
        var evicted = new VmDoomSound1424Slice.State { ChanFlags = VmDoomSound1424Slice.Evicted, Source = "keep" };
        VmDoomSound1424Slice.StopChannel(evicted);
        Assert.Equal("keep", evicted.Source);
        Assert.Equal(1, evicted.BaseStops);
        var gone = new VmDoomSound1424Slice.State { Chan = null, Source = "keep" };
        VmDoomSound1424Slice.StopChannel(gone);
        Assert.Equal("keep", gone.Source);
        Assert.Equal(1, gone.BaseStops);
    }
}
