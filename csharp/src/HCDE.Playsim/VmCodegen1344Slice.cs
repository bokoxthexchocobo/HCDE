namespace HCDE.Playsim;

/// <summary>
/// Member-call resolve tail, VM function-call construction, and the accessibility
/// check from <c>codegen.cpp</c> (9535-9836). Chunk 1344. Scope sides go through
/// <see cref="VmScopeBarrier1207Slice"/>. Name folding, version ordering, and
/// <c>delete this</c> ownership are the host surface those lines call.
/// </summary>
public sealed class VmCodegen1344Slice : Expr1344
{
    public const int VarfOptional = 1 << 0;
    public const int VarfMethod = 1 << 1;
    public const int VarfAction = 1 << 2;
    public const int VarfDeprecated = 1 << 7;
    public const int VarfVirtual = 1 << 8;
    public const int SufActor = 1;
    public const int EfxSelf = 1;
    public const int EfxStructMember = 2;

    public ContainerType1344? Cls;
    public bool StaticOnly;
    public bool NoVirtual;
    public bool FnPtr;
    public bool IsReadonly;
    public FunctionSymbol1344? AfdOverride;
    public int Method;
    public NameTable1344 Names = new();
    public List<Expr1344> ArgList = new();
    public Expr1344? Self;
    public MessageLog1344 Log = new();
    public Func<FindQuery1344, FindResult1344?>? Find;
    public FindQuery1344? LastFind;
    public int FindCalls;
    public Func<FunctionSymbol1344?, FunctionSymbol1344, bool>? Compatible;
    public int CompatibleCalls;
    public bool BarrierBuilt;
    public VmFunctionCall1344? Produced;

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        if (Self != null)
        {
            var owned = Self;
            Self = null;
            owned.Delete();
        }

        foreach (var arg in ArgList)
            arg.Delete();
        ArgList = new List<Expr1344>();
    }

    public Expr1344? Continue(CompileCtx1344 ctx)
    {
        if (Self == null)
            throw new NullReferenceException("self");
        if (Self.ValueType == null)
            throw new NullReferenceException("value type");

        var valueType = Self.ValueType;
        var tookPointer = false;
        if (valueType.IsRealPointer)
        {
            var pointer = valueType.ToPointer();
            if (pointer == null)
                throw new NullReferenceException("pointer");
            if (pointer.Pointed != null)
            {
                tookPointer = true;
                var pointed = pointer.Pointed;
                Cls = pointed.AsContainer();
                if (Cls != null)
                {
                    if (pointed.IsClass && Names.Same(Names.Chars(Method), "GetClass"))
                    {
                        if (ArgList.Count > 0)
                        {
                            Log.Add("error", $"Too many parameters in call to {Names.Chars(Method)}");
                            Delete();
                            return null;
                        }

                        var created = new GetClassExpr1344(Self);
                        return created.Resolve(ctx);
                    }
                }
                else
                {
                    Log.Add("error", $"Left hand side of {Names.Chars(Method)} must point to a class object");
                    Delete();
                    return null;
                }
            }
        }

        if (!tookPointer)
        {
            if (valueType.IsStruct && !IsReadonly)
            {
                var ok = Self.RequestAddress(ctx, out var writable);
                IsReadonly = !(ok && writable);
                if (valueType is not ContainerType1344 container)
                    throw new InvalidCastException("struct");
                Cls = container;
                Self.ValueType = TypeNode1344.PointerTo(valueType);
            }
            else if (valueType.IsFunctionPointer)
            {
                if (Names.Same(Names.Chars(Method), "Call") && valueType.Pointed != TypeNode1344.Void)
                {
                    FnPtr = true;
                    AfdOverride = valueType.FakeFunction;
                }
                else
                {
                    Log.Add("error", $"Unknown function {Names.Chars(Method)}");
                    Delete();
                    return null;
                }
            }
            else
            {
                Log.Add("error", $"Invalid expression on left hand side of {Names.Chars(Method)}");
                Delete();
                return null;
            }
        }

        return ResolveResolved(ctx);
    }

    public Expr1344? ResolveResolved(CompileCtx1344 ctx)
    {
        FunctionSymbol1344? afd;
        var failed = false;
        if (AfdOverride != null)
        {
            afd = AfdOverride;
        }
        else
        {
            if (Find == null)
                throw new InvalidOperationException("FindClassMemberFunction");
            var query = new FindQuery1344
            {
                Cls = Cls,
                ContextClass = ctx.Class,
                Method = Method,
                Version = ctx.Version,
                NotFromDecorate = !ctx.FromDecorate,
            };
            LastFind = query;
            FindCalls++;
            var found = Find(query);
            if (found == null)
                throw new InvalidOperationException("FindClassMemberFunction");
            failed = found.Error;
            afd = found.Function;
        }

        if (failed)
        {
            Delete();
            return null;
        }

        if (afd == null)
        {
            Log.Add("error", $"Unknown function {Names.Chars(Method)}");
            Delete();
            return null;
        }

        if (IsReadonly && (afd.Variant.Flags & VmScopeBarrier1207Slice.VarfReadOnly) == 0)
        {
            Log.Add("error", $"Readonly struct on left hand side of {Names.Chars(Method)} not allowed");
            Delete();
            return null;
        }

        var outerflags = 0;
        if (ctx.Function != null)
        {
            outerflags = ctx.Function.Variant.Flags;
            if ((outerflags & (VmScopeBarrier1207Slice.VarfVirtualScope | VarfVirtual)) == (VmScopeBarrier1207Slice.VarfVirtualScope | VarfVirtual) && ctx.Class != null)
                outerflags = VmScopeBarrier1207Slice.FlagsFromSide(VmScopeBarrier1207Slice.SideFromObjectFlags(ctx.Class.ScopeFlags));
        }

        var innerflags = afd.Variant.Flags;
        var innerside = VmScopeBarrier1207Slice.SideFromFlags(innerflags);
        if (innerside == VmScopeBarrier1207Slice.SideVirtual)
        {
            if (Cls == null)
                throw new NullReferenceException("class");
            innerside = VmScopeBarrier1207Slice.SideFromObjectFlags(Cls.ScopeFlags);
            innerflags = VmScopeBarrier1207Slice.FlagsFromSide(innerside);
        }
        else if (innerside != VmScopeBarrier1207Slice.SideClear)
        {
            if (Self == null)
                throw new NullReferenceException("self");
            if (Self.ExprType == EfxStructMember && innerside == VmScopeBarrier1207Slice.SidePlain)
                innerflags = VmScopeBarrier1207Slice.ChangeSideInFlags(innerflags, Self.BarrierSide);
        }

        var barrier = new VmScopeBarrier1207Slice(outerflags, innerflags, Names.Chars(Method));
        BarrierBuilt = true;
        if (!barrier.Callable)
        {
            Log.Add("error", barrier.CallError);
            Delete();
            return null;
        }

        if (StaticOnly && (afd.Variant.Flags & VarfMethod) != 0)
        {
            if (!NoVirtual || (afd.Variant.Flags & VarfVirtual) == 0)
            {
                var contextClass = ctx.Class?.ToClass();
                var targetClass = Cls?.ToClass();
                if (contextClass == null || targetClass == null)
                {
                    if (Cls == null)
                        throw new NullReferenceException("class");
                    Log.Add("error", $"Cannot call non-static function {Cls.TypeName}::{Names.Chars(Method)} from here");
                    Delete();
                    return null;
                }

                if (contextClass.Descriptor == null || targetClass.Descriptor == null)
                    throw new NullReferenceException("descriptor");
                if (!contextClass.Descriptor.IsDescendantOf(targetClass.Descriptor))
                {
                    Log.Add("error", $"Cannot call non-static function {Cls!.TypeName}::{Names.Chars(Method)} from here");
                    Delete();
                    return null;
                }

                Log.Add("error", $"Qualified member call to parent class {Cls!.TypeName}::{Names.Chars(Method)} is not yet implemented");
                Delete();
                return null;
            }
        }

        if ((afd.Variant.Flags & VarfMethod) != 0)
        {
            if (ctx.Function == null)
            {
                Log.Add("error", $"Unable to call {Names.Chars(Method)} from constant declaration");
                Delete();
                return null;
            }

            if (Self == null)
                throw new NullReferenceException("self");
            if (Self.ExprType == EfxSelf)
            {
                if (Compatible == null)
                    throw new InvalidOperationException("CheckFunctionCompatiblity");
                CompatibleCalls++;
                if (!Compatible(ctx.Function, afd))
                {
                    Delete();
                    return null;
                }
            }
            else if ((afd.Variant.UseFlags & SufActor) == 0)
            {
                Log.Add("error", $"Function {afd.SymbolName} cannot be used with a non-self object");
                Delete();
                return null;
            }
        }

        var calleeSelf = (FnPtr || (afd.Variant.Flags & VarfMethod) != 0) ? Self : null;
        var noVirtual = (StaticOnly || NoVirtual) && !FnPtr;
        var moved = ArgList;
        ArgList = new List<Expr1344>();
        var call = new VmFunctionCall1344(calleeSelf, afd, moved, noVirtual);
        call.Log = Log;
        Produced = call;
        if (ReferenceEquals(Self, calleeSelf))
            Self = null;
        Delete();
        return call.Resolve(ctx);
    }
}

public sealed class ScriptVersion1344
{
    public ushort Major;
    public ushort Minor;
    public uint Revision;

    public static ScriptVersion1344 Make(int major, int minor, uint revision = 0)
    {
        var version = new ScriptVersion1344();
        version.Major = (ushort)major;
        version.Minor = (ushort)minor;
        version.Revision = revision;
        return version;
    }

    public bool LessOrEqual(ScriptVersion1344 other)
    {
        return other.Major > Major || (other.Major == Major && other.Minor > Minor) || (other.Major == Major && other.Minor == Minor && other.Revision >= Revision);
    }

    public bool GreaterOrEqual(ScriptVersion1344 other)
    {
        return other.Major < Major || (other.Major == Major && other.Minor < Minor) || (other.Major == Major && other.Minor == Minor && other.Revision <= Revision);
    }

    public bool Greater(ScriptVersion1344 other)
    {
        return other.Major < Major || (other.Major == Major && other.Minor < Minor) || (other.Major == Major && other.Minor == Minor && other.Revision < Revision);
    }

    public bool Less(ScriptVersion1344 other)
    {
        return other.Major > Major || (other.Major == Major && other.Minor > Minor) || (other.Major == Major && other.Minor == Minor && other.Revision > Revision);
    }
}

public sealed class ScriptNote1344
{
    public string Level = "";
    public string Text = "";
}

public sealed class MessageLog1344
{
    public List<ScriptNote1344> Notes = new();

    public void Add(string level, string text)
    {
        Notes.Add(new ScriptNote1344 { Level = level, Text = text });
    }
}

public sealed class NameTable1344
{
    public List<string> Spellings = new() { "None", "GetClass", "Call" };

    public int Intern(string? text)
    {
        if (text == null)
            return 0;
        for (var i = 0; i < Spellings.Count; i++)
        {
            if (Same(Spellings[i], text))
                return i;
        }

        Spellings.Add(text);
        return Spellings.Count - 1;
    }

    public string Chars(int index)
    {
        return Spellings[index];
    }

    public bool Same(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (var i = 0; i < left.Length; i++)
        {
            var a = left[i];
            var b = right[i];
            if (a >= 'A' && a <= 'Z')
                a = (char)(a + 32);
            if (b >= 'A' && b <= 'Z')
                b = (char)(b + 32);
            if (a != b)
                return false;
        }

        return true;
    }
}

public sealed class ClassDesc1344
{
    public ClassDesc1344? Parent;

    public bool IsDescendantOf(ClassDesc1344 ancestor)
    {
        for (ClassDesc1344? cursor = this; cursor != null; cursor = cursor.Parent)
        {
            if (ReferenceEquals(cursor, ancestor))
                return true;
        }

        return false;
    }
}

public class TypeNode1344
{
    public static readonly TypeNode1344 Void = new() { IsVoid = true };

    public bool IsRealPointer;
    public bool IsClass;
    public bool IsStruct;
    public bool IsFunctionPointer;
    public bool IsVoid;
    public TypeNode1344? Pointer;
    public TypeNode1344? Pointed;
    public FunctionSymbol1344? FakeFunction;

    public static TypeNode1344 PointerTo(TypeNode1344 target)
    {
        var pointer = new TypeNode1344();
        pointer.IsRealPointer = true;
        pointer.Pointer = pointer;
        pointer.Pointed = target;
        return pointer;
    }

    public TypeNode1344? ToPointer()
    {
        if (!IsRealPointer)
            return null;
        return Pointer;
    }

    public ContainerType1344? AsContainer()
    {
        return this as ContainerType1344;
    }
}

public sealed class ContainerType1344 : TypeNode1344
{
    public int ScopeFlags;
    public string TypeName = "";
    public ClassDesc1344? Descriptor;
    public int DefFileNo;

    public ContainerType1344? ToClass()
    {
        if (!IsClass)
            return null;
        return this;
    }
}

public sealed class Proto1344
{
    public List<TypeNode1344> ArgumentTypes = new();
    public List<TypeNode1344> ReturnTypes = new();
}

public sealed class VmRoutine1344
{
    public List<object?> DefaultArgs = new();
    public bool BlockJit;
}

public sealed class FunctionVariant1344
{
    public int Flags;
    public int UseFlags;
    public string DeprecationMessage = "";
    public Proto1344? Proto;
    public List<int> ArgFlags = new();
    public List<string> ArgNames = new();
    public VmRoutine1344? Implementation;
}

public sealed class FunctionSymbol1344
{
    public string SymbolName = "";
    public ScriptVersion1344 Version = ScriptVersion1344.Make(0, 0);
    public ContainerType1344? OwningClass;
    public FunctionVariant1344 Variant = new();

    public int GetImplicitArgs()
    {
        if ((Variant.Flags & VmCodegen1344Slice.VarfAction) != 0)
            return 3;
        if ((Variant.Flags & VmCodegen1344Slice.VarfMethod) != 0)
            return 1;
        return 0;
    }
}

public sealed class FindQuery1344
{
    public ContainerType1344? Cls;
    public ContainerType1344? ContextClass;
    public int Method;
    public ScriptVersion1344 Version = ScriptVersion1344.Make(0, 0);
    public bool NotFromDecorate;
}

public sealed class FindResult1344
{
    public FunctionSymbol1344? Function;
    public bool Error;
}

public sealed class CompileCtx1344
{
    public FunctionSymbol1344? Function;
    public ContainerType1344? Class;
    public bool FromDecorate;
    public ScriptVersion1344 Version = ScriptVersion1344.Make(0, 0);
}

public class Expr1344
{
    public int ExprType;
    public TypeNode1344? ValueType;
    public bool Resolved;
    public bool Deleted;
    public int BarrierSide;
    public Func<CompileCtx1344, Expr1344?>? Resolver;
    public Func<CompileCtx1344, (bool Ok, bool Writable)>? AddressRequest;
    public int ResolveCount;
    public int AddressCount;

    public virtual Expr1344? Resolve(CompileCtx1344 ctx)
    {
        ResolveCount++;
        if (Resolver != null)
            return Resolver(ctx);
        Resolved = true;
        return this;
    }

    public bool RequestAddress(CompileCtx1344 ctx, out bool writable)
    {
        AddressCount++;
        if (AddressRequest != null)
        {
            var result = AddressRequest(ctx);
            writable = result.Writable;
            return result.Ok;
        }

        writable = false;
        return false;
    }

    public virtual Proto1344 ReturnProto()
    {
        if (ValueType == null)
            throw new NullReferenceException("value type");
        var proto = new Proto1344();
        if (!ValueType.IsVoid)
            proto.ReturnTypes.Add(ValueType);
        return proto;
    }

    public virtual void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
    }
}

public sealed class GetClassExpr1344 : Expr1344
{
    public Expr1344? Self;

    public GetClassExpr1344(Expr1344? self)
    {
        Self = self;
    }
}

public sealed class VmFunctionCall1344 : Expr1344
{
    public Expr1344? Self;
    public FunctionSymbol1344 Function;
    public List<Expr1344> ArgList;
    public bool NoVirtual;
    public bool FnPtrCall;
    public FunctionSymbol1344? CallingFunction;
    public bool HasStringArgs;
    public MessageLog1344 Log = new();
    public int AccessibilityCalls;
    public Func<FunctionSymbol1344?, FunctionSymbol1344, bool>? DirectCompatible;
    public int DirectCompatibleCalls;
    public List<TypeNode1344>? CapturedArgTypes;
    public List<string>? CapturedArgNames;
    public List<int>? CapturedArgFlags;
    public List<object?>? Defaults;
    public int Implicit;
    public bool RelaxedNamedArguments;
    public bool Continued;

    public VmFunctionCall1344(Expr1344? self, FunctionSymbol1344 function, List<Expr1344> args, bool novirtual)
    {
        Self = self;
        Function = function;
        ArgList = args;
        NoVirtual = novirtual;
        CallingFunction = null;
        FnPtrCall = self != null && self.ValueType != null && self.ValueType.IsFunctionPointer;
    }

    public override void Delete()
    {
        if (Deleted)
            return;
        Deleted = true;
        foreach (var arg in ArgList)
            arg.Delete();
        ArgList = new List<Expr1344>();
    }

    public override Proto1344 ReturnProto()
    {
        if (HasStringArgs)
            return base.ReturnProto();
        if (Function.Variant.Proto == null)
            throw new NullReferenceException("proto");
        return Function.Variant.Proto;
    }

    public bool CheckAccessibility(ScriptVersion1344 version)
    {
        AccessibilityCalls++;
        var flags = Function.Variant.Flags;
        if (Function.Version.Greater(version) && (flags & VmCodegen1344Slice.VarfDeprecated) == 0)
        {
            string versionString;
            if (version.GreaterOrEqual(ScriptVersion1344.Make(2, 3)))
                versionString = $"ZScript version {version.Major}.{version.Minor}.{version.Revision}";
            else
                versionString = "DECORATE";
            Log.Add("error", $"{Function.SymbolName} not accessible to {versionString}");
            return false;
        }

        if ((flags & VmCodegen1344Slice.VarfDeprecated) != 0 && Function.Version.LessOrEqual(version))
        {
            if (Function.OwningClass == null)
                throw new NullReferenceException("owning class");
            if (FileContainer == null)
                throw new InvalidOperationException("GetFileContainer");
            var internalFile = FileContainer(Function.OwningClass.DefFileNo) == 0;
            var note = Function.Variant.DeprecationMessage ?? "";
            var gap = note.Length == 0 ? "" : ", ";
            var text = $"Accessing deprecated function {Function.SymbolName} - deprecated since {Function.Version.Major}.{Function.Version.Minor}.{Function.Version.Revision}{gap}{note}";
            Log.Add(internalFile ? "debug" : "warning", text);
        }

        return true;
    }

    public Func<int, int>? FileContainer;

    public VmRoutine1344? GetDirectFunction(FunctionSymbol1344? calling, ScriptVersion1344 version)
    {
        var flags = Function.Variant.Flags;
        if (ArgList.Count == 0 && (flags & VmCodegen1344Slice.VarfVirtual) == 0 && !FnPtrCall && CheckAccessibility(version) && InvokeCompatible(calling))
        {
            var imp = Function.GetImplicitArgs();
            var argFlags = Function.Variant.ArgFlags;
            if (argFlags.Count > imp && (argFlags[imp] & VmCodegen1344Slice.VarfOptional) == 0)
                return null;
            return Function.Variant.Implementation;
        }

        return null;
    }

    public override Expr1344? Resolve(CompileCtx1344 ctx)
    {
        if (Resolved)
            return this;
        Resolved = true;
        if (Self != null)
        {
            var resolved = Self.Resolve(ctx);
            Self = resolved;
            if (resolved == null)
            {
                Delete();
                return null;
            }
        }

        var proto = Function.Variant.Proto;
        if (proto == null)
            throw new NullReferenceException("proto");
        CapturedArgTypes = proto.ArgumentTypes;
        CapturedArgNames = Function.Variant.ArgNames;
        CapturedArgFlags = Function.Variant.ArgFlags;
        if (FnPtrCall)
        {
            Defaults = null;
        }
        else
        {
            if (Function.Variant.Implementation == null)
                throw new NullReferenceException("implementation");
            Defaults = Function.Variant.Implementation.DefaultArgs;
        }

        if (FnPtrCall)
        {
            if (ctx.Function == null || ctx.Function.Variant.Implementation == null)
                throw new NullReferenceException("caller");
            ctx.Function.Variant.Implementation.BlockJit = true;
        }

        Implicit = Function.GetImplicitArgs();
        RelaxedNamedArguments = ctx.Version.GreaterOrEqual(ScriptVersion1344.Make(4, 13));
        if (!CheckAccessibility(ctx.Version))
        {
            Delete();
            return null;
        }

        Continued = true;
        return this;
    }

    bool InvokeCompatible(FunctionSymbol1344? calling)
    {
        if (DirectCompatible == null)
            throw new InvalidOperationException("CheckFunctionCompatiblity");
        DirectCompatibleCalls++;
        return DirectCompatible(calling, Function);
    }
}
