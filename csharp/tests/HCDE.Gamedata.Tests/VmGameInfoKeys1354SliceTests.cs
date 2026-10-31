using HCDE.Gamedata;

namespace HCDE.Gamedata.Tests;

public class VmGameInfoKeys1354SliceTests
{
    [Fact]
    public void NamesBordersAndFields()
    {
        Assert.Equal(17, VmGameInfoKeys1354Slice.GameNames.Length);
        Assert.Null(VmGameInfoKeys1354Slice.GameNames[0]);
        Assert.Equal("Doom", VmGameInfoKeys1354Slice.GameNames[1]);
        Assert.Equal("Heretic", VmGameInfoKeys1354Slice.GameNames[2]);
        Assert.Null(VmGameInfoKeys1354Slice.GameNames[3]);
        Assert.Equal("Hexen", VmGameInfoKeys1354Slice.GameNames[4]);
        Assert.Equal("Strife", VmGameInfoKeys1354Slice.GameNames[8]);
        Assert.Null(VmGameInfoKeys1354Slice.GameNames[15]);
        Assert.Equal("Chex", VmGameInfoKeys1354Slice.GameNames[16]);

        Assert.Equal(new string?[] { "DoomBorder", "HereticBorder", "StrifeBorder", null }, VmGameInfoKeys1354Slice.BorderNames);
        Assert.Equal(8, VmGameInfoKeys1354Slice.DoomBorder.Offset);
        Assert.Equal(8, VmGameInfoKeys1354Slice.DoomBorder.Size);
        Assert.Equal("brdr_tl", VmGameInfoKeys1354Slice.DoomBorder.Tl);
        Assert.Equal("brdr_br", VmGameInfoKeys1354Slice.DoomBorder.Br);
        Assert.Equal(4, VmGameInfoKeys1354Slice.HereticBorder.Offset);
        Assert.Equal(16, VmGameInfoKeys1354Slice.HereticBorder.Size);
        Assert.Equal("bordtl", VmGameInfoKeys1354Slice.HereticBorder.Tl);
        Assert.Equal("bordbr", VmGameInfoKeys1354Slice.HereticBorder.Br);
        Assert.Equal(VmGameInfoKeys1354Slice.DoomBorder.Tl, VmGameInfoKeys1354Slice.StrifeBorder.Tl);
        Assert.NotSame(VmGameInfoKeys1354Slice.DoomBorder, VmGameInfoKeys1354Slice.StrifeBorder);

        Assert.Equal("AGameInfoStruct", VmGameInfoKeys1354Slice.ScriptClass);
        Assert.Equal("gameinfo", VmGameInfoKeys1354Slice.GlobalName);
        Assert.Equal("turbo", VmGameInfoKeys1354Slice.TurboCvar);
        Assert.Equal(32, VmGameInfoKeys1354Slice.FieldNames.Length);
        Assert.Equal("backpacktype", VmGameInfoKeys1354Slice.FieldNames[0]);
        Assert.Equal("BloodSplatDecalDistance", VmGameInfoKeys1354Slice.FieldNames[^1]);
        Assert.Contains("mStatscreenMapNameFont", VmGameInfoKeys1354Slice.FieldNames);
        Assert.Contains("normforwardmove", VmGameInfoKeys1354Slice.FieldNames);
    }

    [Fact]
    public void KeyMacros()
    {
        var game = new GameInfo1354();
        var sc = Scan(new Tok1354('s', "Hello"));
        Assert.False(VmGameInfoKeys1354Slice.ApplyCString(game, sc, "other", "mTitle", "title", 8));
        Assert.Equal(0, sc.At);
        Assert.True(VmGameInfoKeys1354Slice.ApplyCString(game, sc, "TITLE", "mTitle", "title", 8));
        Assert.Equal("Hello", game.Text["mTitle"]);

        sc = Scan(new Tok1354('s', "Hello!!!!"));
        Assert.Equal(
            "Value for 'mTitle' can not be longer than 8 characters.",
            Assert.Throws<InvalidOperationException>(() =>
                VmGameInfoKeys1354Slice.ApplyCString(game, sc, "title", "mTitle", "title", 8)).Message);
        Assert.False(game.Text.ContainsKey("creditPages"));

        sc = Scan(new Tok1354('s', "One"), new Tok1354(','), new Tok1354('s', "Two"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyStringArray(game, sc, "addcreditPage", "creditPages", "addcreditPage", 8, false));
        sc = Scan(new Tok1354('s', "Three"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyStringArray(game, sc, "addcreditPage", "creditPages", "addcreditPage", 0, false));
        Assert.Equal(new[] { "One", "Two", "Three" }, game.Texts["creditPages"]);
        sc = Scan(new Tok1354('s', "Only"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyStringArray(game, sc, "CreditPage", "creditPages", "CreditPage", 8, true));
        Assert.Equal(new[] { "Only" }, game.Texts["creditPages"]);
        sc = Scan(new Tok1354('s', "WayTooLong"));
        Assert.Equal(
            "Value for 'creditPages' can not be longer than 8 characters.",
            Assert.Throws<InvalidOperationException>(() =>
                VmGameInfoKeys1354Slice.ApplyStringArray(game, sc, "CreditPage", "creditPages", "CreditPage", 8, true)).Message);

        int sounds = 0;
        sc = Scan(new Tok1354('s', "dspistol"), new Tok1354(','), new Tok1354('s', "dsshotgn"));
        sc.FindSound = name =>
        {
            sounds++;
            return name == "dspistol" ? 3 : 9;
        };
        Assert.True(VmGameInfoKeys1354Slice.ApplySoundArray(game, sc, "precachesounds", "PrecachedSounds", "precachesounds", 0, true));
        Assert.Equal(new[] { 3, 9 }, game.Sounds["PrecachedSounds"]);
        Assert.Equal(2, sounds);
        Assert.Equal("S_FindSound", Assert.Throws<InvalidOperationException>(() =>
            VmGameInfoKeys1354Slice.ApplySoundArray(new GameInfo1354(), Scan(new Tok1354('s', "x")), "precachesounds", "PrecachedSounds", "precachesounds", 0, true)).Message);

        sc = Scan(new Tok1354('s', "sbar"));
        sc.LumpNum = 9;
        sc.Container = lump => lump + 1;
        Assert.True(VmGameInfoKeys1354Slice.ApplyStamped(game, sc, "statusbar", "statusbar", "statusbar", "statusbarfile"));
        Assert.Equal("sbar", game.Text["statusbar"]);
        Assert.Equal(10, game.Ints["statusbarfile"]);

        sc = Scan(new Tok1354('n', "16777217"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyFloat(game, sc, "titleTime", "titleTime", "titleTime"));
        Assert.Equal((float)16777217d, game.Floats["titleTime"]);
        sc = Scan(new Tok1354('n', "16777217"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyDouble(game, sc, "gibfactor", "gibfactor", "gibfactor"));
        Assert.Equal(16777217d, game.Doubles["gibfactor"]);

        sc = Scan(new Tok1354('d', "1.5"), new Tok1354(','), new Tok1354('n', "-2"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyTwoDoubles(game, sc, "normforwardmove", "normforwardmove", "normforwardmove"));
        Assert.Equal((1.5, -2d), game.Pairs["normforwardmove"]);
        sc = Scan(new Tok1354('s', "nope"));
        Assert.Equal("Numeric constant expected", Assert.Throws<InvalidOperationException>(() =>
            VmGameInfoKeys1354Slice.ApplyTwoDoubles(game, sc, "normforwardmove", "normforwardmove", "normforwardmove")).Message);

        sc = Scan(new Tok1354('s', "red"));
        sc.ColorByName = name => name == "red" ? "ff0000" : "";
        sc.ColorFromString = text => text == "ff0000" ? 0xff0000 : 7;
        Assert.True(VmGameInfoKeys1354Slice.ApplyColor(game, sc, "pickupcolor", "pickupcolor", "pickupcolor"));
        Assert.Equal(0xff0000, game.Ints["pickupcolor"]);
        sc = Scan(new Tok1354('s', "#112233"));
        sc.ColorByName = _ => "";
        sc.ColorFromString = text => text == "#112233" ? 0x112233 : 0;
        Assert.True(VmGameInfoKeys1354Slice.ApplyColor(game, sc, "dimcolor", "dimcolor", "dimcolor"));
        Assert.Equal(0x112233, game.Ints["dimcolor"]);

        sc = Scan(new Tok1354('b'));
        Assert.True(VmGameInfoKeys1354Slice.ApplyBool(game, sc, "swapmenu", "swapmenu", "swapmenu"));
        Assert.False(game.Bools["swapmenu"]);
        sc = Scan(new Tok1354('t'));
        Assert.True(VmGameInfoKeys1354Slice.ApplyBool(game, sc, "drawreadthis", "drawreadthis", "drawreadthis"));
        Assert.True(game.Bools["drawreadthis"]);
        sc = Scan(new Tok1354('i', "maybe"));
        Assert.Contains("true", Assert.Throws<InvalidOperationException>(() =>
            VmGameInfoKeys1354Slice.ApplyBool(game, sc, "swapmenu", "swapmenu", "swapmenu")).Message);

        sc = Scan(new Tok1354('s', "BigFont"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyFont(game, sc, "statscreen_mapnamefont", "mStatscreenMapNameFont", "statscreen_mapnamefont"));
        Assert.Equal(("BigFont", "None"), game.Fonts["mStatscreenMapNameFont"]);
        sc = Scan(new Tok1354('s', "Small"), new Tok1354(','), new Tok1354('s', "gold"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyFont(game, sc, "statscreen_authorfont", "mStatscreenAuthorFont", "statscreen_authorfont"));
        Assert.Equal(("Small", "gold"), game.Fonts["mStatscreenAuthorFont"]);
        sc = Scan(new Tok1354('s', "M_SKULL"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyPatch(game, sc, "cursorpic", "CursorPic", "CursorPic"));
        Assert.Equal(("M_SKULL", "Null"), game.Fonts["CursorPic"]);

        sc = Scan(new Tok1354('s', "d_intro:12"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyMusic(game, sc, "TITLEMUSIC", "titleMusic", "titleOrder", "titleMusic"));
        Assert.Equal("d_intro", game.Text["titleMusic"]);
        Assert.Equal(12, game.Ints["titleOrder"]);
        sc = Scan(new Tok1354('s', "d_dm2int"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyMusic(game, sc, "intermissionMusic", "intermissionMusic", "intermissionOrder", "intermissionMusic"));
        Assert.Equal("d_dm2int", game.Text["intermissionMusic"]);
        Assert.Equal(0, game.Ints["intermissionOrder"]);
        sc = Scan(new Tok1354('s', "d_read: 8x"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyMusic(game, sc, "finaleMusic", "finaleMusic", "finaleOrder", "finaleMusic"));
        Assert.Equal("d_read", game.Text["finaleMusic"]);
        Assert.Equal(8, game.Ints["finaleOrder"]);
        sc = Scan(new Tok1354('s', "d_bunny:-2"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyMusic(game, sc, "titleMusic", "titleMusic", "titleOrder", "titleMusic"));
        Assert.Equal(-2, game.Ints["titleOrder"]);

        sc = Scan(new Tok1354('n', "40"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyInt(game, sc, "defKickback", "defKickback", "defKickback"));
        Assert.Equal(40, game.Ints["defKickback"]);
        sc = Scan(new Tok1354('s', "page"));
        Assert.True(VmGameInfoKeys1354Slice.ApplyString(game, sc, "titlePage", "TitlePage", "titlePage"));
        Assert.Equal("page", game.Text["TitlePage"]);
    }

    private static Scan1354 Scan(params Tok1354[] tokens) => new() { Items = [.. tokens] };
}
