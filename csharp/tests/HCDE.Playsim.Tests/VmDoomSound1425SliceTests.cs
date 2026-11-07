using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1425SliceTests
{
    static VmDoomSound1425Slice.State Live() => new()
    {
        HasEngine = true,
        SameLevel = true,
        Id = 7,
        Volume = 0.5f,
        Attenuation = 1f,
        Pitch = 1.25f,
        Start = 0.5f,
        Channel = 4,
    };

    [Fact]
    public void RumbleSourcesAndRolloff()
    {
        Assert.Equal(277, VmDoomSound1425Slice.BandEnd - VmDoomSound1425Slice.BandStart + 1);
        Assert.Equal(574, VmDoomSound1425Slice.StopEnd);
        Assert.Equal(852, VmDoomSound1425Slice.Listener);
        var both = Live();
        both.Flags = VmDoomSound1425Slice.NoRumble | VmDoomSound1425Slice.Rumble;
        VmDoomSound1425Slice.PitchActor(both);
        Assert.Equal(VmDoomSound1425Slice.NoRumble, both.Flags);
        Assert.Equal(VmDoomSound1425Slice.SourceActor, both.Plays[0].Source);
        Assert.Equal(1.25f, both.Plays[0].Pitch);
        var self = Live();
        self.Haptics = true;
        self.PlayerSelf = true;
        VmDoomSound1425Slice.PitchActor(self);
        Assert.Equal(VmDoomSound1425Slice.Rumble, self.Flags);
        var shot = Live();
        shot.Haptics = true;
        shot.ActorFlags = VmDoomSound1425Slice.Missile;
        shot.TargetNull = false;
        shot.TargetSelf = true;
        VmDoomSound1425Slice.PitchActor(shot);
        Assert.Equal(VmDoomSound1425Slice.Rumble, shot.Flags);
        var stray = Live();
        stray.Haptics = true;
        stray.ActorFlags = VmDoomSound1425Slice.Missile;
        stray.TargetNull = true;
        VmDoomSound1425Slice.PitchActor(stray);
        Assert.Equal(0, stray.Flags);
        Assert.Single(stray.Plays);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.PitchActor(new VmDoomSound1425Slice.State { Haptics = true, EntNull = true }));
        var dead = Live();
        dead.EntNull = true;
        VmDoomSound1425Slice.PitchActor(dead);
        Assert.Empty(dead.Plays);
        var dying = Live();
        dying.Euthanize = true;
        dying.SectorNull = true;
        VmDoomSound1425Slice.PitchActor(dying);
        Assert.Empty(dying.Plays);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.PitchActor(new VmDoomSound1425Slice.State { SectorNull = true }));
        var quiet = Live();
        quiet.SectorSilent = true;
        VmDoomSound1425Slice.PitchActor(quiet);
        Assert.Empty(quiet.Plays);
        var away = Live();
        away.SameLevel = false;
        VmDoomSound1425Slice.PitchActor(away);
        Assert.Empty(away.Plays);
        var local = Live();
        local.Flags = VmDoomSound1425Slice.MaybeLocal;
        local.Compat = VmDoomSound1425Slice.SilentPickup;
        local.IsListener = false;
        VmDoomSound1425Slice.PitchActor(local);
        Assert.Empty(local.Plays);
        var magic = Live();
        magic.Compat = VmDoomSound1425Slice.Magic;
        magic.Channel = 4;
        VmDoomSound1425Slice.PitchActor(magic);
        Assert.Equal(VmDoomSound1425Slice.Weapon, magic.Plays[0].Channel);
        var forced = Live();
        forced.Pitch = 1.25f;
        VmDoomSound1425Slice.SoundActor(forced);
        Assert.Equal(1.25f, forced.Pitch);
        Assert.Equal(0f, forced.Plays[0].Pitch);
        var curve = Live();
        curve.Min = 8;
        curve.Max = 200;
        curve.Attenuation = 0.25f;
        VmDoomSound1425Slice.MinMax(curve);
        Assert.True(curve.Plays[0].Rolloff);
        Assert.Equal(VmDoomSound1425Slice.RolloffLinear, curve.Plays[0].RolloffType);
        Assert.Equal(1f, curve.Plays[0].Attenuation);
        Assert.Equal(8f, curve.Plays[0].Min);
        Assert.Equal(200f, curve.Plays[0].Max);
        Assert.False(curve.Plays[0].Position);
        var skipped = Live();
        skipped.SameLevel = false;
        VmDoomSound1425Slice.MinMax(skipped);
        Assert.Empty(skipped.Plays);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.SoundPoly(new VmDoomSound1425Slice.State { PolyNull = true }));
        var poly = Live();
        poly.SameLevel = false;
        VmDoomSound1425Slice.SoundPoly(poly);
        Assert.Empty(poly.Plays);
        poly.SameLevel = true;
        VmDoomSound1425Slice.SoundPoly(poly);
        Assert.Equal(VmDoomSound1425Slice.SourcePoly, poly.Plays[0].Source);
        var point = Live();
        point.PosX = 1;
        point.PosY = 2;
        point.PosZ = 3;
        point.SameLevel = false;
        VmDoomSound1425Slice.SoundPoint(point);
        Assert.Empty(point.Plays);
        point.SameLevel = true;
        VmDoomSound1425Slice.SoundPoint(point);
        Assert.Equal(new[] { 1f, 3f, 2f }, new[] { point.Plays[0].X, point.Plays[0].Z, point.Plays[0].Y });
        Assert.Equal(VmDoomSound1425Slice.SourceUnattached, point.Plays[0].Source);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.SoundSector(new VmDoomSound1425Slice.State { SectorObjNull = true }));
        var sector = Live();
        sector.SameLevel = false;
        VmDoomSound1425Slice.SoundSector(sector);
        Assert.Empty(sector.Plays);
        sector.SameLevel = true;
        VmDoomSound1425Slice.SoundSector(sector);
        Assert.Equal(VmDoomSound1425Slice.SourceSector, sector.Plays[0].Source);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.PitchActor(new VmDoomSound1425Slice.State { HasEngine = false }));
    }

    [Fact]
    public void PlaybackStopsAndQueries()
    {
        var missing = Live();
        missing.EntNull = true;
        missing.SectorNull = true;
        VmDoomSound1425Slice.PlayPitch(missing);
        Assert.Empty(missing.Plays);
        var hole = Live();
        hole.SectorNull = true;
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.PlayPitch(hole));
        var busy = Live();
        busy.Flags = VmDoomSound1425Slice.NoStop;
        busy.PlayingResult = true;
        VmDoomSound1425Slice.PlayPitch(busy);
        Assert.Equal("actor", busy.Plays[0].Kind);
        Assert.Single(busy.Plays);
        var free = Live();
        free.Flags = VmDoomSound1425Slice.NoStop;
        free.PlayingResult = false;
        free.Compat = VmDoomSound1425Slice.Magic;
        VmDoomSound1425Slice.PlayPitch(free);
        Assert.Equal(VmDoomSound1425Slice.Auto, free.Plays[0].Channel);
        Assert.Equal("start", free.Plays[1].Kind);
        Assert.Equal(VmDoomSound1425Slice.Weapon, free.Plays[1].Channel);
        var open = Live();
        VmDoomSound1425Slice.PlayPitch(open);
        Assert.Equal("start", open.Plays[0].Kind);
        var hidden = Live();
        hidden.Flags = VmDoomSound1425Slice.Local;
        hidden.ListenerView = false;
        VmDoomSound1425Slice.PlayPitch(hidden);
        Assert.Empty(hidden.Plays);
        var heard = Live();
        heard.Flags = VmDoomSound1425Slice.Local | VmDoomSound1425Slice.NoStop;
        heard.ListenerView = true;
        heard.NoneBusy = true;
        VmDoomSound1425Slice.PlayPitch(heard);
        Assert.Equal("none", heard.Plays[0].Kind);
        Assert.Equal(VmDoomSound1425Slice.SourceNone, heard.Plays[0].Source);
        Assert.Single(heard.Plays);
        var ear = Live();
        ear.Flags = VmDoomSound1425Slice.Local;
        ear.ListenerView = true;
        ear.Attenuation = 1f;
        VmDoomSound1425Slice.PlayPitch(ear);
        Assert.Equal(VmDoomSound1425Slice.SourceNone, ear.Plays[0].Source);
        Assert.Equal(0f, ear.Plays[0].Attenuation);
        Assert.Equal(1f, ear.Attenuation);
        var plain = Live();
        plain.Pitch = 1.25f;
        plain.Start = 0.5f;
        VmDoomSound1425Slice.Play(plain);
        Assert.Equal(0f, plain.Plays[0].Pitch);
        Assert.Equal(0f, plain.Plays[0].Start);
        Assert.Equal(1.25f, plain.Pitch);
        var deco = Live();
        deco.Flags = 4;
        deco.Looping = 1;
        deco.Local = 1;
        deco.ListenerView = true;
        VmDoomSound1425Slice.Decorate(deco);
        Assert.Equal(4, deco.Channel);
        Assert.Equal("none", deco.Plays[0].Kind);
        Assert.Equal(0f, deco.Plays[1].Attenuation);
        Assert.Equal(VmDoomSound1425Slice.Loop | VmDoomSound1425Slice.NoStop | VmDoomSound1425Slice.Local, deco.Plays[1].Flags);
        var stopped = Live();
        stopped.Compat = VmDoomSound1425Slice.Magic;
        stopped.Channel = 6;
        VmDoomSound1425Slice.StopActor(stopped);
        VmDoomSound1425Slice.StopSector(stopped);
        VmDoomSound1425Slice.StopPoly(stopped);
        Assert.Equal(new[] { -1, -1, -1 }, stopped.Plays.Select(p => p.Channel).ToArray());
        Assert.Equal(new[] { VmDoomSound1425Slice.SourceActor, VmDoomSound1425Slice.SourceSector, VmDoomSound1425Slice.SourcePoly }, stopped.Plays.Select(p => p.Source).ToArray());
        var keep = Live();
        keep.Channel = 3;
        keep.ChanMin = 1;
        keep.ChanMax = 7;
        VmDoomSound1425Slice.StopActor(keep);
        VmDoomSound1425Slice.StopActors(keep);
        Assert.Equal(3, keep.Plays[0].Channel);
        Assert.Equal(1, keep.Plays[1].ChanMin);
        Assert.Equal(7, keep.Plays[1].ChanMax);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.Relink(new VmDoomSound1425Slice.State { FromNull = true }));
        var cut = Live();
        cut.Compat2 = VmDoomSound1425Slice.Cutoff;
        cut.PosX = 9;
        VmDoomSound1425Slice.Relink(cut);
        Assert.False(cut.Plays[0].PassedPosition);
        Assert.Equal(9f, cut.Plays[0].X);
        var link = Live();
        link.PosX = 4;
        link.PosY = 5;
        link.PosZ = 6;
        VmDoomSound1425Slice.Relink(link);
        Assert.True(link.Plays[0].PassedPosition);
        Assert.Equal(new[] { 4f, 5f, 6f }, new[] { link.Plays[0].X, link.Plays[0].Y, link.Plays[0].Z });
        var volume = Live();
        volume.Compat = VmDoomSound1425Slice.Magic;
        volume.Channel = 2;
        volume.Delta = 0.25;
        VmDoomSound1425Slice.ChangeVolume(volume);
        Assert.Equal(-1, volume.Plays[0].Channel);
        Assert.Equal(0.25, volume.Plays[0].Delta);
        var tuned = Live();
        tuned.Compat = VmDoomSound1425Slice.Magic;
        tuned.Channel = 2;
        tuned.Pitch = 1.5f;
        VmDoomSound1425Slice.ChangePitch(tuned);
        Assert.Equal(2, tuned.Plays[0].Channel);
        Assert.Equal(1.5f, tuned.Plays[0].Pitch);
        var info = Live();
        info.PlayingResult = false;
        Assert.False(VmDoomSound1425Slice.PlayingActor(info));
        Assert.True(VmDoomSound1425Slice.PlayingSector(new VmDoomSound1425Slice.State { PlayingResult = true, Id = 3 }));
        Assert.False(VmDoomSound1425Slice.PlayingPoly(info));
        Assert.Equal(VmDoomSound1425Slice.SourcePoly, info.Plays[^1].Source);
        var ask = Live();
        ask.Compat = VmDoomSound1425Slice.Magic;
        ask.Channel = 5;
        ask.PlayingResult = false;
        Assert.False(VmDoomSound1425Slice.ActorPlaying(ask));
        Assert.Equal(5, ask.Channel);
        Assert.Equal(VmDoomSound1425Slice.Auto, ask.Plays[0].Channel);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1425Slice.StopActor(new VmDoomSound1425Slice.State { HasEngine = false }));
    }
}
