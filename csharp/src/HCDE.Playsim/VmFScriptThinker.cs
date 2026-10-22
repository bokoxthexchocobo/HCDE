namespace HCDE.Playsim;

/// <summary><c>DFraggleThinker</c> and script dispatch from <c>fragglescript/t_script.cpp</c> (344–628).</summary>
public static class VmFScriptThinker
{
    public sealed class ThinkerState
    {
        public VmFScriptScript.RunningScript? RunningScripts { get; set; }
        public VmFScriptScript.FsScript? LevelScript { get; set; }
        public VmFScriptScript.FsScript? GlobalScript { get; set; }
        public List<object?> SpawnedThings { get; } = new();
        public Func<VmFScriptScript.FsScript> CreateFsScript { get; init; } = () => new VmFScriptScript.FsScript();
        public Func<object?, VmFScriptScript.FsScript?, int, VmFScriptScript.RunningScript> CreateRunningScript { get; init; } =
            (_, _, _) => new VmFScriptScript.RunningScript();
        public Action InitFunctions { get; init; } = () => { };
        public Action<VmFScriptScript.RunningScript> DestroyRunningScript { get; init; } = _ => { };
        public Action<VmFScriptScript.FsScript> DestroyFsScript { get; init; } = _ => { };
        public Action SuperOnDestroy { get; init; } = () => { };
        public Func<int, IEnumerable<int>> SectorsByTag { get; init; } = _ => Array.Empty<int>();
        public Func<int, bool> SectorHasMovingData { get; init; } = _ => false;
        public object? Player0Mo { get; set; }
        public Action<VmFScriptScript.FsScript, object?> Preprocess { get; init; } = (_, _) => { };
        public Func<long> SuperPropagateMark { get; init; } = () => 0;
        public Func<object?, object?, bool, long> SuperPointerSubstitution { get; init; } = (_, _, _) => 0;
    }

    public static void ConstructThinker(ThinkerState th)
    {
        th.GlobalScript = th.CreateFsScript();
        th.InitFunctions();
    }

    public static void ConstructLevel(ThinkerState th)
    {
        th.RunningScripts = th.CreateRunningScript(null, null, 0);
        th.LevelScript = th.CreateFsScript();
        th.LevelScript.Parent = null;
    }

    public static void OnDestroyThinker(ThinkerState th)
    {
        var p = th.RunningScripts?.Next;
        while (p != null)
        {
            var q = p;
            p = p.Next;
            q.Prev = null;
            q.Next = null;
            th.DestroyRunningScript(q);
        }
        if (th.RunningScripts != null)
            th.RunningScripts = null;
        if (th.GlobalScript != null)
        {
            th.DestroyFsScript(th.GlobalScript);
            th.GlobalScript = null;
        }
        if (th.LevelScript != null)
        {
            th.DestroyFsScript(th.LevelScript);
            th.LevelScript = null;
        }
        th.SpawnedThings.Clear();
        th.SuperOnDestroy();
    }

    public static bool WaitFinished(ThinkerState th, VmFScriptScript.RunningScript script)
    {
        switch (script.WaitType)
        {
            case VmFScriptScript.WtNone:
                return true;
            case VmFScriptScript.WtScriptwait:
            {
                for (var current = th.RunningScripts?.Next; current != null; current = current.Next)
                {
                    if (ReferenceEquals(current, script))
                        continue;
                    if (current.Script?.ScriptNum == script.WaitData)
                        return false;
                }
                return true;
            }
            case VmFScriptScript.WtDelay:
                return --script.WaitData <= 0;
            case VmFScriptScript.WtTagwait:
            {
                foreach (var secnum in th.SectorsByTag(script.WaitData))
                {
                    if (th.SectorHasMovingData(secnum))
                        return false;
                }
                return true;
            }
            case VmFScriptScript.WtScriptwaitpre:
            {
                for (var current = th.RunningScripts?.Next; current != null; current = current.Next)
                {
                    if (ReferenceEquals(current, script))
                        continue;
                    if (current.Script?.ScriptNum == script.WaitData)
                        return true;
                }
                return false;
            }
            default:
                return true;
        }
    }

    public static void Tick(ThinkerState th)
    {
        var current = th.RunningScripts?.Next;
        while (current != null)
        {
            VmFScriptScript.RunningScript? next;
            if (WaitFinished(th, current))
            {
                for (var i = 0; i < VmFScriptScript.VariableSlots; i++)
                {
                    if (current.Script != null)
                    {
                        current.Script.Variables[i] = current.Variables[i];
                        current.Variables[i] = null;
                    }
                }
                if (current.Script != null)
                    current.Script.Trigger = current.Trigger;
                if (current.Prev != null)
                    current.Prev.Next = current.Next;
                if (current.Next != null)
                    current.Next.Prev = current.Prev;
                next = current.Next;
                if (current.Script != null)
                {
                    VmFScriptScript.ParseScript(
                        current.Script,
                        current.SavePoint,
                        new VmFScriptScript.FraggleThinkerHost(),
                        _ => { },
                        _ => { });
                }
                th.DestroyRunningScript(current);
            }
            else
                next = current.Next;
            current = next;
        }
    }

    public static void AddRunningScript(ThinkerState th, VmFScriptScript.RunningScript runScr)
    {
        if (th.RunningScripts == null)
            return;
        runScr.Next = th.RunningScripts.Next;
        runScr.Prev = th.RunningScripts;
        th.RunningScripts.Next = runScr;
        if (runScr.Next != null)
            runScr.Next.Prev = runScr;
    }

    public static void PreprocessScripts(ThinkerState th)
    {
        if (th.LevelScript == null)
            return;
        th.LevelScript.Trigger = th.Player0Mo;
        th.Preprocess(th.LevelScript, th.Player0Mo);
        VmFScriptScript.ParseScript(th.LevelScript, null, new VmFScriptScript.FraggleThinkerHost(), _ => { }, _ => { });
    }

    public static long PropagateMark(ThinkerState th, Action<object?> mark)
    {
        for (var i = 0; i < th.SpawnedThings.Count; i++)
            mark(th.SpawnedThings[i]);
        return th.SuperPropagateMark();
    }

    public static long PointerSubstitution(ThinkerState th, object? oldObj, object? newObj, bool nullOnFail)
    {
        var changed = th.SuperPointerSubstitution(oldObj, newObj, nullOnFail);
        for (var i = 0; i < th.SpawnedThings.Count; i++)
        {
            if (ReferenceEquals(th.SpawnedThings[i], oldObj))
            {
                th.SpawnedThings[i] = newObj;
                changed++;
            }
        }
        return changed;
    }

    public static bool RunScript(ThinkerState? th, int snum, object? trigger)
    {
        if (th?.LevelScript == null)
            return false;
        if (snum < 0 || snum >= VmFScriptScript.MaxScripts)
            return false;
        var script = th.LevelScript.Children[snum];
        if (script == null)
            return false;
        var runScr = th.CreateRunningScript(trigger, script, 0);
        AddRunningScript(th, runScr);
        return true;
    }
}
