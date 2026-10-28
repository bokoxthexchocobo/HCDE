namespace HCDE.Playsim.Tests;

public class VmGLevelExitIntermission1013SliceTests
{
    [Fact]
    public void SecretExitMapAndNormalExit()
    {
        Assert.True(VmGLevelExitIntermission1013Slice.HasChangeLevelActionTarget(4));
        Assert.Equal(
            "MAP02",
            VmGLevelExitIntermission1013Slice.ResolveSecretExitMap("MAP01", "MAP02", secretMapDataValid: true));
        Assert.Equal(
            "MAP01",
            VmGLevelExitIntermission1013Slice.ResolveSecretExitMap("MAP01", "BADMAP", secretMapDataValid: false));
        Assert.Equal(
            "enDSeQ1",
            VmGLevelExitIntermission1013Slice.ResolveSecretExitMap("MAP01", "enDSeQ1", secretMapDataValid: false));

        var exit = VmGLevelExitIntermission1013Slice.PlanNormalExit(
            flags3: 0,
            nextMap: "MAP02",
            position: 1,
            keepFacing: true);
        Assert.Equal((uint)VmGLevelExitIntermission1013Slice.Level3ExitNormalUsed, exit.Flags3After);
        Assert.Equal(VmGLevelExitIntermission1013Slice.ChangeLevelKeepFacing, exit.ChangeFlags);

        var secret = VmGLevelExitIntermission1013Slice.PlanSecretExit(
            flags3: 0,
            resolvedMap: "MAP02S",
            position: 2);
        Assert.Equal((uint)VmGLevelExitIntermission1013Slice.Level3ExitSecretUsed, secret.Flags3After);
        Assert.Equal("MAP02S", secret.ResolvedMap);
    }

    [Fact]
    public void EndgameSequenceAndStrifeEnding()
    {
        Assert.True(VmGLevelExitIntermission1013Slice.MatchesEndSequenceMapPrefix("enDSeQ10"));
        Assert.True(VmGLevelExitIntermission1013Slice.TryParseEndgameSequenceName("enDSeQ10", out var seq));
        Assert.Equal("#16", seq);
        Assert.Equal(
            VmGLevelExitIntermission1013Slice.InterStrifeGood,
            VmGLevelExitIntermission1013Slice.ResolveStrifeEndingSequence(
                VmGLevelExitIntermission1013Slice.InterStrife,
                playerHasQuestItem25: true,
                playerHasQuestItem28: false));
        Assert.Equal(
            VmGLevelExitIntermission1013Slice.InterStrifeSad,
            VmGLevelExitIntermission1013Slice.ResolveStrifeEndingSequence(
                VmGLevelExitIntermission1013Slice.InterStrife,
                playerHasQuestItem25: false,
                playerHasQuestItem28: false));
        Assert.True(VmGLevelExitIntermission1013Slice.ShouldSkipCreateIntermissionForCheat(
            VmGLevelExitIntermission1013Slice.LevelChangeMapCheat));
    }

    [Fact]
    public void IntermissionExitTextKeys()
    {
        Assert.Equal(
            VmGLevelExitIntermission1013Slice.ExitTextKeySecret,
            VmGLevelExitIntermission1013Slice.ResolveEndgameExitTextKey(
                VmGLevelExitIntermission1013Slice.Level3ExitSecretUsed));
        Assert.Equal(
            "MAP03",
            VmGLevelExitIntermission1013Slice.ResolveNonEndgameExitTextKey(0, "MAP03"));
        Assert.True(VmGLevelExitIntermission1013Slice.ShouldUseClusterEndgameText(0));
        Assert.False(VmGLevelExitIntermission1013Slice.ShouldUseClusterEndgameText(
            VmGLevelExitIntermission1013Slice.Level2NoClusterText));

        var plan = VmGLevelExitIntermission1013Slice.PlanMapExitFinaleFromExitText(
            VmGLevelExitIntermission1013Slice.Level3ExitNormalUsed,
            "MAP02",
            VmGLevelExitIntermission1013Slice.ExitTextDefined.Text);
        Assert.True(plan.ShouldLaunch);
        Assert.True(VmGLevelExitIntermission1013Slice.ShouldReturnEarlyAfterMapExitFinale(plan));
        Assert.True(VmGLevelExitIntermission1013Slice.ShouldRunMapExitIntermission(deathmatch: false, levelFlags: 0));
        Assert.False(VmGLevelExitIntermission1013Slice.ShouldRunMapExitIntermission(
            deathmatch: false,
            VmGLevelExitIntermission1013Slice.LevelChangeMapCheat));
    }
}
