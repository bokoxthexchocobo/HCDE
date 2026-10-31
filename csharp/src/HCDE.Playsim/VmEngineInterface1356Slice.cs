using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Engine interface from i_interface.cpp (1-257), through end of file.
/// UpdateLanguage, UpdateGenericUI, and HCDE_MigrateRendererCvars stay injected.
/// UUID draws are consumed in the g++ right-to-left argument order.
/// </summary>
public static class VmEngineInterface1356Slice
{
    public const int GsStartup = 6;
    public const int ArchiveGlobal = 1 | (1 << 10);
    public const int LanguageFlags = ArchiveGlobal | (1 << 9);
    public const string AddonSeparator = "|";
    public const string UuidRngName = "GameUUID";
    public const uint UuidShortBound = 65535;
    public const uint UuidNibbleBound = 4095;

    public static void RequireLp64(int pointerBytes)
    {
        if (pointerBytes == 8)
            return;
        throw new InvalidOperationException("Only LP64/LLP64 builds are officially supported.");
    }

    public sealed record Decl(string Name, string Kind, int Flags, object Value, bool Callback);

    public static readonly Decl[] Declarations =
    {
        new("vid_fps", "Bool", 0, false, false),
        new("queryiwad", "Bool", ArchiveGlobal, true, false),
        new("saveargs", "Bool", ArchiveGlobal, true, false),
        new("savenetfile", "Bool", ArchiveGlobal, false, false),
        new("savenetargs", "Bool", ArchiveGlobal, true, false),
        new("defaultiwad", "String", ArchiveGlobal, "", false),
        new("defaultaddonfiles", "String", ArchiveGlobal, "", false),
        new("defaultargs", "String", ArchiveGlobal, "", false),
        new("defaultnetiwad", "String", ArchiveGlobal, "", false),
        new("defaultnetargs", "String", ArchiveGlobal, "", false),
        new("defaultnetplayers", "Int", ArchiveGlobal, 8, false),
        new("defaultnethostport", "Int", ArchiveGlobal, 0, false),
        new("defaultnetticdup", "Int", ArchiveGlobal, 0, false),
        new("defaultnetgamemode", "Int", ArchiveGlobal, 0, false),
        new("defaultnetaltdm", "Bool", ArchiveGlobal, false, false),
        new("defaultnetaddress", "String", ArchiveGlobal, "", false),
        new("defaultnetjoinport", "Int", ArchiveGlobal, 0, false),
        new("defaultnetpage", "Int", ArchiveGlobal, 0, false),
        new("defaultnethostteam", "Int", ArchiveGlobal, 255, false),
        new("defaultnetjointeam", "Int", ArchiveGlobal, 255, false),
        new("defaultnetextratic", "Bool", ArchiveGlobal, false, false),
        new("defaultnetsavefile", "String", ArchiveGlobal, "", false),
        new("ui_colors", "String", ArchiveGlobal, "", false),
        new("ui_color_mix", "Float", ArchiveGlobal, (float)0.35, false),
        new("language", "String", LanguageFlags, "auto", true),
    };

    public sealed class ArgSwitch
    {
        public string Name = "";
        public string Section = "";
        public string Summary = "";
        public string Usage = "";
        public string Details = "";
        public bool Advanced;
    }

    public static readonly ArgSwitch Pride = new()
    {
        Name = "-pride",
        Section = "Launcher",
        Summary = "Show pride colors",
        Usage = "",
        Details = "Show pride colors in launcher.",
        Advanced = false,
    };

    public sealed class StartupInfo
    {
        public string Name = "";
        public uint FgColor;
        public uint BkColor;
        public string Song = "";
        public string Con = "";
        public string Def = "";
        public string SteamAppId = "";
        public int Type;
        public int LoadLights = -1;
        public int LoadBrightmaps = -1;
        public int LoadWidescreen = -1;
    }

    public sealed class Hooks
    {
        public Action<string>? UpdateLanguage;
        public Action<bool>? UpdateGenericUi;
        public Action? MigrateRenderer;
        public Func<uint>? Next32;
        public Func<uint, uint>? NextBound;
    }

    public sealed class Vars
    {
        public Dictionary<string, object> Values = new();
        public List<string> Assigned = new();

        public Vars()
        {
            foreach (var decl in Declarations)
                Values[decl.Name] = decl.Value;
            Values["ui_generic"] = false;
            Values["vid_preferbackend"] = 0;
            Values["vid_fullscreen"] = false;
        }

        public bool Bool(string name) => (bool)Values[name];
        public int Int(string name) => (int)Values[name];
        public float Float(string name) => (float)Values[name];
        public string Text(string name) => (string)Values[name];

        public void Set(string name, object value)
        {
            Assigned.Add(name);
            Values[name] = value;
        }
    }

    public sealed class Engine
    {
        public object? StartWindow;
        public Action<string>? LanguageChanged;
        public string EndoomName = "";
        public bool BatchRun;
        public float MenuBlur;
        public bool AppActive = true;
        public int ChatMode;
        public int GameState = GsStartup;
        public bool ToggleFullscreen;
        public int Paused;
        public bool PauseExt;
        public StartupInfo Startup = new();
        public string GameUuid = "";
        public Vars Cvars = new();
        public Hooks Hooks = new();
    }

    public sealed class Wad
    {
        public string Path = "";
        public string Name = "";
    }

    public sealed class Args
    {
        public Func<string, int>? CheckParm;
        public Action<string>? AppendRaw;
    }

    public static void OnLanguage(Engine engine, string self, string prev)
    {
        _ = prev;
        if (engine.Hooks.UpdateLanguage == null)
            throw new InvalidOperationException("UpdateLanguage");
        engine.Hooks.UpdateLanguage(self);
        if (engine.Hooks.UpdateGenericUi == null)
            throw new InvalidOperationException("UpdateGenericUI");
        engine.Hooks.UpdateGenericUi(engine.Cvars.Bool("ui_generic"));
        if (engine.LanguageChanged != null)
            engine.LanguageChanged(self);
    }

    public static bool IsSpace(char c)
    {
        if (c > 127)
            return false;
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    public static string StripLeftRight(string text)
    {
        int max = text.Length;
        if (max == 0)
            return text;
        int i = 0;
        while (i < max && IsSpace(text[i]))
            i++;
        int j = max - 1;
        if (i < max)
        {
            while (j >= i && IsSpace(text[j]))
                j--;
        }
        if (i == 0 && j == max - 1)
            return text;
        if (i > j)
            return "";
        return text.Substring(i, j - i + 1);
    }

    public static string Substitute(string text, string oldText, string newText)
    {
        if (oldText.Length == 0)
            return text;
        var built = new System.Text.StringBuilder();
        int cursor = 0;
        while (cursor < text.Length)
        {
            int pos = text.IndexOf(oldText, cursor, StringComparison.Ordinal);
            if (pos < 0)
            {
                built.Append(text, cursor, text.Length - cursor);
                break;
            }
            built.Append(text, cursor, pos - cursor);
            built.Append(newText);
            cursor = pos + oldText.Length;
        }
        return built.ToString();
    }

    public static string QuoteStartupArg(string arg)
    {
        return "\"" + Substitute(arg, "\"", "\\\"") + "\"";
    }

    public static List<string> SplitSkipEmpty(string source, string delimiter)
    {
        var tokens = new List<string>();
        if (source.Length == 0)
            return tokens;
        int last = 0;
        int len = source.Length;
        int delim = delimiter.Length;
        while (last <= len)
        {
            int pos = source.IndexOf(delimiter, last, StringComparison.Ordinal);
            if (pos < 0)
                pos = len;
            if (pos != last)
                tokens.Add(source.Substring(last, pos - last));
            last = pos + delim;
        }
        return tokens;
    }

    public static string BuildStartupFileArgs(string addonFiles)
    {
        string args = "";
        foreach (var raw in SplitSkipEmpty(addonFiles, AddonSeparator))
        {
            string file = StripLeftRight(raw);
            if (file.Length == 0)
                continue;
            if (args.Length != 0)
                args += " ";
            args += "-file ";
            args += QuoteStartupArg(file);
        }
        return args;
    }

    public static bool OpensWithText(string text)
    {
        return text.Length > 0 && text[0] != '\0';
    }

    public static int CompareNoCase(string left, string right)
    {
        int i = 0;
        while (true)
        {
            int a = i < left.Length ? left[i] : 0;
            int b = i < right.Length ? right[i] : 0;
            if (a == 0 || b == 0)
                return AsciiLower(a) - AsciiLower(b);
            int la = AsciiLower(a);
            int lb = AsciiLower(b);
            if (la != lb)
                return la - lb;
            i++;
        }
    }

    static int AsciiLower(int c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }

    public static string CString(string text)
    {
        int cut = text.IndexOf('\0');
        if (cut < 0)
            return text;
        return text.Substring(0, cut);
    }

    public sealed class Selection
    {
        public List<Wad>? Wads;
        public Args? Args;
        public int DefaultIwad;
        public string DefaultAddonFiles = "";
        public string DefaultArgs = "";
        public bool SaveArgs = true;
        public bool IsNewRelease = true;
        public int DefaultStartFlags;
        public bool DefaultQueryIwad = true;
        public string DefaultLanguage = "auto";
        public int DefaultBackend = 1;
        public bool DefaultFullscreen = true;
        public int DefaultFileLoadBehaviour;
        public bool NotifyNewRelease = true;
        public string PrideColors = "";
        public float PrideMix;
        public int DefaultNetIwad;
        public bool NetStart;
        public bool Hosting;
        public bool SaveNetFile;
        public bool SaveNetArgs = true;
        public int DefaultNetPage;
        public string DefaultNetArgs = "";
        public string AdditionalNetArgs = "";
        public string DefaultNetSaveFile = "";
        public int DefaultNetHostTeam = 255;
        public int DefaultNetPlayers = 8;
        public int DefaultNetHostPort;
        public int DefaultNetTicDup;
        public bool DefaultNetExtraTic;
        public int DefaultNetGameMode;
        public bool DefaultNetAltDm;
        public string DefaultNetAddress = "";
        public int DefaultNetJoinPort;
        public int DefaultNetJoinTeam = 255;

        public Selection(Engine engine, List<Wad>? wads, Args? args, int startFlags)
        {
            Wads = wads;
            Args = args;
            DefaultStartFlags = startFlags;
            var cvars = engine.Cvars;
            DefaultQueryIwad = cvars.Bool("queryiwad");
            DefaultLanguage = cvars.Text("language");
            if (engine.Hooks.MigrateRenderer == null)
                throw new InvalidOperationException("HCDE_MigrateRendererCvars");
            engine.Hooks.MigrateRenderer();
            DefaultBackend = cvars.Int("vid_preferbackend");
            DefaultFullscreen = cvars.Bool("vid_fullscreen");
            if (OpensWithText(cvars.Text("defaultiwad")))
                TryAssignIwad(wads, cvars.Text("defaultiwad"), net: false);
            DefaultAddonFiles = cvars.Text("defaultaddonfiles");
            DefaultArgs = cvars.Text("defaultargs");
            SaveArgs = cvars.Bool("saveargs");
            if (OpensWithText(cvars.Text("defaultnetiwad")))
                TryAssignIwad(wads, cvars.Text("defaultnetiwad"), net: true);
            DefaultNetArgs = cvars.Text("defaultnetargs");
            DefaultNetPage = cvars.Int("defaultnetpage");
            DefaultNetSaveFile = cvars.Text("defaultnetsavefile");
            SaveNetFile = cvars.Bool("savenetfile");
            SaveNetArgs = cvars.Bool("savenetargs");
            DefaultNetPlayers = cvars.Int("defaultnetplayers");
            DefaultNetHostPort = cvars.Int("defaultnethostport");
            DefaultNetTicDup = cvars.Int("defaultnetticdup");
            DefaultNetGameMode = cvars.Int("defaultnetgamemode");
            DefaultNetAltDm = cvars.Bool("defaultnetaltdm");
            DefaultNetHostTeam = cvars.Int("defaultnethostteam");
            DefaultNetExtraTic = cvars.Bool("defaultnetextratic");
            DefaultNetAddress = cvars.Text("defaultnetaddress");
            DefaultNetJoinPort = cvars.Int("defaultnetjoinport");
            DefaultNetJoinTeam = cvars.Int("defaultnetjointeam");
            if (args == null)
                throw new NullReferenceException("Args");
            if (args.CheckParm == null)
                throw new InvalidOperationException("CheckParm");
            int slot = args.CheckParm(Pride.Name);
            PrideColors = slot != 0 ? "list" : cvars.Text("ui_colors");
            PrideMix = cvars.Float("ui_color_mix");
        }

        void TryAssignIwad(List<Wad>? wads, string wanted, bool net)
        {
            if (wads == null)
                throw new NullReferenceException("Wads");
            for (int i = 0; i < wads.Count; i++)
            {
                if (CompareNoCase(wads[i].Name, wanted) != 0)
                    continue;
                if (net)
                    DefaultNetIwad = i;
                else
                    DefaultIwad = i;
                return;
            }
        }

        public int SaveInfo(Engine engine)
        {
            DefaultLanguage = StripLeftRight(DefaultLanguage);
            DefaultAddonFiles = StripLeftRight(DefaultAddonFiles);
            DefaultArgs = StripLeftRight(DefaultArgs);
            DefaultNetArgs = StripLeftRight(DefaultNetArgs);
            AdditionalNetArgs = StripLeftRight(AdditionalNetArgs);
            DefaultNetAddress = StripLeftRight(DefaultNetAddress);
            DefaultNetSaveFile = StripLeftRight(DefaultNetSaveFile);
            var cvars = engine.Cvars;
            cvars.Set("queryiwad", DefaultQueryIwad);
            cvars.Set("language", CString(DefaultLanguage));
            cvars.Set("vid_fullscreen", DefaultFullscreen);
            if (DefaultBackend != cvars.Int("vid_preferbackend"))
                cvars.Set("vid_preferbackend", DefaultBackend);
            if (!NetStart)
                return SaveLocal(cvars);
            return SaveNet(cvars);
        }

        int SaveNet(Vars cvars)
        {
            cvars.Set("savenetfile", SaveNetFile);
            cvars.Set("savenetargs", SaveNetArgs);
            cvars.Set("defaultnetiwad", CString(At(Wads, DefaultNetIwad).Name));
            cvars.Set("defaultnetpage", DefaultNetPage);
            cvars.Set("defaultnetsavefile", SaveNetFile ? CString(DefaultNetSaveFile) : "");
            cvars.Set("defaultnetargs", SaveNetArgs ? CString(DefaultNetArgs) : "");
            if (Hosting)
            {
                cvars.Set("defaultnetplayers", DefaultNetPlayers);
                cvars.Set("defaultnethostport", DefaultNetHostPort);
                cvars.Set("defaultnetticdup", DefaultNetTicDup);
                cvars.Set("defaultnetgamemode", DefaultNetGameMode);
                cvars.Set("defaultnetaltdm", DefaultNetAltDm);
                cvars.Set("defaultnethostteam", DefaultNetHostTeam);
                cvars.Set("defaultnetextratic", DefaultNetExtraTic);
            }
            else
            {
                cvars.Set("defaultnetaddress", CString(DefaultNetAddress));
                cvars.Set("defaultnetjoinport", DefaultNetJoinPort);
                cvars.Set("defaultnetjointeam", DefaultNetJoinTeam);
            }
            if (DefaultNetArgs.Length != 0)
                Append(Args, DefaultNetArgs);
            if (AdditionalNetArgs.Length != 0)
                Append(Args, AdditionalNetArgs);
            return DefaultNetIwad;
        }

        int SaveLocal(Vars cvars)
        {
            cvars.Set("defaultiwad", CString(At(Wads, DefaultIwad).Name));
            cvars.Set("saveargs", SaveArgs);
            cvars.Set("defaultaddonfiles", SaveArgs ? CString(DefaultAddonFiles) : "");
            cvars.Set("defaultargs", SaveArgs ? CString(DefaultArgs) : "");
            string localArgs = BuildStartupFileArgs(DefaultAddonFiles);
            if (DefaultArgs.Length != 0)
            {
                if (localArgs.Length != 0)
                    localArgs += " ";
                localArgs += DefaultArgs;
            }
            if (localArgs.Length != 0)
                Append(Args, localArgs);
            return DefaultIwad;
        }

        static Wad At(List<Wad>? wads, int index)
        {
            if (wads == null)
                throw new NullReferenceException("Wads");
            if (index < 0 || index >= wads.Count)
                throw new IndexOutOfRangeException("Wads");
            return wads[index];
        }

        static void Append(Args? args, string text)
        {
            if (args == null)
                throw new NullReferenceException("Args");
            if (args.AppendRaw == null)
                throw new InvalidOperationException("AppendRawArgsString");
            args.AppendRaw(text);
        }
    }

    public static string GenerateUuid(Engine engine)
    {
        uint lastShort = Bound(engine, UuidShortBound);
        uint secondWord = Next(engine);
        uint variant = Bound(engine, UuidNibbleBound);
        uint version = Bound(engine, UuidNibbleBound);
        uint firstShort = Bound(engine, UuidShortBound);
        uint firstWord = Next(engine);
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:X8}-{1:X4}-4{2:X3}-9{3:X3}-{4:X8}{5:X4}",
            firstWord, firstShort, version, variant, secondWord, lastShort);
    }

    static uint Next(Engine engine)
    {
        if (engine.Hooks.Next32 == null)
            throw new InvalidOperationException("GenRand32");
        return engine.Hooks.Next32();
    }

    static uint Bound(Engine engine, uint bound)
    {
        if (engine.Hooks.NextBound == null)
            throw new InvalidOperationException("GenRand32BoundExclusive");
        return engine.Hooks.NextBound(bound);
    }
}
