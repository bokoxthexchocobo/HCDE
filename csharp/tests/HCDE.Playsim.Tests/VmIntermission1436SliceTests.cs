using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmIntermission1436SliceTests
{
    static VmIntermission1436Slice.Anim State(int index, int tics, VmIntermission1436Slice.Anim? next = null, int sprite = 0, int frame = 0, int range = 0)
    {
        return new VmIntermission1436Slice.Anim { Index = index, Tics = tics, Next = next, Sprite = sprite, Frame = frame, TicRange = range };
    }

    [Fact]
    public void TextKeysAndTypewriter()
    {
        Assert.Equal(440, VmIntermission1436Slice.BandStart);
        Assert.Equal(770, VmIntermission1436Slice.BandEnd);
        var world = new VmIntermission1436Slice.World();
        var page = new VmIntermission1436Slice.Page { Delay = 3, Len = 4, Speed = 2, Ticker = 4, Font = new VmIntermission1436Slice.Face() };
        Assert.Equal(1, VmIntermission1436Slice.TextKey(world, page, VmIntermission1436Slice.KeyDown));
        Assert.Equal(11, page.Ticker);
        Assert.Equal(-1, VmIntermission1436Slice.TextKey(world, page, VmIntermission1436Slice.KeyDown));
        Assert.Equal(0, VmIntermission1436Slice.TextKey(world, page, 5));

        var typed = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Height = 8, Width = 8, Kerning = 1 },
            Text = new byte[] { (byte)'A', (byte)'\n', (byte)'B', 0 },
            Speed = 1,
            Ticker = 3,
            X = 10,
            Y = 5,
            DefaultPlace = true,
        };
        VmIntermission1436Slice.DrawText(world, typed);
        Assert.Equal(1, world.SuperDraws);
        Assert.Equal(new[] { "11,10,5,65,320,200", "11,10,16,66,320,200" }, world.Glyphs);

        var scaled = new VmIntermission1436Slice.World { Width = 800, Height = 500 };
        var wide = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Height = 8, Width = 8 },
            Text = new byte[] { (byte)'A', 0 },
            Speed = 1,
            Ticker = 1,
            DefaultPlace = true,
        };
        VmIntermission1436Slice.DrawText(scaled, wide);
        Assert.Equal("11,40,25,65,400,250", Assert.Single(scaled.Glyphs));

        var fixedPage = new VmIntermission1436Slice.World { Width = 320, Height = 40 };
        var anchored = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Height = 10, Width = 4 },
            Text = new byte[] { (byte)'A', (byte)'\n', (byte)'B', 0 },
            Speed = 1,
            Ticker = 3,
            X = 0,
            Y = 100,
        };
        VmIntermission1436Slice.DrawText(fixedPage, anchored);
        Assert.Equal("11,0,20,65,320,40", fixedPage.Glyphs[0]);
        Assert.Equal("11,0,30,66,320,40", fixedPage.Glyphs[1]);
        anchored.DefaultPlace = true;
        anchored.Text = new byte[] { (byte)'A', (byte)'\n', (byte)'B', 0 };
        fixedPage.Glyphs.Clear();
        VmIntermission1436Slice.DrawText(fixedPage, anchored);
        Assert.Equal("11,0,7,65,320,40", fixedPage.Glyphs[0]);

        var raven = new VmIntermission1436Slice.World { Game = 2, Width = 320, Height = 200 };
        var hex = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Height = 8, Width = 4 },
            Text = new byte[] { (byte)'A', (byte)'\n', (byte)'B', 0 },
            Speed = 1,
            Ticker = 3,
            X = 0,
            Y = 100,
        };
        VmIntermission1436Slice.DrawText(raven, hex);
        Assert.Equal("11,0,107,66,320,200", raven.Glyphs[1]);
    }

    [Fact]
    public void TextDelayAndSpeed()
    {
        var overflow = new VmIntermission1436Slice.World { Width = 20, Height = 200 };
        var fat = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Width = 30, Height = 8 },
            Text = new byte[] { (byte)'A', (byte)'B', 0 },
            Speed = 1,
            Ticker = 2,
            X = 160,
            Y = 100,
            DefaultPlace = true,
        };
        VmIntermission1436Slice.DrawText(overflow, fat);
        Assert.Empty(overflow.Glyphs);

        var euro = new VmIntermission1436Slice.World();
        var marks = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face { Width = 4, Height = 8 },
            Text = new byte[] { 0x80, 0xC3, 0xA9, 0 },
            Speed = 1,
            Ticker = 2,
            X = 160,
            Y = 100,
            DefaultPlace = true,
        };
        VmIntermission1436Slice.DrawText(euro, marks);
        Assert.Equal(0x20AC, int.Parse(euro.Glyphs[0].Split(',')[3]));
        Assert.Equal(0xE9, int.Parse(euro.Glyphs[1].Split(',')[3]));

        var early = new VmIntermission1436Slice.Page
        {
            Font = new VmIntermission1436Slice.Face(),
            Delay = 5,
            Ticker = 4,
            Speed = 1,
            Text = new byte[] { (byte)'A', 0 },
        };
        var quiet = new VmIntermission1436Slice.World();
        VmIntermission1436Slice.DrawText(quiet, early);
        Assert.Empty(quiet.Glyphs);
        early.Ticker = 5;
        early.Speed = 0;
        Assert.Throws<DivideByZeroException>(() => VmIntermission1436Slice.DrawText(quiet, early));
    }

    [Fact]
    public void CastAdvanceSkinAndAttack()
    {
        var see = State(0, 5, sprite: 1, frame: 0);
        var step = State(1, 4, sprite: 2, frame: 1);
        see.Next = step;
        var missile = State(10, 8, sprite: 3, frame: 0);
        var melee = State(20, 7, sprite: 4, frame: 2);
        var spawn = State(0, 1, sprite: 1);
        var death = State(30, 6);
        death.Next = death;
        var kind = new VmIntermission1436Slice.Kind();
        kind.Labels.Kids["Death"] = new VmIntermission1436Slice.Node
        {
            State = death,
            Kids = { ["Cast"] = new VmIntermission1436Slice.Node { State = State(31, 9) } },
        };
        var world = new VmIntermission1436Slice.World();
        world.Kinds["Zombie"] = kind;
        world.Sounds["dssee"] = 4;
        world.Sounds["dswpn"] = 6;
        var body = new VmIntermission1436Slice.Body { See = see, Missile = missile, Melee = melee, Spawn = spawn, SeeSound = 4, DeathSound = 8 };
        var page = new VmIntermission1436Slice.Page { Body = body, Font = new VmIntermission1436Slice.Face { Measure = 6 } };
        VmIntermission1436Slice.CastInit(world, page, 50, 2, "Zombie", "Zombie", new List<VmIntermission1436Slice.Snd>
        {
            new() { Sequence = 0, Index = 0, Name = "dswpn" },
            new() { Sequence = 1, Index = 0, Name = "missing" },
        }, true);
        Assert.Equal(0, page.Ticker);
        Assert.Equal(50, page.Duration);
        Assert.Same(see, page.State);
        Assert.Equal(0, page.Translation);
        Assert.Equal(new[] { "2:32:2:1:0", "2:32:4:1:0" }, world.Audio);
        Assert.Equal(6, page.Sounds[0].Id);
        Assert.Equal(0, page.Sounds[1].Id);

        var missing = new VmIntermission1436Slice.Page { Ticker = 9, Name = "keep" };
        VmIntermission1436Slice.CastInit(world, missing, 1, 0, "Nope", null, new List<VmIntermission1436Slice.Snd> { new() { Name = "dswpn" } }, false);
        Assert.Null(missing.State);
        Assert.Null(missing.Name);
        Assert.Empty(missing.Sounds);
        Assert.Equal(0, missing.Ticker);

        page.CastTics = 5;
        Assert.Equal(0, VmIntermission1436Slice.CastTick(world, page));
        Assert.Equal(1, page.Ticker);
        Assert.Equal(4, page.CastTics);
        Assert.Equal(0, page.Frames);

        page.CastTics = 1;
        page.Frames = 11;
        Assert.Equal(0, VmIntermission1436Slice.CastTick(world, page));
        Assert.True(page.Attacking);
        Assert.Same(missile, page.State);
        Assert.Same(missile, page.Base);
        Assert.Equal(1, page.OnMelee);
        Assert.Equal(8, page.CastTics);
        Assert.True(world.Stopped);
        Assert.Equal("1:32:6:1:0", world.Audio[^1]);

        page.Attacking = false;
        page.Frames = 11;
        page.CastTics = 1;
        page.State = see;
        page.OnMelee = 0;
        body.Missile = null;
        world.Stopped = false;
        world.Audio.Clear();
        Assert.Equal(0, VmIntermission1436Slice.CastTick(world, page));
        Assert.Same(melee, page.State);
        Assert.Equal(1, page.OnMelee);
        Assert.Equal(7, page.CastTics);

        var player = new VmIntermission1436Slice.Kind { Player = true };
        player.Labels.Kids["Death"] = kind.Labels.Kids["Death"];
        world.Kinds["Fighter"] = player;
        world.HasActor = true;
        world.SkinnedDeath = 9;
        world.Console = 3;
        var pbody = new VmIntermission1436Slice.Body { See = see, Missile = missile, Spawn = spawn, SeeSound = 0, DeathSound = 8, ScaleX = 2, ScaleY = 3, Style = 4, Alpha = 1 };
        missile = State(10, 8, sprite: 1, frame: 0);
        pbody.Missile = missile;
        var fighter = new VmIntermission1436Slice.Page { Body = pbody, Font = new VmIntermission1436Slice.Face { Measure = 6 }, Name = "$TAG" };
        world.Strings["TAG"] = "Hero";
        world.Audio.Clear();
        VmIntermission1436Slice.CastInit(world, fighter, 10, 1, "Fighter", "$TAG", new List<VmIntermission1436Slice.Snd>(), true);
        Assert.Same(missile, fighter.Advance);
        Assert.Equal((1 << 16) | 3, fighter.Translation);
        Assert.Equal(new[] { "2:32:1:1:0" }, world.Audio);
        world.Audio.Clear();
        Assert.Equal(1, VmIntermission1436Slice.CastKey(world, fighter, VmIntermission1436Slice.KeyDown));
        Assert.True(fighter.Death);
        Assert.Equal(9, fighter.CastTics);
        Assert.Equal("2:32:9:1:0", Assert.Single(world.Audio));
        Assert.Equal(1, VmIntermission1436Slice.CastKey(world, fighter, VmIntermission1436Slice.KeyDown));
        Assert.Equal(0, VmIntermission1436Slice.CastKey(world, fighter, 4));

        fighter.Death = false;
        missile.Next = see;
        fighter.State = missile;
        fighter.Advance = missile;
        fighter.CastTics = 1;
        fighter.Frames = 4;
        fighter.Attacking = true;
        Assert.Equal(0, VmIntermission1436Slice.CastTick(world, fighter));
        Assert.False(fighter.Attacking);
        Assert.Equal(0, fighter.Frames);
        Assert.Same(see, fighter.State);
        Assert.Equal(5, fighter.CastTics);

        var forever = State(40, -1);
        forever.Next = State(41, 3);
        fighter.State = forever;
        fighter.CastTics = 1;
        fighter.Death = false;
        Assert.Equal(-1, VmIntermission1436Slice.CastTick(world, fighter));

        var loop = State(50, 2);
        loop.Next = loop;
        fighter.State = loop;
        fighter.Death = true;
        fighter.CastTics = 1;
        Assert.Equal(-1, VmIntermission1436Slice.CastTick(world, fighter));

        var hold = State(60, 2, range: 3);
        step.Next = hold;
        page.State = step;
        page.Class = kind;
        page.Body = body;
        page.Death = false;
        page.Attacking = false;
        page.Frames = 0;
        page.CastTics = 1;
        body.Missile = missile;
        world.Rolls.Enqueue(5);
        Assert.Equal(0, VmIntermission1436Slice.CastTick(world, page));
        Assert.Same(hold, page.State);
        Assert.Equal(3, page.CastTics);

        page.State = null;
        page.CastTics = 4;
        Assert.Equal(-1, VmIntermission1436Slice.CastTick(world, page));
        Assert.Equal(3, page.CastTics);

        world.CleanX = 2;
        world.Classes.Add(player);
        world.Skins.Add(new VmIntermission1436Slice.Skin { Sprite = 2, ScaleX = 4, ScaleY = 5 });
        world.Sprites.Add(new VmIntermission1436Slice.Sprite());
        world.Sprites.Add(new VmIntermission1436Slice.Sprite { First = 0 });
        world.Sprites.Add(new VmIntermission1436Slice.Sprite { First = 1 });
        world.Frames.Add(new VmIntermission1436Slice.Frame { Pic = "spawn", Flip = 1, DispW = 8, DispH = 10 });
        world.Frames.Add(new VmIntermission1436Slice.Frame { Pic = "skin", Flip = 2, DispW = 3, DispH = 4 });
        fighter.State = missile;
        fighter.Class = player;
        fighter.Death = false;
        VmIntermission1436Slice.DrawCast(world, fighter);
        Assert.Equal("11,154,180,Hero", Assert.Single(world.Titles));
        Assert.Equal("skin,0,12,20,4,1,65539", Assert.Single(world.Pics));

        fighter.Body!.Flags4 = VmIntermission1436Slice.NoSkin;
        world.Pics.Clear();
        world.Titles.Clear();
        fighter.Name = null;
        VmIntermission1436Slice.DrawCast(world, fighter);
        Assert.Empty(world.Titles);
        Assert.Equal("spawn,1,16,30,4,1,65539", Assert.Single(world.Pics));
    }

    [Fact]
    public void CastMissingDeath()
    {
        var see = State(0, 5, sprite: 1);
        var body = new VmIntermission1436Slice.Body { See = see, SeeSound = 4 };
        var world = new VmIntermission1436Slice.World();
        var bare = new VmIntermission1436Slice.Kind();
        world.Kinds["Ghost"] = bare;
        var ghost = new VmIntermission1436Slice.Page { Body = body };
        VmIntermission1436Slice.CastInit(world, ghost, 1, 0, "Ghost", "Ghost", new List<VmIntermission1436Slice.Snd>(), true);
        Assert.Same(see, ghost.State);
        Assert.Equal(-1, VmIntermission1436Slice.CastKey(world, ghost, VmIntermission1436Slice.KeyDown));
        Assert.True(ghost.Death);
        Assert.Null(ghost.State);
        Assert.Equal(0, ghost.CastTics);
    }
}
