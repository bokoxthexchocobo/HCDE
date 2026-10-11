namespace HCDE.Client.Tests;

public class VmSearchPathExpansionTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("plain/~/%NAME%", "plain/~/%NAME%")]
    [InlineData("$NAME/$OTHER", "value/other")]
    [InlineData("$MISSING/tail", "/tail")]
    [InlineData("$NAME_suffix", "")]
    [InlineData("$$NAME/${NAME}/$", "$value/${NAME}/$")]
    [InlineData("$PrOgDiR/data", "engine/data")]
    [InlineData("$NAME-$OTHER", "value-other")]
    [InlineData("$NAME/é", "value/é")]
    public void NativeVariableGrammarAndNonRecursiveSubstitution(string? input, string expected)
    {
        Assert.Equal(expected, VmSearchPathExpansion.ExpandEnvironment(input,
            name => name switch { "NAME" => "value", "OTHER" => "other", _ => null }, "engine"));
        Assert.Equal("$OTHER", VmSearchPathExpansion.ExpandEnvironment("$NAME", _ => "$OTHER"));
    }

    [Fact]
    public void DirectoryExpansionSkipsUnsetPathsAndPreservesProgramDirectory()
    {
        var missing = "HCDE_UNSET_" + Guid.NewGuid().ToString("N");
        Assert.Equal(new[] { AppContext.BaseDirectory }, VmSearchPathExpansion.ExpandDirectories(["$" + missing, "$progdir"]));
        Assert.Throws<ArgumentException>(() => VmSearchPathExpansion.ExpandDirectories([" "]));
    }
}
