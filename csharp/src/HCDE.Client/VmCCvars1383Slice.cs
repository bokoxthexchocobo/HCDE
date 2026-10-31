using System.Globalization;

namespace HCDE.Client;

/// <summary>
/// Handler install, cvar initialization, and base cvar lifetime from <c>c_cvars.cpp</c> 91-292.
/// </summary>
public sealed class VmCCvars1383Slice
{
    public const int NameLengthMax = 63;
    public const int Archive = 1;
    public const int UserInfo = 1 << 1;
    public const int ServerInfo = 1 << 2;
    public const int IsDefault = 1 << 7;
    public const int Ignore = 1 << 14;
    public const int UnsafeContext = 1 << 16;
    public const int Virtual = 1 << 17;
    public const int Hidden = 1 << 22;

    public enum Kind
    {
        Bool,
        Int,
        Float,
        String,
        Color,
        Flag,
        Mask,
        Dummy,
    }

    public sealed class ServerNameTooLongException(string message) : Exception(message);

    public sealed class Handlers
    {
        public Action<CVar>? UserInfoChanged;
    }

    public sealed class Decl
    {
        public string? Name;
        public Kind Type;
        public int Flags;
        public bool BoolDefault;
        public int IntDefault;
        public float FloatDefault;
        public string? StringDefault;
        public CVar? Pointer;
        public Action<CVar, string?>? Callback;
        public string? Description;
        public CVar? Ref;
    }

    public sealed class CVar
    {
        public Kind Kind;
        public string VarName = "";
        public string VarFName = "";
        public string? Description;
        public int Flags;
        public Action<CVar, string?>? Callback;
        public object? Extra;
        public object? Extra2;
        public string? ValueText;
        public string? GenericString;
        public string? GenericDefaultString;
        public string? SafeValue;
        public bool DefaultBool;
        public int DefaultInt;
        public float DefaultFloat;
        public string? DefaultString;
        public int BitValue;
        public CVar? Referenced;
        public int DoSetCount;
        public Kind LastSetType;
        public bool InCallback;
        public bool Destroyed;

        public string? HumanString(int precision)
        {
            _ = precision;
            return GenericString;
        }

        public string? HumanStringDefault(int precision)
        {
            _ = precision;
            return GenericDefaultString;
        }

        public void SetCallback(Action<CVar, string?>? callback) => Callback = callback;

        public void ClearCallback() => Callback = null;

        public void SetExtra(object? pointer) => Extra = pointer;

        public object? GetExtra() => Extra;

        public void SetExtra2(object? pointer) => Extra2 = pointer;

        public object? GetExtra2() => Extra2;
    }

    public Handlers? Callbacks;
    public bool UseCallback;
    public int LastWhich;
    public readonly Dictionary<string, CVar?> Map = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> TabAdds = new();
    public readonly List<string> TabRemoves = new();
    public readonly List<CVar> UserInfoNotes = new();
    public readonly List<(CVar Cvar, string? Previous)> CallbackNotes = new();
    public readonly List<CVar> Destroyed = new();

    public void InstallHandlers(Handlers? handlers)
    {
        Callbacks = handlers;
    }

    public void Init(int which, IReadOnlyList<Decl> decls)
    {
        // Native C_InitCVars accepts which and does not read it.
        LastWhich = which;
        foreach (var decl in decls)
        {
            if (decl.Type is not (Kind.Bool or Kind.Int or Kind.Float or Kind.String or Kind.Color))
                continue;
            var cvar = new CVar { Kind = decl.Type };
            Construct(cvar, decl.Name, decl.Flags, decl.Callback, decl.Description);
            ApplyScalarDefault(cvar, decl);
            decl.Ref = cvar;
        }

        foreach (var decl in decls)
        {
            if (decl.Type is not (Kind.Flag or Kind.Mask))
                continue;
            if (decl.Pointer == null)
                throw new InvalidOperationException("A flag or mask cvar requires a referenced int cvar.");
            var cvar = new CVar
            {
                Kind = decl.Type,
                BitValue = decl.Flags,
                Referenced = decl.Pointer,
            };
            // FFlagCVar / FMaskCVar pass 0 and a null callback into FBaseCVar.
            Construct(cvar, decl.Name, 0, null, decl.Description);
            cvar.Flags &= ~IsDefault;
            decl.Ref = cvar;
        }
    }

    public void Uninit()
    {
        // The value is cleared before delete, so the destructor's Find misses
        // and leaves both the key and the tab command in place.
        foreach (var key in Map.Keys.ToList())
        {
            var cvar = Map[key];
            Map[key] = null;
            if (cvar != null)
                Destroy(cvar);
        }
    }

    public void Construct(CVar cvar, string? name, int flags, Action<CVar, string?>? callback, string? description)
    {
        if (name != null && (flags & ServerInfo) != 0 && name.Length > NameLengthMax)
        {
            throw new ServerNameTooLongException(
                $"Name of the server console variable \"{name}\" is too long.\n" +
                $"Its length should not exceed {NameLengthMax} characters.\n");
        }

        cvar.Callback = callback;
        cvar.Flags = 0;
        cvar.VarName = "";
        cvar.VarFName = "";
        cvar.Description = description;

        CVar? previous = null;
        if (name != null)
        {
            previous = Find(name);
            if ((flags & Hidden) == 0)
                TabAdds.Add(name);
            cvar.VarName = name;
            cvar.VarFName = name;
            Map[name] = cvar;
        }

        if (previous != null)
        {
            ForceSet(cvar, previous.ValueText, previous.Kind, noUserInfoSend: false);
            Destroy(previous);
            cvar.Flags = flags;
        }
        else
        {
            cvar.Flags = flags | IsDefault;
        }
    }

    public void Destroy(CVar cvar)
    {
        cvar.Destroyed = true;
        Destroyed.Add(cvar);
        if (string.IsNullOrEmpty(cvar.VarName))
            return;
        if (!ReferenceEquals(Find(cvar.VarName), cvar))
            return;
        Map.Remove(cvar.VarName);
        TabRemoves.Add(cvar.VarName);
    }

    public CVar? Find(string? name)
    {
        // FindCVar rejects a null pointer and a name that becomes NAME_None.
        if (string.IsNullOrEmpty(name))
            return null;
        return Map.TryGetValue(name, out var cvar) ? cvar : null;
    }

    public void ForceSet(CVar cvar, string? text, Kind type, bool noUserInfoSend)
    {
        var previous = DoSet(cvar, text, type);
        if ((cvar.Flags & UserInfo) != 0
            && !noUserInfoSend
            && (cvar.Flags & Ignore) == 0
            && Callbacks?.UserInfoChanged != null)
        {
            UserInfoNotes.Add(cvar);
            Callbacks.UserInfoChanged(cvar);
        }

        if (UseCallback)
            DoCallback(cvar, previous);
        if ((cvar.Flags & Archive) != 0 && (cvar.Flags & UnsafeContext) == 0)
            cvar.SafeValue = cvar.GenericString;
        cvar.Flags &= ~(IsDefault | UnsafeContext);
    }

    string? DoSet(CVar cvar, string? text, Kind type)
    {
        cvar.DoSetCount++;
        cvar.LastSetType = type;
        var previous = cvar.ValueText;
        cvar.ValueText = text;
        cvar.GenericString = text;
        return previous;
    }

    void DoCallback(CVar cvar, string? previous)
    {
        // Flag and mask DoCallback overrides are empty. Scalar callbacks
        // skip while a virtual cvar is already inside its callback.
        if (cvar.Kind is Kind.Flag or Kind.Mask)
            return;
        if (cvar.Callback == null || cvar.InCallback)
            return;
        cvar.InCallback = (cvar.Flags & Virtual) != 0;
        try
        {
            CallbackNotes.Add((cvar, previous));
            cvar.Callback(cvar, previous);
        }
        finally
        {
            cvar.InCallback = false;
        }
    }

    static void ApplyScalarDefault(CVar cvar, Decl decl)
    {
        cvar.DefaultBool = decl.BoolDefault;
        cvar.DefaultInt = decl.IntDefault;
        cvar.DefaultFloat = decl.FloatDefault;
        cvar.DefaultString = decl.StringDefault;
        var formatted = FormatDefault(decl);
        cvar.GenericDefaultString = formatted;
        if ((cvar.Flags & IsDefault) == 0)
            return;
        cvar.ValueText = formatted;
        cvar.GenericString = formatted;
    }

    static string? FormatDefault(Decl decl)
    {
        return decl.Type switch
        {
            Kind.Bool => decl.BoolDefault ? "true" : "false",
            Kind.Int or Kind.Color => decl.IntDefault.ToString(CultureInfo.InvariantCulture),
            Kind.Float => decl.FloatDefault.ToString(CultureInfo.InvariantCulture),
            Kind.String => decl.StringDefault,
            _ => null,
        };
    }
}
