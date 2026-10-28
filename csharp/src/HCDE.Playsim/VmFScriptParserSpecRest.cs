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

    public readonly record struct VarDeclHeader(int StartToken, int VarType, VmFScriptScript.FsScript TargetScript, bool IsScriptDecl);

    public static bool ValidateFunctionCallSyntax(ExprFrame expr, int start, int stop, out string? error)
    {
        if (expr.TokenType[start] == TokenFunctionExpr
            && expr.TokenType[stop] == TokenOperatorExpr
            && expr.Tokens[stop] == ")")
        {
            error = null;
            return true;
        }

        error = "misplaced closing paren\n";
        return false;
    }

    public static bool ValidateVariableNameToken(ExprFrame expr, int tokenIndex, out string? error)
    {
        if (expr.TokenType[tokenIndex] == TokenNameExpr)
        {
            error = null;
            return true;
        }

        error = $"invalid name for variable: '{expr.Tokens[tokenIndex]}'\n";
        return false;
    }

    public static bool HubMobjCreationBlocked(SpecRestState st, VmFScriptScript.FsScript newVarScript, int newVarType) =>
        newVarScript != st.Expr.Script && newVarType == VmFScriptVariable.SvtMobj;

    public static bool TryResolveStructureCallable(
        SpecRestState st,
        int nameToken,
        out VmFScriptScript.FsVariable? func,
        out string? error)
    {
        error = null;
        func = VmFScriptVariable.VariableForName(st.Expr.GlobalScript!, st.Expr.Tokens[nameToken]);
        if (func == null)
        {
            error = $"no such function: '{st.Expr.Tokens[nameToken]}'\n";
            return false;
        }

        if (func.Type != VmFScriptVariable.SvtFunction)
        {
            error = $"'{st.Expr.Tokens[nameToken]}' not a function\n";
            return false;
        }

        return true;
    }

    public static void MaterializeOpStructureArguments(SpecRestState st, int start, int dotIndex, int stop, out int argc)
    {
        EvaluateExpression(st.Expr, st.TArgv[0], start, dotIndex - 1);
        argc = 1;
        if (stop != dotIndex + 1)
            argc = CollectCommaSeparatedArguments(st, dotIndex + 3, stop, 1);
    }

    public static bool TryParseScriptNumber(SpecRestState st, out int scriptNum, out string? error)
    {
        error = null;
        scriptNum = 0;
        if (st.Expr.NumTokens < 2)
        {
            error = "need newscript number\n";
            return false;
        }

        var result = new ScriptValue();
        EvaluateExpression(st.Expr, result, 1, st.Expr.NumTokens - 1);
        scriptNum = IntValue(result);
        if (scriptNum < 0)
        {
            error = "invalid newscript number\n";
            return false;
        }

        return true;
    }

    public static void AttachChildScriptFromSection(SpecRestState st, int scriptNum, VmFScriptScript.FsScript newScript)
    {
        st.StoreChildScript(st.Expr.Script!, scriptNum, newScript);
        newScript.Data = CopyChildScriptSectionBody(st, st.Expr.Script!, st.Section!);
        newScript.ScriptNum = scriptNum;
        newScript.Parent = st.Expr.Script;
        var host = new VmFScriptPrepro.PreprocessHost { Level = st.Level };
        st.PreprocessScript(host, newScript);
        st.Rover = st.SectionEnd(st.Section!) + 1;
    }

    public static VarDeclHeader ParseVariableDeclarationHeader(SpecRestState st)
    {
        var start = 0;
        var targetScript = st.Expr.Script!;
        if (st.Expr.Tokens[start] == "hub")
            start++;
        if (st.Expr.Tokens[start] == "const")
            return new VarDeclHeader(start + 1, VmFScriptVariable.SvtConst, targetScript, false);
        if (st.Expr.Tokens[start] == "string")
            return new VarDeclHeader(start + 1, VmFScriptVariable.SvtString, targetScript, false);
        if (st.Expr.Tokens[start] == "int")
            return new VarDeclHeader(start + 1, VmFScriptVariable.SvtInt, targetScript, false);
        if (st.Expr.Tokens[start] == "mobj")
            return new VarDeclHeader(start + 1, VmFScriptVariable.SvtMobj, targetScript, false);
        if (st.Expr.Tokens[start] is "fixed" or "float")
            return new VarDeclHeader(start + 1, VmFScriptVariable.SvtFixed, targetScript, false);
        if (st.Expr.Tokens[start] == "script")
            return new VarDeclHeader(start, -1, targetScript, true);
        return new VarDeclHeader(start, -1, targetScript, false);
    }

    public static int CollectCommaSeparatedArguments(
        SpecRestState st,
        int firstToken,
        int stopInclusive,
        int firstArgvSlot)
    {
        var argc = firstArgvSlot;
        var endpoint = firstToken;
        while (endpoint < stopInclusive)
        {
            var startpoint = endpoint;
            endpoint = FindOperator(st.Expr, startpoint, stopInclusive - 1, ",");
            if (endpoint == -1)
                endpoint = stopInclusive;
            if (endpoint - 1 < startpoint)
                break;
            EvaluateExpression(st.Expr, st.TArgv[argc], startpoint, endpoint - 1);
            endpoint++;
            argc++;
        }

        return argc;
    }

    public static void ResetFunctionReturnValue(SpecRestState st)
    {
        st.TReturn.Type = SvtInt;
        st.TReturn.IntValue = 0;
    }

    public static VmFScriptScript.FsVariable? ResolveGlobalCallable(SpecRestState st, int nameToken, out string error)
    {
        error = "";
        var name = st.Expr.Tokens[nameToken];
        var func = VmFScriptVariable.VariableForName(st.Expr.GlobalScript!, name);
        if (func == null)
        {
            error = $"no such function: '{name}'\n";
            return null;
        }

        if (func.Type != VmFScriptVariable.SvtFunction && func.Type != VmFScriptVariable.SvtLinespec)
        {
            error = $"'{name}' not a function\n";
            return null;
        }

        return func;
    }

    public static void InvokeCallable(SpecRestState st, VmFScriptScript.FsVariable func, int nameToken)
    {
        if (func.Type == VmFScriptVariable.SvtFunction)
        {
            if (func.FunctionHandler == null)
                st.Expr.ScriptError($"'{st.Expr.Tokens[nameToken]}' not a function\n");
            else
                st.InvokeSfHandler(func.FunctionHandler);
            return;
        }

        st.RunLineSpecial(func.LineSpecNumber);
    }

    public static byte[] CopyChildScriptSectionBody(SpecRestState st, VmFScriptScript.FsScript parentScript, FsSection section)
    {
        var dataSize = section.EndIndex - section.StartIndex - 2;
        var src = st.SectionStartChars(parentScript, section.StartIndex, section.EndIndex, dataSize);
        var bytes = new byte[dataSize + 1];
        for (var i = 0; i < dataSize; i++)
            bytes[i] = (byte)src[i + 1];
        bytes[dataSize] = 0;
        return bytes;
    }

    public static void CreateVariable(SpecRestState st, int newVarType, VmFScriptScript.FsScript newVarScript, int start, int stop)
    {
        if (!ValidateVariableNameToken(st.Expr, start, out var nameError))
        {
            st.Expr.ScriptError(nameError!);
            return;
        }

        if (VmFScriptVariable.VariableForName(newVarScript, st.Expr.Tokens[start]) != null)
        {
            st.Expr.Printf($"FS: redefined symbol: '{st.Expr.Tokens[start]}'\n");
            return;
        }

        if (HubMobjCreationBlocked(st, newVarScript, newVarType))
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
        var header = ParseVariableDeclarationHeader(st);
        if (header.IsScriptDecl)
        {
            SpecScript(st);
            return true;
        }

        if (header.VarType != -1)
        {
            ParseVarLine(st, header.VarType, header.TargetScript, header.StartToken);
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

        if (!TryParseScriptNumber(st, out var scriptNum, out var parseError))
        {
            st.Expr.ScriptError(parseError!);
            return;
        }

        var newScript = st.CreateChildScript();
        AttachChildScriptFromSection(st, scriptNum, newScript);
    }

    public static void RunEvaluateFunctionPipeline(SpecRestState st, ScriptValue result, int start, int stop)
    {
        if (!ValidateFunctionCallSyntax(st.Expr, start, stop, out var syntaxError))
        {
            st.Expr.ScriptError(syntaxError!);
            return;
        }

        var func = ResolveGlobalCallable(st, start, out var error);
        if (func == null)
        {
            st.Expr.ScriptError(error);
            return;
        }

        st.TArgc = CollectCommaSeparatedArguments(st, start + 2, stop, 0);
        ResetFunctionReturnValue(st);
        InvokeCallable(st, func, start);
        CopyReturn(st.TReturn, result);
    }

    public static void EvaluateFunction(SpecRestState st, ScriptValue result, int start, int stop) =>
        RunEvaluateFunctionPipeline(st, result, start, stop);

    public static void RunOpStructurePipeline(SpecRestState st, ScriptValue result, int start, int n, int stop)
    {
        if (!TryResolveStructureCallable(st, n + 1, out var func, out var resolveError))
        {
            st.Expr.ScriptError(resolveError!);
            return;
        }

        MaterializeOpStructureArguments(st, start, n, stop, out var argc);
        st.TArgc = argc;
        st.TFunc = func!.Name;
        ResetFunctionReturnValue(st);
        if (func.FunctionHandler == null)
            st.Expr.ScriptError($"'{st.Expr.Tokens[n + 1]}' not a function\n");
        else
            st.InvokeSfHandler(func.FunctionHandler);
        CopyReturn(st.TReturn, result);
    }

    public static void OpStructure(SpecRestState st, ScriptValue result, int start, int n, int stop) =>
        RunOpStructurePipeline(st, result, start, n, stop);

    private static void CopyReturn(ScriptValue src, ScriptValue dst)
    {
        dst.Type = src.Type;
        dst.String = src.String;
        dst.IntValue = src.IntValue;
        dst.FixedBits = src.FixedBits;
        dst.Mobj = src.Mobj;
    }
}
