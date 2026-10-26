namespace HCDE.Playsim.Tests;

public class VmPUserPredictClientTests
{
    [Fact]
    public void SinglePlayer_DisablesPrediction() =>
        Assert.True(VmPUserPredictClient.SinglePlayerTearsDownPrediction(false));

    [Fact]
    public void Replay_SkipsWhenCaughtUp() =>
        Assert.True(VmPUserPredictClient.ShouldSkipPredictionReplay(
            new VmPUserPredictClient.PredictionReplayGate(5, 10, VmPUserPredictClient.PstLive, 0)));

    [Fact]
    public void Window_ClampReducesStart()
    {
        var start = VmPUserPredictClient.ClampPredictionStart(10, 3, 1, out var clamped);
        Assert.Equal(7, start);
        Assert.True(clamped);
    }

    [Fact]
    public void Rubberband_RequiresScaleBetweenZeroAndOne() =>
        Assert.True(VmPUserPredictClient.CanRubberband(true, false, 5, 1, 10, 0.5f));

    [Fact]
    public void Rubberband_TriggersOnDistance() =>
        Assert.True(VmPUserPredictClient.EvaluateRubberband(
            new VmPUserPredictClient.RubberbandSample(100, 2, 0.01, 50), out _));

    [Fact]
    public void TryBuildWindow_CapsEnd()
    {
        Assert.True(VmPUserPredictClient.TryBuildPredictionWindow(
            new VmPUserPredictClient.PredictionWindow(1, 20, 5), 15, out _, out var end, out _));
        Assert.Equal(15, end);
    }

    [Fact]
    public void PhantomYaw_FoldsLargeDelta()
    {
        var phantom = VmPUserPredictClient.PhantomYawDegrees(350, 10);
        Assert.True(Math.Abs(phantom) < 30);
    }

    [Fact]
    public void ReplayAccumulators_SumYaw() =>
        Assert.Equal(5, VmPUserPredictClient.AccumulateReplayCmd(
            new VmPUserPredictClient.ReplayDiagAccumulators(0, 0, 0, 0, 0, 0, 0),
            5, 0, 0, 0, 0, 0).YawBam);

    [Fact]
    public void RunEntryGates_SinglePlayerTearsDown()
    {
        var torn = false;
        Assert.Equal(
            VmPUserPredictClient.PredictClientExit.SinglePlayer,
            VmPUserPredictClient.RunEntryGates(new VmPUserPredictClient.PredictClientContext
            {
                Netgame = false,
                TearDownSinglePlayer = () => torn = true,
            }));
        Assert.True(torn);
    }

    [Fact]
    public void RunPredictClient_ReplaysWindow()
    {
        var ticks = new List<int>();
        Assert.True(VmPUserPredictClient.RunPredictClient(
            new VmPUserPredictClient.PredictClientContext
            {
                Netgame = true,
                Gamestate = VmPUserPredictClient.GsLevel,
                ClientTic = 10,
                LastPredictedTic = 5,
                PlayerState = VmPUserPredictClient.PstLive,
                PredictStart = 5,
                PredictEnd = 10,
                PredictMax = 10,
                EndCap = 10,
                AlreadyPredicting = true,
                ReplayTic = ticks.Add,
            },
            out _,
            out _,
            out _));
        Assert.Equal(5, ticks.Count);
    }
}
