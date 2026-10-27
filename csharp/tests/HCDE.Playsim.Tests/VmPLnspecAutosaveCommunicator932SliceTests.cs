namespace HCDE.Playsim.Tests;

public class VmPLnspecAutosaveCommunicator932SliceTests
{
    [Fact]
    public void BuildAutosave_QueuesWhenNotSaving()
    {
        var invoke = VmPLnspecAutosaveCommunicator932Slice.BuildAutosave(0, 1, 2, 0);
        Assert.True(invoke.QueueDem);
        Assert.Equal(0, invoke.ClearedFlags2);
    }

    [Fact]
    public void BuildAutosave_SkipsDuringSaveGame()
    {
        var invoke = VmPLnspecAutosaveCommunicator932Slice.BuildAutosave(1, 1, 2, 3);
        Assert.False(invoke.QueueDem);
        Assert.Equal(3, invoke.ClearedFlags2);
    }

    [Fact]
    public void NextSkillFromArg_ClampsPastSkillCount() =>
        Assert.Equal(-1, VmPLnspecAutosaveCommunicator932Slice.NextSkillFromArg(9, 4));

    [Fact]
    public void NoiseAlertEmitter_ReusesTargetWhenTidsMatch()
    {
        var target = new object();
        Assert.Same(target, VmPLnspecAutosaveCommunicator932Slice.NoiseAlertEmitter(5, 5, null, target, null));
    }

    [Fact]
    public void BuildCommunicator_RejectsBacksideWhenFrontOnly()
    {
        var invoke = VmPLnspecAutosaveCommunicator932Slice.BuildCommunicator(
            frontOnly: true, backSide: true, activator: new(), player: new(), hasCommunicator: true,
            voc: 3, identify: 2, noLog: false, localView: true);
        Assert.Null(invoke);
    }

    [Fact]
    public void BuildCommunicator_FormatsSoundAndSubtitle()
    {
        var invoke = VmPLnspecAutosaveCommunicator932Slice.BuildCommunicator(
            frontOnly: false, backSide: false, activator: new(), player: new(), hasCommunicator: true,
            voc: 7, identify: 4, noLog: true, localView: false);
        Assert.NotNull(invoke);
        Assert.Equal("svox/voc7", invoke!.Value.SoundName);
        Assert.Equal("TXT_COMM4", invoke.Value.SubtitleKey);
        Assert.False(invoke.Value.SetLog);
    }

    [Fact]
    public void BuildGlassBreak_OffsetsSpawnAlongNormal()
    {
        var invoke = VmPLnspecAutosaveCommunicator932Slice.BuildGlassBreak(
            lineFlags: 3, switchedSide0: true, switchedSide1: false, noJunkArg: 0, junkId: 0,
            v1x: 0, v1y: 0, v2x: 4, v2y: 0);
        Assert.Equal(0, invoke.LineFlags);
        Assert.True(invoke.SpawnJunk);
        Assert.Equal(2, invoke.SpawnX);
        Assert.Equal(-2, invoke.SpawnY);
    }
}
