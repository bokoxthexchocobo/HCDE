namespace HCDE.Client.Tests;

public class VmDMainParseCVarInfoTests
{
    [Fact]
    public void NormalizeConfigOnlyFlags_ClearsServerUser()
    {
        var flags = VmDMainParseCVarInfo.CvarServerInfo | VmDMainParseCVarInfo.CvarConfigOnly;
        var normalized = VmDMainParseCVarInfo.NormalizeConfigOnlyFlags(flags);
        Assert.Equal(VmDMainParseCVarInfo.CvarConfigOnly, normalized);
    }

    [Fact]
    public void HasValidScopeFlags_RejectsMissingScope()
    {
        Assert.False(VmDMainParseCVarInfo.HasValidScopeFlags(VmDMainParseCVarInfo.CvarModArchive));
    }

    [Fact]
    public void ApplyAttributeFlag_Server_SetsServerInfo()
    {
        var flags = VmDMainParseCVarInfo.ApplyAttributeFlag("server", VmDMainParseCVarInfo.CvarModArchive, out _, out _);
        Assert.True((flags & VmDMainParseCVarInfo.CvarServerInfo) != 0);
    }

    [Fact]
    public void IsPatchExtension_MatchesDeh()
    {
        Assert.True(VmDMainParseCVarInfo.IsPatchExtension("foo.deh"));
    }

    [Fact]
    public void TryParseDefinition_ServerBoolWithDefault()
    {
        var tokens = new[] { "server", "bool", "mod_test", "=", "true", ";" };
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var index = 0;
        var ok = VmDMainParseCVarInfo.TryParseDefinition(tokens, ref index, names, out var def, out var error);
        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal("mod_test", def.Name);
        Assert.Equal("true", def.DefaultLiteral);
    }

    [Fact]
    public void ParseCvarInfoLump_RejectsDuplicateName()
    {
        var tokens = new[] { "user", "int", "dup", "=", "1", ";", "nosave", "int", "dup", ";" };
        var sink = new List<VmDMainParseCVarInfo.CvarInfoDefinition>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var added = VmDMainParseCVarInfo.ParseCvarInfoLump(tokens, sink, names);
        Assert.Equal(1, added);
        Assert.Single(sink);
    }

    [Fact]
    public void ResolvePatchLoads_PrefersDehThenBex()
    {
        var loads = VmDMainParseCVarInfo.ResolvePatchLoads(
            new[] { "patch" },
            _ => "/w/patch.deh",
            _ => null);
        Assert.Equal("/w/patch.deh", loads[0]);
    }
}
