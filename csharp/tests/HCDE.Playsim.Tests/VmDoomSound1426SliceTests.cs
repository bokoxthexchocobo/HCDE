using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1426SliceTests
{
    [Fact]
    public void ListenerAndUpdate()
    {
        Assert.Equal(163, VmDoomSound1426Slice.BandEnd - VmDoomSound1426Slice.BandStart + 1);
        Assert.Equal(851, VmDoomSound1426Slice.PlayingEnd);
        Assert.Equal(1016, VmDoomSound1426Slice.SectorOrigin);
        var actor = new object();
        var zone = new object();
        var heard = new VmDoomSound1426Slice.State
        {
            Actor = actor,
            Yaw = 2,
            X = 3,
            Y = 4,
            Z = 5,
            Water = 3,
            Zones = new List<object?> { null, zone },
            Zone = 1,
        };
        VmDoomSound1426Slice.Hear(heard);
        Assert.Equal(2f, heard.Heard!.Angle);
        Assert.Equal(3f, heard.Heard.X);
        Assert.Equal(4f, heard.Heard.Y);
        Assert.Equal(5f, heard.Heard.Z);
        Assert.True(heard.Heard.Underwater);
        Assert.True(heard.Heard.VelocityCleared);
        Assert.True(heard.Heard.Valid);
        Assert.Same(zone, heard.Heard.Environment);
        Assert.Same(actor, heard.Heard.Actor);
        var dry = new VmDoomSound1426Slice.State { Actor = actor, Water = 2, Zones = new List<object?> { zone } };
        VmDoomSound1426Slice.Hear(dry);
        Assert.False(dry.Heard!.Underwater);
        var none = new VmDoomSound1426Slice.State();
        VmDoomSound1426Slice.Hear(none);
        Assert.False(none.Heard!.Valid);
        Assert.Equal(0f, none.Heard.Angle);
        Assert.Null(none.Heard.Environment);
        Assert.Null(none.Heard.Actor);
        Assert.True(none.Heard.VelocityCleared);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Hear(new VmDoomSound1426Slice.State { Actor = actor, SectorNull = true }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Hear(new VmDoomSound1426Slice.State { Actor = actor, Zones = null }));
        Assert.Throws<IndexOutOfRangeException>(() => VmDoomSound1426Slice.Hear(new VmDoomSound1426Slice.State { Actor = actor, Zones = new(), Zone = 0 }));
        Assert.Throws<IndexOutOfRangeException>(() => VmDoomSound1426Slice.Hear(new VmDoomSound1426Slice.State { Actor = actor, Zones = new() { zone }, Zone = -1 }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Hear(new VmDoomSound1426Slice.State { EngineNull = true }));
        var update = new VmDoomSound1426Slice.State { Tics = 3, Levels = new List<int> { 1, 2 }, Timer = 40 };
        VmDoomSound1426Slice.Update(update);
        Assert.Equal(6, update.Sequences);
        Assert.Equal(40, update.TimerSeen);
        Assert.NotNull(update.Heard);
        var quiet = new VmDoomSound1426Slice.State { Tics = 0, Levels = null, Timer = 7 };
        VmDoomSound1426Slice.Update(quiet);
        Assert.Equal(0, quiet.Sequences);
        Assert.Equal(7, quiet.TimerSeen);
        var negative = new VmDoomSound1426Slice.State { Tics = -2, Levels = null, Timer = 1 };
        VmDoomSound1426Slice.Update(negative);
        Assert.Equal(0, negative.Sequences);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Update(new VmDoomSound1426Slice.State { Tics = 1, Levels = null }));
        var lost = new VmDoomSound1426Slice.State { Tics = 0, LevelNull = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Update(lost));
        Assert.Equal(-1, lost.TimerSeen);
        Assert.NotNull(lost.Heard);
    }

    [Fact]
    public void SerializeAndSave()
    {
        var current = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceNone, Pitch = 128, Start = 9 };
        var fresh = new VmDoomSound1426Slice.State();
        VmDoomSound1426Slice.Write(fresh, current);
        Assert.Equal(128f, current.Pitch);
        Assert.Equal(1, fresh.Closed);
        Assert.Equal(new[] { "sourcetype", "soundid", "orgid", "volume", "distancescale", "pitch", "chanflags", "entchannel", "priority", "nearlimit", "starttime", "rolloftype", "rolloffmin", "rolloffmax", "limitrange" }, fresh.Keys);
        var old = new VmDoomSound1426Slice.State { SaveVersion = 4559 };
        var aged = new VmDoomSound1426Slice.Chan { Pitch = 256, SourceType = VmDoomSound1426Slice.SourceActor, Source = "old" };
        old.HasReplace = true;
        old.Replace = "new";
        VmDoomSound1426Slice.Write(old, aged);
        Assert.Equal(2f, aged.Pitch);
        Assert.Equal("new", aged.Source);
        Assert.Contains("actor", old.Keys);
        var sector = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceSector };
        var poly = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourcePoly };
        var sectorState = new VmDoomSound1426Slice.State();
        var polyState = new VmDoomSound1426Slice.State();
        VmDoomSound1426Slice.Write(sectorState, sector);
        VmDoomSound1426Slice.Write(polyState, poly);
        Assert.Contains("sector", sectorState.Keys);
        Assert.Contains("poly", polyState.Keys);
        var point = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceUnattached, Point = new[] { 1f, 2f, 3f } };
        var pointed = new VmDoomSound1426Slice.State();
        VmDoomSound1426Slice.Write(pointed, point);
        Assert.Equal(new[] { 1f, 2f, 3f }, pointed.Point);
        var closed = new VmDoomSound1426Slice.State { OpenObject = false, SaveVersion = 1 };
        var kept = new VmDoomSound1426Slice.Chan { Pitch = 64 };
        VmDoomSound1426Slice.Write(closed, kept);
        Assert.Equal(64f, kept.Pitch);
        Assert.Empty(closed.Keys);
        var bad = new VmDoomSound1426Slice.Chan { SourceType = 9 };
        var fatal = Assert.Throws<VmDoomSound1426Slice.Fatal>(() => VmDoomSound1426Slice.Write(new VmDoomSound1426Slice.State(), bad));
        Assert.Equal("Unknown sound source type 9\n", fatal.Message);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Write(new VmDoomSound1426Slice.State(), new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceUnattached }));
        var loaded = new VmDoomSound1426Slice.State
        {
            Loading = true,
            SaveVersion = 4559,
            Incoming = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceNone, Pitch = 128, SoundId = 4, Start = 15 },
        };
        var target = new VmDoomSound1426Slice.Chan { Pitch = 1, SoundId = 0 };
        VmDoomSound1426Slice.Write(loaded, target);
        Assert.Equal(1f, target.Pitch);
        Assert.Equal(4, target.SoundId);
        Assert.Equal(15ul, target.Start);
        var first = new VmDoomSound1426Slice.Chan { Start = 10, SourceType = VmDoomSound1426Slice.SourceNone };
        var second = new VmDoomSound1426Slice.Chan { Start = 20, SourceType = VmDoomSound1426Slice.SourceNone };
        var save = new VmDoomSound1426Slice.State
        {
            Channels = new List<VmDoomSound1426Slice.Chan> { first, second },
            Positions = new List<ulong> { 1, 2 },
        };
        VmDoomSound1426Slice.Save(save);
        Assert.Equal(new[] { true, false }, save.Sync);
        Assert.Equal(1, save.DeviceUpdates);
        Assert.True(save.ArrayOpened);
        Assert.True(save.ArrayClosed);
        Assert.Equal(-1, save.Restart);
        Assert.Equal(10ul, first.Start);
        Assert.Equal(20ul, second.Start);
        Assert.Equal(new[] { 2ul, 1ul }, save.Starts);
        var empty = new VmDoomSound1426Slice.State();
        VmDoomSound1426Slice.Save(empty);
        Assert.False(empty.ArrayOpened);
        Assert.Equal(new[] { true, false }, empty.Sync);
        var skipped = new VmDoomSound1426Slice.State
        {
            Channels = new List<VmDoomSound1426Slice.Chan> { first },
            BeginArray = false,
        };
        VmDoomSound1426Slice.Save(skipped);
        Assert.False(skipped.ArrayOpened);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Save(new VmDoomSound1426Slice.State { DeviceNull = true }));
        var broken = new VmDoomSound1426Slice.State { Channels = new List<VmDoomSound1426Slice.Chan> { first }, EngineNull = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Save(broken));
        Assert.Equal(new[] { true }, broken.Sync);
        Assert.Equal(0, broken.DeviceUpdates);
        var read = new VmDoomSound1426Slice.State
        {
            Writing = false,
            ArrayCount = 2,
            Timer = 10,
            Loading = true,
            Incoming = new VmDoomSound1426Slice.Chan { SourceType = VmDoomSound1426Slice.SourceNone, Flags = 1, Pitch = 128 },
            SaveVersion = 4559,
        };
        VmDoomSound1426Slice.Save(read);
        Assert.Equal(1, read.Stopped);
        Assert.Equal(12, read.Restart);
        Assert.Equal(2, read.Restored.Count);
        Assert.All(read.Restored, chan => Assert.Equal(VmDoomSound1426Slice.Evicted | VmDoomSound1426Slice.AbsTime | 1, chan.Flags));
        Assert.All(read.Restored, chan => Assert.Equal(1f, chan.Pitch));
        Assert.Equal(new[] { true, false }, read.Sync);
        var unread = new VmDoomSound1426Slice.State { Writing = false, BeginArray = false, Timer = 4 };
        VmDoomSound1426Slice.Save(unread);
        Assert.Equal(1, unread.Stopped);
        Assert.False(unread.ArrayOpened);
        Assert.Equal(6, unread.Restart);
        var lost = new VmDoomSound1426Slice.State { Writing = false, ArrayCount = 1, LevelNull = true };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Save(lost));
        Assert.Single(lost.Restored);
        Assert.Equal(-1, lost.Restart);
        Assert.Equal(new[] { true }, lost.Sync);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1426Slice.Save(new VmDoomSound1426Slice.State { Writing = false, FailChannel = true, ArrayCount = 1 }));
    }
}
