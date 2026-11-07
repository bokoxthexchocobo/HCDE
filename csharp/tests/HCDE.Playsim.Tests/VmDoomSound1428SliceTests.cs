using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1428SliceTests
{
    [Fact]
    public void ReadAndReplacement()
    {
        Assert.Equal(269, VmDoomSound1428Slice.BandEnd - VmDoomSound1428Slice.BandStart + 1);
        Assert.Equal(1254, VmDoomSound1428Slice.LimitsEnd);
        Assert.Equal("RandSound", VmDoomSound1428Slice.RngName);
        var read = new VmDoomSound1428Slice.State { Length = 5, Bytes = new byte[] { 1, 2, 3, 4, 5 }, ReadCount = 3 };
        Assert.Equal(new byte[] { 1, 2, 3 }, VmDoomSound1428Slice.ReadSound(read, 7));
        Assert.Equal(7, read.Opened);
        var full = new VmDoomSound1428Slice.State { Length = 2, Bytes = new byte[] { 9, 8 } };
        Assert.Equal(new byte[] { 9, 8 }, VmDoomSound1428Slice.ReadSound(full, 1));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.ReadSound(new VmDoomSound1428Slice.State { ReaderNull = true, Length = 1 }, 0));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.ReadSound(new VmDoomSound1428Slice.State { Length = 2, Bytes = null }, 0));
        var plain = new VmDoomSound1428Slice.State { Sounds = { new VmDoomSound1428Slice.Sfx() } };
        Assert.Equal(0, VmDoomSound1428Slice.PickReplacement(plain, 0));
        Assert.False(plain.Rolled);
        var once = new VmDoomSound1428Slice.State
        {
            Sounds = { new VmDoomSound1428Slice.Sfx { Random = true, Link = 0 }, new VmDoomSound1428Slice.Sfx() },
            Lists = { new VmDoomSound1428Slice.RollList { Choices = new[] { 0, 1 } } },
            Rolls = { 1 },
        };
        Assert.Equal(1, VmDoomSound1428Slice.PickReplacement(once, 0));
        Assert.Equal(new[] { 2 }, once.RollSizes);
        Assert.True(once.Rolled);
        var chain = new VmDoomSound1428Slice.State
        {
            Sounds =
            {
                new VmDoomSound1428Slice.Sfx { Random = true, Link = 0 },
                new VmDoomSound1428Slice.Sfx(),
                new VmDoomSound1428Slice.Sfx(),
                new VmDoomSound1428Slice.Sfx { Random = true, Link = 1 },
            },
            Lists =
            {
                new VmDoomSound1428Slice.RollList { Choices = new[] { 3 } },
                new VmDoomSound1428Slice.RollList { Choices = new[] { 1, 2 } },
            },
            Rolls = { 0, 1 },
        };
        Assert.Equal(2, VmDoomSound1428Slice.PickReplacement(chain, 0));
        Assert.Equal(new[] { 1, 2 }, chain.RollSizes);
        var empty = new VmDoomSound1428Slice.State
        {
            Sounds = { new VmDoomSound1428Slice.Sfx { Random = true, Link = 0 } },
            Lists = { new VmDoomSound1428Slice.RollList() },
            Rolls = { 0 },
        };
        Assert.Throws<IndexOutOfRangeException>(() => VmDoomSound1428Slice.PickReplacement(empty, 0));
        Assert.Equal(new[] { 0 }, empty.RollSizes);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.PickReplacement(new VmDoomSound1428Slice.State(), 0));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.PickReplacement(new VmDoomSound1428Slice.State
        {
            Sounds = { new VmDoomSound1428Slice.Sfx { Random = true, Link = 0 } },
            Lists = { new VmDoomSound1428Slice.RollList { Choices = new[] { 0 } } },
        }, 0));
    }

    [Fact]
    public void DebugListAndCommands()
    {
        var quiet = new VmDoomSound1428Slice.State();
        VmDoomSound1428Slice.NoiseDebug(quiet);
        Assert.Equal(12, quiet.Draws.Count);
        Assert.False(quiet.Heard);
        Assert.Equal("*** SOUND DEBUG INFO ***", quiet.Draws[0].Text);
        Assert.Equal(32, quiet.Draws[0].Y);
        Assert.Equal(VmDoomSound1428Slice.Yellow, quiet.Draws[0].Color);
        Assert.Contains(quiet.Draws, d => d.Text == "vol" && d.X == 220 && d.Y == 40 && d.Color == VmDoomSound1428Slice.Gold);
        var blind = new VmDoomSound1428Slice.State { ChannelsNull = false, CameraNull = true, Channels = { new VmDoomSound1428Slice.Chan() } };
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.NoiseDebug(blind));
        Assert.Equal(12, blind.Draws.Count);
        Assert.False(blind.Heard);
        var row = new VmDoomSound1428Slice.State
        {
            ChannelsNull = false,
            ListenX = 1f,
            Channels =
            {
                new VmDoomSound1428Slice.Chan
                {
                    ShortName = "DSPISTOL",
                    Flags = VmDoomSound1428Slice.Is3D | VmDoomSound1428Slice.Loop,
                    OriginX = 4f,
                    OriginY = 0f,
                    OriginZ = 4f,
                    Volume = 1.25f,
                    Aud = 0.5f,
                    Position = 9,
                    Ent = 2,
                    Priority = 3,
                },
            },
        };
        VmDoomSound1428Slice.NoiseDebug(row);
        Assert.True(row.Heard);
        Assert.Equal(1, row.Calcs);
        var body = row.Draws.Where(d => d.Y == 48).ToList();
        Assert.Equal(11, body.Count);
        Assert.Equal(VmDoomSound1428Slice.Brown, body[0].Color);
        Assert.Equal("DSPISTOL", body[0].Text);
        Assert.Equal("4", body[1].Text);
        Assert.Equal(70, body[1].X);
        Assert.Equal("4", body[2].Text);
        Assert.Equal(120, body[2].X);
        Assert.Equal("0", body[3].Text);
        Assert.Equal(170, body[3].X);
        Assert.Equal("5", body[4].Text);
        Assert.Equal(260, body[4].X);
        Assert.Equal("1.2", body[5].Text);
        Assert.Equal("2", body[6].Text);
        Assert.Equal("3", body[7].Text);
        Assert.Equal(VmDoomSound1428Slice.Green + "3" + VmDoomSound1428Slice.Black + "Z" + VmDoomSound1428Slice.Black + "U" + VmDoomSound1428Slice.Black + "M" + VmDoomSound1428Slice.Black + "N" + VmDoomSound1428Slice.Black + "A" + VmDoomSound1428Slice.Green + "L" + VmDoomSound1428Slice.Black + "E" + VmDoomSound1428Slice.Black + "V", body[8].Text);
        Assert.Equal("0.5000", body[9].Text);
        Assert.Equal("9", body[10].Text);
        var flat = new VmDoomSound1428Slice.State
        {
            ChannelsNull = false,
            Channels = { new VmDoomSound1428Slice.Chan { ShortName = "DSFLAT", Flags = 0, DistanceScale = -1f, Volume = 10f } },
        };
        VmDoomSound1428Slice.NoiseDebug(flat);
        var flatRow = flat.Draws.Where(d => d.Y == 48).ToList();
        Assert.Equal(VmDoomSound1428Slice.Grey, flatRow[0].Color);
        Assert.Equal(new[] { "---", "---", "---", "---" }, flatRow.Skip(1).Take(4).Select(d => d.Text));
        Assert.Equal("10", flatRow[5].Text);
        var far = new VmDoomSound1428Slice.State
        {
            ChannelsNull = false,
            Channels = { new VmDoomSound1428Slice.Chan { ShortName = "DSFAR", Flags = VmDoomSound1428Slice.Is3D, DistanceScale = 0f, OriginX = 8f } },
        };
        VmDoomSound1428Slice.NoiseDebug(far);
        Assert.Equal("---", far.Draws.Single(d => d.Y == 48 && d.X == 260).Text);
        Assert.Equal("8", far.Draws.Single(d => d.Y == 48 && d.X == 70).Text);
        var order = new VmDoomSound1428Slice.State
        {
            ChannelsNull = false,
            Channels =
            {
                new VmDoomSound1428Slice.Chan { ShortName = "new", Next = 1, Prev = -2 },
                new VmDoomSound1428Slice.Chan { ShortName = "old", Next = -1, Prev = 0 },
            },
        };
        VmDoomSound1428Slice.NoiseDebug(order);
        var names = order.Draws.Where(d => d.X == 0 && d.Y >= 48).Select(d => d.Text).ToList();
        Assert.Equal(new[] { "old", "new" }, names);
        Assert.Equal(48, order.Draws.Single(d => d.Text == "old").Y);
        Assert.Equal(56, order.Draws.Single(d => d.Text == "new").Y);
        var clip = new VmDoomSound1428Slice.State
        {
            ChannelsNull = false,
            Height = 64,
            Channels = { new VmDoomSound1428Slice.Chan { ShortName = "skip" } },
        };
        VmDoomSound1428Slice.NoiseDebug(clip);
        Assert.True(clip.Heard);
        Assert.Equal(0, clip.Calcs);
        Assert.DoesNotContain(clip.Draws, d => d.Text == "skip");
        var stat = new VmDoomSound1428Slice.State { ChannelsNull = false, Channels = { new VmDoomSound1428Slice.Chan { ShortName = "stat" } } };
        Assert.Equal("", VmDoomSound1428Slice.SoundDebug(stat));
        Assert.Equal(1, stat.Calcs);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.SoundDebug(new VmDoomSound1428Slice.State { EngineNull = true }));
        var listed = new VmDoomSound1428Slice.State
        {
            Sounds =
            {
                new VmDoomSound1428Slice.Sfx { Random = true, Link = 0, Name = "rnd", Pitch = 3 },
                new VmDoomSound1428Slice.Sfx { Name = "a", Lump = 4, ShortName = "DSA" },
                new VmDoomSound1428Slice.Sfx { Name = "b", Pitch = 1 },
                new VmDoomSound1428Slice.Sfx { Name = "pl", User = VmDoomSound1428Slice.PlayerReserve, Link = 9, Lump = 5, Pitch = 2 },
                new VmDoomSound1428Slice.Sfx { Name = "alias", Link = 1 },
            },
            Lists = { new VmDoomSound1428Slice.RollList { Choices = new[] { 1, 2 } } },
        };
        VmDoomSound1428Slice.PrintSoundList(listed);
        Assert.Equal("  0. rnd -> #0 { a  b }\n    PitchMask = 3\n  1. a (DSA)\n    PitchMask = 0\n  2. b **not present**\n    PitchMask = 1\n  3. pl <<player sound 9>>\n    PitchMask = 2\n  4. alias -> a\n    PitchMask = 0\n", string.Concat(listed.Lines));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.PrintSoundList(new VmDoomSound1428Slice.State { Sounds = { null } }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.PrintSoundList(new VmDoomSound1428Slice.State { Sounds = { new VmDoomSound1428Slice.Sfx { Lump = 1, ShortName = null } } }));
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.SoundList(new VmDoomSound1428Slice.State { EngineNull = true }));
        var none = new VmDoomSound1428Slice.State { Args = new[] { "playsound" }, Ids = { ["dspistol"] = 7 } };
        VmDoomSound1428Slice.PlaySound(none);
        Assert.Equal(0, none.Lookups);
        var missing = new VmDoomSound1428Slice.State { Args = new[] { "playsound", "nope" } };
        VmDoomSound1428Slice.PlaySound(missing);
        Assert.Equal("'nope' is not a sound\n", Assert.Single(missing.Lines));
        Assert.Empty(missing.Plays);
        var played = new VmDoomSound1428Slice.State { Args = new[] { "playsound", "dspistol" }, Ids = { ["dspistol"] = 7 } };
        VmDoomSound1428Slice.PlaySound(played);
        var start = Assert.Single(played.Plays);
        Assert.Equal("ui", start.Kind);
        Assert.Equal(VmDoomSound1428Slice.Auto, start.Channel);
        Assert.Equal(VmDoomSound1428Slice.Ui, start.Flags);
        Assert.Equal(7, start.Id);
        Assert.Equal(1f, start.Volume);
        Assert.Equal(0f, start.Atten);
        var idle = new VmDoomSound1428Slice.State { PlayerNull = true, Args = new[] { "loopsound", "dspistol" }, Ids = { ["dspistol"] = 7 } };
        VmDoomSound1428Slice.LoopSound(idle);
        Assert.Equal(0, idle.Lookups);
        var net = new VmDoomSound1428Slice.State { PlayerNull = false, Net = true, Args = new[] { "loopsound", "dspistol" }, Ids = { ["dspistol"] = 7 } };
        VmDoomSound1428Slice.LoopSound(net);
        Assert.Equal(0, net.Lookups);
        var bad = new VmDoomSound1428Slice.State { PlayerNull = false, Args = new[] { "loopsound", "nope" } };
        VmDoomSound1428Slice.LoopSound(bad);
        Assert.Equal("'nope' is not a sound\n", Assert.Single(bad.Lines));
        Assert.Equal(0, bad.Spawns);
        Assert.Throws<NullReferenceException>(() => VmDoomSound1428Slice.LoopSound(new VmDoomSound1428Slice.State
        {
            PlayerNull = false,
            LevelNull = true,
            Args = new[] { "loopsound", "dspistol" },
            Ids = { ["dspistol"] = 4 },
        }));
        var spawned = new VmDoomSound1428Slice.State { PlayerNull = false, SpawnNull = true, Args = new[] { "loopsound", "dspistol" }, Ids = { ["dspistol"] = 4 } };
        VmDoomSound1428Slice.LoopSound(spawned);
        Assert.Equal(1, spawned.Spawns);
        Assert.Empty(spawned.Plays);
        var looped = new VmDoomSound1428Slice.State
        {
            PlayerNull = false,
            PlayerX = 1,
            PlayerY = 2,
            PlayerZ = 10,
            Args = new[] { "loopsound", "dspistol" },
            Ids = { ["dspistol"] = 4 },
        };
        VmDoomSound1428Slice.LoopSound(looped);
        var icon = Assert.Single(looped.Plays);
        Assert.Equal("body", icon.Kind);
        Assert.Equal(VmDoomSound1428Slice.Body, icon.Channel);
        Assert.Equal(VmDoomSound1428Slice.Loop, icon.Flags);
        Assert.Equal(4, icon.Id);
        Assert.Equal(1f, icon.Volume);
        Assert.Equal(VmDoomSound1428Slice.Idle, icon.Atten);
        Assert.Equal("SpeakerIcon", icon.ClassName);
        Assert.Equal(1d, icon.X);
        Assert.Equal(2d, icon.Y);
        Assert.Equal(42d, icon.Z);
        Assert.Equal(VmDoomSound1428Slice.Replace, icon.Replace);
        var reset = new VmDoomSound1428Slice.State();
        VmDoomSound1428Slice.ResetSounds(reset);
        VmDoomSound1428Slice.ResetSounds(reset);
        Assert.Equal(2, reset.Resets);
        Assert.Equal("0", VmDoomSound1428Slice.VolumeText(0f));
        Assert.Equal("1", VmDoomSound1428Slice.VolumeText(1f));
        Assert.Equal("1.2", VmDoomSound1428Slice.VolumeText(1.2f));
        Assert.Equal("1.2", VmDoomSound1428Slice.VolumeText(1.25f));
        Assert.Equal("1.5", VmDoomSound1428Slice.VolumeText(1.5f));
        Assert.Equal("10", VmDoomSound1428Slice.VolumeText(10f));
        Assert.Equal("1e+02", VmDoomSound1428Slice.VolumeText(100f));
        Assert.Equal("0.5", VmDoomSound1428Slice.VolumeText(0.5f));
        Assert.Equal("0.0012", VmDoomSound1428Slice.VolumeText(0.00123f));
        Assert.Equal("1.2e-05", VmDoomSound1428Slice.VolumeText(0.0000123f));
        Assert.Equal("-0.25", VmDoomSound1428Slice.VolumeText(-0.25f));
        Assert.Equal("2", VmDoomSound1428Slice.Fixed0(1.5f));
        Assert.Equal("2", VmDoomSound1428Slice.Fixed0(2.5f));
        Assert.Equal("0", VmDoomSound1428Slice.Fixed0(0.5f));
        Assert.Equal("-2", VmDoomSound1428Slice.Fixed0(-1.5f));
        Assert.Equal("1", VmDoomSound1428Slice.Fixed0(1.4f));
        Assert.Equal("4", VmDoomSound1428Slice.Fixed0(3.5f));
        Assert.Equal("4", VmDoomSound1428Slice.Fixed0(4.5f));
    }
}
