namespace HCDE.Scripting;

public sealed class VmCVarScope1375Slice
{
    public const int BoolType = 0;
    public const int IntType = 1;
    public const int FloatType = 2;
    public const int StringType = 3;
    public const int ColorType = 4;
    public const int FlagType = 5;
    public const int MaskType = 6;
    public const int DummyType = 7;
    public const string InvalidText = "<INVALID>";

    public Func<string, string>? Localize { get; set; }
    public Func<string, CVar1375?>? FindCVar { get; set; }
    public List<CVar1375> Map { get; } = [];

    public CVar1375Variable ToVariable(CVar1375? cvar)
    {
        if (cvar == null)
            return new CVar1375Variable { Name = InvalidText, Value = InvalidText };

        var variable = new CVar1375Variable { Value = cvar.Human };
        var real = RealTypeName(cvar.RealType);
        if (cvar.RealType == FloatType && variable.Value.IndexOf('.') < 0 && variable.Value.IndexOf('e') < 0)
            variable.Value += ".0";
        if (cvar.RealType == StringType && cvar.FavoriteType == StringType)
            variable.Value = "\"" + variable.Value + "\"";
        if (cvar.Description.Length != 0)
        {
            if (Localize == null)
                throw new InvalidOperationException("localize");
            var localized = Localize(cvar.Description);
            if (localized.Length != 0 && Tail(localized) != cvar.Description)
                variable.Type = real + " (" + localized + ")";
            else
                variable.Type = real;
        }
        else
            variable.Type = real;
        variable.Name = cvar.Name;
        return variable;
    }

    public static string RealTypeName(int type)
    {
        if (type == BoolType)
            return "CVar<Bool>";
        if (type == IntType)
            return "CVar<Int>";
        if (type == FloatType)
            return "CVar<Float>";
        if (type == StringType)
            return "CVar<String>";
        if (type == ColorType)
            return "CVar<Color>";
        if (type == FlagType)
            return "CVar<Flag>";
        if (type == MaskType)
            return "CVar<Mask>";
        if (type == DummyType)
            return "CVar<Dummy>";
        return "<UNKNOWN>";
    }

    public static int CompareNames(string a, string b)
    {
        if (a.Length != 0 && b.Length != 0)
        {
            if (IsAsciiUpper(a[0]) && IsAsciiLower(b[0]))
                return 1;
            if (IsAsciiLower(a[0]) && IsAsciiUpper(b[0]))
                return -1;
        }
        return string.CompareOrdinal(a, b);
    }

    public static string Tail(string text)
    {
        if (text.Length == 0)
            return "";
        return text[1..];
    }

    static bool IsAsciiUpper(char c) => c is >= 'A' and <= 'Z';

    static bool IsAsciiLower(char c) => c is >= 'a' and <= 'z';
}

public sealed class CVar1375
{
    public string Name = "";
    public string Human = "";
    public int RealType;
    public int FavoriteType;
    public string Description = "";
}

public sealed class CVar1375Variable
{
    public string Name = "";
    public string Value = "";
    public string Type = "";
}

public sealed class CVar1375ProtocolScope
{
    public string Name = "";
    public bool Expensive;
    public string PresentationHint = "";
    public int VariablesReference;
    public int NamedVariables;
    public int IndexedVariables;
}

public sealed class CVar1375Node
{
    public CVar1375? CVar;

    public bool SerializeToProtocol(VmCVarScope1375Slice host, out CVar1375Variable variable)
    {
        variable = host.ToVariable(CVar);
        return true;
    }
}

public sealed class CVar1375Scope
{
    public int Id;
    public List<string> CachedNames { get; } = [];
    public Dictionary<string, CVar1375Node> Children { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool SerializeToProtocol(VmCVarScope1375Slice host, out CVar1375ProtocolScope scope)
    {
        scope = new CVar1375ProtocolScope
        {
            Name = "CVars",
            Expensive = true,
            PresentationHint = "cvars",
            VariablesReference = Id,
        };
        if (CachedNames.Count == 0)
            GetChildNames(host);
        scope.NamedVariables = CachedNames.Count;
        scope.IndexedVariables = 0;
        return true;
    }

    public List<string> GetChildNames(VmCVarScope1375Slice host)
    {
        if (CachedNames.Count != 0)
            return new List<string>(CachedNames);
        foreach (var cvar in host.Map)
        {
            CachedNames.Add(cvar.Name);
            Children[cvar.Name] = new CVar1375Node { CVar = cvar };
        }
        CachedNames.Sort(VmCVarScope1375Slice.CompareNames);
        return new List<string>(CachedNames);
    }

    public bool GetChildNode(string name, VmCVarScope1375Slice host, out CVar1375Node? node)
    {
        if (CachedNames.Count == 0)
            GetChildNames(host);
        if (!Children.ContainsKey(name))
        {
            if (host.FindCVar == null)
                throw new InvalidOperationException("FindCVar");
            var found = host.FindCVar(name);
            if (found == null)
            {
                node = null;
                return false;
            }
            Children[found.Name] = new CVar1375Node { CVar = found };
            CachedNames.Add(found.Name);
            CachedNames.Sort(VmCVarScope1375Slice.CompareNames);
        }
        if (Children.TryGetValue(name, out node))
            return true;
        node = null;
        return false;
    }
}
