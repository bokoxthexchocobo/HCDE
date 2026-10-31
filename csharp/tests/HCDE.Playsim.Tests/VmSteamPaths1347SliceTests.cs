using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmSteamPaths1347SliceTests
{
    [Fact]
    public void QueryAndGog()
    {
        var store = new RegStore1347();
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, VmSteamPaths1347Slice.HiveLocalMachine, "missing", "Path", out var missing));
        Assert.Equal("", missing);
        Assert.Equal("HKLM:missing", Assert.Single(store.Opens));

        var wrong = new RegKey1347 { Type = 4, Text = "C:/Nope" };
        store.Keys[("HKLM", "wrong")] = wrong;
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "wrong", "Path", out _));
        Assert.True(wrong.Closed);
        Assert.Equal(0, wrong.ReadQueries);
        Assert.Equal(1, wrong.SizeQueries);

        var zero = new RegKey1347 { PathLen = 0, Text = "C:/Zero" };
        store.Keys[("HKLM", "zero")] = zero;
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "zero", "Path", out _));
        Assert.Equal(0, zero.ReadQueries);
        Assert.True(zero.Closed);

        var sizeFail = new RegKey1347 { SizeOk = false, Text = "C:/Size" };
        store.Keys[("HKLM", "size")] = sizeFail;
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "size", "Path", out _));
        Assert.Equal(0, sizeFail.ReadQueries);

        var readFail = new RegKey1347 { ReadOk = false, Text = "C:/Read" };
        store.Keys[("HKLM", "read")] = readFail;
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "read", "Path", out var unread));
        Assert.Equal("", unread);
        Assert.Equal(1, readFail.ReadQueries);
        Assert.True(readFail.Closed);

        var empty = new RegKey1347 { Text = "" };
        store.Keys[("HKLM", "empty")] = empty;
        Assert.False(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "empty", "Path", out _));
        Assert.Equal(1, empty.ReadQueries);

        var ok = new RegKey1347 { Text = "C:/Doom" };
        store.Keys[("HKLM", "ok")] = ok;
        Assert.True(VmSteamPaths1347Slice.QueryPathKey(store, "HKLM", "ok", "Path", out var found));
        Assert.Equal("C:/Doom", found);
        Assert.Equal("Path", ok.LastName);
        Assert.True(ok.Closed);

        var paths = new VmSteamPaths1347 { Win64 = true };
        Put(paths, "1435827232", "C:/Ultimate");
        Put(paths, "2015545325", "C:/Enhanced");
        Put(paths, "1435848814", "C:/Doom2");
        Put(paths, "1435848742", "C:/Final");
        Put(paths, "1247951670", "C:/Hexen");
        var gog = paths.GogPaths();
        Assert.Equal(new[]
        {
            "C:/Ultimate",
            "C:/Enhanced/DOOM_Data/StreamingAssets",
            "C:/Doom2/doom2",
            "C:/Final/TNT",
            "C:/Final/Plutonia",
            "C:/Hexen",
        }, gog);
        Assert.Contains(paths.Store.Opens, item => item == "HKLM:Software\\Wow6432Node\\GOG.com\\Games\\1435827232");
        Assert.Contains(paths.Store.Opens, item => item == "HKLM:Software\\Wow6432Node\\GOG.com\\Games\\1426071866");
        Assert.DoesNotContain(gog, item => item.Contains("master/wads", StringComparison.Ordinal));

        var narrow = new VmSteamPaths1347 { Win64 = false };
        Put(narrow, "1432899949", "C:/Strife");
        Assert.Equal(new[] { "C:/Strife" }, narrow.GogPaths());
        Assert.Contains(narrow.Store.Opens, item => item == "HKLM:Software\\GOG.com\\Games\\1432899949");
        Assert.DoesNotContain(narrow.Store.Opens, item => item.Contains("Wow6432Node", StringComparison.Ordinal));
    }

    [Fact]
    public void SteamAndBethesda()
    {
        var user = new VmSteamPaths1347();
        user.Store.Keys[("HKCU", "Software\\Valve\\Steam")] = new RegKey1347 { Text = "C:/Steam" };
        Assert.Equal("C:/Steam", user.SteamPath());
        Assert.Equal(new[] { "HKCU:Software\\Valve\\Steam" }, user.Store.Opens);
        Assert.Equal("SteamPath", user.Store.Keys[("HKCU", "Software\\Valve\\Steam")].LastName);

        var machine = new VmSteamPaths1347();
        machine.Store.Keys[("HKLM", "Software\\Valve\\Steam")] = new RegKey1347 { Text = "D:/Steam" };
        Assert.Equal("D:/Steam", machine.SteamPath());
        Assert.Equal(
            new[] { "HKCU:Software\\Valve\\Steam", "HKLM:Software\\Valve\\Steam" },
            machine.Store.Opens);
        Assert.Equal("InstallPath", machine.Store.Keys[("HKLM", "Software\\Valve\\Steam")].LastName);

        var none = new VmSteamPaths1347();
        Assert.Equal("", none.SteamPath());
        Assert.Equal(2, none.Store.Opens.Count);

        var blank = new VmSteamPaths1347();
        blank.Store.Keys[("HKCU", "Software\\Valve\\Steam")] = new RegKey1347 { Text = "" };
        blank.Store.Keys[("HKLM", "Software\\Valve\\Steam")] = new RegKey1347 { Text = "E:/Steam" };
        Assert.Equal("E:/Steam", blank.SteamPath());

        var missing = new VmSteamPaths1347();
        Assert.Empty(missing.BethesdaPaths());
        Assert.Equal(
            "HKLM:Software\\Wow6432Node\\Bethesda Softworks\\Bethesda.net",
            Assert.Single(missing.Store.Opens));

        var bethesda = new VmSteamPaths1347();
        bethesda.Store.Keys[(VmSteamPaths1347Slice.HiveLocalMachine, bethesda.BethesdaRoot)] = new RegKey1347 { Text = "C:/Bethesda" };
        var dirs = bethesda.BethesdaPaths();
        Assert.Equal(8, dirs.Count);
        Assert.Equal("C:/Bethesda/games/DOOM_Classic_2019/base", dirs[0]);
        Assert.Equal("C:/Bethesda/games/DOOM_Classic_2019/rerelease/DOOM_Data/StreamingAssets", dirs[1]);
        Assert.Equal("C:/Bethesda/games/DOOM_II_Classic_2019/base", dirs[2]);
        Assert.Equal("C:/Bethesda/games/DOOM_II_Classic_2019/rerelease/DOOM II_Data/StreamingAssets", dirs[3]);
        Assert.Equal("C:/Bethesda/games/DOOM 3 BFG Edition/base/wads", dirs[4]);
        Assert.Equal("C:/Bethesda/games/Heretic Shadow of the Serpent Riders/base", dirs[5]);
        Assert.Equal("C:/Bethesda/games/Hexen/base", dirs[6]);
        Assert.Equal("C:/Bethesda/games/Hexen Deathkings of the Dark Citadel/base", dirs[7]);
        Assert.DoesNotContain(dirs, item => item.Contains("Ultimate DOOM", StringComparison.Ordinal));
        Assert.DoesNotContain(dirs, item => item.Contains("Doom Eternal", StringComparison.Ordinal));
        Assert.Equal("installLocation", bethesda.Store.Keys[(VmSteamPaths1347Slice.HiveLocalMachine, bethesda.BethesdaRoot)].LastName);

        var bethesda32 = new VmSteamPaths1347 { Win64 = false };
        Assert.Empty(bethesda32.BethesdaPaths());
        Assert.Equal("HKLM:Software\\Bethesda Softworks\\Bethesda.net", Assert.Single(bethesda32.Store.Opens));
    }

    static void Put(VmSteamPaths1347 paths, string gameId, string text)
    {
        paths.Store.Keys[(VmSteamPaths1347Slice.HiveLocalMachine, paths.GogRoot + "\\" + gameId)] = new RegKey1347 { Text = text };
    }
}
