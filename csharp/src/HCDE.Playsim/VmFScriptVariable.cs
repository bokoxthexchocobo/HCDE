using System.Globalization;

namespace HCDE.Playsim;

public static class VmFScriptVariable
{
    public const int SvtString = 0;
    public const int SvtInt = 1;
    public const int SvtMobj = 2;
    public const int SvtFunction = 3;
    public const int SvtConst = 5;
    public const int SvtFixed = 6;
    public const int SvtPInt = 7;
    public const int SvtPMobj = 8;
    public const int SvtLinespec = 9;

    public sealed class ScriptValue
    {
        public int Type { get; set; } = SvtInt;
        public string String { get; set; } = "";
        public int IntValue { get; set; }
        public long FixedBits { get; set; }
        public object? Mobj { get; set; }
    }

    public sealed class VariableHost
    {
        public IList<object?> SpawnedThings { get; init; } = Array.Empty<object?>();
        public Func<object?, bool> IsMapActor { get; init; } = _ => true;
        public Func<object?, string> ActorClassName { get; init; } = _ => "";
        public Action<string> ScriptError { get; init; } = _ => { };
    }

    public static int IntValue(ScriptValue v) =>
        v.Type == SvtString ? int.Parse(v.String, CultureInfo.InvariantCulture) :
        v.Type == SvtFixed ? (int)(v.FixedBits / 65536.0) :
        v.Type == SvtMobj ? -1 : v.IntValue;

    public static long FixedValue(ScriptValue v) =>
        v.Type == SvtFixed ? v.FixedBits :
        v.Type == SvtString ? (long)(double.Parse(v.String, CultureInfo.InvariantCulture) * 65536.0) :
        v.Type == SvtMobj ? -65536 : v.IntValue * 65536L;

    public static double FloatValue(ScriptValue v) =>
        v.Type == SvtString ? double.Parse(v.String, CultureInfo.InvariantCulture) :
        v.Type == SvtFixed ? v.FixedBits / 65536.0 :
        v.Type == SvtMobj ? -1.0 : v.IntValue;

    public static string StringValue(ScriptValue v, VariableHost host) =>
        v.Type switch
        {
            SvtString => v.String,
            SvtMobj => v.Mobj == null ? "" : host.ActorClassName(v.Mobj),
            SvtFixed => (v.FixedBits / 65536.0).ToString(CultureInfo.InvariantCulture),
            _ => v.IntValue.ToString(CultureInfo.InvariantCulture),
        };

    public static object? ActorValue(VariableHost host, ScriptValue svalue)
    {
        if (svalue.Type == SvtMobj)
            return svalue.Mobj == null || !host.IsMapActor(svalue.Mobj) ? null : svalue.Mobj;
        var idx = IntValue(svalue);
        if (idx < 0 || idx >= host.SpawnedThings.Count)
            return null;
        var thing = host.SpawnedThings[idx];
        return thing == null || !host.IsMapActor(thing) ? null : thing;
    }

    public static void GetValue(VmFScriptScript.FsVariable variable, ScriptValue dst)
    {
        switch (variable.Type)
        {
            case SvtPInt:
                dst.Type = SvtInt;
                dst.IntValue = variable.PointerInt ?? 0;
                break;
            case SvtPMobj:
                dst.Type = SvtMobj;
                dst.Mobj = variable.PointerMobj;
                break;
            case SvtMobj:
                dst.Type = SvtMobj;
                dst.Mobj = variable.Actor;
                break;
            case SvtLinespec:
                dst.Type = SvtInt;
                dst.IntValue = variable.LineSpecNumber;
                break;
            case SvtString:
                dst.Type = SvtString;
                dst.String = variable.String;
                break;
            default:
                dst.Type = variable.Type;
                dst.IntValue = variable.IntValue;
                dst.FixedBits = variable.FixedBits;
                break;
        }
    }

    public static void SetValue(VariableHost host, VmFScriptScript.FsVariable variable, ScriptValue src)
    {
        if (variable.Type == SvtConst)
            variable.Type = src.Type;
        switch (variable.Type)
        {
            case SvtInt:
                variable.IntValue = IntValue(src);
                break;
            case SvtString:
                variable.String = src.Type == SvtString ? src.String : StringValue(src, host);
                break;
            case SvtFixed:
                variable.FixedBits = FixedValue(src);
                break;
            case SvtMobj:
                variable.Actor = ActorValue(host, src);
                break;
            case SvtPInt:
                variable.PointerInt = IntValue(src);
                break;
            case SvtPMobj:
                variable.PointerMobj = ActorValue(host, src);
                break;
            case SvtLinespec:
                host.ScriptError("invalid variable type\n");
                break;
            case SvtFunction:
                host.ScriptError("attempt to set function to a value\n");
                break;
            default:
                host.ScriptError("invalid variable type\n");
                break;
        }
    }

    public static VmFScriptScript.FsVariable NewVariable(VmFScriptScript.FsScript script, string name, int vtype)
    {
        var v = new VmFScriptScript.FsVariable { Name = name, Type = vtype };
        var slot = VariableHash(name);
        v.Next = script.Variables[slot];
        script.Variables[slot] = v;
        return v;
    }

    public static void NewFunction(VmFScriptScript.FsScript script, string name, Action handler) =>
        NewVariable(script, name, SvtFunction).FunctionHandler = handler;

    public static VmFScriptScript.FsVariable? VariableForName(VmFScriptScript.FsScript script, string name)
    {
        for (var cur = script.Variables[VariableHash(name)]; cur != null; cur = cur.Next)
            if (cur.Name == name)
                return cur;
        return null;
    }

    public static VmFScriptScript.FsVariable? FindVariable(
        VmFScriptScript.FsScript script,
        string name,
        VmFScriptScript.FsScript? global) 
    {
        for (var cur = script; cur != null;)
        {
            var hit = VariableForName(cur, name);
            if (hit != null)
                return hit;
            cur = cur.Parent == null && global != null && cur != global ? global : cur.Parent;
        }
        return null;
    }

    public static void ClearVariables(VmFScriptScript.FsScript script, bool complete, Action<VmFScriptScript.FsVariable> destroy)
    {
        for (var i = 0; i < VmFScriptScript.VariableSlots; i++)
        {
            var cur = script.Variables[i];
            while (cur != null)
            {
                if (cur.Type == VmFScriptScript.SvtLabel && !complete)
                    break;
                var next = cur.Next;
                destroy(cur);
                cur = next;
            }
            script.Variables[i] = cur;
        }
    }

    public static int? LabelValue(VmFScriptScript.FsScript script, ScriptValue v) =>
        v.Type == VmFScriptScript.SvtLabel ? v.IntValue : null;

    public static int VariableHash(string name)
    {
        if (name.Length == 0) return 0;
        var c1 = name.Length > 1 ? name[1] : '\0';
        var c2 = name.Length > 2 ? name[2] : '\0';
        var c3 = name.Length > 3 ? name[3] : '\0';
        return (name[0] + c1 + (c1 != 0 ? c2 + (c2 != 0 ? c3 : 0) : 0)) % VmFScriptScript.VariableSlots;
    }
}
