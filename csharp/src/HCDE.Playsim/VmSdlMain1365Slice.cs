namespace HCDE.Playsim;

/// <summary>
/// SDL startup from <c>i_main.cpp</c> lines 1-238, through end of file.
/// Crash info, OS detection, and <c>main</c>. The Linux restart body is the compiled path.
/// </summary>
public sealed class VmSdlMain1365Slice
{
    public const string Backend = "SDL2";
    public const string GameName = "HCDE";
    public const string GameNameLower = "hcde";
    public const string CrashLogName = GameNameLower + "-crash.log";
    public const int SigSegv = 11;
    public const int SigIll = 4;
    public const int SigFpe = 8;
    public const int SigBus = 7;
    public const int SigInt = 2;
    public const int SigTerm = 15;
    public const int LcAll = 6;
    public const int ORdonly = 0;
    public const int PathMax = 4096;
    public const int SdlFalse = 0;
    public const int BufferLimit = 255;
    public const int HandlerCount = 4;

    public bool Apple { get; set; }
    public bool Linux { get; set; } = true;
    public bool UseProgdir { get; set; }
    public string ProgdirMacro { get; set; } = "";
    public string VersionString { get; set; } = "";
    public string GitTime { get; set; } = "";
    public string CompileDate { get; set; } = "";
    public string SysOsType { get; set; } = "";
    public string Progdir { get; set; } = "";
    public List<string>? Args { get; set; }
    public int? InstalledUid { get; private set; }
    public int QuitCount { get; private set; }
    public int MouseMode { get; private set; } = int.MinValue;

    public List<string> Trace { get; } = [];
    public List<string> Stdout { get; } = [];
    public List<string> Stderr { get; } = [];
    public List<(int Signal, string Handler)> Signals { get; } = [];
    public List<(string Path, int Result)> Stats { get; } = [];
    public List<string> Popens { get; } = [];
    public List<string> Pcloses { get; } = [];
    public List<(string Path, int Flags, int Fd)> Opens { get; } = [];
    public List<(int Fd, string?[]? Argv, bool Environ)> Execs { get; } = [];
    public List<(string Name, string Value, int Overwrite)> Env { get; } = [];
    public List<(int Category, string Locale)> Locales { get; } = [];

    public Action<int, string?[]?, int, int[], string, Func<byte[]?, int, int>>? InstallHandlers { get; set; }
    public Action<int, string>? Signal { get; set; }
    public Func<int>? GetUid { get; set; }
    public Action<int>? SetEuid { get; set; }
    public Action<string, string, int>? SetEnv { get; set; }
    public Action<int, string>? SetLocale { get; set; }
    public Func<int, int>? SdlInit { get; set; }
    public Func<string?>? SdlGetError { get; set; }
    public Func<string, string?>? Realpath { get; set; }
    public Action? StartupJoysticks { get; set; }
    public Func<int>? GameMain { get; set; }
    public Func<bool>? CheckForRestart { get; set; }
    public Action<int>? SetRelativeMouseMode { get; set; }
    public Action? SdlQuit { get; set; }
    public Func<string, int>? Stat { get; set; }
    public Func<string, byte[]?>? Popen { get; set; }
    public Action<string>? Pclose { get; set; }
    public Func<UtsName?>? Uname { get; set; }
    public Action<byte[], int, string>? CrashInfo { get; set; }
    public Func<string, string?>? GetEnv { get; set; }
    public Func<string, int, int>? Open { get; set; }
    public Action<int, string?[]?, bool>? Fexecve { get; set; }

    public sealed class UtsName
    {
        public string? SysName { get; set; } = "";
        public string? Release { get; set; } = "";
        public string? Machine { get; set; } = "";
    }

    public static string FormatOsReleaseCommand(string path)
    {
        var full = ". " + path + " && echo ${PRETTY_NAME}";
        return full.Length > BufferLimit ? full[..BufferLimit] : full;
    }

    public int GetCrashInfo(byte[]? buffer, int end)
    {
        if (buffer == null)
            throw new InvalidOperationException("buffer");
        if (CrashInfo != null)
            CrashInfo(buffer, end, "\n");
        var length = 0;
        while (length < buffer.Length && buffer[length] != 0)
            length++;
        if (length == buffer.Length)
            throw new IndexOutOfRangeException("buffer");
        return length;
    }

    public string DetectOs()
    {
        var operating = "";
        foreach (var path in new[] { "/etc/os-release", "/usr/lib/os-release" })
        {
            if (Stat == null)
                throw new InvalidOperationException("stat");
            var result = Stat(path);
            Stats.Add((path, result));
            if (result != 0)
                continue;
            var cmdline = FormatOsReleaseCommand(path);
            if (Popen == null)
                throw new InvalidOperationException("popen");
            var data = Popen(cmdline);
            Popens.Add(cmdline);
            if (data == null)
                continue;
            var distribution = new byte[256];
            var copied = Math.Min(data.Length, BufferLimit);
            if (copied > 0)
                Array.Copy(data, distribution, copied);
            var length = 0;
            while (length < distribution.Length && distribution[length] != 0)
                length++;
            if (length > 1)
                operating = Latin1(distribution, length - 1);
            if (Pclose == null)
                throw new InvalidOperationException("pclose");
            Pclose(cmdline);
            Pcloses.Add(cmdline);
            break;
        }

        if (Uname == null)
            throw new InvalidOperationException("uname");
        var uname = Uname();
        if (uname != null)
        {
            var sys = uname.SysName ?? throw new InvalidOperationException("uname");
            var release = uname.Release ?? throw new InvalidOperationException("uname");
            var machine = uname.Machine ?? throw new InvalidOperationException("uname");
            var separator = operating.Length > 0 ? ", " : "";
            operating = operating + separator + sys + " " + release + " on " + machine;
            SysOsType = sys + " " + release + " on " + machine;
        }

        if (operating.Length == 0)
            operating = "Unknown";
        return operating;
    }

    public void TryRestart(string?[]? argv)
    {
        Trace.Add("I_TryRestart");
        if (!Linux)
            return;
        if (GetEnv == null)
            throw new InvalidOperationException("getenv");
        Trace.Add("getenv");
        var appimage = GetEnv("APPIMAGE");
        if (appimage != null)
        {
            Exec(appimage, argv);
            return;
        }

        Exec("/proc/self/exe", argv);
    }

    public int Main(int argc, string?[]? argv)
    {
        if (!Apple)
        {
            if (InstallHandlers == null)
                throw new InvalidOperationException("cc_install_handlers");
            var signals = new[] { SigSegv, SigIll, SigFpe, SigBus };
            Trace.Add("cc_install_handlers");
            InstallHandlers(argc, argv, HandlerCount, signals, CrashLogName, GetCrashInfo);
        }

        InstallSignal(SigInt);
        InstallSignal(SigTerm);
        Trace.Add("printf");
        Stdout.Add(
            GameName + " " + VersionString + " - " + GitTime
            + " - SDL version\nCompiled on " + CompileDate + "\n");
        if (GetUid == null)
            throw new InvalidOperationException("getuid");
        Trace.Add("getuid");
        var uid = GetUid();
        if (SetEuid == null)
            throw new InvalidOperationException("seteuid");
        Trace.Add("seteuid");
        SetEuid(uid);
        InstalledUid = uid;
        if (SetEnv == null)
            throw new InvalidOperationException("setenv");
        Trace.Add("setenv");
        SetEnv("LC_NUMERIC", "C", 1);
        Env.Add(("LC_NUMERIC", "C", 1));
        if (SetLocale == null)
            throw new InvalidOperationException("setlocale");
        Trace.Add("setlocale");
        SetLocale(LcAll, "C");
        Locales.Add((LcAll, "C"));
        if (SdlInit == null)
            throw new InvalidOperationException("SDL_Init");
        Trace.Add("SDL_Init");
        var sdl = SdlInit(0);
        if (sdl < 0)
        {
            if (SdlGetError == null)
                throw new InvalidOperationException("SDL_GetError");
            Trace.Add("SDL_GetError");
            var error = SdlGetError() ?? throw new InvalidOperationException("SDL_GetError");
            Trace.Add("fprintf");
            Stderr.Add("Could not initialize SDL:\n" + error + "\n");
            return -1;
        }

        if (argc < 0 || (argv == null && argc > 0) || (argv != null && argc > argv.Length))
            throw new IndexOutOfRangeException("argv");
        Trace.Add("FArgs");
        var copied = new List<string>(argc);
        for (var i = 0; i < argc; i++)
            copied.Add(argv![i] ?? "");
        Args = copied;
        if (UseProgdir)
        {
            Trace.Add("progdir");
            Progdir = ProgdirMacro;
        }
        else
        {
            if (Realpath == null)
                throw new InvalidOperationException("realpath");
            if (argv == null || argv.Length == 0 || argv[0] == null)
                throw new InvalidOperationException("argv");
            var arg0 = argv[0]!;
            Trace.Add("realpath");
            var resolved = Realpath(arg0);
            var program = resolved ?? arg0;
            if (program.Length >= PathMax)
                throw new InvalidOperationException("program");
            var slash = program.LastIndexOf('/');
            Trace.Add("progdir");
            Progdir = slash >= 0 ? program[..(slash + 1)] : "./";
        }

        if (StartupJoysticks == null)
            throw new InvalidOperationException("I_StartupJoysticks");
        Trace.Add("I_StartupJoysticks");
        StartupJoysticks();
        if (GameMain == null)
            throw new InvalidOperationException("GameMain");
        Trace.Add("GameMain");
        var result = GameMain();
        if (CheckForRestart == null)
            throw new InvalidOperationException("SDL_I_CheckForRestart");
        Trace.Add("SDL_I_CheckForRestart");
        if (CheckForRestart())
            TryRestart(argv);
        if (SetRelativeMouseMode == null)
            throw new InvalidOperationException("SDL_SetRelativeMouseMode");
        Trace.Add("SDL_SetRelativeMouseMode");
        SetRelativeMouseMode(SdlFalse);
        MouseMode = SdlFalse;
        if (SdlQuit == null)
            throw new InvalidOperationException("SDL_Quit");
        Trace.Add("SDL_Quit");
        SdlQuit();
        QuitCount++;
        return result;
    }

    private void InstallSignal(int signal)
    {
        if (Signal == null)
            throw new InvalidOperationException("signal");
        Trace.Add("signal");
        Signal(signal, "SignalHandler");
        Signals.Add((signal, "SignalHandler"));
    }

    private void Exec(string path, string?[]? argv)
    {
        if (Open == null)
            throw new InvalidOperationException("open");
        Trace.Add("open");
        var fd = Open(path, ORdonly);
        Opens.Add((path, ORdonly, fd));
        if (Fexecve == null)
            throw new InvalidOperationException("fexecve");
        Trace.Add("fexecve");
        Fexecve(fd, argv, true);
        Execs.Add((fd, argv, true));
    }

    private static string Latin1(byte[] data, int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = (char)data[i];
        return new string(chars);
    }
}
