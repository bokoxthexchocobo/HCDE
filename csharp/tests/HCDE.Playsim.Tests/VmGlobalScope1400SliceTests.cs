using S = HCDE.Playsim.VmGlobalScope1400Slice;

namespace HCDE.Playsim.Tests;

public class VmGlobalScope1400SliceTests
{
    [Fact]
    public void SerializeAppendsTheStaticGlobalsAndOmitsTheCommentedLevel()
    {
        Assert.Equal(59, S.GlobalNames.Length);
        Assert.Contains("Level", S.GlobalNames);
        Assert.DoesNotContain("level", S.GlobalNames);
        var names = new List<string> { "seed" };
        var scope = new S.Scope();
        Assert.True(scope.GetChildNames(names));
        Assert.Equal(60, names.Count);
        Assert.Equal("seed", names[0]);
        Assert.Equal("AllActorClasses", names[^1]);
        scope.Id = 17;
        var protocol = new S.ProtocolScope();
        Assert.True(scope.Serialize(protocol));
        Assert.Equal("Global", protocol.Name);
        Assert.False(protocol.Expensive);
        Assert.Equal("globals", protocol.PresentationHint);
        Assert.Equal(17, protocol.VariablesReference);
        Assert.Equal(59, protocol.NamedVariables);
        Assert.Equal(0, protocol.IndexedVariables);
        Assert.Equal(0, scope.Scans);
        Assert.Throws<ArgumentNullException>(() => scope.GetChildNames(null!));
    }

    [Fact]
    public void ChildCacheFoldsLevelSkipsNonFieldsAndKeepsTheFirstKey()
    {
        var none = new S.Symbol { Key = "Level", NameIsNone = true, IsField = true, Type = "skip", Offset = 1 };
        var other = new S.Symbol { Key = "not-published", IsField = true, Type = "no", Offset = 2 };
        var plain = new S.Symbol { Key = "players", IsField = false, Type = "fn", Offset = 3 };
        var first = new S.Symbol { Key = "level", IsField = true, Type = "int", Offset = 100, BitValue = 3 };
        var second = new S.Symbol { Key = "LEVEL", IsField = true, Type = "int", Offset = 200, BitValue = -1 };
        var later = new S.Symbol { Key = "paused", IsField = true, Type = "bool", Offset = 9 };
        var ns = new S.Namespace();
        ns.Symbols.Add(other);
        ns.Symbols.Add(none);
        ns.Symbols.Add(plain);
        ns.Symbols.Add(first);
        ns.Symbols.Add(second);
        var scope = new S.Scope();
        scope.Namespaces.Add(ns);
        var reads = new List<(ulong Address, object? Type, int Bit)>();
        S.Node? node = new S.Node { Name = "untouched" };
        bool found = scope.TryGetChild("Level", ref node, (address, type, bit) =>
        {
            reads.Add((address, type, bit));
            return address;
        }, (name, value, type) => new S.Node { Name = name, Value = value, Type = type });
        Assert.True(found);
        Assert.Equal("LEVEL", node!.Name);
        Assert.Equal(200ul, node.Value);
        Assert.Equal("level", node.StoredKey);
        Assert.Equal(2, reads.Count);
        Assert.Equal((100ul, (object?)"int", 3), reads[0]);
        Assert.Equal((200ul, (object?)"int", -1), reads[1]);
        Assert.Equal(0, none.TypeReads);
        Assert.Equal(0, plain.TypeReads);
        Assert.Equal(1, first.TypeReads);
        Assert.Equal(1, scope.Scans);
        ns.Symbols.Add(later);
        S.Node? missing = new S.Node { Name = "keep" };
        Assert.False(scope.TryGetChild("paused", ref missing, (_, _, _) => 0, (_, _, _) => new S.Node()));
        Assert.Equal("keep", missing!.Name);
        Assert.Equal(1, scope.Scans);
        Assert.Equal(2, reads.Count);

        var empty = new S.Scope();
        var bag = new S.Namespace();
        bag.Symbols.Add(plain);
        empty.Namespaces.Add(bag);
        S.Node? sentinel = new S.Node { Name = "stay" };
        Assert.False(empty.TryGetChild("players", ref sentinel, (_, _, _) => 0, (_, _, _) => new S.Node()));
        Assert.Equal("stay", sentinel!.Name);
        bag.Symbols.Add(later);
        Assert.True(empty.TryGetChild("paused", ref sentinel, (address, _, _) => address, (name, value, type) => new S.Node { Name = name, Value = value, Type = type }));
        Assert.Equal(9ul, sentinel!.Value);
        Assert.Equal(2, empty.Scans);
        Assert.Throws<InvalidOperationException>(() =>
        {
            var bad = new S.Scope();
            var broken = new S.Namespace();
            broken.Symbols.Add(null!);
            bad.Namespaces.Add(broken);
            S.Node? unused = null;
            bad.TryGetChild("Level", ref unused, (_, _, _) => 0, (_, _, _) => new S.Node());
        });
    }
}
