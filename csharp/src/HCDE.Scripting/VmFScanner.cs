namespace HCDE.Scripting;

/// <summary>Managed <c>FScanner</c> from <c>sc_man.cpp</c> (string-backed).</summary>
public sealed class VmFScanner
{
    private string script = "";
    private int position;
    private int line = 1;
    private bool scriptOpen;
    private bool alreadyGot;
    private int alreadyGotLine;
    private bool lastGotToken;
    private int lastGotPosition;
    private int lastGotLine;
    private bool cMode;
    private bool noOctals;
    private bool escape = true;
    private byte stateMode;

    public string ScriptName { get; private set; } = "";
    public string String { get; private set; } = "";
    public int TokenType { get; private set; }
    public int Number { get; private set; }
    public long BigNumber { get; private set; }
    public double Float { get; private set; }
    public int Line => line;
    public bool End { get; private set; }
    public bool ParseError { get; private set; }

    private readonly Dictionary<string, VmScannerSymbol> symbols = new(StringComparer.OrdinalIgnoreCase);

    public readonly record struct SavedPos(int Position, int Line, bool End);

    public sealed class VmScannerSymbol
    {
        public int TokenType { get; init; }
        public long Number { get; init; }
        public double Float { get; init; }
    }

    public void OpenString(string name, string buffer)
    {
        script = VmScriptText.Prepare(buffer);
        ScriptName = name;
        position = 0;
        line = 1;
        End = false;
        scriptOpen = true;
        alreadyGot = false;
        lastGotToken = false;
        ParseError = false;
    }

    public void SetCMode(bool value) => cMode = value;
    public void SetNoOctals(bool value) => noOctals = value;
    public void SetEscape(bool value) => escape = value;
    public void SetStateMode(bool stately) => stateMode = stately ? (byte)2 : (byte)0;

    public void AddSymbol(string name, long value) =>
        symbols[name] = new VmScannerSymbol { TokenType = VmScriptTokens.IntConst, Number = value, Float = value };

    public SavedPos SavePos() => new(alreadyGot ? lastGotPosition : position, alreadyGot ? alreadyGotLine : line, End);

    public void RestorePos(SavedPos pos)
    {
        if (!pos.End)
        {
            position = pos.Position;
            line = pos.Line;
            End = false;
        }
        else End = true;
        alreadyGot = false;
        lastGotToken = false;
    }

    public bool GetString() => ScanString(tokens: false);

    public void MustGetString()
    {
        if (!GetString()) ScriptError("Missing string (unexpected end of file).");
    }

    public void MustGetStringName(string name)
    {
        MustGetString();
        if (!Compare(name)) ScriptError($"Expected '{name}', got '{String}'.");
    }

    public bool CheckString(string name)
    {
        if (!GetString()) return false;
        if (Compare(name)) return true;
        UnGet();
        return false;
    }

    public bool GetToken(bool evaluate = false)
    {
        if (!ScanString(tokens: true)) return false;
        if (TokenType == VmScriptTokens.IntConst || TokenType == VmScriptTokens.UIntConst)
        {
            if (!VmScriptNumber.TryParseInteger(String, noOctals, out var big, out var num))
                ScriptError($"Bad numeric constant \"{String}\".");
            BigNumber = big;
            Number = num;
            Float = num;
        }
        else if (TokenType == VmScriptTokens.FloatConst)
        {
            Float = double.Parse(String, System.Globalization.CultureInfo.InvariantCulture);
            Number = (int)Float;
            BigNumber = Number;
        }
        else if (TokenType == VmScriptTokens.StringConst)
        {
            String = VmCmdLibStrBinBridge.Unescape(String);
        }
        else if (TokenType == VmScriptTokens.Identifier && evaluate && symbols.TryGetValue(String, out var sym))
        {
            TokenType = sym.TokenType;
            BigNumber = sym.Number;
            Number = (int)sym.Number;
            Float = sym.Float;
        }

        return true;
    }

    public void MustGetAnyToken(bool evaluate = false)
    {
        if (!GetToken(evaluate)) ScriptError("Missing token (unexpected end of file).");
    }

    public void MustGetToken(int token, bool evaluate = false)
    {
        MustGetAnyToken(evaluate);
        if (TokenType != token) ScriptError($"Expected token {token} but got {TokenType}.");
    }

    public bool CheckToken(int token, bool evaluate = false)
    {
        if (!GetToken(evaluate)) return false;
        if (TokenType == token) return true;
        UnGet();
        return false;
    }

    public bool GetNumber(bool evaluate = false)
    {
        CheckOpen();
        if (!GetString()) return false;
        if (String == "MAXINT") { Number = int.MaxValue; BigNumber = int.MaxValue; Float = Number; return true; }
        if (!VmScriptNumber.TryParseInteger(String, noOctals, out var big, out var num))
        {
            if (evaluate && symbols.TryGetValue(String, out var sym) && sym.TokenType == VmScriptTokens.IntConst)
            {
                BigNumber = sym.Number;
                Number = (int)sym.Number;
                Float = sym.Float;
                return true;
            }

            ScriptError($"SC_GetNumber: Bad numeric constant \"{String}\".");
            return false;
        }

        BigNumber = big;
        Number = num;
        Float = num;
        return true;
    }

    public void MustGetNumber(bool evaluate = false)
    {
        if (!GetNumber(evaluate)) ScriptError("Missing integer (unexpected end of file).");
    }

    public bool CheckNumber(bool evaluate = false)
    {
        if (!GetString()) return false;
        if (String.Length == 0) { UnGet(); return false; }
        if (String == "MAXINT") { BigNumber = long.MaxValue; Number = int.MaxValue; Float = Number; return true; }
        if (!VmScriptNumber.TryParseInteger(String, noOctals, out var big, out var num))
        {
            if (evaluate && symbols.TryGetValue(String, out var sym) && sym.TokenType == VmScriptTokens.IntConst)
            {
                BigNumber = sym.Number;
                Number = (int)sym.Number;
                Float = sym.Float;
                return true;
            }

            UnGet();
            return false;
        }

        BigNumber = big;
        Number = num;
        Float = num;
        return true;
    }

    public bool GetFloat(bool evaluate = false)
    {
        CheckOpen();
        if (!GetString()) return false;
        if (!double.TryParse(String, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            if (evaluate && symbols.TryGetValue(String, out var sym))
            {
                BigNumber = sym.Number;
                Number = (int)sym.Number;
                Float = sym.Float;
                return true;
            }

            ScriptError($"SC_GetFloat: Bad numeric constant \"{String}\".");
            return false;
        }

        Float = value;
        Number = (int)Float;
        BigNumber = Number;
        return true;
    }

    public void MustGetFloat(bool evaluate = false)
    {
        if (!GetFloat(evaluate)) ScriptError("Missing floating-point number (unexpected end of file).");
    }

    public bool CheckFloat(bool evaluate = false)
    {
        if (!GetString()) return false;
        if (String.Length == 0) { UnGet(); return false; }
        if (!double.TryParse(String, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            UnGet();
            return false;
        }

        Float = value;
        Number = (int)Float;
        BigNumber = Number;
        return true;
    }

    public void UnGet()
    {
        alreadyGot = true;
        alreadyGotLine = lastGotLine;
    }

    public bool Compare(string text) => string.Equals(text, String, StringComparison.OrdinalIgnoreCase);

    public int MatchString(IReadOnlyList<string> options)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (Compare(options[i])) return i;
        }

        return -1;
    }

    public int MustMatchString(IReadOnlyList<string> options)
    {
        var index = MatchString(options);
        if (index < 0) ScriptError($"Unknown keyword '{String}'");
        return index;
    }

    public int GetMessageLine() => alreadyGot ? alreadyGotLine : line;

    public void ScriptError(string message)
    {
        ParseError = true;
        throw new VmScriptException(ScriptName, GetMessageLine(), message);
    }

    private void CheckOpen()
    {
        if (!scriptOpen) throw new InvalidOperationException("SC_ call before SC_Open().");
    }

    private bool ScanString(bool tokens)
    {
        CheckOpen();
        if (alreadyGot)
        {
            alreadyGot = false;
            if (!tokens || lastGotToken) return !End;
            position = lastGotPosition;
            line = lastGotLine;
        }

        if (position >= script.Length)
        {
            End = true;
            return false;
        }

        lastGotPosition = position;
        lastGotLine = line;
        SkipLeading();
        if (position >= script.Length)
        {
            End = true;
            return false;
        }

        var start = position;
        if (script[position] == '"')
        {
            ReadQuotedString();
            TokenType = tokens ? VmScriptTokens.StringConst : VmScriptTokens.Identifier;
            lastGotToken = tokens;
            return true;
        }

        if (!cMode && script[position] is '{' or '}' or '|' or '=')
        {
            String = script[position].ToString();
            position++;
            TokenType = script[position - 1];
            lastGotToken = tokens;
            return true;
        }

        if (cMode && IsCModePunctuator(script[position]))
        {
            String = script[position].ToString();
            position++;
            TokenType = script[position - 1];
            lastGotToken = tokens;
            return true;
        }

        if (cMode && (char.IsDigit(script[position]) || script[position] == '.'))
        {
            ReadNumberToken();
            lastGotToken = tokens;
            return true;
        }

        ReadGenericToken();
        if (tokens)
        {
            if (VmScriptTokens.TryGetKeyword(String, out var keyword))
                TokenType = keyword;
            else if (VmScriptNumber.TryParseInteger(String, noOctals, out _, out _))
                TokenType = VmScriptTokens.IntConst;
            else if (double.TryParse(String, System.Globalization.NumberStyles.Float,
                         System.Globalization.CultureInfo.InvariantCulture, out _))
                TokenType = VmScriptTokens.FloatConst;
            else
                TokenType = VmScriptTokens.Identifier;
        }

        lastGotToken = tokens;
        return true;
    }

    private void SkipLeading()
    {
        while (position < script.Length)
        {
            if (script[position] == '\n') { line++; position++; continue; }
            if (script[position] is ' ' or '\t' or '\r') { position++; continue; }
            if (position + 1 < script.Length && script[position] == '/' && script[position + 1] == '/')
            {
                position += 2;
                while (position < script.Length && script[position] != '\n') position++;
                continue;
            }

            if (!cMode && script[position] == ';')
            {
                position++;
                while (position < script.Length && script[position] != '\n') position++;
                continue;
            }

            if (position + 1 < script.Length && script[position] == '/' && script[position + 1] == '*')
            {
                position += 2;
                while (position + 1 < script.Length && !(script[position] == '*' && script[position + 1] == '/')) position++;
                position = Math.Min(position + 2, script.Length);
                continue;
            }

            if (position + 8 < script.Length && script.AsSpan(position, 8).Equals("#region".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                while (position < script.Length && script[position] != '\n') position++;
                continue;
            }

            break;
        }
    }

    private void ReadQuotedString()
    {
        position++;
        var start = position;
        while (position < script.Length)
        {
            if (escape && script[position] == '\\' && position + 1 < script.Length && script[position + 1] == '"')
            {
                position += 2;
                continue;
            }

            if (script[position] == '"') break;
            if (script[position] == '\n') line++;
            position++;
        }

        String = script[start..position];
        if (position < script.Length) position++;
    }

    private void ReadNumberToken()
    {
        var start = position;
        while (position < script.Length && (char.IsDigit(script[position]) || script[position] is '.' or 'e' or 'E' or '+' or '-'))
            position++;
        String = script[start..position];
    }

    private static bool IsCModePunctuator(char ch) =>
        ch is '{' or '}' or '|' or '=' or '/' or '`' or '~' or '!' or '@' or '#' or '$' or '%' or '^'
            or '&' or '*' or '(' or ')' or '[' or ']' or '\\' or '?' or '-' or '+' or ';' or ':'
            or '<' or '>' or ',' or '.';

    private void ReadGenericToken()
    {
        var start = position;
        while (position < script.Length)
        {
            var ch = script[position];
            if (ch is ' ' or '\t' or '\r' or '\n' or '"' or '{' or '}' or '|' or '=') break;
            if (cMode && IsCModePunctuator(ch)) break;
            if (position + 1 < script.Length && ch == '/' && (script[position + 1] == '/' || script[position + 1] == '*')) break;
            position++;
        }

        String = script[start..position];
    }
}

/// <summary>Bridge to client strbin without project reference cycle.</summary>
internal static class VmCmdLibStrBinBridge
{
    public static string Unescape(string text)
    {
        var result = new System.Text.StringBuilder();
        var index = 0;
        while (index < text.Length)
        {
            var c = text[index++];
            if (c != '\\') { result.Append(c); continue; }
            if (index >= text.Length) break;
            var esc = text[index++];
            result.Append(esc switch
            {
                'n' => '\n',
                't' => '\t',
                '"' => '"',
                '\\' => '\\',
                _ => esc
            });
        }

        return result.ToString();
    }
}
