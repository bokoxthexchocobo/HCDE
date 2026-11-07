using System.Globalization;
namespace HCDE.Playsim;
/// <summary>
/// Title demo advance, the endgame command, and CVARINFO parsing from d_main.cpp
/// 1950-2241. Chunk 1414. ConsiderPatches starts at line 2251.
/// </summary>
public static class VmDMain1414Slice
{
    public const int BandStart = 1950;
    public const int BandEnd = 2241;
    public const int ConsiderPatchesStart = 2251;
    public const int TicRate = 35;
    public const int Nothing = 0;
    public const int FullConsole = 17;
    public const int Live = 0;
    public const int DemoScreen = 3;
    public const int Strife = 8;
    public const int GlobalNamespace = 0;
    public const int Archive = 1;
    public const int User = 1 << 1;
    public const int Server = 1 << 2;
    public const int Latch = 1 << 4;
    public const int Mod = 1 << 13;
    public const int Cheat = 1 << 15;
    public const int ConfigOnly = 1 << 18;
    public const int DefaultFlags = Mod | Archive;
    public const int TypeBool = 0;
    public const int TypeInt = 1;
    public const int TypeFloat = 2;
    public const int TypeString = 3;
    public const int TypeColor = 4;
    public const int TypeDummy = 7;
    public const int TkIdentifier = 256, TkBool = 257, TkInt = 258, TkFloat = 259;
    public const int TkColor = 260, TkString = 261, TkTrue = 262, TkFalse = 263;
    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }
    }
    public sealed class Token
    {
        public int Kind;
        public string Text = "";
        public Token(int kind, string text) { Kind = kind; Text = text; }
    }
    public sealed class Lump
    {
        public string Name = "";
        public int Namespace;
        public List<Token> Tokens = new();
    }
    public sealed class Cvar
    {
        public string Name = "";
        public int Type;
        public int Flags;
        public bool Custom;
        public string ClassName = "";
        public string? Default;
    }
    public sealed class State
    {
        public bool AdvancedDemo = true;
        public int GameAction;
        public int ConsolePlayer;
        public List<int> PlayerStates = new();
        public bool UserGame = true;
        public int Paused = 4;
        public bool TitleMap;
        public string? StartedMap;
        public bool StartedMapTitle;
        public int GameType;
        public int StrifeAdvances;
        public int DemoSequence = 3;
        public float AdvisoryTime;
        public float TitleTime = 1;
        public float PageTime = 1;
        public string Advisory = "old";
        public bool AdvisoryValid = true;
        public bool DemoNoPlay;
        public int DemoCount;
        public string DemoName = "DEMO1";
        public List<string> DemoLumps = new();
        public bool SingleDemo = true;
        public string? PlayedDemo;
        public int GameState = 1;
        public string TitlePage = "TITLEPIC";
        public string TitleMusic = "d_intro";
        public int TitleOrder = 2;
        public bool PlayedTitleMusic;
        public int PageCount = 4;
        public List<string> CreditPages = new();
        public string Page = "old-page";
        public int HideConsole;
        public bool NetGame;
        public int DemoChecks;
        public string ConfigName = "HCDE";
        public List<Lump> Lumps = new();
        public HashSet<string> Existing = new(StringComparer.Ordinal);
        public int CModeSets;
        public readonly List<Cvar> Cvars = new();
        public readonly List<string> ModSetups = new();
        public readonly List<string> Steps = new();
    }
    public static void DoAdvanceDemo(State state)
    {
        state.AdvancedDemo = false;
        if (state.GameAction != Nothing)
            return;
        int old = state.PlayerStates[state.ConsolePlayer];
        state.PlayerStates[state.ConsolePlayer] = Live;
        state.Steps.Add(
            "player=" + state.ConsolePlayer + " " + PlayerStateName(old)
            + " -> " + PlayerStateName(Live) + " reason=Title_screen_start");
        state.UserGame = false;
        state.Paused = 0;
        if (state.TitleMap)
        {
            state.StartedMap = "TITLEMAP";
            state.StartedMapTitle = true;
            return;
        }
        if (state.GameType == Strife)
        {
            state.StrifeAdvances++;
            return;
        }
        string pageName = "";
        switch (state.DemoSequence)
        {
            case 3:
                if (state.AdvisoryTime != 0f)
                {
                    state.Advisory = "ADVISOR";
                    state.AdvisoryValid = true;
                    state.DemoSequence = 1;
                    PageTic(state, state.AdvisoryTime);
                    break;
                }
                goto case 1;
            case 1:
                state.AdvisoryValid = false;
                if (!state.DemoNoPlay)
                {
                    state.DemoCount++;
                    state.DemoName = "DEMO" + TrimDemoDigits(state.DemoCount);
                    if (!HasDemo(state, state.DemoName))
                    {
                        state.DemoSequence = 0;
                        state.DemoCount = 0;
                        goto default;
                    }
                    state.SingleDemo = false;
                    state.PlayedDemo = state.DemoName;
                    state.DemoSequence = 2;
                    break;
                }
                goto default;
            default:
                state.GameState = DemoScreen;
                pageName = state.TitlePage;
                PageTic(state, state.TitleTime);
                if (!state.PlayedTitleMusic)
                    state.Steps.Add("music:" + state.TitleMusic + ":" + state.TitleOrder + ":false");
                state.PlayedTitleMusic = true;
                state.DemoSequence = 3;
                state.PageCount = 0;
                state.HideConsole++;
                break;
            case 2:
                PageTic(state, state.PageTime);
                state.GameState = DemoScreen;
                if (state.CreditPages.Count > 0)
                {
                    pageName = state.CreditPages[state.PageCount];
                    state.PageCount = (state.PageCount + 1) % state.CreditPages.Count;
                }
                state.DemoSequence = 1;
                break;
        }
        if (pageName.Length > 0)
            state.Page = pageName;
    }
    public static void StartTitle(State state)
    {
        state.PlayedTitleMusic = false;
        state.GameAction = Nothing;
        state.DemoSequence = -1;
        state.AdvancedDemo = true;
    }
    public static void Endgame(State state)
    {
        if (state.NetGame)
            return;
        state.GameAction = FullConsole;
        state.DemoSequence = -1;
        state.DemoChecks++;
    }
    public static void ParseCVarInfo(State state)
    {
        int last = 0;
        bool added = false;
        while (true)
        {
            int lump = Find(state, ref last);
            if (lump < 0)
                break;
            var scan = new Scan(state.Lumps[lump].Tokens);
            state.CModeSets++;
            while (scan.Get())
            {
                int flags = DefaultFlags;
                bool custom = false;
                string className = "";
                while (scan.Kind == TkIdentifier)
                {
                    string word = scan.Text;
                    if (Same(word, "server"))
                        flags |= Server;
                    else if (Same(word, "user"))
                        flags |= User;
                    else if (Same(word, "noarchive"))
                        flags &= ~Archive;
                    else if (Same(word, "cheat"))
                        flags |= Cheat;
                    else if (Same(word, "latch"))
                        flags |= Latch;
                    else if (Same(word, "nosave"))
                        flags |= ConfigOnly;
                    else if (Same(word, "handlerClass"))
                    {
                        scan.MustName("(");
                        scan.MustString();
                        custom = true;
                        className = scan.Text;
                        scan.MustName(")");
                    }
                    else
                        scan.Fail("Unknown cvar attribute '" + word + "'");
                    scan.MustAny();
                }
                if ((flags & ConfigOnly) != 0)
                    flags &= ~(Server | User);
                if ((flags & (Server | User | ConfigOnly)) == 0
                    || (flags & (Server | User)) == (Server | User))
                    scan.Fail("One of 'server', 'user', or 'nosave' must be specified");
                int type = TypeDummy;
                if (scan.Kind == TkBool)
                    type = TypeBool;
                else if (scan.Kind == TkInt)
                    type = TypeInt;
                else if (scan.Kind == TkFloat)
                    type = TypeFloat;
                else if (scan.Kind == TkColor)
                    type = TypeColor;
                else if (scan.Kind == TkString)
                    type = TypeString;
                else
                    scan.Fail("Bad cvar type '" + scan.Text + "'");
                scan.Must(TkIdentifier);
                string name = scan.Text;
                if (state.Existing.Contains(name))
                    scan.Fail("cvar '" + name + "' already exists");
                string? literal = null;
                if (scan.Check('='))
                {
                    if (type == TypeBool)
                    {
                        if (!scan.Check(TkTrue) && !scan.Check(TkFalse))
                            scan.Fail("Expected true or false");
                        literal = scan.Text;
                    }
                    else if (type == TypeInt)
                    {
                        scan.MustNumber();
                        literal = scan.Text;
                    }
                    else if (type == TypeFloat)
                    {
                        scan.MustFloat();
                        literal = scan.Text;
                    }
                    else
                    {
                        scan.MustString();
                        literal = scan.Text;
                    }
                }
                state.Cvars.Add(new Cvar
                {
                    Name = name,
                    Type = type,
                    Flags = flags,
                    Custom = custom,
                    ClassName = className,
                    Default = literal,
                });
                state.Existing.Add(name);
                scan.Must(';');
                added = true;
            }
        }
        if (added)
            state.ModSetups.Add(state.ConfigName);
    }
    private static void PageTic(State state, float seconds)
    {
        state.Steps.Add("tic:" + (int)(seconds * TicRate));
    }
    private static string TrimDemoDigits(int count)
    {
        string digits = count.ToString(CultureInfo.InvariantCulture);
        return digits.Length > 3 ? digits.Substring(0, 3) : digits;
    }
    private static int Find(State state, ref int last)
    {
        if ((uint)last >= (uint)state.Lumps.Count)
        {
            last = state.Lumps.Count;
            return -1;
        }
        for (int i = last; i < state.Lumps.Count; i++)
        {
            Lump lump = state.Lumps[i];
            if (lump.Namespace == GlobalNamespace && Same(lump.Name, "CVARINFO"))
            {
                last = i + 1;
                return i;
            }
        }
        last = state.Lumps.Count;
        return -1;
    }
    private static bool HasDemo(State state, string name)
    {
        foreach (string lump in state.DemoLumps)
        {
            if (Same(lump, name))
                return true;
        }
        return false;
    }
    private static bool Same(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
    private static string PlayerStateName(int state) => state switch
    {
        0 => "PST_LIVE",
        1 => "PST_DEAD",
        2 => "PST_REBORN",
        3 => "PST_ENTER",
        4 => "PST_GONE",
        _ => "PST_UNKNOWN",
    };
    private sealed class Scan
    {
        private readonly List<Token> tokens;
        private int index;
        private bool ungot;
        private bool has;
        public int Kind;
        public string Text = "";
        public Scan(List<Token> tokens) { this.tokens = tokens; }
        public bool Get()
        {
            if (ungot)
            {
                ungot = false;
                return has;
            }
            if (index >= tokens.Count)
            {
                has = false;
                return false;
            }
            Token token = tokens[index++];
            Kind = token.Kind;
            Text = token.Text;
            has = true;
            return true;
        }
        public bool Check(int kind)
        {
            if (!Get())
                return false;
            if (Kind == kind)
                return true;
            ungot = true;
            return false;
        }
        public void MustAny()
        {
            if (!Get())
                Fail("Missing token (unexpected end of file).");
        }
        public void Must(int kind)
        {
            MustAny();
            if (Kind != kind)
                Fail("Expected '" + kind + "', got '" + Text + "'.");
        }
        public void MustName(string name)
        {
            MustString();
            if (!Same(Text, name))
                Fail("Expected '" + name + "', got '" + Text + "'.");
        }
        public void MustString()
        {
            if (!Get())
                Fail("Missing string (unexpected end of file).");
        }
        public void MustNumber()
        {
            if (!Get())
                Fail("Missing integer (unexpected end of file).");
            if (Text == "MAXINT")
                return;
            if (!int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                Fail("SC_GetNumber: Bad numeric constant \"" + Text + "\".");
        }
        public void MustFloat()
        {
            if (!Get())
                Fail("Missing floating-point number (unexpected end of file).");
            if (!float.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                Fail("SC_GetFloat: Bad numeric constant \"" + Text + "\".");
        }
        public void Fail(string message)
        {
            throw new ScriptError(message);
        }
    }
}
