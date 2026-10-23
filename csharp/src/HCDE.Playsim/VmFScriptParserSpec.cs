namespace HCDE.Playsim;

/// <summary>FraggleScript special statements from <c>fragglescript/t_spec.cpp</c> (1–605).</summary>
public static partial class VmFScriptParser
{
    public const int TokenName = 4;
    public const int TokenOperator = 3;
    public const int TokenNumber = 2;
    public const int TokenString = 1;
    public const int TokenFunction = 5;
    public const int BracketOpen = 1;
    public const int BracketClose = 2;
    public const int MaxArgs = 16;

    public sealed class SpecState
    {
        public VmFScriptScript.FsScript? Script { get; set; }
        public VmFScriptScript.FsScript? GlobalScript { get; set; }
        public object? Level { get; set; }
        public int Rover { get; set; }
        public int BraceType { get; set; }
        public FsSection? Section { get; set; }
        public FsSection? PrevSection { get; set; }
        public int NumTokens { get; set; }
        public string[] Tokens { get; } = new string[64];
        public int[] TokenType { get; } = new int[64];
        public int TArgc { get; set; }
        public ScriptValue[] TArgv { get; } = new ScriptValue[MaxArgs];
        public ScriptValue TReturn { get; } = new();
        public string TFunc { get; set; } = "";
        public Action<string> ScriptError { get; init; } = _ => throw new InvalidOperationException("script error");
        public Action<string> Printf { get; init; } = _ => { };
        public Func<FsSection, int> SectionEnd { get; init; } = s => s.EndIndex;
        public Func<FsSection, int> SectionLoop { get; init; } = s => s.LoopIndex;
        public Func<VmFScriptScript.FsScript, int, int, int, char[]> SectionStartChars { get; init; } = (_, _, _, _) => Array.Empty<char>();
        public Action<VmFScriptPrepro.PreprocessHost, VmFScriptScript.FsScript> PreprocessScript { get; init; } = (_, _) => { };
        public Func<VmFScriptScript.FsScript> CreateChildScript { get; init; } = () => new VmFScriptScript.FsScript();
        public Action<VmFScriptScript.FsScript, int, VmFScriptScript.FsScript?> StoreChildScript { get; init; } = (_, _, _) => { };
        public Action<Action> InvokeSfHandler { get; set; } = h => h();
        public Action<int> RunLineSpecial { get; init; } = _ => { };
    }

    public static void SpecBrace(SpecState st)
    {
        if (st.BraceType != BracketClose)
            return;
        if (st.Section == null)
            return;
        if (st.Section.Type == VmFScriptPrepro.StIf || st.Section.Type == VmFScriptPrepro.StElse)
            return;
        if (st.Section.Type == VmFScriptPrepro.StLoop)
            st.Rover = SectionLoopIndex(st, st.Section);
    }

    private static int SectionLoopIndex(SpecState st, FsSection section) =>
        st.SectionLoop(section);

    public static bool SpecIf(SpecState st)
    {
        var endToken = FindOperator(st, 0, st.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.ScriptError("parse error in if statement\n");
            return false;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st, eval, 2, endToken - 1);
        var ifResult = VmFScriptVariable.IntValue(ToVariableValue(eval)) != 0;
        if (st.Section != null && st.BraceType == BracketOpen && endToken == st.NumTokens - 1)
        {
            if (!ifResult)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else if (ifResult && endToken != st.NumTokens - 1)
            EvaluateExpression(st, eval, endToken + 1, st.NumTokens - 1);
        return ifResult;
    }

    public static bool SpecElseif(SpecState st, bool lastIf)
    {
        var endToken = FindOperator(st, 0, st.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.ScriptError("parse error in elseif statement\n");
            return false;
        }
        if (lastIf)
        {
            st.Rover = st.SectionEnd(st.Section!) + 1;
            return true;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st, eval, 2, endToken - 1);
        var ifResult = VmFScriptVariable.IntValue(ToVariableValue(eval)) != 0;
        if (st.Section != null && st.BraceType == BracketOpen && endToken == st.NumTokens - 1)
        {
            if (!ifResult)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else if (ifResult && endToken != st.NumTokens - 1)
            EvaluateExpression(st, eval, endToken + 1, st.NumTokens - 1);
        return ifResult;
    }

    public static void SpecElse(SpecState st, bool lastIf)
    {
        if (lastIf)
            st.Rover = st.SectionEnd(st.Section!) + 1;
    }

    public static void SpecWhile(SpecState st)
    {
        if (st.Section == null)
        {
            st.ScriptError("no {} section given for loop\n");
            return;
        }
        var endToken = FindOperator(st, 0, st.NumTokens - 1, ")");
        if (endToken == -1)
        {
            st.ScriptError("parse error in loop statement\n");
            return;
        }
        var eval = new ScriptValue();
        EvaluateExpression(st, eval, 2, endToken - 1);
        if (VmFScriptVariable.IntValue(ToVariableValue(eval)) == 0)
            st.Rover = st.SectionEnd(st.Section) + 1;
    }

    public static void SpecFor(SpecState st)
    {
        var eval = new ScriptValue();
        if (st.Section == null)
        {
            st.ScriptError("need {} delimiters for for()\n");
            return;
        }
        const int start = 2;
        var comma1 = FindOperator(st, start, st.NumTokens - 1, ",");
        var comma2 = comma1 == -1 ? -1 : FindOperator(st, comma1 + 1, st.NumTokens - 1, ",");
        if (comma1 == -1 || comma2 == -1)
        {
            st.ScriptError("incorrect arguments to for()\n");
            return;
        }
        if (st.Section == st.PrevSection)
        {
            EvaluateExpression(st, eval, comma2 + 1, st.NumTokens - 2);
            EvaluateExpression(st, eval, comma1 + 1, comma2 - 1);
            if (VmFScriptVariable.IntValue(ToVariableValue(eval)) == 0)
                st.Rover = st.SectionEnd(st.Section) + 1;
        }
        else
            EvaluateExpression(st, eval, start, comma1 - 1);
    }

    public static void CreateVariable(SpecState st, int newVarType, VmFScriptScript.FsScript newVarScript, int start, int stop)
    {
        if (st.TokenType[start] != TokenName)
        {
            st.ScriptError($"invalid name for variable: '{st.Tokens[start]}'\n");
            return;
        }
        if (VariableForName(newVarScript, st.Tokens[start]) != null)
        {
            st.Printf($"FS: redefined symbol: '{st.Tokens[start]}'\n");
            return;
        }
        if (newVarScript != st.Script && newVarType == VmFScriptVariable.SvtMobj)
        {
            st.ScriptError("cannot create mobj reference in hub script\n");
            return;
        }
        VmFScriptVariable.NewVariable(newVarScript, st.Tokens[start], newVarType);
        if (stop != start)
        {
            var scratch = new ScriptValue();
            EvaluateExpression(st, scratch, start, stop);
        }
    }

    public static void ParseVarLine(SpecState st, int newVarType, VmFScriptScript.FsScript newVarScript, int start)
    {
        var startToken = start;
        while (true)
        {
            var endToken = FindOperator(st, startToken, st.NumTokens - 1, ",");
            if (endToken == -1)
                break;
            CreateVariable(st, newVarType, newVarScript, startToken, endToken - 1);
            startToken = endToken + 1;
        }
        CreateVariable(st, newVarType, newVarScript, startToken, st.NumTokens - 1);
    }

    public static bool SpecVariable(SpecState st)
    {
        var start = 0;
        var newVarType = -1;
        var newVarScript = st.Script!;
        if (st.Tokens[start] == "hub")
            start++;
        if (st.Tokens[start] == "const")
        {
            newVarType = VmFScriptVariable.SvtConst;
            start++;
        }
        else if (st.Tokens[start] == "string")
        {
            newVarType = VmFScriptVariable.SvtString;
            start++;
        }
        else if (st.Tokens[start] == "int")
        {
            newVarType = VmFScriptVariable.SvtInt;
            start++;
        }
        else if (st.Tokens[start] == "mobj")
        {
            newVarType = VmFScriptVariable.SvtMobj;
            start++;
        }
        else if (st.Tokens[start] is "fixed" or "float")
        {
            newVarType = VmFScriptVariable.SvtFixed;
            start++;
        }
        else if (st.Tokens[start] == "script")
        {
            SpecScript(st);
            return true;
        }
        if (newVarType != -1)
        {
            ParseVarLine(st, newVarType, newVarScript, start);
            return true;
        }
        return false;
    }

    public static void SpecScript(SpecState st)
    {
        if (st.Section == null)
        {
            st.ScriptError("need seperators for newscript\n");
            return;
        }
        if (st.NumTokens < 2)
        {
            st.ScriptError("need newscript number\n");
            return;
        }
        var result = new ScriptValue();
        EvaluateExpression(st, result, 1, st.NumTokens - 1);
        var scriptNum = VmFScriptVariable.IntValue(ToVariableValue(result));
        if (scriptNum < 0)
        {
            st.ScriptError("invalid newscript number\n");
            return;
        }
        var newScript = st.CreateChildScript();
        st.StoreChildScript(st.Script!, scriptNum, newScript);
        var dataSize = st.Section.EndIndex - st.Section.StartIndex - 2;
        var src = st.SectionStartChars(st.Script!, st.Section.StartIndex, st.Section.EndIndex, dataSize);
        var bytes = new byte[dataSize + 1];
        for (var i = 0; i < dataSize; i++)
            bytes[i] = (byte)src[i + 1];
        bytes[dataSize] = 0;
        newScript.Data = bytes;
        newScript.ScriptNum = scriptNum;
        newScript.Parent = st.Script;
        var host = new VmFScriptPrepro.PreprocessHost { Level = st.Level };
        st.PreprocessScript(host, newScript);
        st.Rover = st.SectionEnd(st.Section) + 1;
    }

    public static void EvaluateFunction(SpecState st, ScriptValue result, int start, int stop)
    {
        if (st.TokenType[start] != TokenFunction || st.TokenType[stop] != TokenOperator || st.Tokens[stop] != ")")
        {
            st.ScriptError("misplaced closing paren\n");
            return;
        }
        var func = VariableForName(st.GlobalScript!, st.Tokens[start]);
        if (func == null)
        {
            st.ScriptError($"no such function: '{st.Tokens[start]}'\n");
            return;
        }
        if (func.Type != VmFScriptVariable.SvtFunction && func.Type != VmFScriptVariable.SvtLinespec)
        {
            st.ScriptError($"'{st.Tokens[start]}' not a function\n");
            return;
        }
        var argc = 0;
        var endpoint = start + 2;
        while (endpoint < stop)
        {
            var startpoint = endpoint;
            endpoint = FindOperator(st, startpoint, stop - 1, ",");
            if (endpoint == -1)
                endpoint = stop;
            if (endpoint - 1 < startpoint)
                break;
            EvaluateExpression(st, st.TArgv[argc], startpoint, endpoint - 1);
            endpoint++;
            argc++;
        }
        st.TArgc = argc;
        st.TReturn.Type = SvtInt;
        st.TReturn.IntValue = 0;
        if (func.Type == VmFScriptVariable.SvtFunction)
        {
            if (func.FunctionHandler == null)
                st.ScriptError($"'{st.Tokens[start]}' not a function\n");
            else
                st.InvokeSfHandler(func.FunctionHandler);
        }
        else
            st.RunLineSpecial(func.LineSpecNumber);
        CopyReturn(st.TReturn, result);
    }

    public static void OpStructure(SpecState st, ScriptValue result, int start, int n, int stop)
    {
        var func = VariableForName(st.GlobalScript!, st.Tokens[n + 1]);
        if (func == null)
        {
            st.ScriptError($"no such function: '{st.Tokens[n + 1]}'\n");
            return;
        }
        if (func.Type != VmFScriptVariable.SvtFunction)
        {
            st.ScriptError($"'{st.Tokens[n + 1]}' not a function\n");
            return;
        }
        EvaluateExpression(st, st.TArgv[0], start, n - 1);
        var argc = 1;
        if (stop != n + 1)
        {
            var endpoint = n + 3;
            while (endpoint < stop)
            {
                var startpoint = endpoint;
                endpoint = FindOperator(st, startpoint, stop - 1, ",");
                if (endpoint == -1)
                    endpoint = stop;
                if (endpoint - 1 < startpoint)
                    break;
                EvaluateExpression(st, st.TArgv[argc], startpoint, endpoint - 1);
                endpoint++;
                argc++;
            }
        }
        st.TArgc = argc;
        st.TFunc = func.Name;
        st.TReturn.Type = SvtInt;
        st.TReturn.IntValue = 0;
        if (func.FunctionHandler == null)
            st.ScriptError($"'{st.Tokens[n + 1]}' not a function\n");
        else
            st.InvokeSfHandler(func.FunctionHandler);
        CopyReturn(st.TReturn, result);
    }

    private static void CopyReturn(ScriptValue src, ScriptValue dst)
    {
        dst.Type = src.Type;
        dst.String = src.String;
        dst.IntValue = src.IntValue;
        dst.Mobj = src.Mobj;
    }

    private static VmFScriptVariable.ScriptValue ToVariableValue(ScriptValue v) =>
        new()
        {
            Type = v.Type switch
            {
                SvtString => VmFScriptVariable.SvtString,
                SvtMobj => VmFScriptVariable.SvtMobj,
                _ => VmFScriptVariable.SvtInt,
            },
            String = v.String,
            IntValue = v.IntValue,
            Mobj = v.Mobj,
        };

    private static VmFScriptScript.FsVariable? VariableForName(VmFScriptScript.FsScript script, string name) =>
        VmFScriptVariable.VariableForName(script, name);

    public static int FindOperator(SpecState st, int start, int stop, string value)
    {
        var bracketLevel = 0;
        for (var i = start; i <= stop; i++)
        {
            if (st.TokenType[i] != TokenOperator)
                continue;
            bracketLevel += st.Tokens[i] == "(" ? 1 : st.Tokens[i] == ")" ? -1 : 0;
            if (bracketLevel == 0 && st.Tokens[i] == value)
                return i;
        }
        return -1;
    }

    public static void EvaluateExpression(SpecState st, ScriptValue result, int start, int stop)
    {
        if (start == stop)
        {
            SimpleEvaluate(st, result, start);
            return;
        }
        var plus = FindOperator(st, start, stop, "+");
        if (plus != -1)
        {
            var left = new ScriptValue();
            var right = new ScriptValue();
            EvaluateExpression(st, left, start, plus - 1);
            EvaluateExpression(st, right, plus + 1, stop);
            result.Type = SvtInt;
            result.IntValue = VmFScriptVariable.IntValue(ToVariableValue(left)) +
                              VmFScriptVariable.IntValue(ToVariableValue(right));
            return;
        }
        var minus = FindOperator(st, start, stop, "-");
        if (minus != -1)
        {
            var left = new ScriptValue();
            var right = new ScriptValue();
            EvaluateExpression(st, left, start, minus - 1);
            EvaluateExpression(st, right, minus + 1, stop);
            result.Type = SvtInt;
            result.IntValue = VmFScriptVariable.IntValue(ToVariableValue(left)) -
                              VmFScriptVariable.IntValue(ToVariableValue(right));
            return;
        }
        if (st.TokenType[start] == TokenFunction)
        {
            EvaluateFunction(st, result, start, stop);
            return;
        }
        st.ScriptError($"couldnt evaluate expression: {string.Join(' ', st.Tokens[start..(stop + 1)])}\n");
    }

    private static void SimpleEvaluate(SpecState st, ScriptValue returnVar, int n)
    {
        switch (st.TokenType[n])
        {
            case TokenString:
                returnVar.Type = SvtString;
                returnVar.String = st.Tokens[n];
                break;
            case TokenNumber:
                returnVar.Type = SvtInt;
                returnVar.IntValue = int.Parse(st.Tokens[n], System.Globalization.CultureInfo.InvariantCulture);
                break;
            case TokenName:
            {
                var var = VmFScriptVariable.VariableForName(st.Script!, st.Tokens[n])
                          ?? VmFScriptVariable.VariableForName(st.GlobalScript!, st.Tokens[n]);
                if (var == null)
                {
                    st.ScriptError($"unknown variable '{st.Tokens[n]}'\n");
                    return;
                }
                var tmp = new VmFScriptVariable.ScriptValue();
                VmFScriptVariable.GetValue(var, tmp);
                returnVar.Type = tmp.Type == VmFScriptVariable.SvtString ? SvtString : SvtInt;
                returnVar.String = tmp.String;
                returnVar.IntValue = tmp.IntValue;
                returnVar.Mobj = tmp.Mobj;
                break;
            }
        }
    }
}
