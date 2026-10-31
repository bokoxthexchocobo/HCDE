using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmRegisters1349SliceTests
{
    [Fact]
    public void ScopeNamesAndSerialize()
    {
        var bare = new Frame1349();
        var scope = new VmRegisters1349Slice(bare);
        var names = new List<string> { "kept" };
        Assert.True(scope.GetChildNames(names));
        Assert.Equal(new[] { "kept", "Params", "Ints", "Floats", "Strings", "Pointers" }, names);
        Assert.Null(VmRegisters1349Slice.ScriptFunction(null));
        Assert.Null(VmRegisters1349Slice.ScriptFunction(new Script1349 { Native = true }));

        var frame = Loaded();
        scope = new VmRegisters1349Slice(frame) { Id = 41 };
        names.Clear();
        Assert.True(scope.GetChildNames(names));
        Assert.Equal(new[] { "Params", "Ints", "Floats", "Strings", "Pointers", "SpecialInits" }, names);

        Node1349? sentinel = new Node1349 { Id = 7 };
        Assert.False(scope.GetChildNode("Nope", ref sentinel));
        Assert.Equal(7, sentinel!.Id);
        Assert.False(scope.GetChildNode("", ref sentinel));
        Assert.Equal(7, sentinel!.Id);

        Assert.True(scope.GetChildNode("params", ref sentinel));
        var parms = Assert.IsType<ParamsNode1349>(sentinel);
        Assert.True(scope.GetChildNode("INTS", ref sentinel));
        var ints = Assert.IsType<IntNode1349>(sentinel);
        Assert.NotSame(parms, scope.GetChildNode("Params", ref sentinel) ? sentinel : null);
        Node1349? again = null;
        Assert.True(scope.GetChildNode("Params", ref again));
        Assert.NotSame(parms, again);
        Assert.True(scope.GetChildNode("floats", ref sentinel));
        Assert.IsType<FloatNode1349>(sentinel);
        Assert.True(scope.GetChildNode("STRINGS", ref sentinel));
        Assert.IsType<StringNode1349>(sentinel);
        Assert.True(scope.GetChildNode("PoInTeRs", ref sentinel));
        Assert.IsType<PointerNode1349>(sentinel);
        Assert.True(scope.GetChildNode("specialinits", ref sentinel));
        Assert.IsType<SpecialNode1349>(sentinel);

        var shown = new Scope1349 { IndexedVariables = 3, NamedVariables = 9 };
        Assert.True(scope.SerializeScope(shown));
        Assert.Equal("Registers", shown.Name);
        Assert.False(shown.Expensive);
        Assert.Equal("registers", shown.PresentationHint);
        Assert.Equal(41, shown.VariablesReference);
        Assert.Equal(6, shown.NamedVariables);
        Assert.Equal(0, shown.IndexedVariables);

        ints.Id = 15;
        var variable = new Var1349 { IndexedVariables = 4 };
        Assert.True(ints.Serialize(variable));
        Assert.Equal("INTS", variable.Name);
        Assert.Equal("INTS Registers", variable.Type);
        Assert.Equal("INTS[3]", variable.Value);
        Assert.Equal(3, variable.NamedVariables);
        Assert.Equal(4, variable.IndexedVariables);
        Assert.Equal(15, variable.VariablesReference);

        parms.Id = 8;
        variable = new Var1349 { NamedVariables = 9, IndexedVariables = 1 };
        Assert.True(parms.Serialize(variable));
        Assert.Equal("Params", variable.Name);
        Assert.Equal("Parameter Registers", variable.Type);
        Assert.Equal("Params - Max: 4, In Use: 2", variable.Value);
        Assert.Equal(9, variable.NamedVariables);
        Assert.Equal(4, variable.IndexedVariables);
        Assert.Equal(8, variable.VariablesReference);

        var special = new SpecialNode1349("specialinits", frame) { Id = 3 };
        variable = new Var1349 { NamedVariables = 6 };
        Assert.True(special.Serialize(variable));
        Assert.Equal("SpecialInits", variable.Name);
        Assert.Equal("Special Setup Registers", variable.Type);
        Assert.Equal("Special Setup - Max: 2", variable.Value);
        Assert.Equal(6, variable.NamedVariables);
        Assert.Equal(2, variable.IndexedVariables);
        Assert.Equal(3, variable.VariablesReference);

        var none = new IntNode1349("Ints", new Frame1349());
        variable = new Var1349();
        Assert.True(none.Serialize(variable));
        Assert.Equal("Ints[0]", variable.Value);
        Assert.Equal(0, variable.NamedVariables);

        var children = new List<string> { "pre" };
        Assert.True(ints.GetChildNames(children));
        Assert.True(new FloatNode1349("Floats", frame).GetChildNames(children));
        Assert.True(new StringNode1349("Strings", frame).GetChildNames(children));
        Assert.True(new PointerNode1349("Pointers", frame).GetChildNames(children));
        Assert.True(parms.GetChildNames(children));
        Assert.True(special.GetChildNames(children));
        Assert.Equal(new[] { "pre", "d0", "d1", "d2", "f0", "s0", "s1", "a0", "a1", "0", "1", "2", "3", "0", "1" }, children);

        Assert.Throws<NullReferenceException>(() => new VmRegisters1349Slice(null).GetChildNames(new List<string>()));
        var native = new SpecialNode1349("SpecialInits", new Frame1349 { Func = new Script1349 { Native = true } });
        var thrown = Assert.Throws<NullReferenceException>(() => native.GetNumberOfRegisters());
        Assert.Equal("script function", thrown.Message);
    }

    [Fact]
    public void ChildIndexAndSpecialSetup()
    {
        var frame = Loaded();
        var seen = new List<(string Name, VmVal1349 Val, string Type)>();
        frame.Create = (name, val, type) =>
        {
            seen.Add((name, val, type));
            return new Node1349 { Id = seen.Count };
        };
        var ints = new IntNode1349("Ints", frame);
        Node1349? node = new Node1349 { Id = 99 };
        Assert.True(ints.GetChildNode("d2", ref node));
        Assert.Equal(1, node!.Id);
        Assert.Equal(("d2", "int", VmRegisters1349Slice.TypeSInt32), (seen[0].Name, seen[0].Val.Kind, seen[0].Type));
        Assert.Equal(7, seen[0].Val.Bits);

        Assert.True(ints.GetChildNode("d2extra", ref node));
        Assert.Equal("d2extra", seen[1].Name);
        Assert.Equal(7, seen[1].Val.Bits);
        Assert.True(ints.GetChildNode("D0", ref node));
        Assert.Equal(10, seen[2].Val.Bits);
        frame.Ints[2] = 70;
        Assert.True(ints.GetChildNode("d2", ref node));
        Assert.Equal(70, seen[3].Val.Bits);

        var bad = Assert.Throws<FormatException>(() => ints.GetChildNode("d", ref node));
        Assert.Equal("stoi", bad.Message);
        Assert.Throws<FormatException>(() => ints.GetChildNode("dabc", ref node));
        Assert.Throws<ArgumentOutOfRangeException>(() => ints.GetChildNode("", ref node));
        var overflow = Assert.Throws<OverflowException>(() => ints.GetChildNode("d2147483648", ref node));
        Assert.Equal("stoi", overflow.Message);
        node = new Node1349 { Id = 99 };
        int before = seen.Count;
        Assert.False(ints.GetChildNode("d2147483647", ref node));
        Assert.False(ints.GetChildNode("d-1", ref node));
        Assert.False(ints.GetChildNode("d-2147483648", ref node));
        Assert.False(ints.GetChildNode("d3", ref node));
        Assert.Equal(before, seen.Count);
        Assert.Equal(99, node!.Id);

        var parms = new ParamsNode1349("params", frame);
        var blank = Assert.Throws<FormatException>(() => parms.GetChildNode("", ref node));
        Assert.Equal("stoi", blank.Message);
        Assert.Throws<FormatException>(() => parms.GetChildNode("+", ref node));
        Assert.True(parms.GetChildNode("  1", ref node));
        Assert.Equal("param", seen[^1].Val.Kind);
        Assert.Equal(55, seen[^1].Val.Bits);
        Assert.Equal(VmRegisters1349Slice.TypeVoidPtr, seen[^1].Type);
        Assert.Equal(10, VmRegisters1349Slice.Stoi("010"));
        Assert.False(parms.GetChildNode("010", ref node));
        Assert.True(parms.GetChildNode("+1", ref node));
        Assert.Equal(55, seen[^1].Val.Bits);

        var pointers = new PointerNode1349("Pointers", frame);
        Assert.True(pointers.GetChildNode("a1", ref node));
        Assert.Same(frame.Pointers[1], seen[^1].Val.Ref);
        Assert.Equal("ptr", seen[^1].Val.Kind);
        Assert.Equal(VmRegisters1349Slice.TypeVoidPtr, seen[^1].Type);
        Assert.True(pointers.GetChildNode("A0", ref node));
        Assert.Null(seen[^1].Val.Ref);
        Assert.False(pointers.GetChildNode("a2", ref node));
        Assert.False(pointers.GetChildNode("a-1", ref node));

        var floats = new FloatNode1349("Floats", frame);
        Assert.True(floats.GetChildNode("f0", ref node));
        Assert.Equal(1.5, seen[^1].Val.Float);
        Assert.Equal(VmRegisters1349Slice.TypeFloat64, seen[^1].Type);

        var strings = new StringNode1349("Strings", frame);
        Assert.True(strings.GetChildNode("s1", ref node));
        Assert.Same(frame.Strings[1], seen[^1].Val.Ref);
        Assert.Equal("string-addr", seen[^1].Val.Kind);
        Assert.Equal(VmRegisters1349Slice.TypeString, seen[^1].Type);

        var reads = new List<(long Addr, string Type)>();
        frame.ReadMemory = (addr, type) =>
        {
            reads.Add((addr, type));
            return new VmVal1349 { Kind = "mem", Bits = addr, TypeName = type };
        };
        var special = new SpecialNode1349("SpecialInits", frame);
        Assert.True(special.GetChildNode("0", ref node));
        Assert.Equal((1008L, "int"), reads[0]);
        Assert.Equal("mem", seen[^1].Val.Kind);
        Assert.Equal("int", seen[^1].Type);
        Assert.True(special.GetChildNode("1x", ref node));
        Assert.Equal((1024L, "string"), reads[1]);
        Assert.Equal("string", special.GetRegisterType(1));
        Assert.False(special.GetChildNode("2", ref node));
        Assert.Equal(2, reads.Count);

        frame.ReadMemory = null;
        Assert.Throws<InvalidOperationException>(() => special.GetRegisterValue(0));
        frame.Create = null;
        Assert.Throws<InvalidOperationException>(() => ints.GetChildNode("d0", ref node));
        Assert.False(VmRegisters1349Slice.SameName("Param", "Params"));
        Assert.True(VmRegisters1349Slice.SameName("Ints", "iNtS"));
    }

    static Frame1349 Loaded()
    {
        var frame = new Frame1349
        {
            NumRegD = 3,
            NumRegF = 1,
            NumRegS = 2,
            NumRegA = 2,
            MaxParam = 4,
            NumParam = 2,
            Ints = new[] { 10, -4, 7 },
            Floats = new[] { 1.5 },
            Pointers = new object?[] { null, "obj" },
            Params = new VmVal1349[4],
            ExtraBase = 1000,
            Func = new Script1349(),
        };
        frame.Strings = new[]
        {
            new Slot1349 { Index = 0, Text = "a" },
            new Slot1349 { Index = 1, Text = "" },
        };
        frame.Params[1] = new VmVal1349 { Kind = "param", Bits = 55 };
        frame.Func.SpecialInits.Add(("int", 8));
        frame.Func.SpecialInits.Add(("string", 24));
        return frame;
    }
}
