namespace HCDE.Playsim.Tests;

public class VmHuScores1185SliceTests
{
    [Fact]
    public void ComparePoints_UsesFragsInDeathmatchAndKillsOtherwise()
    {
        var higher = new VmHuScores1185Slice.ScorePlayer { FragCount = 3, KillCount = 1, Name = "Bob" };
        var lower = new VmHuScores1185Slice.ScorePlayer { FragCount = 1, KillCount = 9, Name = "ann" };
        Assert.True(VmHuScores1185Slice.ComparePoints(true, higher, lower) < 0);
        Assert.True(VmHuScores1185Slice.ComparePoints(false, higher, lower) > 0);

        var sameA = new VmHuScores1185Slice.ScorePlayer { FragCount = 4, Name = "bob" };
        var sameB = new VmHuScores1185Slice.ScorePlayer { FragCount = 4, Name = "BOB" };
        Assert.Equal(0, VmHuScores1185Slice.ComparePoints(true, sameA, sameB));

        var alpha = new VmHuScores1185Slice.ScorePlayer { KillCount = 2, Name = "ada" };
        var beta = new VmHuScores1185Slice.ScorePlayer { KillCount = 2, Name = "Bea" };
        Assert.True(VmHuScores1185Slice.ComparePoints(false, alpha, beta) < 0);
    }

    [Fact]
    public void CompareTeams_OrdersByTeamThenFragsThenName()
    {
        var blue = new VmHuScores1185Slice.ScorePlayer { Team = 1, FragCount = 9, Name = "z" };
        var red = new VmHuScores1185Slice.ScorePlayer { Team = 0, FragCount = 0, Name = "a" };
        Assert.True(VmHuScores1185Slice.CompareTeams(blue, red) > 0);

        var fewer = new VmHuScores1185Slice.ScorePlayer { Team = 2, FragCount = 1, Name = "m" };
        var more = new VmHuScores1185Slice.ScorePlayer { Team = 2, FragCount = 5, Name = "z" };
        Assert.True(VmHuScores1185Slice.CompareTeams(fewer, more) > 0);

        var ann = new VmHuScores1185Slice.ScorePlayer { Team = 2, FragCount = 5, Name = "Ann" };
        var bea = new VmHuScores1185Slice.ScorePlayer { Team = 2, FragCount = 5, Name = "bea" };
        Assert.True(VmHuScores1185Slice.CompareTeams(ann, bea) < 0);
        Assert.Equal(0, VmHuScores1185Slice.CompareTeams(ann, new VmHuScores1185Slice.ScorePlayer { Team = 2, FragCount = 5, Name = "ANN" }));
    }

    [Fact]
    public void DrawAndToggle_ClearTheFlagUnlessTheBarDraws()
    {
        var names = VmHuScores1185Slice.Cvars.Select(cvar => cvar.Name).ToArray();
        Assert.Contains("sb_cooperative_enable", names);
        Assert.Contains("sb_teamdeathmatch_headingcolor", names);
        Assert.Equal(VmHuScores1185Slice.CrRed, VmHuScores1185Slice.Cvars[1].IntDefault);
        Assert.Equal(VmHuScores1185Slice.CrGreen, VmHuScores1185Slice.Cvars[2].IntDefault);
        Assert.Equal(VmHuScores1185Slice.CrGrey, VmHuScores1185Slice.Cvars[3].IntDefault);
        Assert.True(VmHuScores1185Slice.Cvars[0].BoolDefault);
        Assert.All(VmHuScores1185Slice.Cvars, cvar => Assert.True(cvar.Archive));

        var toggled = true;
        var called = false;
        VmHuScores1185Slice.DrawScores(ref toggled, false, _ =>
        {
            called = true;
            return 1;
        }, 0.25);
        Assert.False(called);
        Assert.False(toggled);

        toggled = true;
        double seen = -1;
        VmHuScores1185Slice.DrawScores(ref toggled, true, tic =>
        {
            seen = tic;
            return 0;
        }, 0.5);
        Assert.Equal(0.5, seen);
        Assert.False(toggled);

        toggled = true;
        VmHuScores1185Slice.DrawScores(ref toggled, true, _ => 1, 1);
        Assert.True(toggled);
        VmHuScores1185Slice.ToggleScoreboard(ref toggled);
        Assert.False(toggled);
        VmHuScores1185Slice.ToggleScoreboard(ref toggled);
        Assert.True(toggled);
    }
}
