namespace HCDE.Scripting.Tests;

public class VmCVarScope1375SliceTests
{
    [Fact]
    public void ToVariableFormatsTypeValueAndDescription()
    {
        var host = new VmCVarScope1375Slice();
        var missing = host.ToVariable(null);
        Assert.Equal("<INVALID>", missing.Name);
        Assert.Equal("<INVALID>", missing.Value);
        Assert.Equal("", missing.Type);

        var plain = host.ToVariable(new CVar1375
        {
            Name = "sv_cheats",
            Human = "true",
            RealType = VmCVarScope1375Slice.BoolType,
        });
        Assert.Equal("sv_cheats", plain.Name);
        Assert.Equal("true", plain.Value);
        Assert.Equal("CVar<Bool>", plain.Type);

        Assert.Equal("10.0", host.ToVariable(Float("10")).Value);
        Assert.Equal("1.5", host.ToVariable(Float("1.5")).Value);
        Assert.Equal("1e2", host.ToVariable(Float("1e2")).Value);
        Assert.Equal("1E2.0", host.ToVariable(Float("1E2")).Value);
        Assert.Equal("CVar<Float>", host.ToVariable(Float("10")).Type);

        var quoted = host.ToVariable(new CVar1375
        {
            Name = "name",
            Human = "doom",
            RealType = VmCVarScope1375Slice.StringType,
            FavoriteType = VmCVarScope1375Slice.StringType,
        });
        Assert.Equal("\"doom\"", quoted.Value);
        Assert.Equal("CVar<String>", quoted.Type);
        var unquoted = host.ToVariable(new CVar1375
        {
            Name = "name",
            Human = "doom",
            RealType = VmCVarScope1375Slice.StringType,
            FavoriteType = VmCVarScope1375Slice.IntType,
        });
        Assert.Equal("doom", unquoted.Value);

        Assert.Equal("CVar<Int>", VmCVarScope1375Slice.RealTypeName(VmCVarScope1375Slice.IntType));
        Assert.Equal("CVar<Color>", VmCVarScope1375Slice.RealTypeName(VmCVarScope1375Slice.ColorType));
        Assert.Equal("CVar<Flag>", VmCVarScope1375Slice.RealTypeName(VmCVarScope1375Slice.FlagType));
        Assert.Equal("CVar<Mask>", VmCVarScope1375Slice.RealTypeName(VmCVarScope1375Slice.MaskType));
        Assert.Equal("CVar<Dummy>", VmCVarScope1375Slice.RealTypeName(VmCVarScope1375Slice.DummyType));
        Assert.Equal("<UNKNOWN>", VmCVarScope1375Slice.RealTypeName(99));

        host.Localize = text => text == "$FOO" ? "Health" : text == "$SAME" ? "X$SAME" : "";
        var shown = host.ToVariable(new CVar1375
        {
            Name = "health",
            Human = "100",
            RealType = VmCVarScope1375Slice.IntType,
            Description = "$FOO",
        });
        Assert.Equal("CVar<Int> (Health)", shown.Type);
        var hidden = host.ToVariable(new CVar1375
        {
            Name = "same",
            Human = "1",
            RealType = VmCVarScope1375Slice.IntType,
            Description = "$SAME",
        });
        Assert.Equal("CVar<Int>", hidden.Type);
        var emptyLookup = host.ToVariable(new CVar1375
        {
            Name = "empty",
            Human = "0",
            RealType = VmCVarScope1375Slice.IntType,
            Description = "$MISS",
        });
        Assert.Equal("CVar<Int>", emptyLookup.Type);

        var raw = new VmCVarScope1375Slice
        {
            Localize = text => text,
        };
        Assert.Equal(
            "CVar<Int> (health)",
            raw.ToVariable(new CVar1375
            {
                Name = "n",
                Human = "1",
                RealType = VmCVarScope1375Slice.IntType,
                Description = "health",
            }).Type);
        Assert.Equal("ealth", VmCVarScope1375Slice.Tail("health"));
        Assert.Equal("", VmCVarScope1375Slice.Tail(""));

        var needs = new VmCVarScope1375Slice();
        Assert.Throws<InvalidOperationException>(() => needs.ToVariable(new CVar1375
        {
            Description = "x",
            RealType = VmCVarScope1375Slice.IntType,
        }));

        var node = new CVar1375Node
        {
            CVar = new CVar1375 { Name = "flag", Human = "on", RealType = VmCVarScope1375Slice.FlagType },
        };
        Assert.True(node.SerializeToProtocol(host, out var serialized));
        Assert.Equal("flag", serialized.Name);
        Assert.Equal("CVar<Flag>", serialized.Type);
    }

    [Fact]
    public void ScopeListsChildrenAndFindsLateCvars()
    {
        var host = new VmCVarScope1375Slice();
        host.Map.Add(new CVar1375 { Name = "Beta", Human = "1", RealType = VmCVarScope1375Slice.IntType });
        host.Map.Add(new CVar1375 { Name = "alpha", Human = "2", RealType = VmCVarScope1375Slice.IntType });
        host.Map.Add(new CVar1375 { Name = "aa", Human = "3", RealType = VmCVarScope1375Slice.IntType });
        host.Map.Add(new CVar1375 { Name = "Gamma", Human = "4", RealType = VmCVarScope1375Slice.IntType });
        var scope = new CVar1375Scope { Id = 17 };
        Assert.True(scope.SerializeToProtocol(host, out var protocol));
        Assert.Equal("CVars", protocol.Name);
        Assert.True(protocol.Expensive);
        Assert.Equal("cvars", protocol.PresentationHint);
        Assert.Equal(17, protocol.VariablesReference);
        Assert.Equal(0, protocol.IndexedVariables);
        Assert.Equal(4, protocol.NamedVariables);
        Assert.Equal(new[] { "aa", "alpha", "Beta", "Gamma" }, scope.CachedNames);
        Assert.Equal(new[] { "aa", "alpha", "Beta", "Gamma" }, scope.GetChildNames(host));
        Assert.Equal(4, scope.Children.Count);

        Assert.True(scope.GetChildNode("ALPHA", host, out var alpha));
        Assert.Equal("alpha", alpha!.CVar!.Name);
        Assert.Equal("2", host.ToVariable(alpha.CVar).Value);

        var asked = 0;
        host.FindCVar = name =>
        {
            asked++;
            Assert.Equal("sv_cheats", name);
            return new CVar1375 { Name = "SV_Cheats", Human = "false", RealType = VmCVarScope1375Slice.BoolType };
        };
        Assert.True(scope.GetChildNode("sv_cheats", host, out var late));
        Assert.Equal(1, asked);
        Assert.Equal("SV_Cheats", late!.CVar!.Name);
        Assert.Equal(new[] { "aa", "alpha", "Beta", "Gamma", "SV_Cheats" }, scope.CachedNames);
        Assert.True(scope.GetChildNode("SV_CHEATS", host, out var again));
        Assert.Same(late, again);
        Assert.Equal(1, asked);

        host.FindCVar = _ => null;
        Assert.False(scope.GetChildNode("missing", host, out var none));
        Assert.Null(none);

        Assert.Equal(-1, VmCVarScope1375Slice.CompareNames("alpha", "Beta"));
        Assert.Equal(1, VmCVarScope1375Slice.CompareNames("Beta", "alpha"));
        Assert.Equal(0, VmCVarScope1375Slice.CompareNames("aa", "aa"));
        Assert.True(VmCVarScope1375Slice.CompareNames("", "A") < 0);

        var second = new CVar1375Scope { Id = 3 };
        Assert.True(second.SerializeToProtocol(host, out var againScope));
        Assert.Equal(4, againScope.NamedVariables);
        Assert.Equal(4, second.Children.Count);
    }

    static CVar1375 Float(string human) => new()
    {
        Name = "f",
        Human = human,
        RealType = VmCVarScope1375Slice.FloatType,
    };
}
