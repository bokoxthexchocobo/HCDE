namespace HCDE.Playsim;

/// <summary>FraggleScript tokenization from <c>t_parse.cpp</c> (31–274).</summary>
public static partial class VmFScriptParser
{
    public const int TokenUnsetParse = 0;
    public const int TokenStringParse = 1;
    public const int TokenNumberParse = 2;
    public const int TokenOperatorParse = 3;
    public const int TokenNameParse = 4;
    public const int TokenFunctionParse = 5;

    public sealed class ScriptParseState
    {
        public char[] Data { get; init; } = [];
        public int Rover { get; set; }
        public int LineStart { get; set; }
        public int NumTokens { get; set; }
        public string[] Tokens { get; } = new string[64];
        public int[] TokenType { get; } = new int[64];
        public FsSection? Section { get; set; }
        public FsSection? PrevSection { get; set; }
        public int BraceType { get; set; }
        public VmFScriptScript.FsScript? Script { get; set; }
        public VmFScriptScript.FsScript? GlobalScript { get; set; }
        public Action<string> ScriptError { get; init; } = _ => throw new InvalidOperationException("script error");
        public Action<string>? Printf { get; set; }
        public Func<VmFScriptScript.FsScript, char[], int, FsSection?> FindSectionStart { get; init; } =
            VmFScriptPrepro.FindSectionStart;
        public Func<VmFScriptScript.FsScript, char[], int, FsSection?> FindSectionEnd { get; init; } =
            VmFScriptPrepro.FindSectionEnd;

        public ExprFrame ToExprFrame()
        {
            var f = new ExprFrame
            {
                Script = Script,
                GlobalScript = GlobalScript,
                NumTokens = NumTokens,
                ScriptError = ScriptError,
            };
            for (var i = 0; i < NumTokens; i++)
            {
                f.Tokens[i] = Tokens[i];
                f.TokenType[i] = TokenType[i] switch
                {
                    TokenStringParse => TokenStringExpr,
                    TokenNumberParse => TokenNumberExpr,
                    TokenOperatorParse => TokenOperatorExpr,
                    TokenFunctionParse => TokenFunctionExpr,
                    _ => TokenNameExpr,
                };
            }
            return f;
        }
    }

    private static bool IsNum(char c) => (c >= '0' && c <= '9') || c == '.';

    /// <summary>Native <c>isop</c> from <c>t_script.h</c> (not a fixed operator list).</summary>
    private static bool IsOp(char c) =>
        !((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_');

    private static char EscapeSequence(char c) =>
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

    private static void AppendChar(ScriptParseState st, int toknIndex, char c) =>
        st.Tokens[toknIndex] += c;

    public static void NextToken(ScriptParseState st)
    {
        if (st.Tokens[st.NumTokens - 1].Length > 0 || st.TokenType[st.NumTokens - 1] == TokenStringParse)
        {
            st.NumTokens++;
            st.Tokens[st.NumTokens - 1] = "";
            st.TokenType[st.NumTokens - 1] = TokenNameParse;
        }

        while (true)
        {
            if (st.Rover < st.Data.Length && st.Data[st.Rover] != '\0' &&
                (st.Data[st.Rover] == ' ' || st.Data[st.Rover] < 32))
            {
                while (st.Rover < st.Data.Length && st.Data[st.Rover] != '\0' &&
                       (st.Data[st.Rover] == ' ' || st.Data[st.Rover] < 32))
                    st.Rover++;
            }
            if (st.Rover >= st.Data.Length || st.Data[st.Rover] == '\0')
            {
                if (st.Tokens[0].Length > 0)
                    st.ScriptError("missing ';'\n");
                return;
            }
            break;
        }

        if (st.NumTokens > 1 && st.Rover < st.Data.Length && st.Data[st.Rover] == '(' &&
            st.TokenType[st.NumTokens - 2] == TokenNameParse)
            st.TokenType[st.NumTokens - 2] = TokenFunctionParse;

        if (st.Rover < st.Data.Length && (st.Data[st.Rover] == '{' || st.Data[st.Rover] == '}'))
        {
            if (st.Data[st.Rover] == '{')
            {
                st.BraceType = BracketOpen;
                st.Section = st.FindSectionStart(st.Script!, st.Data, st.Rover);
            }
            else
            {
                st.BraceType = BracketClose;
                st.Section = st.FindSectionEnd(st.Script!, st.Data, st.Rover);
            }
            if (st.Section == null)
            {
                st.ScriptError("section not found!\n");
                return;
            }
        }
        else if (st.Rover < st.Data.Length && st.Data[st.Rover] == ':')
        {
            st.NumTokens = 1;
            st.Tokens[0] = "";
            st.TokenType[0] = TokenNameParse;
            st.Rover++;
        }
        else if (st.Rover < st.Data.Length && st.Data[st.Rover] == '"')
        {
            st.TokenType[st.NumTokens - 1] = TokenStringParse;
            if (st.NumTokens > 1 && st.TokenType[st.NumTokens - 2] == TokenStringParse)
                st.NumTokens--;
            st.Rover++;
        }
        else if (st.Rover < st.Data.Length)
        {
            st.TokenType[st.NumTokens - 1] = IsOp(st.Data[st.Rover]) ? TokenOperatorParse
                : IsNum(st.Data[st.Rover]) ? TokenNumberParse
                : TokenNameParse;
        }
    }

    public static int GetTokens(ScriptParseState st, int startIndex)
    {
        st.Rover = startIndex;
        st.NumTokens = 1;
        st.Tokens[0] = "";
        st.TokenType[0] = TokenNameParse;
        st.Section = null;
        NextToken(st);
        st.LineStart = st.Rover;
        if (st.Rover < st.Data.Length && st.Data[st.Rover] != '\0')
        {
            while (true)
            {
                var toknIndex = st.NumTokens - 1;
                if (st.Section != null)
                    break;
                if (st.TokenType[toknIndex] != TokenStringParse && st.Rover < st.Data.Length && st.Data[st.Rover] == ';')
                    break;
                // Native `script_error("missing ';'")` throws; non-throwing ScriptError callbacks must still exit the scan.
                if (st.Rover >= st.Data.Length || st.Data[st.Rover] == '\0')
                    break;

                switch (st.TokenType[toknIndex])
                {
                    case TokenStringParse:
                        while (st.Rover < st.Data.Length && st.Data[st.Rover] != '"')
                        {
                            if (st.Data[st.Rover] == '\\')
                            {
                                st.Rover++;
                                if (st.Rover < st.Data.Length && st.Data[st.Rover] >= '0' && st.Data[st.Rover] <= '9')
                                {
                                    AppendChar(st, toknIndex, '\x1c');
                                    AppendChar(st, toknIndex, (char)(st.Data[st.Rover] + 'A' - '0'));
                                }
                                else if (st.Rover < st.Data.Length)
                                    AppendChar(st, toknIndex, EscapeSequence(st.Data[st.Rover]));
                            }
                            else
                                AppendChar(st, toknIndex, st.Data[st.Rover]);
                            st.Rover++;
                        }
                        if (st.Rover < st.Data.Length)
                            st.Rover++;
                        NextToken(st);
                        continue;
                    case TokenOperatorParse:
                    {
                        var tok = st.Tokens[toknIndex];
                        if ((tok.Length > 0 && st.Rover < st.Data.Length && st.Data[st.Rover] != '=' &&
                             st.Data[st.Rover] != tok[0]) || tok is "(" or ")")
                        {
                            NextToken(st);
                            continue;
                        }
                        if (st.Rover < st.Data.Length)
                            AppendChar(st, toknIndex, st.Data[st.Rover]);
                        break;
                    }
                    case TokenNumberParse:
                        while (st.Rover < st.Data.Length && IsNum(st.Data[st.Rover]))
                            AppendChar(st, toknIndex, st.Data[st.Rover++]);
                        NextToken(st);
                        continue;
                    case TokenNameParse:
                        while (st.Rover < st.Data.Length && !IsOp(st.Data[st.Rover]))
                            AppendChar(st, toknIndex, st.Data[st.Rover++]);
                        NextToken(st);
                        continue;
                }
                if (st.Rover < st.Data.Length)
                    st.Rover++;
            }
        }

        if (st.Tokens[st.NumTokens - 1].Length == 0)
            st.NumTokens--;
        if (st.Rover < st.Data.Length)
            st.Rover++;
        return st.Rover;
    }
}
