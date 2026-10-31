namespace HCDE.Gamedata.Tests;

public class VmTeamNative1381SliceTests
{
    [Fact]
    public void ValidityChangeAndGetters()
    {
        var slice = new VmTeamNative1381Slice();
        Assert.Equal(64, VmTeamNative1381Slice.MaxPlayers);
        Assert.Equal(11, VmTeamNative1381Slice.CrUntranslated);
        Assert.False(slice.IsValid(0));
        slice.Teams.Add(new VmTeamNative1381Slice.Team { Name = "Red" });
        slice.Teams.Add(new VmTeamNative1381Slice.Team { Name = "" });
        Assert.True(slice.IsValid(1));
        Assert.False(slice.IsValid(2));

        slice.Teamplay = true;
        slice.InGame[0] = true;
        Assert.False(slice.ChangeTeam(0, 0));
        Assert.Empty(slice.Trace);
        slice.Multiplayer = true;
        Assert.False(slice.ChangeTeam(64, 0));
        Assert.False(slice.ChangeTeam(0, 2));
        slice.CurrentTeam[0] = -1;
        Assert.False(slice.ChangeTeam(0, uint.MaxValue));
        Assert.Empty(slice.Trace);
        Assert.True(slice.ChangeTeam(0, 1));
        Assert.Equal(["changed 0 1", "translation 0"], slice.Trace);
        Assert.Equal(1, slice.CurrentTeam[0]);
        Assert.False(slice.ChangeTeam(0, 1));
        Assert.Equal(2, slice.Trace.Count);

        var red = slice.Teams[0];
        Assert.Equal("Red", slice.GetName(red));
        Assert.Equal(0, slice.GetPlayerColor(red));
        red.PlayerColor = 4;
        Assert.Equal(4, slice.GetPlayerColor(red));
        Assert.Equal(11, slice.GetTextColor(red));
        red.TextColor = "$MISSING";
        slice.ParseFontColor = (_, normal, bold) =>
        {
            Assert.Equal(0, normal);
            Assert.Equal(0, bold);
            return -1;
        };
        Assert.Equal(11, slice.GetTextColor(red));
        Assert.Equal(
            "GetTextColor: Undefined color '$MISSING' in definition of team 'Red'.\n",
            Assert.Single(slice.Lines));
        red.TextColor = "Red";
        slice.ParseFontColor = (text, _, _) => text == "Red" ? 6 : -1;
        Assert.Equal(6, slice.GetTextColor(red));
        Assert.False(slice.GetAllowCustomPlayerColor(red));
        red.AllowCustomPlayerColor = true;
        Assert.True(slice.GetAllowCustomPlayerColor(red));

        slice.CheckTexture = (_, _) => throw new InvalidOperationException("texture");
        Assert.Equal(-1, slice.LogoIndex(red));
        red.Logo = "FLAG";
        slice.CheckTexture = (name, type) =>
        {
            Assert.Equal("FLAG", name);
            Assert.Equal(0, type);
            return 42;
        };
        Assert.Equal(42, slice.LogoIndex(red));

        slice.Lines.Clear();
        slice.TeamList();
        Assert.Equal(
            ["Defined teams are as follows:\n", "0 : Red\n", "1 : \n", "End of team list.\n"],
            slice.Lines);
        Assert.Equal("teamlist", VmTeamNative1381Slice.TeamListCommand);
    }

    [Fact]
    public void NativesEvaluateBeforeTheReturnCount()
    {
        var slice = new VmTeamNative1381Slice
        {
            Multiplayer = true,
            Teamplay = true,
            ParseFontColor = (_, _, _) => -1,
            CheckTexture = (_, _) => 3,
        };
        slice.Teams.Add(new VmTeamNative1381Slice.Team { Name = "Blue", TextColor = "nope", Logo = "L" });
        slice.InGame[3] = true;
        slice.CurrentTeam[3] = -1;
        Assert.Equal(0, slice.CallChangeTeam(3, 0, 0, out var discarded));
        Assert.Equal(0, discarded);
        Assert.Equal(0, slice.CurrentTeam[3]);
        Assert.Equal(["changed 3 0", "translation 3"], slice.Trace);
        Assert.Equal(1, slice.CallIsValid(0, 1, out var valid));
        Assert.Equal(1, valid);
        Assert.Equal(1, slice.CallIsValid(4, 1, out valid));
        Assert.Equal(0, valid);

        var team = slice.Teams[0];
        Assert.Equal(0, slice.CallGetTextColor(team, 0, out discarded));
        Assert.Equal(0, discarded);
        Assert.Contains("Undefined color 'nope'", Assert.Single(slice.Lines), StringComparison.Ordinal);
        Assert.Equal(1, slice.CallGetLogo(team, 2, out var logo));
        Assert.Equal(3, logo);
        Assert.Equal(1, slice.CallGetLogoName(team, 1, out var name));
        Assert.Equal("L", name);
        Assert.Equal(0, slice.CallGetLogoName(team, 0, out name));
        Assert.Null(name);
        slice.WriteLogoName(team, out var written);
        Assert.Equal("L", written);
        team.PlayerColor = 9;
        Assert.Equal(1, slice.CallGetPlayerColor(team, 1, out var color));
        Assert.Equal(9, color);
        Assert.Equal(1, slice.CallAllowsCustom(team, 1, out var allow));
        Assert.Equal(0, allow);

        var missing = Assert.Throws<InvalidOperationException>(() => slice.CallGetPlayerColor(null, 1, out _));
        Assert.Equal("In function parameter self", missing.Message);
        Assert.Equal("FTeam", VmTeamNative1381Slice.Exports[1].ClassName);
        Assert.Equal("mName", VmTeamNative1381Slice.Exports[1].ScriptName);
        Assert.Equal(0, VmTeamNative1381Slice.Exports[0].Bits);
        Assert.Equal("AllowsCustomPlayerColor", VmTeamNative1381Slice.Natives[^1]);
        Assert.Equal(7, VmTeamNative1381Slice.Natives.Length);
    }
}
