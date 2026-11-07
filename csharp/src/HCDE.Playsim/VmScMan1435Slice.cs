namespace HCDE.Playsim;

/// <summary>
/// CheckOpen through FScriptPosition::Message from sc_man.cpp 1126-1342.
/// GetString and Compare are callees. ParseHex below this band stays out.
/// </summary>
public static class VmScMan1435Slice
{
    public const int BandStart = 1126;
    public const int BandEnd = 1342;
    public const int IntConst = 259;
    public const int UIntConst = 260;
    public const int FloatConst = 261;
    public const int Warning = 0;
    public const int FatalLevel = 1;
    public const int Error = 2;
    public const int OptError = 3;
    public const int DebugError = 4;
    public const int DebugWarn = 5;
    public const int DebugMsg = 6;
    public const int Log = 7;
    public const int DebugLog = 8;
    public const int MessageLevel = 9;
    public const int DevError = 1;
    public const int DevWarn = 2;
    public const int DevNotify = 3;
    public const int PrintHigh = 2;
    public const int PrintLog = 5;
    public const string Red = "\u001cG";
    public const string Orange = "\u001cI";
    public const string Green = "\u001cD";

    public static int ErrorCounter;
    public static int WarnCounter;
    public static int Developer;
    public static bool StrictErrors;
    public static bool ErrorsOut;
    public static List<Note> Notes = new();

    public sealed class Fatal(string message) : Exception(message);

    public sealed class Word
    {
        public string Text = "";
        public int Line = 1;
    }

    public sealed class Sym
    {
        public int Type;
        public long Number;
        public double Float;
    }

    public sealed class Note
    {
        public int Level;
        public string Text = "";
    }

    public sealed class State
    {
        public List<Word> Words = new();
        public int Index;
        public bool Open = true;
        public bool End;
        public bool AlreadyGot;
        public int LastLine = 1;
        public int AlreadyGotLine;
        public int Line = 1;
        public string? Text;
        public bool ParseError;
        public bool NoFatal;
        public string? Name = "";
        public string? Prepend = "";
        public Dictionary<string, Sym> Symbols = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Lines = new();
    }

    public sealed class Place
    {
        public string File = "";
        public int Line;
    }

    public static void CheckOpen(State s)
    {
        if (!s.Open) throw new Fatal("SC_ call before SC_Open().");
    }

    public static void AddInt(State s, string? name, long value)
    {
        if (name == null) throw new NullReferenceException();
        s.Symbols[name] = new Sym { Type = IntConst, Number = value, Float = value };
    }

    public static void AddUint(State s, string? name, ulong value)
    {
        if (name == null) throw new NullReferenceException();
        s.Symbols[name] = new Sym { Type = UIntConst, Number = unchecked((long)value), Float = value };
    }

    public static void AddFloat(State s, string? name, double value)
    {
        if (name == null) throw new NullReferenceException();
        s.Symbols[name] = new Sym { Type = FloatConst, Number = (long)value, Float = value };
    }

    public static void SkipBlock(State s)
    {
        int depth = 0;
        while (true)
        {
            MustString(s);
            if (Same(s.Text, "{")) depth++;
            else if (Same(s.Text, "}"))
            {
                depth--;
                if (depth < 0) return;
            }
        }
    }

    public static int StartBraces(State s, out int end)
    {
        end = -1;
        if (CheckString(s, "{"))
        {
            Mark here = Save(s);
            SkipBlock(s);
            end = s.Index;
            Restore(s, here);
            return 0;
        }
        ScriptError(s, "'{' expected");
        return -1;
    }

    public static bool FoundEnd(State s, int end) => s.Index >= end;

    public static Place At(string? file, int line)
    {
        if (file == null) throw new NullReferenceException();
        return new Place { File = file, Line = line };
    }

    public static Place AtScanner(State s) => new Place { File = s.Name ?? "", Line = MessageLine(s) };

    public static Place CopyFrom(Place place, State s)
    {
        place.File = s.Name ?? "";
        place.Line = MessageLine(s);
        return place;
    }

    public static void Report(Place place, int severity, string? message, params object[] args)
    {
        if (severity == DebugLog && Developer < DevNotify) return;
        if (severity == DebugError && Developer < DevError) return;
        if (severity == DebugWarn && Developer < DevWarn) return;
        if (severity == DebugMsg && Developer < DevNotify) return;
        if (severity == OptError) severity = StrictErrors ? Error : Warning;
        if (severity == Error && ErrorsOut) severity = FatalLevel;
        string composed = message == null ? "Bad syntax." : Compose(message, args);
        string type;
        string color;
        int level = PrintHigh;
        switch (severity)
        {
            default:
                return;
            case Warning:
            case DebugWarn:
            case DebugError:
                WarnCounter++;
                type = "warning";
                color = Orange;
                break;
            case Error:
                ErrorCounter++;
                type = "error";
                color = Red;
                break;
            case MessageLevel:
            case DebugMsg:
                type = "message";
                color = Green;
                break;
            case DebugLog:
            case Log:
                type = "message";
                level = PrintLog;
                color = "";
                break;
            case FatalLevel:
                throw new Fatal("Script error, \"" + place.File + "\" line " + place.Line + ":\n" + composed + "\n");
        }
        Notes.Add(new Note
        {
            Level = level,
            Text = color + "Script " + type + ", \"" + place.File + "\" line " + place.Line + ":\n" + color + composed + "\n",
        });
    }

    public static bool GetString(State s)
    {
        CheckOpen(s);
        s.ParseError = false;
        if (s.AlreadyGot)
        {
            s.AlreadyGot = false;
            return true;
        }
        if (s.Index >= s.Words.Count)
        {
            s.End = true;
            return false;
        }
        Word word = s.Words[s.Index++];
        s.Text = word.Text;
        s.LastLine = word.Line;
        s.Line = word.Line;
        return true;
    }

    public static void MustString(State s)
    {
        if (!GetString(s)) ScriptError(s, "Missing string (unexpected end of file).");
    }

    public static bool CheckString(State s, string? name)
    {
        if (!GetString(s)) return false;
        if (Same(s.Text, name)) return true;
        UnGet(s);
        return false;
    }

    static void ScriptError(State s, string? message, params object[] args)
    {
        string composed = message == null ? "Bad syntax." : Compose(message, args);
        s.ParseError = true;
        int line = MessageLine(s);
        if (s.NoFatal)
        {
            s.Lines.Add(Red + (s.Prepend ?? "") + "Script error, \"" + s.Name + "\"" + Red + " line " + line + ":\n" + Red + composed + "\n");
            return;
        }
        throw new Fatal((s.Prepend ?? "") + "Script error, \"" + s.Name + "\" line " + line + ":\n" + composed + "\n");
    }

    static int MessageLine(State s) => s.AlreadyGot ? s.AlreadyGotLine : s.Line;

    static void UnGet(State s)
    {
        s.AlreadyGot = true;
        s.AlreadyGotLine = s.LastLine;
    }

    static bool Same(string? left, string? right)
    {
        if (left == null || right == null) throw new NullReferenceException();
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
        {
            int a = left[i] is >= 'A' and <= 'Z' ? left[i] + 32 : left[i];
            int b = right[i] is >= 'A' and <= 'Z' ? right[i] + 32 : right[i];
            if (a != b) return false;
        }
        return true;
    }

    static string Compose(string message, object[] args)
    {
        if (args.Length == 0) return message;
        var built = new System.Text.StringBuilder();
        int at = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] != '%' || i + 1 >= message.Length)
            {
                built.Append(message[i]);
                continue;
            }
            char kind = message[++i];
            if (kind == '%') built.Append('%');
            else if (kind == 's') built.Append(args[at++]);
            else if (kind == 'd' || kind == 'i') built.Append(Convert.ToInt32(args[at++]));
            else built.Append('%').Append(kind);
        }
        return built.ToString();
    }

    sealed class Mark
    {
        public int Index;
        public int Line;
        public bool AtEnd;
    }

    static Mark Save(State s)
    {
        CheckOpen(s);
        return new Mark { Index = s.Index, Line = s.Line, AtEnd = s.End };
    }

    static void Restore(State s, Mark mark)
    {
        if (!mark.AtEnd)
        {
            s.Index = mark.Index;
            s.Line = mark.Line;
            s.End = false;
        }
        else s.End = true;
        s.AlreadyGot = false;
    }
}
