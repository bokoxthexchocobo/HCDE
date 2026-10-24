namespace HCDE.Playsim;

/// <summary>FraggleScript variable/script specials from <c>t_spec.cpp</c> (231–605).</summary>
public static partial class VmFScriptParser
{
    public sealed class SpecRestState
    {
        public ExprFrame Expr { get; } = new();

        public SpecRestState()
        {
            Expr.Tag = this;
            for (var i = 0; i < TArgv.Length; i++)
                TArgv[i] = new ScriptValue();
        }
        public int Rover { get; set; }
        public FsSection? Section { get; set; }
        public Func<FsSection, int> SectionEnd { get; init; } = s => s.EndIndex;
        public Func<VmFScriptScript.FsScript, int, int, int, char[]> SectionStartChars { get; init; } = (_, _, _, _) => Array.Empty<char>();
        public Action<VmFScriptPrepro.PreprocessHost, VmFScriptScript.FsScript> PreprocessScript { get; init; } = (_, _) => { };
        public Func<VmFScriptScript.FsScript> CreateChildScript { get; init; } = () => new VmFScriptScript.FsScript();
        public Action<VmFScriptScript.FsScript, int, VmFScriptScript.FsScript?> StoreChildScript { get; init; } = (_, _, _) => { };
        public object? Level { get; set; }
        public int TArgc { get; set; }
        public ScriptValue[] TArgv { get; } = new ScriptValue[MaxArgs];
        public ScriptValue TReturn { get; } = new();
        public string TFunc { get; set; } = "";
        public Action<Action> InvokeSfHandler { get; set; } = h => h();
        public Action<int> RunLineSpecial { get; init; } = _ => { };
    }

    public const int MaxArgs = 16;

    public static void CreateVariable(SpecRestState st, int newVarType, VmFScriptScript.FsScript newVarScript, int start, int stop)
    {
        if (st.Expr.TokenType[start] != TokenNameExpr)
        {
            st.Expr.ScriptError($"invalid name for variable: '{st.Expr.Tokens[start]}'\n");
            return;
        }
        if (VmFScriptVariable.VariableForName(newVarScript, st.Expr.Tokens[start]) != null)
        {
            st.Expr.Printf($"FS: redefined symbol: '{st.Expr.Tokens[start]}'\n");
            return;
        }
        if (newVarScript != st.Expr.Script && newVarType == VmFScriptVariable.SvtMobj)
        {
            st.Expr.ScriptError("cannot create mobj reference in hub script\n");
            return;
        }
        VmFScriptVariable.NewVariable(newVarScript, st.Expr.Tokens[start], newVarType);
        if (stop != start)
        {
            var scratch = new ScriptValue();
            EvaluateExpression(st.Expr, scratch, start, stop);
        }
    }

    public static void ParseVarLine(SpecRestState st, int newVarType, VmFScriptScript.FsScript newVarScript, int start)
    {
        var startToken = start;
        while (true)
        {
            var endToken = FindOperator(st.Expr, startToken, st.Expr.NumTokens - 1, ",");
            if (endToken == -1)
                break;
            CreateVariable(st, newVarType, newVarScript, startToken, endToken - 1);
            startToken = endToken + 1;
        }
        CreateVariable(st, newVarType, newVarScript, startToken, st.Expr.NumTokens - 1);
    }

    public static bool SpecVariable(SpecRestState st)
    {
        var start = 0;
        var newVarType = -1;
        var newVarScript = st.Expr.Script!;
        if (st.Expr.Tokens[start] == "hub")
            start++;
        if (st.Expr.Tokens[start] == "const")
        {
            newVarType = VmFScriptVariable.SvtConst;
            start++;
        }
        else if (st.Expr.Tokens[start] == "string")
        {
            newVarType = VmFScriptVariable.SvtString;
            start++;
        }
        else if (st.Expr.Tokens[start] == "int")
        {
            newVarType = VmFScriptVariable.SvtInt;
            start++;
        }
        else if (st.Expr.Tokens[start] == "mobj")
        {
            newVarType = VmFScriptVariable.SvtMobj;
            start++;
        }
        else if (st.Expr.Tokens[start] is "fixed" or "float")
        {
            newVarType = VmFScriptVariable.SvtFixed;
            start++;
        }
        else if (st.Expr.Tokens[start] == "script")
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

    public static void SpecScript(SpecRestState st)
    {
        if (st.Section == null)
        {
            st.Expr.ScriptError("need seperators for newscript\n");
            return;
        }
        if (st.Expr.NumTokens < 2)
        {
            st.Expr.ScriptError("need newscript number\n");
            return;
        }
        var result = new ScriptValue();
        EvaluateExpression(st.Expr, result, 1, st.Expr.NumTokens - 1);
        var scriptNum = IntValue(result);
        if (scriptNum < 0)
        {
            st.Expr.ScriptError("invalid newscript number\n");
            return;
        }
        var newScript = st.CreateChildScript();
        st.StoreChildScript(st.Expr.Script!, scriptNum, newScript);
        var dataSize = st.Section.EndIndex - st.Section.StartIndex - 2;
        var src = st.SectionStartChars(st.Expr.Script!, st.Section.StartIndex, st.Section.EndIndex, dataSize);
        var bytes = new byte[dataSize + 1];
        for (var i = 0; i < dataSize; i++)
            bytes[i] = (byte)src[i + 1];
        bytes[dataSize] = 0;
        newScript.Data = bytes;
        newScript.ScriptNum = scriptNum;
        newScript.Parent = st.Expr.Script;
        var host = new VmFScriptPrepro.PreprocessHost { Level = st.Level };
        st.PreprocessScript(host, newScript);
        st.Rover = st.SectionEnd(st.Section) + 1;
    }

    public static void EvaluateFunction(SpecRestState st, ScriptValue result, int start, int stop)
    {
        if (st.Expr.TokenType[start] != TokenFunctionExpr || st.Expr.TokenType[stop] != TokenOperatorExpr || st.Expr.Tokens[stop] != ")")
        {
            st.Expr.ScriptError("misplaced closing paren\n");
            return;
        }
        var func = VmFScriptVariable.VariableForName(st.Expr.GlobalScript!, st.Expr.Tokens[start]);
        if (func == null)
        {
            st.Expr.ScriptError($"no such function: '{st.Expr.Tokens[start]}'\n");
            return;
        }
        if (func.Type != VmFScriptVariable.SvtFunction && func.Type != VmFScriptVariable.SvtLinespec)
        {
            st.Expr.ScriptError($"'{st.Expr.Tokens[start]}' not a function\n");
            return;
        }
        var argc = 0;
        var endpoint = start + 2;
        while (endpoint < stop)
        {
            var startpoint = endpoint;
            endpoint = FindOperator(st.Expr, startpoint, stop - 1, ",");
            if (endpoint == -1)
                endpoint = stop;
            if (endpoint - 1 < startpoint)
                break;
            EvaluateExpression(st.Expr, st.TArgv[argc], startpoint, endpoint - 1);
            endpoint++;
            argc++;
        }
        st.TArgc = argc;
        st.TReturn.Type = SvtInt;
        st.TReturn.IntValue = 0;
        if (func.Type == VmFScriptVariable.SvtFunction)
        {
            if (func.FunctionHandler == null)
                st.Expr.ScriptError($"'{st.Expr.Tokens[start]}' not a function\n");
            else
                st.InvokeSfHandler(func.FunctionHandler);
        }
        else
            st.RunLineSpecial(func.LineSpecNumber);
        CopyReturn(st.TReturn, result);
    }

    public static void OpStructure(SpecRestState st, ScriptValue result, int start, int n, int stop)
    {
        var func = VmFScriptVariable.VariableForName(st.Expr.GlobalScript!, st.Expr.Tokens[n + 1]);
        if (func == null)
        {
            st.Expr.ScriptError($"no such function: '{st.Expr.Tokens[n + 1]}'\n");
            return;
        }
        if (func.Type != VmFScriptVariable.SvtFunction)
        {
            st.Expr.ScriptError($"'{st.Expr.Tokens[n + 1]}' not a function\n");
            return;
        }
        EvaluateExpression(st.Expr, st.TArgv[0], start, n - 1);
        var argc = 1;
        if (stop != n + 1)
        {
            var endpoint = n + 3;
            while (endpoint < stop)
            {
                var startpoint = endpoint;
                endpoint = FindOperator(st.Expr, startpoint, stop - 1, ",");
                if (endpoint == -1)
                    endpoint = stop;
                if (endpoint - 1 < startpoint)
                    break;
                EvaluateExpression(st.Expr, st.TArgv[argc], startpoint, endpoint - 1);
                endpoint++;
                argc++;
            }
        }
        st.TArgc = argc;
        st.TFunc = func.Name;
        st.TReturn.Type = SvtInt;
        st.TReturn.IntValue = 0;
        if (func.FunctionHandler == null)
            st.Expr.ScriptError($"'{st.Expr.Tokens[n + 1]}' not a function\n");
        else
            st.InvokeSfHandler(func.FunctionHandler);
        CopyReturn(st.TReturn, result);
    }

    private static void CopyReturn(ScriptValue src, ScriptValue dst)
    {
        dst.Type = src.Type;
        dst.String = src.String;
        dst.IntValue = src.IntValue;
        dst.FixedBits = src.FixedBits;
        dst.Mobj = src.Mobj;
    }
}
