using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAdvSound1431SliceTests
{
    [Fact]
    public void LinksTickerAndPlayers()
    {
        Assert.Equal(1761, VmAdvSound1431Slice.BandStart);
        Assert.Equal(2062, VmAdvSound1431Slice.BandEnd);
        var s = new VmAdvSound1431Slice.State { Reserves = 1 };
        s.Sounds.Add(new VmAdvSound1431Slice.Sfx { Name = "{ no sound }", Link = -1 });
        s.Sounds.Add(new VmAdvSound1431Slice.Sfx { Name = "weapons/pistol", Link = 2 });
        s.Sounds.Add(new VmAdvSound1431Slice.Sfx { Name = "dspistol", Link = -1 });
        s.Sounds.Add(new VmAdvSound1431Slice.Sfx { Name = "rand", Link = 2, Random = true });
        s.Sounds.Add(new VmAdvSound1431Slice.Sfx { Name = "*pain", User = VmAdvSound1431Slice.Reserve, Link = 0 });
        VmAdvSound1431Slice.SoundLinks(s);
        Assert.Equal(new[] { "weapons/pistol -> dspistol\n" }, s.Lines);
        var fighter = new VmAdvSound1431Slice.Class { Name = "Fighter" };
        fighter.Lists[0] = 0;
        s.Classes.Add(fighter);
        s.Tables.Add(new Dictionary<int, int> { [0] = 2 });
        s.Lines.Clear();
        VmAdvSound1431Slice.PlayerSounds(s);
        Assert.Equal(new[] { "\nFighter, male:\n", " *pain           dspistol\n" }, s.Lines);
        var roll = new VmAdvSound1431Slice.Ambient { Type = VmAdvSound1431Slice.Random, Min = 10, Max = 20 };
        s.Rands.Add(0);
        Assert.Equal(10, VmAdvSound1431Slice.Ticker(s, roll));
        s.Rands.Add(VmAdvSound1431Slice.RandMax);
        Assert.Equal(20, VmAdvSound1431Slice.Ticker(s, roll));
        s.Rands.Add(1073741823);
        Assert.Equal(15, VmAdvSound1431Slice.Ticker(s, roll));
        Assert.Equal(1, VmAdvSound1431Slice.Ticker(s, new VmAdvSound1431Slice.Ambient { Type = VmAdvSound1431Slice.Continuous, Min = 4, Max = 9 }));
        Assert.Equal(7, VmAdvSound1431Slice.Ticker(s, new VmAdvSound1431Slice.Ambient { Type = 2, Min = 7, Max = 99 }));
        Assert.Equal(0, VmAdvSound1431Slice.MarkSound(s, 2));
        Assert.Equal(new[] { 2 }, s.Marks);
    }

    [Fact]
    public void AmbientLifecycleAndMusic()
    {
        var s = new VmAdvSound1431Slice.State();
        var looped = new VmAdvSound1431Slice.Ambient { Type = VmAdvSound1431Slice.Continuous, Sound = 4, Volume = 1f, Atten = 0.5f };
        var timed = new VmAdvSound1431Slice.Ambient { Type = 2, Sound = 5, Volume = 1f, Min = 5, Atten = 0.5f };
        var positional = new VmAdvSound1431Slice.Ambient { Type = 4, Sound = 6, Min = 0 };
        s.Ambients[1] = looped;
        s.Ambients[2] = timed;
        s.Ambients[3] = positional;
        s.Lengths[6] = 2000;
        var self = new VmAdvSound1431Slice.Actor();
        self.Args[0] = 1;
        var who = new VmAdvSound1431Slice.Actor();
        Assert.Equal(0, VmAdvSound1431Slice.Activate(s, self, who));
        Assert.Equal(1, self.Activations);
        Assert.Same(who, self.Last);
        Assert.Equal(1, self.Special1);
        Assert.Equal(1, self.Special2);
        Assert.Equal(0, VmAdvSound1431Slice.Tick(s, self));
        Assert.Equal(1, self.Ticks);
        Assert.Equal(int.MaxValue, self.Special1);
        var one = Assert.Single(s.Plays);
        Assert.Equal("sound", one.Kind);
        Assert.Equal(VmAdvSound1431Slice.Body, one.Channel);
        Assert.Equal(VmAdvSound1431Slice.Loop, one.Flags);
        Assert.Equal(4, one.Sound);
        Assert.Equal(1f, one.Volume);
        Assert.Equal(0.5f, one.Atten);
        Assert.Equal(0, VmAdvSound1431Slice.Tick(s, self));
        Assert.Single(s.Plays);
        Assert.Equal(int.MaxValue - 1, self.Special1);
        Assert.Equal(0, VmAdvSound1431Slice.Deactivate(s, self, who));
        Assert.Equal(0, self.Special2);
        Assert.Equal("stop", s.Plays[1].Kind);
        Assert.Equal(VmAdvSound1431Slice.Body, s.Plays[1].Channel);

        var again = new VmAdvSound1431Slice.Actor { Special2 = 1, Special1 = 9 };
        again.Args[0] = 1;
        VmAdvSound1431Slice.Activate(s, again, null);
        Assert.Equal(9, again.Special1);
        Assert.Equal(1, again.Special2);

        var period = new VmAdvSound1431Slice.Actor { Special2 = 1 };
        period.Args[0] = 2;
        period.Args[1] = 50;
        period.Args[2] = 10;
        period.Args[3] = 40;
        period.Args[4] = 2;
        VmAdvSound1431Slice.Tick(s, period);
        Assert.Equal(5, period.Special1);
        var scaled = s.Plays[s.Plays.Count - 1];
        Assert.Equal("minmax", scaled.Kind);
        Assert.Equal(0, scaled.Flags);
        Assert.Equal(0.5f, scaled.Volume);
        Assert.Equal(20f, scaled.Min);
        Assert.Equal(80f, scaled.Max);
        s.Plays.Clear();
        period.Special1 = 2;
        period.Special2 = 1;
        Assert.Equal(0, VmAdvSound1431Slice.Tick(s, period));
        Assert.Equal(1, period.Special1);
        Assert.Empty(s.Plays);

        var quiet = new VmAdvSound1431Slice.Actor { Special2 = 1 };
        quiet.Args[0] = 2;
        quiet.Args[1] = 200;
        VmAdvSound1431Slice.Tick(s, quiet);
        var loud = s.Plays[s.Plays.Count - 1];
        Assert.Equal(1f, loud.Volume);
        Assert.Equal("sound", loud.Kind);
        var negative = new VmAdvSound1431Slice.Actor { Special2 = 1 };
        negative.Args[0] = 2;
        negative.Args[1] = -100;
        negative.Args[2] = 30;
        negative.Args[3] = 10;
        VmAdvSound1431Slice.Tick(s, negative);
        var dropped = s.Plays[s.Plays.Count - 1];
        Assert.Equal(0f, dropped.Volume);
        Assert.Equal("sound", dropped.Kind);

        var born = new VmAdvSound1431Slice.Actor();
        born.Args[0] = 3;
        VmAdvSound1431Slice.Activate(s, born, null);
        Assert.Equal(70, positional.Min);
        Assert.Equal(0, born.Special1);
        Assert.Equal(1, born.Special2);
        Assert.False(born.Destroyed);
        var dead = new VmAdvSound1431Slice.Actor();
        dead.Args[0] = 9;
        VmAdvSound1431Slice.Activate(s, dead, null);
        Assert.True(dead.Destroyed);
        positional.Sound = 0;
        positional.Min = 0;
        var invalid = new VmAdvSound1431Slice.Actor();
        invalid.Args[0] = 3;
        VmAdvSound1431Slice.Activate(s, invalid, null);
        Assert.True(invalid.Destroyed);
        Assert.Equal(0, invalid.Special2);

        var silent = new VmAdvSound1431Slice.Ambient { Type = 2, Sound = 0, Min = 1 };
        s.Ambients[8] = silent;
        var gone = new VmAdvSound1431Slice.Actor { Special2 = 1 };
        gone.Args[0] = 8;
        VmAdvSound1431Slice.Tick(s, gone);
        Assert.True(gone.Destroyed);
        Assert.Equal(0, VmAdvSound1431Slice.MarkAmbient(s, self));
        Assert.Equal(new[] { 4 }, s.Marks);
        Assert.Equal(0, VmAdvSound1431Slice.MarkAmbient(s, dead));
        Assert.Single(s.Marks);

        var map = new VmAdvSound1431Slice.MapInfo { Name = "MAP01" };
        s.Maps.Add(map);
        var lump = new VmAdvSound1431Slice.Lump { Path = "maps/mus.lmp" };
        lump.Tokens.Add(Tok("map01", 1));
        lump.Tokens.Add(Tok("1", 2));
        lump.Tokens.Add(Tok("D_INTER", 2));
        lump.Tokens.Add(Tok("0", 3));
        lump.Tokens.Add(Tok("D_SKIP", 3));
        lump.Tokens.Add(Tok("08", 4));
        lump.Tokens.Add(Tok("2", 5));
        lump.Tokens.Add(Tok("D_RUN", 5));
        lump.Tokens.Add(Tok("0x10", 6));
        lump.Tokens.Add(Tok("D_HEX", 6));
        lump.Tokens.Add(Tok("MAXINT", 7));
        lump.Tokens.Add(Tok("D_MAX", 7));
        lump.Tokens.Add(Tok("-3", 8));
        lump.Tokens.Add(Tok("D_NEG", 8));
        s.Lumps.Add(lump);
        var more = new VmAdvSound1431Slice.Lump { Path = "maps/second.lmp" };
        more.Tokens.Add(Tok("MAP01", 1));
        more.Tokens.Add(Tok("010", 2));
        more.Tokens.Add(Tok("D_OCT", 2));
        s.Lumps.Add(more);
        VmAdvSound1431Slice.Parse(s);
        Assert.Empty(s.Lines);
        Assert.Equal("D_INTER", map.Music[1]);
        Assert.False(map.Music.ContainsKey(0));
        Assert.Equal("D_OCT", map.Music[8]);
        Assert.Equal("Unnamed", s.Default.Name);
        Assert.Equal("SKY1", s.Default.Sky1);
        Assert.Equal("SKY1", s.Default.Sky2);
        Assert.Equal("SKYMIST1", s.Default.Mist);
        Assert.Equal("D_RUN", s.Default.Music[2]);
        Assert.Equal("D_HEX", s.Default.Music[16]);
        Assert.Equal("D_MAX", s.Default.Music[int.MaxValue]);
        Assert.False(s.Default.Music.ContainsKey(-3));
        var cut = new VmAdvSound1431Slice.Lump { Path = "maps/cut.lmp" };
        cut.Tokens.Add(Tok("MAP01", 1));
        cut.Tokens.Add(Tok("4", 2));
        s.Lumps.Clear();
        s.Lumps.Add(cut);
        var error = Assert.Throws<InvalidOperationException>(() => VmAdvSound1431Slice.Parse(s));
        Assert.Equal("Script error, \"maps/cut.lmp\" line 2:\nMissing string (unexpected end of file).\n", error.Message);
    }

    static VmAdvSound1431Slice.Token Tok(string text, int line) => new() { Text = text, Line = line };
}
