namespace HCDE.Playsim.Tests;

public class VmCVarConvert1332SliceTests
{
    [Fact]
    public void Convert_ParsesBoolIntFloatAndString()
    {
        var w = new VmCVarConvert1332();
        var on = new VmCVarConvert1332.Value { Bool = true, Int = -4, Float = -1.9f, Text = "TRUE" };
        Assert.True(w.ToBool(on, VmCVarConvert1332.Bool));
        Assert.True(w.ToBool(on, VmCVarConvert1332.Color));
        Assert.True(w.ToBool(on, VmCVarConvert1332.Int));
        Assert.True(w.ToBool(on, VmCVarConvert1332.Float));
        Assert.True(w.ToBool(on, VmCVarConvert1332.String));
        Assert.False(w.ToBool(on, VmCVarConvert1332.Flag));

        var off = new VmCVarConvert1332.Value { Float = -0f, Text = "  true" };
        Assert.False(w.ToBool(off, VmCVarConvert1332.Float));
        Assert.False(w.ToBool(off, VmCVarConvert1332.String));
        Assert.False(w.ToBool(new VmCVarConvert1332.Value { Text = "falsehood" }, VmCVarConvert1332.String));
        Assert.True(w.ToBool(new VmCVarConvert1332.Value { Text = "010" }, VmCVarConvert1332.String));
        Assert.True(w.ToBool(new VmCVarConvert1332.Value { Text = "0x1" }, VmCVarConvert1332.String));
        Assert.False(w.ToBool(new VmCVarConvert1332.Value { Text = "0" }, VmCVarConvert1332.String));
        Assert.True(w.ToBool(new VmCVarConvert1332.Value { Float = float.NaN }, VmCVarConvert1332.Float));

        Assert.Equal(1, w.ToInt(on, VmCVarConvert1332.Bool));
        Assert.Equal(-4, w.ToInt(on, VmCVarConvert1332.Color));
        Assert.Equal(-1, w.ToInt(on, VmCVarConvert1332.Float));
        Assert.Equal(0, w.ToInt(new VmCVarConvert1332.Value { Text = "FALSE" }, VmCVarConvert1332.String));
        Assert.Equal(8, w.ToInt(new VmCVarConvert1332.Value { Text = "010" }, VmCVarConvert1332.String));
        Assert.Equal(1, w.ToInt(new VmCVarConvert1332.Value { Text = "019" }, VmCVarConvert1332.String));
        Assert.Equal(16, w.ToInt(new VmCVarConvert1332.Value { Text = "0X10" }, VmCVarConvert1332.String));
        Assert.Equal(-16, w.ToInt(new VmCVarConvert1332.Value { Text = "-0x10" }, VmCVarConvert1332.String));
        Assert.Equal(42, w.ToInt(new VmCVarConvert1332.Value { Text = "  +42abc" }, VmCVarConvert1332.String));
        Assert.Equal(-1, w.ToInt(new VmCVarConvert1332.Value { Text = "0xFFFFFFFF" }, VmCVarConvert1332.String));
        Assert.Equal(-1, w.ToInt(new VmCVarConvert1332.Value { Text = "999999999999999999999" }, VmCVarConvert1332.String));
        Assert.Equal(0, w.ToInt(new VmCVarConvert1332.Value { Text = "-999999999999999999999" }, VmCVarConvert1332.String));
        Assert.Equal(0, w.ToInt(on, VmCVarConvert1332.Dummy));
        Assert.Equal(1, w.ToInt(new VmCVarConvert1332.Value { Float = 1.9f }, VmCVarConvert1332.Float));

        Assert.Equal(1f, w.ToFloat(on, VmCVarConvert1332.Bool));
        Assert.Equal(-4f, w.ToFloat(on, VmCVarConvert1332.Int));
        Assert.Equal(0f, w.ToFloat(new VmCVarConvert1332.Value { Text = "true" }, VmCVarConvert1332.String));
        Assert.Equal(100f, w.ToFloat(new VmCVarConvert1332.Value { Text = "1e2" }, VmCVarConvert1332.String));
        Assert.Equal(3f, w.ToFloat(new VmCVarConvert1332.Value { Text = "0x1.8p1" }, VmCVarConvert1332.String));
        Assert.Equal(0.15f, w.ToFloat(new VmCVarConvert1332.Value { Text = "  1.5e-1xyz" }, VmCVarConvert1332.String));
        Assert.True(float.IsPositiveInfinity(w.ToFloat(new VmCVarConvert1332.Value { Text = " INF" }, VmCVarConvert1332.String)));
        Assert.True(float.IsNaN(w.ToFloat(new VmCVarConvert1332.Value { Text = "-nan" }, VmCVarConvert1332.String)));
        Assert.Equal(0f, w.ToFloat(on, VmCVarConvert1332.Mask));

        Assert.Equal("true", w.ToStringRep(on, VmCVarConvert1332.Bool));
        Assert.Equal("", w.Buffer);
        Assert.Equal("12", w.ToStringRep(new VmCVarConvert1332.Value { Int = 12 }, VmCVarConvert1332.Int));
        Assert.Equal("12", w.Buffer);
        Assert.Equal("hi", w.ToStringRep(new VmCVarConvert1332.Value { Text = "hi" }, VmCVarConvert1332.String));
        Assert.Equal("12", w.Buffer);
        Assert.Equal("1.5", w.ToStringRep(new VmCVarConvert1332.Value { Float = 1.5f }, VmCVarConvert1332.Float));
        Assert.Equal("1.5", w.Buffer);
        Assert.Equal("1e+06", w.ToStringRep(new VmCVarConvert1332.Value { Float = 1000000f }, VmCVarConvert1332.Float));
        Assert.Equal("1000000", w.ToStringRep(new VmCVarConvert1332.Value { Int = 1000000 }, VmCVarConvert1332.Color));
        Assert.Equal("<huh?>", w.ToStringRep(on, VmCVarConvert1332.Flag));
        Assert.Equal("-0", VmCVarConvert1332.FormatG(-0f));
        Assert.Equal("nan", VmCVarConvert1332.FormatG(BitConverter.Int32BitsToSingle(0x7fc00000)));
        Assert.Equal("-nan", VmCVarConvert1332.FormatG(BitConverter.Int32BitsToSingle(unchecked((int)0xffc00000))));
        Assert.Equal("inf", VmCVarConvert1332.FormatG(float.PositiveInfinity));
        Assert.Equal("-inf", VmCVarConvert1332.FormatG(float.NegativeInfinity));

        Assert.True(w.FromBool(true, VmCVarConvert1332.Bool).Bool);
        Assert.Equal(0, w.FromBool(true, VmCVarConvert1332.Color).Int);
        Assert.Equal("true", w.FromBool(true, VmCVarConvert1332.String).Text);
        Assert.Equal("<huh?>", w.Buffer);
        Assert.Equal("7", w.FromInt(7, VmCVarConvert1332.String).Text);
        Assert.Equal("7", w.Buffer);
        Assert.Equal(1.5f, w.FromFloat(1.5f, VmCVarConvert1332.Float).Float);
        Assert.Equal(-1, w.FromFloat(-1.9f, VmCVarConvert1332.Int).Int);
        Assert.False(w.FromFloat(-0f, VmCVarConvert1332.Bool).Bool);
        Assert.Equal(0, w.FromFloat(1.5f, VmCVarConvert1332.Color).Int);
        Assert.Equal("1.25", w.FromFloat(1.25f, VmCVarConvert1332.String).Text);
        Assert.Equal(3, w.FromString("  3", VmCVarConvert1332.Int).Int);
        Assert.False(w.FromString("  true", VmCVarConvert1332.Bool).Bool);
        Assert.Equal("stay", w.FromString("stay", VmCVarConvert1332.String).Text);
        Assert.Equal("1.25", w.Buffer);
        Assert.Equal(0, w.FromString("9", VmCVarConvert1332.Color).Int);
        Assert.Equal(0, w.FromInt(5, VmCVarConvert1332.Dummy).Int);

        Assert.Equal(0, (int)VmCVarConvert1332.HexToByte("A0"));
        Assert.Equal(16, (int)VmCVarConvert1332.HexToByte("1A"));
        Assert.Equal(85, (int)VmCVarConvert1332.HexToByte("ff"));
        Assert.Equal(5, (int)VmCVarConvert1332.HexToByte("0F"));
        Assert.Equal(153, (int)VmCVarConvert1332.HexToByte("99"));
        Assert.Equal(16, (int)VmCVarConvert1332.HexToByte("10ff"));
        Assert.Equal(0L, VmCVarConvert1332.ParseCLong("0x"));
        Assert.Equal(0L, VmCVarConvert1332.ParseCLong("-"));
        Assert.Equal(0d, VmCVarConvert1332.ParseCDouble("."));
        Assert.Equal(16d, VmCVarConvert1332.ParseCDouble("0x10"));
    }

    [Fact]
    public void Gate_SetsNamedCvarsAndWalksCallbacks()
    {
        var w = new VmCVarConvert1332();
        Assert.Null(w.CvarSet(null, "1"));
        Assert.Null(w.CvarSet("", "1"));
        Assert.Null(w.CvarForceSet("missing", "1"));

        VmCVarConvert1332.Var gravity = w.Add("sv_gravity", 0, true);
        VmCVarConvert1332.Var quiet = w.Add("nomonsters", VmCVarConvert1332.NoInitCall, true);
        VmCVarConvert1332.Var custom = w.Add("mod_speed", VmCVarConvert1332.ZsCustom | VmCVarConvert1332.NoInitCall, false);
        w.Add("plain", 0, false);
        Assert.Throws<InvalidOperationException>(() => w.Add("SV_Gravity", 0, false));

        VmCVarConvert1332.Var? set = w.CvarSet("SV_Gravity", "800");
        Assert.Same(gravity, set);
        Assert.Equal(new string?[] { "800" }, gravity.GenericSets);
        Assert.Empty(gravity.ForceSets);
        Assert.Null(w.CvarSet("nope", "1"));

        Assert.Same(quiet, w.CvarForceSet("NoMonsters", null));
        Assert.Equal(new string?[] { null }, quiet.ForceSets);
        Assert.Empty(quiet.GenericSets);

        Assert.False(w.DoNoSet);
        w.EnableNoSet();
        w.EnableNoSet();
        Assert.True(w.DoNoSet);

        Assert.False(w.UseCallback);
        w.EnableCallbacks();
        Assert.True(w.UseCallback);
        Assert.Equal(new[] { "sv_gravity", "plain" }, w.CallbackOrder);
        Assert.Equal(1, gravity.UserFires);
        Assert.Equal(0, quiet.UserFires);
        Assert.Equal(0, custom.UserFires);

        w.InitZSCallbacks();
        w.InitZSCallbacks();
        Assert.Equal(new[] { "mod_speed", "mod_speed" }, w.ZsInits);
        Assert.Equal(new[] { "MarkZSCallbacks", "MarkZSCallbacks" }, w.Markers);
        w.MarkZSCallbacks();
        Assert.Equal(new[] { "mod_speed" }, w.ZsMarks);

        w.DisableCallbacks();
        Assert.False(w.UseCallback);
        Assert.True(w.DoNoSet);
        Assert.Equal(2, w.CallbackOrder.Count);
    }
}
