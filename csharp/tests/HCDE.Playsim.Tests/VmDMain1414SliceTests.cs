using S = HCDE.Playsim.VmDMain1414Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1414SliceTests
{
    [Fact]
    public void AdvanceDemoTitleAndEndgame()
    {
        Assert.Equal(1950, S.BandStart);
        Assert.Equal(2241, S.BandEnd);
        Assert.Equal(2251, S.ConsiderPatchesStart);
        Assert.Equal(8, S.Strife);
        Assert.Equal(17, S.FullConsole);

        var blocked = Demo();
        blocked.GameAction = 5;
        blocked.PlayerStates.Clear();
        S.DoAdvanceDemo(blocked);
        Assert.False(blocked.AdvancedDemo);
        Assert.True(blocked.UserGame);
        Assert.Equal(4, blocked.Paused);
        Assert.Null(blocked.StartedMap);
        Assert.Equal(0, blocked.StrifeAdvances);
        Assert.Empty(blocked.Steps);

        var title = Demo();
        title.TitleMap = true;
        title.GameType = S.Strife;
        S.DoAdvanceDemo(title);
        Assert.Equal("TITLEMAP", title.StartedMap);
        Assert.True(title.StartedMapTitle);
        Assert.False(title.UserGame);
        Assert.Equal(0, title.Paused);
        Assert.Equal(0, title.StrifeAdvances);
        Assert.Equal("old-page", title.Page);
        Assert.Equal(
            "player=0 PST_REBORN -> PST_LIVE reason=Title_screen_start",
            Assert.Single(title.Steps));

        var strife = Demo();
        strife.GameType = S.Strife;
        strife.DemoSequence = 1;
        S.DoAdvanceDemo(strife);
        Assert.Equal(1, strife.StrifeAdvances);
        Assert.Equal(1, strife.DemoSequence);
        Assert.Equal("old-page", strife.Page);
        Assert.False(strife.UserGame);

        var mixed = Demo();
        mixed.GameType = S.Strife | 1;
        mixed.DemoSequence = 0;
        S.DoAdvanceDemo(mixed);
        Assert.Equal(0, mixed.StrifeAdvances);
        Assert.Equal(S.DemoScreen, mixed.GameState);
        Assert.Equal("TITLEPIC", mixed.Page);

        var advisory = Demo();
        advisory.DemoSequence = 3;
        advisory.AdvisoryTime = 0.5f;
        S.DoAdvanceDemo(advisory);
        Assert.Equal("ADVISOR", advisory.Advisory);
        Assert.True(advisory.AdvisoryValid);
        Assert.Equal(1, advisory.DemoSequence);
        Assert.Equal("tic:17", Assert.Single(advisory.Steps, step => step.StartsWith("tic:")));
        Assert.Equal("old-page", advisory.Page);
        Assert.Equal(1, advisory.GameState);
        Assert.Equal(0, advisory.HideConsole);

        var played = Demo();
        played.DemoSequence = 3;
        played.DemoLumps.Add("demo1");
        S.DoAdvanceDemo(played);
        Assert.Equal(1, played.DemoCount);
        Assert.Equal("DEMO1", played.DemoName);
        Assert.Equal("DEMO1", played.PlayedDemo);
        Assert.False(played.SingleDemo);
        Assert.Equal(2, played.DemoSequence);
        Assert.Equal("old-page", played.Page);
        Assert.DoesNotContain(played.Steps, step => step.StartsWith("music:"));
        Assert.Equal(0, played.HideConsole);

        played.DemoSequence = 2;
        played.PageTime = 2f;
        played.CreditPages.Add("A");
        played.CreditPages.Add("B");
        played.PageCount = 0;
        S.DoAdvanceDemo(played);
        Assert.Equal("A", played.Page);
        Assert.Equal(1, played.PageCount);
        Assert.Equal(1, played.DemoSequence);
        Assert.Equal(S.DemoScreen, played.GameState);
        Assert.Contains("tic:70", played.Steps);

        S.DoAdvanceDemo(played);
        Assert.Equal(0, played.DemoCount);
        Assert.Equal("DEMO2", played.DemoName);
        Assert.Equal(3, played.DemoSequence);
        Assert.Equal("TITLEPIC", played.Page);
        Assert.Equal(0, played.PageCount);
        Assert.Equal(1, played.HideConsole);
        Assert.True(played.PlayedTitleMusic);
        Assert.Contains("music:d_intro:2:false", played.Steps);
        int music = played.Steps.Count(step => step.StartsWith("music:"));
        played.DemoNoPlay = true;
        S.DoAdvanceDemo(played);
        Assert.Equal(music, played.Steps.Count(step => step.StartsWith("music:")));
        Assert.Equal(2, played.HideConsole);

        var skipped = Demo();
        skipped.DemoSequence = 1;
        skipped.DemoNoPlay = true;
        S.DoAdvanceDemo(skipped);
        Assert.Equal(0, skipped.DemoCount);
        Assert.Equal("DEMO1", skipped.DemoName);
        Assert.Null(skipped.PlayedDemo);
        Assert.Equal("TITLEPIC", skipped.Page);
        Assert.Equal(3, skipped.DemoSequence);

        var blank = Demo();
        blank.DemoSequence = 2;
        blank.PageCount = 4;
        S.DoAdvanceDemo(blank);
        Assert.Equal("old-page", blank.Page);
        Assert.Equal(1, blank.DemoSequence);
        Assert.Equal(4, blank.PageCount);
        Assert.Equal(S.DemoScreen, blank.GameState);

        var wide = Demo();
        wide.DemoSequence = 1;
        wide.DemoCount = 999;
        wide.DemoLumps.Add("DEMO100");
        S.DoAdvanceDemo(wide);
        Assert.Equal("DEMO100", wide.DemoName);
        Assert.Equal("DEMO100", wide.PlayedDemo);
        Assert.Equal(1000, wide.DemoCount);

        var titleStart = Demo();
        titleStart.PlayedTitleMusic = true;
        titleStart.AdvancedDemo = false;
        titleStart.GameAction = 9;
        titleStart.DemoSequence = 4;
        S.StartTitle(titleStart);
        Assert.False(titleStart.PlayedTitleMusic);
        Assert.Equal(S.Nothing, titleStart.GameAction);
        Assert.Equal(-1, titleStart.DemoSequence);
        Assert.True(titleStart.AdvancedDemo);

        var ended = Demo();
        ended.NetGame = true;
        ended.GameAction = 3;
        ended.DemoSequence = 2;
        S.Endgame(ended);
        Assert.Equal(3, ended.GameAction);
        Assert.Equal(2, ended.DemoSequence);
        Assert.Equal(0, ended.DemoChecks);
        ended.NetGame = false;
        S.Endgame(ended);
        Assert.Equal(S.FullConsole, ended.GameAction);
        Assert.Equal(-1, ended.DemoSequence);
        Assert.Equal(1, ended.DemoChecks);
    }

    [Fact]
    public void ParseCVarInfoReadsEveryGlobalLump()
    {
        Assert.Equal(8193, S.DefaultFlags);
        Assert.Equal(1 << 13, S.Mod);
        Assert.Equal(1 << 18, S.ConfigOnly);
        Assert.Equal(0, S.TypeBool);
        Assert.Equal(4, S.TypeColor);
        Assert.Equal(7, S.TypeDummy);

        var parsed = new S.State { ConfigName = "modcfg" };
        parsed.Lumps.Add(new S.Lump
        {
            Name = "notcvar",
            Namespace = S.GlobalNamespace,
            Tokens = Tokens("server", "int", "skip", ";"),
        });
        parsed.Lumps.Add(new S.Lump
        {
            Name = "CVARINFO",
            Namespace = 1,
            Tokens = Tokens("server", "int", "hidden", ";"),
        });
        parsed.Lumps.Add(new S.Lump
        {
            Name = "cvarinfo",
            Namespace = S.GlobalNamespace,
            Tokens = new List<S.Token>
            {
                Id("SERVER"), Ty(S.TkInt, "int"), Id("hp"), Sym('='), Id("12"), Sym(';'),
                Id("user"), Ty(S.TkFloat, "float"), Id("vol"), Sym('='), Id("1.5"), Sym(';'),
                Id("nosave"), Ty(S.TkString, "string"), Id("msg"), Sym('='), Ty(S.TkString, "hi"), Sym(';'),
                Id("server"), Id("nosave"), Ty(S.TkBool, "bool"), Id("flag"), Sym('='), Ty(S.TkTrue, "true"), Sym(';'),
                Id("server"), Id("user"), Id("nosave"), Ty(S.TkInt, "int"), Id("both"), Sym(';'),
                Id("Server"), Id("noarchive"), Id("cheat"), Id("latch"), Ty(S.TkInt, "int"), Id("plain"), Sym(';'),
                Id("user"), Id("handlerClass"), Sym('('), Id("MyClass"), Sym(')'), Ty(S.TkColor, "color"), Id("tint"),
                Sym('='), Ty(S.TkString, "ff"), Sym(';'),
                Id("user"), Ty(S.TkFloat, "float"), Id("whole"), Sym('='), Id("2"), Sym(';'),
                Id("user"), Ty(S.TkBool, "bool"), Id("off"), Sym('='), Ty(S.TkFalse, "FALSE"), Sym(';'),
                Id("user"), Ty(S.TkInt, "int"), Id("cap"), Sym('='), Id("MAXINT"), Sym(';'),
            },
        });
        S.ParseCVarInfo(parsed);
        Assert.Equal(1, parsed.CModeSets);
        Assert.Equal("modcfg", Assert.Single(parsed.ModSetups));
        Assert.Equal(10, parsed.Cvars.Count);
        Assert.Equal("hp", parsed.Cvars[0].Name);
        Assert.Equal(S.TypeInt, parsed.Cvars[0].Type);
        Assert.Equal(S.DefaultFlags | S.Server, parsed.Cvars[0].Flags);
        Assert.Equal("12", parsed.Cvars[0].Default);
        Assert.Equal(S.DefaultFlags | S.User, parsed.Cvars[1].Flags);
        Assert.Equal(S.TypeFloat, parsed.Cvars[1].Type);
        Assert.Equal("1.5", parsed.Cvars[1].Default);
        Assert.Equal(S.DefaultFlags | S.ConfigOnly, parsed.Cvars[2].Flags);
        Assert.Equal("hi", parsed.Cvars[2].Default);
        Assert.Equal(S.DefaultFlags | S.ConfigOnly, parsed.Cvars[3].Flags);
        Assert.Equal("true", parsed.Cvars[3].Default);
        Assert.Equal(S.DefaultFlags | S.ConfigOnly, parsed.Cvars[4].Flags);
        Assert.Null(parsed.Cvars[4].Default);
        Assert.Equal((S.DefaultFlags & ~S.Archive) | S.Server | S.Cheat | S.Latch, parsed.Cvars[5].Flags);
        Assert.False(parsed.Cvars[5].Custom);
        Assert.True(parsed.Cvars[6].Custom);
        Assert.Equal("MyClass", parsed.Cvars[6].ClassName);
        Assert.Equal(S.TypeColor, parsed.Cvars[6].Type);
        Assert.Equal("ff", parsed.Cvars[6].Default);
        Assert.Equal(S.DefaultFlags | S.User, parsed.Cvars[6].Flags);
        Assert.Equal("2", parsed.Cvars[7].Default);
        Assert.Equal("FALSE", parsed.Cvars[8].Default);
        Assert.Equal("MAXINT", parsed.Cvars[9].Default);

        var second = new S.State();
        second.Lumps.Add(Lump("server", "int", "one", ";"));
        second.Lumps.Add(Lump("user", "string", "two", ";"));
        S.ParseCVarInfo(second);
        Assert.Equal(2, second.CModeSets);
        Assert.Equal(2, second.Cvars.Count);
        Assert.Equal("HCDE", Assert.Single(second.ModSetups));

        var none = new S.State();
        S.ParseCVarInfo(none);
        Assert.Equal(0, none.CModeSets);
        Assert.Empty(none.Cvars);
        Assert.Empty(none.ModSetups);

        var empty = new S.State();
        empty.Lumps.Add(new S.Lump { Name = "CVARINFO", Tokens = new List<S.Token>() });
        S.ParseCVarInfo(empty);
        Assert.Equal(1, empty.CModeSets);
        Assert.Empty(empty.ModSetups);

        var known = new S.State();
        known.Existing.Add("hp");
        known.Lumps.Add(Lump("server", "int", "hp", ";"));
        var exists = Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(known));
        Assert.Equal("cvar 'hp' already exists", exists.Message);
        Assert.Empty(known.Cvars);
        Assert.Empty(known.ModSetups);

        var dup = new S.State();
        dup.Lumps.Add(Lump("server", "int", "hp", ";", "user", "int", "hp", ";"));
        var again = Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(dup));
        Assert.Equal("cvar 'hp' already exists", again.Message);
        Assert.Equal("hp", Assert.Single(dup.Cvars).Name);
        Assert.Empty(dup.ModSetups);

        Assert.Equal(
            "One of 'server', 'user', or 'nosave' must be specified",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("server", "user", "int", "both", ";"))).Message);
        Assert.Equal(
            "One of 'server', 'user', or 'nosave' must be specified",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("int", "bare", ";"))).Message);
        Assert.Equal(
            "Unknown cvar attribute 'zoom'",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("zoom", "int", "x", ";"))).Message);
        var badType = new S.State();
        badType.Lumps.Add(new S.Lump
        {
            Name = "CVARINFO",
            Tokens = new List<S.Token> { Id("server"), Ty(999, "name") },
        });
        Assert.Equal(
            "Bad cvar type 'name'",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(badType)).Message);
        Assert.Equal(
            "Expected true or false",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("user", "bool", "x", "=", "nope", ";"))).Message);
        Assert.Equal(
            "SC_GetNumber: Bad numeric constant \"abc\".",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("user", "int", "x", "=", "abc", ";"))).Message);
        Assert.Equal(
            "SC_GetFloat: Bad numeric constant \"zz\".",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("user", "float", "x", "=", "zz", ";"))).Message);
        Assert.Equal(
            "Missing integer (unexpected end of file).",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("user", "int", "x", "="))).Message);
        Assert.Equal(
            "Expected '(', got 'int'.",
            Assert.Throws<S.ScriptError>(() => S.ParseCVarInfo(Parse("handlerClass", "int", "x", ";"))).Message);
    }

    private static S.State Demo() => new()
    {
        PlayerStates = new List<int> { 2 },
        ConsolePlayer = 0,
    };

    private static S.State Parse(params string[] words)
    {
        var state = new S.State();
        state.Lumps.Add(Words(words));
        return state;
    }

    private static S.Lump Lump(params string[] words) => Words(words);

    private static S.Lump Words(string[] words) => new()
    {
        Name = "CVARINFO",
        Tokens = Tokens(words),
    };

    private static List<S.Token> Tokens(params string[] words)
    {
        var tokens = new List<S.Token>();
        foreach (string word in words)
            tokens.Add(Word(word));
        return tokens;
    }

    private static S.Token Word(string word)
    {
        if (word == ";")
            return Sym(';');
        if (word == "=")
            return Sym('=');
        if (word == "(")
            return Sym('(');
        if (word == ")")
            return Sym(')');
        if (word == "int")
            return Ty(S.TkInt, word);
        if (word == "float")
            return Ty(S.TkFloat, word);
        if (word == "bool")
            return Ty(S.TkBool, word);
        if (word == "string")
            return Ty(S.TkString, word);
        if (word == "color")
            return Ty(S.TkColor, word);
        if (word == "true")
            return Ty(S.TkTrue, word);
        if (word == "false")
            return Ty(S.TkFalse, word);
        return Id(word);
    }

    private static S.Token Id(string text) => new(S.TkIdentifier, text);

    private static S.Token Ty(int kind, string text) => new(kind, text);

    private static S.Token Sym(char text) => new(text, text.ToString());
}
