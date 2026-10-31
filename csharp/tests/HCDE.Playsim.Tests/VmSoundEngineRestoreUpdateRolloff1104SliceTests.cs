namespace HCDE.Playsim.Tests;

public class VmSoundEngineRestoreUpdateRolloff1104SliceTests
{
    [Fact]
    public void GetRolloff_ReturnsOneInsideMinDistance()
    {
        var rolloff = new VmSoundEngineRestoreUpdateRolloff1104Slice.RolloffInfo
        {
            MinDistance = 100f,
            MaxDistance = 1000f,
            RolloffFactor = 1f,
            RolloffType = VmSoundEngineRestoreUpdateRolloff1104Slice.RolloffLinear
        };
        Assert.Equal(1f, VmSoundEngineRestoreUpdateRolloff1104Slice.GetRolloff(rolloff, 50f, null));
    }

    [Fact]
    public void RunUpdateSounds_ClearsJustStartedAndRestoresWhenDue()
    {
        var restored = false;
        var row = new VmSoundEngineRestoreUpdateRolloff1104Slice.UpdateChannelRow
        {
            ChanFlags = VmSoundEngineRestoreUpdateRolloff1104Slice.ChanfJustStarted
        };
        VmSoundEngineRestoreUpdateRolloff1104Slice.RunUpdateSounds(
            new VmSoundEngineRestoreUpdateRolloff1104Slice.UpdateSoundsContext
            {
                Channels = new[] { row },
                Time = 10,
                RestartEvictionsAt = 5,
                RestoreEvicted = () => restored = true
            });
        Assert.True(restored);
        Assert.Equal(0, row.ChanFlags & VmSoundEngineRestoreUpdateRolloff1104Slice.ChanfJustStarted);
    }

    [Fact]
    public void RunRestoreEvictedNode_ReturnsForgettableWithoutSysChannel()
    {
        var returned = false;
        VmSoundEngineRestoreUpdateRolloff1104Slice.RunRestoreEvictedNode(
            new VmSoundEngineRestoreUpdateRolloff1104Slice.RestoreEvictedContext
            {
                ChanFlags = VmSoundEngineRestoreUpdateRolloff1104Slice.ChanfForgettable,
                HasSysChannel = false,
                ReturnChannel = () => returned = true
            });
        Assert.True(returned);
    }
}
