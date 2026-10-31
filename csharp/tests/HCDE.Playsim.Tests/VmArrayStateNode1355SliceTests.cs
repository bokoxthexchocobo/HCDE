using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmArrayStateNode1355SliceTests
{
    [Fact]
    public void ProtocolCountsAndNames()
    {
        var elem = new Type1355 { Descriptive = "Int", Size = 4 };
        var array = new Type1355
        {
            IsArray = true,
            Descriptive = "Array<Int>[4]",
            ElementType = elem,
            ElementCount = 4,
        };
        var dyn = new Type1355
        {
            IsDynArray = true,
            Descriptive = "DynArray<Int>",
            ElementType = elem,
            Size = 16,
        };
        var pointed = new Type1355
        {
            IsPointer = true,
            Descriptive = "Pointer<DynArray<Int>>",
            Pointed = dyn,
        };
        Assert.Same(elem, VmArrayStateNode1355.GetElementType(array));
        Assert.Same(elem, VmArrayStateNode1355.GetElementType(dyn));
        Assert.Same(elem, VmArrayStateNode1355.GetElementType(pointed));
        Assert.Null(VmArrayStateNode1355.GetElementType(new Type1355 { Descriptive = "Void" }));
        Assert.Throws<NullReferenceException>(() =>
            VmArrayStateNode1355.GetElementType(new Type1355 { IsPointer = true }));

        var header = new HeapArray1355 { Count = 4, Most = 9, DataAddress = 0x400 };
        var valid = new VmWord1355 { Bits = 0x2A, Array = header };
        Assert.Equal(4, VmArrayStateNode1355.GetElementCount(valid, dyn));
        Assert.Equal(9u, header.Most);
        header.Count = uint.MaxValue;
        Assert.Equal(-1, VmArrayStateNode1355.GetElementCount(valid, dyn));
        var hidden = new VmWord1355 { Bits = 0x100000000UL, Array = header };
        Assert.False(VmArrayStateNode1355.IsValid(hidden));
        Assert.Equal(0, VmArrayStateNode1355.GetElementCount(hidden, dyn));
        Assert.Equal(0, VmArrayStateNode1355.GetElementCount(valid, new Type1355()));
        array.ElementCount = uint.MaxValue;
        Assert.Equal(-1, VmArrayStateNode1355.GetElementCount(hidden, array));
        array.ElementCount = 4;
        Assert.Equal(4, VmArrayStateNode1355.GetElementCount(hidden, array));
        Assert.Throws<NullReferenceException>(() =>
            VmArrayStateNode1355.GetElementCount(new VmWord1355 { Bits = 3 }, dyn));

        var services = new ArrayServices1355();
        var fixedNode = new VmArrayStateNode1355("Items", hidden, array, services) { Id = 41 };
        Assert.Same(elem, fixedNode.Element);
        var shown = new Var1355 { EvaluateName = "keep", IndexedVariables = 8 };
        Assert.True(fixedNode.Serialize(shown));
        Assert.Equal(41, shown.VariablesReference);
        Assert.Equal(4, shown.IndexedVariables);
        Assert.Equal("Items", shown.Name);
        Assert.Equal("keep", shown.EvaluateName);
        Assert.Equal("Array<Int>[4]", shown.Type);
        Assert.Equal("Int[<NONE>]", shown.Value);
        var names = new List<string> { "keep" };
        Assert.True(fixedNode.GetChildNames(names));
        Assert.Equal(new[] { "keep" }, names);

        header.Count = 2;
        var live = new VmWord1355 { Bits = 0x2A, Array = header };
        var pointerNode = new VmArrayStateNode1355("Box", live, pointed, services) { Id = 7 };
        Assert.Same(elem, pointerNode.Element);
        shown = new Var1355 { EvaluateName = "keep" };
        Assert.True(pointerNode.Serialize(shown));
        Assert.Equal("Int[2] (0x2a)", shown.Value);
        Assert.Equal("Pointer<DynArray<Int>>", shown.Type);
        Assert.Equal(2, shown.IndexedVariables);
        Assert.Equal("keep", shown.EvaluateName);
        pointerNode.EvalName = "slot";
        Assert.True(pointerNode.Serialize(shown));
        Assert.Equal("slot", shown.EvaluateName);
        names = new List<string> { "keep" };
        Assert.True(pointerNode.GetChildNames(names));
        Assert.Equal(new[] { "keep", "0", "1" }, names);

        header.Count = uint.MaxValue;
        names = new List<string> { "keep" };
        Assert.True(pointerNode.GetChildNames(names));
        Assert.Equal(new[] { "keep" }, names);
        shown = new Var1355();
        Assert.True(pointerNode.Serialize(shown));
        Assert.Equal(0, shown.IndexedVariables);
        Assert.Equal("Int[<NONE>]", shown.Value);

        var plain = new Type1355 { IsStaticArray = true, Descriptive = "Resizable" };
        var bare = new VmArrayStateNode1355("Bare", live, plain, services) { Id = 3 };
        Assert.Null(bare.Element);
        header.Count = 2;
        shown = new Var1355();
        var missing = Assert.Throws<NullReferenceException>(() => bare.Serialize(shown));
        Assert.Equal("m_elementType", missing.Message);
        Assert.Equal(3, shown.VariablesReference);
        Assert.Equal(2, shown.IndexedVariables);
        Assert.Equal("Bare", shown.Name);
        Assert.Null(shown.Type);
        Assert.Null(shown.Value);
        names = new List<string>();
        Assert.True(bare.GetChildNames(names));
        Assert.Equal(new[] { "0", "1" }, names);

        Assert.Equal("(nil)", VmArrayStateNode1355.FormatPointer(0));
        Assert.Equal("0x10", VmArrayStateNode1355.FormatPointer(0x10));
        Assert.Equal("0x100000001", VmArrayStateNode1355.FormatPointer(0x100000001UL));
        int parsed = 0;
        Assert.True(VmArrayStateNode1355.ParseInt("  -2x", out parsed));
        Assert.Equal(-2, parsed);
        Assert.True(VmArrayStateNode1355.ParseInt("\t\n 7", out parsed));
        Assert.Equal(7, parsed);
        Assert.True(VmArrayStateNode1355.ParseInt("-0", out parsed));
        Assert.Equal(0, parsed);
        Assert.True(VmArrayStateNode1355.ParseInt("2147483647", out parsed));
        Assert.Equal(int.MaxValue, parsed);
        Assert.True(VmArrayStateNode1355.ParseInt("-2147483648", out parsed));
        Assert.Equal(int.MinValue, parsed);
        Assert.Throws<FormatException>(() => VmArrayStateNode1355.ParseInt("+", out _));
        Assert.Throws<FormatException>(() => VmArrayStateNode1355.ParseInt("   ", out _));
        Assert.Throws<OverflowException>(() => VmArrayStateNode1355.ParseInt("2147483648", out _));
        Assert.Throws<OverflowException>(() => VmArrayStateNode1355.ParseInt("-2147483649", out _));
        Assert.Throws<OverflowException>(() => VmArrayStateNode1355.ParseInt("999999999999", out _));
    }

    [Fact]
    public void ChildrenElementsAndParse()
    {
        var created = new List<(string Name, ulong Bits, Type1355? Type)>();
        int creates = 0;
        var pointerType = new Type1355 { Descriptive = "Pointer<Wide>", IsPointer = true, Size = 8 };
        var services = new ArrayServices1355
        {
            NewPointer = t =>
            {
                pointerType.Pointed = t;
                return pointerType;
            },
            CreateNode = (name, word, type) =>
            {
                creates++;
                created.Add((name, word.Bits, type));
                return new Node1355 { Id = 100 + creates };
            },
        };
        var elem = new Type1355 { Descriptive = "UInt", Size = 4 };
        var dyn = new Type1355 { IsDynArray = true, ElementType = elem, Descriptive = "DynArray<UInt>" };
        byte[] data = new byte[80];
        Write32(data, 0, 0x11223344);
        Write32(data, 48, 0x80000000);
        var header = new HeapArray1355 { Count = 20, Most = 20, Data = data, DataAddress = 0x4000 };
        var word = new VmWord1355 { Bits = 0x55, Array = header };
        var node = new VmArrayStateNode1355("A", word, dyn, services) { Id = 9 };
        Node1355? sentinel = new Node1355 { Id = 4 };
        Assert.False(node.GetChildNode("-1", ref sentinel));
        Assert.Equal(4, sentinel!.Id);
        Assert.False(node.GetChildNode("20", ref sentinel));
        Assert.Equal(4, sentinel!.Id);
        Assert.Throws<FormatException>(() => node.GetChildNode("nope", ref sentinel));
        Assert.Equal(4, sentinel.Id);
        Assert.True(node.GetChildNode(" 12abc", ref sentinel));
        Assert.Equal(101, sentinel!.Id);
        Assert.Equal("12", created[0].Name);
        Assert.Equal(0x80000000UL, created[0].Bits);
        Assert.Same(elem, created[0].Type);
        int before = creates;
        Assert.True(node.GetChildNode("0012", ref sentinel));
        Assert.Equal(101, sentinel!.Id);
        Assert.Equal(before, creates);
        Assert.True(node.GetChildNode("0x10", ref sentinel));
        Assert.Equal("0", created[^1].Name);
        Assert.Equal(0x11223344UL, created[^1].Bits);

        header.Count = 0;
        sentinel = new Node1355 { Id = 4 };
        Assert.False(node.GetChildNode("0", ref sentinel));
        Assert.Equal(4, sentinel!.Id);
        var invalid = new VmArrayStateNode1355("Z", new VmWord1355 { Bits = 0x100000000UL, Array = header }, dyn, services);
        Assert.False(invalid.GetChildNode("0", ref sentinel));
        Assert.Equal(4, sentinel!.Id);

        header.Count = 4;
        float sample = 1.25f;
        Write32(header.Data!, 0, (uint)BitConverter.SingleToInt32Bits(sample));
        var float4 = Float(4);
        ReadKind(services, header, dyn, float4, 0, out var got);
        Assert.Equal(VmWord1355.FromDouble(sample).Bits, got.Bits);
        Assert.Same(float4, got.Type);
        double stored = -2.5;
        Write64(header.Data!, 8, (ulong)BitConverter.DoubleToInt64Bits(stored));
        ReadKind(services, header, dyn, Float(8), 1, out got);
        Assert.Equal(VmWord1355.FromDouble(stored).Bits, got.Bits);
        int wideCalls = 0;
        services.NewPointer = t =>
        {
            wideCalls++;
            pointerType.Pointed = t;
            return pointerType;
        };
        var oddFloat = Float(2);
        oddFloat.Descriptive = "OddFloat";
        header.Data = null;
        ReadKind(services, header, dyn, oddFloat, 0, out got);
        Assert.Equal(0UL, got.Bits);
        Assert.Same(oddFloat, got.Type);
        Assert.Equal(0, wideCalls);
        var bigFloat = Float(16);
        ReadKind(services, header, dyn, bigFloat, 0, out got);
        Assert.Equal(0UL, got.Bits);
        Assert.Same(pointerType, got.Type);
        Assert.Equal(1, wideCalls);

        header.Data = new byte[32];
        Write64(header.Data, 0, 0x1122334455667788UL);
        var objectElem = new Type1355 { IsObjectPointer = true, Size = 4, Descriptive = "Actor" };
        ReadKind(services, header, dyn, objectElem, 0, out got);
        Assert.Equal(0x1122334455667788UL, got.Bits);
        var pointerElem = new Type1355 { IsPointer = true, Size = 4, Descriptive = "void*" };
        Write64(header.Data, 8, 0x99);
        ReadKind(services, header, dyn, pointerElem, 1, out got);
        Assert.Equal(0x99UL, got.Bits);
        header.DataAddress = 0x4000;
        Type1355.StringType.Size = 8;
        ReadKind(services, header, dyn, Type1355.StringType, 2, out got);
        Assert.Equal(0x4000UL + 16, got.Bits);
        var u16 = new Type1355 { Size = 2, Descriptive = "uint16" };
        Write16(header.Data, 0, 0xFFFF);
        ReadKind(services, header, dyn, u16, 0, out got);
        Assert.Equal(0xFFFFUL, got.Bits);
        var u8 = new Type1355 { Size = 1, Descriptive = "uint8" };
        header.Data[3] = 0xAB;
        ReadKind(services, header, dyn, u8, 3, out got);
        Assert.Equal(0xABUL, got.Bits);
        var u64 = new Type1355 { Size = 8, Descriptive = "uint64" };
        Write64(header.Data, 0, 0x0102030405060708UL);
        ReadKind(services, header, dyn, u64, 0, out got);
        Assert.Equal(0x0102030405060708UL, got.Bits);
        var wide = new Type1355 { Size = 12, Descriptive = "Vec" };
        header.DataAddress = 0x1000;
        int pointers = wideCalls;
        ReadKind(services, header, dyn, wide, 1, out got);
        Assert.Equal(0x1000UL + 12, got.Bits);
        Assert.Same(pointerType, got.Type);
        Assert.Equal(pointers + 1, wideCalls);

        services.NewPointer = null;
        var blown = new VmArrayStateNode1355("W", word, dyn, services);
        Node1355? unused = null;
        var missingPointer = Assert.Throws<InvalidOperationException>(() => blown.GetChildNode("0", ref unused));
        Assert.Equal("NewPointer", missingPointer.Message);
        Assert.Null(unused);

        var fixedElem = new Type1355 { Descriptive = "Fixed", Size = 6 };
        var fixedType = new Type1355
        {
            IsArray = true,
            ElementType = fixedElem,
            ElementCount = 3,
            Descriptive = "Array<Fixed>[3]",
        };
        ulong seenAddress = 0;
        Type1355? seenType = null;
        services.GetVmValue = (address, type) =>
        {
            seenAddress = address;
            seenType = type;
            return new VmWord1355 { Bits = 0x77 };
        };
        services.CreateNode = (name, value, type) =>
        {
            created.Add((name, value.Bits, type));
            return new Node1355 { Id = 50 };
        };
        var fixedWord = new VmWord1355 { Bits = 0x5000 };
        var fixedNode = new VmArrayStateNode1355("F", fixedWord, fixedType, services);
        Node1355? child = null;
        Assert.True(fixedNode.GetChildNode("2", ref child));
        Assert.Equal(0x5000UL + 12, seenAddress);
        Assert.Same(fixedElem, seenType);
        Assert.Equal(50, child!.Id);
        Assert.Equal("2", created[^1].Name);
        Assert.Equal(0x77UL, created[^1].Bits);
        Assert.Same(fixedElem, created[^1].Type);

        var pointedArray = new Type1355
        {
            IsPointer = true,
            Pointed = fixedType,
            Descriptive = "Pointer<Array>",
        };
        var pointedNode = new VmArrayStateNode1355("P", fixedWord, pointedArray, services);
        Assert.True(pointedNode.GetChildNode("1", ref child));
        Assert.Equal(0x5000UL + 6, seenAddress);

        services.GetVmValue = null;
        var noRead = new VmArrayStateNode1355("N", fixedWord, fixedType, services);
        var missingRead = Assert.Throws<InvalidOperationException>(() => noRead.GetChildNode("0", ref unused));
        Assert.Equal("GetVMValue", missingRead.Message);
        services.GetVmValue = (_, _) => new VmWord1355 { Bits = 1 };
        services.CreateNode = null;
        var noCreate = new VmArrayStateNode1355("C", fixedWord, fixedType, services);
        var missingCreate = Assert.Throws<InvalidOperationException>(() => noCreate.GetChildNode("0", ref unused));
        Assert.Equal("CreateNodeForVariable", missingCreate.Message);

        var staticOnly = new Type1355 { IsStaticArray = true, Descriptive = "Static" };
        header.Count = 2;
        var staticNode = new VmArrayStateNode1355("S", word, staticOnly, services);
        Assert.Null(staticNode.Element);
        Node1355? kept = new Node1355 { Id = 8 };
        var noElement = Assert.Throws<NullReferenceException>(() => staticNode.GetChildNode("0", ref kept));
        Assert.Equal("ElementType", noElement.Message);
        Assert.Equal(8, kept!.Id);

        var both = new Type1355
        {
            IsArray = true,
            IsStaticArray = true,
            ElementType = u8,
            ElementCount = 99,
            Descriptive = "ResizableArray<uint8>",
        };
        header.Count = 4;
        header.Data = new byte[8];
        header.Data[1] = 0x3C;
        ulong staticBits = 0;
        services.CreateNode = (name, value, type) =>
        {
            staticBits = value.Bits;
            return new Node1355 { Id = 70 };
        };
        services.GetVmValue = (_, _) => throw new InvalidOperationException("fixed");
        var staticArray = new VmArrayStateNode1355("R", word, both, services);
        Assert.Same(u8, staticArray.Element);
        Assert.Equal(4, VmArrayStateNode1355.GetElementCount(word, both));
        Node1355? resized = null;
        Assert.True(staticArray.GetChildNode("1", ref resized));
        Assert.Equal(70, resized!.Id);
        Assert.Equal(0x3CUL, staticBits);
    }

    static Type1355 Float(int size)
    {
        return new Type1355 { IsFloat = true, Size = size, Descriptive = "Float" + size };
    }

    static void ReadKind(
        ArrayServices1355 services,
        HeapArray1355 header,
        Type1355 dyn,
        Type1355 element,
        int index,
        out (ulong Bits, Type1355? Type) got)
    {
        dyn.ElementType = element;
        (ulong Bits, Type1355? Type) seen = default;
        services.CreateNode = (name, word, type) =>
        {
            seen = (word.Bits, type);
            return new Node1355 { Id = 11 };
        };
        var node = new VmArrayStateNode1355("E", new VmWord1355 { Bits = 0x55, Array = header }, dyn, services);
        Node1355? child = null;
        Assert.True(node.GetChildNode(index.ToString(), ref child));
        got = seen;
    }

    static void Write16(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    static void Write32(byte[] data, int offset, uint value)
    {
        for (int i = 0; i < 4; i++)
            data[offset + i] = (byte)(value >> (8 * i));
    }

    static void Write64(byte[] data, int offset, ulong value)
    {
        for (int i = 0; i < 8; i++)
            data[offset + i] = (byte)(value >> (8 * i));
    }
}
