namespace HCDE.Playsim;

/// <summary>
/// DoomEd numbers, spawn types, and spawn-map parsing from <c>g_doomedmap.cpp</c> (<b>250–457</b>).
/// Chunk <b>1189</b>. Actor lookup and the name table are host data.
/// A closing brace returns, so the error summary after that loop never runs.
/// </summary>
public static class VmGDoomedMap1189Slice
{
    public const int FmtUnknown = 0;
    public const int FmtOld = 1;
    public const int FmtNew = 2;
    const string Red = "\u001cG";
    const string CSingles = "{}|=/`~!@#$%^&*()[]\\?-+;:<>,.";

    public static readonly Dictionary<int, DoomEdEntry> DoomEdMap = new();
    public static readonly Dictionary<int, EdItem> DoomEdFromMapinfo = new();
    public static readonly Dictionary<int, string?> SpawnableThings = new();
    public static readonly Dictionary<int, string?> StrifeTypes = new();
    public static readonly Dictionary<int, SpawnItem> SpawnablesFromMapinfo = new();
    public static readonly Dictionary<int, SpawnItem> ConversationIDsFromMapinfo = new();
    public static readonly Dictionary<string, string> Actors = new(StringComparer.OrdinalIgnoreCase);
    public static readonly List<string> Names = new();
    public static readonly List<string> Messages = new();

    public sealed class FatalError : Exception
    {
        public FatalError(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class ScriptFault : Exception
    {
        public ScriptFault(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class DoomEdEntry
    {
        public string? TypeName;
        public int Special;
        public int ArgsDefined;
        public bool NoSkillFlags;
        public int[] Args = new int[5];
    }

    public sealed class EdItem
    {
        public string ClassName = "";
        public int Special;
        public int ArgsDefined;
        public bool NoSkillFlags;
        public int[] Args = new int[5];
        public string FileName = "";
        public int LineNum;
    }

    public sealed class SpawnItem
    {
        public string ClassName = "";
        public string FileName = "";
        public int LineNum;
    }

    public sealed class Parser
    {
        public int Format;
        public readonly Scanner Scanner = new();
    }

    public sealed class Scanner
    {
        public string ScriptName = "";
        public string Text = "";
        public int Number;
        public int Line = 1;
        public bool CMode;
        public bool ParseError;
        string _script = "";
        int _pos;
        bool _alreadyGot;
        int _alreadyGotLine = 1;
        int _lastGotLine = 1;
        string _pending = "";

        public void Open(string name, string buffer)
        {
            if (buffer.Length == 0 || buffer[^1] != '\n')
            {
                if (buffer.Length > 0 && buffer[^1] == '\0')
                    buffer = buffer[..^1] + "\n";
                else
                    buffer += "\n";
            }

            ScriptName = name;
            _script = buffer;
            _pos = 0;
            Line = 1;
            CMode = false;
            ParseError = false;
            _alreadyGot = false;
            _alreadyGotLine = 1;
            _lastGotLine = 1;
            Text = "";
            _pending = "";
        }

        public bool GetString()
        {
            ParseError = false;
            if (_alreadyGot)
            {
                _alreadyGot = false;
                Text = _pending;
                return true;
            }

            if (_pos >= _script.Length)
                return false;

            _lastGotLine = Line;
            if (!ReadToken())
                return false;
            return true;
        }

        public void UnGet()
        {
            _alreadyGot = true;
            _alreadyGotLine = _lastGotLine;
            _pending = Text;
        }

        public bool Compare(string text)
        {
            return string.Equals(text, Text, StringComparison.OrdinalIgnoreCase);
        }

        public bool CheckString(string name)
        {
            if (!GetString())
                return false;
            if (Compare(name))
                return true;
            UnGet();
            return false;
        }

        public bool CheckNumber()
        {
            if (!GetString())
                return false;
            if (Text.Length == 0)
            {
                UnGet();
                return false;
            }

            if (Text == "MAXINT")
            {
                Number = int.MaxValue;
                return true;
            }

            if (!TryParseWhole(Text, out long big))
            {
                UnGet();
                return false;
            }

            Number = unchecked((int)big);
            return true;
        }

        public void MustGetString()
        {
            if (!GetString())
                ScriptError("Missing string (unexpected end of file).");
        }

        public void MustGetStringName(string name)
        {
            MustGetString();
            if (!Compare(name))
                ScriptError("Expected '" + name + "', got '" + Text + "'.");
        }

        public void ScriptMessage(string message)
        {
            ParseError = true;
            int line = _alreadyGot ? _alreadyGotLine : Line;
            Messages.Add(Red + "Script error, \"" + ScriptName + "\"" + Red + " line " + line + ":\n" + Red + message + "\n");
        }

        public void ScriptError(string message)
        {
            ParseError = true;
            int line = _alreadyGot ? _alreadyGotLine : Line;
            throw new ScriptFault("Script error, \"" + ScriptName + "\" line " + line + ":\n" + message + "\n");
        }

        bool ReadToken()
        {
            int len = _script.Length;
            while (true)
            {
                if (_pos >= len)
                    return false;
                char c = _script[_pos];
                if (c == '\n')
                {
                    _pos++;
                    Line++;
                    continue;
                }

                if (c <= ' ')
                {
                    _pos++;
                    continue;
                }

                if (c == '/' && _pos + 1 < len && _script[_pos + 1] == '*')
                {
                    SkipBlock();
                    continue;
                }

                if (c == '/' && _pos + 1 < len && _script[_pos + 1] == '/')
                {
                    SkipLine();
                    continue;
                }

                if (!CMode && c == ';')
                {
                    SkipLine();
                    continue;
                }

                if (MatchAt("#region") || MatchAt("#endregion"))
                {
                    SkipLine();
                    continue;
                }

                break;
            }

            if (_pos >= len)
                return false;

            char c0 = _script[_pos];
            if (c0 == '"')
            {
                ReadString();
                return true;
            }

            if (CMode && c0 == '-' && _pos + 1 < len && _script[_pos + 1] >= '0' && _script[_pos + 1] <= '9')
            {
                int start = _pos;
                _pos++;
                while (_pos < len && !IsCStop(_script[_pos]))
                    _pos++;
                Text = _script.Substring(start, _pos - start);
                return true;
            }

            if (CMode && _pos + 1 < len)
            {
                string pair = _script.Substring(_pos, 2);
                if (pair == "::" || pair == "&&" || pair == "==" || pair == "||" || pair == "<<" || pair == ">>")
                {
                    Text = pair;
                    _pos += 2;
                    return true;
                }
            }

            if (CMode && IsCSingle(c0))
            {
                Text = c0.ToString();
                _pos++;
                return true;
            }

            if (!CMode && (c0 == '{' || c0 == '}' || c0 == '|' || c0 == '='))
            {
                Text = c0.ToString();
                _pos++;
                return true;
            }

            int startId = _pos;
            if (CMode)
            {
                while (_pos < len && !IsCStop(_script[_pos]))
                    _pos++;
            }
            else
            {
                while (_pos < len && !IsClassicStop(_pos))
                    _pos++;
            }

            if (_pos == startId)
            {
                Text = c0.ToString();
                _pos++;
                return true;
            }

            Text = _script.Substring(startId, _pos - startId);
            return true;
        }

        void SkipBlock()
        {
            _pos += 2;
            int len = _script.Length;
            while (_pos < len)
            {
                if (_script[_pos] == '\n')
                {
                    Line++;
                    _pos++;
                    continue;
                }

                if (_script[_pos] == '*' && _pos + 1 < len && _script[_pos + 1] == '/')
                {
                    _pos += 2;
                    return;
                }

                _pos++;
            }
        }

        void SkipLine()
        {
            int len = _script.Length;
            while (_pos < len && _script[_pos] != '\n')
                _pos++;
            if (_pos < len && _script[_pos] == '\n')
            {
                _pos++;
                Line++;
            }
        }

        bool MatchAt(string literal)
        {
            if (_pos + literal.Length > _script.Length)
                return false;
            return string.Compare(_script, _pos, literal, 0, literal.Length, StringComparison.Ordinal) == 0;
        }

        void ReadString()
        {
            _pos++;
            string body = "";
            int len = _script.Length;
            while (_pos < len)
            {
                char ch = _script[_pos];
                if (ch == '\\' && _pos + 1 < len && _script[_pos + 1] == '"')
                {
                    body += "\"";
                    _pos += 2;
                    continue;
                }

                if (ch == '"')
                {
                    _pos++;
                    break;
                }

                if (ch == '\n')
                    Line++;
                body += ch;
                _pos++;
            }

            Text = body;
        }

        bool IsClassicStop(int index)
        {
            if (index >= _script.Length)
                return true;
            char c = _script[index];
            if (c == '\n' || c <= ' ')
                return true;
            if (c == '{' || c == '}' || c == '|' || c == '=' || c == '"' || c == ';')
                return true;
            if (c != '/')
                return false;
            if (index + 1 >= _script.Length)
                return true;
            char next = _script[index + 1];
            if (next == '/' || next == '*')
                return true;
            if (next == '\n' || next <= ' ' || next == '{' || next == '}' || next == '|' || next == '=' || next == '"' || next == ';')
                return true;
            return false;
        }
    }

    public static void Clear()
    {
        DoomEdMap.Clear();
        DoomEdFromMapinfo.Clear();
        SpawnableThings.Clear();
        StrifeTypes.Clear();
        SpawnablesFromMapinfo.Clear();
        ConversationIDsFromMapinfo.Clear();
        Actors.Clear();
        Names.Clear();
        Messages.Clear();
    }

    public static void InitActorNumsFromMapinfo()
    {
        DoomEdMap.Clear();
        int error = 0;
        foreach (var pair in DoomEdFromMapinfo)
        {
            string? cls = null;
            if (!IsNameNone(pair.Value.ClassName))
            {
                cls = FindActor(pair.Value.ClassName);
                if (cls == null)
                {
                    Messages.Add(UnknownActor(pair.Value.FileName, pair.Value.LineNum, pair.Value.ClassName));
                    error++;
                }
            }

            DoomEdMap[pair.Key] = new DoomEdEntry
            {
                TypeName = cls,
                Special = unchecked((short)pair.Value.Special),
                ArgsDefined = unchecked((sbyte)pair.Value.ArgsDefined),
                NoSkillFlags = pair.Value.NoSkillFlags,
                Args = CopyArgs(pair.Value.Args)
            };
        }

        if (error > 0)
            throw new FatalError(error + " unknown actor classes found");
        DoomEdFromMapinfo.Clear();
    }

    public static string? GetSpawnableType(int spawnnum)
    {
        if (spawnnum < 0)
        {
            int index = unchecked(-spawnnum);
            if ((uint)index < (uint)Names.Count)
                return FindActor(Names[index]);
            return null;
        }

        if (SpawnableThings.TryGetValue(spawnnum, out var type))
            return type;
        return null;
    }

    public static string DumpSpawnables()
    {
        return DumpClassMap(SpawnableThings);
    }

    public static string DumpConversationIds()
    {
        return DumpClassMap(StrifeTypes);
    }

    public static string DumpClassMap(Dictionary<int, string?> themap)
    {
        var pairs = new List<KeyValuePair<int, string?>>(themap);
        pairs.Sort((a, b) => unchecked(a.Key - b.Key));
        string text = "";
        foreach (var pair in pairs)
        {
            string line = pair.Key + " " + TypeNameOf(pair.Value) + "\n";
            Messages.Add(line);
            text += line;
        }

        return text;
    }

    public static void ClearStrifeTypes()
    {
        StrifeTypes.Clear();
    }

    public static void ParseOpenBrace(Parser parser)
    {
        Scanner sc = parser.Scanner;
        if (parser.Format == FmtOld)
            return;

        if (parser.Format == FmtNew)
        {
            sc.MustGetStringName("{");
            sc.CMode = true;
            return;
        }

        parser.Format = sc.CheckString("{") ? FmtNew : FmtOld;
        if (parser.Format == FmtNew)
            sc.CMode = true;
    }

    public static void ParseSpawnMap(Scanner sc, Dictionary<int, SpawnItem> themap, string descript)
    {
        var defined = new HashSet<int>();
        string fileName = sc.ScriptName;
        while (true)
        {
            if (sc.CheckString("}"))
                return;
            if (sc.CheckNumber())
            {
                int ednum = sc.Number;
                sc.MustGetStringName("=");
                sc.MustGetString();
                if (defined.Contains(ednum))
                    sc.ScriptMessage(descript + " " + ednum + " defined more than once");
                else if (ednum < 0)
                    sc.ScriptMessage(descript + " must be positive, got " + ednum);
                defined.Add(ednum);
                themap[ednum] = new SpawnItem
                {
                    ClassName = sc.Text,
                    FileName = fileName,
                    LineNum = sc.Line
                };
            }
            else
            {
                sc.ScriptError("Number expected");
            }
        }
    }

    public static void ParseSpawnNums(Parser parser)
    {
        ParseOpenBrace(parser);
        ParseSpawnMap(parser.Scanner, SpawnablesFromMapinfo, "Spawn number");
    }

    public static void ParseConversationIDs(Parser parser)
    {
        ParseOpenBrace(parser);
        ParseSpawnMap(parser.Scanner, ConversationIDsFromMapinfo, "Conversation ID");
    }

    public static void InitClassMap(Dictionary<int, string?> themap, Dictionary<int, SpawnItem> thedata)
    {
        themap.Clear();
        int error = 0;
        foreach (var pair in thedata)
        {
            if (!IsNameNone(pair.Value.ClassName))
            {
                string? cls = FindActor(pair.Value.ClassName);
                if (cls == null)
                {
                    Messages.Add(UnknownActor(pair.Value.FileName, pair.Value.LineNum, pair.Value.ClassName));
                    error++;
                }

                themap[pair.Key] = cls;
            }
            else
            {
                themap.Remove(pair.Key);
            }
        }

        if (error > 0)
            throw new FatalError(error + " unknown actor classes found");
        thedata.Clear();
    }

    public static void InitSpawnablesFromMapinfo()
    {
        InitClassMap(SpawnableThings, SpawnablesFromMapinfo);
        InitClassMap(StrifeTypes, ConversationIDsFromMapinfo);
    }

    static bool IsCSingle(char c)
    {
        return CSingles.IndexOf(c) >= 0;
    }

    static bool IsCStop(char c)
    {
        if (c == '\n' || c <= ' ' || c == '"')
            return true;
        return IsCSingle(c);
    }

    static bool IsNameNone(string name)
    {
        if (name.Length == 0)
            return true;
        return string.Equals(name, "None", StringComparison.OrdinalIgnoreCase);
    }

    static string? FindActor(string name)
    {
        if (Actors.TryGetValue(name, out var found))
            return found;
        return null;
    }

    static string UnknownActor(string file, int line, string className)
    {
        return Red + "Script error, \"" + file + "\" line " + line + ":\nUnknown actor class " + className + "\n";
    }

    static int[] CopyArgs(int[] args)
    {
        var copy = new int[5];
        int n = args.Length < 5 ? args.Length : 5;
        for (int i = 0; i < n; i++)
            copy[i] = args[i];
        return copy;
    }

    static string TypeNameOf(string? typeName)
    {
        if (typeName == null)
            throw new NullReferenceException();
        return typeName;
    }

    static int DigitValue(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 10;
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 10;
        return -1;
    }

    static bool TryParseWhole(string text, out long value)
    {
        value = 0;
        if (text.Length == 0)
            return false;
        int i = 0;
        int sign = 1;
        if (text[0] == '+' || text[0] == '-')
        {
            if (text.Length == 1)
                return false;
            if (text[0] == '-')
                sign = -1;
            i = 1;
        }

        int radix = 10;
        if (text[i] == '0')
        {
            int lead = -1;
            bool hexMark = false;
            if (i + 1 < text.Length && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            {
                hexMark = true;
                if (i + 2 < text.Length)
                    lead = DigitValue(text[i + 2]);
            }

            if (hexMark && lead >= 0 && lead < 16)
            {
                radix = 16;
                i += 2;
            }
            else
                radix = 8;
        }

        int digits = 0;
        long acc = 0;
        for (; i < text.Length; i++)
        {
            int d = DigitValue(text[i]);
            if (d < 0 || d >= radix)
                break;
            acc = unchecked(acc * radix + d);
            digits++;
        }

        if (digits == 0 || i != text.Length)
            return false;
        value = sign < 0 ? unchecked(-acc) : acc;
        return true;
    }
}
