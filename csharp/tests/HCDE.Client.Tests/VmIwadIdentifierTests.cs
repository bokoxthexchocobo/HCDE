namespace HCDE.Client.Tests;

public class VmIwadIdentifierTests
{
    [Fact]
    public void FirstCompleteDefinitionWinsAndEveryRequirementIsNeeded()
    {
        var specific = Definition("specific", ["MAP01", "SPECIAL"]);
        var generic = Definition("generic", ["MAP01"]);
        Assert.Same(generic, VmIwadIdentifier.Identify([specific, generic], [new("map01"), new("map01")]));
        Assert.Same(specific, VmIwadIdentifier.Identify([specific, generic], [new("MAP01"), new("special")]));
        Assert.Same(generic, VmIwadIdentifier.Identify([generic, specific], [new("MAP01"), new("SPECIAL")]));
        Assert.Null(VmIwadIdentifier.Identify([specific, generic], [new("E1M1")]));
    }

    [Theory]
    [InlineData("MAPS/map01.wad", "MAP01")]
    [InlineData("maps/e1m1.map.backup", "E1M1")]
    [InlineData("maps/noextension", "noextension")]
    [InlineData("maps/nested/map01.wad", "nested/map01")]
    public void MapPathUsesEverythingBeforeFirstDotAfterMapsPrefix(string fullName, string expected)
    {
        var definition = Definition("map", [expected]);
        Assert.Same(definition, VmIwadIdentifier.Identify([definition], [new("", fullName)]));
        Assert.Null(VmIwadIdentifier.Identify([definition], [new("", "other/" + fullName)]));
    }

    [Fact]
    public void DefinitionsSnapshotRequirementsAndDoNotLeakMatchesBetweenScans()
    {
        string[] required = ["MAP01", "MAP01"];
        var definition = Definition("game", required);
        required[0] = "CHANGED";
        Assert.Same(definition, VmIwadIdentifier.Identify([definition], [new("map01")]));
        Assert.Null(VmIwadIdentifier.Identify([definition], []));
        Assert.Null(VmIwadIdentifier.Identify([], [new("MAP01")]));
        var unconstrained = Definition("unconstrained", []);
        Assert.Same(unconstrained, VmIwadIdentifier.Identify([unconstrained], []));
        var many = Definition("many", Enumerable.Range(0, 40).Select(index => "L" + index).ToArray());
        Assert.Same(many, VmIwadIdentifier.Identify([many], many.MustContain.Select(name => new VmIwadResourceName(name)).ToArray()));
    }

    private static VmIwadDefinition Definition(string name, IReadOnlyList<string> required) =>
        new(name, VmStartupGameType.Doom, "doom.id.doom2", required);
}
