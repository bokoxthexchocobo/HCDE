using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptVariableTests
{
    [Fact] public void IntValue_FromString() =>
        Assert.Equal(42, VmFScriptVariable.IntValue(new() { Type = VmFScriptVariable.SvtString, String = "42" }));

    [Fact] public void FixedValue_FromInt() =>
        Assert.Equal(131072, VmFScriptVariable.FixedValue(new() { Type = VmFScriptVariable.SvtInt, IntValue = 2 }));

    [Fact] public void FloatValue_FromFixed() =>
        Assert.Equal(1.0, VmFScriptVariable.FloatValue(new() { Type = VmFScriptVariable.SvtFixed, FixedBits = 65536 }));

    [Fact] public void StringValue_FromInt() =>
        Assert.Equal("9", VmFScriptVariable.StringValue(new() { Type = VmFScriptVariable.SvtInt, IntValue = 9 }, new()));

    [Fact] public void ActorValue_MapThingIndex()
    {
        var mo = new object();
        var host = new VmFScriptVariable.VariableHost { SpawnedThings = new List<object?> { mo } };
        Assert.Same(mo, VmFScriptVariable.ActorValue(host, new() { Type = VmFScriptVariable.SvtInt }));
    }

    [Fact] public void NewVariable_ChainsInHash()
    {
        var s = new VmFScriptScript.FsScript();
        var v = VmFScriptVariable.NewVariable(s, "foo", VmFScriptVariable.SvtInt);
        Assert.Same(v, s.Variables[VmFScriptVariable.VariableHash("foo")]);
    }

    [Fact] public void GetValue_CopiesString()
    {
        var v = new VmFScriptScript.FsVariable { Type = VmFScriptVariable.SvtString, String = "x" };
        var outv = new VmFScriptVariable.ScriptValue();
        VmFScriptVariable.GetValue(v, outv);
        Assert.Equal("x", outv.String);
    }

    [Fact] public void SetValue_AdaptsConst()
    {
        var v = new VmFScriptScript.FsVariable { Type = VmFScriptVariable.SvtConst };
        VmFScriptVariable.SetValue(new(), v, new() { Type = VmFScriptVariable.SvtInt, IntValue = 3 });
        Assert.Equal(3, v.IntValue);
    }

    [Fact] public void Serialize_RoundTripsName() =>
        Assert.Equal("ammo", new VmFScriptScript.FsVariable { Name = "ammo" }.Name);

    [Fact] public void NewFunction_StoresHandler()
    {
        var s = new VmFScriptScript.FsScript();
        VmFScriptVariable.NewFunction(s, "fn", () => { });
        Assert.NotNull(VmFScriptVariable.VariableForName(s, "fn")?.FunctionHandler);
    }

    [Fact] public void VariableForName_FindsChain()
    {
        var s = new VmFScriptScript.FsScript();
        VmFScriptVariable.NewVariable(s, "a", VmFScriptVariable.SvtInt);
        Assert.NotNull(VmFScriptVariable.VariableForName(s, "a"));
    }

    [Fact] public void FindVariable_WalksParentAndGlobal()
    {
        var g = new VmFScriptScript.FsScript();
        VmFScriptVariable.NewVariable(g, "g", VmFScriptVariable.SvtInt);
        var child = new VmFScriptScript.FsScript();
        Assert.Same(g.Variables[VmFScriptVariable.VariableHash("g")], VmFScriptVariable.FindVariable(child, "g", g));
    }

    [Fact] public void ClearVariables_StopsAtLabels()
    {
        var n = 0;
        var s = new VmFScriptScript.FsScript();
        var label = VmFScriptVariable.NewVariable(s, "l", VmFScriptScript.SvtLabel);
        var temp = VmFScriptVariable.NewVariable(s, "t", VmFScriptVariable.SvtInt);
        label.Next = temp;
        s.Variables[VmFScriptVariable.VariableHash("l")] = label;
        VmFScriptVariable.ClearVariables(s, false, _ => n++);
        Assert.Equal(1, n);
    }

    [Fact] public void LabelValue_ReturnsIndex()
    {
        var s = new VmFScriptScript.FsScript();
        Assert.Equal(12, VmFScriptVariable.LabelValue(s, new() { Type = VmFScriptScript.SvtLabel, IntValue = 12 }));
    }
}
