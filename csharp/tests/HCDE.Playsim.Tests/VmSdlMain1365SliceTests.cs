using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmSdlMain1365SliceTests
{
    [Fact]
    public void DetectOsAndCrashInfo()
    {
        var plain = new VmSdlMain1365Slice();
        var hi = new byte[] { (byte)'h', (byte)'i', 0, (byte)'x' };
        Assert.Equal(2, plain.GetCrashInfo(hi, 8));
        var seenEnd = -1;
        string? seenLf = null;
        plain.CrashInfo = (buffer, end, lf) =>
        {
            seenEnd = end;
            seenLf = lf;
            buffer[0] = (byte)'z';
            buffer[1] = 0;
        };
        Assert.Equal(1, plain.GetCrashInfo(hi, 8));
        Assert.Equal(8, seenEnd);
        Assert.Equal("\n", seenLf);
        plain.CrashInfo = null;
        var empty = new byte[] { 0 };
        Assert.Equal(0, plain.GetCrashInfo(empty, 1));
        var missing = Assert.Throws<InvalidOperationException>(() => plain.GetCrashInfo(null, 4));
        Assert.Equal("buffer", missing.Message);
        var open = Assert.Throws<IndexOutOfRangeException>(
            () => plain.GetCrashInfo(new byte[] { 1, 2, 3 }, 3));
        Assert.Equal("buffer", open.Message);

        var suffix = " && echo ${PRETTY_NAME}";
        var overhead = 2 + suffix.Length;
        var exactPath = new string('b', 255 - overhead);
        var exact = ". " + exactPath + suffix;
        Assert.Equal(255, exact.Length);
        Assert.Equal(exact, VmSdlMain1365Slice.FormatOsReleaseCommand(exactPath));
        var overPath = new string('b', 256 - overhead);
        var over = ". " + overPath + suffix;
        Assert.Equal(over[..255], VmSdlMain1365Slice.FormatOsReleaseCommand(overPath));
        Assert.Equal(
            ". /etc/os-release && echo ${PRETTY_NAME}",
            VmSdlMain1365Slice.FormatOsReleaseCommand("/etc/os-release"));

        var bothFail = Os(_ => -1, _ => null);
        bothFail.SysOsType = "keep";
        Assert.Equal("Unknown", bothFail.DetectOs());
        Assert.Equal("keep", bothFail.SysOsType);
        Assert.Equal(2, bothFail.Stats.Count);
        Assert.Empty(bothFail.Popens);
        Assert.Empty(bothFail.Pcloses);

        var chopped = Os(_ => 0, cmd =>
        {
            Assert.Equal(". /etc/os-release && echo ${PRETTY_NAME}", cmd);
            return "AB"u8.ToArray();
        });
        chopped.SysOsType = "keep";
        Assert.Equal("A", chopped.DetectOs());
        Assert.Equal("keep", chopped.SysOsType);
        Assert.Single(chopped.Stats);
        Assert.Equal("/etc/os-release", chopped.Stats[0].Path);
        Assert.Single(chopped.Pcloses);

        var newline = Os(_ => 0, _ => "Ubuntu\n"u8.ToArray());
        Assert.Equal("Ubuntu", newline.DetectOs());
        var one = Os(_ => 0, _ => "\n"u8.ToArray());
        one.SysOsType = "keep";
        Assert.Equal("Unknown", one.DetectOs());
        Assert.Equal("keep", one.SysOsType);
        Assert.Single(one.Pcloses);
        Assert.Single(one.Stats);

        var embedded = Os(_ => 0, _ => new byte[] { (byte)'A', 0, (byte)'B', (byte)'\n' });
        Assert.Equal("Unknown", embedded.DetectOs());
        var latin = Os(_ => 0, _ => new byte[] { 0xC3, 0xA9, (byte)'\n' });
        var latinText = latin.DetectOs();
        Assert.Equal(2, latinText.Length);
        Assert.Equal((char)0xC3, latinText[0]);
        Assert.Equal((char)0xA9, latinText[1]);

        var filled = new byte[300];
        Array.Fill(filled, (byte)'Q');
        var wide = Os(_ => 0, _ => filled);
        var wideText = wide.DetectOs();
        Assert.Equal(254, wideText.Length);
        Assert.All(wideText, ch => Assert.Equal('Q', ch));

        var skipped = Os(
            path => path == "/etc/os-release" ? -1 : 0,
            cmd =>
            {
                Assert.Equal(". /usr/lib/os-release && echo ${PRETTY_NAME}", cmd);
                return "Second\n"u8.ToArray();
            });
        Assert.Equal("Second", skipped.DetectOs());
        Assert.Equal(2, skipped.Stats.Count);
        Assert.Single(skipped.Popens);

        var continued = Os(
            _ => 0,
            cmd => cmd.Contains("/etc/", StringComparison.Ordinal) ? null : "Later\n"u8.ToArray());
        Assert.Equal("Later", continued.DetectOs());
        Assert.Equal(2, continued.Popens.Count);
        Assert.Single(continued.Pcloses);
        Assert.Equal(". /usr/lib/os-release && echo ${PRETTY_NAME}", continued.Pcloses[0]);

        var named = Os(_ => 0, _ => "Ubuntu\n"u8.ToArray(), () => new VmSdlMain1365Slice.UtsName
        {
            SysName = "Linux",
            Release = "6.1",
            Machine = "x86_64",
        });
        Assert.Equal("Ubuntu, Linux 6.1 on x86_64", named.DetectOs());
        Assert.Equal("Linux 6.1 on x86_64", named.SysOsType);

        var bare = Os(_ => -1, _ => null, () => new VmSdlMain1365Slice.UtsName
        {
            SysName = "Linux",
            Release = "6.1",
            Machine = "x86_64",
        });
        Assert.Equal("Linux 6.1 on x86_64", bare.DetectOs());
        Assert.Equal("Linux 6.1 on x86_64", bare.SysOsType);

        var blanks = Os(_ => -1, _ => null, () => new VmSdlMain1365Slice.UtsName());
        Assert.Equal("  on ", blanks.DetectOs());
        Assert.Equal("  on ", blanks.SysOsType);

        var broken = Os(_ => -1, _ => null, () => new VmSdlMain1365Slice.UtsName { SysName = null });
        broken.SysOsType = "old";
        var uname = Assert.Throws<InvalidOperationException>(() => broken.DetectOs());
        Assert.Equal("uname", uname.Message);
        Assert.Equal("old", broken.SysOsType);

        var noStat = new VmSdlMain1365Slice { Uname = () => null };
        var stat = Assert.Throws<InvalidOperationException>(() => noStat.DetectOs());
        Assert.Equal("stat", stat.Message);
        Assert.Empty(noStat.Popens);

        var noPopen = Os(_ => 0, _ => null);
        noPopen.Popen = null;
        var popen = Assert.Throws<InvalidOperationException>(() => noPopen.DetectOs());
        Assert.Equal("popen", popen.Message);
        Assert.Empty(noPopen.Popens);
        Assert.Single(noPopen.Stats);

        var noClose = Os(_ => 0, _ => "AB"u8.ToArray());
        noClose.Pclose = null;
        noClose.SysOsType = "old";
        var pclose = Assert.Throws<InvalidOperationException>(() => noClose.DetectOs());
        Assert.Equal("pclose", pclose.Message);
        Assert.Equal("old", noClose.SysOsType);
        Assert.Empty(noClose.Pcloses);

        var noUname = Os(_ => 0, _ => "AB"u8.ToArray());
        noUname.Uname = null;
        noUname.SysOsType = "old";
        var missingUname = Assert.Throws<InvalidOperationException>(() => noUname.DetectOs());
        Assert.Equal("uname", missingUname.Message);
        Assert.Equal("old", noUname.SysOsType);
        Assert.Single(noUname.Pcloses);
    }

    [Fact]
    public void StartupMain()
    {
        var argv = new[] { "/opt/hcde/hcde", "-iwad" };
        int argcSeen = -1;
        string?[]? argvSeen = null;
        var countSeen = -1;
        int[]? signals = null;
        string? log = null;
        Func<byte[]?, int, int>? crash = null;
        var host = Ready();
        host.InstallHandlers = (argc, args, count, installed, name, fn) =>
        {
            argcSeen = argc;
            argvSeen = args;
            countSeen = count;
            signals = installed;
            log = name;
            crash = fn;
        };
        string? realArg = null;
        host.Realpath = path =>
        {
            realArg = path;
            return "/opt/hcde/hcde";
        };
        var uidOrder = new List<string>();
        host.GetUid = () =>
        {
            uidOrder.Add("getuid");
            return 4;
        };
        host.SetEuid = uid => uidOrder.Add("seteuid:" + uid);
        int? sdlFlags = null;
        host.SdlInit = flags =>
        {
            sdlFlags = flags;
            return 0;
        };
        Assert.Equal(7, host.Main(2, argv));
        Assert.Equal(
            new[]
            {
                "cc_install_handlers", "signal", "signal", "printf", "getuid", "seteuid",
                "setenv", "setlocale", "SDL_Init", "FArgs", "realpath", "progdir",
                "I_StartupJoysticks", "GameMain", "SDL_I_CheckForRestart",
                "SDL_SetRelativeMouseMode", "SDL_Quit",
            },
            host.Trace);
        Assert.Equal(new[] { "getuid", "seteuid:4" }, uidOrder);
        Assert.Equal(2, argcSeen);
        Assert.Same(argv, argvSeen);
        Assert.Equal(4, countSeen);
        Assert.Equal(new[] { 11, 4, 8, 7 }, signals);
        Assert.Equal("hcde-crash.log", log);
        Assert.NotNull(crash);
        Assert.Equal("GetCrashInfo", crash.Method.Name);
        Assert.Same(host, crash.Target);
        Assert.Equal(1, crash(new byte[] { (byte)'q', 0 }, 4));
        Assert.Equal((2, "SignalHandler"), host.Signals[0]);
        Assert.Equal((15, "SignalHandler"), host.Signals[1]);
        Assert.Equal(2, host.Signals.Count);
        Assert.DoesNotContain(host.Signals, pair => pair.Signal == 1 || pair.Signal == 3);
        Assert.Equal(
            "HCDE 0.4.12 - 2026-01-01 - SDL version\nCompiled on Oct 31 2026\n",
            Assert.Single(host.Stdout));
        Assert.Equal(("LC_NUMERIC", "C", 1), Assert.Single(host.Env));
        Assert.Equal((6, "C"), Assert.Single(host.Locales));
        Assert.Equal(0, sdlFlags);
        Assert.Equal(4, host.InstalledUid);
        Assert.Equal(argv, host.Args);
        Assert.Equal("/opt/hcde/hcde", realArg);
        Assert.Equal("/opt/hcde/", host.Progdir);
        Assert.Equal(0, host.MouseMode);
        Assert.Equal(1, host.QuitCount);
        Assert.Empty(host.Stderr);
        Assert.Empty(host.Execs);
        Assert.Equal("SDL2", VmSdlMain1365Slice.Backend);

        var apple = Ready();
        apple.Apple = true;
        apple.InstallHandlers = null;
        apple.Main(1, new[] { "hcde" });
        Assert.DoesNotContain("cc_install_handlers", apple.Trace);
        Assert.Equal(2, apple.Signals.Count);
        Assert.Equal(1, apple.QuitCount);

        var quiet = Ready();
        quiet.Linux = false;
        quiet.CheckForRestart = () => true;
        quiet.GetEnv = _ => throw new InvalidOperationException("getenv");
        Assert.Equal(7, quiet.Main(1, new[] { "hcde" }));
        Assert.Contains("I_TryRestart", quiet.Trace);
        Assert.DoesNotContain("getenv", quiet.Trace);
        Assert.Empty(quiet.Opens);
        Assert.Equal(1, quiet.QuitCount);

        var image = Ready();
        image.CheckForRestart = () => true;
        string?[]? execArgv = null;
        image.GetEnv = name =>
        {
            Assert.Equal("APPIMAGE", name);
            return "";
        };
        image.Open = (path, flags) =>
        {
            Assert.Equal("", path);
            Assert.Equal(0, flags);
            return -1;
        };
        image.Fexecve = (fd, args, environ) =>
        {
            Assert.Equal(-1, fd);
            Assert.True(environ);
            execArgv = args;
        };
        var imageArgv = new[] { "hcde", "-a" };
        Assert.Equal(7, image.Main(2, imageArgv));
        Assert.Same(imageArgv, execArgv);
        Assert.Equal("", Assert.Single(image.Opens).Path);
        Assert.DoesNotContain(image.Opens, row => row.Path == "/proc/self/exe");
        Assert.Equal(1, image.QuitCount);
        Assert.Equal(
            new[]
            {
                "SDL_I_CheckForRestart", "I_TryRestart", "getenv", "open", "fexecve",
                "SDL_SetRelativeMouseMode", "SDL_Quit",
            },
            image.Trace.TakeLast(7).ToArray());

        var self = Ready();
        self.CheckForRestart = () => true;
        self.GetEnv = _ => null;
        self.Open = (path, flags) =>
        {
            Assert.Equal("/proc/self/exe", path);
            Assert.Equal(0, flags);
            return 8;
        };
        self.Main(1, new[] { "hcde" });
        Assert.Equal(("/proc/self/exe", 0, 8), Assert.Single(self.Opens));
        Assert.Equal(8, Assert.Single(self.Execs).Fd);

        var macro = Ready();
        macro.UseProgdir = true;
        macro.ProgdirMacro = "/opt/macro/";
        macro.Realpath = _ => throw new InvalidOperationException("realpath");
        macro.Main(0, null);
        Assert.Equal("/opt/macro/", macro.Progdir);
        Assert.NotNull(macro.Args);
        Assert.Empty(macro.Args);
        Assert.DoesNotContain("realpath", macro.Trace);
        var blankMacro = Ready();
        blankMacro.UseProgdir = true;
        blankMacro.ProgdirMacro = "";
        blankMacro.Main(1, new[] { "hcde" });
        Assert.Equal("", blankMacro.Progdir);

        var copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "hcde" });
        Assert.Equal("./", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "dir/hcde" });
        Assert.Equal("dir/", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "/hcde" });
        Assert.Equal("/", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "a/b/c" });
        Assert.Equal("a/b/", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "a/b/" });
        Assert.Equal("a/b/", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "//foo" });
        Assert.Equal("//", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(1, new[] { "a\\b" });
        Assert.Equal("./", copied.Progdir);
        copied = Ready();
        copied.Realpath = _ => null;
        copied.Main(0, new[] { "hcde" });
        Assert.NotNull(copied.Args);
        Assert.Empty(copied.Args);
        Assert.Equal("./", copied.Progdir);

        var longPath = Ready();
        longPath.Realpath = _ => new string('x', 4096);
        var program = Assert.Throws<InvalidOperationException>(() => longPath.Main(1, new[] { "hcde" }));
        Assert.Equal("program", program.Message);
        Assert.Equal("", longPath.Progdir);
        Assert.Equal(0, longPath.QuitCount);
        Assert.DoesNotContain("I_StartupJoysticks", longPath.Trace);
        var fit = Ready();
        fit.Realpath = _ => new string('x', 4095);
        fit.Main(1, new[] { "hcde" });
        Assert.Equal("./", fit.Progdir);

        var sdl = Ready();
        sdl.SdlInit = _ => -5;
        sdl.SdlGetError = () => "no display";
        Assert.Equal(-1, sdl.Main(1, new[] { "hcde" }));
        Assert.Null(sdl.Args);
        Assert.Equal("", sdl.Progdir);
        Assert.Equal(0, sdl.QuitCount);
        Assert.Equal("Could not initialize SDL:\nno display\n", Assert.Single(sdl.Stderr));
        Assert.Contains("printf", sdl.Trace);
        Assert.Contains("setlocale", sdl.Trace);
        Assert.DoesNotContain("FArgs", sdl.Trace);
        Assert.DoesNotContain("SDL_Quit", sdl.Trace);
        var positive = Ready();
        positive.SdlInit = _ => 1;
        positive.GameMain = () => 9;
        Assert.Equal(9, positive.Main(1, new[] { "hcde" }));
        var nullError = Ready();
        nullError.SdlInit = _ => -1;
        nullError.SdlGetError = () => null;
        var error = Assert.Throws<InvalidOperationException>(() => nullError.Main(1, new[] { "hcde" }));
        Assert.Equal("SDL_GetError", error.Message);
        Assert.Empty(nullError.Stderr);
        var missingError = Ready();
        missingError.SdlInit = _ => -1;
        missingError.SdlGetError = null;
        Assert.Equal(
            "SDL_GetError",
            Assert.Throws<InvalidOperationException>(() => missingError.Main(1, new[] { "hcde" })).Message);

        var holes = Ready();
        holes.Realpath = _ => null;
        holes.Main(2, new string?[] { "hcde", null });
        Assert.Equal(new[] { "hcde", "" }, holes.Args);
        var wideArgv = Ready();
        var range = Assert.Throws<IndexOutOfRangeException>(() => wideArgv.Main(3, new[] { "a", "b" }));
        Assert.Equal("argv", range.Message);
        Assert.Null(wideArgv.Args);
        Assert.Equal(0, wideArgv.QuitCount);
        var negative = Ready();
        Assert.Equal(
            "argv",
            Assert.Throws<IndexOutOfRangeException>(() => negative.Main(-1, new[] { "a" })).Message);
        var nullSlot = Ready();
        nullSlot.Realpath = _ => throw new InvalidOperationException("called");
        var nullArg = Assert.Throws<InvalidOperationException>(() => nullSlot.Main(1, new string?[] { null }));
        Assert.Equal("argv", nullArg.Message);
        Assert.Equal(new[] { "" }, nullSlot.Args);
        Assert.DoesNotContain("realpath", nullSlot.Trace);
        var argcZero = Ready();
        argcZero.Realpath = _ => "unused";
        var zero = Assert.Throws<InvalidOperationException>(() => argcZero.Main(0, null));
        Assert.Equal("argv", zero.Message);
        Assert.NotNull(argcZero.Args);
        Assert.Empty(argcZero.Args);
        Assert.Equal("", argcZero.Progdir);
        Assert.Equal(0, argcZero.QuitCount);

        var game = Ready();
        game.GameMain = null;
        Assert.Equal("GameMain", Assert.Throws<InvalidOperationException>(() => game.Main(1, new[] { "hcde" })).Message);
        Assert.Contains("I_StartupJoysticks", game.Trace);
        Assert.DoesNotContain("SDL_I_CheckForRestart", game.Trace);
        Assert.Equal(0, game.QuitCount);
        var check = Ready();
        check.CheckForRestart = null;
        Assert.Equal(
            "SDL_I_CheckForRestart",
            Assert.Throws<InvalidOperationException>(() => check.Main(1, new[] { "hcde" })).Message);
        Assert.DoesNotContain("SDL_SetRelativeMouseMode", check.Trace);
        var mouse = Ready();
        mouse.SetRelativeMouseMode = null;
        Assert.Equal(
            "SDL_SetRelativeMouseMode",
            Assert.Throws<InvalidOperationException>(() => mouse.Main(1, new[] { "hcde" })).Message);
        Assert.Equal(0, mouse.QuitCount);
        var quit = Ready();
        quit.SdlQuit = null;
        Assert.Equal("SDL_Quit", Assert.Throws<InvalidOperationException>(() => quit.Main(1, new[] { "hcde" })).Message);
        Assert.Equal(0, quit.MouseMode);
        Assert.Equal(0, quit.QuitCount);
        var sticks = Ready();
        sticks.StartupJoysticks = null;
        Assert.Equal(
            "I_StartupJoysticks",
            Assert.Throws<InvalidOperationException>(() => sticks.Main(1, new[] { "hcde" })).Message);
        Assert.Contains("progdir", sticks.Trace);
        Assert.DoesNotContain("GameMain", sticks.Trace);
        var uid = Ready();
        uid.GetUid = null;
        Assert.Equal("getuid", Assert.Throws<InvalidOperationException>(() => uid.Main(1, new[] { "hcde" })).Message);
        Assert.Contains("printf", uid.Trace);
        Assert.DoesNotContain("seteuid", uid.Trace);
        var handlers = Ready();
        handlers.InstallHandlers = null;
        Assert.Equal(
            "cc_install_handlers",
            Assert.Throws<InvalidOperationException>(() => handlers.Main(1, new[] { "hcde" })).Message);
        Assert.Empty(handlers.Signals);

        var returned = Ready();
        returned.GameMain = () => -3;
        returned.CheckForRestart = () => true;
        returned.GetEnv = _ => "app.img";
        returned.Open = (path, _) =>
        {
            Assert.Equal("app.img", path);
            return 5;
        };
        Assert.Equal(-3, returned.Main(1, new[] { "hcde" }));
        Assert.Equal(1, returned.QuitCount);
        Assert.Equal(0, returned.MouseMode);
        var opener = Ready();
        opener.CheckForRestart = () => true;
        opener.GetEnv = _ => null;
        opener.Open = null;
        Assert.Equal("open", Assert.Throws<InvalidOperationException>(() => opener.Main(1, new[] { "hcde" })).Message);
        Assert.Empty(opener.Execs);
        Assert.Equal(0, opener.QuitCount);
        var env = Ready();
        env.CheckForRestart = () => true;
        env.GetEnv = null;
        Assert.Equal("getenv", Assert.Throws<InvalidOperationException>(() => env.Main(1, new[] { "hcde" })).Message);
        Assert.Empty(env.Opens);
    }

    private static VmSdlMain1365Slice Os(
        Func<string, int> stat,
        Func<string, byte[]?> popen,
        Func<VmSdlMain1365Slice.UtsName?>? uname = null)
    {
        return new VmSdlMain1365Slice
        {
            Stat = stat,
            Popen = popen,
            Pclose = _ => { },
            Uname = uname ?? (() => null),
        };
    }

    private static VmSdlMain1365Slice Ready()
    {
        return new VmSdlMain1365Slice
        {
            VersionString = "0.4.12",
            GitTime = "2026-01-01",
            CompileDate = "Oct 31 2026",
            InstallHandlers = (_, _, _, _, _, _) => { },
            Signal = (_, _) => { },
            GetUid = () => 1000,
            SetEuid = _ => { },
            SetEnv = (_, _, _) => { },
            SetLocale = (_, _) => { },
            SdlInit = _ => 0,
            SdlGetError = () => "err",
            Realpath = _ => "/opt/hcde/hcde",
            StartupJoysticks = () => { },
            GameMain = () => 7,
            CheckForRestart = () => false,
            SetRelativeMouseMode = _ => { },
            SdlQuit = () => { },
            GetEnv = _ => null,
            Open = (_, _) => 3,
            Fexecve = (_, _, _) => { },
        };
    }
}
