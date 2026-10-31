namespace HCDE.Scripting.Tests;

public class VmCallResolveTail1379SliceTests
{
    [Fact]
    public void PlacementFillsDefaultsAndRejectsExtraArgs()
    {
        var intType = new VmCallResolveTail1379Slice.VmType { Name = "int" };
        var vec = new VmCallResolveTail1379Slice.VmType { Name = "vec", RegCount = 2 };
        var self = new VmCallResolveTail1379Slice.VmType { Name = "self", RegCount = 2 };
        var ptr = new VmCallResolveTail1379Slice.VmType { Name = "int*", IsPointer = true, RegCount = 2 };
        var positional = new VmCallResolveTail1379Slice.VmArg { Kind = "expr", Type = intType };
        var slice = new VmCallResolveTail1379Slice
        {
            FunctionName = "Foo",
            Implicit = 1,
            Count = 3,
            IsVarArg = true,
            VariantFlags = VmCallResolveTail1379Slice.VarVarArg,
            Defaults = [9, 8, 7, 6, 5, 4],
            Resolve = arg => arg.Kind == "cast"
                ? new VmCallResolveTail1379Slice.VmArg
                {
                    Kind = "resolved",
                    Type = arg.Target,
                    Inner = arg.Inner,
                    Explicit = arg.Explicit,
                }
                : arg,
        };
        slice.ArgTypes.AddRange([self, intType, vec, ptr, null]);
        slice.ArgFlags.AddRange([
            0,
            VmCallResolveTail1379Slice.VarOptional,
            VmCallResolveTail1379Slice.VarOptional,
            VmCallResolveTail1379Slice.VarOptional | VmCallResolveTail1379Slice.VarRef,
            0,
        ]);
        slice.ArgNames.AddRange(["self", "a", "b", "c", ""]);
        slice.Args.Add(positional);
        slice.IndexAt.Add(0);

        var placed = slice.ResolveTail();
        Assert.False(placed.Deleted);
        Assert.False(placed.Failed);
        Assert.Equal("resolved", placed.Args![0]!.Kind);
        Assert.Same(positional, placed.Args[0]!.Inner);
        Assert.False(placed.Args[0]!.Explicit);
        Assert.Equal(intType, placed.Args[0]!.Type);
        Assert.Equal("resolved", placed.Args[1]!.Kind);
        Assert.Equal(vec, placed.Args[1]!.Type);
        var vector = placed.Args[1]!.Inner;
        Assert.Equal("vector", vector!.Kind);
        Assert.Equal(6L, vector.Parts![0]!.Word);
        Assert.Equal(5L, vector.Parts![1]!.Word);
        Assert.Null(vector.Parts![2]);
        Assert.Equal(VmCallResolveTail1379Slice.Float64, vector.Parts![0]!.Type);
        Assert.Equal("constant", placed.Args[2]!.Kind);
        Assert.Same(VmCallResolveTail1379Slice.NullPtr, placed.Args[2]!.Type);
        Assert.Equal(4L, placed.Args[2]!.Word);

        var namedValue = new VmCallResolveTail1379Slice.VmArg { Kind = "expr", Type = intType };
        var named = new VmCallResolveTail1379Slice.VmArg { Named = true, Value = namedValue };
        var extra = new VmCallResolveTail1379Slice.VmArg { Kind = "extra" };
        slice.Args.Clear();
        slice.IndexAt.Clear();
        slice.Args.Add(named);
        slice.Args.Add(extra);
        slice.IndexAt.AddRange([0, 3]);
        var pushed = slice.ResolveTail();
        Assert.False(pushed.Deleted);
        Assert.Same(namedValue, pushed.Args![0]!.Inner);
        Assert.Null(named.Value);
        Assert.Same(extra, pushed.Args[3]);

        var kept = new VmCallResolveTail1379Slice.VmArg { Kind = "kept" };
        var blocked = new VmCallResolveTail1379Slice.VmArg { Named = true, Value = kept };
        slice.IsVarArg = false;
        slice.VariantFlags = 0;
        slice.Count = 1;
        slice.Implicit = 0;
        slice.Args.Clear();
        slice.IndexAt.Clear();
        slice.Args.Add(blocked);
        slice.IndexAt.Add(1);
        slice.ArgTypes.Clear();
        slice.ArgFlags.Clear();
        slice.ArgNames.Clear();
        slice.ArgTypes.Add(intType);
        slice.ArgFlags.Add(0);
        slice.ArgNames.Add("x");
        var tooMany = slice.ResolveTail();
        Assert.True(tooMany.Deleted);
        Assert.Null(tooMany.Args);
        Assert.Equal("Too many arguments in call to Foo", Assert.Single(slice.Messages));
        Assert.Same(kept, blocked.Value);

        slice.Messages.Clear();
        slice.Count = 2;
        slice.IsVarArg = false;
        slice.Args.Clear();
        slice.IndexAt.Clear();
        slice.ArgTypes.Clear();
        slice.ArgFlags.Clear();
        slice.ArgNames.Clear();
        slice.ArgTypes.AddRange([intType, intType]);
        slice.ArgFlags.AddRange([VmCallResolveTail1379Slice.VarOptional, 0]);
        slice.ArgNames.AddRange(["a", "y"]);
        slice.Args.Add(positional);
        slice.IndexAt.Add(0);
        slice.Resolve = arg => arg.Kind == "cast"
            ? new VmCallResolveTail1379Slice.VmArg { Kind = "resolved", Type = arg.Target, Inner = arg.Inner }
            : arg;
        var missing = slice.ResolveTail();
        Assert.True(missing.Deleted);
        Assert.Equal("Required argument y has not been passed in call to Foo", Assert.Single(slice.Messages));
    }

    [Fact]
    public void RefsVarargsAndInsufficientCalls()
    {
        var intType = new VmCallResolveTail1379Slice.VmType { Name = "int" };
        var slice = new VmCallResolveTail1379Slice { FunctionName = "Foo" };
        slice.ArgTypes.Add(intType);
        slice.ArgFlags.Add(0);
        var empty = slice.ResolveTail();
        Assert.True(empty.Deleted);
        Assert.Equal("Insufficient arguments in call to Foo", Assert.Single(slice.Messages));

        slice.Messages.Clear();
        slice.ArgFlags[0] = VmCallResolveTail1379Slice.VarOptional;
        var optionalEmpty = slice.ResolveTail();
        Assert.False(optionalEmpty.Deleted);
        Assert.Empty(optionalEmpty.Args!);

        slice = new VmCallResolveTail1379Slice { FunctionName = "Foo", Implicit = 1 };
        slice.ArgTypes.Add(new VmCallResolveTail1379Slice.VmType { Name = "self" });
        slice.ArgFlags.Add(0);
        var onlyImplicit = slice.ResolveTail();
        Assert.False(onlyImplicit.Deleted);
        Assert.Empty(slice.Messages);

        var calls = new List<string>();
        slice = new VmCallResolveTail1379Slice
        {
            FunctionName = "Foo",
            Count = 1,
            IsVarArg = true,
            VariantFlags = VmCallResolveTail1379Slice.VarVarArg,
            Resolve = arg =>
            {
                calls.Add(arg.Kind);
                if (arg.Kind == "sound-expr")
                    return new VmCallResolveTail1379Slice.VmArg { Kind = "sound", Type = VmCallResolveTail1379Slice.Sound };
                if (arg.Kind == "stringcast")
                    return new VmCallResolveTail1379Slice.VmArg { Kind = "string", Type = VmCallResolveTail1379Slice.String };
                if (arg.Kind == "cast")
                    return new VmCallResolveTail1379Slice.VmArg { Kind = "resolved", Type = arg.Target };
                return arg;
            },
        };
        slice.ArgTypes.AddRange([intType, null]);
        slice.ArgFlags.AddRange([0, 0]);
        slice.Args.Add(new VmCallResolveTail1379Slice.VmArg { Kind = "int-expr" });
        slice.Args.Add(new VmCallResolveTail1379Slice.VmArg { Kind = "sound-expr" });
        slice.IndexAt.AddRange([0, 1]);
        var varargs = slice.ResolveTail();
        Assert.False(varargs.Deleted);
        Assert.False(varargs.Failed);
        Assert.True(varargs.HasStringArgs);
        Assert.Equal(new[] { "cast", "sound-expr", "stringcast" }, calls);
        Assert.Equal("string", varargs.Args![1]!.Kind);

        var body = new VmCallResolveTail1379Slice.VmType { Name = "actor", IsStruct = true };
        var structPtr = new VmCallResolveTail1379Slice.VmType
        {
            Name = "actor*",
            IsRealPointer = true,
            IsPointer = true,
            Pointed = body,
        };
        var mapBody = new VmCallResolveTail1379Slice.VmType { Name = "map", IsMap = true };
        var mapPtr = new VmCallResolveTail1379Slice.VmType
        {
            Name = "map*",
            IsRealPointer = true,
            Pointed = mapBody,
        };
        calls.Clear();
        var structArg = new VmCallResolveTail1379Slice.VmArg { Kind = "struct", Type = structPtr };
        var mapArg = new VmCallResolveTail1379Slice.VmArg { Kind = "map", Type = mapPtr };
        slice = new VmCallResolveTail1379Slice
        {
            FunctionName = "Foo",
            Count = 2,
            Resolve = arg =>
            {
                calls.Add(arg.Kind);
                return arg.Kind == "outderef" ? null : arg;
            },
        };
        slice.ArgTypes.AddRange([structPtr, mapPtr]);
        slice.ArgFlags.AddRange([VmCallResolveTail1379Slice.VarRef, VmCallResolveTail1379Slice.VarOut]);
        slice.Args.Add(structArg);
        slice.Args.Add(mapArg);
        slice.IndexAt.AddRange([0, 1]);
        var aborted = slice.ResolveTail();
        Assert.True(aborted.Deleted);
        Assert.Empty(slice.Messages);
        Assert.Equal(new[] { "struct", "map", "outderef" }, calls);

        calls.Clear();
        var text = new VmCallResolveTail1379Slice.VmArg { Kind = "text", Type = VmCallResolveTail1379Slice.String };
        var mismatched = new VmCallResolveTail1379Slice.VmArg { Kind = "num", Type = intType };
        slice = new VmCallResolveTail1379Slice
        {
            FunctionName = "Foo",
            Count = 2,
            Resolve = arg => arg.Kind == "cast"
                ? new VmCallResolveTail1379Slice.VmArg { Kind = "resolved", Type = arg.Target }
                : arg,
            RequestAddress = arg =>
            {
                calls.Add(arg.Kind);
                return true;
            },
            NewPointer = type => new VmCallResolveTail1379Slice.VmType { Name = "ptr:" + type.Name },
        };
        slice.ArgTypes.AddRange([VmCallResolveTail1379Slice.String, intType]);
        slice.ArgFlags.AddRange([0, VmCallResolveTail1379Slice.VarRef]);
        slice.Args.Add(text);
        slice.Args.Add(mismatched);
        slice.IndexAt.AddRange([0, 1]);
        var mixed = slice.ResolveTail();
        Assert.False(mixed.Deleted);
        Assert.True(mixed.Failed);
        Assert.True(mixed.HasStringArgs);
        Assert.Equal("string", mixed.Args![0]!.Type!.Name);
        Assert.Null(mixed.Args[1]);
        Assert.Equal(["num"], calls);
        Assert.Equal("Type mismatch in reference argument Foo", Assert.Single(slice.Messages));

        var kept = new VmCallResolveTail1379Slice.VmArg
        {
            Kind = "nullp",
            Type = VmCallResolveTail1379Slice.NullPtr,
        };
        slice = new VmCallResolveTail1379Slice
        {
            FunctionName = "Foo",
            Count = 1,
            Resolve = arg => arg,
        };
        slice.ArgTypes.Add(structPtr);
        slice.ArgFlags.Add(VmCallResolveTail1379Slice.VarRef);
        slice.Args.Add(structArg);
        slice.IndexAt.Add(0);
        var preserved = slice.ResolveTail();
        Assert.False(preserved.Failed);
        Assert.Same(structArg, preserved.Args![0]);

        slice.Args[0] = kept;
        var nullRef = slice.ResolveTail();
        Assert.False(nullRef.Failed);
        Assert.Same(kept, nullRef.Args![0]);

        slice.Args[0] = new VmCallResolveTail1379Slice.VmArg { Kind = "frozen", Type = intType };
        slice.ArgFlags[0] = VmCallResolveTail1379Slice.VarOut;
        slice.RequestAddress = _ => false;
        var notWritable = slice.ResolveTail();
        Assert.True(notWritable.Deleted);
        Assert.Equal("Argument must be a modifiable value", Assert.Single(slice.Messages));
    }
}
