namespace HCDE.Playsim;

/// <summary><c>DFsScript</c> / <c>DRunningScript</c> from <c>fragglescript/t_script.cpp</c> (97–317).</summary>
public static class VmFScriptScript
{
    public const int VariableSlots = 16;
    public const int SectionSlots = 17;
    public const int MaxScripts = 257;
    public const int SvtLabel = 6;

    public const int WtNone = 0;
    public const int WtDelay = 1;
    public const int WtTagwait = 2;
    public const int WtScriptwait = 3;
    public const int WtScriptwaitpre = 4;

    public sealed class FsVariable
    {
        public string Name { get; set; } = "";
        public int Type { get; set; }
        public int IntValue { get; set; }
        public long FixedBits { get; set; }
        public string String { get; set; } = "";
        public object? Actor { get; set; }
        public int? PointerInt { get; set; }
        public object? PointerMobj { get; set; }
        public int LineSpecNumber { get; set; }
        public Action? FunctionHandler { get; set; }
        public FsVariable? Next { get; set; }
        public Action Destroy { get; init; } = () => { };
    }

    public sealed class FsScript
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int ScriptNum { get; set; } = -1;
        public int Len { get; set; }
        public FsScript? Parent { get; set; }
        public object? Trigger { get; set; }
        public bool LastIfTrue { get; set; }
        public object?[] Sections { get; } = new object?[SectionSlots];
        public FsVariable?[] Variables { get; } = new FsVariable?[VariableSlots];
        public FsScript?[] Children { get; } = new FsScript?[MaxScripts];

        public Action<bool> ClearVariables { get; init; } = _ => { };
        public Action ClearSections { get; init; } = () => { };
        public Action<FsScript?> DestroyChild { get; init; } = _ => { };
        public Action SuperOnDestroy { get; init; } = () => { };
        public Action ResetData { get; init; } = () => { };
        public Action<int, FraggleThinkerHost> ParseScriptBody { get; init; } = (_, _) => { };
    }

    public sealed class RunningScript
    {
        public RunningScript? Prev { get; set; }
        public RunningScript? Next { get; set; }
        public object? Trigger { get; set; }
        public FsScript? Script { get; set; }
        public int SavePoint { get; set; }
        public int WaitType { get; set; }
        public int WaitData { get; set; }
        public FsVariable?[] Variables { get; } = new FsVariable?[VariableSlots];
        public Action SuperOnDestroy { get; init; } = () => { };
        public Action<FsVariable?> DestroyVariable { get; init; } = _ => { };
    }

    public sealed class FraggleThinkerHost
    {
        public object? TriggerObj { get; set; }
    }

    public static void ClearChildren(FsScript script)
    {
        for (var j = 0; j < MaxScripts; j++)
        {
            if (script.Children[j] != null)
            {
                script.DestroyChild(script.Children[j]);
                script.Children[j] = null;
            }
        }
    }

    public static void InitNewScript(FsScript script)
    {
        for (var i = 0; i < SectionSlots; i++)
            script.Sections[i] = null;
        for (var i = 0; i < VariableSlots; i++)
            script.Variables[i] = null;
        for (var i = 0; i < MaxScripts; i++)
            script.Children[i] = null;
        script.ScriptNum = -1;
        script.Len = 0;
        script.Parent = null;
        script.Trigger = null;
        script.LastIfTrue = false;
    }

    public static void OnDestroyScript(FsScript script)
    {
        script.ClearVariables(true);
        script.ClearSections();
        ClearChildren(script);
        script.Parent = null;
        script.ResetData();
        script.Parent = null;
        script.Trigger = null;
        script.SuperOnDestroy();
    }

    public static void ParseScript(
        FsScript script,
        int? positionOffset,
        FraggleThinkerHost th,
        Action<string> printError,
        Action<Exception> printFraggleError)
    {
        int position;
        if (positionOffset == null)
        {
            script.LastIfTrue = false;
            position = 0;
        }
        else
            position = positionOffset.Value;

        if (position < 0 || position > script.Len)
        {
            printError($"script {script.ScriptNum}: trying to continue from point outside script!");
            return;
        }

        th.TriggerObj = script.Trigger;
        try
        {
            script.ParseScriptBody(position, th);
        }
        catch (Exception err)
        {
            printFraggleError(err);
        }

        if (script.ScriptNum != -1)
            script.ClearVariables(false);
        script.LastIfTrue = false;
    }

    public static void InitRunningScript(
        RunningScript run,
        object? trigger,
        FsScript? owner,
        int index)
    {
        run.Prev = null;
        run.Next = null;
        run.Script = owner;
        run.SavePoint = index;
        run.WaitType = WtNone;
        run.WaitData = 0;
        run.Trigger = trigger;

        if (owner == null)
        {
            for (var i = 0; i < VariableSlots; i++)
                run.Variables[i] = null;
            return;
        }

        for (var i = 0; i < VariableSlots; i++)
        {
            run.Variables[i] = owner.Variables[i];
            if (index == 0)
            {
                while (run.Variables[i] != null && run.Variables[i]!.Type != SvtLabel)
                    run.Variables[i] = run.Variables[i]!.Next;
            }
            else
            {
                while (owner.Variables[i] != null && owner.Variables[i]!.Type != SvtLabel)
                    owner.Variables[i] = owner.Variables[i]!.Next;
            }
        }
    }

    public static void OnDestroyRunningScript(RunningScript run)
    {
        for (var i = 0; i < VariableSlots; i++)
        {
            var current = run.Variables[i];
            while (current != null)
            {
                var next = current.Next;
                run.DestroyVariable(current);
                current = next;
            }
            run.Variables[i] = null;
        }
        run.SuperOnDestroy();
    }
}
