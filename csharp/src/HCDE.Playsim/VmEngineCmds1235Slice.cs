namespace HCDE.Playsim;

/// <summary>
/// Game-independent console commands from <c>c_enginecmds.cpp</c> (<b>1–303</b>). Chunk <b>1235</b>.
/// Quit and exit run the endoom screen and throw exit code 0. The logfile append flag is the
/// presence of a third argument. Directory wildcards are decided after the Linux path split,
/// where only <c>/</c> is a separator. <c>wdir</c> lists every lump unless it is given exactly
/// one argument. <c>md5sum</c> reads 8192-byte chunks. <c>printlocalized</c> builds a little-endian
/// language id from the first three lowercased bytes and throws when the tag is only two bytes.
/// <c>crashout</c> is the null store, recorded as a fault and not performed.
/// </summary>
public sealed class VmEngineCmds1235
{
    public const int PrintHigh = 2;
    public const int PrintBold = 200;

    public readonly List<string> Log = new();
    public bool LogOpen;
    public string Clock = "";
    public string Version = "";
    public string GitTime = "";
    public string GitHash = "";
    public string Cwd = "";
    public string Home = "";
    public readonly Dictionary<string, string?> Strings = new(StringComparer.Ordinal);
    public readonly Dictionary<string, string> Languages = new(StringComparer.Ordinal);
    public readonly List<bool> ExecOk = new();
    public readonly HashSet<string> LogFail = new(StringComparer.Ordinal);
    public readonly Dictionary<string, Scan> Scans = new(StringComparer.Ordinal);
    public readonly Dictionary<string, int> WadNums = new(StringComparer.Ordinal);
    public readonly List<Lump> Lumps = new();
    public readonly Dictionary<string, byte[]?> Files = new(StringComparer.Ordinal);
    public readonly Dictionary<string, string> Errors = new(StringComparer.Ordinal);
    int _exec;

    public sealed class Scan
    {
        public bool Ok;
        public readonly List<Row> Rows = new();
    }

    public sealed class Row
    {
        public string Name = "";
        public bool Dir;
    }

    public sealed class Lump
    {
        public int Container;
        public long Length;
        public string Name = "";
    }

    public void Quit(params string[] args) => ExitLike();

    public void Exit(params string[] args) => ExitLike();

    public void GameVersion(params string[] args)
    {
        Say(PrintHigh, Version + " @ " + GitTime);
        Say(PrintHigh, "Commit " + GitHash);
    }

    public void Print(params string[] args)
    {
        if (args.Length != 2)
        {
            Say(PrintHigh, "print <name>: Print a string from the string table");
            return;
        }

        string name = args[1];
        if (!Strings.TryGetValue(name, out var str) || str == null)
            Say(PrintHigh, name + " unknown");
        else
            Say(PrintHigh, str);
    }

    public void Exec(params string[] args)
    {
        if (args.Length < 2)
            return;
        for (int i = 1; i < args.Length; i++)
        {
            bool ok = _exec < ExecOk.Count && ExecOk[_exec];
            _exec++;
            Log.Add("exec " + args[i] + " ok=" + (ok ? 1 : 0));
            if (!ok)
            {
                Say(PrintHigh, "Could not exec \"" + args[i] + "\"");
                break;
            }
        }
    }

    public void Logfile(params string[] args)
    {
        if (LogOpen)
        {
            Say(PrintHigh, "Log stopped: " + Clock);
            LogOpen = false;
            Log.Add("fclose");
        }

        if (args.Length >= 2)
            OpenLog(args[1], args.Length >= 3);
    }

    public void Error(params string[] args)
    {
        if (args.Length > 1)
            Log.Add("ierror " + args[1]);
        else
            Say(PrintHigh, "Usage: error <error text>");
    }

    public void ErrorFatal(params string[] args)
    {
        if (args.Length > 1)
            Log.Add("ifatal " + args[1]);
        else
            Say(PrintHigh, "Usage: error_fatal <error text>");
    }

    public void Crashout(params string[] args)
    {
        Log.Add("crashout fault addr=0");
    }

    public void Dir(params string[] args)
    {
        string path;
        if (args.Length > 1)
            path = Nice(args[1]);
        else
        {
            path = Cwd;
            Log.Add("cwd " + path);
        }

        string baseName = FileBase(path);
        string bpath;
        if (baseName.IndexOf('*') >= 0 || baseName.IndexOf('?') >= 0)
            bpath = FilePath(path);
        else
        {
            baseName = "*";
            bpath = path;
        }

        Log.Add("scan bpath=" + bpath + " pat=" + baseName + " recurse=1");
        if (!Scans.TryGetValue(bpath + "\n" + baseName, out var scan) || scan == null || !scan.Ok)
        {
            Say(PrintHigh, "Nothing matching " + path);
            return;
        }

        Say(PrintHigh, "Listing of " + path + ":");
        foreach (var row in scan.Rows)
        {
            if (row.Dir)
                Say(PrintBold, row.Name + " <dir>");
            else
                Say(PrintHigh, row.Name);
        }
    }

    public void Wdir(params string[] args)
    {
        int wadnum;
        if (args.Length != 2)
            wadnum = -1;
        else
        {
            string name = args[1];
            wadnum = WadNums.TryGetValue(name, out var n) ? n : -1;
            Log.Add("wad " + name + " num=" + wadnum);
            if (wadnum < 0)
            {
                Say(PrintHigh, name + " must be loaded to view its directory.");
                return;
            }
        }

        foreach (var lump in Lumps)
        {
            if (wadnum == -1 || lump.Container == wadnum)
                Say(PrintHigh, lump.Length.ToString().PadLeft(10) + " " + lump.Name);
        }
    }

    public void Md5Sum(params string[] args)
    {
        if (args.Length < 2)
            Say(PrintHigh, "Usage: md5sum <file> ...");
        for (int i = 1; i < args.Length; i++)
            HashFile(args[i]);
    }

    public void PrintLocalized(params string[] args)
    {
        if (args.Length <= 1)
            return;
        if (args.Length > 2)
        {
            string lang = ToLowerAscii(args[2]);
            if (lang.Length >= 2)
            {
                if (lang.Length < 3)
                {
                    Log.Add("bounds index=2 len=" + lang.Length);
                    return;
                }

                uint id = MakeId((byte)lang[0], (byte)lang[1], (byte)lang[2]);
                string hex = id.ToString("x8");
                Log.Add("lang id=" + hex);
                if (!Languages.TryGetValue(args[1] + "|" + hex, out var text))
                {
                    Log.Add("lang null");
                    return;
                }

                Say(PrintHigh, text);
                return;
            }
        }

        Say(PrintHigh, GetString(args[1]));
    }

    void ExitLike()
    {
        Log.Add("endoom");
        Log.Add("throw exit code=0");
    }

    void OpenLog(string name, bool append)
    {
        Log.Add("open " + name + " append=" + (append ? 1 : 0));
        if (LogFail.Contains(name))
        {
            Say(PrintHigh, "Could not start log");
            return;
        }

        LogOpen = true;
        Say(PrintHigh, "Log started: " + Clock);
    }

    void HashFile(string name)
    {
        if (!Files.TryGetValue(name, out var data) || data == null)
        {
            string err = Errors.TryGetValue(name, out var text) ? text : "No such file or directory";
            Say(PrintHigh, name + ": " + err);
            return;
        }

        var md = new VmMd51209Slice();
        int off = 0;
        while (off < data.Length)
        {
            int n = 8192;
            if (n > data.Length - off)
                n = data.Length - off;
            var chunk = new byte[n];
            Array.Copy(data, off, chunk, 0, n);
            md.Update(chunk, n);
            off += n;
            Log.Add("read n=" + n);
        }

        byte[] dig = md.Final();
        var hex = new char[32];
        for (int i = 0; i < 16; i++)
        {
            byte b = dig[i];
            hex[i * 2] = Hex(b >> 4);
            hex[i * 2 + 1] = Hex(b & 15);
        }

        Say(PrintHigh, new string(hex) + " *" + name);
    }

    string Nice(string path)
    {
        if (path.Length == 0)
            return "";
        if (path[0] != '~')
            return path;
        if (path.Length == 1 || path[1] == '/')
        {
            if (Home.Length == 0)
                return path;
            if (path.Length == 1)
                return Home;
            return Home + path.Substring(1);
        }

        return path;
    }

    static string FileBase(string path)
    {
        if (path.Length == 0)
            return "";
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/')
            src--;
        return path.Substring(src);
    }

    static string FilePath(string path)
    {
        if (path.Length == 0)
            return "";
        int src = path.Length - 1;
        while (src != 0 && path[src - 1] != '/')
            src--;
        return path.Substring(0, src);
    }

    string GetString(string name)
    {
        if (Strings.TryGetValue(name, out var str) && str != null)
            return str;
        return name;
    }

    static string ToLowerAscii(string text)
    {
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= 'A' && chars[i] <= 'Z')
                chars[i] = (char)(chars[i] + 32);
        }

        return new string(chars);
    }

    static uint MakeId(byte a, byte b, byte c)
    {
        return (uint)a | ((uint)b << 8) | ((uint)c << 16);
    }

    static char Hex(int n) => (char)(n < 10 ? '0' + n : 'a' + (n - 10));

    void Say(int level, string text) => Log.Add("print " + level + " " + text);
}
