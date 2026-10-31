namespace HCDE.Scripting;

/// <summary>
/// Argument placement, default fill, casts, and the empty-call check from
/// <c>FxVMFunctionCall::Resolve</c> in <c>codegen.cpp</c> 9936-10139.
/// The named-argument search above this band stays outside the slice.
/// <c>IndexAt</c> is the running index after that search. <c>if (failed)</c>
/// at line 10140 is not part of this band.
/// </summary>
public sealed class VmCallResolveTail1379Slice
{
    public const int VarOptional = 1 << 0;
    public const int VarOut = 1 << 11;
    public const int VarRef = 1 << 16;
    public const int VarVarArg = 1 << 19;

    public static readonly VmType NullPtr = new() { Name = "@null", IsPointer = true };
    public static readonly VmType Float64 = new() { Name = "float64" };
    public static readonly VmType Name = new() { Name = "name" };
    public static readonly VmType Sound = new() { Name = "sound" };
    public static readonly VmType String = new() { Name = "string" };

    public string FunctionName { get; set; } = "";
    public int Implicit { get; set; }
    public int Count { get; set; }
    public bool IsVarArg { get; set; }
    public int VariantFlags { get; set; }
    public bool Failed { get; set; }
    public bool HasStringArgs { get; set; }
    public List<VmArg> Args { get; } = [];
    public List<int> IndexAt { get; } = [];
    public List<VmType?> ArgTypes { get; } = [];
    public List<int> ArgFlags { get; } = [];
    public List<string> ArgNames { get; } = [];
    public List<long>? Defaults { get; set; }
    public Func<VmArg, VmArg?>? Resolve { get; set; }
    public Func<VmType, VmType>? NewPointer { get; set; }
    public Func<VmArg, bool>? RequestAddress { get; set; }
    public List<string> Messages { get; } = [];

    public sealed class VmType
    {
        public string Name { get; init; } = "";
        public int RegCount { get; init; } = 1;
        public bool IsPointer { get; init; }
        public bool IsRealPointer { get; init; }
        public bool IsStruct { get; init; }
        public bool IsDynArray { get; init; }
        public bool IsMap { get; init; }
        public bool IsMapIterator { get; init; }
        public VmType? Pointed { get; init; }
    }

    public sealed class VmArg
    {
        public string Kind { get; init; } = "expr";
        public bool Named { get; init; }
        public VmArg? Value { get; set; }
        public VmArg? Inner { get; init; }
        public VmType? Type { get; set; }
        public VmType? Target { get; init; }
        public bool Explicit { get; init; }
        public long Word { get; init; }
        public VmArg?[]? Parts { get; init; }
    }

    public sealed class Result
    {
        public bool Deleted { get; init; }
        public bool Failed { get; init; }
        public bool HasStringArgs { get; init; }
        public List<VmArg?>? Args { get; init; }
    }

    public Result ResolveTail()
    {
        if (Args.Count == 0)
            return ResolveEmpty();
        if (IndexAt.Count != Args.Count)
            throw new InvalidOperationException("index");
        var ordered = new List<VmArg?>(Count);
        for (var n = 0; n < Count; n++)
            ordered.Add(null);
        for (var i = 0; i < Args.Count; i++)
        {
            var at = IndexAt[i];
            var arg = Args[i];
            if (at >= Count)
            {
                if (!IsVarArg)
                {
                    Messages.Add("Too many arguments in call to " + FunctionName);
                    return new Result { Deleted = true };
                }

                ordered.Add(arg);
            }
            else if (arg.Named)
            {
                ordered[at] = arg.Value;
                arg.Value = null;
            }
            else
            {
                ordered[at] = arg;
            }
        }

        return ResolveOrdered(ordered);
    }

    Result ResolveEmpty()
    {
        if (Implicit < ArgTypes.Count && ArgTypes[Implicit] != null && (ArgFlags[Implicit] & VarOptional) == 0)
        {
            Messages.Add("Insufficient arguments in call to " + FunctionName);
            return new Result { Deleted = true };
        }

        return new Result { Failed = Failed, HasStringArgs = HasStringArgs, Args = [] };
    }

    Result ResolveOrdered(List<VmArg?> ordered)
    {
        var defaultsIndex = 0;
        for (var i = 0; i < Implicit; i++)
            defaultsIndex += (ArgTypes[i] ?? throw new InvalidOperationException("argtype")).RegCount;
        VmType? type = null;
        var flag = 0;
        var found = false;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (!found)
            {
                var slot = ArgTypes[i + Implicit];
                if (slot == null)
                    found = true;
                else
                {
                    type = slot;
                    flag = ArgFlags[i + Implicit];
                }
            }

            if (type == null)
                throw new InvalidOperationException("assert");
            if (!found)
            {
                if (ordered[i] == null)
                {
                    if ((flag & VarOptional) == 0)
                    {
                        Messages.Add("Required argument " + ArgNames[i + Implicit] + " has not been passed in call to " + FunctionName);
                        return new Result { Deleted = true };
                    }

                    var ntype = ArgTypes[i + Implicit] ?? throw new InvalidOperationException("argtype");
                    if ((ArgFlags[i + Implicit] & VarRef) != 0)
                    {
                        if (!ntype.IsPointer)
                            throw new InvalidOperationException("assert");
                        ntype = NullPtr;
                    }

                    if (ntype.RegCount == 1)
                        ordered[i] = new VmArg { Kind = "constant", Type = ntype, Word = Word(defaultsIndex) };
                    else
                    {
                        if (ntype.RegCount is < 0 or > 4)
                            throw new InvalidOperationException("regcount");
                        var parts = new VmArg?[4];
                        for (var l = 0; l < ntype.RegCount; l++)
                            parts[l] = new VmArg { Kind = "constant", Type = Float64, Word = Word(defaultsIndex + l) };
                        ordered[i] = new VmArg { Kind = "vector", Parts = parts };
                    }
                }

                defaultsIndex += (ArgTypes[i + Implicit] ?? throw new InvalidOperationException("argtype")).RegCount;
            }

            if (ordered[i] == null)
                throw new InvalidOperationException("assert");
            VmArg? resolved;
            if (found && (VariantFlags & VarVarArg) != 0)
            {
                resolved = MustResolve(ordered[i]!);
                if (resolved != null && (resolved.Type == Name || resolved.Type == Sound))
                    resolved = MustResolve(new VmArg { Kind = "stringcast", Inner = resolved });
            }
            else if ((flag & (VarRef | VarOut)) == 0)
            {
                resolved = MustResolve(new VmArg
                {
                    Kind = "cast",
                    Inner = ordered[i],
                    Target = type,
                    Explicit = false,
                });
            }
            else
            {
                ordered[i] = MustResolve(ordered[i]!);
                var current = ordered[i];
                if (current?.Type is { IsRealPointer: true } real)
                {
                    var pointed = real.Pointed;
                    if (pointed != null && (pointed.IsDynArray || pointed.IsMap || pointed.IsMapIterator))
                    {
                        current = MustResolve(new VmArg { Kind = "outderef", Inner = current });
                        ordered[i] = current;
                        if (current == null)
                            return new Result { Deleted = true };
                    }
                }

                if (current != null && current.Type != NullPtr)
                {
                    var preserve = false;
                    if (type == current.Type && type!.IsRealPointer)
                    {
                        if (type.Pointed == null)
                            throw new InvalidOperationException("pointed");
                        preserve = type.Pointed.IsStruct;
                    }

                    if (!preserve)
                    {
                        if (RequestAddress == null)
                            throw new InvalidOperationException("RequestAddress");
                        var writable = RequestAddress(current);
                        if ((flag & VarOut) != 0 && !writable)
                        {
                            Messages.Add("Argument must be a modifiable value");
                            return new Result { Deleted = true };
                        }

                        if ((flag & VarRef) != 0)
                        {
                            if (NewPointer == null)
                                throw new InvalidOperationException("NewPointer");
                            current.Type = NewPointer(current.Type ?? throw new InvalidOperationException("type"));
                        }
                    }

                    if (type != current.Type)
                    {
                        Messages.Add("Type mismatch in reference argument " + FunctionName);
                        resolved = null;
                    }
                    else
                    {
                        resolved = current;
                    }
                }
                else
                {
                    resolved = current;
                }
            }

            Failed |= resolved == null;
            ordered[i] = resolved;
            if (!Failed && resolved!.Type == String)
                HasStringArgs = true;
        }

        var numargs = ordered.Count + Implicit;
        if (numargs < ArgTypes.Count && ArgTypes[numargs] != null && (ArgFlags[numargs] & VarOptional) == 0)
        {
            Messages.Add("Insufficient arguments in call to " + FunctionName);
            return new Result { Deleted = true };
        }

        return new Result { Failed = Failed, HasStringArgs = HasStringArgs, Args = ordered };
    }

    VmArg? MustResolve(VmArg arg)
    {
        if (Resolve == null)
            throw new InvalidOperationException("Resolve");
        return Resolve(arg);
    }

    long Word(int index)
    {
        if (Defaults == null || (uint)index >= (uint)Defaults.Count)
            throw new InvalidOperationException("defaults");
        return Defaults[index];
    }
}
