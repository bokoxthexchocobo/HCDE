using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmUnixPaths1352SliceTests
{
    [Fact]
    public void EnvCacheAndDocuments()
    {
        var apple = New(UnixKind1352.Apple);
        apple.Env["XDG_CONFIG_HOME"] = "/tmp/custom";
        Assert.Equal("$HOME/.config", apple.Slice.Config());
        Assert.Equal("$HOME/.cache", apple.Slice.Cache());
        Assert.Equal("$HOME/.local/share", apple.Slice.Data());
        Assert.Equal("$HOME/Pictures", apple.Slice.Pictures());
        Assert.Equal(0, apple.EnvReads);

        var unix = New(UnixKind1352.Unix);
        unix.Env["XDG_CONFIG_HOME"] = "";
        Assert.Equal("", unix.Slice.Config());
        unix.Env["XDG_CONFIG_HOME"] = "/later";
        Assert.Equal("", unix.Slice.Config());
        Assert.Equal("$HOME/.local/share", unix.Slice.Data());
        unix.Env["XDG_DATA_HOME"] = "/data";
        Assert.Equal("$HOME/.local/share", unix.Slice.Data());
        Assert.Equal(2, unix.EnvReads);

        var haiku = New(UnixKind1352.Haiku);
        Assert.Equal("$HOME/config/settings", haiku.Slice.Config());
        Assert.Equal("$HOME/config/cache", haiku.Slice.Cache());
        Assert.Equal("$HOME/config/non-packaged/data", haiku.Slice.Data());
        Assert.Equal("Pictures", Assert.Throws<InvalidOperationException>(() => haiku.Slice.Pictures()).Message);
        Assert.Equal("$HOME/config/settings/hcde/", haiku.Slice.Documents());
        Assert.Equal("$HOME/config/settings/hcde/savegames/", haiku.Slice.Savegames());
        haiku.Slice.Netgame = true;
        Assert.Equal("$HOME/config/settings/hcde/savegames/netgame/", haiku.Slice.Savegames());
        Assert.Equal("$HOME/config/settings/hcde/demo/", haiku.Slice.Demo());
        Assert.Equal("$HOME/config/settings/hcde/screenshots", haiku.Slice.Screenshots());
        haiku.NiceCalls.Clear();
        haiku.Slice.Nice = path =>
        {
            haiku.NiceCalls.Add(path);
            return path + "!";
        };
        Assert.Equal("$HOME/config/settings/hcde/screenshots!", haiku.Slice.Screenshots());
        Assert.Equal(new[] { "$HOME/config/settings/hcde/screenshots" }, haiku.NiceCalls);

        unix.Slice.Nice = s => s;
        Assert.Equal("$HOME/.local/share/games/hcde", unix.Slice.AppData(false));
        Assert.Empty(unix.Created);
        Assert.Equal("$HOME/.local/share/games/hcde", unix.Slice.AppData(true));
        Assert.Equal(new[] { "$HOME/.local/share/games/hcde" }, unix.Created);
        Assert.Equal("/cache/doom/ns", NewCache(unix));
        Assert.Equal("$HOME/.local/share/games/hcde/savegames/", unix.Slice.Savegames());
        unix.Slice.Netgame = true;
        Assert.Equal("$HOME/.local/share/games/hcde/savegames/netgame/", unix.Slice.Savegames());
        Assert.Equal("$HOME/.local/share/games/hcde/demo/", unix.Slice.Demo());

        unix.Real = path => path == "rel" ? "/abs" : null;
        Assert.Equal("/abs", unix.Slice.Normalized("rel"));
        Assert.Null(unix.Slice.Normalized("missing"));
        unix.Slice.Realpath = null;
        Assert.Equal("realpath", Assert.Throws<InvalidOperationException>(() => unix.Slice.Normalized("x")).Message);

        var shots = New(UnixKind1352.Unix);
        shots.Slice.Nice = s => s + "!";
        Assert.Equal("$HOME/Pictures/Screenshots/HCDE!", shots.Slice.Screenshots());
        Assert.Equal("$HOME/Pictures/Screenshots/HCDE!!", shots.Slice.Screenshots());
    }

    [Fact]
    public void UserFileMkdirAndConfig()
    {
        var ready = New(UnixKind1352.Unix);
        ready.Env["XDG_CONFIG_HOME"] = "/cfg";
        ready.Dirs["/cfg/hcde/"] = true;
        Assert.Equal("/cfg/hcde/autoexec.cfg", ready.Slice.Autoexec());
        Assert.Empty(ready.Made);
        Assert.Equal("/cfg/hcde/hcde.ini", ready.Slice.ConfigFile(true));
        Assert.Equal("/cfg/hcde/hcde.ini", ready.Slice.ConfigFile(false));

        var created = New(UnixKind1352.Unix);
        created.Env["XDG_CONFIG_HOME"] = "/cfg";
        Assert.Equal("/cfg/hcde/autoexec.cfg", created.Slice.Autoexec());
        Assert.Equal(new[] { ("/cfg", VmUnixPaths1352Slice.UserMode), ("/cfg/hcde/", VmUnixPaths1352Slice.UserMode) }, created.Made);

        var configFail = New(UnixKind1352.Unix);
        configFail.Env["XDG_CONFIG_HOME"] = "/cfg";
        configFail.Fail = "/cfg";
        configFail.Slice.Errno = "EACCES";
        var ex = Assert.Throws<InvalidOperationException>(() => configFail.Slice.UserFile("a"));
        Assert.Equal("Failed to create /cfg directory:\nEACCES", ex.Message);
        Assert.Equal(new[] { ("/cfg", VmUnixPaths1352Slice.UserMode) }, configFail.Made);

        var configFile = New(UnixKind1352.Unix);
        configFile.Env["XDG_CONFIG_HOME"] = "/cfg";
        configFile.Dirs["/cfg"] = false;
        ex = Assert.Throws<InvalidOperationException>(() => configFile.Slice.UserFile("a"));
        Assert.Equal("/cfg must be a directory", ex.Message);
        Assert.Empty(configFile.Made);

        var gameFile = New(UnixKind1352.Unix);
        gameFile.Env["XDG_CONFIG_HOME"] = "/cfg";
        gameFile.Dirs["/cfg/hcde/"] = false;
        ex = Assert.Throws<InvalidOperationException>(() => gameFile.Slice.UserFile("a"));
        Assert.Equal("/cfg/hcde/ must be a directory", ex.Message);
        Assert.Empty(gameFile.Made);

        var gameFail = New(UnixKind1352.Unix);
        gameFail.Env["XDG_CONFIG_HOME"] = "/cfg";
        gameFail.Fail = "/cfg/hcde/";
        gameFail.Slice.Errno = "ENOSPC";
        ex = Assert.Throws<InvalidOperationException>(() => gameFail.Slice.UserFile("a"));
        Assert.Equal("Failed to create /cfg/hcde/ directory:\nENOSPC", ex.Message);
        Assert.Equal(new[] { ("/cfg", VmUnixPaths1352Slice.UserMode), ("/cfg/hcde/", VmUnixPaths1352Slice.UserMode) }, gameFail.Made);

        var niced = New(UnixKind1352.Apple);
        niced.Slice.Nice = s => s == "$HOME/.config/hcde/" ? "/nice/hcde/" : s == "$HOME/.config" ? "/nice" : s;
        niced.Dirs["/nice"] = true;
        Assert.Equal("/nice/hcde/note", niced.Slice.UserFile("note"));
        Assert.Equal(new[] { ("/nice/hcde/", VmUnixPaths1352Slice.UserMode) }, niced.Made);
        Assert.Equal(0, niced.EnvReads);

        var raw = New(UnixKind1352.Unix);
        raw.Env["XDG_CONFIG_HOME"] = "/cfg";
        raw.Slice.Nice = s => s + "N";
        raw.Fail = "/cfgN";
        raw.Slice.Errno = "EIO";
        ex = Assert.Throws<InvalidOperationException>(() => raw.Slice.UserFile("a"));
        Assert.Equal("Failed to create /cfg directory:\nEIO", ex.Message);
    }

    static string NewCache(Mem unix)
    {
        unix.Env["XDG_CACHE_HOME"] = "/cache";
        unix.Created.Clear();
        string path = unix.Slice.CachePath(true, "ns");
        Assert.Equal(new[] { "/cache/doom/ns" }, unix.Created);
        return path;
    }

    static Mem New(UnixKind1352 kind) => new(kind);

    sealed class Mem
    {
        public readonly VmUnixPaths1352Slice Slice = new();
        public readonly Dictionary<string, string?> Env = new();
        public readonly Dictionary<string, bool> Dirs = new();
        public readonly List<(string Path, int Mode)> Made = new();
        public readonly List<string> Created = new();
        public readonly List<string> NiceCalls = new();
        public int EnvReads;
        public string? Fail;
        public Func<string, string?> Real = _ => null;

        public Mem(UnixKind1352 kind)
        {
            Slice.Kind = kind;
            Slice.GetEnv = name =>
            {
                EnvReads++;
                return Env.TryGetValue(name, out string? value) ? value : null;
            };
            Slice.Nice = path =>
            {
                NiceCalls.Add(path);
                return path;
            };
            Slice.Stat = path => Dirs.TryGetValue(path, out bool dir) ? new Stat1352 { IsDir = dir } : null;
            Slice.Mkdir = (path, mode) =>
            {
                Made.Add((path, mode));
                if (Fail == path)
                    return false;
                Dirs[path] = true;
                return true;
            };
            Slice.Create = Created.Add;
            Slice.Realpath = path => Real(path);
        }
    }
}
