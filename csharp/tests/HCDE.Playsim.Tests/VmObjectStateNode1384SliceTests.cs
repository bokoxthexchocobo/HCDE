using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmObjectStateNode1384SliceTests
{
    [Fact]
    public void SerializeFormatsNullSubclassUnrelatedAndSubview()
    {
        var objectDesc = new VmObjectStateNode1384Slice.ClassDesc();
        var actorDesc = new VmObjectStateNode1384Slice.ClassDesc { Parent = objectDesc };
        var impDesc = new VmObjectStateNode1384Slice.ClassDesc { Parent = actorDesc };
        var treeDesc = new VmObjectStateNode1384Slice.ClassDesc();
        var objectType = new VmObjectStateNode1384Slice.TypeNode { Name = "Object", IsClass = true, Descriptor = objectDesc };
        var actorType = new VmObjectStateNode1384Slice.TypeNode
        {
            Name = "Actor",
            IsClass = true,
            ParentType = objectType,
            Descriptor = actorDesc,
        };
        var impType = new VmObjectStateNode1384Slice.TypeNode
        {
            Name = "DoomImp",
            IsClass = true,
            ParentType = actorType,
            Descriptor = impDesc,
        };
        var treeType = new VmObjectStateNode1384Slice.TypeNode { Name = "Tree", IsClass = true, Descriptor = treeDesc };
        actorDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "Health", BitValue = 1, Type = actorType });
        impDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "Health", BitValue = 1, Type = actorType });
        impDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "See", BitValue = 2, Type = actorType });
        treeDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "Age", BitValue = 8, Type = treeType });
        var impPtr = Pointer(impType);
        var actorPtr = Pointer(actorType);
        var slice = new VmObjectStateNode1384Slice
        {
            Read = (_, field) => field.Name + ":" + field.BitValue.ToString(),
        };

        var imp = new VmObjectStateNode1384Slice.Obj { Address = 0x1a2b, VmType = impType, Class = impDesc };
        var node = slice.New("imp", 0x1a2b, true, impPtr, imp);
        node.Id = 4;
        node.EvalName = "self.imp";
        var variable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(node.Serialize(variable));
        Assert.Equal("DoomImp", variable.Type);
        Assert.Equal("DoomImp (0x1a2b)", variable.Value);
        Assert.Equal(4, variable.VariablesReference);
        Assert.Equal(new[] { "Actor", "Health", "See" }, Names(node));
        Assert.Equal("imp", variable.Name);
        Assert.Equal("self.imp", variable.EvaluateName);
        Assert.True(node.TryGetChild("HEALTH", out var health));
        var healthNode = Assert.IsType<VmObjectStateNode1384Slice.Node>(health);
        Assert.Equal("Health:1", healthNode.ValueText);
        Assert.True(node.TryGetChild("actor", out var parent));
        var parentNode = Assert.IsType<VmObjectStateNode1384Slice.ObjectNode>(parent);
        Assert.True(parentNode.SubView);
        var parentVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(parentNode.Serialize(parentVariable));
        Assert.Equal("Actor", parentVariable.Type);
        Assert.Equal("", parentVariable.Value);
        Assert.Equal(new[] { "Object", "Health" }, Names(parentNode));

        var missing = slice.New("imp", 0, true, impPtr, null);
        var nullVariable = new VmObjectStateNode1384Slice.Variable { Value = "stale" };
        Assert.True(missing.Serialize(nullVariable));
        Assert.Equal("DoomImp <NULL>", nullVariable.Value);
        Assert.Equal(0, nullVariable.VariablesReference);
        Assert.Equal(0, nullVariable.NamedVariables);
        Assert.Empty(slice.Logs);

        var tree = new VmObjectStateNode1384Slice.Obj { Address = 0x10, VmType = treeType, Class = treeDesc };
        var alien = slice.New("alien", 0x10, true, actorPtr, tree);
        var alienVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(alien.Serialize(alienVariable));
        Assert.Equal("Tree (0x10) as Actor", alienVariable.Value);
        Assert.Equal(new[] { "Age" }, Names(alien));

        var baseObj = new VmObjectStateNode1384Slice.Obj { Address = 0x20, VmType = actorType, Class = actorDesc };
        var narrower = slice.New("narrow", 0x20, true, impPtr, baseObj);
        var narrowVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(narrower.Serialize(narrowVariable));
        Assert.Equal("Actor (0x20) as DoomImp", narrowVariable.Value);
        Assert.Equal(new[] { "Object", "Health" }, Names(narrower));

        var badMagic = new VmObjectStateNode1384Slice.Obj { Address = 0x30, Valid = false, VmType = impType, Class = impDesc };
        var invalid = slice.New("bad", 0x30, false, impPtr, badMagic);
        invalid.Id = 9;
        var invalidVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(invalid.Serialize(invalidVariable));
        Assert.Equal(0, invalidVariable.VariablesReference);
        Assert.Equal("DoomImp (0x30)", invalidVariable.Value);
        Assert.Equal(0, invalidVariable.NamedVariables);

        var view = slice.New("view", 0x1a2b, true, actorPtr, imp, subView: true);
        var viewVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(view.Serialize(viewVariable));
        Assert.Equal("Actor (0x1a2b)", viewVariable.Value);
        Assert.Equal(new[] { "Object", "Health" }, Names(view));

        var plain = slice.New("plain", 0, false, actorType, null);
        var plainVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(plain.Serialize(plainVariable));
        Assert.Equal("Actor", plainVariable.Value);
    }

    [Fact]
    public void RetryDropsTheParentNameAndResetKeepsTheCache()
    {
        var parentDesc = new VmObjectStateNode1384Slice.ClassDesc();
        var declaredDesc = new VmObjectStateNode1384Slice.ClassDesc { Parent = parentDesc };
        var actualDesc = new VmObjectStateNode1384Slice.ClassDesc { Parent = parentDesc };
        var parentType = new VmObjectStateNode1384Slice.TypeNode { Name = "Actor", IsClass = true, Descriptor = parentDesc };
        var declaredType = new VmObjectStateNode1384Slice.TypeNode
        {
            Name = "DoomImp",
            IsClass = true,
            ParentType = parentType,
            Descriptor = declaredDesc,
        };
        declaredDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "Bad", BitValue = 1 });
        actualDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "A", BitValue = 3 });
        actualDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "B", BitValue = 4 });
        var reads = 0;
        var slice = new VmObjectStateNode1384Slice
        {
            Read = (_, field) =>
            {
                reads++;
                if (field.Name == "Bad")
                    throw new VmObjectStateNode1384Slice.RecoverableError("no weapon");
                return field.Name;
            },
        };
        var obj = new VmObjectStateNode1384Slice.Obj { Address = 0x44, VmType = declaredType, Class = actualDesc };
        var node = slice.New("imp", 0x44, true, Pointer(declaredType), obj);
        var names = new List<string>();
        Assert.True(node.GetChildNames(names));
        Assert.Equal(new[] { "A", "B" }, names);
        Assert.False(node.Children.ContainsKey("Actor"));
        Assert.True(node.VirtualChildren.ContainsKey("ACTOR"));
        var copy = node.VirtualContainerChildren();
        copy.Clear();
        Assert.True(node.VirtualChildren.ContainsKey("Actor"));
        Assert.True(node.TryGetChild("b", out var field));
        Assert.Equal("B", Assert.IsType<VmObjectStateNode1384Slice.Node>(field).Name);
        var before = reads;
        Assert.Equal(new[] { "A", "B" }, Names(node));
        Assert.Equal(before, reads);
        node.Reset();
        Assert.False(node.TryGetChild("A", out _));
        Assert.Equal(new[] { "A", "B" }, node.Cache);

        var boomDesc = new VmObjectStateNode1384Slice.ClassDesc();
        boomDesc.Fields.Add(new VmObjectStateNode1384Slice.Field { Name = "Boom" });
        var boom = new VmObjectStateNode1384Slice.Obj { Address = 0x45, VmType = declaredType, Class = boomDesc };
        var failing = new VmObjectStateNode1384Slice
        {
            Read = (_, field) => throw new VmObjectStateNode1384Slice.RecoverableError(
                field.Name == "Bad" ? "no weapon" : "second"),
        };
        var failed = failing.New("imp", 0x45, true, Pointer(declaredType), boom);
        var kept = new List<string> { "kept" };
        Assert.False(failed.GetChildNames(kept));
        Assert.Equal(new[] { "kept" }, kept);
        Assert.Equal(new[]
        {
            "Failed to get child names for object 'imp' of type DoomImp",
            "Error: no weapon",
            "Error: second",
        }, failing.Logs);
        Assert.True(failed.VirtualChildren.ContainsKey("Actor"));

        var plain = new VmObjectStateNode1384Slice.TypeNode { Name = "int" };
        var logged = new VmObjectStateNode1384Slice();
        var other = logged.New("n", 0x4, true, Pointer(plain), new VmObjectStateNode1384Slice.Obj { Address = 0x4 });
        var otherVariable = new VmObjectStateNode1384Slice.Variable();
        Assert.True(other.Serialize(otherVariable));
        Assert.Equal("int (0x4)", otherVariable.Value);
        Assert.Equal(0, otherVariable.NamedVariables);
        Assert.Equal(new[] { "Failed to get child names for object 'n' of type int" }, logged.Logs);
    }

    static VmObjectStateNode1384Slice.TypeNode Pointer(VmObjectStateNode1384Slice.TypeNode pointed)
    {
        return new VmObjectStateNode1384Slice.TypeNode
        {
            Name = pointed.Name + "*",
            IsObjectPointer = true,
            Pointed = pointed,
        };
    }

    static List<string> Names(VmObjectStateNode1384Slice.ObjectNode node)
    {
        var names = new List<string>();
        Assert.True(node.GetChildNames(names));
        return names;
    }
}
