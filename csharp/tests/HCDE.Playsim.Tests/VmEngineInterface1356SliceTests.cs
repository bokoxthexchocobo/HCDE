using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmEngineInterface1356SliceTests
{
    [Fact]
    public void DefaultsFlagsAndLanguage()
    {
        VmEngineInterface1356Slice.RequireLp64(8);
        var narrow = Assert.Throws<InvalidOperationException>(() => VmEngineInterface1356Slice.RequireLp64(4));
        Assert.Contains("LP64", narrow.Message);
        Assert.Throws<InvalidOperationException>(() => VmEngineInterface1356Slice.RequireLp64(16));

        var decls = VmEngineInterface1356Slice.Declarations;
        Assert.Equal(25, decls.Length);
        Assert.Equal(
            new[]
            {
                "vid_fps", "queryiwad", "saveargs", "savenetfile", "savenetargs",
                "defaultiwad", "defaultaddonfiles", "defaultargs", "defaultnetiwad", "defaultnetargs",
                "defaultnetplayers", "defaultnethostport", "defaultnetticdup", "defaultnetgamemode",
                "defaultnetaltdm", "defaultnetaddress", "defaultnetjoinport", "defaultnetpage",
                "defaultnethostteam", "defaultnetjointeam", "defaultnetextratic", "defaultnetsavefile",
                "ui_colors", "ui_color_mix", "language",
            },
            decls.Select(d => d.Name).ToArray());
        foreach (var decl in decls)
        {
            int flags = decl.Name == "vid_fps"
                ? 0
                : decl.Name == "language"
                    ? VmEngineInterface1356Slice.LanguageFlags
                    : VmEngineInterface1356Slice.ArchiveGlobal;
            Assert.Equal(flags, decl.Flags);
            Assert.Equal(decl.Name == "language", decl.Callback);
        }
        Assert.Equal(1025, VmEngineInterface1356Slice.ArchiveGlobal);
        Assert.Equal(1537, VmEngineInterface1356Slice.LanguageFlags);
        Assert.Equal("Bool", decls[0].Kind);
        Assert.False((bool)decls[0].Value);
        Assert.True((bool)decls.Single(d => d.Name == "queryiwad").Value);
        Assert.Equal(8, (int)decls.Single(d => d.Name == "defaultnetplayers").Value);
        Assert.Equal(255, (int)decls.Single(d => d.Name == "defaultnethostteam").Value);
        Assert.Equal(255, (int)decls.Single(d => d.Name == "defaultnetjointeam").Value);
        Assert.Equal("auto", (string)decls.Single(d => d.Name == "language").Value);
        var mix = Assert.IsType<float>(decls.Single(d => d.Name == "ui_color_mix").Value);
        Assert.Equal(unchecked((int)0x3eb33333), BitConverter.SingleToInt32Bits(mix));
        Assert.DoesNotContain(decls, d => d.Name is "ui_generic" or "vid_preferbackend" or "vid_fullscreen");

        var pride = VmEngineInterface1356Slice.Pride;
        Assert.Equal("-pride", pride.Name);
        Assert.Equal("Launcher", pride.Section);
        Assert.Equal("Show pride colors", pride.Summary);
        Assert.Equal("", pride.Usage);
        Assert.Equal("Show pride colors in launcher.", pride.Details);
        Assert.False(pride.Advanced);
        Assert.Equal("GameUUID", VmEngineInterface1356Slice.UuidRngName);
        Assert.Equal(6, VmEngineInterface1356Slice.GsStartup);

        var engine = new VmEngineInterface1356Slice.Engine();
        Assert.Null(engine.StartWindow);
        Assert.Null(engine.LanguageChanged);
        Assert.Equal("", engine.EndoomName);
        Assert.False(engine.BatchRun);
        Assert.Equal(0f, engine.MenuBlur);
        Assert.True(engine.AppActive);
        Assert.Equal(0, engine.ChatMode);
        Assert.Equal(6, engine.GameState);
        Assert.False(engine.ToggleFullscreen);
        Assert.Equal(0, engine.Paused);
        Assert.False(engine.PauseExt);
        Assert.Equal("", engine.GameUuid);
        Assert.Equal("", engine.Startup.Name);
        Assert.Equal(0u, engine.Startup.FgColor);
        Assert.Equal(0u, engine.Startup.BkColor);
        Assert.Equal("", engine.Startup.Song);
        Assert.Equal("", engine.Startup.SteamAppId);
        Assert.Equal(0, engine.Startup.Type);
        Assert.Equal(-1, engine.Startup.LoadLights);
        Assert.Equal(-1, engine.Startup.LoadBrightmaps);
        Assert.Equal(-1, engine.Startup.LoadWidescreen);
        Assert.True(engine.Cvars.Bool("queryiwad"));
        Assert.True(engine.Cvars.Bool("saveargs"));
        Assert.False(engine.Cvars.Bool("savenetfile"));
        Assert.Equal("auto", engine.Cvars.Text("language"));
        Assert.Equal(8, engine.Cvars.Int("defaultnetplayers"));
        Assert.Equal(mix, engine.Cvars.Float("ui_color_mix"));
        Assert.False(engine.Cvars.Bool("ui_generic"));
        Assert.Equal(0, engine.Cvars.Int("vid_preferbackend"));
        Assert.False(engine.Cvars.Bool("vid_fullscreen"));
        Assert.Empty(engine.Cvars.Assigned);

        var log = new List<string>();
        engine.Cvars.Values["ui_generic"] = true;
        engine.Hooks.UpdateLanguage = text => log.Add("lang:" + text);
        engine.Hooks.UpdateGenericUi = on => log.Add(on ? "ui:1" : "ui:0");
        engine.LanguageChanged = text => log.Add("cb:" + text);
        VmEngineInterface1356Slice.OnLanguage(engine, "en", "old");
        Assert.Equal(new[] { "lang:en", "ui:1", "cb:en" }, log);

        log.Clear();
        engine.LanguageChanged = null;
        VmEngineInterface1356Slice.OnLanguage(engine, "de", "prev");
        Assert.Equal(new[] { "lang:de", "ui:1" }, log);

        engine.Hooks.UpdateLanguage = null;
        var missingLang = Assert.Throws<InvalidOperationException>(
            () => VmEngineInterface1356Slice.OnLanguage(engine, "fr", "prev"));
        Assert.Equal("UpdateLanguage", missingLang.Message);
        Assert.Equal(new[] { "lang:de", "ui:1" }, log);

        engine.Hooks.UpdateLanguage = text => log.Add("lang:" + text);
        engine.Hooks.UpdateGenericUi = null;
        var missingUi = Assert.Throws<InvalidOperationException>(
            () => VmEngineInterface1356Slice.OnLanguage(engine, "it", "prev"));
        Assert.Equal("UpdateGenericUI", missingUi.Message);
        Assert.Equal(new[] { "lang:de", "ui:1", "lang:it" }, log);
    }

    [Fact]
    public void SelectionSaveAndUuid()
    {
        Assert.Equal("", VmEngineInterface1356Slice.StripLeftRight(" \t\n\v\f\r "));
        Assert.Equal("a", VmEngineInterface1356Slice.StripLeftRight("\t\n a \v"));
        Assert.Equal("\u00A0a\u00A0", VmEngineInterface1356Slice.StripLeftRight("\u00A0a\u00A0"));
        Assert.Equal("", VmEngineInterface1356Slice.BuildStartupFileArgs(""));
        Assert.Equal("", VmEngineInterface1356Slice.BuildStartupFileArgs("| |"));
        var quoted = VmEngineInterface1356Slice.BuildStartupFileArgs("a||b");
        Assert.Equal("-file \"a\" -file \"b\"", quoted);
        string q = "\"";
        string bs = "\\";
        Assert.Equal(q + "a" + bs + q + q, VmEngineInterface1356Slice.QuoteStartupArg("a" + q));
        Assert.Equal(q + bs + q + q, VmEngineInterface1356Slice.QuoteStartupArg(q));
        Assert.Equal(0, VmEngineInterface1356Slice.CompareNoCase("Doom\0extra", "Doom"));
        Assert.Equal(0, VmEngineInterface1356Slice.CompareNoCase("DOOM", "doom"));
        Assert.NotEqual(0, VmEngineInterface1356Slice.CompareNoCase("Doom2", "Doom"));
        Assert.False(VmEngineInterface1356Slice.OpensWithText(""));
        Assert.False(VmEngineInterface1356Slice.OpensWithText("\0Doom"));

        var engine = new VmEngineInterface1356Slice.Engine();
        int seenBackend = -1;
        engine.Cvars.Values["vid_preferbackend"] = 4;
        engine.Cvars.Values["vid_fullscreen"] = false;
        engine.Cvars.Values["queryiwad"] = false;
        engine.Cvars.Values["language"] = "en";
        engine.Cvars.Values["saveargs"] = false;
        engine.Cvars.Values["defaultnetplayers"] = 4;
        engine.Cvars.Values["ui_colors"] = "trans";
        engine.Cvars.Values["ui_color_mix"] = 0.5f;
        engine.Hooks.MigrateRenderer = () =>
        {
            seenBackend = engine.Cvars.Int("vid_preferbackend");
            engine.Cvars.Values["vid_preferbackend"] = 7;
            engine.Cvars.Values["vid_fullscreen"] = true;
        };
        var wads = new List<VmEngineInterface1356Slice.Wad>
        {
            new() { Name = "Heretic" },
            new() { Name = "dOOm" },
            new() { Name = "DOOM2" },
        };
        engine.Cvars.Values["defaultiwad"] = "Doom";
        engine.Cvars.Values["defaultnetiwad"] = "doom2";
        string? checkedName = null;
        var args = new VmEngineInterface1356Slice.Args
        {
            CheckParm = name =>
            {
                checkedName = name;
                return -1;
            },
        };
        var sel = new VmEngineInterface1356Slice.Selection(engine, wads, args, 9);
        Assert.Equal(4, seenBackend);
        Assert.Equal("-pride", checkedName);
        Assert.Same(wads, sel.Wads);
        Assert.Equal(9, sel.DefaultStartFlags);
        Assert.False(sel.DefaultQueryIwad);
        Assert.Equal("en", sel.DefaultLanguage);
        Assert.Equal(7, sel.DefaultBackend);
        Assert.True(sel.DefaultFullscreen);
        Assert.Equal(1, sel.DefaultIwad);
        Assert.Equal(2, sel.DefaultNetIwad);
        Assert.False(sel.SaveArgs);
        Assert.Equal(4, sel.DefaultNetPlayers);
        Assert.Equal("list", sel.PrideColors);
        Assert.Equal(0.5f, sel.PrideMix);
        Assert.True(sel.IsNewRelease);
        Assert.True(sel.NotifyNewRelease);
        Assert.Equal(0, sel.DefaultFileLoadBehaviour);
        Assert.False(sel.NetStart);
        Assert.False(sel.Hosting);
        Assert.Equal("", sel.AdditionalNetArgs);
        Assert.Equal(255, sel.DefaultNetHostTeam);
        Assert.Equal(255, sel.DefaultNetJoinTeam);

        engine.Cvars.Values["defaultiwad"] = "nope";
        engine.Cvars.Values["defaultnetiwad"] = "";
        args.CheckParm = name => 0;
        var missed = new VmEngineInterface1356Slice.Selection(engine, wads, args, 0);
        Assert.Equal(0, missed.DefaultIwad);
        Assert.Equal("Heretic", missed.Wads![0].Name);
        Assert.Equal(0, missed.DefaultNetIwad);
        Assert.Equal("trans", missed.PrideColors);

        engine.Cvars.Values["defaultiwad"] = "\0Doom";
        var buried = new VmEngineInterface1356Slice.Selection(engine, wads, args, 0);
        Assert.Equal(0, buried.DefaultIwad);

        engine.Cvars.Values["defaultiwad"] = "Doom";
        wads[1].Name = "Doom\0extra";
        var embedded = new VmEngineInterface1356Slice.Selection(engine, wads, args, 0);
        Assert.Equal(1, embedded.DefaultIwad);
        wads[1].Name = "dOOm";

        bool parmCalled = false;
        engine.Hooks.MigrateRenderer = null;
        engine.Cvars.Values["defaultiwad"] = "Doom";
        var bare = new VmEngineInterface1356Slice.Args
        {
            CheckParm = name =>
            {
                parmCalled = true;
                return 0;
            },
        };
        var noMigrate = Assert.Throws<InvalidOperationException>(
            () => new VmEngineInterface1356Slice.Selection(engine, null, bare, 0));
        Assert.Equal("HCDE_MigrateRendererCvars", noMigrate.Message);
        Assert.False(parmCalled);

        engine.Hooks.MigrateRenderer = () => { };
        var noWads = Assert.Throws<NullReferenceException>(
            () => new VmEngineInterface1356Slice.Selection(engine, null, bare, 0));
        Assert.Equal("Wads", noWads.Message);
        Assert.False(parmCalled);

        engine.Cvars.Values["defaultiwad"] = "";
        engine.Cvars.Values["defaultnetiwad"] = "";
        var noArgs = Assert.Throws<NullReferenceException>(
            () => new VmEngineInterface1356Slice.Selection(engine, wads, null, 0));
        Assert.Equal("Args", noArgs.Message);

        bare.CheckParm = null;
        var noParm = Assert.Throws<InvalidOperationException>(
            () => new VmEngineInterface1356Slice.Selection(engine, wads, bare, 0));
        Assert.Equal("CheckParm", noParm.Message);

        var appended = new List<string>();
        var launch = new VmEngineInterface1356Slice.Args
        {
            CheckParm = name => 0,
            AppendRaw = appended.Add,
        };
        engine.Cvars.Values["vid_preferbackend"] = 3;
        var local = new VmEngineInterface1356Slice.Selection(engine, wads, launch, 1);
        local.DefaultIwad = 1;
        local.DefaultAddonFiles = " a.wad |  | b\"c.pk3 ";
        local.DefaultArgs = " -warp\u00001 ";
        local.DefaultLanguage = "  en  ";
        local.AdditionalNetArgs = "  extra  ";
        local.DefaultNetAddress = "  host  ";
        local.SaveArgs = true;
        local.DefaultQueryIwad = false;
        local.DefaultFullscreen = true;
        local.DefaultBackend = 3;
        Assert.Equal(1, local.SaveInfo(engine));
        Assert.Equal("en", local.DefaultLanguage);
        Assert.Equal("en", engine.Cvars.Text("language"));
        Assert.Equal("extra", local.AdditionalNetArgs);
        Assert.Equal("host", local.DefaultNetAddress);
        Assert.Equal("a.wad |  | b\"c.pk3", engine.Cvars.Text("defaultaddonfiles"));
        Assert.Equal("-warp", engine.Cvars.Text("defaultargs"));
        Assert.Equal("dOOm", engine.Cvars.Text("defaultiwad"));
        Assert.False(engine.Cvars.Bool("queryiwad"));
        Assert.True(engine.Cvars.Bool("vid_fullscreen"));
        Assert.True(engine.Cvars.Bool("saveargs"));
        Assert.Equal(
            new[]
            {
                "queryiwad", "language", "vid_fullscreen", "defaultiwad",
                "saveargs", "defaultaddonfiles", "defaultargs",
            },
            engine.Cvars.Assigned);
        Assert.Equal(
            new[] { "-file \"a.wad\" -file \"b\\\"c.pk3\" -warp\u00001" },
            appended);

        engine.Cvars.Assigned.Clear();
        appended.Clear();
        local.SaveArgs = false;
        local.DefaultAddonFiles = "a.wad";
        local.DefaultArgs = " -warp 1 ";
        local.SaveInfo(engine);
        Assert.Equal("", engine.Cvars.Text("defaultaddonfiles"));
        Assert.Equal("", engine.Cvars.Text("defaultargs"));
        Assert.False(engine.Cvars.Bool("saveargs"));
        Assert.Equal(new[] { "-file \"a.wad\" -warp 1" }, appended);

        engine.Cvars.Assigned.Clear();
        appended.Clear();
        local.DefaultAddonFiles = "";
        local.DefaultArgs = "";
        local.SaveInfo(engine);
        Assert.Empty(appended);

        engine.Cvars.Assigned.Clear();
        appended.Clear();
        engine.Cvars.Values["vid_preferbackend"] = 1;
        var net = new VmEngineInterface1356Slice.Selection(engine, wads, launch, 1);
        net.NetStart = true;
        net.Hosting = true;
        net.DefaultNetIwad = 2;
        net.SaveNetFile = true;
        net.SaveNetArgs = false;
        net.DefaultNetSaveFile = "  sav\u0000e  ";
        net.DefaultNetArgs = "  -dup\u00002  ";
        net.AdditionalNetArgs = " more ";
        net.DefaultNetPage = 2;
        net.DefaultNetPlayers = 6;
        net.DefaultNetHostPort = 5029;
        net.DefaultNetTicDup = 1;
        net.DefaultNetGameMode = 3;
        net.DefaultNetAltDm = true;
        net.DefaultNetHostTeam = 1;
        net.DefaultNetExtraTic = true;
        net.DefaultBackend = 9;
        net.DefaultLanguage = "en";
        net.DefaultQueryIwad = true;
        net.DefaultFullscreen = false;
        Assert.Equal(2, net.SaveInfo(engine));
        Assert.Equal("DOOM2", engine.Cvars.Text("defaultnetiwad"));
        Assert.Equal("sav", engine.Cvars.Text("defaultnetsavefile"));
        Assert.Equal("", engine.Cvars.Text("defaultnetargs"));
        Assert.True(engine.Cvars.Bool("savenetfile"));
        Assert.False(engine.Cvars.Bool("savenetargs"));
        Assert.Equal(2, engine.Cvars.Int("defaultnetpage"));
        Assert.Equal(6, engine.Cvars.Int("defaultnetplayers"));
        Assert.Equal(5029, engine.Cvars.Int("defaultnethostport"));
        Assert.Equal(1, engine.Cvars.Int("defaultnetticdup"));
        Assert.Equal(3, engine.Cvars.Int("defaultnetgamemode"));
        Assert.True(engine.Cvars.Bool("defaultnetaltdm"));
        Assert.Equal(1, engine.Cvars.Int("defaultnethostteam"));
        Assert.True(engine.Cvars.Bool("defaultnetextratic"));
        Assert.Equal(9, engine.Cvars.Int("vid_preferbackend"));
        Assert.Equal(
            new[]
            {
                "queryiwad", "language", "vid_fullscreen", "vid_preferbackend",
                "savenetfile", "savenetargs", "defaultnetiwad", "defaultnetpage",
                "defaultnetsavefile", "defaultnetargs", "defaultnetplayers",
                "defaultnethostport", "defaultnetticdup", "defaultnetgamemode",
                "defaultnetaltdm", "defaultnethostteam", "defaultnetextratic",
            },
            engine.Cvars.Assigned);
        Assert.Equal(new[] { "-dup\u00002", "more" }, appended);
        Assert.DoesNotContain("defaultnetaddress", engine.Cvars.Assigned);

        engine.Cvars.Assigned.Clear();
        appended.Clear();
        net.Hosting = false;
        net.DefaultBackend = 9;
        net.DefaultNetAddress = "  10.0.0.1 ";
        net.DefaultNetJoinPort = 1993;
        net.DefaultNetJoinTeam = 2;
        net.DefaultNetArgs = "";
        net.AdditionalNetArgs = "";
        net.SaveInfo(engine);
        Assert.Equal("10.0.0.1", engine.Cvars.Text("defaultnetaddress"));
        Assert.Equal(1993, engine.Cvars.Int("defaultnetjoinport"));
        Assert.Equal(2, engine.Cvars.Int("defaultnetjointeam"));
        Assert.Empty(appended);
        Assert.DoesNotContain("defaultnetplayers", engine.Cvars.Assigned);

        engine.Cvars.Assigned.Clear();
        local.NetStart = false;
        local.DefaultIwad = 2;
        local.Wads = new List<VmEngineInterface1356Slice.Wad> { new() { Name = "Only" } };
        var oob = Assert.Throws<IndexOutOfRangeException>(() => local.SaveInfo(engine));
        Assert.Equal("Wads", oob.Message);
        Assert.DoesNotContain("defaultiwad", engine.Cvars.Assigned);
        Assert.Contains("vid_fullscreen", engine.Cvars.Assigned);

        engine.Cvars.Assigned.Clear();
        local.Wads = null;
        var nullWads = Assert.Throws<NullReferenceException>(() => local.SaveInfo(engine));
        Assert.Equal("Wads", nullWads.Message);

        engine.Cvars.Assigned.Clear();
        local.Wads = wads;
        local.DefaultIwad = 0;
        local.DefaultAddonFiles = "a.wad";
        local.DefaultArgs = "";
        local.Args = new VmEngineInterface1356Slice.Args { CheckParm = name => 0 };
        var noAppend = Assert.Throws<InvalidOperationException>(() => local.SaveInfo(engine));
        Assert.Equal("AppendRawArgsString", noAppend.Message);
        Assert.Contains("defaultaddonfiles", engine.Cvars.Assigned);

        var draws = new List<uint>();
        engine.GameUuid = "keep";
        engine.Hooks.NextBound = bound =>
        {
            draws.Add(bound);
            return 0;
        };
        engine.Hooks.Next32 = () => 0;
        Assert.Equal(
            "00000000-0000-4000-9000-000000000000",
            VmEngineInterface1356Slice.GenerateUuid(engine));
        Assert.Equal(
            new uint[]
            {
                VmEngineInterface1356Slice.UuidShortBound,
                VmEngineInterface1356Slice.UuidNibbleBound,
                VmEngineInterface1356Slice.UuidNibbleBound,
                VmEngineInterface1356Slice.UuidShortBound,
            },
            draws);
        Assert.Equal("keep", engine.GameUuid);

        draws.Clear();
        var boundValues = new Queue<uint>(new uint[] { 0x1111, 0x222, 0x33, 0x4444 });
        var wordValues = new Queue<uint>(new uint[] { 0xAAAAAAAAu, 0xBBBBBBBBu });
        engine.Hooks.NextBound = bound =>
        {
            draws.Add(bound);
            return boundValues.Dequeue();
        };
        engine.Hooks.Next32 = wordValues.Dequeue;
        Assert.Equal(
            "BBBBBBBB-4444-4033-9222-AAAAAAAA1111",
            VmEngineInterface1356Slice.GenerateUuid(engine));
        Assert.Empty(boundValues);
        Assert.Empty(wordValues);

        var wide = new Queue<uint>(new uint[] { 0, 0, 0x1000, 0 });
        engine.Hooks.NextBound = bound => wide.Dequeue();
        engine.Hooks.Next32 = () => 0;
        Assert.Equal(
            "00000000-0000-41000-9000-000000000000",
            VmEngineInterface1356Slice.GenerateUuid(engine));

        int wordCalls = 0;
        engine.Hooks.Next32 = () =>
        {
            wordCalls++;
            return 0;
        };
        engine.Hooks.NextBound = null;
        var noBound = Assert.Throws<InvalidOperationException>(
            () => VmEngineInterface1356Slice.GenerateUuid(engine));
        Assert.Equal("GenRand32BoundExclusive", noBound.Message);
        Assert.Equal(0, wordCalls);

        draws.Clear();
        engine.Hooks.NextBound = bound =>
        {
            draws.Add(bound);
            return 1;
        };
        engine.Hooks.Next32 = null;
        var noWord = Assert.Throws<InvalidOperationException>(
            () => VmEngineInterface1356Slice.GenerateUuid(engine));
        Assert.Equal("GenRand32", noWord.Message);
        Assert.Equal(new uint[] { 65535 }, draws);
    }
}
