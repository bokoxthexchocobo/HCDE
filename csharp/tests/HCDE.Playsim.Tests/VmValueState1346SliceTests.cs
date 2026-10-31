using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmValueState1346SliceTests
{
    [Fact]
    public void ScalarsPointersAndFloats()
    {
        var host = new VmValueHost1346();
        var five = Show(VmValueState1346Slice.SInt32, VmBits1346.FromInt(5), host);
        Assert.Equal("int32", five.Type);
        Assert.Equal("5", five.Value);
        var neg = Show(VmValueState1346Slice.SInt32, VmBits1346.FromInt(-2), host);
        Assert.Equal("-2", neg.Value);
        var wide = Show(VmValueState1346Slice.UInt32, VmBits1346.FromInt(-1), host);
        Assert.Equal("uint32", wide.Type);
        Assert.Equal("18446744073709551615", wide.Value);
        var half = Show(VmValueState1346Slice.UInt16, VmBits1346.FromInt(0x1ABCD), host);
        Assert.Equal("43981", half.Value);
        var tiny = Show(VmValueState1346Slice.SInt8, VmBits1346.FromInt(0x1FF), host);
        Assert.Equal("int8", tiny.Type);
        Assert.Equal("-1", tiny.Value);
        var byteVal = Show(VmValueState1346Slice.UInt8, VmBits1346.FromInt(0x1FF), host);
        Assert.Equal("255", byteVal.Value);
        var zero = Show(VmValueState1346Slice.SInt32, VmBits1346.FromInt(0), host);
        Assert.Equal("0", zero.Value);

        var third = new VmBits1346 { Float = 1.0 / 3.0 };
        var narrow = Show(VmValueState1346Slice.Float32, third, host);
        Assert.Equal("float", narrow.Type);
        Assert.Equal("0.333333", narrow.Value);
        var wideFloat = Show(VmValueState1346Slice.Float64, new VmBits1346 { Float = -0.0 }, host);
        Assert.Equal("double", wideFloat.Type);
        Assert.Equal("-0.000000", wideFloat.Value);
        var inf = Show(VmValueState1346Slice.Float64, new VmBits1346 { Float = double.PositiveInfinity }, host);
        Assert.Equal("inf", inf.Value);

        host.StringChars = _ => "hi";
        var missing = Show(VmValueState1346Slice.String, VmBits1346.FromInt(0), host);
        Assert.Equal("string", missing.Type);
        Assert.Equal("<EMPTY>", missing.Value);
        Assert.Equal(0, host.StringReads);
        var blank = Show(VmValueState1346Slice.String, VmBits1346.FromInt(4), host);
        host.StringChars = _ => null;
        var emptyChars = Show(VmValueState1346Slice.String, VmBits1346.FromInt(4), host);
        Assert.Equal("\"\"", emptyChars.Value);
        Assert.Equal("\"hi\"", blank.Value);
        Assert.Equal(2, host.StringReads);

        var actor = new Type1346("Actor") { Descriptive = "Actor" };
        var classPtr = new Type1346("class*") { Pointer = true, ClassPointer = true, Pointed = actor };
        var classVar = Show(classPtr, VmBits1346.FromInt(0), host);
        Assert.Equal("ClassPointer", classVar.Type);
        Assert.Equal("Actor", classVar.Value);
        Assert.Equal(1, host.DescriptiveReads);

        var fnType = new Type1346("fn*") { Pointer = true, FunctionPointer = true, Descriptive = "Function<void>" };
        var fnNull = Show(fnType, VmBits1346.FromInt(0), host);
        Assert.Equal("Function<void>", fnNull.Type);
        Assert.Equal("<NULL>", fnNull.Value);
        host.FunctionAt = _ => new FuncInfo1346 { OwningClass = "Actor", Symbol = "Tick", HasVariant = true, Implementation = "Other" };
        var fnClass = Show(fnType, VmBits1346.FromInt(0x20), host);
        Assert.Equal("Actor.Tick (0x20)", fnClass.Value);
        host.FunctionAt = _ => new FuncInfo1346 { HasVariant = true, Implementation = "Printable" };
        var fnImpl = Show(fnType, VmBits1346.FromInt(0x20), host);
        Assert.Equal("Printable (0x20)", fnImpl.Value);
        host.FunctionAt = _ => new FuncInfo1346();
        var fnBare = Show(fnType, VmBits1346.FromInt(0x20), host);
        Assert.Equal("", fnBare.Value);

        var pointed = new Type1346("cell") { Descriptive = "int32", Scalar = true, Int = true };
        var ptr = new Type1346("ptr") { Pointer = true, Pointed = pointed };
        var high = new VmBits1346 { Raw = 0x100000000 };
        Assert.False(VmValueState1346Slice.Valid(high));
        var nullPtr = Show(ptr, high, host);
        Assert.Equal("Pointer<int32>", nullPtr.Type);
        Assert.Equal("<NULL>", nullPtr.Value);
        Assert.Equal(0, host.DerefReads);
        host.Deref = (addr, basic) =>
        {
            Assert.Equal(0x20UL, addr);
            Assert.Equal(VmValueState1346Slice.BasicNone, basic);
            return VmBits1346.FromInt(5);
        };
        var deref = Show(ptr, VmBits1346.FromInt(0x20), host);
        Assert.Equal("0x20 {5}", deref.Value);
        Assert.Equal(1, host.DerefReads);
        var blob = new Type1346("blob") { Descriptive = "Struct", Scalar = false };
        var blobPtr = new Type1346("blob*") { Pointer = true, Pointed = blob };
        var addr = Show(blobPtr, VmBits1346.FromInt(0x20), host);
        Assert.Equal("Pointer<Struct>", addr.Type);
        Assert.Equal("0x20", addr.Value);
        Assert.Equal(1, host.DerefReads);

        var odd = new Type1346("odd") { Descriptive = "Widget" };
        var error = Show(odd, VmBits1346.FromInt(1), host);
        Assert.Equal("Widget", error.Type);
        Assert.Equal("<ERROR?>", error.Value);
        Assert.Equal("NONE", VmValueState1346Slice.BasicNames[VmValueState1346Slice.BasicOf(odd)]);
    }

    [Fact]
    public void NamesColorsAndStates()
    {
        var host = new VmValueHost1346
        {
            NameChars = index => index == 3 || index == 7 ? "Spawn" : null,
            TextureName = index => index == 9 ? "STARTAN" : null,
            NumSounds = 4,
            SoundName = index => "world/shot",
            StateAt = (index, owner) =>
            {
                if (index == -5)
                {
                    return new StateHit1346
                    {
                        Found = true,
                        OwnerName = "DoomImp",
                        Owns = true,
                        Index = 1,
                        LabelCount = 3,
                        Label = "See",
                    };
                }

                return new StateHit1346 { Found = false, OwnerName = owner };
            },
        };

        var yes = Show(VmValueState1346Slice.Bool, VmBits1346.FromInt(2), host);
        Assert.Equal("bool", yes.Type);
        Assert.Equal("true", yes.Value);
        Assert.Equal("false", Show(VmValueState1346Slice.Bool, VmBits1346.FromInt(0), host).Value);
        Assert.Equal("Name# 3: 'Spawn'", Show(VmValueState1346Slice.Name, VmBits1346.FromInt(3), host).Value);
        Assert.Equal("Name", Show(VmValueState1346Slice.Name, VmBits1346.FromInt(3), host).Type);
        Assert.Equal("SpriteID# 4", Show(VmValueState1346Slice.Sprite, VmBits1346.FromInt(4), host).Value);
        Assert.Equal("SpriteID", Show(VmValueState1346Slice.Sprite, VmBits1346.FromInt(4), host).Type);
        Assert.Equal("TextureID# 9 (STARTAN)", Show(VmValueState1346Slice.Texture, VmBits1346.FromInt(9), host).Value);
        Assert.Equal("TextureID# 1 (<INVALID>)", Show(VmValueState1346Slice.Texture, VmBits1346.FromInt(1), host).Value);
        var trans = (2 << 16) | 40;
        Assert.Equal(
            "TranslationID# " + trans.ToString() + " {type: 2, index: 40}",
            Show(VmValueState1346Slice.Translation, VmBits1346.FromInt(trans), host).Value);
        Assert.Equal("TranslationID", Show(VmValueState1346Slice.Translation, VmBits1346.FromInt(trans), host).Type);
        Assert.Equal("Sound# 3 (world/shot)", Show(VmValueState1346Slice.Sound, VmBits1346.FromInt(3), host).Value);
        Assert.Equal("Sound# 0 (<INVALID>)", Show(VmValueState1346Slice.Sound, VmBits1346.FromInt(0), host).Value);
        Assert.Equal("Sound# 4 (<INVALID>)", Show(VmValueState1346Slice.Sound, VmBits1346.FromInt(4), host).Value);
        Assert.Equal(
            "Color #A1B2C3D4 (a: 161, r: 178, g: 195, b: 212)",
            Show(VmValueState1346Slice.Color, VmBits1346.FromInt(unchecked((int)0xA1B2C3D4)), host).Value);

        Assert.Equal(
            "StateLabel# 7: 'DoomImp.Spawn'",
            Show(VmValueState1346Slice.StateLabel, VmBits1346.FromInt(7), host, "DoomImp").Value);
        Assert.Equal(
            "StateLabel# 7: '<unknownOwner>.Spawn'",
            Show(VmValueState1346Slice.StateLabel, VmBits1346.FromInt(7), host).Value);
        Assert.Equal(
            "StateLabel# -5: 'DoomImp.See'",
            Show(VmValueState1346Slice.StateLabel, VmBits1346.FromInt(-5), host, "DoomImp").Value);
        var skipped = new VmValueHost1346
        {
            NameChars = _ => "No",
            StateAt = (_, _) => new StateHit1346 { Found = true, OwnerName = "DoomImp", Owns = false, Index = 0, LabelCount = 2, Label = "See" },
        };
        Assert.Equal("StateLabel# -5: '<unknown_state>'", Show(VmValueState1346Slice.StateLabel, VmBits1346.FromInt(-5), skipped, "DoomImp").Value);

        var enumerated = new Type1346("enum") { IntNotInt = true, Size = 4, Descriptive = "EnumCount" };
        var enumVar = Show(enumerated, VmBits1346.FromInt(6), host);
        Assert.Equal("Enum", enumVar.Type);
        Assert.Equal("6", enumVar.Value);
        var plain = new Type1346("count") { IntNotInt = true, Size = 4, Descriptive = "Count" };
        Assert.Equal("Enum", Show(plain, VmBits1346.FromInt(-3), host).Type);
        Assert.Equal("-3", Show(plain, VmBits1346.FromInt(-3), host).Value);

        var node = new VmValueStateNode1346("health", VmBits1346.FromInt(5), VmValueState1346Slice.SInt32);
        var variable = new DapVar1346 { EvaluateName = "stale" };
        Assert.True(node.Serialize(variable));
        Assert.Equal("health", variable.Name);
        Assert.Equal("stale", variable.EvaluateName);
        Assert.Equal("int32", variable.Type);
        Assert.Equal("5", variable.Value);
        var aliased = new VmValueStateNode1346("health", VmBits1346.FromInt(5), VmValueState1346Slice.SInt32, evalName: "self.health");
        var named = new DapVar1346();
        Assert.True(aliased.Serialize(named));
        Assert.Equal("health", named.Name);
        Assert.Equal("self.health", named.EvaluateName);
    }

    static DapVar1346 Show(Type1346 type, VmBits1346 bits, VmValueHost1346 host, string? owner = null)
    {
        return VmValueStateNode1346.ToVariable(bits, type, owner, host);
    }
}
