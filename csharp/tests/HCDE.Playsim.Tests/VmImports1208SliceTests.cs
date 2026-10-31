namespace HCDE.Playsim.Tests;

public class VmImports1208SliceTests
{
    public VmImports1208SliceTests()
    {
        VmImports1208Slice.Reset();
    }

    [Fact]
    public void Init_SortsPrefixStrippedNamesAndInstallsNativeFunctions()
    {
        VmImports1208Slice.HostFontSize = 48;
        VmImports1208Slice.HostFontAlign = 8;
        var direct = new object();
        var spawn = new VmImports1208Slice.NativeSlot();
        var die = new VmImports1208Slice.NativeSlot();
        var fire = new VmImports1208Slice.NativeSlot();
        VmImports1208Slice.PendingFunctions.Add(new VmImports1208Slice.FuncDesc { ClassName = "AWeapon", FuncName = "Fire", FunctionId = 3, Slot = fire });
        VmImports1208Slice.PendingFunctions.Add(new VmImports1208Slice.FuncDesc { ClassName = "AActor", FuncName = "Spawn", FunctionId = 2, Slot = spawn, DirectNative = direct });
        VmImports1208Slice.PendingFunctions.Add(new VmImports1208Slice.FuncDesc { ClassName = "AActor", FuncName = "Die", FunctionId = 1, Slot = die });
        VmImports1208Slice.PendingFields.Add(new VmImports1208Slice.FieldDesc { ClassName = "AWeapon", FieldName = "Ammo", Offset = 8, Size = 4, BitValue = 0 });
        VmImports1208Slice.PendingFields.Add(new VmImports1208Slice.FieldDesc { ClassName = "AActor", FieldName = "Health", Offset = 4, Size = 4, BitValue = 1 });
        VmImports1208Slice.Init();

        Assert.Equal("Font", VmImports1208Slice.Font!.Name);
        Assert.True(VmImports1208Slice.Font.Native);
        Assert.False(VmImports1208Slice.Font.PointerIsConst);
        Assert.Equal(48, VmImports1208Slice.Font.Size);
        Assert.Equal(8, VmImports1208Slice.Font.Align);
        VmImports1208Slice.WriteFont("font");
        Assert.Equal("font", VmImports1208Slice.FontWriteKey);
        Assert.True(VmImports1208Slice.ReadFont("font"));
        Assert.Equal("font", VmImports1208Slice.FontReadKey);

        Assert.Equal(new[] { "Die", "Spawn", "Fire" }, VmImports1208Slice.Functions.Select(item => item.FuncName).ToArray());
        Assert.Equal("Actor.Spawn", spawn.Function!.QualifiedName);
        Assert.Equal("Actor.Spawn [Native]", spawn.Function.PrintableName);
        Assert.Equal(8, spawn.Function.VarFlags);
        Assert.Same(direct, spawn.Function.DirectNativeCall);
        Assert.Equal(2, VmImports1208Slice.FindFunction("actor", "SPAWN")!.FunctionId);
        Assert.Equal(3, VmImports1208Slice.FindFunction("Weapon", "fire")!.FunctionId);
        Assert.Null(VmImports1208Slice.FindFunction("AActor", "Spawn"));
        Assert.Null(VmImports1208Slice.FindFunction("Actor", "Missing"));
        Assert.Throws<NullReferenceException>(() => VmImports1208Slice.FindFunction(null, "Spawn"));
        Assert.Equal(1, VmImports1208Slice.FindField("ACTOR", "health")!.BitValue);
        Assert.Null(VmImports1208Slice.FindField(null, "Health"));
        Assert.Throws<VmImports1208Slice.ScriptError>(() =>
        {
            VmImports1208Slice.PendingFunctions.Add(new VmImports1208Slice.FuncDesc { ClassName = "AActor", FuncName = "Bad", Slot = null });
            VmImports1208Slice.Init();
        });
    }

    [Fact]
    public void FindVm_WalksParentsAndSplitsTheQualifiedName()
    {
        var parent = new VmImports1208Slice.VmClass { Name = "Actor" };
        parent.Symbols["Tick"] = new VmImports1208Slice.Symbol { IsFunction = true, Implementation = "tick-impl" };
        parent.Symbols["Health"] = new VmImports1208Slice.Symbol { IsFunction = false };
        var child = new VmImports1208Slice.VmClass { Name = "DoomImp", Parent = parent };
        child.Symbols["See"] = new VmImports1208Slice.Symbol { IsFunction = true, Implementation = "see-impl" };
        var empty = new VmImports1208Slice.VmClass { Name = "Broken" };
        empty.Symbols["Nope"] = new VmImports1208Slice.Symbol { IsFunction = true, HasVariant = false };
        var untyped = new VmImports1208Slice.VmClass { Name = "Plain", HasType = false, Parent = parent };
        VmImports1208Slice.Register(parent);
        VmImports1208Slice.Register(child);
        VmImports1208Slice.Register(empty);
        VmImports1208Slice.Register(untyped);

        Assert.Equal("see-impl", VmImports1208Slice.FindVm(child, "See"));
        Assert.Equal("tick-impl", VmImports1208Slice.FindVm(child, "tick"));
        Assert.Null(VmImports1208Slice.FindVm(child, "Health"));
        Assert.Null(VmImports1208Slice.FindVm(untyped, "Tick"));
        Assert.Throws<VmImports1208Slice.ScriptError>(() => VmImports1208Slice.FindVm(empty, "Nope"));
        Assert.Throws<NullReferenceException>(() => VmImports1208Slice.FindVm((VmImports1208Slice.VmClass?)null, "Tick"));
        Assert.Equal("tick-impl", VmImports1208Slice.FindVm("ACTOR.Tick"));
        Assert.Null(VmImports1208Slice.FindVm("Actor"));
        Assert.Null(VmImports1208Slice.FindVm("Missing.Tick"));
        Assert.Null(VmImports1208Slice.FindVm(".Tick"));
    }
}
