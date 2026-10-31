namespace HCDE.Playsim;

/// <summary>
/// DAP runtime path resolve, variable nodes, and the VM frame walk from
/// <c>RuntimeState.cpp</c> (1-324). Chunk 1340. State-node bodies stay in
/// their own types; this slice owns the path map and the factory order.
/// </summary>
public sealed class VmRuntimeState1340
{
    public const int DefaultChildCount = int.MaxValue;

    public static FrameStack? GlobalVmStack;

    public IdProvider Ids { get; }
    public StackNode Root { get; private set; }

    readonly IdMap _paths;

    public VmRuntimeState1340(IdProvider ids)
    {
        Ids = ids;
        _paths = new IdMap(ids);
        Root = new StackNode(1);
        Reset();
    }

    public void Reset()
    {
        _paths.Clear();
        Root = new StackNode(1);
    }

    public bool ResolveStateByPath(string requestedPath, out StateNode? node, bool isEvaluate = false)
    {
        string path = ToLowerCopy(requestedPath);
        List<string> elements = Split(path, ".");
        if (elements.Count == 0)
        {
            node = null;
            return false;
        }

        string stackIdElement = elements[0];
        int stackId = ParseStackId(stackIdElement);
        var currentPathElements = new List<string> { stackIdElement };
        elements.RemoveAt(0);
        StateNode? current = stackId == 1 ? Root : new StackNode(unchecked((uint)stackId));

        StateNode? Traverse(string currentName, StateNode part)
        {
            var structured = part as StructuredNode;
            string currentPath = ToLowerCopy(Join(currentPathElements, "."));
            if (structured != null)
            {
                var childNames = new List<string>();
                structured.GetChildNames(childNames);
                for (int i = 0; i < childNames.Count; i++)
                {
                    string childName = ToLowerCopy(childNames[i]);
                    _paths.AddOrGetExisting(currentPath + "." + childName, out uint addedId);
                    _ = addedId;
                }
            }

            if (structured != null && structured.GetChildNode(currentName, out StateNode? found))
            {
                currentPathElements.Add(currentName);
                return found;
            }

            if (structured != null && isEvaluate)
            {
                var containers = new List<(string Name, StructuredNode? Node)>(structured.GetVirtualContainerChildren());
                for (int i = 0; i < containers.Count; i++)
                {
                    if (containers[i].Node == null)
                    {
                        continue;
                    }

                    currentPathElements.Add(containers[i].Name);
                    StateNode? nested = Traverse(currentName, containers[i].Node!);
                    if (nested != null)
                    {
                        return nested;
                    }

                    currentPathElements.RemoveAt(currentPathElements.Count - 1);
                }
            }

            return null;
        }

        int cursor = 0;
        while (cursor < elements.Count && current != null)
        {
            current = Traverse(elements[cursor], current);
            if (current == null)
            {
                break;
            }

            cursor++;
        }

        if (current == null)
        {
            node = null;
            return false;
        }

        if (currentPathElements.Count > 1)
        {
            string actualPath = ToLowerCopy(Join(currentPathElements, "."));
            _paths.AddOrGetExisting(actualPath, out uint id);
            current.SetId(id);
        }
        else
        {
            current.SetId(unchecked((uint)stackId));
        }

        node = current;
        return true;
    }

    public string GetPathById(uint id)
    {
        if (id == 1)
        {
            return "1";
        }

        if (_paths.Get(id, out string? path) && path != null)
        {
            return path;
        }

        return "";
    }

    public bool ResolveStateById(uint id, out StateNode? node, bool isEvaluate = false)
    {
        if (_paths.Get(id, out string? path) && path != null)
        {
            return ResolveStateByPath(path, out node, isEvaluate);
        }

        node = null;
        return false;
    }

    public bool ResolveChildrenByParentPath(string requestedPath, List<StateNode> nodes, int start = 0, int count = DefaultChildCount)
    {
        if (!ResolveStateByPath(requestedPath, out StateNode? parent) || parent == null)
        {
            return false;
        }

        if (parent is not StructuredNode structured)
        {
            return false;
        }

        var childNames = new List<string>();
        structured.GetChildNames(childNames);
        long window = count == 0 ? childNames.Count : count;
        int maxCount = (int)Math.Min(window + start, childNames.Count);
        for (int i = start; i < maxCount; i++)
        {
            string childPath = requestedPath + "." + childNames[i];
            if (ResolveStateByPath(childPath, out StateNode? child) && child != null)
            {
                nodes.Add(child);
            }
            else
            {
                maxCount = Math.Min(childNames.Count, maxCount + 1);
            }
        }

        return true;
    }

    public bool ResolveChildrenByParentId(uint id, List<StateNode> nodes, int start = 0, int count = DefaultChildCount)
    {
        if (_paths.Get(id, out string? path) && path != null)
        {
            return ResolveChildrenByParentPath(path, nodes, start, count);
        }

        return false;
    }

    public static VariableNode? CreateNodeForVariable(string name, VmValue variable, PType type, Frame? currentFrame = null, PClass? stateOwningClass = null)
    {
        if (PointsAtNonStruct(type))
        {
            PType pointed = type.Pointed!;
            _ = pointed;
        }

        if (TypeIsArrayOrArrayPtr(type))
        {
            return new VariableNode(VariableKind.Array, name, variable, type, null, null);
        }

        if (type.IsBasicNonPointer() || PointsAtNonStruct(type))
        {
            return new VariableNode(VariableKind.Value, name, variable, type, stateOwningClass, null);
        }

        if (ReferenceEquals(type, PType.TypeState))
        {
            return new VariableNode(VariableKind.State, name, variable, null, stateOwningClass, null);
        }

        if (type.IsClass() || type.IsObjectPointer())
        {
            return new VariableNode(VariableKind.Object, name, variable, type, null, null);
        }

        if (TypeIsStructOrStructPtr(type) || TypeIsNonStructContainer(type))
        {
            return new VariableNode(VariableKind.Struct, name, variable, type, null, currentFrame);
        }

        return null;
    }

    public static FrameStack? GetStack(uint stackId)
    {
        _ = stackId;
        return GlobalVmStack;
    }

    public static Frame? GetFrame(uint stackId, uint level)
    {
        var frames = new List<Frame>();
        GetStackFrames(stackId, frames);
        if (frames.Count == 0 || level > (uint)(frames.Count - 1))
        {
            return null;
        }

        return frames[(int)level];
    }

    public static void GetStackFrames(FrameStack stack, List<Frame> frames)
    {
        if (!stack.HasFrames())
        {
            return;
        }

        Frame? frame = stack.TopFrame();
        while (frame != null)
        {
            frames.Add(frame);
            frame = frame.Parent;
        }
    }

    public static bool GetStackFrames(uint stackId, List<Frame> frames)
    {
        FrameStack? stack = GetStack(stackId);
        if (stack == null)
        {
            return false;
        }

        GetStackFrames(stack, frames);
        return true;
    }

    static bool PointsAtNonStruct(PType type)
    {
        if (!type.IsPointer() || type.IsObjectPointer())
        {
            return false;
        }

        PType pointed = type.Pointed ?? throw new InvalidOperationException("null pointed type");
        return !pointed.IsStruct();
    }

    static bool TypeIsStructOrStructPtr(PType? type)
    {
        if (type != null && type.IsPointer())
        {
            type = type.Pointed;
        }

        return type != null && type.IsStruct();
    }

    static bool TypeIsNonStructContainer(PType? type)
    {
        if (type != null && type.IsPointer())
        {
            type = type.Pointed;
        }

        return type != null && type.IsContainer() && !type.IsStruct();
    }

    static bool TypeIsArrayOrArrayPtr(PType type)
    {
        PType cursor = type;
        if (cursor.IsPointer())
        {
            cursor = cursor.Pointed ?? throw new InvalidOperationException("null pointed type");
        }

        return cursor.IsArray() || cursor.IsDynArray() || cursor.IsStaticArray();
    }

    static int ParseStackId(string text)
    {
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
        {
            i++;
        }

        if (i >= text.Length)
        {
            throw new FormatException("stoi");
        }

        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
            {
                sign = -1;
            }

            i++;
        }

        if (i >= text.Length || text[i] < '0' || text[i] > '9')
        {
            throw new FormatException("stoi");
        }

        long value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = value * 10 + (text[i] - '0');
            if (sign > 0 && value > int.MaxValue)
            {
                throw new OverflowException("stoi");
            }

            if (sign < 0 && value > (long)int.MaxValue + 1)
            {
                throw new OverflowException("stoi");
            }

            i++;
        }

        if (sign < 0)
        {
            return (int)-value;
        }

        return (int)value;
    }

    static string ToLowerCopy(string text)
    {
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= 'A' && chars[i] <= 'Z')
            {
                chars[i] = (char)(chars[i] + 32);
            }
        }

        return new string(chars);
    }

    static List<string> Split(string text, string delimiter)
    {
        var parts = new List<string>();
        int pos = 0;
        while (true)
        {
            int found = text.IndexOf(delimiter, pos, StringComparison.Ordinal);
            if (found < 0)
            {
                break;
            }

            parts.Add(text.Substring(pos, found - pos));
            pos = found + delimiter.Length;
        }

        parts.Add(text.Substring(pos));
        return parts;
    }

    static string Join(List<string> elements, string separator)
    {
        if (elements.Count == 0)
        {
            return "";
        }

        if (elements.Count == 1)
        {
            return elements[0];
        }

        return string.Join(separator, elements);
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    public sealed class IdProvider
    {
        uint _current = 1000;

        public uint GetNext()
        {
            return _current++;
        }
    }

    public sealed class IdMap
    {
        readonly IdProvider _ids;
        readonly Dictionary<uint, string> _byId = new();
        readonly Dictionary<string, uint> _byElement = new(StringComparer.Ordinal);

        public IdMap(IdProvider ids)
        {
            _ids = ids;
        }

        public bool Get(uint id, out string? value)
        {
            if (_byId.TryGetValue(id, out string? found))
            {
                value = found;
                return true;
            }

            value = null;
            return false;
        }

        public bool AddOrGetExisting(string element, out uint id)
        {
            if (_byElement.TryGetValue(element, out id))
            {
                return false;
            }

            id = _ids.GetNext();
            _byId[id] = element;
            _byElement[element] = id;
            return true;
        }

        public void Clear()
        {
            _byId.Clear();
            _byElement.Clear();
        }
    }

    public class StateNode
    {
        public uint Id { get; private set; }

        public void SetId(uint id)
        {
            Id = id;
        }

        public int GetId()
        {
            return unchecked((int)Id);
        }
    }

    public class StructuredNode : StateNode
    {
        public List<(string Name, StateNode Node)> Kids { get; } = new();
        public List<string>? ListedNames { get; set; }
        public List<(string Name, StructuredNode? Node)> Virtuals { get; } = new();

        public bool GetChildNames(List<string> names)
        {
            if (ListedNames != null)
            {
                names.AddRange(ListedNames);
                return true;
            }

            for (int i = 0; i < Kids.Count; i++)
            {
                names.Add(Kids[i].Name);
            }

            return true;
        }

        public bool GetChildNode(string name, out StateNode? node)
        {
            for (int i = 0; i < Kids.Count; i++)
            {
                if (CaseEquals(Kids[i].Name, name))
                {
                    node = Kids[i].Node;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public List<(string Name, StructuredNode? Node)> GetVirtualContainerChildren()
        {
            return Virtuals;
        }

        static bool CaseEquals(string left, string right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                char a = left[i];
                char b = right[i];
                if (a >= 'A' && a <= 'Z')
                {
                    a = (char)(a + 32);
                }

                if (b >= 'A' && b <= 'Z')
                {
                    b = (char)(b + 32);
                }

                if (a != b)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class StackNode : StructuredNode
    {
        public uint StackId { get; }

        public StackNode(uint stackId)
        {
            StackId = stackId;
        }
    }

    public enum VariableKind
    {
        Array,
        Value,
        State,
        Object,
        Struct,
    }

    public sealed class VariableNode : StateNode
    {
        public VariableKind Kind { get; }
        public string Name { get; }
        public VmValue Value { get; }
        public PType? Type { get; }
        public PClass? OwningClass { get; }
        public Frame? Frame { get; }

        public VariableNode(VariableKind kind, string name, VmValue value, PType? type, PClass? owningClass, Frame? frame)
        {
            Kind = kind;
            Name = name;
            Value = value;
            Type = type;
            OwningClass = owningClass;
            Frame = frame;
        }
    }

    public sealed class VmValue
    {
        public int I { get; }

        public VmValue(int i)
        {
            I = i;
        }
    }

    public sealed class PClass
    {
        public string Name { get; }

        public PClass(string name)
        {
            Name = name;
        }
    }

    public sealed class PType
    {
        public static readonly PType TypeState = new();

        public bool Pointer { get; init; }
        public bool ObjectPointer { get; init; }
        public bool Struct { get; init; }
        public bool Class { get; init; }
        public bool Array { get; init; }
        public bool DynArray { get; init; }
        public bool StaticArray { get; init; }
        public bool Container { get; init; }
        public bool Scalar { get; init; }
        public bool PointerFlag { get; init; }
        public PType? Pointed { get; init; }

        public bool IsPointer()
        {
            return Pointer;
        }

        public bool IsObjectPointer()
        {
            return ObjectPointer;
        }

        public bool IsStruct()
        {
            return Struct;
        }

        public bool IsClass()
        {
            return Class;
        }

        public bool IsArray()
        {
            return Array;
        }

        public bool IsDynArray()
        {
            return DynArray;
        }

        public bool IsStaticArray()
        {
            return StaticArray;
        }

        public bool IsContainer()
        {
            return Container;
        }

        public bool IsBasicNonPointer()
        {
            return Scalar && !PointerFlag;
        }
    }

    public sealed class Frame
    {
        public Frame? Parent { get; init; }
    }

    public sealed class FrameStack
    {
        public Frame? Top { get; init; }

        public bool HasFrames()
        {
            return Top != null;
        }

        public Frame? TopFrame()
        {
            return Top;
        }
    }
}
