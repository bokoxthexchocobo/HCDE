namespace HCDE.Playsim;

/// <summary>
/// Native function and field tables from <c>imports.cpp</c> (<b>1–226</b>).
/// Chunk <b>1208</b>. Stored class names keep a one-character prefix that lookup names omit.
/// Symbol and class names fold ASCII case the way <c>FName</c> does. Font size and alignment
/// are the host <c>sizeof</c> and <c>alignof</c> of <c>FFont</c>.
/// </summary>
public static class VmImports1208Slice
{
    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message) { Detail = message; }
        public string Detail;
    }

    public sealed class NativeFunction
    {
        public string Name = "";
        public string QualifiedName = "";
        public string PrintableName = "";
        public object? DirectNativeCall;
        public int VarFlags = 8;
    }

    public sealed class NativeSlot
    {
        public NativeFunction? Function;
    }

    public sealed class FuncDesc
    {
        public string ClassName = "";
        public string FuncName = "";
        public int FunctionId;
        public NativeSlot? Slot;
        public object? DirectNative;
    }

    public sealed class FieldDesc
    {
        public string ClassName = "";
        public string FieldName = "";
        public int Offset;
        public int Size;
        public int BitValue;
    }

    public sealed class Symbol
    {
        public bool IsFunction;
        public bool HasVariant = true;
        public string? Implementation;
    }

    public sealed class VmClass
    {
        public string Name = "";
        public bool HasType = true;
        public VmClass? Parent;
        public readonly Dictionary<string, Symbol> Symbols = new(AsciiIgnoreCase.Comparer);
    }

    public sealed class FontStruct
    {
        public string Name = "";
        public bool Native;
        public int Size;
        public int Align;
        public bool PointerIsConst;
    }

    public static int HostFontSize;
    public static int HostFontAlign;
    public static FontStruct? Font;
    public static string? FontWriteKey;
    public static string? FontReadKey;
    public static readonly List<FuncDesc> PendingFunctions = new();
    public static readonly List<FieldDesc> PendingFields = new();
    public static readonly List<FuncDesc> Functions = new();
    public static readonly List<FieldDesc> Fields = new();
    public static readonly Dictionary<string, VmClass> Classes = new(AsciiIgnoreCase.Comparer);

    public static void Reset()
    {
        HostFontSize = 0;
        HostFontAlign = 0;
        Font = null;
        FontWriteKey = null;
        FontReadKey = null;
        PendingFunctions.Clear();
        PendingFields.Clear();
        Functions.Clear();
        Fields.Clear();
        Classes.Clear();
    }

    public static void Init()
    {
        Font = new FontStruct
        {
            Name = "Font",
            Native = true,
            Size = HostFontSize,
            Align = HostFontAlign,
            PointerIsConst = false,
        };
        Functions.Clear();
        if (Functions.Count == 0)
        {
            foreach (var source in PendingFunctions)
            {
                if (source.Slot == null)
                    throw new ScriptError("VMPointer is null.");
                var native = new NativeFunction
                {
                    Name = source.FuncName,
                    QualifiedName = Strip(source.ClassName) + "." + source.FuncName,
                    PrintableName = Strip(source.ClassName) + "." + source.FuncName + " [Native]",
                    DirectNativeCall = source.DirectNative,
                };
                source.Slot.Function = native;
                Functions.Add(new FuncDesc
                {
                    ClassName = source.ClassName,
                    FuncName = source.FuncName,
                    FunctionId = source.FunctionId,
                    Slot = source.Slot,
                    DirectNative = source.DirectNative,
                });
            }

            Functions.Sort(FuncCmp);
        }

        Fields.Clear();
        if (Fields.Count == 0)
        {
            foreach (var source in PendingFields)
            {
                Fields.Add(new FieldDesc
                {
                    ClassName = source.ClassName,
                    FieldName = source.FieldName,
                    Offset = source.Offset,
                    Size = source.Size,
                    BitValue = source.BitValue,
                });
            }

            Fields.Sort(FieldCmp);
        }
    }

    public static void WriteFont(string key)
    {
        FontWriteKey = key;
    }

    public static bool ReadFont(string key)
    {
        FontReadKey = key;
        return true;
    }

    public static FuncDesc? FindFunction(string? className, string func)
    {
        if (className == null)
            throw new NullReferenceException();
        return Search(Functions, className, func, static item => item.ClassName, static item => item.FuncName);
    }

    public static FieldDesc? FindField(string? className, string field)
    {
        string lookup = className ?? "";
        return Search(Fields, lookup, field, static item => item.ClassName, static item => item.FieldName);
    }

    public static string? FindVm(VmClass? cls, string name)
    {
        if (cls == null)
            throw new NullReferenceException();
        var symbol = FindSymbol(cls, name);
        if (symbol == null || !symbol.IsFunction)
            return null;
        if (!symbol.HasVariant)
            throw new ScriptError("function variant 0 is missing.");
        return symbol.Implementation;
    }

    public static string? FindVm(string qualified)
    {
        int dot = qualified.IndexOf('.');
        if (dot < 0)
            return null;
        if (!Classes.TryGetValue(qualified[..dot], out var cls))
            return null;
        return FindVm(cls, qualified[(dot + 1)..]);
    }

    public static void Register(VmClass cls)
    {
        Classes[cls.Name] = cls;
    }

    static Symbol? FindSymbol(VmClass cls, string name)
    {
        if (!cls.HasType)
            return null;
        var current = cls;
        while (current != null)
        {
            if (current.Symbols.TryGetValue(name, out var symbol))
                return symbol;
            current = current.Parent;
        }

        return null;
    }

    static T? Search<T>(List<T> table, string className, string member, Func<T, string> classOf, Func<T, string> nameOf)
    {
        int min = 0;
        int max = table.Count - 1;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            int lex = ClassCmp(className, classOf(table[mid]));
            if (lex == 0)
                lex = Stricmp(member, nameOf(table[mid]));
            if (lex == 0)
                return table[mid];
            if (lex > 0)
                min = mid + 1;
            else
                max = mid - 1;
        }

        return default;
    }

    static int FuncCmp(FuncDesc a, FuncDesc b)
    {
        int result = Stricmp(Strip(a.ClassName), Strip(b.ClassName));
        if (result == 0)
            result = Stricmp(a.FuncName, b.FuncName);
        return result;
    }

    static int FieldCmp(FieldDesc a, FieldDesc b)
    {
        int result = Stricmp(Strip(a.ClassName), Strip(b.ClassName));
        if (result == 0)
            result = Stricmp(a.FieldName, b.FieldName);
        return result;
    }

    static int ClassCmp(string lookup, string stored)
    {
        return Stricmp(lookup, Strip(stored));
    }

    static string Strip(string stored)
    {
        if (stored.Length == 0)
            return "";
        return stored[1..];
    }

    static int Stricmp(string left, string right)
    {
        int count = Math.Min(left.Length, right.Length);
        for (int i = 0; i < count; i++)
        {
            int diff = Fold(left[i]) - Fold(right[i]);
            if (diff != 0)
                return diff;
        }

        return left.Length - right.Length;
    }

    static int Fold(char c)
    {
        if (c is >= 'A' and <= 'Z')
            return c + 32;
        return c;
    }

    sealed class AsciiIgnoreCase : IEqualityComparer<string>
    {
        public static readonly AsciiIgnoreCase Comparer = new();

        public bool Equals(string? x, string? y)
        {
            if (x == null || y == null)
                return x == y;
            return Stricmp(x, y) == 0;
        }

        public int GetHashCode(string obj)
        {
            var hash = new HashCode();
            foreach (char c in obj)
                hash.Add(Fold(c));
            return hash.ToHashCode();
        }
    }
}
