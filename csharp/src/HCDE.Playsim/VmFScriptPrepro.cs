using System.Text;

namespace HCDE.Playsim;

/// <summary>FraggleScript preprocessor from <c>fragglescript/t_prepro.cpp</c> (59–425).</summary>
public static class VmFScriptPrepro
{
    public const int StEmpty = 0;
    public const int StIf = 1;
    public const int StElseif = 2;
    public const int StElse = 3;
    public const int StLoop = 4;

    public sealed class PreprocessHost
    {
        public object? Level { get; set; }
        public Func<string, int> CheckNumForName { get; init; } = _ => -1;
        public Func<int, int> FileLength { get; init; } = _ => 0;
        public Action<int, byte[]> ReadFile { get; init; } = (_, _) => { };
        public Func<string, Exception> FatalError { get; init; } = msg => new InvalidOperationException(msg);
        public Action<string> ScriptError { get; init; } = _ => throw new FsParseError("script error");
        public Action<string> PrintBold { get; init; } = _ => { };
        public Action<VmFScriptScript.FsScript, object?, char[], int, int> RunIncludedScript { get; init; } =
            (_, _, _, _, _) => { };
        public Action<VmFScriptParser.FsSection> DestroySection { get; init; } = _ => { };
    }

    public sealed class FsParseError : Exception
    {
        public FsParseError(string message) : base(message) { }
    }

    public sealed class SectionSerializeFields
    {
        public int Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int LoopIndex { get; set; }
        public VmFScriptParser.FsSection? Next { get; set; }
    }

    public static SectionSerializeFields SerializeSection(VmFScriptParser.FsSection sec) =>
        new()
        {
            Type = sec.Type,
            StartIndex = sec.StartIndex,
            EndIndex = sec.EndIndex,
            LoopIndex = sec.LoopIndex,
            Next = sec.Next,
        };

    public static int MakeIndex(VmFScriptScript.FsScript script, char[] data, int charIndex) => charIndex;

    public static int SectionHash(VmFScriptScript.FsScript script, char[] data, int braceIndex) =>
        MakeIndex(script, data, braceIndex) % VmFScriptScript.SectionSlots;

    public static char[] SectionStart(VmFScriptScript.FsScript script, char[] data, VmFScriptParser.FsSection sec) =>
        data[sec.StartIndex..(sec.StartIndex + 1)];

    public static int SectionStartIndex(VmFScriptScript.FsScript script, VmFScriptParser.FsSection sec) => sec.StartIndex;

    public static int SectionEndIndex(VmFScriptScript.FsScript script, VmFScriptParser.FsSection sec) => sec.EndIndex;

    public static int SectionLoopIndex(VmFScriptScript.FsScript script, VmFScriptParser.FsSection sec) => sec.LoopIndex;

    public static void ClearSections(VmFScriptScript.FsScript script, PreprocessHost host)
    {
        for (var i = 0; i < VmFScriptScript.SectionSlots; i++)
        {
            var var = script.Sections[i] as VmFScriptParser.FsSection;
            while (var != null)
            {
                var next = var.Next;
                host.DestroySection(var);
                var = next;
            }
            script.Sections[i] = null;
        }
    }

    public static VmFScriptParser.FsSection NewSection(
        VmFScriptScript.FsScript script,
        char[] data,
        int braceIndex)
    {
        var n = SectionHash(script, data, braceIndex);
        var newsec = new VmFScriptParser.FsSection { StartIndex = MakeIndex(script, data, braceIndex) };
        newsec.Next = script.Sections[n] as VmFScriptParser.FsSection;
        script.Sections[n] = newsec;
        return newsec;
    }

    public static VmFScriptParser.FsSection? FindSectionStart(
        VmFScriptScript.FsScript script,
        char[] data,
        int braceIndex)
    {
        var n = SectionHash(script, data, braceIndex);
        var current = script.Sections[n] as VmFScriptParser.FsSection;
        while (current != null)
        {
            if (current.StartIndex == braceIndex)
                return current;
            current = current.Next;
        }
        return null;
    }

    public static VmFScriptParser.FsSection? FindSectionEnd(
        VmFScriptScript.FsScript script,
        char[] data,
        int braceIndex)
    {
        for (var n = 0; n < VmFScriptScript.SectionSlots; n++)
        {
            var current = script.Sections[n] as VmFScriptParser.FsSection;
            while (current != null)
            {
                if (current.EndIndex == braceIndex)
                    return current;
                current = current.Next;
            }
        }
        return null;
    }

    public static VmFScriptScript.FsVariable NewVariable(VmFScriptScript.FsScript script, string name, int vtype)
    {
        var newvar = new VmFScriptScript.FsVariable { Name = name, Type = vtype };
        var n = VariableHash(name);
        newvar.Next = script.Variables[n];
        script.Variables[n] = newvar;
        return newvar;
    }

    public static int VariableHash(string name)
    {
        if (name.Length == 0)
            return 0;
        var c0 = name[0];
        var c1 = name.Length > 1 ? name[1] : '\0';
        var c2 = name.Length > 2 ? name[2] : '\0';
        var c3 = name.Length > 3 ? name[3] : '\0';
        return (c0 + c1 + (c1 != '\0' ? c2 + (c2 != '\0' ? c3 : 0) : 0)) % VmFScriptScript.VariableSlots;
    }

    static bool IsOp(char c) =>
        !((c <= 'Z' && c >= 'A') || (c <= 'z' && c >= 'a') || (c <= '9' && c >= '0') || c == '_');

    static bool IsNum(char c) => (c >= '0' && c <= '9') || c == '.';

    public static int? ProcessFindChar(
        VmFScriptScript.FsScript script,
        PreprocessHost host,
        char[] datap,
        char find) =>
        ProcessFindChar(script, host, datap, 0, find);

    public static int? ProcessFindChar(
        VmFScriptScript.FsScript script,
        PreprocessHost host,
        char[] datap,
        int datapIndex,
        char find)
    {
        while (datapIndex < datap.Length && datap[datapIndex] != '\0')
        {
            if (datap[datapIndex] == find)
                return datapIndex;
            if (datap[datapIndex] == '\"')
            {
                datapIndex++;
                while (datapIndex < datap.Length && datap[datapIndex] != '\0' && datap[datapIndex] != '\"')
                {
                    if (datap[datapIndex] == '\\')
                        datapIndex++;
                    datapIndex++;
                }
                if (datapIndex >= datap.Length || datap[datapIndex] == '\0')
                    return null;
            }

            if (datap[datapIndex] == '/' && datapIndex + 1 < datap.Length && datap[datapIndex + 1] == '*')
            {
                while (datapIndex < datap.Length && datap[datapIndex] != '\0' &&
                       (datap[datapIndex] != '*' || datapIndex + 1 >= datap.Length || datap[datapIndex + 1] != '/'))
                {
                    datap[datapIndex] = ' ';
                    datapIndex++;
                }
                if (datapIndex < datap.Length && datap[datapIndex] != '\0')
                {
                    datap[datapIndex] = ' ';
                    if (datapIndex + 1 < datap.Length)
                        datap[datapIndex + 1] = ' ';
                    datapIndex += 2;
                }
                else
                    host.ScriptError("script terminated inside comment\n");
                continue;
            }

            if (datap[datapIndex] == '/' && datapIndex + 1 < datap.Length && datap[datapIndex + 1] == '/')
            {
                while (datapIndex < datap.Length && datap[datapIndex] != '\0' && datap[datapIndex] != '\n')
                {
                    datap[datapIndex] = ' ';
                    datapIndex++;
                }
            }

            if (datap[datapIndex] == ':' && script.ScriptNum != -1)
            {
                var labelptr = datapIndex - 1;
                while (labelptr >= 0 && !IsOp(datap[labelptr]))
                    labelptr--;
                var labelStart = labelptr + 1;
                var labelEnd = labelStart;
                while (labelEnd < datap.Length && datap[labelEnd] != ':' && datap[labelEnd] != '\0')
                    labelEnd++;
                var labelname = labelEnd > labelStart
                    ? new string(datap, labelStart, labelEnd - labelStart)
                    : "";
                if (labelname.Length == 0)
                    host.PrintBold($"Script {script.ScriptNum}: ':' encountrered in incorrect position!\n");
                else
                {
                    var newlabel = NewVariable(script, labelname, VmFScriptScript.SvtLabel);
                    newlabel.IntValue = MakeIndex(script, datap, labelptr + 1);
                }
            }

            if (datap[datapIndex] == '{')
            {
                var newsec = NewSection(script, datap, datapIndex);
                newsec.Type = StEmpty;
                var theend = ProcessFindChar(script, host, datap, datapIndex + 1, '}');
                if (theend == null)
                    throw host.FatalError($"Script {script.ScriptNum}: section error: no ending brace\n");
                newsec.EndIndex = MakeIndex(script, datap, theend.Value);
                datapIndex = theend.Value;
            }

            datapIndex++;
        }
        return null;
    }

    sealed class PreproDryRunParser
    {
        const int TMaxTokens = 256;
        const int TokenName = 0;
        const int TokenNumber = 1;
        const int TokenOperator = 2;
        const int TokenString = 3;
        const int TokenUnset = 4;
        const int TokenFunction = 5;

        readonly PreprocessHost _host;
        readonly VmFScriptScript.FsScript _script;
        readonly char[] _data;
        readonly List<string> _tokens = new();
        readonly List<int> _tokenTypes = new();
        int _rover;
        public VmFScriptParser.FsSection? Section { get; private set; }
        public int LineStart { get; private set; }
        public int NumTokens => _tokens.Count;
        public string TokenAt(int i) => _tokens[i];
        public int TokenTypeAt(int i) => _tokenTypes[i];

        public PreproDryRunParser(PreprocessHost host, VmFScriptScript.FsScript script, char[] data)
        {
            _host = host;
            _script = script;
            _data = data;
        }

        public void ErrorMessage(string msg) => _host.ScriptError(msg);

        public int GetTokens(int startIndex)
        {
            _rover = startIndex;
            _tokens.Clear();
            _tokenTypes.Clear();
            _tokens.Add("");
            _tokenTypes.Add(TokenName);
            Section = null;
            NextToken();
            LineStart = _rover;
            if (_rover < _data.Length && _data[_rover] != '\0')
            {
                while (true)
                {
                    var toknIndex = _tokens.Count - 1;
                    if (Section != null)
                        break;
                    if (_tokenTypes[toknIndex] != TokenString && _rover < _data.Length && _data[_rover] == ';')
                        break;

                    switch (_tokenTypes[toknIndex])
                    {
                        case TokenString:
                            while (_rover < _data.Length && _data[_rover] != '\"')
                            {
                                if (_data[_rover] == '\\')
                                {
                                    _rover++;
                                    if (_rover < _data.Length && _data[_rover] >= '0' && _data[_rover] <= '9')
                                    {
                                        AppendChar(toknIndex, '\x1c');
                                        AppendChar(toknIndex, (char)(_data[_rover] + 'A' - '0'));
                                    }
                                    else if (_rover < _data.Length)
                                        AppendChar(toknIndex, EscapeSequence(_data[_rover]));
                                }
                                else
                                    AppendChar(toknIndex, _data[_rover]);
                                _rover++;
                            }
                            if (_rover < _data.Length)
                                _rover++;
                            NextToken();
                            continue;
                        case TokenOperator:
                            var tok = _tokens[toknIndex];
                            if ((tok.Length > 0 && _rover < _data.Length && _data[_rover] != '=' && _data[_rover] != tok[0]) ||
                                tok is "(" or ")")
                            {
                                NextToken();
                                continue;
                            }
                            if (_rover < _data.Length)
                                AppendChar(toknIndex, _data[_rover]);
                            break;
                        case TokenNumber:
                            while (_rover < _data.Length && IsNum(_data[_rover]))
                                AppendChar(toknIndex, _data[_rover++]);
                            NextToken();
                            continue;
                        case TokenName:
                            while (_rover < _data.Length && !IsOp(_data[_rover]))
                                AppendChar(toknIndex, _data[_rover++]);
                            NextToken();
                            continue;
                    }
                    if (_rover < _data.Length)
                        _rover++;
                }
            }

            if (_tokens.Count > 0 && _tokens[^1].Length == 0)
            {
                _tokens.RemoveAt(_tokens.Count - 1);
                _tokenTypes.RemoveAt(_tokenTypes.Count - 1);
            }

            if (_rover < _data.Length)
                _rover++;
            return _rover;
        }

        void AppendChar(int toknIndex, char c) => _tokens[toknIndex] += c;

        static char EscapeSequence(char c) =>
            c switch
            {
                'n' => '\n',
                '\\' => '\\',
                '"' => '"',
                '?' => '?',
                'a' => '\a',
                't' => '\t',
                _ => c,
            };

        void NextToken()
        {
            if (_tokens.Count > 0 && (_tokens[^1].Length > 0 || _tokenTypes[^1] == TokenString))
            {
                if (_tokens.Count >= TMaxTokens)
                    ErrorMessage("too many tokens");
                _tokens.Add("");
                _tokenTypes.Add(TokenName);
            }

            while (true)
            {
                if (_rover < _data.Length && _data[_rover] != '\0' && (_data[_rover] == ' ' || _data[_rover] < 32))
                {
                    while (_rover < _data.Length && _data[_rover] != '\0' && (_data[_rover] == ' ' || _data[_rover] < 32))
                        _rover++;
                }
                if (_rover >= _data.Length || _data[_rover] == '\0')
                {
                    if (_tokens.Count > 0 && _tokens[0].Length > 0)
                        ErrorMessage("missing ';'\n");
                    return;
                }
                break;
            }

            if (_tokens.Count > 1 && _rover < _data.Length && _data[_rover] == '(' &&
                _tokenTypes[^2] == TokenName)
                _tokenTypes[^2] = TokenFunction;

            if (_rover < _data.Length && (_data[_rover] == '{' || _data[_rover] == '}'))
            {
                if (_data[_rover] == '{')
                    Section = FindSectionStart(_script, _data, _rover);
                else
                    Section = FindSectionEnd(_script, _data, _rover);
                if (Section == null)
                {
                    ErrorMessage("section not found!\n");
                    return;
                }
            }
            else if (_rover < _data.Length && _data[_rover] == ':')
            {
                _tokens.Clear();
                _tokenTypes.Clear();
                _tokens.Add("");
                _tokenTypes.Add(TokenName);
                _rover++;
            }
            else if (_rover < _data.Length && _data[_rover] == '\"')
            {
                _tokenTypes[^1] = TokenString;
                if (_tokenTypes.Count > 1 && _tokenTypes[^2] == TokenString)
                {
                    _tokens.RemoveAt(_tokens.Count - 1);
                    _tokenTypes.RemoveAt(_tokenTypes.Count - 1);
                }
                _rover++;
            }
            else if (_rover < _data.Length)
            {
                _tokenTypes[^1] = IsOp(_data[_rover]) ? TokenOperator
                    : IsNum(_data[_rover]) ? TokenNumber
                    : TokenName;
            }
        }
    }

    public static void DryRunScript(PreprocessHost host, VmFScriptScript.FsScript script, char[] data)
    {
        var end = data.Length;
        var rover = 0;
        var parse = new PreproDryRunParser(host, script, data);
        try
        {
            while (rover < end && rover < data.Length && data[rover] != '\0')
            {
                rover = parse.GetTokens(rover);
                if (parse.NumTokens == 0)
                    continue;
                if (parse.Section != null && parse.TokenTypeAt(0) == 5)
                {
                    var tok = parse.TokenAt(0);
                    if (tok == "if")
                    {
                        parse.Section.Type = StIf;
                        continue;
                    }
                    if (tok == "elseif")
                    {
                        parse.Section.Type = StElseif;
                        continue;
                    }
                    if (tok == "else")
                    {
                        parse.Section.Type = StElse;
                        continue;
                    }
                    if (tok == "while" || tok == "for")
                    {
                        parse.Section.Type = StLoop;
                        parse.Section.LoopIndex = MakeIndex(script, data, parse.LineStart);
                    }
                }
            }
        }
        catch (FsParseError err)
        {
            parse.ErrorMessage(err.Message);
        }
    }

    public static char[] DataAsChars(VmFScriptScript.FsScript script)
    {
        var bytes = script.Data;
        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
            chars[i] = (char)bytes[i];
        return chars;
    }

    public static void WriteCharsToData(VmFScriptScript.FsScript script, char[] chars)
    {
        var bytes = new byte[chars.Length];
        for (var i = 0; i < chars.Length; i++)
            bytes[i] = (byte)chars[i];
        script.Data = bytes;
    }

    public static void Preprocess(PreprocessHost host, VmFScriptScript.FsScript script)
    {
        script.Len = script.Data.Length - 1;
        var chars = DataAsChars(script);
        ProcessFindChar(script, host, chars, '\0');
        WriteCharsToData(script, chars);
        DryRunScript(host, script, chars);
    }

    public static void ParseInclude(PreprocessHost host, VmFScriptScript.FsScript script, string lumpname)
    {
        var lumpnum = host.CheckNumForName(lumpname);
        if (lumpnum == -1)
            throw host.FatalError($"include lump '{lumpname}' not found!\n");
        var lumplen = host.FileLength(lumpnum);
        var lump = new byte[lumplen + 10];
        host.ReadFile(lumpnum, lump);
        lump[lumplen] = 0;
        var chars = new char[lump.Length];
        for (var i = 0; i < lump.Length; i++)
            chars[i] = (char)lump[i];
        ProcessFindChar(script, host, chars, '\0');
        host.RunIncludedScript(script, host.Level, chars, 0, lumplen);
    }
}
