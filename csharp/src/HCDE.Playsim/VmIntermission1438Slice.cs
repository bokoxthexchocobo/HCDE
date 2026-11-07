namespace HCDE.Playsim;

/// <summary>
/// Controller run loop through measureintermissions from intermission.cpp 987-1077, 1086-1104, and 1113-1205.
/// NextPage is a callee. Comment banners 1078-1085 and 1105-1112 stay uncredited.
/// </summary>
public static class VmIntermission1438Slice
{
    public const int BandStart = 987;
    public const int BandEnd = 1205;
    public const int KeyDown = 1;
    public const int Nothing = 0;
    public const int Finale = 2;
    public const int InLevelNoWipe = 3;
    public const int Readied = 77;
    public const int MaxPlayers = 64;
    public const int Title = -2;
    public const uint DefaultTable = 0x85F90A15;

    public const string Texts =
        "E1TEXT,E2TEXT,E3TEXT,E4TEXT,C1TEXT,C2TEXT,C3TEXT,C4TEXT,C5TEXT,P1TEXT,P2TEXT,P3TEXT,P4TEXT,P5TEXT,T1TEXT,T2TEXT,T3TEXT,T4TEXT,T5TEXT,NERVETEXT,HE1TEXT,HE2TEXT,HE3TEXT,HE4TEXT,HE5TEXT,TXT_HEXEN_CLUS1MSG,TXT_HEXEN_CLUS2MSG,TXT_HEXEN_CLUS3MSG,TXT_HEXEN_CLUS4MSG,TXT_HEXEN_WIN1MSG,TXT_HEXEN_WIN2MSG,TXT_HEXEN_WIN3MSG,TXT_HEXDD_CLUS1MSG,TXT_HEXDD_CLUS2MSG,TXT_HEXDD_WIN1MSG,TXT_HEXDD_WIN2MSG,TXT_HEXDD_WIN3MSG";

    public const string Langs = ",cz,de,eng,es,esm,fr,hu,it,pl,pt,ro,ru,sr";

    public sealed class Act
    {
        public int Class = 1;
    }

    public sealed class Desc
    {
        public int Id;
        public bool Opens = true;
        public List<Act> Actions = new();
    }

    public sealed class Screen
    {
        public int Age = 2;
        public int Answer;
        public int TickResult;
        public int Calls;
        public int Ticked;
        public int Drawn;
        public int Started;
        public bool Dead;
    }

    public sealed class Ev
    {
        public int Type;
        public int Key;
    }

    public sealed class Ctrl
    {
        public Desc? Desc;
        public bool Delete;
        public bool EndGame;
        public bool Advance;
        public bool Sent = true;
        public int NextCalls;
        public Screen? Screen;
    }

    public sealed class World
    {
        public int Action = 9;
        public int State = 9;
        public int Wipe;
        public bool Stopped;
        public bool Net;
        public bool Demo;
        public int Console;
        public bool MenuOpen;
        public string Menu = "";
        public int MenuArg;
        public int Clears;
        public Dictionary<int, string> Binds = new();
        public List<int> Ready = new();
        public List<int> Bytes = new();
        public List<int> Deleted = new();
        public Dictionary<string, Desc> Links = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Lines = new();
        public Dictionary<string, int> Widths = new();
        public List<string> Notes = new();
    }

    public static void Begin(Ctrl c)
    {
        if (c.Screen != null) c.Screen.Started++;
    }

    public static bool Respond(World w, Ctrl c, Ev? ev)
    {
        if (c.Screen == null) return false;
        if (ev == null) throw new NullReferenceException();
        if (ev.Type == KeyDown && w.Binds.TryGetValue(ev.Key, out string? cmd) && cmd != null)
        {
            if (Same(cmd, "toggleconsole") || Same(cmd, "screenshot")) return false;
            if (Same(cmd, "menu_main") || Same(cmd, "pause"))
            {
                w.MenuOpen = true;
                w.Menu = "MainMenu";
                w.MenuArg = -1;
                return true;
            }
        }
        if (c.Screen.Age < 2) return false;
        if (w.Net && !w.Demo)
        {
            if (ev.Type != KeyDown) return false;
            if (w.Console >= 0 && w.Console < MaxPlayers) w.Ready.Add(w.Console);
            w.Bytes.Add(Readied);
            return true;
        }
        c.Screen.Calls++;
        if (c.Screen.Answer == -1) c.Advance = true;
        return c.Screen.Answer != 0;
    }

    public static bool Tick(Ctrl c)
    {
        if (c.Advance) c.Sent = false;
        if (c.Screen != null)
        {
            c.Screen.Ticked++;
            c.Advance |= c.Screen.TickResult == -1;
        }
        if (!c.Advance) return true;
        c.Advance = false;
        return Next(c);
    }

    public static void Draw(World w, Ctrl c)
    {
        if (c.Screen == null) return;
        w.Clears++;
        c.Screen.Drawn++;
    }

    public static void Destroy(World w, Ctrl c)
    {
        if (c.Screen != null)
        {
            c.Screen.Dead = true;
            c.Screen = null;
        }
        if (c.Delete && c.Desc != null) w.Deleted.Add(c.Desc.Id);
        c.Desc = null;
    }

    public static bool Next(Ctrl c)
    {
        c.NextCalls++;
        return c.Desc != null && c.Desc.Opens;
    }

    public static Ctrl? Start(World w, Desc desc, int state, bool delete, bool ending)
    {
        w.Stopped = true;
        w.Action = Nothing;
        w.State = Finale;
        if (state == InLevelNoWipe) w.Wipe = Finale;
        var ctrl = new Ctrl { Desc = desc, Delete = delete, EndGame = ending };
        if (!Next(ctrl))
        {
            Destroy(w, ctrl);
            return null;
        }
        return ctrl;
    }

    public static Ctrl? StartNamed(World w, string seq, int state)
    {
        if (!w.Links.TryGetValue(seq, out Desc? desc) || desc.Actions.Count == 0) return null;
        if (desc.Actions[0].Class == Title) return null;
        return Start(w, desc, state, false, false);
    }

    public static bool CallRespond(World w, Ctrl c, Ev? ev) => Respond(w, c, ev);

    public static bool CallTick(Ctrl c) => Tick(c);

    public static int CallBegin(Ctrl c)
    {
        Begin(c);
        return 0;
    }

    public static int CallDraw(World w, Ctrl c)
    {
        Draw(w, c);
        return 0;
    }

    public static bool CallNext(Ctrl c) => Next(c);

    public static void Measure(World w)
    {
        foreach (string lang in Langs.Split(','))
        {
            uint id = lang.Length == 0 ? DefaultTable : Make(lang);
            foreach (string name in Texts.Split(','))
            {
                if (!w.Lines.TryGetValue(name + ":" + id, out string? text) || text == null) continue;
                int rows = 1;
                foreach (char ch in text)
                    if (ch == '\n') rows++;
                int width = w.Widths.TryGetValue(text, out int wide) ? wide : 0;
                if (width > 360 || rows > 20) w.Notes.Add(name + ", " + lang + ": " + width + " x " + rows);
            }
        }
    }

    static uint Make(string lang)
    {
        int b = lang.Length > 1 ? lang[1] : 0;
        int c = lang.Length > 2 ? lang[2] : 0;
        return (uint)(lang[0] | (b << 8) | (c << 16));
    }

    static bool Same(string a, string b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            int x = a[i];
            int y = b[i];
            if (x >= 'A' && x <= 'Z') x += 32;
            if (y >= 'A' && y <= 'Z') y += 32;
            if (x != y) return false;
        }
        return true;
    }
}
