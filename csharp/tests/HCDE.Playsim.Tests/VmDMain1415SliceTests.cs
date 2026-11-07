using HCDE.Playsim;
namespace HCDE.Playsim.Tests;
public class VmDMain1415SliceTests
{
    [Fact]
    public void PatchesExecFilesAndMapFlag()
    {
        var state = new VmDMain1415Slice.State();
        state.Search = (name, ext, _, _) => name == "one" && ext == ".deh" ? "one.deh"
            : name == "two" && ext == ".bex" ? "two.bex" : null;
        Assert.True(VmDMain1415Slice.ConsiderPatches(state, new[] { "one", "two", "three" }));
        Assert.Equal(new[] { ("one", ".deh", false, "GameConfig"), ("two", ".deh", false, "GameConfig"), ("two", ".bex", false, "GameConfig"), ("three", ".deh", false, "GameConfig"), ("three", ".bex", false, "GameConfig") }, state.Searches);
        Assert.Equal(new[] { ("one.deh", 0), ("two.bex", 0) }, state.DehLoads);
        var empty = new VmDMain1415Slice.State();
        Assert.False(VmDMain1415Slice.ConsiderPatches(empty, Array.Empty<string>()));
        Assert.Empty(empty.Searches);
        var missing = new VmDMain1415Slice.State();
        Assert.True(VmDMain1415Slice.ConsiderPatches(missing, new[] { "gone" }));
        Assert.Empty(missing.DehLoads);
        int parses = 0;
        var exec = new VmDMain1415Slice.State
        {
            ParseExec = (item, current) =>
            {
                parses++;
                return current + ">" + item;
            },
        };
        Assert.Equal("E", VmDMain1415Slice.MultiExec(exec, Array.Empty<string>(), "E"));
        Assert.Equal(0, parses);
        Assert.Equal("E>a>b", VmDMain1415Slice.MultiExec(exec, new[] { "a", "b" }, "E"));
        Assert.Equal(2, parses);
        var files = new VmDMain1415Slice.State
        {
            FileArgs = { "skip.wad", "a", "b" },
            Wads = { new VmDMain1415Slice.Wad("base.wad", true) },
            AddWild = (wads, name, _, _, optional) =>
            {
                wads.Add(new VmDMain1415Slice.Wad(name + "!", optional));
                if (name == "a")
                    wads.Add(new VmDMain1415Slice.Wad(name + "!!", optional));
            },
        };
        VmDMain1415Slice.GetCmdLineFiles(files, false);
        Assert.Equal(new[] { ("a", ".wad", "GameConfig", false), ("b", ".wad", "GameConfig", false) }, files.WildCalls);
        Assert.Equal(new[]
        {
            new VmDMain1415Slice.Wad("base.wad", true),
            new VmDMain1415Slice.Wad("a!", false),
            new VmDMain1415Slice.Wad("a!!", false),
            new VmDMain1415Slice.Wad("b!", false),
        }, files.Wads);
        var opt = new VmDMain1415Slice.State
        {
            OptArgs = { "o1", "o2" },
            Wads = { new VmDMain1415Slice.Wad("keep", true) },
        };
        VmDMain1415Slice.GetCmdLineFiles(opt, true);
        Assert.Equal(new[] { ("o1", ".wad", "GameConfig", true), ("o2", ".wad", "GameConfig", true) }, opt.WildCalls);
        Assert.Equal(new[]
        {
            new VmDMain1415Slice.Wad("keep", true),
            new VmDMain1415Slice.Wad("o1", false),
            new VmDMain1415Slice.Wad("o2", false),
        }, opt.Wads);
        var none = Map(4);
        VmDMain1415Slice.SetMapxxFlag(none);
        Assert.Equal(4, none.Flags);
        var pwad = Map(4, Short("MAP01", 1));
        VmDMain1415Slice.SetMapxxFlag(pwad);
        Assert.Equal(4, pwad.Flags);
        var iwad = Map(4, Short("map01", 0));
        VmDMain1415Slice.SetMapxxFlag(iwad);
        Assert.Equal(5, iwad.Flags);
        var full = Map(4, Full("MAPS/MAP01.WAD", 0));
        VmDMain1415Slice.SetMapxxFlag(full);
        Assert.Equal(5, full.Flags);
        var map = Map(2, Full("maps/map01.map", 0));
        VmDMain1415Slice.SetMapxxFlag(map);
        Assert.Equal(3, map.Flags);
        var any = Map(0, Short("MAP01", 9));
        any.Iwad = -1;
        VmDMain1415Slice.SetMapxxFlag(any);
        Assert.Equal(1, any.Flags);
        var later = Map(0, Full("maps/map01.wad", 0), Full("maps/map01.wad", 3));
        later.Iwad = 0;
        VmDMain1415Slice.SetMapxxFlag(later);
        Assert.Equal(1, later.Flags);
    }
    [Fact]
    public void GameInfoParseAndCheck()
    {
        var state = new VmDMain1415Slice.State
        {
            Tokens = Script(),
            Exists = path => path == "C:/mods/there.wad" ? (true, true) : (false, false),
            Accept = name => name != "C:/mods/there.wad",
            Color = text => text == "ff00" ? 7 : 8,
        };
        var pwads = new List<VmDMain1415Slice.Wad>();
        Assert.Equal("Doom2.wad", VmDMain1415Slice.ParseGameInfo(state, pwads, "C:/mods/pack.wad", "abc"));
        Assert.Equal("GAMEINFO", state.Script);
        Assert.Equal(3, state.ScriptSize);
        Assert.Equal(new[] { ("C:/mods/there.wad", true, true), ("C:/mods/gone.wad", false, false) }, state.Looks);
        Assert.Equal(new[] { ("C:/mods/there.wad", 0, true, false), ("gone.wad", 0, true, false) }, state.Adds);
        Assert.Equal(new[] { new VmDMain1415Slice.Wad("gone.wad", false) }, pwads);
        Assert.True(state.NoSpriteRename);
        Assert.Equal("Hello", state.StartupName);
        Assert.Equal(new[] { "ff00", "10" }, state.Colors);
        Assert.Equal(7, state.Fg);
        Assert.Equal(8, state.Bk);
        Assert.Equal(VmDMain1415Slice.HereticStartup, state.StartupType);
        Assert.Equal("d_intro", state.Song);
        Assert.Equal(1, state.LoadLights);
        Assert.Equal(0, state.LoadBrightmaps);
        Assert.Equal(1, state.LoadWidescreen);
        Assert.Equal("99", state.Steam);
        var titled = new VmDMain1415Slice.State { Tokens = { Id("startuptitle"), Sym('='), Id("Only") } };
        Assert.Equal("", VmDMain1415Slice.ParseGameInfo(titled, new List<VmDMain1415Slice.Wad>(), "bare.wad", ""));
        Assert.Equal(-1, titled.LoadLights);
        Assert.Equal(-1, titled.LoadBrightmaps);
        Assert.Equal(-1, titled.LoadWidescreen);
        Assert.Equal("Only", titled.StartupName);
        var shown = new VmDMain1415Slice.State
        {
            ShowLauncher = true,
            Tokens = { Id("IWAD"), Sym('='), Id("Doom2.wad"), Id("STEAMAPPID"), Sym('='), Id("5") },
        };
        Assert.Equal("", VmDMain1415Slice.ParseGameInfo(shown, new List<VmDMain1415Slice.Wad>(), "pack.wad", "z"));
        Assert.Equal("5", shown.Steam);
        var colon = new VmDMain1415Slice.State
        {
            Tokens = { Id("LOAD"), Sym('='), Id("x.wad") },
            Exists = path => (path == "C/x.wad", false),
        };
        var colonWads = new List<VmDMain1415Slice.Wad>();
        VmDMain1415Slice.ParseGameInfo(colon, colonWads, "C:pack.wad", "q");
        Assert.Equal(new[] { new VmDMain1415Slice.Wad("C/x.wad", false) }, colonWads);
        var plain = new VmDMain1415Slice.State { Tokens = { Id("LOAD"), Sym('='), Id("x.wad") } };
        VmDMain1415Slice.ParseGameInfo(plain, new List<VmDMain1415Slice.Wad>(), "pack.wad", "q");
        Assert.Equal(new[] { ("x.wad", false, false) }, plain.Looks);
        var typed = new VmDMain1415Slice.State { Tokens = { Id("startuptype"), Sym('='), Id("quake") } };
        VmDMain1415Slice.ParseGameInfo(typed, new List<VmDMain1415Slice.Wad>(), "pack.wad", "q");
        Assert.Equal(VmDMain1415Slice.DefaultStartup, typed.StartupType);
        var no = new VmDMain1415Slice.State { Tokens = { Id("NOSPRITERENAME"), Sym('='), Id("Yes") } };
        VmDMain1415Slice.ParseGameInfo(no, new List<VmDMain1415Slice.Wad>(), "pack.wad", "q");
        Assert.False(no.NoSpriteRename);
        Assert.Equal("Expected identifier but got '=' instead.", Boom(new (int, string)[] { Sym('=') }));
        Assert.Equal("Expected '=' but got identifier 'load' instead.", Boom(new[] { Id("iwad"), Id("load") }));
        Assert.Equal("SC_GetNumber: Bad numeric constant \"12x\".", Boom(new[] { Id("loadlights"), Sym('='), Id("12x") }));
        Assert.Equal("Missing integer (unexpected end of file).", Boom(new[] { Id("loadlights"), Sym('=') }));
        Assert.Equal("Expected ',', got 'x'.", Boom(new[] { Id("startupcolors"), Sym('='), Id("ff"), Id("x") }));
        var closed = new VmDMain1415Slice.State();
        List<VmDMain1415Slice.Wad>? seen = null;
        closed.Init = list =>
        {
            seen = list;
            return false;
        };
        var pending = new List<VmDMain1415Slice.Wad> { new("a.wad", false) };
        Assert.Equal("", VmDMain1415Slice.CheckGameInfo(closed, pending));
        Assert.Same(pending, seen);
        var filter = closed.UsedFilter ?? throw new InvalidOperationException("missing filter");
        Assert.Equal(VmDMain1415Slice.Folders, filter.Folders);
        Assert.Equal(VmDMain1415Slice.Prefixes, filter.Prefixes);
        Assert.Equal(VmDMain1415Slice.Blocked, filter.Blocked);
        Assert.Null(closed.Script);
        var open = new VmDMain1415Slice.State
        {
            Init = _ => true,
            Tokens = { Id("IWAD"), Sym('='), Id("WIN") },
            CheckLumps =
            {
                new("GAMEINFO", "", 0, 0, "aa", "old.wad"),
                new("GAMEINFO", "", 0, 1, "nope", "bad.wad"),
                new("gameinfo", "", 0, 0, "bbbb", "mods/win.wad"),
            },
        };
        Assert.Equal("WIN", VmDMain1415Slice.CheckGameInfo(open, new List<VmDMain1415Slice.Wad>()));
        Assert.Equal("mods/win.wad", open.UsedResource);
        Assert.Equal(4, open.ScriptSize);
        var skipped = new VmDMain1415Slice.State
        {
            Init = _ => true,
            CheckLumps = { new("GAMEINFO", "", 0, 1, "nope", "bad.wad") },
        };
        Assert.Equal("", VmDMain1415Slice.CheckGameInfo(skipped, new List<VmDMain1415Slice.Wad>()));
        Assert.Null(skipped.Script);
    }
    static VmDMain1415Slice.State Map(int flags, params VmDMain1415Slice.Lump[] lumps)
    {
        var state = new VmDMain1415Slice.State { Flags = flags };
        state.MapLumps.AddRange(lumps);
        return state;
    }
    static VmDMain1415Slice.Lump Short(string name, int container) => new(name, "", container, 0, "", "");
    static VmDMain1415Slice.Lump Full(string name, int container) => new("", name, container, 0, "", "");
    static (int Kind, string Text) Id(string text) => (VmDMain1415Slice.Identifier, text);
    static (int Kind, string Text) Sym(char kind) => (kind, kind.ToString());
    static List<(int, string)> Script() => new()
    {
        Id("iwad"), Sym('='), Id("Doom2.wad"),
        Id("load"), Sym('='), Id("there.wad"), Sym(','), Id("gone.wad"),
        Id("nospriterename"), Sym('='), Id("TRUE"),
        Id("startuptitle"), Sym('='), Id("Hello"),
        Id("startupcolors"), Sym('='), Id("ff00"), Sym(','), Id("10"),
        Id("startuptype"), Sym('='), Id("Heretic"),
        Id("startupsong"), Sym('='), Id("d_intro"),
        Id("loadlights"), Sym('='), Id("-2"),
        Id("loadbrightmaps"), Sym('='), Id("0"),
        Id("loadwidescreen"), Sym('='), Id("MAXINT"),
        Id("discordappid"), Sym('='), Id("secret"),
        Id("foo"), Sym('='), Id("1"), Sym(','), Id("2"),
        Id("steamappid"), Sym('='), Id("99"),
    };
    static string Boom(IEnumerable<(int, string)> tokens)
    {
        var state = new VmDMain1415Slice.State();
        state.Tokens.AddRange(tokens);
        var error = Assert.Throws<VmDMain1415Slice.ScriptError>(() => VmDMain1415Slice.ParseGameInfo(state, new List<VmDMain1415Slice.Wad>(), "pack.wad", "q"));
        return error.Message;
    }
}
