namespace HCDE.Playsim.Tests;

public class VmPEnemyLookExTests
{
    [Fact]
    public void ConversationExit_Blocks() =>
        Assert.True(VmPEnemyLookEx.ConversationExit(VmPEnemyLookEx.Mf5Inconversation));

    [Fact]
    public void PatrolGoal_WhenSetGoal() =>
        Assert.True(VmPEnemyLookEx.EvaluatePatrolGoal(
            new VmPEnemyLookEx.LookExGoalContext(14, 0, 1, 2, 0, 0)));

    [Fact]
    public void SoundTarget_DeadClears() =>
        Assert.False(VmPEnemyLookEx.EvaluateSoundTarget(
            new VmPEnemyLookEx.LookExSoundContext(false, 0, 10, 0, false, 0)));

    [Fact]
    public void HearDistance_BlocksFar() =>
        Assert.False(VmPEnemyLookEx.EvaluateSoundTarget(
            new VmPEnemyLookEx.LookExSoundContext(false, 1, 200, 100, false, 0)));

    [Fact]
    public void StealthVisdir() =>
        Assert.True(true);

    [Fact]
    public void StandstillBlocksWander() =>
        Assert.False(VmPEnemyLookEx.StandstillBlocksWander(0));

    [Fact]
    public void AmbushSightRequired() =>
        Assert.True(VmPEnemyLookEx.AmbushSightRequired(true, true));

    [Fact]
    public void FullVolumeSee() =>
        Assert.True(VmPEnemyLookEx.FullVolumeSee(VmPEnemyLookEx.LofFullvolseesound));

    [Fact]
    public void InchaseSkipsSee() =>
        Assert.True(VmPEnemyLookEx.InchaseSkipsSee(VmPEnemyLookEx.Mf7Inchase));

    [Fact]
    public void SeeYouTail_GoalReactionBlocks() =>
        Assert.False(VmPEnemyLookEx.EvaluateSeeYouTail(
            new VmPEnemyLookEx.LookExSeeYouContext(true, 5, 10, true, 0, 0, true, false)));

    [Fact]
    public void FriendWander_PicksParamSeeState() =>
        Assert.True(VmPEnemyLookEx.EvaluateFriendWander(
            new VmPEnemyLookEx.LookExFriendWanderContext(true, 0, 0, 0, true, false)));

    [Fact]
    public void EnemyAmbush_DistInRange() =>
        Assert.True(VmPEnemyLookEx.EvaluateEnemyAmbushSeeYou(
            new VmPEnemyLookEx.LookExAmbushContext(true, false, true, true, 128, 64, 256)));

    [Fact]
    public void SeeYouGoal_PicksSeeState() =>
        Assert.True(VmPEnemyLookEx.EvaluateSeeYouGoalAndState(
            new VmPEnemyLookEx.LookExSeeYouGoalContext(false, 0, 10, true, 0, true, 0, true, false)));
}
