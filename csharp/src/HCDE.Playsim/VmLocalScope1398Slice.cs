namespace HCDE.Playsim;

/// <summary>
/// Local scope node from <c>LocalScopeStateNode.cpp</c> 1-168. Chunk 1398.
/// Locals load, class descriptor, action check, id, and node creation stay callees.
/// </summary>
public static class VmLocalScope1398Slice
{
    public const string LocalName = "Local";
    public const string SelfName = "self";
    public const string InvokerName = "invoker";
    public const string LineMarker = " @ line ";
    public const string PresentationHint = "locals";

    public sealed class Frame
    {
        public bool IsScriptFunction;
        public int CurrentLine;
        public int ImplicitArgs;
        public bool IsAction;
        public int Id;
    }

    public sealed class Local
    {
        public string Name = "";
        public int Line = -1;
        public object? Type;
        public object? Value;
    }

    public sealed class Child
    {
        public string Name = "";
        public bool CanRename = true;
        public object? Value;
        public object? Type;
        public object? Invoker;
        public Frame? Frame;
    }

    public sealed class Scope
    {
        public Frame? Frame;
        public List<Local> Locals { get; } = new();
        public CaselessMap<Child> Children { get; } = new();
    }

    /// <summary>std::map with <c>ci_less</c>: lookup folds A-Z, and a hit keeps the stored key.</summary>
    public sealed class CaselessMap<T>
    {
        private readonly List<(string Key, T Value)> _items = new();
        public int Count => _items.Count;
        public void Clear() => _items.Clear();
        public bool Contains(string key) => IndexOf(key) >= 0;

        public T Get(string key)
        {
            int index = IndexOf(key);
            if (index < 0)
                throw new InvalidOperationException(key);
            return _items[index].Value;
        }

        public void Set(string key, T value)
        {
            int index = IndexOf(key);
            if (index >= 0)
                _items[index] = (_items[index].Key, value);
            else
                _items.Add((key, value));
        }

        public bool Remove(string key)
        {
            int index = IndexOf(key);
            if (index < 0)
                return false;
            _items.RemoveAt(index);
            return true;
        }

        private int IndexOf(string key)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (CiEqual(_items[i].Key, key))
                    return i;
            }
            return -1;
        }
    }

    public readonly record struct ProtocolScope(string Name, bool Expensive, int VariablesReference, string Hint, int NamedVariables, int IndexedVariables);

    public static Scope Create(Frame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));
        return new Scope { Frame = frame };
    }

    public static string GetLineQualifiedName(string name, int line)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        return name + LineMarker + line.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static int GetLineFromLineQualifiedName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        int pos = name.IndexOf(LineMarker, StringComparison.Ordinal);
        if (pos < 0)
            return -1;
        return ParseStoi(name.Substring(pos + LineMarker.Length));
    }

    public static ProtocolScope SerializeToProtocol(Scope scope, List<string> names, Func<Frame, IReadOnlyList<Local>> load)
    {
        GetChildNames(scope, names, load);
        return new ProtocolScope(LocalName, false, scope.Frame!.Id, PresentationHint, names.Count, 0);
    }

    public static bool GetChildNames(Scope scope, List<string> names, Func<Frame, IReadOnlyList<Local>> load)
    {
        if (names == null)
            throw new ArgumentNullException(nameof(names));
        EnsureLocals(scope, load);
        Frame frame = scope.Frame!;
        var localIdx = new List<int>();
        for (int i = 0; i < scope.Locals.Count; i++)
        {
            Local local = scope.Locals[i];
            if (frame.IsScriptFunction && frame.CurrentLine <= local.Line)
                continue;
            int oldPos = IndexOfOrdinal(names, local.Name);
            string name = local.Name;
            if (oldPos >= 0)
            {
                names[oldPos] = GetLineQualifiedName(local.Name, scope.Locals[localIdx[oldPos]].Line);
                name = GetLineQualifiedName(local.Name, local.Line);
            }
            localIdx.Add(i);
            names.Add(name);
        }
        return true;
    }

    public static void CacheChildren(
        Scope scope,
        Func<Frame, IReadOnlyList<Local>> load,
        Func<object?, object?> classDescriptor,
        Func<string, object?, object?, Frame, object?, Child> create)
    {
        if (classDescriptor == null)
            throw new ArgumentNullException(nameof(classDescriptor));
        if (create == null)
            throw new ArgumentNullException(nameof(create));
        EnsureLocals(scope, load);
        scope.Children.Clear();
        Frame frame = scope.Frame!;
        object? invoker = ResolveInvoker(frame, scope.Locals, classDescriptor);
        var lines = new CaselessMap<int>();
        foreach (Local local in scope.Locals)
        {
            if (frame.IsScriptFunction && frame.CurrentLine <= local.Line)
                continue;
            string name = local.Name;
            Child node = create(name, local.Value, local.Type, frame, invoker);
            if (scope.Children.Contains(name))
            {
                Child previous = scope.Children.Get(name);
                if (previous.CanRename && node.CanRename)
                {
                    int oldLine = lines.Get(name);
                    string oldName = GetLineQualifiedName(name, oldLine);
                    string newName = GetLineQualifiedName(name, local.Line);
                    previous.Name = oldName;
                    node.Name = newName;
                    scope.Children.Set(oldName, previous);
                    scope.Children.Remove(name);
                    lines.Remove(name);
                    lines.Set(oldName, oldLine);
                    name = newName;
                }
            }
            lines.Set(name, local.Line);
            scope.Children.Set(name, node);
        }
    }

    public static bool TryGetChildNode(
        Scope scope,
        string name,
        Func<Frame, IReadOnlyList<Local>> load,
        Func<object?, object?> classDescriptor,
        Func<string, object?, object?, Frame, object?, Child> create,
        out Child? node)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        EnsureLocals(scope, load);
        if (scope.Children.Count == 0)
            CacheChildren(scope, load, classDescriptor, create);
        if (!scope.Children.Contains(name))
        {
            node = null;
            return false;
        }
        node = scope.Children.Get(name);
        return true;
    }

    public static int ParseStoi(string text)
    {
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        if (i >= text.Length)
            throw new FormatException(text);
        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        if (i >= text.Length || text[i] < '0' || text[i] > '9')
            throw new FormatException(text);
        long value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            int digit = text[i] - '0';
            if (value > (long.MaxValue - digit) / 10)
                throw new OverflowException(text);
            value = value * 10 + digit;
            if ((sign > 0 && value > int.MaxValue) || (sign < 0 && value > (long)int.MaxValue + 1))
                throw new OverflowException(text);
            i++;
        }
        return (int)(sign * value);
    }

    private static object? ResolveInvoker(Frame frame, List<Local> locals, Func<object?, object?> classDescriptor)
    {
        if (frame.ImplicitArgs >= 2 && locals.Count >= 2 && locals[1].Name == InvokerName)
            return classDescriptor(locals[1].Type);
        if (frame.IsAction && frame.ImplicitArgs >= 1 && locals.Count >= 1 && locals[0].Name == SelfName)
            return classDescriptor(locals[0].Type);
        return null;
    }

    private static void EnsureLocals(Scope scope, Func<Frame, IReadOnlyList<Local>> load)
    {
        if (scope == null)
            throw new ArgumentNullException(nameof(scope));
        if (scope.Frame == null)
            throw new InvalidOperationException("frame");
        if (load == null)
            throw new ArgumentNullException(nameof(load));
        if (scope.Locals.Count != 0)
            return;
        IReadOnlyList<Local> loaded = load(scope.Frame) ?? throw new InvalidOperationException("locals");
        scope.Locals.AddRange(loaded);
    }

    private static int IndexOfOrdinal(List<string> names, string name)
    {
        for (int i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], name, StringComparison.Ordinal))
                return i;
        }
        return -1;
    }

    private static bool CiEqual(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (CLower(left[i]) != CLower(right[i]))
                return false;
        }
        return true;
    }

    private static char CLower(char value) => value is >= 'A' and <= 'Z' ? (char)(value + 32) : value;

    private static bool IsCSpace(char value) => value is ' ' or '\f' or '\n' or '\r' or '\t' or '\v';
}
