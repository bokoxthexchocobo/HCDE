namespace HCDE.Gamedata.Tests;

public class VmTeamInfoParserTests
{
    [Fact]
    public void ParseTeam_RequiresTwoTeams()
    {
        var tokens = VmLockDefTokenize.Tokenize("Team Red { Game Any Logo R }");
        var i = 0;
        var teams = new List<VmTeamDefinition>();
        var errors = new List<string>();
        Assert.False(VmTeamInfoParser.TryParseDocument(tokens, ref i, teams, "Doom", errors));
        Assert.Contains("two teams", errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseTeam_ReadsOptions()
    {
        var text = "Team Red { Game Any PlayerColor #ff0000 Logo LOGO AllowCustomPlayerColor } Team Blue { Game doom Logo B }";
        var tokens = VmLockDefTokenize.Tokenize(text);
        var i = 0;
        var teams = new List<VmTeamDefinition>();
        var errors = new List<string>();
        Assert.True(VmTeamInfoParser.TryParseDocument(tokens, ref i, teams, "Doom", errors), string.Join("; ", errors));
        Assert.Equal(2, teams.Count);
        Assert.True(teams[0].AllowCustomPlayerColor);
    }
}
