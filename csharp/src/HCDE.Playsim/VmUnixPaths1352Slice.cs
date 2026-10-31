namespace HCDE.Playsim;

/// <summary>
/// Unix special folders from <c>i_specialpaths.cpp</c> (1-262).
/// Chunk 1352. NicePath, stat, mkdir, CreatePath, getenv, and realpath
/// stay injected. Apple skips the environment. Haiku uses its own fallbacks.
/// </summary>
public sealed class VmUnixPaths1352Slice
{
    public const string GameLower = "hcde";
    public const string GameName = "HCDE";
    public const int UserMode = 0x1C0;

    public UnixKind1352 Kind;
    public bool Netgame;
    public Func<string, string?>? GetEnv;
    public Func<string, string>? Nice;
    public Func<string, Stat1352?>? Stat;
    public Func<string, int, bool>? Mkdir;
    public Func<string, string?>? Realpath;
    public Action<string>? Create;
    public string Errno = "";

    readonly Dictionary<string, string> _seen = new();
    string? _shots;

    public string Config() => Lookup("XDG_CONFIG_HOME", Kind == UnixKind1352.Haiku ? "$HOME/config/settings" : "$HOME/.config");

    public string Cache() => Lookup("XDG_CACHE_HOME", Kind == UnixKind1352.Haiku ? "$HOME/config/cache" : "$HOME/.cache");

    public string Data() => Lookup("XDG_DATA_HOME", Kind == UnixKind1352.Haiku ? "$HOME/config/non-packaged/data" : "$HOME/.local/share");

    public string Pictures()
    {
        if (Kind == UnixKind1352.Haiku)
            throw new InvalidOperationException("Pictures");
        return Lookup("XDG_PICTURES_DIR", "$HOME/Pictures");
    }

    public string UserFile(string file)
    {
        string path = NicePath(Config() + "/" + GameLower + "/");
        if (!TryStat(path, out Stat1352 info))
        {
            string configPath = NicePath(Config());
            if (!TryStat(configPath, out Stat1352 extra))
            {
                if (!Make(configPath))
                    Fatal("Failed to create " + Config() + " directory:\n" + Errno);
            }
            else if (!extra.IsDir)
            {
                Fatal(Config() + " must be a directory");
            }
            if (!Make(path))
                Fatal("Failed to create " + path + " directory:\n" + Errno);
        }
        else if (!info.IsDir)
        {
            Fatal(path + " must be a directory");
        }
        return path + file;
    }

    public string AppData(bool create)
    {
        string path = NicePath(Data() + "/games/" + GameLower);
        if (create)
            MakeTree(path);
        return path;
    }

    public string CachePath(bool create, string ns)
    {
        string path = NicePath(Cache() + "/doom/" + ns);
        if (create)
            MakeTree(path);
        return path;
    }

    public string Autoexec() => UserFile("autoexec.cfg");

    public string ConfigFile(bool forReading)
    {
        _ = forReading;
        return UserFile(GameLower + ".ini");
    }

    public string Documents()
    {
        if (Kind == UnixKind1352.Haiku)
            return Config() + "/" + GameLower + "/";
        return AppData(false) + "/";
    }

    public string Screenshots()
    {
        if (_shots == null)
        {
            _shots = Kind == UnixKind1352.Haiku
                ? Documents() + "screenshots"
                : Pictures() + "/Screenshots/" + GameName;
        }
        _shots = NicePath(_shots);
        return _shots;
    }

    public string Savegames()
    {
        string docs = Documents();
        if (Netgame)
            return docs + "savegames/netgame/";
        return docs + "savegames/";
    }

    public string Demo() => Documents() + "demo/";

    public string? Normalized(string path)
    {
        if (Realpath == null)
            throw new InvalidOperationException("realpath");
        return Realpath(path);
    }

    string Lookup(string variable, string fallback)
    {
        if (Kind == UnixKind1352.Apple)
            return fallback;
        if (_seen.TryGetValue(variable, out string? cached))
            return cached;
        if (GetEnv == null)
            throw new InvalidOperationException("getenv");
        string? env = GetEnv(variable);
        string chosen = env == null ? fallback : env;
        _seen[variable] = chosen;
        return chosen;
    }

    string NicePath(string path)
    {
        if (Nice == null)
            throw new InvalidOperationException("NicePath");
        return Nice(path);
    }

    bool TryStat(string path, out Stat1352 info)
    {
        if (Stat == null)
            throw new InvalidOperationException("stat");
        Stat1352? got = Stat(path);
        if (got == null)
        {
            info = new Stat1352();
            return false;
        }
        info = got;
        return true;
    }

    bool Make(string path)
    {
        if (Mkdir == null)
            throw new InvalidOperationException("mkdir");
        return Mkdir(path, UserMode);
    }

    void MakeTree(string path)
    {
        if (Create == null)
            throw new InvalidOperationException("CreatePath");
        Create(path);
    }

    static void Fatal(string message) => throw new InvalidOperationException(message);
}

public enum UnixKind1352
{
    Unix,
    Apple,
    Haiku,
}

public sealed class Stat1352
{
    public bool IsDir;
}
