namespace HCDE.Playsim;

/// <summary>
/// ZScript scope sides and field access from <c>scopebarrier.cpp</c> (<b>1–223</b>).
/// Chunk <b>1207</b>. Class and function names stand in for <c>PClass</c> and <c>VMFunction</c>.
/// A missing function type is the native null pointer. <c>X_OTHER</c> has no extra sentence.
/// </summary>
public sealed class VmScopeBarrier1207Slice
{
    public const int VarfReadOnly = 1 << 4;
    public const int VarfUi = 1 << 20;
    public const int VarfPlay = 1 << 21;
    public const int VarfVirtualScope = 1 << 22;
    public const int VarfClearScope = 1 << 23;
    public const int VarfNoRollback = 1 << 26;
    public const int ScopeAll = 0;
    public const int ScopeUi = 1;
    public const int ScopePlay = 2;
    public const int SidePlain = 0;
    public const int SideUi = 1;
    public const int SidePlay = 2;
    public const int SideVirtual = 3;
    public const int SideClear = 4;

    public bool Callable = true;
    public bool Readable = true;
    public bool Writable = true;
    public string CallError = "";
    public string ReadError = "";
    public string WriteError = "";
    public int SideFrom = -1;
    public int SideLast = -1;

    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message) { Detail = message; }
        public string Detail;
    }

    public VmScopeBarrier1207Slice()
    {
        Callable = true;
        Readable = true;
        Writable = true;
        CallError = "";
        ReadError = "";
        WriteError = "";
        SideFrom = -1;
        SideLast = -1;
    }

    public VmScopeBarrier1207Slice(int flags1, int flags2, string? name)
    {
        AddFlags(flags1, flags2, name);
    }

    public static int SideFromFlags(int flags)
    {
        if ((flags & VarfUi) != 0)
            return SideUi;
        if ((flags & VarfPlay) != 0)
            return SidePlay;
        if ((flags & VarfVirtualScope) != 0)
            return SideVirtual;
        if ((flags & VarfClearScope) != 0)
            return SideClear;
        return SidePlain;
    }

    public static int SideFromObjectFlags(int flags)
    {
        if ((flags & ScopeUi) != 0)
            return SideUi;
        if ((flags & ScopePlay) != 0)
            return SidePlay;
        return SidePlain;
    }

    public static int FlagsFromSide(int side)
    {
        switch (side)
        {
            case SidePlay: return VarfPlay;
            case SideUi: return VarfUi;
            case SideVirtual: return VarfVirtualScope;
            case SideClear: return VarfClearScope;
            default: return 0;
        }
    }

    public static int ObjectFlagsFromSide(int side)
    {
        switch (side)
        {
            case SidePlay: return ScopePlay;
            case SideUi: return ScopeUi;
            default: return ScopeAll;
        }
    }

    public static string StringFromSide(int side)
    {
        switch (side)
        {
            case SidePlain: return "data";
            case SideUi: return "ui";
            case SidePlay: return "play";
            case SideVirtual: return "virtualscope";
            case SideClear: return "clearscope";
            default: return "unknown";
        }
    }

    public static int ChangeSideInFlags(int flags, int side)
    {
        flags &= ~(VarfUi | VarfPlay | VarfVirtualScope | VarfClearScope | VarfNoRollback);
        flags |= FlagsFromSide(side);
        return flags;
    }

    public static int ChangeSideInObjectFlags(int flags, int side)
    {
        flags &= ~(ScopeUi | ScopePlay);
        flags |= ObjectFlagsFromSide(side);
        return flags;
    }

    public void AddFlags(int flags1, int flags2, string? name)
    {
        if (!Readable)
            return;
        flags1 &= VarfUi | VarfPlay;
        flags2 &= VarfUi | VarfPlay | VarfReadOnly;
        if (SideFrom < 0)
            SideFrom = SideFromFlags(flags1);
        if (SideLast < 0)
            SideLast = SideFrom;
        int sideTo = SideFromFlags(flags2);
        if (sideTo != SidePlain)
            SideLast = sideTo;
        else
            sideTo = SideLast;
        if (sideTo == SideUi && SideFrom != SideUi)
        {
            Readable = false;
            if (name != null)
                ReadError = $"Can't read {StringFromSide(sideTo)} field {name} from {StringFromSide(SideFrom)} context";
        }

        if (!Readable)
        {
            Writable = false;
            Callable = false;
            if (name != null)
            {
                WriteError = $"Can't write {StringFromSide(sideTo)} field {name} from {StringFromSide(SideFrom)} context (not readable)";
                CallError = $"Can't call {StringFromSide(sideTo)} function {name} from {StringFromSide(SideFrom)} context (not readable)";
            }

            return;
        }

        if (Writable && SideFrom != sideTo)
        {
            Writable = false;
            if (name != null)
                WriteError = $"Can't write {StringFromSide(sideTo)} field {name} from {StringFromSide(SideFrom)} context";
        }

        if (Callable && SideFrom != sideTo && (flags2 & VarfReadOnly) == 0)
        {
            Callable = false;
            if (name != null)
                CallError = $"Can't call {StringFromSide(sideTo)} function {name} from {StringFromSide(SideFrom)} context";
        }
    }

    public static bool CheckSidesForFunctionPointer(int from, int to)
    {
        if (to == -1)
            return true;
        if (from == SideClear)
            from = SidePlain;
        return from == to || from == SidePlain;
    }

    public static void ValidateNew(bool hasType, string className, int scopeFlags, int outerSide)
    {
        if (!hasType)
            throw new ScriptError($"VM execution aborted: Cannot instantiate invalid class {className}");
        int inner = SideFromObjectFlags(scopeFlags);
        if (outerSide != inner && inner != SidePlain)
            throw new ScriptError($"VM execution aborted: Cannot construct {StringFromSide(inner)} class {className} from {StringFromSide(outerSide)} context");
    }

    public static void ValidateCall(bool hasType, int scopeFlags, string functionName, int outerSide)
    {
        if (!hasType)
            throw new NullReferenceException();
        int inner = SideFromObjectFlags(scopeFlags);
        if (outerSide != inner && inner != SidePlain)
            throw new ScriptError($"VM execution aborted: Cannot call {StringFromSide(inner)} function {functionName} from {StringFromSide(outerSide)} context");
    }
}
