namespace HCDE.Playsim;

/// <summary>
/// Debugger object node from <c>ObjectStateNode.cpp</c> 1-196, through end of file.
/// <c>SetVariableName</c>, <c>IsVMValValidDObject</c>, <c>GetVMValueVar</c>, and
/// <c>CreateNodeForVariable</c> stay callees. <c>IsVirtualStructure</c> is inline in the header.
/// </summary>
public sealed class VmObjectStateNode1384Slice
{
    public sealed class RecoverableError(string message) : Exception(message);

    public sealed class TypeNode
    {
        public string Name = "";
        public bool IsObjectPointer;
        public bool IsClass;
        public TypeNode? Pointed;
        public TypeNode? ParentType;
        public ClassDesc? Descriptor;
    }

    public sealed class ClassDesc
    {
        public ClassDesc? Parent;
        public List<Field> Fields = new();

        public bool IsDescendantOf(ClassDesc? other)
        {
            for (var walk = this; walk != null; walk = walk.Parent)
            {
                if (ReferenceEquals(walk, other))
                    return true;
            }

            return false;
        }
    }

    public sealed class Field
    {
        public string Name = "";
        public TypeNode? Type;
        public int BitValue;
    }

    public sealed class Obj
    {
        public ulong Address;
        public bool Valid = true;
        public TypeNode? VmType;
        public ClassDesc? Class;
    }

    public sealed class Variable
    {
        public string Name = "";
        public string? EvaluateName;
        public string Type = "";
        public string Value = "";
        public int VariablesReference;
        public int NamedVariables;
    }

    public sealed class Node
    {
        public string Name = "";
        public string ValueText = "";
        public TypeNode? Type;
        public ClassDesc? Owner;
    }

    public Func<Obj, Field, string> Read = (_, field) => field.Name;
    public Func<string, string, TypeNode?, ClassDesc?, Node> Create =
        (name, value, type, owner) => new Node { Name = name, ValueText = value, Type = type, Owner = owner };
    public readonly List<string> Logs = new();

    public ObjectNode New(string name, ulong address, bool valid, TypeNode type, Obj? obj, bool subView = false)
    {
        return new ObjectNode(this, name, address, valid, type, obj, subView);
    }

    public sealed class ObjectNode
    {
        readonly VmObjectStateNode1384Slice _slice;
        public string Name;
        public string EvalName;
        public int Id;
        public bool SubView;
        public ulong Address;
        public bool Valid;
        public TypeNode ClassType;
        public Obj? Object;
        public TypeNode? VmType;
        public readonly List<string> Cache = new();
        public readonly Dictionary<string, object> Children = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, object> VirtualChildren = new(StringComparer.OrdinalIgnoreCase);

        public ObjectNode(
            VmObjectStateNode1384Slice slice,
            string name,
            ulong address,
            bool valid,
            TypeNode type,
            Obj? obj,
            bool subView)
        {
            _slice = slice;
            Name = name;
            EvalName = name;
            Address = address;
            Valid = valid;
            ClassType = type;
            Object = obj;
            SubView = subView;
        }

        public bool Serialize(Variable variable)
        {
            variable.VariablesReference = IsValidObject() ? Id : 0;
            var pointed = ClassType.IsObjectPointer ? ClassType.Pointed! : ClassType;
            variable.Name = Name;
            if (EvalName != Name)
                variable.EvaluateName = EvalName;
            variable.Type = pointed.Name;
            var names = new List<string>();
            GetChildNames(names);
            variable.NamedVariables = names.Count;
            if (ClassType.IsObjectPointer)
            {
                if (Address == 0)
                    variable.Value = pointed.Name + " <NULL>";
                else if (VmType != null && !SubView && !ReferenceEquals(pointed, VmType))
                {
                    var vmClass = VmType.Descriptor;
                    var declared = pointed.Descriptor;
                    var vmName = VmType.Name;
                    if (vmClass != null && declared != null && !vmClass.IsDescendantOf(declared))
                        variable.Value = vmName + " (" + Pointer(Address) + ") as " + pointed.Name;
                    else
                        variable.Value = vmName + " (" + Pointer(Address) + ")";
                }
                else
                {
                    variable.Value = pointed.Name + " (" + Pointer(Address) + ")";
                }
            }
            else if (!SubView)
            {
                variable.Value = pointed.Name;
            }

            return true;
        }

        public bool GetChildNames(List<string> names)
        {
            if (Cache.Count > 0)
            {
                names.Clear();
                names.AddRange(Cache);
                return true;
            }

            var pType = ClassType.IsObjectPointer ? ClassType.Pointed! : ClassType;
            if (!pType.IsClass)
            {
                _slice.Logs.Add("Failed to get child names for object '" + Name + "' of type " + pType.Name);
                return false;
            }

            var obj = IsValidObject() ? Object : null;
            if (obj == null)
                return false;
            VmType = obj.VmType;
            var classType = pType;
            var descriptor = classType.Descriptor;
            if (!SubView && VmType != null && !ReferenceEquals(VmType, classType) && VmType.IsClass)
            {
                classType = VmType;
                descriptor = obj.Class;
            }

            var errorMsg = "";
            try
            {
                if (classType.ParentType != null && descriptor != null && descriptor.Parent != null)
                {
                    var parentName = classType.ParentType.Name;
                    var parent = new ObjectNode(_slice, parentName, Address, Valid, classType.ParentType, Object, true);
                    Children[parentName] = parent;
                    VirtualChildren[parentName] = parent;
                    Cache.Add(parentName);
                }

                foreach (var field in descriptor!.Fields)
                {
                    var name = field.Name;
                    try
                    {
                        var text = _slice.Read(obj, field);
                        Children[name] = _slice.Create(name, text, field.Type, descriptor);
                    }
                    catch (RecoverableError error)
                    {
                        errorMsg = error.Message;
                        Children.Clear();
                        Cache.Clear();
                        descriptor = obj.Class!;
                        foreach (var actual in descriptor.Fields)
                        {
                            name = actual.Name;
                            var text = _slice.Read(obj, actual);
                            Children[name] = _slice.Create(name, text, actual.Type, descriptor);
                            Cache.Add(name);
                        }

                        break;
                    }

                    Cache.Add(name);
                }
            }
            catch (RecoverableError error)
            {
                _slice.Logs.Add("Failed to get child names for object '" + Name + "' of type " + pType.Name);
                if (errorMsg.Length > 0)
                    _slice.Logs.Add("Error: " + errorMsg);
                _slice.Logs.Add("Error: " + error.Message);
                return false;
            }

            names.Clear();
            names.AddRange(Cache);
            return true;
        }

        public bool TryGetChild(string name, out object? node)
        {
            if (Children.Count == 0)
            {
                var names = new List<string>();
                GetChildNames(names);
            }

            return Children.TryGetValue(name, out node);
        }

        public void Reset()
        {
            Children.Clear();
        }

        public Dictionary<string, object> VirtualContainerChildren()
        {
            var copy = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in VirtualChildren)
                copy[pair.Key] = pair.Value;
            return copy;
        }

        bool IsValidObject()
        {
            return Address != 0 && Object != null && Object.Valid;
        }

        static string Pointer(ulong address)
        {
            return "0x" + address.ToString("x");
        }
    }
}
