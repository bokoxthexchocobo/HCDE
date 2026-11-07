namespace HCDE.Playsim;

/// <summary>
/// ScanValue through ScriptMessage from sc_man.cpp 933-1118.
/// GetToken, UnGet, and CheckToken are callees below this band. Comment banners stay uncredited.
/// </summary>
public static class VmScMan1434Slice
{
    public const int BandStart = 933;
    public const int BandEnd = 1118;
    public const int Identifier = 256;
    public const int StringConst = 257;
    public const int IntConst = 259;
    public const int UIntConst = 260;
    public const int FloatConst = 261;
    public const int TrueToken = 340;
    public const int FalseToken = 341;
    public const int LastToken = 413;
    public const string Red = "\u001cG";

    public sealed class Fatal(string message) : Exception(message);

    public sealed class Tok
    {
        public int Type;
        public string Text = "";
        public long Big;
        public int Number;
        public double Float;
        public int Line = 1;
    }

    public sealed class State
    {
        public List<Tok> Tokens = new();
        public int Index;
        public bool Open = true;
        public bool End;
        public bool AlreadyGot;
        public bool LastGotToken;
        public int LastLine = 1;
        public int AlreadyGotLine;
        public int Line = 1;
        public int TokenType;
        public string? Text;
        public long Big;
        public int Number;
        public double Float;
        public bool ParseError;
        public bool NoFatal;
        public string? Name = "";
        public string? Prepend = "";
        public Dictionary<string, double> Constants = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Lines = new();
        public bool LastEvaluate;
    }

    public static bool GetToken(State s, bool evaluate)
    {
        if (!s.Open) throw new Fatal("SC_ call before SC_Open().");
        s.ParseError = false;
        s.LastEvaluate = evaluate;
        if (s.AlreadyGot)
        {
            s.AlreadyGot = false;
            if (s.LastGotToken) return true;
        }
        if (s.Index >= s.Tokens.Count)
        {
            s.End = true;
            return false;
        }
        Tok tok = s.Tokens[s.Index++];
        s.LastGotToken = true;
        s.LastLine = tok.Line;
        s.Line = tok.Line;
        s.TokenType = tok.Type;
        s.Text = tok.Text;
        if (tok.Type == IntConst || tok.Type == UIntConst)
        {
            s.Big = tok.Big;
            s.Number = tok.Number;
            s.Float = tok.Float;
        }
        else if (tok.Type == FloatConst) s.Float = tok.Float;
        return true;
    }

    public static void UnGet(State s)
    {
        s.AlreadyGot = true;
        s.AlreadyGotLine = s.LastLine;
    }

    public static bool CheckToken(State s, int token, bool evaluate = false)
    {
        if (!GetToken(s, evaluate)) return false;
        if (s.TokenType == token) return true;
        UnGet(s);
        return false;
    }

    public static bool ScanValue(State s, bool allowFloat, bool evaluate)
    {
        bool neg = false;
        if (!GetToken(s, evaluate)) return false;
        if (s.TokenType == '-' || s.TokenType == '+')
        {
            neg = s.TokenType == '-';
            if (!GetToken(s, evaluate)) return false;
        }
        if (s.TokenType == FloatConst && !allowFloat) return false;
        if (s.TokenType != IntConst && s.TokenType != FloatConst)
        {
            if (s.Text == null) throw new NullReferenceException();
            if (!s.Constants.TryGetValue(s.Text, out double value)) return false;
            if (!allowFloat && (long)value != value) return false;
            s.Big = (long)value;
            s.Number = (int)value;
            s.Float = value;
        }
        if (neg)
        {
            s.Big = -s.Big;
            s.Number = -s.Number;
            s.Float = -s.Float;
        }
        return true;
    }

    public static bool CheckValue(State s, bool allowFloat, bool evaluate = true)
    {
        var saved = Save(s);
        bool ok = ScanValue(s, allowFloat, evaluate);
        if (!ok) Restore(s, saved);
        return ok;
    }

    public static void MustGetValue(State s, bool allowFloat, bool evaluate = true)
    {
        if (!ScanValue(s, allowFloat, evaluate))
            ScriptError(s, allowFloat ? "Numeric constant expected" : "Integer constant expected");
    }

    public static bool CheckBool(State s)
    {
        if (CheckToken(s, TrueToken))
        {
            s.Number = 1;
            s.Float = 1;
            return true;
        }
        if (CheckToken(s, FalseToken))
        {
            s.Number = 0;
            s.Float = 0;
            return true;
        }
        return false;
    }

    public static void MustGetBool(State s)
    {
        if (!CheckBool(s)) ScriptError(s, "Expected true or false");
    }

    public static string TokenName(int token, string? text = null)
    {
        if (token > ' ' && token < 256) return "'" + (char)token + "'";
        if (token >= Identifier && token < LastToken)
        {
            string work = Names[token - Identifier];
            if (text != null && token <= FloatConst)
            {
                char quote = token == StringConst ? '"' : '\'';
                work = work + " " + quote + text + quote;
            }
            return work;
        }
        return "Unknown(" + token + ")";
    }

    public static int MessageLine(State s) => s.AlreadyGot ? s.AlreadyGotLine : s.Line;

    public static void ScriptError(State s, string? message, params object[] args)
    {
        string composed = Compose(message, args);
        s.ParseError = true;
        int line = MessageLine(s);
        if (s.NoFatal)
        {
            s.Lines.Add(Red + (s.Prepend ?? "") + "Script error, \"" + s.Name + "\"" + Red + " line " + line + ":\n" + Red + composed + "\n");
            return;
        }
        throw new Fatal((s.Prepend ?? "") + "Script error, \"" + s.Name + "\" line " + line + ":\n" + composed + "\n");
    }

    public static void ScriptMessage(State s, string? message, params object[] args)
    {
        string composed = Compose(message, args);
        s.ParseError = true;
        int line = MessageLine(s);
        s.Lines.Add(Red + (s.Prepend ?? "") + "Script error, \"" + s.Name + "\"" + Red + " line " + line + ":\n" + Red + composed + "\n");
    }

    static string Compose(string? message, object[] args)
    {
        if (message == null) return "Bad syntax.";
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
        if (!s.Open) throw new Fatal("SC_ call before SC_Open().");
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
        s.LastGotToken = false;
    }

    static readonly string[] Names = "identifier\0string constant\0name constant\0integer constant\0unsigned constant\0float constant\0non-whitespace\0'::'\0'..'\0'...'\0'>>='\0'>>>='\0'<<='\0'+='\0'-='\0'*='\0'/='\0'%='\0'&='\0'^='\0'|='\0'>>'\0'>>>'\0'<<'\0'++'\0'--'\0'&&'\0'||'\0'<='\0'>='\0'=='\0'!='\0'~=='\0'<>='\0'<=>'\0'**'\0'->'\0'action'\0'break'\0'case'\0'const'\0'continue'\0'default'\0'do'\0'else'\0'for'\0'if'\0'return'\0'switch'\0'until'\0'while'\0'bool'\0'float'\0'float32'\0'double'\0'char'\0'byte'\0'sbyte'\0'short'\0'ushort'\0'int8'\0'uint8'\0'int16'\0'uint16'\0'int'\0'uint'\0'long'\0'ulong'\0'void'\0'struct'\0'flagdef'\0'property'\0'class'\0'mixin'\0'enum'\0'name'\0'string'\0'sound'\0'state'\0'color'\0'goto'\0'abstract'\0'foreach'\0'unsafe'\0'true'\0'false'\0'none'\0'instanceof'\0'auto'\0'exec'\0'defaultproperties'\0'native'\0'var'\0'out'\0'ref'\0'event'\0'static'\0'transient'\0'norollback'\0'volatile'\0'final'\0'throws'\0'extend'\0'public'\0'protected'\0'private'\0'dot'\0'cross'\0'ignores'\0'localized'\0'latent'\0'singular'\0'config'\0'coerce'\0'iterator'\0'virtual'\0'vararg'\0'ui'\0'play'\0'clearscope'\0'virtualscope'\0'override'\0'super'\0'null'\0'global'\0'stop'\0'include'\0'sealed'\0'is'\0'replaces'\0'vector2'\0'vector3'\0'map'\0'mapiterator'\0'array'\0'function'\0'in'\0'sizeof'\0'alignof'\0'states'\0'loop'\0'fail'\0'wait'\0'meta'\0'deprecated'\0'version'\0'readonly'\0'internal'\0'canraise'\0'fast'\0'light'\0'nodelay'\0'offset'\0'slow'\0'bright'\0'let'\0'static const'".Split('\0');
}
