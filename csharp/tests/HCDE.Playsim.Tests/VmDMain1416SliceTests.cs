using HCDE.Playsim;
namespace HCDE.Playsim.Tests;
public class VmDMain1416SliceTests
{
    [Fact]
    public void InitCollectsAndAutoloadPaths()
    {
        var open = new VmDMain1416Slice.State();
        VmDMain1416Slice.DoomInit(open);
        Assert.Equal(1, open.Responses);
        Assert.Equal(1, open.Defaults);
        Assert.Equal(VmDMain1416Slice.Startup, open.GameState);
        Assert.Equal(new (string, string?)[]
        {
            ("-deh", ".deh"), ("-bex", ".bex"), ("-exec", ".cfg"), ("-playdemo", ".lmp"), ("-file", null), ("-optfile", null),
        }, open.Gathers);
        Assert.Equal(new[] { "M_LoadDefaults: Load system defaults.\n" }, open.Lines);
        var batch = new VmDMain1416Slice.State { Batch = true };
        VmDMain1416Slice.DoomInit(batch);
        Assert.Empty(batch.Lines);
        Assert.Equal(1, batch.Defaults);
        Assert.Equal(VmDMain1416Slice.Startup, batch.GameState);
        var share = new VmDMain1416Slice.State
        {
            Flags = VmDMain1416Slice.Shareware,
            LoadLights = 1,
            AutoloadLights = true,
        };
        VmDMain1416Slice.AddAutoloadFiles(share, "doom2");
        Assert.Equal("doom2.", share.LumpFilter);
        Assert.Empty(share.Searches);
        Assert.Empty(share.Files);
        Assert.Empty(share.Folders);
        Assert.Empty(share.Sections);
        var blocked = new VmDMain1416Slice.State { LoadLights = 2, AutoloadLights = false, LoadWidescreen = 0, AutoloadWidescreen = true };
        blocked.Args.Add("-nolights");
        blocked.FindFile = (name, _) => name;
        VmDMain1416Slice.AddAutoloadFiles(blocked, ".");
        Assert.Equal("..", blocked.LumpFilter);
        Assert.Equal(new[] { ("zvox.wad", (string?)null, false, "GameConfig") }, blocked.Searches);
        Assert.Equal(new[] { ("zvox.wad", true, -1, "GameConfig", true) }, blocked.Files);
        Assert.Equal(new[] { ("Global.Autoload", "*.wad", "GameConfig", true) }, blocked.Sections);
        var unix = new VmDMain1416Slice.State
        {
            Unix = true,
            Config = "Cfg",
            ShareDir = "/usr/local/share/",
            LoadLights = -1,
            AutoloadLights = true,
            LoadBrightmaps = 0,
            AutoloadBrightmaps = true,
            LoadWidescreen = 1,
            AutoloadWidescreen = false,
        };
        bool created = false;
        string? niced = null;
        unix.AppData = create =>
        {
            created = create;
            return "/home/me";
        };
        unix.Nice = path =>
        {
            niced = path;
            return "/nice/skins";
        };
        unix.FindFile = (name, first) => name switch
        {
            "lights.pk3" when first => "L",
            "game_widescreen_gfx.pk3" when first => "W",
            "zvox.wad" when !first => "Z",
            _ => null,
        };
        VmDMain1416Slice.AddAutoloadFiles(unix, "doom.id");
        Assert.True(created);
        Assert.Equal("/home/me/skins", niced);
        Assert.Equal(new[]
        {
            ("lights.pk3", (string?)null, true, "Cfg"),
            ("game_widescreen_gfx.pk3", null, true, "Cfg"),
            ("zvox.wad", null, false, "Cfg"),
        }, unix.Searches);
        Assert.Equal(new[]
        {
            ("L", true, -1, "Cfg", true),
            ("W", true, -1, "Cfg", true),
            ("Z", true, -1, "Cfg", true),
        }, unix.Files);
        Assert.Equal(new[]
        {
            ("/usr/local/share/skins", "*.wad", "Cfg", true),
            ("/usr/local/share/skins", "*.pk3", "Cfg", true),
            ("/nice/skins", "*.wad", "Cfg", true),
            ("/nice/skins", "*.pk3", "Cfg", true),
        }, unix.Folders);
        Assert.Equal(new[]
        {
            ("Global.Autoload", "*.wad", "Cfg", true),
            ("doom.Autoload", "*.wad", "Cfg", true),
            ("doom.id.Autoload", "*.wad", "Cfg", true),
        }, unix.Sections);
        var local = new VmDMain1416Slice.State { ProgDir = "C:/g/", LoadLights = 1, AutoloadLights = false };
        int apps = 0;
        local.AppData = _ =>
        {
            apps++;
            return "nope";
        };
        local.FindFile = (_, _) => null;
        local.Args.Add("-host");
        VmDMain1416Slice.AddAutoloadFiles(local, "");
        Assert.Equal(0, apps);
        Assert.Equal(new[] { ("lights.pk3", (string?)null, true, "GameConfig") }, local.Searches);
        Assert.Empty(local.Files);
        Assert.Empty(local.Folders);
        Assert.Equal(".", local.LumpFilter);
        var joined = new VmDMain1416Slice.State { Dedicated = true, LoadLights = 1 };
        joined.Args.Add("-noextras");
        joined.Args.Add("-dedicatedjoin");
        joined.FindFile = (name, _) => name;
        VmDMain1416Slice.AddAutoloadFiles(joined, "doom2");
        Assert.Empty(joined.Searches);
        Assert.Empty(joined.Folders);
        var off = new VmDMain1416Slice.State { DisableAutoload = true, LoadBrightmaps = 1 };
        off.Args.Add("-noautoload");
        off.FindFile = (name, _) => name;
        VmDMain1416Slice.AddAutoloadFiles(off, "a");
        Assert.Equal(new[] { ("brightmaps.pk3", (string?)null, true, "GameConfig") }, off.Searches);
        Assert.Empty(off.Sections);
    }
    [Fact]
    public void CommandLineModesWarpAndStatus()
    {
        var stale = Line();
        stale.Args[0] = "-nomonsters";
        stale.Dmflags = 1;
        stale.Devparm = true;
        stale.SetMap = true;
        stale.Turbo = 9f;
        VmDMain1416Slice.CheckCmdLine(stale);
        Assert.Equal(1, stale.Dmflags);
        Assert.False(stale.Devparm);
        Assert.False(stale.SetMap);
        Assert.Equal(9f, stale.Turbo);
        Assert.Equal("E1M1", stale.StartMap);
        Assert.Equal(-1, stale.SetSkill);
        var bits = Line("-NOMONSTERS", "-respawn", "-FAST");
        bits.Dmflags = 1;
        VmDMain1416Slice.CheckCmdLine(bits);
        Assert.Equal(1 | VmDMain1416Slice.NoMonsters | VmDMain1416Slice.MonstersRespawn | VmDMain1416Slice.FastMonsters, bits.Dmflags);
        Assert.Empty(bits.Traces);
        var held = Line("-altdeath");
        held.Deathmatch = 1;
        held.SvGametype = 1;
        held.Dmflags = 1;
        VmDMain1416Slice.CheckCmdLine(held);
        Assert.Empty(held.Traces);
        Assert.Equal(1, held.Deathmatch);
        Assert.Equal(1 | VmDMain1416Slice.ItemsRespawn, held.Dmflags);
        Assert.Equal(0, held.Dmflags & VmDMain1416Slice.WeaponsStay);
        var both = Line("-altdeath", "-deathmatch");
        both.Gametic = 9;
        both.Room = 3;
        both.LevelName = "MAP09";
        VmDMain1416Slice.CheckCmdLine(both);
        Assert.Equal(new[]
        {
            "deathmatch 0 -> 1 reason=cmdline-altdeath gametic=9",
            "sv_gametype 0 -> 1 reason=cmdline-altdeath gametic=9 room=3 map=MAP09",
        }, both.Traces);
        Assert.Equal(0, both.Dmflags & VmDMain1416Slice.WeaponsStay);
        var coop = Line("-coop");
        coop.Deathmatch = 2;
        coop.Teamplay = 1;
        coop.SvGametype = 3;
        coop.Dmflags3 = 8;
        coop.GameType = 6;
        VmDMain1416Slice.CheckCmdLine(coop);
        Assert.Equal(new[]
        {
            "deathmatch 2 -> 0 reason=cmdline-coop gametic=0",
            "teamplay 1 -> 0 reason=cmdline-coop gametic=0",
            "sv_gametype 3 -> 0 reason=cmdline-coop gametic=0 room=0 map=<none>",
        }, coop.Traces);
        Assert.Equal(VmDMain1416Slice.NoCoopWeapons, coop.Dmflags);
        Assert.Equal(8 | VmDMain1416Slice.NoPlayerClip | VmDMain1416Slice.ShareKeys | VmDMain1416Slice.RememberWeapon | VmDMain1416Slice.LocalItems, coop.Dmflags3);
        var hexen = Line("-coop");
        hexen.GameType = VmDMain1416Slice.Hexen;
        hexen.Flags = VmDMain1416Slice.Mapxx;
        VmDMain1416Slice.CheckCmdLine(hexen);
        Assert.Equal(VmDMain1416Slice.NoPlayerClip | VmDMain1416Slice.ShareKeys | VmDMain1416Slice.RememberWeapon, hexen.Dmflags3);
        Assert.Equal(0, hexen.Dmflags3 & VmDMain1416Slice.LocalItems);
        Assert.Equal("&wt@01", hexen.StartMap);
        var invasion = Line("+SET", "SV_GAMETYPE", "4abc", "-coop");
        invasion.Deathmatch = 2;
        invasion.Dmflags3 = 8;
        VmDMain1416Slice.CheckCmdLine(invasion);
        Assert.Empty(invasion.Traces);
        Assert.Equal(2, invasion.Deathmatch);
        Assert.Equal(8, invasion.Dmflags3);
        Assert.Equal(0, invasion.Dmflags);
        var typed = Line("+Sv_Gametype");
        typed.Deathmatch = 5;
        VmDMain1416Slice.CheckCmdLine(typed);
        Assert.Equal(5, typed.Deathmatch);
        Assert.Empty(typed.Traces);
        var first = Line("+sv_gametype", "0", "-coop");
        VmDMain1416Slice.CheckCmdLine(first);
        Assert.Equal(VmDMain1416Slice.NoCoopWeapons, first.Dmflags);
        Assert.Equal(VmDMain1416Slice.LocalItems, first.Dmflags3 & VmDMain1416Slice.LocalItems);
        var solo = Line();
        solo.Deathmatch = 2;
        solo.Teamplay = 1;
        solo.SvGametype = 4;
        solo.StartScreen = true;
        VmDMain1416Slice.CheckCmdLine(solo);
        Assert.Equal(0, solo.Deathmatch);
        Assert.Equal(0, solo.Teamplay);
        Assert.Equal(0, solo.SvGametype);
        Assert.Empty(solo.Status);
        Assert.Equal(new[]
        {
            "deathmatch 2 -> 0 reason=cmdline-solo-default gametic=0",
            "teamplay 1 -> 0 reason=cmdline-solo-default gametic=0",
            "sv_gametype 4 -> 0 reason=cmdline-solo-default gametic=0 room=0 map=<none>",
        }, solo.Traces);
        var host = Line("-host");
        host.Deathmatch = 2;
        host.StartScreen = true;
        VmDMain1416Slice.CheckCmdLine(host);
        Assert.Equal(2, host.Deathmatch);
        Assert.Empty(host.Traces);
        Assert.Equal(new[] { "DeathMatch..." }, host.Status);
        var dedicated = Line();
        dedicated.Dedicated = true;
        dedicated.Deathmatch = 2;
        VmDMain1416Slice.CheckCmdLine(dedicated);
        Assert.Equal(2, dedicated.Deathmatch);
        Assert.Empty(dedicated.Traces);
        var screen = Line("-deathmatch", "-nomonsters", "-respawn", "-fast", "-skill", "3", "-warp", "2", "5", "-devparm", "-turbo", "150");
        screen.DevText = "Devs";
        screen.StartScreen = true;
        screen.Dmflags = 1;
        screen.Gametic = 4;
        screen.Room = 8;
        screen.LevelName = "E2M5";
        VmDMain1416Slice.CheckCmdLine(screen);
        Assert.Equal(new[]
        {
            "Checking cmd-line parameters...\n",
            "Devs\n",
            "turbo scale: 150%\n",
        }, screen.Lines);
        Assert.Equal(new[]
        {
            "deathmatch 0 -> 1 reason=cmdline-deathmatch gametic=4",
            "sv_gametype 0 -> 1 reason=cmdline-deathmatch gametic=4 room=8 map=E2M5",
        }, screen.Traces);
        Assert.Equal(1 | VmDMain1416Slice.NoMonsters | VmDMain1416Slice.MonstersRespawn | VmDMain1416Slice.FastMonsters | VmDMain1416Slice.WeaponsStay | VmDMain1416Slice.ItemsRespawn, screen.Dmflags);
        Assert.Equal("E2M5", screen.StartMap);
        Assert.True(screen.SetMap);
        Assert.True(screen.Autostart);
        Assert.Equal(2, screen.SetSkill);
        Assert.Equal(150f, screen.Turbo);
        Assert.Equal(new[]
        {
            "DeathMatch...",
            "No Monsters...",
            "Respawning...",
            "Warp to map E2M5, Skill 3 ",
        }, screen.Status);
        var quiet = Line("-turbo", "150");
        quiet.StartScreen = false;
        VmDMain1416Slice.CheckCmdLine(quiet);
        Assert.Empty(quiet.Status);
        Assert.Equal(150f, quiet.Turbo);
        var batched = Line("-deathmatch");
        batched.Batch = true;
        batched.StartScreen = true;
        VmDMain1416Slice.CheckCmdLine(batched);
        Assert.Empty(batched.Lines);
        Assert.Equal(new[] { "DeathMatch..." }, batched.Status);
        var clock = Line("-timer", "1.5", "-avg", "yes");
        VmDMain1416Slice.CheckCmdLine(clock);
        Assert.Equal(new[]
        {
            "Checking cmd-line parameters...\n",
            "Levels will end after 1.5 minutes.\n",
            "Austin Virtual Gaming: Levels will end after 20 minutes\n",
        }, clock.Lines);
        Assert.Equal(20f, clock.Timelimit);
        var minute = Line("-timer", "1");
        VmDMain1416Slice.CheckCmdLine(minute);
        Assert.Contains("Levels will end after 1 minute.\n", minute.Lines);
        Assert.Equal(1f, minute.Timelimit);
        var bare = Line("-avg");
        bare.Timelimit = 7f;
        VmDMain1416Slice.CheckCmdLine(bare);
        Assert.Equal(7f, bare.Timelimit);
        Assert.Equal(new[] { "Checking cmd-line parameters...\n" }, bare.Lines);
        var scale = Line("+MAP", "MAP07", "-turbo", "2.6");
        scale.MapData = name => name == "MAP07";
        VmDMain1416Slice.CheckCmdLine(scale);
        Assert.Equal(new[] { "hcde", "-turbo", "2.6" }, scale.Args);
        Assert.Equal("MAP07", scale.StartMap);
        Assert.False(scale.FromLauncher);
        Assert.Equal((float)2.6, scale.Turbo);
        Assert.Contains("turbo scale: 3%\n", scale.Lines);
        var launcher = Line("+map", "e1m1");
        launcher.MapData = name => name == "e1m1";
        VmDMain1416Slice.CheckCmdLine(launcher);
        Assert.Equal("e1m1", launcher.StartMap);
        Assert.True(launcher.FromLauncher);
        Assert.True(launcher.SetMap);
        var remapped = Line("+map", "MAP01");
        remapped.Resolve = _ => "MAP15";
        remapped.MapData = name => name == "MAP01" || name == "MAP15";
        VmDMain1416Slice.CheckCmdLine(remapped);
        Assert.Equal("MAP15", remapped.StartMap);
        Assert.False(remapped.FromLauncher);
        Assert.Contains("HCDE: remapped startup map MAP01 -> MAP15 for active mod compatibility.\n", remapped.Lines);
        var rejected = Line("+map", "MAP01");
        rejected.Resolve = _ => "NOPE";
        rejected.MapData = name => name == "MAP01";
        VmDMain1416Slice.CheckCmdLine(rejected);
        Assert.Equal("MAP01", rejected.StartMap);
        Assert.False(rejected.FromLauncher);
        Assert.Equal(new[] { "Checking cmd-line parameters...\n" }, rejected.Lines);
        var missing = Line("-warp", "2", "5", "+map", "NOPE");
        missing.MapData = _ => false;
        VmDMain1416Slice.CheckCmdLine(missing);
        Assert.Equal("E2M5", missing.StartMap);
        Assert.True(missing.SetMap);
        Assert.True(missing.Autostart);
        Assert.Contains("Can't find map NOPE\n", missing.Lines);
        var episode = Line("-warp", "4");
        VmDMain1416Slice.CheckCmdLine(episode);
        Assert.Equal("E1M4", episode.StartMap);
        Assert.True(episode.SetMap);
        var wide = Line("-warp", "2", "11");
        VmDMain1416Slice.CheckCmdLine(wide);
        Assert.Equal("E1M2", wide.StartMap);
        var zero = Line("-warp", "5", "0");
        VmDMain1416Slice.CheckCmdLine(zero);
        Assert.Equal("E1M5", zero.StartMap);
        var maps = Line("-warp", " 8", "3");
        maps.Flags = VmDMain1416Slice.Mapxx;
        VmDMain1416Slice.CheckCmdLine(maps);
        Assert.Equal("MAP08", maps.StartMap);
        var hanging = Line("-warp");
        VmDMain1416Slice.CheckCmdLine(hanging);
        Assert.False(hanging.SetMap);
        Assert.Equal("E1M1", hanging.StartMap);
        var skill = Line("-skill", "-warp");
        VmDMain1416Slice.CheckCmdLine(skill);
        Assert.Equal(-1, skill.SetSkill);
        Assert.False(skill.Autostart);
        var stored = Line();
        stored.StoredWarp = "E3M4";
        stored.StartScreen = true;
        VmDMain1416Slice.CheckCmdLine(stored);
        Assert.Equal("E1M1", stored.StartMap);
        Assert.False(stored.SetMap);
        Assert.True(stored.Autostart);
        Assert.Equal(new[] { "Warp to map E1M1, Skill 0 " }, stored.Status);
        var emptied = Line("+map", "");
        VmDMain1416Slice.CheckCmdLine(emptied);
        Assert.Equal(new[] { "hcde" }, emptied.Args);
        Assert.False(emptied.SetMap);
        var switched = Line("+map", "-file");
        VmDMain1416Slice.CheckCmdLine(switched);
        Assert.Equal(new[] { "hcde", "-file" }, switched.Args);
        Assert.False(switched.SetMap);
    }
    static VmDMain1416Slice.State Line(params string[] args)
    {
        var state = new VmDMain1416Slice.State();
        state.Args.AddRange(args);
        return state;
    }
}
