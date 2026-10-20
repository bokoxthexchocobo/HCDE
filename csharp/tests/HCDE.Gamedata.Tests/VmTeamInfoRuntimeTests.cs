namespace HCDE.Gamedata.Tests;

public class VmTeamInfoRuntimeTests
{
    [Fact]
    public void TryChangeTeam_RejectsSinglePlayer()
    {
        Assert.False(VmTeamInfoRuntime.TryChangeTeam(false, true, 0, true, 0, 1, 2));
    }

    [Fact]
    public void GetTeamName_ReturnsDefinitionName()
    {
        Assert.Equal("Blue", VmTeamInfoRuntime.GetTeamName(new VmTeamDefinition { Name = "Blue" }));
    }
}
