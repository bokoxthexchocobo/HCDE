using S = HCDE.Playsim.VmLocalScope1398Slice;

namespace HCDE.Playsim.Tests;

public class VmLocalScope1398SliceTests
{
    [Fact]
    public void QualifiedNamesSerializeAndSkipFutureLines()
    {
        Assert.Equal("x @ line 12", S.GetLineQualifiedName("x", 12));
        Assert.Equal("x @ line -3", S.GetLineQualifiedName("x", -3));
        Assert.Equal(-1, S.GetLineFromLineQualifiedName("foo"));
        Assert.Equal(12, S.GetLineFromLineQualifiedName("foo @ line 12"));
        Assert.Equal(12, S.GetLineFromLineQualifiedName("foo @ line 12abc"));
        Assert.Equal(-3, S.GetLineFromLineQualifiedName("foo @ line -3"));
        Assert.Equal(7, S.GetLineFromLineQualifiedName("foo @ line   7"));
        Assert.Equal(8, S.GetLineFromLineQualifiedName("foo @ line +8"));
        Assert.Equal(9, S.GetLineFromLineQualifiedName("foo @ line \t9"));
        Assert.Equal(1, S.GetLineFromLineQualifiedName("a @ line 1 @ line 2"));
        Assert.Equal(0, S.GetLineFromLineQualifiedName(" @ line 0"));
        Assert.Equal(int.MaxValue, S.GetLineFromLineQualifiedName("n @ line 2147483647"));
        Assert.Equal(int.MinValue, S.GetLineFromLineQualifiedName("n @ line -2147483648"));
        Assert.Equal(0, S.GetLineFromLineQualifiedName("n @ line 0x10"));
        Assert.Equal(10, S.GetLineFromLineQualifiedName("n @ line 010"));
        Assert.Equal(3, S.GetLineFromLineQualifiedName("n @ line \n3"));
        Assert.Equal(4, S.GetLineFromLineQualifiedName("n @ line 4 "));
        Assert.Throws<FormatException>(() => S.GetLineFromLineQualifiedName("foo @ line "));
        Assert.Throws<FormatException>(() => S.GetLineFromLineQualifiedName("n @ line +"));
        Assert.Throws<OverflowException>(() => S.GetLineFromLineQualifiedName("n @ line 2147483648"));
        Assert.Throws<OverflowException>(() => S.GetLineFromLineQualifiedName("n @ line -2147483649"));

        var locals = new List<S.Local>
        {
            new() { Name = "a", Line = 9 },
            new() { Name = "a", Line = 4 },
            new() { Name = "hidden", Line = 10 },
            new() { Name = "later", Line = 11 },
            new() { Name = "X", Line = 1 },
            new() { Name = "x", Line = 2 },
            new() { Name = "a", Line = 3 },
        };
        int loads = 0;
        IReadOnlyList<S.Local> Load(S.Frame _)
        {
            loads++;
            return locals;
        }

        var scope = S.Create(new S.Frame { IsScriptFunction = true, CurrentLine = 10, Id = 44 });
        var names = new List<string>();
        Assert.True(S.GetChildNames(scope, names, Load));
        Assert.Equal(1, loads);
        Assert.Equal(new[] { "a @ line 9", "a @ line 4", "X", "x", "a" }, names);

        var again = new List<string>();
        Assert.True(S.GetChildNames(scope, again, Load));
        Assert.Equal(1, loads);
        Assert.Equal(names, again);

        var proto = S.SerializeToProtocol(scope, new List<string>(), Load);
        Assert.Equal(S.LocalName, proto.Name);
        Assert.False(proto.Expensive);
        Assert.Equal(44, proto.VariablesReference);
        Assert.Equal(S.PresentationHint, proto.Hint);
        Assert.Equal(0, proto.IndexedVariables);
        Assert.Equal(5, proto.NamedVariables);
        Assert.Throws<ArgumentOutOfRangeException>(() => S.GetChildNames(scope, new List<string> { "a" }, Load));

        var rawNames = new List<string>();
        Assert.True(S.GetChildNames(S.Create(new S.Frame { IsScriptFunction = false, CurrentLine = 10 }), rawNames, Load));
        Assert.Equal(new[] { "a @ line 9", "a @ line 4", "hidden", "later", "X", "x", "a" }, rawNames);
        Assert.Throws<ArgumentNullException>(() => S.Create(null!));
    }

    [Fact]
    public void CacheRenamesOnCaseFoldAndResolvesInvoker()
    {
        var locals = new List<S.Local>
        {
            new() { Name = "self", Line = 1, Type = "SelfType" },
            new() { Name = "invoker", Line = 1, Type = "InvType" },
            new() { Name = "X", Line = 1 },
            new() { Name = "x", Line = 2 },
            new() { Name = "n", Line = 3 },
            new() { Name = "n", Line = 4 },
            new() { Name = "n", Line = 5 },
            new() { Name = "future", Line = 50 },
        };
        var seenTypes = new List<object?>();
        int creates = 0;
        object? Descriptor(object? type)
        {
            seenTypes.Add(type);
            return "Desc";
        }
        S.Child Make(string name, object? value, object? type, S.Frame frame, object? invoker)
        {
            creates++;
            return new S.Child
            {
                Name = name,
                CanRename = value is not false,
                Value = value,
                Type = type,
                Invoker = invoker,
                Frame = frame,
            };
        }

        var frame = new S.Frame { IsScriptFunction = true, CurrentLine = 50, ImplicitArgs = 2, IsAction = true, Id = 7 };
        var scope = S.Create(frame);
        Assert.True(S.TryGetChildNode(scope, "N", _ => locals, Descriptor, Make, out S.Child? third));
        Assert.Single(seenTypes);
        Assert.Equal("InvType", seenTypes[0]);
        Assert.Equal("n", third!.Name);
        Assert.Equal("Desc", third.Invoker);
        Assert.Same(frame, third.Frame);
        Assert.False(scope.Children.Contains("future"));
        Assert.False(scope.Children.Contains("X"));
        Assert.Equal(7, creates);
        Assert.Equal(7, scope.Children.Count);
        Assert.Equal("x @ line 1", scope.Children.Get("x @ line 1").Name);
        Assert.Equal("x @ line 2", scope.Children.Get("x @ line 2").Name);
        Assert.True(scope.Children.Contains("self"));
        Assert.True(scope.Children.Contains("n @ line 3"));
        Assert.True(scope.Children.Contains("n @ line 4"));
        Assert.False(scope.Children.Contains("x"));
        int created = creates;
        Assert.False(S.TryGetChildNode(scope, "missing", _ => locals, Descriptor, Make, out S.Child? missing));
        Assert.Null(missing);
        Assert.Equal(created, creates);

        var selfFrame = new S.Frame { IsScriptFunction = false, ImplicitArgs = 1, IsAction = true };
        var selfScope = S.Create(selfFrame);
        var selfLocals = new List<S.Local>
        {
            new() { Name = "self", Line = 1, Type = "SelfType" },
            new() { Name = "Invoker", Line = 2, Type = "No" },
        };
        S.CacheChildren(selfScope, _ => selfLocals, Descriptor, Make);
        Assert.Equal(new object?[] { "InvType", "SelfType" }, seenTypes);
        Assert.Equal("Desc", selfScope.Children.Get("SELF").Invoker);

        var plain = S.Create(new S.Frame { IsScriptFunction = false });
        S.CacheChildren(plain, _ => new[] { new S.Local { Name = "only", Line = 1 } }, _ => throw new InvalidOperationException("descriptor"), Make);
        Assert.Null(plain.Children.Get("only").Invoker);

        var folded = S.Create(new S.Frame { IsScriptFunction = false });
        S.CacheChildren(
            folded,
            _ => new[]
            {
                new S.Local { Name = "X", Line = 8, Value = true },
                new S.Local { Name = "x", Line = 9, Value = false },
            },
            _ => null,
            Make);
        Assert.True(folded.Children.Count == 1);
        Assert.Equal("x", folded.Children.Get("X").Name);
        Assert.False(folded.Children.Contains("x @ line 8"));
    }
}
